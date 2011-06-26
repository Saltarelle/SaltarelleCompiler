using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class ExpressionStatement : Statement {
        public Expression Expression { get; private set; }

        public ExpressionStatement(Expression expression, string statementLabel = null) : base(statementLabel) {
            if (expression == null) throw new ArgumentNullException("expression");
            Expression = expression;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
