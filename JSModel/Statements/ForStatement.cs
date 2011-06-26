using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class ForStatement : Statement {
        /// <summary>
        /// Initialization statement (first part). Must be a VariableDeclarationStatement or an ExpressionStatement.
        /// </summary>
        public Statement InitStatement { get; private set; }
        public Expression ConditionExpression { get; private set; }
        public Expression IncrementExpression { get; private set; }
        public Statement Body { get; private set; }

        public ForStatement(Statement initStatement, Expression conditionExpression, Expression incrementExpression, Statement body, string statementLabel = null) : base(statementLabel) {
            if (initStatement == null) throw new ArgumentNullException("initStatement");
            if (!(initStatement is VariableDeclarationStatement || initStatement is ExpressionStatement)) throw new ArgumentException("initStatement must be a VariableDeclarationStatement or an ExpressionStatement.", "initStatement");
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
