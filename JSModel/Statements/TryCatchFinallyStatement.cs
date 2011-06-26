using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class TryCatchFinallyStatement : Statement {
        public class CatchClause {
            public string Identifier { get; private set; }
            public BlockStatement Body { get; private set; }

            public CatchClause(string identifier, Statement body) {
                if (identifier == null) throw new ArgumentNullException("identifier");
                if (body == null) throw new ArgumentNullException("body");
                Identifier = identifier;
                Body = BlockStatement.MakeBlock(body);
            }
        }

        public BlockStatement GuardedStatement { get; private set; }

        /// <summary>
        /// Can be null if the catch is not specified (but then there must be a finally).
        /// </summary>
        public CatchClause Catch { get; private set; }

        /// <summary>
        /// Can be null if the finally is not specified (but then there must be a catch).
        /// </summary>
        public BlockStatement Finally { get; private set; }

        public TryCatchFinallyStatement(Statement guardedStatement, CatchClause catchClause, Statement @finally, string statementLabel = null) : base(statementLabel) {
            if (guardedStatement == null) throw new ArgumentException("guardedStatement");
            if (catchClause == null && @finally == null) throw new ArgumentException("Either catchClause or finally (or both) must be specified");

            GuardedStatement = BlockStatement.MakeBlock(guardedStatement);
            Catch            = catchClause;
            Finally          = BlockStatement.MakeBlock(@finally);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
