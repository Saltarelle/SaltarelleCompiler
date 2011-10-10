using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsVariableDeclarationStatement : JsStatement {
        public class VariableDeclaration {
            public string Name { get; private set; }
            /// <summary>
            /// Null if the variable is not initialized.
            /// </summary>
            public JsExpression Initializer { get; private set; }

            public VariableDeclaration(string name, JsExpression initializer) {
                if (name == null) throw new ArgumentNullException("name");
                if (!name.IsValidJavaScriptIdentifier()) throw new ArgumentException("name");
                Name        = name;
                Initializer = initializer;
            }
        }

        public ReadOnlyCollection<VariableDeclaration> Declarations { get; private set; }

        public JsVariableDeclarationStatement(IEnumerable<VariableDeclaration> declarations, string statementLabel = null) : base(statementLabel) {
            if (declarations == null) throw new ArgumentNullException("declarations");
            Declarations = declarations.AsReadOnly();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
