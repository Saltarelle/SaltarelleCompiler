using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Utils;
using Saltarelle.Compiler.JSModel.Expressions;
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

		[DebuggerDisplay("{DebugToString()}")]
		class StackEntry {
			public JsBlockStatement Block { get; private set; }
			public int Index { get; private set; }
			public string ExitLabel { get; private set; }

			public StackEntry(JsBlockStatement block, int index, string exitLabel) {
				Block = block;
				Index = index;
				ExitLabel = exitLabel;
			}

			public string DebugToString() {
				return new JsBlockStatement(Block.Statements.Skip(Index)).DebugToString();
			}
		}

		int _currentAnonymousBlockIndex;
		Queue<Tuple<ImmutableStack<StackEntry>, string>> _outstandingBlocks = new Queue<Tuple<ImmutableStack<StackEntry>, string>>();
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
			return !(current is JsReturnStatement || current is JsGotoStatement || current is JsThrowStatement);
		}

		private void Enqueue(ImmutableStack<StackEntry> stack, string label) {
			var tos = stack.Peek();
			if (_processedStatements.Contains(tos.Block.Statements[tos.Index]))
				throw new Exception("Don't think this will happen");
			Console.WriteLine("Enqueue " + tos.Block.Statements[tos.Index].DebugToString());
			_outstandingBlocks.Enqueue(Tuple.Create(stack, label));
			_processedStatements.Add(tos.Block.Statements[tos.Index]);
		}

		private bool ContainsLabels(JsStatement statement) {
			return new ContainsLabelsVisitor().Process(statement);
		}

		public IList<LabelledBlock> Gather(JsBlockStatement statement) {
			var startBlockName = CreateAnonymousBlockName();
			_processedStatements = new HashSet<JsStatement>(ReferenceComparer.Instance);
			_outstandingBlocks = new Queue<Tuple<ImmutableStack<StackEntry>, string>>();
			_outstandingBlocks.Enqueue(Tuple.Create(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(statement, 0, LabelledBlock.ExitLabelName)), startBlockName));

			var result = new List<LabelledBlock>();

			while (_outstandingBlocks.Count > 0) {
				var current = _outstandingBlocks.Dequeue();
				Console.WriteLine("Dequeue " + current.Item1.Peek().Block.Statements[current.Item1.Peek().Index].DebugToString());
				result.Add(new LabelledBlock(current.Item2, Handle(current.Item1, true)));
			}

			return result;
		}

		private List<JsStatement> Handle(ImmutableStack<StackEntry> stack, bool dequeued) {
			var currentBlock = new List<JsStatement>();
			bool first = true;
			string exitLabel = LabelledBlock.ExitLabelName;
			while (!stack.IsEmpty) {
				var tos = stack.Peek();
				stack = stack.Pop();
				exitLabel = tos.ExitLabel;

				var stmt = tos.Block.Statements[tos.Index];
				var lbl = stmt as JsLabelledStatement;
				if (lbl != null) {
					if (dequeued && first) {
						// First statement in the new block
						stmt = lbl.Statement;
					}
					else {
						// A label that terminates the current block.
						Enqueue(stack.Push(new StackEntry(tos.Block, tos.Index, tos.ExitLabel)), lbl.Label);
						if (currentBlock.Count == 0 || IsNextStatementReachable(currentBlock[currentBlock.Count - 1]))
							currentBlock.Add(new JsGotoStatement(lbl.Label));
						return currentBlock;
					}
				}

				if (tos.Index < tos.Block.Statements.Count - 1)
					stack = stack.Push(new StackEntry(tos.Block, tos.Index + 1, tos.ExitLabel));

				if (ContainsLabels(stmt)) {
					if (stmt is JsBlockStatement) {
						stack = stack.Push(new StackEntry((JsBlockStatement)stmt, 0, tos.ExitLabel));
					}
					else {
						var ifst = (JsIfStatement)stmt;
						string labelAfter;
						bool needInsertLabelAfter = false;
						if (tos.Index < tos.Block.Statements.Count - 1) {
							if (tos.Block.Statements[tos.Index + 1] is JsLabelledStatement) {
								labelAfter = (tos.Block.Statements[tos.Index + 1] as JsLabelledStatement).Label;
							}
							else {
								labelAfter = CreateAnonymousBlockName();
								needInsertLabelAfter = true;
							}
						}
						else {
							labelAfter = tos.ExitLabel;
						}

						var thenPart = Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(ifst.Then, 0, labelAfter)), false);
						var elsePart = ifst.Else != null ? Handle(ImmutableStack<StackEntry>.Empty.Push(new StackEntry(ifst.Else, 0, labelAfter)), false) : null;

						currentBlock.Add(new JsIfStatement(ifst.Test, new JsBlockStatement(thenPart), elsePart != null ? new JsBlockStatement(elsePart) : null));
						if (elsePart == null)
							currentBlock.Add(new JsGotoStatement(exitLabel));

						if (needInsertLabelAfter) {
							Enqueue(stack, labelAfter);
							return currentBlock;
						}
					}
				}
				else {
					currentBlock.Add(stmt);	// No rewrites necessary in this statement.
				}

				first = false;
			}
			if (currentBlock.Count == 0 || IsNextStatementReachable(currentBlock[currentBlock.Count - 1]))
				currentBlock.Add(new JsGotoStatement(exitLabel));

			return currentBlock;
		}
