using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class ThrowStatementTests : MethodCompilerTestBase {
		[Test]
		public void ThrowStatementThatDoesNotRequireAdditionalStatementsWorks() {
			AssertCorrect(
@"public void M() {
	System.Exception ex = null;
	// BEGIN
	throw ex;
	// END
}",
@"	// @(4, 2) - (4, 11)
	throw $ex;
", addSourceLocations: true);
		}

		[Test]
		public void ThrowStatementThatRequiresAdditionalStatementsWorks() {
			AssertCorrect(
@"Exception MyProperty { get; set; }
public void M() {
	Exception ex = null;
	// BEGIN
	throw (MyProperty = ex);
	// END
}",
@"	// @(5, 2) - (5, 26)
	this.set_$MyProperty($ex);
	throw $ex;
", addSourceLocations: true);
		}

		[Test]
		public void RethrowStatementWorksThatDoesNotRequireAdditionalStatementsWorks() {
			AssertCorrect(
@"class ArgumentException : Exception {}
public void M() {
	// BEGIN
	try {
		try {
			try {
			}
			catch (Exception) {
				if (true) {
					throw;
				}
			}
		}
		catch (ArgumentException) {
			throw;
		}
	}
	catch {
		try {
		}
		catch {
			throw;
		}
		throw;
	}
	// END
}",
@"	try {
		try {
			try {
			}
			catch ($tmp1) {
				// @(9, 5) - (9, 14)
				if (true) {
					// @(10, 6) - (10, 12)
					throw $tmp1;
				}
			}
		}
		catch ($tmp2) {
			// @(14, 3) - (14, 28)
			$tmp2 = $MakeException($tmp2);
			if ($TypeIs($tmp2, {ct_ArgumentException})) {
				// @(15, 4) - (15, 10)
				throw $tmp2;
			}
			else {
				// @(16, 3) - (16, 4)
				throw $tmp2;
			}
		}
	}
	catch ($tmp3) {
		try {
		}
		catch ($tmp4) {
			// @(22, 4) - (22, 10)
			throw $tmp4;
		}
		// @(24, 3) - (24, 9)
		throw $tmp3;
	}
", addSourceLocations: true);
		}
	}
}
