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
@"			var b = !ss.staticEquals(this.$f(), null);
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
	ss.registerClass(global, 'Ф.Класс', $Ф_Класс, Object);
})();
");
		}
	
		[Test]
		public void DefaultValuesAreCorrect() {
			SourceVerifier.AssertSourceCorrect(
@"using System;
using System.Runtime.CompilerServices;

class X {}
interface I {}
[IncludeGenericArguments(true)]
class C<T1, T2> where T1 : class {
	public static bool f1;
	public static byte f2;
	public static sbyte f3;
	public static char f4;
	public static short f5;
	public static ushort f6;
	public static int f7;
	public static uint f8;
	public static long f9;
	public static ulong f10;
	public static decimal f11;
	public static float f12;
	public static double f13;
	public static string f14;
	public static object f15;
	public static X f16;
	public static C<object, int> f17;
	public static int? f18;
	public static T1 f19;
	public static T2 f20;
	public static I f21;
	public static DateTime f22;
}",
@"(function() {
	////////////////////////////////////////////////////////////////////////////////
	// C
	var $$C$2 = function(T1, T2) {
		var $type = function() {
		};
		ss.registerGenericClassInstance($type, $$C$2, [T1, T2], function() {
			return Object;
		}, function() {
			return [];
		});
		ss.makeGenericType($$C$2, [T1, T2]).$f1 = false;
		ss.makeGenericType($$C$2, [T1, T2]).$f2 = 0;
		ss.makeGenericType($$C$2, [T1, T2]).$f3 = 0;
		ss.makeGenericType($$C$2, [T1, T2]).$f4 = 0;
		ss.makeGenericType($$C$2, [T1, T2]).$f5 = 0;
		ss.makeGenericType($$C$2, [T1, T2]).$f6 = 0;
		ss.makeGenericType($$C$2, [T1, T2]).$f7 = 0;
		ss.makeGenericType($$C$2, [T1, T2]).$f8 = 0;
		ss.makeGenericType($$C$2, [T1, T2]).$f9 = 0;
		ss.makeGenericType($$C$2, [T1, T2]).$f10 = 0;
		ss.makeGenericType($$C$2, [T1, T2]).$f11 = 0;
		ss.makeGenericType($$C$2, [T1, T2]).$f12 = 0;
		ss.makeGenericType($$C$2, [T1, T2]).$f13 = 0;
		ss.makeGenericType($$C$2, [T1, T2]).$f14 = null;
		ss.makeGenericType($$C$2, [T1, T2]).$f15 = null;
		ss.makeGenericType($$C$2, [T1, T2]).$f16 = null;
		ss.makeGenericType($$C$2, [T1, T2]).$f17 = null;
		ss.makeGenericType($$C$2, [T1, T2]).$f18 = null;
		ss.makeGenericType($$C$2, [T1, T2]).$f19 = null;
		ss.makeGenericType($$C$2, [T1, T2]).$f20 = ss.getDefaultValue(T2);
		ss.makeGenericType($$C$2, [T1, T2]).$f21 = null;
		ss.makeGenericType($$C$2, [T1, T2]).$f22 = new Date(0);
		return $type;
	};
	ss.registerGenericClass(null, '$C$2', $$C$2, 2);
	////////////////////////////////////////////////////////////////////////////////
	// I
	var $$I = function() {
	};
	////////////////////////////////////////////////////////////////////////////////
	// X
	var $$X = function() {
	};
	ss.registerInterface(null, '$I', $$I, []);
	ss.registerClass(null, '$X', $$X, Object);
})();
");
		}
	}
}
