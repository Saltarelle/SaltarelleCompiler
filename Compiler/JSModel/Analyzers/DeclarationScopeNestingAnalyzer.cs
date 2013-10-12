using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.Analyzers {
	public class DeclarationScopeNestingAnalyzer : RewriterVisitorBase<Tuple<JsDeclarationScope, DeclarationScopeHierarchy>> {
		private readonly Dictionary<JsDeclarationScope, DeclarationScopeHierarchy> _result = new Dictionary<JsDeclarationScope, DeclarationScopeHierarchy>();
		
		private DeclarationScopeNestingAnalyzer() {
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, Tuple<JsDeclarationScope, DeclarationScopeHierarchy> data) {
			data.Item2.ChildScopes.Add(expression);
			var hier = new DeclarationScopeHierarchy(data.Item1);
			_result[expression] = hier;
			return base.VisitFunctionDefinitionExpression(expression, Tuple.Create((JsDeclarationScope)expression, hier));
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, Tuple<JsDeclarationScope, DeclarationScopeHierarchy> data) {
			data.Item2.ChildScopes.Add(statement);
			var hier = new DeclarationScopeHierarchy(data.Item1);
			_result[statement] = hier;
			return base.VisitFunctionStatement(statement, Tuple.Create((JsDeclarationScope)statement, hier));
		}

		public override JsCatchClause VisitCatchClause(JsCatchClause clause, Tuple<JsDeclarationScope, DeclarationScopeHierarchy> data) {
			data.Item2.ChildScopes.Add(clause);
			var hier = new DeclarationScopeHierarchy(data.Item1);
			_result[clause] = hier;
			return base.VisitCatchClause(clause, Tuple.Create((JsDeclarationScope)clause, hier));
		}

		public static IDictionary<JsDeclarationScope, DeclarationScopeHierarchy> Analyze(IEnumerable<JsStatement> statements) {
			var obj = new DeclarationScopeNestingAnalyzer();
			var hier = new DeclarationScopeHierarchy(null);
			obj._result[JsDeclarationScope.Root] = hier;
			foreach (var s in statements)
				obj.VisitStatement(s, Tuple.Create(JsDeclarationScope.Root, hier));
			return obj._result;
		}
	}
}
