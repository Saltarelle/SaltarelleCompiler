using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.Driver;

namespace Saltarelle.Compiler.Tests.DriverTests {
	[TestFixture]
	public class DriverTests {
		[Test]
		public void CanCompileSimpleFile() {
			try {
				File.WriteAllText("File1.cs", @"using System.Collections; public class C1 { public JsDictionary M() { return null; } }");
				File.WriteAllText("File2.cs", @"using System.Collections; public class C2 { public JsDictionary M() { return null; } }");
				var options = new CompilerOptions {
					References = { new Reference(Common.SSMscorlibPath) },
					SourceFiles = { Path.GetFullPath("File1.cs"), Path.GetFullPath("File2.cs") },
					OutputAssemblyPath = "Test.dll",
					OutputScriptPath = "Test.js"
				};
				var driver = new CompilerDriver();
				driver.Compile(options, new VisualStudioFormatErrorReporter(Console.Out));
			}
			finally {
				File.Delete("File1.cs");
				File.Delete("File2.cs");
				File.Delete("Test.dll");
				File.Delete("Test.js");
			}
		}
	}
}
