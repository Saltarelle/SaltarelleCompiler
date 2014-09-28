using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler.Expressions {
	partial class ExpressionCompiler {
		private ExpressionCompiler Clone(NestedFunctionContext nestedFunctionContext = null) {
			return new ExpressionCompiler(_compilation, _semanticModel, _metadataImporter, _namer, _runtimeLibrary, _errorReporter, _variables, _createTemporaryVariable, _createInnerCompiler, _thisAlias, nestedFunctionContext ?? _nestedFunctionContext, _activeRangeVariableSubstitutions, _anonymousTypeCache, _transparentTypeCache);
		}

		private ExpressionCompileResult CloneAndCompile(ExpressionSyntax expression, bool returnValueIsImportant, NestedFunctionContext nestedFunctionContext = null, bool returnMultidimArrayValueByReference = false) {
			return Clone(nestedFunctionContext).Compile(expression, returnValueIsImportant, returnMultidimArrayValueByReference: returnMultidimArrayValueByReference);
		}

		private ExpressionCompileResult CloneAndCompile(ArgumentForCall argument, bool returnValueIsImportant, NestedFunctionContext nestedFunctionContext = null, bool returnMultidimArrayValueByReference = false) {
			if (argument.Argument != null)
				return Clone(nestedFunctionContext).Compile(argument.Argument, returnValueIsImportant, returnMultidimArrayValueByReference: returnMultidimArrayValueByReference);
			else if (argument.ParamArray != null) {
				var expressions = new List<JsExpression>();
				var additionalStatements = new List<JsStatement>();
				foreach (var init in argument.ParamArray.Item2) {
					var innerResult = CloneAndCompile(init, true);
					additionalStatements.AddRange(innerResult.AdditionalStatements);
					expressions.Add(MaybeCloneValueType(innerResult.Expression, init, argument.ParamArray.Item1));
				}
				return new ExpressionCompileResult(JsExpression.ArrayLiteral(expressions), additionalStatements);
			}
			else
				return new ExpressionCompileResult(CompileTypedConstant(argument.Constant), ImmutableArray<JsStatement>.Empty);
		}

		private void CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(IList<JsExpression> expressions, ExpressionCompileResult newExpressions) {
			Utils.CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(_additionalStatements, expressions, newExpressions, () => { var temp = _createTemporaryVariable(); return _variables[temp].Name; });
		}

		private void CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(IList<JsExpression> expressions, JsExpression newExpression) {
			CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(expressions, new ExpressionCompileResult(newExpression, new JsStatement[0]));
		}

		private JsExpression InnerCompile(ArgumentForCall argument, bool usedMultipleTimes, IList<JsExpression> expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression, bool returnMultidimArrayValueByReference = false) {
			var result = CloneAndCompile(argument, returnValueIsImportant: true, returnMultidimArrayValueByReference: returnMultidimArrayValueByReference);

			bool needsTemporary = usedMultipleTimes && IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(result.Expression);
			if (result.AdditionalStatements.Count > 0 || needsTemporary) {
				CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression, result);
			}

			_additionalStatements.AddRange(result.AdditionalStatements);

			if (needsTemporary) {
				var temp = _createTemporaryVariable();
				_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, result.Expression));
				return JsExpression.Identifier(_variables[temp].Name);
			}
			else {
				return result.Expression;
			}
		}

		private JsExpression InnerCompile(ArgumentForCall argument, bool usedMultipleTimes, ref JsExpression expressionThatHasToBeEvaluatedInOrderBeforeThisExpression, bool returnMultidimArrayValueByReference = false) {
			var l = new List<JsExpression>();
			if (expressionThatHasToBeEvaluatedInOrderBeforeThisExpression != null)
				l.Add(expressionThatHasToBeEvaluatedInOrderBeforeThisExpression);
			var r = InnerCompile(argument, usedMultipleTimes, l, returnMultidimArrayValueByReference);
			if (l.Count > 0)
				expressionThatHasToBeEvaluatedInOrderBeforeThisExpression = l[0];
			return r;
		}

		private JsExpression InnerCompile(ArgumentForCall argument, bool usedMultipleTimes, bool returnMultidimArrayValueByReference = false) {
			JsExpression _ = null;
			return InnerCompile(argument, usedMultipleTimes, ref _, returnMultidimArrayValueByReference);
		}

		private JsExpression InnerCompile(ExpressionSyntax node, bool usedMultipleTimes, IList<JsExpression> expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression, bool returnMultidimArrayValueByReference = false) {
			return InnerCompile(new ArgumentForCall(node), usedMultipleTimes, expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression, returnMultidimArrayValueByReference);
		}

		private JsExpression InnerCompile(ExpressionSyntax node, bool usedMultipleTimes, ref JsExpression expressionThatHasToBeEvaluatedInOrderBeforeThisExpression, bool returnMultidimArrayValueByReference = false) {
			return InnerCompile(new ArgumentForCall(node), usedMultipleTimes, ref expressionThatHasToBeEvaluatedInOrderBeforeThisExpression, returnMultidimArrayValueByReference);
		}

		private JsExpression InnerCompile(ExpressionSyntax node, bool usedMultipleTimes, bool returnMultidimArrayValueByReference = false) {
			return InnerCompile(new ArgumentForCall(node), usedMultipleTimes, returnMultidimArrayValueByReference);
		}

		private JsExpression CompileTypedConstant(Tuple<ITypeSymbol, object> constant) {
			if (constant.Item2 == null) {
				return JsExpression.Null;
			}
			else if (constant.Item2 is IReadOnlyList<Tuple<ITypeSymbol, object>>) {
				var c = (IReadOnlyList<Tuple<ITypeSymbol, object>>)constant.Item2;
				var elements = new JsExpression[c.Count];
				for (int i = 0; i < c.Count; i++)
					elements[i] = CompileTypedConstant(c[i]);
				return JsExpression.ArrayLiteral(elements);
			}
			else if (constant.Item2 is ITypeSymbol) {
				return InstantiateType((ITypeSymbol)constant.Item2);
			}
			else if (constant.Item1.TypeKind == TypeKind.Enum) {
				var field = constant.Item1.GetFields().FirstOrDefault(f => Equals(f.ConstantValue, constant.Item2));
				if (field == null)
					return JSModel.Utils.MakeConstantExpression(constant.Item2);

				var impl = _metadataImporter.GetFieldSemantics(field.OriginalDefinition);
				switch (impl.Type) {
					case FieldScriptSemantics.ImplType.Field:
						return JsExpression.Member(InstantiateType(constant.Item1), impl.Name);
					case FieldScriptSemantics.ImplType.Constant:
						return JSModel.Utils.MakeConstantExpression(impl.Value);
					default:
						_errorReporter.Message(Messages._7509, field.FullyQualifiedName());
						return JsExpression.Null;
				}
			}
			else {
				return JSModel.Utils.MakeConstantExpression(constant.Item2);
			}
		}

		private JsExpression Visit(SyntaxNode node, bool returnValueIsImportant, bool returnMultidimArrayValueByReference) {
			var oldReturnValueIsImportant = _returnValueIsImportant;
			var oldReturnMultidimArrayValueByReference = _returnMultidimArrayValueByReference;
			_returnValueIsImportant = returnValueIsImportant;
			_returnMultidimArrayValueByReference = returnMultidimArrayValueByReference;
			try {
				return Visit(node);
			}
			finally {
				_returnValueIsImportant = oldReturnValueIsImportant;
				_returnMultidimArrayValueByReference = oldReturnMultidimArrayValueByReference;
			}
		}

		private bool IsIntegerType(ITypeSymbol type) {
			type = type.UnpackNullable();

			return type.SpecialType == SpecialType.System_Byte
			    || type.SpecialType == SpecialType.System_SByte
			    || type.SpecialType == SpecialType.System_Char
			    || type.SpecialType == SpecialType.System_Int16
			    || type.SpecialType == SpecialType.System_UInt16
			    || type.SpecialType == SpecialType.System_Int32
			    || type.SpecialType == SpecialType.System_UInt32
			    || type.SpecialType == SpecialType.System_Int64
			    || type.SpecialType == SpecialType.System_UInt64;
		}

		private bool IsUnsignedType(ITypeSymbol type) {
			type = type.UnpackNullable();

			return type.SpecialType == SpecialType.System_Byte
			    || type.SpecialType == SpecialType.System_UInt16
			    || type.SpecialType == SpecialType.System_UInt32
			    || type.SpecialType == SpecialType.System_UInt64;
		}

		private bool IsNullableBooleanType(ITypeSymbol type) {
			return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
			    && ((INamedTypeSymbol)type).TypeArguments[0].SpecialType == SpecialType.System_Boolean;
		}

		private bool IsAssignmentOperator(SyntaxNode node) {
			var kind = node.CSharpKind();
			return kind == SyntaxKind.AddAssignmentExpression
			    || kind == SyntaxKind.AndAssignmentExpression
			    || kind == SyntaxKind.DivideAssignmentExpression
			    || kind == SyntaxKind.ExclusiveOrAssignmentExpression
			    || kind == SyntaxKind.LeftShiftAssignmentExpression
			    || kind == SyntaxKind.ModuloAssignmentExpression
			    || kind == SyntaxKind.MultiplyAssignmentExpression
			    || kind == SyntaxKind.OrAssignmentExpression
			    || kind == SyntaxKind.RightShiftAssignmentExpression
			    || kind == SyntaxKind.SubtractAssignmentExpression;
		}

		private bool IsMutableValueType(ITypeSymbol type) {
			return Utils.IsMutableValueType(type, _metadataImporter);
		}

		private JsExpression MaybeCloneValueType(JsExpression input, ExpressionSyntax csharpInput, ITypeSymbol type, bool forceClone = false) {
			return Utils.MaybeCloneValueType(input, csharpInput, type, _metadataImporter, _runtimeLibrary, this, forceClone);
		}

		private JsExpression MaybeCloneValueType(JsExpression input, ArgumentForCall? csharpInput, ITypeSymbol type, bool forceClone = false) {
			return MaybeCloneValueType(input, csharpInput != null ? csharpInput.Value.Argument : null, type, forceClone);
		}

		private JsExpression MaybeCloneValueType(JsExpression input, ITypeSymbol type, bool forceClone = false) {
			return MaybeCloneValueType(input, (ExpressionSyntax)null, type, forceClone);
		}

		private INamedTypeSymbol GetContainingType(SyntaxNode syntax) {
			syntax = syntax.Parent;
			while (syntax != null) {
				if (syntax is TypeDeclarationSyntax)
					return (INamedTypeSymbol)_semanticModel.GetDeclaredSymbol(syntax);
				else
					syntax = syntax.Parent;
			}
			_errorReporter.InternalError("No containing type found for " + syntax);
			return null;
		}

		private IMethodSymbol GetContainingMethod(SyntaxNode syntax) {
			syntax = syntax.Parent;
			while (syntax != null) {
				if (syntax is MethodDeclarationSyntax || syntax is AccessorDeclarationSyntax || syntax is ConstructorDeclarationSyntax || syntax is OperatorDeclarationSyntax)
					return (IMethodSymbol)_semanticModel.GetDeclaredSymbol(syntax);
				else if (syntax is SimpleLambdaExpressionSyntax || syntax is ParenthesizedLambdaExpressionSyntax || syntax is AnonymousMethodExpressionSyntax)
					return (IMethodSymbol)_semanticModel.GetSymbolInfo(syntax).Symbol;
				else
					syntax = syntax.Parent;
			}
			_errorReporter.InternalError("No containing method found for " + syntax);
			return null;
		}

		private bool IsReadonlyField(ExpressionSyntax r) {
			for (;;) {
				var sym = ModelExtensions.GetSymbolInfo(_semanticModel, r).Symbol as IFieldSymbol;
				if (sym == null || sym.Type.TypeKind != TypeKind.Struct)
					return false;
				if (sym.IsReadOnly)
					return true;

				var mr = r as MemberAccessExpressionSyntax;
				if (mr == null)
					return false;
				r = mr.Expression;
			}
		}

		private JsExpression CompileThis() {
			if (_thisAlias != null) {
				return JsExpression.Identifier(_thisAlias);
			}
			else if (_nestedFunctionContext != null && _nestedFunctionContext.CapturedByRefVariables.Count != 0) {
				return JsExpression.Member(JsExpression.This, _namer.ThisAlias);
			}
			else {
				return JsExpression.This;
			}
		}

		private JsExpression CompileLocal(ISymbol variable, bool returnReference) {
			var data = _variables[variable];
			if (data.UseByRefSemantics) {
				var target = _nestedFunctionContext != null && _nestedFunctionContext.CapturedByRefVariables.Contains(variable)
				           ? (JsExpression)JsExpression.Member(JsExpression.This, data.Name)	// If using a captured by-ref variable, we access it using this.name.$
				           : (JsExpression)JsExpression.Identifier(data.Name);

				return returnReference ? target : JsExpression.Member(target, "$");
			}
			else {
				return JsExpression.Identifier(_variables[variable].Name);
			}
		}

		private JsExpression InstantiateType(ITypeSymbol type) {
			return _runtimeLibrary.InstantiateType(type, this);
		}

		private JsExpression InstantiateTypeForExpressionTree(ITypeSymbol type) {
			JsIdentifierExpression result;
			if (_anonymousTypeCache.TryGetValue(type, out result))
				return result;

			if (type.IsAnonymousType) {
				var temp = _createTemporaryVariable();
				var tempname = _variables[temp].Name;
				var expr = _runtimeLibrary.GetAnonymousTypeInfo((INamedTypeSymbol)type, this);

				_additionalStatements.Add(JsStatement.Var(tempname, expr));
				return _anonymousTypeCache[type] = JsExpression.Identifier(tempname);
			}
			else {
				return _runtimeLibrary.InstantiateType(type, this);
			}
		}

		private JsIdentifierExpression InstantiateTransparentType(ITypeSymbol type, IEnumerable<Tuple<JsExpression, string>> members) {
			JsIdentifierExpression result;
			if (_transparentTypeCache.TryGetValue(type, out result))
				return result;

			var expr = _runtimeLibrary.GetTransparentTypeInfo(members, this);
			var temp = _createTemporaryVariable();
			var tempname = _variables[temp].Name;
			_additionalStatements.Add(JsStatement.Var(tempname, expr));
			return _transparentTypeCache[type] = JsExpression.Identifier(tempname);
		}

		private ExpressionTreeBuilder CreateExpressionTreeBuilder() {
			return new ExpressionTreeBuilder(_semanticModel,
			                                 _metadataImporter,
			                                 _errorReporter,
			                                 () => { var v = _createTemporaryVariable(); return _variables[v].Name; },
			                                 _variables,
			                                 (m, t, a) => {
			                                     var c = Clone();
			                                     c._additionalStatements = new List<JsStatement>();
			                                     var sem = _metadataImporter.GetMethodSemantics(m.OriginalDefinition);
			                                     if (sem.Type == MethodScriptSemantics.ImplType.InlineCode) {
			                                         var tokens = InlineCodeMethodCompiler.Tokenize(m, sem.LiteralCode, _ => {});
			                                         if (tokens != null) {
			                                             for (int i = 0; i < a.Length; i++) {
			                                                 if (tokens.Count(k => k.Type == InlineCodeToken.TokenType.Parameter && k.Index == i) > 1) {
			                                                     if (IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(a[i])) {
			                                                         var temp = _createTemporaryVariable();
			                                                         c._additionalStatements.Add(JsStatement.Var(_variables[temp].Name, a[i]));
			                                                         a[i] = JsExpression.Identifier(_variables[temp].Name);
			                                                     }
			                                                 }
			                                             }
			                                         }
			                                     }
			                                     var e = c.CompileMethodInvocation(_metadataImporter.GetMethodSemantics(m.OriginalDefinition), m, new[] { m.IsStatic ? InstantiateType(m.ContainingType) : t }.Concat(a).ToList(), false);
			                                     return new ExpressionCompileResult(e, c._additionalStatements);
			                                 },
			                                 InstantiateTypeForExpressionTree,
			                                 InstantiateTransparentType,
			                                 t => _transparentTypeCache[t],
			                                 t => _runtimeLibrary.Default(t, this),
			                                 m => _runtimeLibrary.GetMember(m, this),
			                                 v => _runtimeLibrary.GetExpressionForLocal(v.Name, CompileLocal(v, false), (v is ILocalSymbol ? ((ILocalSymbol)v).Type : ((IParameterSymbol)v).Type), this),
			                                 CompileThis()
			                                );
		}
	}
}
