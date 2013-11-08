using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	internal class VariableHoistingVisitor : RewriterVisitorBase<object> {
		private List<string> _variables = new List<string>();

		private VariableHoistingVisitor() {
		}

		public override JsStatement VisitVariableDeclarationStatement(JsVariableDeclarationStatement statement, object data) {
			List<JsExpression> list = null;
			foreach (var d in statement.Declarations) {
				_variables.Add(d.Name);
				if (d.Initializer != null) {
					list = list ?? new List<JsExpression>();
					list.Add(JsExpression.Assign(JsExpression.Identifier(d.Name), d.Initializer));
				}
			}
			if (list == null)
				return JsStatement.BlockMerged(new JsStatement[0]);
			else if (list.Count == 1)
				return list[0];
			else
				return JsExpression.Comma(list);
		}

		public override JsStatement VisitForStatement(JsForStatement statement, object data) {
			var initStatement = statement.InitStatement       != null ? VisitStatement(statement.InitStatement, data)        : null;
			var condition     = statement.ConditionExpression != null ? VisitExpression(statement.ConditionExpression, data) : null;
			var iterator      = statement.IteratorExpression  != null ? VisitExpression(statement.IteratorExpression, data)  : null;
			var body          = VisitStatement(statement.Body, data);

			if (initStatement is JsBlockStatement) {	// Will happen if the init statement is a variable declaration without initializers.
				Debug.Assert(((JsBlockStatement)initStatement).Statements.Count == 0);
				initStatement = JsStatement.Empty;
			}

			return ReferenceEquals(initStatement, statement.InitStatement) && ReferenceEquals(condition, statement.ConditionExpression) && ReferenceEquals(iterator, statement.IteratorExpression) && ReferenceEquals(body, statement.Body)
			     ? statement
			     : JsStatement.For(initStatement, condition, iterator, body);
		}

		public override JsStatement VisitForEachInStatement(JsForEachInStatement statement, object data) {
			var objectToIterateOver = VisitExpression(statement.ObjectToIterateOver, data);
			var body = VisitStatement(statement.Body, data);
			if (statement.IsLoopVariableDeclared) {
				_variables.Add(statement.LoopVariableName);
				return JsStatement.ForIn(statement.LoopVariableName, objectToIterateOver, body, false);
			}
			else {
				return ReferenceEquals(objectToIterateOver, statement.ObjectToIterateOver) && ReferenceEquals(body, statement.Body) ? statement : JsStatement.ForIn(statement.LoopVariableName, objectToIterateOver, body, false);
			}
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
			return expression;
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
			return statement;
		}

		public static Tuple<JsBlockStatement, List<string>> Process(JsBlockStatement statement) {
			var obj = new VariableHoistingVisitor();
			var result = (JsBlockStatement)obj.VisitStatement(statement, null);
			return Tuple.Create(result, obj._variables);
		}
	}
}
