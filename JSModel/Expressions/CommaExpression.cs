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

        public CommaExpression(params Expression[] expressions) : this((IEnumerable<Expression>)expressions) {
        }

        public override int Precedence { get { return ExpressionPrecedence.Comma; } }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
