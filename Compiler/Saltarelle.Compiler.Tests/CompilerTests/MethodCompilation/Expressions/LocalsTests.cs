using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class LocalsTests : MethodCompilerTestBase {
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

		[Test]
		public void CannotAccessExpandedParamsParameterInExpressionLambda() {
			var er = new MockErrorReporter();

			Compile(new[] {
@"public delegate int F(int x, int y, params int[] args);
public class C {
	public void M() {
		F f = (a, b, myParamArray) => myParamArray[1];
	}
}" }, metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(expandParams: true) }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("myParamArray") && er.AllMessagesText[0].Contains("expand") && er.AllMessagesText[0].Contains("param array"));
		}

		[Test]
		public void CannotAccessExpandedParamsParameterInStatementLambda() {
			var er = new MockErrorReporter();

			Compile(new[] {
@"public delegate int F(int x, int y, params int[] args);
public class C {
	public void M() {
		F f = (a, b, myParamArray) => {
			return myParamArray[1];
		};
	}
}" }, metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(expandParams: true) }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("myParamArray") && er.AllMessagesText[0].Contains("expand") && er.AllMessagesText[0].Contains("param array"));
		}
	}
}
