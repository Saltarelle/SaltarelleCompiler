using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
	[Serializable]
	public class JsBinaryExpression : JsExpression {
		public JsExpression Left { get; private set; }
		public JsExpression Right { get; private set; }

		internal JsBinaryExpression(ExpressionNodeType nodeType, JsExpression left, JsExpression right) : base(nodeType) {
			if (nodeType < ExpressionNodeType.BinaryFirst || nodeType > ExpressionNodeType.BinaryLast) throw new ArgumentException("nodeType");
			if (left == null) throw new ArgumentNullException("left");
			if (right == null) throw new ArgumentNullException("right");

			Left  = left;
			Right = right;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitBinaryExpression(this, data);
		}

		public override string ToString() {
			return NodeType.ToString() + " (" + Left.ToString() + ", " + Right.ToString() + ")";
		}
	}
}
