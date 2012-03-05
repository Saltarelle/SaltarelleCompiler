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
		private readonly INamingConventionResolver _namingConvention;
		private readonly IDictionary<IVariable, VariableData> _variables;

		public class Result {
			public JsExpression Expression { get; set; }
			public ReadOnlyCollection<JsStatement> AdditionalStatements { get; private set; }

			public Result(JsExpression expression, IEnumerable<JsStatement> additionalStatements) {
				this.Expression           = expression;
				this.AdditionalStatements = additionalStatements.AsReadOnly();
			}
		}

		public ExpressionCompiler(INamingConventionResolver namingConvention, IDictionary<IVariable, VariableData> variables) {
			_namingConvention = namingConvention;
			_variables = variables;
		}

		private List<JsStatement> _additionalStatements;

		public Result Compile(ResolveResult expression, bool returnValueIsImportant) {
			_additionalStatements = new List<JsStatement>();
			var expr = VisitResolveResult(expression, null);
			return new Result(expr, _additionalStatements);
		}

		// TODO: UNTESTED, but needed for the statement compiler.
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

		// TODO: UNTESTED, but needed for the statement compiler.
		public override JsExpression VisitLocalResolveResult(LocalResolveResult rr, object data) {
			return JsExpression.Identifier(_variables[rr.Variable].Name);	// Not really, this (might, or it might happen in the step above it) needs to take care of by-ref variables.
		}

		// TODO: UNTESTED and REALLY hacky, but needed for the statement compiler
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
			else if (rr.OperatorType == ExpressionType.PostIncrementAssign) {
				return JsExpression.PostfixPlusPlus(VisitResolveResult(rr.Operands[0], data));
			}
			return base.VisitOperatorResolveResult(rr, data);
		}

		public override JsExpression VisitCSharpInvocationResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.CSharpInvocationResolveResult rr, object data) {
			// TODO: Obviously not how it should be
			return JsExpression.This;
		}
	}
}
