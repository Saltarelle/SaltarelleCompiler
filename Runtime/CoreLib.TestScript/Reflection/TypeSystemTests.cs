using System.Collections.Generic;
using System.Runtime.CompilerServices;
using QUnit;
using System;

namespace CoreLib.TestScript.Reflection {
	[TestFixture]
	public class TypeSystemTests {
		public class ClassWithExpandParamsCtor {
			public object[] CtorArgs;
			[ExpandParams]
			public ClassWithExpandParamsCtor(params object[] args) {
				this.CtorArgs = args;
			}
		}

		public interface I1 {}
		public interface I2 : I1 {}
		public interface I3 {}
		public interface I4 : I3 {}
		public class B : I2 {}
		public class C : B, I4 {}

		[IncludeGenericArguments]
		public interface IG<T> {}
		[IncludeGenericArguments]
		public class BX<T> {}
		[IncludeGenericArguments]
		public class G<T1, T2> : BX<G<T1,C>>, IG<G<T2,string>> {
			public static string field;
			static G() {
				field = typeof(T1).FullName + " " + typeof(T2).FullName;
			}
		}

		public enum E1 {}
		[Flags]
		public enum E2 {}

		[Imported] public interface IImported {}

		[Serializable]
		public class BS {
			public int X;
			public BS(int x) {
				X = x;
			}
		}

		public class DS : BS {
			public int GetX() { return X; }
			public DS(int x) : base(x) {
			}
		}

		public class CS2 : Record {
			public int X;
		}

		[Serializable(TypeCheckCode = "{$System.Script}.isValue({this}.y)")]
		public class DS2 : BS {
			public DS2() : base(0) {}
		}

		[Test]
		public void FullNamePropertyReturnsTheNameWithTheNamespace() {
			Assert.AreEqual(typeof(TypeSystemTests).FullName, "CoreLib.TestScript.Reflection.TypeSystemTests");
		}

		[Test]
		public void InstantiatingClassWithConstructorThatNeedsToBeAppliedWorks() {
			var args = new List<object> { 42, "x", 18 };
			var obj = new ClassWithExpandParamsCtor(args.ToArray());

			Assert.AreEqual(obj.CtorArgs, args);
			Assert.AreEqual(obj.GetType(), typeof(ClassWithExpandParamsCtor));
		}

		[Test]
		public void NamePropertyRemovesTheNamespace() {
			Assert.AreEqual(typeof(TypeSystemTests).Name, "TypeSystemTests", "non-generic");
			Assert.AreEqual(typeof(G<int, string>).Name, "TypeSystemTests$G$2[ss.Int32,String]", "generic");
			Assert.AreEqual(typeof(G<BX<double>, string>).Name, "TypeSystemTests$G$2[CoreLib.TestScript.Reflection.TypeSystemTests$BX$1[Number],String]", "nested generic");
		}

		[Test]
		public void GettingBaseTypeWorks() {
			Assert.AreStrictEqual(typeof(B).BaseType, typeof(object));
			Assert.AreStrictEqual(typeof(C).BaseType, typeof(B));
			Assert.AreStrictEqual(typeof(object).BaseType, null);
		}

		[Test]
		public void GettingImplementedInterfacesWorks() {
			var ifs = typeof(C).GetInterfaces();
			Assert.AreEqual(ifs.Length, 4);
			Assert.IsTrue(ifs.Contains(typeof(I1)));
			Assert.IsTrue(ifs.Contains(typeof(I2)));
			Assert.IsTrue(ifs.Contains(typeof(I3)));
			Assert.IsTrue(ifs.Contains(typeof(I4)));
		}

		[Test]
		public void TypeOfAnOpenGenericClassWorks() {
			Assert.AreEqual(typeof(G<,>).FullName, "CoreLib.TestScript.Reflection.TypeSystemTests$G$2");
		}

		[Test]
		public void TypeOfAnOpenGenericInterfaceWorks() {
			Assert.AreEqual(typeof(IG<>).FullName, "CoreLib.TestScript.Reflection.TypeSystemTests$IG$1");
		}

		[Test]
		public void TypeOfInstantiatedGenericClassWorks() {
			Assert.AreEqual(typeof(G<int,C>).FullName, "CoreLib.TestScript.Reflection.TypeSystemTests$G$2[ss.Int32,CoreLib.TestScript.Reflection.TypeSystemTests$C]");
		}

