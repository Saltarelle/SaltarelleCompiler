using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Analyzers;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;

namespace CoreLib.Plugin {
	/// <summary>
	/// This reference importer assumes that root namespaces and types are global objects.
	/// </summary>
	public class Linker : ILinker {
		private class IntroducedNamesGatherer : RewriterVisitorBase<IList<string>> {
			private readonly Dictionary<JsDeclarationScope, IList<string>> _result = new Dictionary<JsDeclarationScope, IList<string>>();
			private readonly IMetadataImporter _metadataImporter;
			private readonly IAssembly _mainAssembly;
			private readonly string _mainModuleName;

			private IntroducedNamesGatherer(IMetadataImporter metadataImporter, IAssembly mainAssembly) {
				_metadataImporter = metadataImporter;
				_mainAssembly = mainAssembly;
				_mainModuleName = MetadataUtils.GetModuleName(_mainAssembly);
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, IList<string> data) {
				return base.VisitFunctionDefinitionExpression(expression, _result[expression] = new List<string>());
			}

			public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, IList<string> data) {
				return base.VisitFunctionStatement(statement, _result[statement] = new List<string>());
			}

			public override JsCatchClause VisitCatchClause(JsCatchClause clause, IList<string> data) {
				return base.VisitCatchClause(clause, _result[clause] = new List<string>());
			}

			public override JsExpression VisitTypeReferenceExpression(JsTypeReferenceExpression expression, IList<string> data) {
				var sem = _metadataImporter.GetTypeSemantics(expression.Type);
				if (sem.Type != TypeScriptSemantics.ImplType.NormalType)
					throw new ArgumentException("The type " + expression.Type.FullName + " appears in the output stage but is not a normal type.");

				if (!IsLocalReference(expression.Type, _mainAssembly, _mainModuleName)) {	// Types in our own assembly will not clash with anything because we use the type variable (or the 'exports' object in case of global methods).
					string moduleName = GetTypeModuleName(expression.Type);
					if (moduleName == null) {	// Imported modules will never clash with anything because we handle that elsewhere
						var parts = sem.Name.Split('.');
						data.Add(parts[0]);
					}
				}

				return expression;
			}

			public override JsExpression VisitMemberAccessExpression(JsMemberAccessExpression expression, IList<string> data) {
				if (expression.Target is JsTypeReferenceExpression) {
					var type = ((JsTypeReferenceExpression)expression.Target).Type;
					var sem = _metadataImporter.GetTypeSemantics(type);
					if (string.IsNullOrEmpty(sem.Name) && GetTypeModuleName(type) == null)	// Handle types marked with [GlobalMethods] that are not from modules.
						data.Add(expression.MemberName);
				}
				return base.VisitMemberAccessExpression(expression, data);
			}

			public static IDictionary<JsDeclarationScope, IList<string>> Analyze(IEnumerable<JsStatement> statements, IMetadataImporter metadataImporter, IAssembly mainAssembly) {
				var obj = new IntroducedNamesGatherer(metadataImporter, mainAssembly);
				var root = new List<string>();
				obj._result[JsDeclarationScope.Root] = root;
				foreach (var statement in statements)
					obj.VisitStatement(statement, root);
				return obj._result;
			}
		}

		private class RenameMapBuilder {
			private readonly Dictionary<JsDeclarationScope, HashSet<string>> _locals;
			private readonly Dictionary<JsDeclarationScope, HashSet<string>> _globals;
			private readonly IDictionary<JsDeclarationScope, IList<string>> _introducedNames;
			private readonly Dictionary<JsDeclarationScope, HashSet<string>> _allVisibleLocals = new Dictionary<JsDeclarationScope, HashSet<string>>();
			private readonly Dictionary<JsDeclarationScope, HashSet<string>> _usedNames = new Dictionary<JsDeclarationScope, HashSet<string>>();
			private readonly Dictionary<JsDeclarationScope, IDictionary<string, string>> _result = new Dictionary<JsDeclarationScope, IDictionary<string, string>>();
			private readonly IDictionary<JsDeclarationScope, DeclarationScopeHierarchy> _hierarchy;
			private readonly INamer _namer;

