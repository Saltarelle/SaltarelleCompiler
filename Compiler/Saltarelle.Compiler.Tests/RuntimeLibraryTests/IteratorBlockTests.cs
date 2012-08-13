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
		public void EnumeratingIEnumeratorIteratorToEndWorks() {
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
			AssertStringsEqual((string)result,
@"yielding 0
got 0
yielding 1
got 1
yielding -1
got -1
in finally
");
		}

		[Test]
		public void PrematureDisposalOfIEnumeratorIteratorExecutesFinallyBlocks() {
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
		}
		finally {
			_sb.AppendLine(""in finally"");
		}
	}

	public static string M() {
		var sb = new StringBuilder();
		var enm = new C(sb).GetEnumerator(5);

		for (int i = 0; i < 2; i++) {
			enm.MoveNext();
			sb.AppendLine(""got "" + enm.Current);
		}
		enm.Dispose();

		return sb.ToString();
	}
}", "C.M");
			AssertStringsEqual((string)result,
@"yielding 0
got 0
yielding 1
got 1
in finally
");
		}

		[Test]
		public void ExceptionInIEnumeratorIteratorBodyExecutesFinallyBlocks() {
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
			_sb.AppendLine(""yielding 1"");
			yield return 1;
			_sb.AppendLine(""yielding 2"");
			yield return 2;
			_sb.AppendLine(""throwing"");
			throw new Exception(""test"");
			_sb.AppendLine(""yielding 3"");
			yield return 3;
		}
		finally {
			_sb.AppendLine(""in finally"");
		}
	}

	public static string M() {
		var sb = new StringBuilder();
		var enm = new C(sb).GetEnumerator(5);

		try {
			for (;;) {
				enm.MoveNext();
				sb.AppendLine(""got "" + enm.Current);
			}
		}
		catch (Exception) {
			sb.AppendLine(""caught exception"");
		}

		return sb.ToString();
	}
}", "C.M");
			AssertStringsEqual((string)result,
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
			var result = ExecuteCSharp(@"
using System;
using System.Collections;
using System.Collections.Generic;

public class C {
	IEnumerable<int> GetEnumerable() {
		try {
			yield return 1;
		}
		finally {
			int x = 0;
		}
	}

	public static bool[] M() {
		var enm = new C().GetEnumerable();
		return new[] { enm is IEnumerable<int>, enm is IEnumerable };
	}
}", "C.M");
			Assert.That(result, Has.All.True);
		}

		[Test]
		public void EnumeratingIEnumerableIteratorToEndWorks() {
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

	IEnumerable<int> GetEnumerable(int n) {
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
		foreach (int i in new C(sb).GetEnumerable(2)) {
			sb.AppendLine(""got "" + i);
		}

		return sb.ToString();
	}
}", "C.M");
			AssertStringsEqual((string)result,
@"yielding 0
got 0
yielding 1
got 1
yielding -1
got -1
in finally
");
		}

		[Test]
		public void PrematureDisposalOfIEnumerableIteratorExecutesFinallyBlocks() {
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

	IEnumerable<int> GetEnumerable(int n) {
		try {
			for (int i = 0; i < n; i++) {
				_sb.AppendLine(""yielding "" + i);
				yield return i;
			}
		}
		finally {
			_sb.AppendLine(""in finally"");
		}
	}

	public static string M() {
		var sb = new StringBuilder();
		int n = 0;
		foreach (var i in new C(sb).GetEnumerable(5)) {
			sb.AppendLine(""got "" + i);
			if (++n == 2)
				break;
		}

		return sb.ToString();
	}
}", "C.M");
			AssertStringsEqual((string)result,
@"yielding 0
got 0
yielding 1
got 1
in finally
");
		}

		[Test]
		public void ExceptionInIEnumerableIteratorBodyExecutesFinallyBlocks() {
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

	IEnumerable<int> GetEnumerable(int n) {
		try {
			_sb.AppendLine(""yielding 1"");
			yield return 1;
			_sb.AppendLine(""yielding 2"");
			yield return 2;
			_sb.AppendLine(""throwing"");
			throw new Exception(""test"");
			_sb.AppendLine(""yielding 3"");
			yield return 3;
		}
		finally {
			_sb.AppendLine(""in finally"");
		}
	}

	public static string M() {
		var sb = new StringBuilder();
		var enumerable = new C(sb).GetEnumerable(5);

		try {
			var enumerator = enumerable.GetEnumerator();
			for (;;) {
				enumerator.MoveNext();
				sb.AppendLine(""got "" + enumerator.Current);
			}
		}
		catch (Exception) {
			sb.AppendLine(""caught exception"");
		}

		return sb.ToString();
	}
}", "C.M");
			AssertStringsEqual((string)result,
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

	IEnumerable<int> GetEnumerable(int n) {
		for (; n > 0; n--) {
			yield return n;
		}
	}

	public static string M() {
		var sb = new StringBuilder();

		var enm = new C(sb).GetEnumerable(3);
		foreach (int i in enm)
			sb.AppendLine(i);
		foreach (int i in enm)
			sb.AppendLine(i);

		return sb.ToString();
	}
}", "C.M");
			AssertStringsEqual((string)result,
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

	IEnumerable<int> GetEnumerable(int n) {
		for (int i = 0; i < n; i++) {
			yield return i;
		}
		yield return -1;
	}

	public static string M() {
		var sb = new StringBuilder();

		var enumerable = new C(sb).GetEnumerable(3);
		var enm1 = enumerable.GetEnumerator();
		var enm2 = enumerable.GetEnumerator();

		while (enm1.MoveNext()) {
			enm2.MoveNext();
			sb.AppendLine(enm1.Current);
			sb.AppendLine(enm2.Current);
		}

		return sb.ToString();
	}
}", "C.M");
			AssertStringsEqual((string)result,
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
