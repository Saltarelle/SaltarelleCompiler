using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Compiler {
	public class ExpressionCompileResult {
		public JsExpression Expression { get; set; }
		public ReadOnlyCollection<JsStatement> AdditionalStatements { get; private set; }

		public ExpressionCompileResult(JsExpression expression, IEnumerable<JsStatement> additionalStatements) {
			this.Expression           = expression;
			this.AdditionalStatements = additionalStatements.AsReadOnly();
		}

		public IEnumerable<JsStatement> GetStatements() {
			foreach (var s in AdditionalStatements)
				yield return s;
			if (Expression.NodeType != ExpressionNodeType.Null)
				yield return Expression;
		}

		public IEnumerable<JsStatement> GetStatementsWithReturn() {
			foreach (var s in AdditionalStatements)
				yield return s;
			yield return JsStatement.Return(Expression);
		}
	}
}