			private RenameMapBuilder(IList<JsStatement> statements, Dictionary<JsDeclarationScope, HashSet<string>> locals, Dictionary<JsDeclarationScope, HashSet<string>> globals, IDictionary<JsDeclarationScope, IList<string>> introducedNames, INamer namer) {
				_locals = locals;
				_globals = globals;
				_introducedNames = introducedNames;
				_namer = namer;
				_hierarchy = DeclarationScopeNestingAnalyzer.Analyze(statements);
				foreach (var s in _hierarchy.Keys)
					_result[s] = new Dictionary<string, string>();
				FillAllVisibleLocals(JsDeclarationScope.Root);
				FillUsedNames(JsDeclarationScope.Root);
				Analyze(JsDeclarationScope.Root);
			}

			private void FillAllVisibleLocals(JsDeclarationScope scope) {
				var hier = _hierarchy[scope];
				_allVisibleLocals[scope] = new HashSet<string>(hier.ParentScope != null ? _allVisibleLocals[hier.ParentScope] : (IEnumerable<string>)new string[0]);
				_allVisibleLocals[scope].UnionWith(_locals[scope]);
				foreach (var c in hier.ChildScopes)
					FillAllVisibleLocals(c);
			}

			private void FillUsedNames(JsDeclarationScope scope) {
				var hier = _hierarchy[scope];
				var set = new HashSet<string>();
				foreach (var c in hier.ChildScopes) {
					FillUsedNames(c);
					set.UnionWith(_usedNames[c]);
				}
				set.UnionWith(_locals[scope]);
				set.UnionWith(_globals[scope]);
				_usedNames[scope] = set;
			}

			private JsDeclarationScope FindDeclaringScope(string name, JsDeclarationScope scope) {
				for (;;) {
					if (_locals[scope].Contains(name))
						return scope;
					scope = _hierarchy[scope].ParentScope;
				}
			}

			private string FindNewName(string name, JsDeclarationScope scope) {
				var usedNames = _usedNames[scope];
				return _namer.GetVariableName(name, usedNames);
			}

			private void AddRename(string oldName, string newName, JsDeclarationScope scope) {
				_result[scope][oldName] = newName;
				_usedNames[scope].Add(newName);
				foreach (var c in _hierarchy[scope].ChildScopes)
					AddRename(oldName, newName, c);
			}

			private void Analyze(JsDeclarationScope scope) {
				var allVisibleLocals = _allVisibleLocals[scope];
				var introducedNames = _introducedNames[scope];
				foreach (var toRename in introducedNames.Where(allVisibleLocals.Contains)) {
					var declaringScope = FindDeclaringScope(toRename, scope);
					var newName = FindNewName(toRename, declaringScope);
					AddRename(toRename, newName, declaringScope);
				}
				foreach (var c in _hierarchy[scope].ChildScopes)
					Analyze(c);
			}

			public static IDictionary<JsDeclarationScope, IDictionary<string, string>> BuildMap(IList<JsStatement> statements, Dictionary<JsDeclarationScope, HashSet<string>> locals, Dictionary<JsDeclarationScope, HashSet<string>> globals, IDictionary<JsDeclarationScope, IList<string>> introducedNames, INamer namer) {
				return new RenameMapBuilder(statements, locals, globals, introducedNames, namer)._result;
			}
		}

		private class ImportVisitor : RewriterVisitorBase<object> {
			private readonly IMetadataImporter _metadataImporter;
			private readonly INamer _namer;
			private readonly IAssembly _mainAssembly;
			private readonly HashSet<string> _usedSymbols;
			private readonly string _mainModuleName;

			private readonly Dictionary<string, string> _moduleAliases;

