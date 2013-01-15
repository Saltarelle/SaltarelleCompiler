using System;
using System.Collections;
using System.Collections.Generic;
using QUnit;
using System.Text;

#pragma warning disable 162	// CS0162: Unreachable code detected. Disable because we want to assert that code does not reach unreachable parts

namespace CoreLibTests {
	[TestFixture]
	public class IteratorBlockTests {
		public class C {
			private StringBuilder _sb;

			public C(StringBuilder sb) {
				_sb = sb;
			}

			public IEnumerator<int> GetEnumerator(int n) {
				try {
					for (int i = 0; i < n; i++) {
						_sb.AppendLine("yielding " + i);
						yield return i;
					}
					_sb.AppendLine("yielding -1");
					yield return -1;
				}
				finally {
					_sb.AppendLine("in finally");
				}
			}

			public IEnumerator<int> GetEnumeratorThrows() {
				try {
					_sb.AppendLine("yielding 1");
					yield return 1;
					_sb.AppendLine("yielding 2");
					yield return 2;
					_sb.AppendLine("throwing");
					throw new Exception("test");
					_sb.AppendLine("yielding 3");
					yield return 3;
				}
				finally {
					_sb.AppendLine("in finally");
				}
			}

			public IEnumerable<int> GetEnumerable(int n) {
				try {
					for (int i = 0; i < n; i++) {
						_sb.AppendLine("yielding " + i);
						yield return i;
					}
					_sb.AppendLine("yielding -1");
					yield return -1;
				}
				finally {
					_sb.AppendLine("in finally");
				}
				n = 0; // Just to verify that the value of 'n' is not reused in the next call
			}

			public IEnumerable<int> GetEnumerableThrows(int n) {
				try {
					_sb.AppendLine("yielding 1");
					yield return 1;
					_sb.AppendLine("yielding 2");
					yield return 2;
					_sb.AppendLine("throwing");
					throw new Exception("test");
					_sb.AppendLine("yielding 3");
					yield return 3;
				}
				finally {
					_sb.AppendLine("in finally");
				}
			}

			public IEnumerable<int> GetEnumerableMutateParameter(int n) {
				for (; n > 0; n--) {
					yield return n;
				}
			}

			public IEnumerable<int> GetEnumerableSimple(int n) {
				for (int i = 0; i < n; i++) {
					yield return i;
				}
				yield return -1;
			}
		}

		[Test]
		public void TypeReturnedByIteratorBlockReturningIEnumeratorImplementsThatInterfaceAndIDisposable() {
			var enm = new C(null).GetEnumerator(0);
			Assert.IsTrue((object)enm is IEnumerator<int>);
			Assert.IsTrue((object)enm is IEnumerator);
			Assert.IsTrue((object)enm is IDisposable);
		}

