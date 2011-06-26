using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class IdentifierExpression : Expression {
        public string Name { get; private set; }

        public IdentifierExpression(string name) {
            if (name == null) throw new ArgumentNullException("name");
            if (!name.IsValidJavaScriptIdentifier()) throw new ArgumentException("name");
            Name = name;
        }

        public override string ToString() {
            return Name;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IExpressionVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }
}
