using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
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
@"	var $tmp1 = $list.$GetEnumerator();
	while ($tmp1.$MoveNext()) {
		var $item = $tmp1.get_$Current();
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
@"	this.set_$SomeProperty($list);
	var $tmp1 = this.$Method($list).$GetEnumerator();
	while ($tmp1.$MoveNext()) {
		var $item = $tmp1.get_$Current();
		var $x = 0;
	}
");
		}

		[Test]
		public void ForeachOverArrayIsOptimizedToForLoop() {
			AssertCorrect(
@"public void M() {
	var arr = new[] { 1, 2, 3};
	// BEGIN
	foreach (var item in arr) {
		int x = 0;
	}
	// END
}",
@"	for (var $tmp1 = 0; $tmp1 < $arr.$Length; $tmp1++) {
		var $item = $arr[$tmp1];
		var $x = 0;
	}
");
		}

		[Test]
		public void ForeachOverArrayCreatesATemporaryArrayVariableWhenTheArrayExpressionIsComplex() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	foreach (var item in new[] { 1, 2, 3}) {
		int x = 0;
	}
	// END
}",
@"	var $tmp1 = [1, 2, 3];
	for (var $tmp2 = 0; $tmp2 < $tmp1.$Length; $tmp2++) {
		var $item = $tmp1[$tmp2];
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
@"	var $tmp1 = $e.$GetEnumerator();
	while ($tmp1.$MoveNext()) {
		var $item = $tmp1.get_$Current();
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
@"	var $tmp1 = $e.$GetEnumerator();
	try {
		while ($tmp1.$MoveNext()) {
			var $item = $tmp1.get_$Current();
			var $x = 0;
		}
	}
	finally {
		$Upcast($tmp1, {ct_IDisposable}).$Dispose();
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
@"	var $tmp1 = $e.$GetEnumerator();
	try {
		while ($tmp1.$MoveNext()) {
			var $item = $tmp1.get_$Current();
			var $x = 0;
		}
	}
	finally {
		$Upcast($tmp1, {ct_IDisposable}).$Dispose();
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
@"	var $tmp1 = $e.$GetEnumerator();
	try {
		while ($tmp1.$MoveNext()) {
			var $item = $tmp1.get_$Current();
			var $x = 0;
		}
	}
	finally {
		if ($TypeIs($tmp1, {ct_IDisposable})) {
			$Cast($tmp1, {ct_IDisposable}).$Dispose();
		}
	}
");
		}

		[Test]
		public void GetEnumeratorWithEnumerateAsArray() {
			AssertCorrect(
@"public class X {
class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() {}
}
public void M() {
	var enm = new MyEnumerable();
	// BEGIN
	foreach (var item in enm) {
		int x = 0;
	}
	// END
}",
@"	for (var $tmp1 = 0; $tmp1 < $enm.$Length; $tmp1++) {
		var $item = $enm[$tmp1];
		var $x = 0;
	}
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, enumerateAsArray: m.Name == "GetEnumerator") });
		}

		[Test]
		public void GetEnumeratorAsStaticMethodWithThisAsFirstArgumentWithEnumerateAsArray() {
			AssertCorrect(
@"public class X {
class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() {}
}
public void M() {
	var enm = new MyEnumerable();
	// BEGIN
	foreach (var item in enm) {
		int x = 0;
	}
	// END
}",
@"	for (var $tmp1 = 0; $tmp1 < $enm.$Length; $tmp1++) {
		var $item = $enm[$tmp1];
		var $x = 0;
	}
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "GetEnumerator" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name, enumerateAsArray: true) : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void GetEnumeratorAsInlineCodeWithEnumerateAsArray() {
			AssertCorrect(
@"public class X {
class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() {}
}
public void M() {
	var enm = new MyEnumerable();
	// BEGIN
	foreach (var item in enm) {
		int x = 0;
	}
	// END
}",
@"	for (var $tmp1 = 0; $tmp1 < $enm.$Length; $tmp1++) {
		var $item = $enm[$tmp1];
		var $x = 0;
	}
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "GetEnumerator" ? MethodScriptSemantics.InlineCode("X", enumerateAsArray: true) : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}


		[Test]
		public void ForEachOptimizedIntoForLoopWorksWhenTheIteratorIsUsedByReference() {
			AssertCorrect(@"
void F(ref object x) {}
public void M() {
	object[] arr = null;
	// BEGIN
	foreach (var o in arr) {
		F(ref o);
	}
	// END
}
",
@"	for (var $tmp1 = 0; $tmp1 < $arr.$Length; $tmp1++) {
		var $o = { $: $arr[$tmp1] };
		this.$F($o);
	}
");
		}

		[Test]
		public void ForEachWorksWhenTheIteratorIsUsedByReference() {
			AssertCorrect(@"
class MyEnumerable {
	public System.Collections.Generic.IEnumerator<object> GetEnumerator() { return null; }
}
void F(ref object x) {}
public void M() {
	MyEnumerable enm = null;
	// BEGIN
	foreach (var o in enm) {
		F(ref o);
	}
	// END
}
",
@"	var $tmp1 = $enm.$GetEnumerator();
	try {
		while ($tmp1.$MoveNext()) {
			var $o = { $: $tmp1.get_$Current() };
			this.$F($o);
		}
	}
	finally {
		$Upcast($tmp1, {ct_IDisposable}).$Dispose();
	}
");
		}
	}
}
