using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler;

namespace CoreLib.Tests.Core {
	[TestFixture]
	public class TypeSystemLanguageSupportTests : CoreLibTestBase {
		[Test]
		public void ConversionToAndFromDynamicWorks() {
			SourceVerifier.AssertSourceCorrect(@"
public class C {
	private void M() {
		int i = 0;
		object o = null;
		dynamic d = null;
		// BEGIN
		d = i;
		i = d;
		d = o;
		o = d;
		// END
	}
}",
@"			d = i;
			i = ss.Nullable.unbox(ss.cast(d, ss.Int32));
			d = o;
			o = d;
");
		}

		[Test]
		public void IsOperatorWorksForImportedTypeWithTypeCheckCode() {
			SourceVerifier.AssertSourceCorrect(@"
using System.Runtime.CompilerServices;
[Imported(TypeCheckCode = ""true"")] public class C1 {}
[Imported(TypeCheckCode = ""{this}.x"")] public class C2 {}
[Imported(TypeCheckCode = ""{this}.x || {this}.y"")] public class C3 {}
[Imported(TypeCheckCode = ""{this}.x == {T}"")] public class C4<T> {}
public class C {
	private object GetO() { return null; }
	private void M() {
		object o = null;
		// BEGIN
		bool b1 = o is C1;
		bool b2 = o is C2;
		bool b3 = o is C3;
		bool b4 = GetO() is C1;
		bool b5 = GetO() is C2;
		bool b6 = GetO() is C3;
		bool b7 = o is C4<int>;
		// END
	}
}",
@"			var b1 = ss.isValue(o) && true;
			var b2 = ss.isValue(o) && o.x;
			var b3 = ss.isValue(o) && (o.x || o.y);
			var b4 = ss.isValue(this.$getO()) && true;
			var $t1 = this.$getO();
			var b5 = ss.isValue($t1) && $t1.x;
			var $t2 = this.$getO();
			var b6 = ss.isValue($t2) && ($t2.x || $t2.y);
			var b7 = ss.isValue(o) && o.x == ss.Int32;
");
		}

		[Test]
		public void AsOperatorWorksForImportedTypeWithTypeCheckCode() {
			SourceVerifier.AssertSourceCorrect(@"
using System.Runtime.CompilerServices;
[Imported(TypeCheckCode = ""true"")] public class C1 {}
[Imported(TypeCheckCode = ""{this}.x"")] public class C2 {}
[Imported(TypeCheckCode = ""{this}.x || {this}.y"")] public class C3 {}
[Imported(TypeCheckCode = ""{this}.x == {T}"")] public class C4<T> {}
public class C {
	private object GetO() { return null; }
	private void M() {
		object o = null;
		// BEGIN
		var o1 = o as C1;
		var o2 = o as C2;
		var o3 = o as C3;
		var o4 = GetO() as C1;
		var o5 = GetO() as C2;
		var o6 = GetO() as C3;
		var o7 = o as C4<int>;
		// END
	}
}",
@"			var o1 = ss.safeCast(o, ss.isValue(o) && true);
			var o2 = ss.safeCast(o, ss.isValue(o) && o.x);
			var o3 = ss.safeCast(o, ss.isValue(o) && (o.x || o.y));
			var $t1 = this.$getO();
			var o4 = ss.safeCast($t1, ss.isValue($t1) && true);
			var $t2 = this.$getO();
			var o5 = ss.safeCast($t2, ss.isValue($t2) && $t2.x);
			var $t3 = this.$getO();
			var o6 = ss.safeCast($t3, ss.isValue($t3) && ($t3.x || $t3.y));
			var o7 = ss.safeCast(o, ss.isValue(o) && o.x == ss.Int32);
");
		}

		[Test]
		public void CastWorksForImportedTypeWithTypeCheckCode() {
			SourceVerifier.AssertSourceCorrect(@"
using System.Runtime.CompilerServices;
[Imported(TypeCheckCode = ""true"")] public class C1 {}
[Imported(TypeCheckCode = ""{this}.x"")] public class C2 {}
[Imported(TypeCheckCode = ""{this}.x || {this}.y"")] public class C3 {}
[Imported(TypeCheckCode = ""{this}.x == {T}"")] public class C4<T> {}
public class C {
	private object GetO() { return null; }
	private void M() {
		object o = null;
		// BEGIN
		var o1 = (C1)o;
		var o2 = (C2)o;
		var o3 = (C3)o;
		var o4 = (C1)GetO();
		var o5 = (C2)GetO();
		var o6 = (C3)GetO();
		var o7 = (C4<int>)o;
		// END
	}
}",
@"			var o1 = ss.cast(o, ss.isValue(o) && true);
			var o2 = ss.cast(o, ss.isValue(o) && o.x);
			var o3 = ss.cast(o, ss.isValue(o) && (o.x || o.y));
			var $t1 = this.$getO();
			var o4 = ss.cast($t1, ss.isValue($t1) && true);
			var $t2 = this.$getO();
			var o5 = ss.cast($t2, ss.isValue($t2) && $t2.x);
			var $t3 = this.$getO();
			var o6 = ss.cast($t3, ss.isValue($t3) && ($t3.x || $t3.y));
			var o7 = ss.cast(o, ss.isValue(o) && o.x == ss.Int32);
");
		}

		[Test]
		public void CannotUseTheIsOperatorWithSerializableTypeWithoutTypeCheckCode() {
			var actual = SourceVerifier.Compile(@"
[System.Serializable] class C1 {}
class C {
	public void M() {
		var x = new object() is C1;
	}
}
", expectErrors: true);
			Assert.That(actual.Item2.AllMessages.Count, Is.EqualTo(1));
			Assert.That(actual.Item2.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7701 && m.FormattedMessage.Contains("'is' operator") && m.FormattedMessage.Contains("C1")));
		}

		[Test]
		public void CannotUseTheIsOperatorWithImportedTypeThatDoesNotObeyTheTypeSystemOrHaveTypeCheckCode() {
			var actual = SourceVerifier.Compile(@"
[System.Runtime.CompilerServices.Imported] class C1 {}
class C {
	public void M() {
		var x = new object() is C1;
	}
}
", expectErrors: true);
			Assert.That(actual.Item2.AllMessages.Count, Is.EqualTo(1));
			Assert.That(actual.Item2.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7701 && m.FormattedMessage.Contains("'is' operator") && m.FormattedMessage.Contains("C1")));
		}

		[Test]
		public void CannotUseTheAsOperatorWithSerializableTypeWithoutTypeCheckCode() {
			var actual = SourceVerifier.Compile(@"
[System.Serializable] class C1 {}
class C {
	public void M() {
		var x = new object() as C1;
	}
}
", expectErrors: true);
			Assert.That(actual.Item2.AllMessages.Count, Is.EqualTo(1));
			Assert.That(actual.Item2.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7702 && m.FormattedMessage.Contains("'as' operator") && m.FormattedMessage.Contains("C1")));
		}

		[Test]
		public void CannotUseTheAsOperatorWithImportedTypeThatDoesNotObeyTheTypeSystemOrHaveTypeCheckCode() {
			var actual = SourceVerifier.Compile(@"
[System.Runtime.CompilerServices.Imported] class C1 {}
class C {
	public void M() {
		var x = new object() as C1;
	}
}
", expectErrors: true);
			Assert.That(actual.Item2.AllMessages.Count, Is.EqualTo(1));
			Assert.That(actual.Item2.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7702 && m.FormattedMessage.Contains("'as' operator") && m.FormattedMessage.Contains("C1")));
		}
	}
}