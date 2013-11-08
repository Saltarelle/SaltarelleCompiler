using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsThrowStatement : JsStatement {
		public new JsExpression Expression { get; private set; }

		[Obsolete("Use factory method JsStatement.Throw")]
		public JsThrowStatement(JsExpression expression) {
			this.Expression = expression;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitThrowStatement(this, data);
		}
	}
}
