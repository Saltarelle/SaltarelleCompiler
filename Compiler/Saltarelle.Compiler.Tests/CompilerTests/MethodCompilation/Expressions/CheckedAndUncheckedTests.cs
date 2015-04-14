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
	unchecked {
		// BEGIN
		int c = checked(a + b);
		// END
	}
}",
@"		var $c = $Check($a + $b, {ct_Int32});
");
		}

		[Test]
		public void UncheckedExpressionWorks() {
			AssertCorrect(
@"public void M() {
	int a = 0, b = 2;
	checked {
		// BEGIN
		int c = unchecked(a + b);
		// END
	}
}",
@"		var $c = $a + $b;
");
		}
	}
}
