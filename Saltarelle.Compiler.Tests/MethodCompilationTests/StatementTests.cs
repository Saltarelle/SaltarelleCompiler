using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests {
	[TestFixture]
	public class StatementTests : MethodCompilerTestBase {
		private void AssertCorrect(string csharp, string js) {
			CompileMethod(csharp);
			string compiled = OutputFormatter.Format(CompiledMethod.Body);
			Assert.That(compiled, Is.EqualTo(js));
		}

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
@"{
	// Some comment
	// And some
	// multiline
	// comment
}
");
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
@"{
}
");
		}
	}
}
