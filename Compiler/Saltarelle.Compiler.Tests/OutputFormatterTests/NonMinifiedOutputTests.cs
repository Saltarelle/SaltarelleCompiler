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
            AssertCorrect(JsExpression.String("'"), "'\\''");
            AssertCorrect(JsExpression.String("\r\n/\\"), "'\\r\\n/\\\\'");
        }

        [Test]
        public void RegularExpressionLiteralsAreCorrectlyEncoded() {
            AssertCorrect(JsExpression.Regexp("x"), "/x/");
            AssertCorrect(JsExpression.Regexp("\""), "/\"/");
            AssertCorrect(JsExpression.Regexp("/"), "/\\//");
            AssertCorrect(JsExpression.Regexp("\r\n/\\"), "/\\r\\n\\/\\\\/");
            AssertCorrect(JsExpression.Regexp("x", "g"), "/x/g");
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
                                                                          new JsObjectLiteralProperty("y", JsExpression.FunctionDefinition(new string[0], new JsReturnStatement())),
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
        public void ObjectLiteralWithNumericPropertyWorks() {
            AssertCorrect(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("1", JsExpression.Number(2))), "{ '1': 2 }");
        }

        [Test]
        public void ObjectLiteralWithInvalidIdentifierPropertyWorks() {
            AssertCorrect(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("a\\b", JsExpression.Number(1))), "{ 'a\\\\b': 1 }");
        }

        [Test]
		public void FunctionDefinitionExpressionIsCorrectlyOutput() {
            AssertCorrect(JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.Null)), "function() {\n\treturn null;\n}");
            AssertCorrect(JsExpression.FunctionDefinition(new [] { "a", "b" }, new JsReturnStatement(JsExpression.Null)), "function(a, b) {\n\treturn null;\n}");
            AssertCorrect(JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.Null), name: "myFunction"), "function myFunction() {\n\treturn null;\n}");
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
			AssertCorrect(new JsComment("Line 1"), "//Line 1\n");
			AssertCorrect(new JsComment(" With spaces "), "// With spaces \n");
			AssertCorrect(new JsComment(" With\n Multiple\n Lines"), "// With\n// Multiple\n// Lines\n");
		}

		[Test]
		public void BlockStatementsAreCorrectlyOutput() {
			AssertCorrect(new JsBlockStatement(new JsStatement[0]), "{\n}\n");
			AssertCorrect(new JsBlockStatement(new[] { new JsComment("X") }), "{\n\t//X\n}\n");
			AssertCorrect(new JsBlockStatement(new[] { new JsComment("X"), new JsComment("Y") }), "{\n\t//X\n\t//Y\n}\n");
		}

		[Test]
		public void VariableDeclarationStatementsAreCorrectlyOutput() {
			AssertCorrect(new JsVariableDeclarationStatement(new[] { new JsVariableDeclaration("i", null) }), "var i;\n");
			AssertCorrect(new JsVariableDeclarationStatement(new[] { new JsVariableDeclaration("i", null), new JsVariableDeclaration("j", null) }), "var i, j;\n");
			AssertCorrect(new JsVariableDeclarationStatement(new[] { new JsVariableDeclaration("i", JsExpression.Number(0)) }), "var i = 0;\n");
			AssertCorrect(new JsVariableDeclarationStatement(new[] { new JsVariableDeclaration("i", JsExpression.Number(0)), new JsVariableDeclaration("j", JsExpression.Number(1)) }), "var i = 0, j = 1;\n");
			AssertCorrect(new JsVariableDeclarationStatement(new[] { new JsVariableDeclaration("i", JsExpression.Number(0)), new JsVariableDeclaration("j", null) }), "var i = 0, j;\n");
		}

		[Test]
		public void ExpressionStatementsAreCorrectlyOutput() {
			AssertCorrect(new JsExpressionStatement(JsExpression.This), "this;\n");
		}

		[Test]
		public void ForStatementsAreCorrectlyOutput() {
			AssertCorrect(new JsForStatement(new JsVariableDeclarationStatement(new JsVariableDeclaration("i", JsExpression.Number(0))),
			                                                      JsExpression.Lesser(JsExpression.Identifier("i"), JsExpression.Number(10)),
			                                                      JsExpression.PostfixPlusPlus(JsExpression.Identifier("i")),
			                                                      JsBlockStatement.EmptyStatement),
			              "for (var i = 0; i < 10; i++) {\n}\n");

			AssertCorrect(new JsForStatement(new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0))), 
			                                                      JsExpression.Lesser(JsExpression.Identifier("i"), JsExpression.Number(10)),
			                                                      JsExpression.PostfixPlusPlus(JsExpression.Identifier("i")),
			                                                      JsBlockStatement.EmptyStatement),
			              "for (i = 0; i < 10; i++) {\n}\n");

			AssertCorrect(new JsForStatement(new JsVariableDeclarationStatement(new JsVariableDeclaration("i", JsExpression.Number(0)), new JsVariableDeclaration("j", JsExpression.Number(1))),
			                                                      JsExpression.Lesser(JsExpression.Identifier("i"), JsExpression.Number(10)),
			                                                      JsExpression.Comma(JsExpression.PostfixPlusPlus(JsExpression.Identifier("i")), JsExpression.PostfixPlusPlus(JsExpression.Identifier("j"))),
			                                                      JsBlockStatement.EmptyStatement),
			              "for (var i = 0, j = 1; i < 10; i++, j++) {\n}\n");

			AssertCorrect(new JsForStatement(new JsEmptyStatement(), null, null, JsBlockStatement.EmptyStatement),
			              "for (;;) {\n}\n");
		}

		[Test]
		public void ForEachInStatementsAreCorrectlyOutput() {
			AssertCorrect(new JsForEachInStatement("x", JsExpression.Identifier("o"), JsBlockStatement.EmptyStatement, true),
			              "for (var x in o) {\n}\n");

			AssertCorrect(new JsForEachInStatement("x", JsExpression.Identifier("o"), JsBlockStatement.EmptyStatement, false),
			              "for (x in o) {\n}\n");
		}

		[Test]
		public void EmptyStatementIsCorrectlyOutput() {
			AssertCorrect(new JsEmptyStatement(), ";\n");
		}

		[Test]
		public void IfStatementIsCorrectlyOutput() {
			AssertCorrect(new JsIfStatement(JsExpression.True, new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0))), null),
			              "if (true) {\n\ti = 0;\n}\n");
			AssertCorrect(new JsIfStatement(JsExpression.True, new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0))), new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(1)))),
			              "if (true) {\n\ti = 0;\n}\nelse {\n\ti = 1;\n}\n");
		}

		[Test]
		public void IfAndElseIfStatementsAreChained() {
			AssertCorrect(new JsIfStatement(JsExpression.True, new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0))), null),
			              "if (true) {\n\ti = 0;\n}\n");
			AssertCorrect(new JsIfStatement(JsExpression.Identifier("a"), new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0))),
			                  new JsIfStatement(JsExpression.Identifier("b"), new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(1))),
			                  new JsIfStatement(JsExpression.Identifier("c"), new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(2))), new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(3)))))),
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
			AssertCorrect(new JsBreakStatement(), "break;\n");
			AssertCorrect(new JsBreakStatement("someLabel"), "break someLabel;\n");
		}

		[Test]
		public void ContinueStatementIsCorrectlyOutput() {
			AssertCorrect(new JsContinueStatement(), "continue;\n");
			AssertCorrect(new JsContinueStatement("someLabel"), "continue someLabel;\n");
		}

		[Test]
		public void DoWhileStatementIsCorrectlyOutput() {
			AssertCorrect(new JsDoWhileStatement(JsExpression.True, new JsBlockStatement(new JsVariableDeclarationStatement(new JsVariableDeclaration("x", JsExpression.Number(0))))),
			              "do {\n\tvar x = 0;\n} while (true);\n");
		}

		[Test]
		public void WhileStatementIsCorrectlyOutput() {
			AssertCorrect(new JsWhileStatement(JsExpression.True, new JsBlockStatement(new JsVariableDeclarationStatement(new JsVariableDeclaration("x", JsExpression.Number(0))))),
			              "while (true) {\n\tvar x = 0;\n}\n");
		}

		[Test]
		public void ReturnStatementWithOrWithoutExpressionIsCorrectlyOutput() {
			AssertCorrect(new JsReturnStatement(null), "return;\n");
			AssertCorrect(new JsReturnStatement(JsExpression.Identifier("x")), "return x;\n");
		}

		[Test]
		public void TryCatchFinallyStatementWithCatchOrFinallyOrBothIsCorrectlyOutput() {
			AssertCorrect(new JsTryStatement(new JsExpressionStatement(JsExpression.Identifier("x")), new JsCatchClause("e", new JsExpressionStatement(JsExpression.Identifier("y"))), new JsExpressionStatement(JsExpression.Identifier("z"))),
			              "try {\n\tx;\n}\ncatch (e) {\n\ty;\n}\nfinally {\n\tz;\n}\n");
			AssertCorrect(new JsTryStatement(new JsExpressionStatement(JsExpression.Identifier("x")), new JsCatchClause("e", new JsExpressionStatement(JsExpression.Identifier("y"))), null),
			              "try {\n\tx;\n}\ncatch (e) {\n\ty;\n}\n");
			AssertCorrect(new JsTryStatement(new JsExpressionStatement(JsExpression.Identifier("x")), null, new JsExpressionStatement(JsExpression.Identifier("z"))),
			              "try {\n\tx;\n}\nfinally {\n\tz;\n}\n");
		}

		[Test]
		public void ThrowStatementIsCorrectlyOutput() {
			AssertCorrect(new JsThrowStatement(JsExpression.Identifier("x")), "throw x;\n");
		}

		[Test]
		public void SwitchStatementIsCorrectlyOutput() {
			AssertCorrect(new JsSwitchStatement(JsExpression.Identifier("x"),
			                  new JsSwitchSection(new[] { JsExpression.Number(0) }, new JsExpressionStatement(JsExpression.Identifier("a"))),
			                  new JsSwitchSection(new[] { JsExpression.Number(1), JsExpression.Number(2) }, new JsExpressionStatement(JsExpression.Identifier("b"))),
			                  new JsSwitchSection(new[] { null, JsExpression.Number(3) }, new JsExpressionStatement(JsExpression.Identifier("c")))
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
			AssertCorrect(new JsFunctionStatement("f", new[] { "a", "b", "c" }, new JsExpressionStatement(JsExpression.Identifier("x"))), "function f(a, b, c) {\n\tx;\n}\n");
		}

		[Test]
		public void WithStatementIsCorrectlyOutput() {
			AssertCorrect(new JsWithStatement(JsExpression.Identifier("o"), JsBlockStatement.EmptyStatement), "with (o) {\n}\n");
		}

		[Test]
		public void LabelledStatementIsCorrectlyOutput() {
			AssertCorrect(new JsBlockStatement(new JsLabelledStatement("lbl", new JsExpressionStatement(JsExpression.Identifier("X")))),
			              "{\n\tlbl:\n\tX;\n}\n");
		}
    }
}
