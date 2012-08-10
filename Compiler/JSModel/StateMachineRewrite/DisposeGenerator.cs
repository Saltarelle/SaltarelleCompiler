using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	internal static class DisposeGenerator {
		private class Node {
			public string HandlerName { get; private set; }
			public List<int> StateValues { get; private set; }
			public List<Node> Children { get; private set; }

			public Node(string handlerName) {
				this.HandlerName = handlerName;
				this.StateValues = new List<int>();
				this.Children = new List<Node>();
			}
		}

		private static void AddItem(List<Node> nodes, Tuple<int, List<string>> item, int listIndex) {
			var n = nodes.SingleOrDefault(x => x.HandlerName == item.Item2[listIndex]);
			if (n == null)
				nodes.Add(n = new Node(item.Item2[listIndex]));
			n.StateValues.Add(item.Item1);
			if (listIndex < item.Item2.Count - 1)
				AddItem(n.Children, item, listIndex + 1);
		}

		private static List<Node> GenerateHandlerTree(List<Tuple<int, List<string>>> flat) {
			var result = new List<Node>();
			foreach (var item in flat) {
				AddItem(result, item, 0);
			}

			return result;
		}

		private static JsStatement GenerateBody(string stateVariableName, List<Node> nodes) {
			if (nodes.Count == 0)
				return JsBlockStatement.EmptyStatement;

			return new JsSwitchStatement(JsExpression.Identifier(stateVariableName),
			                             nodes.Select(n => new JsSwitchSection(n.StateValues.Select(v => JsExpression.Number(v)),
			                                                   new JsTryStatement(
			                                                       GenerateBody(stateVariableName, n.Children),
			                                                       null,
			                                                       new JsExpressionStatement(JsExpression.Invocation(JsExpression.Identifier(n.HandlerName)))))));
		}

		public static JsBlockStatement GenerateDisposer(string stateVariableName, List<Tuple<int, List<string>>> stateFinallyHandlers) {
			if (stateFinallyHandlers.Count == 0)
				return null;

			return new JsBlockStatement(
			           new JsTryStatement(
			               GenerateBody(stateVariableName, GenerateHandlerTree(stateFinallyHandlers)),
			               null,
			               new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(stateVariableName), JsExpression.Number(-1)))
			           )
			       );
		}
	}
}
