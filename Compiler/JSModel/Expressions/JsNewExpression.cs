using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class JsNewExpression : JsExpression {
        public JsExpression Constructor { get; private set; }
        public ReadOnlyCollection<JsExpression> Arguments { get; private set; }

        internal JsNewExpression(JsExpression constructor, IEnumerable<JsExpression> arguments) : base(ExpressionNodeType.New) {
            if (constructor == null) throw new ArgumentNullException("constructor");
            if (arguments == null) throw new ArgumentNullException("arguments");

            Constructor = constructor;
            Arguments = arguments.AsReadOnly();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.VisitNewExpression(this, data);
        }
    }
}
