using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ICSharpCode.NRefactory.Utils;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.Minimization
{
	public class Minimizer {
		internal struct Function : IEquatable<Function> {
			public JsFunctionDefinitionExpression Expr { get; private set; }
			public JsFunctionStatement Stmt { get; private set; }

			public static implicit operator Function(JsFunctionDefinitionExpression expr) {
				return new Function { Expr = expr };
			}

			public static implicit operator Function(JsFunctionStatement stmt) {
				return new Function { Stmt = stmt };
			}

			public bool Equals(Function other) {
				return ReferenceEquals(other.Expr, Expr) && ReferenceEquals(other.Stmt, Stmt);
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj)) return false;
				if (obj.GetType() != typeof (Function)) return false;
				return Equals((Function) obj);
			}

			public override int GetHashCode() {
				return RuntimeHelpers.GetHashCode(Expr) ^ RuntimeHelpers.GetHashCode(Stmt);
			}
		}

		internal class LocalVariableGatherer : RewriterVisitorBase<HashSet<string>> {
			private Dictionary<Function, HashSet<string>> _result = new Dictionary<Function, HashSet<string>>();

			private LocalVariableGatherer() {
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, HashSet<string> data) {
				return base.VisitFunctionDefinitionExpression(expression, _result[expression] = new HashSet<string>(expression.ParameterNames));
			}

			public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, HashSet<string> data) {
				return base.VisitFunctionStatement(statement, _result[statement] = new HashSet<string>(statement.ParameterNames));
			}

			public override JsVariableDeclaration VisitVariableDeclaration(JsVariableDeclaration declaration, HashSet<string> data) {
				data.Add(declaration.Name);
				return base.VisitVariableDeclaration(declaration, data);
			}

			public override JsStatement VisitForEachInStatement(JsForEachInStatement statement, HashSet<string> data) {
				if (statement.IsLoopVariableDeclared)
					data.Add(statement.LoopVariableName);
				return base.VisitForEachInStatement(statement, data);
			}

			public static Dictionary<Function, HashSet<string>> Analyze(JsStatement statement) {
				var obj = new LocalVariableGatherer();
				obj.VisitStatement(statement, obj._result[new Function()] = new HashSet<string>());
				return obj._result;
			}
		}

		internal class ImplicitGlobalsGatherer : RewriterVisitorBase<Tuple<ImmutableStack<Function>, HashSet<string>>> {
			private Dictionary<Function, HashSet<string>> _result = new Dictionary<Function, HashSet<string>>();
			private Dictionary<Function, HashSet<string>> _locals;

			private ImplicitGlobalsGatherer(Dictionary<Function, HashSet<string>> locals) {
				_locals = locals;
			}

			private static HashSet<string> Union(IEnumerable<string> prev, IEnumerable<string> current) {
				var result = prev != null ? new HashSet<string>(prev) : new HashSet<string>();
				result.UnionWith(current);
				return result;
			}

			private void MaybeAddGlobal(string name, HashSet<string> locals, ImmutableStack<Function> functionStack) {
				if (!locals.Contains(name)) {
					foreach (var f in functionStack)
						_result[f].Add(name);
				}
			}

			public override JsExpression VisitIdentifierExpression(JsIdentifierExpression expression, Tuple<ImmutableStack<Function>, HashSet<string>> data) {
				MaybeAddGlobal(expression.Name, data.Item2, data.Item1);
				return expression;
			}

			public override JsStatement VisitForEachInStatement(JsForEachInStatement statement, Tuple<ImmutableStack<Function>, HashSet<string>> data) {
				if (!statement.IsLoopVariableDeclared)
					MaybeAddGlobal(statement.LoopVariableName, data.Item2, data.Item1);
				return base.VisitForEachInStatement(statement, data);
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, Tuple<ImmutableStack<Function>, HashSet<string>> data) {
				_result[expression] = new HashSet<string>();
				return base.VisitFunctionDefinitionExpression(expression, Tuple.Create(data.Item1.Push(expression), Union(data.Item2, _locals[expression])));
			}

			public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, Tuple<ImmutableStack<Function>, HashSet<string>> data) {
				_result[statement] = new HashSet<string>();
				MaybeAddGlobal(statement.Name, data.Item2, data.Item1);
				return base.VisitFunctionStatement(statement, Tuple.Create(data.Item1.Push(statement), Union(data.Item2, _locals[statement])));
			}

			public override JsCatchClause VisitCatchClause(JsCatchClause clause, Tuple<ImmutableStack<Function>, HashSet<string>> data) {
				return base.VisitCatchClause(clause, Tuple.Create(data.Item1, Union(data.Item2, new[] { clause.Identifier })));
			}

			public static Dictionary<Function, HashSet<string>> Analyze(JsStatement statement, Dictionary<Function, HashSet<string>> locals) {
				var obj = new ImplicitGlobalsGatherer(locals);
				obj._result[new Function()] = new HashSet<string>();
				obj.VisitStatement(statement, Tuple.Create(ImmutableStack<Function>.Empty.Push(new Function()), Union(null, locals[new Function()])));
				return obj._result;
			}
		}

		internal class IdentifierMinimizerRewriter : RewriterVisitorBase<Tuple<Dictionary<string, string>, HashSet<string>>> {
			private Dictionary<Function, HashSet<string>> _locals;
			private Dictionary<Function, HashSet<string>> _globals;
			private readonly Func<string, HashSet<string>, string> _generateName;

			private IdentifierMinimizerRewriter(Dictionary<Function, HashSet<string>> locals, Dictionary<Function, HashSet<string>> globals, Func<string, HashSet<string>, string> generateName) {
				_locals = locals;
				_globals = globals;
				_generateName = generateName;
			}

			private Tuple<Dictionary<string, string>, HashSet<string>> BuildMap(Dictionary<string, string> prev, Function function) {
				var newLocals = _locals[function];
				var newGlobals = _globals[function];

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

			public override JsExpression VisitIdentifierExpression(JsIdentifierExpression expression, Tuple<Dictionary<string, string>, HashSet<string>> data) {
				string newName;
				return data.Item1.TryGetValue(expression.Name, out newName) ? JsExpression.Identifier(newName) : expression;
			}

			public override JsVariableDeclaration VisitVariableDeclaration(JsVariableDeclaration declaration, Tuple<Dictionary<string, string>, HashSet<string>> data) {
				string newName;
				if (data.Item1.TryGetValue(declaration.Name, out newName))
					return new JsVariableDeclaration(newName, declaration.Initializer != null ? VisitExpression(declaration.Initializer, data) : null);
				else
					return base.VisitVariableDeclaration(declaration, data);
			}

			public override JsStatement VisitForEachInStatement(JsForEachInStatement statement, Tuple<Dictionary<string, string>, HashSet<string>> data) {
				string newName;
				if (data.Item1.TryGetValue(statement.LoopVariableName, out newName))
					return new JsForEachInStatement(newName, VisitExpression(statement.ObjectToIterateOver, data), VisitStatement(statement.Body, data), statement.IsLoopVariableDeclared);
				else
					return base.VisitForEachInStatement(statement, data);
			}

			public override JsCatchClause VisitCatchClause(JsCatchClause clause, Tuple<Dictionary<string, string>, HashSet<string>> data) {
				var newData = new Dictionary<string, string>(data.Item1);
				var usedNames = new HashSet<string>(data.Item1.Values.Concat(data.Item2));
				string newName = _generateName(clause.Identifier, usedNames);
				newData.Add(clause.Identifier, newName);
				return new JsCatchClause(newName, VisitStatement(clause.Body, Tuple.Create(newData, data.Item2)));
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, Tuple<Dictionary<string, string>, HashSet<string>> data) {
				var newData = BuildMap(data.Item1, expression);
				return JsExpression.FunctionDefinition(expression.ParameterNames.Select(p => { string s; newData.Item1.TryGetValue(p, out s); return s ?? p; }), VisitStatement(expression.Body, newData), expression.Name);
			}

			public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, Tuple<Dictionary<string, string>, HashSet<string>> data) {
				var newData = BuildMap(data.Item1, statement);
				return new JsFunctionStatement(statement.Name, statement.ParameterNames.Select(p => { string s; newData.Item1.TryGetValue(p, out s); return s ?? p; }), VisitStatement(statement.Body, newData));
			}

			public static JsStatement Process(JsStatement statement, Dictionary<Function, HashSet<string>> locals, Dictionary<Function, HashSet<string>> globals, Func<string, HashSet<string>, string> generateName) {
				var obj = new IdentifierMinimizerRewriter(locals, globals, generateName);
				return obj.VisitStatement(statement, obj.BuildMap(null, new Function()));
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

		public JsStatement Process(JsStatement statement) {
			var locals  = LocalVariableGatherer.Analyze(statement);
			var globals = ImplicitGlobalsGatherer.Analyze(statement, locals);
			return IdentifierMinimizerRewriter.Process(statement, locals, globals, GenerateName);
		}
	}
}
