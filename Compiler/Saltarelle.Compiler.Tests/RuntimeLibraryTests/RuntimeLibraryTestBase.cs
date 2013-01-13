using System;
using System.Collections;
using FluentAssertions;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.Linker;
using Saltarelle.Compiler.RuntimeLibrary;
using com.gargoylesoftware.htmlunit;
using com.gargoylesoftware.htmlunit.html;
using java.net;
using System.Linq;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	public class RuntimeLibraryTestBase {
		private enum OutputType { None, GeneratedScript, Html };

		private OutputType Output = OutputType.None;

		protected void AssertStringsEqual(string expected, string actual) {
			Assert.That(expected.Replace("\r\n", "\n"), Is.EqualTo(actual.Replace("\r\n", "\n")));
		}

		private HtmlPage GeneratePage(string script, bool includeLinq) {
			WebClient client = new WebClient();
			try {
				var html =
@"<html>
	<head>
		<title>Test</title>
	</head>
	<body>
		<script type=""text/javascript"">" + Environment.NewLine + Common.MscorlibScript + @"</script>
" + (includeLinq ? @"		<script type=""text/javascript"">" + Environment.NewLine + Common.LinqScript + @"</script>" : "") + @"
		<script type=""text/javascript"">" + Environment.NewLine + script + @"</script>
	</body>
</html>
";
				if (Output == OutputType.Html)
					Console.Write(html);

				var response = new StringWebResponse(html, new URL("http://localhost/test.htm"));
				return HTMLParser.parseHtml(response, client.getCurrentWindow());
			}
			finally {
				client.closeAllWindows();
			}
		}

		private object ConvertResult(object result) {
			if (result is IEnumerable && !(result is string)) {
				var l = new ArrayList();
				foreach (var i in (IEnumerable)result)
					l.Add(ConvertResult(i));
				return l;
			}
			else if (result is java.lang.Boolean) {
				return ((java.lang.Boolean)result).booleanValue();
			}
			else if (result is java.lang.Double) {
				return ((java.lang.Double)result).doubleValue();
			}
			else if (result is java.lang.Integer) {
				return ((java.lang.Integer)result).intValue();
			}
			else if (result != null && result.GetType().FullName == "net.sourceforge.htmlunit.corejs.javascript.NativeString") {
				return result.ToString();
			}
			else {
				return result;
			}
		}

		protected object ExecuteScript(string generatedScript, string scriptToReturn, bool includeLinq = false) {
			var page = GeneratePage(generatedScript, includeLinq);
			var result = page.executeJavaScript(scriptToReturn).getJavaScriptResult();
			return ConvertResult(result);
		}

		internal Tuple<string, ICompilation, IMetadataImporter, MockErrorReporter> Compile(string source, bool includeLinq = false, bool expectErrors = false) {
			var sourceFile = new MockSourceFile("file.cs", source);
            var er = new MockErrorReporter(!expectErrors);
			var md = new MetadataImporter.ScriptSharpMetadataImporter(er);
			var n = new DefaultNamer();
            var references = includeLinq ? new[] { Common.Mscorlib, Common.Linq } : new[] { Common.Mscorlib };
			var compilation = PreparedCompilation.CreateCompilation(new[] { sourceFile }, references, null);;
			var rtl = new ScriptSharpRuntimeLibrary(md, er, n, compilation.Compilation);
			md.Prepare(compilation.Compilation.GetAllTypeDefinitions(), false, compilation.Compilation.MainAssembly);
            var compiler = new Compiler.Compiler(md, n, rtl, er);

			var compiledTypes = compiler.Compile(compilation);

			if (expectErrors) {
				Assert.That(er.AllMessages, Is.Not.Empty, "Compile should have generated errors");
				return Tuple.Create((string)null, compilation.Compilation, (IMetadataImporter)md, er);
			}

            er.AllMessages.Should().BeEmpty("Compile should not generate errors");

			var js = new OOPEmulator.ScriptSharpOOPEmulator(compilation.Compilation, md, rtl, n, er).Process(compiledTypes, null);
			js = new DefaultLinker(md, n).Process(js, compilation.Compilation.MainAssembly);

			string script = string.Join("", js.Select(s => OutputFormatter.Format(s, allowIntermediates: false)));

			if (Output == OutputType.GeneratedScript)
				Console.WriteLine(script);
			return Tuple.Create(script, compilation.Compilation, (IMetadataImporter)md, er);
		}

		protected object ExecuteCSharp(string source, string methodName, bool includeLinq = false) {
			var compiled = Compile(source, includeLinq: includeLinq);

			int lastDot = methodName.LastIndexOf(".", System.StringComparison.Ordinal);
			var type = ReflectionHelper.ParseReflectionName(methodName.Substring(0, lastDot)).Resolve(compiled.Item2).GetDefinition();
			var method = type.Methods.Single(m => m.Name == methodName.Substring(lastDot + 1));
			string scriptMethod = compiled.Item3.GetTypeSemantics(type).Name + "." + compiled.Item3.GetMethodSemantics(method).Name;

			return ExecuteScript(compiled.Item1, scriptMethod + "()", includeLinq: includeLinq);
		}

		protected void AssertSourceCorrect(string csharp, string expectedJs, bool includeLinq = false) {
			string actual = Compile(csharp, includeLinq).Item1;

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
