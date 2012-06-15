using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.JSModel.Expressions {
    public class JsTypeReferenceExpression : JsExpression {
        public IAssembly Assembly { get; private set; }
		public string TypeName { get; private set; }

        public JsTypeReferenceExpression(IAssembly assembly, string typeName) : base(ExpressionNodeType.TypeReference) {
            Require.ValidJavaScriptNestedIdentifier(typeName, "typeName");
			Assembly = assembly;
			TypeName = typeName;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
