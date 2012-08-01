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
	internal class LabelledBlockGatherer {
		internal const string ExitLabelName = "$exit";	// Use this label as a name to denote that when control leaves a block through this path, it means that it is leaving the current state machine.

		class ContainsLabelsVisitor : RewriterVisitorBase<object> {
			bool _result;

			public override JsStatement Visit(JsFunctionStatement statement, object data) {
				return statement;
			}

			public override JsExpression Visit(JsFunctionDefinitionExpression expression, object data) {
				return expression;
			}

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
		HashSet<string> _processedLabels = new HashSet<string>();

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
			if (_processedLabels.Contains(name))
				throw new InvalidOperationException("Duplicate enqueueing of " + name);
			_processedLabels.Add(name);
			if (stack.IsEmpty)
				throw new InvalidOperationException("Empty stack for label " + name);
			var tos = stack.Peek();
			Console.WriteLine("Enqueue " + tos.Block.Statements[tos.Index].DebugToString());
			_outstandingBlocks.Enqueue(new OutstandingBlock(stack, name, exitLabel));
		}

		private bool ContainsLabels(JsStatement statement) {
			return new ContainsLabelsVisitor().Process(statement);
		}

		private bool ContainsBreak(JsStatement statement) {
			return new ContainsBreakVisitor().Process(statement);
		}

		private ImmutableStack<StackEntry> PushFollowing(ImmutableStack<StackEntry> stack, JsBlockStatement block, int currentIndex) {
			return currentIndex < block.Statements.Count - 1 ? stack.Push(new StackEntry(block, currentIndex + 1)) : stack;
		}

		public IList<LabelledBlock> Gather(JsBlockStatement statement) {
			var startBlockName = CreateAnonymousBlockName();
			_outstandingBlocks = new Queue<OutstandingBlock>();
			_outstandingBlocks.Enqueue(new OutstandingBlock(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(statement, 0)), startBlockName, ExitLabelName));

			var result = new List<LabelledBlock>();

			while (_outstandingBlocks.Count > 0) {
				var current = _outstandingBlocks.Dequeue();
				Console.WriteLine("Dequeue " + current.Stack.Peek().Block.Statements[current.Stack.Peek().Index].DebugToString());
				result.Add(new LabelledBlock(current.Name, Handle(current.Stack, current.Name, current.ReturnLabel)));
			}

			return result;
		}

		private List<JsStatement> Handle(ImmutableStack<StackEntry> stack, string blockName, string returnLabel) {
			var currentBlock = new List<JsStatement>();
			while (!stack.IsEmpty) {
				var tos = stack.Peek();
				stack = stack.Pop();

				var stmt = tos.Block.Statements[tos.Index];
				var lbl = stmt as JsLabelledStatement;
				if (lbl != null) {
					if (_processedLabels.Contains(lbl.Label)) {
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
							if (!HandleIfStatement((JsIfStatement)stmt, tos, returnLabel, stack, blockName, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsDoWhileStatement) {
							if (!HandleDoWhileStatement((JsDoWhileStatement)stmt, tos, returnLabel, stack, blockName, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsWhileStatement) {
							if (!HandleWhileStatement((JsWhileStatement)stmt, tos, returnLabel, stack, blockName, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsForStatement) {
							if (!HandleForStatement((JsForStatement)stmt, tos, returnLabel, stack, blockName, currentBlock))
								return currentBlock;
						}
						else if (stmt is JsSwitchStatement) {
							if (!HandleSwitchStatement((JsSwitchStatement)stmt, tos, returnLabel, stack, blockName, currentBlock))
								return currentBlock;
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

		private bool HandleIfStatement(JsIfStatement stmt, StackEntry location, string returnLabel, ImmutableStack<StackEntry> stack, string blockName, IList<JsStatement> currentBlock) {
			var labelAfter = GetLabelAfterStatement(location.Block, location.Index, returnLabel);

			var thenPart = Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Then, 0)), blockName, labelAfter.Item1);
			var elsePart = stmt.Else != null ? Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(stmt.Else, 0)), blockName, labelAfter.Item1) : null;

			currentBlock.Add(new JsIfStatement(stmt.Test, new JsBlockStatement(thenPart), elsePart != null ? new JsBlockStatement(elsePart) : null));
			if (elsePart == null)
				currentBlock.Add(new JsGotoStatement(labelAfter.Item1));

			if (labelAfter.Item2) {
				Enqueue(PushFollowing(stack, location.Block, location.Index), labelAfter.Item1, returnLabel);
				return false;
			}

			return true;
		}

		private bool HandleDoWhileStatement(JsDoWhileStatement stmt, StackEntry location, string returnLabel, ImmutableStack<StackEntry> stack, string blockName, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0) {
				// We have to create a new block for the statement.
				string topOfLoopLabelName = CreateAnonymousBlockName();
				Enqueue(stack.Push(location), topOfLoopLabelName, returnLabel);
				currentBlock.Add(new JsGotoStatement(topOfLoopLabelName));
				return false;
			}
			else {
				string beforeConditionLabel = CreateAnonymousBlockName();
				JsBlockStatement body;
				Tuple<string, bool> afterLoopLabel;
				if (ContainsBreak(stmt.Body)) {
					afterLoopLabel = GetLabelAfterStatement(location.Block, location.Index, returnLabel);
					body = new ReplaceBreakWithGotoVisitor().Process(stmt.Body, afterLoopLabel.Item1);
				}
				else {
					afterLoopLabel = Tuple.Create(returnLabel, false);
					body = stmt.Body;
				}
				body = new ReplaceContinueWithGotoVisitor().Process(body, beforeConditionLabel);

				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(body, 0)), blockName, beforeConditionLabel));

				if (afterLoopLabel.Item2) {
					Enqueue(PushFollowing(stack, location.Block, location.Index), afterLoopLabel.Item1, returnLabel);
					Enqueue(stack.Push(new StackEntry(new JsBlockStatement(new JsIfStatement(stmt.Condition, new JsGotoStatement(blockName), null)), 0)), beforeConditionLabel, afterLoopLabel.Item1);
				}
				else {
					Enqueue(PushFollowing(stack, location.Block, location.Index).Push(new StackEntry(new JsBlockStatement(new JsIfStatement(stmt.Condition, new JsGotoStatement(blockName), null)), 0)), beforeConditionLabel, returnLabel);
				}

				return false;
			}
		}

		private bool HandleWhileStatement(JsWhileStatement stmt, StackEntry location, string returnLabel, ImmutableStack<StackEntry> stack, string blockName, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0) {
				// We have to create a new block for the statement.
				string topOfLoopLabelName = CreateAnonymousBlockName();
				Enqueue(stack.Push(location), topOfLoopLabelName, returnLabel);
				currentBlock.Add(new JsGotoStatement(topOfLoopLabelName));
				return false;
			}
			else {
				var afterLoopLabel = GetLabelAfterStatement(location.Block, location.Index, returnLabel);

				currentBlock.Add(new JsIfStatement(JsExpression.LogicalNot(stmt.Condition), new JsGotoStatement(afterLoopLabel.Item1), null));
				var body = new ReplaceBreakWithGotoVisitor().Process(stmt.Body, afterLoopLabel.Item1);
				body = new ReplaceContinueWithGotoVisitor().Process(body, blockName);
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(body, 0)), blockName, blockName));

				if (location.Index < location.Block.Statements.Count - 1) {
					Enqueue(PushFollowing(stack, location.Block, location.Index), afterLoopLabel.Item1, returnLabel);
				}

				return false;
			}
		}

		private bool HandleForStatement(JsForStatement stmt, StackEntry location, string returnLabel, ImmutableStack<StackEntry> stack, string blockName, IList<JsStatement> currentBlock) {
			if (currentBlock.Count > 0 || (!(stmt.InitStatement is JsEmptyStatement) && !location.AfterForInitializer)) {
				// We have to create a new block for the statement.
				string topOfLoopLabelName = CreateAnonymousBlockName();
				Enqueue(stack.Push(new StackEntry(location.Block, location.Index, true)), topOfLoopLabelName, returnLabel);
				if (!(stmt.InitStatement is JsEmptyStatement))
					currentBlock.Add(stmt.InitStatement);
				currentBlock.Add(new JsGotoStatement(topOfLoopLabelName));
				return false;
			}
			else {
				var iteratorLabel = (stmt.IteratorExpression != null ? CreateAnonymousBlockName() : blockName);
				var afterLoopLabel = GetLabelAfterStatement(location.Block, location.Index, returnLabel);

				if (stmt.ConditionExpression != null)
					currentBlock.Add(new JsIfStatement(JsExpression.LogicalNot(stmt.ConditionExpression), new JsGotoStatement(afterLoopLabel.Item1), null));
				var body = new ReplaceBreakWithGotoVisitor().Process(stmt.Body, afterLoopLabel.Item1);
				body = new ReplaceContinueWithGotoVisitor().Process(body, iteratorLabel);
				currentBlock.AddRange(Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(body, 0)), blockName, iteratorLabel));

				if (stmt.IteratorExpression != null) {
					Enqueue(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(JsBlockStatement.MakeBlock(new JsExpressionStatement(stmt.IteratorExpression)), 0)), iteratorLabel, blockName);
				}

				if (location.Index < location.Block.Statements.Count - 1) {
					Enqueue(PushFollowing(stack, location.Block, location.Index), afterLoopLabel.Item1, returnLabel);
				}

				return false;
			}
		}

		private bool HandleSwitchStatement(JsSwitchStatement stmt, StackEntry location, string returnLabel, ImmutableStack<StackEntry> stack, string blockName, IList<JsStatement> currentBlock) {
			var labelAfter = GetLabelAfterStatement(location.Block, location.Index, returnLabel);
			JsExpression expression = stmt.Expression;

			var clauses = new List<Tuple<JsExpression, JsBlockStatement>>();
			JsStatement defaultClause = null;
			string currentFallthroughLabel = null;
			for (int i = 0; i < stmt.Clauses.Count; i++) {
				var clause = stmt.Clauses[i];

				var origBody = new List<JsStatement>();
				origBody.AddRange(clause.Body.Statements);
				if (currentFallthroughLabel != null && (origBody.Count == 0 || !(origBody[0] is JsLabelledStatement)))
					origBody[0] = new JsLabelledStatement(currentFallthroughLabel, origBody.Count > 0 ? origBody[0] : new JsEmptyStatement());

				if (origBody.Count > 0 && origBody[origBody.Count - 1] is JsBreakStatement) {	// TODO: Also check if it has a label that causes it to reference something else (but we don't generate those kinds of labels, at least not currently).
					// Remove break statements that come last in the clause - they are unnecessary since we use if/else if/else
					origBody.RemoveAt(origBody.Count - 1);
					currentFallthroughLabel = null;
				}
				else if (i < stmt.Clauses.Count - 1) {
					// Fallthrough
					var nextBody = stmt.Clauses[i + 1].Body.Statements;
					if (nextBody.Count > 0 && nextBody[0] is JsLabelledStatement)
						currentFallthroughLabel = ((JsLabelledStatement)nextBody[0]).Label;
					else
						currentFallthroughLabel = CreateAnonymousBlockName();

					origBody.Add(new JsGotoStatement(currentFallthroughLabel));
				}

				var b = new ReplaceBreakWithGotoVisitor().Process(new JsBlockStatement(origBody), labelAfter.Item1);
				var body = Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(b, 0)), blockName, labelAfter.Item1);
				if (body.Count > 0 && body[body.Count - 1] is JsGotoStatement && ((JsGotoStatement)body[body.Count - 1]).TargetLabel == labelAfter.Item1)
					body.RemoveAt(body.Count - 1);	// If the last statement says to go to after this statement, it can safely be ignored because we use if/else if/else.

				if (clause.Values.Any(v => v == null)) {
					defaultClause = new JsBlockStatement(body);
				}
				else {
					JsExpression test = clause.Values.Select(v => JsExpression.Same(expression, v)).Aggregate((o, e) => o != null ? JsExpression.LogicalOr(o, e) : e);
					clauses.Add(Tuple.Create(test, new JsBlockStatement(body)));
				}
			}
			clauses.Reverse();

			currentBlock.Add(clauses.Where(c => c.Item1 != null).Aggregate(defaultClause, (o, n) => new JsIfStatement(n.Item1, n.Item2, o)));
			currentBlock.Add(new JsGotoStatement(labelAfter.Item1));

			if (labelAfter.Item2) {
				Enqueue(PushFollowing(stack, location.Block, location.Index), labelAfter.Item1, returnLabel);
				return false;
			}

			return true;
		}
	}
}