		[Test]
		public void TypeOfInstantiatedGenericInterfaceWorks() {
			Assert.AreEqual(typeof(IG<int>).FullName, "CoreLib.TestScript.Reflection.TypeSystemTests$IG$1[ss.Int32]");
		}

		[Test]
		public void ConstructingAGenericTypeTwiceWithTheSameArgumentsReturnsTheSameInstance() {
			var t1 = typeof(G<int,C>);
			var t2 = typeof(G<C,int>);
			var t3 = typeof(G<int,C>);
			Assert.IsFalse(t1 == t2);
			Assert.IsTrue (t1 == t3);
		}

		[Test]
		public void AccessingAStaticMemberInAGenericClassWorks() {
			Assert.AreEqual(G<int,C>.field, "ss.Int32 CoreLib.TestScript.Reflection.TypeSystemTests$C");
			Assert.AreEqual(G<C,int>.field, "CoreLib.TestScript.Reflection.TypeSystemTests$C ss.Int32");
			Assert.AreEqual(G<G<C,int>,G<string,C>>.field, "CoreLib.TestScript.Reflection.TypeSystemTests$G$2[CoreLib.TestScript.Reflection.TypeSystemTests$C,ss.Int32] CoreLib.TestScript.Reflection.TypeSystemTests$G$2[String,CoreLib.TestScript.Reflection.TypeSystemTests$C]");
		}

		[Test]
		public void TypeOfNestedGenericClassWorks() {
			Assert.AreEqual(typeof(G<int,G<C,IG<string>>>).FullName, "CoreLib.TestScript.Reflection.TypeSystemTests$G$2[ss.Int32,CoreLib.TestScript.Reflection.TypeSystemTests$G$2[CoreLib.TestScript.Reflection.TypeSystemTests$C,CoreLib.TestScript.Reflection.TypeSystemTests$IG$1[String]]]");
		}

		[Test]
		public void BaseTypeAndImplementedInterfacesForGenericTypeWorks() {
			Assert.AreEqual(typeof(G<int,G<C,IG<string>>>).BaseType.FullName, "CoreLib.TestScript.Reflection.TypeSystemTests$BX$1[CoreLib.TestScript.Reflection.TypeSystemTests$G$2[ss.Int32,CoreLib.TestScript.Reflection.TypeSystemTests$C]]");
			Assert.AreEqual(typeof(G<int,G<C,IG<string>>>).GetInterfaces()[0].FullName, "CoreLib.TestScript.Reflection.TypeSystemTests$IG$1[CoreLib.TestScript.Reflection.TypeSystemTests$G$2[CoreLib.TestScript.Reflection.TypeSystemTests$G$2[CoreLib.TestScript.Reflection.TypeSystemTests$C,CoreLib.TestScript.Reflection.TypeSystemTests$IG$1[String]],String]]");
		}

		[Test]
		public void IsGenericTypeDefinitionWorksAsExpected() {
			Assert.IsTrue (typeof(G<,>).IsGenericTypeDefinition);
			Assert.IsFalse(typeof(G<int,string>).IsGenericTypeDefinition);
			Assert.IsFalse(typeof(C).IsGenericTypeDefinition);
			Assert.IsTrue (typeof(IG<>).IsGenericTypeDefinition);
			Assert.IsFalse(typeof(IG<int>).IsGenericTypeDefinition);
			Assert.IsFalse(typeof(I2).IsGenericTypeDefinition);
			Assert.IsFalse(typeof(E1).IsGenericTypeDefinition);
		}

		[Test]
		public void GenericParameterCountReturnsZeroForConstructedTypesAndNonZeroForOpenOnes() {
			Assert.AreEqual(typeof(G<,>).GenericParameterCount, 2);
			Assert.AreEqual(typeof(G<int,string>).GenericParameterCount, 0);
			Assert.AreEqual(typeof(C).GenericParameterCount, 0);
			Assert.AreEqual(typeof(IG<>).GenericParameterCount, 1);
			Assert.AreEqual(typeof(IG<int>).GenericParameterCount, 0);
			Assert.AreEqual(typeof(I2).GenericParameterCount, 0);
			Assert.AreEqual(typeof(E1).GenericParameterCount, 0);
		}

