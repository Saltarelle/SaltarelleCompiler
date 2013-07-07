using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsExpressionStatement : JsStatement {
		public new JsExpression Expression { get; private set; }

		[Obsolete("Use factory method JsStatement.Expression or the conversion operators defined on JsExpression")]
		public JsExpressionStatement(JsExpression expression) {
			if (expression == null) throw new ArgumentNullException("expression");
			Expression = expression;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitExpressionStatement(this, data);
		}
	}
}
