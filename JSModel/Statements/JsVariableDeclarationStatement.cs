using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
    public class JsVariableDeclarationStatement : JsStatement {
        public ReadOnlyCollection<JsVariableDeclaration> Declarations { get; private set; }

        public JsVariableDeclarationStatement(IEnumerable<JsVariableDeclaration> declarations) {
            if (declarations == null) throw new ArgumentNullException("declarations");
            Declarations = declarations.AsReadOnly();
        }

        public JsVariableDeclarationStatement(params JsVariableDeclaration[] declarations) : this((IEnumerable<JsVariableDeclaration>)declarations) {
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
