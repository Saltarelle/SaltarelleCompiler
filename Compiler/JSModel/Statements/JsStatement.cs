using System;
using System.Diagnostics;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	[DebuggerDisplay("{DebugToString()}")]
	public abstract class JsStatement {
		[System.Diagnostics.DebuggerStepThrough]
		public abstract TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data);

		public string DebugToString() {
			return new System.Text.RegularExpressions.Regex("\\s+").Replace(OutputFormatter.Format(this, true), " ");
		}

		public static JsStatement UseStrict {
			get { return new JsExpressionStatement(JsExpression.String("use strict")); }
		}
	}
}
