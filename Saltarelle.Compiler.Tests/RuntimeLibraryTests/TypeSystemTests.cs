using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class TypeSystemTests : RuntimeLibraryTestBase {
		[Test]
		public void TypeOfObjectIsObject() {
			var result = ExecuteCSharp(
@"public class C {
	public static string M() {
		return typeof(object).FullName;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("Object"));
		}

		[Test]
		public void CanGetTypeNameOfUserDefinedClass() {
			var result = ExecuteCSharp(
@"namespace MyNamespace {
	public class MyClass {
		public static string M() {
			return typeof(MyClass).FullName;
		}
}", "MyNamespace.MyClass.M");
			Assert.That(result, Is.EqualTo("MyNamespace.MyClass"));
		}


	}
}
