using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class ArrayLiteralExpression : Expression {
        public ReadOnlyCollection<Expression> Elements { get; private set; }

        public override int Precedence { get { return ExpressionPrecedence.Terminal; } }

        public ArrayLiteralExpression(IEnumerable<Expression> elements) {
            if (elements == null) throw new ArgumentNullException("elements");
            Elements = elements.AsReadOnly();
        }

        public ArrayLiteralExpression(params Expression[] elements) : this((IEnumerable<Expression>)elements) {
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
