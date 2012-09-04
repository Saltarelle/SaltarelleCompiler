using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class NullableTests : RuntimeLibraryTestBase {
		[Test]
		public void UnboxingValueOfWrongTypeThrowsAnException() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	static bool DoesItThrow(Action a) {
		try {
			a();
			return false;
		}
		catch {
			return true;
		}
	}

	public static bool M() {
		return DoesItThrow(() => {
			object o = ""x"";
			int x = (int)o;
		});
	}
}", "C.M");
			Assert.That(result, Is.True);
		}
	}
}
