using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsBlockStatement : JsStatement {
        public ReadOnlyCollection<JsStatement> Statements { get; private set; }

        public JsBlockStatement(IEnumerable<JsStatement> statements) {
            if (statements == null) throw new ArgumentNullException("statements");
            Statements = statements.AsReadOnly();
        }

        public JsBlockStatement(params JsStatement[] statements) : this((IEnumerable<JsStatement>)statements) {
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
