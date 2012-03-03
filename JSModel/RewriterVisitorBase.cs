using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel
{
    public abstract class RewriterVisitorBase<TData> : IExpressionVisitor<JsExpression, TData>, IStatementVisitor<JsStatement, TData> {
        protected static IEnumerable<T> VisitCollection<T>(ReadOnlyCollection<T> orig, Func<T, T> visitor) {
            List<T> list = null;
            for (int i = 0; i < orig.Count; i++) {
                var before = orig[i];
                var after  = visitor(before);
                if (list != null) {
                    list.Add(after);
                }
                else if (!ReferenceEquals(before, after)) {
                    list = new List<T>();
                    for (int j = 0; j < i; j++)
                        list.Add(orig[j]);
                    list.Add(after);
                }
            }
            return list != null ? (IEnumerable<T>)list : orig;
        }

        public virtual IEnumerable<JsExpression> Visit(ReadOnlyCollection<JsExpression> expressions, TData data) {
            return VisitCollection(expressions, expr => Visit(expr, data));
        }

        public virtual IEnumerable<JsObjectLiteralProperty> Visit(ReadOnlyCollection<JsObjectLiteralProperty> values, TData data) {
            return VisitCollection(values, v => Visit(v, data));
        }

        public virtual JsObjectLiteralProperty Visit(JsObjectLiteralProperty value, TData data) {
            var after = Visit(value.Value, data);
            return ReferenceEquals(after, value.Value) ? value : new JsObjectLiteralProperty(value.Name, after);
        }

        public virtual JsExpression Visit(JsExpression expression, TData data) {
            return expression.Accept(this, data);
        }

        public virtual JsExpression Visit(JsArrayLiteralExpression expression, TData data) {
            var after  = Visit(expression.Elements, data);
            return ReferenceEquals(after, expression.Elements) ? expression : JsExpression.ArrayLiteral(after);
        }

        public virtual JsExpression Visit(JsBinaryExpression expression, TData data) {
            var left  = Visit(expression.Left, data);
            var right = Visit(expression.Right, data);
            return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right) ? expression : JsExpression.Binary(expression.NodeType, left, right);
        }

        public virtual JsExpression Visit(JsCommaExpression expression, TData data) {
            var after = Visit(expression.Expressions, data);
            return ReferenceEquals(after, expression.Expressions) ? expression : JsExpression.Comma(after);
        }

        public virtual JsExpression Visit(JsConditionalExpression expression, TData data) {
            var test      = Visit(expression.Test, data);
            var truePart  = Visit(expression.TruePart, data);
            var falsePart = Visit(expression.FalsePart, data);
            return ReferenceEquals(test, expression.Test) && ReferenceEquals(truePart, expression.TruePart) && ReferenceEquals(falsePart, expression.FalsePart) ? expression : JsExpression.Conditional(test, truePart, falsePart);
        }

        public virtual JsExpression Visit(JsConstantExpression expression, TData data) {
            return expression;
        }

        public virtual JsExpression Visit(JsFunctionDefinitionExpression expression, TData data) {
            var body = Visit(expression.Body, data);
            return ReferenceEquals(body, expression.Body) ? expression : JsExpression.FunctionDefinition(expression.ParameterNames, body, expression.Name);
        }

        public virtual JsExpression Visit(JsIdentifierExpression expression, TData data) {
            return expression;
        }

        public virtual JsExpression Visit(JsInvocationExpression expression, TData data) {
            var method    = Visit(expression.Method, data);
            var arguments = Visit(expression.Arguments, data);
            return ReferenceEquals(method, expression.Method) && ReferenceEquals(arguments, expression.Arguments) ? expression : JsExpression.Invocation(method, arguments);
        }

        public virtual JsExpression Visit(JsObjectLiteralExpression expression, TData data) {
            var values = Visit(expression.Values, data);
            return ReferenceEquals(values, expression.Values) ? expression : JsExpression.ObjectLiteral(values);
        }

        public virtual JsExpression Visit(JsMemberAccessExpression expression, TData data) {
            var target = Visit(expression.Target, data);
            return ReferenceEquals(target, expression.Target) ? expression : JsExpression.MemberAccess(target, expression.Member);
        }

        public virtual JsExpression Visit(JsNewExpression expression, TData data) {
            var constructor = Visit(expression.Constructor, data);
            var arguments   = Visit(expression.Arguments, data);
            return ReferenceEquals(constructor, expression.Constructor) && ReferenceEquals(arguments, expression.Arguments) ? expression : JsExpression.New(constructor, arguments);
        }

        public virtual JsExpression Visit(JsUnaryExpression expression, TData data) {
            var operand = Visit(expression.Operand, data);
            return ReferenceEquals(operand, expression.Operand) ? expression : JsExpression.Unary(expression.NodeType, operand);
        }

        public virtual JsExpression Visit(JsTypeReferenceExpression expression, TData data) {
            return expression;
        }

        public virtual JsExpression Visit(JsThisExpression expression, TData data) {
            return expression;
        }

        public virtual IEnumerable<JsStatement> Visit(ReadOnlyCollection<JsStatement> statements, TData data) {
            return VisitCollection(statements, s => Visit(s, data));
        }

        public virtual IEnumerable<JsSwitchStatement.Clause> Visit(ReadOnlyCollection<JsSwitchStatement.Clause> clauses, TData data) {
            return VisitCollection(clauses, c => Visit(c, data));
        }

        public virtual IEnumerable<JsVariableDeclaration> Visit(ReadOnlyCollection<JsVariableDeclaration> declarations, TData data) {
            return VisitCollection(declarations, d => Visit(d, data));
        }

        public virtual JsSwitchStatement.Clause Visit(JsSwitchStatement.Clause clause, TData data) {
            var value = clause.Value != null ? Visit(clause.Value, data) : null;
            var body  = Visit(clause.Body, data);
            return ReferenceEquals(value, clause.Value) && ReferenceEquals(body, clause.Body) ? clause : new JsSwitchStatement.Clause(value, body);
        }

        public virtual JsTryCatchFinallyStatement.CatchClause Visit(JsTryCatchFinallyStatement.CatchClause clause, TData data) {
            var body = Visit(clause.Body, data);
            return ReferenceEquals(body, clause.Body) ? clause : new JsTryCatchFinallyStatement.CatchClause(clause.Identifier, body);
        }

        public virtual JsVariableDeclaration Visit(JsVariableDeclaration declaration, TData data) {
            var after = Visit(declaration.Initializer, data);
            return ReferenceEquals(after, declaration.Initializer) ? declaration : new JsVariableDeclaration(declaration.Name, after);
        }

        public virtual JsStatement Visit(JsStatement statement, TData data) {
            return statement.Accept(this, data);
        }

        public virtual JsStatement Visit(JsComment comment, TData data) {
            return comment;
        }

        public virtual JsStatement Visit(JsBlockStatement statement, TData data) {
            var after = Visit(statement.Statements, data);
            return ReferenceEquals(after, statement.Statements) ? statement : new JsBlockStatement(after);
        }

        public virtual JsStatement Visit(JsBreakStatement statement, TData data) {
            return statement;
        }

        public virtual JsStatement Visit(JsContinueStatement statement, TData data) {
            return statement;
        }

        public virtual JsStatement Visit(JsDoWhileStatement statement, TData data) {
            var condition = Visit(statement.Condition, data);
            var body      = Visit(statement.Body, data);
            return ReferenceEquals(condition, statement.Condition) && ReferenceEquals(body, statement.Body) ? statement : new JsDoWhileStatement(condition, body);
        }

        public virtual JsStatement Visit(JsEmptyStatement statement, TData data) {
            return statement;
        }

        public virtual JsStatement Visit(JsExpressionStatement statement, TData data) {
            var after = Visit(statement.Expression, data);
            return ReferenceEquals(after, statement.Expression) ? statement : new JsExpressionStatement(after);
        }

        public virtual JsStatement Visit(JsForEachInStatement statement, TData data) {
            var objectToIterateOver = Visit(statement.ObjectToIterateOver, data);
            var body = Visit(statement.Body, data);
            return ReferenceEquals(objectToIterateOver, statement.ObjectToIterateOver) && ReferenceEquals(body, statement.Body) ? statement : new JsForEachInStatement(statement.LoopVariableName, objectToIterateOver, body, statement.IsLoopVariableDeclared);
        }

        public virtual JsStatement Visit(JsForStatement statement, TData data) {
            var initStatement = Visit(statement.InitStatement, data);
            var condition     = Visit(statement.ConditionExpression, data);
            var incr          = Visit(statement.IncrementExpression, data);
            var body          = Visit(statement.Body, data);
            return ReferenceEquals(initStatement, statement.InitStatement) && ReferenceEquals(condition, statement.ConditionExpression) && ReferenceEquals(incr, statement.IncrementExpression) && ReferenceEquals(body, statement.Body)
                 ? statement
                 : new JsForStatement(initStatement, condition, incr, body);
        }

        public virtual JsStatement Visit(JsIfStatement statement, TData data) {
            var test  = Visit(statement.Test, data);
            var then  = Visit(statement.Then, data);
            var @else = statement.Else != null ? Visit(statement.Else, data) : null;
            return ReferenceEquals(test, statement.Test) && ReferenceEquals(then, statement.Then) && ReferenceEquals(@else, statement.Else) ? statement : new JsIfStatement(test, then, @else);
        }

        public virtual JsStatement Visit(JsReturnStatement statement, TData data) {
            var value = (statement.Value != null ? Visit(statement.Value, data) : null);
            return ReferenceEquals(value, statement.Value) ? statement : new JsReturnStatement(value);
        }

        public virtual JsStatement Visit(JsSwitchStatement statement, TData data) {
            var test = Visit(statement.Test, data);
            var clauses = Visit(statement.Clauses, data);
            return ReferenceEquals(test, statement.Test) && ReferenceEquals(clauses, statement.Clauses) ? statement : new JsSwitchStatement(test, clauses);
        }

        public virtual JsStatement Visit(JsThrowStatement statement, TData data) {
            var expr = Visit(statement.Expression, data);
            return ReferenceEquals(expr, statement.Expression) ? statement : new JsThrowStatement(expr);
        }

        public virtual JsStatement Visit(JsTryCatchFinallyStatement statement, TData data) {
            var guarded  = Visit(statement.GuardedStatement, data);
            var @catch   = statement.Catch != null ? Visit(statement.Catch, data) : null;
            var @finally = statement.Finally != null ? Visit(statement.Finally, data) : null;
            return ReferenceEquals(guarded, statement.GuardedStatement) && ReferenceEquals(@catch, statement.Catch) && ReferenceEquals(@finally, statement.Finally) ? statement : new JsTryCatchFinallyStatement(guarded, @catch, @finally);
        }

        public virtual JsStatement Visit(JsVariableDeclarationStatement statement, TData data) {
            var declarations = Visit(statement.Declarations, data);
            return ReferenceEquals(declarations, statement.Declarations) ? statement : new JsVariableDeclarationStatement(declarations);
        }

        public virtual JsStatement Visit(JsWhileStatement statement, TData data) {
            var condition = Visit(statement.Condition, data);
            var body      = Visit(statement.Body, data);
            return ReferenceEquals(condition, statement.Condition) && ReferenceEquals(body, statement.Body) ? statement : new JsWhileStatement(condition, body);
        }

        public virtual JsStatement Visit(JsWithStatement statement, TData data) {
            var @object = Visit(statement.Object, data);
            var body    = Visit(statement.Body, data);
            return ReferenceEquals(@object, statement.Object) && ReferenceEquals(body, statement.Body) ? statement : new JsWithStatement(@object, body);
        }
    }
}
