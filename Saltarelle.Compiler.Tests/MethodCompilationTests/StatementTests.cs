using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests {
	[TestFixture]
	public class StatementTests : MethodCompilerTestBase {
		private void AssertCorrect(string csharp, string expected) {
			CompileMethod(csharp);
			string actual = OutputFormatter.Format(CompiledMethod.Body, true);

			int begin = actual.IndexOf("// BEGIN");
			if (begin > -1) {
				while (begin < (actual.Length - 1) && actual[begin - 1] != '\n')
					begin++;
				actual = actual.Substring(begin);
			}

			int end = actual.IndexOf("// END");
			if (end >= 0) {
				while (end >= 0 && actual[end] != '\n')
					end--;
				actual = actual.Substring(0, end + 1);
			}
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		[Test]
		public void CommentsAreCorrectlyTransferred() {
			AssertCorrect(
@"public void M() {
	// Some comment
	/* And some
	   multiline
	   comment
	*/
}",
@"{
	// Some comment
	// And some
	// multiline
	// comment
}
");
		}

		[Test]
		public void InactiveCodeIsNotTransferred() {
			AssertCorrect(
@"public void M() {
#if FALSE
	This is some stuff
	that should not appear in the script
#endif
}",
@"{
}
");
		}

		[Test]
		public void VariableDeclarationsWithoutInitializerWork() {
			AssertCorrect(
@"public void M() {
	int i, j;
	string s;
}",
@"{
	var $i, $j;
	var $s;
}
");
		}

		[Test]
		public void VariableDeclarationsWithInitializerWork() {
			AssertCorrect(
@"public void M() {
	int i = 0, j = 1;
	string s = ""X"";
}",
@"{
	var $i = 0, $j = 1;
	var $s = 'X';
}
");
		}

		[Test]
		public void VariableDeclarationsForVariablesUsedByReferenceWork() {
			AssertCorrect(
@"public void OtherMethod(out int x, out int y) { x = 0; y = 0; }
public void M() {
	// BEGIN
	int i = 0, j;
	// END
	OtherMethod(out i, out j);
}",
@"	var $i = { $: 0 }, $j = { $: null };
");
		}

		[Test]
		public void VariableDeclarationsWhichRequireMultipleStatementsWork() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	int i = (SomeProperty = 1), j = 2, k = 3, l = (SomeProperty = i), m = 4;
}",
@"{
	this.set_SomeProperty(1);
	var $i = 1, $j = 2, $k = 3;
	this.set_SomeProperty($i);
	var $l = $i, $m = 4;
}
");
		}

		[Test]
		public void ForStatementWithVariableDeclarationsWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (int i = 0, j = 1; i < 10; i++) {
		int k = i;
	}
	// END
}",
@"	for (var $i = 0, $j = 1; $i < 10; $i++) {
		var $k = $i;
	}
");
		}

		[Test]
		public void ForStatementWithoutVariableDeclarationWorks() {
			AssertCorrect(
@"public void M() {
	int i;
	// BEGIN
	for (i = 0; i < 10; i++) {
		int k = 0;
	}
	// END
}",
@"	for ($i = 0; $i < 10; $i++) {
		var $k = 0;
	}
");
		}

		[Test]
		public void ForStatementWithMultipleInitializersWorks() {
			AssertCorrect(
@"public void M() {
	int i, j;
	// BEGIN
	for (i = 0, j = 1; i < 10; i++) {
		int k = 0;
	}
	// END
}",
@"	for ($i = 0, $j = 1; $i < 10; $i++) {
		var $k = 0;
	}
");
		}

		[Test]
		public void ForStatementWithVariableDeclarationInitializersRequiringMultipleStatementsWork() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	// BEGIN
	for (int i = (SomeProperty = 1), j = 2, k = 3, l = (SomeProperty = i), m = 4; i < 10; i++) {
		int x = 0;
	}
	// END
}",
@"	this.set_SomeProperty(1);
	var $i = 1, $j = 2, $k = 3;
	this.set_SomeProperty($i);
	for (var $l = $i, $m = 4; $i < 10; $i++) {
		var $x = 0;
	}
