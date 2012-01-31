using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsBlockStatement : JsStatement {
        public ReadOnlyCollection<JsStatement> Statements { get; private set; }

        public JsBlockStatement(IEnumerable<JsStatement> statements, string statementLabel = null) : base(statementLabel) {
            if (statements == null) throw new ArgumentNullException("statements");
            Statements = statements.AsReadOnly();
        }

        public JsBlockStatement(params JsStatement[] statements) : this(null, statements) {
        }

        public JsBlockStatement(string statementLabel, params JsStatement[] statements) : this(statements, statementLabel) {
        }

        public static JsBlockStatement MakeBlock(JsStatement content) {
            return content as JsBlockStatement ?? new JsBlockStatement(content);
        }

        private static readonly JsBlockStatement _emptyStatement = new JsBlockStatement(new JsStatement[0]);
        public static JsBlockStatement EmptyStatement { get { return _emptyStatement; } }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
