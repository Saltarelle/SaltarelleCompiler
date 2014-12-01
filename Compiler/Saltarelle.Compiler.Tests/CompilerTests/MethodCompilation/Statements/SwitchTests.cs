using System.Linq;
using Microsoft.CodeAnalysis;
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
", metadataImporter: new MockMetadataImporter { GetFieldSemantics = f => f.ContainingType.Name == "E" ? FieldScriptSemantics.StringConstant(f.Name) : FieldScriptSemantics.Field(f.Name) });
		}

		[Test]
		public void SwitchStatementWithLargeLabels() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(
@"public void M() {
	long i = 0;
	// BEGIN
	switch (i) {
		case 5000000001:
			goto case 5000000002;
		case 5000000002:
			if (true) {
				goto case -5000000001;
			}
			else {
				goto case -5000000002;
			}
		case -5000000001:
		case -5000000002:
		case 5000000003:
			break;
		case 5000000004:
			goto case -5000000001;
		case 5000000005:
			break;
		case 5000000006:
			goto default;
		default:
			break;
	}
	// END
}",
@"	switch ($i) {
		case 5000000001: {
			goto $label1;
		}
		case 5000000002: {
			$label1:
			if (true) {
				goto $label2;
			}
			else {
				goto $label2;
			}
		}
		case -5000000001:
		case -5000000002:
		case 5000000003: {
			$label2:
			break;
		}
		case 5000000004: {
			goto $label2;
		}
		case 5000000005: {
			break;
		}
		case 5000000006: {
			goto $label3;
		}
		default: {
			$label3:
			break;
		}
	}
");
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void CaseLabelsWhichAreNotExactlyRepresentableInDoubleAreAnError() {
			var er = new MockErrorReporter();
			Compile(new[] {
@"class C {
	public void M() {
		long i = 0;
		switch (i) {
			case (1L << 53) + 1:
				break;
		}
	}
}",
}, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7543));

			er = new MockErrorReporter();
			Compile(new[] {
@"class C {
	public void M() {
		long i = 0;
		switch (i) {
			case -(1L << 53) - 1:
				break;
		}
	}
}",
}, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7543));

			er = new MockErrorReporter();
			Compile(new[] {
@"class C {
	public void M() {
		ulong i = 0;
		switch (i) {
			case (1UL << 63) + 1:
				break;
		}
	}
}",
}, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7543));
		}
	}
}
