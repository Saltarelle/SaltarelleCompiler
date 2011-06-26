using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel
{
    public abstract class RewriterVisitorBase : IExpressionVisitor<Expression>, IStatementVisitor<Statement> {
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

        public virtual IEnumerable<Expression> Visit(ReadOnlyCollection<Expression> expressions) {
            return VisitCollection(expressions, Visit);
        }

        public virtual IEnumerable<JsonExpression.ValueEntry> Visit(ReadOnlyCollection<JsonExpression.ValueEntry> values) {
            return VisitCollection(values, Visit);
        }

        public virtual JsonExpression.ValueEntry Visit(JsonExpression.ValueEntry value) {
            var after = Visit(value.Value);
            return ReferenceEquals(after, value.Value) ? value : new JsonExpression.ValueEntry(value.Name, after);
        }

        public virtual Expression Visit(Expression expression) {
            return expression.Accept(this);
        }

        public virtual Expression Visit(ArrayLiteralExpression expression) {
            var after  = Visit(expression.Elements);
            return ReferenceEquals(after, expression.Elements) ? expression : new ArrayLiteralExpression(after);
        }

        public virtual Expression Visit(BinaryExpression expression) {
            var left  = Visit(expression.Left);
            var right = Visit(expression.Right);
            return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right) ? expression : new BinaryExpression(expression.Operator, left, right);
        }

        public virtual Expression Visit(CommaExpression expression) {
            var after = Visit(expression.Expressions);
            return ReferenceEquals(after, expression.Expressions) ? expression : new CommaExpression(after);
        }

        public virtual Expression Visit(ConditionalExpression expression) {
            var test      = Visit(expression.Test);
            var truePart  = Visit(expression.TruePart);
            var falsePart = Visit(expression.FalsePart);
            return ReferenceEquals(test, expression.Test) && ReferenceEquals(truePart, expression.TruePart) && ReferenceEquals(falsePart, expression.FalsePart) ? expression : new ConditionalExpression(test, truePart, falsePart);
        }

        public virtual Expression Visit(ConstantExpression expression) {
            return expression;
        }

        public virtual Expression Visit(FunctionExpression expression) {
            var body = Visit(expression.Body);
            return ReferenceEquals(body, expression.Body) ? expression : new FunctionExpression(expression.ParameterNames, body, expression.Name);
        }

        public virtual Expression Visit(IdentifierExpression expression) {
            return expression;
        }

        public virtual Expression Visit(InvocationExpression expression) {
            var method    = Visit(expression.Method);
            var arguments = Visit(expression.Arguments);
            return ReferenceEquals(method, expression.Method) && ReferenceEquals(arguments, expression.Arguments) ? expression : new InvocationExpression(method, arguments);
        }

        public virtual Expression Visit(JsonExpression expression) {
            var values = Visit(expression.Values);
            return ReferenceEquals(values, expression.Values) ? expression : new JsonExpression(values);
        }

        public virtual Expression Visit(MemberAccessExpression expression) {
            var target = Visit(expression.Target);
            return ReferenceEquals(target, expression.Target) ? expression : new MemberAccessExpression(target, expression.Member);
        }

        public virtual Expression Visit(NewExpression expression) {
            var constructor = Visit(expression.Constructor);
            var arguments   = Visit(expression.Arguments);
            return ReferenceEquals(constructor, expression.Constructor) && ReferenceEquals(arguments, expression.Arguments) ? expression : new NewExpression(constructor, arguments);
        }

        public virtual Expression Visit(UnaryExpression expression) {
            var operand = Visit(expression.Operand);
            return ReferenceEquals(operand, expression.Operand) ? expression : new UnaryExpression(expression.Operator, operand);
        }

        public virtual IEnumerable<Statement> Visit(ReadOnlyCollection<Statement> statements) {
            return VisitCollection(statements, Visit);
        }

        public virtual IEnumerable<SwitchStatement.Clause> Visit(ReadOnlyCollection<SwitchStatement.Clause> clauses) {
            return VisitCollection(clauses, Visit);
        }

        public virtual IEnumerable<VariableDeclarationStatement.VariableDeclaration> Visit(ReadOnlyCollection<VariableDeclarationStatement.VariableDeclaration> declarations) {
            return VisitCollection(declarations, Visit);
        }

        public virtual SwitchStatement.Clause Visit(SwitchStatement.Clause clause) {
            var value = clause.Value != null ? Visit(clause.Value) : null;
            var body  = Visit(clause.Body);
            return ReferenceEquals(value, clause.Value) && ReferenceEquals(body, clause.Body) ? clause : new SwitchStatement.Clause(value, body);
        }

        public virtual TryCatchFinallyStatement.CatchClause Visit(TryCatchFinallyStatement.CatchClause clause) {
            var body = Visit(clause.Body);
            return ReferenceEquals(body, clause.Body) ? clause : new TryCatchFinallyStatement.CatchClause(clause.Identifier, body);
        }

        public virtual VariableDeclarationStatement.VariableDeclaration Visit(VariableDeclarationStatement.VariableDeclaration declaration) {
            var after = Visit(declaration.Initializer);
            return ReferenceEquals(after, declaration.Initializer) ? declaration : new VariableDeclarationStatement.VariableDeclaration(declaration.Name, after);
        }

        public virtual Statement Visit(Statement statement) {
            return statement.Accept(this);
        }

        public virtual Statement Visit(BlockStatement statement) {
            var after = Visit(statement.Statements);
            return ReferenceEquals(after, statement.Statements) ? statement : new BlockStatement(after, statement.StatementLabel);
        }

        public virtual Statement Visit(BreakStatement statement) {
            return statement;
        }

        public virtual Statement Visit(ContinueStatement statement) {
            return statement;
        }

        public virtual Statement Visit(DoWhileStatement statement) {
            var condition = Visit(statement.Condition);
            var body      = Visit(statement.Body);
            return ReferenceEquals(condition, statement.Condition) && ReferenceEquals(body, statement.Body) ? statement : new DoWhileStatement(condition, body, statement.StatementLabel);
        }

        public virtual Statement Visit(EmptyStatement statement) {
            return statement;
        }

        public virtual Statement Visit(ExpressionStatement statement) {
            var after = Visit(statement.Expression);
            return ReferenceEquals(after, statement.Expression) ? statement : new ExpressionStatement(after, statement.StatementLabel);
        }

        public virtual Statement Visit(ForEachInStatement statement) {
            var objectToIterateOver = Visit(statement.ObjectToIterateOver);
            var body = Visit(statement.Body);
            return ReferenceEquals(objectToIterateOver, statement.ObjectToIterateOver) && ReferenceEquals(body, statement.Body) ? statement : new ForEachInStatement(statement.LoopVariableName, objectToIterateOver, body, statement.IsLoopVariableDeclared, statement.StatementLabel);
        }

        public virtual Statement Visit(ForStatement statement) {
            var initStatement = Visit(statement.InitStatement);
            var condition     = Visit(statement.ConditionExpression);
            var incr          = Visit(statement.IncrementExpression);
            var body          = Visit(statement.Body);
            return ReferenceEquals(initStatement, statement.InitStatement) && ReferenceEquals(condition, statement.ConditionExpression) && ReferenceEquals(incr, statement.IncrementExpression) && ReferenceEquals(body, statement.Body)
                 ? statement
                 : new ForStatement(initStatement, condition, incr, body, statement.StatementLabel);
        }

        public virtual Statement Visit(IfStatement statement) {
            var test  = Visit(statement.Test);
            var then  = Visit(statement.Then);
            var @else = statement.Else != null ? Visit(statement.Else) : null;
            return ReferenceEquals(test, statement.Test) && ReferenceEquals(then, statement.Then) && ReferenceEquals(@else, statement.Else) ? statement : new IfStatement(test, then, @else, statement.StatementLabel);
        }

        public virtual Statement Visit(ReturnStatement statement) {
            var value = (statement.Value != null ? Visit(statement.Value) : null);
            return ReferenceEquals(value, statement.Value) ? statement : new ReturnStatement(value, statement.StatementLabel);
        }

        public virtual Statement Visit(SwitchStatement statement) {
            var test = Visit(statement.Test);
            var clauses = Visit(statement.Clauses);
            return ReferenceEquals(test, statement.Test) && ReferenceEquals(clauses, statement.Clauses) ? statement : new SwitchStatement(test, clauses, statement.StatementLabel);
        }

        public virtual Statement Visit(ThrowStatement statement) {
            var expr = Visit(statement.Expression);
            return ReferenceEquals(expr, statement.Expression) ? statement : new ThrowStatement(expr, statement.StatementLabel);
        }

        public virtual Statement Visit(TryCatchFinallyStatement statement) {
            var guarded  = Visit(statement.GuardedStatement);
            var @catch   = statement.Catch != null ? Visit(statement.Catch) : null;
            var @finally = statement.Finally != null ? Visit(statement.Finally) : null;
            return ReferenceEquals(guarded, statement.GuardedStatement) && ReferenceEquals(@catch, statement.Catch) && ReferenceEquals(@finally, statement.Finally) ? statement : new TryCatchFinallyStatement(guarded, @catch, @finally, statement.StatementLabel);
        }

        public virtual Statement Visit(VariableDeclarationStatement statement) {
            var declarations = Visit(statement.Declarations);
            return ReferenceEquals(declarations, statement.Declarations) ? statement : new VariableDeclarationStatement(declarations, statement.StatementLabel);
        }

        public virtual Statement Visit(WhileStatement statement) {
            var condition = Visit(statement.Condition);
            var body      = Visit(statement.Body);
            return ReferenceEquals(condition, statement.Condition) && ReferenceEquals(body, statement.Body) ? statement : new WhileStatement(condition, body, statement.StatementLabel);
        }

        public virtual Statement Visit(WithStatement statement) {
            var @object = Visit(statement.Object);
            var body    = Visit(statement.Body);
            return ReferenceEquals(@object, statement.Object) && ReferenceEquals(body, statement.Body) ? statement : new WithStatement(@object, body, statement.StatementLabel);
        }
    }
}
