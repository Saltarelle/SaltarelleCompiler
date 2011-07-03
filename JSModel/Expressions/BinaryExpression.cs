using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class BinaryExpression : Expression {
        public Expression Left { get; private set; }
        public Expression Right { get; private set; }

        internal BinaryExpression(ExpressionNodeType nodeType, Expression left, Expression right) : base(nodeType) {
            if (nodeType < ExpressionNodeType.BinaryFirst || nodeType > ExpressionNodeType.BinaryLast) throw new ArgumentException("nodeType");
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");

            Left  = left;
            Right = right;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }

        public override string ToString() {
            return NodeType.ToString() + " (" + Left.ToString() + ", " + Right.ToString() + ")";
        }
    }
}
