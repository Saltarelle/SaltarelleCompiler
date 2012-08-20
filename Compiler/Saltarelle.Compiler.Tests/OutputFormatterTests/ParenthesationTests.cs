using System.Collections.Generic;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests.OutputFormatterTests
{
    [TestFixture]
    public class ParenthesationTests {
		private void AssertCorrect(JsExpression expr, string expected) {
			var actual = OutputFormatter.Format(expr);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		private void AssertCorrect(JsStatement stmt, string expected) {
			var actual = OutputFormatter.Format(stmt);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

        [Test]
        public void LeftToRightAssociativityWorksForExpressionNodeTypes() {
            AssertCorrect(JsExpression.Binary(ExpressionNodeType.Subtract,
                              JsExpression.Binary(ExpressionNodeType.Subtract,
                                  JsExpression.Number(1),
                                  JsExpression.Number(2)
                              ),
                              JsExpression.Number(3)
                          ),
                          "1 - 2 - 3");

            AssertCorrect(JsExpression.Binary(ExpressionNodeType.Subtract,
                              JsExpression.Number(1),
                              JsExpression.Binary(ExpressionNodeType.Subtract,
                                  JsExpression.Number(2),
                                  JsExpression.Number(3)
                              )
                          ),
                          "1 - (2 - 3)");
        }

        [Test]
        public void RightToLeftAssociativityWorksForExpressionNodeTypes() {
            AssertCorrect(JsExpression.Binary(ExpressionNodeType.Assign,
                              JsExpression.Number(1),
                              JsExpression.Binary(ExpressionNodeType.Assign,
                                  JsExpression.Number(2),
                                  JsExpression.Number(3)
                              )
                          ),
                          "1 = 2 = 3");

            AssertCorrect(JsExpression.Binary(ExpressionNodeType.Assign,
                              JsExpression.Binary(ExpressionNodeType.Assign,
                                  JsExpression.Number(1),
                                  JsExpression.Number(2)
                              ),
                              JsExpression.Number(3)
                          ),
                          "(1 = 2) = 3");
        }

        [Test]
        public void MultiplyHasHigherPrecedenceThanAdd() {
            AssertCorrect(JsExpression.Binary(ExpressionNodeType.Add,
                              JsExpression.Binary(ExpressionNodeType.Multiply,
                                  JsExpression.Number(1),
                                  JsExpression.Number(2)
                              ),
                              JsExpression.Number(3)
                          ),
                          "1 * 2 + 3");

            AssertCorrect(JsExpression.Binary(ExpressionNodeType.Add,
                              JsExpression.Number(1),
                              JsExpression.Binary(ExpressionNodeType.Multiply,
                                  JsExpression.Number(2),
                                  JsExpression.Number(3)
                              )
                          ),
                          "1 + 2 * 3");

            AssertCorrect(JsExpression.Binary(ExpressionNodeType.Multiply,
                              JsExpression.Binary(ExpressionNodeType.Add,
                                  JsExpression.Number(1),
                                  JsExpression.Number(2)
                              ),
                              JsExpression.Number(3)
                          ),
                          "(1 + 2) * 3");

            AssertCorrect(JsExpression.Binary(ExpressionNodeType.Multiply,
                              JsExpression.Number(1),
                              JsExpression.Binary(ExpressionNodeType.Add,
                                  JsExpression.Number(2),
                                  JsExpression.Number(3)
                              )
                          ),
                          "1 * (2 + 3)");
        }

        [Test]
        public void CommaIsParenthesizedAsAssignmentValue() {
            AssertCorrect(JsExpression.Binary(ExpressionNodeType.Assign,
                              JsExpression.Number(1),
                              JsExpression.Comma(
                                  JsExpression.Number(2),
                                  JsExpression.Number(3)
                              )
                          ),
                          "1 = (2, 3)");
        }

        [Test]
        public void CommaIsNotParenthesizedInsideOtherComma() {
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
                          "1, 2, 3, 4");
        }

        [Test]
        public void CommaIsParenthesizedInsideArrayLiteral() {
            AssertCorrect(JsExpression.ArrayLiteral(
                              JsExpression.Comma(
                                  JsExpression.Number(1),
                                  JsExpression.Number(2)
                              ),
                              JsExpression.Number(3),
                              JsExpression.Comma(
                                  JsExpression.Number(4),
                                  JsExpression.Number(5)
                              )
                          ),
                          "[(1, 2), 3, (4, 5)]");
        }

        [Test]
        public void AssignmentIsNotParenthesizedInsideArrayLiteral() {
            AssertCorrect(JsExpression.ArrayLiteral(
                              JsExpression.Binary(ExpressionNodeType.Assign,
                                  JsExpression.Number(1),
                                  JsExpression.Number(2)
                              )
                          ),
                          "[1 = 2]");
        }

        [Test]
        public void ConditionalIsAlwaysParenthesized() {
            AssertCorrect(JsExpression.Conditional(
                              JsExpression.Number(1),
                              JsExpression.Number(2),
                              JsExpression.Number(3)
                          ),
                          "(1 ? 2 : 3)");
        }

        [Test]
        public void ConditionalIsNotDoublyParenthesized() {
            AssertCorrect(JsExpression.Binary(ExpressionNodeType.Add,
                              JsExpression.Number(1),
                              JsExpression.Conditional(
                                  JsExpression.Number(2),
                                  JsExpression.Number(3),
                                  JsExpression.Number(4)
                              )
                          ),
                          "1 + (2 ? 3 : 4)");
        }

        [Test]
        public void MultiplicationInConditionalIsParenthesized() {
            AssertCorrect(JsExpression.Conditional(
                              JsExpression.Binary(ExpressionNodeType.Multiply,
                                  JsExpression.Number(1),
                                  JsExpression.Number(2)
                              ),
                              JsExpression.Binary(ExpressionNodeType.Multiply,
                                  JsExpression.Number(3),
                                  JsExpression.Number(4)
                              ),
                              JsExpression.Binary(ExpressionNodeType.Multiply,
                                  JsExpression.Number(5),
                                  JsExpression.Number(6)
                              )
                          ),
                          "((1 * 2) ? (3 * 4) : (5 * 6))");
        }

        [Test]
        public void ExpressionNodeTypeIsNotParenthesizedInsideConditional() {
            AssertCorrect(JsExpression.Conditional(
                              JsExpression.Unary(ExpressionNodeType.Negate, JsExpression.Number(1)),
                              JsExpression.Unary(ExpressionNodeType.Negate, JsExpression.Number(2)),
                              JsExpression.Unary(ExpressionNodeType.Negate, JsExpression.Number(3))
                          ),
                          "(-1 ? -2 : -3)");
        }

        [Test]
        public void CommaIsParenthesizedInsideInvocation() {
            AssertCorrect(JsExpression.Invocation(
                              JsExpression.Identifier("f"),
                              JsExpression.Comma(
                                  JsExpression.Number(1),
                                  JsExpression.Number(2)
                              ),
                              JsExpression.Number(3),
                              JsExpression.Comma(
                                  JsExpression.Number(4),
                                  JsExpression.Number(5)
                              )
                          ),
                          "f((1, 2), 3, (4, 5))");
        }

        [Test]
        public void CommaIsNotParenthesizedInsideIndexing() {
            AssertCorrect(JsExpression.Binary(ExpressionNodeType.Index,
                              JsExpression.Number(1),
                              JsExpression.Comma(
                                  JsExpression.Number(2),
                                  JsExpression.Number(3)
                              )
                          ),
                          "1[2, 3]");
        }

        [Test]
        public void NestedMemberExpressionsAreNotParenthesized() {
            AssertCorrect(JsExpression.MemberAccess(
                              JsExpression.MemberAccess(
                                  JsExpression.MemberAccess(
                                      JsExpression.Identifier("x"),
                                      "Member1"
                                  ),
                                  "Member2"
                              ),
                              "Member3"
                          ),
                          "x.Member1.Member2.Member3");
        }

        [Test]
        public void InvocationIsNotParenthesizedWhenUsedAsMemberAccessTarget() {
            AssertCorrect(JsExpression.MemberAccess(
                              JsExpression.Invocation(
                                  JsExpression.Number(1),
                                  new[] { JsExpression.Number(2) }
                              ),
                              "Member"
                          ),
                          "1(2).Member");
        }

        [Test]
        public void NestedNewExpressionsAreParenthesized() {
            // I don't know if this makes sense, but if it does, it should be correct.
            AssertCorrect(JsExpression.New(
                              JsExpression.New(
                                  JsExpression.New(
                                      JsExpression.Number(1)
                                  )
                              )
                          ),
                          "new (new (new 1())())()");
        }

        [Test]
        public void NewExpressionIsParenthesizedWhenItIsTheTargetOfAMemberAccess() {
            AssertCorrect(JsExpression.MemberAccess(
                              JsExpression.New(
                                  JsExpression.Number(1),
                                  new[] { JsExpression.Number(2) }
                              ),
                              "Member"
                          ),
                          "(new 1(2)).Member");
        }

        [Test]
        public void CommaIsParenthesizedInsideConstructorArgumentList() {
            AssertCorrect(JsExpression.New(
                              JsExpression.Number(10),
                              JsExpression.Comma(
                                  JsExpression.Number(1),
                                  JsExpression.Number(2)
                              ),
                              JsExpression.Number(3),
                              JsExpression.Comma(
                                  JsExpression.Number(4),
                                  JsExpression.Number(5)
                              )
                          ),
                          "new 10((1, 2), 3, (4, 5))");
        }

        [Test]
        public void AssignmentIsNotParenthesizedInsideConstructorArgumentList() {
            AssertCorrect(JsExpression.ArrayLiteral(
                              JsExpression.Binary(ExpressionNodeType.Assign,
                                  JsExpression.Number(1),
                                  JsExpression.Number(2)
                              )
                          ),
                          "[1 = 2]");
        }

        [Test]
        public void IdentifierIsNotParenthesizedWhenUsedAsConstructor() {
            AssertCorrect(JsExpression.New(
                              JsExpression.Identifier("X"),
                              new JsExpression[0]
                          ),
                          "new X()");
        }

        [Test]
        public void IncrementIsParenthesizedWhenUsedAsInvocationMethod() {
            AssertCorrect(JsExpression.Invocation(
                              JsExpression.Unary(ExpressionNodeType.PostfixMinusMinus,
                                  JsExpression.Identifier("x")
                              ),
                              new JsExpression[0]
                          ),
                          "(x--)()");
        }

        [Test]
        public void MemberAccessIsNotParenthesizedWhenUsedAsInvocationTarget() {
            AssertCorrect(JsExpression.Invocation(
                              JsExpression.MemberAccess(
                                  JsExpression.Identifier("x"),
                                  "Member"
                              ),
                              new JsExpression[0]
                          ),
                          "x.Member()");
        }

        [Test]
        public void ChainedFunctionCallsAreNotParenthtesized() {
            AssertCorrect(JsExpression.Invocation(
                              JsExpression.Invocation(
                                  JsExpression.Identifier("x"),
                                  new[] { JsExpression.Number(1) }
                              ),
                              new[] { JsExpression.Number(2) }
                          ),
                          "x(1)(2)");
        }

        [Test]
        public void ChainedFunctionCallsAndMemberAccessesAreNotParenthtesized() {
            AssertCorrect(JsExpression.Invocation(
                              JsExpression.MemberAccess(
                                  JsExpression.Invocation(
                                      JsExpression.MemberAccess(
                                          JsExpression.Invocation(
                                              JsExpression.MemberAccess(JsExpression.This, "x"),
                                              new[] { JsExpression.Number(1) }
                                          ), "y"),
                                      new[] { JsExpression.Number(2) }
                                   ), "z"),
                              new[] { JsExpression.Number(3) }
                          ),
                          "this.x(1).y(2).z(3)");
        }

        [Test]
        public void NewExpressionIsParenthesizedWhenBeingUsedAsInvocationTarget() {
            // Just to get rid of ambiguities
            AssertCorrect(JsExpression.Invocation(
                              JsExpression.New(
                                  JsExpression.Identifier("X"),
                                  new JsExpression[0]
                              ),
                              new[] { JsExpression.Number(1) }
                          ),
                          "(new X())(1)");
        }

        [Test]
        public void CreatingObjectOfNestedTypeDoesNotCauseUnnecessaryParentheses() {
            // Just to get rid of ambiguities
            AssertCorrect(JsExpression.New(
                              JsExpression.MemberAccess(
                                  JsExpression.MemberAccess(
                                      JsExpression.Identifier("X"),
                                      "Y"
                                  ),
                                  "Z"
                              )
                          ),
                          "new X.Y.Z()");
        }

        [Test]
        public void IncrementIsParenthesizedWhenBeingUsedAsInvocationTarget() {
            AssertCorrect(JsExpression.Invocation(
                              JsExpression.Unary(ExpressionNodeType.PostfixPlusPlus,
                                  JsExpression.Identifier("X")
                              ),
                              new[] { JsExpression.Number(1) }
                          ),
                          "(X++)(1)");
        }

        [Test]
        public void ExpressionNodeTypeIsNotParenthesizedWhenUsedAsBinaryArgument() {
            AssertCorrect(JsExpression.Binary(ExpressionNodeType.Multiply,
                              JsExpression.Unary(ExpressionNodeType.Negate,
                                  JsExpression.Identifier("X")
                              ),
                              JsExpression.Number(1)
                          ),
                          "-X * 1");
        }

        [Test]
        public void CommaExpressionIsParenthesizedInsideObjectLiteral() {
            AssertCorrect(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("x", JsExpression.Comma(JsExpression.Number(1), JsExpression.Number(2))),
                                                     new JsObjectLiteralProperty("y", JsExpression.Number(3))),
                          "{ x: (1, 2), y: 3 }");
        }

        [Test]
        public void FunctionIsParenthesizedWhenInvokedDirectly() {
            AssertCorrect(JsExpression.Invocation(
                              JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement, null)
                          ),
                          "(function() {\r\n})()");
        }

		[Test]
		public void LiteralExpressionIsParenthesizedInsideCommaOperator() {
			AssertCorrect(JsExpression.Comma(JsExpression.Identifier("a"),
			                                 JsExpression.Literal("_{0}_", new[] { JsExpression.Identifier("X") }),
			                                 JsExpression.Identifier("b")),
			              "a, (_X_), b");
		}

		[Test]
		public void ExpressionStatementsContainingOnlyAFunctionDefinitionParenthesizesThatDefinition() {
			AssertCorrect(new JsExpressionStatement(JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement)), "(function() {\r\n});\r\n");
		}

		[Test]
		public void NumberIsParenthesizedWhenUsedAsMemberAccessTarget() {
			AssertCorrect(JsExpression.MemberAccess(JsExpression.Number(1), "X"), "(1).X");
		}
    }
}
