using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class IteratorBlockTests : RuntimeLibraryTestBase {
		[Test]
		public void TypeReturnedByIteratorBlockReturningIEnumeratorImplementsThatInterfaceAndIDisposable() {
			var result = ExecuteCSharp(@"
using System;
using System.Collections;
using System.Collections.Generic;

public class C {
	IEnumerator<int> GetEnumerator() {
		try {
			yield return 1;
		}
		finally {
			int x = 0;
		}
	}

	public static bool[] M() {
		var enm = new C().GetEnumerator();
		return new[] { enm is IEnumerator<int>, enm is IEnumerator, enm is IDisposable };
	}
}", "C.M");
			Assert.That(result, Has.All.True);
		}

		[Test]
		public void EnumeratingIteratorToEndWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class C {
	private StringBuilder _sb;

	public C(StringBuilder sb) {
		_sb = sb;
	}

	IEnumerator<int> GetEnumerator(int n) {
		try {
			for (int i = 0; i < n; i++) {
				_sb.AppendLine(""yielding "" + i);
				yield return i;
			}
			_sb.AppendLine(""yielding -1"");
			yield return -1;
		}
		finally {
			_sb.AppendLine(""in finally"");
		}
	}

	public static string M() {
		var sb = new StringBuilder();
		var enm = new C(sb).GetEnumerator(2);

		while (enm.MoveNext()) {
			sb.AppendLine(""got "" + enm.Current);
		}

		return sb.ToString();
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(
@"yielding 0
got 0
yielding 1
got 1
yielding -1
got -1
in finally
"));
		}

		[Test]
		public void PrematureDisposalOfIteratorExecutesFinallyBlocks() {
			Assert.Fail("TODO");
		}

		[Test]
		public void ExceptionInIteratorBodyExecutesFinallyBlocks() {
			Assert.Fail("TODO");
		}
	}
}
