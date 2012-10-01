using System;
using System.Collections;
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
	}
}", "MyNamespace.MyClass.M");
			Assert.That(result, Is.EqualTo("MyNamespace.MyClass"));
		}

		[Test]
		public void TypeNamePropertyRemovesTheNamespace() {
			var result = ExecuteCSharp(
@"namespace MyNamespace {
	public class MyClass {
		public static string M() {
			return typeof(MyClass).Name;
		}
	}
}", "MyNamespace.MyClass.M");
			Assert.That(result, Is.EqualTo("MyClass"));
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
		return new[] { typeof(MyDelegate).FullName, typeof(Func<,,>).FullName, typeof(Func<int, int, string>).FullName, typeof(System.Delegate).FullName };
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
			Assert.That(result, Has.All.EqualTo("ss.Int32"));
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

		[Test]
		public void GettingBaseTypeWorks() {
			var result = ExecuteCSharp(
@"public class B {}
public class C : B {
	private static string Name(System.Type t) { return t != null ? t.FullName : null; }
	public static string[] M() {
		return new[] { Name(typeof(B).BaseType), Name(typeof(C).BaseType), Name(typeof(object).BaseType) };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { "Object", "B", null }));
		}

		[Test]
		public void GettingImplementedInterfacesWorks() {
			var result = ExecuteCSharp(
@"public interface I1 {}
public interface I2 : I1 {}
public interface I3 {}
public interface I4 : I3 {}

public class B : I2 {}
public class C : B, I4 {
	private static string Name(System.Type t) { return t != null ? t.FullName : null; }
	public static string[] M() {
		var ifs = typeof(C).GetInterfaces();
		var result = new string[ifs.Length];
		for (int i = 0; i < ifs.Length; i++)
			result[i] = ifs[i].FullName;
		return result;
	}
}", "C.M");
			Assert.That(result, Is.EquivalentTo(new[] { "I1", "I2", "I3", "I4" }));
		}

		[Test]
		public void TypeOfAnOpenGenericClassWorks() {
			var result = ExecuteCSharp(
@"public class G<T1, T2> {}
public interface I<T1> {}
public class C {
	public static string M() {
		return typeof(G<,>).FullName;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("G$2"));
		}

		[Test]
		public void TypeOfAnOpenGenericTypeWorks() {
			var result = ExecuteCSharp(
@"public interface I<T1> {}
public class C {
	public static string M() {
		return typeof(I<>).FullName;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("I$1"));
		}

		[Test]
		public void TypeOfInstantiatedGenericClassWorks() {
			var result = ExecuteCSharp(
@"public class G<T1, T2> {}
public interface I<T1> {}
public class C {
	public static string M() {
		return typeof(G<int,C>).FullName;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("G$2[ss.Int32,C]"));
		}

		[Test]
		public void TypeOfInstantiatedGenericInterfaceWorks() {
			var result = ExecuteCSharp(
@"public interface I<T1, T2> {}
public class C {
	public static string M() {
		return typeof(I<int,C>).FullName;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("I$2[ss.Int32,C]"));
		}

		[Test]
		public void ConstructingAGenericTypeTwiceWithTheSameArgumentsReturnsTheSameInstance() {
			var result = ExecuteCSharp(
@"public class G<T1, T2> {}
public class C {
	public static string M() {
		var t1 = typeof(G<int,C>);
		var t2 = typeof(G<C,int>);
		var t3 = typeof(G<int,C>);
		if (t1 == t2) return ""t1 was equal to t2"";
		if (t1 != t3) return ""t1 was not equal to t3"";
		return null;
	}
}", "C.M");
			Assert.That(result, Is.Null);
		}

		[Test]
		public void AccessingAStaticMemberInAGenericClassWorks() {
			var result = ExecuteCSharp(
@"public class G<T1, T2> {
	public static string Field;
	static G() {
		Field = typeof(T1).FullName + "" "" + typeof(T2).FullName;
	}
}
public class C {
	public static string [] M() {
		return new[] { G<int,C>.Field, G<C,int>.Field, G<G<C,int>,G<string,C>>.Field };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { "ss.Int32 C", "C ss.Int32", "G$2[C,ss.Int32] G$2[String,C]" }));
		}

		[Test]
		public void TypeOfNestedGenericGenericClassWorks() {
			var result = ExecuteCSharp(
@"public class G<T1, T2> {}
public interface I<T1> {}
public class C {
	public static string M() {
		return typeof(G<int,G<C,I<string>>>).FullName;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("G$2[ss.Int32,G$2[C,I$1[String]]]"));
		}

		[Test]
		public void BaseTypeAndImplementedInterfacesForGenericTypeWorks() {
			var t = typeof(Dictionary<,>);
			var result = ExecuteCSharp(
@"public class B<T> {}
public class G<T1, T2> : B<G<T1,C>>, I<G<T2,string>> {}
public interface I<T1> {}
public class C {
	public static string[] M() {
		return new[] { typeof(G<int,G<C,I<string>>>).BaseType.FullName, typeof(G<int,G<C,I<string>>>).GetInterfaces()[0].FullName };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { "B$1[G$2[ss.Int32,C]]", "I$1[G$2[G$2[C,I$1[String]],String]]" }));
		}

		[Test]
		public void IsGenericTypeDefinitionWorksAsExpected() {
			var result = ExecuteCSharp(
@"public class C1<T> {}
public class C2 {}
public interface I1<T1> {}
public interface I2 {}
public enum E {}
public class C {
	public static bool[] M() {
		return new[] { typeof(C1<>).IsGenericTypeDefinition, typeof(C1<int>).IsGenericTypeDefinition, typeof(C2).IsGenericTypeDefinition, typeof(I1<>).IsGenericTypeDefinition, typeof(I1<int>).IsGenericTypeDefinition, typeof(I2).IsGenericTypeDefinition, typeof(E).IsGenericTypeDefinition };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { true, false, false, true, false, false, false }));
		}

		[Test]
		public void GenericParameterCountReturnsZeroForConstructedTypesAndNonZeroForOpenOnes() {
			var result = ExecuteCSharp(
@"public class C1<T> {}
public class C2 {}
public interface I1<T1> {}
public interface I2 {}
public enum E {}
public class C {
	public static int[] M() {
		return new[] { typeof(C1<>).GenericParameterCount, typeof(C1<int>).GenericParameterCount, typeof(C2).GenericParameterCount, typeof(I1<>).GenericParameterCount, typeof(I1<int>).GenericParameterCount, typeof(I2).GenericParameterCount, typeof(E).GenericParameterCount };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { 1, 0, 0, 1, 0, 0, 0 }));
		}

		[Test]
		public void GetGenericArgumentsReturnsTheCorrectTypesForConstructedTypesOtherwiseNull() {
			var result = ExecuteCSharp(
@"public class C1<T> {}
public class C2 {}
public interface I1<T1> {}
public interface I2 {}
public enum E {}
public class C {
	public static string[] F(System.Type[] types) {
		if (types == null)
			return null;
		var result = new string[types.Length];
		for (int i = 0; i < types.Length; i++)
			result[i] = types[i].FullName;
		return result;
	}

	public static string[][] M() {
		return new[] { F(typeof(C1<>).GetGenericArguments()), F(typeof(C1<int>).GetGenericArguments()), F(typeof(C2).GetGenericArguments()), F(typeof(I1<>).GetGenericArguments()), F(typeof(I1<string>).GetGenericArguments()), F(typeof(I2).GetGenericArguments()), F(typeof(E).GetGenericArguments()) };
	}
}", "C.M");
			var l = ((IEnumerable)result).Cast<IList>().ToList();
			Assert.That(l.Select(x => x != null).ToList(), Is.EqualTo(new[] { false, true, false, false, true, false, false }));
			Assert.That(l[1], Is.EqualTo(new[] { "ss.Int32" }));
			Assert.That(l[4], Is.EqualTo(new[] { "String" }));
		}

		[Test]
		public void GetGenericTypeDefinitionReturnsTheGenericTypeDefinitionForConstructedTypeOtherwiseNull() {
			var result = ExecuteCSharp(
@"public class C1<T> {}
public class C2 {}
public interface I1<T1> {}
public interface I2 {}
public enum E {}
public class C {
	private static string Name(System.Type t) { return t != null ? t.FullName : null; }
	public static string[] M() {
		return new[] { Name(typeof(C1<>).GetGenericTypeDefinition()), Name(typeof(C1<int>).GetGenericTypeDefinition()), Name(typeof(C2).GetGenericTypeDefinition()), Name(typeof(I1<>).GetGenericTypeDefinition()), Name(typeof(I1<string>).GetGenericTypeDefinition()), Name(typeof(I2).GetGenericTypeDefinition()), Name(typeof(E).GetGenericTypeDefinition()) };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { null, "C1$1", null, null, "I1$1", null, null }));
		}

		[Test]
		public void IsAssignableFromWorks() {
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
		return new[] { typeof(C1).IsAssignableFrom(typeof(object)),             // false
		               typeof(object).IsAssignableFrom(typeof(C1)),             // true
		               typeof(I1).IsAssignableFrom(typeof(object)),             // false
		               typeof(object).IsAssignableFrom(typeof(I1)),             // true
		               typeof(I3).IsAssignableFrom(typeof(I1)),                 // false
		               typeof(I1).IsAssignableFrom(typeof(I3)),                 // true
		               typeof(D1).IsAssignableFrom(typeof(C1)),                 // false
		               typeof(C1).IsAssignableFrom(typeof(D1)),                 // true
		               typeof(I1).IsAssignableFrom(typeof(D1)),                 // true
		               typeof(C2<int>).IsAssignableFrom(typeof(D2<int>)),       // true
		               typeof(C2<string>).IsAssignableFrom(typeof(D2<int>)),    // false
		               typeof(I2<int>).IsAssignableFrom(typeof(D2<int>)),       // true
		               typeof(I2<string>).IsAssignableFrom(typeof(D2<int>)),    // false
		               typeof(I1).IsAssignableFrom(typeof(D2<int>)),            // true
		               typeof(C2<string>).IsAssignableFrom(typeof(D3)),         // false
		               typeof(C2<int>).IsAssignableFrom(typeof(D3)),            // true
		               typeof(I2<int>).IsAssignableFrom(typeof(D3)),            // false
		               typeof(I2<string>).IsAssignableFrom(typeof(D3)),         // true
		               typeof(I2<int>).IsAssignableFrom(typeof(I5<string>)),    // false
		               typeof(I2<int>).IsAssignableFrom(typeof(I5<int>)),       // true
		               typeof(I5<int>).IsAssignableFrom(typeof(I2<int>)),       // false
		               typeof(I1).IsAssignableFrom(typeof(D4)),                 // true
		               typeof(I3).IsAssignableFrom(typeof(D4)),                 // true
		               typeof(I4).IsAssignableFrom(typeof(D4)),                 // true
		               typeof(I1).IsAssignableFrom(typeof(X2)),                 // true
		               typeof(I2<>).IsAssignableFrom(typeof(I5<>)),             // false
		               typeof(C2<>).IsAssignableFrom(typeof(D2<>)),             // false
		               typeof(C2<>).IsAssignableFrom(typeof(D3)),               // false
		               typeof(E1).IsAssignableFrom(typeof(E2)),                 // false
		               typeof(int).IsAssignableFrom(typeof(E1)),                // false
		               typeof(object).IsAssignableFrom(typeof(E1)),             // true
		             };
		};
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { false, true, false, true, false, true, false, true, true, true, false, true, false, true, false, true, false, true, false, true, false, true, true, true, true, false, false, false, false, false, true }));
		}

		[Test]
		public void IsClassWorks() {
			var result = ExecuteCSharp(
@"using System;
public enum E1 {}
[Flags] public enum E2 {}
public class C {}
public class C2<T> {}
public interface I {}
public interface I2<T> {}

public class X {
	public static bool[] M() {
		return new[] { typeof(E1).IsClass,       // false
		               typeof(E2).IsClass,       // false
		               typeof(C).IsClass,        // true
		               typeof(C2<>).IsClass,     // true
		               typeof(C2<int>).IsClass,  // true
		               typeof(I).IsClass,        // false
		               typeof(I2<>).IsClass,     // false
		               typeof(I2<int>).IsClass,  // false
		             };
	}
}", "X.M");

			Assert.That(result, Is.EqualTo(new[] { false, false, true, true, true, false, false, false }));
		}

		[Test]
		public void IsEnumWorks() {
			var result = ExecuteCSharp(
@"using System;
public enum E1 {}
[Flags] public enum E2 {}
public class C {}
public class C2<T> {}
public interface I {}
public interface I2<T> {}

public class X {
	public static bool[] M() {
		return new[] { typeof(E1).IsEnum,       // true
		               typeof(E2).IsEnum,       // true
		               typeof(C).IsEnum,        // false
		               typeof(C2<>).IsEnum,     // false
		               typeof(C2<int>).IsEnum,  // false
		               typeof(I).IsEnum,        // false
		               typeof(I2<>).IsEnum,     // false
		               typeof(I2<int>).IsEnum,  // false
		             };
	}
}", "X.M");

			Assert.That(result, Is.EqualTo(new[] { true, true, false, false, false, false, false, false }));
		}

		[Test]
		public void IsFlags() {
			var result = ExecuteCSharp(
@"using System;
public enum E1 {}
[Flags] public enum E2 {}
public class C {}
public class C2<T> {}
public interface I {}
public interface I2<T> {}

public class X {
	public static bool[] M() {
		return new[] { typeof(E1).IsFlags,       // false
		               typeof(E2).IsFlags,       // true
		               typeof(C).IsFlags,        // false
		               typeof(C2<>).IsFlags,     // false
		               typeof(C2<int>).IsFlags,  // false
		               typeof(I).IsFlags,        // false
		               typeof(I2<>).IsFlags,     // false
		               typeof(I2<int>).IsFlags,  // false
		             };
	}
}", "X.M");

			Assert.That(result, Is.EqualTo(new[] { false, true, false, false, false, false, false, false }));
		}

		[Test]
		public void IsInterfaceWorks() {
			var result = ExecuteCSharp(
@"using System;
public enum E1 {}
[Flags] public enum E2 {}
public class C {}
public class C2<T> {}
public interface I {}
public interface I2<T> {}

public class X {
	public static bool[] M() {
		return new[] { typeof(E1).IsInterface,       // false
		               typeof(E2).IsInterface,       // false
		               typeof(C).IsInterface,        // false
		               typeof(C2<>).IsInterface,     // false
		               typeof(C2<int>).IsInterface,  // false
		               typeof(I).IsInterface,        // true
		               typeof(I2<>).IsInterface,     // true
		               typeof(I2<int>).IsInterface,  // true
		             };
	}
}", "X.M");

			Assert.That(result, Is.EqualTo(new[] { false, false, false, false, false, true, true, true }));
		}

		[Test]
		public void IsInstanceOfTypeWorksForReferenceTypes() {
			var result = ExecuteCSharp(
@"using System;

public class C1 {}
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
		return new[] { Type.IsInstanceOfType(new object(), typeof(C1)),             // false
		               Type.IsInstanceOfType(new C1(), typeof(object)),             // true
		               Type.IsInstanceOfType(new object(), typeof(I1)),             // false
		               Type.IsInstanceOfType(new C1(), typeof(D1)),                 // false
		               Type.IsInstanceOfType(new D1(), typeof(C1)),                 // true
		               Type.IsInstanceOfType(new D1(), typeof(I1)),                 // true
		               Type.IsInstanceOfType(new D2<int>(), typeof(C2<int>)),       // true
		               Type.IsInstanceOfType(new D2<int>(), typeof(C2<string>)),    // false
		               Type.IsInstanceOfType(new D2<int>(), typeof(I2<int>)),       // true
		               Type.IsInstanceOfType(new D2<int>(), typeof(I2<string>)),    // false
		               Type.IsInstanceOfType(new D2<int>(), typeof(I1)),            // true
		               Type.IsInstanceOfType(new D3(), typeof(C2<string>)),         // false
		               Type.IsInstanceOfType(new D3(), typeof(C2<int>)),            // true
		               Type.IsInstanceOfType(new D3(), typeof(I2<int>)),            // false
		               Type.IsInstanceOfType(new D3(), typeof(I2<string>)),         // true
		               Type.IsInstanceOfType(new D4(), typeof(I1)),                 // true
		               Type.IsInstanceOfType(new D4(), typeof(I3)),                 // true
		               Type.IsInstanceOfType(new D4(), typeof(I4)),                 // true
		               Type.IsInstanceOfType(new X2(), typeof(I1)),                 // true
		               Type.IsInstanceOfType(new D3(), typeof(C2<>)),               // false
		               Type.IsInstanceOfType(new E2(), typeof(E1)),                 // true
		               Type.IsInstanceOfType(new E1(), typeof(int)),                // true
		               Type.IsInstanceOfType(new E1(), typeof(object)),             // true
		               Type.IsInstanceOfType(null, typeof(object)),                 // false
		             };
		};
	}
}", "C.M");

			Assert.That(result, Is.EqualTo(new[] { false, true, false, false, true, true, true, false, true, false, true, false, true, false, true, true, true, true, true, false, true, true, true, false }));
		}

		[Test]
		public void IsInstanceOfTypeWorksForInt32() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	public static bool[] M() {
		return new[] { Type.IsInstanceOfType(null, typeof(int)), Type.IsInstanceOfType(new object(), typeof(int)), Type.IsInstanceOfType(1.5, typeof(int)), Type.IsInstanceOfType(1, typeof(int)) };
	}
}", "C.M");

			Assert.That(result, Is.EqualTo(new[] { false, false, false, true }));
		}

		[Test]
		public void DefaultValuesWork() {
			var result = ExecuteCSharp(@"
using System.Collections.Generic;
public enum E {}
public class C {
	static string s;
	static T F<T>() {
		return default(T);
	}

	public static object[] M() {
		return new object[] {
			F<sbyte>(),
			F<byte>(),
			F<short>(),
			F<ushort>(),
			F<int>(),
			F<uint>(),
			F<long>(),
			F<ulong>(),
			F<float>(),
			F<double>(),
			F<decimal>(),
			F<bool>(),
			F<object>(),
			F<C>(),
			F<IDictionary<string, int>>(),
			F<Dictionary<string, int>>(),
			F<int?>(),
			F<E>(),
			F<E?>(),
		};
	}
}", "C.M");

			Assert.That(result, Is.EqualTo(new object[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, false, null, null, null, null, null, 0, null }));
		}

		[Test]
		public void InvokingBaseUnnamedConstructorWithoutArgumentsWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;
public class B {
	public string messageB;

	public B() {
		messageB = ""X"";
	}
}

public class D : B {
	public string messageD;

	public D() {
		messageD = ""Y"";
	}
}


public class C {
	public static string M() {
		var d = new D();
		return d.messageB + ""|"" + d.messageD;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("X|Y"));
		}

		[Test]
		public void InvokingBaseUnnamedConstructorWithArgumentsWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

public class B {
	public string messageB;

	public B(int x, int y) {
		messageB = x + "" "" + y;
	}
}

public class D : B {
	public string messageD;

	public D(int x, int y) : base(x + 1, y + 1) {
		messageD = x + "" "" + y;
	}
}


public class C {
	public static string M() {
		var d = new D(5, 8);
		return d.messageB + ""|"" + d.messageD;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("6 9|5 8"));
		}

		[Test]
		public void InvokingBaseNamedConstructorWithoutArgumentsWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

public class B {
	public string messageB;

	[ScriptName(""myCtor"")]
	public B() {
		messageB = ""X"";
	}
}

public class D : B {
	public string messageD;

	public D() {
		messageD = ""Y"";
	}
}


public class C {
	public static string M() {
		var d = new D();
		return d.messageB + ""|"" + d.messageD;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("X|Y"));
		}

		[Test]
		public void InvokingBaseNamedConstructorWithArgumentsWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

public class B {
	public string messageB;

	[ScriptName(""myCtor"")]
	public B(int x, int y) {
		messageB = x + "" "" + y;
	}
}

public class D : B {
	public string messageD;

	public D(int x, int y) : base(x + 1, y + 1) {
		messageD = x + "" "" + y;
	}
}


public class C {
	public static string M() {
		var d = new D(5, 8);
		return d.messageB + ""|"" + d.messageD;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("6 9|5 8"));
		}

		[Test]
		public void ConstructingInstanceWithNamedConstructorWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

public class D {
	public string GetMessage() { return ""The message "" + f; }

	private string f;

	[ScriptName(""myCtor"")]
	public D() {
		f = ""from ctor"";
	}
}


public class C {
	public static string M() {
		return new D().GetMessage();
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("The message from ctor"));
		}

		[Test]
		public void InvokingBaseMethodWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

public class B {
	public virtual int F(int x, int y) { return x - y; }
}

public class D : B {
	public override int F(int x, int y) { return x + y; }

	public int DoIt(int x, int y) {
		return base.F(x, y);
	}
}

public class C {
	public static int M() {
		return new D().DoIt(5, 3);
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(2));
		}

		[Test]
		public void InvokingGenericBaseMethodWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

public class B {
	public virtual int F<T>(int x, int y) { return x - y; }
}

public class D : B {
	public override int F<T>(int x, int y) { return x + y; }

	public int DoIt(int x, int y) {
		return base.F<string>(x, y);
	}
}

public class C {
	public static int M() {
		return new D().DoIt(5, 3);
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(2));
		}

		[Test]
		public void ConvertingAMethodToADelegateWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

public class D  {
	private int m;

	public int F(int x, int y) { return x + y + m; }

	public D(int m) {
		this.m = m;
	}

	public Func<int, int, int> GetF() {
		return F;
	}
}

public class C {
	public static int M() {
		return new D(5).GetF()(5, 3);
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(13));
		}

		[Test]
		public void ConvertingAGenericMethodToADelegateWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

public class D  {
	private int m;

	public int F<T>(int x, int y) { return x + y + m; }

	public D(int m) {
		this.m = m;
	}

	public Func<int, int, int> GetF() {
		return F<string>;
	}
}

public class C {
	public static int M() {
		return new D(5).GetF()(5, 3);
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(13));
		}

		[Test]
		public void ConvertingABaseMethodToADelegateWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

public class B {
	public int m;

	public virtual int F(int x, int y) { return x + y + m; }

	public B(int m) {
		this.m = m;
	}
}

public class D : B {
	public override int F(int x, int y) { return x - y - m; }

	public Func<int, int, int> GetF() {
		return base.F;
	}

	public D(int m) : base(m) {
	}
}

public class C {
	public static int M() {
		return new D(5).GetF()(5, 3);
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(13));
		}

		[Test]
		public void ConvertingAGenericBaseMethodToADelegateWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

public class B {
	public int m;

	public virtual int F<T>(int x, int y) { return x + y + m; }

	public B(int m) {
		this.m = m;
	}
}

public class D : B {
	public override int F<T>(int x, int y) { return x - y - m; }

	public Func<int, int, int> GetF() {
		return base.F<string>;
	}

	public D(int m) : base(m) {
	}
}

public class C {
	public static int M() {
		return new D(5).GetF()(5, 3);
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(13));
		}

		[Test]
		public void ImportedInterfaceAppearsAsObjectWhenUsedWithTypeOfAndAsGenericArgument() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

[Imported] public interface I1 {}
public class C1<T> {}

public class C {
	public static bool[] M() {
		var tobj = typeof(object);
		var ti1  = typeof(I1);
		var tcofo = typeof(C1<object>);
		var tcofi = typeof(C1<I1>);
		return new[] { tobj == ti1, tcofo == tcofi };
	}
}", "C.M");
			Assert.That(result, Has.All.True);
		}

		[Test]
		public void TypeOfEnumIsTheEnumType() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

public enum E {}

public class C {
	public static string M() {
		return typeof(E).FullName;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("E"));
		}

		[Test]
		public void ConversionsToEnumAreTreatedAsConversionsToTheUnderlyingType() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;

public enum E {}

public class C {
	private static bool DoesItThrow(Action a) {
		try {
			a();
			return false;
		}
		catch {
			return true;
		}
	}

	public static bool[] M() {
		return new[] { DoesItThrow(() => { var e = (E)(object)0; }), DoesItThrow(() => { var e = (E)(object)0.5; }) };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { false, true }));
		}

		[Test]
		public void CastToSerializableTypeIsANoOp() {
			AssertSourceCorrect(@"
using System;
using System.Runtime.CompilerServices;

[Serializable]
sealed class R {}

public class C {
	private void M() {
		object o = null;
		// BEGIN
		var v1 = (R)o;
		var v2 = o as R;
		var v3 = o is R;
		// END
	}
}",
@"		var v1 = o;
		var v2 = o;
		var v3 = ss.isValue(o);
");
		}

		[Test]
		public void CastToImportedInterfaceIsANoOp() {
			AssertSourceCorrect(@"
using System;
using System.Runtime.CompilerServices;

[Imported]
interface I {}

public class C {
	private void M() {
		object o = null;
		// BEGIN
		var v1 = (I)o;
		var v2 = o as I;
		var v3 = o is I;
		// END
	}
}",
@"		var v1 = o;
		var v2 = o;
		var v3 = ss.isValue(o);
");
		}

		[Test]
		public void CastToImportedGenericClassIsANoOp() {
			AssertSourceCorrect(@"
using System;
using System.Runtime.CompilerServices;

[Imported]
class C1<T> {}

public class C {
	private void M() {
		object o = null;
		// BEGIN
		var v1 = (C1<int>)o;
		var v2 = o as C1<int>;
		var v3 = o is C1<int>;
		// END
	}
}",
@"		var v1 = o;
		var v2 = o;
		var v3 = ss.isValue(o);
");
		}

		[Test]
		public void CastBetweenTypesWithTheSameScriptNameIsANoOp() {
			AssertSourceCorrect(@"
using System;
using System.Runtime.CompilerServices;

[ScriptName(""X"")]
class C1 {}
[ScriptName(""X"")]
class C2 : C1 {}

public class C {
	private void M() {
		C1 o = null;
		// BEGIN
		var v1 = (C2)o;
		var v2 = o as C2;
		var v3 = o is C2;
		// END
	}
}",
@"		var v1 = o;
		var v2 = o;
		var v3 = ss.isValue(o);
");
		}

		[Test]
		public void ComparingObjectToNullIsCallToIsNullOrUndefined() {
			AssertSourceCorrect(@"
using System;

public class C {
	private void M() {
		object o = null;
		// BEGIN
		var v1 = o == null;
		var v2 = null == o;
		var v3 = o != null;
		var v4 = null != o;
		// END
	}
}",
@"		var v1 = ss.isNullOrUndefined(o);
		var v2 = ss.isNullOrUndefined(o);
		var v3 = ss.isValue(o);
		var v4 = ss.isValue(o);
");
		}

		[Test]
		public void ComparingWithLiteralStringIsNotCallToIsNullOrUndefined() {
			AssertSourceCorrect(@"
using System;

public class C {
	private void M() {
		object o = null;
		// BEGIN
		var v1 = o == ""X"";
		var v2 = ""X"" == o;
		var v3 = o != ""X"";
		var v4 = ""X"" != o;
		// END
	}
}",
@"		var v1 = o === 'X';
		var v2 = 'X' === o;
		var v3 = o !== 'X';
		var v4 = 'X' !== o;
");
		}

		[Test]
		public void ComparingObjectsIsCallToReferenceEquals() {
			AssertSourceCorrect(@"
using System;

public class C {
	private void M() {
		object o1 = null, o2 = null;
		// BEGIN
		var v1 = o1 == o2;
		var v2 = o1 != o2;
		// END
	}
}",
@"		var v1 = ss.referenceEquals(o1, o2);
		var v2 = !ss.referenceEquals(o1, o2);
");
		}

		[Test]
		public void ConvertingDynamicToBoolUsesDoubleNegation() {
			AssertSourceCorrect(@"
using System;

public class C {
	private void M() {
		dynamic d = null;
		// BEGIN
		bool b = d;
		// END
	}
}",
@"		var b = !!d;
");
		}

		[Test]
		public void FalseIsFunctionShouldReturnFalse() {
			var result = ExecuteCSharp(
@"using System;
public class C {
	public static bool M() {
		return (object)false is Function;
	}
}", "C.M");
			Assert.That(result, Is.False);
		}
	}
}
