using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsForStatement : JsStatement {
        /// <summary>
        /// Initialization statement (first part). Must be a VariableDeclarationStatement or an ExpressionStatement.
        /// </summary>
        public JsStatement InitStatement { get; private set; }
        public JsExpression ConditionExpression { get; private set; }
        public JsExpression IncrementExpression { get; private set; }
        public JsStatement Body { get; private set; }

        public JsForStatement(JsStatement initStatement, JsExpression conditionExpression, JsExpression incrementExpression, JsStatement body, string statementLabel = null) : base(statementLabel) {
            if (initStatement == null) throw new ArgumentNullException("initStatement");
            if (!(initStatement is JsVariableDeclarationStatement || initStatement is JsExpressionStatement)) throw new ArgumentException("initStatement must be a VariableDeclarationStatement or an ExpressionStatement.", "initStatement");
            if (conditionExpression == null) throw new ArgumentNullException("conditionExpression");
            if (incrementExpression == null) throw new ArgumentNullException("incrementExpression");
            if (body == null) throw new ArgumentNullException("body");
            
            InitStatement       = initStatement;
            ConditionExpression = conditionExpression;
            IncrementExpression = incrementExpression;
            Body                = body;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
