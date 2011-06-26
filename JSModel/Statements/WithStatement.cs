using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class WithStatement : Statement {
        public Expression Object { get; private set; }
        public Statement Body { get; private set; }

        public WithStatement(Expression @object, Statement body, string statementLabel = null) : base(statementLabel) {
            if (@object == null) throw new ArgumentNullException("object");
            if (body == null) throw new ArgumentNullException("body");
            Object = @object;
            Body   = body;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
