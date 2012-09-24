using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using com.gargoylesoftware.htmlunit;
using com.gargoylesoftware.htmlunit.html;

namespace RuntimeLibrary.Tests
{
	public abstract class TestBase {
        private static readonly Lazy<string> _mscorlibScriptLazy = new Lazy<string>(() => File.ReadAllText(@"..\..\..\..\bin\Script\mscorlib.debug.js"));
		internal static string MscorlibScript { get { return _mscorlibScriptLazy.Value; } }

        private static readonly Lazy<string> _qunitCss = new Lazy<string>(() => File.ReadAllText(@"..\..\..\Libraries\CoreLib\qunit-1.9.0.css"));
		internal static string QUnitCss { get { return _qunitCss.Value; } }

        private static readonly Lazy<string> _qunitScript = new Lazy<string>(() => File.ReadAllText(@"..\..\..\Libraries\CoreLib\qunit-1.9.0.js"));
		internal static string QUnitScript { get { return _qunitScript.Value; } }

		protected abstract IEnumerable<string> ScriptSources { get; }
		protected abstract string TestClassName { get; }

		protected HtmlPage GeneratePage(bool print = false) {
			var client = new WebClient();
			try {
				var html =
@"<html>
	<head>
		<title>Test</title>
		<style>" + QUnitCss + @"</style>
	</head>
	<body>
		<script type=""text/javascript"">" + Environment.NewLine + MscorlibScript + @"</script>
		<script type=""text/javascript"">" + Environment.NewLine + QUnitScript + @"</script>";

				foreach (var src in ScriptSources)
					html += Environment.NewLine + "<script type=\"text/javascript\">" + Environment.NewLine + src + "</script>";
		html += @"
		<div id=""qunit""></div>
		<script type=""text/javascript"">(new " + TestClassName + @"()).runTests();</script>
	</body>
</html>
";
				if (print)
					Console.Write(html);

				var tempFile = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString("N") + ".htm");
				try {
					File.WriteAllText(tempFile, html);
					var result = (HtmlPage)client.getPage("file://" + tempFile.Replace("\\", "/"));
					DateTime startTime = DateTime.Now;
					while (!result.getElementById("qunit-testresult").getTextContent().Contains("completed")) {
						System.Threading.Thread.Sleep(100);
						if ((DateTime.Now - startTime).Seconds > 3600)
							throw new Exception("Tests timed out");
					}
					return result;
				}
				finally {
					try { File.Delete(tempFile); } catch {}
				}
			}
			finally {
				client.closeAllWindows();
			}
		}

		[Test, Ignore("Not a real test")]
		public void WriteThePage() {
			GeneratePage(true);
		}

		[TestCaseSource("PerformTest")]
		public void Outcome(bool pass, string errorMessage) {
			if (!pass)
				Assert.Fail(errorMessage);
		}

		public IEnumerable<TestCaseData> PerformTest() {
			try {
				var result = new List<TestCaseData>();
				var page = GeneratePage();
				var elems = page.querySelectorAll("#qunit-tests > li");
				for (int i = 0; i < elems.getLength(); i++) {
					var elem = (HtmlElement)elems.get(i);
					bool pass = (" " + elem.getAttribute("class") + " ").Contains(" pass ");
					var categoryElem = page.querySelector("#" + elem.getId() + " .module-name");
					string category = (categoryElem != null ? categoryElem.getTextContent() : null);
					string testName = page.querySelector("#" + elem.getId() + " .test-name").getTextContent();
					string errorMessage;
					if (pass) {
						errorMessage = null;
					}
					else {
						errorMessage = "";
						var allFailures = page.querySelectorAll("#" + elem.getId() + " .fail");
						for (int j = 0, n = allFailures.getLength(); j < n; j++) {
							var failure = (HtmlElement)allFailures.get(j);

							failure.setId("__current");

							errorMessage += (errorMessage != "" ? Environment.NewLine + Environment.NewLine : "") + page.querySelector("#__current .test-message").getTextContent();
							var expected = page.querySelector("#__current .test-expected");
							if (expected != null) {
								var expectedText = expected.getTextContent();
								errorMessage += "," + (expectedText.Contains("\n") ? Environment.NewLine : " ") + expectedText;
							}
							var actual = page.querySelector("#__current .test-actual");
							if (actual != null) {
								var actualText = actual.getTextContent();
								errorMessage += "," + (actualText.Contains("\n") ? Environment.NewLine : " ") + actualText;
							}

							failure.setId("");
						}
					}
					result.Add(new TestCaseData(pass, errorMessage).SetName((category != null ? category + ": " : "") + testName));
				}
				return result;
			}
			catch (Exception ex) {
				return new[] { new TestCaseData(false, ex.Message).SetName(ex.Message) };
			}
		}
	}
}
