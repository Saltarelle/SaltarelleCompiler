using System;
using System.Runtime.CompilerServices;
using QUnit;
using System.Text;

namespace CoreLib.TestScript {
	[TestFixture]
	public class DelegateTests {
		public delegate void D1();
		public delegate void D2();
		[BindThisToFirstParameter]
		public delegate int D3(int a, int b);

		class C {
			public void F1() {}
			public void F2() {}
		}

		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Delegate).FullName, "Function");
			Assert.IsTrue(typeof(Delegate).IsClass);
			Assert.AreEqual(typeof(Func<int, string>).FullName, "Function");
			Assert.AreEqual(typeof(Func<,>).FullName, "Function");
			Assert.IsTrue((object)(Action)(() => {}) is Delegate);

			var interfaces = typeof(Delegate).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0);
		}

		[PreserveName]
		private int testField = 12;

		[Test]
		public void CreatingAndInvokingADelegateWorks() {
			Func<int, int> d = x => testField + x;
			Assert.AreEqual(d(13), 25);
		}

		[Test]
		public void CreateWorks() {
			var d = (Func<int, int>)Delegate.Create(this, new Function("x", "{ return x + this.testField; }"));
			Assert.AreEqual(d(13), 25);
		}

		[Test]
		public void CombineWorks() {
			var sb = new StringBuilder();
			Action d = (Action)Delegate.Combine((Action)(() => sb.Append("1")), (Action)(() => sb.Append("2")));
			d();
			Assert.AreEqual(sb.ToString(), "12");
		}

		[Test]
		public void CombineDoesAddsDuplicateDelegates() {
			C c1 = new C(), c2 = new C();
			Action a = c1.F1;
			a += c1.F2;
			Assert.AreEqual(a.GetInvocationList().Length, 2);
			a += c2.F1;
			Assert.AreEqual(a.GetInvocationList().Length, 3);
			a += c1.F1;
			Assert.AreEqual(a.GetInvocationList().Length, 4);
		}

		[Test]
		public void CombineDoesNotAffectOriginal() {
			C c = new C();
			Action a = c.F1;
			Action a2 = a + c.F2;
			Assert.AreEqual(a.GetInvocationList().Length, 1);
			Assert.AreEqual(a2.GetInvocationList().Length, 2);
		}

		[Test]
		public void AddWorks() {
			var sb = new StringBuilder();
			Action d = (Action)(() => sb.Append("1")) + (Action)(() => sb.Append("2"));
			d();
			Assert.AreEqual(sb.ToString(), "12");
		}

		[Test]
		public void AddAssignWorks() {
			var sb = new StringBuilder();
			Action d = () => sb.Append("1");
			d += () => sb.Append("2");
			d();
			Assert.AreEqual(sb.ToString(), "12");
		}

		[Test]
		public void RemoveWorks() {
			var sb = new StringBuilder();
			Action d2 = () => sb.Append("2");
			Action d = (Action)Delegate.Combine(Delegate.Combine((Action)(() => sb.Append("1")), d2), (Action)(() => sb.Append("3")));
			var d3 = (Action)Delegate.Remove(d, d2);
			d3();
			Assert.AreEqual(sb.ToString(), "13");
		}

		[Test]
		public void RemoveDoesNotAffectOriginal() {
			C c = new C();
			Action a = c.F1;
			Action a2 = a + c.F2;
			Action a3 = a2 - a;
			Assert.AreEqual(a.GetInvocationList().Length, 1);
			Assert.AreEqual(a2.GetInvocationList().Length, 2);
			Assert.AreEqual(a3.GetInvocationList().Length, 1);
		}

		[Test]
		public void SubtractingDelegateFromItselfReturnsNull() {
			Action a = () => {};
			Action a2 = a - a;
			Assert.IsTrue(a2 == null);
		}

		void A() {}

		[Test]
		public void RemoveWorksWithMethodGroupConversion() {
			Action a = () => {};
			Action a2 = a + A;
			Action a3 = a2 - A;
			Assert.IsFalse(a.Equals(a2));
			Assert.IsTrue(a.Equals(a3));
		}

		[Test]
		public void SubtractWorks() {
			var sb = new StringBuilder();
			Action d2 = () => sb.Append("2");
			Action d = (Action)Delegate.Combine(Delegate.Combine((Action)(() => sb.Append("1")), d2), (Action)(() => sb.Append("3")));
			var d3 = d - d2;
			d3();
			Assert.AreEqual(sb.ToString(), "13");
		}

		[Test]
		public void SubtractAssignWorks() {
			var sb = new StringBuilder();
			Action d2 = () => sb.Append("2");
			Action d = (Action)Delegate.Combine(Delegate.Combine((Action)(() => sb.Append("1")), d2), (Action)(() => sb.Append("3")));
			d -= d2;
			d();
			Assert.AreEqual(sb.ToString(), "13");
		}

		[Test]
		public void CloneWorks() {
			var sb = new StringBuilder();
			Action d1 = () => { sb.Append("1"); };
			Action d2 = (Action)Delegate.Clone(d1);
			Assert.IsTrue(d1 == d2, "Should be equal");
			Assert.IsFalse(ReferenceEquals(d1, d2), "Should not be same");
			d2();
			Assert.AreEqual(sb.ToString(), "1");
		}

		[InlineCode("{d}.apply({t}, {args})")]
		public object Call(object t, Delegate d, params object[] args) {
			return null;
		}

		[Test]
		public void ThisFixWorks() {
			var target = new { someField = 13 };
			var d = Delegate.ThisFix((Func<dynamic, int, int, int>)((t, a, b) => t.someField + this.testField + a + b));
			Assert.AreEqual(Call(target, d, 3, 5), 33);
		}

		[Test]
		public void CloningDelegateToADifferentTypeIsANoOp() {
			D1 d1 = () => {};
			D2 d2 = new D2(d1);
			Assert.IsTrue((object)d1 == (object)d2);
		}

		[Test]
		public void CloningDelegateToTheSameTypeCreatesANewClone() {
			int x = 0;
			D1 d1 = () => x++;
			D1 d2 = new D1(d1);
			d1();
			d2();

			Assert.IsFalse(d1 == d2);
			Assert.AreEqual(x, 2);
		}

		[Test]
		public void DelegateWithBindThisToFirstParameterWorksWhenInvokedFromScript() {
			D3 d = (a, b) => a + b;
			Function f = (Function)d;
			Assert.AreEqual(f.Call(10, 20), 30);
		}

		[Test]
		public void DelegateWithBindThisToFirstParameterWorksWhenInvokedFromCode() {
			D3 d = (a, b) => a + b;
			Assert.AreEqual(d(10, 20), 30);
		}

		[Test]
		public void EqualityAndInequalityOperatorsAndEqualsMethod() {
#pragma warning disable 1718
			C c1 = new C(), c2 = new C();
			Action n = null;
			Action f11 = c1.F1, f11_2 = c1.F1, f12 = c1.F2, f21 = c2.F1;

			Assert.IsFalse(n == f11, "n == f11");
			Assert.IsTrue (n != f11, "n != f11");
			Assert.IsFalse(f11 == n, "f11 == n");
			Assert.IsFalse(f11.Equals(n), "f11.Equals(n)");
			Assert.IsTrue (f11 != n, "f11 != n");
			Assert.IsTrue (n == n, "n == n");
			Assert.IsFalse(n != n, "n != n");

			Assert.IsTrue (f11 == f11, "f11 == f11");
			Assert.IsTrue (f11.Equals(f11), "f11.Equals(f11)");
			Assert.IsFalse(f11 != f11, "f11 != f11");

			Assert.IsTrue (f11 == f11_2, "f11 == f11_2");
			Assert.IsTrue (f11.Equals(f11_2), "f11.Equals(f11_2)");
			Assert.IsFalse(f11 != f11_2, "f11 != f11_2");

			Assert.IsFalse(f11 == f12, "f11 == f12");
			Assert.IsFalse(f11.Equals(f12), "f11.Equals(f12)");
			Assert.IsTrue (f11 != f12, "f11 != f12");

			Assert.IsFalse(f11 == f21, "f11 == f21");
			Assert.IsFalse(f11.Equals(f21), "f11.Equals(f21)");
			Assert.IsTrue (f11 != f21, "f11 != f21");

			Action m1 = f11 + f21, m2 = f11 + f21, m3 = f21 + f11;

			Assert.IsTrue (m1 == m2, "m1 == m2");
			Assert.IsTrue (m1.Equals(m2), "m1.Equals(m2)");
			Assert.IsFalse(m1 != m2, "m1 != m2");

			Assert.IsFalse(m1 == m3, "m1 == m3");
			Assert.IsFalse(m1.Equals(m3), "m1.Equals(m3)");
			Assert.IsTrue (m1 != m3, "m1 != m3");

			Assert.IsFalse(m1 == f11, "m1 == f11");
			Assert.IsFalse(m1.Equals(f11), "m1.Equals(f11)");
			Assert.IsTrue (m1 != f11, "m1 != f11");
#pragma warning restore 1718
		}

		[Test]
		public void GetInvocationListWorksForImportedFunction() {
			var f = (MulticastDelegate)new Function("");
			var l = f.GetInvocationList();
			Assert.AreEqual(l.Length, 1);
			Assert.IsTrue(ReferenceEquals(f, l[0]));
		}

		[Test]
		public void GetInvocationListWorksForMulticastDelegate() {
			C c1 = new C(), c2 = new C();
			Action f11 = c1.F1, f11_2 = c1.F1, f12 = c1.F2, f21 = c2.F1;
			Action combined = f11 + f21 + f12 + f11_2;
			var l = combined.GetInvocationList();
			Assert.IsTrue(l.Length == 4);
			Assert.IsTrue((Action)l[0] == f11);
			Assert.IsTrue((Action)l[1] == f21);
			Assert.IsTrue((Action)l[2] == f12);
			Assert.IsTrue((Action)l[3] == f11_2);
		}
	}
}
