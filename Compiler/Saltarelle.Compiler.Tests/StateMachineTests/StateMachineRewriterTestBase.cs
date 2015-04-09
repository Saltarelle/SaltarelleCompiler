using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.StateMachineRewrite;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests.StateMachineTests {
	public enum MethodType {
		Normal,
		Iterator,
		AsyncVoid,
		AsyncTask
	}

	public class StateMachineRewriterTestBase {
		class CustomStatementsRewriter : RewriterVisitorBase<object> {
			private const string IdentifierPattern = "[a-zA-Z][a-zA-Z0-9]*";
			private const string IntPattern = "[0-9]+";

			private static readonly Regex _yieldRegex = new Regex(@"\s*yield\s+(?:return\s(" + IdentifierPattern + "|" + IntPattern + @")|break)\s*");
			private static readonly Regex _gotoRegex = new Regex(@"\s*goto\s+(" + IdentifierPattern + @")\s*");
			private static readonly Regex _awaitRegex = new Regex(@"\s*await\s+(" + IdentifierPattern + @")\s*:\s*(" + IdentifierPattern + @")\s*");

			private CustomStatementsRewriter() {
			}

			public override JsStatement VisitComment(JsComment comment, object data) {
				var m = _yieldRegex.Match(comment.Text);
				if (m.Success) {
					if (m.Groups[1].Captures.Count > 0) {
						int i;
						if (int.TryParse(m.Groups[1].Captures[0].Value, out i))
							return JsStatement.Yield(JsExpression.Number(i));
						else
							return JsStatement.Yield(JsExpression.Identifier(m.Groups[1].Captures[0].Value));
					}
					else
						return JsStatement.Yield(null);
				}

				m = _gotoRegex.Match(comment.Text);
				if (m.Success) {
					return JsStatement.Goto(m.Groups[1].Captures[0].Value);
				}

				m = _awaitRegex.Match(comment.Text);
				if (m.Success) {
					return JsStatement.Await(JsExpression.Identifier(m.Groups[1].Captures[0].Value), m.Groups[2].Captures[0].Value);
				}

				return comment;
			}

			private static readonly CustomStatementsRewriter _instance = new CustomStatementsRewriter();
			public static JsBlockStatement Process(JsBlockStatement statement) {
				return (JsBlockStatement)_instance.VisitStatement(statement, null);
			}
		}

		protected void AssertCorrect(string orig, string expected, MethodType methodType = MethodType.Normal) {
			int tempIndex = 0, stateIndex = 0, loopLabelIndex = 0;
			var stmt = JsStatement.EnsureBlock(JavaScriptParser.Parser.ParseStatement(orig, singleLineCommentsAreStatements: true));
			stmt = CustomStatementsRewriter.Process(stmt);
			JsBlockStatement result;
			if (methodType == MethodType.Iterator) {
				int finallyHandlerIndex = 0;
				result = StateMachineRewriter.RewriteIteratorBlock(stmt, e => e.NodeType != ExpressionNodeType.Identifier, () => "$tmp" + (++tempIndex).ToString(CultureInfo.InvariantCulture), () => "$state" + (++stateIndex).ToString(CultureInfo.InvariantCulture), () => string.Format("$loop" + (++loopLabelIndex).ToString(CultureInfo.InvariantCulture)), () => string.Format("$finally" + (++finallyHandlerIndex).ToString(CultureInfo.InvariantCulture)), v => JsExpression.Invoke(JsExpression.Identifier("setCurrent"), v), sm => {
					var body = new List<JsStatement>();
					if (sm.Variables.Count > 0)
						body.Add(JsStatement.Var(sm.Variables));
					body.AddRange(sm.FinallyHandlers.Select(h => (JsStatement)JsExpression.Assign(JsExpression.Identifier(h.Item1), h.Item2)));
					if (sm.Disposer != null)
						body.Add(JsExpression.Assign(JsExpression.Identifier("dispose"), JsExpression.FunctionDefinition(new string[0], sm.Disposer)));
					body.Add(sm.MainBlock);
					return JsStatement.Block(body);
				});
			}
			else if (methodType == MethodType.AsyncTask || methodType == MethodType.AsyncVoid) {
				result = StateMachineRewriter.RewriteAsyncMethod(stmt,
				                                                 e => e.NodeType != ExpressionNodeType.Identifier,
				                                                 () => "$tmp" + (++tempIndex).ToString(CultureInfo.InvariantCulture),
				                                                 () => "$state" + (++stateIndex).ToString(CultureInfo.InvariantCulture),
				                                                 () => string.Format("$loop" + (++loopLabelIndex).ToString(CultureInfo.InvariantCulture)),
				                                                 "$sm",
				                                                 "$doFinally",
				                                                 methodType == MethodType.AsyncTask ? JsStatement.Declaration("$tcs", JsExpression.New(JsExpression.Identifier("TaskCompletionSource"))) : null,
				                                                 expr => { if (methodType != MethodType.AsyncTask) throw new InvalidOperationException("Should not set result in async void method"); return JsExpression.InvokeMember(JsExpression.Identifier("$tcs"), "setResult", expr ?? JsExpression.String("<<null>>")); },
				                                                 expr => { if (methodType != MethodType.AsyncTask) throw new InvalidOperationException("Should not set exception in async void method"); return JsExpression.InvokeMember(JsExpression.Identifier("$tcs"), "setException", expr); },
				                                                 ()   => { if (methodType != MethodType.AsyncTask) throw new InvalidOperationException("Should not get task async void method"); return JsExpression.InvokeMember(JsExpression.Identifier("$tcs"), "getTask"); },
				                                                 (sm, ctx) => JsExpression.Invoke(JsExpression.Identifier("$Bind"), sm, ctx));
			}
			else {
				result = StateMachineRewriter.RewriteNormalMethod(stmt, e => e.NodeType != ExpressionNodeType.Identifier, () => "$tmp" + (++tempIndex).ToString(CultureInfo.InvariantCulture), () => "$state" + (++stateIndex).ToString(CultureInfo.InvariantCulture), () => string.Format("$loop" + (++loopLabelIndex).ToString(CultureInfo.InvariantCulture)));
			}
			var actual = OutputFormatter.Format(result);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}
	}
}
