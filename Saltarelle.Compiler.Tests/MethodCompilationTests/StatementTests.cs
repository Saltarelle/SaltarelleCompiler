using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests {
	[TestFixture]
	public class StatementTests : MethodCompilerTestBase {
		private void AssertCorrect(string csharp, string expected) {
			CompileMethod(csharp);
			string actual = OutputFormatter.Format(CompiledMethod.Body);

			int begin = actual.IndexOf("// BEGIN");
			if (begin > -1) {
				while (begin < (actual.Length - 1) && actual[begin - 1] != '\n')
					begin++;
				actual = actual.Substring(begin);
			}

			int end = actual.IndexOf("// END");
			if (end >= 0) {
				while (end >= 0 && actual[end] != '\n')
					end--;
				actual = actual.Substring(0, end + 1);
			}
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
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

		[Test]
		public void VariableDeclarationsWithoutInitializerWork() {
			AssertCorrect(
@"public void M() {
	int i, j;
	string s;
}",
@"{
	var $i, $j;
	var $s;
}
");
		}

		[Test]
		public void VariableDeclarationsWithInitializerWork() {
			AssertCorrect(
@"public void M() {
	int i = 0, j = 1;
	string s = ""X"";
}",
@"{
	var $i = 0, $j = 1;
	var $s = 'X';
}
");
		}

		[Test]
		public void VariableDeclarationsForVariablesUsedByReferenceWork() {
			AssertCorrect(
@"public void OtherMethod(out int x, out int y) { x = 0; y = 0; }
public void M() {
	// BEGIN
	int i = 0, j;
	// END
	OtherMethod(out i, out j);
}",
@"	var $i = { $: 0 }, $j = { $: null };
");
		}

		[Test]
		public void VariableDeclarationsWhichRequireMultipleStatementsWork() {
			AssertCorrect(
@"public int SomeProperty { get; set; }
public void M() {
	int i = (SomeProperty = 1), j = 2, k = 3, l = (SomeProperty = i), m = 4;
}",
@"{
	this.set_SomeProperty(1);
	var $i = 1, $j = 2, $k = 3;
	this.set_SomeProperty($i);
	var $l = $i, $m = 4;
}
");
		}
	}
}
