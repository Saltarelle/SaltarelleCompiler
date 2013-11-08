using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsYieldStatement : JsStatement {
		/// <summary>
		/// Value to yield. If this is null (as opposed to JsExpression.Null), the yield terminates the iterator block (C#: 'yield break').
		/// </summary>
		public JsExpression Value { get; private set; }

		[Obsolete("Use factory method JsStatement.Yield")]
		public JsYieldStatement(JsExpression value) {
			this.Value = value;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitYieldStatement(this, data);
		}
	}
}
