using System;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public abstract class Statement {
        /// <summary>
        /// Null if the statement is not labelled.
        /// </summary>
        public string StatementLabel { get; private set; }

        [System.Diagnostics.DebuggerStepThrough]
        public abstract TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data);

        protected Statement(string statementLabel) {
            if (statementLabel != null && !statementLabel.IsValidJavaScriptIdentifier()) throw new ArgumentException("statementLabel");
            StatementLabel = statementLabel;
        }
    }
}
