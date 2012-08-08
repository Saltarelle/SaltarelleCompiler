using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Utils;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.GotoRewrite {
	internal class NeedsRewriteVisitor : RewriterVisitorBase<object> {
		bool _result;

		private NeedsRewriteVisitor() {
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			return statement;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			return expression;
		}

		public override JsStatement VisitLabelledStatement(JsLabelledStatement statement, object data) {
			_result = true;
			return statement;
		}

		public override JsStatement VisitYieldStatement(JsYieldStatement statement, object data) {
			_result = true;
			return statement;
		}

		public static bool Analyze(JsStatement statement) {
			var obj = new NeedsRewriteVisitor();
			obj.VisitStatement(statement, null);
			return obj._result;
		}
	}

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
			Visit(statement.Clauses, null);
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

	internal class SingleStateMachineRewriter {
		[DebuggerDisplay("{DebugToString()}")]
		class StackEntry {
			public JsBlockStatement Block { get; private set; }
			public int Index { get; private set; }
			public bool AfterForInitializer { get; private set; }

			public StackEntry(JsBlockStatement block, int index, bool afterForInitializer = false) {
				Block = block;
				Index = index;
				AfterForInitializer = afterForInitializer;
			}

			public string DebugToString() {
				return new JsBlockStatement(Block.Statements.Skip(Index)).DebugToString();
			}
		}

		class RemainingBlock {
			public ImmutableStack<StackEntry> Stack { get; private set; }
			public ImmutableStack<Tuple<string, int>> BreakStack { get; private set; }
			public ImmutableStack<Tuple<string, int>> ContinueStack { get; private set; }
			public int StateValue { get; private set; }
			public int ReturnState { get; private set; }

			public RemainingBlock(ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, int>> breakStack, ImmutableStack<Tuple<string, int>> continueStack, int stateValue, int returnState) {
				Stack = stack;
				BreakStack = breakStack;
				ContinueStack = continueStack;
				StateValue = stateValue;
				ReturnState = returnState;
			}
		}

		class Section {
			public int StateValue { get; private set; }
			public IList<JsStatement> Statements { get; private set; }

			public Section(int stateValue, IEnumerable<JsStatement> statements) {
				this.StateValue = stateValue;
				this.Statements = new List<JsStatement>(statements);
			}

			public void Freeze() {
				if (Statements is List<JsStatement>)
					Statements = ((List<JsStatement>)Statements).AsReadOnly();
			}
		}

		int _nextStateIndex;
		string _stateVariableName;
		string _loopLabel;
		Queue<RemainingBlock> _remainingBlocks = new Queue<RemainingBlock>();
		HashSet<int> _processedStates = new HashSet<int>();
		Dictionary<string, int> _labelStateValues = new Dictionary<string, int>();

		readonly Func<JsExpression, bool> _isExpressionComplexEnoughForATemporaryVariable;
		readonly Func<string> _allocateTempVariable;
		readonly Func<JsExpression, JsExpression> _makeSetCurrent;

		public SingleStateMachineRewriter(Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<JsExpression, JsExpression> makeSetCurrent) {
			_isExpressionComplexEnoughForATemporaryVariable = isExpressionComplexEnoughForATemporaryVariable;
			_allocateTempVariable = allocateTempVariable;
			_makeSetCurrent = makeSetCurrent;
		}

		private int GetNewStateValue() {
			return _nextStateIndex++;
		}

		private string GetLabelForState(int state) {
			return _labelStateValues.SingleOrDefault(x => x.Value == state).Key;
		}

		private int GetStateForLabel(string labelName) {
			int result;
			if (_labelStateValues.TryGetValue(labelName, out result))
				return result;
			_labelStateValues[labelName] = result = GetNewStateValue();
			return result;
		}

		private bool IsNextStatementReachable(JsStatement current) {
			while (current is JsBlockStatement) {
				var block = (JsBlockStatement)current;
				if (block.Statements.Count == 0)
					return true;
				current = block.Statements[block.Statements.Count - 1];
			}
			var ifst = current as JsIfStatement;
			if (ifst != null) {
				return ifst.Else == null || ifst.Then.Statements.Count == 0 || ifst.Else.Statements.Count == 0 || IsNextStatementReachable(ifst.Then.Statements[ifst.Then.Statements.Count - 1]) || IsNextStatementReachable(ifst.Else.Statements[ifst.Else.Statements.Count - 1]);
			}

			return !(current is JsReturnStatement || current is JsThrowStatement || current is JsBreakStatement || current is JsContinueStatement);
		}

		private void Enqueue(ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, int>> breakStack, ImmutableStack<Tuple<string, int>> continueStack, int stateValue, int returnState) {
			if (_processedStates.Contains(stateValue))
				throw new InvalidOperationException("Duplicate enqueueing of " + stateValue);
			_processedStates.Add(stateValue);
			if (stack.IsEmpty)
				throw new InvalidOperationException("Empty stack for state " + stateValue);
			_remainingBlocks.Enqueue(new RemainingBlock(stack, breakStack, continueStack, stateValue, returnState));
		}

		private ImmutableStack<StackEntry> PushFollowing(ImmutableStack<StackEntry> stack, StackEntry location) {
			return location.Index < location.Block.Statements.Count - 1 ? stack.Push(new StackEntry(location.Block, location.Index + 1)) : stack;
		}

		private void SetNextState(IList<JsStatement> statements, int stateIndex) {
			statements.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(_stateVariableName), JsExpression.Number(stateIndex))));
		}

		private void GotoState(IList<JsStatement> statements, int stateIndex) {
			if (stateIndex == -1) {
				statements.Add(new JsBreakStatement(_loopLabel));
			}
			else {
				SetNextState(statements, stateIndex);
				statements.Add(new JsContinueStatement(_loopLabel));
			}
		}

		private JsBlockStatement GotoState(int stateIndex) {
			var list = new List<JsStatement>();
			if (stateIndex == -1) {
				list.Add(new JsBreakStatement(_loopLabel));
			}
			else {
				SetNextState(list, stateIndex);
				list.Add(new JsContinueStatement(_loopLabel));
			}
			return new JsBlockStatement(list);
		}

		public JsBlockStatement Process(JsBlockStatement statement, string stateVariableName, string loopLabel, bool isIteratorBlock) {
			_nextStateIndex = 0;
			_stateVariableName = stateVariableName;
			_loopLabel = loopLabel;
			_processedStates.Clear();
			_labelStateValues.Clear();
			_remainingBlocks = new Queue<RemainingBlock>();
			_remainingBlocks.Enqueue(new RemainingBlock(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(statement, 0)), ImmutableStack<Tuple<string, int>>.Empty, ImmutableStack<Tuple<string, int>>.Empty, GetNewStateValue(), -1));

			var sections = new List<Section>();

			while (_remainingBlocks.Count > 0) {
				var current = _remainingBlocks.Dequeue();
				var list = Handle(current.Stack, current.BreakStack, current.ContinueStack, current.StateValue, current.ReturnState);
				if (isIteratorBlock)
					list.Insert(0, new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(stateVariableName), JsExpression.Number(-1))));
				sections.Add(new Section(current.StateValue, list));
			}

			var body = new List<JsStatement> {
			    new JsVariableDeclarationStatement(stateVariableName, JsExpression.Number(0)),
			    new JsLabelledStatement(loopLabel,
			        new JsForStatement(new JsEmptyStatement(), null, null,
			            new JsSwitchStatement(JsExpression.Identifier(stateVariableName),
			                sections.Select(b =>
			                    new JsSwitchSection(
			                        new[] { JsExpression.Number(b.StateValue) },
			                        new JsBlockStatement(b.Statements))))))
			};
			if (isIteratorBlock)
				body.Add(new JsReturnStatement(JsExpression.False));

			return new JsBlockStatement(body);
		}

		private List<JsStatement> Handle(ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, int>> breakStack, ImmutableStack<Tuple<string, int>> continueStack, int currentState, int returnState) {
			var currentBlock = new List<JsStatement>();
			while (!stack.IsEmpty) {
				var tos = stack.Peek();
				stack = stack.Pop();

				var stmt = tos.Block.Statements[tos.Index];
				var lbl = stmt as JsLabelledStatement;
				if (lbl != null) {
					if (_processedStates.Contains(GetStateForLabel(lbl.Label))) {
						// First statement in the new block
						stmt = lbl.Statement;
					}
					else {
						// A label that terminates the current block.
						Enqueue(stack.Push(new StackEntry(tos.Block, tos.Index)), breakStack, continueStack, GetStateForLabel(lbl.Label), returnState);
						if (currentBlock.Count == 0 || IsNextStatementReachable(currentBlock[currentBlock.Count - 1]))
							GotoState(currentBlock, GetStateForLabel(lbl.Label));
						return currentBlock;
					}
				}

				if (stmt is JsYieldStatement) {
					if (!HandleYieldStatement((JsYieldStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock))
						return currentBlock;
				}
				else if (NeedsRewriteVisitor.Analyze(stmt)) {
					if (stmt is JsBlockStatement) {
						stack = PushFollowing(stack, tos).Push(new StackEntry((JsBlockStatement)stmt, 0));
					}
					else {
						if (stmt is JsIfStatement) {
							if (!HandleIfStatement((JsIfStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsDoWhileStatement) {
							if (!HandleDoWhileStatement((JsDoWhileStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsWhileStatement) {
							if (!HandleWhileStatement((JsWhileStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsForStatement) {
							if (!HandleForStatement((JsForStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsSwitchStatement) {
							if (!HandleSwitchStatement((JsSwitchStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock))
								return currentBlock;
						}
						else {
							throw new NotSupportedException("Statement " + stmt + " cannot contain labels.");
						}

						stack = PushFollowing(stack, tos);
					}
				}
				else {
#warning Need to handle break/continue even inside statements that do not themselves need processing. Visitor?
					if (stmt is JsBreakStatement) {
						var brkStmt = (JsBreakStatement)stmt;
						if (brkStmt.TargetLabel == null) {
							GotoState(currentBlock, breakStack.Peek().Item2);
						}
						else {
							GotoState(currentBlock, breakStack.Single(x => x.Item1 == brkStmt.TargetLabel).Item2);
						}
					}
					else if (stmt is JsContinueStatement) {
						var contStmt = (JsContinueStatement)stmt;
						if (contStmt.TargetLabel == null) {
							GotoState(currentBlock, continueStack.Peek().Item2);
						}
						else {
							GotoState(currentBlock, continueStack.Single(x => x.Item1 == contStmt.TargetLabel).Item2);
						}
					}
					else if (stmt is JsGotoStatement) {
						GotoState(currentBlock, GetStateForLabel(((JsGotoStatement)stmt).TargetLabel));
					}
					else {
						currentBlock.Add(stmt);	// No rewrites necessary in this statement.
					}
					stack = PushFollowing(stack, tos);
				}
			}
			if (currentBlock.Count == 0 || IsNextStatementReachable(currentBlock[currentBlock.Count - 1]))
				GotoState(currentBlock, returnState);

			return currentBlock;
		}

		private Tuple<int, bool> GetStateAfterStatement(StackEntry location, ImmutableStack<StackEntry> stack, int returnState) {
			JsStatement next;
			if (location.Index < location.Block.Statements.Count - 1) {
				next = location.Block.Statements[location.Index + 1];
			}
			else if (!stack.IsEmpty) {
				var tos = stack.Peek();
				next = tos.Block.Statements[tos.Index];
			}
			else
				next = null;

			if (next is JsLabelledStatement) {
				return Tuple.Create(GetStateForLabel((next as JsLabelledStatement).Label), false);
			}
			else if (next != null) {
				return Tuple.Create(GetNewStateValue(), true);
			}
			else {
				return Tuple.Create(returnState, false);
			}
		}

		private bool HandleYieldStatement(JsYieldStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, int>> breakStack, ImmutableStack<Tuple<string, int>> continueStack, int currentState, int returnState, IList<JsStatement> currentBlock) {
			var stateAfter = GetStateAfterStatement(location, stack, returnState);

			if (stmt.Value != null) {
				currentBlock.Add(new JsExpressionStatement(_makeSetCurrent(stmt.Value)));
				SetNextState(currentBlock, stateAfter.Item1);
				currentBlock.Add(new JsReturnStatement(JsExpression.True));
			}
			else {
				currentBlock.Add(new JsReturnStatement(JsExpression.False));
			}

			if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
			}

			return false;
		}

		private bool HandleIfStatement(JsIfStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, int>> breakStack, ImmutableStack<Tuple<string, int>> continueStack, int currentState, int returnState, IList<JsStatement> currentBlock) {
			var stateAfter = GetStateAfterStatement(location, stack, returnState);

			var thenPart = Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Then, 0)), breakStack, continueStack, currentState, stateAfter.Item1);
			var elsePart = stmt.Else != null ? Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Else, 0)), breakStack, continueStack, currentState, stateAfter.Item1) : null;

			currentBlock.Add(new JsIfStatement(stmt.Test, new JsBlockStatement(thenPart), elsePart != null ? new JsBlockStatement(elsePart) : null));
			if (elsePart == null)
				GotoState(currentBlock, stateAfter.Item1);

			if (stateAfter.Item2) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
				return false;
			}

			return true;
		}

		private bool HandleDoWhileStatement(JsDoWhileStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, int>> breakStack, ImmutableStack<Tuple<string, int>> continueStack, int currentState, int returnState, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0) {
				// We have to create a new block for the statement.
				int topOfLoopState = GetNewStateValue();
				Enqueue(stack.Push(location), breakStack, continueStack, topOfLoopState, returnState);
				GotoState(currentBlock, topOfLoopState);
				return false;
			}
			else {
				int beforeConditionState = GetNewStateValue();
				Tuple<int, bool> afterLoopState;
				string currentName = GetLabelForState(currentState);
				if (new ContainsBreakVisitor().Analyze(stmt.Body, currentName)) {
					afterLoopState = GetStateAfterStatement(location, stack, returnState);
					breakStack = breakStack.Push(Tuple.Create(currentName, afterLoopState.Item1));
				}
				else {
					afterLoopState = Tuple.Create(returnState, false);
				}

				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Body, 0)), breakStack, continueStack.Push(Tuple.Create(GetLabelForState(currentState), beforeConditionState)), currentState, beforeConditionState));

				if (afterLoopState.Item2) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, afterLoopState.Item1, returnState);
					Enqueue(stack.Push(new StackEntry(new JsBlockStatement(new JsIfStatement(stmt.Condition, GotoState(currentState), null)), 0)), breakStack, continueStack, beforeConditionState, afterLoopState.Item1);
				}
				else {
					Enqueue(PushFollowing(stack, location).Push(new StackEntry(new JsBlockStatement(new JsIfStatement(stmt.Condition, GotoState(currentState), null)), 0)), breakStack, continueStack, beforeConditionState, returnState);
				}

				return false;
			}
		}

		private bool HandleWhileStatement(JsWhileStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, int>> breakStack, ImmutableStack<Tuple<string, int>> continueStack, int currentState, int returnState, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0) {
				// We have to create a new block for the statement.
				int topOfLoopState = GetNewStateValue();
				Enqueue(stack.Push(location), breakStack, continueStack, topOfLoopState, returnState);
				GotoState(currentBlock, topOfLoopState);
				return false;
			}
			else {
				var afterLoopState = GetStateAfterStatement(location, stack, returnState);

				currentBlock.Add(new JsIfStatement(JsExpression.LogicalNot(stmt.Condition), GotoState(afterLoopState.Item1), null));
				var currentName = GetLabelForState(currentState);
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Body, 0)), breakStack.Push(Tuple.Create(currentName, afterLoopState.Item1)), continueStack.Push(Tuple.Create(currentName, currentState)), currentState, currentState));

				if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, afterLoopState.Item1, returnState);
				}

				return false;
			}
		}

		private bool HandleForStatement(JsForStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, int>> breakStack, ImmutableStack<Tuple<string, int>> continueStack, int currentState, int returnState, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0 || (!(stmt.InitStatement is JsEmptyStatement) && !location.AfterForInitializer)) {
				// We have to create a new block for the statement.
				int topOfLoopState = GetNewStateValue();
				Enqueue(stack.Push(new StackEntry(location.Block, location.Index, true)), breakStack, continueStack, topOfLoopState, returnState);
				if (!(stmt.InitStatement is JsEmptyStatement))
					currentBlock.Add(stmt.InitStatement);
				GotoState(currentBlock, topOfLoopState);
				return false;
			}
			else {
				var iteratorState = (stmt.IteratorExpression != null ? GetNewStateValue() : currentState);
				var afterLoopState = GetStateAfterStatement(location, stack, returnState);

				if (stmt.ConditionExpression != null)
					currentBlock.Add(new JsIfStatement(JsExpression.LogicalNot(stmt.ConditionExpression), GotoState(afterLoopState.Item1), null));
				string currentName = GetLabelForState(currentState);
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Body, 0)), breakStack.Push(Tuple.Create(currentName, afterLoopState.Item1)), continueStack.Push(Tuple.Create(currentName, iteratorState)), currentState, iteratorState));

				if (stmt.IteratorExpression != null) {
					Enqueue(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(JsBlockStatement.MakeBlock(new JsExpressionStatement(stmt.IteratorExpression)), 0)), breakStack, continueStack, iteratorState, currentState);
				}

				if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, afterLoopState.Item1, returnState);
				}

				return false;
			}
		}

		private bool HandleSwitchStatement(JsSwitchStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, int>> breakStack, ImmutableStack<Tuple<string, int>> continueStack, int currentState, int returnState, IList<JsStatement> currentBlock) {
			var stateAfter = GetStateAfterStatement(location, stack, returnState);
			JsExpression expression = stmt.Expression;
			if (_isExpressionComplexEnoughForATemporaryVariable(expression)) {
				string newName = _allocateTempVariable();
				currentBlock.Add(new JsVariableDeclarationStatement(newName, expression));
				expression = JsExpression.Identifier(newName);
			}

			var clauses = new List<Tuple<JsExpression, JsBlockStatement>>();
			JsStatement defaultClause = null;
			int? currentFallthroughState = null;
			for (int i = 0; i < stmt.Clauses.Count; i++) {
				var clause = stmt.Clauses[i];

				var origBody = new List<JsStatement>();
				origBody.AddRange(clause.Body.Statements);

				int? nextFallthroughState;

				if (origBody.Count > 0 && origBody[origBody.Count - 1] is JsBreakStatement) {	// TODO: Also check if it has a label that causes it to reference something else (but we don't generate those kinds of labels, at least not currently).
					// Remove break statements that come last in the clause - they are unnecessary since we use if/else if/else
					origBody.RemoveAt(origBody.Count - 1);
					nextFallthroughState = null;
				}
				else if (i < stmt.Clauses.Count - 1 && (origBody.Count == 0 || IsNextStatementReachable(origBody[origBody.Count - 1]))) {
					// Fallthrough
					var nextBody = stmt.Clauses[i + 1].Body.Statements;
					if (nextBody.Count > 0 && nextBody[0] is JsLabelledStatement)
						nextFallthroughState = GetStateForLabel(((JsLabelledStatement)nextBody[0]).Label);
					else
						nextFallthroughState = GetNewStateValue();
				}
				else {
					nextFallthroughState = null;
				}

				breakStack = breakStack.Push(Tuple.Create(GetLabelForState(currentState), stateAfter.Item1));

				IList<JsStatement> body;
				if (currentFallthroughState != null) {
					body = new List<JsStatement>();
					GotoState(body, currentFallthroughState.Value);
					Enqueue(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(new JsBlockStatement(origBody), 0)), breakStack, continueStack, currentFallthroughState.Value, stateAfter.Item1);
				}
				else {
					body = Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(new JsBlockStatement(origBody), 0)), breakStack, continueStack, currentState, nextFallthroughState ?? stateAfter.Item1);
				}

				if (clause.Values.Any(v => v == null)) {
					defaultClause = new JsBlockStatement(body);
				}
				else {
					JsExpression test = clause.Values.Select(v => JsExpression.Same(expression, v)).Aggregate((o, e) => o != null ? JsExpression.LogicalOr(o, e) : e);
					clauses.Add(Tuple.Create(test, new JsBlockStatement(body)));
				}

				currentFallthroughState = nextFallthroughState;
			}
			clauses.Reverse();

			currentBlock.Add(clauses.Where(c => c.Item1 != null).Aggregate(defaultClause, (o, n) => new JsIfStatement(n.Item1, n.Item2, o)));
			GotoState(currentBlock, stateAfter.Item1);

			if (stateAfter.Item2) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
				return false;
			}

			return true;
		}
	}

	public class StateMachineRewriter : RewriterVisitorBase<object> {
		private readonly Func<JsExpression, bool> _isExpressionComplexEnoughForATemporaryVariable;
		private readonly Func<string> _allocateTempVariable;
		private readonly Func<JsExpression, JsExpression> _makeSetCurrent;
		private readonly bool _isIteratorBlock;

		private int _currentLoopNameIndex;

		private StateMachineRewriter(Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<JsExpression, JsExpression> makeSetCurrent, bool isIteratorBlock) {
			_isExpressionComplexEnoughForATemporaryVariable = isExpressionComplexEnoughForATemporaryVariable;
			_allocateTempVariable = allocateTempVariable;
			_makeSetCurrent = makeSetCurrent;
			_isIteratorBlock = isIteratorBlock;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			var body = DoRewrite((JsBlockStatement)VisitBlockStatement(expression.Body, data));
	        return ReferenceEquals(body, expression.Body) ? expression : JsExpression.FunctionDefinition(expression.ParameterNames, body, expression.Name);
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			var body = DoRewrite((JsBlockStatement)VisitBlockStatement(statement.Body, data));
	        return ReferenceEquals(body, statement.Body) ? statement : new JsFunctionStatement(statement.Name, statement.ParameterNames, body);
		}

		private JsBlockStatement DoRewrite(JsBlockStatement block) {
			if (!NeedsRewriteVisitor.Analyze(block))
				return block;

			var stateVar  = _allocateTempVariable();
			var loopLabel = "$loop" + (++_currentLoopNameIndex).ToString(CultureInfo.InvariantCulture);
			return new SingleStateMachineRewriter(_isExpressionComplexEnoughForATemporaryVariable, _allocateTempVariable, _makeSetCurrent).Process(block, stateVar, loopLabel, _isIteratorBlock);
		}

		public static JsBlockStatement Rewrite(JsBlockStatement block, Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<JsExpression, JsExpression> makeSetCurrent, bool isIteratorBlock) {
			var obj = new StateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, makeSetCurrent, isIteratorBlock);
			return obj.DoRewrite((JsBlockStatement)obj.VisitBlockStatement(block, null));
		}
	}
}
