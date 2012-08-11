using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	public class IteratorStateMachine {
		public JsBlockStatement MainBlock { get; private set; }
		public ReadOnlyCollection<JsVariableDeclaration> Variables { get; private set; }
		public ReadOnlyCollection<Tuple<string, JsFunctionDefinitionExpression>> FinallyHandlers { get; private set; }
		public JsBlockStatement Disposer { get; private set; }

		public IteratorStateMachine(JsBlockStatement mainBlock, IEnumerable<JsVariableDeclaration> variables, IEnumerable<Tuple<string, JsFunctionDefinitionExpression>> finallyHandlers, JsBlockStatement disposer) {
			MainBlock       = mainBlock;
			Variables       = variables.AsReadOnly();
			FinallyHandlers = finallyHandlers.AsReadOnly();
			Disposer        = disposer;
		}
	}
}