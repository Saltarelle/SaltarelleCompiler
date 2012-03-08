using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class LabelStatementTests : StatementTestBase {
		[Test]
		public void LabelWorks() {
			AssertCorrect(
@"Exception MyProperty { get; set; }
public void M() {
	// BEGIN
myLabel:
	int i = 0;
	// END
}",
@"	myLabel:
	var $i = 0;
");
		}
	}
}
