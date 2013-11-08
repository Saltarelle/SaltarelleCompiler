using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsComment : JsStatement {
		public string Text { get; private set; }

		[Obsolete("Use factory method JsStatement.Comment")]
		public JsComment(string text) {
			Text = text;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitComment(this, data);
		}
	}
}
