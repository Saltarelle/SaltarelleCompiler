using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.StateMachineRewrite;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler.Expressions {
	partial class ExpressionCompiler {
		private JsExpression ProcessConversion(JsExpression js, ExpressionSyntax cs) {
			var typeInfo = _semanticModel.GetTypeInfo(cs);
			var conversion = _semanticModel.GetConversion(cs);
			return PerformConversion(js, conversion, typeInfo.Type, typeInfo.ConvertedType, cs);
		}

		private JsExpression PerformConversion(JsExpression input, Conversion c, ITypeSymbol fromType, ITypeSymbol toType, ExpressionSyntax csharpInput) {
			if (c.IsIdentity) {
				return input;
			}
			else if (c.IsMethodGroup || c.IsAnonymousFunction) {
				return input;	// Conversion should have been performed as part of processing the converted expression
			}
			else if (c.IsReference) {
				if (fromType == null)
					return input;	// Null literal (Isn't this a NullLiteral conversion? Roslyn bug?)
				if (toType.TypeKind == TypeKind.Array && fromType.TypeKind == TypeKind.Array)	// Array covariance / contravariance.
					return input;
				else if (toType.TypeKind == TypeKind.Dynamic)
					return input;
				else if (toType.TypeKind == TypeKind.Delegate && fromType.TypeKind == TypeKind.Delegate && toType.SpecialType != SpecialType.System_MulticastDelegate && fromType.SpecialType != SpecialType.System_MulticastDelegate)
					return input;	// Conversion between compatible delegate types.
				else if (c.IsImplicit)
					return _runtimeLibrary.Upcast(input, fromType, toType, this);
				else
					return _runtimeLibrary.Downcast(input, fromType, toType, this);
			}
			else if (c.IsNumeric || c.IsNullable) {
				var result = input;
				if (fromType.IsNullable() && !toType.IsNullable())
					result = _runtimeLibrary.FromNullable(result, this);

				if (toType.IsNullable() && !fromType.IsNullable()) {
					var otherConversion = _compilation.ClassifyConversion(fromType, toType.UnpackNullable());
					if (otherConversion.IsUserDefined)
						return PerformConversion(input, otherConversion, fromType, toType.UnpackNullable(), csharpInput);	// Seems to be a Roslyn bug: implicit user-defined conversions are returned as nullable conversions
				}

				var unpackedFromType = fromType.UnpackNullable();
				var unpackedToType = toType.UnpackNullable();
				if (!IsIntegerType(unpackedFromType) && unpackedFromType.TypeKind != TypeKind.Enum && IsIntegerType(unpackedToType)) {
					result = _runtimeLibrary.FloatToInt(result, this);

					if (fromType.IsNullable() && toType.IsNullable()) {
						result = _runtimeLibrary.Lift(result, this);
					}
				}
				return result;
			}
			else if (c.IsDynamic) {
				JsExpression result;
				if (toType.IsNullable()) {
					// Unboxing to nullable type.
					result = _runtimeLibrary.Downcast(input, fromType, toType.UnpackNullable(), this);
				}
				else if (toType.TypeKind == TypeKind.Struct) {
					// Unboxing to non-nullable type.
					result = _runtimeLibrary.FromNullable(_runtimeLibrary.Downcast(input, fromType, toType, this), this);
				}
				else {
					// Converting to a boring reference type.
					result = _runtimeLibrary.Downcast(input, fromType, toType, this);
				}
				return MaybeCloneValueType(result, toType, forceClone: true);
			}
			else if (c.IsEnumeration) {
				if (csharpInput != null && toType.UnpackNullable().TypeKind == TypeKind.Enum) {
					var constant = _semanticModel.GetConstantValue(csharpInput);
					if (constant.HasValue && Equals(constant.Value, 0)) {
						return _runtimeLibrary.Default(toType.UnpackNullable(), this);
					}
				}
				if (fromType.IsNullable() && !toType.IsNullable())
					return _runtimeLibrary.FromNullable(input, this);
				return input;
			}
			else if (c.IsBoxing) {
				var box = MaybeCloneValueType(input, fromType);

				// Conversion between type parameters are classified as boxing conversions, so it's sometimes an upcast, sometimes a downcast.
				if (toType.TypeKind == TypeKind.Dynamic) {
					return box;
				}
				else {
					var fromTypeParam = fromType.UnpackNullable() as ITypeParameterSymbol;
					if (fromTypeParam != null && !fromTypeParam.ConstraintTypes.Contains(toType))
						return _runtimeLibrary.Downcast(box, fromType, toType, this);
					else
						return _runtimeLibrary.Upcast(box, fromType, toType, this);
				}
			}
			else if (c.IsUnboxing) {
				JsExpression result;
				if (toType.IsNullable()) {
					result = _runtimeLibrary.Downcast(input, fromType, toType.UnpackNullable(), this);
				}
				else {
					result = _runtimeLibrary.Downcast(input, fromType, toType, this);
					if (toType.TypeKind == TypeKind.Struct)
						result = _runtimeLibrary.FromNullable(result, this);	// hidden gem in the C# spec: conversions involving type parameter which are not known to not be unboxing are considered unboxing conversions.
				}
				return MaybeCloneValueType(result, toType, forceClone: true);
			}
			else if (c.IsUserDefined) {
				input = PerformConversion(input, c.UserDefinedFromConversion(), fromType, c.MethodSymbol.Parameters[0].Type, csharpInput);
				var impl = _metadataImporter.GetMethodSemantics(c.MethodSymbol.OriginalDefinition);
				var result = CompileMethodInvocation(impl, c.MethodSymbol, new[] { InstantiateType(c.MethodSymbol.ContainingType), input }, false);
				if (_semanticModel.IsLiftedConversion(c, fromType))
					result = _runtimeLibrary.Lift(result, this);
				result = PerformConversion(result, c.UserDefinedToConversion(), c.MethodSymbol.ReturnType, toType, csharpInput);
				return result;
			}
			else if (c.IsNullLiteral || c.IsConstantExpression) {
				return input;
			}
			else {
				_errorReporter.InternalError("Conversion " + c + " is not implemented");
				return JsExpression.Null;
			}
		}

		private JsExpression PerformMethodGroupConversionOnNormalMethod(IMethodSymbol method, ITypeSymbol delegateType, bool isBaseCall, Func<bool, JsExpression> getTarget, MethodScriptSemantics methodSemantics, DelegateScriptSemantics delegateSemantics) {
			bool isExtensionMethodGroupConversion = method.ReducedFrom != null;
			method = method.UnReduceIfExtensionMethod();

			if (methodSemantics.ExpandParams != delegateSemantics.ExpandParams) {
				_errorReporter.Message(Messages._7524, method.FullyQualifiedName(), delegateType.FullyQualifiedName());
				return JsExpression.Null;
			}

			var typeArguments = methodSemantics.IgnoreGenericArguments ? ImmutableArray<ITypeSymbol>.Empty : method.TypeArguments;

			JsExpression result;

			if (isBaseCall) {
				// base.Method
				var jsTarget = getTarget(true);
				result = _runtimeLibrary.BindBaseCall(method, jsTarget, this);
			}
			else if (isExtensionMethodGroupConversion) {
				IList<string> parameters;
				JsExpression body;
				var jsTarget = getTarget(true);
				if (methodSemantics.ExpandParams) {
					parameters = ImmutableArray<string>.Empty;
					body = JsExpression.Invocation(JsExpression.Member(JsExpression.Member(InstantiateType(method.ContainingType), methodSemantics.Name), "apply"), JsExpression.Null, JsExpression.Invocation(JsExpression.Member(JsExpression.ArrayLiteral(jsTarget), "concat"), JsExpression.Invocation(JsExpression.Member(JsExpression.Member(JsExpression.Member(JsExpression.Identifier("Array"), "prototype"), "slice"), "call"), JsExpression.Identifier("arguments"))));
				}
				else {
					parameters = new string[method.Parameters.Length - 1];
					for (int i = 0; i < parameters.Count; i++)
						parameters[i] = _variables[_createTemporaryVariable()].Name;
					body = CompileMethodInvocation(methodSemantics, method, new[] { InstantiateType(method.ContainingType), jsTarget }.Concat(parameters.Select(JsExpression.Identifier)).ToList(), false);
				}
				result = JsExpression.FunctionDefinition(parameters, method.ReturnType.SpecialType == SpecialType.System_Void ? (JsStatement)body : JsStatement.Return(body));
				if (UsesThisVisitor.Analyze(body))
					result = _runtimeLibrary.Bind(result, JsExpression.This, this);
			}
			else {
				JsExpression jsTarget, jsMethod;

				if (method.IsStatic) {
					jsTarget = null;
					jsMethod = JsExpression.Member(InstantiateType(method.ContainingType), methodSemantics.Name);
				}
				else {
					jsTarget = getTarget(true);
					jsMethod = JsExpression.Member(jsTarget, methodSemantics.Name);
				}

				if (typeArguments.Length > 0) {
					jsMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments, this);
				}

				result = jsTarget != null ? _runtimeLibrary.Bind(jsMethod, jsTarget, this) : jsMethod;
			}

			if (delegateSemantics.BindThisToFirstParameter)
				result = _runtimeLibrary.BindFirstParameterToThis(result, this);

			return result;
		}

		private JsExpression PerformMethodGroupConversionOnInlineCodeMethod(IMethodSymbol method, ITypeSymbol delegateType, bool isBaseCall, Func<bool, JsExpression> getTarget, MethodScriptSemantics methodSemantics, DelegateScriptSemantics delegateSemantics) {
			bool isExtensionMethodGroupConversion = method.ReducedFrom != null;
			method = method.UnReduceIfExtensionMethod();
			string code = isBaseCall ? methodSemantics.NonVirtualInvocationLiteralCode : methodSemantics.NonExpandedFormLiteralCode;
			var tokens = InlineCodeMethodCompiler.Tokenize(method, code, s => _errorReporter.Message(Messages._7525, s));
			if (tokens == null) {
				return JsExpression.Null;
			}
			else if (tokens.Any(t => t.Type == InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier)) {
				_errorReporter.Message(Messages._7523, method.FullyQualifiedName(), "it uses a literal string as code ({@arg})");
				return JsExpression.Null;
			}
			else if (tokens.Any(t => t.Type == InlineCodeToken.TokenType.Parameter && t.IsExpandedParamArray)) {
				_errorReporter.Message(Messages._7523, method.FullyQualifiedName(), "it has an expanded param array parameter ({*arg})");
				return JsExpression.Null;
			}

			var parameters = new string[method.Parameters.Length - (delegateSemantics.ExpandParams ? 1 : 0) - (isExtensionMethodGroupConversion ? 1 : 0)];
			for (int i = 0; i < parameters.Length; i++)
				parameters[i] = _variables[_createTemporaryVariable()].Name;

			var jsTarget = method.IsStatic && !isExtensionMethodGroupConversion ? JsExpression.Null : getTarget(tokens.Count(t => t.Type == InlineCodeToken.TokenType.This) > 1);
			var arguments = new List<JsExpression>();
			if (isExtensionMethodGroupConversion)
				arguments.Add(jsTarget);
			arguments.AddRange(parameters.Select(p => (JsExpression)JsExpression.Identifier(p)));
			if (delegateSemantics.ExpandParams)
				arguments.Add(JsExpression.Invocation(JsExpression.Member(JsExpression.Member(JsExpression.Member(JsExpression.Identifier("Array"), "prototype"), "slice"), "call"), JsExpression.Identifier("arguments"), JsExpression.Number(parameters.Length)));

			bool usesThis;
			JsExpression result;
			if (method.ReturnType.SpecialType == SpecialType.System_Void) {
				var list = InlineCodeMethodCompiler.CompileStatementListInlineCodeMethodInvocation(method,
				                                                                                   tokens,
				                                                                                   method.IsStatic ? null : jsTarget,
				                                                                                   arguments,
				                                                                                   ResolveTypeForInlineCode,
				                                                                                   t => _runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, this),
				                                                                                   s => _errorReporter.Message(Messages._7525, s));
				var body = JsStatement.Block(list);
				result = JsExpression.FunctionDefinition(parameters, body);
				usesThis = UsesThisVisitor.Analyze(body);
			}
			else {
				var body = InlineCodeMethodCompiler.CompileExpressionInlineCodeMethodInvocation(method,
				                                                                                tokens,
				                                                                                method.IsStatic ? null : jsTarget,
				                                                                                arguments,
				                                                                                ResolveTypeForInlineCode,
				                                                                                t => _runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, this),
				                                                                                s => _errorReporter.Message(Messages._7525, s));
				result = JsExpression.FunctionDefinition(parameters, JsStatement.Return(body));
				usesThis = UsesThisVisitor.Analyze(body);
			}

			if (usesThis)
				result = _runtimeLibrary.Bind(result, JsExpression.This, this);
			if (delegateSemantics.BindThisToFirstParameter)
				result = _runtimeLibrary.BindFirstParameterToThis(result, this);
			return result;
		}

		private JsExpression PerformMethodGroupConversionOnStaticMethodWithThisAsFirstArgument(IMethodSymbol method, ITypeSymbol delegateType, Func<bool, JsExpression> getTarget, MethodScriptSemantics methodSemantics, DelegateScriptSemantics delegateSemantics) {
			if (methodSemantics.ExpandParams != delegateSemantics.ExpandParams) {
				_errorReporter.Message(Messages._7524, method.FullyQualifiedName(), delegateType.FullyQualifiedName());
				return JsExpression.Null;
			}

			JsExpression result;
			if (methodSemantics.ExpandParams) {
				var body = JsExpression.Invocation(JsExpression.Member(JsExpression.Member(InstantiateType(method.ContainingType), methodSemantics.Name), "apply"), JsExpression.Null, JsExpression.Invocation(JsExpression.Member(JsExpression.ArrayLiteral(JsExpression.This), "concat"), JsExpression.Invocation(JsExpression.Member(JsExpression.Member(JsExpression.Member(JsExpression.Identifier("Array"), "prototype"), "slice"), "call"), JsExpression.Identifier("arguments"))));
				result = JsExpression.FunctionDefinition(new string[0], method.ReturnType.SpecialType == SpecialType.System_Void ? (JsStatement)body : JsStatement.Return(body));
			}
			else {
				var parameters = new string[method.Parameters.Length];
				for (int i = 0; i < method.Parameters.Length; i++)
					parameters[i] = _variables[_createTemporaryVariable()].Name;

				var body = JsExpression.Invocation(JsExpression.Member(InstantiateType(method.ContainingType), methodSemantics.Name), new[] { JsExpression.This }.Concat(parameters.Select(p => (JsExpression)JsExpression.Identifier(p))));
				result = JsExpression.FunctionDefinition(parameters, method.ReturnType.SpecialType == SpecialType.System_Void ? (JsStatement)body : JsStatement.Return(body));
			}

			result = _runtimeLibrary.Bind(result, getTarget(false), this);
			if (delegateSemantics.BindThisToFirstParameter)
				result = _runtimeLibrary.BindFirstParameterToThis(result, this);
			return result;
		}

		private JsExpression PerformMethodGroupConversion(Func<bool, JsExpression> getTarget, INamedTypeSymbol targetType, IMethodSymbol symbol, bool isNonVirtualLookup) {
			var methodSemantics = _metadataImporter.GetMethodSemantics(symbol.UnReduceIfExtensionMethod().OriginalDefinition);
			var delegateSemantics = _metadataImporter.GetDelegateSemantics(targetType.OriginalDefinition);
			switch (methodSemantics.Type) {
				case MethodScriptSemantics.ImplType.NormalMethod:
					return PerformMethodGroupConversionOnNormalMethod(symbol, targetType, symbol.IsOverridable() && isNonVirtualLookup, getTarget, methodSemantics, delegateSemantics);
				case MethodScriptSemantics.ImplType.InlineCode:
					return PerformMethodGroupConversionOnInlineCodeMethod(symbol, targetType, symbol.IsOverridable() && isNonVirtualLookup, getTarget, methodSemantics, delegateSemantics);
				case MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument:
					return PerformMethodGroupConversionOnStaticMethodWithThisAsFirstArgument(symbol, targetType, getTarget, methodSemantics, delegateSemantics);
				default:
					_errorReporter.Message(Messages._7523, symbol.FullyQualifiedName(), "it is not a normal method");
					return JsExpression.Null;
			}
		}

		private JsExpression PerformExpressionTreeLambdaConversion(IReadOnlyList<ParameterSyntax> parameters, ExpressionSyntax body) {
			var tree = CreateExpressionTreeBuilder().BuildExpressionTree(parameters, body);
			_additionalStatements.AddRange(tree.AdditionalStatements);
			return tree.Expression;
		}

		private JsExpression CompileLambda(SyntaxNode lambdaNode, IReadOnlyList<IParameterSymbol> lambdaParameters, SyntaxNode body, bool isAsync, INamedTypeSymbol delegateType) {
			return BindToCaptureObject(lambdaNode, delegateType, newContext => {
				var methodType = delegateType.DelegateInvokeMethod;
				var delegateSemantics = _metadataImporter.GetDelegateSemantics(delegateType.OriginalDefinition);

				StateMachineType smt = StateMachineType.NormalMethod;
				ITypeSymbol taskGenericArgument = null;
				if (isAsync) {
					smt = methodType.ReturnsVoid ? StateMachineType.AsyncVoid : StateMachineType.AsyncTask;
					taskGenericArgument = methodType.ReturnType is INamedTypeSymbol && ((INamedTypeSymbol)methodType.ReturnType).TypeArguments.Length > 0 ? ((INamedTypeSymbol)methodType.ReturnType).TypeArguments[0] : null;
				}

				if (body is BlockSyntax) {
					return _createInnerCompiler(newContext, _activeRangeVariableSubstitutions).CompileMethod(lambdaParameters, _variables, (BlockSyntax)body, false, delegateSemantics.ExpandParams, smt, taskGenericArgument);
				}
				else if (body is ExpressionSyntax) {
					var lambdaReturnType = isAsync ? (taskGenericArgument ?? _compilation.GetSpecialType(SpecialType.System_Void)) : methodType.ReturnType;
					return _createInnerCompiler(newContext, _activeRangeVariableSubstitutions).CompileMethod(lambdaParameters, _variables, (ExpressionSyntax)body, lambdaReturnType, false, delegateSemantics.ExpandParams, smt, taskGenericArgument);
				}
				else {
					_errorReporter.InternalError("Unsupported body node for lambda");
					return JsExpression.FunctionDefinition(new String[0], JsStatement.Block());
				}
			});
		}

		private JsExpression BindToCaptureObject(SyntaxNode node, INamedTypeSymbol delegateType, Func<NestedFunctionContext, JsFunctionDefinitionExpression> compileBody) {
			var f = LocalUsageGatherer.GatherInfo(_semanticModel, node);

			var capturedByRefVariables = f.DirectlyOrIndirectlyUsedLocals.Where(v => _variables[v].UseByRefSemantics && !v.DeclaringSyntaxReferences[0].GetSyntax().Ancestors(true).Contains(node)).ToList();
			bool captureThis = (_thisAlias == null && f.DirectlyOrIndirectlyUsesThis);
			var newContext = new NestedFunctionContext(capturedByRefVariables);

			var compiledBody = compileBody(newContext);

			JsExpression captureObject;
			if (newContext.CapturedByRefVariables.Count > 0) {
				var toCapture = newContext.CapturedByRefVariables.Select(v => new JsObjectLiteralProperty(_variables[v].Name, CompileLocal(v, true))).ToList();
				if (captureThis)
					toCapture.Add(new JsObjectLiteralProperty(_namer.ThisAlias, CompileThis()));
				captureObject = JsExpression.ObjectLiteral(toCapture);
			}
			else if (captureThis) {
				captureObject = CompileThis();
			}
			else {
				captureObject = null;
			}

			var result = captureObject != null ? _runtimeLibrary.Bind(compiledBody, captureObject, this) : compiledBody;
			var delegateSemantics = _metadataImporter.GetDelegateSemantics(delegateType.OriginalDefinition);
			if (delegateSemantics.BindThisToFirstParameter)
				result = _runtimeLibrary.BindFirstParameterToThis(result, this);
			return result;
		}
	}
}
