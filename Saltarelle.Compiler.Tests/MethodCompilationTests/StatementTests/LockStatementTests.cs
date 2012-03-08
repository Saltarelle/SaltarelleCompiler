using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class LockStatementTests : StatementTestBase {
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
	}
}
