using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Mono.Cecil;
using NUnit.Framework;
using System.Xml.XPath;
using Saltarelle.Compiler.Driver;

namespace Saltarelle.Compiler.Tests.EndToEndTests {
	[TestFixture]
	public class MscorlibCompilation {
		[Test]
		public void CanCompileMscorlib() {
			string basePath = Path.GetFullPath(@"..\..\..\Runtime\src\Libraries\CoreLib");

			var opts = new CompilerOptions();

			var project = XDocument.Load(Path.Combine(basePath, "CoreLib.csproj"));
			var r = new XmlNamespaceManager(new NameTable());
			r.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003");

			opts.SourceFiles.AddRange(project.XPathSelectElements("msb:Project/msb:ItemGroup/msb:Compile", r).Select(item => Path.Combine(basePath, item.Attributes("Include").Single().Value)));
			opts.OutputAssemblyPath = Path.GetFullPath("output.dll");
			opts.OutputScriptPath   = Path.GetFullPath("output.js");

			try {
				var er = new MockErrorReporter();
				var d = new CompilerDriver(er);
				bool result = d.Compile(opts);
				Assert.That(result, Is.True);
				Assert.That(er.AllMessages, Is.Empty);
			}
			finally {
				try { File.Delete(Path.GetFullPath("output.dll")); } catch {}
				try { File.Delete(Path.GetFullPath("output.js")); } catch {}
			}
		}
	}
}
