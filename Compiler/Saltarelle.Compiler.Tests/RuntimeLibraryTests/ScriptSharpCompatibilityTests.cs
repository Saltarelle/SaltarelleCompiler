using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class ScriptSharpCompatibilityTests : RuntimeLibraryTestBase {
		[Test]
		public void OmitDowncastsCausesDowncastsToBeOmitted() {
			AssertSourceCorrect(@"
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
			AssertSourceCorrect(@"
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
