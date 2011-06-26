using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class BinaryExpression : Expression {
        public Expression Left { get; private set; }
        public Expression Right { get; private set; }
        public BinaryOperator Operator { get; private set; }

        public BinaryExpression(BinaryOperator oper, Expression left, Expression right) {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");

            this.Operator = oper;
            this.Left     = left;
            this.Right    = right;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IExpressionVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }

        public override string ToString() {
            return Operator.ToString() + " (" + Left.ToString() + ", " + Right.ToString() + ")";
        }
    }

    public enum BinaryOperator {
        And,
        Or,
        NotEqual,
        LesserOrEqual,
        GreaterOrEqual,
        Lesser,
        Greater,
        Equal,
        Subtract,
        Add,
        Modulo,
        Divide,
        Multiply,
        Pow,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXOr,
        Same,
        NotSame,
        LeftShift,
        RightShift,
        UnsignedRightShift,
        InstanceOf,
        In,
        Index,

        Assign,
        MultiplyAssign,
        DivideAssign,
        ModuloAssign,
        AddAssign,
        SubtractAssign,
        LeftShiftAssign,
        RightShiftAssign,
        UnsignedRightShiftAssign,
        BitwiseAndAssign,
        BitwiseOrAssign,
        BitwiseXOrAssign,
    }


}
