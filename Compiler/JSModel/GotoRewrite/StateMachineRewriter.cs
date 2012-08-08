using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		bool _yieldBreakNeedsRewrite;
		bool _result;

		private NeedsRewriteVisitor(bool yieldBreakNeedsRewrite) {
			_yieldBreakNeedsRewrite = yieldBreakNeedsRewrite;
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
			if (statement.Value != null || _yieldBreakNeedsRewrite)
				_result = true;
			return statement;
		}

		public static bool Analyze(JsStatement statement, bool yieldBreakNeedsRewrite) {
			var obj = new NeedsRewriteVisitor(yieldBreakNeedsRewrite);
			obj.VisitStatement(statement, null);
			return obj._result;
		}
	}

	internal class ContainsYieldVisitor : RewriterVisitorBase<object> {
		bool _result;

		private ContainsYieldVisitor() {
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			return statement;
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			return expression;
		}

		public override JsStatement VisitYieldStatement(JsYieldStatement statement, object data) {
			_result = true;
			return statement;
		}

		public static bool Analyze(JsStatement statement) {
			var obj = new ContainsYieldVisitor();
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

	public class StateMachine {
		public JsBlockStatement MainBlock { get; private set; }
		public ReadOnlyCollection<Tuple<string, JsFunctionDefinitionExpression>> FinallyHandlers { get; private set; }
		public JsBlockStatement Disposer { get; private set; }

		public StateMachine(JsBlockStatement mainBlock, IEnumerable<Tuple<string, JsFunctionDefinitionExpression>> finallyHandlers, JsBlockStatement disposer) {
			MainBlock       = mainBlock;
			FinallyHandlers = finallyHandlers.AsReadOnly();
			Disposer        = disposer;
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
			public ImmutableStack<Tuple<string, State>> BreakStack { get; private set; }
			public ImmutableStack<Tuple<string, State>> ContinueStack { get; private set; }
			public ImmutableStack<Tuple<int, string>> FinallyStack { get; private set; }
			public State StateValue { get; private set; }
			public State ReturnState { get; private set; }

			public RemainingBlock(ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack, State stateValue, State returnState) {
				Stack = stack;
				BreakStack = breakStack;
				ContinueStack = continueStack;
				FinallyStack = finallyStack;
				StateValue = stateValue;
				ReturnState = returnState;
			}
		}

		class Section {
			public State State { get; private set; }
			public IList<JsStatement> Statements { get; private set; }

			public Section(State state, IEnumerable<JsStatement> statements) {
				this.State = state;
				this.Statements = new List<JsStatement>(statements);
			}
		}

		interface IExecuteFinallyBlockStatementVisitor<TReturn, TData> {
			TReturn VisitExecuteFinallyBlocksStatement(JsExecuteFinallyBlocksStatement stmt, TData data);
		}

		class JsExecuteFinallyBlocksStatement : JsStatement {
			public string CompareLabel { get; private set; }
			public ImmutableStack<Tuple<int, string>> CompareFinallyStack { get; private set; }
			public ImmutableStack<Tuple<int, string>> OwnStack { get; private set; }

			public JsExecuteFinallyBlocksStatement(string compareLabel, ImmutableStack<Tuple<int, string>> ownStack) {
				CompareLabel = compareLabel;
				OwnStack = ownStack;
			}

			public JsExecuteFinallyBlocksStatement(ImmutableStack<Tuple<int, string>> compareFinallyStack, ImmutableStack<Tuple<int, string>> ownStack) {
				CompareFinallyStack = compareFinallyStack;
				OwnStack = ownStack;
			}

			public override TReturn Accept<TReturn, TData>(IStatementVisitor<TReturn, TData> visitor, TData data) {
				return ((IExecuteFinallyBlockStatementVisitor<TReturn, TData>)visitor).VisitExecuteFinallyBlocksStatement(this, data);
			}
		}

		class NestedBreakAndContinueRewriter : RewriterVisitorBase<object> {
			private ImmutableStack<Tuple<string, State>> _breakStack;
			private ImmutableStack<Tuple<string, State>> _continueStack;
			private ImmutableStack<Tuple<int, string>> _finallyStack;

			public NestedBreakAndContinueRewriter(ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack) {
				_breakStack = breakStack;
				_continueStack = continueStack;
				_finallyStack = finallyStack;
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
					return VisitBlockStatement(block, data);
				}
				finally {
					_breakStack = oldBreak;
					_continueStack = oldContinue;
				}
			}

			public override JsStatement VisitForStatement(JsForStatement statement, object data) {
				var body = VisitLoopBody(statement.Body, data);
				return ReferenceEquals(body, statement.Body) ? statement : new JsForStatement(statement.InitStatement, statement.ConditionExpression, statement.IteratorExpression, body);
			}

			public override JsStatement VisitForEachInStatement(JsForEachInStatement statement, object data) {
				var body = VisitLoopBody(statement.Body, data);
				return ReferenceEquals(body, statement.Body) ? statement : new JsForEachInStatement(statement.LoopVariableName, statement.ObjectToIterateOver, body, statement.IsLoopVariableDeclared);
			}

			public override JsStatement VisitWhileStatement(JsWhileStatement statement, object data) {
				var body = VisitLoopBody(statement.Body, data);
				return ReferenceEquals(body, statement.Body) ? statement : new JsWhileStatement(statement.Condition, body);
			}

			public override JsStatement VisitDoWhileStatement(JsDoWhileStatement statement, object data) {
				var body = VisitLoopBody(statement.Body, data);
				return ReferenceEquals(body, statement.Body) ? statement : new JsDoWhileStatement(statement.Condition, body);
			}

			public override JsStatement VisitSwitchStatement(JsSwitchStatement statement, object data) {
				var oldBreak = _breakStack;
				var oldContinue = _continueStack;
				try {
					_breakStack = _breakStack.Push(null);
					_continueStack = _continueStack.Push(null);
					var sections = VisitSwitchSections(statement.Sections, data);
					return ReferenceEquals(sections, statement.Sections) ? statement : new JsSwitchStatement(statement.Expression, sections);
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
					var l = new List<JsStatement>() { new JsExecuteFinallyBlocksStatement(_finallyStack, state.Item2.FinallyStack) };
					GotoState(l, state.Item2);
					return new JsBlockStatement(l, mergeWithParent: true);
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
					var l = new List<JsStatement>() { new JsExecuteFinallyBlocksStatement(_finallyStack, state.Item2.FinallyStack) };
					GotoState(l, state.Item2);
					return new JsBlockStatement(l, mergeWithParent: true);
				}
				else {
					return statement;
				}
			}

			public override JsStatement VisitYieldStatement(JsYieldStatement statement, object data) {
				if (statement.Value != null)
					throw new InvalidOperationException("yield return should have been already taken care of.");

				return new JsBlockStatement(new JsStatement[] {
					new JsExecuteFinallyBlocksStatement(ImmutableStack<Tuple<int, string>>.Empty, _finallyStack),
					new JsReturnStatement(JsExpression.False)
				}, mergeWithParent: true);
			}

			public override JsStatement VisitGotoStatement(JsGotoStatement statement, object data) {
				return new JsBlockStatement(new JsStatement[] {
					new JsExecuteFinallyBlocksStatement(statement.TargetLabel, _finallyStack),
					statement
				}, mergeWithParent: true);
			}

			public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
				return statement;
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
				return expression;
			}
		}

		class FinalizerRewriter : RewriterVisitorBase<object>, IExecuteFinallyBlockStatementVisitor<JsStatement, object> {
			Dictionary<string, State> _labelStates = new Dictionary<string, State>();

			public FinalizerRewriter(Dictionary<string, State> labelStates) {
				_labelStates = labelStates;
			}

			public JsBlockStatement Process(JsBlockStatement statement) {
				return (JsBlockStatement)VisitStatement(statement, null);
			}

			public override JsStatement VisitGotoStatement(JsGotoStatement statement, object data) {
				State targetState;
				if (!_labelStates.TryGetValue(statement.TargetLabel, out targetState))
					throw new InvalidOperationException("The label " + statement.TargetLabel + " does not exist.");
				return GotoState(targetState, true);
			}

			public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
				return statement;
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
				return expression;
			}

			public JsStatement VisitExecuteFinallyBlocksStatement(JsExecuteFinallyBlocksStatement statement, object data) {
				var result = new List<JsStatement>();
				var compareStack = (statement.CompareFinallyStack ?? _labelStates[statement.CompareLabel].FinallyStack);
				var remaining = statement.OwnStack;
				for (int i = 0, n = remaining.Count() - compareStack.Count(); i < n; i++) {
					var current = remaining.Peek();
					remaining = remaining.Pop();
					#warning TODO: Obviously the variable name must be changed (but this is about to be redone anyway)
					result.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("$tmp1"), JsExpression.Number(remaining.IsEmpty ? -1 : remaining.Peek().Item1))));
					result.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Identifier(current.Item2))));
				}
				return new JsBlockStatement(result, mergeWithParent: true);
			}
		}

		struct State : IEquatable<State> {
			public string VariableName { get; private set; }
			public string LoopLabelName { get; private set; }
			public int StateValue { get; private set; }
			public ImmutableStack<Tuple<int, string>> FinallyStack { get; private set; }

			public State(string variableName, string loopLabelName, int stateValue, ImmutableStack<Tuple<int, string>> finallyStack) : this() {
				VariableName = variableName;
				LoopLabelName = loopLabelName;
				StateValue = stateValue;
				FinallyStack = finallyStack;
			}

			public bool Equals(State other) {
				return Equals(other.VariableName, VariableName) && Equals(other.LoopLabelName, LoopLabelName) && other.StateValue == StateValue;
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj)) return false;
				if (obj.GetType() != typeof(State)) return false;
				return Equals((State)obj);
			}

			public override int GetHashCode() {
				unchecked {
					int result = VariableName.GetHashCode();
					result = (result*397) ^ LoopLabelName.GetHashCode();
					result = (result*397) ^ StateValue;
					return result;
				}
			}
		}

		int _nextStateIndex;
		string _currentStateVariableName;
		string _currentLoopLabel;
		bool _isIteratorBlock;
		Queue<RemainingBlock> _remainingBlocks = new Queue<RemainingBlock>();
		HashSet<State> _processedStates = new HashSet<State>();
		Dictionary<string, State> _labelStates = new Dictionary<string, State>();

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

		private State CreateNewStateValue(ImmutableStack<Tuple<int, string>> finallyStack) {
			return new State(_currentStateVariableName, _currentLoopLabel, _nextStateIndex++, finallyStack);
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

			return !(current is JsReturnStatement || current is JsGotoStatement || current is JsThrowStatement || current is JsBreakStatement || current is JsContinueStatement);
		}

		private void Enqueue(ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack, State stateValue, State returnState) {
			if (_processedStates.Contains(stateValue))
				throw new InvalidOperationException("Duplicate enqueueing of " + stateValue);
			_processedStates.Add(stateValue);
			if (stack.IsEmpty)
				throw new InvalidOperationException("Empty stack for state " + stateValue);
			_remainingBlocks.Enqueue(new RemainingBlock(stack, breakStack, continueStack, finallyStack, stateValue, returnState));
		}

		private ImmutableStack<StackEntry> PushFollowing(ImmutableStack<StackEntry> stack, StackEntry location) {
			return location.Index < location.Block.Statements.Count - 1 ? stack.Push(new StackEntry(location.Block, location.Index + 1)) : stack;
		}

		private static void SetNextState(IList<JsStatement> statements, State state) {
			statements.Add(new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(state.VariableName), JsExpression.Number(state.StateValue))));
		}

		private static void GotoState(IList<JsStatement> statements, State state) {
			if (state.StateValue == -1) {
				statements.Add(new JsBreakStatement(state.LoopLabelName));
			}
			else {
				SetNextState(statements, state);
				statements.Add(new JsContinueStatement(state.LoopLabelName));
			}
		}

		private static JsBlockStatement GotoState(State state, bool mergeWithParent) {
			var list = new List<JsStatement>();
			if (state.StateValue == -1) {
				list.Add(new JsBreakStatement(state.LoopLabelName));
			}
			else {
				SetNextState(list, state);
				list.Add(new JsContinueStatement(state.LoopLabelName));
			}
			return new JsBlockStatement(list, mergeWithParent);
		}

		public JsBlockStatement Process(JsBlockStatement statement, bool isIteratorBlock) {
			var stmt = ProcessInner(statement, isIteratorBlock, ImmutableStack<Tuple<string, State>>.Empty, ImmutableStack<Tuple<string, State>>.Empty, ImmutableStack<Tuple<int, string>>.Empty);
			return new FinalizerRewriter(_labelStates).Process(stmt);
		}

		private JsBlockStatement ProcessInner(JsBlockStatement statement, bool isIteratorBlock, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack) {
			_nextStateIndex = 0;
			_currentStateVariableName = _allocateTempVariable();
			_currentLoopLabel = _allocateLoopLabel();
			_isIteratorBlock = isIteratorBlock;
			_processedStates.Clear();
			_labelStates.Clear();
			_remainingBlocks = new Queue<RemainingBlock>();
			_remainingBlocks.Enqueue(new RemainingBlock(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(statement, 0)), breakStack, continueStack, finallyStack, CreateNewStateValue(finallyStack), new State(_currentStateVariableName, _currentLoopLabel, -1, finallyStack)));

			var sections = new List<Section>();

			while (_remainingBlocks.Count > 0) {
				var current = _remainingBlocks.Dequeue();
				var list = Handle(current.Stack, current.BreakStack, current.ContinueStack, current.FinallyStack, current.StateValue, current.ReturnState);
				if (isIteratorBlock)
					list.Insert(0, new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(_currentStateVariableName), JsExpression.Number(current.FinallyStack.IsEmpty ? -1 : current.FinallyStack.Peek().Item1))));
				sections.Add(new Section(current.StateValue, list));
			}

			var body = new List<JsStatement> {
			    new JsVariableDeclarationStatement(_currentStateVariableName, JsExpression.Number(0)),
			    new JsLabelledStatement(_currentLoopLabel,
			        new JsForStatement(new JsEmptyStatement(), null, null,
			            new JsSwitchStatement(JsExpression.Identifier(_currentStateVariableName),
			                sections.Select(b =>
			                    new JsSwitchSection(
			                        new[] { JsExpression.Number(b.State.StateValue) },
			                        new JsBlockStatement(b.Statements))))))
			};
			if (isIteratorBlock)
				body.Add(new JsReturnStatement(JsExpression.False));
			return new JsBlockStatement(body);
		}

		private List<JsStatement> Handle(ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack, State currentState, State returnState) {
			var currentBlock = new List<JsStatement>();
			while (!stack.IsEmpty) {
				var tos = stack.Peek();
				stack = stack.Pop();

				var stmt = tos.Block.Statements[tos.Index];
				var lbl = stmt as JsLabelledStatement;
				if (lbl != null) {
					if (_processedStates.Contains(GetOrCreateStateForLabel(lbl.Label, finallyStack))) {
						// First statement in the new block
						stmt = lbl.Statement;
					}
					else {
						// A label that terminates the current block.
						Enqueue(stack.Push(new StackEntry(tos.Block, tos.Index)), breakStack, continueStack, finallyStack, GetOrCreateStateForLabel(lbl.Label, finallyStack), returnState);
						if (currentBlock.Count == 0 || IsNextStatementReachable(currentBlock[currentBlock.Count - 1]))
							GotoState(currentBlock, GetOrCreateStateForLabel(lbl.Label, finallyStack));
						return currentBlock;
					}
				}

				if (stmt is JsYieldStatement) {
					var ystmt = (JsYieldStatement)stmt;
					if (ystmt.Value != null) {
						if (!HandleYieldReturnStatement(ystmt, tos, stack, breakStack, continueStack, finallyStack, currentState, returnState, currentBlock))
							return currentBlock;
					}
					else {
						currentBlock.AddRange(new NestedBreakAndContinueRewriter(breakStack, continueStack, finallyStack).Process(stmt));
						stack = PushFollowing(stack, tos);
					}
				}
				else if (NeedsRewriteVisitor.Analyze(stmt, true)) {
					if (stmt is JsBlockStatement) {
						stack = PushFollowing(stack, tos).Push(new StackEntry((JsBlockStatement)stmt, 0));
					}
					else {
						if (stmt is JsTryStatement) {
							if (!HandleTryStatement((JsTryStatement)stmt, tos, stack, breakStack, continueStack, finallyStack, currentState, returnState, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsIfStatement) {
							if (!HandleIfStatement((JsIfStatement)stmt, tos, stack, breakStack, continueStack, finallyStack, currentState, returnState, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsDoWhileStatement) {
							if (!HandleDoWhileStatement((JsDoWhileStatement)stmt, tos, stack, breakStack, continueStack, finallyStack, currentState, returnState, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsWhileStatement) {
							if (!HandleWhileStatement((JsWhileStatement)stmt, tos, stack, breakStack, continueStack, finallyStack, currentState, returnState, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsForStatement) {
							if (!HandleForStatement((JsForStatement)stmt, tos, stack, breakStack, continueStack, finallyStack, currentState, returnState, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsSwitchStatement) {
							if (!HandleSwitchStatement((JsSwitchStatement)stmt, tos, stack, breakStack, continueStack, finallyStack, currentState, returnState, currentBlock))
								return currentBlock;
						}
						else {
							throw new NotSupportedException("Statement " + stmt + " cannot contain labels.");
						}

						stack = PushFollowing(stack, tos);
					}
				}
				else {
					currentBlock.AddRange(new NestedBreakAndContinueRewriter(breakStack, continueStack, finallyStack).Process(stmt));
					stack = PushFollowing(stack, tos);
				}
			}
			if (currentBlock.Count == 0 || IsNextStatementReachable(currentBlock[currentBlock.Count - 1]))
				GotoState(currentBlock, returnState);

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

		private bool HandleYieldReturnStatement(JsYieldStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			var stateAfter = GetStateAfterStatement(location, stack, finallyStack, returnState);

			currentBlock.Add(new JsExpressionStatement(_makeSetCurrent(stmt.Value)));
			SetNextState(currentBlock, stateAfter.Item1);
			currentBlock.Add(new JsReturnStatement(JsExpression.True));

			if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, finallyStack, stateAfter.Item1, returnState);
			}

			return false;
		}

		private JsBlockStatement InnerRewrite(JsBlockStatement stmt, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack) {
			var innerRewriter = new SingleStateMachineRewriter(_isExpressionComplexEnoughForATemporaryVariable, _allocateTempVariable, _allocateLoopLabel, _allocateFinallyHandler, _makeSetCurrent);
			var result = innerRewriter.ProcessInner(stmt, _isIteratorBlock, breakStack, continueStack, finallyStack);
			foreach (var ls in innerRewriter._labelStates)
				this._labelStates[ls.Key] = ls.Value;
			return result;
		}

		private bool HandleTryStatement(JsTryStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			if (ContainsYieldVisitor.Analyze(stmt.GuardedStatement)) {
				if (stmt.Catch != null)
					throw new InvalidOperationException("Cannot yield from try with catch");
				string handlerName = _allocateFinallyHandler(NeedsRewriteVisitor.Analyze(stmt.Finally, false) ? InnerRewrite(stmt.Finally, breakStack, continueStack, finallyStack) : stmt.Finally);
				var stateAfter = GetStateAfterStatement(location, stack, finallyStack, returnState);
				var innerState = CreateNewStateValue(finallyStack);
				var stateBeforeFinally = CreateNewStateValue(finallyStack);
				SetNextState(currentBlock, innerState);
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.GuardedStatement, 0)), breakStack, continueStack, finallyStack.Push(Tuple.Create(innerState.StateValue, handlerName)), currentState, stateBeforeFinally));

				Enqueue(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(JsBlockStatement.MakeBlock(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Identifier(handlerName)))), 0)), breakStack, continueStack, finallyStack, stateBeforeFinally, stateAfter.Item1);
				if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, finallyStack, stateAfter.Item1, returnState);
				}
				return false;
			}
			else {
				var guarded = NeedsRewriteVisitor.Analyze(stmt.GuardedStatement, false) ? InnerRewrite(stmt.GuardedStatement, breakStack, continueStack, finallyStack) : stmt.GuardedStatement;
				var @catch = stmt.Catch != null && NeedsRewriteVisitor.Analyze(stmt.Catch.Body, false) ? new JsCatchClause(stmt.Catch.Identifier, InnerRewrite(stmt.Catch.Body, breakStack, continueStack, finallyStack)) : stmt.Catch;
 				var @finally = stmt.Finally != null && NeedsRewriteVisitor.Analyze(stmt.Finally, false) ? InnerRewrite(stmt.Finally, breakStack, continueStack, finallyStack) : stmt.Finally;
				currentBlock.Add(new JsTryStatement(guarded, @catch, @finally));
				return true;
			}
		}

		private bool HandleIfStatement(JsIfStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			var stateAfter = GetStateAfterStatement(location, stack, finallyStack, returnState);

			var thenPart = Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Then, 0)), breakStack, continueStack, finallyStack, currentState, stateAfter.Item1);
			var elsePart = stmt.Else != null ? Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Else, 0)), breakStack, continueStack, finallyStack, currentState, stateAfter.Item1) : null;

			currentBlock.Add(new JsIfStatement(stmt.Test, new JsBlockStatement(thenPart), elsePart != null ? new JsBlockStatement(elsePart) : null));
			if (elsePart == null)
				GotoState(currentBlock, stateAfter.Item1);

			if (stateAfter.Item2) {
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, finallyStack, stateAfter.Item1, returnState);
				return false;
			}

			return true;
		}

		private bool HandleDoWhileStatement(JsDoWhileStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0) {
				// We have to create a new block for the statement.
				var topOfLoopState = CreateNewStateValue(finallyStack);
				Enqueue(stack.Push(location), breakStack, continueStack, finallyStack, topOfLoopState, returnState);
				GotoState(currentBlock, topOfLoopState);
				return false;
			}
			else {
				var beforeConditionState = CreateNewStateValue(finallyStack);
				Tuple<State, bool> afterLoopState;
				string currentName = GetLabelForState(currentState);
				if (new ContainsBreakVisitor().Analyze(stmt.Body, currentName)) {
					afterLoopState = GetStateAfterStatement(location, stack, finallyStack, returnState);
					breakStack = breakStack.Push(Tuple.Create(currentName, afterLoopState.Item1));
				}
				else {
					afterLoopState = Tuple.Create(returnState, false);
				}

				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Body, 0)), breakStack, continueStack.Push(Tuple.Create(GetLabelForState(currentState), beforeConditionState)), finallyStack, currentState, beforeConditionState));

				if (afterLoopState.Item2) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, finallyStack, afterLoopState.Item1, returnState);
					Enqueue(stack.Push(new StackEntry(new JsBlockStatement(new JsIfStatement(stmt.Condition, GotoState(currentState, false), null)), 0)), breakStack, continueStack, finallyStack, beforeConditionState, afterLoopState.Item1);
				}
				else {
					Enqueue(PushFollowing(stack, location).Push(new StackEntry(new JsBlockStatement(new JsIfStatement(stmt.Condition, GotoState(currentState, false), null)), 0)), breakStack, continueStack, finallyStack, beforeConditionState, returnState);
				}

				return false;
			}
		}

		private bool HandleWhileStatement(JsWhileStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0) {
				// We have to create a new block for the statement.
				var topOfLoopState = CreateNewStateValue(finallyStack);
				Enqueue(stack.Push(location), breakStack, continueStack, finallyStack, topOfLoopState, returnState);
				GotoState(currentBlock, topOfLoopState);
				return false;
			}
			else {
				var afterLoopState = GetStateAfterStatement(location, stack, finallyStack, returnState);

				currentBlock.Add(new JsIfStatement(JsExpression.LogicalNot(stmt.Condition), GotoState(afterLoopState.Item1, false), null));
				var currentName = GetLabelForState(currentState);
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Body, 0)), breakStack.Push(Tuple.Create(currentName, afterLoopState.Item1)), continueStack.Push(Tuple.Create(currentName, currentState)), finallyStack, currentState, currentState));

				if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, finallyStack, afterLoopState.Item1, returnState);
				}

				return false;
			}
		}

		private bool HandleForStatement(JsForStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0 || (!(stmt.InitStatement is JsEmptyStatement) && !location.AfterForInitializer)) {
				// We have to create a new block for the statement.
				var topOfLoopState = CreateNewStateValue(finallyStack);
				Enqueue(stack.Push(new StackEntry(location.Block, location.Index, true)), breakStack, continueStack, finallyStack, topOfLoopState, returnState);
				if (!(stmt.InitStatement is JsEmptyStatement))
					currentBlock.Add(stmt.InitStatement);
				GotoState(currentBlock, topOfLoopState);
				return false;
			}
			else {
				var iteratorState = (stmt.IteratorExpression != null ? CreateNewStateValue(finallyStack) : currentState);
				var afterLoopState = GetStateAfterStatement(location, stack, finallyStack, returnState);

				if (stmt.ConditionExpression != null)
					currentBlock.Add(new JsIfStatement(JsExpression.LogicalNot(stmt.ConditionExpression), GotoState(afterLoopState.Item1, false), null));
				string currentName = GetLabelForState(currentState);
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Body, 0)), breakStack.Push(Tuple.Create(currentName, afterLoopState.Item1)), continueStack.Push(Tuple.Create(currentName, iteratorState)), finallyStack, currentState, iteratorState));

				if (stmt.IteratorExpression != null) {
					Enqueue(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(JsBlockStatement.MakeBlock(new JsExpressionStatement(stmt.IteratorExpression)), 0)), breakStack, continueStack, finallyStack, iteratorState, currentState);
				}

				if (!stack.IsEmpty || location.Index < location.Block.Statements.Count - 1) {
					Enqueue(PushFollowing(stack, location), breakStack, continueStack, finallyStack, afterLoopState.Item1, returnState);
				}

				return false;
			}
		}

		private bool HandleSwitchStatement(JsSwitchStatement stmt, StackEntry location, ImmutableStack<StackEntry> stack, ImmutableStack<Tuple<string, State>> breakStack, ImmutableStack<Tuple<string, State>> continueStack, ImmutableStack<Tuple<int, string>> finallyStack, State currentState, State returnState, IList<JsStatement> currentBlock) {
			var stateAfter = GetStateAfterStatement(location, stack, finallyStack, returnState);
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
						nextFallthroughState = GetOrCreateStateForLabel(((JsLabelledStatement)nextBody[0]).Label, finallyStack);
					else
						nextFallthroughState = CreateNewStateValue(finallyStack);
				}
				else {
					nextFallthroughState = null;
				}

				breakStack = breakStack.Push(Tuple.Create(GetLabelForState(currentState), stateAfter.Item1));

				IList<JsStatement> body;
				if (currentFallthroughState != null) {
					body = new List<JsStatement>();
					GotoState(body, currentFallthroughState.Value);
					Enqueue(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(new JsBlockStatement(origBody), 0)), breakStack, continueStack, finallyStack, currentFallthroughState.Value, stateAfter.Item1);
				}
				else {
					body = Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(new JsBlockStatement(origBody), 0)), breakStack, continueStack, finallyStack, currentState, nextFallthroughState ?? stateAfter.Item1);
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
				Enqueue(PushFollowing(stack, location), breakStack, continueStack, finallyStack, stateAfter.Item1, returnState);
				return false;
			}

			return true;
		}
	}

	public class StateMachineRewriter : RewriterVisitorBase<object> {
		private readonly Func<JsExpression, bool> _isExpressionComplexEnoughForATemporaryVariable;
		private readonly Func<string> _allocateTempVariable;
		private readonly Func<string> _allocateLoopLabel;
		private readonly Func<string> _allocateFinallyHandler;
		private readonly Func<JsExpression, JsExpression> _makeSetCurrent;
		private readonly bool _isIteratorBlock;

		private readonly List<Tuple<string, JsFunctionDefinitionExpression>> _finallyHandlers = new List<Tuple<string, JsFunctionDefinitionExpression>>();

		private StateMachineRewriter(Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel, Func<string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent, bool isIteratorBlock) {
			_isExpressionComplexEnoughForATemporaryVariable = isExpressionComplexEnoughForATemporaryVariable;
			_allocateTempVariable = allocateTempVariable;
			_allocateLoopLabel = allocateLoopLabel;
			_allocateFinallyHandler = allocateFinallyHandler;
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

		private string AddFinallyHandler(JsBlockStatement body) {
			var name = _allocateFinallyHandler();
			_finallyHandlers.Add(Tuple.Create(name, new JsFunctionDefinitionExpression(new string[0], body, null)));
			return name;
		}

		private JsBlockStatement DoRewrite(JsBlockStatement block) {
			if (!NeedsRewriteVisitor.Analyze(block, true))
				return block;

			return new SingleStateMachineRewriter(_isExpressionComplexEnoughForATemporaryVariable, _allocateTempVariable, _allocateLoopLabel, AddFinallyHandler, _makeSetCurrent).Process(block, _isIteratorBlock);
		}

		public static StateMachine Rewrite(JsBlockStatement block, Func<JsExpression, bool> isExpressionComplexEnoughForATemporaryVariable, Func<string> allocateTempVariable, Func<string> allocateLoopLabel, Func<string> allocateFinallyHandler, Func<JsExpression, JsExpression> makeSetCurrent, bool isIteratorBlock) {
			var obj = new StateMachineRewriter(isExpressionComplexEnoughForATemporaryVariable, allocateTempVariable, allocateLoopLabel, allocateFinallyHandler, makeSetCurrent, isIteratorBlock);
			var mainBlock = obj.DoRewrite((JsBlockStatement)obj.VisitBlockStatement(block, null));
			return new StateMachine(mainBlock, obj._finallyHandlers, new JsBlockStatement());
		}
	}
}
