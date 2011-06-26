using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Tests
{
    [TestFixture]
    public class OutpuFormatterTestss
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
            Assert.Fail();
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
        public void TestIndexing() {
            Assert.Fail("TODO");
        }
    }
}
