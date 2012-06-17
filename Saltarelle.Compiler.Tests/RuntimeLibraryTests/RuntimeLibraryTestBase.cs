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
		private const bool WriteScriptsToConsole = true;

		private HtmlPage GeneratePage() {
			WebClient client = new WebClient();
			try {
				var html =
@"<html>
	<head>
		<title>Test</title>
	</head>
	<body>
		<script type=""text/javascript"">" + Common.SSMscorlibScript + @"</script>
	</body>
</html>
";
				var response = new StringWebResponse(html, new URL("http://localhost/test.htm"));
				return HTMLParser.parseHtml(response, client.getCurrentWindow());
			}
			finally {
				client.closeAllWindows();
			}
		}

		protected object ExecuteScript(string script) {
			var page = GeneratePage();
			var result = page.executeJavaScript(script).getJavaScriptResult();
			if (result is IEnumerable) {
				var l = new ArrayList();
				foreach (var i in (IEnumerable)result)
					l.Add(i);
				result = l;
			}
			return result;
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

			if (WriteScriptsToConsole)
				Console.WriteLine(script);

			int lastDot = methodName.LastIndexOf(".", System.StringComparison.Ordinal);
			var type = ReflectionHelper.ParseReflectionName(methodName.Substring(0, lastDot)).Resolve(compilation.Compilation).GetDefinition();
			var method = type.Methods.Single(m => m.Name == methodName.Substring(lastDot + 1));
			string scriptMethod = nc.GetTypeSemantics(type).Name + "." + nc.GetMethodSemantics(method).Name;

			script += script + Environment.NewLine + scriptMethod + "()";

			return ExecuteScript(script);
		}
	}
}
