using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class CommaExpression : Expression {
        public ReadOnlyCollection<Expression> Expressions { get; set; }

        internal CommaExpression(IEnumerable<Expression> expressions) : base(ExpressionNodeType.Comma) {
            if (expressions == null) throw new ArgumentNullException("expressions");
            Expressions = expressions.AsReadOnly();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
