using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.Driver;

namespace Saltarelle.Compiler.Tests.Exe {
	[TestFixture]
	public class CommandLineArgumentTests {
		private void RunTest(string[] args, Action<CompilerOptions> optionsChecker, Action<string> errorChecker) {
			var infoWriter  = new StringWriter();
			var errorWriter = new StringWriter();
			var options = Program.ParseOptions(args, infoWriter, errorWriter);
			string errors = errorWriter.ToString();
			if (optionsChecker != null) {
				Assert.That(options, Is.Not.Null);
				Assert.That(errors, Is.EqualTo(""));
				optionsChecker(options);
			}
			else {
				Assert.That(options, Is.Null);
				Assert.That(errors, Is.Not.EqualTo(""));
				errorChecker(errors);
			}
		}

		private void ExpectSuccess(string[] args, Action<CompilerOptions> checker) {
			RunTest(args, checker, null);
		}

		private void ExpectError(string[] args, Action<string> checker) {
			RunTest(args, null, checker);
		}

		[Test]
		public void SimplestCommandLineReturnsTheExpectedOptions() {
			ExpectSuccess(new [] { "File1.cs" }, options => {
				Assert.That(options.AdditionalLibPaths, Is.Empty);
				Assert.That(options.MinimizeScript, Is.True);
				Assert.That(options.DefineConstants, Is.Empty);
				Assert.That(options.DisabledWarnings, Is.Empty);
				Assert.That(options.DocumentationFile, Is.Null);
				Assert.That(options.OutputAssemblyPath, Is.Null);
				Assert.That(options.OutputScriptPath, Is.Null);
				Assert.That(options.References, Is.Empty);
				Assert.That(options.SourceFiles, Is.EqualTo(new[] { "File1.cs" }));
				Assert.That(options.TreatWarningsAsErrors, Is.False);
				Assert.That(options.WarningLevel, Is.EqualTo(4));
				Assert.That(options.WarningsAsErrors, Is.Empty);
				Assert.That(options.WarningsNotAsErrors, Is.Empty);
			});
		}

		[Test]
		public void InputFilesAndOutputFileNamesWork() {
			ExpectSuccess(new[] { "/outasm:MyAssembly.dll", "/outscript:MyScript.dll", "/doc:MyDocFile.xml", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.OutputAssemblyPath, Is.EqualTo("MyAssembly.dll"));
				Assert.That(options.OutputScriptPath, Is.EqualTo("MyScript.dll"));
				Assert.That(options.DocumentationFile, Is.EqualTo("MyDocFile.xml"));
				Assert.That(options.SourceFiles, Is.EqualTo(new[] { "MyFile1.cs", "MyFile2.cs" }));
			});
		}

