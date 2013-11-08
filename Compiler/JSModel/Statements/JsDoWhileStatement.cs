﻿using System;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsDoWhileStatement : JsStatement {
		public JsExpression Condition { get; private set; }
		public JsBlockStatement Body { get; private set; }

		[Obsolete("Use factory method JsStatement.DoWhile")]
		public JsDoWhileStatement(JsExpression condition, JsStatement body) {
			if (condition == null) throw new ArgumentNullException("condition");
			if (body == null) throw new ArgumentNullException("body");
			Condition = condition;
			Body = EnsureBlock(body);
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitDoWhileStatement(this, data);
		}
	}
}