		[Test]
		public void EnumeratingIEnumeratorIteratorToEndWorks() {
			var sb = new StringBuilder();
			var enm = new C(sb).GetEnumerator(2);
	
			while (enm.MoveNext()) {
				sb.AppendLine("got " + enm.Current);
			}

			Assert.AreEqual(sb.ToString().Replace("\r\n", "\n"),
@"yielding 0
got 0
yielding 1
got 1
yielding -1
got -1
in finally
".Replace("\r\n", "\n"));
		}

		[Test]
		public void PrematureDisposalOfIEnumeratorIteratorExecutesFinallyBlocks() {
			var sb = new StringBuilder();
			var enm = new C(sb).GetEnumerator(5);
	
			for (int i = 0; i < 2; i++) {
				enm.MoveNext();
				sb.AppendLine("got " + enm.Current);
			}
			enm.Dispose();
	
			Assert.AreEqual(sb.ToString().Replace("\r\n", "\n"),
@"yielding 0
got 0
yielding 1
got 1
in finally
".Replace("\r\n", "\n"));
		}

		[Test]
		public void ExceptionInIEnumeratorIteratorBodyExecutesFinallyBlocks() {
			var sb = new StringBuilder();
			var enm = new C(sb).GetEnumeratorThrows();
	
			try {
				for (int i = 0; i < 100; i++) {
					enm.MoveNext();
					sb.AppendLine("got " + enm.Current);
				}
				Assert.Fail("Should have thrown an exception in the loop");
			}
			catch (Exception) {
				sb.AppendLine("caught exception");
			}
	
			Assert.AreEqual(sb.ToString(),
@"yielding 1
got 1
yielding 2
got 2
throwing
in finally
caught exception
");
		}

		[Test]
		public void TypeReturnedByIteratorBlockReturningIEnumerableImplementsThatInterface() {
			var enm = new C(null).GetEnumerable(0);
			Assert.IsTrue((object)enm is IEnumerable<int>);
			Assert.IsTrue((object)enm is IEnumerable);
		}

		[Test]
		public void EnumeratingIEnumerableIteratorToEndWorks() {
			var sb = new StringBuilder();
			var enm = new C(sb).GetEnumerable(2);

			foreach (var i in enm)
				sb.AppendLine("got " + i);
			sb.AppendLine("-");
			foreach (var i in enm)
				sb.AppendLine("got " + i);

			Assert.AreEqual(sb.ToString().Replace("\r\n", "\n"),
@"yielding 0
got 0
yielding 1
got 1
yielding -1
got -1
in finally
-
yielding 0
got 0
yielding 1
got 1
yielding -1
got -1
in finally
".Replace("\r\n", "\n"));
		}

		[Test]
		public void PrematureDisposalOfIEnumerableIteratorExecutesFinallyBlocks() {
			var sb = new StringBuilder();
			int n = 0;
			foreach (var i in new C(sb).GetEnumerable(5)) {
				sb.AppendLine("got " + i);
				if (++n == 2)
					break;
			}

			Assert.AreEqual(sb.ToString(),
@"yielding 0
got 0
yielding 1
got 1
in finally
");
		}

		[Test]
		public void ExceptionInIEnumerableIteratorBodyExecutesFinallyBlocks() {
			var sb = new StringBuilder();
			var enumerable = new C(sb).GetEnumerableThrows(5);
	
			try {
				var enumerator = enumerable.GetEnumerator();
				for (int i = 0; i < 100; i++) {
					enumerator.MoveNext();
					sb.AppendLine("got " + enumerator.Current);
				}
				Assert.Fail("Should have thrown");
			}
			catch (Exception) {
				sb.AppendLine("caught exception");
			}
	
			Assert.AreEqual(sb.ToString(),
@"yielding 1
got 1
yielding 2
got 2
throwing
in finally
caught exception
");
		}

		[Test]
		public void EnumeratingAnIteratorBlockReturningIEnumerableMultipleTimesUsesTheInitialValuesForParameters() {
			var sb = new StringBuilder();

			var enm = new C(sb).GetEnumerableMutateParameter(3);
			foreach (int i in enm)
				sb.AppendLine(i);
			foreach (int i in enm)
				sb.AppendLine(i);

			Assert.AreEqual(sb.ToString(),
@"3
2
1
3
2
1
");
		}

		[Test]
		public void DifferentGetEnumeratorCallsOnIteratorBlockReturningIEnumerableGetOwnCopiesOfLocals() {
			var sb = new StringBuilder();
	
			var enumerable = new C(sb).GetEnumerableSimple(3);
			var enm1 = enumerable.GetEnumerator();
			var enm2 = enumerable.GetEnumerator();
	
			while (enm1.MoveNext()) {
				enm2.MoveNext();
				sb.AppendLine(enm1.Current);
				sb.AppendLine(enm2.Current);
			}
	
			Assert.AreEqual(sb.ToString(),
@"0
0
1
1
2
2
-1
-1
");
		}
	}
}
