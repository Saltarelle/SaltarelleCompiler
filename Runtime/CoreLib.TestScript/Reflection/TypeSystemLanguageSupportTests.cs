using System;
using System.Runtime.CompilerServices;
using QUnit;

#pragma warning disable 184, 458, 1720

namespace CoreLib.TestScript.Reflection {
	[TestFixture]
	public class TypeSystemLanguageSupportTests {
		public class C1 {}
		[IncludeGenericArguments]
		public class C2<T> {}
		public interface I1 {}
		[IncludeGenericArguments]
		public interface I2<T1> {}
		public interface I3 : I1 {}
		public interface I4 {}
		[IncludeGenericArguments]
		public interface I5<T1> : I2<T1> {}
		public class D1 : C1, I1 {}
		[IncludeGenericArguments]
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

		[Serializable]
		public class BS {
			public int X;
			public BS(int x) {
				X = x;
			}
		}

		[Serializable(TypeCheckCode = "{$System.Script}.isValue({this}.y)")]
		public class DS : BS {
			public DS() : base(0) {}
		}

		[Imported(TypeCheckCode = "{$System.Script}.isValue({this}.y)")]
		public class CI {
		}

		[IncludeGenericArguments]
		private static bool CanConvert<T>(object arg) {
			try {
				#pragma warning disable 219	// The variable `x' is assigned but its value is never used
				var x = (T)arg;
				#pragma warning restore 219
				return true;
			}
			catch {
				return false;
			}
		}
		
		[Test]
		public void TypeIsWorksForReferenceTypes() {
			Assert.IsFalse((object)new object() is C1, "#1");
			Assert.IsTrue ((object)new C1() is object, "#2");
			Assert.IsFalse((object)new object() is I1, "#3");
			Assert.IsFalse((object)new C1() is D1, "#4");
			Assert.IsTrue ((object)new D1() is C1, "#5");
			Assert.IsTrue ((object)new D1() is I1, "#6");
			Assert.IsTrue ((object)new D2<int>() is C2<int>, "#7");
			Assert.IsFalse((object)new D2<int>() is C2<string>, "#8");
			Assert.IsTrue ((object)new D2<int>() is I2<int>, "#9");
			Assert.IsFalse((object)new D2<int>() is I2<string>, "#10");
			Assert.IsTrue ((object)new D2<int>() is I1, "#11");
			Assert.IsFalse((object)new D3() is C2<string>, "#12");
			Assert.IsTrue ((object)new D3() is C2<int>, "#13");
			Assert.IsFalse((object)new D3() is I2<int>, "#14");
			Assert.IsTrue ((object)new D3() is I2<string>, "#15");
			Assert.IsTrue ((object)new D4() is I1, "#16");
			Assert.IsTrue ((object)new D4() is I3, "#17");
			Assert.IsTrue ((object)new D4() is I4, "#18");
			Assert.IsTrue ((object)new X2() is I1, "#19");
			Assert.IsTrue ((object)new E2() is E1, "#20");
			Assert.IsTrue ((object)new E1() is int, "#21");
			Assert.IsTrue ((object)new E1() is object, "#22");
			Assert.IsFalse((object)null is object, "#23");
		}

		[Test]
		public void TypeAsWorksForReferenceTypes() {
			Assert.IsFalse(((object)new object() as C1) != null, "#1");
			Assert.IsTrue (((object)new C1() as object) != null, "#2");
			Assert.IsFalse(((object)new object() as I1) != null, "#3");
			Assert.IsFalse(((object)new C1() as D1) != null, "#4");
			Assert.IsTrue (((object)new D1() as C1) != null, "#5");
			Assert.IsTrue (((object)new D1() as I1) != null, "#6");
			Assert.IsTrue(((object)new D2<int>() as C2<int>) != null, "#7");
			Assert.IsFalse(((object)new D2<int>() as C2<string>) != null, "#8");
			Assert.IsTrue (((object)new D2<int>() as I2<int>) != null, "#9");
			Assert.IsFalse(((object)new D2<int>() as I2<string>) != null, "#10");
			Assert.IsTrue (((object)new D2<int>() as I1) != null, "#11");
			Assert.IsFalse(((object)new D3() as C2<string>) != null, "#12");
			Assert.IsTrue (((object)new D3() as C2<int>) != null, "#13");
			Assert.IsFalse(((object)new D3() as I2<int>) != null, "#14");
			Assert.IsTrue (((object)new D3() as I2<string>) != null, "#15");
			Assert.IsTrue (((object)new D4() as I1) != null, "#16");
			Assert.IsTrue (((object)new D4() as I3) != null, "#17");
			Assert.IsTrue (((object)new D4() as I4) != null, "#18");
			Assert.IsTrue (((object)new X2() as I1) != null, "#19");
			Assert.IsTrue (((object)new E2() as E1?) != null, "#20");
			Assert.IsTrue (((object)new E1() as int?) != null, "#21");
			Assert.IsTrue (((object)new E1() as object) != null, "#22");
			Assert.IsFalse(((object)null as object) != null, "#23");
		}

