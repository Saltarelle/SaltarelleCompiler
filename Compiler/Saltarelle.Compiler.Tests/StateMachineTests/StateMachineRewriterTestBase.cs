using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
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
		protected void AssertCorrect(string orig, string expected, MethodType methodType = MethodType.Normal) {
			int tempIndex = 0, stateIndex = 0, loopLabelIndex = 0;
			var stmt = JsStatement.EnsureBlock(JavaScriptParser.Parser.ParseStatement(orig, allowCustomKeywords: true));
			JsBlockStatement result;
			if (methodType == MethodType.Iterator) {
				int finallyHandlerIndex = 0;
				result = StateMachineRewriter.RewriteIteratorBlock(stmt, e => e.NodeType != ExpressionNodeType.Identifier, () => "$tmp" + (++tempIndex).ToString(CultureInfo.InvariantCulture), () => "$state" + (++stateIndex).ToString(CultureInfo.InvariantCulture), () => string.Format("$loop" + (++loopLabelIndex).ToString(CultureInfo.InvariantCulture)), () => string.Format("$finally" + (++finallyHandlerIndex).ToString(CultureInfo.InvariantCulture)), v => JsExpression.Invocation(JsExpression.Identifier("setCurrent"), v), sm => {
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
				                                                 expr => { if (methodType != MethodType.AsyncTask) throw new InvalidOperationException("Should not set result in async void method"); return JsExpression.Invocation(JsExpression.Member(JsExpression.Identifier("$tcs"), "setResult"), expr ?? JsExpression.String("<<null>>")); },
				                                                 expr => { if (methodType != MethodType.AsyncTask) throw new InvalidOperationException("Should not set exception in async void method"); return JsExpression.Invocation(JsExpression.Member(JsExpression.Identifier("$tcs"), "setException"), expr); },
				                                                 ()   => { if (methodType != MethodType.AsyncTask) throw new InvalidOperationException("Should not get task async void method"); return JsExpression.Invocation(JsExpression.Member(JsExpression.Identifier("$tcs"), "getTask")); },
				                                                 (sm, ctx) => JsExpression.Invocation(JsExpression.Identifier("$Bind"), sm, ctx));
			}
			else {
				result = StateMachineRewriter.RewriteNormalMethod(stmt, e => e.NodeType != ExpressionNodeType.Identifier, () => "$tmp" + (++tempIndex).ToString(CultureInfo.InvariantCulture), () => "$state" + (++stateIndex).ToString(CultureInfo.InvariantCulture), () => string.Format("$loop" + (++loopLabelIndex).ToString(CultureInfo.InvariantCulture)));
			}
			var actual = OutputFormatter.Format(result);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}
	}
}
