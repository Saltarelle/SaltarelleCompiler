using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsGotoStatement : JsStatement {
		public string TargetLabel { get; private set; }

		public JsGotoStatement(string targetLabel) {
			Require.ValidJavaScriptIdentifier(targetLabel, "targetLabel", allowNull: false);
			TargetLabel = targetLabel;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitGotoStatement(this, data);
		}
	}
}
