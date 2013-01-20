using NUnit.Framework;

namespace CoreLib.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class ScriptSharpCompatibilityTests {
		[Test]
		public void OmitDowncastsCausesDowncastsToBeOmitted() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
using System.Runtime.CompilerServices;
[assembly: ScriptSharpCompatibility(OmitDowncasts = true)]

public class B {}
public class D : B {}

public class C {
	private void M() {
		B b = null;
		// BEGIN
		var d = (D)b;
		// END
	}
}",
@"			var d = b;
");
		}

		[Test]
		public void OmitNullableChecksCausesDowncastsToBeOmitted() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
using System.Runtime.CompilerServices;
[assembly: ScriptSharpCompatibility(OmitNullableChecks = true)]

public class C {
	private void M() {
		int? ni = null;
		// BEGIN
		int i = (int)ni;
		// END
	}
}",
@"			var i = ni;
");
		}
	}
}