");
		}

		[Test]
		public void ForStatementWithExpressionInitializersRequiringMultipleStatementsWork() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	int i, j, k, l, m;
	// BEGIN
	for (i = (SomeProperty = 1), j = 2, k = 3, l = (SomeProperty = i), m = 4; i < 10; i++) {
		int x = 0;
	}
	// END
}",
@"	this.set_SomeProperty(1);
	$i = 1, $j = 2, $k = 3;
	this.set_SomeProperty($i);
	for ($l = $i, $m = 4; $i < 10; $i++) {
		var $x = 0;
	}
");
		}

		[Test]
		public void ForStatementWithoutInitializerWorks() {
			AssertCorrect(
@"public void M() {
	int i = 0;
	// BEGIN
	for (; i < 10; i++) {
		int k = i;
	}
	// END
}",
@"	for (; $i < 10; $i++) {
		var $k = $i;
	}
");
		}

		[Test]
		public void ForStatementWithoutConditionWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (int i = 0; ; i++) {
		int k = i;
	}
	// END
}",
@"	for (var $i = 0;; $i++) {
		var $k = $i;
	}
");
		}

		[Test]
		public void ForStatementWithoutIteratorWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (int i = 0; i < 10;) {
		int k = i;
	}
	// END
}",
@"	for (var $i = 0; $i < 10;) {
		var $k = $i;
	}
");
		}

		[Test]
		public void ForStatementWithMultipleIteratorsWorks() {
			AssertCorrect(
@"public void M() {
	int i = 0, j = 0;
	// BEGIN
	for (; i < 10; i++, j++) {
		int k = 0;
	}
	// END
}",
@"	for (; $i < 10; $i++, $j++) {
		var $k = 0;
	}
");
		}

		[Test]
		public void ForEverStatementWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (;;) {
		int k = 0;
	}
	// END
}",
@"	for (;;) {
		var $k = 0;
	}
");
		}

		[Test]
		public void ForStatementWithConditionThatNeedExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	// BEGIN
	for (int i = 0; i < (SomeProperty = 1); i++) {
		int x = 0;
	}
	// END
}",
@"	for (var $i = 0;; $i++) {
		this.set_SomeProperty(1);
		if (!($i < 1)) {
			break;
		}
		var $x = 0;
	}
");
		}

		[Test]
		public void ForStatementWithIteratorThatNeedExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	int i, j, k, l, m;
	// BEGIN
	for (i = 0; i < 10; i = (SomeProperty = 1), j = 2, k = 3, l = (SomeProperty = i), m = 4) {
		int x = 0;
	}
	// END
}",
@"	for ($i = 0; $i < 10;) {
		var $x = 0;
		this.set_SomeProperty(1);
		$i = 1;
		$j = 2;
		$k = 3;
		this.set_SomeProperty($i);
		$l = $i;
		$m = 4;
	}
");
		}

		[Test]
		public void ExpressionStatementThatOnlyRequiresASingleScriptStatementWorks() {
			AssertCorrect(
@"public void M() {
	int i;
	// BEGIN
	i = 0;
	// END
}",
@"	$i = 0;
");
		}

		[Test]
		public void ExpressionStatementThatRequiresMultipleScriptStatementsWorks() {
			AssertCorrect(
@"public int P1 { get; set; }
public int P2 { get; set; }
public int P3 { get; set; }
public void M() {
	int i;
	// BEGIN
	i = (P1 = P2 = P3 = 1);
	// END
}",
@"	this.set_P3(1);
	this.set_P2(1);
	this.set_P1(1);
	$i = 1;
");
		}

		[Test]
		public void BreakStatementWorks() {
			AssertCorrect(
@"public void M() {
	for (int i = 0; i < 10; i++) {
		// BEGIN
		break;
		// END
	}
}",
@"		break;
");
		}

		[Test]
		public void ContinueStatementWorks() {
			AssertCorrect(
@"public void M() {
	for (int i = 0; i < 10; i++) {
		// BEGIN
		continue;
		// END
	}
}",
@"		continue;
");
		}

		[Test]
		public void EmptyStatementWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	;
	// END
}",
@"	;
");
		}

		[Test]
		public void BlockStatementWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	{
		int i = 0;
		int j = 1;
	}
	// END
}",
@"	{
		var $i = 0;
		var $j = 1;
	}
