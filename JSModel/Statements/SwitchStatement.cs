using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class SwitchStatement : Statement {
        [Serializable]
        public class Clause {
            /// <summary>
            /// Null for the default clause.
            /// </summary>
            public Expression Value { get; private set; }
            public ReadOnlyCollection<Statement> Body { get; private set; }

            public Clause(Expression value, IEnumerable<Statement> body) {
                if (body == null) throw new ArgumentNullException("body");
                Value = value;
                Body  = body.AsReadOnly();
            }
        }

        public Expression Test { get; private set; }
        public ReadOnlyCollection<Clause> Clauses { get; private set; }

        public SwitchStatement(Expression test, IEnumerable<Clause> clauses, string statementLabel = null) : base(statementLabel) {
            if (test == null) throw new ArgumentNullException("test");
            if (clauses == null) throw new ArgumentNullException("clauses");

            Test    = test;
            Clauses = clauses.AsReadOnly();

            if (clauses.Count(c => c.Value == null) > 1) throw new ArgumentException("Can only have one default clause", "clauses");
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn>(IStatementVisitor<TReturn> visitor) {
            return visitor.Visit(this);
        }
    }
}
