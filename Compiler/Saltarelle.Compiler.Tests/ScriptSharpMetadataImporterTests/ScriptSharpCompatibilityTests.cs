using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporterTests {
	[TestFixture]
	public class ScriptSharpCompatibilityTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void OmitNullableChecksWorks() {
			Prepare("using System.Runtime.CompilerServices; class C {}");
			Assert.That(Metadata.OmitNullableChecks, Is.False);
			Prepare("using System.Runtime.CompilerServices; [assembly: ScriptSharpCompatibility] class C {}");
			Assert.That(Metadata.OmitNullableChecks, Is.False);
			Prepare("using System.Runtime.CompilerServices; [assembly: ScriptSharpCompatibility(OmitNullableChecks = false)] class C {}");
			Assert.That(Metadata.OmitNullableChecks, Is.False);
			Prepare("using System.Runtime.CompilerServices; [assembly: ScriptSharpCompatibility(OmitNullableChecks = true)] class C {}");
			Assert.That(Metadata.OmitNullableChecks, Is.True);
		}

		[Test]
		public void OmitDowncastsWorks() {
			Prepare("using System.Runtime.CompilerServices; class C {}");
			Assert.That(Metadata.OmitDowncasts, Is.False);
			Prepare("using System.Runtime.CompilerServices; [assembly: ScriptSharpCompatibility] class C {}");
			Assert.That(Metadata.OmitDowncasts, Is.False);
			Prepare("using System.Runtime.CompilerServices; [assembly: ScriptSharpCompatibility(OmitDowncasts = false)] class C {}");
			Assert.That(Metadata.OmitDowncasts, Is.False);
			Prepare("using System.Runtime.CompilerServices; [assembly: ScriptSharpCompatibility(OmitDowncasts = true)] class C {}");
			Assert.That(Metadata.OmitDowncasts, Is.True);
		}
	}
}
