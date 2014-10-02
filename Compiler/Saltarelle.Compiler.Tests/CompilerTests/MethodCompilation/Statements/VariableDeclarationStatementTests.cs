using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class VariableDeclarationStatementTests : MethodCompilerTestBase {
		[Test]
		public void VariableDeclarationsWithoutInitializerWork() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	int i, j;
	string s;
	// END
}",
@"	var $i, $j;
	var $s;
");
		}

		[Test]
		public void VariableDeclarationsWithInitializerWork() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	int i = 0, j = 1;
	string s = ""X"";
	// END
}",
@"	var $i = 0, $j = 1;
	var $s = 'X';
");
		}

		[Test]
		public void VariableDeclarationsWithInitializerWorkStruct() {
			AssertCorrect(
@"struct S {}
public void M() {
	S s1 = default(S), s2 = default(S);
	// BEGIN
	S i = s1, j = s2;
	// END
}",
@"	var $i = $Clone($s1, {to_S}), $j = $Clone($s2, {to_S});
", mutableValueTypes: true);
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
@"	var $i = { $: 0 }, $j = {};
");
		}

		[Test]
		public void VariableDeclarationsForVariablesUsedByReferenceWorkStruct() {
			AssertCorrect(
@"public struct S {}
public void OtherMethod(out S x, out S y) { x = default(S); y = default(S); }
public void M() {
	S s = default(S);
	// BEGIN
	S i = s, j;
	// END
	OtherMethod(out i, out j);
}",
@"	var $i = { $: $Clone($s, {to_S}) }, $j = {};
", mutableValueTypes: true);
		}

		[Test]
		public void VariableDeclarationsWhichRequireMultipleStatementsWork() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	// BEGIN
	int i = (SomeProperty = 1), j = 2, k = 3, l = (SomeProperty = i), m = 4;
	// END
}",
@"	this.set_$SomeProperty(1);
	var $i = 1, $j = 2, $k = 3;
	this.set_$SomeProperty($i);
	var $l = $i, $m = 4;
");
		}
	}
}
