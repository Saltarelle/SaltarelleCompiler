using System.Collections.Generic;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class QueueTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Queue<int>).FullName, "Array", "FullName should be Array");
			Assert.IsTrue(typeof(Queue<int>).IsClass, "IsClass should be true");
			object list = new Queue<int>();
			Assert.IsTrue(list is Queue<int>, "is Queue<int> should be true");
		}

		[Test]
		public void CountWorks() {
			var q = new Queue<int>();
			Assert.AreEqual(q.Count, 0);
			q.Enqueue(1);
			Assert.AreEqual(q.Count, 1);
			q.Enqueue(10);
			Assert.AreEqual(q.Count, 2);
		}

		[Test]
		public void EnqueueAndDequeueWork() {
			var q = new Queue<int>();
			q.Enqueue(10);
			q.Enqueue(2);
			q.Enqueue(4);
			Assert.AreEqual(q.Dequeue(), 10);
			Assert.AreEqual(q.Dequeue(), 2);
			Assert.AreEqual(q.Dequeue(), 4);
		}

		[Test]
		public void PeekWorks() {
			var q = new Queue<int>();
			q.Enqueue(10);
			Assert.AreEqual(q.Peek(), 10);
			q.Enqueue(2);
			Assert.AreEqual(q.Peek(), 10);
			q.Dequeue();
			Assert.AreEqual(q.Peek(), 2);
		}

		[Test]
		public void ContainsWorks() {
			var q = new Queue<int>();
			q.Enqueue(10);
			q.Enqueue(2);
			q.Enqueue(4);
			Assert.IsTrue(q.Contains(10));
			Assert.IsTrue(q.Contains(2));
			Assert.IsFalse(q.Contains(11));
		}

		[Test]
		public void ClearWorks() {
			var q = new Queue<int>();
			q.Enqueue(10);
			q.Enqueue(2);
			q.Enqueue(4);
			q.Clear();
			Assert.AreEqual(q.Count, 0);
		}
	}
}
