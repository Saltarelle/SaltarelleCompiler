using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class WhileStatement : Statement {
        public Expression Condition { get; private set; }
        public Statement Body { get; private set; }

        public WhileStatement(Expression condition, Statement body, string statementLabel = null) : base(statementLabel) {
            if (condition == null) throw new ArgumentNullException("condition");
            if (body == null) throw new ArgumentNullException("body");

            Condition = condition;
            Body      = body;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
