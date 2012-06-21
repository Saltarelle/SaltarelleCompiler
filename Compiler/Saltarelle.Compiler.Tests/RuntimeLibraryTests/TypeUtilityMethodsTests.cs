using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class TypeUtilityMethodsTests : RuntimeLibraryTestBase {
		[Test]
		public void AddHandlerWorks() {
			var result = ExecuteCSharp(
@"using System;
using System.Runtime.CompilerServices;

public class C {
	[PreserveCase]
	public event EventHandler Evt;

	void Raise() { if (Evt != null) Evt(this, null); }

	public static bool M() {
		bool invoked = false;
		var c = new C();
		Type.AddHandler(c, ""Evt"", (EventHandler)((s, e) => invoked = true));
		c.Raise();
		return invoked;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(true));
		}

		[Test]
		public void DeleteFieldWorks() {
			var result = ExecuteCSharp(
@"using System;
public class C {
	public int i;

	public static string[] M() {
		var c = new C();
		var s1 = Type.GetScriptType(c.i);
		Type.DeleteField(c, ""i"");
		return new[] { s1, Type.GetScriptType(c.i) };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { "number", "undefined" }));
		}

		[Test]
		public void GetFieldWorks() {
			var result = ExecuteCSharp(
@"using System;
public class C {
	public int i;

	public static int M() {
		var c = new C();
		c.i = 438;
		return (int)Type.GetField(c, ""i"");
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(438));
		}

		[Test]
		public void GetPropertyWorks() {
			var result = ExecuteCSharp(
@"using System;
using System.Runtime.CompilerServices;
public class C {
	[PreserveCase]
	public int P { get; set; }

	public static int M() {
		var c = new C();
		c.P = 456;
		return (int)Type.GetProperty(c, ""P"");
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(456));
		}

		[Test]
		public void GetScriptTypeWorks() {
			var result = ExecuteCSharp(
@"using System;
public class C {
	public static string[] M() {
		return new[] { Type.GetScriptType(null), Type.GetScriptType(new object()), Type.GetScriptType(""X""), Type.GetScriptType(1) };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { "object", "object", "string", "number" }));
		}

		[Test]
		public void StaticGetTypeMethodWorks() {
			var result = ExecuteCSharp(
@"using System;
namespace Namespace.Inner {
	public class X {}
}
public class C {
	public static string[] M() {
		return new[] { Type.GetType(""C"").FullName, Type.GetType(""Namespace.Inner.X"").FullName };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { "C", "Namespace.Inner.X" }));
		}

		[Test]
		public void HasFieldWorks() {
			var result = ExecuteCSharp(
@"using System;
public class C {
	public int field1;
	public static bool[] M() {
		var c = new C();
		return new[] { Type.HasField(c, ""field1""), Type.HasField(c, ""field2"") };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { true, false }));
		}

		[Test]
		public void HasMethodWorks() {
			var result = ExecuteCSharp(
@"using System;
public class C {
	public void SomeMethod() {}
	public static bool[] M() {
		var c = new C();
		return new[] { Type.HasMethod(c, ""someMethod""), Type.HasMethod(c, ""otherMethod""), Type.HasMethod(c, ""m""), Type.HasMethod(typeof(C), ""m"") };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { true, false, false, true }));
		}

		[Test]
		public void HasPropertyWorks() {
			var result = ExecuteCSharp(
@"using System;
public class C {
	public int Prop1 { get; set; }
	public int Prop2 { get; }
	public int Prop3 { set; }
	public static bool[] M() {
		var c = new C();
		return new[] { Type.HasProperty(c, ""prop1""), Type.HasProperty(c, ""prop2""), Type.HasProperty(c, ""prop3""), Type.HasProperty(c, ""prop4"") };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { true, true, true, false }));
		}

		[Test]
		public void InvokeMethodWorks() {
			var result = ExecuteCSharp(
@"using System;
using System.Runtime.CompilerServices;

public class C {
	[PreserveCase]
	public int F1() { return 42; }
	[PreserveCase]
	public int F2(int i) { return i + 10; }
	[PreserveCase]
	public int F3(int i, int j) { return i + j; }

	public static int[] M() {
		var c = new C();
		return new[] { (int)Type.InvokeMethod(c, ""F1""), (int)Type.InvokeMethod(c, ""F2"", 17), (int)Type.InvokeMethod(c, ""F3"", 19, 2) };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { 42, 27, 21 }));
		}

		[Test]
		public void RemoveHandlerWorks() {
			var result = ExecuteCSharp(
@"using System;
using System.Runtime.CompilerServices;

public class C {
	[PreserveCase]
	public event EventHandler Evt;

	void Raise() { if (Evt != null) Evt(this, null); }

	public static bool[] M() {
		bool invoked = false;
		var handler = (EventHandler)((s, e) => invoked = true);
		var c = new C();
		c.Evt += handler;
		c.Raise();
		bool b = invoked;
		invoked = false;
		Type.RemoveHandler(c, ""Evt"", handler);
		c.Raise();
		return new[] { b, invoked };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { true, false }));
        }

		[Test]
        public void SetFieldWorks() {
			var result = ExecuteCSharp(
@"using System;
public class C {
	public int i;

	public static int M() {
		var c = new C();
		Type.SetField(c, ""i"", 546);
		return c.i;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(546));
        }

		[Test]
        public void SetPropertyWorks() {
			var result = ExecuteCSharp(
@"using System;
using System.Runtime.CompilerServices;
public class C {
	[PreserveCase]
	public int P { get; set; }

	public static int M() {
		var c = new C();
		Type.SetProperty(c, ""P"", 543);
		return c.P;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(543));
        }
	}
}
