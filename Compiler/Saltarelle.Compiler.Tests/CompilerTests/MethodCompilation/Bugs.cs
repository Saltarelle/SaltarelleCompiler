using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	[TestFixture]
	public class Bugs : MethodCompilerTestBase {
		[Test]
		public void DuplicateUsingDirectivesDoNotCauseIssues() {
			AssertCorrect(@"
using foo;
using foo;
namespace bar {
	public class Bar {
		public void M() {
			// BEGIN
			var fm = new Foo();
			// END
		}
	}
}
namespace foo {
	public class Foo {
	}
}",
@"	var $fm = new {inst_Foo}();
", addSkeleton: false);
		}
	}
}
