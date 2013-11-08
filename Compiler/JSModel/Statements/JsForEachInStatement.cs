using System;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsForEachInStatement : JsStatement {
		/// <summary>
		/// Whether the loop variable is declared. for (x in y) => !IsVariableDeclared. for (var x in y) => IsVariableDeclared.
		/// </summary>
		public bool IsLoopVariableDeclared { get; private set; }
		public string LoopVariableName { get; private set; }
		public JsExpression ObjectToIterateOver { get; private set; }
		public JsBlockStatement Body { get; private set; }

		[Obsolete("Use factory method JsStatement.ForIn")]
		public JsForEachInStatement(string loopVariableName, JsExpression objectToIterateOver, JsStatement body, bool isLoopVariableDeclared = true) {
			if (loopVariableName == null) throw new ArgumentNullException("loopVariableName");
			if (!loopVariableName.IsValidJavaScriptIdentifier()) throw new ArgumentException("loopVariableName");
			if (objectToIterateOver == null) throw new ArgumentNullException("objectToIterateOver");
			if (body == null) throw new ArgumentNullException("body");

			LoopVariableName       = loopVariableName;
			ObjectToIterateOver    = objectToIterateOver;
			Body                   = EnsureBlock(body);
			IsLoopVariableDeclared = isLoopVariableDeclared;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
			return visitor.VisitForEachInStatement(this, data);
		}
	}
}
