using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.Utils;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.GotoRewrite
{
	internal class SingleStateMachineRewriter {
		int _nextStateIndex;
		string _stateVariableName;
		string _currentLoopLabel;
		bool _isIteratorBlock;
		Queue<RemainingBlock> _remainingBlocks = new Queue<RemainingBlock>();
		HashSet<State> _processedStates = new HashSet<State>();
		Dictionary<string, State> _labelStates = new Dictionary<string, State>();
		State? _exitState;

		readonly Func<JsExpression, bool> _isExpressionComplexEnoughForATemporaryVariable;
		readonly Func<string> _allocateTempVariable;
		readonly Func<string> _allocateLoopLabel;
		readonly Func<JsBlockStatement, string> _allocateFinallyHandler;
		readonly Func<JsExpression, JsExpression> _makeSetCurrent;

		public SingleStateMachineRewriter(Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel, Func<JsBlockStatement, string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent) {
			_isExpressionComplexEnoughForATemporaryVariable = isExpressionComplexEnoughForATemporaryVariable;
			_allocateTempVariable = allocateTempVariable;
			_allocateLoopLabel = allocateLoopLabel;
			_allocateFinallyHandler = allocateFinallyHandler;
			_makeSetCurrent = makeSetCurrent;
		}

		private State CreateNewStateValue(ImmutableStack<Tuple<int, string>> finallyStack, string finallyHandlerToPush = null) {
			int value = _nextStateIndex++;
			finallyStack = finallyHandlerToPush != null ? finallyStack.Push(Tuple.Create(value, finallyHandlerToPush)) : finallyStack;
			return new State(_currentLoopLabel, value, finallyStack);
		}

		private string GetLabelForState(State state) {
			return _labelStates.SingleOrDefault(x => x.Value.Equals(state)).Key;
		}

		private State GetOrCreateStateForLabel(string labelName, ImmutableStack<Tuple<int, string>> finallyStack) {
			State result;
			if (_labelStates.TryGetValue(labelName, out result))
				return result;
			_labelStates[labelName] = result = CreateNewStateValue(finallyStack);
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

			return !(current is JsReturnStatement || current is JsGotoStatement || current is JsGotoStateStatement || current is JsThrowStatement || current is JsBreakStatement || current is JsContinueStatement);
		}

		private void Enqueue(ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State stateValue, State returnState) {
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

		internal static void SetNextState(IList<JsStatement> statements, string stateVariableName, int stateValue) {
			statements.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(stateVariableName), JsExpression.Number(stateValue))));
		}

		public JsBlockStatement Process(JsBlockStatement statement, bool isIteratorBlock) {
			_stateVariableName = _allocateTempVariable();
			_nextStateIndex = 0;
			_isIteratorBlock = isIteratorBlock;
			_processedStates.Clear();
			_labelStates.Clear();
			_remainingBlocks = new Queue<RemainingBlock>();
			_exitState = null;
			var body = ProcessInner(statement, ImmutableStack<Tuple<string, State>>.Empty, ImmutableStack<Tuple<string, State>>.Empty, ImmutableStack<Tuple<int, string>>.Empty);
			body[0] = new JsVariableDeclarationStatement(_stateVariableName, JsExpression.Number(0));	// Replace the assignment statement with a variable declaration.
			if (_isIteratorBlock)
				body.Add(new JsReturnStatement(JsExpression.False));
			return new FinalizerRewriter(_stateVariableName, _labelStates).Process(new JsBlockStatement(body));
		}

		private IList<JsStatement> ProcessInner(JsBlockStatement statement, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack) {
			var oldLoopLabel = _currentLoopLabel;
			var oldRemainingBlocks = _remainingBlocks;
			try {
				_currentLoopLabel = _allocateLoopLabel();
				_remainingBlocks = new Queue<RemainingBlock>();
				_remainingBlocks.Enqueue(new RemainingBlock(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(statement, 0)), breakStack, continueStack, CreateNewStateValue(finallyStack), new State(_currentLoopLabel, -1, finallyStack)));
				if (_exitState == null)
					_exitState = new State(_currentLoopLabel, -1, ImmutableStack<Tuple<int, string>>.Empty);
	
				var sections = new List<Section>();
	
				while (_remainingBlocks.Count > 0) {
					var current = _remainingBlocks.Dequeue();
					var list = Handle(current.Stack, current.BreakStack, current.ContinueStack, current.StateValue, current.ReturnState);
					// Merge all top-level blocks that should be merged with their parents.
					list = list.SelectMany(stmt => (stmt is JsBlockStatement && ((JsBlockStatement)stmt).MergeWithParent) ? ((JsBlockStatement)stmt).Statements : (IList<JsStatement>)new[] { stmt }).ToList();
					if (_isIteratorBlock)
						list.Insert(0, new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(_stateVariableName), JsExpression.Number(current.StateValue.FinallyStack.IsEmpty ? -1 : current.StateValue.FinallyStack.Peek().Item1))));
					sections.Add(new Section(current.StateValue, list));
				}
	
				var body = new List<JsStatement> {
				                                 	new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(_stateVariableName), JsExpression.Number(sections[0].State.StateValue))),
				                                 	new JsLabelledStatement(_currentLoopLabel,
				                                 	                        new JsForStatement(new JsEmptyStatement(), null, null,
				                                 	                                           new JsSwitchStatement(JsExpression.Identifier(_stateVariableName),
				                                 	                                                                 sections.Select(b =>
				                                 	                                                                                 new JsSwitchSection(
				                                 	                                                                                 	new[] { JsExpression.Number(b.State.StateValue) },
				                                 	                                                                                 	new JsBlockStatement(b.Statements))))))
				                                 };
				return body;
			}
			finally {
				_currentLoopLabel = oldLoopLabel;
				_remainingBlocks = oldRemainingBlocks;
			}
		}

		private List<JsStatement> Handle(ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState) {
			var currentBlock = new List<JsStatement>();
			while (!stack.IsEmpty) {
				var tos = stack.Peek();
				stack = stack.Pop();

				var stmt = tos.Block.Statements[tos.Index];
				var lbl = stmt as JsLabelledStatement;
				if (lbl != null) {
					if (_processedStates.Contains(GetOrCreateStateForLabel(lbl.Label, currentState.FinallyStack))) {
						// First statement in the new block
						stmt = lbl.Statement;
					}
					else {
						// A label that terminates the current block.
						Enqueue(stack.Push(new StackEntry(tos.Block, tos.Index)), breakStack, continueStack, GetOrCreateStateForLabel(lbl.Label, currentState.FinallyStack), returnState);
						if (currentBlock.Count == 0 || IsNextStatementReachable(currentBlock[currentBlock.Count - 1]))
							currentBlock.Add(new JsGotoStateStatement(GetOrCreateStateForLabel(lbl.Label, currentState.FinallyStack), currentState));
						return currentBlock;
					}
				}

				if (stmt is JsYieldStatement) {
					var ystmt = (JsYieldStatement)stmt;
					if (ystmt.Value != null) {
						if (!HandleYieldReturnStatement(ystmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock))
							return currentBlock;
					}
					else {
						currentBlock.AddRange(new NestedJumpStatementRewriter(breakStack, continueStack, currentState, _exitState.Value).Process(stmt));
						stack = PushFollowing(stack, tos);
					}
				}
				else if (stmt is JsTryStatement) {
					if (!HandleTryStatement((JsTryStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock))
						return currentBlock;
					stack = PushFollowing(stack, tos);
				}
				else if (FindInterestingConstructsVisitor.Analyze(stmt, InterestingConstruct.YieldReturn | InterestingConstruct.Label)) {
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
					currentBlock.AddRange(new NestedJumpStatementRewriter(breakStack, continueStack, currentState, _exitState.Value).Process(stmt));
					stack = PushFollowing(stack, tos);
				}
			}
			if (currentBlock.Count == 0 || IsNextStatementReachable(currentBlock[currentBlock.Count - 1]))
				currentBlock.Add(new JsGotoStateStatement(returnState, currentState));

			return currentBlock;
		}

		private Tuple<State, bool> GetStateAfterStatement(StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<int, string>> finallyStack, State returnState) {
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
				return Tuple.Create(GetOrCreateStateForLabel((next as JsLabelledStatement).Label, finallyStack), false);
			}
			else if (next != null) {
				return Tuple.Create(CreateNewStateValue(finallyStack), true);
			}
			else {
				return Tuple.Create(returnState, false);
			}
		}

		private bool HandleYieldReturnStatement(JsYieldStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			var stateAfter = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState);

			currentBlock.Add(new JsExpressionStatement(_makeSetCurrent(stmt.Value)));
			SetNextState(currentBlock, _stateVariableName, stateAfter.Item1.StateValue);
			currentBlock.Add(new JsReturnStatement(JsExpression.True));

			if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
			}

			return false;
		}

		private bool HandleTryStatement(JsTryStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			if (FindInterestingConstructsVisitor.Analyze(stmt.GuardedStatement, InterestingConstruct.YieldReturn) || (stmt.Finally != null && stmt.Catch == null && !currentState.FinallyStack.IsEmpty)) {
				if (stmt.Catch != null)
					throw new InvalidOperationException("Cannot yield return from try with catch");
				string handlerName = _allocateFinallyHandler(FindInterestingConstructsVisitor.Analyze(stmt.Finally, InterestingConstruct.Label) ? new FinalizerRewriter(_stateVariableName, _labelStates).Process(new JsBlockStatement(ProcessInner(stmt.Finally, breakStack, continueStack, currentState.FinallyStack))) : stmt.Finally);
				var stateAfter = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState);
				var innerState = CreateNewStateValue(currentState.FinallyStack, handlerName);
				var stateBeforeFinally = CreateNewStateValue(innerState.FinallyStack);
				SetNextState(currentBlock, _stateVariableName, innerState.StateValue);
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.GuardedStatement, 0)), breakStack, continueStack, innerState, stateBeforeFinally));

				Enqueue(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(new JsBlockStatement(new JsBlockStatement(new JsStatement[0], true)), 0)), breakStack, continueStack, stateBeforeFinally, stateAfter.Item1);
				if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
				}
				return false;
			}
			else if (stmt.Finally != null && !currentState.FinallyStack.IsEmpty) {
				// This is necessary to special-case in order to ensure that the inner finally block is executed before all outer ones.
				return HandleTryStatement(new JsTryStatement(new JsTryStatement(stmt.GuardedStatement, stmt.Catch, null), null, stmt.Finally), location, stack, breakStack, continueStack, currentState, returnState, currentBlock);
			}
			else {
				var rewriter = new NestedJumpStatementRewriter(breakStack, continueStack, currentState, _exitState.Value);
				var guarded = FindInterestingConstructsVisitor.Analyze(stmt.GuardedStatement, InterestingConstruct.Label)
				              ? new JsBlockStatement(ProcessInner(stmt.GuardedStatement, breakStack, continueStack, currentState.FinallyStack))
				              : rewriter.Process(stmt.GuardedStatement);

				JsCatchClause @catch;
				if (stmt.Catch != null) {
					if (FindInterestingConstructsVisitor.Analyze(stmt.Catch.Body, InterestingConstruct.Label)) {
						@catch = new JsCatchClause(stmt.Catch.Identifier, new JsBlockStatement(ProcessInner(stmt.Catch.Body, breakStack, continueStack, currentState.FinallyStack)));
					}
					else {
						var body = rewriter.Process(stmt.Catch.Body);
						@catch = ReferenceEquals(body, stmt.Catch.Body) ? stmt.Catch : new JsCatchClause(stmt.Catch.Identifier, body);
					}
				}
				else
					@catch = null;

				JsBlockStatement @finally;
				if (stmt.Finally != null) {
					if (FindInterestingConstructsVisitor.Analyze(stmt.Finally, InterestingConstruct.Label))
						@finally = new JsBlockStatement(ProcessInner(stmt.Finally, breakStack, continueStack, currentState.FinallyStack));
					else
						@finally = rewriter.Process(stmt.Finally);
				}
				else
					@finally = null;

				currentBlock.Add(new JsTryStatement(guarded, @catch, @finally));
				return true;
			}
		}

		private bool HandleIfStatement(JsIfStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			var stateAfter = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState);

			var thenPart = Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Then, 0)), breakStack, continueStack, currentState, stateAfter.Item1);
			var elsePart = stmt.Else != null ? Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Else, 0)), breakStack, continueStack, currentState, stateAfter.Item1) : null;

			currentBlock.Add(new JsIfStatement(stmt.Test, new JsBlockStatement(thenPart), elsePart != null ? new JsBlockStatement(elsePart) : null));
			if (elsePart == null)
				currentBlock.Add(new JsGotoStateStatement(stateAfter.Item1, currentState));

			if (stateAfter.Item2) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
				return false;
			}

			return true;
		}

		private bool HandleDoWhileStatement(JsDoWhileStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0) {
				// We have to create a new block for the statement.
				var topOfLoopState = CreateNewStateValue(currentState.FinallyStack);
				Enqueue(stack.Push(location), breakStack, continueStack, topOfLoopState, returnState);
				currentBlock.Add(new JsGotoStateStatement(topOfLoopState, currentState));
				return false;
			}
			else {
				var beforeConditionState = CreateNewStateValue(currentState.FinallyStack);
				Tuple<State, bool> afterLoopState;
				string currentName = GetLabelForState(currentState);
				if (new ContainsBreakVisitor().Analyze(stmt.Body, currentName)) {
					afterLoopState = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState);
					breakStack = breakStack.Push(Tuple.Create(currentName, afterLoopState.Item1));
				}
				else {
					afterLoopState = Tuple.Create(returnState, false);
				}

				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Body, 0)), breakStack, continueStack.Push(Tuple.Create(GetLabelForState(currentState), beforeConditionState)), currentState, beforeConditionState));

				if (afterLoopState.Item2) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, afterLoopState.Item1, returnState);
					Enqueue(stack.Push(new StackEntry(new JsBlockStatement(new JsIfStatement(stmt.Condition, new JsGotoStateStatement(currentState, currentState), null)), 0)), breakStack, continueStack, beforeConditionState, afterLoopState.Item1);
				}
				else {
					Enqueue(PushFollowing(stack, location).Push(new StackEntry(new JsBlockStatement(new JsIfStatement(stmt.Condition, new JsGotoStateStatement(currentState, currentState), null)), 0)), breakStack, continueStack, beforeConditionState, returnState);
				}

				return false;
			}
		}

		private bool HandleWhileStatement(JsWhileStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0) {
				// We have to create a new block for the statement.
				var topOfLoopState = CreateNewStateValue(currentState.FinallyStack);
				Enqueue(stack.Push(location), breakStack, continueStack, topOfLoopState, returnState);
				currentBlock.Add(new JsGotoStateStatement(topOfLoopState, currentState));
				return false;
			}
			else {
				var afterLoopState = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState);

				currentBlock.Add(new JsIfStatement(JsExpression.LogicalNot(stmt.Condition), new JsGotoStateStatement(afterLoopState.Item1, currentState), null));
				var currentName = GetLabelForState(currentState);
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Body, 0)), breakStack.Push(Tuple.Create(currentName, afterLoopState.Item1)), continueStack.Push(Tuple.Create(currentName, currentState)), currentState, currentState));

				if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, afterLoopState.Item1, returnState);
				}

				return false;
			}
		}

		private bool HandleForStatement(JsForStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0 || (!(stmt.InitStatement is JsEmptyStatement) && !location.AfterForInitializer)) {
				// We have to create a new block for the statement.
				var topOfLoopState = CreateNewStateValue(currentState.FinallyStack);
				Enqueue(stack.Push(new StackEntry(location.Block, location.Index, true)), breakStack, continueStack, topOfLoopState, returnState);
				if (!(stmt.InitStatement is JsEmptyStatement))
					currentBlock.Add(stmt.InitStatement);
				currentBlock.Add(new JsGotoStateStatement(topOfLoopState, currentState));
				return false;
			}
			else {
				var iteratorState = (stmt.IteratorExpression != null ? CreateNewStateValue(currentState.FinallyStack) : currentState);
				var afterLoopState = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState);

				if (stmt.ConditionExpression != null)
					currentBlock.Add(new JsIfStatement(JsExpression.LogicalNot(stmt.ConditionExpression), new JsGotoStateStatement(afterLoopState.Item1, currentState), null));
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

		private bool HandleSwitchStatement(JsSwitchStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			var stateAfter = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState);
			JsExpression expression = stmt.Expression;
			if (_isExpressionComplexEnoughForATemporaryVariable(expression)) {
				string newName = _allocateTempVariable();
				currentBlock.Add(new JsVariableDeclarationStatement(newName, expression));
				expression = JsExpression.Identifier(newName);
			}

			var clauses = new List<Tuple<JsExpression, JsBlockStatement>>();
			JsStatement defaultClause = null;
			State? currentFallthroughState = null;
			for (int i = 0; i < stmt.Sections.Count; i++) {
				var clause = stmt.Sections[i];

				var origBody = new List<JsStatement>();
				origBody.AddRange(clause.Body.Statements);

				State? nextFallthroughState;

				if (i < stmt.Sections.Count - 1 && (origBody.Count == 0 || IsNextStatementReachable(origBody[origBody.Count - 1]))) {
					// Fallthrough
					var nextBody = stmt.Sections[i + 1].Body.Statements;
					if (nextBody.Count > 0 && nextBody[0] is JsLabelledStatement)
						nextFallthroughState = GetOrCreateStateForLabel(((JsLabelledStatement)nextBody[0]).Label, currentState.FinallyStack);
					else
						nextFallthroughState = CreateNewStateValue(currentState.FinallyStack);
				}
				else {
					nextFallthroughState = null;
				}

				breakStack = breakStack.Push(Tuple.Create(GetLabelForState(currentState), stateAfter.Item1));

				IList<JsStatement> body;
				if (currentFallthroughState != null) {
					body = new List<JsStatement>();
					body.Add(new JsGotoStateStatement(currentFallthroughState.Value, currentState));
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
			currentBlock.Add(new JsGotoStateStatement(stateAfter.Item1, currentState));

			if (stateAfter.Item2) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
				return false;
			}

			return true;
		}
	}
}