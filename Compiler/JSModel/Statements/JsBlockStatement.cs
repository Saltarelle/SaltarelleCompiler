using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Statements {
    [Serializable]
    public class JsBlockStatement : JsStatement {
        public ReadOnlyCollection<JsStatement> Statements { get; private set; }
		/// <summary>
		/// If true, this statement should be merged with its parent (if possible). Primarilly useful in rewriters.
		/// </summary>
		public bool MergeWithParent { get; private set; }

        public JsBlockStatement(IEnumerable<JsStatement> statements, bool mergeWithParent = false) {
            if (statements == null) throw new ArgumentNullException("statements");
            Statements = statements.AsReadOnly();
			MergeWithParent = mergeWithParent;
        }

        public JsBlockStatement(params JsStatement[] statements) : this((IEnumerable<JsStatement>)statements) {
        }

        /// <summary>
        /// Convert a statement to a block statement. Returns null if the input is null.
        /// </summary>
		public static JsBlockStatement MakeBlock(JsStatement content) {
			if (content == null)
				return null;
			else if (content is JsBlockStatement)
				return (JsBlockStatement)content;
			else
				return new JsBlockStatement(content);
        }

        private static readonly JsBlockStatement _emptyStatement = new JsBlockStatement(new JsStatement[0]);
        public static JsBlockStatement EmptyStatement { get { return _emptyStatement; } }

        [System.Diagnostics.DebuggerStepThrough]
        public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
            return visitor.VisitBlockStatement(this, data);
        }
    }
}
