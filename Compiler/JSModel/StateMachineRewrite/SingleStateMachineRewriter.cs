using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.Utils;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	internal class SingleStateMachineRewriter {
		int _nextStateIndex;
		string _stateVariableName;
		string _currentLoopLabel;
		bool _isIteratorBlock;
		bool _isAsync;
		Queue<RemainingBlock> _remainingBlocks;
		HashSet<State> _processedStates;
		Dictionary<string, State> _labelStates;
		Dictionary<int, List<int>> _childStates;
		List<Tuple<string, JsBlockStatement>> _finallyHandlers;
		List<State> _allStates;
		State? _exitState;

		readonly Func<JsExpression, bool> _isExpressionComplexEnoughForATemporaryVariable;
		readonly Func<string> _allocateTempVariable;
		readonly Func<string> _allocateStateVariable;
		readonly Func<string> _allocateLoopLabel;
		Func<string> _allocateFinallyHandler;
		Func<JsExpression, JsExpression> _makeSetCurrent;
		Func<JsExpression, JsExpression> _makeSetResult;
		string _stateMachineMethodName;
		string _doFinallyBlocksVariableName;
		bool _needDoFinallyBlocksVariable;
		bool _needSetDoFinally;

		public SingleStateMachineRewriter(Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateStateVariable, Func<string> allocateLoopLabel) {
			_isExpressionComplexEnoughForATemporaryVariable = isExpressionComplexEnoughForATemporaryVariable;
			_allocateTempVariable = allocateTempVariable;
			_allocateStateVariable = allocateStateVariable;
			_allocateLoopLabel = allocateLoopLabel;
		}

		private State CreateNewStateValue(ImmutableStack<Tuple<int, string>> finallyStack, string finallyHandlerToPush = null) {
			int value = _nextStateIndex++;
			finallyStack = finallyHandlerToPush != null ? finallyStack.Push(Tuple.Create(value, finallyHandlerToPush)) : finallyStack;
			var result = new State(_currentLoopLabel, value, finallyStack);
			_allStates.Add(result);
			return result;
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

		internal JsBlockStatement Process(JsBlockStatement statement) {
			_allocateFinallyHandler = null;
			_makeSetCurrent = null;
			var result = Process(statement, false, false);
			var hoistResult = VariableHoistingVisitor.Process(result);
			return new JsBlockStatement(new[] { new JsVariableDeclarationStatement(new[] { new JsVariableDeclaration(_stateVariableName, JsExpression.Number(0)) }.Concat(hoistResult.Item2.Select(v => new JsVariableDeclaration(v, null)))) }.Concat(hoistResult.Item1.Statements));
		}

		internal IteratorStateMachine ProcessIteratorBlock(JsBlockStatement statement, Func<string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent) {
			_allocateFinallyHandler = allocateFinallyHandler;
			_makeSetCurrent = makeSetCurrent;

			var result = Process(statement, isIteratorBlock: true, isAsync: false);

			var stateFinallyHandlers = _allStates.Where(s => !s.FinallyStack.IsEmpty).Select(s => Tuple.Create(s.StateValue, s.FinallyStack.Select(x => x.Item2).Reverse().ToList())).ToList();

			var hoistResult = VariableHoistingVisitor.Process(result);
			return new IteratorStateMachine(hoistResult.Item1,
			                                new[] { new JsVariableDeclaration(_stateVariableName, JsExpression.Number(0)) }.Concat(hoistResult.Item2.Select(v => new JsVariableDeclaration(v, null))),
			                                _finallyHandlers.Select(h => Tuple.Create(h.Item1, JsExpression.FunctionDefinition(new string[0], h.Item2))),
			                                stateFinallyHandlers.Count > 0 ? DisposeGenerator.GenerateDisposer(_stateVariableName, stateFinallyHandlers) : null);

		}

		internal JsBlockStatement ProcessAsyncMethod(JsBlockStatement statement, string stateMachineMethodName, string doFinallyBlocksVariableName, JsVariableDeclaration taskCompletionSource, Func<JsExpression, JsExpression> makeSetResult, Func<JsExpression, JsExpression> makeSetException, Func<JsExpression> getTask) {
			_stateMachineMethodName = stateMachineMethodName;
			_doFinallyBlocksVariableName = doFinallyBlocksVariableName;
			_makeSetResult = taskCompletionSource != null ? makeSetResult : null;
			_needDoFinallyBlocksVariable = new HasAwaitInsideTryWithFinallyVisitor().Analyze(statement);

			var result = Process(statement, isIteratorBlock: false, isAsync: true);
			var hoistResult = VariableHoistingVisitor.Process(result);

			string catchVariable = _allocateTempVariable();

			JsBlockStatement tryBody = (taskCompletionSource != null ? new JsBlockStatement(hoistResult.Item1.Statements.Concat(new[] { new JsExpressionStatement(makeSetResult(null)) })) : hoistResult.Item1);

			JsStatement body = new JsTryStatement(tryBody, new JsCatchClause(catchVariable,
			                                                                 taskCompletionSource != null
			                                                                     ? new JsBlockStatement(new JsExpressionStatement(makeSetException(JsExpression.Identifier(catchVariable))))
			                                                                     : JsBlockStatement.EmptyStatement), null);

			IEnumerable<JsVariableDeclaration> declarations = new[] { new JsVariableDeclaration(_stateVariableName, JsExpression.Number(0)) };
			if (taskCompletionSource != null)
				declarations = declarations.Concat(new[] { taskCompletionSource });
			declarations = declarations.Concat(hoistResult.Item2.Select(v => new JsVariableDeclaration(v, null)));

			if (_needDoFinallyBlocksVariable)
				body = new JsBlockStatement(new JsVariableDeclarationStatement(_doFinallyBlocksVariableName, JsExpression.True), body);

			IEnumerable<JsStatement> stmts = new JsStatement[] { new JsVariableDeclarationStatement(declarations),
			                                                     new JsVariableDeclarationStatement(stateMachineMethodName, JsExpression.FunctionDefinition(new string[0], body)),
			                                                     new JsExpressionStatement(JsExpression.Invocation(JsExpression.Identifier(stateMachineMethodName)))
			                                                   };
			if (taskCompletionSource != null)
				stmts = stmts.Concat(new[] { new JsReturnStatement(getTask()) });

			return new JsBlockStatement(stmts);
		}

		private JsBlockStatement Process(JsBlockStatement statement, bool isIteratorBlock, bool isAsync) {
			_stateVariableName = _allocateStateVariable();
			_nextStateIndex = 0;
			_isIteratorBlock = isIteratorBlock;
			_isAsync = isAsync;
			_processedStates = new HashSet<State>();
			_labelStates = new Dictionary<string, State>();
			_finallyHandlers = new List<Tuple<string, JsBlockStatement>>();
			_allStates = new List<State>();
			_remainingBlocks = new Queue<RemainingBlock>();
			_exitState = null;
			_childStates = new Dictionary<int, List<int>>();
			var body = ProcessInner(statement, ImmutableStack<Tuple<string, State>>.Empty, ImmutableStack<Tuple<string, State>>.Empty, ImmutableStack<Tuple<int, string>>.Empty, null);
			body.RemoveAt(0);	// Remove the initial assignment of the state. This will instead happen in the statement that declares the variables for this state machine.
			if (_isIteratorBlock)
				body.Add(new JsReturnStatement(JsExpression.False));
			var resultBody = new FinalizerRewriter(_stateVariableName, _labelStates).Process(new JsBlockStatement(body));
			return resultBody;
		}

		private IEnumerable<int> GetAllContainedStateValues(int state) {
			yield return state;
			List<int> childStates;
			if (_childStates.TryGetValue(state, out childStates)) {
				foreach (var c in childStates)
					yield return c;
			}
		}

		private IList<JsStatement> ProcessInner(JsBlockStatement statement, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack, int? parentState) {
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
					if ((_isIteratorBlock || _isAsync) && !(list.Count > 0 && (list[0] is JsSetNextStateStatement || list[0] is JsGotoStateStatement)))
						list.Insert(0, new JsSetNextStateStatement(current.StateValue.FinallyStack.IsEmpty ? -1 : current.StateValue.FinallyStack.Peek().Item1));
					sections.Add(new Section(current.StateValue, list));
				}

				if (parentState != null) {
					List<int> childStates;
					if (!_childStates.TryGetValue(parentState.Value, out childStates))
						_childStates[parentState.Value] = childStates = new List<int>();
					childStates.AddRange(sections.Select(s => s.State.StateValue));
				}

				var body = new List<JsStatement> {
				               new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(_stateVariableName), JsExpression.Number(sections[0].State.StateValue))),				                                 	new JsLabelledStatement(_currentLoopLabel,
				                   new JsForStatement(new JsEmptyStatement(), null, null,
				                       new JsSwitchStatement(JsExpression.Identifier(_stateVariableName),
				                           sections.Select(b => new JsSwitchSection(
				                                                    GetAllContainedStateValues(b.State.StateValue).OrderBy(v => v).Select(v => JsExpression.Number(v)),
				                                                    new JsBlockStatement(b.Statements)))
				                                   .Concat(new[] { new JsSwitchSection(new JsExpression[] { null }, new JsBreakStatement(_currentLoopLabel)) }))))
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
					}
					stack = PushFollowing(stack, tos);
				}
				else if (stmt is JsAwaitStatement) {
					if (!HandleAwaitStatement((JsAwaitStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock))
						return currentBlock;
					stack = PushFollowing(stack, tos);
				}
				else if (stmt is JsTryStatement) {
					if (!HandleTryStatement((JsTryStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock))
						return currentBlock;
					stack = PushFollowing(stack, tos);
				}
				else if (stmt is JsReturnStatement) {
					if (_makeSetResult != null)
						currentBlock.Add(new JsExpressionStatement(_makeSetResult(((JsReturnStatement)stmt).Value)));
					if (_isAsync)
						currentBlock.Add(new JsReturnStatement());
					else
						currentBlock.Add(stmt);
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
			currentBlock.Add(new JsSetNextStateStatement(stateAfter.Item1.StateValue));
			currentBlock.Add(new JsReturnStatement(JsExpression.True));

			if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
			}

			return false;
		}

		private bool HandleAwaitStatement(JsAwaitStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			var stateAfter = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState);

			currentBlock.Add(new JsSetNextStateStatement(stateAfter.Item1.StateValue));
			currentBlock.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.MemberAccess(stmt.Awaiter, stmt.OnCompletedMethodName), JsExpression.Identifier(_stateMachineMethodName))));
			if (_needDoFinallyBlocksVariable)
				currentBlock.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(_doFinallyBlocksVariableName), JsExpression.False)));
			currentBlock.Add(new JsReturnStatement());

			if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
			}

			return false;
		}

		private bool HandleTryStatement(JsTryStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			if (_isAsync && stmt.Finally != null && FindInterestingConstructsVisitor.Analyze(stmt.GuardedStatement, InterestingConstruct.Await)) {
				// Wrap the finally block inside an 'if (doFinallyBlocks) {}'
				stmt = new JsTryStatement(stmt.GuardedStatement, stmt.Catch, new JsIfStatement(JsExpression.Identifier(_doFinallyBlocksVariableName), stmt.Finally, null));
			}

			if (_isIteratorBlock && (FindInterestingConstructsVisitor.Analyze(stmt.GuardedStatement, InterestingConstruct.YieldReturn) || (stmt.Finally != null && stmt.Catch == null && !currentState.FinallyStack.IsEmpty))) {
				if (stmt.Catch != null)
					throw new InvalidOperationException("Cannot yield return from try with catch");
				string handlerName = _allocateFinallyHandler();
				_finallyHandlers.Add(Tuple.Create(handlerName, FindInterestingConstructsVisitor.Analyze(stmt.Finally, InterestingConstruct.Label) ? new FinalizerRewriter(_stateVariableName, _labelStates).Process(new JsBlockStatement(ProcessInner(stmt.Finally, breakStack, continueStack, currentState.FinallyStack, currentState.StateValue))) : stmt.Finally));
				var stateAfter = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState);
				var innerState = CreateNewStateValue(currentState.FinallyStack, handlerName);
				var stateBeforeFinally = CreateNewStateValue(innerState.FinallyStack);
				currentBlock.Add(new JsSetNextStateStatement(innerState.StateValue));
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.GuardedStatement, 0)), breakStack, continueStack, innerState, stateBeforeFinally));

				Enqueue(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(new JsBlockStatement(new JsBlockStatement(new JsStatement[0], true)), 0)), breakStack, continueStack, stateBeforeFinally, stateAfter.Item1);
				if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
				}
				return false;
			}
			else if (_isIteratorBlock && stmt.Finally != null && !currentState.FinallyStack.IsEmpty) {
				// This is necessary to special-case in order to ensure that the inner finally block is executed before all outer ones.
				return HandleTryStatement(new JsTryStatement(new JsTryStatement(stmt.GuardedStatement, stmt.Catch, null), null, stmt.Finally), location, stack, breakStack, continueStack, currentState, returnState, currentBlock);
			}
			else {
				var rewriter = new NestedJumpStatementRewriter(breakStack, continueStack, currentState, _exitState.Value);
				var guarded = FindInterestingConstructsVisitor.Analyze(stmt.GuardedStatement, InterestingConstruct.Label | InterestingConstruct.Await)
				              ? new JsBlockStatement(ProcessInner(stmt.GuardedStatement, breakStack, continueStack, currentState.FinallyStack, currentState.StateValue))
				              : rewriter.Process(stmt.GuardedStatement);

				JsCatchClause @catch;
				if (stmt.Catch != null) {
					if (FindInterestingConstructsVisitor.Analyze(stmt.Catch.Body, InterestingConstruct.Label)) {
						@catch = new JsCatchClause(stmt.Catch.Identifier, new JsBlockStatement(ProcessInner(stmt.Catch.Body, breakStack, continueStack, currentState.FinallyStack, currentState.StateValue)));
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
						@finally = new JsBlockStatement(ProcessInner(stmt.Finally, breakStack, continueStack, currentState.FinallyStack, currentState.StateValue));
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