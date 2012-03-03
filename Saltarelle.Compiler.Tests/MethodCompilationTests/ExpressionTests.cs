using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests {
	[TestFixture]
	public class ExpressionTests : MethodCompilerTestBase {
		private void AssertCorrect(string csharp, string js) {
			CompileMethod(csharp);
			string compiled = OutputFormatter.Format(CompiledMethod.Body);
			Assert.That(compiled, Is.EqualTo(js));
		}
	}
}
