using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class ThisTests : MethodCompilerTestBase {
		[Test]
		public void ThisWorksInsideNormalMethod() {
			AssertCorrect(
@"int x;
public void M() {
	// BEGIN
	int i = x;
	// END
}",
@"	var $i = this.$x;
");
		}

		[Test]
		public void ThisWorksInsideAnonymousMethodInsideNormalMethod() {
			// It works like this because the anonymous method conversion will perform the correct bind.
			AssertCorrect(
@"int x;
public void M() {
	Action a = () => {
		// BEGIN
		int i = x;
		// END
	};
}",
@"		var $i = this.$x;
");
		}

		[Test]
		public void ThisWorksInsideAnonymousMethodThatUsesByRefVariablesInsideNormalMethod() {
			// It works like this because the anonymous method conversion will perform the correct bind.
			AssertCorrect(
@"void F(ref int a) {}
int x;
public void M() {
	int y = 0;
	F(ref y);
	Action a = () => {
		// BEGIN
		int i = x;
		// END
		int j = y;
	};
}",
@"		var $i = this.$this.$x;
");
		}

		[Test]
		public void ThisWorksInsideStaticMethodWithThisAsFirstArgument() {
			AssertCorrect(
@"int x;
public void M() {
	// BEGIN
	int i = x;
	// END
}",
@"	var $i = $this.$x;
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) });
		}

		[Test]
		public void ThisWorksInsideAnonymousMethodInsideStaticMethodWithThisAsFirstArgument() {
			// It works like this because the anonymous method conversion will perform the correct bind.
			AssertCorrect(
@"int x;
public void M() {
	Action a = () => {
		// BEGIN
		int i = x;
		// END
	};
}",
@"		var $i = $this.$x;
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) });
		}

		[Test]
		public void CannotAccessExpandedParamsParameter() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {
	public void M(int i, int j, params int[] myParamArray) {
		int x = myParamArray[3];
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: true) }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("myParamArray") && er.AllMessagesText[0].Contains("expand") && er.AllMessagesText[0].Contains("param array"));
		}
	}
}
