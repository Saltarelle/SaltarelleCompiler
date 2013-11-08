using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
	[Serializable]
	public class JsConditionalExpression : JsExpression {
		public JsExpression Test { get; private set; }
		public JsExpression TruePart { get; private set; }
		public JsExpression FalsePart { get; private set; }

		internal JsConditionalExpression(JsExpression test, JsExpression truePart, JsExpression falsePart) : base(ExpressionNodeType.Conditional) {
			if (test == null) throw new ArgumentNullException("test");
			if (truePart == null) throw new ArgumentNullException("truePart");
			if (falsePart == null) throw new ArgumentNullException("falsePart");

			this.Test      = test;
			this.TruePart  = truePart;
			this.FalsePart = falsePart;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitConditionalExpression(this, data);
		}

		public override string ToString() {
			return Test.ToString() + " (" + TruePart.ToString() + ", " + FalsePart.ToString() + ")";
		}
	}

}