			private string GetModuleAlias(string moduleName) {
				string result;
				if (_moduleAliases.TryGetValue(moduleName, out result))
					return result;

				result = "";
				for (int i = 0; i < moduleName.Length; i++) {
					char ch = moduleName[i];
					if (ch == '_' || ch == '$' || char.IsLetter(ch) || (i > 0 && char.IsDigit(ch)))
						result += ch;
				}
				if (result == "" || char.IsDigit(result[0]))
					result = "_" + result;

				result = _namer.GetVariableName(result, _usedSymbols);
				_usedSymbols.Add(result);
				
				return _moduleAliases[moduleName] = result;
			}

			public override JsExpression VisitTypeReferenceExpression(JsTypeReferenceExpression expression, object data) {
				var sem = _metadataImporter.GetTypeSemantics(expression.Type);
				if (sem.Type != TypeScriptSemantics.ImplType.NormalType)
					throw new ArgumentException("The type " + expression.Type.FullName + " appears in the output stage but is not a normal type.");

				if (IsLocalReference(expression.Type, _mainAssembly, _mainModuleName)) {
					if (string.IsNullOrEmpty(sem.Name))
						return JsExpression.Identifier("exports");	// Referencing a [GlobalMethods] type. Since it was not handled in the member expression, we must be in a module, which means that the function should exist on the exports object.

					// For types in our own assembly, we can use the $TYPE variable in the pattern "var $TYPE = function() {} ... Type.registerClass(global, 'The.Name', $TYPE)"
					return JsExpression.Identifier(_namer.GetTypeVariableName(_metadataImporter.GetTypeSemantics(expression.Type).Name));
				}

				string moduleName = GetTypeModuleName(expression.Type);

				var parts = sem.Name.Split('.');
				JsExpression result;
				if (moduleName != null) {
					result = JsExpression.Identifier(GetModuleAlias(moduleName));
					if (!string.IsNullOrEmpty(sem.Name))	// Test for [GlobalMethods] types.
						result = JsExpression.Member(result, parts[0]);
				}
				else {
					result = JsExpression.Identifier(parts[0]);
				}

				for (int i = 1; i < parts.Length; i++)
					result = JsExpression.Member(result, parts[i]);
				return result;
			}

			public override JsExpression VisitMemberAccessExpression(JsMemberAccessExpression expression, object data) {
				if (expression.Target is JsTypeReferenceExpression) {
					var type = ((JsTypeReferenceExpression)expression.Target).Type;
					var sem = _metadataImporter.GetTypeSemantics(type);
					if (string.IsNullOrEmpty(sem.Name) && GetTypeModuleName(type) == null)	// Handle types marked with [GlobalMethods] that are not from modules.
						return JsExpression.Identifier(expression.MemberName);
				}
				return base.VisitMemberAccessExpression(expression, data);
			}

			private ImportVisitor(IMetadataImporter metadataImporter, INamer namer, IAssembly mainAssembly, HashSet<string> usedSymbols) {
				_metadataImporter = metadataImporter;
				_namer            = namer;
				_mainModuleName   = MetadataUtils.GetModuleName(mainAssembly);
				_mainAssembly     = mainAssembly;
				_usedSymbols      = usedSymbols;
				_moduleAliases    = new Dictionary<string, string>();
			}

