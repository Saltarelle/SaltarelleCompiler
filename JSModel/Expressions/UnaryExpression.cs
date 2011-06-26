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

        public override int Precedence {
            get {
                switch (Operator) {
                    case UnaryOperator.PrefixPlusPlus:
                    case UnaryOperator.PrefixMinusMinus:
                    case UnaryOperator.PostfixPlusPlus:
                    case UnaryOperator.PostfixMinusMinus:
                        return ExpressionPrecedence.IncrDecr;

                    case UnaryOperator.LogicalNot:
                    case UnaryOperator.BitwiseNot:
                    case UnaryOperator.Positive:
                    case UnaryOperator.Negate:
                    case UnaryOperator.TypeOf:
                    case UnaryOperator.Void:
                    case UnaryOperator.Delete:
                        return ExpressionPrecedence.OtherUnary;
                    default:
                        throw new InvalidOperationException("Invalid operator");
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }

    public enum UnaryOperator {
        TypeOf,
        LogicalNot,
        Negate,
        Positive,
        PrefixPlusPlus,
        PrefixMinusMinus,
        PostfixPlusPlus,
        PostfixMinusMinus,
        Delete,
        Void,
        BitwiseNot,
    }
}
