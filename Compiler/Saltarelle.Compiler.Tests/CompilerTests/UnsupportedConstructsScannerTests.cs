using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests {
	[TestFixture]
	public class UnsupportedConstructsScannerTests : CompilerTestBase {
		private void AssertCorrect(string code, string feature) {
			var er = new MockErrorReporter();
			Compile(new[] { code }, errorReporter: er, allowUnsupportedConstructs: false);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].Code, Is.EqualTo(7998));
			Assert.That(er.AllMessages[0].Args[0], Is.EqualTo(feature));
		}

		[Test]
		public void StructIsReportedAsUnsupported() {
			AssertCorrect(@"
struct S {
}
", "user-defined value type (struct)");
		}
	}
}
