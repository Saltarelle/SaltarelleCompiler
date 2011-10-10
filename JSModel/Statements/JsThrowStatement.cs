using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsThrowStatement : JsStatement {
        public JsExpression Expression { get; private set; }

        public JsThrowStatement(JsExpression expression, string statementLabel = null) : base(statementLabel) {
            this.Expression = expression;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
