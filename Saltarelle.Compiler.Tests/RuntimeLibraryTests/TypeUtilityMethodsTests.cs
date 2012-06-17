using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class TypeUtilityMethodsTests : RuntimeLibraryTestBase {
		[Test, Ignore("TODO, Requires an improvement of the InlineCode method implementation")]
		public void AddHandlerWorks() {
			Assert.Fail("TODO: Implement and test");
		}

		[Test, Ignore("TODO: Requires improvement of InlineCode")]
		public void CreateInstanceWorks() {
			Assert.Fail("TODO: Implement and test");
		}

		[Test, Ignore("TODO: Requires improvement of InlineCode")]
		public void DeleteFieldWorks() {
			Assert.Fail("TODO: Implement and test");
		}

		[Test, Ignore("TODO: Requires improvement of InlineCode")]
		public void GetFieldWorks() {
			Assert.Fail("TODO: Implement and test, both overloads");
		}

		[Test, Ignore("TODO: Requires improvement of InlineCode")]
		public void GetPropertyWorks() {
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

		[Test, Ignore("TODO: Requires improvement of InlineCode")]
		public void InvokeMethodWorks() {
			Assert.Fail("TODO: Implement and test");
		}
	}
}
