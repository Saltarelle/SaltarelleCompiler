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
/*	internal class LabelledBlockGatherer : RewriterVisitorBase<object> {
		List<LabelledBlock> _result;
		Queue<Tuple<JsBlockStatement, int, string>> _outstandingBlocks = new Queue<Tuple<JsBlockStatement, int, string>>();

		public IList<LabelledBlock> Gather(JsBlockStatement statement) {
			_outstandingBlocks = new Queue<Tuple<JsBlockStatement, int, string>>();
			_outstandingBlocks.Enqueue(Tuple.Create(statement, 0, LabelledBlock.ExitLabelName));
			_result = new List<LabelledBlock>();

			while (_outstandingBlocks.Count > 0) {
				var current = _outstandingBlocks.Dequeue();
				Visit(current.Item2 == 0 ? current.Item1 : new JsBlockStatement(current.Item1.Statements.Skip(current.Item2)));
			}

			_result = new List<LabelledBlock>();
			_currentBlock = null;
			NewBlock(null);
			bool reachable = Visit(statement, LabelledBlock.ExitLabelName);
			if (reachable)
				_currentBlock.Add(new JsGotoStatement(LabelledBlock.ExitLabelName));
			_result.Add(new LabelledBlock(_currentBlockName, _currentBlock));
			return _result;
		}
	}
*/
	internal class LabelledBlockGatherer {
		internal const string ExitLabelName = "$exit";	// Use this label as a name to denote that when control leaves a block through this path, it means that it is leaving the current state machine.

		class ContainsLabelsVisitor : RewriterVisitorBase<object> {
			bool _result;

			public override JsStatement Visit(JsLabelledStatement statement, object data) {
				_result = true;
				return statement;
			}

			public bool Process(JsStatement statement) {
				_result = false;
				Visit(statement, null);
				return _result;
			}
		}

		// TODO: This class does not support 'break label' and 'continue label' statements (but we don't generate those).
		class DoNotEnterLoopsVisitor<T> : RewriterVisitorBase<T> {
			public override JsStatement Visit(JsForStatement statement, T data) {
				return statement;
			}

			public override JsStatement Visit(JsForEachInStatement statement, T data) {
				return statement;
			}

			public override JsStatement Visit(JsWhileStatement statement, T data) {
				return statement;
			}

			public override JsStatement Visit(JsDoWhileStatement statement, T data) {
				return statement;
			}

			public override JsStatement Visit(JsFunctionStatement statement, T data) {
				return statement;
			}

			public override JsExpression Visit(JsFunctionDefinitionExpression expression, T data) {
				return expression;
			}
		}

		class ReplaceBreakWithGotoVisitor : DoNotEnterLoopsVisitor<string> {
			public override JsStatement Visit(JsSwitchStatement statement, string data) {
				return statement;
			}

			public override JsStatement Visit(JsBreakStatement statement, string data) {
				return new JsGotoStatement(data);
			}

			public JsBlockStatement Process(JsBlockStatement statement, string labelName) {
				return (JsBlockStatement)Visit(statement, labelName);
			}
		}

		class ContainsBreakVisitor : RewriterVisitorBase<object> {
			bool _result = false;

			public override JsStatement Visit(JsSwitchStatement statement, object data) {
				return statement;
			}

			public override JsStatement Visit(JsBreakStatement statement, object data) {
				_result = true;
				return statement;
			}

			public bool Process(JsStatement statement) {
				_result = false;
				Visit(statement, null);
				return _result;
			}
		}

		class ReplaceContinueWithGotoVisitor : DoNotEnterLoopsVisitor<string> {
			public override JsStatement Visit(JsContinueStatement statement, string data) {
				return new JsGotoStatement(data);
			}

			public JsBlockStatement Process(JsBlockStatement statement, string labelName) {
				return (JsBlockStatement)Visit(statement, labelName);
			}
		}

		class ContainsContinueVisitor : RewriterVisitorBase<object> {
			bool _result = false;

			public override JsStatement Visit(JsContinueStatement statement, object data) {
				_result = true;
				return statement;
			}

			public bool Process(JsStatement statement) {
				_result = false;
				Visit(statement, null);
				return _result;
			}
		}

		[DebuggerDisplay("{DebugToString()}")]
		class StackEntry {
			public JsBlockStatement Block { get; private set; }
			public int Index { get; private set; }

			public StackEntry(JsBlockStatement block, int index) {
				Block = block;
				Index = index;
			}

			public string DebugToString() {
				return new JsBlockStatement(Block.Statements.Skip(Index)).DebugToString();
			}
		}

		class OutstandingBlock {
			public ImmutableStack<StackEntry> Stack { get; private set; }
			public string Name { get; private set; }
			public string ReturnLabel { get; private set; }

			public OutstandingBlock(ImmutableStack<StackEntry> stack, string name, string returnLabel) {
				Stack = stack;
				Name = name;
				ReturnLabel = returnLabel;
			}
		}

		int _currentAnonymousBlockIndex;
		Queue<OutstandingBlock> _outstandingBlocks = new Queue<OutstandingBlock>();
		HashSet<JsStatement> _processedStatements;

		private string CreateAnonymousBlockName() {
			return string.Format(CultureInfo.InvariantCulture, "${0}", _currentAnonymousBlockIndex++);
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

			return !(current is JsReturnStatement || current is JsGotoStatement || current is JsThrowStatement);
		}

		private void Enqueue(ImmutableStack<StackEntry> stack, string name, string exitLabel) {
			var tos = stack.Peek();
			if (_processedStatements.Contains(tos.Block.Statements[tos.Index]))
				throw new Exception("Don't think this will happen");
			Console.WriteLine("Enqueue " + tos.Block.Statements[tos.Index].DebugToString());
			_outstandingBlocks.Enqueue(new OutstandingBlock(stack, name, exitLabel));
			_processedStatements.Add(tos.Block.Statements[tos.Index]);
		}

		private bool ContainsLabels(JsStatement statement) {
			return new ContainsLabelsVisitor().Process(statement);
		}

		private bool ContainsBreak(JsStatement statement) {
			return new ContainsBreakVisitor().Process(statement);
		}

		private bool ContainsContinue(JsStatement statement) {
			return new ContainsContinueVisitor().Process(statement);
		}

		private ImmutableStack<StackEntry> PushFollowing(ImmutableStack<StackEntry> stack, JsBlockStatement block, int currentIndex) {
			return currentIndex < block.Statements.Count - 1 ? stack.Push(new StackEntry(block, currentIndex + 1)) : stack;
		}

		public IList<LabelledBlock> Gather(JsBlockStatement statement) {
			var startBlockName = CreateAnonymousBlockName();
			_processedStatements = new HashSet<JsStatement>(ReferenceComparer.Instance);
			_outstandingBlocks = new Queue<OutstandingBlock>();
			_outstandingBlocks.Enqueue(new OutstandingBlock(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(statement, 0)), startBlockName, ExitLabelName));

			var result = new List<LabelledBlock>();

			while (_outstandingBlocks.Count > 0) {
				var current = _outstandingBlocks.Dequeue();
				Console.WriteLine("Dequeue " + current.Stack.Peek().Block.Statements[current.Stack.Peek().Index].DebugToString());
				result.Add(new LabelledBlock(current.Name, Handle(current.Stack, true, current.Name, current.ReturnLabel)));
			}

			return result;
		}

		private List<JsStatement> Handle(ImmutableStack<StackEntry> stack, bool dequeued, string blockName, string returnLabel) {
			var currentBlock = new List<JsStatement>();
			bool first = true;
			while (!stack.IsEmpty) {
				var tos = stack.Peek();
				stack = stack.Pop();

				var stmt = tos.Block.Statements[tos.Index];
				var lbl = stmt as JsLabelledStatement;
				if (lbl != null) {
					if (dequeued && first) {
						// First statement in the new block
						stmt = lbl.Statement;
					}
					else {
						// A label that terminates the current block.
						Enqueue(stack.Push(new StackEntry(tos.Block, tos.Index)), lbl.Label, returnLabel);
						if (currentBlock.Count == 0 || IsNextStatementReachable(currentBlock[currentBlock.Count - 1]))
							currentBlock.Add(new JsGotoStatement(lbl.Label));
						return currentBlock;
					}
				}

				if (ContainsLabels(stmt)) {
					if (stmt is JsBlockStatement) {
						stack = PushFollowing(stack, tos.Block, tos.Index).Push(new StackEntry((JsBlockStatement)stmt, 0));
					}
					else {
						if (stmt is JsIfStatement) {
							if (!HandleIfStatement((JsIfStatement)stmt, tos.Block, tos.Index, returnLabel, stack, blockName, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsDoWhileStatement) {
							if (!HandleDoWhileStatement((JsDoWhileStatement)stmt, tos.Block, tos.Index, returnLabel, stack, blockName, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsWhileStatement) {
							throw new NotImplementedException();
						}
						else if (stmt is JsForStatement) {
							throw new NotImplementedException();
						}
						else if (stmt is JsSwitchStatement) {
							throw new NotImplementedException();
						}
						else {
							throw new NotSupportedException("Statement " + stmt + " cannot contain labels.");
						}

						stack = PushFollowing(stack, tos.Block, tos.Index);
					}
				}
				else {
					stack = PushFollowing(stack, tos.Block, tos.Index);
					currentBlock.Add(stmt);	// No rewrites necessary in this statement.
				}

				first = false;
			}
			if (currentBlock.Count == 0 || IsNextStatementReachable(currentBlock[currentBlock.Count - 1]))
				currentBlock.Add(new JsGotoStatement(returnLabel));

			return currentBlock;
		}

		private Tuple<string, bool> GetLabelAfterStatement(JsBlockStatement parent, int index, string returnLabel) {
			if (index < parent.Statements.Count - 1) {
				if (parent.Statements[index + 1] is JsLabelledStatement) {
					return Tuple.Create((parent.Statements[index + 1] as JsLabelledStatement).Label, false);
				}
				else {
					return Tuple.Create(CreateAnonymousBlockName(), true);
				}
			}
			else {
				return Tuple.Create(returnLabel, false);
			}
		}

		private bool HandleIfStatement(JsIfStatement stmt, JsBlockStatement parent, int index, string returnLabel, ImmutableStack<StackEntry> stack, string blockName, IList<JsStatement> currentBlock) {
			var labelAfter = GetLabelAfterStatement(parent, index, returnLabel);

			var thenPart = Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Then, 0)), false, blockName, labelAfter.Item1);
			var elsePart = stmt.Else != null ? Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Else, 0)), false, blockName, labelAfter.Item1) : null;

			currentBlock.Add(new JsIfStatement(stmt.Test, new JsBlockStatement(thenPart), elsePart != null ? new JsBlockStatement(elsePart) : null));
			if (elsePart == null)
				currentBlock.Add(new JsGotoStatement(labelAfter.Item1));

			if (labelAfter.Item2) {
				Enqueue(PushFollowing(stack, parent, index), labelAfter.Item1, returnLabel);
				return false;
			}

			return true;
		}

		private bool HandleDoWhileStatement(JsDoWhileStatement stmt, JsBlockStatement parent, int index, string returnLabel, ImmutableStack<StackEntry> stack, string blockName, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0) {
				// We have to create a new block for the statement.
				var lbl = parent.Statements[index] as JsLabelledStatement;
				string topOfLoopLabelName = lbl != null ? lbl.Label : CreateAnonymousBlockName();
				Enqueue(stack.Push(new StackEntry(parent, index)), topOfLoopLabelName, returnLabel);
				currentBlock.Add(new JsGotoStatement(topOfLoopLabelName));
				return false;
			}
			else {
				string beforeConditionLabel = CreateAnonymousBlockName();
				JsBlockStatement body;
				Tuple<string, bool> afterLoopLabel;
				if (ContainsBreak(stmt.Body)) {
					afterLoopLabel = GetLabelAfterStatement(parent, index, returnLabel);
					body = new ReplaceBreakWithGotoVisitor().Process(stmt.Body, afterLoopLabel.Item1);
				}
				else {
					afterLoopLabel = Tuple.Create(returnLabel, false);
					body = stmt.Body;
				}
				body = new ReplaceContinueWithGotoVisitor().Process(body, beforeConditionLabel);

				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(body, 0)), false, blockName, beforeConditionLabel));

				if (afterLoopLabel.Item2) {
					Enqueue(PushFollowing(stack, parent, index), afterLoopLabel.Item1, returnLabel);
					Enqueue(stack.Push(new StackEntry(new JsBlockStatement(new JsIfStatement(stmt.Condition, new JsGotoStatement(blockName), null)), 0)), beforeConditionLabel, afterLoopLabel.Item1);
				}
				else {
					Enqueue(PushFollowing(stack, parent, index).Push(new StackEntry(new JsBlockStatement(new JsIfStatement(stmt.Condition, new JsGotoStatement(blockName), null)), 0)), beforeConditionLabel, returnLabel);
				}

				return false;
			}
		}
	}
}
