using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel {
	public class IdentifierRenamer : RewriterVisitorBase<IDictionary<string, string>> {
		private readonly IDictionary<JsDeclarationScope, IDictionary<string, string>> _renames;

		private IdentifierRenamer(IDictionary<JsDeclarationScope, IDictionary<string, string>> renames) {
			_renames = renames;
		}

		public override JsExpression VisitIdentifierExpression(JsIdentifierExpression expression, IDictionary<string, string> data) {
			string newName;
			return data.TryGetValue(expression.Name, out newName) ? JsExpression.Identifier(newName) : expression;
		}

		public override JsVariableDeclaration VisitVariableDeclaration(JsVariableDeclaration declaration, IDictionary<string, string> data) {
			string newName;
			if (data.TryGetValue(declaration.Name, out newName))
				return JsStatement.Declaration(newName, declaration.Initializer != null ? VisitExpression(declaration.Initializer, data) : null);
			else
				return base.VisitVariableDeclaration(declaration, data);
		}

		public override JsStatement VisitForEachInStatement(JsForEachInStatement statement, IDictionary<string, string> data) {
			string newName;
			if (data.TryGetValue(statement.LoopVariableName, out newName))
				return JsStatement.ForIn(newName, VisitExpression(statement.ObjectToIterateOver, data), VisitStatement(statement.Body, data), statement.IsLoopVariableDeclared);
			else
				return base.VisitForEachInStatement(statement, data);
		}

		public override JsCatchClause VisitCatchClause(JsCatchClause clause, IDictionary<string, string> data) {
			IDictionary<string, string> newData;
			if (!_renames.TryGetValue(clause, out newData))
				newData = data;

			string newName;
			if (newData.TryGetValue(clause.Identifier, out newName))
				return JsStatement.Catch(newName, VisitStatement(clause.Body, newData));
			else
				return base.VisitCatchClause(clause, newData);
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, IDictionary<string, string> data) {
			IDictionary<string, string> newData;
			if (!_renames.TryGetValue(expression, out newData))
				newData = data;
			bool renamed = false;
			var paramNames = expression.ParameterNames.Select(n => { string s; if (newData.TryGetValue(n, out s)) { renamed = true; return s; } else return n; }).ToList();
			if (renamed)
				return JsExpression.FunctionDefinition(paramNames, VisitStatement(expression.Body, newData), expression.Name);
			else
				return base.VisitFunctionDefinitionExpression(expression, newData);
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, IDictionary<string, string> data) {
			IDictionary<string, string> newData;
			if (!_renames.TryGetValue(statement, out newData))
				newData = data;
			bool renamed = false;
			var paramNames = statement.ParameterNames.Select(n => { string s; if (newData.TryGetValue(n, out s)) { renamed = true; return s; } else return n; }).ToList();
			if (renamed)
				return JsStatement.Function(statement.Name, paramNames, VisitStatement(statement.Body, newData));
			else
				return base.VisitFunctionStatement(statement, newData);
		}

		public static JsStatement Process(JsStatement statement, IDictionary<JsDeclarationScope, IDictionary<string, string>> renames) {
			IDictionary<string, string> data;
			if (!renames.TryGetValue(JsDeclarationScope.Root, out data))
				data = new Dictionary<string, string>();
			return new IdentifierRenamer(renames).VisitStatement(statement, data);
		}

		public static IEnumerable<JsStatement> Process(IEnumerable<JsStatement> statements, IDictionary<JsDeclarationScope, IDictionary<string, string>> renames) {
			IDictionary<string, string> data;
			if (!renames.TryGetValue(JsDeclarationScope.Root, out data))
				data = new Dictionary<string, string>();
			var obj = new IdentifierRenamer(renames);
			return statements.Select(s => obj.VisitStatement(s, data));
		}

		public static JsExpression Process(JsExpression expression, IDictionary<JsDeclarationScope, IDictionary<string, string>> renames) {
			IDictionary<string, string> data;
			if (!renames.TryGetValue(JsDeclarationScope.Root, out data))
				data = new Dictionary<string, string>();
			return new IdentifierRenamer(renames).VisitExpression(expression, data);
		}
	}
}
