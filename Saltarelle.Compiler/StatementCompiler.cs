using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler {
	public class StatementCompiler : DepthFirstAstVisitor {
		private readonly INamingConventionResolver _namingConvention;
		private readonly IErrorReporter _errorReporter;
		private readonly ICompilation _compilation;
		private readonly CSharpAstResolver _resolver;
		private readonly IDictionary<IVariable, VariableData> _variables;
		private readonly IDictionary<LambdaResolveResult, NestedFunctionData> _nestedFunctions;
		private readonly ExpressionCompiler _expressionCompiler;

		private List<JsStatement> _result;

		public StatementCompiler(INamingConventionResolver namingConvention, IErrorReporter errorReporter, ICompilation compilation, CSharpAstResolver resolver, IDictionary<IVariable, VariableData> variables, IDictionary<LambdaResolveResult, NestedFunctionData> nestedFunctions) {
			_namingConvention   = namingConvention;
			_errorReporter      = errorReporter;
			_compilation        = compilation;
			_resolver           = resolver;
			_variables          = variables;
			_nestedFunctions    = nestedFunctions;
			_expressionCompiler = new ExpressionCompiler(_namingConvention, _variables);
		}

		public StatementCompiler Clone() {
			return new StatementCompiler(_namingConvention, _errorReporter, _compilation, _resolver, _variables, _nestedFunctions);
		}

		public JsBlockStatement Compile(Statement statement) {
			_result = new List<JsStatement>();
			statement.AcceptVisitor(this);
			return new JsBlockStatement(_result);
		}

		public override void VisitComment(Comment comment) {
			switch (comment.CommentType) {
				case CommentType.SingleLine: {
					_result.Add(new JsComment(comment.Content));
					break;
				}

				case CommentType.MultiLine: {
					string prefix = new Regex(@"^\s*").Match(comment.Content).Captures[0].Value;
					List<string> commentLines = comment.Content.Replace("\r", "").Split('\n').Select(item => item.Trim()).SkipWhile(l => l == "").ToList();
					while (commentLines.Count > 0 && commentLines[commentLines.Count - 1] == "")
						commentLines.RemoveAt(commentLines.Count - 1);

					if (commentLines.Count > 0)
						_result.Add(new JsComment(string.Join(Environment.NewLine, commentLines.Select(item => prefix + item))));	// Replace the space at the start of each line with the same as the space in the first line.
					break;
				}
					
				case CommentType.Documentation:
				case CommentType.MultiLineDocumentation:
					// Better to use the NRefactory XML support if we want these.
					break;
				case CommentType.InactiveCode:
					// Should not appear in script.
					break;
				default:
					throw new ArgumentException("Invalid comment type " + comment.CommentType);
			}
		}

		public override void VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement) {
			var declarations = new List<JsVariableDeclaration>();
			foreach (var d in variableDeclarationStatement.Variables) {
				var data = _variables[((LocalResolveResult)_resolver.Resolve(d)).Variable];
				JsExpression initializer;
				if (!d.Initializer.IsNull) {
					var exprCompileResult = _expressionCompiler.Compile(_resolver.Resolve(d.Initializer));
					if (exprCompileResult.AdditionalStatements.Count > 0) {
						if (declarations.Count > 0) {
							_result.Add(new JsVariableDeclarationStatement(declarations));
							declarations = new List<JsVariableDeclaration>();
						}
						foreach (var s in exprCompileResult.AdditionalStatements) {
							_result.Add(s);
						}
					}
					initializer = (data.UseByRefSemantics ? JsExpression.ObjectLiteral(new[] { new JsObjectLiteralProperty("$", exprCompileResult.Expression) }) : exprCompileResult.Expression);
				}
				else {
					if (data.UseByRefSemantics)
						initializer = JsExpression.ObjectLiteral(new[] { new JsObjectLiteralProperty("$", JsExpression.Null) });
					else
						initializer = null;
				}
				declarations.Add(new JsVariableDeclaration(data.Name, initializer));
			}

			if (declarations.Count > 0)
				_result.Add(new JsVariableDeclarationStatement(declarations));

			base.VisitVariableDeclarationStatement(variableDeclarationStatement);
		}

		public override void VisitExpressionStatement(ExpressionStatement expressionStatement) {
			#warning Not finished
			var compiled = _expressionCompiler.Compile(_resolver.Resolve(expressionStatement.Expression));
			_result.AddRange(compiled.AdditionalStatements);
			_result.Add(new JsExpressionStatement(compiled.Expression));
		}

		public override void VisitForStatement(ForStatement forStatement) {
			var oldResult = _result;
			try {
				// Initializer
				JsStatement initializer;
				if (forStatement.Initializers.Count == 1 && forStatement.Initializers.First() is VariableDeclarationStatement) {
					forStatement.Initializers.First().AcceptVisitor(this);
					initializer = _result[_result.Count - 1];
					_result.RemoveAt(_result.Count - 1);
				}
				else {
					JsExpression initExpr = null;
					foreach (var init in forStatement.Initializers) {
						var compiledInit = _expressionCompiler.Compile(_resolver.Resolve(((ExpressionStatement)init).Expression));
						initExpr = (initExpr != null ? JsExpression.Comma(initExpr, compiledInit.Expression) : compiledInit.Expression);
					}
					initializer = (initExpr != null ? (JsStatement)new JsExpressionStatement(initExpr) : (JsStatement)new JsEmptyStatement());
				}

				// Condition
				JsExpression condition;
				if (!forStatement.Condition.IsNull) {
					condition = _expressionCompiler.Compile(_resolver.Resolve(forStatement.Condition)).Expression;
				}
				else {
					condition = null;
				}

				// Iterators
				JsExpression iterator = null;
				foreach (var iter in forStatement.Iterators) {
					var compiledIter = _expressionCompiler.Compile(_resolver.Resolve(((ExpressionStatement)iter).Expression));
					iterator = (iterator != null ? JsExpression.Comma(iterator, compiledIter.Expression) : compiledIter.Expression);
				}

				// Body
				var body = Clone().Compile(forStatement.EmbeddedStatement);

				oldResult.Add(new JsForStatement(initializer, condition, iterator, body));
			}
			finally {
				_result = oldResult;
			}
		}
	}
}
