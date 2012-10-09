using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class MiscTests : RuntimeLibraryTestBase {
		[Test]
		public void CoalesceWorks() {
			var result = ExecuteCSharp(@"
public class C {
	public static object[] M() {
		int? v1 = null, v2 = 1, v3 = 0, v4 = 2;
		string s1 = null, s2 = ""x"";
		return new object[] { v1 ?? v1, v1 ?? v2, v3 ?? v4, s1 ?? s1, s1 ?? s2 };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new object [] { null, 1, 0, null, "x" }));
		}

		[Test]
		public void QueryExpressionsWork() {
			var result = ExecuteCSharp(@"
using System.Linq;
public class C {
	public static void M() {
		string[] args = new[] { ""4"", ""5"", ""7"" };
		// BEGIN
		return (from a in args let b = int.Parse(a) let c = b + 1 select a + b.ToString() + c.ToString()).ToArray();
		// END
	}
}", "C.M", includeLinq: true);
			Assert.That(result, Is.EqualTo(new[] { "445", "556", "778" }));
		}

		[Test]
		public void UpcastCharToObjectIsAnError() {
			var result = Compile(@"
public class C {
	public static void M() {
		string s = ""X"" + 'c';
	}
}", expectErrors: true);
			Assert.That(result.Item4.AllMessages.Select(m => m.Code), Is.EqualTo(new[] { 7700 }));
		}

		[Test]
		public void ComparingNullableDateTimeToNullWorks() {
			AssertSourceCorrect(@"
using System;
public class C {
	DateTime? F() { return null; }
	
	DateTime? M() {
		// BEGIN
		bool b = F() != null;
		// END
	}	
}
",
@"		var b = Date.areNotEqual(this.$f(), null);
");
		}

		[Test]
		public void CanCompileCodeWithNonAsciiCharacters() {
			AssertSourceCorrect(
@"namespace Ф {
	public class Класс {
		void Я() {
			string Щ = ""г"";
		}
	}
}",
@"////////////////////////////////////////////////////////////////////////////////
// Ф.Класс
var $Ф_Класс = function() {
};
$Ф_Класс.prototype = {
	$я: function() {
		var Щ = 'г';
	}
};
Type.registerClass(global, 'Ф.Класс', $Ф_Класс, Object);
");
		}
	}
}
