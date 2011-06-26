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
        public override TReturn Accept<TReturn>(IStatementVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }
}
