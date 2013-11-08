using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsLabelledStatement : JsStatement {
		public new string Label { get; private set; }
		public JsStatement Statement { get; private set; }

		[Obsolete("Use factory method JsStatement.Label")]
		public JsLabelledStatement(string label, JsStatement statement) {
			Require.ValidJavaScriptIdentifier(label, "label", allowNull: false);
			Label = label;
			Statement = statement;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitLabelledStatement(this, data);
		}
	}
}
