using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsSwitchStatement : JsStatement {
        [Serializable]
        public class Clause {
            /// <summary>
            /// Null for the default clause.
            /// </summary>
            public JsExpression Value { get; private set; }
            public ReadOnlyCollection<JsStatement> Body { get; private set; }

            public Clause(JsExpression value, IEnumerable<JsStatement> body) {
                if (body == null) throw new ArgumentNullException("body");
                Value = value;
                Body  = body.AsReadOnly();
            }
        }

        public JsExpression Test { get; private set; }
        public ReadOnlyCollection<Clause> Clauses { get; private set; }

        public JsSwitchStatement(JsExpression test, IEnumerable<Clause> clauses) {
            if (test == null) throw new ArgumentNullException("test");
            if (clauses == null) throw new ArgumentNullException("clauses");

            Test    = test;
            Clauses = clauses.AsReadOnly();

            if (clauses.Count(c => c.Value == null) > 1) throw new ArgumentException("Can only have one default clause", "clauses");
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
