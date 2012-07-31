using System;
using System.Collections.Generic;
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
			return !(current is JsReturnStatement || current is JsGotoStatement);
		}

		private void Enqueue(JsBlockStatement block, int i, string label, string returnLabel) {
			if (_processedStatements.Contains(block.Statements[i]))
				return;
			Console.WriteLine("Enqueue " + block.Statements[i].DebugToString());
			_outstandingBlocks.Enqueue(Tuple.Create(_currentStack.Push(Tuple.Create(block, i)), label, returnLabel));
			_processedStatements.Add(block.Statements[i]);
		}

		private void EnqueueScanOnly(JsBlockStatement block, int i) {
			if (_processedStatements.Contains(block.Statements[i]))
				return;
			Console.WriteLine("Enqueue " + block.Statements[i].DebugToString());
			_outstandingBlocks.Enqueue(Tuple.Create(_currentStack.Push(Tuple.Create(block, i)), (string)null, (string)null));
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
//			var referenced = new FindReferencedLabelsVisitor().FindReferencedLabels(result.SelectMany(b => b.Statements));
//			referenced.Add(startBlockName);
//			result.RemoveAll(b => !referenced.Contains(b.Name));

			return result;
		}
/*
		private void NewBlock(string name = null, bool connect = false) {
			string newName = name ?? string.Format(CultureInfo.InvariantCulture, "${0}", _currentAnonymousBlockIndex++);
			if (_currentBlock != null) {
				if (connect)
					_currentBlock.Add(new JsGotoStatement(newName));
				_result.Add(new LabelledBlock(_currentBlockName, _currentBlock));
			}
			_currentBlock = new List<JsStatement>();
			_currentBlockName = newName;
		}

		public IList<LabelledBlock> Gather(JsStatement statement) {
			_result = new List<LabelledBlock>();
			_currentBlock = null;
			NewBlock(null);
			bool reachable = Visit(statement, LabelledBlock.ExitLabelName);
			if (reachable)
				_currentBlock.Add(new JsGotoStatement(LabelledBlock.ExitLabelName));
			_result.Add(new LabelledBlock(_currentBlockName, _currentBlock));
			return _result;
		}
		*/
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
			return false;
		}

		public bool Visit(JsDoWhileStatement statement, string data) {
			throw new NotImplementedException();
		}

		public bool Visit(JsForEachInStatement statement, string data) {
			throw new NotImplementedException();
		}

		public bool Visit(JsForStatement statement, string data) {
			throw new NotImplementedException();
		}

		public bool Visit(JsIfStatement statement, string data) {
			throw new NotImplementedException();
		}

		public bool Visit(JsSwitchStatement statement, string data) {
			throw new NotImplementedException();
		}

		public bool Visit(JsThrowStatement statement, string data) {
			throw new NotImplementedException();
		}

		public bool Visit(JsTryCatchFinallyStatement statement, string data) {
			throw new NotImplementedException();
		}

		public bool Visit(JsWhileStatement statement, string data) {
			throw new NotImplementedException();
		}

		public bool Visit(JsWithStatement statement, string data) {
			throw new NotImplementedException();
		}

		public bool Visit(JsLabelledStatement statement, string data) {
			throw new InvalidOperationException("JsLabelledStatement should be handled above.");
		}

		public bool Visit(JsGotoStatement statement, string data) {
			_currentBlock.Add(statement);
			return false;
		}

		public bool Visit(JsFunctionStatement statement, string data) {
			throw new NotImplementedException();
		}

		public bool Visit(JsYieldReturnStatement statement, string data) {
			throw new NotSupportedException();
		}

		public bool Visit(JsYieldBreakStatement statement, string data) {
			throw new NotSupportedException();
		}

		public bool Visit(JsComment statement, string data) {
			_currentBlock.Add(statement);
			return false;
		}

		public bool Visit(JsBreakStatement statement, string data) {
			_currentBlock.Add(statement);
			return false;
		}

		public bool Visit(JsContinueStatement statement, string data) {
			_currentBlock.Add(statement);
			return false;
		}

		public bool Visit(JsEmptyStatement statement, string data) {
			_currentBlock.Add(statement);
			return false;
		}

		public bool Visit(JsExpressionStatement statement, string data) {
			_currentBlock.Add(statement);
			return false;
		}

		public bool Visit(JsReturnStatement statement, string data) {
			_currentBlock.Add(statement);
			return false;
		}

		public bool Visit(JsVariableDeclarationStatement statement, string data) {
			_currentBlock.Add(statement);
			return false;
		}
	}
}
