using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class NewExpression : Expression {
        public Expression Constructor { get; private set; }
        public ReadOnlyCollection<Expression> Arguments { get; private set; }

        internal NewExpression(Expression constructor, IEnumerable<Expression> arguments) : base(ExpressionNodeType.New) {
            if (constructor == null) throw new ArgumentNullException("constructor");
            if (arguments == null) throw new ArgumentNullException("arguments");

            Constructor = constructor;
            Arguments = arguments.AsReadOnly();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
