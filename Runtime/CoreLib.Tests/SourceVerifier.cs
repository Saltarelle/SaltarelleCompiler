using System;
using System.Collections.Generic;
using CoreLib.Plugin;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using System.Linq;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.OOPEmulation;
using Saltarelle.Compiler.Tests;
using CompilerOptions = Saltarelle.Compiler.CompilerOptions;

namespace CoreLib.Tests {
	public static class SourceVerifier {
		private class MockLinker : ILinker {
			public IList<JsStatement> Process(IList<JsStatement> statements) {
				throw new NotImplementedException();
			}

			public JsExpression CurrentAssemblyExpression { get { return JsExpression.Identifier("$asm"); } }
		}

		public static Tuple<string, MockErrorReporter> Compile(string source, bool expectErrors = false) {
			var sourceFile = new MockSourceFile("file.cs", source);
			var er = new MockErrorReporter(!expectErrors);
			var n = new Namer();
			var references = new[] { Files.Mscorlib };
			var compilation = PreparedCompilation.CreateCompilation("x", new[] { sourceFile }, references, null);;
			var s = new AttributeStore(compilation.Compilation, er);
			var md = new MetadataImporter(er, compilation.Compilation, s, new CompilerOptions());
			var rtl = new RuntimeLibrary(md, er, compilation.Compilation, n, s);
			var l = new MockLinker();
			md.Prepare(compilation.Compilation.GetAllTypeDefinitions());
			var compiler = new Compiler(md, n, rtl, er);

			var compiledTypes = compiler.Compile(compilation).ToList();

			if (expectErrors) {
				Assert.That(er.AllMessages, Is.Not.Empty, "Compile should have generated errors");
				return Tuple.Create((string)null, er);
			}

			Assert.That(er.AllMessages, Is.Empty, "Compile should not generate errors");

			var js = new OOPEmulatorInvoker(new OOPEmulator(compilation.Compilation, md, rtl, n, l, s, er), md, er).Process(compiledTypes, null);
			js = new Linker(md, n, s, compilation.Compilation).Process(js);

			string script = OutputFormatter.Format(js, allowIntermediates: false);

			return Tuple.Create(script, er);
		}

		public static void AssertSourceCorrect(string csharp, string expectedJs) {
			string actual = Compile(csharp).Item1;

			int begin = actual.IndexOf("// BEGIN");
			if (begin > -1) {
				while (begin < (actual.Length - 1) && actual[begin - 1] != '\n')
					begin++;
				actual = actual.Substring(begin);
			}

			int end = actual.IndexOf("// END");
			if (end >= 0) {
				while (end >= 0 && actual[end] != '\n')
					end--;
				actual = actual.Substring(0, end + 1);
			}
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expectedJs.Replace("\r\n", "\n")), "Expected:" + Environment.NewLine + expectedJs + Environment.NewLine + Environment.NewLine + "Actual:" + Environment.NewLine + actual);
		}
	}
}
