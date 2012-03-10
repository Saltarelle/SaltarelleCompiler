using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class ForeachStatementTests : MethodCompilerTestBase {
		[Test]
		public void ForeachStatementThatDoesNotRequireExtraStatementsForInitializerWorks() {
			AssertCorrect(
@"
class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() {}
}

public void M() {
	MyEnumerable list = null;
	// BEGIN
	foreach (var item in list) {
		int x = 0;
	}
	// END
}",
@"	var $tmp1 = $list.GetEnumerator();
	while ($tmp1.MoveNext()) {
		var $item = $tmp1.get_Current();
		var $x = 0;
	}
");
		}

		[Test]
		public void ForeachStatementThatDoesRequireExtraStatementsForInitializerWorks() {
			AssertCorrect(
@"
class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() {}
}
MyEnumerable SomeProperty { get; set; }
public MyEnumerable Method(MyEnumerable l) { return null; }

public void M() {
	MyEnumerable list = null;
	// BEGIN
	foreach (var item in (Method(SomeProperty = list))) {
		int x = 0;
	}
	// END
}",
@"	this.set_SomeProperty($list);
	var $tmp1 = this.Method($list).GetEnumerator();
	while ($tmp1.MoveNext()) {
		var $item = $tmp1.get_Current();
		var $x = 0;
	}
");
		}

		[Test]
		public void ForeachStatementDoesNotGenerateDisposeCallForArrayEnumerator() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	foreach (var item in new[] { 1, 2, 3}) {
		int x = 0;
	}
	// END
}",
@"	var $tmp1 = $Upcast([1, 2, 3], {IEnumerable}).GetEnumerator();
	while ($tmp1.MoveNext()) {
		var $item = $tmp1.get_Current();
		var $x = 0;
	}
");
		}

		[Test]
		public void ForeachStatementDoesNotGenerateDisposeCallForSealedEnumeratorTypeThatDoesNotImplementIDisposable() {
			AssertCorrect(
@"
class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() {}
}

public void M() {
	MyEnumerable e = null;
	// BEGIN
	foreach (var item in e) {
		int x = 0;
	}
	// END
}",
@"	var $tmp1 = $e.GetEnumerator();
	while ($tmp1.MoveNext()) {
		var $item = $tmp1.get_Current();
		var $x = 0;
	}
");
		}

		[Test]
		public void ForeachStatementGeneratesDisposeCallForSealedEnumeratorTypeThatImplementsIDisposable() {
			AssertCorrect(
@"
class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator : IDisposable {
	public int Current { get { return 0; } }
	public bool MoveNext() {}
	public void Dispose() {}
}

public void M() {
	MyEnumerable e = null;
	// BEGIN
	foreach (var item in e) {
		int x = 0;
	}
	// END
}",
@"	var $tmp1 = $e.GetEnumerator();
	try {
		while ($tmp1.MoveNext()) {
			var $item = $tmp1.get_Current();
			var $x = 0;
		}
	}
	finally {
		$Upcast($tmp1, {IDisposable}).Dispose();
	}
");
		}

		[Test]
		public void ForeachStatementGeneratesDisposeCallForUnsealedEnumeratorTypeThatImplementsIDisposable() {
			AssertCorrect(
@"
class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
class MyEnumerator : IDisposable {
	public int Current { get { return 0; } }
	public bool MoveNext() {}
	public void Dispose() {}
}

public void M() {
	MyEnumerable e = null;
	// BEGIN
	foreach (var item in e) {
		int x = 0;
	}
	// END
}",
@"	var $tmp1 = $e.GetEnumerator();
	try {
		while ($tmp1.MoveNext()) {
			var $item = $tmp1.get_Current();
			var $x = 0;
		}
	}
	finally {
		$Upcast($tmp1, {IDisposable}).Dispose();
	}
");
		}

		[Test]
		public void ForeachStatementGeneratesCheckedDisposeCallForUnsealedEnumeratorTypeThatDoesNotImplementIDisposable() {
			AssertCorrect(
@"
class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() {}
}

public void M() {
	MyEnumerable e = null;
	// BEGIN
	foreach (var item in e) {
		int x = 0;
	}
	// END
}",
@"	var $tmp1 = $e.GetEnumerator();
	try {
		while ($tmp1.MoveNext()) {
			var $item = $tmp1.get_Current();
			var $x = 0;
		}
	}
	finally {
		if ($TypeIs($tmp1, {IDisposable})) {
			$Cast($tmp1, {IDisposable}).Dispose();
		}
	}
");
		}
	}
}
