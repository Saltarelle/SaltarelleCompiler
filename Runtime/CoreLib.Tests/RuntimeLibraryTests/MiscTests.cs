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
	'use strict';
	var $asm = {};
	global.Ф = global.Ф || {};
	ss.initAssembly($asm, 'x');
	////////////////////////////////////////////////////////////////////////////////
	// Ф.Класс
	var $Ф_Класс = function() {
	};
	$Ф_Класс.__typeName = 'Ф.Класс';
	global.Ф.Класс = $Ф_Класс;
	ss.initClass($Ф_Класс, $asm, {
		$я: function() {
			var Щ = 'г';
		}
	});
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
	'use strict';
	var $asm = {};
	ss.initAssembly($asm, 'x');
	////////////////////////////////////////////////////////////////////////////////
	// C
	var $$C$2 = function(T1, T2) {
		var $type = function() {
		};
		ss.registerGenericClassInstance($type, $$C$2, [T1, T2], {}, function() {
			return null;
		}, function() {
			return [];
		});
		$type.$f1 = false;
		$type.$f2 = 0;
		$type.$f3 = 0;
		$type.$f4 = 0;
		$type.$f5 = 0;
		$type.$f6 = 0;
		$type.$f7 = 0;
		$type.$f8 = 0;
		$type.$f9 = 0;
		$type.$f10 = 0;
		$type.$f11 = 0;
		$type.$f12 = 0;
		$type.$f13 = 0;
		$type.$f14 = null;
		$type.$f15 = null;
		$type.$f16 = null;
		$type.$f17 = null;
		$type.$f18 = null;
		$type.$f19 = null;
		$type.$f20 = ss.getDefaultValue(T2);
		$type.$f21 = null;
		$type.$f22 = new Date(0);
		return $type;
	};
	$$C$2.__typeName = '$C$2';
	ss.initGenericClass($$C$2, $asm, 2);
	////////////////////////////////////////////////////////////////////////////////
	// I
	var $$I = function() {
	};
	$$I.__typeName = '$I';
	////////////////////////////////////////////////////////////////////////////////
	// X
	var $$X = function() {
	};
	$$X.__typeName = '$X';
	ss.initInterface($$I, $asm, {});
	ss.initClass($$X, $asm, {});
})();
");
		}

		[Test]
		public void CastToNamedValuesEnumIsCastToString() {
			SourceVerifier.AssertSourceCorrect(
@"using System;
using System.Runtime.CompilerServices;
[NamedValues] enum E {}
class C {
	public void M(object o) {
		// BEGIN
		E e = (E)o;
		// END
	}
}
",
@"			var e = ss.cast(o, String);
");
		}
	}
}
