using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler {
	public class StatementCompiler : DepthFirstAstVisitor {
		private readonly INamingConventionResolver _namingConvention;
		private readonly IErrorReporter _errorReporter;
		private readonly ICompilation _compilation;
		private readonly CSharpAstResolver _resolver;
		private readonly IDictionary<IVariable, VariableData> _variables;
		private readonly IDictionary<LambdaResolveResult, NestedFunctionData> _nestedFunctions;

		private List<JsStatement> _result;

		public StatementCompiler(INamingConventionResolver namingConvention, IErrorReporter errorReporter, ICompilation compilation, CSharpAstResolver resolver, IDictionary<IVariable, VariableData> variables, List<NestedFunctionData> nestedFunctions) {
			_namingConvention = namingConvention;
			_errorReporter = errorReporter;
			_compilation = compilation;
			_resolver = resolver;
			_variables = variables;
			_nestedFunctions = nestedFunctions.SelectMany(f => f.SelfAndDirectlyOrIndirectlyNestedFunctions).ToDictionary(f => f.ResolveResult);
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
	}
}
