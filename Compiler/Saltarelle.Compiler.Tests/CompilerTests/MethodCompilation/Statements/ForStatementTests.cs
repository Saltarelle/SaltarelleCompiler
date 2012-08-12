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
@"	for (var $i = 0, $j = 1; $i < 10; $i++) {
		var $k = $i;
	}
");
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
@"	for ($i = 0; $i < 10; $i++) {
		var $k = 0;
	}
");
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
@"	for ($i = 0, $j = 1; $i < 10; $i++) {
		var $k = 0;
	}
");
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
@"	this.set_$SomeProperty(1);
	var $i = 1, $j = 2, $k = 3;
	this.set_$SomeProperty($i);
	for (var $l = $i, $m = 4; $i < 10; $i++) {
		var $x = 0;
	}
");
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
@"	this.set_$SomeProperty(1);
	$i = 1, $j = 2, $k = 3;
	this.set_$SomeProperty($i);
	for ($l = $i, $m = 4; $i < 10; $i++) {
		var $x = 0;
	}
");
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
@"	for (; $i < 10; $i++) {
		var $k = $i;
	}
");
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
@"	for (var $i = 0;; $i++) {
		var $k = $i;
	}
");
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
@"	for (var $i = 0; $i < 10;) {
		var $k = $i;
	}
");
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
@"	for (; $i < 10; $i++, $j++) {
		var $k = 0;
	}
");
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
@"	for (;;) {
		var $k = 0;
	}
");
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
@"	for (var $i = 0;; $i++) {
		this.set_$SomeProperty(1);
		if (!($i < 1)) {
			break;
		}
		var $x = 0;
	}
");
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
@"	for ($i = 0; $i < 10;) {
		var $x = 0;
		this.set_$SomeProperty(1);
		$i = 1;
		$j = 2;
		$k = 3;
		this.set_$SomeProperty($i);
		$l = $i;
		$m = 4;
	}
");
		}
	}
}
