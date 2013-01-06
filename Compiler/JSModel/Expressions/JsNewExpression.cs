using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class JsNewExpression : JsExpression {
        public JsExpression Constructor { get; private set; }

		/// <summary>
		/// Can be null, in which case the expression does not have an argument list (as opposed to having a list with zero arguments).
		/// </summary>
        public ReadOnlyCollection<JsExpression> Arguments { get; private set; }

        internal JsNewExpression(JsExpression constructor, IEnumerable<JsExpression> arguments) : base(ExpressionNodeType.New) {
            if (constructor == null) throw new ArgumentNullException("constructor");

            Constructor = constructor;
            Arguments = arguments != null ? arguments.AsReadOnly() : null;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.VisitNewExpression(this, data);
        }
    }
}
