using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class ConditionalExpression : Expression {
        public Expression Test { get; private set; }
        public Expression TruePart { get; private set; }
        public Expression FalsePart { get; private set; }

        public ConditionalExpression(Expression test, Expression truePart, Expression falsePart) {
            if (test == null) throw new ArgumentNullException("test");
            if (truePart == null) throw new ArgumentNullException("truePart");
            if (falsePart == null) throw new ArgumentNullException("falsePart");

            this.Test      = test;
            this.TruePart  = truePart;
            this.FalsePart = falsePart;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IExpressionVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }

        public override string ToString() {
            return Test.ToString() + " (" + TruePart.ToString() + ", " + FalsePart.ToString() + ")";
        }
    }

}
