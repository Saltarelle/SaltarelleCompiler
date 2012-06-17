using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class DelegateTests : RuntimeLibraryTestBase {
		[Test]
		public void DelegateEmptyFieldCanBeInvokedAndDoesNothing() {
			ExecuteCSharp(@"
using System;			
public class C {
	public static string M() {
		Action a = (Action)Delegate.Empty;
		a();
	}
}", "C.M");
			// No exception is good enough
		}

		[Test]
		public void DelegateCreateCombineAndRemoveWork() {
			var result = ExecuteCSharp(@"
using System;			
public class C {
	private static string s;

	private string _name;

	private C(string name) {
		_name = name;
	}

	public void M1() { s += ""("" + _name + ""M1)""; }
	public void M2() { s += ""("" + _name + ""M2)""; }
	public void M3() { s += ""("" + _name + ""M3)""; }

	public static void S1() { s += ""("" + ""S1"" + "")""; }
	public static void S2() { s += ""("" + ""S2"" + "")""; }

	public static string M() {
		C c1 = new C(""1""), c2 = new C(""2""), c3 = new C(""3"");
		s = """";
		Action a = c1.M1;
		a(); s += ""\n"";
		a += c1.M2;
		a(); s += ""\n"";
		a += c2.M1;
		a(); s += ""\n"";
		a += c2.M3;
		a(); s += ""\n"";
		a += S1;
		a(); s += ""\n"";
		a += c2.M1;
		a(); s += ""\n"";
		a += c3.M1;
		a(); s += ""\n"";
		a += S1;
		a(); s += ""\n"";
		a += S2;
		a(); s += ""\n"";
		a -= c2.M1;
		a(); s += ""\n"";
		a -= S1;
		a(); s += ""\n"";
		a -= S2;
		a(); s += ""\n"";
		a -= c1.M1;
		a(); s += ""\n"";
		a -= c2.M3;
		a(); s += ""\n"";
		a -= c3.M1;
		a(); s += ""\n"";
		a -= c1.M2;
		a(); s += ""\n"";
		a -= c2.M1;
		a(); s += ""\n"";
		a -= S1;
		s += (a != null ? ""not null"" : ""null"") + ""\n"";

		return s;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(
@"(1M1)
(1M1)(1M2)
(1M1)(1M2)(2M1)
(1M1)(1M2)(2M1)(2M3)
(1M1)(1M2)(2M1)(2M3)(S1)
(1M1)(1M2)(2M1)(2M3)(S1)(2M1)
(1M1)(1M2)(2M1)(2M3)(S1)(2M1)(3M1)
(1M1)(1M2)(2M1)(2M3)(S1)(2M1)(3M1)(S1)
(1M1)(1M2)(2M1)(2M3)(S1)(2M1)(3M1)(S1)(S2)
(1M1)(1M2)(2M3)(S1)(2M1)(3M1)(S1)(S2)
(1M1)(1M2)(2M3)(2M1)(3M1)(S1)(S2)
(1M1)(1M2)(2M3)(2M1)(3M1)(S1)
(1M2)(2M3)(2M1)(3M1)(S1)
(1M2)(2M1)(3M1)(S1)
(1M2)(2M1)(S1)
(2M1)(S1)
(S1)
null
".Replace("\r\n", "\n")));
		}
	}
}
