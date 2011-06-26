using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class IfStatement : Statement {
        public Expression Test { get; private set; }
        public Statement Then { get; private set; }
        /// <summary>
        /// Can be null if there is no else clause.
        /// </summary>
        public Statement Else { get; private set; }

        public IfStatement(Expression test, Statement then, Statement @else, string statementLabel = null) : base(statementLabel) {
            if (test == null) throw new ArgumentNullException("test");
            if (then == null) throw new ArgumentNullException("then");
            Test = test;
            Then = then;
            Else = @else;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
