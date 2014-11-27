using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests.OutputFormatterTests {
	[TestFixture]
	public class SourceLocationRecordingTests {
		private JsSequencePoint SequencePoint(int line, int col) {
			return new JsSequencePoint(Location.Create("file", new TextSpan(line, 1), new LinePositionSpan(new LinePosition(line - 1, col - 1), new LinePosition(line - 1, col))));
		}

		private class SourceMapEntry {
			public int ScriptLine { get; private set; }
			public int ScriptCol { get; private set; }
			public int SourceLine { get; private set; }
			public int SourceCol { get; private set; }

			public SourceMapEntry(int scriptLine, int scriptCol, int sourceLine, int sourceCol) {
				ScriptLine = scriptLine;
				ScriptCol = scriptCol;
				SourceLine = sourceLine;
				SourceCol = sourceCol;
			}

			protected bool Equals(SourceMapEntry other) {
				return ScriptLine == other.ScriptLine && ScriptCol == other.ScriptCol && SourceLine == other.SourceLine && SourceCol == other.SourceCol;
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((SourceMapEntry)obj);
			}

			public override int GetHashCode() {
				unchecked {
					var hashCode = ScriptLine;
					hashCode = (hashCode*397) ^ ScriptCol;
					hashCode = (hashCode*397) ^ SourceLine;
					hashCode = (hashCode*397) ^ SourceCol;
					return hashCode;
				}
			}

			public override string ToString() {
				return string.Format("ScriptLine: {0}, ScriptCol: {1}, SourceLine: {2}, SourceCol: {3}", ScriptLine, ScriptCol, SourceLine, SourceCol);
			}
		}

		private class TestRecorder : ISourceMapRecorder {
			public List<SourceMapEntry> Entries { get; private set; }

			public TestRecorder() {
				Entries = new List<SourceMapEntry>();
			}

			public void RecordLocation(int scriptLine, int scriptCol, string sourcePath, int sourceLine, int sourceCol) {
				Entries.Add(new SourceMapEntry(scriptLine, scriptCol, sourceLine, sourceCol));
			}
		}

		private void AssertCorrect(JsStatement[] statements, string expectedCode, SourceMapEntry[] expectedEntries) {
			var recorder = new TestRecorder();
			var actualCode = OutputFormatter.Format(statements, recorder);
			Assert.That(actualCode.Replace("\r\n", "\n"), Is.EqualTo(expectedCode.Replace("\r\n", "\n")));
			Assert.That(recorder.Entries, Is.EqualTo(expectedEntries));
		}

		[Test]
		public void SourceLocationsAreCorrectlyRecorded() {
			AssertCorrect(new JsStatement[] {
				SequencePoint(1, 1),
				JsExpression.Number(1),
				JsExpression.Number(2),
				SequencePoint(2, 1),
				JsStatement.Block(
					SequencePoint(3, 1),
					JsExpression.Number(3)
				),
				SequencePoint(4, 1),
				JsExpression.Number(4),
			},
			"1;\n" +
			"2;\n" +
			"{\n" +
			"\t3;\n" +
			"}\n" +
			"4;\n",
			new[] {
				new SourceMapEntry(1, 1, 1, 1),
				new SourceMapEntry(3, 1, 2, 1),
				new SourceMapEntry(4, 2, 3, 1),
				new SourceMapEntry(6, 1, 4, 1),
			});
		}

		[Test]
		public void SequencePointAtTheEndOfABlockIsCorrectlyRecorded() {
			AssertCorrect(new JsStatement[] {
				SequencePoint(1, 1),
				JsExpression.Number(1),
				SequencePoint(2, 1),
				JsStatement.Block(
					SequencePoint(3, 1),
					JsExpression.Number(2),
					SequencePoint(4, 1)
				),
			},
			"1;\n" +
			"{\n" +
			"\t2;\n" +
			"}\n",
			new[] {
				new SourceMapEntry(1, 1, 1, 1),
				new SourceMapEntry(2, 1, 2, 1),
				new SourceMapEntry(3, 2, 3, 1),
				new SourceMapEntry(4, 1, 4, 1),
			});
		}

		[Test]
		public void SequencePointWithNoLocationIsRecordedAsNoSourceLocation() {
			AssertCorrect(new JsStatement[] {
				new JsSequencePoint(null),
				JsExpression.Number(1),
			},
			"1;\n",
			new[] {
				new SourceMapEntry(1, 1, 0, 0),
			});
		}

		[Test]
		public void FunctionStatementOutputsPreviousSourceLocationAfterEndingBrace() {
			AssertCorrect(new JsStatement[] {
				JsExpression.Number(1),
				JsStatement.Function("f1", new[] { "x" }, JsStatement.Block(
					SequencePoint(1, 1),
					JsExpression.Number(2),
					SequencePoint(2, 1),
					JsStatement.Function("f2", new[] { "y" }, JsStatement.Block(
						SequencePoint(3, 1),
						JsExpression.Number(3),
						SequencePoint(4, 1)
					)),
					JsExpression.Number(4)
				)),
				JsExpression.Number(5)
			},
			"1;\n" +
			"function f1(x) {\n" +
			"\t2;\n" +
			"\tfunction f2(y) {\n" +
			"\t\t3;\n" +
			"\t}\n" +
			"\t4;\n" +
			"}\n" +
			"5;\n",
			new[] {
				new SourceMapEntry(3, 2, 1, 1),
				new SourceMapEntry(4, 2, 2, 1),
				new SourceMapEntry(5, 3, 3, 1),
				new SourceMapEntry(6, 2, 4, 1),
				new SourceMapEntry(7, 2, 2, 1),
				new SourceMapEntry(9, 1, 0, 0),
			});
		}

		[Test]
		public void SequencePointAfterFunctionStatementCausesThePreviousLocationToNotBeEmitted() {
			AssertCorrect(new JsStatement[] {
				SequencePoint(1, 1),
				JsStatement.Function("f1", new[] { "x" }, JsStatement.Block(
					SequencePoint(2, 1),
					JsExpression.Number(1)
				)),
				SequencePoint(3, 1),
				JsExpression.Number(2)
			},
			"function f1(x) {\n" +
			"\t1;\n" +
			"}\n" +
			"2;\n",
			new[] {
				new SourceMapEntry(1, 1, 1, 1),
				new SourceMapEntry(2, 2, 2, 1),
				new SourceMapEntry(4, 1, 3, 1),
			});
		}

		[Test]
		public void FunctionStatementInsertsNoLocationOnTopIfItDoesNotStartWithASequencePoint() {
			AssertCorrect(new JsStatement[] {
				SequencePoint(1, 1),
				JsStatement.Function("f1", new[] { "x" }, JsStatement.Block(
					JsExpression.Number(1),
					SequencePoint(2, 1),
					JsExpression.Number(2)
				)),
			},
			"function f1(x) {\n" +
			"\t1;\n" +
			"\t2;\n" +
			"}\n",
			new[] {
				new SourceMapEntry(1, 1, 1, 1),
				new SourceMapEntry(2, 2, 0, 0),
				new SourceMapEntry(3, 2, 2, 1),
			});
		}

		[Test]
		public void FunctionExpressionOutputsPreviousSourceLocationAfterEndingBrace() {
			AssertCorrect(new JsStatement[] {
				JsExpression.Number(1),
				JsStatement.Var("f1", JsExpression.FunctionDefinition(new[] { "x" }, JsStatement.Block(
					SequencePoint(1, 1),
					JsExpression.Number(2),
					SequencePoint(2, 1),
					JsStatement.Var("f2", JsExpression.FunctionDefinition(new[] { "y" }, JsStatement.Block(
						SequencePoint(3, 1),
						JsExpression.Number(3),
						SequencePoint(4, 1)
					))),
					JsExpression.Number(4)
				))),
				JsExpression.Number(5)
			},
			"1;\n" +
			"var f1 = function(x) {\n" +
			"\t2;\n" +
			"\tvar f2 = function(y) {\n" +
			"\t\t3;\n" +
			"\t};\n" +
			"\t4;\n" +
			"};\n" +
			"5;\n",
			new[] {
				new SourceMapEntry(3, 2, 1, 1),
				new SourceMapEntry(4, 2, 2, 1),
				new SourceMapEntry(5, 3, 3, 1),
				new SourceMapEntry(6, 2, 4, 1),
				new SourceMapEntry(6, 3, 2, 1),
				new SourceMapEntry(8, 2, 0, 0),
			});
		}

		[Test]
		public void FunctionExpressionInsertsNoLocationOnTopIfItDoesNotStartWithASequencePoint() {
			AssertCorrect(new JsStatement[] {
				SequencePoint(1, 1),
				JsStatement.Var("f1", JsExpression.FunctionDefinition(new[] { "x" }, JsStatement.Block(
					JsExpression.Number(1),
					SequencePoint(2, 1),
					JsExpression.Number(2)
				))),
			},
			"var f1 = function(x) {\n" +
			"\t1;\n" +
			"\t2;\n" +
			"};\n",
			new[] {
				new SourceMapEntry(1, 1, 1, 1),
				new SourceMapEntry(2, 2, 0, 0),
				new SourceMapEntry(3, 2, 2, 1),
				new SourceMapEntry(4, 2, 1, 1),
			});
		}
	}
}
