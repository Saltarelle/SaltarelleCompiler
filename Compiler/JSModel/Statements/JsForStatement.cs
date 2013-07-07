using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsForStatement : JsStatement {
		/// <summary>
		/// Initialization statement (first part). Must be a VariableDeclarationStatement, an ExpressionStatement or an EmptyStatement.
		/// </summary>
		public JsStatement InitStatement { get; private set; }
		public JsExpression ConditionExpression { get; private set; }
		public JsExpression IteratorExpression { get; private set; }
		public JsBlockStatement Body { get; private set; }

		[Obsolete("Use factory method JsStatement.For")]
		public JsForStatement(JsStatement initStatement, JsExpression conditionExpression, JsExpression iteratorExpression, JsStatement body) {
			if (initStatement == null) throw new ArgumentNullException("initStatement");
			if (!(initStatement is JsEmptyStatement || initStatement is JsVariableDeclarationStatement || initStatement is JsExpressionStatement)) throw new ArgumentException("initStatement must be a VariableDeclarationStatement or an ExpressionStatement.", "initStatement");
			if (body == null) throw new ArgumentNullException("body");
			
			InitStatement       = initStatement;
			ConditionExpression = conditionExpression;
			IteratorExpression  = iteratorExpression;
			Body                = EnsureBlock(body);
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitForStatement(this, data);
		}
	}
}
