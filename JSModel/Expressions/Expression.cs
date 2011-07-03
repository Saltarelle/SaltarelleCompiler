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
    public abstract class Expression {
        [System.Diagnostics.DebuggerStepThrough]
        public abstract TReturn Accept<TReturn, TData>(IExpressionVisitor<TReturn, TData> visitor, TData data);

        public ExpressionNodeType NodeType { get; private set; }

        protected Expression(ExpressionNodeType nodeType) {
            NodeType = nodeType;
        }

        public static ArrayLiteralExpression ArrayLiteral(IEnumerable<Expression> elements) {
            return new ArrayLiteralExpression(elements);
        }

        public static ArrayLiteralExpression ArrayLiteral(params Expression[] elements) {
            return ArrayLiteral((IEnumerable<Expression>)elements);
        }

        public static BinaryExpression Binary(ExpressionNodeType nodeType, Expression left, Expression right) {
            return new BinaryExpression(nodeType, left, right);
        }

        public static BinaryExpression LogicalAnd(Expression left, Expression right) {
            return Binary(ExpressionNodeType.LogicalAnd, left, right);
        }

        public static BinaryExpression LogicalOr(Expression left, Expression right) {
            return Binary(ExpressionNodeType.LogicalOr, left, right);
        }

        public static BinaryExpression NotEqual(Expression left, Expression right) {
            return Binary(ExpressionNodeType.NotEqual, left, right);
        }

        public static BinaryExpression LesserOrEqual(Expression left, Expression right) {
            return Binary(ExpressionNodeType.LesserOrEqual, left, right);
        }

        public static BinaryExpression GreaterOrEqual(Expression left, Expression right) {
            return Binary(ExpressionNodeType.GreaterOrEqual, left, right);
        }

        public static BinaryExpression Lesser(Expression left, Expression right) {
            return Binary(ExpressionNodeType.Lesser, left, right);
        }

        public static BinaryExpression Greater(Expression left, Expression right) {
            return Binary(ExpressionNodeType.Greater, left, right);
        }

        public static BinaryExpression Equal(Expression left, Expression right) {
            return Binary(ExpressionNodeType.Equal, left, right);
        }

        public static BinaryExpression Subtract(Expression left, Expression right) {
            return Binary(ExpressionNodeType.Subtract, left, right);
        }

        public static BinaryExpression Add(Expression left, Expression right) {
            return Binary(ExpressionNodeType.Add, left, right);
        }

        public static BinaryExpression Modulo(Expression left, Expression right) {
            return Binary(ExpressionNodeType.Modulo, left, right);
        }

        public static BinaryExpression Divide(Expression left, Expression right) {
            return Binary(ExpressionNodeType.Divide, left, right);
        }

        public static BinaryExpression Multiply(Expression left, Expression right) {
            return Binary(ExpressionNodeType.Multiply, left, right);
        }

        public static BinaryExpression BitwiseAnd(Expression left, Expression right) {
            return Binary(ExpressionNodeType.BitwiseAnd, left, right);
        }

        public static BinaryExpression BitwiseOr(Expression left, Expression right) {
            return Binary(ExpressionNodeType.BitwiseOr, left, right);
        }

        public static BinaryExpression BitwiseXor(Expression left, Expression right) {
            return Binary(ExpressionNodeType.BitwiseXor, left, right);
        }

        public static BinaryExpression Same(Expression left, Expression right) {
            return Binary(ExpressionNodeType.Same, left, right);
        }

        public static BinaryExpression NotSame(Expression left, Expression right) {
            return Binary(ExpressionNodeType.NotSame, left, right);
        }

        public static BinaryExpression LeftShift(Expression left, Expression right) {
            return Binary(ExpressionNodeType.LeftShift, left, right);
        }

        public static BinaryExpression RightShiftSigned(Expression left, Expression right) {
            return Binary(ExpressionNodeType.RightShiftSigned, left, right);
        }

        public static BinaryExpression RightShiftUnsigned(Expression left, Expression right) {
            return Binary(ExpressionNodeType.RightShiftUnsigned, left, right);
        }

        public static BinaryExpression InstanceOf(Expression left, Expression right) {
            return Binary(ExpressionNodeType.InstanceOf, left, right);
        }

        public static BinaryExpression In(Expression left, Expression right) {
            return Binary(ExpressionNodeType.In, left, right);
        }

        public static BinaryExpression Index(Expression left, Expression right) {
            return Binary(ExpressionNodeType.Index, left, right);
        }

        public static BinaryExpression Assign(Expression left, Expression right) {
            return Binary(ExpressionNodeType.Assign, left, right);
        }

        public static BinaryExpression MultiplyAssign(Expression left, Expression right) {
            return Binary(ExpressionNodeType.MultiplyAssign, left, right);
        }

        public static BinaryExpression DivideAssign(Expression left, Expression right) {
            return Binary(ExpressionNodeType.DivideAssign, left, right);
        }

        public static BinaryExpression ModuloAssign(Expression left, Expression right) {
            return Binary(ExpressionNodeType.ModuloAssign, left, right);
        }

        public static BinaryExpression AddAssign(Expression left, Expression right) {
            return Binary(ExpressionNodeType.AddAssign, left, right);
        }

        public static BinaryExpression SubtractAssign(Expression left, Expression right) {
            return Binary(ExpressionNodeType.SubtractAssign, left, right);
        }

        public static BinaryExpression LeftShiftAssign(Expression left, Expression right) {
            return Binary(ExpressionNodeType.LeftShiftAssign, left, right);
        }

        public static BinaryExpression RightShiftAssign(Expression left, Expression right) {
            return Binary(ExpressionNodeType.RightShiftAssign, left, right);
        }

        public static BinaryExpression UnsignedRightShiftAssign(Expression left, Expression right) {
            return Binary(ExpressionNodeType.UnsignedRightShiftAssign, left, right);
        }

        public static BinaryExpression BitwiseAndAssign(Expression left, Expression right) {
            return Binary(ExpressionNodeType.BitwiseAndAssign, left, right);
        }

        public static BinaryExpression BitwiseOrAssign(Expression left, Expression right) {
            return Binary(ExpressionNodeType.BitwiseOrAssign, left, right);
        }

        public static BinaryExpression BitwiseXOrAssign(Expression left, Expression right) {
            return Binary(ExpressionNodeType.BitwiseXOrAssign, left, right);
        }

        public static CommaExpression Comma(IEnumerable<Expression> expressions) {
            return new CommaExpression(expressions);
        }

        public static CommaExpression Comma(params Expression[] expressions) {
            return Comma((IEnumerable<Expression>)expressions);
        }

        public static ConditionalExpression Conditional(Expression test, Expression truePart, Expression falsePart) {
            return new ConditionalExpression(test, truePart, falsePart);
        }

        public static ConstantExpression Regexp(string pattern, string options = null) {
            return new ConstantExpression(new ConstantExpression.RegexpData(pattern, options));
        }

        public static ConstantExpression Number(double value) {
            return new ConstantExpression(value);
        }

        public static ConstantExpression String(string value) {
            return new ConstantExpression(value);
        }

        public static ConstantExpression Null { get { return ConstantExpression.Null; } }

        public static FunctionDefinitionExpression FunctionDefinition(IEnumerable<string> parameterNames, Statement body, string name = null) {
            return new FunctionDefinitionExpression(parameterNames, body, name);
        }

        public static IdentifierExpression Identifier(string name) {
            return new IdentifierExpression(name);
        }

        public static InvocationExpression Invocation(Expression method, IEnumerable<Expression> arguments) {
            return new InvocationExpression(method, arguments);
        }

        public static InvocationExpression Invocation(Expression method, params Expression[] arguments) {
            return Invocation(method, (IEnumerable<Expression>)arguments);
        }

        public static MemberAccessExpression MemberAccess(Expression target, string member) {
            return new MemberAccessExpression(target, member);
        }

        public static NewExpression New(Expression constructor, IEnumerable<Expression> arguments) {
            return new NewExpression(constructor, arguments);
        }

        public static NewExpression New(Expression constructor, params Expression[] arguments) {
            return New(constructor, (IEnumerable<Expression>)arguments);
        }

        public static ObjectLiteralExpression ObjectLiteral(IEnumerable<ObjectLiteralProperty> values) {
            return new ObjectLiteralExpression(values);
        }

        public static ObjectLiteralExpression ObjectLiteral(params ObjectLiteralProperty[] values) {
            return ObjectLiteral((IEnumerable<ObjectLiteralProperty>)values);
        }

        public static UnaryExpression Unary(ExpressionNodeType nodeType, Expression operand) {
            return new UnaryExpression(nodeType, operand);
        }

        public static UnaryExpression TypeOf(Expression operand) {
            return Unary(ExpressionNodeType.TypeOf, operand);
        }

        public static UnaryExpression LogicalNot(Expression operand) {
            return Unary(ExpressionNodeType.LogicalNot, operand);
        }

        public static UnaryExpression Negate(Expression operand) {
            return Unary(ExpressionNodeType.Negate, operand);
        }

        public static UnaryExpression Positive(Expression operand) {
            return Unary(ExpressionNodeType.Positive, operand);
        }

        public static UnaryExpression PrefixPlusPlus(Expression operand) {
            return Unary(ExpressionNodeType.PrefixPlusPlus, operand);
        }

        public static UnaryExpression PrefixMinusMinus(Expression operand) {
            return Unary(ExpressionNodeType.PrefixMinusMinus, operand);
        }

        public static UnaryExpression PostfixPlusPlus(Expression operand) {
            return Unary(ExpressionNodeType.PostfixPlusPlus, operand);
        }

        public static UnaryExpression PostfixMinusMinus(Expression operand) {
            return Unary(ExpressionNodeType.PostfixMinusMinus, operand);
        }

        public static UnaryExpression Delete(Expression operand) {
            return Unary(ExpressionNodeType.Delete, operand);
        }

        public static UnaryExpression Void(Expression operand) {
            return Unary(ExpressionNodeType.Void, operand);
        }

        public static UnaryExpression BitwiseNot(Expression operand) {
            return Unary(ExpressionNodeType.BitwiseNot, operand);
        }
    }
}
