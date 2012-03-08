using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class IfStatementTests : StatementTestBase {
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
	}
}
