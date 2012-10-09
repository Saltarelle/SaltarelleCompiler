using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.MetadataImporter;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Linker {
	/// <summary>
	/// This reference importer assumes that root namespaces and types are global objects.
	/// </summary>
	public class DefaultLinker : ILinker {
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
			private readonly IScriptSharpMetadataImporter _metadataImporter;
			private readonly INamer _namer;
			private readonly IAssembly _mainAssembly;
			private readonly HashSet<string> _usedSymbols;

			private readonly Dictionary<string, string> _moduleAliases;

			private bool IsLocalReference(ITypeDefinition type) {
				if (_metadataImporter.IsImported(type))	// Imported types must always be referenced the hard way...
					return false;
				if (!type.ParentAssembly.Equals(_mainAssembly))	// ...so must types from other assemblies...
					return false;
				var mainModule = _metadataImporter.MainModuleName;
				var typeModule = _metadataImporter.GetModuleName(type);
				return string.IsNullOrEmpty(mainModule) && string.IsNullOrEmpty(typeModule) || mainModule == typeModule;	// ...and types with a [ModuleName] that differs from that of the assembly.
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
				if (result == "")
					result = "_";

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

				string moduleName = _metadataImporter.GetModuleName(expression.Type);

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
					if (string.IsNullOrEmpty(sem.Name) && _metadataImporter.GetModuleName(type) == null)	// Handle types marked with [GlobalMethods] that are not from modules.
						return JsExpression.Identifier(expression.MemberName);
				}
				return base.VisitMemberAccessExpression(expression, data);
			}

			private ImportVisitor(IScriptSharpMetadataImporter metadataImporter, INamer namer, IAssembly mainAssembly, HashSet<string> usedSymbols) {
				_metadataImporter = metadataImporter;
				_namer            = namer;
				_mainAssembly     = mainAssembly;
				_usedSymbols      = usedSymbols;
				_moduleAliases    = new Dictionary<string, string>();
			}

			public static IList<JsStatement> Process(IScriptSharpMetadataImporter metadataImporter, INamer namer, IAssembly mainAssembly, IList<JsStatement> statements) {
				var importer = new ImportVisitor(metadataImporter, namer, mainAssembly, UsedSymbolsGatherer.Analyze(statements));
				var body = statements.Select(s => importer.VisitStatement(s, null)).ToList();
				if (importer._moduleAliases.Count > 0) {
					// If we require any module, we require mscorlib. This should work even if we are a leaf module that doesn't include any other module because our parent script will do the mscorlib require for us.
					body.InsertRange(0, new[] { (JsStatement)new JsExpressionStatement(JsExpression.Invocation(JsExpression.Identifier("require"), JsExpression.String("mscorlib"))) }
					                    .Concat(importer._moduleAliases.OrderBy(x => x.Key)
					                                                   .Select(x => new JsVariableDeclarationStatement(
					                                                                        x.Value,
					                                                                        JsExpression.Invocation(
					                                                                            JsExpression.Identifier("require"),
					                                                                            JsExpression.String(x.Key))))
					                                                   .ToList()));
				}						        
				return body;
			}
		}

		private readonly IScriptSharpMetadataImporter _metadataImporter;
		private readonly INamer _namer;

		public DefaultLinker(IScriptSharpMetadataImporter metadataImporter, INamer namer) {
			_metadataImporter = metadataImporter;
			_namer            = namer;
		}

		public IList<JsStatement> Process(IList<JsStatement> statements, IAssembly mainAssembly) {
			return ImportVisitor.Process(_metadataImporter, _namer, mainAssembly, statements);
		}
	}
}
