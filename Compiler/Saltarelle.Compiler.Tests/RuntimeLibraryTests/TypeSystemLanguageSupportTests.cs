using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class TypeSystemLanguageSupportTests : RuntimeLibraryTestBase {
		[Test]
		public void TypeIsWorksForReferenceTypes() {
			var result = ExecuteCSharp(
@"public class C1 {}
public class C2<T> {}
public interface I1 {}
public interface I2<T1> {}
public interface I3 : I1 {}
public interface I4 {}
public interface I5<T1> : I2<T1> {}
public class D1 : C1, I1 {}
public class D2<T> : C2<T>, I2<T>, I1 {
}
public class D3 : C2<int>, I2<string> {
}
public class D4 : I3, I4 {
}
public class X1 : I1 {
}
public class X2 : X1 {
}
public enum E1 {}
public enum E2 {}

public class C {
	public static bool[] M() {
		return new[] { new object() is C1,             // false
		               new C1() is object,             // true
		               new object() is I1,             // false
		               new C1() is D1,                 // false
		               new D1() is C1,                 // true
		               new D1() is I1,                 // true
		               new D2<int>() is C2<int>,       // true
		               new D2<int>() is C2<string>,    // false
		               new D2<int>() is I2<int>,       // true
		               new D2<int>() is I2<string>,    // false
		               new D2<int>() is I1,            // true
		               new D3() is C2<string>,         // false
		               new D3() is C2<int>,            // true
		               new D3() is I2<int>,            // false
		               new D3() is I2<string>,         // true
		               new D4() is I1,                 // true
		               new D4() is I3,                 // true
		               new D4() is I4,                 // true
		               new X2() is I1,                 // true
		               new E2() is E1,                 // true
		               new E1() is int,                // true
		               new E1() is object,             // true
		               null is object,                 // false
		             };
		};
	}
}", "C.M");

			Assert.That(result, Is.EqualTo(new[] { false, true, false, false, true, true, true, false, true, false, true, false, true, false, true, true, true, true, true, true, true, true, false }));
		}

		[Test]
		public void TypeIsWorksForInt32() {
			var result = ExecuteCSharp(@"
public class C {
	public static bool[] M() {
		return new[] { null is int, new object() is int, 1.5 is int, 1 is int };
	}
}", "C.M");

			Assert.That(result, Is.EqualTo(new[] { false, false, false, true }));
		}

		[Test]
		public void TypeAsWorksForReferenceTypes() {
			var result = ExecuteCSharp(
@"public class C1 {}
public class C2<T> {}
public interface I1 {}
public interface I2<T1> {}
public interface I3 : I1 {}
public interface I4 {}
public interface I5<T1> : I2<T1> {}
public class D1 : C1, I1 {}
public class D2<T> : C2<T>, I2<T>, I1 {
}
public class D3 : C2<int>, I2<string> {
}
public class D4 : I3, I4 {
}
public class X1 : I1 {
}
public class X2 : X1 {
}
public enum E1 {}
public enum E2 {}

public class C {
	public static bool[] M() {
		return new[] { (new object() as C1) != null,             // false
		               (new C1() as object) != null,             // true
		               (new object() as I1) != null,             // false
		               (new C1() as D1) != null,                 // false
		               (new D1() as C1) != null,                 // true
		               (new D1() as I1) != null,                 // true
		               (new D2<int>() as C2<int>) != null,       // true
		               (new D2<int>() as C2<string>) != null,    // false
		               (new D2<int>() as I2<int>) != null,       // true
		               (new D2<int>() as I2<string>) != null,    // false
		               (new D2<int>() as I1) != null,            // true
		               (new D3() as C2<string>) != null,         // false
		               (new D3() as C2<int>) != null,            // true
		               (new D3() as I2<int>) != null,            // false
		               (new D3() as I2<string>) != null,         // true
		               (new D4() as I1) != null,                 // true
		               (new D4() as I3) != null,                 // true
		               (new D4() as I4) != null,                 // true
		               (new X2() as I1) != null,                 // true
		               (new E2() as E1?) != null,                // true
		               (new E1() as int) != null,                // true
		               (new E1() as object) != null,             // true
		               (null as object) != null,                 // false
		             };
		};
	}
}", "C.M");

			Assert.That(result, Is.EqualTo(new[] { false, true, false, false, true, true, true, false, true, false, true, false, true, false, true, true, true, true, true, true, true, true, false }));
		}

		[Test]
		public void TypeAsWorksForInt32() {
			var result = ExecuteCSharp(@"
public class C {
	public static bool[] M() {
		return new[] { (null as int?) != null, (new object() as int?) != null, (1.5 as int?) != null, (1 as int?) != null };
	}
}", "C.M");

			Assert.That(result, Is.EqualTo(new[] { false, false, false, true }));
		}

		[Test]
		public void CastWorksForReferenceTypes() {
			var result = ExecuteCSharp(
@"public class C1 {}
public class C2<T> {}
public interface I1 {}
public interface I2<T1> {}
public interface I3 : I1 {}
public interface I4 {}
public interface I5<T1> : I2<T1> {}
public class D1 : C1, I1 {}
public class D2<T> : C2<T>, I2<T>, I1 {
}
public class D3 : C2<int>, I2<string> {
}
public class D4 : I3, I4 {
}
public class X1 : I1 {
}
public class X2 : X1 {
}
public enum E1 {}
public enum E2 {}

public class C {
	private static bool F<T>(object arg) {
		try {
			var x = (T)arg;
			return true;
		}
		catch {
			return false;
		}
	}

	public static bool[] M() {
		return new[] { F<C1>(new object()),             // false
		               F<object>(new C1()),             // true
		               F<I1>(new object()),             // false
		               F<D1>(new C1()),                 // false
		               F<C1>(new D1()),                 // true
		               F<I1>(new D1()),                 // true
		               F<C2<int>>(new D2<int>()),       // true
		               F<C2<string>>(new D2<int>()),    // false
		               F<I2<int>>(new D2<int>()),       // true
		               F<I2<string>>(new D2<int>()),    // false
		               F<I1>(new D2<int>()),            // true
		               F<C2<string>>(new D3()),         // false
		               F<C2<int>>(new D3()),            // true
		               F<I2<int>>(new D3()),            // false
		               F<I2<string>>(new D3()),         // true
		               F<I1>(new D4()),                 // true
		               F<I3>(new D4()),                 // true
		               F<I4>(new D4()),                 // true
		               F<I1>(new X2()),                 // true
		               F<E1>(new E2()),                 // true
		               F<int>(new E1()),                // true
		               F<object>(new E1()),             // true
		               F<object>(null),                 // false
		             };
		};
	}
}", "C.M");

			Assert.That(result, Is.EqualTo(new[] { false, true, false, false, true, true, true, false, true, false, true, false, true, false, true, true, true, true, true, true, true, true, true }));
		}

		[Test]
		public void CastsWorksForInt32() {
			var result = ExecuteCSharp(@"
public class C {
	private static bool F(object arg) {
		try {
			int? x = (int?)arg;
			return true;
		}
		catch {
			return false;
		}
	}

	public static bool[] M() {
		return new[] { F(null), F(new object()), F(1.5), F(1) };
	}
}", "C.M");

			Assert.That(result, Is.EqualTo(new[] { true, false, false, true }));
		}

		[Test]
		public void GetTypeWorksOnObjects() {
			var result = ExecuteCSharp(@"
using System;
public class C1 {}
public class C2<T> {}
public class C {
	public static string[] M() {
		Action a = () => {};
		return new[] { new C1().GetType().FullName,
		               new C2<int>().GetType().FullName,
		               new C2<string>().GetType().FullName,
		               (1).GetType().FullName,
		               ""X"".GetType().FullName,
		               a.GetType().FullName,
		               new object().GetType().FullName,
		               new[] { 1, 2 }.GetType().FullName
		             };
	}
}", "C.M");

			Assert.That(result, Is.EqualTo(new[] { "C1", "C2$1[ss.Int32]", "C2$1[String]", "Number", "String", "Function", "Object", "Array" }));
		}

		[Test]
		public void GetTypeOnNullInstanceThrowsException() {
			var result = ExecuteCSharp(@"
using System;
public class C1 {}
public class C2<T> {}
public class C {
	public static bool M() {
		bool result = false;
		try {
			((object)null).GetType();
		}
		catch {
			result = true;
		}
		return result;
	}
}", "C.M");

			Assert.That(result, Is.True);
		}
	}
}
