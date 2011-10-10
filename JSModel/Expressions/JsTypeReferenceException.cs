using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.JSModel.Expressions {
    public class JsTypeReferenceExpression : JsExpression {
        public ITypeDefinition TypeDefinition { get; set; }

        public JsTypeReferenceExpression(ITypeDefinition typeDefinition) : base(ExpressionNodeType.TypeReference) {
            Require.NotNull(typeDefinition, "typeDefinition");
            TypeDefinition = typeDefinition;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
