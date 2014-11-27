using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	public class StateMachineRewriter {
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

		public static JsBlockStatement RewriteNormalMethod(JsBlockStatement block, Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateStateVariable, Func<string> allocateLoopLabel) {
			if (FindInterestingConstructsVisitor.Analyze(block, InterestingConstruct.Label)) {
				return new StateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, allocateStateVariable, allocateLoopLabel).Process(block);
			}
			else {
				return block;
			}
		}

		public static JsBlockStatement RewriteAsyncMethod(JsBlockStatement block, Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateStateVariable, Func<string> allocateLoopLabel, string stateMachineVariableName, string doFinallyBlocksVariableName, JsVariableDeclaration taskCompletionSource, Func<JsExpression, JsExpression> makeSetResult, Func<JsExpression, JsExpression> makeSetException, Func<JsExpression> getTask, Func<JsExpression, JsExpression, JsExpression> bindToContext) {
			var obj = new StateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, allocateStateVariable, allocateLoopLabel);
			return obj.ProcessAsyncMethod(block, stateMachineVariableName, doFinallyBlocksVariableName, taskCompletionSource, makeSetResult, makeSetException, getTask, bindToContext);
		}

		public static JsBlockStatement RewriteIteratorBlock(JsBlockStatement block, Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateStateVariable, Func<string> allocateLoopLabel, Func<string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent, Func<IteratorStateMachine, JsBlockStatement> makeIteratorBody) {
			var obj = new StateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, allocateStateVariable, allocateLoopLabel);
			var sm = obj.ProcessIteratorBlock(block, allocateFinallyHandler, makeSetCurrent);
			return makeIteratorBody(sm);
		}

		private bool IsConcreteStatement(JsStatement statement) {
			return !(statement is JsSequencePoint || statement is JsComment);
		}

		private StateMachineRewriter(Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateStateVariable, Func<string> allocateLoopLabel) {
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

		private bool IsLastStatementReachable(IList<JsStatement> statements) {
			JsStatement last = null;
			for (int i = statements.Count - 1; i >= 0; i--) {
				last = statements[i];
				if (IsConcreteStatement(last))
					break;
			}
			if (last == null)
				return true;
			var bst = last as JsBlockStatement;
			if (bst != null)
				return IsLastStatementReachable(bst.Statements);
			var ifst = last as JsIfStatement;
			if (ifst != null) {
				return ifst.Else == null || ifst.Then.Statements.Count == 0 || ifst.Else.Statements.Count == 0 || IsLastStatementReachable(ifst.Then.Statements) || IsLastStatementReachable(ifst.Else.Statements);
			}
			return !(last is JsReturnStatement || last is JsGotoStatement || last is JsGotoStateStatement || last is JsThrowStatement || last is JsBreakStatement || last is JsContinueStatement);
		}

		private bool HasFollowingStatement(StackEntry location) {
			for (int i = location.Index + 1; i < location.Block.Statements.Count; i++) {
				if (IsConcreteStatement(location.Block.Statements[i]))
					return true;
			}
			return false;
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
			return HasFollowingStatement(location) ? stack.Push(new StackEntry(location.Block, location.Index + 1)) : stack;
		}

		internal JsBlockStatement Process(JsBlockStatement statement) {
			_allocateFinallyHandler = null;
			_makeSetCurrent = null;
			var result = Process(statement, false, false);
			var hoistResult = VariableHoistingVisitor.Process(result);
			return JsStatement.Block(new[] { JsStatement.Var(new[] { JsStatement.Declaration(_stateVariableName, JsExpression.Number(0)) }.Concat(hoistResult.Item2.Select(v => JsStatement.Declaration(v, null)))) }.Concat(hoistResult.Item1.Statements));
		}

		private IteratorStateMachine ProcessIteratorBlock(JsBlockStatement statement, Func<string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent) {
			_allocateFinallyHandler = allocateFinallyHandler;
			_makeSetCurrent = makeSetCurrent;

			var result = Process(statement, isIteratorBlock: true, isAsync: false);

			var stateFinallyHandlers = _allStates.Where(s => !s.FinallyStack.IsEmpty).Select(s => Tuple.Create(s.StateValue, s.FinallyStack.Select(x => x.Item2).Reverse().ToList())).ToList();

			var hoistResult = VariableHoistingVisitor.Process(result);
			return new IteratorStateMachine(hoistResult.Item1,
			                                new[] { JsStatement.Declaration(_stateVariableName, JsExpression.Number(0)) }.Concat(hoistResult.Item2.Select(v => JsStatement.Declaration(v, null))),
			                                _finallyHandlers.Select(h => Tuple.Create(h.Item1, JsExpression.FunctionDefinition(new string[0], h.Item2))),
			                                stateFinallyHandlers.Count > 0 ? DisposeGenerator.GenerateDisposer(_stateVariableName, stateFinallyHandlers) : null);

		}

		private JsBlockStatement ProcessAsyncMethod(JsBlockStatement statement, string stateMachineMethodName, string doFinallyBlocksVariableName, JsVariableDeclaration taskCompletionSource, Func<JsExpression, JsExpression> makeSetResult, Func<JsExpression, JsExpression> makeSetException, Func<JsExpression> getTask, Func<JsExpression, JsExpression, JsExpression> bindToContext) {
			_stateMachineMethodName = stateMachineMethodName;
			_doFinallyBlocksVariableName = doFinallyBlocksVariableName;
			_makeSetResult = taskCompletionSource != null ? makeSetResult : null;
			_needDoFinallyBlocksVariable = new HasAwaitInsideTryWithFinallyVisitor().Analyze(statement);

			var result = Process(statement, isIteratorBlock: false, isAsync: true);
			var hoistResult = VariableHoistingVisitor.Process(result);

			string catchVariable = _allocateTempVariable();

			JsStatement body;
			if (taskCompletionSource != null && (statement.Statements.Count == 0 || IsLastStatementReachable(statement.Statements))) {	// If we return the task, and if we risk falling out of the original method, we need to add a setResult call.
				body = JsStatement.Block(hoistResult.Item1.Statements.Concat(new JsStatement[] { makeSetResult(null) }));
			}
			else {
				body = hoistResult.Item1;
			}

			if (taskCompletionSource != null)
				body = JsStatement.Try(body, JsStatement.Catch(catchVariable, JsStatement.Block(makeSetException(JsExpression.Identifier(catchVariable)))), null);

			IEnumerable<JsVariableDeclaration> declarations = new[] { JsStatement.Declaration(_stateVariableName, JsExpression.Number(0)) };
			if (taskCompletionSource != null)
				declarations = declarations.Concat(new[] { taskCompletionSource });
			declarations = declarations.Concat(hoistResult.Item2.Select(v => JsStatement.Declaration(v, null)));

			if (_needDoFinallyBlocksVariable)
				body = JsStatement.Block(new[] { JsStatement.Var(_doFinallyBlocksVariableName, JsExpression.True) }.Concat(body is JsBlockStatement ? ((JsBlockStatement)body).Statements : (IEnumerable<JsStatement>)new[] { body }));

			var stateMachine = JsExpression.FunctionDefinition(new string[0], body);
			var boundStateMachine = UsesThisVisitor.Analyze(stateMachine.Body) ? bindToContext(stateMachine, JsExpression.This) : stateMachine;
			
			IEnumerable<JsStatement> stmts = new JsStatement[] { JsStatement.Var(declarations),
			                                                     JsStatement.Var(stateMachineMethodName, boundStateMachine),
			                                                     JsExpression.Invocation(JsExpression.Identifier(stateMachineMethodName))
			                                                   };
			if (taskCompletionSource != null)
				stmts = stmts.Concat(new[] { JsStatement.Return(getTask()) });

			return JsStatement.Block(stmts);
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
			var body = ProcessInner(statement, ImmutableStack<Tuple<string, State>>.Empty, ImmutableStack<Tuple<string, State>>.Empty, ImmutableStack<Tuple<int, string>>.Empty, null).Item1;

			if (_isIteratorBlock)
				body.Add(JsStatement.Return(JsExpression.False));
			var resultBody = new FinalizerRewriter(_stateVariableName, _labelStates).Process(JsStatement.Block(body));
			return resultBody;
		}

		private IEnumerable<int> GetAllContainedStateValues(int state) {
			yield return state;
			List<int> childStates;
			if (_childStates.TryGetValue(state, out childStates)) {
				foreach (var c in childStates.SelectMany(GetAllContainedStateValues))
					yield return c;
			}
		}

		private Tuple<List<JsStatement>, int> ProcessInner(JsBlockStatement statement, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack, int? parentState) {
			var oldLoopLabel = _currentLoopLabel;
			var oldRemainingBlocks = _remainingBlocks;
			try {
				_currentLoopLabel = _allocateLoopLabel();
				_remainingBlocks = new Queue<RemainingBlock>();
				_remainingBlocks.Enqueue(new RemainingBlock(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(statement, 0)), breakStack, continueStack, CreateNewStateValue(finallyStack), new State(_currentLoopLabel, -1, finallyStack)));
				if (_exitState == null)
					_exitState = new State(_currentLoopLabel, -1, ImmutableStack<Tuple<int, string>>.Empty);
	
				var sections = new List<Section>();

				int iterationCount = 0;
	
				while (_remainingBlocks.Count > 0) {
					var current = _remainingBlocks.Dequeue();
					var list = Handle(current.Stack, current.BreakStack, current.ContinueStack, current.StateValue, current.ReturnState, _isIteratorBlock || _isAsync, true);
					// Merge all top-level blocks that should be merged with their parents.
					list = list.SelectMany(stmt => (stmt is JsBlockStatement && ((JsBlockStatement)stmt).MergeWithParent) ? ((JsBlockStatement)stmt).Statements : (IList<JsStatement>)new[] { stmt }).ToList();
					sections.Add(new Section(current.StateValue, list));

					if (iterationCount++ > 100000)
						throw new Exception("Infinite loop when rewriting method to a state machine");
				}

				if (parentState != null && _isAsync) {
					List<int> childStates;
					if (!_childStates.TryGetValue(parentState.Value, out childStates))
						_childStates[parentState.Value] = childStates = new List<int>();
					childStates.AddRange(sections.Select(s => s.State.StateValue));
				}

				var body = new List<JsStatement> {
				               JsStatement.Label(_currentLoopLabel,
				                   JsStatement.For(JsStatement.Empty, null, null,
				                       JsStatement.Switch(JsExpression.Identifier(_stateVariableName),
				                           sections.Select(b => JsStatement.SwitchSection(
				                                                    GetAllContainedStateValues(b.State.StateValue).OrderBy(v => v).Select(v => JsExpression.Number(v)),
				                                                    JsStatement.Block(b.Statements)))
				                                   .Concat(new[] { JsStatement.SwitchSection(new JsExpression[] { null }, JsStatement.Break(_currentLoopLabel)) }))))
				           };
				return Tuple.Create(body, sections[0].State.StateValue);
			}
			finally {
				_currentLoopLabel = oldLoopLabel;
				_remainingBlocks = oldRemainingBlocks;
			}
		}

		private List<JsStatement> Handle(ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, bool setIntermediateState, bool isRoot) {
			var currentBlock = new List<JsStatement>();
			if (setIntermediateState) {
				currentBlock.Add(new JsSetNextStateStatement(currentState.FinallyStack.IsEmpty ? -1 : currentState.FinallyStack.Peek().Item1));
			}

			bool isFirstStatement = isRoot;
			while (!stack.IsEmpty) {
				bool setIsFirstStatementFalse = true;
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
						if (currentBlock.Count == 0 || IsLastStatementReachable(currentBlock))
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
						currentBlock.AddRange(new NestedStatementFixer(breakStack, continueStack, currentState, _exitState.Value, _makeSetResult).Process(stmt));
					}
					stack = PushFollowing(stack, tos);
				}
				else if (stmt is JsAwaitStatement) {
					if (!HandleAwaitStatement((JsAwaitStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock))
						return currentBlock;
					stack = PushFollowing(stack, tos);
				}
				else if (stmt is JsTryStatement) {
					if (!HandleTryStatement((JsTryStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock, isFirstStatement))
						return currentBlock;
					stack = PushFollowing(stack, tos);
				}
				else if (FindInterestingConstructsVisitor.Analyze(stmt, InterestingConstruct.YieldReturn | InterestingConstruct.Label | InterestingConstruct.Await)) {
					if (stmt is JsBlockStatement) {
						stack = PushFollowing(stack, tos).Push(new StackEntry((JsBlockStatement)stmt, 0));
						setIsFirstStatementFalse = false;
					}
					else {
						if (stmt is JsIfStatement) {
							if (!HandleIfStatement((JsIfStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsDoWhileStatement) {
							if (!HandleDoWhileStatement((JsDoWhileStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock, isFirstStatement))
								return currentBlock;
						}
						else if (stmt is JsWhileStatement) {
							if (!HandleWhileStatement((JsWhileStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock, isFirstStatement))
								return currentBlock;
						}
						else if (stmt is JsForStatement) {
							if (!HandleForStatement((JsForStatement)stmt, tos, stack, breakStack, continueStack, currentState, returnState, currentBlock, isFirstStatement))
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
					currentBlock.AddRange(new NestedStatementFixer(breakStack, continueStack, currentState, _exitState.Value, _makeSetResult).Process(stmt));
					stack = PushFollowing(stack, tos);
				}
				if (setIsFirstStatementFalse)
					isFirstStatement = false;
			}
			if (currentBlock.Count == 0 || IsLastStatementReachable(currentBlock))
				currentBlock.Add(new JsGotoStateStatement(returnState, currentState));

			return currentBlock;
		}

		private Tuple<State, bool> GetStateAfterStatement(StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<int, string>> finallyStack, State returnState) {
			JsStatement next;
			if (HasFollowingStatement(location)) {
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

			currentBlock.Add(_makeSetCurrent(stmt.Value));
			currentBlock.Add(new JsSetNextStateStatement(stateAfter.Item1.StateValue));
			currentBlock.Add(JsStatement.Return(JsExpression.True));

			if (!stack.IsEmpty || HasFollowingStatement(location)) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
			}

			return false;
		}

		private bool HandleAwaitStatement(JsAwaitStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			var stateAfter = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState).Item1;
			bool createDummyState = false;
			if (stateAfter.StateValue == returnState.StateValue) {
				stateAfter = CreateNewStateValue(currentState.FinallyStack);
				createDummyState = true;	// We must never return to our parent state after an await because 
			}

			currentBlock.Add(new JsSetNextStateStatement(stateAfter.StateValue));
			currentBlock.Add(JsExpression.Invocation(JsExpression.Member(stmt.Awaiter, stmt.OnCompletedMethodName), JsExpression.Identifier(_stateMachineMethodName)));
			if (_needDoFinallyBlocksVariable)
				currentBlock.Add(JsExpression.Assign(JsExpression.Identifier(_doFinallyBlocksVariableName), JsExpression.False));
			currentBlock.Add(JsStatement.Return());

			if (!stack.IsEmpty || HasFollowingStatement(location)) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter, returnState);
			}
			if (createDummyState) {
				Enqueue(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(JsStatement.Block(JsStatement.BlockMerged(new JsStatement[0])), 0)), breakStack, continueStack, stateAfter, returnState);
			}

			return false;
		}

		private bool HandleTryStatement(JsTryStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock, bool isFirstStatement) {
			if (_isIteratorBlock && (FindInterestingConstructsVisitor.Analyze(stmt.GuardedStatement, InterestingConstruct.YieldReturn) || (stmt.Finally != null && stmt.Catch == null && !currentState.FinallyStack.IsEmpty))) {
				if (stmt.Catch != null)
					throw new InvalidOperationException("Cannot yield return from try with catch");
				string handlerName = _allocateFinallyHandler();
				JsBlockStatement handler;
				if (FindInterestingConstructsVisitor.Analyze(stmt.Finally, InterestingConstruct.Label)) {
					var inner = ProcessInner(stmt.Finally, breakStack, continueStack, currentState.FinallyStack, currentState.StateValue);
					handler = JsStatement.Block(new[]  { new JsSetNextStateStatement(inner.Item2) }.Concat(inner.Item1));
					handler = new FinalizerRewriter(_stateVariableName, _labelStates).Process(handler);
				}
				else {
					handler = stmt.Finally;
				}

				_finallyHandlers.Add(Tuple.Create(handlerName, handler));
				var stateAfter = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState);
				var innerState = CreateNewStateValue(currentState.FinallyStack, handlerName);
				var stateBeforeFinally = CreateNewStateValue(innerState.FinallyStack);
				currentBlock.Add(new JsSetNextStateStatement(innerState.StateValue));
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.GuardedStatement, 0)), breakStack, continueStack, new State(currentState.LoopLabelName, currentState.StateValue, innerState.FinallyStack), stateBeforeFinally, false, false));

				Enqueue(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(JsStatement.Block(JsStatement.BlockMerged(new JsStatement[0])), 0)), breakStack, continueStack, stateBeforeFinally, stateAfter.Item1);
				if (!stack.IsEmpty || HasFollowingStatement(location)) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
				}
				return false;
			}
			else if (_isIteratorBlock && stmt.Finally != null && !currentState.FinallyStack.IsEmpty) {
				// This is necessary to special-case in order to ensure that the inner finally block is executed before all outer ones.
				return HandleTryStatement(JsStatement.Try(JsStatement.Try(stmt.GuardedStatement, stmt.Catch, null), null, stmt.Finally), location, stack, breakStack, continueStack, currentState, returnState, currentBlock, isFirstStatement);
			}
			else {
				var rewriter = new NestedStatementFixer(breakStack, continueStack, currentState, _exitState.Value, _makeSetResult);
				JsBlockStatement guarded;
				var guardedConstructs = FindInterestingConstructsVisitor.Analyze(stmt.GuardedStatement);
				if ((guardedConstructs & (InterestingConstruct.Label | InterestingConstruct.Await)) != InterestingConstruct.None) {
					if (!isFirstStatement) {
						var sv = CreateNewStateValue(currentState.FinallyStack);
						Enqueue(stack.Push(location), breakStack, continueStack, sv, returnState);
						currentBlock.Add(new JsGotoStateStatement(sv, currentState));
						return false;
					}

					var inner  = ProcessInner(stmt.GuardedStatement, breakStack, continueStack, currentState.FinallyStack, currentState.StateValue);
					guarded    = JsStatement.Block(inner.Item1);
					currentBlock.Add(new JsSetNextStateStatement(inner.Item2));
				}
				else {
					guarded = rewriter.Process(stmt.GuardedStatement);
				}

				JsCatchClause @catch;
				if (stmt.Catch != null) {
					if (FindInterestingConstructsVisitor.Analyze(stmt.Catch.Body, InterestingConstruct.Label)) {
						var inner = ProcessInner(stmt.Catch.Body, breakStack, continueStack, currentState.FinallyStack, null);
						@catch = JsStatement.Catch(stmt.Catch.Identifier, JsStatement.Block(new[] { new JsSetNextStateStatement(inner.Item2) }.Concat(inner.Item1)));
					}
					else {
						var body = rewriter.Process(stmt.Catch.Body);
						@catch = ReferenceEquals(body, stmt.Catch.Body) ? stmt.Catch : JsStatement.Catch(stmt.Catch.Identifier, body);
					}
				}
				else
					@catch = null;

				JsBlockStatement @finally;
				if (stmt.Finally != null) {
					if (FindInterestingConstructsVisitor.Analyze(stmt.Finally, InterestingConstruct.Label)) {
						var inner = ProcessInner(stmt.Finally, breakStack, continueStack, currentState.FinallyStack, null);
						@finally = JsStatement.Block(new[] { new JsSetNextStateStatement(inner.Item2) }.Concat(inner.Item1));
					}
					else
						@finally = rewriter.Process(stmt.Finally);

					if ((guardedConstructs & InterestingConstruct.Await) != InterestingConstruct.None) {
						// Wrap the finally block inside an 'if (doFinallyBlocks) {}'
						@finally = JsStatement.Block(JsStatement.If(JsExpression.Identifier(_doFinallyBlocksVariableName), @finally, null));
					}
				}
				else
					@finally = null;

				if (currentBlock.Count > 0 && _childStates.ContainsKey(currentState.StateValue)) {
					var newBlock = JsStatement.If(JsExpression.Same(JsExpression.Identifier(_stateVariableName), JsExpression.Number(currentState.StateValue)), JsStatement.Block(currentBlock), null);
					currentBlock.Clear();
					currentBlock.Add(newBlock);
				}

				currentBlock.Add(JsStatement.Try(guarded, @catch, @finally));
				return true;
			}
		}

		private bool HandleIfStatement(JsIfStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			var stateAfter = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState);

			IList<JsStatement> thenPart, elsePart;
			if (stmt.Then.Statements.Count == 0)
				thenPart = ImmutableList<JsStatement>.Empty;
			else
				thenPart = Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Then, 0)), breakStack, continueStack, currentState, stateAfter.Item1, false, false);

			if (stmt.Else == null)
				elsePart = null;
			else if (stmt.Else.Statements.Count == 0)
				elsePart = ImmutableList<JsStatement>.Empty;
			else
				elsePart = Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Else, 0)), breakStack, continueStack, currentState, stateAfter.Item1, false, false);

			currentBlock.Add(JsStatement.If(stmt.Test, JsStatement.Block(thenPart), elsePart != null ? JsStatement.Block(elsePart) : null));
			if (thenPart.Count == 0 || elsePart == null || elsePart.Count == 0)
				currentBlock.Add(new JsGotoStateStatement(stateAfter.Item1, currentState));

			if (stateAfter.Item2) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
				return false;
			}

			return true;
		}

		private bool HandleDoWhileStatement(JsDoWhileStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock, bool isFirstStatement) {
			if (!isFirstStatement) {
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

				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Body, 0)), breakStack, continueStack.Push(Tuple.Create(GetLabelForState(currentState), beforeConditionState)), currentState, beforeConditionState, false, false));

				if (afterLoopState.Item2) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, afterLoopState.Item1, returnState);
					Enqueue(stack.Push(new StackEntry(JsStatement.Block(JsStatement.If(stmt.Condition, new JsGotoStateStatement(currentState, currentState), null)), 0)), breakStack, continueStack, beforeConditionState, afterLoopState.Item1);
				}
				else {
					Enqueue(PushFollowing(stack, location).Push(new StackEntry(JsStatement.Block(JsStatement.If(stmt.Condition, new JsGotoStateStatement(currentState, currentState), null)), 0)), breakStack, continueStack, beforeConditionState, returnState);
				}

				return false;
			}
		}

		private bool HandleWhileStatement(JsWhileStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock, bool isFirstStatement) {
			if (!isFirstStatement) {
				// We have to create a new block for the statement.
				var topOfLoopState = CreateNewStateValue(currentState.FinallyStack);
				Enqueue(stack.Push(location), breakStack, continueStack, topOfLoopState, returnState);
				currentBlock.Add(new JsGotoStateStatement(topOfLoopState, currentState));
				return false;
			}
			else {
				var afterLoopState = GetStateAfterStatement(location, stack, currentState.FinallyStack, returnState);

				currentBlock.Add(JsStatement.If(JsExpression.LogicalNot(stmt.Condition), new JsGotoStateStatement(afterLoopState.Item1, currentState), null));
				var currentName = GetLabelForState(currentState);
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Body, 0)), breakStack.Push(Tuple.Create(currentName, afterLoopState.Item1)), continueStack.Push(Tuple.Create(currentName, currentState)), currentState, currentState, false, false));

				if (!stack.IsEmpty || HasFollowingStatement(location)) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, afterLoopState.Item1, returnState);
				}

				return false;
			}
		}

		private bool HandleForStatement(JsForStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, State currentState, State returnState, IList<JsStatement> currentBlock, bool isFirstStatement) {
			if (!(isFirstStatement && (stmt.InitStatement is JsEmptyStatement || location.AfterForInitializer))) {
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
					currentBlock.Add(JsStatement.If(JsExpression.LogicalNot(stmt.ConditionExpression), new JsGotoStateStatement(afterLoopState.Item1, currentState), null));
				string currentName = GetLabelForState(currentState);
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Body, 0)), breakStack.Push(Tuple.Create(currentName, afterLoopState.Item1)), continueStack.Push(Tuple.Create(currentName, iteratorState)), currentState, iteratorState, false, false));

				if (stmt.IteratorExpression != null) {
					Enqueue(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(JsStatement.Block(stmt.IteratorExpression), 0)), breakStack, continueStack, iteratorState, currentState);
				}

				if (!stack.IsEmpty || HasFollowingStatement(location)) {
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
				currentBlock.Add(JsStatement.Var(newName, expression));
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

				if (i < stmt.Sections.Count - 1 && (origBody.Count == 0 || IsLastStatementReachable(origBody))) {
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

				var innerBreakStack = breakStack.Push(Tuple.Create(GetLabelForState(currentState), stateAfter.Item1));

				IList<JsStatement> body;
				if (currentFallthroughState != null) {
					body = new List<JsStatement>();
					body.Add(new JsGotoStateStatement(currentFallthroughState.Value, currentState));
					Enqueue(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(JsStatement.Block(origBody), 0)), innerBreakStack, continueStack, currentFallthroughState.Value, stateAfter.Item1);
				}
				else {
					body = Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(JsStatement.Block(origBody), 0)), innerBreakStack, continueStack, currentState, nextFallthroughState ?? stateAfter.Item1, false, false);
				}

				if (clause.Values.Any(v => v == null)) {
					defaultClause = JsStatement.Block(body);
				}
				else {
					JsExpression test = clause.Values.Select(v => JsExpression.Same(expression, v)).Aggregate((o, e) => o != null ? JsExpression.LogicalOr(o, e) : e);
					clauses.Add(Tuple.Create(test, JsStatement.Block(body)));
				}

				currentFallthroughState = nextFallthroughState;
			}
			clauses.Reverse();

			currentBlock.Add(clauses.Where(c => c.Item1 != null).Aggregate(defaultClause, (o, n) => JsStatement.If(n.Item1, n.Item2, o)));
			currentBlock.Add(new JsGotoStateStatement(stateAfter.Item1, currentState));

			if (stateAfter.Item2) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, stateAfter.Item1, returnState);
				return false;
			}

			return true;
		}
	}
}
