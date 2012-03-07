using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler {
	public class ExpressionCompiler : ResolveResultVisitor<JsExpression, object> {
		private readonly ICompilation _compilation;
		private readonly INamingConventionResolver _namingConvention;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly IDictionary<IVariable, VariableData> _variables;
		private readonly SharedValue<int> _nextTemporaryVariableIndex;

		public class Result {
			public JsExpression Expression { get; set; }
			public ReadOnlyCollection<JsStatement> AdditionalStatements { get; private set; }

			public Result(JsExpression expression, IEnumerable<JsStatement> additionalStatements) {
				this.Expression           = expression;
				this.AdditionalStatements = additionalStatements.AsReadOnly();
			}
		}

		public ExpressionCompiler(ICompilation compilation, INamingConventionResolver namingConvention, IRuntimeLibrary runtimeLibrary, IDictionary<IVariable, VariableData> variables, SharedValue<int> nextTemporaryVariableIndex) {
			_compilation = compilation;
			_namingConvention = namingConvention;
			_runtimeLibrary = runtimeLibrary;
			_variables = variables;
			_nextTemporaryVariableIndex = nextTemporaryVariableIndex;
		}

		private List<JsStatement> _additionalStatements;

		public Result Compile(ResolveResult expression, bool returnValueIsImportant) {
			_additionalStatements = new List<JsStatement>();
			var expr = VisitResolveResult(expression, null);
			return new Result(expr, _additionalStatements);
		}

		// TODO: Methods below are UNTESTED and REALLY hacky, but needed for the statement compiler

		public override JsExpression VisitConstantResolveResult(ConstantResolveResult rr, object data) {
			if (rr.ConstantValue is string)
				return JsExpression.String((string)rr.ConstantValue);
			else if (rr.ConstantValue is int)
				return JsExpression.Number((int)rr.ConstantValue);
			else if (rr.ConstantValue is bool)
				return (bool)rr.ConstantValue ? JsExpression.True : JsExpression.False;
			else
				return JsExpression.Null;
		}

		public override JsExpression VisitLocalResolveResult(LocalResolveResult rr, object data) {
			return JsExpression.Identifier(_variables[rr.Variable].Name);	// Not really, this (might, or it might happen in the step above it) needs to take care of by-ref variables.
		}

		public override JsExpression VisitOperatorResolveResult(OperatorResolveResult rr, object data) {
			if (rr.OperatorType == ExpressionType.Assign) {
				if (rr.Operands[0] is MemberResolveResult && ((MemberResolveResult)rr.Operands[0]).Member is IProperty) {
					var p = (IProperty)((MemberResolveResult)rr.Operands[0]).Member;
					var impl = _namingConvention.GetPropertyImplementation(p);
					if (impl.Type == PropertyImplOptions.ImplType.GetAndSetMethods) {
						// TODO: This is not even close to working
						JsExpression value = VisitResolveResult(rr.Operands[1], data);
						_additionalStatements.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.MemberAccess(JsExpression.This, impl.SetMethod.Name), value)));
						return value;
					}
				}
				else {
					return JsExpression.Assign(VisitResolveResult(rr.Operands[0], data), VisitResolveResult(rr.Operands[1], data));
				}
			}
			else if (rr.OperatorType == ExpressionType.LessThan) {
				return JsExpression.Binary(ExpressionNodeType.Lesser, VisitResolveResult(rr.Operands[0], data), VisitResolveResult(rr.Operands[1], data));
			}
			else if (rr.OperatorType == ExpressionType.Add) {
				return JsExpression.Add(VisitResolveResult(rr.Operands[0], data), VisitResolveResult(rr.Operands[1], data));
			}
			else if (rr.OperatorType == ExpressionType.PostIncrementAssign) {
				return JsExpression.PostfixPlusPlus(VisitResolveResult(rr.Operands[0], data));
			}
			else if (rr.OperatorType == ExpressionType.Equal) {
				return JsExpression.Equal(VisitResolveResult(rr.Operands[0], data), VisitResolveResult(rr.Operands[1], data));
			}
			else if (rr.OperatorType == ExpressionType.NotEqual) {
				return JsExpression.NotEqual(VisitResolveResult(rr.Operands[0], data), VisitResolveResult(rr.Operands[1], data));
			}
			throw new NotImplementedException();
		}

		public override JsExpression VisitCSharpInvocationResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.CSharpInvocationResolveResult rr, object data) {
			// Note: This might also represent a constructor.
			var arguments = rr.Arguments.Select(a => VisitResolveResult(a, true));
			if (rr.Member is IMethod && ((IMethod)rr.Member).IsConstructor) {
				return JsExpression.New(new JsTypeReferenceExpression(rr.Member.DeclaringType.GetDefinition()), arguments);
			}
			else {
				return JsExpression.Invocation(JsExpression.MemberAccess(rr.TargetResult != null ? VisitResolveResult(rr.TargetResult, true) : new JsTypeReferenceExpression(rr.Member.DeclaringType.GetDefinition()), rr.Member.Name), arguments);
			}
		}

		public override JsExpression VisitThisResolveResult(ThisResolveResult rr, object data) {
			return JsExpression.This;
		}

		public override JsExpression VisitMemberResolveResult(MemberResolveResult rr, object data) {
			if (rr.Member is IProperty) {
				return JsExpression.Invocation(JsExpression.MemberAccess(VisitResolveResult(rr.TargetResult, false), "get_" + rr.Member.Name));
			}
			else if (rr.Member is IField) {
				return JsExpression.MemberAccess(VisitResolveResult(rr.TargetResult, false), rr.Member.Name);
			}
			return base.VisitMemberResolveResult(rr, data);
		}

		public override JsExpression VisitConversionResolveResult(ConversionResolveResult rr, object data) {
			if (rr.Conversion.IsIdentityConversion) {
				return VisitResolveResult(rr.Input, data);
			}
			if (rr.Conversion.IsTryCast) {
				return _runtimeLibrary.TryCast(_compilation, VisitResolveResult(rr.Input, data), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			else if (rr.Conversion.IsReferenceConversion) {
				if (rr.Conversion.IsImplicit)
					return _runtimeLibrary.ImplicitReferenceConversion(_compilation, VisitResolveResult(rr.Input, data), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
				else
					return _runtimeLibrary.Cast(_compilation, VisitResolveResult(rr.Input, data), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			else if (rr.Conversion.IsDynamicConversion) {
				return _runtimeLibrary.Cast(_compilation, VisitResolveResult(rr.Input, data), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			throw new NotImplementedException();
		}

		public override JsExpression VisitArrayCreateResolveResult(ArrayCreateResolveResult rr, object data) {
			return JsExpression.ArrayLiteral(rr.InitializerElements.Select(e => VisitResolveResult(e, data)));
		}

		public override JsExpression VisitByReferenceResolveResult(ByReferenceResolveResult rr, object data) {
			return VisitResolveResult(rr.ElementResult, data);
		}

		public override JsExpression VisitArrayAccessResolveResult(ArrayAccessResolveResult rr, object data) {
			throw new NotImplementedException();
		}

		public override JsExpression VisitDefaultResolveResult(ResolveResult rr, object data) {
			if (rr.Type.Kind == TypeKind.Null)
				return JsExpression.Null;
			throw new NotImplementedException();
		}

		public override JsExpression VisitInvocationResolveResult(InvocationResolveResult rr, object data) {
			throw new NotImplementedException();
		}

		public override JsExpression VisitLambdaResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.LambdaResolveResult rr, object data) {
			return JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.Number(0))); 
		}

		public override JsExpression VisitMethodGroupResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.MethodGroupResolveResult rr, object data) {
			var m = rr.Methods.First();
			return JsExpression.MemberAccess(new JsTypeReferenceExpression(m.DeclaringType.GetDefinition()), m.Name);
		}

		public override JsExpression VisitTypeOfResolveResult(TypeOfResolveResult rr, object data) {
			throw new NotImplementedException();
		}

		public override JsExpression VisitTypeResolveResult(TypeResolveResult rr, object data) {
			return new JsTypeReferenceExpression(rr.Type.GetDefinition());
		}

        public override JsExpression VisitTypeIsResolveResult(TypeIsResolveResult rr, object data) {
			return _runtimeLibrary.TypeIs(_compilation, VisitResolveResult(rr.Input, data), new JsTypeReferenceExpression(rr.TargetType.GetDefinition()));
        }
	}
}
