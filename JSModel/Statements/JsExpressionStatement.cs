using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsExpressionStatement : JsStatement {
        public JsExpression Expression { get; private set; }

        public JsExpressionStatement(JsExpression expression, string statementLabel = null) : base(statementLabel) {
            if (expression == null) throw new ArgumentNullException("expression");
            Expression = expression;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
