using System;
using System.Collections.Generic;
using System.Linq;
using Saltarelle.Compiler.JSModel.Analyzers;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.Minification {
	public static class Minifier {
		internal class IdentifierRenameMapBuilder : RewriterVisitorBase<Tuple<Dictionary<string, string>, HashSet<string>>> {
			private readonly Dictionary<JsDeclarationScope, HashSet<string>> _locals;
			private readonly Dictionary<JsDeclarationScope, HashSet<string>> _globals;
			private readonly Func<string, HashSet<string>, string> _generateName;
			private readonly Dictionary<JsDeclarationScope, IDictionary<string, string>> _result;

			private IdentifierRenameMapBuilder(Dictionary<JsDeclarationScope, HashSet<string>> locals, Dictionary<JsDeclarationScope, HashSet<string>> globals, Func<string, HashSet<string>, string> generateName) {
				_locals = locals;
				_globals = globals;
				_generateName = generateName;
				_result = new Dictionary<JsDeclarationScope, IDictionary<string, string>>();
			}

			private Tuple<Dictionary<string, string>, HashSet<string>> BuildMap(Dictionary<string, string> prev, JsDeclarationScope declarationScope) {
				var newLocals = _locals[declarationScope];
				var newGlobals = _globals[declarationScope];

				if (newLocals.Count == 0)
					return Tuple.Create(prev ?? new Dictionary<string, string>(), newGlobals);

				var result = prev != null ? new Dictionary<string, string>(prev) : new Dictionary<string, string>();
				var usedNames = new HashSet<string>(result.Values.Concat(newGlobals));
				foreach (var nl in newLocals) {
					if (!result.ContainsKey(nl)) {
						var n = _generateName(nl, usedNames);
						usedNames.Add(n);
						result[nl] = n;
					}
				}
				return Tuple.Create(result, newGlobals);
			}

			public override JsCatchClause VisitCatchClause(JsCatchClause clause, Tuple<Dictionary<string, string>, HashSet<string>> data) {
				var newData = new Dictionary<string, string>(data.Item1);
				var usedNames = new HashSet<string>(data.Item1.Values.Concat(data.Item2));
				string newName = _generateName(clause.Identifier, usedNames);
				newData.Add(clause.Identifier, newName);
				_result[clause] = newData;
				return base.VisitCatchClause(clause, Tuple.Create(newData, data.Item2));
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, Tuple<Dictionary<string, string>, HashSet<string>> data) {
				var newData = BuildMap(data.Item1, expression);
				_result[expression] = newData.Item1;
				return base.VisitFunctionDefinitionExpression(expression, newData);
			}

			public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, Tuple<Dictionary<string, string>, HashSet<string>> data) {
				var newData = BuildMap(data.Item1, statement);
				_result[statement] = newData.Item1;
				return base.VisitFunctionStatement(statement, newData);
			}

			public static IDictionary<JsDeclarationScope, IDictionary<string, string>> Analyze(JsStatement statement, Dictionary<JsDeclarationScope, HashSet<string>> locals, Dictionary<JsDeclarationScope, HashSet<string>> globals, Func<string, HashSet<string>, string> generateName) {
				var obj = new IdentifierRenameMapBuilder(locals, globals, generateName);
				var rootMap = obj.BuildMap(null, JsDeclarationScope.Root);
				obj._result[JsDeclarationScope.Root] = rootMap.Item1;
				obj.VisitStatement(statement, rootMap);
				return obj._result;
			}
		}

		private const string _encodeNumberTable = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		internal static string EncodeNumber(int i) {
			string result = _encodeNumberTable.Substring(i % _encodeNumberTable.Length, 1);
			while (i >= _encodeNumberTable.Length) {
				i /= _encodeNumberTable.Length;
				result = _encodeNumberTable.Substring(i % _encodeNumberTable.Length, 1) + result;
			}
			return result;
		}

		internal static string GenerateName(string oldName, HashSet<string> usedNames) {
			int i = 0;
			string result;
			do {
				result = EncodeNumber(i++);
			} while (Utils.IsJavaScriptReservedWord(result) || usedNames.Contains(result));
			return result;
		}

		public static JsStatement Process(JsStatement statement) {
			var locals  = LocalVariableGatherer.Analyze(statement);
			var globals = ImplicitGlobalsGatherer.Analyze(statement, locals, reportGlobalsAsUsedInAllParentScopes: true);
			var renames = IdentifierRenameMapBuilder.Analyze(statement, locals, globals, GenerateName);
			return IdentifierRenamer.Process(statement, renames);
		}
	}
}
