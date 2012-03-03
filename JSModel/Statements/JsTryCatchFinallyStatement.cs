using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsTryCatchFinallyStatement : JsStatement {
        public class CatchClause {
            public string Identifier { get; private set; }
            public JsBlockStatement Body { get; private set; }

            public CatchClause(string identifier, JsStatement body) {
                if (identifier == null) throw new ArgumentNullException("identifier");
                if (body == null) throw new ArgumentNullException("body");
                Identifier = identifier;
                Body = JsBlockStatement.MakeBlock(body);
            }
        }

        public JsBlockStatement GuardedStatement { get; private set; }

        /// <summary>
        /// Can be null if the catch is not specified (but then there must be a finally).
        /// </summary>
        public CatchClause Catch { get; private set; }

        /// <summary>
        /// Can be null if the finally is not specified (but then there must be a catch).
        /// </summary>
        public JsBlockStatement Finally { get; private set; }

        public JsTryCatchFinallyStatement(JsStatement guardedStatement, CatchClause catchClause, JsStatement @finally) {
            if (guardedStatement == null) throw new ArgumentException("guardedStatement");
            if (catchClause == null && @finally == null) throw new ArgumentException("Either catchClause or finally (or both) must be specified");

            GuardedStatement = JsBlockStatement.MakeBlock(guardedStatement);
            Catch            = catchClause;
            Finally          = JsBlockStatement.MakeBlock(@finally);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
