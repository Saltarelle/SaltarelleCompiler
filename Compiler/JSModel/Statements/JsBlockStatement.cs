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

		[Obsolete("Use factory method JsStatement.Block")]
		public JsBlockStatement(IEnumerable<JsStatement> statements, bool mergeWithParent = false) {
			if (statements == null) throw new ArgumentNullException("statements");
			Statements = statements.AsReadOnly();
			MergeWithParent = mergeWithParent;
		}

		[Obsolete("Use factory method JsStatement.Block")]
		public JsBlockStatement(params JsStatement[] statements) : this((IEnumerable<JsStatement>)statements) {
		}

		[Obsolete("Use JsStatement.EnsureBlock")]
		public static JsBlockStatement MakeBlock(JsStatement content) {
			return EnsureBlock(content);
		}

		[Obsolete("Use JsStatement.EmptyBlock")]
		public static JsBlockStatement EmptyStatement { get { return EmptyBlock; } }

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitBlockStatement(this, data);
		}
	}
}
