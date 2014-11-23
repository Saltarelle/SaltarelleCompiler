using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests.OutputFormatterTests {
	[TestFixture]
	public class MinifiedOutputTests {
		private void AssertCorrect(JsExpression expr, string expected) {
			var actual = OutputFormatter.FormatMinified(expr);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		private void AssertCorrect(JsStatement stmt, string expected) {
			var actual = OutputFormatter.FormatMinified(stmt);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		[Test]
		public void CommaIsOutputWithoutSpace() {
			AssertCorrect(JsExpression.Comma(
			                  JsExpression.Comma(
			                      JsExpression.Number(1),
			                      JsExpression.Number(2)
			                  ),
			                  JsExpression.Comma(
			                      JsExpression.Number(3),
			                      JsExpression.Number(4)
			                  )
			              ),
			              "1,2,3,4");
		}

		[Test]
		public void ArrayLiteralsContainNoSpaces() {
			AssertCorrect(JsExpression.ArrayLiteral(), "[]");
			AssertCorrect(JsExpression.ArrayLiteral(JsExpression.Number(1)), "[1]");
			AssertCorrect(JsExpression.ArrayLiteral(JsExpression.Number(1), JsExpression.Number(2)), "[1,2]");
			AssertCorrect(JsExpression.ArrayLiteral(JsExpression.Number(1), null, JsExpression.Number(2), null), "[1,,2,]");
		}

		[Test]
		public void ConditionalDoesNotContainEmbeddedSpaces() {
			AssertCorrect(JsExpression.Conditional(
			                  JsExpression.Number(1),
			                  JsExpression.Number(2),
			                  JsExpression.Number(3)
			              ),
			              "(1?2:3)");
		}

		[Test]
		public void NewArgumentListContainsNoSpaces() {
			AssertCorrect(JsExpression.New(JsExpression.Identifier("x"), null), "new x");
			AssertCorrect(JsExpression.New(JsExpression.Identifier("x")), "new x()");
			AssertCorrect(JsExpression.New(JsExpression.Identifier("x"), JsExpression.Number(1), JsExpression.Number(2), JsExpression.Number(3)), "new x(1,2,3)");
		}

		[Test]
		public void InvocationArgumentListContainsNoSpaces() {
			AssertCorrect(JsExpression.Invocation(JsExpression.Identifier("x"), JsExpression.Number(1), JsExpression.Number(2), JsExpression.Number(3)), "x(1,2,3)");
		}

		[Test]
		public void ObjectLiteralsContainNoEmbeddedSpaces() {
			AssertCorrect(JsExpression.ObjectLiteral(), "{}");
			AssertCorrect(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("x", JsExpression.Number(1))), "{x:1}");
			AssertCorrect(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("x", JsExpression.Number(1)),
			                                         new JsObjectLiteralProperty("y", JsExpression.Number(2)),
			                                         new JsObjectLiteralProperty("z", JsExpression.Number(3))),
			              "{x:1,y:2,z:3}");
		}

		[Test]
		public void ObjectLiteralWithFunctionValuesAreNotOutputOnMultipleLines() {
			AssertCorrect(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("x", JsExpression.Number(1)),
			                                         new JsObjectLiteralProperty("y", JsExpression.FunctionDefinition(new string[0], JsStatement.Return())),
			                                         new JsObjectLiteralProperty("z", JsExpression.Number(3))),
			              "{x:1,y:function(){return;},z:3}");
		}

		[Test]
		public void FunctionDefinitionExpressionIsOutputCorrectly() {
			AssertCorrect(JsExpression.FunctionDefinition(new string[0], JsStatement.Return(JsExpression.Null)), "function(){return null;}");
			AssertCorrect(JsExpression.FunctionDefinition(new [] { "a", "b" }, JsStatement.Return(JsExpression.Null)), "function(a,b){return null;}");
			AssertCorrect(JsExpression.FunctionDefinition(new string[0], JsStatement.Return(JsExpression.Null), name: "myFunction"), "function myFunction(){return null;}");
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
		public void BinaryOperatorsAreCorrectlyOutput() {
			var operators = new Dictionary<ExpressionNodeType, string> { { ExpressionNodeType.LogicalAnd, "{0}&&{1}" },
			                                                             { ExpressionNodeType.LogicalOr, "{0}||{1}" },
			                                                             { ExpressionNodeType.NotEqual, "{0}!={1}" },
			                                                             { ExpressionNodeType.LesserOrEqual, "{0}<={1}" },
			                                                             { ExpressionNodeType.GreaterOrEqual, "{0}>={1}" },
			                                                             { ExpressionNodeType.Lesser, "{0}<{1}" },
			                                                             { ExpressionNodeType.Greater, "{0}>{1}" },
			                                                             { ExpressionNodeType.Equal, "{0}=={1}" },
			                                                             { ExpressionNodeType.Subtract, "{0}-{1}" },
			                                                             { ExpressionNodeType.Add, "{0}+{1}" },
			                                                             { ExpressionNodeType.Modulo, "{0}%{1}" },
			                                                             { ExpressionNodeType.Divide, "{0}/{1}" },
			                                                             { ExpressionNodeType.Multiply, "{0}*{1}" },
			                                                             { ExpressionNodeType.BitwiseAnd, "{0}&{1}" },
			                                                             { ExpressionNodeType.BitwiseOr, "{0}|{1}" },
			                                                             { ExpressionNodeType.BitwiseXor, "{0}^{1}" },
			                                                             { ExpressionNodeType.Same, "{0}==={1}" },
			                                                             { ExpressionNodeType.NotSame, "{0}!=={1}" },
			                                                             { ExpressionNodeType.LeftShift, "{0}<<{1}" },
			                                                             { ExpressionNodeType.RightShiftSigned, "{0}>>{1}" },
			                                                             { ExpressionNodeType.RightShiftUnsigned, "{0}>>>{1}" },
			                                                             { ExpressionNodeType.InstanceOf, "{0} instanceof {1}" },
			                                                             { ExpressionNodeType.In, "{0} in {1}" },
			                                                             { ExpressionNodeType.Index, "{0}[{1}]" },
			                                                             { ExpressionNodeType.Assign, "{0}={1}" },
			                                                             { ExpressionNodeType.MultiplyAssign, "{0}*={1}" },
			                                                             { ExpressionNodeType.DivideAssign, "{0}/={1}" },
			                                                             { ExpressionNodeType.ModuloAssign, "{0}%={1}" },
			                                                             { ExpressionNodeType.AddAssign, "{0}+={1}" },
			                                                             { ExpressionNodeType.SubtractAssign, "{0}-={1}" },
			                                                             { ExpressionNodeType.LeftShiftAssign, "{0}<<={1}" },
			                                                             { ExpressionNodeType.RightShiftSignedAssign, "{0}>>={1}" },
			                                                             { ExpressionNodeType.RightShiftUnsignedAssign, "{0}>>>={1}" },
			                                                             { ExpressionNodeType.BitwiseAndAssign, "{0}&={1}" },
			                                                             { ExpressionNodeType.BitwiseOrAssign, "{0}|={1}" },
			                                                             { ExpressionNodeType.BitwiseXorAssign, "{0}^={1}" },
			                                                           };

			for (var oper = ExpressionNodeType.BinaryFirst; oper <= ExpressionNodeType.BinaryLast; oper++) {
				Assert.That(operators.ContainsKey(oper), string.Format("Unexpected operator {0}", oper));
				var expr = JsExpression.Binary(oper, JsExpression.Identifier("a"), JsExpression.Identifier("b"));
				AssertCorrect(expr, string.Format(operators[oper], "a", "b"));
			}
		}

		[Test]
		public void ASpaceIsInsertedBetweenBinaryAndUnaryPlusAndMinusToAvoidParseAsIncrementOrDecrement() {
			AssertCorrect(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Identifier("a"), JsExpression.Unary(ExpressionNodeType.Positive, JsExpression.Identifier("b"))), "a+ +b");
			AssertCorrect(JsExpression.Binary(ExpressionNodeType.Subtract, JsExpression.Identifier("a"), JsExpression.Unary(ExpressionNodeType.Negate, JsExpression.Identifier("b"))), "a- -b");
			AssertCorrect(JsExpression.Binary(ExpressionNodeType.Subtract, JsExpression.Identifier("a"), JsExpression.Unary(ExpressionNodeType.Positive, JsExpression.Identifier("b"))), "a-+b");
			AssertCorrect(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Identifier("a"), JsExpression.Unary(ExpressionNodeType.Negate, JsExpression.Identifier("b"))), "a+-b");
		}

		[Test]
		public void CommentsAreIgnored() {
			AssertCorrect(JsStatement.Comment("Line 1"), "");
		}

		[Test]
		public void BlockStatementsAreCorrectlyOutput() {
			AssertCorrect(JsStatement.Block(new JsStatement[0]), "{}");
			AssertCorrect(JsStatement.Block(new JsStatement[] { JsExpression.Identifier("X") }), "{X;}");
			AssertCorrect(JsStatement.Block(new JsStatement[] { JsExpression.Identifier("X"), JsExpression.Identifier("Y") }), "{X;Y;}");
		}

		[Test]
		public void VariableDeclarationStatementsAreCorrectlyOutput() {
			AssertCorrect(JsStatement.Var(new[] { JsStatement.Declaration("i", null) }), "var i;");
			AssertCorrect(JsStatement.Var(new[] { JsStatement.Declaration("i", null), JsStatement.Declaration("j", null) }), "var i,j;");
			AssertCorrect(JsStatement.Var(new[] { JsStatement.Declaration("i", JsExpression.Number(0)) }), "var i=0;");
			AssertCorrect(JsStatement.Var(new[] { JsStatement.Declaration("i", JsExpression.Number(0)), JsStatement.Declaration("j", JsExpression.Number(1)) }), "var i=0,j=1;");
			AssertCorrect(JsStatement.Var(new[] { JsStatement.Declaration("i", JsExpression.Number(0)), JsStatement.Declaration("j", null) }), "var i=0,j;");
		}

		[Test]
		public void ExpressionStatementsAreCorrectlyOutput() {
			AssertCorrect((JsStatement)JsExpression.This, "this;");
		}

		[Test]
		public void ForStatementsAreCorrectlyOutput() {
			AssertCorrect(JsStatement.For(JsStatement.Var("i", JsExpression.Number(0)),
			                              JsExpression.Lesser(JsExpression.Identifier("i"), JsExpression.Number(10)),
			                              JsExpression.PostfixPlusPlus(JsExpression.Identifier("i")),
			                              JsStatement.EmptyBlock),
			              "for(var i=0;i<10;i++){}");

			AssertCorrect(JsStatement.For(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0)),
			                              JsExpression.Lesser(JsExpression.Identifier("i"), JsExpression.Number(10)),
			                              JsExpression.PostfixPlusPlus(JsExpression.Identifier("i")),
			                              JsStatement.EmptyBlock),
			              "for(i=0;i<10;i++){}");

			AssertCorrect(JsStatement.For(JsStatement.Var(JsStatement.Declaration("i", JsExpression.Number(0)), JsStatement.Declaration("j", JsExpression.Number(1))),
			                              JsExpression.Lesser(JsExpression.Identifier("i"), JsExpression.Number(10)),
			                              JsExpression.Comma(JsExpression.PostfixPlusPlus(JsExpression.Identifier("i")), JsExpression.PostfixPlusPlus(JsExpression.Identifier("j"))),
			                              JsStatement.EmptyBlock),
			              "for(var i=0,j=1;i<10;i++,j++){}");

			AssertCorrect(JsStatement.For(JsStatement.Empty, null, null, JsStatement.EmptyBlock), "for(;;){}");
		}

		[Test]
		public void ForInStatementsAreCorrectlyOutput() {
			AssertCorrect(JsStatement.ForIn("x", JsExpression.Identifier("o"), JsStatement.EmptyBlock, true),
			              "for(var x in o){}");

			AssertCorrect(JsStatement.ForIn("x", JsExpression.Identifier("o"), JsStatement.EmptyBlock, false),
			              "for(x in o){}");
		}

		[Test]
		public void EmptyStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Empty, ";");
		}

		[Test]
		public void IfStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.If(JsExpression.True, JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0)), null),
			              "if(true){i=0;}");
			AssertCorrect(JsStatement.If(JsExpression.True, JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0)), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(1))),
			              "if(true){i=0;}else{i=1;}");
		}

		[Test]
		public void IfAndElseIfStatementsAreChained() {
			AssertCorrect(JsStatement.If(JsExpression.True, JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0)), null),
			              "if(true){i=0;}");
			AssertCorrect(JsStatement.If(JsExpression.Identifier("a"), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0)),
			                                 JsStatement.If(JsExpression.Identifier("b"), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(1)),
			                                 JsStatement.If(JsExpression.Identifier("c"), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(2)), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(3))))),
			              "if(a){i=0;}else if(b){i=1;}else if(c){i=2;}else{i=3;}");
		}

		[Test]
		public void IfAndElseIfStatementsAreChainedWhenThereAreSequencePoints() {
			Func<int, Location> createLocation = i => Location.Create("", new TextSpan(i, 1), new LinePositionSpan(new LinePosition(i, 1), new LinePosition(i, 2)));

			AssertCorrect(JsStatement.If(JsExpression.Identifier("a"), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0)),
			                             JsStatement.Block(JsStatement.SequencePoint(createLocation(1)), JsStatement.If(JsExpression.Identifier("b"), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(1)),
			                             JsStatement.Block(JsStatement.SequencePoint(createLocation(2)), JsStatement.If(JsExpression.Identifier("c"), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(2)), JsStatement.Block(JsStatement.SequencePoint(createLocation(3)), JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(3)))))))),
			              "if(a){i=0;}else if(b){i=1;}else if(c){i=2;}else{i=3;}");
		}

		[Test]
		public void BreakStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Break(), "break;");
			AssertCorrect(JsStatement.Break("someLabel"), "break someLabel;");
		}

		[Test]
		public void ContinueStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Continue(), "continue;");
			AssertCorrect(JsStatement.Continue("someLabel"), "continue someLabel;");
		}

		[Test]
		public void DoWhileStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.DoWhile(JsExpression.True, JsStatement.Block(JsStatement.Var("x", JsExpression.Number(0)))),
			              "do{var x=0;}while(true);");
		}

		[Test]
		public void WhileStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.While(JsExpression.True, JsStatement.Block(JsStatement.Var("x", JsExpression.Number(0)))),
			              "while(true){var x=0;}");
		}

		[Test]
		public void ReturnStatementWithOrWithoutExpressionIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Return(null), "return;");
			AssertCorrect(JsStatement.Return(JsExpression.Identifier("x")), "return x;");
		}

		[Test]
		public void TryCatchFinallyStatementWithCatchOrFinallyOrBothWorks() {
			AssertCorrect(JsStatement.Try(JsExpression.Identifier("x"), JsStatement.Catch("e", JsExpression.Identifier("y")), JsExpression.Identifier("z")),
			              "try{x;}catch(e){y;}finally{z;}");
			AssertCorrect(JsStatement.Try(JsExpression.Identifier("x"), JsStatement.Catch("e", JsExpression.Identifier("y")), null),
			              "try{x;}catch(e){y;}");
			AssertCorrect(JsStatement.Try(JsExpression.Identifier("x"), null, JsExpression.Identifier("z")),
			              "try{x;}finally{z;}");
		}

		[Test]
		public void ThrowStatementWorks() {
			AssertCorrect(JsStatement.Throw(JsExpression.Identifier("x")), "throw x;");
		}

		[Test]
		public void SwitchStatementWorks() {
			AssertCorrect(JsStatement.Switch(JsExpression.Identifier("x"),
			                  JsStatement.SwitchSection(new[] { JsExpression.Number(0) }, JsExpression.Identifier("a")),
			                  JsStatement.SwitchSection(new[] { JsExpression.Number(1), JsExpression.Number(2) }, JsExpression.Identifier("b")),
			                  JsStatement.SwitchSection(new[] { null, JsExpression.Number(3) }, JsExpression.Identifier("c"))
			              ),
			              "switch(x){case 0:{a;}case 1:case 2:{b;}default:case 3:{c;}}");

		}

		[Test]
		public void FunctionDefinitionStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Function("f", new[] { "a", "b", "c" }, JsExpression.Identifier("x")),
			              "function f(a,b,c){x;}");
		}

		[Test]
		public void WithStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.With(JsExpression.Identifier("o"), JsStatement.EmptyBlock), "with(o){}");
		}

		[Test]
		public void LabelledStatementIsCorrectlyOutput() {
			AssertCorrect(JsStatement.Block(JsStatement.Label("lbl", JsExpression.Identifier("X"))),
			              "{lbl:X;}");
		}
	}
}