		[Test]
		public void GetGenericArgumentsReturnsTheCorrectTypesForConstructedTypesOtherwiseNull() {
			Assert.AreEqual(typeof(G<,>).GetGenericArguments(), null);
			Assert.AreEqual(typeof(G<int, string>).GetGenericArguments(), new[] { typeof(int), typeof(string) });
			Assert.AreEqual(typeof(C).GetGenericArguments(), null);
			Assert.AreEqual(typeof(IG<>).GetGenericArguments(), null);
			Assert.AreEqual(typeof(IG<string>).GetGenericArguments(), new[] { typeof(string) });
			Assert.AreEqual(typeof(I2).GetGenericArguments(), null);
			Assert.AreEqual(typeof(E1).GetGenericArguments(), null);
		}

		[Test]
		public void GetGenericTypeDefinitionReturnsTheGenericTypeDefinitionForConstructedTypeOtherwiseNull() {
			Assert.AreEqual(typeof(G<,>).GetGenericTypeDefinition(), null);
			Assert.AreEqual(typeof(G<int,string>).GetGenericTypeDefinition(), typeof(G<,>));
			Assert.AreEqual(typeof(C).GetGenericTypeDefinition(), null);
			Assert.AreEqual(typeof(IG<>).GetGenericTypeDefinition(), null);
			Assert.AreEqual(typeof(IG<string>).GetGenericTypeDefinition(), typeof(IG<>));
			Assert.AreEqual(typeof(I2).GetGenericTypeDefinition(), null);
			Assert.AreEqual(typeof(E1).GetGenericTypeDefinition(), null);
		}

		class IsAssignableFromTypes {
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
			[IncludeGenericArguments]
			public interface I6<out T> { }
			[IncludeGenericArguments]
			public interface I7<in T> { }
			[IncludeGenericArguments]
			public interface I8<out T1, in T2> : I6<T1>, I7<T2> { }
			[IncludeGenericArguments]
			public interface I9<T1, out T2> { }
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
			[IncludeGenericArguments]
			public class Y1<T> : I6<T> { }
			public class Y1X1 : Y1<X1> {}
			public class Y1X2 : Y1<X2> {}
			[IncludeGenericArguments]
			public class Y2<T> : I7<T> { }
			public class Y2X1 : Y2<X1> {}
			public class Y2X2 : Y2<X2> {}
			[IncludeGenericArguments]
			public class Y3<T1, T2> : I8<T1, T2> { }
			public class Y3X1X1 : Y3<X1, X1> {}
			public class Y3X1X2 : Y3<X1, X2> {}
			public class Y3X2X1 : Y3<X2, X1> {}
			public class Y3X2X2 : Y3<X2, X2> {}
			[IncludeGenericArguments]
			public class Y4<T1, T2> : I9<T1, T2> { }
		}

