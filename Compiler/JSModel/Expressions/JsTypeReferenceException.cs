using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler.JSModel.Expressions {
	public class JsTypeReferenceExpression : JsExpression {
		public INamedTypeSymbol Type { get; private set; }

		public JsTypeReferenceExpression(INamedTypeSymbol type) : base(ExpressionNodeType.TypeReference) {
			this.Type = type;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitTypeReferenceExpression(this, data);
		}
	}
}
