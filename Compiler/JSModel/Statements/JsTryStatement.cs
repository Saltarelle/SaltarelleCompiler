using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsTryStatement : JsStatement {
		public JsBlockStatement GuardedStatement { get; private set; }

		/// <summary>
		/// Can be null if the catch is not specified (but then there must be a finally).
		/// </summary>
		public new JsCatchClause Catch { get; private set; }

		/// <summary>
		/// Can be null if the finally is not specified (but then there must be a catch).
		/// </summary>
		public JsBlockStatement Finally { get; private set; }

		[Obsolete("Use factory method JsStatement.Try")]
		public JsTryStatement(JsStatement guardedStatement, JsCatchClause catchClause, JsStatement @finally) {
			if (guardedStatement == null) throw new ArgumentException("guardedStatement");
			if (catchClause == null && @finally == null) throw new ArgumentException("Either catchClause or finally (or both) must be specified");

			GuardedStatement = EnsureBlock(guardedStatement);
			Catch            = catchClause;
			Finally          = EnsureBlock(@finally);
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitTryStatement(this, data);
		}
	}
}
