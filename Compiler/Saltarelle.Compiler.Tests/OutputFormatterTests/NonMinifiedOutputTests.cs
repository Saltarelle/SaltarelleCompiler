using System.Collections.Generic;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests.OutputFormatterTests
{
	[TestFixture]
	public class NonMinifiedOutputTests {
		private void AssertCorrect(JsExpression expr, string expected) {
			var actual = OutputFormatter.Format(expr);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		private void AssertCorrect(JsStatement stmt, string expected) {
			var actual = OutputFormatter.Format(stmt);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		[Test]
		public void ArrayLiteralWorks() {
			AssertCorrect(JsExpression.ArrayLiteral(), "[]");
			AssertCorrect(JsExpression.ArrayLiteral(JsExpression.Number(1)), "[1]");
			AssertCorrect(JsExpression.ArrayLiteral(JsExpression.Number(1), JsExpression.Number(2)), "[1, 2]");
			AssertCorrect(JsExpression.ArrayLiteral(JsExpression.Number(1), null, JsExpression.Number(2), null), "[1, , 2, ]");
		}

		[Test]
		public void IndexingWorks() {
			AssertCorrect(JsExpression.Binary(ExpressionNodeType.Index,
			                   JsExpression.Binary(ExpressionNodeType.Index,
			                       JsExpression.Number(1),
			                       JsExpression.Number(2)
			                   ),
			                   JsExpression.Number(3)
			               ),
			               "1[2][3]");
		}

		[Test]
		public void StringLiteralsAreCorrectlyEncoded() {
			AssertCorrect(JsExpression.String("x"), "'x'");
			AssertCorrect(JsExpression.String("\""), "'\"'");
			AssertCorrect(JsExpression.String("'"), "\"'\"");
			AssertCorrect(JsExpression.String("\r\n/\\"), "'\\r\\n/\\\\'");
		}

		[Test]
		public void RegularExpressionLiteralsAreCorrectlyEncoded() {
			AssertCorrect(JsExpression.Regexp("x"), "/x/");
			AssertCorrect(JsExpression.Regexp("\""), "/\"/");
			AssertCorrect(JsExpression.Regexp("/"), "/\\//");
			AssertCorrect(JsExpression.Regexp("'"), "/'/");
			AssertCorrect(JsExpression.Regexp("\""), "/\"/");
			AssertCorrect(JsExpression.Regexp("x", "g"), "/x/g");
			AssertCorrect(JsExpression.Regexp(@"\s \\ x"), @"/\s \\ x/");
		}

		[Test]
		public void NullLiteralWorks() {
			AssertCorrect(JsExpression.Null, "null");
		}

		[Test]
		public void BooleanLiteralsWork() {
			AssertCorrect(JsExpression.True, "true");
			AssertCorrect(JsExpression.False, "false");
		}

		[Test]
		public void NumbersAreCorrectlyRepresented() {
			AssertCorrect(JsExpression.Number(1), "1");
			AssertCorrect(JsExpression.Number(1.25), "1.25");
			AssertCorrect(JsExpression.Number(double.PositiveInfinity), "Infinity");
			AssertCorrect(JsExpression.Number(double.NegativeInfinity), "-Infinity");
			AssertCorrect(JsExpression.Number(double.NaN), "NaN");
			AssertCorrect(JsExpression.Number(9007199254740992), "9007199254740992");
			AssertCorrect(JsExpression.Number(-9007199254740992), "-9007199254740992");
			AssertCorrect(JsExpression.Number(9.22337203685477E+18), "9.22337203685477E+18");
			AssertCorrect(JsExpression.Number(-9.22337203685477E+18), "-9.22337203685477E+18");
		}

		[Test]
		public void IdentifierIsOutputCorrectly() {
			AssertCorrect(JsExpression.Identifier("SomeIdentifier"), "SomeIdentifier");
		}

		[Test]
		public void ObjectLiteralIsOutputCorrectly() {
			AssertCorrect(JsExpression.ObjectLiteral(), "{}");
			AssertCorrect(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("x", JsExpression.Number(1))), "{ x: 1 }");
			AssertCorrect(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("x", JsExpression.Number(1)),
			                                         new JsObjectLiteralProperty("y", JsExpression.Number(2)),
			                                         new JsObjectLiteralProperty("z", JsExpression.Number(3))),
			              "{ x: 1, y: 2, z: 3 }");
		}

		[Test]
		public void ObjectLiteralWithFunctionValuesAreOutputOnMultipleLines() {
			AssertCorrect(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("x", JsExpression.Number(1)),
			                                         new JsObjectLiteralProperty("y", JsExpression.FunctionDefinition(new string[0], JsStatement.Return())),
			                                         new JsObjectLiteralProperty("z", JsExpression.Number(3))),
@"{
	x: 1,
	y: function() {
		return;
	},
	z: 3
}");
		}

		[Test]
		public void ObjectLiteralWithAccessorsAreOutputCorrectly() {
			AssertCorrect(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("x", ObjectLiteralPropertyKind.GetAccessor, JsExpression.FunctionDefinition(new string[0], JsExpression.Number(1))),
			                                         new JsObjectLiteralProperty("x", ObjectLiteralPropertyKind.SetAccessor, JsExpression.FunctionDefinition(new[] { "v" }, JsExpression.Number(2))),
			                                         new JsObjectLiteralProperty("z", JsExpression.Number(3))),
@"{
	get x() {
		1;
	},
	set x(v) {
		2;
	},
	z: 3
}");
		}

		[Test]
		public void ObjectLiteralWithNumericPropertyWorks() {
			AssertCorrect(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("1", JsExpression.Number(2))), "{ '1': 2 }");
		}

		[Test]
		public void ObjectLiteralWithInvalidIdentifierPropertyWorks() {
			AssertCorrect(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("a\\b", JsExpression.Number(1))), "{ 'a\\\\b': 1 }");
		}

		[Test]
		public void FunctionDefinitionExpressionIsCorrectlyOutput() {
			AssertCorrect(JsExpression.FunctionDefinition(new string[0], JsStatement.Return(JsExpression.Null)), "function() {\n\treturn null;\n}");
			AssertCorrect(JsExpression.FunctionDefinition(new [] { "a", "b" }, JsStatement.Return(JsExpression.Null)), "function(a, b) {\n\treturn null;\n}");
			AssertCorrect(JsExpression.FunctionDefinition(new string[0], JsStatement.Return(JsExpression.Null), name: "myFunction"), "function myFunction() {\n\treturn null;\n}");
		}

		[Test]
		public void UnaryOperatorsAreCorrectlyOutput() {
			var operators = new Dictionary<ExpressionNodeType, string> { { ExpressionNodeType.TypeOf, "typeof({0})" },
			                                                             { ExpressionNodeType.LogicalNot, "!{0}" },
			                                                             { ExpressionNodeType.Negate, "-{0}" },
			                                                             { ExpressionNodeType.Positive, "+{0}" },
			                                                             { ExpressionNodeType.PrefixPlusPlus, "++{0}" },
			                                                             { ExpressionNodeType.PrefixMinusMinus, "--{0}" },
			                                                             { ExpressionNodeType.PostfixPlusPlus, "{0}++" },
			                                                             { ExpressionNodeType.PostfixMinusMinus, "{0}--" },
			                                                             { ExpressionNodeType.Delete, "delete {0}" },
			                                                             { ExpressionNodeType.Void, "void({0})" },
			                                                             { ExpressionNodeType.BitwiseNot, "~{0}" },
			                                                           };

			for (var oper = ExpressionNodeType.UnaryFirst; oper <= ExpressionNodeType.UnaryLast; oper++) {
				Assert.That(operators.ContainsKey(oper), string.Format("Unexpected operator {0}", oper));
				var expr = JsExpression.Unary(oper, JsExpression.Identifier("a"));
				AssertCorrect(expr, string.Format(operators[oper], "a"));
			}
		}

		[Test]
		public void BinaryOperatorssAreCorrectlyOutput() {
			var operators = new Dictionary<ExpressionNodeType, string> { { ExpressionNodeType.LogicalAnd, "{0} && {1}" },
			                                                             { ExpressionNodeType.LogicalOr, "{0} || {1}" },
			                                                             { ExpressionNodeType.NotEqual, "{0} != {1}" },
			                                                             { ExpressionNodeType.LesserOrEqual, "{0} <= {1}" },
			                                                             { ExpressionNodeType.GreaterOrEqual, "{0} >= {1}" },
			                                                             { ExpressionNodeType.Lesser, "{0} < {1}" },
			                                                             { ExpressionNodeType.Greater, "{0} > {1}" },
			                                                             { ExpressionNodeType.Equal, "{0} == {1}" },
			                                                             { ExpressionNodeType.Subtract, "{0} - {1}" },
			                                                             { ExpressionNodeType.Add, "{0} + {1}" },
			                                                             { ExpressionNodeType.Modulo, "{0} % {1}" },
			                                                             { ExpressionNodeType.Divide, "{0} / {1}" },
			                                                             { ExpressionNodeType.Multiply, "{0} * {1}" },
			                                                             { ExpressionNodeType.BitwiseAnd, "{0} & {1}" },
			                                                             { ExpressionNodeType.BitwiseOr, "{0} | {1}" },
			                                                             { ExpressionNodeType.BitwiseXor, "{0} ^ {1}" },
			                                                             { ExpressionNodeType.Same, "{0} === {1}" },
			                                                             { ExpressionNodeType.NotSame, "{0} !== {1}" },
			                                                             { ExpressionNodeType.LeftShift, "{0} << {1}" },
			                                                             { ExpressionNodeType.RightShiftSigned, "{0} >> {1}" },
			                                                             { ExpressionNodeType.RightShiftUnsigned, "{0} >>> {1}" },
			                                                             { ExpressionNodeType.InstanceOf, "{0} instanceof {1}" },
			                                                             { ExpressionNodeType.In, "{0} in {1}" },
			                                                             { ExpressionNodeType.Index, "{0}[{1}]" },
			                                                             { ExpressionNodeType.Assign, "{0} = {1}" },
			                                                             { ExpressionNodeType.MultiplyAssign, "{0} *= {1}" },
			                                                             { ExpressionNodeType.DivideAssign, "{0} /= {1}" },
			                                                             { ExpressionNodeType.ModuloAssign, "{0} %= {1}" },
			                                                             { ExpressionNodeType.AddAssign, "{0} += {1}" },
			                                                             { ExpressionNodeType.SubtractAssign, "{0} -= {1}" },
			                                                             { ExpressionNodeType.LeftShiftAssign, "{0} <<= {1}" },
			                                                             { ExpressionNodeType.RightShiftSignedAssign, "{0} >>= {1}" },
			                                                             { ExpressionNodeType.RightShiftUnsignedAssign, "{0} >>>= {1}" },
			                                                             { ExpressionNodeType.BitwiseAndAssign, "{0} &= {1}" },
			                                                             { ExpressionNodeType.BitwiseOrAssign, "{0} |= {1}" },
			                                                             { ExpressionNodeType.BitwiseXorAssign, "{0} ^= {1}" },
			                                                           };

			for (var oper = ExpressionNodeType.BinaryFirst; oper <= ExpressionNodeType.BinaryLast; oper++) {
				Assert.That(operators.ContainsKey(oper), string.Format("Unexpected operator {0}", oper));
				var expr = JsExpression.Binary(oper, JsExpression.Identifier("a"), JsExpression.Identifier("b"));
				AssertCorrect(expr, string.Format(operators[oper], "a", "b"));
			}
		}

		[Test]
		public void ThisIsCorrectlyOutput() {
			AssertCorrect(JsExpression.This, "this");
		}

		[Test]
		public void CommentsAreCorrectlyOutput() {
			AssertCorrect(JsStatement.Comment("Line 1"), "//Line 1\n");
			AssertCorrect(JsStatement.Comment(" With spaces "), "// With spaces \n");
			AssertCorrect(JsStatement.Comment(" With\n Multiple\n Lines"), "// With\n// Multiple\n// Lines\n");
		}

		[Test]
		public void BlockStatementsAreCorrectlyOutput() {
			AssertCorrect(JsStatement.Block(new JsStatement[0]), "{\n}\n");
			AssertCorrect(JsStatement.Block(new[] { JsStatement.Comment("X") }), "{\n\t//X\n}\n");
			AssertCorrect(JsStatement.Block(new[] { JsStatement.Comment("X"), JsStatement.Comment("Y") }), "{\n\t//X\n\t//Y\n}\n");
		}

		[Test]
		public void VariableDeclarationStatementsAreCorrectlyOutput() {
			AssertCorrect(JsStatement.Var(new[] { JsStatement.Declaration("i", null) }), "var i;\n");
			AssertCorrect(JsStatement.Var(new[] { JsStatement.Declaration("i", null), JsStatement.Declaration("j", null) }), "var i, j;\n");
			AssertCorrect(JsStatement.Var(new[] { JsStatement.Declaration("i", JsExpression.Number(0)) }), "var i = 0;\n");
			AssertCorrect(JsStatement.Var(new[] { JsStatement.Declaration("i", JsExpression.Number(0)), JsStatement.Declaration("j", JsExpression.Number(1)) }), "var i = 0, j = 1;\n");
			AssertCorrect(JsStatement.Var(new[] { JsStatement.Declaration("i", JsExpression.Number(0)), JsStatement.Declaration("j", null) }), "var i = 0, j;\n");
		}

		[Test]
		public void ExpressionStatementsAreCorrectlyOutput() {
			AssertCorrect((JsStatement)JsExpression.This, "this;\n");
		}

		[Test]
		public void ForStatementsAreCorrectlyOutput() {
			AssertCorrect(JsStatement.For(JsStatement.Var(JsStatement.Declaration("i", JsExpression.Number(0))),
			                              JsExpression.Lesser(JsExpression.Identifier("i"), JsExpression.Number(10)),
			                              JsExpression.PostfixPlusPlus(JsExpression.Identifier("i")),
			                              JsStatement.EmptyBlock),
			              "for (var i = 0; i < 10; i++) {\n}\n");

			AssertCorrect(JsStatement.For(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0)),
			                              JsExpression.Lesser(JsExpression.Identifier("i"), JsExpression.Number(10)),
			                              JsExpression.PostfixPlusPlus(JsExpression.Identifier("i")),
			                              JsStatement.EmptyBlock),
			              "for (i = 0; i < 10; i++) {\n}\n");

			AssertCorrect(JsStatement.For(JsStatement.Var(JsStatement.Declaration("i", JsExpression.Number(0)), JsStatement.Declaration("j", JsExpression.Number(1))),
			                              JsExpression.Lesser(JsExpression.Identifier("i"), JsExpression.Number(10)),
			                              JsExpression.Comma(JsExpression.PostfixPlusPlus(JsExpression.Identifier("i")), JsExpression.PostfixPlusPlus(JsExpression.Identifier("j"))),
			                              JsStatement.EmptyBlock),
			              "for (var i = 0, j = 1; i < 10; i++, j++) {\n}\n");

			AssertCorrect(JsStatement.For(JsStatement.Empty, null, null, JsStatement.EmptyBlock),
			              "for (;;) {\n}\n");
		}

		[Test]
		public void ForInStatementsAreCorrectlyOutput() {
			AssertCorrect(JsStatement.ForIn("x", JsExpression.Identifier("o"), JsStatement.EmptyBlock, true),
			              "for (var x in o) {\n}\n");

			AssertCorrect(JsStatement.ForIn("x", JsExpression.Identifier("o"), JsStatement.EmptyBlock, false),
			              "for (x in o) {\n}\n");
		}

		[Test]
		public void EmptyStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Empty, ";\n");
		}

		[Test]
		public void IfStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.If(JsExpression.True, JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0)), null),
			              "if (true) {\n\ti = 0;\n}\n");
			AssertCorrect(JsStatement.If(JsExpression.True, JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0)), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(1))),
			              "if (true) {\n\ti = 0;\n}\nelse {\n\ti = 1;\n}\n");
		}

		[Test]
		public void IfAndElseIfStatementsAreChained() {
			AssertCorrect(JsStatement.If(JsExpression.True, JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0)), null),
			              "if (true) {\n\ti = 0;\n}\n");
			AssertCorrect(JsStatement.If(JsExpression.Identifier("a"), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0)),
			                             JsStatement.If(JsExpression.Identifier("b"), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(1)),
			                             JsStatement.If(JsExpression.Identifier("c"), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(2)), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(3))))),
