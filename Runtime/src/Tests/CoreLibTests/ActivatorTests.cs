using System;
using System.Collections.Generic;
using QUnit;
using System.Text;

namespace CoreLibTests {
	[TestFixture]
	public class ActivatorTests {
		class C1 {
			public int i;
			public C1() {
				i = 1;
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

		[Test]
		public void NonGenericCreateInstanceWithoutArgumentsWorks() {
			C1 c = (C1)Activator.CreateInstance(typeof(C1));
			Assert.AreNotEqual(c, null);
			Assert.AreEqual(c.i, 1);
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
			Assert.AreEqual(c.i, 1);
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

		private T Instantiate<T>() where T : new() {
			return new T();
		}

		[Test]
		public void InstantiatingTypeParameterWithDefaultConstructorConstraintWorks() {
			var c = Instantiate<C1>();
			Assert.AreEqual(c.i, 1);
			Assert.AreStrictEqual(Instantiate<int>(), 0);
		}
	}
}