		[Test]
		public void CastWorksForReferenceTypes() {
			Assert.IsFalse(CanConvert<C1>(new object()), "#1");
			Assert.IsTrue (CanConvert<object>(new C1()), "#2");
			Assert.IsFalse(CanConvert<I1>(new object()), "#3");
			Assert.IsFalse(CanConvert<D1>(new C1()), "#4");
			Assert.IsTrue (CanConvert<C1>(new D1()), "#5");
			Assert.IsTrue (CanConvert<I1>(new D1()), "#6");
			Assert.IsTrue (CanConvert<C2<int>>(new D2<int>()), "#7");
			Assert.IsFalse(CanConvert<C2<string>>(new D2<int>()), "#8");
			Assert.IsTrue (CanConvert<I2<int>>(new D2<int>()), "#9");
			Assert.IsFalse(CanConvert<I2<string>>(new D2<int>()), "#10");
			Assert.IsTrue (CanConvert<I1>(new D2<int>()), "#11");
			Assert.IsFalse(CanConvert<C2<string>>(new D3()), "#12");
			Assert.IsTrue (CanConvert<C2<int>>(new D3()), "#13");
			Assert.IsFalse(CanConvert<I2<int>>(new D3()), "#14");
			Assert.IsTrue (CanConvert<I2<string>>(new D3()), "#15");
			Assert.IsTrue (CanConvert<I1>(new D4()), "#16");
			Assert.IsTrue (CanConvert<I3>(new D4()), "#17");
			Assert.IsTrue (CanConvert<I4>(new D4()), "#18");
			Assert.IsTrue (CanConvert<I1>(new X2()), "#19");
			Assert.IsTrue (CanConvert<E1>(new E2()), "#20");
			Assert.IsTrue (CanConvert<int>(new E1()), "#21");
			Assert.IsTrue (CanConvert<object>(new E1()), "#22");
			Assert.IsTrue (CanConvert<object>(null), "#23");
		}

		[Test]
		public void GetTypeWorksOnObjects() {
			Action a = () => {};
			Assert.AreEqual(new C1().GetType().FullName, "CoreLib.TestScript.Reflection.TypeSystemLanguageSupportTests$C1");
			Assert.AreEqual(new C2<int>().GetType().FullName, "CoreLib.TestScript.Reflection.TypeSystemLanguageSupportTests$C2$1[ss.Int32]");
			Assert.AreEqual(new C2<string>().GetType().FullName, "CoreLib.TestScript.Reflection.TypeSystemLanguageSupportTests$C2$1[String]");
			Assert.AreEqual((1).GetType().FullName, "Number");
			Assert.AreEqual("X".GetType().FullName, "String");
			Assert.AreEqual(a.GetType().FullName, "Function");
			Assert.AreEqual(new object().GetType().FullName, "Object");
			Assert.AreEqual(new[] { 1, 2 }.GetType().FullName, "Array");
		}

		[Test]
		public void GetTypeOnNullInstanceThrowsException() {
			Assert.Throws(() => ((object)null).GetType());
		}

#pragma warning disable 219
		[Test]
		public void CastOperatorsWorkForSerializableTypesWithCustomTypeCheckCode() {
			object o1 = new { x = 1 };
			object o2 = new { x = 1, y = 2 };
			Assert.IsFalse(o1 is DS, "o1 should not be of type");
			Assert.IsTrue (o2 is DS, "o2 should be of type");
			Assert.AreStrictEqual(o1 as DS, null, "Try cast o1 to type should be null");
			Assert.IsTrue((o2 as DS) == o2, "Try cast o2 to type should return o2");
			Assert.Throws(() => { object x = (DS)o1; }, "Cast o1 to type should throw");
			Assert.IsTrue((DS)o2 == o2, "Cast o2 to type should return o2");
		}

		[Test]
		public void CastOperatorsWorkForImportedTypesWithCustomTypeCheckCode() {
			object o1 = new { x = 1 };
			object o2 = new { x = 1, y = 2 };
			Assert.IsFalse(o1 is CI, "o1 should not be of type");
			Assert.IsTrue (o2 is CI, "o2 should be of type");
			Assert.AreStrictEqual(o1 as CI, null, "Try cast o1 to type should be null");
			Assert.IsTrue((o2 as CI) == o2, "Try cast o2 to type should return o2");
			Assert.Throws(() => { object x = (DS)o1; }, "Cast o1 to type should throw");
			Assert.IsTrue((CI)o2 == o2, "Cast o2 to type should return o2");
		}
#pragma warning restore 219
	}
}
