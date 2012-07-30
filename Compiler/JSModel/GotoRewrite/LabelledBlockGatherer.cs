using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.GotoRewrite {
	internal class LabelledBlockGatherer : IStatementVisitor<bool, string> {
		int _currentAnonymousBlockIndex;
		List<LabelledBlock> _result;
		List<JsStatement> _currentBlock;
		string _currentBlockName;

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

		public bool Visit(JsStatement statement, string data) {
			return statement.Accept(this, data);
		}

		public bool Visit(JsBlockStatement statement, string data) {
			var stmts = statement.Statements;
			bool reachable = true;
			for (int i = 0; i < stmts.Count; i++) {
				var lbl = stmts[i] as JsLabelStatement;
				if (lbl != null) {
					NewBlock(lbl.Name, reachable);
					reachable = true;
				}
				else {
					reachable = Visit(stmts[i], data);
				}
			}
			return reachable;
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

		public bool Visit(JsLabelStatement statement, string data) {
			throw new InvalidOperationException("JsLabelStatement should be handled above.");
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
			return true;
		}

		public bool Visit(JsBreakStatement statement, string data) {
			_currentBlock.Add(statement);
			return true;
		}

		public bool Visit(JsContinueStatement statement, string data) {
			_currentBlock.Add(statement);
			return true;
		}

		public bool Visit(JsEmptyStatement statement, string data) {
			_currentBlock.Add(statement);
			return true;
		}

		public bool Visit(JsExpressionStatement statement, string data) {
			_currentBlock.Add(statement);
			return true;
		}

		public bool Visit(JsReturnStatement statement, string data) {
			_currentBlock.Add(statement);
			return false;
		}

		public bool Visit(JsVariableDeclarationStatement statement, string data) {
			_currentBlock.Add(statement);
			return true;
		}
	}
}
