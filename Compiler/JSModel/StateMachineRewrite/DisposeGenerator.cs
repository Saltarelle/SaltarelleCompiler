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
			return JsStatement.Switch(JsExpression.Identifier(stateVariableName),
			                          nodes.Select(n => JsStatement.SwitchSection(n.StateValues.Select(v => JsExpression.Number(v)),
			                                                JsStatement.Try(
			                                                    n.Children.Count > 0 ? (JsStatement)JsStatement.Block(GenerateBody(stateVariableName, n.Children), JsStatement.Break()) : JsStatement.Break(),
			                                                    null,
			                                                    JsExpression.Invocation(JsExpression.Member(JsExpression.Identifier(n.HandlerName), "call"), JsExpression.This)))));
		}

		public static JsBlockStatement GenerateDisposer(string stateVariableName, List<Tuple<int, List<string>>> stateFinallyHandlers) {
			if (stateFinallyHandlers.Count == 0)
				return null;

			return JsStatement.Block(
			           JsStatement.Try(
			               GenerateBody(stateVariableName, GenerateHandlerTree(stateFinallyHandlers)),
			               null,
			               JsExpression.Assign(JsExpression.Identifier(stateVariableName), JsExpression.Number(-1))
			           )
			       );
		}
	}
}
