using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
    /// <summary>
    /// A MemberExpression represents an elements which applies on a previous Expression
    /// </summary>
    [Serializable]
    public class JsMemberAccessExpression : JsExpression {
        public const string Prototype = "prototype";

        public JsExpression Target { get; private set; }
        public string Member { get; private set; }

        internal JsMemberAccessExpression(JsExpression target, string member) : base(ExpressionNodeType.MemberAccess) {
            if (target == null) throw new ArgumentNullException("target");
            if (member == null) throw new ArgumentNullException("member");
            if (!member.IsValidJavaScriptIdentifier()) throw new ArgumentException("member");

            Member = member;
            Target = target;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
