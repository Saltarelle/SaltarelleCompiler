using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.GotoRewrite
{
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
}