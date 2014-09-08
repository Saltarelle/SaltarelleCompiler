using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CoreLib.Plugin;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests;
using Saltarelle.Compiler.Roslyn;

namespace CoreLib.Tests.MetadataImporterTests {
	public class MetadataImporterTestBase {
		private MockErrorReporter _errorReporter;

		protected Dictionary<string, INamedTypeSymbol> AllTypes { get; private set; }
		protected IMetadataImporter Metadata { get; private set; }
		protected IList<string> AllErrorTexts { get; private set; }
		protected IList<Message> AllErrors { get; private set; }

		private void RunAutomaticMetadataAttributeAppliers(IAttributeStore store, Compilation compilation) {
			var processors = new IAutomaticMetadataAttributeApplier[] { new MakeMembersWithScriptableAttributesReflectable(store) };
			foreach (var p in processors) {
				foreach (var reference in compilation.References) {
					var asm = (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(reference);
					p.Process(asm);
				}
				foreach (var t in compilation.GetAllTypes())
					p.Process(t);
			}
		}

		protected void Prepare(string source, bool minimizeNames = true, bool expectErrors = false) {
			var compilation = Common.CreateCompilation(source);
			var errors = string.Join(Environment.NewLine, compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.GetMessage()));
			if (!string.IsNullOrEmpty(errors))
				Assert.Fail("Compilation errors:" + Environment.NewLine + errors);

			_errorReporter = new MockErrorReporter(!expectErrors);

			var s = new AttributeStore(compilation, _errorReporter);
			RunAutomaticMetadataAttributeAppliers(s, compilation);
			s.RunAttributeCode();

			Metadata = new MetadataImporter(Common.ReferenceMetadataImporter, _errorReporter, compilation, s, new CompilerOptions { MinimizeScript = minimizeNames });

			Metadata.Prepare(compilation.GetAllTypes());

			AllErrors = _errorReporter.AllMessages.ToList().AsReadOnly();
			AllErrorTexts = _errorReporter.AllMessages.Select(m => m.FormattedMessage).ToList().AsReadOnly();
			if (expectErrors) {
				Assert.That(AllErrorTexts, Is.Not.Empty, "Compile should have generated errors");
			}
			else {
				Assert.That(AllErrorTexts, Is.Empty, "Compile should not generate errors");
			}

			#warning TODO: Verify that all compiled members and types have had their metadata set

			AllTypes = compilation.Assembly.GetAllTypes().ToDictionary(t => t.MetadataName);
		}

		protected TypeScriptSemantics FindType(string name) {
			return Metadata.GetTypeSemantics(AllTypes[name]);
		}

		protected DelegateScriptSemantics FindDelegate(string name) {
			return Metadata.GetDelegateSemantics(AllTypes[name]);
		}

		protected IEnumerable<ISymbol> FindMembers(string name) {
			var lastDot = name.LastIndexOf('.');
			return AllTypes[name.Substring(0, lastDot)].GetMembers().Where(m => m.Name.Substring(m.Name.LastIndexOf('.') + 1) == name.Substring(lastDot + 1));
		}

		protected List<Tuple<IMethodSymbol, MethodScriptSemantics>> FindMethods(string name) {
			return FindMembers(name).Cast<IMethodSymbol>().Select(m => Tuple.Create(m, Metadata.GetMethodSemantics(m))).ToList();
		}

		protected MethodScriptSemantics FindMethod(string name) {
			return FindMethods(name).Single().Item2;
		}

		protected MethodScriptSemantics FindMethod(string name, int parameterCount) {
			return FindMethods(name).Single(m => m.Item1.Parameters.Length == parameterCount).Item2;
		}

		protected PropertyScriptSemantics FindProperty(string name) {
			return FindMembers(name).Cast<IPropertySymbol>().Where(p => !p.IsIndexer).Select(p => Metadata.GetPropertySemantics(p)).Single();
		}

		protected FieldScriptSemantics FindField(string name) {
			return FindMembers(name).Cast<IFieldSymbol>().Select(f => Metadata.GetFieldSemantics(f)).Single();
		}

		protected PropertyScriptSemantics FindIndexer(string typeName, int parameterCount) {
			return AllTypes[typeName].GetMembers().OfType<IPropertySymbol>().Where(p => p.Parameters.Length == parameterCount).Select(p => Metadata.GetPropertySemantics(p)).Single();
		}

		protected EventScriptSemantics FindEvent(string name) {
			return FindMembers(name).Cast<IEventSymbol>().Select(p => Metadata.GetEventSemantics(p)).Single();
		}

		protected ConstructorScriptSemantics FindConstructor(string typeName, int parameterCount) {
			return Metadata.GetConstructorSemantics(AllTypes[typeName].GetMembers().OfType<IMethodSymbol>().Single(m => m.MethodKind == MethodKind.Constructor && !m.IsStatic && m.Parameters.Length == parameterCount));
		}
	}
}
