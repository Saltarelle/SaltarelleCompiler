using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsLabelStatement : JsStatement {
        public string Name { get; private set; }

        public JsLabelStatement(string name) {
			Require.ValidJavaScriptIdentifier(name, "name", allowNull: false);
			Name = name;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
