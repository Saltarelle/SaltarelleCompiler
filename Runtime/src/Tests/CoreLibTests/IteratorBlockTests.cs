using System;
using System.Collections.Generic;
using QUnit;
using System.Text;

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
		}

		[Test]
		public void IteratorBlocksReturningEnumeratorWork() {
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
		public void IteratorBlocksReturningEnumerableWork() {
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
	}
}
