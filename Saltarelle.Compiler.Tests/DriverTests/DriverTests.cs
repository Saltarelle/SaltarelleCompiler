using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ICSharpCode.NRefactory;
using Mono.Cecil;
using NUnit.Framework;
using Saltarelle.Compiler.Driver;
using System.Xml.XPath;

namespace Saltarelle.Compiler.Tests.DriverTests {
	[TestFixture]
	public class DriverTests {
		private void UsingFiles(Action a, params string[] files) {
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
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = y; } }");
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
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 103 && m.File == Path.GetFullPath("File.cs") && m.Location == new TextLocation(1, 45) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.False, "Assembly should not be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.False, "Script should not be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void CompileWarningsAreReportedButFilesAreGenerated() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = 0; } }");
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
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Warning && m.Code == 219 && m.File == Path.GetFullPath("File.cs") && m.Location == new TextLocation(1, 41) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void GlobalWarningsAsErrorsWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = 0; } }");
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
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 219 && m.File == Path.GetFullPath("File.cs") && m.Location == new TextLocation(1, 41) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.False, "Assembly should not be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.False, "Script should not be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void SpecificWarningsAsErrorsWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = 0; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					WarningsAsErrors   = { 219 }
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 219 && m.File == Path.GetFullPath("File.cs") && m.Location == new TextLocation(1, 41) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.False, "Assembly should not be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.False, "Script should not be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void SpecificWarningsNotAsErrorsWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = 0; } }");
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
				Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Warning && m.Code == 219 && m.File == Path.GetFullPath("File.cs") && m.Location == new TextLocation(1, 41) && m.Format != null && m.Args.Length == 0));
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.True, "Assembly should be written");
				Assert.That(File.Exists(Path.GetFullPath("Test.js")), Is.True, "Script should be written");
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void ChangingTheWarningLevelWorks() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"public class C1 { public void M() { var x = 0l; } }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath) },
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
					References         = { new Reference(Common.SSMscorlibPath) },
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
					References         = { new Reference(Common.SSMscorlibPath) },
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
					References         = { new Reference(Common.SSMscorlibPath) },
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
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath) },
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
			}, "File.cs", "Test.dll", "Test.js");

			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath) },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					MinimizeScript     = true,
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.ReadAllText(Path.GetFullPath("Test.js")).Contains("Class1"), Is.False);
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void TheAssemblyNameIsCorrect() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath) },
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

				var asm = AssemblyDefinition.ReadAssembly(Path.GetFullPath("MyOutputAssembly.dll"));
				Assert.That(asm.Name.Name, Is.EqualTo("MyOutputAssembly"));
			}, "File.cs", "MyOutputAssembly.dll", "Test.js", "Test.xml");
		}

		[Test]
		public void TheOutputFileNamesAreTakenFromTheFirstSourceFileIfNotSpecified() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("FirstFile.cs"), @"class C1 { public void M() {} }");
				File.WriteAllText(Path.GetFullPath("SecondFile.cs"), @"class C2 { public void M() {} }");
				var options = new CompilerOptions {
					References  = { new Reference(Common.SSMscorlibPath) },
					SourceFiles = { Path.GetFullPath("FirstFile.cs"), Path.GetFullPath("SecondFile.cs") },
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(result, Is.True);
				Assert.That(File.Exists(Path.GetFullPath("FirstFile.dll")), Is.True);
				Assert.That(File.Exists(Path.GetFullPath("FirstFile.js")), Is.True);

				var asm = AssemblyDefinition.ReadAssembly(Path.GetFullPath("FirstFile.dll"));
				Assert.That(asm.Name.Name, Is.EqualTo("FirstFile"));
			}, "FirstFile.cs", "SecondFile.cs", "FirstFile.dll", "FirstFile.js");
		}

		[Test]
		public void NotSpecifyingAnyFilesToCompileIsAnError() {
			UsingFiles(() => {
				var options = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath) },
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
					References         = { new Reference(Common.SSMscorlibPath) },
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
					References         = { new Reference(Common.SSMscorlibPath) },
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
					References         = { new Reference(Common.SSMscorlibPath) },
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
					References         = { new Reference(Common.SSMscorlibPath) },
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
					References         = { new Reference(Common.SSMscorlibPath) },
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
				File.Copy(Common.SSMscorlibPath, "mscorlib.dll");
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
				File.Copy(Common.SSMscorlibPath, "mscorlib.dll");
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
		public void AssemblyThatCanNotBeLocatedCausesError7998() {
			UsingFiles(() => {
				File.WriteAllText(Path.GetFullPath("File.cs"), @"class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath), new Reference("MyNonexistentAssembly") },
					SourceFiles        = { Path.GetFullPath("File.cs") },
					OutputAssemblyPath = Path.GetFullPath("Test.dll"),
					OutputScriptPath   = Path.GetFullPath("Test.js"),
					MinimizeScript     = false,
				};
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);
				var result = driver.Compile(options);

				Assert.That(er.AllMessages, Has.Count.EqualTo(1));
				Assert.That(er.AllMessages.Any(m => m.Code == 7998 && (string)m.Args[0] == "MyNonexistentAssembly"));

				Assert.That(result, Is.False);
				Assert.That(File.Exists(Path.GetFullPath("Test.dll")), Is.False);
			}, "File.cs", "Test.dll", "Test.js");
		}

		[Test]
		public void UsingAliasedReferenceCausesError7997() {
			UsingFiles(() => {
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);

				File.WriteAllText(Path.GetFullPath("Ref.cs"), @"public class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath) },
					SourceFiles        = { Path.GetFullPath("Ref.cs") },
					OutputAssemblyPath = Path.GetFullPath("Ref.dll"),
					OutputScriptPath   = Path.GetFullPath("Ref.js"),
				};
				bool result = driver.Compile(options);
				Assert.That(result, Is.True);
				Assert.That(er.AllMessages, Is.Empty);

				File.WriteAllText(Path.GetFullPath("Out.cs"), @"extern alias myalias; class Class2 : myalias::Class1 { public void M2() {} }");
				options = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath), new Reference(Path.GetFullPath("Ref.dll"), alias: "myalias") },
					SourceFiles        = { Path.GetFullPath("Out.cs") },
					OutputAssemblyPath = Path.GetFullPath("Out.dll"),
					OutputScriptPath   = Path.GetFullPath("Out.js"),
				};

				result = driver.Compile(options);
				Assert.That(result, Is.False);
				Assert.That(er.AllMessages.Single().Code, Is.EqualTo(7997));

			}, "Ref.cs", "Ref.dll", "Ref.js", "Out.cs", "Out.dll", "Out.js");
		}

		[Test]
		public void ReferenceInAdditionalLibPathCanBeLocated() {
			UsingFiles(() => {
				Directory.CreateDirectory("MyAdditionalReferencePath");
				var er = new MockErrorReporter();
				var driver = new CompilerDriver(er);

				File.WriteAllText(Path.GetFullPath("MyAdditionalReferencePath\\Ref.cs"), @"public class Class1 { public void M() {} }");
				var options = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath) },
					SourceFiles        = { Path.GetFullPath("MyAdditionalReferencePath\\Ref.cs") },
					OutputAssemblyPath = Path.GetFullPath("MyAdditionalReferencePath\\Ref.dll"),
					OutputScriptPath   = Path.GetFullPath("MyAdditionalReferencePath\\Ref.js"),
				};
				bool result = driver.Compile(options);
				Assert.That(result, Is.True);
				Assert.That(er.AllMessages, Is.Empty);

				File.WriteAllText(Path.GetFullPath("Out.cs"), @"class Class2 : Class1 { public void M2() {} }");
				options = new CompilerOptions {
					References         = { new Reference(Common.SSMscorlibPath), new Reference("Ref.dll") },
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
	}
}
