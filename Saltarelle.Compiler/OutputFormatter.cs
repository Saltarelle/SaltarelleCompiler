using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler
{
    public class OutputFormatter : IExpressionVisitor<object, object>
    {
        private readonly CodeBuilder _cb = new CodeBuilder();

        private OutputFormatter() {
        }

/*        public static string Format(Statement statement) {
            var fmt = new OutputFormatter();
            fmt.Visit(statement, null);
            return fmt._cb.ToString();
        }
*/
        public static string Format(Expression expression) {
            var fmt = new OutputFormatter();
            fmt.Visit(expression, null);
            return fmt._cb.ToString();
        }

        public object Visit(Expression expression, object data) {
            return expression.Accept(this, data);
        }

        public object Visit(ArrayLiteralExpression expression, object data) {
            _cb.Append("[");
            for (int i = 0; i < expression.Elements.Count; i++) {
                if (i != 0)
                    _cb.Append(", ");
                var x = expression.Elements[i];
                if (x is CommaExpression)
                    _cb.Append("(");
                Visit(x, null);
                if (x is CommaExpression)
                    _cb.Append(")");
            }
            _cb.Append("]");
            return null;
        }

        public object Visit(BinaryExpression expression, object data) {
            if (expression.Operator == BinaryOperator.Index) {
                throw new NotImplementedException();
            }
            else {
                bool parenLeft  = expression.Left.Precedence > expression.Precedence - (expression.IsRightAssociative ? 1 : 0);
                bool parenRight = expression.Right.Precedence > expression.Precedence - (expression.IsRightAssociative ? 0 : 1);
                if (parenLeft) _cb.Append("(");
                Visit(expression.Left, null);
                if (parenLeft) _cb.Append(")");
                _cb.Append(" ").Append(GetOperatorString(expression.Operator)).Append(" ");
                if (parenRight) _cb.Append("(");
                Visit(expression.Right, null);
                if (parenRight) _cb.Append(")");
            }
            return null;
        }

        public object Visit(CommaExpression expression, object data) {
            for (int i = 0; i < expression.Expressions.Count; i++) {
                if (i > 0)
                    _cb.Append(", ");
                Visit(expression.Expressions[i], null);
            }
            return null;
        }

        public object Visit(ConditionalExpression expression, object data) {
            _cb.Append("(");
            var parenTest  = (expression.Test.Precedence >= ExpressionPrecedence.Multiply);
            var parenTrue  = (expression.TruePart.Precedence >= ExpressionPrecedence.Multiply);
            var parenFalse = (expression.FalsePart.Precedence >= ExpressionPrecedence.Multiply);

            throw new NotImplementedException();
        }

        public object Visit(ConstantExpression expression, object data) {
            _cb.Append(expression.Format());
            return null;
        }

        public object Visit(FunctionExpression expression, object data) {
            throw new NotImplementedException();
        }

        public object Visit(IdentifierExpression expression, object data) {
            throw new NotImplementedException();
        }

        public object Visit(InvocationExpression expression, object data) {
            throw new NotImplementedException();
        }

        public object Visit(JsonExpression expression, object data) {
            throw new NotImplementedException();
        }

        public object Visit(MemberAccessExpression expression, object data) {
            throw new NotImplementedException();
        }

        public object Visit(NewExpression expression, object data) {
            throw new NotImplementedException();
        }

        public object Visit(UnaryExpression expression, object data) {
            throw new NotImplementedException();
        }

        private static string GetOperatorString(BinaryOperator oper) {
            switch (oper) {
                case BinaryOperator.Multiply:                 return "*";
                case BinaryOperator.Divide:                   return "/";
                case BinaryOperator.Modulo:                   return "%";
                case BinaryOperator.Add:                      return "+";
                case BinaryOperator.Subtract:                 return "-";
                case BinaryOperator.LeftShift:                return "<<";
                case BinaryOperator.RightShiftSigned:         return ">>";
                case BinaryOperator.RightShiftUnsigned:       return ">>>";
                case BinaryOperator.Lesser:                   return "<";
                case BinaryOperator.LesserOrEqual:            return "<=";
                case BinaryOperator.Greater:                  return ">";
                case BinaryOperator.GreaterOrEqual:           return ">=";
                case BinaryOperator.In:                       return "in";
                case BinaryOperator.InstanceOf:               return "instanceof";
                case BinaryOperator.Equal:                    return "==";
                case BinaryOperator.NotEqual:                 return "!=";
                case BinaryOperator.Same:                     return "===";
                case BinaryOperator.NotSame:                  return "!==";
                case BinaryOperator.BitwiseAnd:               return "&";
                case BinaryOperator.BitwiseXor:               return "^";
                case BinaryOperator.BitwiseOr:                return "|";
                case BinaryOperator.LogicalAnd:               return "&&";
                case BinaryOperator.LogicalOr:                return "||";
                case BinaryOperator.Assign:                   return "=";
                case BinaryOperator.MultiplyAssign:           return "*=";
                case BinaryOperator.DivideAssign:             return "/=";
                case BinaryOperator.ModuloAssign:             return "%=";
                case BinaryOperator.AddAssign:                return "+=";
                case BinaryOperator.SubtractAssign:           return "-=";
                case BinaryOperator.LeftShiftAssign:          return "<<=";
                case BinaryOperator.RightShiftAssign:         return ">>=";
                case BinaryOperator.UnsignedRightShiftAssign: return ">>>=";
                case BinaryOperator.BitwiseAndAssign:         return "&=";
                case BinaryOperator.BitwiseOrAssign:          return "|=";
                case BinaryOperator.BitwiseXOrAssign:         return "^=";
                case BinaryOperator.Index:
                default:
                    throw new InvalidOperationException("Invalid operator " + oper.ToString());
            }
        }
    }

/*        private CodeBuilder _cb = new CodeBuilder();

        private OutputFormatter() {
        }

        public static string Format(Statement statement) {
            var fmt = new OutputFormatter();
            fmt.Visit(statement, null);
            return fmt._cb.ToString();
        }

        public static string Format(Expression expression) {
            var fmt = new OutputFormatter();
            fmt.Visit(expression, null);
            return fmt._cb.ToString();
        }

        #region Overrides of RewriterVisitorBase

        public override IEnumerable<Expression> Visit(ReadOnlyCollection<Expression> expressions, string data) {
            throw new NotImplementedException();
        }

        public override IEnumerable<JsonExpression.ValueEntry> Visit(ReadOnlyCollection<JsonExpression.ValueEntry> values, string data) {
            throw new NotImplementedException();
        }

        public override JsonExpression.ValueEntry Visit(JsonExpression.ValueEntry value, string data) {
            throw new NotImplementedException();
        }

        public override Expression Visit(Expression expression, string data) {
            return base.Visit(expression, data);
        }

        public override Expression Visit(ArrayLiteralExpression expression, string data) {
            throw new NotImplementedException();
        }

        public override Expression Visit(BinaryExpression expression, string data) {
        }

        public override Expression Visit(CommaExpression expression, string data) {
            throw new NotImplementedException();
        }

        public override Expression Visit(ConditionalExpression expression, string data) {
            throw new NotImplementedException();
        }

        public override Expression Visit(ConstantExpression expression, string data) {
            throw new NotImplementedException();
        }

        public override Expression Visit(FunctionExpression expression, string data) {
            throw new NotImplementedException();
        }

        public override Expression Visit(IdentifierExpression expression, string data) {
            throw new NotImplementedException();
        }

        public override Expression Visit(InvocationExpression expression, string data) {
            throw new NotImplementedException();
        }

        public override Expression Visit(JsonExpression expression, string data) {
            throw new NotImplementedException();
        }

        public override Expression Visit(MemberAccessExpression expression, string data) {
            throw new NotImplementedException();
        }

        public override Expression Visit(NewExpression expression, string data) {
            throw new NotImplementedException();
        }

        public override Expression Visit(UnaryExpression expression, string data) {
            throw new NotImplementedException();
        }

        public override IEnumerable<Statement> Visit(ReadOnlyCollection<Statement> statements, string data) {
            throw new NotImplementedException();
        }

        public override IEnumerable<SwitchStatement.Clause> Visit(ReadOnlyCollection<SwitchStatement.Clause> clauses, string data) {
            throw new NotImplementedException();
        }

        public override IEnumerable<VariableDeclarationStatement.VariableDeclaration> Visit(ReadOnlyCollection<VariableDeclarationStatement.VariableDeclaration> declarations, string data) {
            throw new NotImplementedException();
        }

        public override SwitchStatement.Clause Visit(SwitchStatement.Clause clause, string data) {
            throw new NotImplementedException();
        }

        public override TryCatchFinallyStatement.CatchClause Visit(TryCatchFinallyStatement.CatchClause clause, string data) {
            throw new NotImplementedException();
        }

        public override VariableDeclarationStatement.VariableDeclaration Visit(VariableDeclarationStatement.VariableDeclaration declaration, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(Statement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(BlockStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(BreakStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(ContinueStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(DoWhileStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(EmptyStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(ExpressionStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(ForEachInStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(ForStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(IfStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(ReturnStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(SwitchStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(ThrowStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(TryCatchFinallyStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(VariableDeclarationStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(WhileStatement statement, string data) {
            throw new NotImplementedException();
        }

        public override Statement Visit(WithStatement statement, string data) {
            throw new NotImplementedException();
        }

        #endregion*/
//    }
}
