using System;
using System.Collections;
using FluentAssertions;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ReferenceImporter;
using Saltarelle.Compiler.RuntimeLibrary;
using com.gargoylesoftware.htmlunit;
using com.gargoylesoftware.htmlunit.html;
using java.net;
using System.Linq;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	public class RuntimeLibraryTestBase {
		private enum OutputType { None, GeneratedScript, Html };

		private OutputType Output = OutputType.Html;

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

		private Tuple<string, ICompilation, INamingConventionResolver> Compile(string source, bool includeLinq = false) {
			var sourceFile = new MockSourceFile("file.cs", source);
			var nc = new MetadataImporter.ScriptSharpMetadataImporter(false);
            var er = new MockErrorReporter(true);
			PreparedCompilation compilation = null;
			var rtl = new ScriptSharpRuntimeLibrary(nc, tr => { var t = tr.Resolve(compilation.Compilation).GetDefinition(); return new JsTypeReferenceExpression(t.ParentAssembly, nc.GetTypeSemantics(t).Name); });
            var compiler = new Saltarelle.Compiler.Compiler.Compiler(nc, rtl, er);

            er.AllMessagesText.Should().BeEmpty("Compile should not generate errors");

            var references = includeLinq ? new[] { Common.Mscorlib, Common.Linq } : new[] { Common.Mscorlib };
			compilation = compiler.CreateCompilation(new[] { sourceFile }, references, null);
			var compiledTypes = compiler.Compile(compilation);

			var js = new OOPEmulator.ScriptSharpOOPEmulator(nc, er).Rewrite(compiledTypes, compilation.Compilation);
			js = new GlobalNamespaceReferenceImporter().ImportReferences(js);

			string script = string.Join("", js.Select(s => OutputFormatter.Format(s, allowIntermediates: false)));

			if (Output == OutputType.GeneratedScript)
				Console.WriteLine(script);
			return Tuple.Create(script, compilation.Compilation, (INamingConventionResolver)nc);
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
