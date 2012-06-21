using System;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public abstract class JsStatement {
        [System.Diagnostics.DebuggerStepThrough]
        public abstract TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data);
    }
}
