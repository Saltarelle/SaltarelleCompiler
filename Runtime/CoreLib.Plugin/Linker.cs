using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;

namespace CoreLib.Plugin {
	/// <summary>
	/// This reference importer assumes that root namespaces and types are global objects.
	/// </summary>
	public class Linker : ILinker {
		internal class UsedSymbolsGatherer : RewriterVisitorBase<object> {
			private readonly HashSet<string> _result = new HashSet<string>();

			private UsedSymbolsGatherer() {
			}

			public override JsVariableDeclaration VisitVariableDeclaration(JsVariableDeclaration declaration, object data) {
				_result.Add(declaration.Name);
				return base.VisitVariableDeclaration(declaration, data);
			}

			public override JsStatement VisitForEachInStatement(JsForEachInStatement statement, object data) {
				_result.Add(statement.LoopVariableName);
				return base.VisitForEachInStatement(statement, data);
			}

			public override JsCatchClause VisitCatchClause(JsCatchClause clause, object data) {
				_result.Add(clause.Identifier);
				return base.VisitCatchClause(clause, data);
			}

			public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
				_result.Add(statement.Name);
				foreach (var p in statement.ParameterNames)
					_result.Add(p);
				return base.VisitFunctionStatement(statement, data);
			}

			public override JsExpression VisitIdentifierExpression(JsIdentifierExpression expression, object data) {
				_result.Add(expression.Name);
				return base.VisitIdentifierExpression(expression, data);
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
				if (expression.Name != null)
					_result.Add(expression.Name);
				foreach (var p in expression.ParameterNames)
					_result.Add(p);
				return base.VisitFunctionDefinitionExpression(expression, data);
			}

			public static HashSet<string> Analyze(IEnumerable<JsStatement> statements) {
				var o = new UsedSymbolsGatherer();
				foreach (var s in statements)
					o.VisitStatement(s, null);
				return o._result;
			}
		}

		private class ImportVisitor : RewriterVisitorBase<object> {
			private readonly IMetadataImporter _metadataImporter;
			private readonly INamer _namer;
			private readonly IAssembly _mainAssembly;
			private readonly HashSet<string> _usedSymbols;
			private readonly string _mainModuleName;

			private readonly Dictionary<string, string> _moduleAliases;

			private readonly Dictionary<ITypeDefinition, string> _typeModuleNames = new Dictionary<ITypeDefinition, string>();
			private string GetTypeModuleName(ITypeDefinition type) {
				string result;
				if (!_typeModuleNames.TryGetValue(type, out result)) {
					_typeModuleNames[type] = result = MetadataUtils.GetModuleName(type);
				}
				return result;
			}

			private bool IsLocalReference(ITypeDefinition type) {
				if (MetadataUtils.IsImported(type))	// Imported types must always be referenced the hard way...
					return false;
				if (!type.ParentAssembly.Equals(_mainAssembly))	// ...so must types from other assemblies...
					return false;
				var typeModule = GetTypeModuleName(type);
				return string.IsNullOrEmpty(_mainModuleName) && string.IsNullOrEmpty(typeModule) || _mainModuleName == typeModule;	// ...and types with a [ModuleName] that differs from that of the assembly.
			}

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

				if (IsLocalReference(expression.Type)) {
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
				var usedSymbols = UsedSymbolsGatherer.Analyze(statements);
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
