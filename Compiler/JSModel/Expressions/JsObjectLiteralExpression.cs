using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Expressions {
	[Serializable]
	public class JsObjectLiteralExpression : JsExpression {
		public ReadOnlyCollection<JsObjectLiteralProperty> Values { get; private set; }

		internal JsObjectLiteralExpression(IEnumerable<JsObjectLiteralProperty> values) : base(ExpressionNodeType.ObjectLiteral) {
			if (values == null) throw new ArgumentNullException("values");
			Values = values.AsReadOnly();
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitObjectLiteralExpression(this, data);
		}
	}
}
