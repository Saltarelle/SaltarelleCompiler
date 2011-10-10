using System;
using System.Collections.Generic;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.Expressions {
    public enum ExpressionNodeType {
        ArrayLiteral,

        // Binary
        LogicalAnd,
        LogicalOr,
        NotEqual,
        LesserOrEqual,
        GreaterOrEqual,
        Lesser,
        Greater,
        Equal,
        Subtract,
        Add,
        Modulo,
        Divide,
        Multiply,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        Same,
        NotSame,
        LeftShift,
        RightShiftSigned,
        RightShiftUnsigned,
        InstanceOf,
        In,
        Index,

        Assign,
        MultiplyAssign,
        DivideAssign,
        ModuloAssign,
        AddAssign,
        SubtractAssign,
        LeftShiftAssign,
        RightShiftAssign,
        UnsignedRightShiftAssign,
        BitwiseAndAssign,
        BitwiseOrAssign,
        BitwiseXOrAssign,
        // /Binary

        Comma,
        Conditional,

        // Constants
        Number,
        String,
        Regexp,
        Null,
        // /Constants

        FunctionDefinition,
        Identifier,
        Invocation,
        MemberAccess,
        New,
        ObjectLiteral,

        // Unary
        TypeOf,
        LogicalNot,
        Negate,
        Positive,
        PrefixPlusPlus,
        PrefixMinusMinus,
        PostfixPlusPlus,
        PostfixMinusMinus,
        Delete,
        Void,
        BitwiseNot,
        // /Unary

        // Fake
        TypeReference,

        AssignFirst = Assign,
        AssignLast = BitwiseXOrAssign,
        BinaryFirst = LogicalAnd,
        BinaryLast = BitwiseXOrAssign,
        ConstantFirst = Number,
        ConstantLast = Null,
        UnaryFirst = TypeOf,
        UnaryLast = BitwiseNot
    }

    [Serializable]
    public abstract class JsExpression {
        [System.Diagnostics.DebuggerStepThrough]
        public abstract TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data);

        public ExpressionNodeType NodeType { get; private set; }

        protected JsExpression(ExpressionNodeType nodeType) {
            NodeType = nodeType;
        }

        public static JsArrayLiteralExpression ArrayLiteral(IEnumerable<JsExpression> elements) {
            return new JsArrayLiteralExpression(elements);
        }

        public static JsArrayLiteralExpression ArrayLiteral(params JsExpression[] elements) {
            return ArrayLiteral((IEnumerable<JsExpression>)elements);
        }

        public static JsBinaryExpression Binary(ExpressionNodeType nodeType, JsExpression left, JsExpression right) {
            return new JsBinaryExpression(nodeType, left, right);
        }

        public static JsBinaryExpression LogicalAnd(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.LogicalAnd, left, right);
        }

        public static JsBinaryExpression LogicalOr(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.LogicalOr, left, right);
        }

        public static JsBinaryExpression NotEqual(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.NotEqual, left, right);
        }

        public static JsBinaryExpression LesserOrEqual(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.LesserOrEqual, left, right);
        }

        public static JsBinaryExpression GreaterOrEqual(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.GreaterOrEqual, left, right);
        }

        public static JsBinaryExpression Lesser(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.Lesser, left, right);
        }

        public static JsBinaryExpression Greater(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.Greater, left, right);
        }

        public static JsBinaryExpression Equal(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.Equal, left, right);
        }

        public static JsBinaryExpression Subtract(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.Subtract, left, right);
        }

        public static JsBinaryExpression Add(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.Add, left, right);
        }

        public static JsBinaryExpression Modulo(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.Modulo, left, right);
        }

        public static JsBinaryExpression Divide(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.Divide, left, right);
        }

        public static JsBinaryExpression Multiply(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.Multiply, left, right);
        }

        public static JsBinaryExpression BitwiseAnd(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.BitwiseAnd, left, right);
        }

        public static JsBinaryExpression BitwiseOr(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.BitwiseOr, left, right);
        }

        public static JsBinaryExpression BitwiseXor(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.BitwiseXor, left, right);
        }

        public static JsBinaryExpression Same(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.Same, left, right);
        }

        public static JsBinaryExpression NotSame(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.NotSame, left, right);
        }

        public static JsBinaryExpression LeftShift(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.LeftShift, left, right);
        }

        public static JsBinaryExpression RightShiftSigned(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.RightShiftSigned, left, right);
        }

        public static JsBinaryExpression RightShiftUnsigned(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.RightShiftUnsigned, left, right);
        }

        public static JsBinaryExpression InstanceOf(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.InstanceOf, left, right);
        }

        public static JsBinaryExpression In(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.In, left, right);
        }

        public static JsBinaryExpression Index(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.Index, left, right);
        }

        public static JsBinaryExpression Assign(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.Assign, left, right);
        }

        public static JsBinaryExpression MultiplyAssign(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.MultiplyAssign, left, right);
        }

        public static JsBinaryExpression DivideAssign(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.DivideAssign, left, right);
        }

        public static JsBinaryExpression ModuloAssign(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.ModuloAssign, left, right);
        }

        public static JsBinaryExpression AddAssign(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.AddAssign, left, right);
        }

        public static JsBinaryExpression SubtractAssign(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.SubtractAssign, left, right);
        }

        public static JsBinaryExpression LeftShiftAssign(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.LeftShiftAssign, left, right);
        }

        public static JsBinaryExpression RightShiftAssign(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.RightShiftAssign, left, right);
        }

        public static JsBinaryExpression UnsignedRightShiftAssign(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.UnsignedRightShiftAssign, left, right);
        }

        public static JsBinaryExpression BitwiseAndAssign(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.BitwiseAndAssign, left, right);
        }

        public static JsBinaryExpression BitwiseOrAssign(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.BitwiseOrAssign, left, right);
        }

        public static JsBinaryExpression BitwiseXOrAssign(JsExpression left, JsExpression right) {
            return Binary(ExpressionNodeType.BitwiseXOrAssign, left, right);
        }

        public static JsCommaExpression Comma(IEnumerable<JsExpression> expressions) {
            return new JsCommaExpression(expressions);
        }

        public static JsCommaExpression Comma(params JsExpression[] expressions) {
            return Comma((IEnumerable<JsExpression>)expressions);
        }

        public static JsConditionalExpression Conditional(JsExpression test, JsExpression truePart, JsExpression falsePart) {
            return new JsConditionalExpression(test, truePart, falsePart);
        }

        public static JsConstantExpression Regexp(string pattern, string options = null) {
            return new JsConstantExpression(new JsConstantExpression.RegexpData(pattern, options));
        }

        public static JsConstantExpression Number(double value) {
            return new JsConstantExpression(value);
        }

        public static JsConstantExpression String(string value) {
            return new JsConstantExpression(value);
        }

        public static JsConstantExpression Null { get { return JsConstantExpression.Null; } }

        public static JsFunctionDefinitionExpression FunctionDefinition(IEnumerable<string> parameterNames, JsStatement body, string name = null) {
            return new JsFunctionDefinitionExpression(parameterNames, body, name);
        }

        public static JsIdentifierExpression Identifier(string name) {
            return new JsIdentifierExpression(name);
        }

        public static JsInvocationExpression Invocation(JsExpression method, IEnumerable<JsExpression> arguments) {
            return new JsInvocationExpression(method, arguments);
        }

        public static JsInvocationExpression Invocation(JsExpression method, params JsExpression[] arguments) {
            return Invocation(method, (IEnumerable<JsExpression>)arguments);
        }

        public static JsMemberAccessExpression MemberAccess(JsExpression target, string member) {
            return new JsMemberAccessExpression(target, member);
        }

        public static JsNewExpression New(JsExpression constructor, IEnumerable<JsExpression> arguments) {
            return new JsNewExpression(constructor, arguments);
        }

        public static JsNewExpression New(JsExpression constructor, params JsExpression[] arguments) {
            return New(constructor, (IEnumerable<JsExpression>)arguments);
        }

        public static JsObjectLiteralExpression ObjectLiteral(IEnumerable<JsObjectLiteralProperty> values) {
            return new JsObjectLiteralExpression(values);
        }

        public static JsObjectLiteralExpression ObjectLiteral(params JsObjectLiteralProperty[] values) {
            return ObjectLiteral((IEnumerable<JsObjectLiteralProperty>)values);
        }

        public static JsUnaryExpression Unary(ExpressionNodeType nodeType, JsExpression operand) {
            return new JsUnaryExpression(nodeType, operand);
        }

        public static JsUnaryExpression TypeOf(JsExpression operand) {
            return Unary(ExpressionNodeType.TypeOf, operand);
        }

        public static JsUnaryExpression LogicalNot(JsExpression operand) {
            return Unary(ExpressionNodeType.LogicalNot, operand);
        }

        public static JsUnaryExpression Negate(JsExpression operand) {
            return Unary(ExpressionNodeType.Negate, operand);
        }

        public static JsUnaryExpression Positive(JsExpression operand) {
            return Unary(ExpressionNodeType.Positive, operand);
        }

        public static JsUnaryExpression PrefixPlusPlus(JsExpression operand) {
            return Unary(ExpressionNodeType.PrefixPlusPlus, operand);
        }

        public static JsUnaryExpression PrefixMinusMinus(JsExpression operand) {
            return Unary(ExpressionNodeType.PrefixMinusMinus, operand);
        }

        public static JsUnaryExpression PostfixPlusPlus(JsExpression operand) {
            return Unary(ExpressionNodeType.PostfixPlusPlus, operand);
        }

        public static JsUnaryExpression PostfixMinusMinus(JsExpression operand) {
            return Unary(ExpressionNodeType.PostfixMinusMinus, operand);
        }

        public static JsUnaryExpression Delete(JsExpression operand) {
            return Unary(ExpressionNodeType.Delete, operand);
        }

        public static JsUnaryExpression Void(JsExpression operand) {
            return Unary(ExpressionNodeType.Void, operand);
        }

        public static JsUnaryExpression BitwiseNot(JsExpression operand) {
            return Unary(ExpressionNodeType.BitwiseNot, operand);
        }
    }
}
