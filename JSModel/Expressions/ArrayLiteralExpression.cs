using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class ArrayLiteralExpression : Expression {
        public ReadOnlyCollection<Expression> Elements { get; private set; }

        public ArrayLiteralExpression(IEnumerable<Expression> elements) {
            if (elements == null) throw new ArgumentNullException("elements");
            Elements = elements.AsReadOnly();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IExpressionVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }
}
