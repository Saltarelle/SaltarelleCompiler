using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsEmptyStatement : JsStatement {
		[Obsolete("Use factory method JsStatement.Empty")]
		public JsEmptyStatement() {
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitEmptyStatement(this, data);
		}
	}
}
