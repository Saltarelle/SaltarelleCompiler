using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler {
	public class ExpressionCompiler : ResolveResultVisitor<JsExpression, bool> {
		internal class IsJsExpressionComplexEnoughToGetATemporaryVariable : RewriterVisitorBase<object> {
			private bool _result;

			public static bool Process(JsExpression expression) {
				var v = new IsJsExpressionComplexEnoughToGetATemporaryVariable();
				expression.Accept(v, null);
				return v._result;
			}

			public override JsExpression Visit(JsArrayLiteralExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsBinaryExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsCommaExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsFunctionDefinitionExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsInvocationExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsNewExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsObjectLiteralExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsUnaryExpression expression, object data) {
				_result = true;
				return expression;
			}
		}

		internal class DoesJsExpressionHaveSideEffects : RewriterVisitorBase<object> {
			private bool _result;

			public static bool Process(JsExpression expression) {
				var v = new DoesJsExpressionHaveSideEffects();
				expression.Accept(v, null);
				return v._result;
			}

			public override JsExpression Visit(JsInvocationExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsNewExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsBinaryExpression expression, object data) {
				if (expression.NodeType >= ExpressionNodeType.AssignFirst && expression.NodeType <= ExpressionNodeType.AssignLast) {
					_result = true;
					return expression;
				}
				else {
					return base.Visit(expression, data);
				}
			}

			public override JsExpression Visit(JsUnaryExpression expression, object data) {
				switch (expression.NodeType) {
					case ExpressionNodeType.PrefixPlusPlus:
					case ExpressionNodeType.PrefixMinusMinus:
					case ExpressionNodeType.PostfixPlusPlus:
					case ExpressionNodeType.PostfixMinusMinus:
					case ExpressionNodeType.Delete:
						_result = true;
						return expression;
					default:
						return base.Visit(expression, data);
				}
			}
		}

		private readonly ICompilation _compilation;
		private readonly INamingConventionResolver _namingConvention;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly IErrorReporter _errorReporter;
		private readonly IDictionary<IVariable, VariableData> _variables;
		private readonly Func<IType, LocalResolveResult> _createTemporaryVariable;
		private readonly Func<string, bool> _isVariableTemporary;

		public class Result {
			public JsExpression Expression { get; set; }
			public ReadOnlyCollection<JsStatement> AdditionalStatements { get; private set; }

			public Result(JsExpression expression, IEnumerable<JsStatement> additionalStatements) {
				this.Expression           = expression;
				this.AdditionalStatements = additionalStatements.AsReadOnly();
			}
		}

		public ExpressionCompiler(ICompilation compilation, INamingConventionResolver namingConvention, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, IDictionary<IVariable, VariableData> variables, Func<IType, LocalResolveResult> createTemporaryVariable, Func<string, bool> isVariableTemporary) {
			_compilation = compilation;
			_namingConvention = namingConvention;
			_runtimeLibrary = runtimeLibrary;
			_errorReporter = errorReporter;
			_variables = variables;
			_createTemporaryVariable = createTemporaryVariable;
			_isVariableTemporary = isVariableTemporary;
		}

		private List<JsStatement> _additionalStatements;

		public Result Compile(ResolveResult expression, bool returnValueIsImportant) {
			_additionalStatements = new List<JsStatement>();
			var expr = VisitResolveResult(expression, returnValueIsImportant);
			return new Result(expr, _additionalStatements);
		}

		private Result InnerCompile(ResolveResult expression, bool returnValueIsImportant) {
			return new ExpressionCompiler(_compilation, _namingConvention, _runtimeLibrary, _errorReporter, _variables, _createTemporaryVariable, _isVariableTemporary).Compile(expression, returnValueIsImportant);
		}

		private bool IsExpressionInvariantToOrder(JsExpression expression) {
			if (expression is JsIdentifierExpression && _isVariableTemporary(((JsIdentifierExpression)expression).Name))
				return true;	// Don't have to reorder expressions which only contain a temporary variable since noone is going to change the value of that variable. This check is important to get sensible results if using this method multiple times on the same list.
			else if (expression is JsThisExpression)
				return true;
			else
				return false;
		}

		private JsExpression InnerCompile(ResolveResult rr, bool usedMultipleTimes, IList<JsExpression> expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression) {
			var result = InnerCompile(rr, true);

			bool needsTemporary = usedMultipleTimes && IsJsExpressionComplexEnoughToGetATemporaryVariable.Process(result.Expression);
			if (result.AdditionalStatements.Count > 0 || needsTemporary || DoesJsExpressionHaveSideEffects.Process(result.Expression)) {
				// We have to ensure that everything is ordered correctly. First ensure that all expressions that have to be evaluated first actually are evaluated first.
				for (int i = 0; i < expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression.Count; i++) {
					if (IsExpressionInvariantToOrder(expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression[i]))
						continue;
					var temp = _createTemporaryVariable(_compilation.FindType(KnownTypeCode.Object));
					_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp.Variable].Name, expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression[i]));
					expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression[i] = JsExpression.Identifier(_variables[temp.Variable].Name);
				}
			}

			_additionalStatements.AddRange(result.AdditionalStatements);

			if (needsTemporary) {
				var temp = _createTemporaryVariable(rr.Type);
				_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp.Variable].Name, result.Expression));
				return JsExpression.Identifier(_variables[temp.Variable].Name);
			}
			else {
				return result.Expression;
			}
		}

		private JsExpression InnerCompile(ResolveResult rr, bool usedMultipleTimes, ref JsExpression expressionThatHasToBeEvaluatedInOrderBeforeThisExpression) {
			var l = new List<JsExpression> { expressionThatHasToBeEvaluatedInOrderBeforeThisExpression };
			var r = InnerCompile(rr, usedMultipleTimes, l);
			expressionThatHasToBeEvaluatedInOrderBeforeThisExpression = l[0];
			return r;
		}

		private JsExpression CompileMethodCall(MethodImplOptions impl, JsExpression target, IEnumerable<JsExpression> arguments) {
			// TODO
			switch (impl.Type) {
				case MethodImplOptions.ImplType.InstanceMethod:
				case MethodImplOptions.ImplType.StaticMethod:
					return JsExpression.Invocation(JsExpression.MemberAccess(target, impl.Name), arguments);
				default:
					throw new NotImplementedException();
			}
		}

		public override JsExpression VisitOperatorResolveResult(OperatorResolveResult rr, bool returnValueIsImportant) {
			if (rr.OperatorType == ExpressionType.Assign) {
				if (rr.Operands[0] is MemberResolveResult && ((MemberResolveResult)rr.Operands[0]).Member is IProperty) {
					var mrr = (MemberResolveResult)rr.Operands[0];
					var targetProperty = (IProperty)mrr.Member;
					var impl = _namingConvention.GetPropertyImplementation(targetProperty);

					if (targetProperty.IsIndexer) {
						var indexerInvocation = (CSharpInvocationResolveResult)mrr;
						var expressions = new List<JsExpression>();
						expressions.Add(VisitResolveResult(indexerInvocation.TargetResult, true));
						foreach (var a in indexerInvocation.Arguments) {
							expressions.Add(InnerCompile(a, false, expressions));
						}
						switch (impl.Type) {
							case PropertyImplOptions.ImplType.GetAndSetMethods: {
								var value = InnerCompile(rr.Operands[1], returnValueIsImportant, expressions);
								var setter = CompileMethodCall(impl.SetMethod, expressions[0], expressions.Skip(1).Concat(new[] { value }));
								if (returnValueIsImportant) {
									_additionalStatements.Add(new JsExpressionStatement(setter));
									return value;
								}
								else {
									return setter;
								}
							}
							case PropertyImplOptions.ImplType.Field:
								_errorReporter.Error("Property " + targetProperty.DeclaringType.FullName + "." + targetProperty.Name + ", declared as being a field, is an indexer.");
								return JsExpression.Number(0);

							case PropertyImplOptions.ImplType.NativeIndexer: {
								if (expressions.Count != 2)
									_errorReporter.Error("Property " + targetProperty.DeclaringType.FullName + "." + targetProperty.Name + ", declared as being a native indexer, does not have exactly one argument.");
								return JsExpression.Assign(JsExpression.Index(expressions[0], expressions[1]), VisitResolveResult(rr.Operands[1], true));
							}

							default:
								_errorReporter.Error("Cannot use property " + targetProperty.DeclaringType.FullName + "." + targetProperty.Name + " from script.");
								return JsExpression.Number(0);
						}
					}
					else {
						JsExpression target = VisitResolveResult(mrr.TargetResult, true);
						switch (impl.Type) {
							case PropertyImplOptions.ImplType.GetAndSetMethods: {
								var value = InnerCompile(rr.Operands[1], returnValueIsImportant, ref target);
								var setter = CompileMethodCall(impl.SetMethod, target, new[] { value });
								if (returnValueIsImportant) {
									_additionalStatements.Add(new JsExpressionStatement(setter));
									return value;
								}
								else {
									return setter;
								}
							}
							case PropertyImplOptions.ImplType.Field: {
								var value = InnerCompile(rr.Operands[1], returnValueIsImportant, ref target);
								return JsExpression.Assign(JsExpression.MemberAccess(target, impl.FieldName), value);
							}

							case PropertyImplOptions.ImplType.NativeIndexer:
								_errorReporter.Error("Property " + targetProperty.DeclaringType.FullName + "." + targetProperty.Name + ", declared as being a native indexer, is not an indexer.");
								return JsExpression.Number(0);

							default:
								_errorReporter.Error("Cannot use property " + targetProperty.DeclaringType.FullName + "." + targetProperty.Name + " from script.");
								return JsExpression.Number(0);
						}
					}
				}
				else {
					return JsExpression.Assign(VisitResolveResult(rr.Operands[0], true), VisitResolveResult(rr.Operands[1], true));
				}
			}
			else if (rr.OperatorType == ExpressionType.LessThan) {
				return JsExpression.Binary(ExpressionNodeType.Lesser, VisitResolveResult(rr.Operands[0], true), VisitResolveResult(rr.Operands[1], true));
			}
			else if (rr.OperatorType == ExpressionType.Add) {
				return JsExpression.Add(VisitResolveResult(rr.Operands[0], true), VisitResolveResult(rr.Operands[1], true));
			}
			else if (rr.OperatorType == ExpressionType.PostIncrementAssign) {
				return JsExpression.PostfixPlusPlus(VisitResolveResult(rr.Operands[0], true));
			}
			else if (rr.OperatorType == ExpressionType.Equal) {
				return JsExpression.Equal(VisitResolveResult(rr.Operands[0], true), VisitResolveResult(rr.Operands[1], true));
			}
			else if (rr.OperatorType == ExpressionType.NotEqual) {
				return JsExpression.NotEqual(VisitResolveResult(rr.Operands[0], true), VisitResolveResult(rr.Operands[1], true));
			}
			throw new NotImplementedException();
		}


		// TODO: Methods below are UNTESTED and REALLY hacky, but needed for the statement compiler

		public override JsExpression VisitConstantResolveResult(ConstantResolveResult rr, bool returnValueIsImportant) {
			if (rr.ConstantValue is string)
				return JsExpression.String((string)rr.ConstantValue);
			else if (rr.ConstantValue is int)
				return JsExpression.Number((int)rr.ConstantValue);
			else if (rr.ConstantValue is bool)
				return (bool)rr.ConstantValue ? JsExpression.True : JsExpression.False;
			else
				return JsExpression.Null;
		}

		public override JsExpression VisitLocalResolveResult(LocalResolveResult rr, bool returnValueIsImportant) {
			return JsExpression.Identifier(_variables[rr.Variable].Name);	// Not really, this (might, or it might happen in the step above it) needs to take care of by-ref variables.
		}

		public override JsExpression VisitCSharpInvocationResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.CSharpInvocationResolveResult rr, bool returnValueIsImportant) {
			// Note: This might also represent a constructor.
			var arguments = rr.Arguments.Select(a => VisitResolveResult(a, true));
			if (rr.Member is IMethod && ((IMethod)rr.Member).IsConstructor) {
				return JsExpression.New(new JsTypeReferenceExpression(rr.Member.DeclaringType.GetDefinition()), arguments);
			}
			else {
				return JsExpression.Invocation(JsExpression.MemberAccess(rr.TargetResult != null ? VisitResolveResult(rr.TargetResult, true) : new JsTypeReferenceExpression(rr.Member.DeclaringType.GetDefinition()), rr.Member.Name), arguments);
			}
		}

		public override JsExpression VisitThisResolveResult(ThisResolveResult rr, bool returnValueIsImportant) {
			return JsExpression.This;
		}

		public override JsExpression VisitMemberResolveResult(MemberResolveResult rr, bool returnValueIsImportant) {
			if (rr.Member is IProperty) {
				return JsExpression.Invocation(JsExpression.MemberAccess(VisitResolveResult(rr.TargetResult, false), "get_" + rr.Member.Name));
			}
			else if (rr.Member is IField) {
				return JsExpression.MemberAccess(VisitResolveResult(rr.TargetResult, false), rr.Member.Name);
			}
			return base.VisitMemberResolveResult(rr, returnValueIsImportant);
		}

		public override JsExpression VisitConversionResolveResult(ConversionResolveResult rr, bool returnValueIsImportant) {
			if (rr.Conversion.IsIdentityConversion) {
				return VisitResolveResult(rr.Input, true);
			}
			if (rr.Conversion.IsTryCast) {
				return _runtimeLibrary.TryCast(_compilation, VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			else if (rr.Conversion.IsReferenceConversion) {
				if (rr.Conversion.IsImplicit)
					return _runtimeLibrary.ImplicitReferenceConversion(_compilation, VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
				else
					return _runtimeLibrary.Cast(_compilation, VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			else if (rr.Conversion.IsDynamicConversion) {
				return _runtimeLibrary.Cast(_compilation, VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			throw new NotImplementedException();
		}

		public override JsExpression VisitArrayCreateResolveResult(ArrayCreateResolveResult rr, bool returnValueIsImportant) {
			return JsExpression.ArrayLiteral(rr.InitializerElements.Select(e => VisitResolveResult(e, true)));
		}

		public override JsExpression VisitByReferenceResolveResult(ByReferenceResolveResult rr, bool returnValueIsImportant) {
			return VisitResolveResult(rr.ElementResult, true);
		}

		public override JsExpression VisitArrayAccessResolveResult(ArrayAccessResolveResult rr, bool returnValueIsImportant) {
			throw new NotImplementedException();
		}

		public override JsExpression VisitDefaultResolveResult(ResolveResult rr, bool returnValueIsImportant) {
			if (rr.Type.Kind == TypeKind.Null)
				return JsExpression.Null;
			throw new NotImplementedException();
		}

		public override JsExpression VisitInvocationResolveResult(InvocationResolveResult rr, bool returnValueIsImportant) {
			throw new NotImplementedException();
		}

		public override JsExpression VisitLambdaResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.LambdaResolveResult rr, bool returnValueIsImportant) {
			return JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.Number(0))); 
		}

		public override JsExpression VisitMethodGroupResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.MethodGroupResolveResult rr, bool returnValueIsImportant) {
			var m = rr.Methods.First();
			return JsExpression.MemberAccess(new JsTypeReferenceExpression(m.DeclaringType.GetDefinition()), m.Name);
		}

		public override JsExpression VisitTypeOfResolveResult(TypeOfResolveResult rr, bool returnValueIsImportant) {
			throw new NotImplementedException();
		}

		public override JsExpression VisitTypeResolveResult(TypeResolveResult rr, bool returnValueIsImportant) {
			return new JsTypeReferenceExpression(rr.Type.GetDefinition());
		}

        public override JsExpression VisitTypeIsResolveResult(TypeIsResolveResult rr, bool returnValueIsImportant) {
			return _runtimeLibrary.TypeIs(_compilation, VisitResolveResult(rr.Input, returnValueIsImportant), new JsTypeReferenceExpression(rr.TargetType.GetDefinition()));
        }
	}
}
