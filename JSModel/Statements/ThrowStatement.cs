using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class ThrowStatement : Statement {
        public Expression Expression { get; private set; }

        public ThrowStatement(Expression expression, string statementLabel = null) : base(statementLabel) {
            this.Expression = expression;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IStatementVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }
}
