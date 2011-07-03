using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class ArrayLiteralExpression : Expression {
        public ReadOnlyCollection<Expression> Elements { get; private set; }

        internal ArrayLiteralExpression(IEnumerable<Expression> elements) : base(ExpressionNodeType.ArrayLiteral) {
            if (elements == null) throw new ArgumentNullException("elements");
            Elements = elements.AsReadOnly();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
