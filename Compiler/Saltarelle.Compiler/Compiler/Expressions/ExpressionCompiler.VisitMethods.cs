using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler.Expressions {
	partial class ExpressionCompiler {
		public override JsExpression Visit(SyntaxNode node) {
			var expr = node as ExpressionSyntax;
			if (expr == null) {
				_errorReporter.InternalError("Unexpected node " + node);
				return JsExpression.Null;
			}

			bool oldIgnoreConversion = _ignoreConversion;
			_ignoreConversion = false;
			var result = base.Visit(node);

			return oldIgnoreConversion ? result : ProcessConversion(result, expr);
		}

		public override JsExpression VisitBinaryExpression(BinaryExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

			if (symbol != null && symbol.MethodKind == MethodKind.UserDefinedOperator) {
				var impl = _metadataImporter.GetMethodSemantics(symbol);
				if (impl.Type != MethodScriptSemantics.ImplType.NativeOperator) {
					Func<JsExpression, JsExpression, JsExpression> invocation = (a, b) => CompileMethodInvocation(impl, symbol, new[] { InstantiateType(symbol.ContainingType), a, b }, false);
					if (IsAssignmentOperator(node))
						return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), null, invocation, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, invocation, _semanticModel.IsLiftedOperator(node));
				}
			}

			switch (node.CSharpKind()) {
				case SyntaxKind.SimpleAssignmentExpression:
					return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), JsExpression.Assign, (a, b) => b, _returnValueIsImportant, false, oldValueIsImportant: false);

				// Compound assignment operators

				case SyntaxKind.AddAssignmentExpression: {
					var leftSymbol = _semanticModel.GetSymbolInfo(node.Left).Symbol;
					if (leftSymbol is IEventSymbol) {
						return CompileEventAddOrRemove(node.Left, (IEventSymbol)leftSymbol, node.Right, true);
					}
					else {
						if (symbol != null && symbol.ContainingType != null && symbol.ContainingType.TypeKind == TypeKind.Delegate) {
							var del = _compilation.GetSpecialType(SpecialType.System_Delegate);
							var combine = (IMethodSymbol)del.GetMembers("Combine").Single();
							var impl = _metadataImporter.GetMethodSemantics(combine);
							return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), null, (a, b) => CompileMethodInvocation(impl, combine, new[] { InstantiateType(del), a, b }, false), _returnValueIsImportant, false);
						}
						else {
							return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), JsExpression.AddAssign, JsExpression.Add, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));
						}
					}
				}

				case SyntaxKind.AndAssignmentExpression:
					if (IsNullableBooleanType(_semanticModel.GetTypeInfo(node.Left).Type))
						return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), null, (a, b) => _runtimeLibrary.LiftedBooleanAnd(a, b, this), _returnValueIsImportant, false);
					else
						return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), JsExpression.BitwiseAndAssign, JsExpression.BitwiseAnd, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.DivideAssignmentExpression:
					if (IsIntegerType(_semanticModel.GetTypeInfo(node).Type))
						return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), null, (a, b) => _runtimeLibrary.IntegerDivision(a, b, this), _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));
					else
						return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), JsExpression.DivideAssign, JsExpression.Divide, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.ExclusiveOrAssignmentExpression:
					return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), JsExpression.BitwiseXorAssign, JsExpression.BitwiseXor, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.LeftShiftAssignmentExpression:
					return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), JsExpression.LeftShiftAssign, JsExpression.LeftShift, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.ModuloAssignmentExpression:
					return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), JsExpression.ModuloAssign, JsExpression.Modulo, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.MultiplyAssignmentExpression:
					return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), JsExpression.MultiplyAssign, JsExpression.Multiply, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.OrAssignmentExpression:
					if (IsNullableBooleanType(_semanticModel.GetTypeInfo(node.Left).Type))
						return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), null, (a, b) => _runtimeLibrary.LiftedBooleanOr(a, b, this), _returnValueIsImportant, false);
					else
						return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), JsExpression.BitwiseOrAssign, JsExpression.BitwiseOr, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.RightShiftAssignmentExpression:
					if (IsUnsignedType(_semanticModel.GetTypeInfo(node).Type))
						return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), JsExpression.RightShiftUnsignedAssign, JsExpression.RightShiftUnsigned, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));
					else
						return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), JsExpression.RightShiftSignedAssign, JsExpression.RightShiftSigned, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.SubtractAssignmentExpression: {
					var leftSymbol = _semanticModel.GetSymbolInfo(node.Left).Symbol;
					if (leftSymbol is IEventSymbol) {
						return CompileEventAddOrRemove(node.Left, (IEventSymbol)leftSymbol, node.Right, false);
					}
					else {
						if (symbol != null && symbol.ContainingType != null && symbol.ContainingType.TypeKind == TypeKind.Delegate) {
							var del = _compilation.GetSpecialType(SpecialType.System_Delegate);
							var remove = (IMethodSymbol)del.GetMembers("Remove").Single();
							var impl = _metadataImporter.GetMethodSemantics(remove);
							return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), null, (a, b) => CompileMethodInvocation(impl, remove, new[] { InstantiateType(del), a, b }, false), _returnValueIsImportant, false);
						}
						else {
							return CompileCompoundAssignment(node.Left, new ArgumentForCall(node.Right), JsExpression.SubtractAssign, JsExpression.Subtract, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));
						}
					}
				}

				// Binary non-assigning operators

				case SyntaxKind.AddExpression:
					if (_semanticModel.GetTypeInfo(node.Left).Type.TypeKind == TypeKind.Delegate) {
						var del = _compilation.GetSpecialType(SpecialType.System_Delegate);
						var combine = (IMethodSymbol)del.GetMembers("Combine").Single();
						var impl = _metadataImporter.GetMethodSemantics(combine);
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => CompileMethodInvocation(impl, combine, new[] { InstantiateType(del), a, b }, false), false);
					}
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Add, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.BitwiseAndExpression:
					if (IsNullableBooleanType(_semanticModel.GetTypeInfo(node.Left).Type))
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => _runtimeLibrary.LiftedBooleanAnd(a, b, this), false);	// We have already lifted it, so it should not be lifted again.
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.BitwiseAnd, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.LogicalAndExpression:
					return CompileAndAlsoOrOrElse(node.Left, node.Right, true);

				case SyntaxKind.CoalesceExpression:
					return CompileCoalesce(node.Left, node.Right);

				case SyntaxKind.DivideExpression:
					if (IsIntegerType(_semanticModel.GetTypeInfo(node).Type))
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => _runtimeLibrary.IntegerDivision(a, b, this), _semanticModel.IsLiftedOperator(node));
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Divide, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.ExclusiveOrExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.BitwiseXor, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.GreaterThanExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Greater, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.GreaterThanOrEqualExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.GreaterOrEqual, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.EqualsExpression: {
					var leftType = _semanticModel.GetTypeInfo(node.Left).ConvertedType;
					if (leftType.TypeKind == TypeKind.Delegate) {
						var rightType = _semanticModel.GetTypeInfo(node.Right).ConvertedType;
						if (rightType.TypeKind == TypeKind.Delegate) {
							var delegateEquals = (IMethodSymbol)_compilation.GetSpecialType(SpecialType.System_Delegate).GetMembers("op_Equality").Single();
							var impl = _metadataImporter.GetMethodSemantics(delegateEquals);
							return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => CompileMethodInvocation(impl, delegateEquals, new[] { InstantiateType(delegateEquals.ContainingType), a, b }, false), false);
						}
					}

					return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => CanDoSimpleComparisonForEquals(node.Left, node.Right) ? JsExpression.Same(a, b) : _runtimeLibrary.ReferenceEquals(a, b, this), false);
				}

				case SyntaxKind.LeftShiftExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.LeftShift, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.LessThanExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Lesser, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.LessThanOrEqualExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.LesserOrEqual, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.ModuloExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Modulo, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.MultiplyExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Multiply, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.NotEqualsExpression: {
					var leftType = _semanticModel.GetTypeInfo(node.Left).ConvertedType;
					if (leftType.TypeKind == TypeKind.Delegate) {
						var rightType = _semanticModel.GetTypeInfo(node.Right).ConvertedType;
						if (rightType.TypeKind == TypeKind.Delegate) {
							var delegateNotEquals = (IMethodSymbol)_compilation.GetSpecialType(SpecialType.System_Delegate).GetMembers("op_Inequality").Single();
							var impl = _metadataImporter.GetMethodSemantics(delegateNotEquals);
							return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => CompileMethodInvocation(impl, delegateNotEquals, new[] { InstantiateType(delegateNotEquals.ContainingType), a, b }, false), false);
						}
					}

					return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => CanDoSimpleComparisonForEquals(node.Left, node.Right) ? JsExpression.NotSame(a, b) : _runtimeLibrary.ReferenceNotEquals(a, b, this), false);
				}

				case SyntaxKind.BitwiseOrExpression:
					if (IsNullableBooleanType(_semanticModel.GetTypeInfo(node.Left).Type))
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => _runtimeLibrary.LiftedBooleanOr(a, b, this), false);	// We have already lifted it, so it should not be lifted again.
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.BitwiseOr, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.LogicalOrExpression:
					return CompileAndAlsoOrOrElse(node.Left, node.Right, false);

				case SyntaxKind.RightShiftExpression:
					if (IsUnsignedType(_semanticModel.GetTypeInfo(node).Type))
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.RightShiftUnsigned, _semanticModel.IsLiftedOperator(node));
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.RightShiftSigned, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.SubtractExpression:
					if (_semanticModel.GetTypeInfo(node.Left).Type.TypeKind == TypeKind.Delegate) {
						var del = _compilation.GetSpecialType(SpecialType.System_Delegate);
						var remove = (IMethodSymbol)del.GetMembers("Remove").Single();
						var impl = _metadataImporter.GetMethodSemantics(remove);
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => CompileMethodInvocation(impl, remove, new[] { InstantiateType(del), a, b }, false), false);
					}
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Subtract, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.AsExpression:
					return _runtimeLibrary.TryDowncast(InnerCompile(node.Left, false), _semanticModel.GetTypeInfo(node.Left).Type, ((ITypeSymbol)_semanticModel.GetSymbolInfo(node.Right).Symbol).UnpackNullable(), this);

				case SyntaxKind.IsExpression:
					var targetType = ((ITypeSymbol)_semanticModel.GetSymbolInfo(node.Right).Symbol).UnpackNullable();
					return _runtimeLibrary.TypeIs(Visit(node.Left, true, _returnMultidimArrayValueByReference), _semanticModel.GetTypeInfo(node.Left).ConvertedType, targetType, this);

				default:
					_errorReporter.InternalError("Unsupported operator " + node.CSharpKind());
					return JsExpression.Null;
			}
		}

		public override JsExpression VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

			if (symbol != null && symbol.MethodKind == MethodKind.UserDefinedOperator) {
				var impl = _metadataImporter.GetMethodSemantics(symbol);
				if (impl.Type != MethodScriptSemantics.ImplType.NativeOperator) {
					if (node.CSharpKind() == SyntaxKind.PreIncrementExpression || node.CSharpKind() == SyntaxKind.PreDecrementExpression) {
						Func<JsExpression, JsExpression, JsExpression> invocation = (a, b) => CompileMethodInvocation(impl, symbol, new[] { InstantiateType(symbol.ContainingType), a }, false);
						return CompileCompoundAssignment(node.Operand, null, null, invocation, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node), false);
					}
					else {
						return CompileUnaryOperator(node.Operand, a => CompileMethodInvocation(impl, symbol, new[] { InstantiateType(symbol.ContainingType), a }, false), _semanticModel.IsLiftedOperator(node));
					}
				}
			}

			switch (node.CSharpKind()) {
				case SyntaxKind.PreIncrementExpression:
					return CompileCompoundAssignment(node.Operand, null, (a, b) => JsExpression.PrefixPlusPlus(a), (a, b) => JsExpression.Add(a, JsExpression.Number(1)), _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.PreDecrementExpression:
					return CompileCompoundAssignment(node.Operand, null, (a, b) => JsExpression.PrefixMinusMinus(a), (a, b) => JsExpression.Subtract(a, JsExpression.Number(1)), _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.UnaryMinusExpression:
					return CompileUnaryOperator(node.Operand, JsExpression.Negate, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.UnaryPlusExpression:
					return CompileUnaryOperator(node.Operand, JsExpression.Positive, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.LogicalNotExpression:
					return CompileUnaryOperator(node.Operand, JsExpression.LogicalNot, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.BitwiseNotExpression:
					return CompileUnaryOperator(node.Operand, JsExpression.BitwiseNot, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.AwaitExpression:
					return CompileAwait(node);

				default:
					_errorReporter.InternalError("Unsupported operator " + node.OperatorToken.CSharpKind());
					return JsExpression.Null;
			}
		}

		public override JsExpression VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

			if (symbol != null && symbol.MethodKind == MethodKind.UserDefinedOperator) {
				var impl = _metadataImporter.GetMethodSemantics(symbol);
				if (impl.Type != MethodScriptSemantics.ImplType.NativeOperator) {
					if (node.CSharpKind() == SyntaxKind.PostIncrementExpression || node.CSharpKind() == SyntaxKind.PostDecrementExpression) {
						Func<JsExpression, JsExpression, JsExpression> invocation = (a, b) => CompileMethodInvocation(impl, symbol, new[] { InstantiateType(symbol.ContainingType), _returnValueIsImportant ? MaybeCloneValueType(a, symbol.Parameters[0].Type) : a }, false);
						return CompileCompoundAssignment(node.Operand, null, null, invocation, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node), true);
					}
				}
			}

			switch (node.CSharpKind()) {
				case SyntaxKind.PostIncrementExpression:
					return CompileCompoundAssignment(node.Operand, null, (a, b) => JsExpression.PostfixPlusPlus(a), (a, b) => JsExpression.Add(a, JsExpression.Number(1)), _returnValueIsImportant, _semanticModel.IsLiftedOperator(node), returnValueBeforeChange: true);

				case SyntaxKind.PostDecrementExpression:
					return CompileCompoundAssignment(node.Operand, null, (a, b) => JsExpression.PostfixMinusMinus(a), (a, b) => JsExpression.Subtract(a, JsExpression.Number(1)), _returnValueIsImportant, _semanticModel.IsLiftedOperator(node), returnValueBeforeChange: true);

				default:
					_errorReporter.InternalError("Unsupported operator " + node.OperatorToken.CSharpKind());
					return JsExpression.Null;
			}
		}
		
		public override JsExpression VisitConditionalExpression(ConditionalExpressionSyntax node) {
			return CompileConditionalOperator(node.Condition, node.WhenTrue, node.WhenFalse);
		}

		public override JsExpression VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) {
			return node.Expression.Accept(this);	// Don't use Visit since that would double any conversion on the node.
		}

		public override JsExpression VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
			var conversion = _semanticModel.GetConversion(node);
			if (conversion.IsMethodGroup) {
				var targetType = _semanticModel.GetTypeInfo(node).ConvertedType;
				return PerformMethodGroupConversion(usedMultipleTimes => InnerCompile(node.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), (INamedTypeSymbol)targetType, (IMethodSymbol)symbol, node.IsNonVirtualAccess());
			}
			else {
				var targetType = _semanticModel.GetTypeInfo(node.Expression).ConvertedType;
				if (targetType.TypeKind == TypeKind.DynamicType) {
					return JsExpression.Member(InnerCompile(node.Expression, false), node.Name.Identifier.Text);
				}
				else {
					return HandleMemberRead(usedMultipleTimes => InnerCompile(node.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), _semanticModel.GetSymbolInfo(node).Symbol, node.IsNonVirtualAccess(), IsReadonlyField(node));
				}
			}
		}

		public override JsExpression VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node) {
			return CompileJsonConstructorCall(ConstructorScriptSemantics.Json(ImmutableArray<ISymbol>.Empty), ArgumentMap.Empty, node.Initializers.Select(init => Tuple.Create((ISymbol)_semanticModel.GetDeclaredSymbol(init), init.Expression)).ToList());
		}

		public override JsExpression VisitObjectCreationExpression(ObjectCreationExpressionSyntax node) {
			var type = _semanticModel.GetTypeInfo(node).Type;

			if (type.TypeKind == TypeKind.Enum) {
				return _runtimeLibrary.Default(type, this);
			}
			else if (type.TypeKind == TypeKind.TypeParameter) {
				var activator = _compilation.GetTypeByMetadataName(typeof(System.Activator).FullName);
				var createInstance = activator.GetMembers("CreateInstance").OfType<IMethodSymbol>().Single(m => m.IsStatic && m.TypeParameters.Length == 1 && m.Parameters.Length == 0);
				var createInstanceSpec = createInstance.Construct(type);
				var createdObject = CompileMethodInvocation(_metadataImporter.GetMethodSemantics(createInstanceSpec), createInstanceSpec, new[] { InstantiateType(activator) }, false);
				return CompileInitializerStatements(createdObject, node.Initializer != null ? ResolveInitializedMembers(node.Initializer.Expressions).ToList() : (IReadOnlyList<Tuple<ISymbol, ExpressionSyntax>>)ImmutableArray<Tuple<ISymbol, ExpressionSyntax>>.Empty);
			}
			else if (type.TypeKind == TypeKind.Delegate && node.ArgumentList != null && node.ArgumentList.Arguments.Count == 1) {
				var arg = node.ArgumentList.Arguments[0].Expression;
				var conversion = _semanticModel.GetConversion(arg);
				if (conversion.IsAnonymousFunction || conversion.IsMethodGroup) {
					return Visit(arg);
				}
				else {
					var sourceType = _semanticModel.GetTypeInfo(arg).Type;
					var targetSem = _metadataImporter.GetDelegateSemantics((INamedTypeSymbol)type.OriginalDefinition);
					var sourceSem = _metadataImporter.GetDelegateSemantics((INamedTypeSymbol)sourceType.OriginalDefinition);
					if (targetSem.BindThisToFirstParameter != sourceSem.BindThisToFirstParameter) {
						_errorReporter.Message(Messages._7533, type.FullyQualifiedName(), sourceType.FullyQualifiedName());
						return JsExpression.Null;
					}
					if (targetSem.ExpandParams != sourceSem.ExpandParams) {
						_errorReporter.Message(Messages._7537, type.FullyQualifiedName(), sourceType.FullyQualifiedName());
						return JsExpression.Null;
					}

					if (sourceType.TypeKind == TypeKind.Delegate) {
						return _runtimeLibrary.CloneDelegate(Visit(arg), sourceType, type, this);
					}
					else {
						_errorReporter.InternalError("Unexpected delegate construction " + node);
						return JsExpression.Null;
					}
				}
			}
			else {
				var ctor = _semanticModel.GetSymbolInfo(node);
				if (ctor.Symbol == null && ctor.CandidateReason == CandidateReason.LateBound) {
					if (node.ArgumentList.Arguments.Any(arg => arg.NameColon != null)) {
						_errorReporter.Message(Messages._7526);
						return JsExpression.Null;
					}

					var semantics = ctor.CandidateSymbols.Select(s => _metadataImporter.GetConstructorSemantics((IMethodSymbol)s)).ToList();

					if (semantics.Select(s => s.Type).Distinct().Count() > 1) {
						_errorReporter.Message(Messages._7531);
						return JsExpression.Null;
					}
					switch (semantics[0].Type) {
						case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
							break;

						case ConstructorScriptSemantics.ImplType.NamedConstructor:
						case ConstructorScriptSemantics.ImplType.StaticMethod:
							if (semantics.Select(s => s.Name).Distinct().Count() > 1) {
								_errorReporter.Message(Messages._7531);
								return JsExpression.Null;
							}
							break;

						default:
							_errorReporter.Message(Messages._7531);
							return JsExpression.Null;
					}

					return CompileConstructorInvocation(semantics[0], (IMethodSymbol)ctor.CandidateSymbols[0], ArgumentMap.CreateIdentity(node.ArgumentList.Arguments.Select(a => a.Expression)), node.Initializer != null ? ResolveInitializedMembers(node.Initializer.Expressions).ToList() : (IReadOnlyList<Tuple<ISymbol, ExpressionSyntax>>)ImmutableArray<Tuple<ISymbol, ExpressionSyntax>>.Empty);
				}
				else {
					var method = (IMethodSymbol)ctor.Symbol;
					return CompileConstructorInvocation(_metadataImporter.GetConstructorSemantics(method), method, _semanticModel.GetArgumentMap(node), node.Initializer != null ? ResolveInitializedMembers(node.Initializer.Expressions).ToList() : (IReadOnlyList<Tuple<ISymbol, ExpressionSyntax>>)ImmutableArray<Tuple<ISymbol, ExpressionSyntax>>.Empty);
				}
			}
		}

		public override JsExpression VisitInvocationExpression(InvocationExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node);
			if (symbol.Symbol == null) {
				if (symbol.CandidateReason == CandidateReason.LateBound) {
					if (symbol.CandidateSymbols.Length > 0) {
						return CompileLateBoundCallWithCandidateSymbols(symbol.CandidateSymbols, node.Expression, node.ArgumentList.Arguments,
						                                                c => _metadataImporter.GetMethodSemantics((IMethodSymbol)c).Type == MethodScriptSemantics.ImplType.NormalMethod,
						                                                c => _metadataImporter.GetMethodSemantics((IMethodSymbol)c).Name);
					}
					else {
						var expressions = new List<JsExpression>();
						expressions.Add(InnerCompile(node.Expression, false));

						foreach (var arg in node.ArgumentList.Arguments) {
							if (arg.NameColon != null) {
								_errorReporter.Message(Messages._7526);
								return JsExpression.Null;
							}
							expressions.Add(InnerCompile(arg.Expression, false, expressions));
						}

						return JsExpression.Invocation(expressions[0], expressions.Skip(1));
					}
				}
				else {
					_errorReporter.InternalError("Invocation does not resolve");
					return JsExpression.Null;
				}
			}

			var method = symbol.Symbol as IMethodSymbol;
			if (method == null) {
				_errorReporter.InternalError("Invocation of non-method");
				return JsExpression.Null;
			}

			if (method.ContainingType.TypeKind == TypeKind.Delegate && method.Name == "Invoke") {
				var sem = _metadataImporter.GetDelegateSemantics(method.ContainingType);
			
				var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, null, InnerCompile(node.Expression, usedMultipleTimes: false, returnMultidimArrayValueByReference: true), false, _semanticModel.GetArgumentMap(node), sem.OmitUnspecifiedArgumentsFrom);
				var methodExpr = thisAndArguments[0];
				thisAndArguments = thisAndArguments.Skip(1).ToList();
			
				if (sem.BindThisToFirstParameter) {
					return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, methodExpr, sem.ExpandParams, true);
				}
				else {
					thisAndArguments.Insert(0, JsExpression.Null);
					return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, methodExpr, sem.ExpandParams, false);
				}
			}

			if (node.Expression is MemberAccessExpressionSyntax) {
				var mae = (MemberAccessExpressionSyntax)node.Expression;
				return CompileMethodInvocation(_metadataImporter.GetMethodSemantics(method), method, usedMultipleTimes => InnerCompile(mae.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), IsReadonlyField(mae.Expression), _semanticModel.GetArgumentMap(node), node.Expression.IsNonVirtualAccess());
			}
			else {
				return CompileMethodInvocation(_metadataImporter.GetMethodSemantics(method), method, usedMultipleTimes => CompileThis(), IsReadonlyField(node.Expression), _semanticModel.GetArgumentMap(node), node.Expression.IsNonVirtualAccess());
			}
		}

		public override JsExpression VisitLiteralExpression(LiteralExpressionSyntax node) {
			var value = _semanticModel.GetConstantValue(node);
			if (!value.HasValue) {
				_errorReporter.InternalError("Literal does not have constant value");
				return JsExpression.Null;
			}
			return JSModel.Utils.MakeConstantExpression(value.Value);
		}

		public override JsExpression VisitDefaultExpression(DefaultExpressionSyntax node) {
			var type = _semanticModel.GetTypeInfo(node).Type;
			if (type.IsReferenceType) {
				return JsExpression.Null;
			}
			else {
				var constant = _semanticModel.GetConstantValue(node);
				if (constant.HasValue && type.TypeKind != TypeKind.Enum)
					return JSModel.Utils.MakeConstantExpression(constant.Value);
				else
					return _runtimeLibrary.Default(_semanticModel.GetTypeInfo(node).Type, this);
			}
		}

		public override JsExpression VisitThisExpression(ThisExpressionSyntax node) {
			return CompileThis();
		}

		public override JsExpression VisitBaseExpression(BaseExpressionSyntax node) {
			return CompileThis();
		}

		public override JsExpression VisitIdentifierName(IdentifierNameSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
			var conversion = _semanticModel.GetConversion(node);
			if (conversion.IsMethodGroup) {
				var targetType = _semanticModel.GetTypeInfo(node).ConvertedType;
				return PerformMethodGroupConversion(_ => symbol.IsStatic ? InstantiateType(GetContainingType(node)) : CompileThis(), (INamedTypeSymbol)targetType, (IMethodSymbol)symbol, false);
			}
			else {
				if (symbol is ILocalSymbol || symbol is IParameterSymbol) {
					return CompileLocal(_semanticModel.GetSymbolInfo(node).Symbol, false);
				}
				else if (symbol is IRangeVariableSymbol) {
					return CompileRangeVariableAccess((IRangeVariableSymbol)symbol);
				}
				else if (symbol is IMethodSymbol || symbol is IPropertySymbol || symbol is IFieldSymbol || symbol is IEventSymbol) {
					return HandleMemberRead(usedMultipleTimes => CompileThis(), _semanticModel.GetSymbolInfo(node).Symbol, false, IsReadonlyField(node));
				}
				else if (symbol is ITypeSymbol) {
					return InstantiateType((ITypeSymbol)symbol);
				}
				else {
					_errorReporter.InternalError("Cannot handle identifier " + node);
					return JsExpression.Null;
				}
			}
		}

		public override JsExpression VisitGenericName(GenericNameSyntax node) {
			var conversion = _semanticModel.GetConversion(node);
			if (conversion.IsMethodGroup) {
				var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
				var targetType = _semanticModel.GetTypeInfo(node).ConvertedType;
				return PerformMethodGroupConversion(_ => symbol.IsStatic ? InstantiateType(GetContainingType(node)) : CompileThis(), (INamedTypeSymbol)targetType, (IMethodSymbol)symbol, false);
			}
			else {
				var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
				if (symbol is ITypeSymbol) {
					return InstantiateType((ITypeSymbol)symbol);
				}
				else {
					_errorReporter.InternalError("Unexpected generic name " + node);
					return JsExpression.Null;
				}
			}
		}

		public override JsExpression VisitTypeOfExpression(TypeOfExpressionSyntax node) {
			var type = (ITypeSymbol)_semanticModel.GetSymbolInfo(node.Type).Symbol;
			var errors = Utils.FindTypeUsageErrors(new[] { type }, _metadataImporter);
			if (errors.HasErrors) {
				foreach (var ut in errors.UsedUnusableTypes)
					_errorReporter.Message(Messages._7522, ut.FullyQualifiedName());
				foreach (var t in errors.MutableValueTypesBoundToTypeArguments)
					_errorReporter.Message(Messages._7539, t.FullyQualifiedName());

				return JsExpression.Null;
			}
			else
				return _runtimeLibrary.TypeOf(type, this);
		}

		public override JsExpression VisitElementAccessExpression(ElementAccessExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node);
			var type = _semanticModel.GetTypeInfo(node).Type;

			if (symbol.Symbol == null && symbol.CandidateReason == CandidateReason.LateBound) {
				if (symbol.CandidateSymbols.Length > 0) {
					return CompileLateBoundCallWithCandidateSymbols(symbol.CandidateSymbols, node, node.ArgumentList.Arguments,
					                                                c => { var sem = _metadataImporter.GetPropertySemantics((IPropertySymbol)c); return sem.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods && sem.GetMethod.Type == MethodScriptSemantics.ImplType.NormalMethod; },
					                                                c => { var sem = _metadataImporter.GetPropertySemantics((IPropertySymbol)c); return sem.GetMethod.Name; });
				}
				else {
					if (node.ArgumentList.Arguments.Count != 1) {
						_errorReporter.Message(Messages._7528);
						return JsExpression.Null;
					}
					var expr = InnerCompile(node.Expression, false, returnMultidimArrayValueByReference: true);
					var arg  = InnerCompile(node.ArgumentList.Arguments[0].Expression, false, ref expr);
					return JsExpression.Index(expr, arg);
				}
			}
			else if (symbol.Symbol is IPropertySymbol) {
				var property = (IPropertySymbol)symbol.Symbol;
				var impl = _metadataImporter.GetPropertySemantics(property);
				if (impl.Type != PropertyScriptSemantics.ImplType.GetAndSetMethods) {
					_errorReporter.InternalError("Cannot invoke property that does not have a get method.");
					return JsExpression.Null;
				}
				return CompileMethodInvocation(impl.GetMethod, property.GetMethod, usedMultipleTimes => InnerCompile(node.Expression, usedMultipleTimes), IsReadonlyField(node.Expression), _semanticModel.GetArgumentMap(node), node.IsNonVirtualAccess());
			}
			else {
				var expressions = new List<JsExpression>();
				expressions.Add(InnerCompile(node.Expression, false, returnMultidimArrayValueByReference: true));
				foreach (var i in node.ArgumentList.Arguments)
					expressions.Add(InnerCompile(i.Expression, false, expressions));

				if (node.ArgumentList.Arguments.Count == 1) {
					return JsExpression.Index(expressions[0], expressions[1]);
				}
				else {
					var result = _runtimeLibrary.GetMultiDimensionalArrayValue(expressions[0], expressions.Skip(1), this);
					if (!_returnMultidimArrayValueByReference) {
						type = type.UnpackNullable();
						if (IsMutableValueType(type)) {
							result = _runtimeLibrary.CloneValueType(result, type, this);
						}
					}
					return result;
				}
			}
		}

		public override JsExpression VisitArrayCreationExpression(ArrayCreationExpressionSyntax node) {
			return HandleArrayCreation((IArrayTypeSymbol)_semanticModel.GetTypeInfo(node).Type, node.Initializer, node.Type.RankSpecifiers);
		}

		public override JsExpression VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node) {
			return HandleArrayCreation((IArrayTypeSymbol)_semanticModel.GetTypeInfo(node).Type, node.Initializer, null);
		}

		public override JsExpression VisitCastExpression(CastExpressionSyntax node) {
			var info = _semanticModel.GetCastInfo(node);
			var input = Visit(node.Expression, true, _returnMultidimArrayValueByReference);
			return PerformConversion(input, info.Conversion, info.FromType, info.ToType, node.Expression);
		}

		public override JsExpression VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) {
			var targetType = (INamedTypeSymbol)_semanticModel.GetTypeInfo(node).ConvertedType;
			if (targetType.Name == typeof(System.Linq.Expressions.Expression).Name && targetType.ContainingNamespace.FullyQualifiedName() == typeof(System.Linq.Expressions.Expression).Namespace && targetType.Arity == 1) {
				return PerformExpressionTreeLambdaConversion(new[] { node.Parameter }, (ExpressionSyntax)node.Body);
			}
			else {
				var lambdaSymbol = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
				return CompileLambda(node, lambdaSymbol.Parameters, node.Body, node.AsyncKeyword.CSharpKind() != SyntaxKind.None, targetType);
			}
		}

		public override JsExpression VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) {
			var targetType = (INamedTypeSymbol)_semanticModel.GetTypeInfo(node).ConvertedType;
			if (targetType.Name == typeof(System.Linq.Expressions.Expression).Name && targetType.ContainingNamespace.FullyQualifiedName() == typeof(System.Linq.Expressions.Expression).Namespace && targetType.Arity == 1) {
				return PerformExpressionTreeLambdaConversion(node.ParameterList.Parameters, (ExpressionSyntax)node.Body);
			}
			else {
				var lambdaSymbol = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
				return CompileLambda(node, lambdaSymbol.Parameters, node.Body, node.AsyncKeyword.CSharpKind() != SyntaxKind.None, targetType);
			}
		}

		public override JsExpression VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) {
			var targetType = (INamedTypeSymbol)_semanticModel.GetTypeInfo(node).ConvertedType;
			var parameters = node.ParameterList != null ? ((IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol).Parameters : ImmutableArray<IParameterSymbol>.Empty;
			return CompileLambda(node, parameters, node.Block, node.AsyncKeyword.CSharpKind() != SyntaxKind.None, targetType);
		}

		public override JsExpression VisitCheckedExpression(CheckedExpressionSyntax node) {
			return Visit(node.Expression);
		}

		public override JsExpression VisitSizeOfExpression(SizeOfExpressionSyntax node) {
			var value = _semanticModel.GetConstantValue(node);
			if (!value.HasValue) {
				// This is an internal error because AFAIK, using sizeof() with anything that doesn't return a compile-time constant (with our enum extensions) can only be done in an unsafe context.
				_errorReporter.InternalError("Cannot take the size of type " + _semanticModel.GetSymbolInfo(node.Type).Symbol.FullyQualifiedName());
				return JsExpression.Null;
			}
			return JSModel.Utils.MakeConstantExpression(value.Value);
		}

		public override JsExpression VisitQueryExpression(QueryExpressionSyntax node) {
			return HandleQueryExpression(node);
		}
	}
}
