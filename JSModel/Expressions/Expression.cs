using System;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public abstract class Expression {
        [System.Diagnostics.DebuggerStepThrough]
        public abstract TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data);

        /// <summary>
        /// Get precedence of an expression. 0 is highest.
        /// </summary>
        public abstract int Precedence { get; }
    }
}
