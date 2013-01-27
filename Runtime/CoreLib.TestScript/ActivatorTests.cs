using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class ActivatorTests {
		class C1 {
			public int i;
			public C1() {
				i = 42;
			}
		}

		class C2 {
			public int i;
			public C2(int i) {
				this.i = i;
			}
		}

		class C3 {
			public int i, j;
			public C3(int i, int j) {
				this.i = i;
				this.j = j;
			}
		}

		public class C4 {
			public int i;
			[ScriptName("named")]
			public C4() {
				i = 42;
			}

			[ScriptName("")]
			public C4(int i) {
				this.i = 1;
			}
		}

		public class C5 {
			[PreserveCase]
			public int i;

			[InlineCode("{{ i: 42 }}")]
			public C5() {}
		}

		[Serializable]
		public class C6 {
			public int i;
			public C6() {
				i = 42;
			}
		}
		
		[Serializable]
		class C7 {
			[ObjectLiteral]
			public C7() {
			}
		}

		[IncludeGenericArguments]
		public class C8<T> {
			public int I;
			[ScriptName("named")]
			public C8() {
				I = 42;
			}
		
			[ScriptName("")]
			public C8(T t) {
				I = 1;
			}
		}

		[Test]
		public void NonGenericCreateInstanceWithoutArgumentsWorks() {
			C1 c = (C1)Activator.CreateInstance(typeof(C1));
			Assert.AreNotEqual(c, null);
			Assert.AreEqual(c.i, 42);
		}

		[Test]
		public void NonGenericCreateInstanceWithOneArgumentWorks() {
			C2 c = (C2)Activator.CreateInstance(typeof(C2), 3);
			Assert.AreNotEqual(c, null);
			Assert.AreEqual(c.i, 3);
		}

		[Test]
		public void NonGenericCreateInstanceWithTwoArgumentsWorks() {
			C3 c = (C3)Activator.CreateInstance(typeof(C3), 7, 8);
			Assert.AreNotEqual(c, null);
			Assert.AreEqual(c.i, 7);
			Assert.AreEqual(c.j, 8);
		}

		[Test]
		public void GenericCreateInstanceWithoutArgumentsWorks() {
			C1 c = Activator.CreateInstance<C1>();
			Assert.AreNotEqual(c, null);
			Assert.AreEqual(c.i, 42);
		}

		[Test]
		public void GenericCreateInstanceWithOneArgumentWorks() {
			C2 c = Activator.CreateInstance<C2>(3);
			Assert.AreNotEqual(c, null);
			Assert.AreEqual(c.i, 3);
		}

		[Test]
		public void GenericCreateInstanceWithTwoArgumentsWorks() {
			C3 c = Activator.CreateInstance<C3>(7, 8);
			Assert.AreNotEqual(c, null);
			Assert.AreEqual(c.i, 7);
			Assert.AreEqual(c.j, 8);
		}

		[IncludeGenericArguments]
		private T Instantiate<T>() where T : new() {
			return new T();
		}

		[Test]
		public void InstantiatingTypeParameterWithDefaultConstructorConstraintWorks() {
			var c = Instantiate<C1>();
			Assert.AreEqual(c.i, 42);
			Assert.AreStrictEqual(Instantiate<int>(), 0);
		}

		[Test]
		public void CreateInstanceWithNoArgumentsWorksForClassWithUnnamedDefaultConstructor() {
			var c1 = Activator.CreateInstance<C1>();
			var c2 = (C1)Activator.CreateInstance(typeof(C1));
			var c3 = Instantiate<C1>();

			Assert.AreEqual(c1.i, 42);
			Assert.AreEqual(c2.i, 42);
			Assert.AreEqual(c3.i, 42);
		}

		[Test]
		public void CreateInstanceWithNoArgumentsWorksForClassWithNamedDefaultConstructor() {
			var c1 = Activator.CreateInstance<C4>();
			var c2 = (C4)Activator.CreateInstance(typeof(C4));
			var c3 = Instantiate<C4>();

			Assert.AreEqual(c1.i, 42);
			Assert.AreEqual(c2.i, 42);
			Assert.AreEqual(c3.i, 42);
		}

		[Test]
		public void CreateInstanceWithNoArgumentsWorksForClassWithInlineCodeDefaultConstructor() {
			var c1 = Activator.CreateInstance<C5>();
			var c2 = Script.Reinterpret<C5>(Activator.CreateInstance(typeof(C5)));
			var c3 = Instantiate<C5>();

			Assert.AreEqual(c1.i, 42);
			Assert.AreEqual(c2.i, 42);
			Assert.AreEqual(c3.i, 42);
		}

		[Test]
		public void CreateInstanceWithNoArgumentsWorksForClassWithStaticMethodDefaultConstructor() {
			var c1 = Activator.CreateInstance<C6>();
			var c2 = (C6)Activator.CreateInstance(typeof(C6));
			var c3 = (C6)Instantiate<C6>();

			Assert.AreEqual(c1.i, 42);
			Assert.AreEqual(c2.i, 42);
			Assert.AreEqual(c3.i, 42);
		}

		[Test]
		public void CreateInstanceWithNoArgumentsWorksForClassWithJsonDefaultConstructor() {
			var c1 = Activator.CreateInstance<C7>();
			var c2 = (C7)Activator.CreateInstance(typeof(C7));
			var c3 = Instantiate<C7>();

			Assert.AreEqual(((dynamic)c1).constructor, typeof(object));
			Assert.AreEqual(((dynamic)c2).constructor, typeof(object));
			Assert.AreEqual(((dynamic)c3).constructor, typeof(object));
		}

		[Test]
		public void CreateInstanceWithNoArgumentsWorksForGenericClassWithNamedDefaultConstructor() {
			var c1 = Activator.CreateInstance<C8<int>>();
			var c2 = (C8<int>)Activator.CreateInstance(typeof(C8<int>));
			var c3 = Instantiate<C8<int>>();

			Assert.AreEqual(c1.I, 42);
			Assert.AreEqual(c1.GetType().GetGenericArguments()[0], typeof(int));
			Assert.AreEqual(c2.I, 42);
			Assert.AreEqual(c2.GetType().GetGenericArguments()[0], typeof(int));
			Assert.AreEqual(c3.I, 42);
			Assert.AreEqual(c3.GetType().GetGenericArguments()[0], typeof(int));
		}
	}
}
