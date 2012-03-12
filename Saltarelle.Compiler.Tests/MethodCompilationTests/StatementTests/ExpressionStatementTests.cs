using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class ExpressionStatementTests : MethodCompilerTestBase {
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
@"	this.set_$P3(1);
	this.set_$P2(1);
	this.set_$P1(1);
	$i = 1;
");
		}

		[Test]
		public void CallToPartialMethodWithoutDefinitionIsRemoved() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void CallToPartialMethodWithDefinitionIsNotRemoved() {
			Assert.Inconclusive("TODO");
		}
	}
}
