using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class VariableDeclarationStatementTests : MethodCompilerTestBase {
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
	this.set_$SomeProperty(1);
	var $i = 1, $j = 2, $k = 3;
	this.set_$SomeProperty($i);
	var $l = $i, $m = 4;
}
");
		}
	}
}
