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
				File.WriteAllText("File.cs", @"using System.Collections; public class C { public JsDictionary M() { return null; } }");
				var options = new CompilerOptions {
					References = { new Reference(Common.SSMscorlibPath) },
					SourceFiles = { "File.cs" },
					OutputAssemblyPath = "File.dll",
				};
				var driver = new CompilerDriver();
				driver.Compile(options);
			}
			finally {
				File.Delete("File.cs");
				File.Delete("File.dll");
			}
		}
	}
}
