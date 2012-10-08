using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.JSModel.Expressions {
	public class JsTypeReferenceExpression : JsExpression {
		public ITypeDefinition Type { get; private set; }

		public JsTypeReferenceExpression(ITypeDefinition type) : base(ExpressionNodeType.TypeReference) {
			this.Type = type;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitTypeReferenceExpression(this, data);
		}
	}
}