		[Test]
		public void IsAssignableFromWorks() {
			Assert.IsTrue (typeof(IsAssignableFromTypes.C1).IsAssignableFrom(typeof(IsAssignableFromTypes.C1)), "#1");
			Assert.IsFalse(typeof(IsAssignableFromTypes.C1).IsAssignableFrom(typeof(object)), "#2");
			Assert.IsTrue (typeof(object).IsAssignableFrom(typeof(IsAssignableFromTypes.C1)), "#3");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(object)), "#4");
			Assert.IsTrue (typeof(object).IsAssignableFrom(typeof(IsAssignableFromTypes.I1)), "#5");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I3).IsAssignableFrom(typeof(IsAssignableFromTypes.I1)), "#6");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.I3)), "#7");
			Assert.IsFalse(typeof(IsAssignableFromTypes.D1).IsAssignableFrom(typeof(IsAssignableFromTypes.C1)), "#8");
			Assert.IsTrue (typeof(IsAssignableFromTypes.C1).IsAssignableFrom(typeof(IsAssignableFromTypes.D1)), "#9");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.D1)), "#10");
			Assert.IsTrue (typeof(IsAssignableFromTypes.C2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)), "#11");
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<string>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)), "#12");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)), "#13");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I2<string>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)), "#14");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)), "#15");
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<string>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)), "#16");
			Assert.IsTrue (typeof(IsAssignableFromTypes.C2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)), "#17");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)), "#18");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I2<string>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)), "#19");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.I5<string>)), "#20");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.I5<int>)), "#21");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I5<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.I2<int>)), "#22");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.D4)), "#23");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I3).IsAssignableFrom(typeof(IsAssignableFromTypes.D4)), "#24");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I4).IsAssignableFrom(typeof(IsAssignableFromTypes.D4)), "#25");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.X2)), "#26");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I2<>).IsAssignableFrom(typeof(IsAssignableFromTypes.I5<>)), "#27");
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<>)), "#28");
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)), "#29");
			Assert.IsFalse(typeof(E1).IsAssignableFrom(typeof(E2)), "#30");
			Assert.IsFalse(typeof(int).IsAssignableFrom(typeof(E1)), "#31");
			Assert.IsTrue (typeof(object).IsAssignableFrom(typeof(E1)), "#32");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>)), "#33");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>)), "#34");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>)), "#35");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>)), "#36");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>)), "#37");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1<IsAssignableFromTypes.X1>)), "#38");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1<IsAssignableFromTypes.X1>)), "#39");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1X1)), "#40");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1<IsAssignableFromTypes.X1>)), "#41");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1X1)), "#42");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1<IsAssignableFromTypes.X2>)), "#43");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1X2)), "#44");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1<IsAssignableFromTypes.X2>)), "#45");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y1X2)), "#46");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>)), "#47");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>)), "#48");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>)), "#49");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>)), "#50");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>)), "#51");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2<IsAssignableFromTypes.X1>)), "#52");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2<IsAssignableFromTypes.X1>)), "#53");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2X1)), "#54");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2<IsAssignableFromTypes.X1>)), "#55");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2X1)), "#56");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2<IsAssignableFromTypes.X2>)), "#57");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2X2)), "#58");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2<IsAssignableFromTypes.X2>)), "#59");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y2X2)), "#60");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#61");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#62");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#63");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#64");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#65");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#66");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#67");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#68");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#69");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#70");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#71");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#72");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#73");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#74");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#75");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#76");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#77");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#78");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#79");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X1)), "#80");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#81");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X2)), "#82");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#83");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X1)), "#84");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#85");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X2)), "#86");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#87");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X1)), "#88");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#89");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X2)), "#90");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#91");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X1)), "#92");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#93");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X2)), "#94");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#95");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X1)), "#96");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#97");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X2)), "#98");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#99");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X1)), "#100");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#101");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X2)), "#102");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>)), "#103");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X1)), "#104");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>)), "#105");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X1X2)), "#106");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>)), "#107");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X1)), "#108");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>)), "#109");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y3X2X2)), "#110");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>)), "#111");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>)), "#112");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>)), "#113");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>)), "#114");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>)), "#115");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>)), "#116");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>)), "#117");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>)), "#118");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>)), "#119");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>)), "#120");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>)), "#121");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>)), "#122");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>)), "#123");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>)), "#124");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>)), "#125");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>)), "#126");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>)), "#127");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>)), "#128");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>)), "#129");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>)), "#130");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>)), "#131");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>)), "#132");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>)), "#133");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>)), "#134");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>)), "#135");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>)), "#136");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>)), "#137");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>)), "#138");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>)), "#139");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>)), "#140");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>)), "#141");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsAssignableFrom(typeof(IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>)), "#142");
		}

		class IsSubclassOfTypes {
			public class C1 {}
			[IncludeGenericArguments]
			public class C2<T> {}
			public class D1 : C1 {}
			[IncludeGenericArguments]
			public class D2<T> : C2<T> {}
			public class D3 : C2<int> {}
		}

		[Test]
		public void IsSubclassOfWorks() {
			Assert.IsFalse(typeof(IsSubclassOfTypes.C1).IsSubclassOf(typeof(IsSubclassOfTypes.C1)), "#1");
			Assert.IsTrue (typeof(IsSubclassOfTypes.C1).IsSubclassOf(typeof(object)), "#2");
			Assert.IsFalse(typeof(object).IsSubclassOf(typeof(IsSubclassOfTypes.C1)), "#3");
			Assert.IsTrue (typeof(IsSubclassOfTypes.D1).IsSubclassOf(typeof(IsSubclassOfTypes.C1)), "#4");
			Assert.IsFalse(typeof(IsSubclassOfTypes.C1).IsSubclassOf(typeof(IsSubclassOfTypes.D1)), "#5");
			Assert.IsTrue (typeof(IsSubclassOfTypes.D1).IsSubclassOf(typeof(object)), "#6");
			Assert.IsTrue (typeof(IsSubclassOfTypes.D2<int>).IsSubclassOf(typeof(IsSubclassOfTypes.C2<int>)), "#7");
			Assert.IsFalse(typeof(IsSubclassOfTypes.D2<string>).IsSubclassOf(typeof(IsSubclassOfTypes.C2<int>)), "#8");
			Assert.IsFalse(typeof(IsSubclassOfTypes.D3).IsSubclassOf(typeof(IsSubclassOfTypes.C2<string>)), "#9");
			Assert.IsTrue (typeof(IsSubclassOfTypes.D3).IsSubclassOf(typeof(IsSubclassOfTypes.C2<int>)), "#10");
			Assert.IsFalse(typeof(IsSubclassOfTypes.D2<>).IsSubclassOf(typeof(IsSubclassOfTypes.C2<>)), "#11");
			Assert.IsFalse(typeof(IsSubclassOfTypes.D3).IsSubclassOf(typeof(IsSubclassOfTypes.C2<>)), "#12");
		}

		[Test]
		public void IsClassWorks() {
			Assert.IsFalse(typeof(E1).IsClass);
			Assert.IsFalse(typeof(E2).IsClass);
			Assert.IsTrue (typeof(C).IsClass);
			Assert.IsTrue (typeof(G<,>).IsClass);
			Assert.IsTrue (typeof(G<int,string>).IsClass);
			Assert.IsFalse(typeof(I1).IsClass);
			Assert.IsFalse(typeof(IG<>).IsClass);
			Assert.IsFalse(typeof(IG<int>).IsClass);
		}

		[Test]
		public void IsEnumWorks() {
			Assert.IsTrue (typeof(E1).IsEnum);
			Assert.IsTrue (typeof(E2).IsEnum);
			Assert.IsFalse(typeof(C).IsEnum);
			Assert.IsFalse(typeof(G<,>).IsEnum);
			Assert.IsFalse(typeof(G<int,string>).IsEnum);
			Assert.IsFalse(typeof(I1).IsEnum);
			Assert.IsFalse(typeof(IG<>).IsEnum);
			Assert.IsFalse(typeof(IG<int>).IsEnum);
		}

		[Test]
		public void IsFlagsWorks() {
			Assert.IsFalse(typeof(E1).IsFlags);
			Assert.IsTrue (typeof(E2).IsFlags);
			Assert.IsFalse(typeof(C).IsFlags);
			Assert.IsFalse(typeof(G<,>).IsFlags);
			Assert.IsFalse(typeof(G<int,string>).IsFlags);
			Assert.IsFalse(typeof(I1).IsFlags);
			Assert.IsFalse(typeof(IG<>).IsFlags);
			Assert.IsFalse(typeof(IG<int>).IsFlags);
		}

		[Test]
		public void IsInterfaceWorks() {
			Assert.IsFalse(typeof(E1).IsInterface);
			Assert.IsFalse(typeof(E2).IsInterface);
			Assert.IsFalse(typeof(C).IsInterface);
			Assert.IsFalse(typeof(G<,>).IsInterface);
			Assert.IsFalse(typeof(G<int,string>).IsInterface);
			Assert.IsTrue (typeof(I1).IsInterface);
			Assert.IsTrue (typeof(IG<>).IsInterface);
			Assert.IsTrue (typeof(IG<int>).IsInterface);
		}

		[Test]
		public void IsInstanceOfTypeWorksForReferenceTypes() {
			Assert.IsFalse(Type.IsInstanceOfType(new object(), typeof(IsAssignableFromTypes.C1)), "#1");
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.C1(), typeof(object)), "#2");
			Assert.IsFalse(Type.IsInstanceOfType(new object(), typeof(IsAssignableFromTypes.I1)), "#3");
			Assert.IsFalse(Type.IsInstanceOfType(new IsAssignableFromTypes.C1(), typeof(IsAssignableFromTypes.D1)), "#4");
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D1(), typeof(IsAssignableFromTypes.C1)), "#5");
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D1(), typeof(IsAssignableFromTypes.I1)), "#6");
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.C2<int>)), "#7");
			Assert.IsFalse(Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.C2<string>)), "#8");
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.I2<int>)), "#9");
			Assert.IsFalse(Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.I2<string>)), "#0");
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.I1)), "#11");
			Assert.IsFalse(Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.C2<string>)), "#12");
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.C2<int>)), "#13");
			Assert.IsFalse(Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.I2<int>)), "#14");
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.I2<string>)), "#15");
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D4(), typeof(IsAssignableFromTypes.I1)), "#16");
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D4(), typeof(IsAssignableFromTypes.I3)), "#17");
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D4(), typeof(IsAssignableFromTypes.I4)), "#18");
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.X2(), typeof(IsAssignableFromTypes.I1)), "#19");
			Assert.IsFalse(Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.C2<>)), "#10");
			Assert.IsTrue (Type.IsInstanceOfType(new E2(), typeof(E1)), "#21");
			Assert.IsTrue (Type.IsInstanceOfType(new E1(), typeof(int)), "#22");
			Assert.IsTrue (Type.IsInstanceOfType(new E1(), typeof(object)), "#23");
			Assert.IsFalse(Type.IsInstanceOfType(null, typeof(object)), "#24");

			Assert.IsFalse(typeof(IsAssignableFromTypes.C1).IsInstanceOfType(new object()), "#25");
			Assert.IsTrue (typeof(object).IsInstanceOfType(new IsAssignableFromTypes.C1()), "#26");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new object()), "#27");
			Assert.IsFalse(typeof(IsAssignableFromTypes.D1).IsInstanceOfType(new IsAssignableFromTypes.C1()), "#28");
			Assert.IsTrue (typeof(IsAssignableFromTypes.C1).IsInstanceOfType(new IsAssignableFromTypes.D1()), "#29");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.D1()), "#30");
			Assert.IsTrue (typeof(IsAssignableFromTypes.C2<int>).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()), "#31");
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<string>).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()), "#32");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I2<int>).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()), "#33");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I2<string>).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()), "#34");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()), "#35");
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<string>).IsInstanceOfType(new IsAssignableFromTypes.D3()), "#36");
			Assert.IsTrue (typeof(IsAssignableFromTypes.C2<int>).IsInstanceOfType(new IsAssignableFromTypes.D3()), "#37");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I2<int>).IsInstanceOfType(new IsAssignableFromTypes.D3()), "#38");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I2<string>).IsInstanceOfType(new IsAssignableFromTypes.D3()), "#39");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.D4()), "#40");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I3).IsInstanceOfType(new IsAssignableFromTypes.D4()), "#41");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I4).IsInstanceOfType(new IsAssignableFromTypes.D4()), "#42");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.X2()), "#43");
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<>).IsInstanceOfType(new IsAssignableFromTypes.D3()), "#44");
			Assert.IsTrue (typeof(E1).IsInstanceOfType(new E2()), "#45");
			Assert.IsTrue (typeof(int).IsInstanceOfType(new E1()), "#46");
			Assert.IsTrue (typeof(object).IsInstanceOfType(new E1()), "#47");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y1<IsAssignableFromTypes.X1>()), "#48");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y1<IsAssignableFromTypes.X1>()), "#49");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y1X1()), "#50");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y1<IsAssignableFromTypes.X1>()), "#51");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y1X1()), "#52");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y1<IsAssignableFromTypes.X2>()), "#53");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y1X2()), "#54");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y1<IsAssignableFromTypes.X2>()), "#55");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y1X2()), "#56");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I6<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y2<IsAssignableFromTypes.X1>()), "#57");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y2<IsAssignableFromTypes.X1>()), "#58");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y2X1()), "#59");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y2<IsAssignableFromTypes.X1>()), "#60");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y2X1()), "#61");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y2<IsAssignableFromTypes.X2>()), "#62");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y2X2()), "#63");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y2<IsAssignableFromTypes.X2>()), "#64");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I7<IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y2X2()), "#65");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#66");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#67");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X1()), "#68");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>()), "#69");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X2()), "#70");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>()), "#71");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X1()), "#72");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#73");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X2()), "#74");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#75");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X1()), "#76");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>()), "#77");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X2()), "#78");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>()), "#79");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X1()), "#80");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#81");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X2()), "#82");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#83");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X1()), "#84");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>()), "#85");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X2()), "#86");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>()), "#87");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X1()), "#88");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#89");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X2()), "#90");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X1>()), "#91");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X1()), "#92");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X1, IsAssignableFromTypes.X2>()), "#93");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X1X2()), "#94");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X1>()), "#95");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X1()), "#96");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>()), "#97");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I8<IsAssignableFromTypes.X2, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y3X2X2()), "#98");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>()), "#99");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>()), "#100");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>()), "#101");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>()), "#102");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>()), "#103");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X1>()), "#104");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>()), "#105");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X1>()), "#106");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>()), "#107");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>()), "#108");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>()), "#109");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X1>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>()), "#110");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>()), "#111");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<string, IsAssignableFromTypes.X2>()), "#112");
			Assert.IsFalse(typeof(IsAssignableFromTypes.I9<string, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>()), "#113");
			Assert.IsTrue (typeof(IsAssignableFromTypes.I9<object, IsAssignableFromTypes.X2>).IsInstanceOfType(new IsAssignableFromTypes.Y4<object, IsAssignableFromTypes.X2>()), "#114");
			Assert.IsFalse(typeof(object).IsInstanceOfType(null), "#115");
		}

		public class BaseUnnamedConstructorWithoutArgumentsTypes {
			public class B {
				public string messageB;
			
				public B() {
					messageB = "X";
				}
			}
			
			public class D : B {
				public string messageD;
			
				public D() {
					messageD = "Y";
				}
			}
		}

		[Test]
		public void InvokingBaseUnnamedConstructorWithoutArgumentsWorks() {
			var d = new BaseUnnamedConstructorWithoutArgumentsTypes.D();
			Assert.AreEqual(d.messageB + "|" + d.messageD, "X|Y");
		}

		public class BaseUnnamedConstructorWithArgumentsTypes {
			public class B {
				public string messageB;
			
				public B(int x, int y) {
					messageB = x + " " + y;
				}
			}
			
			public class D : B {
				public string messageD;
			
				public D(int x, int y) : base(x + 1, y + 1) {
					messageD = x + " " + y;
				}
			}
		}

		[Test]
		public void InvokingBaseUnnamedConstructorWithArgumentsWorks() {
			var d = new BaseUnnamedConstructorWithArgumentsTypes.D(5, 8);
			Assert.AreEqual(d.messageB + "|" + d.messageD, "6 9|5 8");
		}

		public class BaseNamedConstructorWithoutArgumentsTypes {
			public class B {
				public string messageB;
			
				[ScriptName("myCtor")]
				public B() {
					messageB = "X";
				}
			}
			
			public class D : B {
				public string messageD;
			
				public D() {
					messageD = "Y";
				}
			}
		}

		[Test]
		public void InvokingBaseNamedConstructorWithoutArgumentsWorks() {
			var d = new BaseNamedConstructorWithoutArgumentsTypes.D();
			Assert.AreEqual(d.messageB + "|" + d.messageD, "X|Y");
		}

		public class BaseNamedConstructorWithArgumentsTypes {
			public class B {
				public string messageB;
			
				[ScriptName("myCtor")]
				public B(int x, int y) {
					messageB = x + " " + y;
				}
			}
			
			public class D : B {
				public string messageD;
			
				public D(int x, int y) : base(x + 1, y + 1) {
					messageD = x + " " + y;
				}
			}
		}

		[Test]
		public void InvokingBaseNamedConstructorWithArgumentsWorks() {
			var d = new BaseNamedConstructorWithArgumentsTypes.D(5, 8);
			Assert.AreEqual(d.messageB + "|" + d.messageD, "6 9|5 8");
		}

		public class ConstructingInstanceWithNamedConstructorTypes {
			public class D {
				public virtual string GetMessage() { return "The message " + f; }
			
				private string f;
			
				[ScriptName("myCtor")]
				public D() {
					f = "from ctor";
				}
			}

			public class E : D {
				public override string GetMessage() { return base.GetMessage() + g; }
			
				private string g;
			
				[ScriptName("myCtor")]
				public E() {
					g = " and derived ctor";
				}
			}
		}

		[Test]
		public void ConstructingInstanceWithNamedConstructorWorks() {
			var d = new ConstructingInstanceWithNamedConstructorTypes.D();
			Assert.AreEqual(d.GetType(), typeof(ConstructingInstanceWithNamedConstructorTypes.D));
			Assert.IsTrue((object)d is ConstructingInstanceWithNamedConstructorTypes.D);
			Assert.AreEqual(d.GetMessage(), "The message from ctor");
		}

		[Test]
		public void ConstructingInstanceWithNamedConstructorWorks2() {
			var d = new ConstructingInstanceWithNamedConstructorTypes.E();
			var t = d.GetType();
			Assert.AreEqual(t, typeof(ConstructingInstanceWithNamedConstructorTypes.E), "#1");
			Assert.AreEqual(t.BaseType, typeof(ConstructingInstanceWithNamedConstructorTypes.D), "#2");
			Assert.IsTrue((object)d is ConstructingInstanceWithNamedConstructorTypes.E, "#3");
			Assert.IsTrue((object)d is ConstructingInstanceWithNamedConstructorTypes.D, "#4");
			Assert.AreEqual(d.GetMessage(), "The message from ctor and derived ctor");
		}

		public class BaseMethodInvocationTypes {
			public class B {
				public virtual int F(int x, int y) { return x - y; }
				[IncludeGenericArguments]
				public virtual int G<T>(int x, int y) { return x - y; }
			}
			
			public class D : B {
				public override int F(int x, int y) { return x + y; }
				public override int G<T>(int x, int y) { return x + y; }
			
				public int DoIt(int x, int y) {
					return base.F(x, y);
				}

				public int DoItGeneric(int x, int y) {
					return base.G<string>(x, y);
				}
			}
		}

		[Test]
		public void InvokingBaseMethodWorks() {
			Assert.AreEqual(new BaseMethodInvocationTypes.D().DoIt(5, 3), 2);
		}

		[Test]
		public void InvokingGenericBaseMethodWorks() {
			Assert.AreEqual(new BaseMethodInvocationTypes.D().DoItGeneric(5, 3), 2);
		}

		public class MethodGroupConversionTypes {
			public class C  {
				private int m;
			
				public int F(int x, int y) { return x + y + m; }
				[IncludeGenericArguments]
				public string G<T>(int x, int y) { return x + y + m + typeof(T).Name; }

				public C(int m) {
					this.m = m;
				}
			
				public Func<int, int, int> GetF() {
					return F;
				}

				public Func<int, int, string> GetG() {
					return G<string>;
				}
			}

			public class B {
				public int m;

				public virtual int F(int x, int y) { return x + y + m; }
				[IncludeGenericArguments]
				public virtual string G<T>(int x, int y) { return x + y + m + typeof(T).Name; }

				public B(int m) {
					this.m = m;
				}
			}

			public class D : B {
				public override int F(int x, int y) { return x - y - m; }
				public override string G<T>(int x, int y) { return x - y - m + typeof(T).Name; }

				public Func<int, int, int> GetF() {
					return base.F;
				}

				public Func<int, int, string> GetG() {
					return base.G<string>;
				}

				public D(int m) : base(m) {
				}
			}
		}

		[Test]
		public void MethodGroupConversionWorks() {
			var f = new MethodGroupConversionTypes.C(4).GetF();
			Assert.AreEqual(f(5, 3), 12);
		}

		[Test]
		public void MethodGroupConversionOnGenericMethodWorks() {
			var f = new MethodGroupConversionTypes.C(4).GetG();
			Assert.AreEqual(f(5, 3), "12String");
		}

		[Test]
		public void MethodGroupConversionOnBaseMethodWorks() {
			var f = new MethodGroupConversionTypes.D(4).GetF();
			Assert.AreEqual(f(3, 5), 12);
		}

		[Test]
		public void MethodGroupConversionOnGenericBaseMethodWorks() {
			var g = new MethodGroupConversionTypes.C(4).GetG();
			Assert.AreEqual(g(5, 3), "12String");
		}

		[Test]
		public void ImportedInterfaceAppearsAsObjectWhenUsedAsGenericArgument() {
			Assert.AreStrictEqual(typeof(BX<IImported>), typeof(BX<object>));
		}

		[Test]
		public void FalseIsFunctionShouldReturnFalse() {
			Assert.IsFalse((object)false is Function);
		}

		[Test]
		public void CastingUndefinedToOtherTypeShouldReturnUndefined() {
			Assert.AreEqual(Script.TypeOf((C)Script.Undefined), "undefined");
		}

		[Test]
		public void NonSerializableTypeCanInheritFromSerializableType() {
			var d = new DS(42);
			Assert.AreEqual(d.X, 42, "d.X");
			Assert.AreEqual(d.GetX(), 42, "d.GetX");
		}
		
		[Test]
		public void InheritingFromRecordWorks() {
			var c = new CS2() { X = 42 };
			Assert.AreEqual(c.X, 42);
		}

		[Test]
		public void InstanceOfWorksForSerializableTypesWithCustomTypeCheckCode() {
			object o1 = new { x = 1 };
			object o2 = new { x = 1, y = 2 };
			Assert.IsFalse(typeof(DS2).IsInstanceOfType(o1), "o1 should not be of type");
			Assert.IsTrue (typeof(DS2).IsInstanceOfType(o2), "o2 should be of type");
		}

		[Test]
		public void StaticGetTypeMethodWorks() {
			Assert.AreEqual(Type.GetType("CoreLib.TestScript.Reflection.TypeSystemTests"), typeof(TypeSystemTests));
		}
	}
}
