using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CoreLib.Plugin;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.OOPEmulation;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests.OOPEmulatorTests {
	public class OOPEmulatorTestBase {
		protected class MockLinker : ILinker {
			public IList<JsStatement> Process(IList<JsStatement> statements) {
				throw new NotImplementedException();
			}

			public JsExpression CurrentAssemblyExpression { get { return JsExpression.Identifier("$asm"); } }
		}

		private void RunAutomaticMetadataAttributeAppliers(IAttributeStore store, ICompilation compilation) {
			var processors = new IAutomaticMetadataAttributeApplier[] { new MakeMembersWithScriptableAttributesReflectable(store) };
			foreach (var p in processors) {
				foreach (var asm in compilation.Assemblies)
					p.Process(asm);
				foreach (var t in compilation.GetAllTypeDefinitions())
					p.Process(t);
			}
		}

		protected Tuple<ICompilation, IOOPEmulator, List<JsType>> Compile(string source, IEnumerable<IAssemblyResource> resources = null, IErrorReporter errorReporter = null) {
			errorReporter = errorReporter ?? new MockErrorReporter(true);
			var sourceFile = new MockSourceFile("file.cs", source);
			var n = new Namer();
			var references = new[] { Files.Mscorlib };
			var compilation = PreparedCompilation.CreateCompilation("x", new[] { sourceFile }, references, null, resources);
			var s = new AttributeStore(compilation.Compilation, errorReporter);
			RunAutomaticMetadataAttributeAppliers(s, compilation.Compilation);
			s.RunAttributeCode();
			var md = new MetadataImporter(errorReporter, compilation.Compilation, s, new CompilerOptions());
			var rtl = new RuntimeLibrary(md, errorReporter, compilation.Compilation, n, s);
			md.Prepare(compilation.Compilation.GetAllTypeDefinitions());
			var compiler = new Compiler(md, n, rtl, errorReporter);
			var compiledTypes = compiler.Compile(compilation).ToList();

			return Tuple.Create(compilation.Compilation, (IOOPEmulator)new OOPEmulator(compilation.Compilation, md, rtl, n, new MockLinker(), s, errorReporter), compiledTypes);

		}

		protected TypeOOPEmulation EmulateType(string source, string typeName, IErrorReporter errorReporter = null) {
			bool assertNoErrors = errorReporter == null;
			errorReporter = errorReporter ?? new MockErrorReporter(true);

			var compiled = Compile(source, errorReporter: errorReporter);

			var emulated = compiled.Item2.EmulateType(compiled.Item3.Single(t => t.CSharpTypeDefinition.FullName == typeName));

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
