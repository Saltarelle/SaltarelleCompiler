using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsReturnStatement : JsStatement {
		/// <summary>
		/// Can be null if the statement does not return a value.
		/// </summary>
		public JsExpression Value { get; private set; }

		[Obsolete("Use factory method JsStatement.Return")]
		public JsReturnStatement(JsExpression value) {
			Value = value;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitReturnStatement(this, data);
		}
	}
}
