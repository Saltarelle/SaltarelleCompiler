using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.Expressions {
    [Serializable]
    public class FunctionDefinitionExpression : Expression {
        public ReadOnlyCollection<string> ParameterNames { get; private set; }
        public BlockStatement Body { get; private set; }

        /// <summary>
        /// Null if the function does not have a name.
        /// </summary>
        public string Name { get; private set; }

        internal FunctionDefinitionExpression(IEnumerable<string> parameterNames, Statement body, string name = null) : base(ExpressionNodeType.FunctionDefinition) {
            if (parameterNames == null) throw new ArgumentNullException("parameterNames");
            if (body == null) throw new ArgumentNullException("body");
            if (name != null && !name.IsValidJavaScriptIdentifier()) throw new ArgumentException("name");

            ParameterNames = parameterNames.AsReadOnly();
            Body = BlockStatement.MakeBlock(body);
            Name = name;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
