using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ICSharpCode.NRefactory;
using IKVM.Reflection;
using NUnit.Framework;
using Saltarelle.Compiler.Driver;
using System.Xml.XPath;

namespace Saltarelle.Compiler.Tests.DriverTests {
	[TestFixture]
	public class DriverTests {
		private static void UsingAssembly(string path, Action<Assembly> action) {
			using (var universe = new Universe()) {
				action(universe.LoadFile(path));
			}
		}

		private static void UsingFiles(Action a, params string[] files) {
			try {
				foreach (var f in files) {
					var fn = Path.GetFullPath(f);
					if (Directory.Exists(f))
						Directory.Delete(fn, true);
					else
						File.Delete(fn);
				}
				a();
			}
			finally {
				foreach (var f in files) {
					try {
						var fn = Path.GetFullPath(f);
						if (Directory.Exists(f))
							Directory.Delete(fn, true);
						else
							File.Delete(fn);
					}
					catch {}
				}
			}
		}

		[Test]
		public void SimpleCompilationWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File1.cs"), @"using System.Collections; public class C1 { public JsDictionary M() { return null; } }");
				File.WriteAllText(Path.GetFullPath("File2.cs"), @"using System.Collections; public class C2 { public JsDictionary M() { return null; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File1.cs"), Path.GetFullPath("File2.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var driver = new CompilerDriver(new MockErrorReporter());
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
			}, "File1.cs", "File2.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CompileErrorsAreReportedAndCauseFilesNotToBeGenerated() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = y; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 103 && m.Region.FileName == Path.GetFullPath("File.cs") && m.Region.Begin == new TextLocation(1, 45) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.False, "Assembly should not be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.False, "Script should not be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CompileWarningsAreReportedButFilesAreGenerated() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = 0; } }");
				var options            = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Warning && m.Code == 219 && m.Region.FileName == Path.GetFullPath("File.cs") && m.Region.Begin == new TextLocation(1, 41) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void GlobalWarningsAsErrorsWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = 0; } }");
				var options = new CompilerOptions {
					References            = { new Reference(Common.MscorlibPath) },
					SourceFiles           = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath    = Path.GetFullPath("Test.dll"),
					OutputScriptPath      = Path.GetFullPath("Test.js"),
					TreatWarningsAsErrors = true
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 219 && m.Region.FileName == Path.GetFullPath("File.cs") && m.Region.Begin == new TextLocation(1, 41) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.False, "Assembly should not be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.False, "Script should not be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void SpecificWarningsAsErrorsWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = 0; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					WarningsAsErrors   = { 219 }
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 219 && m.Region.FileName == Path.GetFullPath("File.cs") && m.Region.Begin == new TextLocation(1, 41) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.False, "Assembly should not be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.False, "Script should not be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void SpecificWarningsNotAsErrorsWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = 0; } }");
				var options = new CompilerOptions {
					References            = { new Reference(Common.MscorlibPath) },
					SourceFiles           = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath    = Path.GetFullPath("Test.dll"),
					OutputScriptPath      = Path.GetFullPath("Test.js"),
					TreatWarningsAsErrors = true,
					WarningsNotAsErrors   = { 219 }
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Warning && m.Code == 219 && m.Region.FileName == Path.GetFullPath("File.cs") && m.Region.Begin == new TextLocation(1, 41) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void ChangingTheWarningLevelWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = 0l; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(er.AllMessages.Select(m => m.Code).ToList(), Is.EquivalentTo(new[] { 219, 78 }));
			}, "File.cs", "Test.dll", "Test.js");

			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = 0l; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					WarningLevel       = 3,
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(er.AllMessages.Select(m => m.Code), Is.EqualTo(new[] { 219 }));
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void DisabledWarningsWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = 0l; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					DisabledWarnings   = { 78 },
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(er.AllMessages.Select(m => m.Code).ToList(), Is.EquivalentTo(new[] { 219 }));
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void ConditionalSymbolsWork() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"),
@"
public class C1 {
	public void M() {
#if MY_SYMBOL
		var x = ""$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$"";
#endif
	}
}");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					DefineConstants    = { "MY_SYMBOL" }
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(er.AllMessages.Select(m => m.Code).ToList(), Is.EquivalentTo(new[] { 219 }));                              // Verify that the symbol was passed to mcs.
				Assert.That(File.ReadAllText(Path.GetFullPath("Test.js")).Contains("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$"));    // Verify that the symbol was passed to the script compiler.
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void MinimizeScriptWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M(int someVariable) {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					MinimizeScript     = false,
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.ReadAllText(Path.GetFullPath("Test.js")).Contains("Class1"), Is.True);
				Assert.That(File.ReadAllText(Path.GetFullPath("Test.js")).Contains("someVariable"), Is.True);
				Assert.That(File.ReadAllText(Path.GetFullPath("Test.js")).Contains(" "), Is.True);
			}, "File.cs", "Test.dll", "Test.js");

			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M(int someVariable) {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					MinimizeScript     = true,
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				var content = File.ReadAllText(Path.GetFullPath("Test.js")).Replace("\r\n", "\n");
				Assert.That(content.Contains("Class1"), Is.False);
				Assert.That(content.Contains("someVariable"), Is.False);
				Assert.That(new System.Text.RegularExpressions.Regex("(?<!var|use) ").IsMatch(content), Is.False);
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CompilingAnonymousTypeWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("Test.cs"), @"using System.Collections; public class C1 { public void M() { var o = new { someValue = 1 }; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("Test.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True, "Compilation failed with " + string.Join(Environment.NewLine, er.AllMessages.Select(m => m.FormattedMessage)));
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CompilingLiftedEqualityWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("Test.cs"), @"public class C1 { public void M() { int? i1 = null; int i2 = 0; bool b = (i1 == i2); } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("Test.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True, "Compilation failed with " + string.Join(Environment.NewLine, er.AllMessages.Select(m => m.FormattedMessage)));
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CanInitializeListWithCollectionInitializer() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("Test.cs"), @"using System.Collections.Generic; public class C1 { public void M() { var l = new List<int> { 1 }; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("Test.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True, "Compilation failed with " + string.Join(Environment.NewLine, er.AllMessages.Select(m => m.FormattedMessage)));
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CanCompileIteratorBlock() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File1.cs"), @"using System.Collections.Generic; public class C1 { public IEnumerable<int> M() { yield return 0; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File1.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var driver = new CompilerDriver(new MockErrorReporter());
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CanCompileLockStatement() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File1.cs"), @"using System.Collections.Generic; public class C1 { public void M() { lock (new object()) {} } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File1.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var driver = new CompilerDriver(new MockErrorReporter());
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CanSwitchOnString() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File1.cs"), @"
using System.Collections.Generic;
public class C1 {
	string F() { return ""X""; }
	public int M() {
		switch (F()) {
			case ""X"": return 1;
			case ""Y"": return 2;
			case ""Z"": return 3;
			default:    return 0;
		}
	}
}");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File1.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var driver = new CompilerDriver(new MockErrorReporter());
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CannotDeclareUserDefinedStructOutsideOfCorlib() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File1.cs"), @"
using System.Collections.Generic;
public struct C1 {}
");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File1.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Where(m => m.Severity == MessageSeverity.Error && m.Code == 7998 && m.Args[0].Equals("user-defined value type (struct)")), Is.Not.Empty);
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CanCompileAsyncVoidMethod() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File1.cs"), @"
using System.Threading.Tasks;
public class C1 {
	public async void M() {
		var t = new Task(() => {});
		await t;
	}
}");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File1.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CanCompileAsyncTaskNonGenericMethod() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File1.cs"), @"
using System.Threading.Tasks;
public class C1 {
	public async Task M() {
		var t = new Task(() => {});
		await t;
	}
}");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File1.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CanCompileAsyncTaskGenericMethod() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File1.cs"), @"
using System.Threading.Tasks;
public class C1 {
	public async Task<int> M() {
		var t = new Task(() => {});
		await t;
		return 0;
	}
}");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File1.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CanUseAsyncMethodAsArgumentToAnotherMethod() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File1.cs"), @"
using System;
using System.Threading.Tasks;

