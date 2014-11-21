using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
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
@"	// @(4, 2) - (4, 12)
	switch ($i) {
		case 1: {
			// @(6, 4) - (6, 14)
			var $x = 0;
			// @(7, 4) - (7, 10)
			break;
		}
		case 2:
		case 3: {
			// @(10, 4) - (10, 14)
			var $y = 0;
			// @(11, 4) - (11, 10)
			break;
		}
		default: {
			// @(14, 4) - (14, 14)
			var $z = 0;
			// @(15, 4) - (15, 10)
			break;
		}
	}
", addSourceLocations: true);
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
@"	// @(5, 2) - (5, 27)
	this.set_$SomeProperty($i);
	switch ($i) {
		case 1: {
			// @(7, 4) - (7, 14)
			var $x = 0;
			// @(8, 4) - (8, 10)
			break;
		}
	}
", addSourceLocations: true);
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
@"	// @(4, 2) - (4, 12)
	switch ($s) {
		case 'X': {
			// @(6, 4) - (6, 14)
			var $x = 0;
			// @(7, 4) - (7, 10)
			break;
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void GotoCaseAndGotoDefaultStatementsWork() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

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
@"	// @(4, 2) - (4, 12)
	switch ($i) {
		case 1: {
			// @(6, 4) - (6, 16)
			goto $label1;
		}
		case 2: {
			$label1:
			// @(8, 4) - (8, 13)
			if (true) {
				// @(9, 5) - (9, 18)
				goto $label2;
			}
			else {
				// @(12, 5) - (12, 18)
				goto $label2;
			}
		}
		case 3:
		case 4:
		case 5: {
			$label2:
			// @(17, 4) - (17, 10)
			break;
		}
		case 6: {
			// @(19, 4) - (19, 16)
			goto $label2;
		}
		case 7: {
			// @(21, 4) - (21, 10)
			break;
		}
		case 8: {
			// @(23, 4) - (23, 17)
			goto $label3;
		}
		default: {
			$label3:
			// @(25, 4) - (25, 10)
			break;
		}
	}
", addSourceLocations: true);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void GotoCaseAndGotoDefaultStatementsWorkWithNestedSwitches() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

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
@"	// @(4, 2) - (4, 12)
	switch ($i) {
		case 1: {
			// @(6, 4) - (6, 14)
			switch ($j) {
				case 1: {
					// @(8, 6) - (8, 18)
					goto $label3;
				}
				case 2: {
					$label3:
					// @(10, 6) - (10, 19)
					goto $label4;
				}
				default: {
					$label4:
					// @(12, 6) - (12, 12)
					break;
				}
			}
			// @(14, 4) - (14, 13)
			if (true) {
				// @(15, 5) - (15, 17)
				goto $label1;
			}
			else {
				// @(17, 5) - (17, 18)
				goto $label2;
			}
		}
		case 2: {
			$label1:
			// @(19, 4) - (19, 10)
			break;
		}
		default: {
			$label2:
			// @(21, 4) - (21, 10)
			break;
		}
	}
", addSourceLocations: true);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void NullCaseWorksWithGotoCase() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

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
@"	// @(4, 2) - (4, 12)
	switch ($i) {
		case 1: {
			// @(6, 4) - (6, 19)
			goto $label1;
		}
		case 2: {
			// @(8, 4) - (8, 17)
			goto $label2;
		}
		case null: {
			$label1:
			// @(10, 4) - (10, 10)
			break;
		}
		default: {
			$label2:
			// @(12, 4) - (12, 10)
			break;
		}
	}
", addSourceLocations: true);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
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
@"	// @(5, 2) - (5, 12)
	switch ($e) {
		case 'Value1': {
			// @(7, 4) - (7, 14)
			var $x = 0;
			// @(8, 4) - (8, 10)
			break;
		}
		case 'Value2': {
			// @(10, 4) - (10, 14)
			var $y = 0;
			// @(11, 4) - (11, 10)
			break;
		}
		default: {
			// @(13, 4) - (13, 14)
			var $z = 0;
			// @(14, 4) - (14, 10)
			break;
		}
	}
", metadataImporter: new MockMetadataImporter { GetFieldSemantics = f => f.ContainingType.Name == "E" ? FieldScriptSemantics.StringConstant(f.Name) : FieldScriptSemantics.Field(f.Name) }, addSourceLocations: true);
		}
	}
}
