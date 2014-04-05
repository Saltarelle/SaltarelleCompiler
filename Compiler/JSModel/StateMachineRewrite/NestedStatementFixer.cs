using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	internal class NestedStatementFixer : RewriterVisitorBase<object>, IStateMachineRewriterIntermediateStatementsVisitor<JsStatement, object> {
		private ImmutableStack<Tuple<string, State>> _breakStack;
		private ImmutableStack<Tuple<string, State>> _continueStack;
		private readonly State _currentState;
		private readonly State _exitState;
		private readonly Func<JsExpression, JsExpression> _makeSetResult;

		public NestedStatementFixer(ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State exitState, Func<JsExpression, JsExpression> makeSetResult) {
			_breakStack = breakStack;
			_continueStack = continueStack;
			_currentState = currentState;
			_exitState = exitState;
			_makeSetResult = makeSetResult;
		}

		public JsBlockStatement Process(JsBlockStatement statement) {
			return (JsBlockStatement)VisitStatement(statement, null);
		}

		public IEnumerable<JsStatement> Process(JsStatement statement) {
			var result = VisitStatement(statement, null);
			var block = result as JsBlockStatement;
			return block != null && block.MergeWithParent ? block.Statements : (IEnumerable<JsStatement>)new[] { result };
		}

		private JsStatement VisitLoopBody(JsBlockStatement block, object data) {
			var oldBreak = _breakStack;
			var oldContinue = _continueStack;
			try {
				_breakStack = _breakStack.Push(null);
				_continueStack = _continueStack.Push(null);
				return VisitStatement(block, data);
			}
			finally {
				_breakStack = oldBreak;
				_continueStack = oldContinue;
			}
		}

		public override JsStatement VisitForStatement(JsForStatement statement, object data) {
			var body = VisitLoopBody(statement.Body, data);
			return ReferenceEquals(body, statement.Body) ? statement : JsStatement.For(statement.InitStatement, statement.ConditionExpression, statement.IteratorExpression, body);
		}

		public override JsStatement VisitForEachInStatement(JsForEachInStatement statement, object data) {
			var body = VisitLoopBody(statement.Body, data);
			return ReferenceEquals(body, statement.Body) ? statement : JsStatement.ForIn(statement.LoopVariableName, statement.ObjectToIterateOver, body, statement.IsLoopVariableDeclared);
		}

		public override JsStatement VisitWhileStatement(JsWhileStatement statement, object data) {
			var body = VisitLoopBody(statement.Body, data);
			return ReferenceEquals(body, statement.Body) ? statement : JsStatement.While(statement.Condition, body);
		}

		public override JsStatement VisitDoWhileStatement(JsDoWhileStatement statement, object data) {
			var body = VisitLoopBody(statement.Body, data);
			return ReferenceEquals(body, statement.Body) ? statement : JsStatement.DoWhile(statement.Condition, body);
		}

		public override JsStatement VisitSwitchStatement(JsSwitchStatement statement, object data) {
			var oldBreak = _breakStack;
			var oldContinue = _continueStack;
			try {
				_breakStack = _breakStack.Push(null);
				_continueStack = _continueStack.Push(null);
				var sections = VisitSwitchSections(statement.Sections, data);
				return ReferenceEquals(sections, statement.Sections) ? statement : JsStatement.Switch(statement.Expression, sections);
			}
			finally {
				_breakStack = oldBreak;
				_continueStack = oldContinue;
			}
		}

		public override JsStatement VisitBreakStatement(JsBreakStatement statement, object data) {
			Tuple<string, State> state;
			if (statement.TargetLabel == null) {
				state = _breakStack.Peek();
			}
			else {
				state = _breakStack.SingleOrDefault(x => x != null && x.Item1 == statement.TargetLabel);
			}

			if (state != null) {
				return new JsGotoStateStatement(state.Item2, _currentState);
			}
			else {
				return statement;
			}
		}

		public override JsStatement VisitContinueStatement(JsContinueStatement statement, object data) {
			Tuple<string, State> state;
			if (statement.TargetLabel == null) {
				state = _continueStack.Peek();
			}
			else {
				state = _continueStack.SingleOrDefault(x => x != null && x.Item1 == statement.TargetLabel);
			}

			if (state != null) {
				return new JsGotoStateStatement(state.Item2, _currentState);
			}
			else {
				return statement;
			}
		}

		public override JsStatement VisitYieldStatement(JsYieldStatement statement, object data) {
			if (statement.Value != null)
				throw new InvalidOperationException("yield return should have been already taken care of.");
			return new JsGotoStateStatement(_exitState, _currentState);
		}

		public override JsStatement VisitGotoStatement(JsGotoStatement statement, object data) {
			return new JsGotoStateStatement(statement.TargetLabel, _currentState);
		}

		public override JsStatement VisitReturnStatement(JsReturnStatement statement, object data) {
			if (_makeSetResult != null)
				return JsStatement.BlockMerged(_makeSetResult(statement.Value), JsStatement.Return());
			else
				return statement;
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			return statement;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			return expression;
		}

		public JsStatement VisitGotoStateStatement(JsGotoStateStatement stmt, object data) {
			return stmt;
		}

		public JsStatement VisitSetNextStateStatement(JsSetNextStateStatement stmt, object data) {
			return stmt;
		}
	}
}