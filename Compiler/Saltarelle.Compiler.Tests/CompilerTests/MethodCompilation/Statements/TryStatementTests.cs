using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class TryStatementTests : MethodCompilerTestBase {
		[Test]
		public void TryFinallyBlockWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	finally {
		int y = 0;
	}
	// END
}",
@"	try {
		// @(4, 3) - (4, 13)
		var $x = 0;
	}
	finally {
		// @(7, 3) - (7, 13)
		var $y = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void TryCatchBlockThatCatchesSpecificExceptionTypeToAVariableWorks() {
			AssertCorrect(
@"class ArgumentException : Exception {}
public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (ArgumentException ex) {
		int y = 0;
	}
	// END
}",
@"	try {
		// @(5, 3) - (5, 13)
		var $x = 0;
	}
	catch ($tmp1) {
		// @(7, 2) - (7, 30)
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ct_ArgumentException})) {
			var $ex = $Cast($tmp1, {ct_ArgumentException});
			// @(8, 3) - (8, 13)
			var $y = 0;
		}
		else {
			// @(9, 2) - (9, 3)
			throw $tmp1;
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void TryCatchBlockThatCatchesSpecificExceptionTypeWithoutStoringInAVariableWorks() {
			AssertCorrect(
@"class ArgumentException : Exception {}
public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (ArgumentException) {
		int y = 0;
	}
	// END
}",
@"	try {
		// @(5, 3) - (5, 13)
		var $x = 0;
	}
	catch ($tmp1) {
		// @(7, 2) - (7, 27)
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ct_ArgumentException})) {
			// @(8, 3) - (8, 13)
			var $y = 0;
		}
		else {
			// @(9, 2) - (9, 3)
			throw $tmp1;
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void TryCatchBlockWithVariableDeclarationThatOnlyCatchesSystemExceptionWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (System.Exception ex) {
		int y = 0;
	}
	// END
}",
@"	try {
		// @(4, 3) - (4, 13)
		var $x = 0;
	}
	catch ($tmp1) {
		// @(6, 2) - (6, 29)
		var $ex = $MakeException($tmp1);
		// @(7, 3) - (7, 13)
		var $y = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void TryCatchBlockWithoutVariableDeclarationThatOnlyCatchesSystemExceptionWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (System.Exception) {
		int y = 0;
	}
	// END
}",
@"	try {
		// @(4, 3) - (4, 13)
		var $x = 0;
	}
	catch ($tmp1) {
		// @(7, 3) - (7, 13)
		var $y = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void TryCatchBlockWithOnlyEmptyCatchClauseWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch {
		int y = 0;
	}
	// END
}",
@"	try {
		// @(4, 3) - (4, 13)
		var $x = 0;
	}
	catch ($tmp1) {
		// @(7, 3) - (7, 13)
		var $y = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void TryCatchBlockThatCatchesBothSystemExceptionAndHasAnEmptyCatchClauseWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (System.Exception ex) {
		int y = 0;
	}
	catch {
		int z = 0;
	}
	// END
}",
@"	try {
		// @(4, 3) - (4, 13)
		var $x = 0;
	}
	catch ($tmp1) {
		// @(6, 2) - (6, 29)
		var $ex = $MakeException($tmp1);
		// @(7, 3) - (7, 13)
		var $y = 0;
	}
