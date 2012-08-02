using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Expressions {
	[Serializable]
	public class JsLiteralExpression : JsExpression {
        public string Format { get; private set; }
        public ReadOnlyCollection<JsExpression> Arguments { get; private set; }

        internal JsLiteralExpression(string format, IEnumerable<JsExpression> arguments) : base(ExpressionNodeType.Literal) {
            if (format == null) throw new ArgumentNullException("format");
            if (arguments == null) throw new ArgumentNullException("arguments");
            Format = format;
            Arguments = arguments.AsReadOnly();
			if (Arguments.Any(a => a == null)) throw new ArgumentException("All arguments must be non-null", "arguments");
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.VisitLiteralExpression(this, data);
        }
	}
}
