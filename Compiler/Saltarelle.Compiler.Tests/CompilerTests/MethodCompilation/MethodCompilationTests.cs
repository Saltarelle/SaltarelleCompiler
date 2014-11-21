using System;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	[TestFixture]
	public class MethodCompilationTests : MethodCompilerTestBase {
		[Test]
		public void NormalMethodCanBeCompiled() {
			AssertCorrect(@"
int M(int a, string b) {
	return a;
}",
@"function($a, $b) {
	// @(3, 2) - (3, 11)
	return $a;
	// @(4, 1) - (4, 2)
}", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name) }, addSourceLocations: true);
		}

		[Test]
		public void StaticMethodWithThisAsFirstArgumentCanBeCompiled() {
			AssertCorrect(@"
int M(int a, string b) {
	return a;
}",
@"function($this, $a, $b) {
	// @(3, 2) - (3, 11)
	return $a;
	// @(4, 1) - (4, 2)
}", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) }, addSourceLocations: true);
		}
	}
}
