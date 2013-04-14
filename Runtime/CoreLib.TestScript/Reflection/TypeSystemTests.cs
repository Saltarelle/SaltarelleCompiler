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
		}

		[Test]
		public void IsAssignableFromWorks() {
			Assert.IsFalse(typeof(IsAssignableFromTypes.C1).IsAssignableFrom(typeof(object)));
			Assert.IsTrue (typeof(object).IsAssignableFrom(typeof(IsAssignableFromTypes.C1)));
			Assert.IsFalse(typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(object)));
			Assert.IsTrue (typeof(object).IsAssignableFrom(typeof(IsAssignableFromTypes.I1)));
			Assert.IsFalse(typeof(IsAssignableFromTypes.I3).IsAssignableFrom(typeof(IsAssignableFromTypes.I1)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.I3)));
			Assert.IsFalse(typeof(IsAssignableFromTypes.D1).IsAssignableFrom(typeof(IsAssignableFromTypes.C1)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.C1).IsAssignableFrom(typeof(IsAssignableFromTypes.D1)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.D1)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.C2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)));
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<string>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)));
			Assert.IsFalse(typeof(IsAssignableFromTypes.I2<string>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<int>)));
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<string>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.C2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)));
			Assert.IsFalse(typeof(IsAssignableFromTypes.I2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I2<string>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)));
			Assert.IsFalse(typeof(IsAssignableFromTypes.I2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.I5<string>)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I2<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.I5<int>)));
			Assert.IsFalse(typeof(IsAssignableFromTypes.I5<int>).IsAssignableFrom(typeof(IsAssignableFromTypes.I2<int>)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.D4)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I3).IsAssignableFrom(typeof(IsAssignableFromTypes.D4)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I4).IsAssignableFrom(typeof(IsAssignableFromTypes.D4)));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsAssignableFrom(typeof(IsAssignableFromTypes.X2)));
			Assert.IsFalse(typeof(IsAssignableFromTypes.I2<>).IsAssignableFrom(typeof(IsAssignableFromTypes.I5<>)));
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<>).IsAssignableFrom(typeof(IsAssignableFromTypes.D2<>)));
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<>).IsAssignableFrom(typeof(IsAssignableFromTypes.D3)));
			Assert.IsFalse(typeof(E1).IsAssignableFrom(typeof(E2)));
			Assert.IsFalse(typeof(int).IsAssignableFrom(typeof(E1)));
			Assert.IsTrue (typeof(object).IsAssignableFrom(typeof(E1)));
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
			Assert.IsFalse(Type.IsInstanceOfType(new object(), typeof(IsAssignableFromTypes.C1)));
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.C1(), typeof(object)));
			Assert.IsFalse(Type.IsInstanceOfType(new object(), typeof(IsAssignableFromTypes.I1)));
			Assert.IsFalse(Type.IsInstanceOfType(new IsAssignableFromTypes.C1(), typeof(IsAssignableFromTypes.D1)));
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D1(), typeof(IsAssignableFromTypes.C1)));
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D1(), typeof(IsAssignableFromTypes.I1)));
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.C2<int>)));
			Assert.IsFalse(Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.C2<string>)));
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.I2<int>)));
			Assert.IsFalse(Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.I2<string>)));
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D2<int>(), typeof(IsAssignableFromTypes.I1)));
			Assert.IsFalse(Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.C2<string>)));
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.C2<int>)));
			Assert.IsFalse(Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.I2<int>)));
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.I2<string>)));
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D4(), typeof(IsAssignableFromTypes.I1)));
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D4(), typeof(IsAssignableFromTypes.I3)));
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.D4(), typeof(IsAssignableFromTypes.I4)));
			Assert.IsTrue (Type.IsInstanceOfType(new IsAssignableFromTypes.X2(), typeof(IsAssignableFromTypes.I1)));
			Assert.IsFalse(Type.IsInstanceOfType(new IsAssignableFromTypes.D3(), typeof(IsAssignableFromTypes.C2<>)));
			Assert.IsTrue (Type.IsInstanceOfType(new E2(), typeof(E1)));
			Assert.IsTrue (Type.IsInstanceOfType(new E1(), typeof(int)));
			Assert.IsTrue (Type.IsInstanceOfType(new E1(), typeof(object)));
			Assert.IsFalse(Type.IsInstanceOfType(null, typeof(object)));

			Assert.IsFalse(typeof(IsAssignableFromTypes.C1).IsInstanceOfType(new object()));
			Assert.IsTrue (typeof(object).IsInstanceOfType(new IsAssignableFromTypes.C1()));
			Assert.IsFalse(typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new object()));
			Assert.IsFalse(typeof(IsAssignableFromTypes.D1).IsInstanceOfType(new IsAssignableFromTypes.C1()));
			Assert.IsTrue (typeof(IsAssignableFromTypes.C1).IsInstanceOfType(new IsAssignableFromTypes.D1()));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.D1()));
			Assert.IsTrue (typeof(IsAssignableFromTypes.C2<int>).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()));
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<string>).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I2<int>).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()));
			Assert.IsFalse(typeof(IsAssignableFromTypes.I2<string>).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.D2<int>()));
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<string>).IsInstanceOfType(new IsAssignableFromTypes.D3()));
			Assert.IsTrue (typeof(IsAssignableFromTypes.C2<int>).IsInstanceOfType(new IsAssignableFromTypes.D3()));
			Assert.IsFalse(typeof(IsAssignableFromTypes.I2<int>).IsInstanceOfType(new IsAssignableFromTypes.D3()));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I2<string>).IsInstanceOfType(new IsAssignableFromTypes.D3()));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.D4()));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I3).IsInstanceOfType(new IsAssignableFromTypes.D4()));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I4).IsInstanceOfType(new IsAssignableFromTypes.D4()));
			Assert.IsTrue (typeof(IsAssignableFromTypes.I1).IsInstanceOfType(new IsAssignableFromTypes.X2()));
			Assert.IsFalse(typeof(IsAssignableFromTypes.C2<>).IsInstanceOfType(new IsAssignableFromTypes.D3()));
			Assert.IsTrue (typeof(E1).IsInstanceOfType(new E2()));
			Assert.IsTrue (typeof(int).IsInstanceOfType(new E1()));
			Assert.IsTrue (typeof(object).IsInstanceOfType(new E1()));
			Assert.IsFalse(typeof(object).IsInstanceOfType(null));
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
				public string GetMessage() { return "The message " + f; }
			
				private string f;
			
				[ScriptName("myCtor")]
				public D() {
					f = "from ctor";
				}
			}
		}

		[Test]
		public void ConstructingInstanceWithNamedConstructorWorks() {
			var d = new ConstructingInstanceWithNamedConstructorTypes.D();
			Assert.AreEqual(d.GetType(), typeof(ConstructingInstanceWithNamedConstructorTypes.D));
			Assert.AreEqual(d.GetMessage(), "The message from ctor");
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
			Assert.AreEqual(Type.GetScriptType((C)Script.Undefined), "undefined");
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
	}
}