		[Test]
		public void DefineConstantsWork() {
			ExpectSuccess(new[] { "/define:MY_SYMBOL1;MY_SYMBOL2", "/define:MY_SYMBOL3;MY_SYMBOL2", "/define:MY_SYMBOL4", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.DefineConstants, Is.EquivalentTo(new[] { "MY_SYMBOL1", "MY_SYMBOL2", "MY_SYMBOL3", "MY_SYMBOL4" }));
				Assert.That(options.SourceFiles, Is.EqualTo(new[] { "MyFile1.cs", "MyFile2.cs" }));
			});
		}

		[Test]
		public void AdditionalLibPathsWork() {
			ExpectSuccess(new[] { @"/lib:C:\Some\Path\1,C:\Some\Other\Path", @"/lib:Some\Relative\Path,C:\Some\Other\Path", @"/lib:LastPath", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.AdditionalLibPaths, Is.EquivalentTo(new[] { @"C:\Some\Path\1", @"C:\Some\Other\Path", @"Some\Relative\Path", @"LastPath" }));
				Assert.That(options.SourceFiles, Is.EqualTo(new[] { "MyFile1.cs", "MyFile2.cs" }));
			});
		}

		[Test]
		public void ReferencesWork() {
			ExpectSuccess(new[] { @"/reference:SomeReference1", @"/reference:Some\Relative\Path\SomeOtherReference", @"/reference:somealias=AliasedReference", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.References.Select(r => new { r.Alias, r.Filename }).ToList(), Is.EquivalentTo(new[] { new { Alias = (string)null, Filename = "SomeReference1" }, new { Alias = (string)null, Filename = @"Some\Relative\Path\SomeOtherReference" }, new { Alias = "somealias", Filename = "AliasedReference" } }));
				Assert.That(options.SourceFiles, Is.EqualTo(new[] { "MyFile1.cs", "MyFile2.cs" }));
			});

			ExpectSuccess(new[] { @"/reference:SomeReference1,Some\Relative\Path\SomeOtherReference,somealias=AliasedReference", @"/reference:OtherReference", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.References.Select(r => new { r.Alias, r.Filename }).ToList(), Is.EquivalentTo(new[] { new { Alias = (string)null, Filename = "SomeReference1" }, new { Alias = (string)null, Filename = @"Some\Relative\Path\SomeOtherReference" }, new { Alias = "somealias", Filename = "AliasedReference" }, new { Alias = (string)null, Filename = "OtherReference" } }));
				Assert.That(options.SourceFiles, Is.EqualTo(new[] { "MyFile1.cs", "MyFile2.cs" }));
			});
		}

		[Test]
		public void DebugActsAsDoNotMinimize() {
			ExpectSuccess(new[] { @"/debug", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.MinimizeScript, Is.False);
			});

			ExpectSuccess(new[] { @"/debug+", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.MinimizeScript, Is.False);
			});

			ExpectSuccess(new[] { @"/debug-", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.MinimizeScript, Is.True);
			});
		}

		[Test]
		public void WarningLevelsWork() {
			for (int i = 0; i <= 4; i++) {
				ExpectSuccess(new[] { string.Format(CultureInfo.InvariantCulture, "/warn:{0}", i), "MyFile1.cs", "MyFile2.cs" }, options => {
					Assert.That(options.WarningLevel, Is.EqualTo(i));
				});
			}

			ExpectError(new[] { "/warn:not-a-number" }, error => {
				Assert.That(error.Contains("not-a-number"));
			});

			ExpectError(new[] { "/warn:5" }, error => {
				Assert.That(error, Is.Not.Empty);
			});
		}

		[Test]
		public void DisabledWarningsWork() {
			ExpectSuccess(new[] { "/nowarn:123,145", "/nowarn:158,123,654", "/nowarn:78", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.DisabledWarnings, Is.EquivalentTo(new[] { 123, 145, 158, 654, 78 }));
				Assert.That(options.SourceFiles, Is.EqualTo(new[] { "MyFile1.cs", "MyFile2.cs" }));
			});

			ExpectError(new[] { "/nowarn:142,not-a-number,234" }, error => {
				Assert.That(error.Contains("not-a-number"));
			});
		}

		[Test]
		public void WarningsAsErrorsWorks() {
			ExpectSuccess(new[] { "/warnaserror", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.TreatWarningsAsErrors, Is.True);
			});

			ExpectSuccess(new[] { "/warnaserror+", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.TreatWarningsAsErrors, Is.True);
			});

			ExpectSuccess(new[] { "/warnaserror-", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.TreatWarningsAsErrors, Is.False);
			});

			ExpectSuccess(new[] { "/warnaserror+:234,745,364", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.TreatWarningsAsErrors, Is.False);
				Assert.That(options.WarningsAsErrors, Is.EquivalentTo(new[] { 234, 745, 364 }));
			});

			ExpectSuccess(new[] { "/warnaserror:234,745,364", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.TreatWarningsAsErrors, Is.False);
				Assert.That(options.WarningsAsErrors, Is.EquivalentTo(new[] { 234, 745, 364 }));
			});

			ExpectSuccess(new[] { "/warnaserror-:234,745,364", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.TreatWarningsAsErrors, Is.False);
				Assert.That(options.WarningsAsErrors, Is.Empty);
			});

			ExpectSuccess(new[] { "/warnaserror", "/warnaserror-:234,745,364", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.TreatWarningsAsErrors, Is.True);
				Assert.That(options.WarningsNotAsErrors, Is.EquivalentTo(new[] { 234, 745, 364 }));
			});

			ExpectError(new[] { "/warnaserror:142,not-a-number,234" }, error => {
				Assert.That(error.Contains("not-a-number"));
			});

			ExpectError(new[] { "/warnaserror-:142,not-a-number,234" }, error => {
				Assert.That(error.Contains("not-a-number"));
			});
		}

		[Test]
		public void SigningWorks() {
			ExpectSuccess(new[] { "/keyfile:MyKeyFile.snk", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.KeyFile, Is.EqualTo("MyKeyFile.snk"));
			});

			ExpectSuccess(new[] { "/keycontainer:MyKeyContainer", "MyFile1.cs", "MyFile2.cs" }, options => {
				Assert.That(options.KeyContainer, Is.EqualTo("MyKeyContainer"));
			});
		}

		[Test]
		public void EmptyCommandLineShowsTheHelpMessage() {
			var errorWriter = new StringWriter();
			var infoWriter = new StringWriter();

			var result = Program.ParseOptions(new string[0], infoWriter, errorWriter);
			Assert.That(result, Is.Null);
			Assert.That(infoWriter.ToString().Contains(Program.OptionsText));
		}

		[Test]
		public void HelpWorks() {
			var errorWriter = new StringWriter();
			var infoWriter = new StringWriter();

			var result = Program.ParseOptions(new[] { "/?", "File.cs" }, infoWriter, errorWriter);
			Assert.That(result, Is.Null);
			Assert.That(infoWriter.ToString().Contains(Program.OptionsText));

			errorWriter = new StringWriter();
			infoWriter = new StringWriter();

			result = Program.ParseOptions(new[] { "/help", "File.cs" }, infoWriter, errorWriter);
			Assert.That(result, Is.Null);
			Assert.That(infoWriter.ToString().Contains(Program.OptionsText));
		}
	}
}
