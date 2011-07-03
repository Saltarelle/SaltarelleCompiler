using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests
{
    [TestFixture]
    public class OutpuFormatterTests
    {
        [Test]
        public void LeftToRightAssociativityWorksForExpressionNodeTypes() {
            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Subtract,
                                                   Expression.Binary(ExpressionNodeType.Subtract,
                                                       Expression.Number(1),
                                                       Expression.Number(2)
                                                   ),
                                                   Expression.Number(3)
                                               )
                        ), Is.EqualTo("1 - 2 - 3"));

            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Subtract,
                                                   Expression.Number(1),
                                                   Expression.Binary(ExpressionNodeType.Subtract,
                                                       Expression.Number(2),
                                                       Expression.Number(3)
                                                   )
                                               )
                        ), Is.EqualTo("1 - (2 - 3)"));
        }

        [Test]
        public void RightToLeftAssociativityWorksForExpressionNodeTypes() {
            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Assign,
                                                   Expression.Number(1),
                                                   Expression.Binary(ExpressionNodeType.Assign,
                                                       Expression.Number(2),
                                                       Expression.Number(3)
                                                   )
                                               )
                        ), Is.EqualTo("1 = 2 = 3"));

            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Assign,
                                                   Expression.Binary(ExpressionNodeType.Assign,
                                                       Expression.Number(1),
                                                       Expression.Number(2)
                                                   ),
                                                   Expression.Number(3)
                                               )
                        ), Is.EqualTo("(1 = 2) = 3"));
        }

        [Test]
        public void MultiplyHasHigherPrecedenceThanAdd() {
            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Add,
                                                   Expression.Binary(ExpressionNodeType.Multiply,
                                                       Expression.Number(1),
                                                       Expression.Number(2)
                                                   ),
                                                   Expression.Number(3)
                                               )
                        ), Is.EqualTo("1 * 2 + 3"));

            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Add,
                                                   Expression.Number(1),
                                                   Expression.Binary(ExpressionNodeType.Multiply,
                                                       Expression.Number(2),
                                                       Expression.Number(3)
                                                   )
                                               )
                        ), Is.EqualTo("1 + 2 * 3"));

            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Multiply,
                                                   Expression.Binary(ExpressionNodeType.Add,
                                                       Expression.Number(1),
                                                       Expression.Number(2)
                                                   ),
                                                   Expression.Number(3)
                                               )
                        ), Is.EqualTo("(1 + 2) * 3"));

            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Multiply,
                                                   Expression.Number(1),
                                                   Expression.Binary(ExpressionNodeType.Add,
                                                       Expression.Number(2),
                                                       Expression.Number(3)
                                                   )
                                               )
                        ), Is.EqualTo("1 * (2 + 3)"));
        }

        [Test]
        public void CommaIsParenthesizedAsAssignmentValue() {
            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Assign,
                                                   Expression.Number(1),
                                                   Expression.Comma(
                                                       Expression.Number(2),
                                                       Expression.Number(3)
                                                   )
                                               )
                        ), Is.EqualTo("1 = (2, 3)"));
        }

        [Test]
        public void CommaIsNotParenthesizedInsideOtherComma() {
            Assert.That(OutputFormatter.Format(Expression.Comma(
                                                   Expression.Comma(
                                                       Expression.Number(1),
                                                       Expression.Number(2)
                                                   ),
                                                   Expression.Comma(
                                                       Expression.Number(3),
                                                       Expression.Number(4)
                                                   )
                                               )
                        ), Is.EqualTo("1, 2, 3, 4"));
        }

        [Test]
        public void CommaIsParenthesizedInsideArrayLiteral() {
            Assert.That(OutputFormatter.Format(Expression.ArrayLiteral(
                                                   Expression.Comma(
                                                       Expression.Number(1),
                                                       Expression.Number(2)
                                                   ),
                                                   Expression.Number(3),
                                                   Expression.Comma(
                                                       Expression.Number(4),
                                                       Expression.Number(5)
                                                   )
                                               )
                        ), Is.EqualTo("[(1, 2), 3, (4, 5)]"));
        }

        [Test]
        public void AssignmentIsNotParenthesizedInsideArrayLiteral() {
            Assert.That(OutputFormatter.Format(Expression.ArrayLiteral(
                                                   Expression.Binary(ExpressionNodeType.Assign,
                                                       Expression.Number(1),
                                                       Expression.Number(2)
                                                   )
                                               )
                        ), Is.EqualTo("[1 = 2]"));
        }

        [Test]
        public void EmptyArrayLiteralWorks() {
            Assert.That(OutputFormatter.Format(Expression.ArrayLiteral()), Is.EqualTo("[]"));
        }

        [Test]
        public void OneElementArrayLiteralWorks() {
            Assert.That(OutputFormatter.Format(Expression.ArrayLiteral(Expression.Number(1))), Is.EqualTo("[1]"));
        }

        [Test]
        public void ConditionalIsAlwaysParenthesized() {
            Assert.That(OutputFormatter.Format(Expression.Conditional(
                                                   Expression.Number(1),
                                                   Expression.Number(2),
                                                   Expression.Number(3)
                                               )
                        ), Is.EqualTo("(1 ? 2 : 3)"));
        }

        [Test]
        public void ConditionalIsNotDoublyParenthesized() {
            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Add,
                                                   Expression.Number(1),
                                                   Expression.Conditional(
                                                       Expression.Number(2),
                                                       Expression.Number(3),
                                                       Expression.Number(4)
                                                   )
                                               )
                        ), Is.EqualTo("1 + (2 ? 3 : 4)"));
        }

        [Test]
        public void MultiplicationInConditionalIsParenthesized() {
            Assert.That(OutputFormatter.Format(Expression.Conditional(
                                                   Expression.Binary(ExpressionNodeType.Multiply,
                                                       Expression.Number(1),
                                                       Expression.Number(2)
                                                   ),
                                                   Expression.Binary(ExpressionNodeType.Multiply,
                                                       Expression.Number(3),
                                                       Expression.Number(4)
                                                   ),
                                                   Expression.Binary(ExpressionNodeType.Multiply,
                                                       Expression.Number(5),
                                                       Expression.Number(6)
                                                   )
                                               )
                        ), Is.EqualTo("((1 * 2) ? (3 * 4) : (5 * 6))"));
        }

        [Test]
        public void ExpressionNodeTypeIsNotParenthesizedInsideConditional() {
            Assert.That(OutputFormatter.Format(Expression.Conditional(
                                                   Expression.Unary(ExpressionNodeType.Negate, Expression.Number(1)),
                                                   Expression.Unary(ExpressionNodeType.Negate, Expression.Number(2)),
                                                   Expression.Unary(ExpressionNodeType.Negate, Expression.Number(3))
                                               )
                        ), Is.EqualTo("(-1 ? -2 : -3)"));
        }

        [Test]
        public void CommaIsParenthesizedInsideInvocation() {
            Assert.That(OutputFormatter.Format(Expression.Invocation(
                                                   Expression.Identifier("f"),
                                                   Expression.Comma(
                                                       Expression.Number(1),
                                                       Expression.Number(2)
                                                   ),
                                                   Expression.Number(3),
                                                   Expression.Comma(
                                                       Expression.Number(4),
                                                       Expression.Number(5)
                                                   )
                                               )
                        ), Is.EqualTo("f((1, 2), 3, (4, 5))"));
        }

        [Test]
        public void IndexingWorks() {
            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Index,
                                                   Expression.Binary(ExpressionNodeType.Index,
                                                       Expression.Number(1),
                                                       Expression.Number(2)
                                                   ),
                                                   Expression.Number(3)
                                               )
                        ), Is.EqualTo("1[2][3]"));
        }

        [Test]
        public void CommaIsNotParenthesizedInsideIndexing() {
            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Index,
                                                   Expression.Number(1),
                                                   Expression.Comma(
                                                       Expression.Number(2),
                                                       Expression.Number(3)
                                                   )
                                               )
                        ), Is.EqualTo("1[2, 3]"));
        }

        [Test]
        public void StringLiteralsAreCorrectlyEncoded() {
            Assert.That(OutputFormatter.Format(Expression.String("x")), Is.EqualTo("'x'"));
            Assert.That(OutputFormatter.Format(Expression.String("\"")), Is.EqualTo("'\"'"));
            Assert.That(OutputFormatter.Format(Expression.String("'")), Is.EqualTo("'\\''"));
            Assert.That(OutputFormatter.Format(Expression.String("\r\n/\\")), Is.EqualTo("'\\r\\n/\\\\'"));
        }

        [Test]
        public void RegularExpressionLiteralsAreCorrectlyEncoded() {
            Assert.That(OutputFormatter.Format(Expression.Regexp("x")), Is.EqualTo("/x/"));
            Assert.That(OutputFormatter.Format(Expression.Regexp("\"")), Is.EqualTo("/\"/"));
            Assert.That(OutputFormatter.Format(Expression.Regexp("/")), Is.EqualTo("/\\//"));
            Assert.That(OutputFormatter.Format(Expression.Regexp("\r\n/\\")), Is.EqualTo("/\\r\\n\\/\\\\/"));
            Assert.That(OutputFormatter.Format(Expression.Regexp("x", "g")), Is.EqualTo("/x/g"));
        }

        [Test]
        public void NullLiteralWorks() {
            Assert.That(OutputFormatter.Format(Expression.Null), Is.EqualTo("null"));
        }

        [Test]
        public void NumbersAreCorrectlyRepresented() {
            Assert.That(OutputFormatter.Format(Expression.Number(1)), Is.EqualTo("1"));
            Assert.That(OutputFormatter.Format(Expression.Number(1.25)), Is.EqualTo("1.25"));
            Assert.That(OutputFormatter.Format(Expression.Number(double.PositiveInfinity)), Is.EqualTo("Infinity"));
            Assert.That(OutputFormatter.Format(Expression.Number(double.NegativeInfinity)), Is.EqualTo("-Infinity"));
            Assert.That(OutputFormatter.Format(Expression.Number(double.NaN)), Is.EqualTo("NaN"));
        }

        [Test]
        public void NestedMemberExpressionsAreNotParenthesized() {
            Assert.That(OutputFormatter.Format(Expression.MemberAccess(
                                                   Expression.MemberAccess(
                                                       Expression.MemberAccess(
                                                           Expression.Number(1),
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
            Assert.That(OutputFormatter.Format(Expression.MemberAccess(
                                                   Expression.Invocation(
                                                       Expression.Number(1),
                                                       new[] { Expression.Number(2) }
                                                   ),
                                                   "Member"
                                               )
                        ), Is.EqualTo("(1(2)).Member"));
        }

        [Test]
        public void NestedNewExpressionsAreParenthesized() {
            // I don't know if this makes sense, but if it does, it should be correct.
            Assert.That(OutputFormatter.Format(Expression.New(
                                                   Expression.New(
                                                       Expression.New(
                                                           Expression.Number(1)
                                                       )
                                                   )
                                               )
                        ), Is.EqualTo("new (new (new 1())())()"));
        }

        [Test]
        public void NewExpressionsAndMemberExpressionsAreParenthesizedInsideEachOther() {
            // Other stuff from the department "strange edge cases". Should be parenthesized to cause as little trouble as possible.
            Assert.That(OutputFormatter.Format(Expression.MemberAccess(
                                                   Expression.New(
                                                       Expression.Number(1),
                                                       new[] { Expression.Number(2) }
                                                   ),
                                                   "Member"
                                               )
                        ), Is.EqualTo("(new 1(2)).Member"));

            // Other stuff from the department "strange edge cases". Should be parenthesized to cause as little trouble as possible.
            Assert.That(OutputFormatter.Format(Expression.New(
                                                   Expression.MemberAccess(
                                                       Expression.Number(1),
                                                       "Member"
                                                   ),
                                                   new[] { Expression.Number(2) }
                                               )
                        ), Is.EqualTo("new (1.Member)(2)"));
        }

        [Test]
        public void CommaIsParenthesizedInsideConstructorArgumentList() {
            Assert.That(OutputFormatter.Format(Expression.New(
                                                   Expression.Number(10),
                                                   Expression.Comma(
                                                       Expression.Number(1),
                                                       Expression.Number(2)
                                                   ),
                                                   Expression.Number(3),
                                                   Expression.Comma(
                                                       Expression.Number(4),
                                                       Expression.Number(5)
                                                   )
                                               )
                        ), Is.EqualTo("new 10((1, 2), 3, (4, 5))"));
        }

        [Test]
        public void AssignmentIsNotParenthesizedInsideConstructorArgumentList() {
            Assert.That(OutputFormatter.Format(Expression.ArrayLiteral(
                                                   Expression.Binary(ExpressionNodeType.Assign,
                                                       Expression.Number(1),
                                                       Expression.Number(2)
                                                   )
                                               )
                        ), Is.EqualTo("[1 = 2]"));
        }

        [Test]
        public void IdentifierIsNotParenthesizedWhenUsedAsConstructor() {
            Assert.That(OutputFormatter.Format(Expression.New(
                                                   Expression.Identifier("X"),
                                                   new Expression[0]
                                               )
                        ), Is.EqualTo("new X()"));
        }

        [Test]
        public void IdentifierIsOutputCorrectly() {
            Assert.That(OutputFormatter.Format(Expression.Identifier("SomeIdentifier")), Is.EqualTo("SomeIdentifier"));
        }

        [Test]
        public void IncrementIsParenthesizedWhenUsedAsInvocationMethod() {
            Assert.That(OutputFormatter.Format(Expression.Invocation(
                                                   Expression.Unary(ExpressionNodeType.PostfixMinusMinus,
                                                       Expression.Identifier("x")
                                                   ),
                                                   new Expression[0]
                                               )
                        ), Is.EqualTo("(x--)()"));
        }

        [Test]
        public void MemberAccessIsNotParenthesizedWhenUsedAsInvocationTarget() {
            Assert.That(OutputFormatter.Format(Expression.Invocation(
                                                   Expression.MemberAccess(
                                                       Expression.Identifier("x"),
                                                       "Member"
                                                   ),
                                                   new Expression[0]
                                               )
                        ), Is.EqualTo("x.Member()"));
        }

        [Test]
        public void ChainedFunctionCallsAreNotParenthtesized() {
            Assert.That(OutputFormatter.Format(Expression.Invocation(
                                                   Expression.Invocation(
                                                       Expression.Identifier("x"),
                                                       new[] { Expression.Number(1) }
                                                   ),
                                                   new[] { Expression.Number(2) }
                                               )
                        ), Is.EqualTo("x(1)(2)"));
        }

        [Test]
        public void NewExpressionIsParenthesizedWhenBeingUsedAsInvocationTarget() {
            // Just to get rid of ambiguities
            Assert.That(OutputFormatter.Format(Expression.Invocation(
                                                   Expression.New(
                                                       Expression.Identifier("X"),
                                                       new Expression[0]
                                                   ),
                                                   new[] { Expression.Number(1) }
                                               )
                        ), Is.EqualTo("(new X())(1)"));
        }

        [Test]
        public void IncrementIsParenthesizedWhenBeingUsedAsInvocationTarget() {
            Assert.That(OutputFormatter.Format(Expression.Invocation(
                                                   Expression.Unary(ExpressionNodeType.PostfixPlusPlus,
                                                       Expression.Identifier("X")
                                                   ),
                                                   new[] { Expression.Number(1) }
                                               )
                        ), Is.EqualTo("(X++)(1)"));
        }

        [Test]
        public void ExpressionNodeTypeIsNotParenthesizedWhenUsedAsBinaryArgument() {
            Assert.That(OutputFormatter.Format(Expression.Binary(ExpressionNodeType.Multiply,
                                                   Expression.Unary(ExpressionNodeType.Negate,
                                                       Expression.Identifier("X")
                                                   ),
                                                   Expression.Number(1)
                                               )
                        ), Is.EqualTo("-X * 1"));
        }

        [Test]
        public void ExpressionNodeTypesAreParenthesizedInsideEachother() {
            // Just to get rid of ambiguities
            Assert.That(OutputFormatter.Format(Expression.Unary(ExpressionNodeType.PostfixPlusPlus,
                                                   Expression.Unary(ExpressionNodeType.LogicalNot,
                                                       Expression.Identifier("X")
                                                   )
                                               )
                        ), Is.EqualTo("(!X)++"));

            Assert.That(OutputFormatter.Format(Expression.Unary(ExpressionNodeType.LogicalNot,
                                                   Expression.Unary(ExpressionNodeType.PostfixPlusPlus,
                                                       Expression.Identifier("X")
                                                   )
                                               )
                        ), Is.EqualTo("!(X++)"));
        }

        [Test]
        public void EmptyObjectLiteralIsOutputCorrectly() {
            Assert.That(OutputFormatter.Format(Expression.ObjectLiteral()), Is.EqualTo("{}"));
        }

        [Test]
        public void ObjectLiteralWithOneValueIsOutputCorrectly() {
            Assert.That(OutputFormatter.Format(Expression.ObjectLiteral(new ObjectLiteralProperty("x", Expression.Number(1)))), Is.EqualTo("{ x: 1 }"));
        }

        [Test]
        public void ObjectLiteralWithThreeValuesIsOutputCorrectly() {
            Assert.That(OutputFormatter.Format(Expression.ObjectLiteral(new ObjectLiteralProperty("x", Expression.Number(1)),
                                                                        new ObjectLiteralProperty("y", Expression.Number(2)),
                                                                        new ObjectLiteralProperty("z", Expression.Number(3)))
                       ), Is.EqualTo("{ x: 1, y: 2, z: 3 }"));
        }

        [Test]
        public void ObjectLiteralWithNumericPropertyWorks() {
            Assert.That(OutputFormatter.Format(Expression.ObjectLiteral(new ObjectLiteralProperty("1", Expression.Number(2)))), Is.EqualTo("{ '1': 2 }"));
        }

        [Test]
        public void ObjectLiteralWithInvalidIdentifierPropertyWorks() {
            Assert.That(OutputFormatter.Format(Expression.ObjectLiteral(new ObjectLiteralProperty("a\\b", Expression.Number(1)))), Is.EqualTo("{ 'a\\\\b': 1 }"));
        }

        [Test]
        public void CommaExpressionIsParenthesizedInsideObjectLiteral() {
            Assert.That(OutputFormatter.Format(Expression.ObjectLiteral(new ObjectLiteralProperty("x", Expression.Comma(Expression.Number(1), Expression.Number(2))),
                                                                        new ObjectLiteralProperty("y", Expression.Number(3)))
                        ), Is.EqualTo("{ x: (1, 2), y: 3 }"));
        }

        [Test]
        public void EmptyFunctionDefinitionWithoutNameIsCorrectlyOutput() {
            Assert.That(OutputFormatter.Format(Expression.FunctionDefinition(new string[0], BlockStatement.Empty, null)), Is.EqualTo("function() {}"));
        }

        [Test]
        public void EmptyFunctionDefinitionWithNameIsCorrectlyOutput() {
            Assert.That(OutputFormatter.Format(Expression.FunctionDefinition(new string[0], BlockStatement.Empty, "test")), Is.EqualTo("function test() {}"));
        }

        [Test]
        public void EmptyFunctionDefinitionsWithArgumentsAreCorrectlyOutput() {
            Assert.That(OutputFormatter.Format(Expression.FunctionDefinition(new[] { "a" }, BlockStatement.Empty, null)), Is.EqualTo("function(a) {}"));
            Assert.That(OutputFormatter.Format(Expression.FunctionDefinition(new[] { "a", "b" }, BlockStatement.Empty, null)), Is.EqualTo("function(a, b) {}"));
            Assert.That(OutputFormatter.Format(Expression.FunctionDefinition(new[] { "a", "b", "c" }, BlockStatement.Empty, "test")), Is.EqualTo("function test(a, b, c) {}"));
        }

        [Test]
        public void FunctionIsParenthesizedWhenInvokedDirectly() {
            Assert.That(OutputFormatter.Format(Expression.Invocation(
                                                   Expression.FunctionDefinition(new string[0], BlockStatement.Empty, null)
                                               )
                        ), Is.EqualTo("(function() {})()"));
        }

        [Test, Ignore("Can't yet output function definitions.")]
        public void FunctionDefinitionWithContentIsCorrectlyOutput() {
            Assert.That(OutputFormatter.Format(Expression.FunctionDefinition(new string[0], new ReturnStatement(Expression.Null))), Is.EqualTo("function() { return null; }"));
        }

        [Test]
        public void BinaryOperatorsAreCorrectlyOutput() {
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
                var expr = Expression.Unary(oper, Expression.Identifier("a"));
                Assert.That(OutputFormatter.Format(expr), Is.EqualTo(string.Format(operators[oper], "a")));
            }
        }

        [Test]
        public void UnaryOperatorssAreCorrectlyOutput() {
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
                                                                         { ExpressionNodeType.RightShiftAssign, "{0} >>= {1}" },
                                                                         { ExpressionNodeType.UnsignedRightShiftAssign, "{0} >>>= {1}" },
                                                                         { ExpressionNodeType.BitwiseAndAssign, "{0} &= {1}" },
                                                                         { ExpressionNodeType.BitwiseOrAssign, "{0} |= {1}" },
                                                                         { ExpressionNodeType.BitwiseXOrAssign, "{0} ^= {1}" },
                                                                       };

            for (var oper = ExpressionNodeType.BinaryFirst; oper <= ExpressionNodeType.BinaryLast; oper++) {
                Assert.That(operators.ContainsKey(oper), string.Format("Unexpected operator {0}", oper));
                var expr = Expression.Binary(oper, Expression.Identifier("a"), Expression.Identifier("b"));
                Assert.That(OutputFormatter.Format(expr), Is.EqualTo(string.Format(operators[oper], "a", "b")));
            }
        }
    }
}
