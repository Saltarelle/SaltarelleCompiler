using System;
using System.Diagnostics;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
	[DebuggerDisplay("{DebugToString()}")]
    public abstract class JsStatement {
        [System.Diagnostics.DebuggerStepThrough]
        public abstract TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data);

		public string DebugToString() {
			return new System.Text.RegularExpressions.Regex("\\s+").Replace(OutputFormatter.Format(this, true), " ");
		}
    }
}
