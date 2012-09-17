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
		public void CloningDelegateToADifferenTypeIsANoOp() {
			var result = ExecuteCSharp(@"
using System;
public delegate void D1();
public delegate void D2();
public class C {
	public static bool M() {
		D1 d1 = () => {};
		D2 d2 = new D2(d1);
		return (object)d1 == (object)d2;
	}
}", "C.M");
			Assert.That(result, Is.True);
		}

		[Test]
		public void CloningDelegateToTheSameTypeCreatesANewClone() {
			var result = ExecuteCSharp(@"
using System;
public delegate void D();
public class C {
	public static object[] M() {
		int x = 0;
		D d1 = () => x++;
		D d2 = new D(d1);
		d1();
		d2();
		return new object[] { d1 == d2, x };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new object[] { false, 2 }));
		}

		[Test]
		public void DelegateWithBindThisToFirstParameterWorksWhenInvokedFromScript() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;
[BindThisToFirstParameter]
public delegate int D(int a, int b);
public class C {
	public static int M() {
		D d = (a, b) => a + b;
		Function f = (Function)d;
		return (int)f.Call(10, 20);
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(30));
		}

		[Test]
		public void DelegateWithBindThisToFirstParameterWorksWhenInvokedFromCode() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;
[BindThisToFirstParameter]
public delegate int D(int a, int b);
public class C {
	public static int M() {
		D d = (a, b) => a + b;
		return d(10, 20);
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(30));
		}
	}
}
