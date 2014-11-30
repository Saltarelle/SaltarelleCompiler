using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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
			private readonly bool _includeSourceLocations;
			private const string IdentifierPattern = "[a-zA-Z][a-zA-Z0-9]*";
			private const string IntPattern = "[0-9]+";

			private static readonly Regex _yieldRegex = new Regex(@"^\s*yield\s+(?:return\s(" + IdentifierPattern + "|" + IntPattern + @")|break)\s*$");
			private static readonly Regex _gotoRegex = new Regex(@"^\s*goto\s+(" + IdentifierPattern + @")\s*$");
			private static readonly Regex _awaitRegex = new Regex(@"^\s*await\s+(" + IdentifierPattern + @")\s*:\s*(" + IdentifierPattern + @")\s*$");
			private static readonly Regex _sourceLocationRegex = new Regex(@"^\s*@\s*([0-9]+)\s*$");

			private CustomStatementsRewriter(bool includeSourceLocations) {
				_includeSourceLocations = includeSourceLocations;
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

				m = _sourceLocationRegex.Match(comment.Text);
				if (m.Success) {
					if (_includeSourceLocations) {
						int i = int.Parse(m.Groups[1].Captures[0].Value);
						return JsStatement.SequencePoint(Location.Create("file", new TextSpan(i, 1), new LinePositionSpan(new LinePosition(i, 0), new LinePosition(i, 2))));
					}
					else {
						return JsStatement.BlockMerged();
					}
				}

				return comment;
			}

			private static readonly CustomStatementsRewriter _withSourceLocationsInstance = new CustomStatementsRewriter(true);
			private static readonly CustomStatementsRewriter _withoutSourceLocationsInstance = new CustomStatementsRewriter(false);
			public static JsBlockStatement ProcessWithSourceLocations(JsBlockStatement statement) {
				return (JsBlockStatement)_withSourceLocationsInstance.VisitStatement(statement, null);
			}

			public static JsBlockStatement ProcessWithoutSourceLocations(JsBlockStatement statement) {
				return (JsBlockStatement)_withoutSourceLocationsInstance.VisitStatement(statement, null);
			}
		}

		class SequencePointInserter : RewriterVisitorBase<object> {
			public override JsStatement VisitSequencePoint(JsSequencePoint sequencePoint, object data) {
				return JsStatement.Comment("@ " + (sequencePoint.Location != null ? sequencePoint.Location.GetMappedLineSpan().StartLinePosition.Line.ToString(CultureInfo.InvariantCulture) : "none"));
			}

			private static readonly SequencePointInserter _instance = new SequencePointInserter();
			public static JsBlockStatement Process(JsBlockStatement statement) {
				return (JsBlockStatement)_instance.VisitStatement(statement, null);
			}
		}

		private JsBlockStatement PerformRewrite(JsBlockStatement stmt, MethodType methodType) {
			int tempIndex = 0, stateIndex = 0, loopLabelIndex = 0;
			if (methodType == MethodType.Iterator) {
				int finallyHandlerIndex = 0;
				return StateMachineRewriter.RewriteIteratorBlock(stmt, () => "$tmp" + (++tempIndex).ToString(CultureInfo.InvariantCulture), () => "$state" + (++stateIndex).ToString(CultureInfo.InvariantCulture), () => string.Format("$loop" + (++loopLabelIndex).ToString(CultureInfo.InvariantCulture)), () => string.Format("$finally" + (++finallyHandlerIndex).ToString(CultureInfo.InvariantCulture)), v => JsExpression.Invocation(JsExpression.Identifier("setCurrent"), v), sm => {
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
				return StateMachineRewriter.RewriteAsyncMethod(stmt,
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
				return StateMachineRewriter.RewriteNormalMethod(stmt, () => "$tmp" + (++tempIndex).ToString(CultureInfo.InvariantCulture), () => "$state" + (++stateIndex).ToString(CultureInfo.InvariantCulture), () => string.Format("$loop" + (++loopLabelIndex).ToString(CultureInfo.InvariantCulture)));
			}
		}

		private static readonly Regex _sourceLocationsRegex = new Regex(@"^\t*//@ .+\n", RegexOptions.Multiline);
		protected void AssertCorrect(string orig, string expected, MethodType methodType = MethodType.Normal) {
			var parsed = JsStatement.EnsureBlock(JavaScriptParser.Parser.ParseStatement(orig, singleLineCommentsAreStatements: true));

			var withSourceLocations = SequencePointInserter.Process(PerformRewrite(CustomStatementsRewriter.ProcessWithSourceLocations(parsed), methodType));
			var actualWithSourceLocations = OutputFormatter.Format(withSourceLocations);
			Assert.That(actualWithSourceLocations.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "With source locations.\nExpected:\n" + expected + "\n\nActual:\n" + actualWithSourceLocations);

			var withoutSourceLocations = PerformRewrite(CustomStatementsRewriter.ProcessWithoutSourceLocations(parsed), methodType);
			var expectedWithoutSourceLocations = _sourceLocationsRegex.Replace(expected.Replace("\r\n", "\n"), "");
			var actualWithoutSourceLocations = OutputFormatter.Format(withoutSourceLocations);
			Assert.That(actualWithoutSourceLocations.Replace("\r\n", "\n"), Is.EqualTo(expectedWithoutSourceLocations), "Without source locations.\nExpected:\n" + expectedWithoutSourceLocations + "\n\nActual:\n" + actualWithoutSourceLocations);

		}
	}
}
