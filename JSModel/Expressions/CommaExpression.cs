using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class CommaExpression : Expression {
        public ReadOnlyCollection<Expression> Expressions { get; set; }

        public CommaExpression(IEnumerable<Expression> expressions) {
            if (expressions == null) throw new ArgumentNullException("expressions");
            Expressions = expressions.AsReadOnly();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IExpressionVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }
}
