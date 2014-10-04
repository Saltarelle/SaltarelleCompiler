using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
			get { return GetType().FullName.Replace("CoreLib.Tests.Core.", "CoreLib.TestScript."); }
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

		private static readonly Dictionary<string, List<TestCaseData>> _testOutcomesByTypeName = new Dictionary<string, List<TestCaseData>>();

		public IEnumerable<TestCaseData> PerformTest() {
			lock (_testOutcomesByTypeName) {
				List<TestCaseData> result;
				if (_testOutcomesByTypeName.TryGetValue(GetType().FullName, out result))
					return result;

				string filename = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString("N") + ".js");
				var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && typeof(CoreLibTestBase).IsAssignableFrom(t)).ToList();
				Process process = null;
				try {
					File.WriteAllText(filename, string.Join("", types.Select(t => "QUnit.module('" + t.FullName + "'); (new " + ((CoreLibTestBase)Activator.CreateInstance(t)).TestClassName + @"()).runTests();")));
					process = Process.Start(new ProcessStartInfo { FileName = Path.GetFullPath("runner/node.exe"), Arguments = "run-tests.js \"" + filename + "\"", WorkingDirectory = Path.GetFullPath("runner"), RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true });
					var stdout = process.StandardOutput.ReadToEnd();
					var stderr = process.StandardError.ReadToEnd();

					if (!string.IsNullOrEmpty(stderr))
						throw new Exception(stderr.Trim().Replace("\r", "").Replace("\n", " "));

					foreach (var t in types)
						_testOutcomesByTypeName[t.FullName] = new List<TestCaseData>();

					var output = JsonConvert.DeserializeObject<QUnitOutput>(stdout);
					foreach (var t in output.tests) {
						List<TestCaseData> list;
						if (!_testOutcomesByTypeName.TryGetValue(t.module, out list)) {
							throw new Exception("Test module " + t.module + " is not the name of a type. The QUnit.module() method cannot be used because modules are used by the test infrastructure");
						}

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
						d.SetName(t.name);
						list.Add(d);
					}
					return _testOutcomesByTypeName[GetType().FullName];
				}
				catch (Exception ex) {
					var tc = new TestCaseData(false, ex.Message);
					tc.SetName("Failed to run tests");
					var l = new List<TestCaseData> { tc };
					foreach (var t in types) {
						_testOutcomesByTypeName[t.FullName] = l;
					}
					return l;
				}
				finally {
					try {
						if (process != null)
							process.Close();
					}
					catch {
					}
					try { File.Delete(filename); } catch {}
				}
			}
		}
	}
}
