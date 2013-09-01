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
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests.OOPEmulatorTests {
	public class OOPEmulatorTestBase {
		private class MockLinker : ILinker {
			public IList<JsStatement> Process(IList<JsStatement> statements) {
				throw new NotImplementedException();
			}

			public JsExpression CurrentAssemblyExpression { get { return JsExpression.Identifier("$asm"); } }
		}

		protected string Process(string source, string[] typeNames = null, string entryPoint = null, IErrorReporter errorReporter = null) {
			bool assertNoErrors = errorReporter == null;
			errorReporter = errorReporter ?? new MockErrorReporter(true);
			var sourceFile = new MockSourceFile("file.cs", source);
			var n = new Namer();
			var references = new[] { Files.Mscorlib };
			var compilation = PreparedCompilation.CreateCompilation("x", new[] { sourceFile }, references, null);;
			var md = new MetadataImporter(errorReporter, compilation.Compilation, new CompilerOptions());
			var rtl = new RuntimeLibrary(md, errorReporter, compilation.Compilation, n);
			var l = new MockLinker();
			md.Prepare(compilation.Compilation.GetAllTypeDefinitions());
			var compiler = new Compiler(md, n, rtl, errorReporter);
			var compiledTypes = compiler.Compile(compilation).ToList();
			var obj = new OOPEmulator(compilation.Compilation, md, rtl, n, l, errorReporter);
			IMethod ep;
			if (entryPoint != null) {
				var type = compiledTypes.Single(c => c.CSharpTypeDefinition.FullName == entryPoint.Substring(0, entryPoint.IndexOf('.')));
				ep = type.CSharpTypeDefinition.Methods.Single(m => m.FullName == entryPoint);
			}
			else {
				ep = null;
			}
			var rewritten = obj.Process(compiledTypes.Where(t => typeNames == null || typeNames.Contains(t.CSharpTypeDefinition.FullName)), ep);

			if (assertNoErrors)
				Assert.That(((MockErrorReporter)errorReporter).AllMessages, Is.Empty, "Should not have errors");

			return string.Join("", rewritten.Select(s => OutputFormatter.Format(s, allowIntermediates: true)));
		}

		protected void AssertCorrect(string source, string expected, string[] typeNames = null, string entryPoint = null) {
			string actual = Process(source, typeNames, entryPoint);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:" + Environment.NewLine + expected + Environment.NewLine + Environment.NewLine + "Actual:" + Environment.NewLine + actual);
		}
	}
}
