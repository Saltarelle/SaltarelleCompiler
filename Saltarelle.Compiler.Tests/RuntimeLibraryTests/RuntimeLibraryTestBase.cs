using System;
using System.Collections;
using FluentAssertions;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.ReferenceImporter;
using Saltarelle.Compiler.RuntimeLibrary;
using com.gargoylesoftware.htmlunit;
using com.gargoylesoftware.htmlunit.html;
using java.net;
using System.Linq;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	public class RuntimeLibraryTestBase {
		private enum OutputType { None, GeneratedScript, Html };

		private OutputType Output = OutputType.GeneratedScript;

		private HtmlPage GeneratePage(string script) {
			WebClient client = new WebClient();
			try {
				var html =
@"<html>
	<head>
		<title>Test</title>
	</head>
	<body>
		<script type=""text/javascript"">" + Environment.NewLine + Common.SSMscorlibScript + @"</script>
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
			else {
				return result;
			}
		}

		protected object ExecuteScript(string generatedScript, string scriptToReturn) {
			var page = GeneratePage(generatedScript);
			var result = page.executeJavaScript(scriptToReturn).getJavaScriptResult();
			return ConvertResult(result);
		}

		protected object ExecuteCSharp(string source, string methodName) {
			var sourceFile = new MockSourceFile("file.cs", source);
			var nc = new MetadataImporter.ScriptSharpMetadataImporter(false);
            var er = new MockErrorReporter(true);
			PreparedCompilation compilation = null;
			var rtl = new ScriptSharpRuntimeLibrary(nc, tr => Utils.CreateJsTypeReferenceExpression(tr.Resolve(compilation.Compilation).GetDefinition(), nc));
            var compiler = new Saltarelle.Compiler.Compiler.Compiler(nc, rtl, er);

            er.AllMessages.Should().BeEmpty("Compile should not generate errors");

            compilation = compiler.CreateCompilation(new[] { sourceFile }, new[] { Common.SSMscorlib });
			var compiledTypes = compiler.Compile(compilation);

			var js = new OOPEmulator.ScriptSharpOOPEmulator(nc).Rewrite(compiledTypes, compilation.Compilation);
			js = new GlobalNamespaceReferenceImporter().ImportReferences(js);

			string script = string.Join("", js.Select(s => OutputFormatter.Format(s, allowIntermediates: false)));

			if (Output == OutputType.GeneratedScript)
				Console.WriteLine(script);

			int lastDot = methodName.LastIndexOf(".", System.StringComparison.Ordinal);
			var type = ReflectionHelper.ParseReflectionName(methodName.Substring(0, lastDot)).Resolve(compilation.Compilation).GetDefinition();
			var method = type.Methods.Single(m => m.Name == methodName.Substring(lastDot + 1));
			string scriptMethod = nc.GetTypeSemantics(type).Name + "." + nc.GetMethodSemantics(method).Name;

			return ExecuteScript(script, scriptMethod + "()");
		}
	}
}