class Program {
	public class C {
		public void M() {}
	}

	public static void F(Action<C> a) {
	}

	public static void Main() {
		F(async (c) => {
			await Task.Run(() => {});
			c.M();
		});
	}
}
");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File1.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void TheAssemblyNameIsCorrect() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("MyOutputAssembly.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					DocumentationFile  = Path.GetFullPath("Test.xml"),
					MinimizeScript     = false,
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.xml")), Is.True);
				string doc = File.ReadAllText(Path.GetFullPath("Test.xml"));
				Assert.That(XDocument.Parse(doc).XPathSelectElement("/doc/assembly/name").Value, Is.EqualTo("MyOutputAssembly"));

				UsingAssembly(Path.GetFullPath("MyOutputAssembly.dll"), asm => {
					Assert.That(asm.GetName().Name, Is.EqualTo("MyOutputAssembly"));
				});
			}, "File.cs", "MyOutputAssembly.dll", "Test.js", "Test.xml");
		}

		[Test]
		public void TheOutputFileNamesAreTakenFromTheFirstSourceFileIfNotSpecified() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("FirstFile.cs"), @"class C1 { public void M() {} }");
				File.WriteAllText(Path.GetFullPath("SecondFile.cs"), @"class C2 { public void M() {} }");
				var options = new CompilerOptions {
					References  = { new Reference(Common.MscorlibPath) },
					SourceFiles = { Path.GetFullPath("FirstFile.cs"), Path.GetFullPath("SecondFile.cs") },
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("FirstFile.dll")), Is.True);
				Assert.That(File.Exists(Path.GetFullPath("FirstFile.js")), Is.True);

				UsingAssembly(Path.GetFullPath("FirstFile.dll"), asm => {
					Assert.That(asm.GetName().Name, Is.EqualTo("FirstFile"));
				});
			}, "FirstFile.cs", "SecondFile.cs", "FirstFile.dll", "FirstFile.js");
		}

		[Test]
		public void NotSpecifyingAnyFilesToCompileIsAnError() {
			UsingFiles(() => {
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = {},
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Where(m => m.Severity == MessageSeverity.Error), Is.Not.Empty);
			}, "Test.dll", "Test.js");
		}

		[Test]
		public void NonExistentSourceFilesAreHandledGracefully() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("ExistentFile.cs"), @"class C1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("NonExistentFile.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Where(m => m.Severity == MessageSeverity.Error && m.Format.Contains("NonExistentFile.cs")), Is.Not.Empty);
			}, "ExistentFile.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void ErrorWritingTheOutputAssemblyGivesCS7950() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("MyOutputFile.dll"),
					OutputScriptPath   = Path.GetFullPath("MyOutputFile.js"),
					DocumentationFile  = Path.GetFullPath("MyOutputFile.xml"),
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);

				bool result;
				using (File.Open(Path.GetFullPath("MyOutputFile.dll"), FileMode.Create)) {
					result = driver.Compile(options);
				}

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Any(m => m.Code == 7950 && m.Args.Length == 1));
			}, "File.cs", "MyOutputFile.dll", "MyOutputFile.js", "MyOutputFile.xml");
		}

		[Test]
		public void ErrorWritingTheOutputScriptGivesCS7951() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("MyOutputFile.dll"),
					OutputScriptPath   = Path.GetFullPath("MyOutputFile.js"),
					DocumentationFile  = Path.GetFullPath("MyOutputFile.xml"),
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);

				bool result;
				using (File.Open(Path.GetFullPath("MyOutputFile.js"), FileMode.Create)) {
					result = driver.Compile(options);
				}

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Any(m => m.Code == 7951 && m.Args.Length == 1));
			}, "File.cs", "MyOutputFile.dll", "MyOutputFile.js", "MyOutputFile.xml");
		}

		[Test]
		public void ErrorWritingTheDocumentationFileGivesCS7952() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("MyOutputFile.dll"),
					OutputScriptPath   = Path.GetFullPath("MyOutputFile.js"),
					DocumentationFile  = Path.GetFullPath("MyOutputFile.xml"),
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);

				bool result;
				using (File.Open(Path.GetFullPath("MyOutputFile.xml"), FileMode.Create)) {
					result = driver.Compile(options);
				}

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Any(m => m.Code == 7952 && m.Args.Length == 1));
			}, "File.cs", "MyOutputFile.dll", "MyOutputFile.js", "MyOutputFile.xml");
		}

		[Test]
		public void GeneratingDocumentationFileWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"/** <summary>$$$$$$$$$$$$$$$</summary>*/ class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					DocumentationFile  = Path.GetFullPath("Test.xml"),
					MinimizeScript     = false,
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.xml")), Is.True);
				string doc = File.ReadAllText(Path.GetFullPath("Test.xml"));
				Assert.That(doc, Is.StringContaining("$$$$$$$$$$$$$$$"));
			}, "File.cs", "Test.dll", "Test.js", "Test.xml");
		}

		[Test]
		public void ReferenceInTheCurrentDirectoryCanBeResolved() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M() {} }");
				File.Copy(Common.MscorlibPath, "mscorlib.dll");
				var options = new CompilerOptions {
					References         = { new Reference("mscorlib.dll") },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					MinimizeScript     = false,
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True);
			}, "File.cs", "Test.dll", "Test.js", "mscorlib.dll");
		}

		[Test]
		public void ReferenceWithoutExtensionTheCurrentDirectoryCanBeResolved() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M() {} }");
				File.Copy(Common.MscorlibPath, "mscorlib.dll");
				var options = new CompilerOptions {
					References         = { new Reference("mscorlib") },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					MinimizeScript     = false,
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True);
			}, "File.cs", "Test.dll", "Test.js", "mscorlib.dll");
		}

		[Test]
		public void AssemblyThatCanNotBeLocatedCausesError7997() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath), new Reference("MyNonexistentAssembly") },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					MinimizeScript     = false,
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(er.AllMessages, Has.Count.EqualTo(1));
				Assert.That(er.AllMessages.Any(m => m.Code == 7997 && (string)m.Args[0] == "MyNonexistentAssembly"));

				Assert.That(result, Is.False);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.False);
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void UsingAliasedReferenceCausesError7998() {
			UsingFiles(() => {
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);

				File.WriteAllText(Path.GetFullPath("Ref.cs"), @"public class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("Ref.cs") },
					OutputAssemblyPath = Path.GetFullPath("Ref.dll"),
					OutputScriptPath   = Path.GetFullPath("Ref.js"),
				};
				bool result = driver.Compile(options);
				Assert.That(result, Is.True);
				Assert.That(er.AllMessages, Is.Empty);

				File.WriteAllText(Path.GetFullPath("Out.cs"), @"extern alias myalias; class Class2 : myalias::Class1 { public void M2() {} }");
				options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath), new Reference(Path.GetFullPath("Ref.dll"), alias: "myalias") },
					SourceFiles        = { Path.GetFullPath("Out.cs") },
					OutputAssemblyPath = Path.GetFullPath("Out.dll"),
					OutputScriptPath   = Path.GetFullPath("Out.js"),
				};

				result = driver.Compile(options);
				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Single().Code, Is.EqualTo(7998));
				Assert.That(er.AllMessages.Single().Args[0], Is.EqualTo("aliased reference"));

			}, "Ref.cs", "Ref.dll", "Ref.js", "Out.cs", "Out.dll", "Out.js");
		}

		[Test]
		public void ReferenceInAdditionalLibPathCanBeLocated() {
			UsingFiles(() => {
				Directory.CreateDirectory("MyAdditionalReferencePath");
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);

				File.WriteAllText(Path.GetFullPath("MyAdditionalReferencePath/Ref.cs"), @"public class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("MyAdditionalReferencePath/Ref.cs") },
					OutputAssemblyPath = Path.GetFullPath("MyAdditionalReferencePath/Ref.dll"),
					OutputScriptPath   = Path.GetFullPath("MyAdditionalReferencePath/Ref.js"),
				};
				bool result = driver.Compile(options);
				Assert.That(result, Is.True);
				Assert.That(er.AllMessages, Is.Empty);

				File.WriteAllText(Path.GetFullPath("Out.cs"), @"class Class2 : Class1 { public void M2() {} }");
				options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath), new Reference("Ref.dll") },
					SourceFiles        = { Path.GetFullPath("Out.cs") },
					OutputAssemblyPath = Path.GetFullPath("Out.dll"),
					OutputScriptPath   = Path.GetFullPath("Out.js"),
					AdditionalLibPaths = { Path.GetFullPath("MyAdditionalReferencePath") },
				};

				result = driver.Compile(options);
				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Out.dll")), Is.True);

			}, "MyAdditionalReferencePath", "Out.cs", "Out.dll", "Out.js");
		}

		[Test]
		public void EntryPointCanBeAutomaticallyDetermined() {
			UsingFiles(() => {
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);

				File.WriteAllText(Path.GetFullPath("File1.cs"), @"public class Class1 { public static void Main() {} } public class Class2 { public static object Main() { return null; } public static void Main(int x) {} public static void Main(string[] a, int b) {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File1.cs") },
					HasEntryPoint      = true,
					DisabledWarnings   = { 28 },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				bool result = driver.Compile(options);
				Assert.That(result, Is.True);
				Assert.That(er.AllMessages, Is.Empty);

				var content = File.ReadAllText(Path.GetFullPath("Test.js"));
				Assert.That(content.Replace("\r\n", "\n").EndsWith("\t$Class1.main();\n})();\n"), Is.True);
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void EntryPointCanBeSpecified() {
			UsingFiles(() => {
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);

				File.WriteAllText(Path.GetFullPath("File1.cs"), @"namespace MyNamespace { public class SomeClass { public static void Main() {} } } public class OtherClass { public static void Main() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File1.cs") },
					HasEntryPoint      = true,
					EntryPointClass    = "MyNamespace.SomeClass",
					DisabledWarnings   = { 28 },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				bool result = driver.Compile(options);
				Assert.That(result, Is.True);
				Assert.That(er.AllMessages, Is.Empty);

				var content = File.ReadAllText(Path.GetFullPath("Test.js"));
				Assert.That(content.Replace("\r\n", "\n").EndsWith("\t$MyNamespace_SomeClass.main();\n})();\n"), Is.True);
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void SigningWorks() {
			string key = "0702000000240000525341320004000001000100BF8CF25A8FAE18A956C58C7F0484E846B1DAF18C64DDC3C04B668894E90AFB7C796F86B2926EB59548DDF82097805AE0A981C553A639A0669B39BECD22C1026A3F8E0F90E01BF6993EA18F8E2EA60F4F1B1563FDBB9F8D501A0E0736C3ACCD6BA86B6B2002D20AE83A5E62218BC2ADA819FF0B1521E56801684FA07726EB6DAAC9DF138633A3495C1687045E1B98ECAC630F4BB278AEFF7D6276A88DFFFF02D556562579E144591166595656519A0620F272E8FE1F29DC6EAB1D14319A77EDEB479C09294F0970F1293273AA6E5A8DB32DB6C156E070672F7EEA2C1111E040FB8B992329CD8572D48D9BB256A5EE0329B69ABAFB227BBEEEF402F7383DE4EDB83947AF3B87F9ED7B2A3F3F4572F871020606778C0CEF86C77ECF6F9E8A5112A5B06FA33255A1D8AF6F2401DFA6AC3220181B1BB99D79C931B416E06926DA0E21B79DA68D3ED95CBBFE513990B3BFB4419A390206B48AC93BC397183CD608E0ECA794B66AEC94521E655559B7A098711D2FFD531BED25FF797B8320E415E99F70995777243C3940AF6672976EF37D851D93F765EC0F35FE641279F14400E227A1627CDDCCE09F6B3543681544A169DC78B6AF734AFDAF2C50015E6B932E6BD913619BA04FB5BE03428EAB072C64F7743E1E9DDDADE9DCA6A1E47C648BE01D9133F7D227FAE72337E662459B6A0CA11410FA0179F22312A534B5CABE611742A11A890B1893CD0402CE01778EDC921F0D27CBC96AEE75ECB4D4E083301A843E9716BBB0AD689FDEE275321EA915FD44F696883DAF4E3CAB3D0229283ED43FB12747";
			byte[] keyBytes = Enumerable.Range(0, key.Length / 2).Select(i => Convert.ToByte(key.Substring(i * 2, 2), 16)).ToArray();

			UsingFiles(() => {
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);

				File.WriteAllBytes(Path.GetFullPath("Key.snk"), keyBytes);
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("File.dll"),
					OutputScriptPath   = Path.GetFullPath("File.js"),
					KeyFile            = Path.GetFullPath("Key.snk"),
				};

				bool result = driver.Compile(options);
				Assert.That(result, Is.True);
				Assert.That(er.AllMessages, Is.Empty);

				UsingAssembly("File.dll", asm => {
					Assert.That(asm.GetName().GetPublicKeyToken(), Is.EqualTo(new[] { 0xf5, 0xa5, 0x6d, 0x86, 0x8e, 0xa6, 0xbd, 0x2e }));
				});
			}, "Key.snk", "File.cs", "File.dll", "File.js");
		}

		private Tuple<bool, List<Message>> Compile(string source, string baseName, params string[] references) {
			var er = new MockErrorReporter();
			var driver = new CompilerDriver(er);

			File.WriteAllText(Path.GetFullPath(baseName + ".cs"), source);
			var options = new CompilerOptions { References = { new Reference(Common.MscorlibPath) },
				SourceFiles        = { Path.GetFullPath(baseName + ".cs") },
				OutputAssemblyPath = Path.GetFullPath(baseName + ".dll"),
				OutputScriptPath   = Path.GetFullPath(baseName + ".js"),
			};
			foreach (var r in references)
				options.References.Add(new Reference(Path.GetFullPath(r + ".dll")));

			bool result = driver.Compile(options);
			return Tuple.Create(result, er.AllMessages);
		}

		[Test]
		public void IndirectlyReferencedAssemblyMustBeReferenced() {
			UsingFiles(() => {
				Compile("public class Asm1 { public int M() { return 0; } }", "Asm1");
				Compile("public class Asm2 { public Asm1 M() { return null; } }", "Asm2", "Asm1");

				var result = Compile("public class Asm3 { public Asm2 M() { return null; } }", "Asm3", "Asm1", "Asm2");
				Assert.That(result.Item1, Is.True);
				Assert.That(result.Item2, Is.Empty);

				result = Compile("public class Asm4 { public Asm2 M() { return null; } }", "Asm4", "Asm2");
				Assert.That(result.Item1, Is.False);
				Assert.That(result.Item2.Count, Is.EqualTo(1));
				Assert.That(result.Item2[0].Code, Is.EqualTo(7996));
				Assert.That(result.Item2[0].Args[0], Is.EqualTo("Asm1"));
			}, (from name in new[] { "Asm1", "Asm2", "Asm3", "Asm4" } from ext in new[] { ".cs", ".dll", ".js" } select name + ext).ToArray());
		}

		[Test]
		public void GeneratedMetadataDoesNotHaveLeadingAtSignsInIdentifiers() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File1.cs"),