@"if (a) {
	i = 0;
}
else if (b) {
	i = 1;
}
else if (c) {
	i = 2;
}
else {
	i = 3;
}
");
		}

		[Test]
		public void BreakStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Break(), "break;\n");
			AssertCorrect(JsStatement.Break("someLabel"), "break someLabel;\n");
		}

		[Test]
		public void ContinueStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Continue(), "continue;\n");
			AssertCorrect(JsStatement.Continue("someLabel"), "continue someLabel;\n");
		}

		[Test]
		public void DoWhileStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.DoWhile(JsExpression.True, JsStatement.Block(JsStatement.Var("x", JsExpression.Number(0)))),
			              "do {\n\tvar x = 0;\n} while (true);\n");
		}

		[Test]
		public void WhileStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.While(JsExpression.True, JsStatement.Block(JsStatement.Var("x", JsExpression.Number(0)))),
			              "while (true) {\n\tvar x = 0;\n}\n");
		}

		[Test]
		public void ReturnStatementWithOrWithoutExpressionIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Return(null), "return;\n");
			AssertCorrect(JsStatement.Return(JsExpression.Identifier("x")), "return x;\n");
		}

		[Test]
		public void TryCatchFinallyStatementWithCatchOrFinallyOrBothIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Try(JsExpression.Identifier("x"), JsStatement.Catch("e", JsExpression.Identifier("y")), JsExpression.Identifier("z")),
			              "try {\n\tx;\n}\ncatch (e) {\n\ty;\n}\nfinally {\n\tz;\n}\n");
			AssertCorrect(JsStatement.Try(JsExpression.Identifier("x"), JsStatement.Catch("e", JsExpression.Identifier("y")), null),
			              "try {\n\tx;\n}\ncatch (e) {\n\ty;\n}\n");
			AssertCorrect(JsStatement.Try(JsExpression.Identifier("x"), null, JsExpression.Identifier("z")),
			              "try {\n\tx;\n}\nfinally {\n\tz;\n}\n");
		}

		[Test]
		public void ThrowStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Throw(JsExpression.Identifier("x")), "throw x;\n");
		}

		[Test]
		public void SwitchStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Switch(JsExpression.Identifier("x"),
			                  JsStatement.SwitchSection(new[] { JsExpression.Number(0) }, JsExpression.Identifier("a")),
			                  JsStatement.SwitchSection(new[] { JsExpression.Number(1), JsExpression.Number(2) }, JsExpression.Identifier("b")),
			                  JsStatement.SwitchSection(new[] { null, JsExpression.Number(3) }, JsExpression.Identifier("c"))
			              ),
@"switch (x) {
	case 0: {
		a;
	}
	case 1:
	case 2: {
		b;
	}
	default:
	case 3: {
		c;
	}
}
");

		}

		[Test]
		public void FunctionDefinitionStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Function("f", new[] { "a", "b", "c" }, JsExpression.Identifier("x")), "function f(a, b, c) {\n\tx;\n}\n");
		}

		[Test]
		public void WithStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.With(JsExpression.Identifier("o"), JsStatement.EmptyBlock), "with (o) {\n}\n");
		}

		[Test]
		public void LabelledStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Block(JsStatement.Label("lbl", JsExpression.Identifier("X"))),
			              "{\n\tlbl:\n\tX;\n}\n");
		}
	}
}
