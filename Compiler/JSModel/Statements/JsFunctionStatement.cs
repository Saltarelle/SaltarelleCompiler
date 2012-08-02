using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsFunctionStatement : JsStatement {
        public string Name { get; private set; }
        public ReadOnlyCollection<string> ParameterNames { get; private set; }
        public JsBlockStatement Body { get; private set; }

        public JsFunctionStatement(string name, IEnumerable<string> parameterNames, JsStatement body) {
            if (!name.IsValidJavaScriptIdentifier()) throw new ArgumentException("name");
            if (parameterNames == null) throw new ArgumentNullException("parameterNames");
            if (body == null) throw new ArgumentNullException("body");

            ParameterNames = parameterNames.AsReadOnly();
            Body = JsBlockStatement.MakeBlock(body);
            Name = name;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.VisitFunctionStatement(this, data);
        }
    }
}