@"class @char {}
namespace @string {
	namespace @float {
		class @for {}
	}
}
public class C {
	public void M(int @event) {
		@event = 10;
		@int = @event;
		@short = @event;
		@double(@event);
		var t = typeof(@char);
		var t2 = typeof(@string.@float.@for);
	}
	public int @int { get; set; }
	public int @short;
	public void @double(int x) {}
	public event System.EventHandler @if;
}");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File1.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var driver = new CompilerDriver(new MockErrorReporter());
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
				UsingAssembly(Path.GetFullPath("Test.dll"), asm => {
					var types = asm.GetTypes();
					Assert.That(types.Any(t => t.Name == "char"));
					Assert.That(types.Any(t => t.FullName == "string.float.for"));
					var c = types.Single(t => t.Name == "C");
					Assert.That(c.GetProperties().Any(p => p.Name == "int"));
					Assert.That(c.GetFields().Any(p => p.Name == "short"));
					Assert.That(c.GetMethods().Any(p => p.Name == "double"));
					Assert.That(c.GetEvents().Any(p => p.Name == "if"));
				});
			}, "File1.cs", "Test.dll", "Test.js");	
		}

		[Test]
		public void ExpressionTreesWork() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("Test.cs"), @"public class C1 { public void M() { System.Linq.Expressions.Expression<System.Func<int, int, int>> e = (a, b) => a + b; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("Test.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True, "Compilation failed with " + string.Join(Environment.NewLine, er.AllMessages.Select(m => m.FormattedMessage)));
			}, "File1.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CanEmbedResources() {
			byte[] publicContent  = new byte[] { 0xB7, 0xF3, 0x36, 0x6F, 0xA3, 0x4B, 0x4B, 0x19, 0x83, 0x27, 0x1C, 0x02, 0x19, 0xCA, 0x2E, 0x2E };
			byte[] privateContent = new byte[] { 0xCB, 0xDC, 0xDB, 0x54, 0x38, 0x9E, 0x42, 0x1A, 0xAA, 0x35, 0xD8, 0x95, 0x8D, 0x97, 0xF0, 0xCF };
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("Test.cs"), @"public class C1 { public void M() {} }");
				File.WriteAllBytes(Path.GetFullPath("PublicResource.txt"), publicContent);
				File.WriteAllBytes(Path.GetFullPath("PrivateResource.txt"), privateContent);
				var options = new CompilerOptions {
					References         = { new Reference(Common.MscorlibPath) },
					SourceFiles        = { Path.GetFullPath("Test.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					EmbeddedResources  = { new EmbeddedResource(Path.GetFullPath("PublicResource.txt"), "The.Resource.Name", true), new EmbeddedResource(Path.GetFullPath("PrivateResource.txt"), "Secret.Name", false) }
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True, "Compilation failed with " + string.Join(Environment.NewLine, er.AllMessages.Select(m => m.FormattedMessage)));
				UsingAssembly("Test.dll", asm => {
					using (var res1 = asm.GetManifestResourceStream("The.Resource.Name")) {
						Assert.That(res1, Is.Not.Null, "Resource 1 not found");
						using (var ms = new MemoryStream()) {
							res1.CopyTo(ms);
							Assert.That(ms.ToArray(), Is.EqualTo(publicContent));
						}
					}
					Assert.That(asm.GetManifestResourceInfo("The.Resource.Name").__ResourceAttributes, Is.EqualTo(ResourceAttributes.Public));

					using (var res2 = asm.GetManifestResourceStream("Secret.Name")) {
						Assert.That(res2, Is.Not.Null, "Resource 2 not found");
						using (var ms = new MemoryStream()) {
							res2.CopyTo(ms);
							Assert.That(ms.ToArray(), Is.EqualTo(privateContent));
						}
					}
					Assert.That(asm.GetManifestResourceInfo("Secret.Name").__ResourceAttributes, Is.EqualTo(ResourceAttributes.Private));
				});
			}, "File1.cs", "PublicResource.txt", "PrivateResource.txt", "Test.dll", "Test.js");
		}
	}
}
