using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	internal class ContainsBreakVisitor : RewriterVisitorBase<object> {
		bool _result;
		bool _unnamedIsMatch;
		string _statementName;

		public override JsStatement VisitForStatement(JsForStatement statement, object data) {
			bool old = _unnamedIsMatch;
			_unnamedIsMatch = false;
			VisitBlockStatement(statement.Body, null);
			_unnamedIsMatch = old;
			return statement;
		}

		public override JsStatement VisitForEachInStatement(JsForEachInStatement statement, object data) {
			bool old = _unnamedIsMatch;
			_unnamedIsMatch = false;
			VisitBlockStatement(statement.Body, null);
			_unnamedIsMatch = old;
			return statement;
		}

		public override JsStatement VisitWhileStatement(JsWhileStatement statement, object data) {
			bool old = _unnamedIsMatch;
			_unnamedIsMatch = false;
			VisitBlockStatement(statement.Body, null);
			_unnamedIsMatch = old;
			return statement;
		}

		public override JsStatement VisitDoWhileStatement(JsDoWhileStatement statement, object data) {
			bool old = _unnamedIsMatch;
			_unnamedIsMatch = false;
			VisitBlockStatement(statement.Body, null);
			_unnamedIsMatch = old;
			return statement;
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			return statement;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			return expression;
		}

		public override JsStatement VisitSwitchStatement(JsSwitchStatement statement, object data) {
			bool old = _unnamedIsMatch;
			_unnamedIsMatch = false;
			VisitSwitchSections(statement.Sections, null);
			_unnamedIsMatch = old;
			return statement;
		}

		public override JsStatement VisitBreakStatement(JsBreakStatement statement, object data) {
			if (statement.TargetLabel == null && _unnamedIsMatch || (statement.TargetLabel != null && statement.TargetLabel == _statementName))
				_result = true;
			return statement;
		}

		public bool Analyze(JsBlockStatement block, string statementName) {
			_result = false;
			_unnamedIsMatch = true;
			_statementName = statementName;
			VisitStatement(block, null);
			return _result;
		}
	}
}