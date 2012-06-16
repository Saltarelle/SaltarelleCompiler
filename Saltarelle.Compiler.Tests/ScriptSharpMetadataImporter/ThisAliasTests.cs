using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class ThisAliasTests {
		[Test]
		public void IsDollarThisIfNotMinimizing() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			Assert.That(md.ThisAlias, Is.EqualTo("$this"));
		}

		[Test]
		public void IsDollarUnderscoreIfMinimizing() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			Assert.That(md.ThisAlias, Is.EqualTo("$_"));
		}
	}
}