");
		}

		[Test]
		public void IfStatementWithoutElseWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	if (true) {
		int x = 0;
	}
	// END
}",
@"	if (true) {
		var $x = 0;
	}
");
		}

		[Test]
		public void IfStatementWithElseWorks() {
			AssertCorrect(
@"public void M() {
	int x;
	// BEGIN
	if (true) {
		x = 0;
	}
	else {
		x = 1;
	}
	// END
}",
@"	if (true) {
		$x = 0;
	}
	else {
		$x = 1;
	}
");
		}

		[Test]
		public void IfStatementWithConditionThatRequiresExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	// BEGIN
	if ((SomeProperty = 1) < 0) {
		int x = 0;
	}
	// END
}",
@"	this.set_SomeProperty(1);
	if (1 < 0) {
		var $x = 0;
	}
");
		}

		[Test]
		public void CheckedStatementActsAsABlockStatement() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	checked {
		int x = 0;
	}
	// END
}",
@"	{
		var $x = 0;
	}
");
		}

		[Test]
		public void UncheckedStatementActsAsABlockStatement() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	unchecked {
		int x = 0;
	}
	// END
}",
@"	{
		var $x = 0;
	}
");
		}

		[Test]
		public void LockStatementEvaluatesArgumentThatDoesNotRequireExtraStatementsAndActsAsABlockStatement() {
			AssertCorrect(
@"public object SomeProperty { get; set; }
public object Method(object o) { return null; }
public void M() {
	object o = null;
	// BEGIN
	lock (Method(SomeProperty = o)) {
		int x = 0;
	}
	// END
}",
@"	this.set_SomeProperty($o);
	this.Method($o);
	{
		var $x = 0;
	}
");
		}

		[Test]
		public void LockStatementEvaluatesArgumentThatDoesRequireExtraStatementsAndActsAsABlockStatement() {
			AssertCorrect(
@"public object P1 { get; set; }
public object P2 { get; set; }
public void M() {
	object o = null;
	// BEGIN
	lock (P1 = P2 = o) {
		int x = 0;
	}
	// END
}",
@"	this.set_P2($o);
	this.set_P1($o);
	$o;
	{
		var $x = 0;
	}
");
		}

		[Test]
		public void DoWhileStatementWithExpressionWithoutExtraStatementsWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	do {
		int x = 0;
	} while (true);
	// END
}",
@"	do {
		var $x = 0;
	} while (true);
");
		}

		[Test]
		public void DoWhileStatementWithExpressionWithExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	// BEGIN
	do {
		int x = 0;
	} while ((SomeProperty = 1) < 0);
	// END
}",
@"	do {
		var $x = 0;
		this.set_SomeProperty(1);
	} while (1 < 0);
");
		}

		[Test]
		public void WhileStatementWithExpressionWithoutExtraStatementsWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	while (true) {
		int x = 0;
	}
	// END
}",
@"	while (true) {
		var $x = 0;
	}
");
		}

		[Test]
		public void WhileStatementWithExpressionWithExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	// BEGIN
	while ((SomeProperty = 1) < 0) {
		int x = 0;
	}
	// END
}",
@"	while (true) {
		this.set_SomeProperty(1);
		if (!(1 < 0)) {
			break;
		}
		var $x = 0;
	}
