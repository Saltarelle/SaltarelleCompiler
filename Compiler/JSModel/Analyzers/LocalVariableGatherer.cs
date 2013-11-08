using System.Collections.Generic;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.Analyzers {
	public class LocalVariableGatherer : RewriterVisitorBase<HashSet<string>> {
		private readonly Dictionary<JsDeclarationScope, HashSet<string>> _result = new Dictionary<JsDeclarationScope, HashSet<string>>();

		private LocalVariableGatherer() {
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, HashSet<string> data) {
			return base.VisitFunctionDefinitionExpression(expression, _result[expression] = new HashSet<string>(expression.ParameterNames));
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, HashSet<string> data) {
			return base.VisitFunctionStatement(statement, _result[statement] = new HashSet<string>(statement.ParameterNames));
		}

		public override JsCatchClause VisitCatchClause(JsCatchClause clause, HashSet<string> data) {
			return base.VisitCatchClause(clause, _result[clause] = new HashSet<string> { clause.Identifier });
		}

		public override JsVariableDeclaration VisitVariableDeclaration(JsVariableDeclaration declaration, HashSet<string> data) {
			data.Add(declaration.Name);
			return base.VisitVariableDeclaration(declaration, data);
		}

		public override JsStatement VisitForEachInStatement(JsForEachInStatement statement, HashSet<string> data) {
			if (statement.IsLoopVariableDeclared)
				data.Add(statement.LoopVariableName);
			return base.VisitForEachInStatement(statement, data);
		}

		public static Dictionary<JsDeclarationScope, HashSet<string>> Analyze(JsStatement statement) {
			return Analyze(new[] { statement });
		}

		public static Dictionary<JsDeclarationScope, HashSet<string>> Analyze(IEnumerable<JsStatement> statements) {
			var obj = new LocalVariableGatherer();
			var rootData = new HashSet<string>();
			obj._result[JsDeclarationScope.Root] = rootData;
			foreach (var statement in statements)
				obj.VisitStatement(statement, rootData);
			return obj._result;
		}
	}
}