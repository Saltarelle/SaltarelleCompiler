using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class ContinueStatement : Statement {
        /// <summary>
        /// Can be null if the statement does not have a target label.
        /// </summary>
        public string TargetLabel { get; private set; }

        public ContinueStatement(string targetLabel = null, string statementLabel = null) : base(statementLabel) {
            if (targetLabel != null && !targetLabel.IsValidJavaScriptIdentifier()) throw new ArgumentException("targetLabel");
            TargetLabel = targetLabel;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IStatementVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }
}
