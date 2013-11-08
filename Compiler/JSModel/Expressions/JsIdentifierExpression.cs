using System;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Expressions {
	[Serializable]
	public class JsIdentifierExpression : JsExpression {
		public string Name { get; private set; }

		internal JsIdentifierExpression(string name) : base(ExpressionNodeType.Identifier) {
			if (name == null) throw new ArgumentNullException("name");
			if (!name.IsValidJavaScriptIdentifier()) throw new ArgumentException("name");
			Name = name;
		}

		public override string ToString() {
			return Name;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitIdentifierExpression(this, data);
		}
	}
}
