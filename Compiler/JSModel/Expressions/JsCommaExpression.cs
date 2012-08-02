using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class JsCommaExpression : JsExpression {
        public ReadOnlyCollection<JsExpression> Expressions { get; set; }

        internal JsCommaExpression(IEnumerable<JsExpression> expressions) : base(ExpressionNodeType.Comma) {
            if (expressions == null) throw new ArgumentNullException("expressions");
            Expressions = expressions.AsReadOnly();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.VisitCommaExpression(this, data);
        }
    }
}
