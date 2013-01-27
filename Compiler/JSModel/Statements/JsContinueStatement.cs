using System;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsContinueStatement : JsStatement {
		/// <summary>
		/// Can be null if the statement does not have a target label.
		/// </summary>
		public string TargetLabel { get; private set; }

		public JsContinueStatement(string targetLabel = null) {
			if (targetLabel != null && !targetLabel.IsValidJavaScriptIdentifier()) throw new ArgumentException("targetLabel");
			TargetLabel = targetLabel;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitContinueStatement(this, data);
		}
	}
}
