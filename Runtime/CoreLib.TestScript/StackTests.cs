using System.Collections.Generic;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class StackTests {
		private class C {
			public readonly int i;

			public C(int i) {
				this.i = i;
			}

			public override bool Equals(object o) {
				return o is C && i == ((C)o).i;
			}
			public override int GetHashCode() {
				return i;
			}
		}

		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Stack<int>).FullName, "Array", "FullName should be Array");
			Assert.IsTrue(typeof(Stack<int>).IsClass, "IsClass should be true");
			object list = new Stack<int>();
			Assert.IsTrue(list is Stack<int>, "is Queue<int> should be true");
		}

		[Test]
		public void CountWorks() {
			var s = new Stack<int>();
			Assert.AreEqual(s.Count, 0);
			s.Push(1);
			Assert.AreEqual(s.Count, 1);
			s.Push(10);
			Assert.AreEqual(s.Count, 2);
		}

		[Test]
		public void PushAndPopWork() {
			var s = new Stack<int>();
			s.Push(10);
			s.Push(2);
			s.Push(4);
			Assert.AreEqual(s.Pop(), 4);
			Assert.AreEqual(s.Pop(), 2);
			Assert.AreEqual(s.Pop(), 10);
		}

		[Test]
		public void PeekWorks() {
			var s = new Stack<int>();
			s.Push(10);
			Assert.AreEqual(s.Peek(), 10);
			s.Push(2);
			Assert.AreEqual(s.Peek(), 2);
			s.Push(4);
			Assert.AreEqual(s.Peek(), 4);
		}

		[Test]
		public void ContainsWorks() {
			var s = new Stack<int>();
			s.Push(10);
			s.Push(2);
			s.Push(4);
			Assert.IsTrue(s.Contains(10));
			Assert.IsTrue(s.Contains(2));
			Assert.IsFalse(s.Contains(11));
		}

		[Test]
		public void ContainsUsesEqualsMethod() {
			var s = new Stack<C>();
			s.Push(new C(1));
			s.Push(new C(2));
			s.Push(new C(3));
			Assert.IsTrue(s.Contains(new C(2)));
			Assert.IsFalse(s.Contains(new C(4)));
		}

		[Test]
		public void ClearWorks() {
			var s = new Stack<int>();
			s.Push(10);
			s.Push(2);
			s.Push(4);
			s.Clear();
			Assert.AreEqual(s.Count, 0);
		}
	}
}
