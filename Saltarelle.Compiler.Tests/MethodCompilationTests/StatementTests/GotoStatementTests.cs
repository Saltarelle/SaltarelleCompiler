using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class GotoStatementTests : StatementTestBase {
		[Test]
		public void GotoWorks() {
			AssertCorrect(
@"Exception MyProperty { get; set; }
public void M() {
myLabel:
	int i = 0;
	// BEGIN
	goto myLabel;
	// END
}",
@"	goto myLabel;
");
		}
	}
}
