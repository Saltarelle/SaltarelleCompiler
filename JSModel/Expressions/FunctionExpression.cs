using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class FunctionExpression : Expression {
        public ReadOnlyCollection<string> ParameterNames { get; private set; }
        public BlockStatement Body { get; private set; }

        /// <summary>
        /// Null if the function does not have a name.
        /// </summary>
        public string Name { get; private set; }

        public FunctionExpression(IEnumerable<string> parameterNames, Statement body, string name = null) {
            if (parameterNames == null) throw new ArgumentNullException("parameterNames");
            if (body == null) throw new ArgumentNullException("body");
            if (name != null && !name.IsValidJavaScriptIdentifier()) throw new ArgumentException("name");

            ParameterNames = parameterNames.AsReadOnly();
            Body = BlockStatement.MakeBlock(body);
            Name = name;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IExpressionVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }
}
