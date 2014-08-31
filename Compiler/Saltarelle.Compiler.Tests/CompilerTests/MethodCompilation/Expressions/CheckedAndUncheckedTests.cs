using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class CheckedAndUncheckedTests : MethodCompilerTestBase {
		[Test]
		public void CheckedExpressionWorks() {
			AssertCorrect(
@"public void M() {
	int a = 0, b = 2;
	// BEGIN
	int c = checked(a + b);
	// END
}",
@"	var $c = $a + $b;
");
		}

		[Test]
		public void UncheckedExpressionWorks() {
			AssertCorrect(
@"public void M() {
	int a = 0, b = 2;
	// BEGIN
	int c = checked(a + b);
	// END
}",
@"	var $c = $a + $b;
");
		}
	}
}
