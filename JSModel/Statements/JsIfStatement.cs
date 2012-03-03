using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsIfStatement : JsStatement {
        public JsExpression Test { get; private set; }
        public JsStatement Then { get; private set; }
        /// <summary>
        /// Can be null if there is no else clause.
        /// </summary>
        public JsStatement Else { get; private set; }

        public JsIfStatement(JsExpression test, JsStatement then, JsStatement @else) {
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
