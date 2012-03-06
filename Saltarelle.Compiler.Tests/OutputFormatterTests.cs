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
            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Subtract,
                                                   JsExpression.Binary(ExpressionNodeType.Subtract,
                                                       JsExpression.Number(1),
                                                       JsExpression.Number(2)
                                                   ),
                                                   JsExpression.Number(3)
                                               )
                        ), Is.EqualTo("1 - 2 - 3"));

            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Subtract,
                                                   JsExpression.Number(1),
                                                   JsExpression.Binary(ExpressionNodeType.Subtract,
                                                       JsExpression.Number(2),
                                                       JsExpression.Number(3)
                                                   )
                                               )
                        ), Is.EqualTo("1 - (2 - 3)"));
        }

        [Test]
        public void RightToLeftAssociativityWorksForExpressionNodeTypes() {
            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Assign,
                                                   JsExpression.Number(1),
                                                   JsExpression.Binary(ExpressionNodeType.Assign,
                                                       JsExpression.Number(2),
                                                       JsExpression.Number(3)
                                                   )
                                               )
                        ), Is.EqualTo("1 = 2 = 3"));

            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Assign,
                                                   JsExpression.Binary(ExpressionNodeType.Assign,
                                                       JsExpression.Number(1),
                                                       JsExpression.Number(2)
                                                   ),
                                                   JsExpression.Number(3)
                                               )
                        ), Is.EqualTo("(1 = 2) = 3"));
        }

        [Test]
        public void MultiplyHasHigherPrecedenceThanAdd() {
            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Add,
                                                   JsExpression.Binary(ExpressionNodeType.Multiply,
                                                       JsExpression.Number(1),
                                                       JsExpression.Number(2)
                                                   ),
                                                   JsExpression.Number(3)
                                               )
                        ), Is.EqualTo("1 * 2 + 3"));

            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Add,
                                                   JsExpression.Number(1),
                                                   JsExpression.Binary(ExpressionNodeType.Multiply,
                                                       JsExpression.Number(2),
                                                       JsExpression.Number(3)
                                                   )
                                               )
                        ), Is.EqualTo("1 + 2 * 3"));

            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Multiply,
                                                   JsExpression.Binary(ExpressionNodeType.Add,
                                                       JsExpression.Number(1),
                                                       JsExpression.Number(2)
                                                   ),
                                                   JsExpression.Number(3)
                                               )
                        ), Is.EqualTo("(1 + 2) * 3"));

            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Multiply,
                                                   JsExpression.Number(1),
                                                   JsExpression.Binary(ExpressionNodeType.Add,
                                                       JsExpression.Number(2),
                                                       JsExpression.Number(3)
                                                   )
                                               )
                        ), Is.EqualTo("1 * (2 + 3)"));
        }

        [Test]
        public void CommaIsParenthesizedAsAssignmentValue() {
            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Assign,
                                                   JsExpression.Number(1),
                                                   JsExpression.Comma(
                                                       JsExpression.Number(2),
                                                       JsExpression.Number(3)
                                                   )
                                               )
                        ), Is.EqualTo("1 = (2, 3)"));
        }

        [Test]
        public void CommaIsNotParenthesizedInsideOtherComma() {
            Assert.That(OutputFormatter.Format(JsExpression.Comma(
                                                   JsExpression.Comma(
                                                       JsExpression.Number(1),
                                                       JsExpression.Number(2)
                                                   ),
                                                   JsExpression.Comma(
                                                       JsExpression.Number(3),
                                                       JsExpression.Number(4)
                                                   )
                                               )
                        ), Is.EqualTo("1, 2, 3, 4"));
        }

        [Test]
        public void CommaIsParenthesizedInsideArrayLiteral() {
            Assert.That(OutputFormatter.Format(JsExpression.ArrayLiteral(
                                                   JsExpression.Comma(
                                                       JsExpression.Number(1),
                                                       JsExpression.Number(2)
                                                   ),
                                                   JsExpression.Number(3),
                                                   JsExpression.Comma(
                                                       JsExpression.Number(4),
                                                       JsExpression.Number(5)
                                                   )
                                               )
                        ), Is.EqualTo("[(1, 2), 3, (4, 5)]"));
        }

        [Test]
        public void AssignmentIsNotParenthesizedInsideArrayLiteral() {
            Assert.That(OutputFormatter.Format(JsExpression.ArrayLiteral(
                                                   JsExpression.Binary(ExpressionNodeType.Assign,
                                                       JsExpression.Number(1),
                                                       JsExpression.Number(2)
                                                   )
                                               )
                        ), Is.EqualTo("[1 = 2]"));
        }

        [Test]
        public void EmptyArrayLiteralWorks() {
            Assert.That(OutputFormatter.Format(JsExpression.ArrayLiteral()), Is.EqualTo("[]"));
        }

        [Test]
        public void OneElementArrayLiteralWorks() {
            Assert.That(OutputFormatter.Format(JsExpression.ArrayLiteral(JsExpression.Number(1))), Is.EqualTo("[1]"));
        }

        [Test]
        public void ConditionalIsAlwaysParenthesized() {
            Assert.That(OutputFormatter.Format(JsExpression.Conditional(
                                                   JsExpression.Number(1),
                                                   JsExpression.Number(2),
                                                   JsExpression.Number(3)
                                               )
                        ), Is.EqualTo("(1 ? 2 : 3)"));
        }

        [Test]
        public void ConditionalIsNotDoublyParenthesized() {
            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Add,
                                                   JsExpression.Number(1),
                                                   JsExpression.Conditional(
                                                       JsExpression.Number(2),
                                                       JsExpression.Number(3),
                                                       JsExpression.Number(4)
                                                   )
                                               )
                        ), Is.EqualTo("1 + (2 ? 3 : 4)"));
        }

        [Test]
        public void MultiplicationInConditionalIsParenthesized() {
            Assert.That(OutputFormatter.Format(JsExpression.Conditional(
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
                                               )
                        ), Is.EqualTo("((1 * 2) ? (3 * 4) : (5 * 6))"));
        }

        [Test]
        public void ExpressionNodeTypeIsNotParenthesizedInsideConditional() {
            Assert.That(OutputFormatter.Format(JsExpression.Conditional(
                                                   JsExpression.Unary(ExpressionNodeType.Negate, JsExpression.Number(1)),
                                                   JsExpression.Unary(ExpressionNodeType.Negate, JsExpression.Number(2)),
                                                   JsExpression.Unary(ExpressionNodeType.Negate, JsExpression.Number(3))
                                               )
                        ), Is.EqualTo("(-1 ? -2 : -3)"));
        }

        [Test]
        public void CommaIsParenthesizedInsideInvocation() {
            Assert.That(OutputFormatter.Format(JsExpression.Invocation(
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
                                               )
                        ), Is.EqualTo("f((1, 2), 3, (4, 5))"));
        }

        [Test]
        public void IndexingWorks() {
            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Index,
                                                   JsExpression.Binary(ExpressionNodeType.Index,
                                                       JsExpression.Number(1),
                                                       JsExpression.Number(2)
                                                   ),
                                                   JsExpression.Number(3)
                                               )
                        ), Is.EqualTo("1[2][3]"));
        }

        [Test]
        public void CommaIsNotParenthesizedInsideIndexing() {
            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Index,
                                                   JsExpression.Number(1),
                                                   JsExpression.Comma(
                                                       JsExpression.Number(2),
                                                       JsExpression.Number(3)
                                                   )
                                               )
                        ), Is.EqualTo("1[2, 3]"));
        }

        [Test]
        public void StringLiteralsAreCorrectlyEncoded() {
            Assert.That(OutputFormatter.Format(JsExpression.String("x")), Is.EqualTo("'x'"));
            Assert.That(OutputFormatter.Format(JsExpression.String("\"")), Is.EqualTo("'\"'"));
            Assert.That(OutputFormatter.Format(JsExpression.String("'")), Is.EqualTo("'\\''"));
            Assert.That(OutputFormatter.Format(JsExpression.String("\r\n/\\")), Is.EqualTo("'\\r\\n/\\\\'"));
        }

        [Test]
        public void RegularExpressionLiteralsAreCorrectlyEncoded() {
            Assert.That(OutputFormatter.Format(JsExpression.Regexp("x")), Is.EqualTo("/x/"));
            Assert.That(OutputFormatter.Format(JsExpression.Regexp("\"")), Is.EqualTo("/\"/"));
            Assert.That(OutputFormatter.Format(JsExpression.Regexp("/")), Is.EqualTo("/\\//"));
            Assert.That(OutputFormatter.Format(JsExpression.Regexp("\r\n/\\")), Is.EqualTo("/\\r\\n\\/\\\\/"));
            Assert.That(OutputFormatter.Format(JsExpression.Regexp("x", "g")), Is.EqualTo("/x/g"));
        }

        [Test]
        public void NullLiteralWorks() {
            Assert.That(OutputFormatter.Format(JsExpression.Null), Is.EqualTo("null"));
        }

        [Test]
        public void BooleanLiteralsWork() {
            Assert.That(OutputFormatter.Format(JsExpression.True), Is.EqualTo("true"));
            Assert.That(OutputFormatter.Format(JsExpression.False), Is.EqualTo("false"));
        }

        [Test]
        public void NumbersAreCorrectlyRepresented() {
            Assert.That(OutputFormatter.Format(JsExpression.Number(1)), Is.EqualTo("1"));
            Assert.That(OutputFormatter.Format(JsExpression.Number(1.25)), Is.EqualTo("1.25"));
            Assert.That(OutputFormatter.Format(JsExpression.Number(double.PositiveInfinity)), Is.EqualTo("Infinity"));
            Assert.That(OutputFormatter.Format(JsExpression.Number(double.NegativeInfinity)), Is.EqualTo("-Infinity"));
            Assert.That(OutputFormatter.Format(JsExpression.Number(double.NaN)), Is.EqualTo("NaN"));
        }

        [Test]
        public void NestedMemberExpressionsAreNotParenthesized() {
            Assert.That(OutputFormatter.Format(JsExpression.MemberAccess(
                                                   JsExpression.MemberAccess(
                                                       JsExpression.MemberAccess(
                                                           JsExpression.Number(1),
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
            Assert.That(OutputFormatter.Format(JsExpression.MemberAccess(
                                                   JsExpression.Invocation(
                                                       JsExpression.Number(1),
                                                       new[] { JsExpression.Number(2) }
                                                   ),
                                                   "Member"
                                               )
                        ), Is.EqualTo("(1(2)).Member"));
        }

        [Test]
        public void NestedNewExpressionsAreParenthesized() {
            // I don't know if this makes sense, but if it does, it should be correct.
            Assert.That(OutputFormatter.Format(JsExpression.New(
                                                   JsExpression.New(
                                                       JsExpression.New(
                                                           JsExpression.Number(1)
                                                       )
                                                   )
                                               )
                        ), Is.EqualTo("new (new (new 1())())()"));
        }

        [Test]
        public void NewExpressionsAndMemberExpressionsAreParenthesizedInsideEachOther() {
            // Other stuff from the department "strange edge cases". Should be parenthesized to cause as little trouble as possible.
            Assert.That(OutputFormatter.Format(JsExpression.MemberAccess(
                                                   JsExpression.New(
                                                       JsExpression.Number(1),
                                                       new[] { JsExpression.Number(2) }
                                                   ),
                                                   "Member"
                                               )
                        ), Is.EqualTo("(new 1(2)).Member"));

            // Other stuff from the department "strange edge cases". Should be parenthesized to cause as little trouble as possible.
            Assert.That(OutputFormatter.Format(JsExpression.New(
                                                   JsExpression.MemberAccess(
                                                       JsExpression.Number(1),
                                                       "Member"
                                                   ),
                                                   new[] { JsExpression.Number(2) }
                                               )
                        ), Is.EqualTo("new (1.Member)(2)"));
        }

        [Test]
        public void CommaIsParenthesizedInsideConstructorArgumentList() {
            Assert.That(OutputFormatter.Format(JsExpression.New(
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
                                               )
                        ), Is.EqualTo("new 10((1, 2), 3, (4, 5))"));
        }

        [Test]
        public void AssignmentIsNotParenthesizedInsideConstructorArgumentList() {
            Assert.That(OutputFormatter.Format(JsExpression.ArrayLiteral(
                                                   JsExpression.Binary(ExpressionNodeType.Assign,
                                                       JsExpression.Number(1),
                                                       JsExpression.Number(2)
                                                   )
                                               )
                        ), Is.EqualTo("[1 = 2]"));
        }

        [Test]
        public void IdentifierIsNotParenthesizedWhenUsedAsConstructor() {
            Assert.That(OutputFormatter.Format(JsExpression.New(
                                                   JsExpression.Identifier("X"),
                                                   new JsExpression[0]
                                               )
                        ), Is.EqualTo("new X()"));
        }

        [Test]
        public void IdentifierIsOutputCorrectly() {
            Assert.That(OutputFormatter.Format(JsExpression.Identifier("SomeIdentifier")), Is.EqualTo("SomeIdentifier"));
        }

        [Test]
        public void IncrementIsParenthesizedWhenUsedAsInvocationMethod() {
            Assert.That(OutputFormatter.Format(JsExpression.Invocation(
                                                   JsExpression.Unary(ExpressionNodeType.PostfixMinusMinus,
                                                       JsExpression.Identifier("x")
                                                   ),
                                                   new JsExpression[0]
                                               )
                        ), Is.EqualTo("(x--)()"));
        }

        [Test]
        public void MemberAccessIsNotParenthesizedWhenUsedAsInvocationTarget() {
            Assert.That(OutputFormatter.Format(JsExpression.Invocation(
                                                   JsExpression.MemberAccess(
                                                       JsExpression.Identifier("x"),
                                                       "Member"
                                                   ),
                                                   new JsExpression[0]
                                               )
                        ), Is.EqualTo("x.Member()"));
        }

        [Test]
        public void ChainedFunctionCallsAreNotParenthtesized() {
            Assert.That(OutputFormatter.Format(JsExpression.Invocation(
                                                   JsExpression.Invocation(
                                                       JsExpression.Identifier("x"),
                                                       new[] { JsExpression.Number(1) }
                                                   ),
                                                   new[] { JsExpression.Number(2) }
                                               )
                        ), Is.EqualTo("x(1)(2)"));
        }

        [Test]
        public void NewExpressionIsParenthesizedWhenBeingUsedAsInvocationTarget() {
            // Just to get rid of ambiguities
            Assert.That(OutputFormatter.Format(JsExpression.Invocation(
                                                   JsExpression.New(
                                                       JsExpression.Identifier("X"),
                                                       new JsExpression[0]
                                                   ),
                                                   new[] { JsExpression.Number(1) }
                                               )
                        ), Is.EqualTo("(new X())(1)"));
        }

        [Test]
        public void IncrementIsParenthesizedWhenBeingUsedAsInvocationTarget() {
            Assert.That(OutputFormatter.Format(JsExpression.Invocation(
                                                   JsExpression.Unary(ExpressionNodeType.PostfixPlusPlus,
                                                       JsExpression.Identifier("X")
                                                   ),
                                                   new[] { JsExpression.Number(1) }
                                               )
                        ), Is.EqualTo("(X++)(1)"));
        }

        [Test]
        public void ExpressionNodeTypeIsNotParenthesizedWhenUsedAsBinaryArgument() {
            Assert.That(OutputFormatter.Format(JsExpression.Binary(ExpressionNodeType.Multiply,
                                                   JsExpression.Unary(ExpressionNodeType.Negate,
                                                       JsExpression.Identifier("X")
                                                   ),
                                                   JsExpression.Number(1)
                                               )
                        ), Is.EqualTo("-X * 1"));
        }

        [Test]
        public void ExpressionNodeTypesAreParenthesizedInsideEachother() {
            // Just to get rid of ambiguities
            Assert.That(OutputFormatter.Format(JsExpression.Unary(ExpressionNodeType.PostfixPlusPlus,
                                                   JsExpression.Unary(ExpressionNodeType.LogicalNot,
                                                       JsExpression.Identifier("X")
                                                   )
                                               )
                        ), Is.EqualTo("(!X)++"));

            Assert.That(OutputFormatter.Format(JsExpression.Unary(ExpressionNodeType.LogicalNot,
                                                   JsExpression.Unary(ExpressionNodeType.PostfixPlusPlus,
                                                       JsExpression.Identifier("X")
                                                   )
                                               )
                        ), Is.EqualTo("!(X++)"));
        }

        [Test]
        public void EmptyObjectLiteralIsOutputCorrectly() {
            Assert.That(OutputFormatter.Format(JsExpression.ObjectLiteral()), Is.EqualTo("{}"));
        }

        [Test]
        public void ObjectLiteralWithOneValueIsOutputCorrectly() {
            Assert.That(OutputFormatter.Format(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("x", JsExpression.Number(1)))), Is.EqualTo("{ x: 1 }"));
        }

        [Test]
        public void ObjectLiteralWithThreeValuesIsOutputCorrectly() {
            Assert.That(OutputFormatter.Format(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("x", JsExpression.Number(1)),
                                                                        new JsObjectLiteralProperty("y", JsExpression.Number(2)),
                                                                        new JsObjectLiteralProperty("z", JsExpression.Number(3)))
                       ), Is.EqualTo("{ x: 1, y: 2, z: 3 }"));
        }

        [Test]
        public void ObjectLiteralWithNumericPropertyWorks() {
            Assert.That(OutputFormatter.Format(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("1", JsExpression.Number(2)))), Is.EqualTo("{ '1': 2 }"));
        }

        [Test]
        public void ObjectLiteralWithInvalidIdentifierPropertyWorks() {
            Assert.That(OutputFormatter.Format(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("a\\b", JsExpression.Number(1)))), Is.EqualTo("{ 'a\\\\b': 1 }"));
        }

        [Test]
        public void CommaExpressionIsParenthesizedInsideObjectLiteral() {
            Assert.That(OutputFormatter.Format(JsExpression.ObjectLiteral(new JsObjectLiteralProperty("x", JsExpression.Comma(JsExpression.Number(1), JsExpression.Number(2))),
                                                                        new JsObjectLiteralProperty("y", JsExpression.Number(3)))
                        ), Is.EqualTo("{ x: (1, 2), y: 3 }"));
        }

        [Test]
        public void EmptyFunctionDefinitionWithoutNameIsCorrectlyOutput() {
            Assert.That(OutputFormatter.Format(JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement, null)), Is.EqualTo("function() {}"));
        }

        [Test]
        public void EmptyFunctionDefinitionWithNameIsCorrectlyOutput() {
            Assert.That(OutputFormatter.Format(JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement, "test")), Is.EqualTo("function test() {}"));
        }

        [Test]
        public void EmptyFunctionDefinitionsWithArgumentsAreCorrectlyOutput() {
            Assert.That(OutputFormatter.Format(JsExpression.FunctionDefinition(new[] { "a" }, JsBlockStatement.EmptyStatement, null)), Is.EqualTo("function(a) {}"));
            Assert.That(OutputFormatter.Format(JsExpression.FunctionDefinition(new[] { "a", "b" }, JsBlockStatement.EmptyStatement, null)), Is.EqualTo("function(a, b) {}"));
            Assert.That(OutputFormatter.Format(JsExpression.FunctionDefinition(new[] { "a", "b", "c" }, JsBlockStatement.EmptyStatement, "test")), Is.EqualTo("function test(a, b, c) {}"));
        }

        [Test]
        public void FunctionIsParenthesizedWhenInvokedDirectly() {
            Assert.That(OutputFormatter.Format(JsExpression.Invocation(
                                                   JsExpression.FunctionDefinition(new string[0], JsBlockStatement.EmptyStatement, null)
                                               )
                        ), Is.EqualTo("(function() {})()"));
        }

        [Test, Ignore("Can't yet output function definitions.")]
        public void FunctionDefinitionWithContentIsCorrectlyOutput() {
            Assert.That(OutputFormatter.Format(JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.Null))), Is.EqualTo("function() { return null; }"));
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
                var expr = JsExpression.Unary(oper, JsExpression.Identifier("a"));
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
                var expr = JsExpression.Binary(oper, JsExpression.Identifier("a"), JsExpression.Identifier("b"));
                Assert.That(OutputFormatter.Format(expr), Is.EqualTo(string.Format(operators[oper], "a", "b")));
            }
        }

		[Test]
		public void ThisIsCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(JsExpression.This), Is.EqualTo("this"));
		}

		[Test]
		public void CommentsAreCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(new JsComment("Line 1")), Is.EqualTo("//Line 1\r\n"));
			Assert.That(OutputFormatter.Format(new JsComment(" With spaces ")), Is.EqualTo("// With spaces \r\n"));
			Assert.That(OutputFormatter.Format(new JsComment(" With\r\n Multiple\n Lines")), Is.EqualTo("// With\r\n// Multiple\r\n// Lines\r\n"));
		}

		[Test]
		public void BlockStatementsAreCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(new JsBlockStatement(new JsStatement[0])), Is.EqualTo("{\r\n}\r\n"));
			Assert.That(OutputFormatter.Format(new JsBlockStatement(new[] { new JsComment("X") })), Is.EqualTo("{\r\n\t//X\r\n}\r\n"));
			Assert.That(OutputFormatter.Format(new JsBlockStatement(new[] { new JsComment("X"), new JsComment("Y") })), Is.EqualTo("{\r\n\t//X\r\n\t//Y\r\n}\r\n"));
		}

		[Test]
		public void VariableDeclarationStatementsAreCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(new JsVariableDeclarationStatement(new[] { new JsVariableDeclaration("i", null) })), Is.EqualTo("var i;\r\n"));
			Assert.That(OutputFormatter.Format(new JsVariableDeclarationStatement(new[] { new JsVariableDeclaration("i", null), new JsVariableDeclaration("j", null) })), Is.EqualTo("var i, j;\r\n"));
			Assert.That(OutputFormatter.Format(new JsVariableDeclarationStatement(new[] { new JsVariableDeclaration("i", JsExpression.Number(0)) })), Is.EqualTo("var i = 0;\r\n"));
			Assert.That(OutputFormatter.Format(new JsVariableDeclarationStatement(new[] { new JsVariableDeclaration("i", JsExpression.Number(0)), new JsVariableDeclaration("j", JsExpression.Number(1)) })), Is.EqualTo("var i = 0, j = 1;\r\n"));
			Assert.That(OutputFormatter.Format(new JsVariableDeclarationStatement(new[] { new JsVariableDeclaration("i", JsExpression.Number(0)), new JsVariableDeclaration("j", null) })), Is.EqualTo("var i = 0, j;\r\n"));
		}

		[Test]
		public void ExpressionStatementsAreCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(new JsExpressionStatement(JsExpression.This)), Is.EqualTo("this;\r\n"));
		}

		[Test]
		public void ForStatementsAreCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(new JsForStatement(new JsVariableDeclarationStatement(new JsVariableDeclaration("i", JsExpression.Number(0))),
			                                                      JsExpression.Lesser(JsExpression.Identifier("i"), JsExpression.Number(10)),
			                                                      JsExpression.PostfixPlusPlus(JsExpression.Identifier("i")),
			                                                      JsBlockStatement.EmptyStatement)),
			            Is.EqualTo("for (var i = 0; i < 10; i++) {\r\n}\r\n"));

			Assert.That(OutputFormatter.Format(new JsForStatement(new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0))), 
			                                                      JsExpression.Lesser(JsExpression.Identifier("i"), JsExpression.Number(10)),
			                                                      JsExpression.PostfixPlusPlus(JsExpression.Identifier("i")),
			                                                      JsBlockStatement.EmptyStatement)),
			            Is.EqualTo("for (i = 0; i < 10; i++) {\r\n}\r\n"));

			Assert.That(OutputFormatter.Format(new JsForStatement(new JsVariableDeclarationStatement(new JsVariableDeclaration("i", JsExpression.Number(0)), new JsVariableDeclaration("j", JsExpression.Number(1))),
			                                                      JsExpression.Lesser(JsExpression.Identifier("i"), JsExpression.Number(10)),
			                                                      JsExpression.Comma(JsExpression.PostfixPlusPlus(JsExpression.Identifier("i")), JsExpression.PostfixPlusPlus(JsExpression.Identifier("j"))),
			                                                      JsBlockStatement.EmptyStatement)),
			            Is.EqualTo("for (var i = 0, j = 1; i < 10; i++, j++) {\r\n}\r\n"));

			Assert.That(OutputFormatter.Format(new JsForStatement(new JsEmptyStatement(),
			                                                      null,
			                                                      null,
			                                                      JsBlockStatement.EmptyStatement)),
			            Is.EqualTo("for (;;) {\r\n}\r\n"));
		}

		[Test]
		public void EmptyStatementIsCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(new JsEmptyStatement()), Is.EqualTo(";\r\n"));
		}

		[Test]
		public void IfStatementIsCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(new JsIfStatement(JsExpression.True, new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0))), null)),
			            Is.EqualTo("if (true) {\r\n\ti = 0;\r\n}\r\n"));
			Assert.That(OutputFormatter.Format(new JsIfStatement(JsExpression.True, new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(0))), new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier("i"), JsExpression.Number(1))))),
			            Is.EqualTo("if (true) {\r\n\ti = 0;\r\n}\r\nelse {\r\n\ti = 1;\r\n}\r\n"));
		}

		[Test]
		public void BreakStatementIsCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(new JsBreakStatement()), Is.EqualTo("break;\r\n"));
			Assert.That(OutputFormatter.Format(new JsBreakStatement("someLabel")), Is.EqualTo("break someLabel;\r\n"));
		}

		[Test]
		public void ContinueStatementIsCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(new JsContinueStatement()), Is.EqualTo("continue;\r\n"));
			Assert.That(OutputFormatter.Format(new JsContinueStatement("someLabel")), Is.EqualTo("continue someLabel;\r\n"));
		}

		[Test]
		public void DoWhileStatementIsCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(new JsDoWhileStatement(JsExpression.True, new JsBlockStatement(new JsVariableDeclarationStatement(new JsVariableDeclaration("x", JsExpression.Number(0)))))),
			            Is.EqualTo("do {\r\n\tvar x = 0;\r\n} while (true);\r\n"));
		}

		[Test]
		public void WhileStatementIsCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(new JsWhileStatement(JsExpression.True, new JsBlockStatement(new JsVariableDeclarationStatement(new JsVariableDeclaration("x", JsExpression.Number(0)))))),
			            Is.EqualTo("while (true) {\r\n\tvar x = 0;\r\n}\r\n"));
		}

		[Test]
		public void ReturnStatementWithOrWithoutExpressionIsCorrectlyOutput() {
			Assert.That(OutputFormatter.Format(new JsReturnStatement(null)), Is.EqualTo("return;\r\n"));
			Assert.That(OutputFormatter.Format(new JsReturnStatement(JsExpression.Identifier("x"))), Is.EqualTo("return x;\r\n"));
		}

		[Test]
		public void TryCatchFinallyStatementWithCatchOrFinallyOrBothWorks() {
			Assert.That(OutputFormatter.Format(new JsTryCatchFinallyStatement(new JsExpressionStatement(JsExpression.Identifier("x")), new JsCatchClause("e", new JsExpressionStatement(JsExpression.Identifier("y"))), new JsExpressionStatement(JsExpression.Identifier("z")))),
			            Is.EqualTo("try {\r\n\tx;\r\n}\r\ncatch (e) {\r\n\ty;\r\n}\r\nfinally {\r\n\tz;\r\n}\r\n"));
			Assert.That(OutputFormatter.Format(new JsTryCatchFinallyStatement(new JsExpressionStatement(JsExpression.Identifier("x")), new JsCatchClause("e", new JsExpressionStatement(JsExpression.Identifier("y"))), null)),
			            Is.EqualTo("try {\r\n\tx;\r\n}\r\ncatch (e) {\r\n\ty;\r\n}\r\n"));
			Assert.That(OutputFormatter.Format(new JsTryCatchFinallyStatement(new JsExpressionStatement(JsExpression.Identifier("x")), null, new JsExpressionStatement(JsExpression.Identifier("z")))),
			            Is.EqualTo("try {\r\n\tx;\r\n}\r\nfinally {\r\n\tz;\r\n}\r\n"));
		}
    }
}