/*
		private bool HandleBlock(JsBlockStatement statement, string exitLabel) {
			var stmts = statement.Statements;
			for (int i = 0; i < stmts.Count; i++) {
				var lbl = stmts[i] as JsLabelledStatement;
				JsStatement stmt;
				if (lbl != null) {
				}
				else {
					stmt = stmts[i];	// Not a labelled statement
				}

				if (ContainsLabels(stmt)) {
					_currentStack = _currentStack.Push(new StackEntry(statement, i + 1, exitLabel));
					bool didRewrite = Visit(stmt, data);
					_currentStack = _currentStack.Pop();
					if (didRewrite) {
						if (i < stmts.Count - 1) {
							string label = (stmts[i + 1] is JsLabelledStatement ? ((JsLabelledStatement)stmts[i + 1]).Label : CreateAnonymousBlockName());
							Enqueue(statement, i + 1, label, data);
						}
						return true;
					}
				}
				else {
					_currentBlock.Add(stmt);
				}
			}
		}
		*

		public bool Visit(JsBlockStatement statement, string data) {
		}*/
	}
}

#if false
	internal class LabelledBlockGatherer : IStatementVisitor<bool, string> {
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

		class FindReferencedLabelsVisitor : RewriterVisitorBase<object> {
			HashSet<string> _result;

			public override JsStatement Visit(JsGotoStatement statement, object data) {
				_result.Add(statement.TargetLabel);
				return statement;
			}

			public HashSet<string> FindReferencedLabels(IEnumerable<JsStatement> statements) {
				_result = new HashSet<string>();
				foreach (var s in statements)
					Visit(s, null);
				return _result;
			}
		}
/*
		class FindLabelLocationsVisitor : RewriterVisitorBase<object> {
			private ImmutableStack<Tuple<JsBlockStatement, int>> _currentStack;
			private List<Tuple<ImmutableStack<Tuple<JsBlockStatement, int>>, string>> _result;

			public override JsStatement Visit(JsBlockStatement statement, object data) {
				_currentStack = _currentStack.Push(Tuple.Create(statement, 0));
				try {
					var stmts = statement.Statements;
					for (int i = 0; i < stmts.Count; i++) {
						_currentStack = _currentStack.Pop().Push(Tuple.Create(statement, i + 1));

						var lbl = stmts[i] as JsLabelStatement;
						if (lbl != null) {
							_result.Add(Tuple.Create(_currentStack.Pop().Push(Tuple.Create(statement, i + 1)), lbl.Name));
						}
						else {
							_currentStack = _currentStack.Pop().Push(Tuple.Create(statement, i + 1));
							Visit(stmts[i], data);
						}
					}
					return statement;
				}
				finally {
					_currentStack = _currentStack.Pop();
				}
			}

			public List<Tuple<ImmutableStack<Tuple<JsBlockStatement, int>>, string>> Process(JsBlockStatement statement) {
				_currentStack = ImmutableStack<Tuple<JsBlockStatement, int>>.Empty;
				_result = new List<Tuple<ImmutableStack<Tuple<JsBlockStatement, int>>, string>>();
				Visit(statement, null);
				return _result;
			}
		}
		*/
		int _currentAnonymousBlockIndex;
		List<JsStatement> _currentBlock;
		string _currentBlockName;
		ImmutableStack<Tuple<JsBlockStatement, int>> _currentStack;
		Queue<Tuple<ImmutableStack<Tuple<JsBlockStatement, int>>, string, string>> _outstandingBlocks = new Queue<Tuple<ImmutableStack<Tuple<JsBlockStatement, int>>, string, string>>();
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
			return !(current is JsReturnStatement || current is JsGotoStatement || current is JsThrowStatement);
		}

		private void Enqueue(JsBlockStatement block, int i, string label, string returnLabel) {
			if (_processedStatements.Contains(block.Statements[i]))
				return;
			Console.WriteLine("Enqueue " + block.Statements[i].DebugToString());
			_outstandingBlocks.Enqueue(Tuple.Create(_currentStack.Push(Tuple.Create(block, i)), label, returnLabel));
			_processedStatements.Add(block.Statements[i]);
		}

		public IList<LabelledBlock> Gather(JsBlockStatement statement) {
			var startBlockName = CreateAnonymousBlockName();
			_processedStatements = new HashSet<JsStatement>(ReferenceComparer.Instance);
			_outstandingBlocks = new Queue<Tuple<ImmutableStack<Tuple<JsBlockStatement, int>>, string, string>>();
			_outstandingBlocks.Enqueue(Tuple.Create(ImmutableStack<Tuple<JsBlockStatement, int>>.Empty.Push(Tuple.Create(statement, 0)), startBlockName, LabelledBlock.ExitLabelName));

			var result = new List<LabelledBlock>();
			int i = 0;

			while (_outstandingBlocks.Count > 0) {
				var current = _outstandingBlocks.Dequeue();
				Console.WriteLine("Dequeue " + current.Item1.Peek().Item1.Statements[current.Item1.Peek().Item2].DebugToString());

				_currentStack = current.Item1;
				_currentBlock = new List<JsStatement>();
				_currentBlockName = current.Item2;
				bool reachable = true;
				while (!_currentStack.IsEmpty) {
					var tos = _currentStack.Peek();
					_currentStack = _currentStack.Pop();
					Visit(tos.Item2 == 0 ? tos.Item1 : new JsBlockStatement(tos.Item1.Statements.Skip(tos.Item2)), current.Item3);
					reachable = IsNextStatementReachable(_currentBlock[_currentBlock.Count - 1]);
					if (!reachable)
						break;	// No need to check the rest of the stack since we can never get there.
				}
				if (reachable)
					_currentBlock.Add(new JsGotoStatement(LabelledBlock.ExitLabelName));
				result.Add(new LabelledBlock(_currentBlockName, _currentBlock));

				if (i++ > 1000)
					throw new Exception("Internal error, infinite loop in LabelledBlockGatherer");
			}

			// The visitor sometimes creates some unnecessary blocks - remove them.
			var referenced = new FindReferencedLabelsVisitor().FindReferencedLabels(result.SelectMany(b => b.Statements));
			referenced.Add(startBlockName);
			result.RemoveAll(b => !referenced.Contains(b.Name));

			return result;
		}

		public bool Visit(JsStatement statement, string data) {
			return statement.Accept(this, data);
		}

		public bool Visit(JsBlockStatement statement, string data) {
			var stmts = statement.Statements;
			for (int i = 0; i < stmts.Count; i++) {
				var lbl = stmts[i] as JsLabelledStatement;
				JsStatement stmt;
				if (lbl != null) {
					if (lbl.Label != _currentBlockName) {
						// A label that terminates the current block.
						Enqueue(statement, i, lbl.Label, data);
						if (_currentBlock.Count == 0 || IsNextStatementReachable(_currentBlock[_currentBlock.Count - 1]))
							_currentBlock.Add(new JsGotoStatement(lbl.Label));
						return true;
					}
					else {
						// First statement in the new block
						stmt = lbl.Statement;
					}
				}
				else {
					stmt = stmts[i];	// Not a labelled statement
				}

				if (new ContainsLabelsVisitor().Process(stmt)) {
					_currentStack = _currentStack.Push(Tuple.Create(statement, i + 1));
					bool didRewrite = Visit(stmt, data);
					_currentStack = _currentStack.Pop();
					if (didRewrite) {
						if (i < stmts.Count - 1) {
							string label = (stmts[i + 1] is JsLabelledStatement ? ((JsLabelledStatement)stmts[i + 1]).Label : CreateAnonymousBlockName());
							Enqueue(statement, i + 1, label, data);
						}
						return true;
					}
				}
				else {
					_currentBlock.Add(stmt);
				}
			}
			return false;
		}

		public bool Visit(JsIfStatement statement, string data) {
			var oldStack = _currentStack;
			try {
				if (statement.Else == null) {
				}
			}
			finally {
				_currentStack = oldStack;
			}
		}

		public bool Visit(JsFunctionStatement statement, string data) {
			_currentBlock.Add(statement);
			return false;
		}

		public bool Visit(JsDoWhileStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsForEachInStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsForStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsSwitchStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsTryCatchFinallyStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsWhileStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsWithStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsLabelledStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsGotoStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsYieldReturnStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsYieldBreakStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsComment statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsThrowStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsBreakStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsContinueStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsEmptyStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsExpressionStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsReturnStatement statement, string data) {
			throw new InvalidOperationException();
		}

		public bool Visit(JsVariableDeclarationStatement statement, string data) {
			_currentBlock.Add(statement);
			return false;
		}
	}
}
#endif