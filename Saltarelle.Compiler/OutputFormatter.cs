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
    public class OutputFormatter : IExpressionVisitor<object, bool>
    {
        private readonly CodeBuilder _cb = new CodeBuilder();

        private OutputFormatter() {
        }

        public static string Format(Expression expression) {
            var fmt = new OutputFormatter();
            fmt.Visit(expression, false);
            return fmt._cb.ToString();
        }

        public object Visit(Expression expression, bool parenthesized) {
            if (parenthesized)
                _cb.Append("(");
            expression.Accept(this, parenthesized);
            if (parenthesized)
                _cb.Append(")");
            return null;
        }

        private void VisitExpressionList(IEnumerable<Expression> expressions) {
            bool first = true;
            foreach (var x in expressions) {
                if (!first)
                    _cb.Append(", ");
                Visit(x, x.Precedence >= ExpressionPrecedence.Comma); // We ned to parenthesize comma expressions, eg. [1, (2, 3), 4]
                first = false;
            }
        }

        public object Visit(ArrayLiteralExpression expression, bool parenthesized) {
            _cb.Append("[");
            VisitExpressionList(expression.Elements);
            _cb.Append("]");
            return null;
        }

        public object Visit(BinaryExpression expression, bool parenthesized) {
            if (expression.Operator == BinaryOperator.Index) {
                Visit(expression.Left, expression.Left.Precedence > expression.Precedence);
                _cb.Append("[");
                Visit(expression.Right, false);
                _cb.Append("]");
            }
            else {
                Visit(expression.Left, expression.Left.Precedence > expression.Precedence - (expression.IsRightAssociative ? 1 : 0));
                _cb.Append(" ").Append(GetOperatorString(expression.Operator)).Append(" ");
                Visit(expression.Right, expression.Right.Precedence > expression.Precedence - (expression.IsRightAssociative ? 0 : 1));
            }
            return null;
        }

        public object Visit(CommaExpression expression, bool parenthesized) {
            for (int i = 0; i < expression.Expressions.Count; i++) {
                if (i > 0)
                    _cb.Append(", ");
                Visit(expression.Expressions[i], false);
            }
            return null;
        }

        public object Visit(ConditionalExpression expression, bool parenthesized) {
            // Always parenthesize conditionals (but beware of double parentheses). Better this than accidentally getting the tricky precedence wrong sometimes.
            if (!parenthesized)
                _cb.Append("(");

            // Also, be rather liberal when parenthesizing the operands, partly to avoid bugs, partly for readability.
            Visit(expression.Test, expression.Test.Precedence >= ExpressionPrecedence.Multiply);
            _cb.Append(" ? ");
            Visit(expression.TruePart, expression.TruePart.Precedence >= ExpressionPrecedence.Multiply);
            _cb.Append(" : ");
            Visit(expression.FalsePart, expression.FalsePart.Precedence >= ExpressionPrecedence.Multiply);

            if (!parenthesized)
                _cb.Append(")");

            return null;
        }

        public object Visit(ConstantExpression expression, bool parenthesized) {
            _cb.Append(expression.Format());
            return null;
        }

        public object Visit(FunctionExpression expression, bool parenthesized) {
            throw new NotImplementedException();
        }

        public object Visit(IdentifierExpression expression, bool parenthesized) {
            _cb.Append(expression.Name);
            return null;
        }

        public object Visit(InvocationExpression expression, bool parenthesized) {
            Visit(expression.Method, expression.Method.Precedence > expression.Precedence || (expression.Method is NewExpression)); // Ugly code to make sure that we put parentheses around "new", eg. "(new X())(1)" rather than "new X()(1)"
            _cb.Append("(");
            VisitExpressionList(expression.Arguments);
            _cb.Append(")");
            return null;
        }

        public object Visit(ObjectLiteralExpression expression, bool parenthesized) {
            if (expression.Values.Count == 0) {
                _cb.Append("{}");
            }
            else {
                _cb.Append("{ ");
                bool first = true;
                foreach (var v in expression.Values) {
                    if (!first)
                        _cb.Append(", ");
                    _cb.Append(v.Name.IsValidJavaScriptIdentifier() ? v.Name : ("'" + ConstantExpression.FixStringLiteral(v.Name, false) + "'"))
                       .Append(": ");
                    Visit(v.Value, v.Value.Precedence >= ExpressionPrecedence.Comma); // We ned to parenthesize comma expressions, eg. [1, (2, 3), 4]
                    first = false;
                }
                _cb.Append(" }");
            }
            return null;
        }

        public object Visit(MemberAccessExpression expression, bool parenthesized) {
            Visit(expression.Target, (expression.Target.Precedence >= expression.Precedence) && !(expression.Target is MemberAccessExpression)); // Ugly code to ensure that nested member accesses are not parenthesized, but member access nested in new are (and vice versa)
            _cb.Append(".");
            _cb.Append(expression.Member);
            return null;
        }

        public object Visit(NewExpression expression, bool parenthesized) {
            _cb.Append("new ");
            Visit(expression.Constructor, expression.Constructor.Precedence >= expression.Precedence);
            _cb.Append("(");
            VisitExpressionList(expression.Arguments);
            _cb.Append(")");
            return null;
        }

        public object Visit(UnaryExpression expression, bool parenthesized) {
            string prefix = "", postfix = "";
            bool alwaysParenthesize = false;
            switch (expression.Operator) {
                case UnaryOperator.PrefixPlusPlus:        prefix = "++"; break;
                case UnaryOperator.PrefixMinusMinus:      prefix = "--"; break;
                case UnaryOperator.PostfixPlusPlus:       postfix = "++"; break;
                case UnaryOperator.PostfixMinusMinus:     postfix = "--"; break;
                case UnaryOperator.LogicalNot:            prefix = "!"; break;
                case UnaryOperator.BitwiseNot:            prefix = "~"; break;
                case UnaryOperator.Positive:              prefix = "+"; break;
                case UnaryOperator.Negate:                prefix = "-"; break;
                case UnaryOperator.TypeOf:                prefix = "typeof"; alwaysParenthesize = true; break;
                case UnaryOperator.Void:                  prefix = "void"; alwaysParenthesize = true; break;
                case UnaryOperator.Delete:                prefix = "delete "; break;
                default: throw new ArgumentException("expression");
            }
            _cb.Append(prefix);
            Visit(expression.Operand, (expression.Operand.Precedence >= ExpressionPrecedence.IncrDecr) || alwaysParenthesize);
            _cb.Append(postfix);
            return null;
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
}
