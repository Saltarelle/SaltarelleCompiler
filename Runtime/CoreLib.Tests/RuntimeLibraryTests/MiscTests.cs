using System.Linq;
using NUnit.Framework;

namespace CoreLib.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class MiscTests {
		[Test]
		public void UpcastCharToObjectIsAnError() {
			var result = SourceVerifier.Compile(@"
public class C {
	public static void M() {
		string s = ""X"" + 'c';
	}
}", expectErrors: true);
			Assert.That(result.Item2.AllMessages.Select(m => m.Code), Is.EqualTo(new[] { 7700 }));
		}

		[Test]
		public void ComparingNullableDateTimeToNullWorks() {
			SourceVerifier.AssertSourceCorrect(@"
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
@"			var b = Date.areNotEqual(this.$f(), null);
");
		}

		[Test]
		public void CanCompileCodeWithNonAsciiCharacters() {
			SourceVerifier.AssertSourceCorrect(
@"namespace Ф {
	public class Класс {
		void Я() {
			string Щ = ""г"";
		}
	}
}",
@"(function() {
	////////////////////////////////////////////////////////////////////////////////
	// Ф.Класс
	var $Ф_Класс = function() {
	};
	$Ф_Класс.prototype = {
		$я: function() {
			var Щ = 'г';
		}
	};
	Type.registerClass(global, 'Ф.Класс', $Ф_Класс, Object);
})();
");
		}
	}
}
