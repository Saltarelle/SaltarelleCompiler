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
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }

        public override string ToString() {
            return Operator.ToString() + " (" + Left.ToString() + ", " + Right.ToString() + ")";
        }

        public bool IsAssignment { get { return Operator >= BinaryOperator.AssignFirst && Operator <= BinaryOperator.AssignLast; } }

        public bool IsRightAssociative { get { return IsAssignment; } }

        public override int Precedence {
            get {
                switch (Operator) {
                    case BinaryOperator.Index:
                        return ExpressionPrecedence.MemberOrNew;

                    case BinaryOperator.Multiply:
                    case BinaryOperator.Divide:
                    case BinaryOperator.Modulo:
                        return ExpressionPrecedence.Multiply;

                    case BinaryOperator.Add:
                    case BinaryOperator.Subtract:
                        return ExpressionPrecedence.Addition;

                    case BinaryOperator.LeftShift:
                    case BinaryOperator.RightShiftSigned:
                    case BinaryOperator.RightShiftUnsigned:
                        return ExpressionPrecedence.BitwiseShift;

                    case BinaryOperator.Lesser:
                    case BinaryOperator.LesserOrEqual:
                    case BinaryOperator.Greater:
                    case BinaryOperator.GreaterOrEqual:
                    case BinaryOperator.In:
                    case BinaryOperator.InstanceOf:
                        return ExpressionPrecedence.Relational;

                    case BinaryOperator.Equal:
                    case BinaryOperator.NotEqual:
                    case BinaryOperator.Same:
                    case BinaryOperator.NotSame:
                        return ExpressionPrecedence.Equality;

                    case BinaryOperator.BitwiseAnd:
                        return ExpressionPrecedence.BitwiseAnd;

                    case BinaryOperator.BitwiseXor:
                        return ExpressionPrecedence.BitwiseXor;

                    case BinaryOperator.BitwiseOr:
                        return ExpressionPrecedence.BitwiseOr;

                    case BinaryOperator.LogicalAnd:
                        return ExpressionPrecedence.LogicalAnd;

                    case BinaryOperator.LogicalOr:
                        return ExpressionPrecedence.LogicalOr;

                    case BinaryOperator.Assign:
                    case BinaryOperator.MultiplyAssign:
                    case BinaryOperator.DivideAssign:
                    case BinaryOperator.ModuloAssign:
                    case BinaryOperator.AddAssign:
                    case BinaryOperator.SubtractAssign:
                    case BinaryOperator.LeftShiftAssign:
                    case BinaryOperator.RightShiftAssign:
                    case BinaryOperator.UnsignedRightShiftAssign:
                    case BinaryOperator.BitwiseAndAssign:
                    case BinaryOperator.BitwiseOrAssign:
                    case BinaryOperator.BitwiseXOrAssign:
                        return ExpressionPrecedence.Assignment;
                    default:
                        throw new InvalidOperationException("Invalid operator " + Operator.ToString());
                }
            }
        }
    }

    public enum BinaryOperator {
        LogicalAnd,
        LogicalOr,
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
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        Same,
        NotSame,
        LeftShift,
        RightShiftSigned,
        RightShiftUnsigned,
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

        AssignFirst = Assign,
        AssignLast = BitwiseXOrAssign
    }


}
