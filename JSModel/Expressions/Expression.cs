using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public abstract class Expression {
        [System.Diagnostics.DebuggerStepThrough]
        public abstract TReturn Accept<TReturn>(IExpressionVisitor<TReturn> visitor);
    }
}
