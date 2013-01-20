using System;
using CoreLib.Plugin;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using System.Linq;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests {
	public static class SourceVerifier {
		public static Tuple<string, MockErrorReporter> Compile(string source, bool expectErrors = false) {
			var sourceFile = new MockSourceFile("file.cs", source);
			var er = new MockErrorReporter(!expectErrors);
			var md = new MetadataImporter(er);
			var n = new DefaultNamer();
			var references = new[] { Files.Mscorlib };
			var compilation = PreparedCompilation.CreateCompilation(new[] { sourceFile }, references, null);;
			var rtl = new CoreLib.Plugin.RuntimeLibrary(md, er, n, compilation.Compilation);
			md.Prepare(compilation.Compilation.GetAllTypeDefinitions(), false, compilation.Compilation.MainAssembly);
			var compiler = new Saltarelle.Compiler.Compiler.Compiler(md, n, rtl, er);

			var compiledTypes = compiler.Compile(compilation);

			if (expectErrors) {
				Assert.That(er.AllMessages, Is.Not.Empty, "Compile should have generated errors");
				return Tuple.Create((string)null, er);
			}

			Assert.That(er.AllMessages, Is.Empty, "Compile should not generate errors");

			var js = new OOPEmulator(compilation.Compilation, md, rtl, n, er).Process(compiledTypes, null);
			js = new Linker(md, n, compilation.Compilation).Process(js);

			string script = string.Join("", js.Select(s => OutputFormatter.Format(s, allowIntermediates: false)));

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
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expectedJs.Replace("\r\n", "\n")));
		}
	}
}
