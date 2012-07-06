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
		private CompilerOptions ReadProject(string filename) {
			var basePath = Path.GetDirectoryName(filename);
			var opts = new CompilerOptions();

			var project = XDocument.Load(filename);
			var nsm = new XmlNamespaceManager(new NameTable());
			nsm.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003");

			opts.SourceFiles.AddRange(project.XPathSelectElements("msb:Project/msb:ItemGroup/msb:Compile", nsm).Select(item => Path.GetFullPath(Path.Combine(basePath, item.Attributes("Include").Single().Value))));
			opts.References.AddRange(project.XPathSelectElements("msb:Project/msb:ItemGroup/msb:Reference/msb:HintPath", nsm).Select(item => new Reference(Path.GetFullPath(Path.Combine(basePath, item.Value)))));
			opts.OutputAssemblyPath = Path.GetFullPath("output.dll");
			opts.OutputScriptPath   = Path.GetFullPath("output.js");

			return opts;
		}

		[Test]
		public void CanCompileMscorlib() {
			var opts = ReadProject(Path.GetFullPath(@"..\..\..\Runtime\src\Libraries\CoreLib\CoreLib.csproj"));

			try {
				var er = new MockErrorReporter();
				var d = new CompilerDriver(er);
				bool result = d.Compile(opts, false);
				Assert.That(result, Is.True);
				Assert.That(er.AllMessages, Is.Empty);
			}
			finally {
				try { File.Delete(Path.GetFullPath("output.dll")); } catch {}
				try { File.Delete(Path.GetFullPath("output.js")); } catch {}
			}
		}

		[Test, Ignore("Debugging purposes")]
		public void TestIt() {
			var opts = ReadProject(@"C:\Projects\Saltarelle\Saltarelle\SaltarelleLib\SaltarelleLib.Client\SaltarelleLib.Client.csproj");
			opts.DefineConstants.Add("CLIENT");
			try {
				var er = new MockErrorReporter();
				var d = new CompilerDriver(er);
				bool result = d.Compile(opts, false);
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
