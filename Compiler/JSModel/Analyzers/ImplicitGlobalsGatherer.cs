using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Utils;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.Analyzers {
	public class ImplicitGlobalsGatherer : RewriterVisitorBase<Tuple<ImmutableStack<JsDeclarationScope>, HashSet<string>>> {
		private readonly Dictionary<JsDeclarationScope, HashSet<string>> _result = new Dictionary<JsDeclarationScope, HashSet<string>>();
		private readonly Dictionary<JsDeclarationScope, HashSet<string>> _locals;
		private readonly bool _reportGlobalsAsUsedInAllParentScopes;

		private ImplicitGlobalsGatherer(Dictionary<JsDeclarationScope, HashSet<string>> locals, bool reportGlobalsAsUsedInAllParentScopes) {
			_locals = locals;
			_reportGlobalsAsUsedInAllParentScopes = reportGlobalsAsUsedInAllParentScopes;
		}

		private static HashSet<string> Union(IEnumerable<string> prev, IEnumerable<string> current) {
			var result = prev != null ? new HashSet<string>(prev) : new HashSet<string>();
			result.UnionWith(current);
			return result;
		}

		private void MaybeAddGlobal(string name, HashSet<string> locals, ImmutableStack<JsDeclarationScope> functionStack) {
			if (!locals.Contains(name)) {
				if (_reportGlobalsAsUsedInAllParentScopes) {
					foreach (var f in functionStack)
						_result[f].Add(name);
				}
				else {
					if (!functionStack.IsEmpty)
						_result[functionStack.Peek()].Add(name);
				}
			}
		}

		public override JsExpression VisitIdentifierExpression(JsIdentifierExpression expression, Tuple<ImmutableStack<JsDeclarationScope>, HashSet<string>> data) {
			MaybeAddGlobal(expression.Name, data.Item2, data.Item1);
			return expression;
		}

		public override JsStatement VisitForEachInStatement(JsForEachInStatement statement, Tuple<ImmutableStack<JsDeclarationScope>, HashSet<string>> data) {
			if (!statement.IsLoopVariableDeclared)
				MaybeAddGlobal(statement.LoopVariableName, data.Item2, data.Item1);
			return base.VisitForEachInStatement(statement, data);
		}

		public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, Tuple<ImmutableStack<JsDeclarationScope>, HashSet<string>> data) {
			_result[expression] = new HashSet<string>();
			return base.VisitFunctionDefinitionExpression(expression, Tuple.Create(data.Item1.Push(expression), Union(data.Item2, _locals[expression])));
		}

		public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, Tuple<ImmutableStack<JsDeclarationScope>, HashSet<string>> data) {
			_result[statement] = new HashSet<string>();
			MaybeAddGlobal(statement.Name, data.Item2, data.Item1);
			return base.VisitFunctionStatement(statement, Tuple.Create(data.Item1.Push(statement), Union(data.Item2, _locals[statement])));
		}

		public override JsCatchClause VisitCatchClause(JsCatchClause clause, Tuple<ImmutableStack<JsDeclarationScope>, HashSet<string>> data) {
			_result[clause] = new HashSet<string>();
			return base.VisitCatchClause(clause, Tuple.Create(data.Item1.Push(clause), Union(data.Item2, new[] { clause.Identifier })));
		}

		public static Dictionary<JsDeclarationScope, HashSet<string>> Analyze(JsStatement statement, Dictionary<JsDeclarationScope, HashSet<string>> locals, bool reportGlobalsAsUsedInAllParentScopes) {
			return Analyze(new[] { statement }, locals, reportGlobalsAsUsedInAllParentScopes);
		}

		public static Dictionary<JsDeclarationScope, HashSet<string>> Analyze(IEnumerable<JsStatement> statements, Dictionary<JsDeclarationScope, HashSet<string>> locals, bool reportGlobalsAsUsedInAllParentScopes) {
			var obj = new ImplicitGlobalsGatherer(locals, reportGlobalsAsUsedInAllParentScopes);
			obj._result[JsDeclarationScope.Root] = new HashSet<string>();
			var rootData = Tuple.Create(ImmutableStack<JsDeclarationScope>.Empty.Push(JsDeclarationScope.Root), Union(null, locals[JsDeclarationScope.Root]));
			foreach (var statement in statements)
				obj.VisitStatement(statement, rootData);
			return obj._result;
		}
	}
}