			public static IList<JsStatement> Process(IMetadataImporter metadataImporter, INamer namer, ICompilation compilation, IList<JsStatement> statements) {
				var locals = LocalVariableGatherer.Analyze(statements);
				var globals = ImplicitGlobalsGatherer.Analyze(statements, locals, reportGlobalsAsUsedInAllParentScopes: false);
				var introducedNames = IntroducedNamesGatherer.Analyze(statements, metadataImporter, compilation.MainAssembly);
				var renameMap = RenameMapBuilder.BuildMap(statements, locals, globals, introducedNames, namer);

				var usedSymbols = new HashSet<string>();
				foreach (var sym in         locals.Values.SelectMany(v => v)            // Declared locals.
				                    .Concat(globals.Values.SelectMany(v => v))          // Implicitly declared globals.
				                    .Concat(renameMap.Values.SelectMany(v => v.Values)) // Locals created during preparing rename.
				                    .Concat(introducedNames.Values.SelectMany(v => v))  // All global types used.
				) {
					usedSymbols.Add(sym);
				}

				statements = IdentifierRenamer.Process(statements, renameMap).ToList();

				var importer = new ImportVisitor(metadataImporter, namer, compilation.MainAssembly, usedSymbols);
				var body = statements.Select(s => importer.VisitStatement(s, null)).ToList();
				var moduleDependencies = importer._moduleAliases.Concat(MetadataUtils.GetAdditionalDependencies(compilation.MainAssembly));

				if (MetadataUtils.IsAsyncModule(compilation.MainAssembly)) {
					body.InsertRange(0, new[] { JsStatement.UseStrict, JsStatement.Var("exports", JsExpression.ObjectLiteral()) });
					body.Add(JsStatement.Return(JsExpression.Identifier("exports")));

					var pairs = new[] { new KeyValuePair<string, string>("mscorlib", namer.GetVariableName("_", usedSymbols)) }
						.Concat(moduleDependencies.OrderBy(x => x.Key))
						.ToList();

					body = new List<JsStatement> {
					           JsExpression.Invocation(
					               JsExpression.Identifier("define"),
					               JsExpression.ArrayLiteral(pairs.Select(p => JsExpression.String(p.Key))),
					               JsExpression.FunctionDefinition(
					                   pairs.Select(p => p.Value),
					                   JsStatement.Block(body)
					               )
					           )
					       };
				}
				else if (moduleDependencies.Any()) {
					// If we require any module, we require mscorlib. This should work even if we are a leaf module that doesn't include any other module because our parent script will do the mscorlib require for us.
					body.InsertRange(0, new[] { JsStatement.UseStrict, JsExpression.Invocation(JsExpression.Identifier("require"), JsExpression.String("mscorlib")) }
										.Concat(moduleDependencies
											.OrderBy(x => x.Key).OrderBy(x => x.Key)
												.Select(x => JsStatement.Var(
													x.Value,
													JsExpression.Invocation(
														JsExpression.Identifier("require"),
														JsExpression.String(x.Key))))
												.ToList()));
				}
				else {
					body.Insert(0, JsStatement.UseStrict);
					body = new List<JsStatement> { JsExpression.Invocation(JsExpression.FunctionDefinition(new string[0], JsStatement.Block(body))) };
				}

				return body;
			}
		}

		private static readonly Dictionary<ITypeDefinition, string> _typeModuleNames = new Dictionary<ITypeDefinition, string>();
		private static string GetTypeModuleName(ITypeDefinition type) {
			string result;
			if (!_typeModuleNames.TryGetValue(type, out result)) {
				_typeModuleNames[type] = result = MetadataUtils.GetModuleName(type);
			}
			return result;
		}

		private static bool IsLocalReference(ITypeDefinition type, IAssembly mainAssembly, string mainModuleName) {
			if (MetadataUtils.IsImported(type))	// Imported types must always be referenced the hard way...
				return false;
			if (!type.ParentAssembly.Equals(mainAssembly))	// ...so must types from other assemblies...
				return false;
			var typeModule = GetTypeModuleName(type);
			return string.IsNullOrEmpty(mainModuleName) && string.IsNullOrEmpty(typeModule) || mainModuleName == typeModule;	// ...and types with a [ModuleName] that differs from that of the assembly.
		}


		private readonly IMetadataImporter _metadataImporter;
		private readonly INamer _namer;
		private readonly ICompilation _compilation;

		public Linker(IMetadataImporter metadataImporter, INamer namer, ICompilation compilation) {
			_metadataImporter = metadataImporter;
			_namer            = namer;
			_compilation      = compilation;
		}

		public IList<JsStatement> Process(IList<JsStatement> statements) {
			return ImportVisitor.Process(_metadataImporter, _namer, _compilation, statements);
		}
	}
}
