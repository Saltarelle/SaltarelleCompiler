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
		protected static IList<T> VisitCollection<T>(IList<T> orig, Func<T, int, T> visitor) {
            List<T> list = null;
            for (int i = 0; i < orig.Count; i++) {
                var before = orig[i];
                var after  = visitor(before, i);
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
            return list ?? orig;
		}

        protected static IList<T> VisitCollection<T>(IList<T> orig, Func<T, T> visitor) {
			return VisitCollection(orig, (x, _) => visitor(x));
        }

        protected static IList<T> VisitCollection<T>(IList<T> orig, Func<T, int, IList<T>> visitor) {
            List<T> list = null;
            for (int i = 0; i < orig.Count; i++) {
                var before = orig[i];
				IList<T> after = visitor(before, i);

                if (list != null) {
                    list.AddRange(after);
                }
                else if (after.Count != 1 || !ReferenceEquals(before, after[0])) {
                    list = new List<T>();
                    for (int j = 0; j < i; j++)
                        list.Add(orig[j]);
                    list.AddRange(after);
                }
            }
            return list ?? orig;		}

        protected static IList<T> VisitCollection<T>(IList<T> orig, Func<T, IList<T>> visitor) {
			return VisitCollection(orig, (x, _) => visitor(x));
        }

        public virtual IList<JsExpression> VisitExpressions(IList<JsExpression> expressions, TData data) {
            return VisitCollection(expressions, expr => expr != null ? VisitExpression(expr, data) : null);
        }

        public virtual IList<JsObjectLiteralProperty> VisitObjectLiteralProperties(IList<JsObjectLiteralProperty> values, TData data) {
            return VisitCollection(values, v => VisitObjectLiteralProperty(v, data));
        }

        public virtual JsObjectLiteralProperty VisitObjectLiteralProperty(JsObjectLiteralProperty value, TData data) {
            var after = VisitExpression(value.Value, data);
            return ReferenceEquals(after, value.Value) ? value : new JsObjectLiteralProperty(value.Name, after);
        }

        public virtual JsExpression VisitExpression(JsExpression expression, TData data) {
            return expression.Accept(this, data);
        }

        public virtual JsExpression VisitArrayLiteralExpression(JsArrayLiteralExpression expression, TData data) {
            var after  = VisitExpressions(expression.Elements, data);
            return ReferenceEquals(after, expression.Elements) ? expression : JsExpression.ArrayLiteral(after);
        }

        public virtual JsExpression VisitBinaryExpression(JsBinaryExpression expression, TData data) {
            var left  = VisitExpression(expression.Left, data);
            var right = VisitExpression(expression.Right, data);
            return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right) ? expression : JsExpression.Binary(expression.NodeType, left, right);
        }

        public virtual JsExpression VisitCommaExpression(JsCommaExpression expression, TData data) {
            var after = VisitExpressions(expression.Expressions, data);
            return ReferenceEquals(after, expression.Expressions) ? expression : JsExpression.Comma(after);
        }

        public virtual JsExpression VisitConditionalExpression(JsConditionalExpression expression, TData data) {
            var test      = VisitExpression(expression.Test, data);
            var truePart  = VisitExpression(expression.TruePart, data);
            var falsePart = VisitExpression(expression.FalsePart, data);
            return ReferenceEquals(test, expression.Test) && ReferenceEquals(truePart, expression.TruePart) && ReferenceEquals(falsePart, expression.FalsePart) ? expression : JsExpression.Conditional(test, truePart, falsePart);
        }

        public virtual JsExpression VisitConstantExpression(JsConstantExpression expression, TData data) {
            return expression;
        }

        public virtual JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, TData data) {
            var body = VisitStatement(expression.Body, data);
            return ReferenceEquals(body, expression.Body) ? expression : JsExpression.FunctionDefinition(expression.ParameterNames, body, expression.Name);
        }

        public virtual JsExpression VisitIdentifierExpression(JsIdentifierExpression expression, TData data) {
            return expression;
        }

        public virtual JsExpression VisitInvocationExpression(JsInvocationExpression expression, TData data) {
            var method    = VisitExpression(expression.Method, data);
            var arguments = VisitExpressions(expression.Arguments, data);
            return ReferenceEquals(method, expression.Method) && ReferenceEquals(arguments, expression.Arguments) ? expression : JsExpression.Invocation(method, arguments);
        }

        public virtual JsExpression VisitObjectLiteralExpression(JsObjectLiteralExpression expression, TData data) {
            var values = VisitObjectLiteralProperties(expression.Values, data);
            return ReferenceEquals(values, expression.Values) ? expression : JsExpression.ObjectLiteral(values);
        }

        public virtual JsExpression VisitMemberAccessExpression(JsMemberAccessExpression expression, TData data) {
            var target = VisitExpression(expression.Target, data);
            return ReferenceEquals(target, expression.Target) ? expression : JsExpression.Member(target, expression.MemberName);
        }

        public virtual JsExpression VisitNewExpression(JsNewExpression expression, TData data) {
            var constructor = VisitExpression(expression.Constructor, data);
            var arguments   = VisitExpressions(expression.Arguments, data);
            return ReferenceEquals(constructor, expression.Constructor) && ReferenceEquals(arguments, expression.Arguments) ? expression : JsExpression.New(constructor, arguments);
        }

        public virtual JsExpression VisitUnaryExpression(JsUnaryExpression expression, TData data) {
            var operand = VisitExpression(expression.Operand, data);
            return ReferenceEquals(operand, expression.Operand) ? expression : JsExpression.Unary(expression.NodeType, operand);
        }

        public virtual JsExpression VisitTypeReferenceExpression(JsTypeReferenceExpression expression, TData data) {
            return expression;
        }

        public virtual JsExpression VisitThisExpression(JsThisExpression expression, TData data) {
            return expression;
        }

        public virtual IList<JsStatement> VisitStatements(IList<JsStatement> statements, TData data) {
            return VisitCollection(statements, s => {
				var after = VisitStatement(s, data);
				var afterBlock = after as JsBlockStatement;
				if (afterBlock != null && afterBlock.MergeWithParent)
					return afterBlock.Statements;
				else
					return new[] { after };
			});
        }

        public virtual IList<JsSwitchSection> VisitSwitchSections(IList<JsSwitchSection> clauses, TData data) {
            return VisitCollection(clauses, c => VisitSwitchSection(c, data));
        }

        public virtual IList<JsVariableDeclaration> VisitVariableDeclarations(IList<JsVariableDeclaration> declarations, TData data) {
            return VisitCollection(declarations, d => VisitVariableDeclaration(d, data));
        }

        public virtual JsSwitchSection VisitSwitchSection(JsSwitchSection clause, TData data) {
            var values = VisitCollection(clause.Values, x => x != null ? x.Accept(this, data) : null);
            var body  = VisitStatement(clause.Body, data);
            return ReferenceEquals(values, clause.Values) && ReferenceEquals(body, clause.Body) ? clause : new JsSwitchSection(values, body);
        }

        public virtual JsCatchClause VisitCatchClause(JsCatchClause clause, TData data) {
            var body = VisitStatement(clause.Body, data);
            return ReferenceEquals(body, clause.Body) ? clause : new JsCatchClause(clause.Identifier, body);
        }

        public virtual JsVariableDeclaration VisitVariableDeclaration(JsVariableDeclaration declaration, TData data) {
            var after = (declaration.Initializer != null ? VisitExpression(declaration.Initializer, data) : null);
            return ReferenceEquals(after, declaration.Initializer) ? declaration : new JsVariableDeclaration(declaration.Name, after);
        }

        public virtual JsStatement VisitStatement(JsStatement statement, TData data) {
            return statement.Accept(this, data);
        }

        public virtual JsStatement VisitComment(JsComment comment, TData data) {
            return comment;
        }

        public virtual JsStatement VisitBlockStatement(JsBlockStatement statement, TData data) {
            var after = VisitStatements(statement.Statements, data);
            return ReferenceEquals(after, statement.Statements) ? statement : new JsBlockStatement(after, statement.MergeWithParent);
        }

        public virtual JsStatement VisitBreakStatement(JsBreakStatement statement, TData data) {
            return statement;
        }

        public virtual JsStatement VisitContinueStatement(JsContinueStatement statement, TData data) {
            return statement;
        }

        public virtual JsStatement VisitDoWhileStatement(JsDoWhileStatement statement, TData data) {
            var condition = VisitExpression(statement.Condition, data);
            var body      = VisitStatement(statement.Body, data);
            return ReferenceEquals(condition, statement.Condition) && ReferenceEquals(body, statement.Body) ? statement : new JsDoWhileStatement(condition, body);
        }

        public virtual JsStatement VisitEmptyStatement(JsEmptyStatement statement, TData data) {
            return statement;
        }

        public virtual JsStatement VisitExpressionStatement(JsExpressionStatement statement, TData data) {
            var after = VisitExpression(statement.Expression, data);
            return ReferenceEquals(after, statement.Expression) ? statement : new JsExpressionStatement(after);
        }

        public virtual JsStatement VisitForEachInStatement(JsForEachInStatement statement, TData data) {
            var objectToIterateOver = VisitExpression(statement.ObjectToIterateOver, data);
            var body = VisitStatement(statement.Body, data);
            return ReferenceEquals(objectToIterateOver, statement.ObjectToIterateOver) && ReferenceEquals(body, statement.Body) ? statement : new JsForEachInStatement(statement.LoopVariableName, objectToIterateOver, body, statement.IsLoopVariableDeclared);
        }

        public virtual JsStatement VisitForStatement(JsForStatement statement, TData data) {
            var initStatement = statement.InitStatement       != null ? VisitStatement(statement.InitStatement, data)        : null;
            var condition     = statement.ConditionExpression != null ? VisitExpression(statement.ConditionExpression, data) : null;
            var iterator      = statement.IteratorExpression  != null ? VisitExpression(statement.IteratorExpression, data)  : null;
            var body          = VisitStatement(statement.Body, data);
            return ReferenceEquals(initStatement, statement.InitStatement) && ReferenceEquals(condition, statement.ConditionExpression) && ReferenceEquals(iterator, statement.IteratorExpression) && ReferenceEquals(body, statement.Body)
                 ? statement
                 : new JsForStatement(initStatement, condition, iterator, body);
        }

        public virtual JsStatement VisitIfStatement(JsIfStatement statement, TData data) {
            var test  = VisitExpression(statement.Test, data);
            var then  = VisitStatement(statement.Then, data);
            var @else = statement.Else != null ? VisitStatement(statement.Else, data) : null;
            return ReferenceEquals(test, statement.Test) && ReferenceEquals(then, statement.Then) && ReferenceEquals(@else, statement.Else) ? statement : new JsIfStatement(test, then, @else);
        }

        public virtual JsStatement VisitReturnStatement(JsReturnStatement statement, TData data) {
            var value = (statement.Value != null ? VisitExpression(statement.Value, data) : null);
            return ReferenceEquals(value, statement.Value) ? statement : new JsReturnStatement(value);
        }

        public virtual JsStatement VisitSwitchStatement(JsSwitchStatement statement, TData data) {
            var test = VisitExpression(statement.Expression, data);
            var clauses = VisitSwitchSections(statement.Sections, data);
            return ReferenceEquals(test, statement.Expression) && ReferenceEquals(clauses, statement.Sections) ? statement : new JsSwitchStatement(test, clauses);
        }

        public virtual JsStatement VisitThrowStatement(JsThrowStatement statement, TData data) {
            var expr = VisitExpression(statement.Expression, data);
            return ReferenceEquals(expr, statement.Expression) ? statement : new JsThrowStatement(expr);
        }

        public virtual JsStatement VisitTryStatement(JsTryStatement statement, TData data) {
            var guarded  = VisitStatement(statement.GuardedStatement, data);
            var @catch   = statement.Catch != null ? VisitCatchClause(statement.Catch, data) : null;
            var @finally = statement.Finally != null ? VisitStatement(statement.Finally, data) : null;
            return ReferenceEquals(guarded, statement.GuardedStatement) && ReferenceEquals(@catch, statement.Catch) && ReferenceEquals(@finally, statement.Finally) ? statement : new JsTryStatement(guarded, @catch, @finally);
        }

        public virtual JsStatement VisitVariableDeclarationStatement(JsVariableDeclarationStatement statement, TData data) {
            var declarations = VisitVariableDeclarations(statement.Declarations, data);
            return ReferenceEquals(declarations, statement.Declarations) ? statement : new JsVariableDeclarationStatement(declarations);
        }

        public virtual JsStatement VisitWhileStatement(JsWhileStatement statement, TData data) {
            var condition = VisitExpression(statement.Condition, data);
            var body      = VisitStatement(statement.Body, data);
            return ReferenceEquals(condition, statement.Condition) && ReferenceEquals(body, statement.Body) ? statement : new JsWhileStatement(condition, body);
        }

        public virtual JsStatement VisitWithStatement(JsWithStatement statement, TData data) {
            var @object = VisitExpression(statement.Object, data);
            var body    = VisitStatement(statement.Body, data);
            return ReferenceEquals(@object, statement.Object) && ReferenceEquals(body, statement.Body) ? statement : new JsWithStatement(@object, body);
        }

    	public virtual JsStatement VisitLabelledStatement(JsLabelledStatement statement, TData data) {
			var stmt = VisitStatement(statement.Statement, data);
    		return ReferenceEquals(stmt, statement.Statement) ? statement : new JsLabelledStatement(statement.Label, stmt);
    	}

		public virtual JsStatement VisitFunctionStatement(JsFunctionStatement statement, TData data) {
			var body = VisitStatement(statement.Body, data);
			return ReferenceEquals(body, statement.Body) ? statement : new JsFunctionStatement(statement.Name, statement.ParameterNames, body);
		}

    	public virtual JsStatement VisitGotoStatement(JsGotoStatement statement, TData data) {
    		return statement;
    	}

    	public virtual JsStatement VisitYieldStatement(JsYieldStatement statement, TData data) {
    		var value = statement.Value != null ? VisitExpression(statement.Value, data) : null;
			return ReferenceEquals(value, statement.Value) ? statement : new JsYieldStatement(value);
    	}

    	public virtual JsStatement VisitAwaitStatement(JsAwaitStatement statement, TData data) {
    		var awaiter = VisitExpression(statement.Awaiter, data);
			return ReferenceEquals(awaiter, statement.Awaiter) ? statement : new JsAwaitStatement(awaiter, statement.OnCompletedMethodName);
    	}
    }
}
