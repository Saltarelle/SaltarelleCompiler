using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Expressions {
	[Serializable]
	public class JsArrayLiteralExpression : JsExpression {
		/// <summary>
		/// The elements of the array. An item can be null eg. in [ 1, , 2 ].
		/// </summary>
		public ReadOnlyCollection<JsExpression> Elements { get; private set; }

		internal JsArrayLiteralExpression(IEnumerable<JsExpression> elements) : base(ExpressionNodeType.ArrayLiteral) {
			if (elements == null) throw new ArgumentNullException("elements");
			Elements = elements.AsReadOnly();
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitArrayLiteralExpression(this, data);
		}
	}
}
