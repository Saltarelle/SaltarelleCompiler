using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;
using NUnit.Framework;
using Saltarelle.Compiler.Driver;

namespace Saltarelle.Compiler.Tests.DriverTests {
	[TestFixture]
	public class DriverTests {
		private void UsingFiles(Action a, params string[] files) {
			try {
				foreach (var f in files)
					File.Delete(Path.GetFullPath(f));
				a();
			}
			finally {
				foreach (var f in files) {
					try {
						File.Delete(Path.GetFullPath(f));
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
					References         = { new Reference(Common.SSMscorlibPath) },
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
				File.WriteAllText(Path.GetFullPath("File.cs"), @"using System.Collections; public class C1 { public void M() { var x = y; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js")
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 103 && m.File == Path.GetFullPath("File.cs") && m.Location == new TextLocation(1, 71) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.False, "Assembly should not be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.False, "Script should not be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CompileWarningsAreReportedButFilesAreGenerated() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"using System.Collections; public class C1 { public void M() { var x = 0; } }");
				var options            = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Warning && m.Code == 219 && m.File == Path.GetFullPath("File.cs") && m.Location == new TextLocation(1, 67) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should not be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should not be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void GlobalWarningsAsErrorsWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"using System.Collections; public class C1 { public void M() { var x = 0; } }");
				var options = new CompilerOptions {
					References            = { new Reference(Common.SSMscorlibPath) },
					SourceFiles           = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath    = Path.GetFullPath("Test.dll"),
					OutputScriptPath      = Path.GetFullPath("Test.js"),
					TreatWarningsAsErrors = true
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 219 && m.File == Path.GetFullPath("File.cs") && m.Location == new TextLocation(1, 67) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.False, "Assembly should not be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.False, "Script should not be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void SpecificWarningsAsErrorsWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"using System.Collections; public class C1 { public void M() { var x = 0; } }");
				var options = new CompilerOptions {
					References            = { new Reference(Common.SSMscorlibPath) },
					SourceFiles           = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath    = Path.GetFullPath("Test.dll"),
					OutputScriptPath      = Path.GetFullPath("Test.js"),
					WarningsAsErrors      = { 219 }
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 219 && m.File == Path.GetFullPath("File.cs") && m.Location == new TextLocation(1, 67) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.False, "Assembly should not be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.False, "Script should not be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void SpecificWarningsNotAsErrorsWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"using System.Collections; public class C1 { public void M() { var x = 0; } }");
				var options = new CompilerOptions {
					References            = { new Reference(Common.SSMscorlibPath) },
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
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Warning && m.Code == 219 && m.File == Path.GetFullPath("File.cs") && m.Location == new TextLocation(1, 67) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should not be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should not be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void ChangingTheWarningLevelWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"using System.Collections; public class C1 { public void M() { var x = 0l; } }");
				var options = new CompilerOptions {
					References            = { new Reference(Common.SSMscorlibPath) },
					SourceFiles           = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath    = Path.GetFullPath("Test.dll"),
					OutputScriptPath      = Path.GetFullPath("Test.js"),
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(er.AllMessages.Select(m => m.Code).ToList(), Is.EquivalentTo(new[] { 219, 78 }));
			}, "File.cs", "Test.dll", "Test.js");

			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"using System.Collections; public class C1 { public void M() { var x = 0l; } }");
				var options = new CompilerOptions {
					References            = { new Reference(Common.SSMscorlibPath) },
					SourceFiles           = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath    = Path.GetFullPath("Test.dll"),
					OutputScriptPath      = Path.GetFullPath("Test.js"),
					WarningLevel          = 3,
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
				File.WriteAllText(Path.GetFullPath("File.cs"), @"using System.Collections; public class C1 { public void M() { var x = 0l; } }");
				var options = new CompilerOptions {
					References            = { new Reference(Common.SSMscorlibPath) },
					SourceFiles           = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath    = Path.GetFullPath("Test.dll"),
					OutputScriptPath      = Path.GetFullPath("Test.js"),
					DisabledWarnings      = { 78 },
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
using System.Collections;
public class C1 {
	public void M() {
#if MY_SYMBOL
		var x = ""$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$"";
#endif
	}
}");
				var options = new CompilerOptions {
					References            = { new Reference(Common.SSMscorlibPath) },
					SourceFiles           = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath    = Path.GetFullPath("Test.dll"),
					OutputScriptPath      = Path.GetFullPath("Test.js"),
					DefineConstants       = { "MY_SYMBOL" }
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(er.AllMessages.Select(m => m.Code).ToList(), Is.EquivalentTo(new[] { 219 }));
				Assert.That(File.ReadAllText(Path.GetFullPath("Test.js")).Contains("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$"));
			}, "File.cs", "Test.dll", "Test.js");
		}
	}
}
