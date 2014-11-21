using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class ForStatementTests : MethodCompilerTestBase {
		[Test]
		public void ForStatementWithVariableDeclarationsWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (int i = 0, j = 1; i < 10; i++) {
		int k = i;
	}
	// END
}",
@"	// @(3, 2) - (3, 37)
	for (var $i = 0, $j = 1; $i < 10; $i++) {
		// @(4, 3) - (4, 13)
		var $k = $i;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForStatementWithVariableDeclarationsWorksStruct() {
			AssertCorrect(
@"struct S { public int b; }
public void M() {
	S s1 = default(S), s2 = default(S);
	// BEGIN
	for (S i = s1, j = s2; i.b < 10; i.b++) {
		S k = i;
	}
	// END
}",
@"	// @(5, 2) - (5, 41)
	for (var $i = $Clone($s1, {to_S}), $j = $Clone($s2, {to_S}); $i.$b < 10; $i.$b++) {
		// @(6, 3) - (6, 11)
		var $k = $Clone($i, {to_S});
	}
", mutableValueTypes: true, addSourceLocations: true);
		}

		[Test]
		public void ForStatementWithoutVariableDeclarationWorks() {
			AssertCorrect(
@"public void M() {
	int i;
	// BEGIN
	for (i = 0; i < 10; i++) {
		int k = 0;
	}
	// END
}",
@"	// @(4, 2) - (4, 26)
	for ($i = 0; $i < 10; $i++) {
		// @(5, 3) - (5, 13)
		var $k = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForStatementWithMultipleInitializersWorks() {
			AssertCorrect(
@"public void M() {
	int i, j;
	// BEGIN
	for (i = 0, j = 1; i < 10; i++) {
		int k = 0;
	}
	// END
}",
@"	// @(4, 2) - (4, 33)
	for ($i = 0, $j = 1; $i < 10; $i++) {
		// @(5, 3) - (5, 13)
		var $k = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForStatementWithVariableDeclarationInitializersRequiringMultipleStatementsWork() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	// BEGIN
	for (int i = (SomeProperty = 1), j = 2, k = 3, l = (SomeProperty = i), m = 4; i < 10; i++) {
		int x = 0;
	}
	// END
}",
@"	// @(4, 2) - (4, 92)
	this.set_$SomeProperty(1);
	var $i = 1, $j = 2, $k = 3;
	this.set_$SomeProperty($i);
	for (var $l = $i, $m = 4; $i < 10; $i++) {
		// @(5, 3) - (5, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForStatementWithExpressionInitializersRequiringMultipleStatementsWork() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	int i, j, k, l, m;
	// BEGIN
	for (i = (SomeProperty = 1), j = 2, k = 3, l = (SomeProperty = i), m = 4; i < 10; i++) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 88)
	this.set_$SomeProperty(1);
	$i = 1, $j = 2, $k = 3;
	this.set_$SomeProperty($i);
	for ($l = $i, $m = 4; $i < 10; $i++) {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForStatementWithoutInitializerWorks() {
			AssertCorrect(
@"public void M() {
	int i = 0;
	// BEGIN
	for (; i < 10; i++) {
		int k = i;
	}
	// END
}",
@"	// @(4, 2) - (4, 21)
	for (; $i < 10; $i++) {
		// @(5, 3) - (5, 13)
		var $k = $i;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForStatementWithoutConditionWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (int i = 0; ; i++) {
		int k = i;
	}
	// END
}",
@"	// @(3, 2) - (3, 24)
	for (var $i = 0;; $i++) {
		// @(4, 3) - (4, 13)
		var $k = $i;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForStatementWithoutIteratorWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (int i = 0; i < 10;) {
		int k = i;
	}
	// END
}",
@"	// @(3, 2) - (3, 26)
	for (var $i = 0; $i < 10;) {
		// @(4, 3) - (4, 13)
		var $k = $i;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForStatementWithMultipleIteratorsWorks() {
			AssertCorrect(
@"public void M() {
	int i = 0, j = 0;
	// BEGIN
	for (; i < 10; i++, j++) {
		int k = 0;
	}
	// END
}",
@"	// @(4, 2) - (4, 26)
	for (; $i < 10; $i++, $j++) {
		// @(5, 3) - (5, 13)
		var $k = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForEverStatementWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	for (;;) {
		int k = 0;
	}
	// END
}",
@"	// @(3, 2) - (3, 10)
	for (;;) {
		// @(4, 3) - (4, 13)
		var $k = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForStatementWithConditionThatNeedExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	// BEGIN
	for (int i = 0; i < (SomeProperty = 1); i++) {
		int x = 0;
	}
	// END
}",
@"	// @(4, 2) - (4, 46)
	for (var $i = 0;; $i++) {
		this.set_$SomeProperty(1);
		if (!($i < 1)) {
			break;
		}
		// @(5, 3) - (5, 13)
		var $x = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void ForStatementWithIteratorThatNeedExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	int i, j, k, l, m;
	// BEGIN
	for (i = 0; i < 10; i = (SomeProperty = 1), j = 2, k = 3, l = (SomeProperty = i), m = 4) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 90)
	for ($i = 0; $i < 10;) {
		// @(6, 3) - (6, 13)
		var $x = 0;
		// @(5, 2) - (5, 90)
		this.set_$SomeProperty(1);
		$i = 1;
		$j = 2;
		$k = 3;
		this.set_$SomeProperty($i);
		$l = $i;
		$m = 4;
	}
", addSourceLocations: true);
		}
	}
}
