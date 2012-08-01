using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
	[TestFixture]
	public class SwitchTests : MethodCompilerTestBase {
		[Test]
		public void SwitchStatementWorks() {
			AssertCorrect(
@"public void M() {
	int i = 0;
	// BEGIN
	switch (i) {
		case 1:
			int x = 0;
			break;
		case 2:
		case 3: {
			int y = 0;
			break;
		}
		default:
			int z = 0;
			break;
	}
	// END
}",
@"	switch ($i) {
		case 1: {
			var $x = 0;
			break;
		}
		case 2:
		case 3: {
			var $y = 0;
			break;
		}
		default: {
			var $z = 0;
			break;
		}
	}
");
		}

		[Test]
		public void SwitchStatementWithSwitchExpressionThatRequiresExtraStatementsWorks() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	switch (SomeProperty = i) {
		case 1:
			int x = 0;
			break;
	}
	// END
}",
@"	this.set_$SomeProperty($i);
	switch ($i) {
		case 1: {
			var $x = 0;
			break;
		}
	}
");
		}

		[Test]
		public void CanSwitchOnString() {
			AssertCorrect(
@"public void M() {
	string s = null;
	// BEGIN
	switch (s) {
		case ""X"":
			int x = 0;
			break;
	}
	// END
}",
@"	switch ($s) {
		case 'X': {
			var $x = 0;
			break;
		}
	}
");
		}

		[Test]
		public void GotoCaseAndGotoDefaultStatementsWork() {
			AssertCorrect(
@"public void M() {
	long i = 0;
	// BEGIN
	switch (i) {
		case 1:
			goto case 2;
		case 2:
			if (true) {
				goto case 3L;
			}
			else {
				goto case 4U;
			}
		case 3:
		case 4:
		case 5:
			break;
		case 6:
			goto case 3;
		case 7:
			break;
		case 8:
			goto default;
		default:
			break;
	}
	// END
}",
@"	switch ($i) {
		case 1: {
			goto $label1;
		}
		case 2: {
			$label1:
			if (true) {
				goto $label2;
			}
			else {
				goto $label2;
			}
		}
		case 3:
		case 4:
		case 5: {
			$label2:
			break;
		}
		case 6: {
			goto $label2;
		}
		case 7: {
			break;
		}
		case 8: {
			goto $label3;
		}
		default: {
			$label3:
			break;
		}
	}
");
		}

		[Test]
		public void GotoCaseAndGotoDefaultStatementsWorkWithNestedSwitches() {
			AssertCorrect(
@"public void M() {
	int i = 0, j = 0;
	// BEGIN
	switch (i) {
		case 1:
			switch (j) {
				case 1:
					goto case 2;
				case 2:
					goto default;
				default:
					break;
			}
			if (true)
				goto case 2;
			else
				goto default;
		case 2:
			break;
		default:
			break;
	}
	// END
}",
@"	switch ($i) {
		case 1: {
			switch ($j) {
				case 1: {
					goto $label3;
				}
				case 2: {
					$label3:
					goto $label4;
				}
				default: {
					$label4:
					break;
				}
			}
			if (true) {
				goto $label1;
			}
			else {
				goto $label2;
			}
		}
		case 2: {
			$label1:
			break;
		}
		default: {
			$label2:
			break;
		}
	}
");
		}

		[Test]
		public void NullCaseWorksWithGotoCase() {
			AssertCorrect(
@"public void M() {
	int? i = 0;
	// BEGIN
	switch (i) {
		case 1:
			goto case null;
		case 2:
			goto default;
		case null:
			break;
		default:
			break;
	}
	// END
}",
@"	switch ($i) {
		case 1: {
			goto $label1;
		}
		case 2: {
			goto $label2;
		}
		case null: {
			$label1:
			break;
		}
		default: {
			$label2:
			break;
		}
	}
");
		}

		[Test]
		public void FieldsImplementedAsConstantsWorkAsCaseLabels() {
			AssertCorrect(
@"enum E { Value1, Value2 }
public void M() {
	E e = E.Value1;
	// BEGIN
	switch (e) {
		case E.Value1:
			int x = 0;
			break;
		case E.Value2:
			int y = 0;
			break;
		default:
			int z = 0;
			break;
	}
	// END
}",
@"	switch ($e) {
		case 'Value1': {
			var $x = 0;
			break;
		}
		case 'Value2': {
			var $y = 0;
			break;
		}
		default: {
			var $z = 0;
			break;
		}
	}
", namingConvention: new MockNamingConventionResolver { GetFieldSemantics = f => f.DeclaringType.Name == "E" ? FieldScriptSemantics.StringConstant(f.Name) : FieldScriptSemantics.Field(f.Name) });
		}
	}
}
