using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

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

        public JsVariableDeclarationStatement(string name, JsExpression initializer) : this(new JsVariableDeclaration(name, initializer)) {
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.VisitVariableDeclarationStatement(this, data);
        }
    }
}
