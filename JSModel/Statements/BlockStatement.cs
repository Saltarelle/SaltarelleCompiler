using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class BlockStatement : Statement {
        public ReadOnlyCollection<Statement> Statements { get; private set; }

        public BlockStatement(IEnumerable<Statement> statements, string statementLabel = null) : base(statementLabel) {
            if (statements == null) throw new ArgumentNullException("statements");
            Statements = statements.AsReadOnly();
        }

        public BlockStatement(params Statement[] statements) : this(null, statements) {
        }

        public BlockStatement(string statementLabel, params Statement[] statements) : this(statements, statementLabel) {
        }

        public static BlockStatement MakeBlock(Statement content) {
            return content as BlockStatement ?? new BlockStatement(content);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IStatementVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }
}
