using System;
using System.Collections.Generic;
using System.Linq;
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

		protected OOPEmulator CreateEmulator(ICompilation compilation, IErrorReporter errorReporter = null) {
			var n = new Namer();
			errorReporter = errorReporter ?? new MockErrorReporter();
			var md = new MetadataImporter(errorReporter, compilation, new CompilerOptions());
			md.Prepare(compilation.GetAllTypeDefinitions());
			var rtl = new RuntimeLibrary(md, errorReporter, compilation, n);
			return new OOPEmulator(compilation, md, rtl, n, new MockLinker(), errorReporter);
		}

		protected Tuple<ICompilation, List<JsType>> Compile(string source, IEnumerable<IAssemblyResource> resources = null) {
			var errorReporter = new MockErrorReporter(true);
			var sourceFile = new MockSourceFile("file.cs", source);
			var n = new Namer();
			var references = new[] { Files.Mscorlib };
			var compilation = PreparedCompilation.CreateCompilation("x", new[] { sourceFile }, references, null, resources);
			var md = new MetadataImporter(errorReporter, compilation.Compilation, new CompilerOptions());
			var rtl = new RuntimeLibrary(md, errorReporter, compilation.Compilation, n);
			md.Prepare(compilation.Compilation.GetAllTypeDefinitions());
			var compiler = new Compiler(md, n, rtl, errorReporter);
			var compiledTypes = compiler.Compile(compilation).ToList();

			return Tuple.Create(compilation.Compilation, compiledTypes);

		}

		protected TypeOOPEmulation EmulateType(string source, string typeName, IErrorReporter errorReporter = null) {
			bool assertNoErrors = errorReporter == null;
			errorReporter = errorReporter ?? new MockErrorReporter(true);

			var compiled = Compile(source);

			var emulator = CreateEmulator(compiled.Item1, errorReporter);

			var emulated = emulator.EmulateType(compiled.Item2.Single(t => t.CSharpTypeDefinition.FullName == typeName));

			if (assertNoErrors) {
				Assert.That(((MockErrorReporter)errorReporter).AllMessages, Is.Empty, "Should not have errors");
			}

			return emulated;
		}

		protected void AssertCorrectEmulation(string source, string expected, string typeName) {
			var emulated = EmulateType(source, typeName);
			var actual = string.Join("-\n", emulated.Phases.Where(p => p != null).Select(p => string.Join("", p.Statements.Select(s => OutputFormatter.Format(s, allowIntermediates: true)))));
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}
	}
}
