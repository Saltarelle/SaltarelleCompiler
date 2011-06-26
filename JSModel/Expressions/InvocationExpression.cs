using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class InvocationExpression : Expression {
        public Expression Method { get; private set; }
        public ReadOnlyCollection<Expression> Arguments { get; private set; }

        public InvocationExpression(Expression method, IEnumerable<Expression> arguments) {
            if (method == null) throw new ArgumentNullException("method");
            if (arguments == null) throw new ArgumentNullException("arguments");
            Method = method;
            Arguments = arguments.AsReadOnly();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IExpressionVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }
}
