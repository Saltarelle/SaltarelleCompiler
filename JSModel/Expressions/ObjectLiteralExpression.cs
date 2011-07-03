using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class ObjectLiteralExpression : Expression {
        public ReadOnlyCollection<ObjectLiteralProperty> Values { get; private set; }

        internal ObjectLiteralExpression(IEnumerable<ObjectLiteralProperty> values) : base(ExpressionNodeType.ObjectLiteral) {
            if (values == null) throw new ArgumentNullException("values");
            Values = values.AsReadOnly();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
