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
		var $x = 0;
	}
	finally {
		var $y = 0;
	}
");
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
		var $x = 0;
	}
	catch ($tmp1) {
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ArgumentException})) {
			var $ex = $Cast($tmp1, {ArgumentException});
			var $y = 0;
		}
		else {
			throw $tmp1;
		}
	}
");
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
		var $x = 0;
	}
	catch ($tmp1) {
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ArgumentException})) {
			var $y = 0;
		}
		else {
			throw $tmp1;
		}
	}
");
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
		var $x = 0;
	}
	catch ($tmp1) {
		var $ex = $MakeException($tmp1);
		var $y = 0;
	}
");
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
		var $x = 0;
	}
	catch ($tmp1) {
		var $y = 0;
	}
");
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
		var $x = 0;
	}
	catch ($tmp1) {
		var $y = 0;
	}
");
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
		var $x = 0;
	}
	catch ($tmp1) {
		var $ex = $MakeException($tmp1);
		var $y = 0;
	}
");
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
		var $x = 0;
	}
	catch ($tmp1) {
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ArgumentNullException})) {
			var $ex = $Cast($tmp1, {ArgumentNullException});
			var $y = 0;
		}
		else if ($TypeIs($tmp1, {ArgumentException})) {
			var $z = 0;
		}
		else {
			throw $tmp1;
		}
	}
");
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
		var $x = 0;
	}
	catch ($tmp1) {
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ArgumentNullException})) {
			var $y = 0;
		}
		else if ($TypeIs($tmp1, {ArgumentException})) {
			var $ex = $Cast($tmp1, {ArgumentException});
			var $z = 0;
		}
		else {
			var $z2 = 0;
		}
	}
");
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
		var $x = 0;
	}
	catch ($tmp1) {
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ArgumentNullException})) {
			var $y = 0;
		}
		else if ($TypeIs($tmp1, {ArgumentException})) {
			var $ex = $Cast($tmp1, {ArgumentException});
			var $z = 0;
		}
		else {
			var $ex2 = $tmp1;
			var $z2 = 0;
		}
	}
");
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
		var $x = 0;
	}
	catch ($tmp1) {
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ArgumentNullException})) {
			var $y = 0;
		}
		else if ($TypeIs($tmp1, {ArgumentException})) {
			var $z = 0;
		}
		else {
			var $z2 = 0;
		}
	}
");
		}

		[Test]
		public void TryCatchBlockThatCatchesSpecificExceptionTypesAfterSystemExceptionWorks() {
			AssertCorrect(
@"class ArgumentException : Exception {}
class ArgumentNullException : ArgumentException {}
class InvalidOperationException : Exception {}
class ArgumentOutOfRangeException : ArgumentException {}
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
	catch (Exception) {
		int z2 = 0;
	}
	catch (InvalidOperationException) {
	}
	catch (ArgumentOutOfRangeException) {
	}
	// END
}",
@"	try {
		var $x = 0;
	}
	catch ($tmp1) {
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ArgumentNullException})) {
			var $y = 0;
		}
		else if ($TypeIs($tmp1, {ArgumentException})) {
			var $z = 0;
		}
		else {
			var $z2 = 0;
		}
	}
");
		}

		[Test]
		public void TryCatchBlockThatCatchesSystemExceptionFirstWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (System.Exception) {
		int y = 0;
	}
	catch (System.InvalidOperationException) {
		int z1 = 0;
	}
	catch (System.ArgumentOutOfRangeException) {
		int z2 = 0;
	}
	// END
}",
@"	try {
		var $x = 0;
	}
	catch ($tmp1) {
		var $y = 0;
	}
");
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
		var $x = 0;
	}
	catch ($tmp1) {
		$tmp1 = $MakeException($tmp1);
		if ($TypeIs($tmp1, {ArgumentException})) {
			var $y = 0;
		}
		else {
			throw $tmp1;
		}
	}
	finally {
		var $z = 0;
	}
");
		}
	}
}
