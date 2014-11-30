using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsLabel : JsStatement {
		public new string Label { get; private set; }

		[Obsolete("Use factory method JsStatement.Label")]
		internal JsLabel(string label) {
			Require.ValidJavaScriptIdentifier(label, "label", allowNull: false);
			Label = label;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitLabel(this, data);
		}
	}
}
