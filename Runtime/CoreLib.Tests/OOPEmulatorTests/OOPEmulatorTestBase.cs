using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib.Plugin;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.OOPEmulation;
using Saltarelle.Compiler.Tests;
using Saltarelle.Compiler.Roslyn;

namespace CoreLib.Tests.OOPEmulatorTests {
	public class OOPEmulatorTestBase {
		protected class MockLinker : ILinker {
			public IList<JsStatement> Process(IList<JsStatement> statements) {
				throw new NotImplementedException();
			}

			public JsExpression CurrentAssemblyExpression { get { return JsExpression.Identifier("$asm"); } }
		}

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

		protected Tuple<Compilation, IOOPEmulator, List<JsType>> Compile(string source, IEnumerable<object> resources = null, IErrorReporter errorReporter = null) {
			errorReporter = errorReporter ?? new MockErrorReporter(true);
			var n = new Namer();
			var compilation = Common.CreateCompilation(source);
			var errors = string.Join(Environment.NewLine, compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.GetMessage()));
			if (!string.IsNullOrEmpty(errors)) {
				Assert.Fail("Compilation Errors:" + Environment.NewLine + errors);
			}
			var s = new AttributeStore(compilation, errorReporter);
			RunAutomaticMetadataAttributeAppliers(s, compilation);
			s.RunAttributeCode();
			var md = new MetadataImporter(Common.ReferenceMetadataImporter, errorReporter, compilation, s, new CompilerOptions());
			var rtl = new RuntimeLibrary(md, errorReporter, compilation, n, s);
			md.Prepare(compilation.GetAllTypes());
			var compiler = new Compiler(md, n, rtl, errorReporter);
			var compiledTypes = compiler.Compile(compilation).ToList();

			return Tuple.Create((Compilation)compilation, (IOOPEmulator)new OOPEmulator(compilation, md, rtl, n, new MockLinker(), s, errorReporter), compiledTypes);
		}

		protected TypeOOPEmulation EmulateType(string source, string typeName, IErrorReporter errorReporter = null) {
			bool assertNoErrors = errorReporter == null;
			errorReporter = errorReporter ?? new MockErrorReporter(true);

			var compiled = Compile(source, errorReporter: errorReporter);

			var emulated = compiled.Item2.EmulateType(compiled.Item3.Single(t => t.CSharpTypeDefinition.FullyQualifiedName() == typeName));

			if (assertNoErrors) {
				Assert.That(((MockErrorReporter)errorReporter).AllMessages, Is.Empty, "Should not have errors");
			}

			return emulated;
		}

		protected void AssertCorrectEmulation(string source, string expected, string typeName) {
			var emulated = EmulateType(source, typeName);
			var actual = string.Join("-\n", emulated.Phases.Where(p => p != null).Select(p => string.Join("", OutputFormatter.Format(p.Statements, allowIntermediates: true))));
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}
	}
}
