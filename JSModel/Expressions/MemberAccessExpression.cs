using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
    /// <summary>
    /// A MemberExpression represents an elements which applies on a previous Expression
    /// </summary>
    [Serializable]
    public class MemberAccessExpression : Expression {
        public const string Prototype = "prototype";

        public Expression Target { get; private set; }
        public string Member { get; private set; }

        public MemberAccessExpression(Expression target, string member) {
            if (target == null) throw new ArgumentNullException("target");
            if (member == null) throw new ArgumentNullException("member");
            if (!member.IsValidJavaScriptIdentifier()) throw new ArgumentException("member");

            Member = member;
            Target = target;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IExpressionVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }
}
