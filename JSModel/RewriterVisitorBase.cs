using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel
{
    public abstract class RewriterVisitorBase<TData> : IExpressionVisitor<Expression, TData>, IStatementVisitor<Statement, TData> {
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

        public virtual IEnumerable<Expression> Visit(ReadOnlyCollection<Expression> expressions, TData data) {
            return VisitCollection(expressions, expr => Visit(expr, data));
        }

        public virtual IEnumerable<ObjectLiteralExpression.ValueEntry> Visit(ReadOnlyCollection<ObjectLiteralExpression.ValueEntry> values, TData data) {
            return VisitCollection(values, v => Visit(v, data));
        }

        public virtual ObjectLiteralExpression.ValueEntry Visit(ObjectLiteralExpression.ValueEntry value, TData data) {
            var after = Visit(value.Value, data);
            return ReferenceEquals(after, value.Value) ? value : new ObjectLiteralExpression.ValueEntry(value.Name, after);
        }

        public virtual Expression Visit(Expression expression, TData data) {
            return expression.Accept(this, data);
        }

        public virtual Expression Visit(ArrayLiteralExpression expression, TData data) {
            var after  = Visit(expression.Elements, data);
            return ReferenceEquals(after, expression.Elements) ? expression : new ArrayLiteralExpression(after);
        }

        public virtual Expression Visit(BinaryExpression expression, TData data) {
            var left  = Visit(expression.Left, data);
            var right = Visit(expression.Right, data);
            return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right) ? expression : new BinaryExpression(expression.Operator, left, right);
        }

        public virtual Expression Visit(CommaExpression expression, TData data) {
            var after = Visit(expression.Expressions, data);
            return ReferenceEquals(after, expression.Expressions) ? expression : new CommaExpression(after);
        }

        public virtual Expression Visit(ConditionalExpression expression, TData data) {
            var test      = Visit(expression.Test, data);
            var truePart  = Visit(expression.TruePart, data);
            var falsePart = Visit(expression.FalsePart, data);
            return ReferenceEquals(test, expression.Test) && ReferenceEquals(truePart, expression.TruePart) && ReferenceEquals(falsePart, expression.FalsePart) ? expression : new ConditionalExpression(test, truePart, falsePart);
        }

        public virtual Expression Visit(ConstantExpression expression, TData data) {
            return expression;
        }

        public virtual Expression Visit(FunctionExpression expression, TData data) {
            var body = Visit(expression.Body, data);
            return ReferenceEquals(body, expression.Body) ? expression : new FunctionExpression(expression.ParameterNames, body, expression.Name);
        }

        public virtual Expression Visit(IdentifierExpression expression, TData data) {
            return expression;
        }

        public virtual Expression Visit(InvocationExpression expression, TData data) {
            var method    = Visit(expression.Method, data);
            var arguments = Visit(expression.Arguments, data);
            return ReferenceEquals(method, expression.Method) && ReferenceEquals(arguments, expression.Arguments) ? expression : new InvocationExpression(method, arguments);
        }

        public virtual Expression Visit(ObjectLiteralExpression expression, TData data) {
            var values = Visit(expression.Values, data);
            return ReferenceEquals(values, expression.Values) ? expression : new ObjectLiteralExpression(values);
        }

        public virtual Expression Visit(MemberAccessExpression expression, TData data) {
            var target = Visit(expression.Target, data);
            return ReferenceEquals(target, expression.Target) ? expression : new MemberAccessExpression(target, expression.Member);
        }

        public virtual Expression Visit(NewExpression expression, TData data) {
            var constructor = Visit(expression.Constructor, data);
            var arguments   = Visit(expression.Arguments, data);
            return ReferenceEquals(constructor, expression.Constructor) && ReferenceEquals(arguments, expression.Arguments) ? expression : new NewExpression(constructor, arguments);
        }

        public virtual Expression Visit(UnaryExpression expression, TData data) {
            var operand = Visit(expression.Operand, data);
            return ReferenceEquals(operand, expression.Operand) ? expression : new UnaryExpression(expression.Operator, operand);
        }

        public virtual IEnumerable<Statement> Visit(ReadOnlyCollection<Statement> statements, TData data) {
            return VisitCollection(statements, s => Visit(s, data));
        }

        public virtual IEnumerable<SwitchStatement.Clause> Visit(ReadOnlyCollection<SwitchStatement.Clause> clauses, TData data) {
            return VisitCollection(clauses, c => Visit(c, data));
        }

        public virtual IEnumerable<VariableDeclarationStatement.VariableDeclaration> Visit(ReadOnlyCollection<VariableDeclarationStatement.VariableDeclaration> declarations, TData data) {
            return VisitCollection(declarations, d => Visit(d, data));
        }

        public virtual SwitchStatement.Clause Visit(SwitchStatement.Clause clause, TData data) {
            var value = clause.Value != null ? Visit(clause.Value, data) : null;
            var body  = Visit(clause.Body, data);
            return ReferenceEquals(value, clause.Value) && ReferenceEquals(body, clause.Body) ? clause : new SwitchStatement.Clause(value, body);
        }

        public virtual TryCatchFinallyStatement.CatchClause Visit(TryCatchFinallyStatement.CatchClause clause, TData data) {
            var body = Visit(clause.Body, data);
            return ReferenceEquals(body, clause.Body) ? clause : new TryCatchFinallyStatement.CatchClause(clause.Identifier, body);
        }

        public virtual VariableDeclarationStatement.VariableDeclaration Visit(VariableDeclarationStatement.VariableDeclaration declaration, TData data) {
            var after = Visit(declaration.Initializer, data);
            return ReferenceEquals(after, declaration.Initializer) ? declaration : new VariableDeclarationStatement.VariableDeclaration(declaration.Name, after);
        }

        public virtual Statement Visit(Statement statement, TData data) {
            return statement.Accept(this, data);
        }

        public virtual Statement Visit(BlockStatement statement, TData data) {
            var after = Visit(statement.Statements, data);
            return ReferenceEquals(after, statement.Statements) ? statement : new BlockStatement(after, statement.StatementLabel);
        }

        public virtual Statement Visit(BreakStatement statement, TData data) {
            return statement;
        }

        public virtual Statement Visit(ContinueStatement statement, TData data) {
            return statement;
        }

        public virtual Statement Visit(DoWhileStatement statement, TData data) {
            var condition = Visit(statement.Condition, data);
            var body      = Visit(statement.Body, data);
            return ReferenceEquals(condition, statement.Condition) && ReferenceEquals(body, statement.Body) ? statement : new DoWhileStatement(condition, body, statement.StatementLabel);
        }

        public virtual Statement Visit(EmptyStatement statement, TData data) {
            return statement;
        }

        public virtual Statement Visit(ExpressionStatement statement, TData data) {
            var after = Visit(statement.Expression, data);
            return ReferenceEquals(after, statement.Expression) ? statement : new ExpressionStatement(after, statement.StatementLabel);
        }

        public virtual Statement Visit(ForEachInStatement statement, TData data) {
            var objectToIterateOver = Visit(statement.ObjectToIterateOver, data);
            var body = Visit(statement.Body, data);
            return ReferenceEquals(objectToIterateOver, statement.ObjectToIterateOver) && ReferenceEquals(body, statement.Body) ? statement : new ForEachInStatement(statement.LoopVariableName, objectToIterateOver, body, statement.IsLoopVariableDeclared, statement.StatementLabel);
        }

        public virtual Statement Visit(ForStatement statement, TData data) {
            var initStatement = Visit(statement.InitStatement, data);
            var condition     = Visit(statement.ConditionExpression, data);
            var incr          = Visit(statement.IncrementExpression, data);
            var body          = Visit(statement.Body, data);
            return ReferenceEquals(initStatement, statement.InitStatement) && ReferenceEquals(condition, statement.ConditionExpression) && ReferenceEquals(incr, statement.IncrementExpression) && ReferenceEquals(body, statement.Body)
                 ? statement
                 : new ForStatement(initStatement, condition, incr, body, statement.StatementLabel);
        }

        public virtual Statement Visit(IfStatement statement, TData data) {
            var test  = Visit(statement.Test, data);
            var then  = Visit(statement.Then, data);
            var @else = statement.Else != null ? Visit(statement.Else, data) : null;
            return ReferenceEquals(test, statement.Test) && ReferenceEquals(then, statement.Then) && ReferenceEquals(@else, statement.Else) ? statement : new IfStatement(test, then, @else, statement.StatementLabel);
        }

        public virtual Statement Visit(ReturnStatement statement, TData data) {
            var value = (statement.Value != null ? Visit(statement.Value, data) : null);
            return ReferenceEquals(value, statement.Value) ? statement : new ReturnStatement(value, statement.StatementLabel);
        }

        public virtual Statement Visit(SwitchStatement statement, TData data) {
            var test = Visit(statement.Test, data);
            var clauses = Visit(statement.Clauses, data);
            return ReferenceEquals(test, statement.Test) && ReferenceEquals(clauses, statement.Clauses) ? statement : new SwitchStatement(test, clauses, statement.StatementLabel);
        }

        public virtual Statement Visit(ThrowStatement statement, TData data) {
            var expr = Visit(statement.Expression, data);
            return ReferenceEquals(expr, statement.Expression) ? statement : new ThrowStatement(expr, statement.StatementLabel);
        }

        public virtual Statement Visit(TryCatchFinallyStatement statement, TData data) {
            var guarded  = Visit(statement.GuardedStatement, data);
            var @catch   = statement.Catch != null ? Visit(statement.Catch, data) : null;
            var @finally = statement.Finally != null ? Visit(statement.Finally, data) : null;
            return ReferenceEquals(guarded, statement.GuardedStatement) && ReferenceEquals(@catch, statement.Catch) && ReferenceEquals(@finally, statement.Finally) ? statement : new TryCatchFinallyStatement(guarded, @catch, @finally, statement.StatementLabel);
        }

        public virtual Statement Visit(VariableDeclarationStatement statement, TData data) {
            var declarations = Visit(statement.Declarations, data);
            return ReferenceEquals(declarations, statement.Declarations) ? statement : new VariableDeclarationStatement(declarations, statement.StatementLabel);
        }

        public virtual Statement Visit(WhileStatement statement, TData data) {
            var condition = Visit(statement.Condition, data);
            var body      = Visit(statement.Body, data);
            return ReferenceEquals(condition, statement.Condition) && ReferenceEquals(body, statement.Body) ? statement : new WhileStatement(condition, body, statement.StatementLabel);
        }

        public virtual Statement Visit(WithStatement statement, TData data) {
            var @object = Visit(statement.Object, data);
            var body    = Visit(statement.Body, data);
            return ReferenceEquals(@object, statement.Object) && ReferenceEquals(body, statement.Body) ? statement : new WithStatement(@object, body, statement.StatementLabel);
        }
    }
}