");
		}

		[Test]
		public void ReturnVoidStatementWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	return;
	// END
}",
@"	return;
");
		}

		[Test]
		public void ReturnExpressionStatementWithExpressionWithoutExtraStatementsWorks() {
			AssertCorrect(
@"public int M() {
	int x = 0;
	// BEGIN
	return x;
	// END
}",
@"	return $x;
");
		}

		[Test]
		public void ReturnExpressionStatementWithExpressionWithExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	// BEGIN
	return (SomeProperty = 1);
	// END
}",
@"	this.set_SomeProperty(1);
	return 1;
");
		}

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
@"	var $tmp1 = [1, 2, 3].GetEnumerator();
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
		$tmp1.Dispose();
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
		$tmp1.Dispose();
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
		if ({System.Type}.TypeIs($tmp1, {System.IDisposable})) {
			{System.Type}.Cast($tmp1, {System.IDisposable}).Dispose();
		}
	}
");
		}

		[Test]
		public void UsingStatementWithSingleVariableDeclarationWithSimpleInitializerWorks() {
			AssertCorrect(
@"public void M() {
	IDisposable a = null;
	// BEGIN
	using (IDisposable d = a) {
		int x = 0;
	}
	// END
}",
@"	{
		var $d = $a;
		try {
			var $x = 0;
		}
		finally {
			if ($d != null) {
				$d.Dispose();
			}
		}
	}
");
		}

		[Test]
		public void UsingStatementWithSingleVariableDeclarationWorks() {
			AssertCorrect(
@"IDisposable MyProperty { get; set; }
public void M() {
	IDisposable a = null;
	// BEGIN
	using (IDisposable d = (MyProperty = a)) {
		int x = 0;
	}
	// END
}",
@"	{
		this.set_MyProperty($a);
		var $d = $a;
		try {
			var $x = 0;
		}
		finally {
			if ($d != null) {
				$d.Dispose();
			}
		}
	}
");
		}

		[Test]
		public void UsingStatementWithoutVariableDeclarationWorks() {
			AssertCorrect(
@"IDisposable MyProperty { get; set; }
public void M() {
	IDisposable a = null;
	// BEGIN
	using (MyProperty = a) {
		int x = 0;
	}
	// END
}",
@"	{
		this.set_MyProperty($a);
		var $tmp1 = $a;
		try {
			var $x = 0;
		}
		finally {
			if ($tmp1 != null) {
				$tmp1.Dispose();
			}
		}
	}
");
		}

		[Test]
		public void UsingStatementWithMultipleVariableDeclarationsWork() {
			AssertCorrect(
@"IDisposable P1 { get; set; }
IDisposable P2 { get; set; }
IDisposable P3 { get; set; }
public void M() {
	IDisposable a = null, b = null, c = null;
	// BEGIN
	using (IDisposable d1 = (P1 = a), d2 = (P2 = b), d3 = (P3 = c)) {
		int x = 0;
	}
	// END
}",
@"	{
		this.set_P1($a);
		var $d1 = $a;
		try {
			this.set_P2($b);
			var $d2 = $b;
			try {
				this.set_P3($c);
				var $d3 = $c;
				try {
					var $x = 0;
				}
				finally {
					if ($d3 != null) {
						$d3.Dispose();
					}
				}
			}
			finally {
				if ($d2 != null) {
					$d2.Dispose();
				}
			}
		}
		finally {
			if ($d1 != null) {
				$d1.Dispose();
			}
		}
	}
");
		}

		[Test]
		public void UsingStatementWithDynamicResourceWorks() {
			AssertCorrect(
@"IDisposable MyProperty { get; set; }
public void M() {
	IDisposable a = null;
	// BEGIN
	using (dynamic d = (MyProperty = a)) {
		int x = 0;
	}
	// END
}",
@"	{
		this.set_MyProperty($a);
		var $d = $a;
		var $tmp1 = {System.Type}.Cast($d, {System.IDisposable});
		try {
			var $x = 0;
		}
		finally {
			$tmp1.Dispose();
		}
	}
");
		}
	}
}
