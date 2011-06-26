using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class ReturnStatement : Statement {
        /// <summary>
        /// Can be null if the statement does not return a value.
        /// </summary>
        public Expression Value { get; private set; }

        public ReturnStatement(Expression value = null, string statementLabel = null) : base(statementLabel) {
            Value = value;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
