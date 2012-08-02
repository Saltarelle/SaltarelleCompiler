using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class JsInvocationExpression : JsExpression {
        public JsExpression Method { get; private set; }
        public ReadOnlyCollection<JsExpression> Arguments { get; private set; }

        internal JsInvocationExpression(JsExpression method, IEnumerable<JsExpression> arguments) : base(ExpressionNodeType.Invocation) {
            if (method == null) throw new ArgumentNullException("method");
            if (arguments == null) throw new ArgumentNullException("arguments");
            Method = method;
            Arguments = arguments.AsReadOnly();
			if (Arguments.Any(a => a == null)) throw new ArgumentException("All arguments must be non-null", "arguments");
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.VisitInvocationExpression(this, data);
        }
    }
}
