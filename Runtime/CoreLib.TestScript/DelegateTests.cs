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

		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Delegate).FullName, "Function");
			Assert.IsTrue(typeof(Delegate).IsClass);
			Assert.AreEqual(typeof(Func<int, string>).FullName, "Function");
			Assert.AreEqual(typeof(Func<,>).FullName, "Function");
			Assert.IsTrue((object)(Action)(() => {}) is Delegate);
		}

		[Test(ExpectedAssertionCount = 0)]
		public void EmptyFieldCanBeInvoked() {
			((Action)Delegate.Empty)();
			// No exception is good enough
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
			Assert.IsFalse(d1 == d2);
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
	}
}
