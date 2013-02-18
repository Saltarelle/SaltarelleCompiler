using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class ReflectionTests {
		class A1Attribute : Attribute {}

		[NonScriptable]
		class A2Attribute : Attribute {}

		public class C1 {
			public void M1() {}
			[A1]
			public void M2() {}
			[Reflectable]
			public void M3() {}
			[A2]
			public void M4() {}
		}

		public class C2 {
			[Reflectable] public void M1() {}
			[Reflectable] public static void M2() {}
		}

		public class C3 {
			[Reflectable] public int M1() { return 0; }
			[Reflectable] public int M2(string x) { return 0; }
			[Reflectable] public int M3(string x, int y) { return 0; }
			[Reflectable] public void M4() {}
		}

		public class C4 {
			[Reflectable] public void M() {}
			[Reflectable] public void M(int i) {}
			[Reflectable, ScriptName("x")] public void M(int i, string s) {}
		}

		public class C5<T1, T2> {
			[Reflectable] public T1 M(T2 t2, string s) { return default(T1); }
			[Reflectable] public object M2() { return null; }
		}

		public class C6 {
			[Reflectable] public T1 M1<T1, T2>(T2 t2, string s) { return default(T1); }
			[Reflectable] public T1 M2<T1>(string s) { return default(T1); }
			[Reflectable] public void M3(string s) {}
		}

		[Serializable]
		public class C7 {
			[Reflectable] public void M1(int x) {}
			[Reflectable] public static void M2(string x) {}
		}

		public class C8 {
			private string s;
			public C8(string s) {
				this.s = s;
			}
			[Reflectable] public string M1(string a, string b) { return s + " " + a + " " + b; }
			[Reflectable] public static string M2(string a, string b) { return a + " " + b; }
			[Reflectable] public string M3<T1, T2>(string a) { return s + " " + typeof(T1).FullName + " " + typeof(T2).FullName + " " + a; }
			[Reflectable] public static string M4<T1, T2>(string a) { return typeof(T1).FullName + " " + typeof(T2).FullName + " " + a; }
		}

		public class C9<T1, T2> {
			[Reflectable] public static string M(string a) { return typeof(T1).FullName + " " + typeof(T2).FullName + " " + a; }
		}

		public class C10 {
			public int X;
			public string S;

			[Reflectable, ScriptName("")] public C10(int x) { X = x; S = "X"; }
			[Reflectable, ScriptName("ctor1")] public C10(int x, string s) { X = x; S = s; }
		}

		[Serializable]
		public class C11 {
			public DateTime D;
			[Reflectable] public C11(DateTime dt) { D = dt; }
		}

		public class C12 {
			[Reflectable] public int F1;
			[Reflectable] public DateTime F2;
			[Reflectable] public static string F3;
		}

		private MethodInfo GetMethod(Type type, string name, BindingFlags flags = BindingFlags.Default) {
			return (MethodInfo)type.GetMembers(flags).Filter(m => m.Name == name)[0];
		}

		private FieldInfo GetField(Type type, string name, BindingFlags flags = BindingFlags.Default) {
			return (FieldInfo)type.GetMembers(flags).Filter(m => m.Name == name)[0];
		}

		[Test]
		public void GetMembersReturnsMethodsWithAnyScriptableAttributeOrReflectableAttribute() {
			var methods = typeof(C1).GetMembers(BindingFlags.Default);
			Assert.AreEqual(methods.Length, 2, "Should be two methods");
			Assert.IsTrue(methods[0].Name == "M2" || methods[1].Name == "M2");
			Assert.IsTrue(methods[0].Name == "M3" || methods[1].Name == "M3");
		}

		[Test]
		public void GetMemberBindingFlagsInstanceOrStaticWorks() {
			var methods = typeof(C2).GetMembers(BindingFlags.Default);
			Assert.AreEqual(methods.Length, 2, "Default should be two methods");
			Assert.IsTrue(methods[0].Name == "M1" || methods[1].Name == "M1", "M1 should be returned with default binding flags");
			Assert.IsTrue(methods[0].Name == "M2" || methods[1].Name == "M2", "M2 should be returned with default binding flags");

			methods = typeof(C2).GetMembers(BindingFlags.Instance | BindingFlags.Static);
			Assert.AreEqual(methods.Length, 2, "Instance and static should be two methods");
			Assert.IsTrue(methods[0].Name == "M1" || methods[1].Name == "M1", "M1 should be returned with both instance and static binding flags");
			Assert.IsTrue(methods[0].Name == "M2" || methods[1].Name == "M2", "M2 should be returned with both instance and static binding flags");

			methods = typeof(C2).GetMembers(BindingFlags.Instance);
			Assert.AreEqual(methods.Length, 1, "Should be one instance method");
			Assert.IsTrue(methods[0].Name == "M1" || methods[1].Name == "M1", "Instance method should be m1");

			methods = typeof(C2).GetMembers(BindingFlags.Static);
			Assert.AreEqual(methods.Length, 1, "Should be one static method");
			Assert.IsTrue(methods[0].Name == "M2", "Static method should be M2");
		}

		[Test]
		public void IsStaticFlagWorksForMethod() {
			Assert.AreStrictEqual(((MethodInfo)typeof(C2).GetMembers(BindingFlags.Instance)[0]).IsStatic, false, "Instance member should not be static");
			Assert.AreStrictEqual(((MethodInfo)typeof(C2).GetMembers(BindingFlags.Static)[0]).IsStatic, true, "Static member should be static");
		}

		[Test]
		public void MemberTypeIsMethodForMethod() {
			Assert.AreStrictEqual(GetMethod(typeof(C3), "M1").MemberType, MemberTypes.Method);
		}

		[Test]
		public void IsConstructorIsFalseForMethod() {
			Assert.AreStrictEqual(GetMethod(typeof(C3), "M1").IsConstructor, false);
		}

		[Test]
		public void IsConstructorIsTrueForAllKindsOfConstructors() {
			var c10 = typeof(C10).GetMembers();
			var c11 = typeof(C11).GetMembers();
			Assert.IsTrue(((ConstructorInfo)c10[0]).IsConstructor, "Unnamed");
			Assert.IsTrue(((ConstructorInfo)c10[1]).IsConstructor, "Named");
			Assert.IsTrue(((ConstructorInfo)c11[0]).IsConstructor, "Static method");
		}

		[Test]
		public void IsStaticIsFalseForAllKindsOfConstructors() {
			var c10 = typeof(C10).GetMembers();
			var c11 = typeof(C11).GetMembers();
			Assert.IsTrue(((ConstructorInfo)c10[0]).IsConstructor, "Unnamed");
			Assert.IsTrue(((ConstructorInfo)c10[1]).IsConstructor, "Named");
			Assert.IsTrue(((ConstructorInfo)c11[0]).IsConstructor, "Static method");
		}

		[Test]
		public void MemberTypeIsConstructorForAllKindsOfConstructors() {
			var c10 = typeof(C10).GetMembers();
			var c11 = typeof(C11).GetMembers();
			Assert.AreEqual(c10[0].MemberType, MemberTypes.Constructor, "Unnamed");
			Assert.AreEqual(c10[1].MemberType, MemberTypes.Constructor, "Named");
			Assert.AreEqual(c11[0].MemberType, MemberTypes.Constructor, "Static method");
		}

		[Test]
		public void NameIsCtorForAllKindsOfConstructors() {
			var c10 = typeof(C10).GetMembers();
			var c11 = typeof(C11).GetMembers();
			Assert.AreEqual(c10[0].Name, ".ctor", "Unnamed");
			Assert.AreEqual(c10[1].Name, ".ctor", "Named");
			Assert.AreEqual(c11[0].Name, ".ctor", "Static method");
		}

		[Test]
		public void DeclaringTypeIsCorrectForAllKindsOfConstructors() {
			var c10 = typeof(C10).GetMembers();
			var c11 = typeof(C11).GetMembers();
			Assert.AreEqual(c10[0].DeclaringType, typeof(C10), "Unnamed");
			Assert.AreEqual(c10[1].DeclaringType, typeof(C10), "Named");
			Assert.AreEqual(c11[0].DeclaringType, typeof(C11), "Static method");
		}

		[Test]
		public void DeclaringTypeShouldBeCorrectForMethods() {
			Assert.AreStrictEqual(GetMethod(typeof(C3), "M1").DeclaringType, typeof(C3), "Simple type");
			Assert.AreStrictEqual(GetMethod(typeof(C5<,>), "M").DeclaringType, typeof(C5<,>), "Open generic type");
			Assert.AreStrictEqual(GetMethod(typeof(C5<int,string>), "M").DeclaringType, typeof(C5<int,string>), "Constructed generic type");
		}

		[Test]
		public void ReturnTypeAndParameterTypesAreCorrectForMethods() {
			var m1 = GetMethod(typeof(C3), "M1");
			Assert.AreEqual(m1.ReturnType, typeof(int), "Return type should be int");
			Assert.AreEqual(m1.ParameterTypes.Length, 0, "M1 should have no parameters");

			var m2 = GetMethod(typeof(C3), "M2");
			Assert.AreEqual(m2.ParameterTypes, new[] { typeof(string) }, "M2 parameter types should be correct");

			var m3 = GetMethod(typeof(C3), "M3");
			Assert.AreEqual(m3.ParameterTypes, new[] { typeof(string), typeof(int) }, "M3 parameter types should be correct");
		}

		[Test]
		public void ParameterTypesShouldBeCorrectForConstructors() {
			var c10 = typeof(C10).GetMembers();
			var c11 = typeof(C11).GetMembers();
			Assert.AreEqual(((ConstructorInfo)c10[0]).ParameterTypes, new[] { typeof(int) }, "Unnamed");
			Assert.AreEqual(((ConstructorInfo)c10[1]).ParameterTypes, new[] { typeof(int), typeof(string) }, "Named");
			Assert.AreEqual(((ConstructorInfo)c11[0]).ParameterTypes, new[] { typeof(DateTime) }, "Static method");
		}

		[Test]
		public void VoidIsConsideredObjectAsReturnType() {
			Assert.AreStrictEqual(GetMethod(typeof(C3), "M4").ReturnType, typeof(object), "Return type of void method should be object");
		}

		[Test]
		public void MethodNameIsTheCSharp() {
			var members = (MethodInfo[])typeof(C4).GetMembers();
			Assert.AreEqual(members.Filter(m => m.Name == "M").Length, 3, "All methods should have name M");
		}

		[Test]
		public void InstanceMethodForSerializableTypeIsConsideredStaticWithExtraParameter() {
			var m1 = GetMethod(typeof(C7), "M1");
			Assert.IsTrue(m1.IsStatic, "M1 should be static");
			Assert.AreEqual(m1.ParameterTypes, new[] { typeof(C7), typeof(int) }, "M1 parameters should be correct");

			var m2 = GetMethod(typeof(C7), "M2");
			Assert.IsTrue(m2.IsStatic, "M2 should be static");
			Assert.AreEqual(m2.ParameterTypes, new[] { typeof(string) }, "M2 parameters should be correct");
		}

		[Test]
		public void TypeParametersAreReplacedWithObjectForReturnAndParameterTypesForOpenGenericTypes() {
			var m = GetMethod(typeof(C5<,>), "M");
			Assert.AreEqual(m.ReturnType, typeof(object), "Return type should be object");
			Assert.AreEqual(m.ParameterTypes, new[] { typeof(object), typeof(string) }, "Parameters should be correct");
		}

		[Test]
		public void TypeParametersAreCorrectForReturnAndParameterTypesForConstructedGenericTypes() {
			var m = GetMethod(typeof(C5<string,DateTime>), "M");
			Assert.AreEqual(m.ReturnType, typeof(string), "Return type of M should be string");
			Assert.AreEqual(m.ParameterTypes, new[] { typeof(DateTime), typeof(string) }, "Parameters to M should be correct");

			var m2 = GetMethod(typeof(C5<string,DateTime>), "M2");
			Assert.AreEqual(m2.ReturnType, typeof(object), "Return type of M2 should be object");
			Assert.AreEqual(m2.ParameterTypes.Length, 0, "M2 should not have any parameters");
		}

		[Test]
		public void MethodTypeParametersAreReplacedWithObjectForReturnAndParameterTypes() {
			var m = GetMethod(typeof(C6), "M1");
			Assert.AreEqual(m.ReturnType, typeof(object), "Return type should be object");
			Assert.AreEqual(m.ParameterTypes, new[] { typeof(object), typeof(string) }, "Parameters should be correct");
		}

		[Test]
		public void IsGenericMethodDefinitionAndTypeParameterCountWork() {
			Assert.IsTrue (GetMethod(typeof(C6), "M1").IsGenericMethodDefinition, "M1 should be generic");
			Assert.IsTrue (GetMethod(typeof(C6), "M2").IsGenericMethodDefinition, "M2 should be generic");
			Assert.IsFalse(GetMethod(typeof(C6), "M3").IsGenericMethodDefinition, "M3 should not be generic");
			Assert.AreStrictEqual(GetMethod(typeof(C6), "M1").TypeParameterCount, 2, "M1 should have 2 type parameters");
			Assert.AreStrictEqual(GetMethod(typeof(C6), "M2").TypeParameterCount, 1, "M2 should have 1 type parameters");
			Assert.AreStrictEqual(GetMethod(typeof(C6), "M3").TypeParameterCount, 0, "M3 should have 0 type parameters");
		}

		[Test]
		public void CreateDelegateWorksForNonGenericInstanceMethods() {
			var m = GetMethod(typeof(C8), "M1");
			var c = new C8("X");
			var f1 = (Func<string, string, string>)m.CreateDelegate(typeof(Func<string, string, string>), c);
			var f2 = (Func<string, string, string>)m.CreateDelegate(c);
			Assert.AreEqual(f1("a", "b"), "X a b", "Delegate created with delegate type should be correct");
			Assert.AreEqual(f2("c", "d"), "X c d", "Delegate created without delegate type should be correct");
			Assert.Throws(() => m.CreateDelegate(typeof(Func<string, string, string>)), "Without target with delegate type should throw");
			Assert.Throws(() => m.CreateDelegate(), "Without target without delegate type should throw");
			Assert.Throws(() => m.CreateDelegate(typeof(Func<string, string, string>), (object)null), "Null target with delegate type should throw");
			Assert.Throws(() => m.CreateDelegate((object)null), "Null target without delegate type should throw");
			Assert.Throws(() => m.CreateDelegate(c, new[] { typeof(string) }), "With type arguments with target should throw");
			Assert.Throws(() => m.CreateDelegate(new[] { typeof(string) }), "With type arguments without target should throw");
			Assert.Throws(() => m.CreateDelegate((object)null, new[] { typeof(string) }), "With type arguments with null target should throw");
		}

		[Test]
		public void CreateDelegateWorksNonGenericStaticMethods() {
			var m = GetMethod(typeof(C8), "M2");
			var f1 = (Func<string, string, string>)m.CreateDelegate(typeof(Func<string, string, string>));
			var f2 = (Func<string, string, string>)m.CreateDelegate();
			var f3 = (Func<string, string, string>)m.CreateDelegate(typeof(Func<string, string, string>), (object)null);
			var f4 = (Func<string, string, string>)m.CreateDelegate((object)null);
			Assert.AreEqual(f1("a", "b"), "a b", "Delegate created with delegate type without target should be correct");
			Assert.AreEqual(f2("c", "d"), "c d", "Delegate created without delegate type without target should be correct");
			Assert.AreEqual(f3("e", "f"), "e f", "Delegate created with delegate type with null target should be correct");
			Assert.AreEqual(f4("g", "h"), "g h", "Delegate created without delegate type with null target should be correct");
			Assert.Throws(() => m.CreateDelegate(typeof(Func<string, string, string>), new C8("")), "With target with delegate type should throw");
			Assert.Throws(() => m.CreateDelegate(new C8("")), "With target without delegate type should throw");
			Assert.Throws(() => m.CreateDelegate(new C8(""), new[] { typeof(string) }), "With type arguments with target should throw");
			Assert.Throws(() => m.CreateDelegate(new[] { typeof(string) }), "With type arguments without target should throw");
			Assert.Throws(() => m.CreateDelegate((object)null, new[] { typeof(string) }), "With type arguments with null target should throw");
		}

		[Test]
		public void CreateDelegateWorksNonGenericStaticMethodOfGenericType() {
			var m = GetMethod(typeof(C9<int, string>), "M");
			var f = (Func<string, string>)m.CreateDelegate();
			Assert.AreEqual(f("a"), "ss.Int32 String a", "Delegate should return correct results");
		}

		[Test]
		public void CreateDelegateWorksForGenericInstanceMethods() {
			var m = GetMethod(typeof(C8), "M3");
			var c = new C8("X");
			var f = (Func<string, string>)m.CreateDelegate(c, new[] { typeof(int), typeof(string) });
			Assert.AreEqual(f("a"), "X ss.Int32 String a", "Result of invoking delegate should be correct");
			Assert.Throws(() => m.CreateDelegate((object)null, new[] { typeof(int), typeof(string) }), "Null target with correct type arguments should throw");
			Assert.Throws(() => m.CreateDelegate(c), "No type arguments with target should throw");
			Assert.Throws(() => m.CreateDelegate(c, new Type[0]), "0 type arguments with target should throw");
			Assert.Throws(() => m.CreateDelegate(c, new Type[1]), "1 type arguments with target should throw");
			Assert.Throws(() => m.CreateDelegate(c, new Type[3]), "3 type arguments with target should throw");
		}

		[Test]
		public void CreateDelegateWorksForGenericStaticMethods() {
			var m = GetMethod(typeof(C8), "M4");
			var f = (Func<string, string>)m.CreateDelegate((object)null, new[] { typeof(int), typeof(string) });
			Assert.AreEqual(f("a"), "ss.Int32 String a", "Result of invoking delegate should be correct");
			Assert.Throws(() => m.CreateDelegate(new C8(""), new[] { typeof(int), typeof(string) }), "Target with correct type arguments should throw");
			Assert.Throws(() => m.CreateDelegate((object)null), "No type arguments without target should throw");
			Assert.Throws(() => m.CreateDelegate((object)null, new Type[0]), "0 type arguments without target should throw");
			Assert.Throws(() => m.CreateDelegate((object)null, new Type[1]), "1 type arguments without target should throw");
			Assert.Throws(() => m.CreateDelegate((object)null, new Type[3]), "3 type arguments without target should throw");
		}

		[Test]
		public void InvokeWorksForNonGenericInstanceMethods() {
			var m = GetMethod(typeof(C8), "M1");
			var c = new C8("X");
			Assert.AreEqual(m.Invoke(c, "a", "b"), "X a b", "Invoke with target should work");
			Assert.Throws(() => m.Invoke(null, "a", "b"), "Invoke without target should throw");
			Assert.Throws(() => m.Invoke(c, new[] { typeof(string) }, "a", "b"), "Invoke with type arguments with target should throw");
			Assert.Throws(() => m.Invoke(null, new[] { typeof(string) }, "a", "b"), "Invoke with type arguments without target should throw");
		}

		[Test]
		public void InvokeWorksNonGenericStaticMethods() {
			var m = GetMethod(typeof(C8), "M2");
			Assert.AreEqual(m.Invoke(null, "a", "b"), "a b", "Invoke without target should work");
			Assert.Throws(() => m.Invoke(new C8(""), "a", "b"), "Invoke with target should throw");
			Assert.Throws(() => m.Invoke(new C8(""), new[] { typeof(string) }, "a", "b"), "Invoke with type arguments with target should throw");
			Assert.Throws(() => m.Invoke(null, new[] { typeof(string) }, "a", "b"), "Invoke with type arguments without target should throw");
		}

		[Test]
		public void InvokeWorksForGenericInstanceMethod() {
			var m = GetMethod(typeof(C8), "M3");
			var c = new C8("X");
			Assert.AreEqual(m.Invoke(c, new[] { typeof(int), typeof(string) }, "a"), "X ss.Int32 String a", "Result of invoking delegate should be correct");
			Assert.Throws(() => m.Invoke(null, new[] { typeof(int), typeof(string) }, "a"), "Null target with correct type arguments should throw");
			Assert.Throws(() => m.Invoke(c, "a"), "No type arguments with target should throw");
			Assert.Throws(() => m.Invoke(c, new Type[0], "a"), "0 type arguments with target should throw");
			Assert.Throws(() => m.Invoke(c, new Type[1], "a"), "1 type arguments with target should throw");
			Assert.Throws(() => m.Invoke(c, new Type[3], "a"), "3 type arguments with target should throw");
		}

		[Test]
		public void InvokeWorksForGenericStaticMethod() {
			var m = GetMethod(typeof(C8), "M4");
			Assert.AreEqual(m.Invoke(null, new[] { typeof(int), typeof(string) }, "a"), "ss.Int32 String a", "Result of invoking delegate should be correct");
			Assert.Throws(() => m.Invoke(new C8(""), new[] { typeof(int), typeof(string) }, "a"), "Target with correct type arguments should throw");
			Assert.Throws(() => m.Invoke(null, "a"), "No type arguments without target should throw");
			Assert.Throws(() => m.Invoke(null, new Type[0], "a"), "0 type arguments without target should throw");
			Assert.Throws(() => m.Invoke(null, new Type[1], "a"), "1 type arguments without target should throw");
			Assert.Throws(() => m.Invoke(null, new Type[3], "a"), "3 type arguments without target should throw");
		}

		[Test]
		public void InvokeWorksForAllKindsOfConstructors() {
			var c1 = (ConstructorInfo)typeof(C10).GetMembers().Filter(m => ((ConstructorInfo)m).ParameterTypes.Length == 1)[0];
			var o1 = (C10)c1.Invoke(42);
			Assert.AreEqual(o1.X, 42, "o1.X");
			Assert.AreEqual(o1.S, "X", "o1.S");

			var c2 = (ConstructorInfo)typeof(C10).GetMembers().Filter(m => ((ConstructorInfo)m).ParameterTypes.Length == 2)[0];
			var o2 = (C10)c2.Invoke(14, "Hello");
			Assert.AreEqual(o2.X, 14, "o2.X");
			Assert.AreEqual(o2.S, "Hello", "o2.S");

			var c3 = (ConstructorInfo)typeof(C11).GetMembers()[0];
			var o3 = (C11)c3.Invoke(new DateTime(2012, 1, 2));
			Assert.AreEqual(o3.D, new DateTime(2012, 1, 2), "o3.D");
		}

		[Test]
		public void MemberTypeIsFieldForField() {
			Assert.AreStrictEqual(GetField(typeof(C12), "F1").MemberType, MemberTypes.Field, "Instance");
			Assert.AreStrictEqual(GetField(typeof(C12), "F3").MemberType, MemberTypes.Field, "Static");
		}

		[Test]
		public void DeclaringTypeIsCorrectForField() {
			Assert.AreStrictEqual(GetField(typeof(C12), "F1").DeclaringType, typeof(C12), "Instance");
			Assert.AreStrictEqual(GetField(typeof(C12), "F3").DeclaringType, typeof(C12), "Static");
		}

		[Test]
		public void NameIsCorrectForField() {
			Assert.AreStrictEqual(GetField(typeof(C12), "F1").Name, "F1", "Instance");
			Assert.AreStrictEqual(GetField(typeof(C12), "F3").Name, "F3", "Static");
		}

		[Test]
		public void FieldTypeIsCorrectForField() {
			Assert.AreStrictEqual(GetField(typeof(C12), "F1").FieldType, typeof(int), "Instance 1");
			Assert.AreStrictEqual(GetField(typeof(C12), "F2").FieldType, typeof(DateTime), "Instance 2");
			Assert.AreStrictEqual(GetField(typeof(C12), "F3").FieldType, typeof(string), "Static");
		}

		[Test]
		public void IsStaticIsCorrectForField() {
			Assert.AreStrictEqual(GetField(typeof(C12), "F1").IsStatic, false, "Instance 1");
			Assert.AreStrictEqual(GetField(typeof(C12), "F2").IsStatic, false, "Instance 2");
			Assert.AreStrictEqual(GetField(typeof(C12), "F3").IsStatic, true, "Static");
		}

		[Test]
		public void GetValueWorksForInstanceField() {
			var c = new C12 { F1 = 42 };
			Assert.AreEqual(GetField(typeof(C12), "F1").GetValue(c), 42);
		}

		[Test]
		public void GetValueWorksForStaticField() {
			C12.F3 = "X_Test";
			Assert.AreEqual(GetField(typeof(C12), "F3").GetValue(null), "X_Test");
		}

		[Test]
		public void SetValueWorksForInstanceField() {
			var c = new C12();
			GetField(typeof(C12), "F1").SetValue(c, 14);
			Assert.AreEqual(c.F1, 14);
		}

		[Test]
		public void SetValueWorksForStaticField() {
			GetField(typeof(C12), "F3").SetValue(null, "Hello, world");
			Assert.AreEqual(C12.F3, "Hello, world");
		}
	}
}
