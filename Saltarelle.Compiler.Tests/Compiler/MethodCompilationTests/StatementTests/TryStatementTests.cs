using NUnit.Framework;
using Saltarelle.Compiler.Tests.MethodCompilationTests;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.StatementTests {
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
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (System.ArgumentException ex) {
		int y = 0;
	}
	// END
}",
@"	try {
		var $x = 0;
	}
	catch ($tmp1) {
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
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (System.ArgumentException) {
		int y = 0;
	}
	// END
}",
@"	try {
		var $x = 0;
	}
	catch ($tmp1) {
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
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (System.ArgumentNullException ex) {
		int y = 0;
	}
	catch (System.ArgumentException) {
		int z = 0;
	}
	// END
}",
@"	try {
		var $x = 0;
	}
	catch ($tmp1) {
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
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (System.ArgumentNullException) {
		int y = 0;
	}
	catch (System.ArgumentException ex) {
		int z = 0;
	}
	catch (System.Exception) {
		int z2 = 0;
	}
	// END
}",
@"	try {
		var $x = 0;
	}
	catch ($tmp1) {
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
		public void TryCatchBlockThatCatchesSpecificExceptionTypesAndHasEmptyCatchClauseWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (System.ArgumentNullException) {
		int y = 0;
	}
	catch (System.ArgumentException) {
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
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (System.ArgumentNullException) {
		int y = 0;
	}
	catch (System.ArgumentException) {
		int z = 0;
	}
	catch (System.Exception) {
		int z2 = 0;
	}
	catch (System.InvalidOperationException) {
	}
	catch (System.ArgumentOutOfRangeException) {
	}
	// END
}",
@"	try {
		var $x = 0;
	}
	catch ($tmp1) {
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
@"public void M() {
	// BEGIN
	try {
		int x = 0;
	}
	catch (System.ArgumentException) {
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
