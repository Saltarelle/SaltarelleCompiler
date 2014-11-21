using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class ForeachStatementTests : MethodCompilerTestBase {
		[Test]
		public void ForeachStatementThatDoesNotRequireExtraStatementsForInitializerWorks() {
			AssertCorrect(
@"class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() { return true; }
}

public void M() {
	MyEnumerable list = null;
	// BEGIN
	foreach (var item in list) {
		int x = 0;
	}
	// END
}",
@"	// @(12, 2) - (12, 28)
	var $tmp1 = $list.$GetEnumerator();
	while ($tmp1.$MoveNext()) {
		var $item = $tmp1.get_$Current();
		// @(13, 3) - (13, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForeachStatementWithStructElementType() {
			AssertCorrect(
@"class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() { return true; }
}

public void M() {
	MyEnumerable list = null;
	// BEGIN
	foreach (var item in list) {
		int x = 0;
	}
	// END
}",
@"	// @(12, 2) - (12, 28)
	var $tmp1 = $list.GetEnumerator();
	while ($tmp1.MoveNext()) {
		var $item = $Clone($tmp1.Current, {to_Int32});
		// @(13, 3) - (13, 13)
		var $x = 0;
	}
", metadataImporter: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.MutableValueType(t.Name), GetPropertySemantics = p => PropertyScriptSemantics.Field(p.Name) }, addSourceLocations: true);
		}

		[Test]
		public void ForeachStatementThatDoesRequireExtraStatementsForInitializerWorks() {
			AssertCorrect(
@"class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() { return true; }
}
MyEnumerable SomeProperty { get; set; }
MyEnumerable Method(MyEnumerable l) { return null; }

public void M() {
	MyEnumerable list = null;
	// BEGIN
	foreach (var item in (Method(SomeProperty = list))) {
		int x = 0;
	}
	// END
}",
@"	// @(14, 2) - (14, 53)
	this.set_$SomeProperty($list);
	var $tmp1 = this.$Method($list).$GetEnumerator();
	while ($tmp1.$MoveNext()) {
		var $item = $tmp1.get_$Current();
		// @(15, 3) - (15, 13)
		var $x = 0;
	}
", addSourceLocations: true);
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
@"	// @(4, 2) - (4, 27)
	for (var $tmp1 = 0; $tmp1 < $arr.$Length; $tmp1++) {
		var $item = $arr[$tmp1];
		// @(5, 3) - (5, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForeachOverArrayIsOptimizedToForLoopStruct() {
			AssertCorrect(
@"public void M() {
	var arr = new[] { 1, 2, 3};
	// BEGIN
	foreach (var item in arr) {
		int x = 0;
	}
	// END
}",
@"	// @(4, 2) - (4, 27)
	for (var $tmp1 = 0; $tmp1 < $arr.Length; $tmp1++) {
		var $item = $Clone($arr[$tmp1], {to_Int32});
		// @(5, 3) - (5, 13)
		var $x = 0;
	}
", metadataImporter: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.MutableValueType(t.Name), GetPropertySemantics = p => PropertyScriptSemantics.Field(p.Name) }, addSourceLocations: true);
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
@"	// @(3, 2) - (3, 40)
	var $tmp1 = [1, 2, 3];
	for (var $tmp2 = 0; $tmp2 < $tmp1.$Length; $tmp2++) {
		var $item = $tmp1[$tmp2];
		// @(4, 3) - (4, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForeachStatementDoesNotGenerateDisposeCallForSealedEnumeratorTypeThatDoesNotImplementIDisposable() {
			AssertCorrect(
@"class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() { return true; }
}

public void M() {
	MyEnumerable e = null;
	// BEGIN
	foreach (var item in e) {
		int x = 0;
	}
	// END
}",
@"	// @(12, 2) - (12, 25)
	var $tmp1 = $e.$GetEnumerator();
	while ($tmp1.$MoveNext()) {
		var $item = $tmp1.get_$Current();
		// @(13, 3) - (13, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForeachStatementGeneratesDisposeCallForSealedEnumeratorTypeThatImplementsIDisposable() {
			AssertCorrect(
@"class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator : IDisposable {
	public int Current { get { return 0; } }
	public bool MoveNext() { return true; }
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
@"	// @(13, 2) - (13, 25)
	var $tmp1 = $e.$GetEnumerator();
	try {
		while ($tmp1.$MoveNext()) {
			var $item = $tmp1.get_$Current();
			// @(14, 3) - (14, 13)
			var $x = 0;
		}
	}
	finally {
		// @(13, 2) - (13, 25)
		$tmp1.$Dispose();
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForeachStatementWorksWithInlineCodeDisposeMethod() {
			AssertCorrect(
@"class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator : IDisposable {
	public int Current { get { return 0; } }
	public bool MoveNext() { return true; }
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
@"	// @(13, 2) - (13, 25)
	var $tmp1 = $e.$GetEnumerator();
	try {
		while ($tmp1.$MoveNext()) {
			var $item = $tmp1.get_Current();
			// @(14, 3) - (14, 13)
			var $x = 0;
		}
	}
	finally {
		// @(13, 2) - (13, 25)
		dispose_it($tmp1);
	}
", new MockMetadataImporter { GetMethodSemantics = m => m.ContainingType.Name == "MyEnumerator" && m.Name == "Dispose" ? MethodScriptSemantics.InlineCode("dispose_it({this})") : MethodScriptSemantics.NormalMethod("$" + m.Name) }, addSourceLocations: true);
		}

		[Test]
		public void ForeachStatementWorksWithInlineCodeDisposeMethodWithMultipleStatements() {
			AssertCorrect(
@"class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator : IDisposable {
	public int Current { get { return 0; } }
	public bool MoveNext() { return true; }
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
@"	// @(13, 2) - (13, 25)
	var $tmp1 = $e.$GetEnumerator();
	try {
		while ($tmp1.$MoveNext()) {
			var $item = $tmp1.get_Current();
			// @(14, 3) - (14, 13)
			var $x = 0;
		}
	}
	finally {
		// @(13, 2) - (13, 25)
		dispose_it($tmp1);
		something_else;
	}
", new MockMetadataImporter { GetMethodSemantics = m => m.ContainingType.Name == "MyEnumerator" && m.Name == "Dispose" ? MethodScriptSemantics.InlineCode("dispose_it({this}); something_else;") : MethodScriptSemantics.NormalMethod("$" + m.Name) }, addSourceLocations: true);
		}

		[Test]
		public void ForeachStatementGeneratesDisposeCallForUnsealedEnumeratorTypeThatImplementsIDisposable() {
			AssertCorrect(
@"class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
class MyEnumerator : IDisposable {
	public int Current { get { return 0; } }
	public bool MoveNext() { return true; }
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
@"	// @(13, 2) - (13, 25)
	var $tmp1 = $e.$GetEnumerator();
	try {
		while ($tmp1.$MoveNext()) {
			var $item = $tmp1.get_$Current();
			// @(14, 3) - (14, 13)
			var $x = 0;
		}
	}
	finally {
		// @(13, 2) - (13, 25)
		$tmp1.$Dispose();
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForeachStatementGeneratesCheckedDisposeCallForUnsealedEnumeratorTypeThatDoesNotImplementIDisposable() {
			AssertCorrect(
@"class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() { return true; }
}

public void M() {
	MyEnumerable e = null;
	// BEGIN
	foreach (var item in e) {
		int x = 0;
	}
	// END
}",
@"	// @(12, 2) - (12, 25)
	var $tmp1 = $e.$GetEnumerator();
	try {
		while ($tmp1.$MoveNext()) {
			var $item = $tmp1.get_$Current();
			// @(13, 3) - (13, 13)
			var $x = 0;
		}
	}
	finally {
		// @(12, 2) - (12, 25)
		if ($TypeIs($tmp1, {ct_IDisposable})) {
			$Cast($tmp1, {ct_IDisposable}).$Dispose();
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void GetEnumeratorWithEnumerateAsArray() {
			AssertCorrect(
@"class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() { return true; }
}
public void M() {
	var enm = new MyEnumerable();
	// BEGIN
	foreach (var item in enm) {
		int x = 0;
	}
	// END
}",
@"	// @(11, 2) - (11, 27)
	for (var $tmp1 = 0; $tmp1 < $enm.$Length; $tmp1++) {
		var $item = $enm[$tmp1];
		// @(12, 3) - (12, 13)
		var $x = 0;
	}
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, enumerateAsArray: m.Name == "GetEnumerator") }, addSourceLocations: true);
		}

		[Test]
		public void GetEnumeratorAsStaticMethodWithThisAsFirstArgumentWithEnumerateAsArray() {
			AssertCorrect(
@"class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() { return true; }
}
public void M() {
	var enm = new MyEnumerable();
	// BEGIN
	foreach (var item in enm) {
		int x = 0;
	}
	// END
}",
@"	// @(11, 2) - (11, 27)
	for (var $tmp1 = 0; $tmp1 < $enm.$Length; $tmp1++) {
		var $item = $enm[$tmp1];
		// @(12, 3) - (12, 13)
		var $x = 0;
	}
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "GetEnumerator" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name, enumerateAsArray: true) : MethodScriptSemantics.NormalMethod("$" + m.Name) }, addSourceLocations: true);
		}

		[Test]
		public void GetEnumeratorAsInlineCodeWithEnumerateAsArray() {
			AssertCorrect(
@"class MyEnumerable {
	public MyEnumerator GetEnumerator() { return null; }
}
sealed class MyEnumerator {
	public int Current { get { return 0; } }
	public bool MoveNext() { return true; }
}
public void M() {
	var enm = new MyEnumerable();
	// BEGIN
	foreach (var item in enm) {
		int x = 0;
	}
	// END
}",
@"	// @(11, 2) - (11, 27)
	for (var $tmp1 = 0; $tmp1 < $enm.$Length; $tmp1++) {
		var $item = $enm[$tmp1];
		// @(12, 3) - (12, 13)
		var $x = 0;
	}
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "GetEnumerator" ? MethodScriptSemantics.InlineCode("X", enumerateAsArray: true) : MethodScriptSemantics.NormalMethod("$" + m.Name) }, addSourceLocations: true);
		}

		[Test]
		public void ForEachOptimizedIntoForLoopWorksWhenTheIteratorIsUsedByReference() {
			AssertCorrect(
@"void F(Func<object> f) {}
public void M() {
	object[] arr = null;
	// BEGIN
	foreach (var o in arr) {
		F(() => o);
	}
	// END
}
",
@"	// @(5, 2) - (5, 24)
	for (var $tmp1 = 0; $tmp1 < $arr.$Length; $tmp1++) {
		var $o = { $: $arr[$tmp1] };
		// @(6, 3) - (6, 14)
		this.$F($Bind(function() {
			// @(6, 11) - (6, 12)
			return this.$o.$;
		}, { $o: $o }));
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForEachWorksWhenTheIteratorIsUsedByReference() {
			AssertCorrect(
@"class MyEnumerable {
	public System.Collections.Generic.IEnumerator<object> GetEnumerator() { return null; }
}
void F(Func<object> f) {}
public void M() {
	MyEnumerable enm = null;
	// BEGIN
	foreach (var o in enm) {
		F(() => o);
	}
	// END
}
",
@"	// @(8, 2) - (8, 24)
	var $tmp1 = $enm.$GetEnumerator();
	try {
		while ($tmp1.$MoveNext()) {
			var $o = { $: $tmp1.get_$Current() };
			// @(9, 3) - (9, 14)
			this.$F($Bind(function() {
				// @(9, 11) - (9, 12)
				return this.$o.$;
			}, { $o: $o }));
		}
	}
	finally {
		// @(8, 2) - (8, 24)
		$Upcast($tmp1, {ct_IDisposable}).$Dispose();
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForEachWithElementConversion() {
			AssertCorrect(
@"class MyEnumerable {
	public System.Collections.Generic.IEnumerator<object> GetEnumerator() { return null; }
}
void F(ref object x) {}
public void M() {
	MyEnumerable enm = null;
	// BEGIN
	foreach (int i in enm) {
		int j = i;
	}
	// END
}
",
@"	// @(8, 2) - (8, 24)
	var $tmp1 = $enm.$GetEnumerator();
	try {
		while ($tmp1.$MoveNext()) {
			var $i = $FromNullable($Cast($tmp1.get_$Current(), {ct_Int32}));
			// @(9, 3) - (9, 13)
			var $j = $i;
		}
	}
	finally {
		// @(8, 2) - (8, 24)
		$Upcast($tmp1, {ct_IDisposable}).$Dispose();
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForEachOverArrayWithElementConversion() {
			AssertCorrect(
@"class MyEnumerable {
	public System.Collections.Generic.IEnumerator<object> GetEnumerator() { return null; }
}
void F(ref object x) {}
public void M() {
	object[] arr = null;
	// BEGIN
	foreach (int i in arr) {
		int j = i;
	}
	// END
}
",
@"	// @(8, 2) - (8, 24)
	for (var $tmp1 = 0; $tmp1 < $arr.$Length; $tmp1++) {
		var $i = $FromNullable($Cast($arr[$tmp1], {ct_Int32}));
		// @(9, 3) - (9, 13)
		var $j = $i;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForeachOverDynamicIsAnError() {
			var er = new MockErrorReporter();
			Compile(new[] { @"
using System;
public class C {
	public async void M() {
		dynamic x = null;
		foreach (var i in x) {
		}
	}
}
" }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7542 && m.FormattedMessage.Contains("dynamic")));
		}
	}
}
