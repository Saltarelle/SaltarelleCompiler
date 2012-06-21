using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
    public class JsSwitchStatement : JsStatement {
        public JsExpression Expression { get; private set; }
        public ReadOnlyCollection<JsSwitchSection> Clauses { get; private set; }

        public JsSwitchStatement(JsExpression expression, IEnumerable<JsSwitchSection> clauses) {
            if (expression == null) throw new ArgumentNullException("test");
            if (clauses == null) throw new ArgumentNullException("clauses");

            Expression = expression;
            Clauses    = clauses.AsReadOnly();

            if (Clauses.SelectMany(c => c.Values).Count(v => v == null) > 1) throw new ArgumentException("Can only have one default clause", "clauses");
        }

        public JsSwitchStatement(JsExpression expression, params JsSwitchSection[] clauses) : this(expression, (IEnumerable<JsSwitchSection>)clauses) {
		}

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.Visit(this, data);
        }
    }
}
