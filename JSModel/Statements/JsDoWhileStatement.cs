using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsDoWhileStatement : JsStatement {
        public JsExpression Condition { get; private set; }
        public JsStatement Body { get; private set; }

        public JsDoWhileStatement(JsExpression condition, JsStatement body, string statementLabel = null) : base(statementLabel) {
            if (condition == null) throw new ArgumentNullException("condition");
            if (body == null) throw new ArgumentNullException("body");
            Condition = condition;
            Body = body;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
