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

		private readonly ICompilation _compilation;
		private readonly INamingConventionResolver _namingConvention;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly IErrorReporter _errorReporter;
		private readonly IDictionary<IVariable, VariableData> _variables;
		private readonly Func<IType, LocalResolveResult> _createTemporaryVariable;

		public class Result {
			public JsExpression Expression { get; set; }
			public ReadOnlyCollection<JsStatement> AdditionalStatements { get; private set; }

			public Result(JsExpression expression, IEnumerable<JsStatement> additionalStatements) {
				this.Expression           = expression;
				this.AdditionalStatements = additionalStatements.AsReadOnly();
			}
		}

		public ExpressionCompiler(ICompilation compilation, INamingConventionResolver namingConvention, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, IDictionary<IVariable, VariableData> variables, Func<IType, LocalResolveResult> createTemporaryVariable) {
			_compilation = compilation;
			_namingConvention = namingConvention;
			_runtimeLibrary = runtimeLibrary;
			_errorReporter = errorReporter;
			_variables = variables;
			_createTemporaryVariable = createTemporaryVariable;
		}

		private List<JsStatement> _additionalStatements;

		public Result Compile(ResolveResult expression, bool returnValueIsImportant) {
			_additionalStatements = new List<JsStatement>();
			var expr = VisitResolveResult(expression, returnValueIsImportant);
			return new Result(expr, _additionalStatements);
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
						JsExpression target = VisitResolveResult(indexerInvocation.TargetResult, true);
						var indexerArgs = indexerInvocation.Arguments.Select(a => VisitResolveResult(a, true));
						switch (impl.Type) {
							case PropertyImplOptions.ImplType.GetAndSetMethods: {
								var value = VisitResolveResult(rr.Operands[1], true);
								if (returnValueIsImportant && IsJsExpressionComplexEnoughToGetATemporaryVariable.Process(value)) {
									var temp  = _createTemporaryVariable(rr.Operands[1].Type);
									_additionalStatements.Add(new JsVariableDeclarationStatement(new JsVariableDeclaration(_variables[temp.Variable].Name, value)));
									value = JsExpression.Identifier(_variables[temp.Variable].Name);
								}
								var setter = CompileMethodCall(impl.SetMethod, target, indexerArgs.Concat(new[] { value }));
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
								var l = indexerArgs.ToList();
								if (l.Count != 1)
									_errorReporter.Error("Property " + targetProperty.DeclaringType.FullName + "." + targetProperty.Name + ", declared as being a native indexer, does not have exactly one argument.");
								return JsExpression.Assign(JsExpression.Index(target, l[0]), VisitResolveResult(rr.Operands[1], true));
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
								var value = VisitResolveResult(rr.Operands[1], true);
								if (returnValueIsImportant && IsJsExpressionComplexEnoughToGetATemporaryVariable.Process(value)) {
									var temp  = _createTemporaryVariable(rr.Operands[1].Type);
									_additionalStatements.Add(new JsVariableDeclarationStatement(new JsVariableDeclaration(_variables[temp.Variable].Name, value)));
									value = JsExpression.Identifier(_variables[temp.Variable].Name);
								}
								var setter = CompileMethodCall(impl.SetMethod, target, new[] { value });
								if (returnValueIsImportant) {
									_additionalStatements.Add(new JsExpressionStatement(setter));
									return value;
								}
								else {
									return setter;
								}
							}
							case PropertyImplOptions.ImplType.Field:
								return JsExpression.Assign(JsExpression.MemberAccess(VisitResolveResult(mrr.TargetResult, true), impl.FieldName), VisitResolveResult(rr.Operands[1], true));

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
