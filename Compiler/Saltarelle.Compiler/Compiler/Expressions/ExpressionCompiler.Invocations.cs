using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler.Compiler.Expressions {
	partial class ExpressionCompiler {
		private IList<int> CreateInlineCodeExpressionToOrderMap(IList<InlineCodeToken> tokens, int argumentCount, IList<int> argumentToParameterMap) {
			var dict = Enumerable.Range(-1, argumentCount + 1).OrderBy(x => FindIndexInTokens(tokens, x)).Select((i, n) => new { i, n }).ToDictionary(x => x.i, x => x.n);
			return new[] { -1 }.Concat(argumentToParameterMap).Select(x => dict[x]).ToList();
		}

		private int FindIndexInTokens(IList<InlineCodeToken> tokens, int parameterIndex) {
			for (int i = 0; i < tokens.Count; i++) {
				if (parameterIndex == -1) {
					if (tokens[i].Type == InlineCodeToken.TokenType.This)
						return i;
				}
				else {
					if ((tokens[i].Type == InlineCodeToken.TokenType.Parameter) && tokens[i].Index == parameterIndex)
						return i;
				}
			}
			return -1;
		}

		private void CreateTemporariesForExpressionsAndAllRequiredExpressionsLeftOfIt(List<JsExpression> expressions, int index) {
			for (int i = 0; i < index; i++) {
				if (ExpressionOrderer.DoesOrderMatter(expressions[i], expressions[index])) {
					CreateTemporariesForExpressionsAndAllRequiredExpressionsLeftOfIt(expressions, i);
				}
			}
			var temp = _createTemporaryVariable();
			_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, expressions[index]));
			expressions[index] = JsExpression.Identifier(_variables[temp].Name);
		}

		private List<JsExpression> CompileThisAndArgumentListForMethodCall(IMethodSymbol member, string literalCode, JsExpression target, bool argumentsUsedMultipleTimes, ArgumentMap argumentMap, int? omitUnspecifiedArgumentsFrom) {
			member = member.UnReduceIfExtensionMethod();
			IList<InlineCodeToken> tokens = null;
			var expressions = new List<JsExpression> { target };
			if (literalCode != null) {
				bool hasError = false;
				tokens = InlineCodeMethodCompiler.Tokenize((IMethodSymbol)member, literalCode, s => hasError = true);
				if (hasError)
					tokens = null;
			}

			if (tokens != null && target != null && !member.IsStatic && member.MethodKind != MethodKind.Constructor) {
				int thisUseCount = tokens.Count(t => t.Type == InlineCodeToken.TokenType.This);
				if (thisUseCount > 1 && IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(target)) {
					// Create a temporary for {this}, if required.
					var temp = _createTemporaryVariable();
					_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, expressions[0]));
					expressions[0] = JsExpression.Identifier(_variables[temp].Name);
				}
				else if (thisUseCount == 0 && DoesJsExpressionHaveSideEffects.Analyze(target)) {
					// Ensure that 'this' is evaluated if required, even if not used by the inline code.
					_additionalStatements.Add(target);
					expressions[0] = JsExpression.Null;
				}
			}

			bool hasCreatedParamArray = false;
			int lastSpecifiedArgument = -1;

			// Compile the arguments left to right
			foreach (var i in argumentMap.ArgumentToParameterMap) {
				if (member.Parameters[i].IsParams) {
					if (hasCreatedParamArray)
						continue;
					hasCreatedParamArray = true;
				}

				var a = argumentMap.ArgumentsForCall[i];
				if (a.IsSpecified)
					lastSpecifiedArgument = Math.Max(lastSpecifiedArgument, i);

				if (member.Parameters[i].RefKind != RefKind.None) {
					var symbol = _semanticModel.GetSymbolInfo(a.Argument).Symbol;
					if (symbol is ILocalSymbol || symbol is IParameterSymbol) {
						expressions.Add(CompileLocal(symbol, true));
					}
					else {
						_errorReporter.Message(Messages._7513);
						expressions.Add(JsExpression.Null);
					}
				}
				else {
					int useCount = (tokens != null ? tokens.Count(t => t.Type == InlineCodeToken.TokenType.Parameter && t.Index == i) : 1);
					bool usedMultipleTimes = argumentsUsedMultipleTimes || useCount > 1;
					if (useCount >= 1) {
						expressions.Add(InnerCompile(a, usedMultipleTimes, expressions));
					}
					else if (tokens != null && tokens.Count(t => t.Type == InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier && t.Index == i) > 0) {
						var result = CloneAndCompile(a, false);
						expressions.Add(result.Expression);	// Will later give an error if the result is not a literal string.
					}
					else {
						var result = CloneAndCompile(a, false);
						if (result.AdditionalStatements.Count > 0 || DoesJsExpressionHaveSideEffects.Analyze(result.Expression)) {
							CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(expressions, result);
							_additionalStatements.AddRange(result.AdditionalStatements);
							_additionalStatements.Add(result.Expression);
						}
						expressions.Add(JsExpression.Null);	// Will be ignored later, anyway
					}
				}
			}

			// Ensure that expressions are evaluated left-to-right in the resulting script.
			var expressionToOrderMap = tokens == null ? new[] { 0 }.Concat(argumentMap.ArgumentToParameterMap.Select(x => x + 1)).ToList() : CreateInlineCodeExpressionToOrderMap(tokens, argumentMap.ArgumentsForCall.Length, argumentMap.ArgumentToParameterMap);
			for (int i = 0; i < expressions.Count; i++) {
				var haveToBeEvaluatedBefore = Enumerable.Range(i + 1, expressions.Count - i - 1).Where(x => expressionToOrderMap[x] < expressionToOrderMap[i]);
				if (haveToBeEvaluatedBefore.Any(other => ExpressionOrderer.DoesOrderMatter(expressions[i], expressions[other]))) {
					CreateTemporariesForExpressionsAndAllRequiredExpressionsLeftOfIt(expressions, i);
				}
			}

			// Rearrange the arguments so they appear in the order the method expects them to.
			if ((argumentMap.ArgumentToParameterMap.Length != argumentMap.ArgumentsForCall.Length || argumentMap.ArgumentToParameterMap.Select((i, n) => new { i, n }).Any(t => t.i != t.n))) {	// If we have an argument to parameter map and it actually performs any reordering.
				// Ensure that expressions are evaluated left-to-right in case arguments are reordered
				var newExpressions = new List<JsExpression> { expressions[0] };
				for (int i = 0; i < argumentMap.ArgumentsForCall.Length; i++) {
					int specifiedIndex = argumentMap.ArgumentToParameterMap.IndexOf(i);
					newExpressions.Add(specifiedIndex != -1 ? expressions[specifiedIndex + 1] : InnerCompile(argumentMap.ArgumentsForCall[i], false));	// If the argument was not specified, use the value in argumentsForCall, which has to be constant.
				}
				expressions = newExpressions;
			}

			for (int i = 1; i < expressions.Count; i++) {
				if ((i - 1) >= member.Parameters.Length || member.Parameters[i - 1].RefKind == RefKind.None) {
					expressions[i] = MaybeCloneValueType(expressions[i], argumentMap.ArgumentsForCall[i - 1], member.Parameters[Math.Min(i - 1, member.Parameters.Length - 1)].Type);	// Math.Min() because the last parameter might be an expanded param array.
				}
			}

			if (omitUnspecifiedArgumentsFrom != null) {
				var firstRemoved = Math.Max(omitUnspecifiedArgumentsFrom.Value, lastSpecifiedArgument + 1) + 1;
				if (firstRemoved < expressions.Count)
					expressions.RemoveRange(firstRemoved, expressions.Count - firstRemoved);
			}

			return expressions;
		}

		private string GetActualInlineCode(MethodScriptSemantics sem, bool isNonVirtualInvocationOfVirtualMethod, bool isParamArrayExpanded) {
			if (sem.Type == MethodScriptSemantics.ImplType.InlineCode) {
				if (isNonVirtualInvocationOfVirtualMethod)
					return sem.NonVirtualInvocationLiteralCode;
				else if (!isParamArrayExpanded)
					return sem.NonExpandedFormLiteralCode;
				else
					return sem.LiteralCode;
			}
			else {
				return null;
			}
		}

		private string GetActualInlineCode(ConstructorScriptSemantics sem, bool isParamArrayExpanded) {
			if (sem.Type == ConstructorScriptSemantics.ImplType.InlineCode) {
				if (!isParamArrayExpanded)
					return sem.NonExpandedFormLiteralCode;
				else
					return sem.LiteralCode;
			}
			else {
				return null;
			}
		}

		private JsExpression CompileNonExtensionMethodInvocationWithSemantics(MethodScriptSemantics sem, IMethodSymbol method, Func<bool, JsExpression> getTarget, bool targetIsReadOnlyField, ArgumentMap argumentMap, bool isNonVirtualInvocation) {
			Debug.Assert(method.ReducedFrom == null);

			isNonVirtualInvocation &= method.IsOverridable();
			bool targetUsedMultipleTimes = sem != null && ((!sem.IgnoreGenericArguments && method.TypeParameters.Length > 0) || (sem.ExpandParams && !argumentMap.CanBeTreatedAsExpandedForm));
			string literalCode = GetActualInlineCode(sem, isNonVirtualInvocation, argumentMap.CanBeTreatedAsExpandedForm);

			var jsTarget = method.IsStatic ? InstantiateType(method.ContainingType) : getTarget(targetUsedMultipleTimes);
			if (IsMutableValueType(method.ContainingType) && targetIsReadOnlyField) {
				jsTarget = MaybeCloneValueType(jsTarget, method.ContainingType, forceClone: true);
			}

			var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, literalCode, jsTarget, false, argumentMap, sem.Type == MethodScriptSemantics.ImplType.NormalMethod || sem.Type == MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument ? sem.OmitUnspecifiedArgumentsFrom : null);
			return CompileMethodInvocation(sem, method, thisAndArguments, isNonVirtualInvocation);
		}

		private JsExpression CompileMethodInvocation(IMethodSymbol method, Func<bool, JsExpression> getTarget, bool targetIsReadOnlyField, ArgumentMap argumentMap, bool isNonVirtualInvocation) {
			if (method.CallsAreOmitted(_semanticModel.SyntaxTree))
				return JsExpression.Null;

			method = method.UnReduceIfExtensionMethod();
			var sem = _metadataImporter.GetMethodSemantics(method.OriginalDefinition);

			return CompileNonExtensionMethodInvocationWithSemantics(sem, method, getTarget, targetIsReadOnlyField, argumentMap, isNonVirtualInvocation);
		}

		private JsExpression CompileConstructorInvocationWithPotentialExpandParams(IList<JsExpression> arguments, JsExpression constructor, bool expandParams) {
			if (expandParams) {
				if (arguments[arguments.Count - 1] is JsArrayLiteralExpression) {
					var args = arguments.Take(arguments.Count - 1).Concat(((JsArrayLiteralExpression)arguments[arguments.Count - 1]).Elements);
					return JsExpression.New(constructor, args);
				}
				else {
					return _runtimeLibrary.ApplyConstructor(constructor, arguments.Count == 1 ? arguments[0] : JsExpression.Invocation(JsExpression.Member(JsExpression.ArrayLiteral(arguments.Take(arguments.Count - 1)), "concat"), arguments[arguments.Count - 1]), this);
				}
			}
			else {
				return JsExpression.New(constructor, arguments);
			}
		}

		private JsExpression CompileMethodInvocationWithPotentialExpandParams(IList<JsExpression> thisAndArguments, JsExpression method, bool expandParams, bool needCall) {
			if (expandParams) {
				if (thisAndArguments[thisAndArguments.Count - 1] is JsArrayLiteralExpression) {
					var args = thisAndArguments.Take(thisAndArguments.Count - 1).Concat(((JsArrayLiteralExpression)thisAndArguments[thisAndArguments.Count - 1]).Elements);
					return needCall ? JsExpression.Invocation(JsExpression.Member(method, "call"), args) : JsExpression.Invocation(method, args.Skip(1));
				}
				else {
					return JsExpression.Invocation(JsExpression.Member(method, "apply"), thisAndArguments[0], thisAndArguments.Count == 2 ? thisAndArguments[1] : JsExpression.Invocation(JsExpression.Member(JsExpression.ArrayLiteral(thisAndArguments.Skip(1).Take(thisAndArguments.Count - 2)), "concat"), thisAndArguments[thisAndArguments.Count - 1]));
				}
			}
			else {
				return needCall ? JsExpression.Invocation(JsExpression.Member(method, "call"), thisAndArguments) : JsExpression.Invocation(method, thisAndArguments.Skip(1));
			}
		}

		private JsExpression CompileMethodInvocation(MethodScriptSemantics impl, IMethodSymbol method, IList<JsExpression> thisAndArguments, bool isNonVirtualInvocation) {
			if (method.ReducedFrom != null) {
				_errorReporter.InternalError("Reduced extension method  " + method + " should already have been unreduced");
				return JsExpression.Null;
			}

			isNonVirtualInvocation &= method.IsOverridable();
			var errors = Utils.FindGenericInstantiationErrors(method.TypeArguments, _metadataImporter);
			if (errors.HasErrors) {
				foreach (var ut in errors.UsedUnusableTypes)
					_errorReporter.Message(Messages._7515, ut.Name, method.FullyQualifiedName());
				foreach (var t in errors.MutableValueTypesBoundToTypeArguments)
					_errorReporter.Message(Messages._7539, t.Name);
				return JsExpression.Null;
			}

			var typeArguments = (impl != null && !impl.IgnoreGenericArguments ? method.TypeArguments : ImmutableArray<ITypeSymbol>.Empty);

			switch (impl.Type) {
				case MethodScriptSemantics.ImplType.NormalMethod: {
					if (isNonVirtualInvocation) {
						return _runtimeLibrary.CallBase(method, thisAndArguments, this);
					}
					else {
						var jsMethod = JsExpression.Member(thisAndArguments[0], impl.Name);
						if (method.IsStatic)
							thisAndArguments = new[] { JsExpression.Null }.Concat(thisAndArguments.Skip(1)).ToList();

						if (typeArguments.Length > 0) {
							return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments, this), impl.ExpandParams, true);
						}
						else
							return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, jsMethod, impl.ExpandParams, false);
					}
				}

				case MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument: {
					var jsMethod = JsExpression.Member(InstantiateType(method.ContainingType), impl.Name);
					thisAndArguments.Insert(0, JsExpression.Null);
					if (typeArguments.Length > 0) {
						return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments, this), impl.ExpandParams, true);
					}
					else {
						return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, jsMethod, impl.ExpandParams, false);
					}
				}

				case MethodScriptSemantics.ImplType.InlineCode:
					return CompileInlineCodeMethodInvocation(method, GetActualInlineCode(impl, isNonVirtualInvocation, thisAndArguments[thisAndArguments.Count - 1] is JsArrayLiteralExpression), method.IsStatic ? null : thisAndArguments[0], thisAndArguments.Skip(1).ToList());

				case MethodScriptSemantics.ImplType.NativeIndexer:
					return JsExpression.Index(thisAndArguments[0], thisAndArguments[1]);

				default: {
					_errorReporter.Message(Messages._7516, method.FullyQualifiedName());
					return JsExpression.Null;
				}
			}
		}

		private JsExpression ResolveTypeForInlineCode(string typeName) {
			var type = _compilation.GetTypeByMetadataName(typeName);
			if (type == null) {
				_errorReporter.Message(Messages._7525, "Unknown type '" + typeName + "' specified in inline implementation");
				return JsExpression.Null;
			}
			else {
				if (type.Arity > 0)
					type = type.ConstructUnboundGenericType();
				return InstantiateType(type);
			}
		}

		private JsExpression CompileInlineCodeMethodInvocation(IMethodSymbol method, string code, JsExpression @this, IList<JsExpression> arguments) {
			var tokens = InlineCodeMethodCompiler.Tokenize(method, code, s => _errorReporter.Message(Messages._7525, s));
			if (tokens == null) {
				return JsExpression.Null;
			}
			if (tokens.Any(t => t.Type == InlineCodeToken.TokenType.Parameter && t.IsExpandedParamArray) && !(arguments[arguments.Count - 1] is JsArrayLiteralExpression)) {
				_errorReporter.Message(Messages._7525, string.Format("The {0} can only be invoked with its params parameter expanded", method.MethodKind == MethodKind.Constructor ? "constructor " + method.ContainingType.FullyQualifiedName() : ("method " + method.FullyQualifiedName())));
				return JsExpression.Null;
			}
			if (method.ReturnType.SpecialType == SpecialType.System_Void && method.MethodKind != MethodKind.Constructor) {
				_additionalStatements.AddRange(InlineCodeMethodCompiler.CompileStatementListInlineCodeMethodInvocation(method, tokens, @this, arguments, ResolveTypeForInlineCode, t => _runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, this), s => _errorReporter.Message(Messages._7525, s)));
				return JsExpression.Null;
			}
			else {
				return InlineCodeMethodCompiler.CompileExpressionInlineCodeMethodInvocation(method, tokens, @this, arguments, ResolveTypeForInlineCode, t => _runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, this), s => _errorReporter.Message(Messages._7525, s));
			}
		}

		private string GetMemberNameForJsonConstructor(ISymbol member) {
			if (member is IPropertySymbol) {
				var currentImpl = _metadataImporter.GetPropertySemantics((IPropertySymbol)member.OriginalDefinition);
				if (currentImpl.Type == PropertyScriptSemantics.ImplType.Field) {
					return currentImpl.FieldName;
				}
				else {
					_errorReporter.Message(Messages._7517, member.FullyQualifiedName());
					return null;
				}
			}
			else if (member is IFieldSymbol) {
				var currentImpl = _metadataImporter.GetFieldSemantics((IFieldSymbol)member.OriginalDefinition);
				if (currentImpl.Type == FieldScriptSemantics.ImplType.Field) {
					return currentImpl.Name;
				}
				else {
					_errorReporter.Message(Messages._7518, member.FullyQualifiedName());
					return null;
				}
			}
			else {
				_errorReporter.InternalError("Unsupported member " + member + " in object initializer.");
				return null;
			}
		}

		private JsExpression CompileJsonConstructorCall(ConstructorScriptSemantics impl, ArgumentMap argumentMap, IReadOnlyList<Tuple<ISymbol, ExpressionSyntax>> initializers) {
			var jsPropertyNames = new List<string>();
			var expressions = new List<JsExpression>();
			// Add initializers for specified arguments.
			foreach (int arg in argumentMap.ArgumentToParameterMap) {
				var m = impl.ParameterToMemberMap[arg];
				string name = GetMemberNameForJsonConstructor(m);
				if (name != null) {
					jsPropertyNames.Add(name);
					expressions.Add(InnerCompile(argumentMap.ArgumentsForCall[arg], false, expressions));
				}
			}
			// Add initializers for initializer statements
			foreach (var init in initializers) {
				if (init.Item1 != null) {
					string name = GetMemberNameForJsonConstructor(init.Item1);
					if (name != null) {
						if (jsPropertyNames.Contains(name)) {
							_errorReporter.Message(Messages._7527, init.Item1.Name);
						}
						else {
							jsPropertyNames.Add(name);
							expressions.Add(InnerCompile(init.Item2, false, expressions));
						}
					}
				}
				else {
					_errorReporter.InternalError("Expected an assignment to an identifier, got " + init.Item2);
				}
			}

			// Add initializers for unspecified arguments
			for (int i = 0; i < argumentMap.ArgumentsForCall.Length; i++) {
				if (!argumentMap.ArgumentToParameterMap.Contains(i)) {
					string name = GetMemberNameForJsonConstructor(impl.ParameterToMemberMap[i]);
					if (name != null && !jsPropertyNames.Contains(name)) {
						jsPropertyNames.Add(name);
						expressions.Add(InnerCompile(argumentMap.ArgumentsForCall[i], false, expressions));
					}
				}
			}

			var jsProperties = new List<JsObjectLiteralProperty>();
			for (int i = 0; i < expressions.Count; i++)
				jsProperties.Add(new JsObjectLiteralProperty(jsPropertyNames[i], expressions[i]));
			return JsExpression.ObjectLiteral(jsProperties);
		}

		private IEnumerable<Tuple<ISymbol, ExpressionSyntax>> ResolveInitializedMembers(IEnumerable<ExpressionSyntax> initializers) {
			foreach (var init in initializers) {
				if (init.CSharpKind() == SyntaxKind.SimpleAssignmentExpression) {
					var be = (AssignmentExpressionSyntax)init;
					yield return Tuple.Create(_semanticModel.GetSymbolInfo(be.Left).Symbol, be.Right);
				}
				else {
					yield return Tuple.Create((ISymbol)null, init);
				}
			}
		}

		private void CompileInitializerStatementsInner(Func<JsExpression> getTarget, IEnumerable<Tuple<ISymbol, ExpressionSyntax>> initializers) {
			foreach (var init in initializers) {
				if (init.Item1 == null) {
					var collectionInitializer = (IMethodSymbol)_semanticModel.GetCollectionInitializerSymbolInfoWorking(init.Item2);
					var arguments = init.Item2.CSharpKind() == SyntaxKind.ComplexElementInitializerExpression ? ((InitializerExpressionSyntax)init.Item2).Expressions : (IReadOnlyList<ExpressionSyntax>)new[] { init.Item2 };

					var js = CompileMethodInvocation(collectionInitializer, _ => getTarget(), false, ArgumentMap.CreateIdentity(arguments), false);
					if (js.NodeType != ExpressionNodeType.Null) {
						_additionalStatements.Add(js);
					}
				}
				else {
					var nestedInitializer = init.Item2 as InitializerExpressionSyntax;
					if (nestedInitializer != null) {
						CompileInitializerStatementsInner(() => HandleMemberRead(_ => getTarget(), init.Item1, false, false), ResolveInitializedMembers(nestedInitializer.Expressions));
					}
					else {
						var type = _semanticModel.GetTypeInfo(init.Item2).Type;
						var js = CompileMemberAssignment(_ => getTarget(), false, type, init.Item1, null, new ArgumentForCall(init.Item2), null, (a, b) => b, false, false, false);
						if (js.NodeType != ExpressionNodeType.Null) {
							_additionalStatements.Add(js);
						}
					}
				}
			}
		}

		private JsExpression CompileInitializerStatements(JsExpression objectBeingInitialized, IReadOnlyList<Tuple<ISymbol, ExpressionSyntax>> initializers) {
			if (initializers != null && initializers.Count > 0) {
				var tempVar = _createTemporaryVariable();
				var tempName = _variables[tempVar].Name;
				_additionalStatements.Add(JsStatement.Var(tempName, objectBeingInitialized));
					CompileInitializerStatementsInner(() => JsExpression.Identifier(tempName), initializers);
				return JsExpression.Identifier(tempName);
			}
			else {
				return objectBeingInitialized;
			}
		}

		private JsExpression CompileNonJsonConstructorInvocation(ConstructorScriptSemantics impl, IMethodSymbol method, IList<JsExpression> arguments, bool canBeTreatedAsExpandedForm) {
			var type = InstantiateType(method.ContainingType);
			switch (impl.Type) {
				case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
					return CompileConstructorInvocationWithPotentialExpandParams(arguments, type, impl.ExpandParams);

				case ConstructorScriptSemantics.ImplType.NamedConstructor:
					return CompileConstructorInvocationWithPotentialExpandParams(arguments, JsExpression.Member(type, impl.Name), impl.ExpandParams);

				case ConstructorScriptSemantics.ImplType.StaticMethod:
					return CompileMethodInvocationWithPotentialExpandParams(new[] { JsExpression.Null }.Concat(arguments).ToList(), JsExpression.Member(type, impl.Name), impl.ExpandParams, false);

				case ConstructorScriptSemantics.ImplType.InlineCode:
					string literalCode = GetActualInlineCode(impl, canBeTreatedAsExpandedForm);
					return CompileInlineCodeMethodInvocation(method, literalCode, null , arguments);

				default:
					_errorReporter.Message(Messages._7505);
					return JsExpression.Null;
			}
		}

		private JsExpression CompileConstructorInvocation(ConstructorScriptSemantics impl, IMethodSymbol method, ArgumentMap argumentMap, IReadOnlyList<Tuple<ISymbol, ExpressionSyntax>> initializers) {
			var typeToConstruct = method.ContainingType;
			var typeToConstructDef = typeToConstruct.ConstructedFrom;
			if (typeToConstructDef != null && _metadataImporter.GetTypeSemantics(typeToConstructDef.OriginalDefinition).Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_errorReporter.Message(Messages._7519, typeToConstruct.FullyQualifiedName());
				return JsExpression.Null;
			}
			if (typeToConstruct.TypeArguments.Length > 0) {
				var errors = Utils.FindGenericInstantiationErrors(typeToConstruct.TypeArguments, _metadataImporter);
				if (errors.HasErrors) {
					foreach (var ut in errors.UsedUnusableTypes)
						_errorReporter.Message(Messages._7520, ut.FullyQualifiedName(), typeToConstructDef.FullyQualifiedName());
					foreach (var t in errors.MutableValueTypesBoundToTypeArguments)
						_errorReporter.Message(Messages._7539, t.FullyQualifiedName());
					return JsExpression.Null;
				}
			}

			if (impl.Type == ConstructorScriptSemantics.ImplType.Json) {
				return CompileJsonConstructorCall(impl, argumentMap, initializers);
			}
			else {
				string literalCode = GetActualInlineCode(impl, argumentMap.CanBeTreatedAsExpandedForm);
				var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, literalCode, InstantiateType(method.ContainingType), false, argumentMap, impl.Type == ConstructorScriptSemantics.ImplType.UnnamedConstructor || impl.Type == ConstructorScriptSemantics.ImplType.NamedConstructor || impl.Type == ConstructorScriptSemantics.ImplType.StaticMethod ? impl.OmitUnspecifiedArgumentsFrom : null);
				var constructorCall = CompileNonJsonConstructorInvocation(impl, method, thisAndArguments.Skip(1).ToList(), argumentMap.CanBeTreatedAsExpandedForm);
				return CompileInitializerStatements(constructorCall, initializers);
			}
		}

		private JsExpression CompileLateBoundCallWithCandidateSymbols(ImmutableArray<ISymbol> candidateSymbols, ExpressionSyntax expression, IReadOnlyList<ArgumentSyntax> arguments, Func<ISymbol, bool> normalityValidator, Func<ISymbol, string> getName) {
			var expressions = new List<JsExpression>();

			if (candidateSymbols.Any(x => !normalityValidator(x))) {
				_errorReporter.Message(Messages._7530);
				return JsExpression.Null;
			}
			var name = getName(candidateSymbols[0]);
			if (candidateSymbols.Any(x => getName(x) != name)) {
				_errorReporter.Message(Messages._7529);
				return JsExpression.Null;
			}
			JsExpression target;
			if (candidateSymbols[0].IsStatic) {
				target = InstantiateType(candidateSymbols[0].ContainingType);
			}
			else if (expression is MemberAccessExpressionSyntax) {
				target = InnerCompile(((MemberAccessExpressionSyntax)expression).Expression, false);
			}
			else if (expression is ElementAccessExpressionSyntax) {
				target = InnerCompile(((ElementAccessExpressionSyntax)expression).Expression, false);
			}
			else if (expression is IdentifierNameSyntax) {
				target = CompileThis();
			}
			else {
				_errorReporter.InternalError("Unsupported target for dynamic invocation " + expression);
				return JsExpression.Null;
			}
			expressions.Add(JsExpression.Member(target, name));

			foreach (var arg in arguments) {
				if (arg.NameColon != null) {
					_errorReporter.Message(Messages._7526);
					return JsExpression.Null;
				}
				expressions.Add(InnerCompile(arg.Expression, false, expressions));
			}

			return JsExpression.Invocation(expressions[0], expressions.Skip(1));
		}

		private JsExpression CompileEventAddOrRemove(ExpressionSyntax target, IEventSymbol eventSymbol, ExpressionSyntax value, bool isAdd) {
			Func<bool, JsExpression> getTarget;
			if (eventSymbol.IsStatic) {
				getTarget = _ => InstantiateType(eventSymbol.ContainingType);
			}
			else if (target is MemberAccessExpressionSyntax) {
				getTarget = usedMultipleTimes => InnerCompile(((MemberAccessExpressionSyntax)target).Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true);
			}
			else if (target is IdentifierNameSyntax) {
				getTarget = _ => CompileThis();
			}
			else {
				_errorReporter.InternalError("Bad target node for event");
				return JsExpression.Null;
			}

			var impl = _metadataImporter.GetEventSemantics(eventSymbol.OriginalDefinition);
			switch (impl.Type) {
				case EventScriptSemantics.ImplType.AddAndRemoveMethods: {
					var accessor = isAdd ? eventSymbol.AddMethod : eventSymbol.RemoveMethod;
					return CompileNonExtensionMethodInvocationWithSemantics(isAdd ? impl.AddMethod : impl.RemoveMethod, accessor, getTarget, IsReadonlyField(target), ArgumentMap.CreateIdentity(value), target.IsNonVirtualAccess());
				}
				default:
					_errorReporter.Message(Messages._7511, eventSymbol.FullyQualifiedName());
					return JsExpression.Null;
			}
		}
	}
}
