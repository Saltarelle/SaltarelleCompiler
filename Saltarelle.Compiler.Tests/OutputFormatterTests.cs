using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Tests
{
    [TestFixture]
    public class OutpuFormatterTests
    {
        [Test]
        public void LeftToRightAssociativityWorksForBinaryOperators() {
            Assert.That(OutputFormatter.Format(new BinaryExpression(BinaryOperator.Subtract,
                                                   new BinaryExpression(BinaryOperator.Subtract,
                                                       ConstantExpression.Number(1),
                                                       ConstantExpression.Number(2)
                                                   ),
                                                   ConstantExpression.Number(3)
                                              )
                        ), Is.EqualTo("1 - 2 - 3"));

            Assert.That(OutputFormatter.Format(new BinaryExpression(BinaryOperator.Subtract,
                                                   ConstantExpression.Number(1),
                                                   new BinaryExpression(BinaryOperator.Subtract,
                                                       ConstantExpression.Number(2),
                                                       ConstantExpression.Number(3)
                                                   )
                                              )
                        ), Is.EqualTo("1 - (2 - 3)"));
        }

        [Test]
        public void RightToLeftAssociativityWorksForBinaryOperators() {
            Assert.That(OutputFormatter.Format(new BinaryExpression(BinaryOperator.Assign,
                                                   ConstantExpression.Number(1),
                                                   new BinaryExpression(BinaryOperator.Assign,
                                                       ConstantExpression.Number(2),
                                                       ConstantExpression.Number(3)
                                                   )
                                              )
                        ), Is.EqualTo("1 = 2 = 3"));

            Assert.That(OutputFormatter.Format(new BinaryExpression(BinaryOperator.Assign,
                                                   new BinaryExpression(BinaryOperator.Assign,
                                                       ConstantExpression.Number(1),
                                                       ConstantExpression.Number(2)
                                                   ),
                                                   ConstantExpression.Number(3)
                                              )
                        ), Is.EqualTo("(1 = 2) = 3"));
        }

        [Test]
        public void MultiplyHasHigherPrecedenceThanAdd() {
            Assert.That(OutputFormatter.Format(new BinaryExpression(BinaryOperator.Add,
                                                   new BinaryExpression(BinaryOperator.Multiply,
                                                       ConstantExpression.Number(1),
                                                       ConstantExpression.Number(2)
                                                   ),
                                                   ConstantExpression.Number(3)
                                              )
                        ), Is.EqualTo("1 * 2 + 3"));

            Assert.That(OutputFormatter.Format(new BinaryExpression(BinaryOperator.Add,
                                                   ConstantExpression.Number(1),
                                                   new BinaryExpression(BinaryOperator.Multiply,
                                                       ConstantExpression.Number(2),
                                                       ConstantExpression.Number(3)
                                                   )
                                              )
                        ), Is.EqualTo("1 + 2 * 3"));

            Assert.That(OutputFormatter.Format(new BinaryExpression(BinaryOperator.Multiply,
                                                   new BinaryExpression(BinaryOperator.Add,
                                                       ConstantExpression.Number(1),
                                                       ConstantExpression.Number(2)
                                                   ),
                                                   ConstantExpression.Number(3)
                                              )
                        ), Is.EqualTo("(1 + 2) * 3"));

            Assert.That(OutputFormatter.Format(new BinaryExpression(BinaryOperator.Multiply,
                                                   ConstantExpression.Number(1),
                                                   new BinaryExpression(BinaryOperator.Add,
                                                       ConstantExpression.Number(2),
                                                       ConstantExpression.Number(3)
                                                   )
                                              )
                        ), Is.EqualTo("1 * (2 + 3)"));
        }

        [Test]
        public void CommaIsParenthesizedAsAssignmentValue() {
            Assert.That(OutputFormatter.Format(new BinaryExpression(BinaryOperator.Assign,
                                                   ConstantExpression.Number(1),
                                                   new CommaExpression(
                                                       ConstantExpression.Number(2),
                                                       ConstantExpression.Number(3)
                                                   )
                                              )
                        ), Is.EqualTo("1 = (2, 3)"));
        }

        [Test]
        public void CommaIsNotParenthesizedInsideOtherComma() {
            Assert.That(OutputFormatter.Format(new CommaExpression(
                                                   new CommaExpression(
                                                       ConstantExpression.Number(1),
                                                       ConstantExpression.Number(2)
                                                   ),
                                                   new CommaExpression(
                                                       ConstantExpression.Number(3),
                                                       ConstantExpression.Number(4)
                                                   )
                                              )
                        ), Is.EqualTo("1, 2, 3, 4"));
        }

        [Test]
        public void CommaIsParenthesizedInsideArrayLiteral() {
            Assert.That(OutputFormatter.Format(new ArrayLiteralExpression(
                                                   new CommaExpression(
                                                       ConstantExpression.Number(1),
                                                       ConstantExpression.Number(2)
                                                   ),
                                                   ConstantExpression.Number(3),
                                                   new CommaExpression(
                                                       ConstantExpression.Number(4),
                                                       ConstantExpression.Number(5)
                                                   )
                                              )
                        ), Is.EqualTo("[(1, 2), 3, (4, 5)]"));
        }

        [Test]
        public void AssignmentIsNotParenthesizedInsideArrayLiteral() {
            Assert.That(OutputFormatter.Format(new ArrayLiteralExpression(
                                                   new BinaryExpression(BinaryOperator.Assign,
                                                       ConstantExpression.Number(1),
                                                       ConstantExpression.Number(2)
                                                   )
                                              )
                        ), Is.EqualTo("[1 = 2]"));
        }

        [Test]
        public void EmptyArrayLiteralWorks() {
            Assert.That(OutputFormatter.Format(new ArrayLiteralExpression()), Is.EqualTo("[]"));
        }

        [Test]
        public void OneElementArrayLiteralWorks() {
            Assert.That(OutputFormatter.Format(new ArrayLiteralExpression(ConstantExpression.Number(1))), Is.EqualTo("[1]"));
        }

        [Test]
        public void ConditionalIsAlwaysParenthesized() {
            Assert.That(OutputFormatter.Format(new ConditionalExpression(
                                                   ConstantExpression.Number(1),
                                                   ConstantExpression.Number(2),
                                                   ConstantExpression.Number(3)
                                              )
                        ), Is.EqualTo("(1 ? 2 : 3)"));
        }

        [Test]
        public void ConditionalIsNotDoublyParenthesized() {
            Assert.That(OutputFormatter.Format(new BinaryExpression(BinaryOperator.Add,
                                                   ConstantExpression.Number(1),
                                                   new ConditionalExpression(
                                                       ConstantExpression.Number(2),
                                                       ConstantExpression.Number(3),
                                                       ConstantExpression.Number(4)
                                                  )
                                              )
                        ), Is.EqualTo("1 + (2 ? 3 : 4)"));
        }

        [Test]
        public void MultiplicationInConditionalIsParenthesized() {
            Assert.That(OutputFormatter.Format(new ConditionalExpression(
                                                   new BinaryExpression(BinaryOperator.Multiply,
                                                       ConstantExpression.Number(1),
                                                       ConstantExpression.Number(2)
                                                   ),
                                                   new BinaryExpression(BinaryOperator.Multiply,
                                                       ConstantExpression.Number(3),
                                                       ConstantExpression.Number(4)
                                                   ),
                                                   new BinaryExpression(BinaryOperator.Multiply,
                                                       ConstantExpression.Number(5),
                                                       ConstantExpression.Number(6)
                                                   )
                                              )
                        ), Is.EqualTo("((1 * 2) ? (3 * 4) : (5 * 6))"));
        }

        [Test]
        public void UnaryOperatorIsNotParenthesizedInsideConditional() {
            Assert.That(OutputFormatter.Format(new ConditionalExpression(
                                                   new UnaryExpression(UnaryOperator.Negate, ConstantExpression.Number(1)),
                                                   new UnaryExpression(UnaryOperator.Negate, ConstantExpression.Number(2)),
                                                   new UnaryExpression(UnaryOperator.Negate, ConstantExpression.Number(3))
                                              )
                        ), Is.EqualTo("(-1 ? -2 : -3)"));
        }

        [Test]
        public void CommaIsParenthesizedInsideInvocation() {
            Assert.That(OutputFormatter.Format(new InvocationExpression(
                                                   new IdentifierExpression("f"),
                                                   new Expression[] {
                                                       new CommaExpression(
                                                           ConstantExpression.Number(1),
                                                           ConstantExpression.Number(2)
                                                       ),
                                                       ConstantExpression.Number(3),
                                                       new CommaExpression(
                                                           ConstantExpression.Number(4),
                                                           ConstantExpression.Number(5)
                                                       )
                                                   }
                                              )
                        ), Is.EqualTo("f((1, 2), 3, (4, 5))"));
        }

        [Test]
        public void IndexingWorks() {
            Assert.That(OutputFormatter.Format(new BinaryExpression(BinaryOperator.Index,
                                                   new BinaryExpression(BinaryOperator.Index,
                                                       ConstantExpression.Number(1),
                                                       ConstantExpression.Number(2)
                                                   ),
                                                   ConstantExpression.Number(3)
                                              )
                        ), Is.EqualTo("1[2][3]"));
        }

        [Test]
        public void CommaIsNotParenthesizedInsideIndexing() {
            Assert.That(OutputFormatter.Format(new BinaryExpression(BinaryOperator.Index,
                                                   ConstantExpression.Number(1),
                                                   new CommaExpression(
                                                       ConstantExpression.Number(2),
                                                       ConstantExpression.Number(3)
                                                   )
                                              )
                        ), Is.EqualTo("1[2, 3]"));
        }

        [Test]
        public void StringLiteralsAreCorrectlyEncoded() {
            Assert.That(OutputFormatter.Format(ConstantExpression.String("x")), Is.EqualTo("'x'"));
            Assert.That(OutputFormatter.Format(ConstantExpression.String("\"")), Is.EqualTo("'\"'"));
            Assert.That(OutputFormatter.Format(ConstantExpression.String("'")), Is.EqualTo("'\\''"));
            Assert.That(OutputFormatter.Format(ConstantExpression.String("\r\n/\\")), Is.EqualTo("'\\r\\n/\\\\'"));
        }

        [Test]
        public void RegularExpressionLiteralsAreCorrectlyEncoded() {
            Assert.That(OutputFormatter.Format(ConstantExpression.Regexp("x")), Is.EqualTo("/x/"));
            Assert.That(OutputFormatter.Format(ConstantExpression.Regexp("\"")), Is.EqualTo("/\"/"));
            Assert.That(OutputFormatter.Format(ConstantExpression.Regexp("/")), Is.EqualTo("/\\//"));
            Assert.That(OutputFormatter.Format(ConstantExpression.Regexp("\r\n/\\")), Is.EqualTo("/\\r\\n\\/\\\\/"));
            Assert.That(OutputFormatter.Format(ConstantExpression.Regexp("x", "g")), Is.EqualTo("/x/g"));
        }

        [Test]
        public void NullLiteralWorks() {
            Assert.That(OutputFormatter.Format(ConstantExpression.Null), Is.EqualTo("null"));
        }

        [Test]
        public void NumbersAreCorrectlyRepresented() {
            Assert.That(OutputFormatter.Format(ConstantExpression.Number(1)), Is.EqualTo("1"));
            Assert.That(OutputFormatter.Format(ConstantExpression.Number(1.25)), Is.EqualTo("1.25"));
            Assert.That(OutputFormatter.Format(ConstantExpression.Number(double.PositiveInfinity)), Is.EqualTo("Infinity"));
            Assert.That(OutputFormatter.Format(ConstantExpression.Number(double.NegativeInfinity)), Is.EqualTo("-Infinity"));
            Assert.That(OutputFormatter.Format(ConstantExpression.Number(double.NaN)), Is.EqualTo("NaN"));
        }

        [Test]
        public void NestedMemberExpressionsAreNotParenthesized() {
            Assert.That(OutputFormatter.Format(new MemberAccessExpression(
                                                   new MemberAccessExpression(
                                                       new MemberAccessExpression(
                                                           ConstantExpression.Number(1),
                                                           "Member1"
                                                       ),
                                                       "Member2"
                                                   ),
                                                   "Member3"
                                               )
                        ), Is.EqualTo("1.Member1.Member2.Member3"));
        }

        [Test]
        public void InvocationIsParenthesizedWhenUsedAsMemberAccessTarget() {
            Assert.That(OutputFormatter.Format(new MemberAccessExpression(
                                                   new InvocationExpression(
                                                       ConstantExpression.Number(1),
                                                       new[] { ConstantExpression.Number(2) }
                                                   ),
                                                   "Member"
                                               )
                        ), Is.EqualTo("(1(2)).Member"));
        }

        [Test]
        public void NestedNewExpressionsAreParenthesized() {
            // I don't know if this makes sense, but if it does, it should be correct.
            Assert.That(OutputFormatter.Format(new NewExpression(
                                                   new NewExpression(
                                                       new NewExpression(
                                                           ConstantExpression.Number(1),
                                                           new Expression[0]
                                                       ),
                                                       new Expression[0]
                                                   ),
                                                   new Expression[0]
                                               )
                        ), Is.EqualTo("new (new (new 1())())()"));
        }

        [Test]
        public void NewExpressionsAndMemberExpressionsAreParenthesizedInsideEachOther() {
            // Other stuff from the department "strange edge cases". Should be parenthesized to cause as little trouble as possible.
            Assert.That(OutputFormatter.Format(new MemberAccessExpression(
                                                   new NewExpression(
                                                       ConstantExpression.Number(1),
                                                       new[] { ConstantExpression.Number(2) }
                                                   ),
                                                   "Member"
                                               )
                        ), Is.EqualTo("(new 1(2)).Member"));

            // Other stuff from the department "strange edge cases". Should be parenthesized to cause as little trouble as possible.
            Assert.That(OutputFormatter.Format(new NewExpression(
                                                   new MemberAccessExpression(
                                                       ConstantExpression.Number(1),
                                                       "Member"
                                                   ),
                                                   new[] { ConstantExpression.Number(2) }
                                               )
                        ), Is.EqualTo("new (1.Member)(2)"));
        }

        [Test]
        public void CommaIsParenthesizedInsideConstructorArgumentList() {
            Assert.That(OutputFormatter.Format(new NewExpression(
                                                   ConstantExpression.Number(10),
                                                   new Expression[] {
                                                       new CommaExpression(
                                                           ConstantExpression.Number(1),
                                                           ConstantExpression.Number(2)
                                                       ),
                                                       ConstantExpression.Number(3),
                                                       new CommaExpression(
                                                           ConstantExpression.Number(4),
                                                           ConstantExpression.Number(5)
                                                       )
                                                   }
                                              )
                        ), Is.EqualTo("new 10((1, 2), 3, (4, 5))"));
        }

        [Test]
        public void AssignmentIsNotParenthesizedInsideConstructorArgumentList() {
            Assert.That(OutputFormatter.Format(new ArrayLiteralExpression(
                                                   new BinaryExpression(BinaryOperator.Assign,
                                                       ConstantExpression.Number(1),
                                                       ConstantExpression.Number(2)
                                                   )
                                              )
                        ), Is.EqualTo("[1 = 2]"));
        }

        [Test]
        public void IdentifierIsNotParenthesizedWhenUsedAsConstructor() {
            Assert.That(OutputFormatter.Format(new NewExpression(
                                                   new IdentifierExpression("X"),
                                                   new Expression[0]
                                              )
                        ), Is.EqualTo("new X()"));
        }

        [Test]
        public void IdentifierIsOutputCorrectly() {
            Assert.That(OutputFormatter.Format(new IdentifierExpression("SomeIdentifier")), Is.EqualTo("SomeIdentifier"));
        }

        [Test]
        public void IncrementIsParenthesizedWhenUsedAsInvocationMethod() {
            Assert.That(OutputFormatter.Format(new InvocationExpression(
                                                   new UnaryExpression(UnaryOperator.PostfixMinusMinus,
                                                       new IdentifierExpression("x")
                                                   ),
                                                   new Expression[0]
                                              )
                        ), Is.EqualTo("(x--)()"));
        }

        [Test]
        public void MemberAccessIsNotParenthesizedWhenUsedAsInvocationTarget() {
            Assert.That(OutputFormatter.Format(new InvocationExpression(
                                                   new MemberAccessExpression(
                                                       new IdentifierExpression("x"),
                                                       "Member"
                                                   ),
                                                   new Expression[0]
                                              )
                        ), Is.EqualTo("x.Member()"));
        }

        [Test]
        public void ChainedFunctionCallsAreNotParenthtesized() {
            Assert.That(OutputFormatter.Format(new InvocationExpression(
                                                   new InvocationExpression(
                                                       new IdentifierExpression("x"),
                                                       new[] { ConstantExpression.Number(1) }
                                                   ),
                                                   new[] { ConstantExpression.Number(2) }
                                               )
                        ), Is.EqualTo("x(1)(2)"));
        }

        [Test]
        public void NewExpressionIsParenthesizedWhenBeingUsedAsInvocationTarget() {
            // Just to get rid of ambiguities
            Assert.That(OutputFormatter.Format(new InvocationExpression(
                                                   new NewExpression(
                                                       new IdentifierExpression("X"),
                                                       new Expression[0]
                                                   ),
                                                   new[] { ConstantExpression.Number(1) }
                                               )
                        ), Is.EqualTo("(new X())(1)"));
        }

        [Test]
        public void TestUnaryExpressions() {
            Assert.Fail("TODO");
        }

        [Test]
        public void TestBinaryExpressions() {
            Assert.Fail("TODO More tests");
        }
    }
}
