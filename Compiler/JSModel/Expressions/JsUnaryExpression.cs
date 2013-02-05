using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
	[Serializable]
	public class JsUnaryExpression : JsExpression {
		public JsExpression Operand { get; private set; }

		internal JsUnaryExpression(ExpressionNodeType nodeType, JsExpression operand) : base(nodeType) {
			if (nodeType < ExpressionNodeType.UnaryFirst || nodeType > ExpressionNodeType.UnaryLast) throw new ArgumentException("nodeType");
			if (operand == null) throw new ArgumentNullException("operand");

			this.Operand = operand;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitUnaryExpression(this, data);
		}
	}
}
