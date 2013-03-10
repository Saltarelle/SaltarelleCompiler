using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Newtonsoft.Json;
using System.Linq;

namespace CoreLib.Tests
{
	public abstract class CoreLibTestBase {
		public class QUnitTest {
			public string module;
			public string name;
			public int failed;
			public int passed;
			public int total;
		}

		public class QUnitFailure {
			public string module;
			public string test;
			public dynamic expected;
			public dynamic actual;
			public string message;
			public string source;
		}

		public class QUnitOutput {
			public List<QUnitTest> tests;
			public List<QUnitFailure> failures;
		}

		protected virtual string TestClassName {
			get { return "CoreLib.TestScript." + GetType().Name; }
		}

		//[Test, Ignore("Not a real test")]
		public void WriteThePage() {
			var html =
@"<html>
	<head>
		<title>Test</title>
		<link rel=""stylesheet"" href=""file://" + Path.GetFullPath("qunit-1.9.0.css").Replace("\\", "/") + @"""/>
	</head>
	<body>
		<script type=""text/javascript"" src=""file://" + Path.GetFullPath("mscorlib.js").Replace("\\", "/") + @"""></script>
		<script type=""text/javascript"" src=""file://" + Path.GetFullPath("qunit-1.9.0.js").Replace("\\", "/") + @"""></script>
		<script type=""text/javascript"" src=""file://" + Path.GetFullPath("SimplePromise.js").Replace("\\", "/") + @"""></script>
		<script type=""text/javascript"" src=""file://" + Path.GetFullPath("CoreLib.TestScript.js").Replace("\\", "/") + @"""></script>
		<div id=""qunit""></div>
		<script type=""text/javascript"">(new " + TestClassName + @"()).runTests();</script>
	</body>
</html>
";
			Console.Write(html);
		}

		[TestCaseSource("PerformTest")]
		public void Outcome(bool pass, string errorMessage) {
			if (!pass)
				Assert.Fail(errorMessage);
		}

		public IEnumerable<TestCaseData> PerformTest() {
			string filename = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString("N") + ".js");
			try {
				File.WriteAllText(filename, "(new " + TestClassName + @"()).runTests();");
				var p = Process.Start(new ProcessStartInfo { FileName = Path.GetFullPath("runner/node.exe"), Arguments = "run-tests.js \"" + filename + "\"", WorkingDirectory = Path.GetFullPath("runner"), RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true });
				var output = JsonConvert.DeserializeObject<QUnitOutput>(p.StandardOutput.ReadToEnd());
				var result = new List<TestCaseData>();
				foreach (var t in output.tests) {
					TestCaseData d;
					if (t.failed == 0) {
						d = new TestCaseData(true, null);
					}
					else {
						var failures = output.failures.Where(f => f.module == t.module && f.test == t.name).ToList();
						string errorMessage = string.Join("\n", failures.Select(f => f.message + (f.expected != null ? ", expected: " + f.expected.ToString() : "") + (f.actual != null ? ", actual: " + f.actual.ToString() : "")));
						if (errorMessage == "")
							errorMessage = "Failed";
						d = new TestCaseData(false, errorMessage);
					}
					d.SetName((t.module != "CoreLib.TestScript" ? t.module + ": " : "") + t.name);
					result.Add(d);
				}
				p.Close();
				return result;
			}
			catch (Exception ex) {
				return new[] { new TestCaseData(false, ex.Message).SetName(ex.Message) };
			}
			finally {
				try { File.Delete(filename); } catch {}
			}
		}
	}
}
