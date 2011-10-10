using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsBreakStatement : JsStatement {
        /// <summary>
        /// Can be null if the statement does not have a target label.
        /// </summary>
        public string TargetLabel { get; private set; }

        public JsBreakStatement(string targetLabel = null, string statementLabel = null) : base(statementLabel) {
            if (targetLabel != null && !targetLabel.IsValidJavaScriptIdentifier()) throw new ArgumentException("targetLabel");
            TargetLabel = targetLabel;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
