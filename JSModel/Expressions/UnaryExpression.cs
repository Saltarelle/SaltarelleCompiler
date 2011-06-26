using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class UnaryExpression : Expression {
        public Expression Operand { get; private set; }
        public UnaryOperator Operator { get; private set; }

        public UnaryExpression(UnaryOperator oper, Expression operand) {
            if (operand == null) throw new ArgumentNullException("operand");

            this.Operator = oper;
            this.Operand = operand;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IExpressionVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }

    public enum UnaryOperator {
        TypeOf,
        Not,
        Negate,
        Positive,
        PrefixPlusPlus,
        PrefixMinusMinus,
        PostfixPlusPlus,
        PostfixMinusMinus,
        Delete,
        Void,
        Inv,
        Unknown
    }
}
