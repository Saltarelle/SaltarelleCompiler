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

		[Test]
		public void TypeOfArrayTypeIsArray() {
			var result = ExecuteCSharp(
@"public class C {
	private static string[] M2<T>() {
		return new[] { typeof(int[]).FullName, typeof(string[][]).FullName, typeof(System.Array).FullName, typeof(T[]).FullName };
	}

	public static string[] M() {
		return M2<object>();
	}
}", "C.M");
			Assert.That(result, Has.All.EqualTo("Array"));
		}

		[Test]
		public void TypeOfDelegateTypeIsFunction() {
			var result = ExecuteCSharp(
@"public class C {
	delegate int MyDelegate(int a);
	delegate TResult Func<T1, T2, TResult>(T1 t1, T2 t2);
	public static string[] M() {
		return new[] { typeof(MyDelegate).FullName, typeof(Func<,,>).FullName, typeof(Func<int, int, string>).FullName, typeof(System.Delegate) };
	}
}", "C.M");
			Assert.That(result, Has.All.EqualTo("Function"));
		}

		[Test]
		public void TypeOfIntegerTypeIsInt32() {
			var result = ExecuteCSharp(
@"public class C {
	public static string[] M() {
		return new[] { typeof(sbyte).FullName, typeof(byte).FullName, typeof(short).FullName, typeof(ushort).FullName, typeof(int).FullName, typeof(uint).FullName, typeof(long).FullName, typeof(ulong).FullName };
	}
}", "C.M");
			Assert.That(result, Has.All.EqualTo("Int32"));
		}

		[Test]
		public void TypeOfFloatingPointTypeIsNumber() {
			var result = ExecuteCSharp(
@"public class C {
	public static string[] M() {
		return new[] { typeof(float).FullName, typeof(double).FullName, typeof(decimal).FullName };
	}
}", "C.M");
			Assert.That(result, Has.All.EqualTo("Number"));
		}
	}
}
