using System.Collections.Generic;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Compiler {
	internal class ExpressionOrderer {
		private class ObservableState {
			public HashSet<string> LocalReadSet { get; private set; }
			public HashSet<string> LocalWriteSet { get; private set; }
			public bool UsesExternalState { get; set; }

			public ObservableState() {
				LocalReadSet = new HashSet<string>();
				LocalWriteSet = new HashSet<string>();
			}
		}

		private class FindObservableStateVisitor : RewriterVisitorBase<object> {
			public ObservableState Result { get; set; }

			public FindObservableStateVisitor() {
				Result = new ObservableState();
			}

			public override JsExpression VisitBinaryExpression(JsBinaryExpression expression, object data) {
				if (expression.NodeType >= ExpressionNodeType.AssignFirst && expression.NodeType <= ExpressionNodeType.AssignLast) {
					if (expression.Left is JsIdentifierExpression) {
						string name = ((JsIdentifierExpression)expression.Left).Name;
						if (expression.NodeType != ExpressionNodeType.Assign)
							Result.LocalReadSet.Add(name);	// Compound assignment means that we both read and write the state.

						Result.LocalWriteSet.Add(name);
						base.VisitExpression(expression.Right, data);
						return expression;
					}
					else {
						Result.UsesExternalState = true;	// Assigning to something that is not a local. This counts as external state.
					}
				}

				return base.VisitBinaryExpression(expression, data);
			}

			public override JsExpression VisitIdentifierExpression(JsIdentifierExpression expression, object data) {
				Result.LocalReadSet.Add(expression.Name);	// It has to be a read since we don't call Visit() for the target of an assignment.
				return base.VisitIdentifierExpression(expression, data);
			}

			public override JsExpression VisitInvocationExpression(JsInvocationExpression expression, object data) {
				Result.UsesExternalState = true;	// Invoking anything counts as external state.
				return base.VisitInvocationExpression(expression, data);
			}

			public override JsExpression VisitMemberAccessExpression(JsMemberAccessExpression expression, object data) {
				Result.UsesExternalState = true;	// Member access has to count as external state. Otherwise, what if someone does "var a = this" and then tries to order "a.i" and "this.i" (aliasing)
				return base.VisitMemberAccessExpression(expression, data);
			}

			public override JsExpression VisitNewExpression(JsNewExpression expression, object data) {
				Result.UsesExternalState = true;	// Constructor invocation is external state.
				return base.VisitNewExpression(expression, data);
			}

			public override JsExpression VisitUnaryExpression(JsUnaryExpression expression, object data) {
				switch (expression.NodeType) {
					case ExpressionNodeType.PrefixPlusPlus:
					case ExpressionNodeType.PostfixPlusPlus:
					case ExpressionNodeType.PrefixMinusMinus:
					case ExpressionNodeType.PostfixMinusMinus:
						if (expression.Operand is JsIdentifierExpression) {
							// Increment/decrement both reads and writes.
							string name = ((JsIdentifierExpression)expression.Operand).Name;
							Result.LocalReadSet.Add(name);
							Result.LocalWriteSet.Add(name);
							return expression;
						}
						else {
							Result.UsesExternalState = true;
							return base.VisitExpression(expression.Operand, data);	// Increments/decrements something that is not a local. This is external state.
						}

					case ExpressionNodeType.Delete: {
						Result.UsesExternalState = true;	// Delete counts as external state.
						return base.VisitExpression(expression.Operand, data);
					}

					default:
						return base.VisitUnaryExpression(expression, data);
				}
			}
		}

		/// <summary>
		/// Determine whether it matters if an expression is run before or after another sequence of statements/expressions
		/// </summary>
		public static bool DoesOrderMatter(JsExpression expr1, ExpressionCompiler.Result expr2) {
			// The algorithm is rather simple and conservative: For the both expression (sequences), determine a) which locals are read, b) which locals are written, and c) whether can possibly read or write any external state.
			// The order of the two expressions then matters if and only if:
			// 1) Either expression writes a local that the other uses,
			// 2) The expressions write to the same locals, or
			// 3) Both the expressions use any external state.
			var v1 = new FindObservableStateVisitor();
			v1.VisitExpression(expr1, null);
			var v2 = new FindObservableStateVisitor();
			foreach (var s in expr2.AdditionalStatements)
				v2.VisitStatement(s, null);
			v2.VisitExpression(expr2.Expression, null);

			if (v1.Result.LocalReadSet.Overlaps(v2.Result.LocalWriteSet) || v1.Result.LocalWriteSet.Overlaps(v2.Result.LocalReadSet))
				return true;
			if (v1.Result.LocalWriteSet.Overlaps(v2.Result.LocalWriteSet))
				return true;
			if (v1.Result.UsesExternalState && v2.Result.UsesExternalState)
				return true;

			return false;
		}

		/// <summary>
		/// Determine whether it matters if an expression is run before or after another expression
		/// </summary>
		public static bool DoesOrderMatter(JsExpression expr1, JsExpression expr2) {
			return DoesOrderMatter(expr1, new ExpressionCompiler.Result(expr2, new JsStatement[0]));
		}
	}
}
