using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
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
@"	// @(4, 2) - (4, 8)
	$i = 0;
", addSourceLocations: true);
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
@"	// @(7, 2) - (7, 25)
	this.set_$P3(1);
	this.set_$P2(1);
	this.set_$P1(1);
	$i = 1;
", addSourceLocations: true);
		}

		[Test]
		public void CallToPartialMethodWithoutDefinitionIsRemoved() {
			AssertCorrect(
@"partial class C {
	partial void Method();

	public void M() {
		// BEGIN
		int x = 0;
		Method();
		int y = 0;
		// END
	}
}",
@"	// @(6, 3) - (6, 13)
	var $x = 0;
	// @(8, 3) - (8, 13)
	var $y = 0;
", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void CallToExternMethodIsNotRemoved() {
			AssertCorrect(
@"class C {
	extern void Method();

	public void M() {
		// BEGIN
		int x = 0;
		Method();
		int y = 0;
		// END
	}
}",
@"	// @(6, 3) - (6, 13)
	var $x = 0;
	// @(7, 3) - (7, 12)
	this.$Method();
	// @(8, 3) - (8, 13)
	var $y = 0;
", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void CallToPartialMethodWithDefinitionIsNotRemoved() {
			AssertCorrect(
@"partial class C {
	partial void Method() {
	}
}
partial class C {
	partial void Method();

	public void M() {
		// BEGIN
		int x = 0;
		Method();
		int y = 0;
		// END
	}
}",
@"	// @(10, 3) - (10, 13)
	var $x = 0;
	// @(11, 3) - (11, 12)
	this.$Method();
	// @(12, 3) - (12, 13)
	var $y = 0;
", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void CallToConditionalMethodIsRemovedWhenTheSymbolIsNotDefined() {
			AssertCorrect(
@"class C {
	[System.Diagnostics.Conditional(""__TEST__"")]
	void Method() {
	}

	public void M() {
		// BEGIN
		int x = 0;
		Method();
		int y = 0;
		// END
	}
}",
@"	// @(8, 3) - (8, 13)
	var $x = 0;
	// @(10, 3) - (10, 13)
	var $y = 0;
", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void CallToConditionalMethodIsNotRemovedWhenTheSymbolIsDefined() {
			AssertCorrect(
@"
#define __TEST__
class C {
	[System.Diagnostics.Conditional(""__TEST__"")]
	void Method() {
	}
	public void M() {
		// BEGIN
		int x = 0;
		Method();
		int y = 0;
		// END
	}
}",
@"	// @(9, 3) - (9, 13)
	var $x = 0;
	// @(10, 3) - (10, 12)
	this.$Method();
	// @(11, 3) - (11, 13)
	var $y = 0;
", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void UndefCausesCallToConditionalMethodToBeRemoved() {
			AssertCorrect(
@"
#define __TEST__
#undef __TEST__
class C {
	[System.Diagnostics.Conditional(""__TEST__"")]
	void Method() {
	}

	public void M() {
		// BEGIN
		int x = 0;
		Method();
		int y = 0;
		// END
	}
}",
@"	// @(11, 3) - (11, 13)
	var $x = 0;
	// @(13, 3) - (13, 13)
	var $y = 0;
", addSkeleton: false, addSourceLocations: true);
		}
	}
}
