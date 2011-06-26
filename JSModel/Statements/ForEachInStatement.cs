using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class ForEachInStatement : Statement {
        /// <summary>
        /// Whether the loop variable is declared. for (x in y) => !IsVariableDeclared. for (var x in y) => IsVariableDeclared.
        /// </summary>
        public bool IsLoopVariableDeclared { get; private set; }
        public string LoopVariableName { get; private set; }
        public Expression ObjectToIterateOver { get; private set; }
        public Statement Body { get; private set; }

        public ForEachInStatement(string loopVariableName, Expression objectToIterateOver, Statement body, bool isLoopVariableDeclared = true, string statementLabel = null) : base(statementLabel) {
            if (loopVariableName == null) throw new ArgumentNullException("loopVariableName");
            if (!loopVariableName.IsValidJavaScriptIdentifier()) throw new ArgumentException("loopVariableName");
            if (objectToIterateOver == null) throw new ArgumentNullException("objectToIterateOver");
            if (body == null) throw new ArgumentNullException("body");

            LoopVariableName       = loopVariableName;
            ObjectToIterateOver    = objectToIterateOver;
            Body                   = body;
            IsLoopVariableDeclared = isLoopVariableDeclared;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IStatementVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }
}
