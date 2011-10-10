using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class JsArrayLiteralExpression : JsExpression {
        public ReadOnlyCollection<JsExpression> Elements { get; private set; }

        internal JsArrayLiteralExpression(IEnumerable<JsExpression> elements) : base(ExpressionNodeType.ArrayLiteral) {
            if (elements == null) throw new ArgumentNullException("elements");
            Elements = elements.AsReadOnly();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
