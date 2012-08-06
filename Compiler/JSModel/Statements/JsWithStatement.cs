using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsWithStatement : JsStatement {
        public JsExpression Object { get; private set; }
        public JsStatement Body { get; private set; }

        public JsWithStatement(JsExpression @object, JsStatement body) {
            if (@object == null) throw new ArgumentNullException("object");
            if (body == null) throw new ArgumentNullException("body");
            Object = @object;
            Body   = body;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.VisitWithStatement(this, data);
        }
    }
}
