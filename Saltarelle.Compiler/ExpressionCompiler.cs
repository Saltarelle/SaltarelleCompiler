using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Semantics;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler {
	public class ExpressionCompiler : ResolveResultVisitor {
		public class Result {
			public JsExpression Expression { get; set; }
			public ReadOnlyCollection<JsStatement> AdditionalStatements { get; private set; }

			public Result(JsExpression expression, IEnumerable<JsStatement> additionalStatements) {
				this.Expression           = expression;
				this.AdditionalStatements = additionalStatements.AsReadOnly();
			}
		}

		public ExpressionCompiler() {
		}

		private JsExpression _expression;
		private List<JsStatement> _additionalExpressions;

		public Result Compile(ResolveResult expression) {
			_expression = null;
			_additionalExpressions = new List<JsStatement>();

			VisitResolveResult(expression);

			return new Result(_expression, _additionalExpressions);
		}
	}
}
