using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class CommentTests : MethodCompilerTestBase {
		[Test]
		public void CommentsAreCorrectlyTransferred() {
			AssertCorrect(
@"public void M() {
	// Some comment
	/* And some
	   multiline
	   comment
	*/
}",
@"function() {
	// Some comment
	// And some
	// multiline
	// comment
}");
		}

		[Test]
		public void CommentsAreCorrectlyTransferred2() {
			AssertCorrect(
@"public void M() {
	// Comment 1
	int x = 0;
	// Comment 2
}",
@"function() {
	// Comment 1
	var $x = 0;
	// Comment 2
}");
		}

		[Test]
		public void InactiveCodeIsNotTransferred() {
			AssertCorrect(
@"public void M() {
#if FALSE
	This is some stuff
	that should not appear in the script
#endif
}",
@"function() {
}");
		}
	}
}
