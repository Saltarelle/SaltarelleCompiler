using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
	[Serializable]
	public class JsThisExpression : JsExpression {
		private JsThisExpression() : base(ExpressionNodeType.This) {
		}

		private static JsThisExpression _instance = new JsThisExpression();
		internal static new JsThisExpression This { get { return _instance; } }

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitThisExpression(this, data);
		}
	}
}