", addSourceLocations: true);
		}

		[Test]
		public void TryCatchBlockThatCatchesTwoSpecificExceptionTypesWorks() {
			AssertCorrect(
@"class ArgumentException : Exception {}
class ArgumentNullException : ArgumentException {}
public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (ArgumentNullException ex) {
		int y = 0;
	}
	catch (ArgumentException) {
		int z = 0;
	}
	// END
}",
@"	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	catch ($tmp1) {
		// @(8, 2) - (8, 34)
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ct_ArgumentNullException})) {
			var $ex = $Cast($tmp1, {ct_ArgumentNullException});
			// @(9, 3) - (9, 13)
			var $y = 0;
		}
		else {
			// @(11, 2) - (11, 27)
			if ($TypeIs($tmp1, {ct_ArgumentException})) {
				// @(12, 3) - (12, 13)
				var $z = 0;
			}
			else {
				// @(13, 2) - (13, 3)
				throw $tmp1;
			}
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void TryCatchBlockThatCatchesSpecificExceptionTypesAndSystemExceptionWorks() {
			AssertCorrect(
@"class ArgumentException : Exception {}
class ArgumentNullException : ArgumentException {}
public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (ArgumentNullException) {
		int y = 0;
	}
	catch (ArgumentException ex) {
		int z = 0;
	}
	catch (Exception) {
		int z2 = 0;
	}
	// END
}",
@"	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	catch ($tmp1) {
		// @(8, 2) - (8, 31)
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ct_ArgumentNullException})) {
			// @(9, 3) - (9, 13)
			var $y = 0;
		}
		else {
			// @(11, 2) - (11, 30)
			if ($TypeIs($tmp1, {ct_ArgumentException})) {
				var $ex = $Cast($tmp1, {ct_ArgumentException});
				// @(12, 3) - (12, 13)
				var $z = 0;
			}
			else {
				// @(15, 3) - (15, 14)
				var $z2 = 0;
			}
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void TryCatchBlockThatCatchesSpecificExceptionTypesAndNamedSystemExceptionWorks() {
			AssertCorrect(
@"class ArgumentException : Exception {}
class ArgumentNullException : ArgumentException {}
public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (ArgumentNullException) {
		int y = 0;
	}
	catch (ArgumentException ex) {
		int z = 0;
	}
	catch (Exception ex2) {
		int z2 = 0;
	}
	// END
}",
@"	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	catch ($tmp1) {
		// @(8, 2) - (8, 31)
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ct_ArgumentNullException})) {
			// @(9, 3) - (9, 13)
			var $y = 0;
		}
		else {
			// @(11, 2) - (11, 30)
			if ($TypeIs($tmp1, {ct_ArgumentException})) {
				var $ex = $Cast($tmp1, {ct_ArgumentException});
				// @(12, 3) - (12, 13)
				var $z = 0;
			}
			else {
				var $ex2 = $tmp1;
				// @(15, 3) - (15, 14)
				var $z2 = 0;
			}
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void TryCatchBlockThatCatchesSpecificExceptionTypesAndHasEmptyCatchClauseWorks() {
			AssertCorrect(
@"class ArgumentException : Exception {}
class ArgumentNullException : ArgumentException {}
public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (ArgumentNullException) {
		int y = 0;
	}
	catch (ArgumentException) {
		int z = 0;
	}
	catch {
		int z2 = 0;
	}
	// END
}",
@"	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	catch ($tmp1) {
		// @(8, 2) - (8, 31)
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ct_ArgumentNullException})) {
			// @(9, 3) - (9, 13)
			var $y = 0;
		}
		else {
			// @(11, 2) - (11, 27)
			if ($TypeIs($tmp1, {ct_ArgumentException})) {
				// @(12, 3) - (12, 13)
				var $z = 0;
			}
			else {
				// @(15, 3) - (15, 14)
				var $z2 = 0;
			}
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void TryCatchFinallyBlockWorks() {
			AssertCorrect(
@"class ArgumentException : Exception {}
public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (ArgumentException) {
		int y = 0;
	}
	finally {
		int z = 0;
	}
	// END
}",
@"	try {
		// @(5, 3) - (5, 13)
		var $x = 0;
	}
	catch ($tmp1) {
		// @(7, 2) - (7, 27)
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ct_ArgumentException})) {
			// @(8, 3) - (8, 13)
			var $y = 0;
		}
		else {
			// @(9, 2) - (9, 3)
			throw $tmp1;
		}
	}
	finally {
		// @(11, 3) - (11, 13)
		var $z = 0;
	}
", addSourceLocations: true);
		}
	}
}
