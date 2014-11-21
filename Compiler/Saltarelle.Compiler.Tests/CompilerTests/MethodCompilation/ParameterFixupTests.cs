using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	[TestFixture]
	public class ParameterFixupTests : MethodCompilerTestBase {
		protected void AssertCorrectConstructor(string source, string expected, string className, IMetadataImporter metadataImporter = null) {
			JsExpression compiledConstructor = null;
			Compile(new[] { source }, methodCompiled: (m, res, mc) => {
				if (m.MethodKind == MethodKind.Constructor && m.ContainingType.Name == className) {
					compiledConstructor = SourceLocationsInserter.Process(res);
				}
			}, metadataImporter: metadataImporter);

			Assert.That(compiledConstructor, Is.Not.Null, "No constructor was compiled.");

			string actual = OutputFormatter.Format(compiledConstructor, allowIntermediates: true);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}

		[Test]
		public void ParametersUsingByRefSemanticsAreConvertedAtTheTopOfMethods() {
			AssertCorrect(
@"void F(ref int x) {}
void M(int a, ref int b, out int c, int d, int e) {
	c = 0;
	F(ref a);
	F(ref b);
	F(ref c);
	F(ref d);
}",
@"function($a, $b, $c, $d, $e) {
	// @(2, 51) - (2, 52)
	$a = { $: $a };
	$d = { $: $d };
	// @(3, 2) - (3, 8)
	$c.$ = 0;
	// @(4, 2) - (4, 11)
	this.$F($a);
	// @(5, 2) - (5, 11)
	this.$F($b);
	// @(6, 2) - (6, 11)
	this.$F($c);
	// @(7, 2) - (7, 11)
	this.$F($d);
	// @(8, 1) - (8, 2)
}", addSourceLocations: true);
		}

		[Test]
		public void ParametersUsingByRefSemanticsAreConvertedAtTheTopOfAnonymousDelegateExpressions() {
			AssertCorrect(
@"static void F(ref int x) {}
delegate void D(int a, ref int b, out int c, int d, int e);
void M() {
	// BEGIN
	D x = delegate(int a, ref int b, out int c, int d, int e) {
		c = 0;
		F(ref a);
		F(ref b);
		F(ref c);
		F(ref d);
	};
	// END
}",
@"	// @(5, 2) - (11, 4)
	var $x = function($a, $b, $c, $d, $e) {
		// @(5, 60) - (5, 61)
		$a = { $: $a };
		$d = { $: $d };
		// @(6, 3) - (6, 9)
		$c.$ = 0;
		// @(7, 3) - (7, 12)
		{sm_C}.$F($a);
		// @(8, 3) - (8, 12)
		{sm_C}.$F($b);
		// @(9, 3) - (9, 12)
		{sm_C}.$F($c);
		// @(10, 3) - (10, 12)
		{sm_C}.$F($d);
		// @(11, 2) - (11, 3)
	};
", addSourceLocations: true);
		}

		[Test]
		public void ParametersUsingByRefSemanticsAreConvertedAtTheTopOfStatementLambda() {
			AssertCorrect(
@"static void F(ref int x) {}
delegate void D(int a, ref int b, out int c, int d, int e);
void M() {
	// BEGIN
	D x = (int a, ref int b, out int c, int d, int e) => {
		c = 0;
		F(ref a);
		F(ref b);
		F(ref c);
		F(ref d);
	};
	// END
}",
@"	// @(5, 2) - (11, 4)
	var $x = function($a, $b, $c, $d, $e) {
		// @(5, 55) - (5, 56)
		$a = { $: $a };
		$d = { $: $d };
		// @(6, 3) - (6, 9)
		$c.$ = 0;
		// @(7, 3) - (7, 12)
		{sm_C}.$F($a);
		// @(8, 3) - (8, 12)
		{sm_C}.$F($b);
		// @(9, 3) - (9, 12)
		{sm_C}.$F($c);
		// @(10, 3) - (10, 12)
		{sm_C}.$F($d);
		// @(11, 2) - (11, 3)
	};
", addSourceLocations: true);
		}

		[Test]
		public void ParametersUsingByRefSemanticsAreConvertedAtTheTopOfExpressionLambda() {
			AssertCorrect(
@"static void F(ref int x) {}
delegate void D(int a);
void M() {
	// BEGIN
	D x = a => F(ref a);
	// END
}",
@"	// @(5, 2) - (5, 22)
	var $x = function($a) {
		// @(5, 13) - (5, 21)
		$a = { $: $a };
		{sm_C}.$F($a);
	};
", addSourceLocations: true);
		}

		[Test]
		public void ParametersUsingByRefSemanticsAreConvertedAtTheTopOfConstructors() {
			AssertCorrectConstructor(
@"class B {}
class C : B {
	static void F(ref int x) {}
	C(int a, ref int b, out int c, int d, int e) {
		c = 0;
		F(ref a);
		F(ref b);
		F(ref c);
		F(ref d);
	}
}",
@"function($a, $b, $c, $d, $e) {
	// @(4, 47) - (4, 48)
	$a = { $: $a };
	$d = { $: $d };
	{sm_B}.call(this);
	// @(5, 3) - (5, 9)
	$c.$ = 0;
	// @(6, 3) - (6, 12)
	{sm_C}.F($a);
	// @(7, 3) - (7, 12)
	{sm_C}.F($b);
	// @(8, 3) - (8, 12)
	{sm_C}.F($c);
	// @(9, 3) - (9, 12)
	{sm_C}.F($d);
	// @(10, 2) - (10, 3)
}", "C");
		}

		[Test]
		public void ExpandedParamArrayIsFixedAtTheTopOfMethods() {
			AssertCorrect(@"
void M(int a, int b, params int[] c) {
}",
@"function($a, $b) {
	// @(2, 38) - (2, 39)
	var $c = Array.prototype.slice.call(arguments, 2);
	// @(3, 1) - (3, 2)
}", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: true) }, addSourceLocations: true);
		}

		[Test]
		public void ExpandedParamArrayIsFixedAtTheTopOfMethodsForStaticMethodsWithThisAsFirstArgument() {
			AssertCorrect(@"
void M(int a, int b, params int[] c) {
}",
@"function($this, $a, $b) {
	// @(2, 38) - (2, 39)
	var $c = Array.prototype.slice.call(arguments, 3);
	// @(3, 1) - (3, 2)
}", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name, expandParams: true) }, addSourceLocations: true);
		}

		[Test]
		public void ExpandedParamArrayIsFixedAtTheTopOfAnonymousDelegateExpressions() {
			AssertCorrect(
@"delegate void D(int a, int b, string c, params object[] d);
void M() {
	// BEGIN
	D x = delegate(int a, int b, string c, object[] d) {};
	// END
}",
@"	// @(4, 2) - (4, 56)
	var $x = function($a, $b, $c) {
		// @(4, 53) - (4, 54)
		var $d = Array.prototype.slice.call(arguments, 3);
		// @(4, 54) - (4, 55)
	};
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = m => new DelegateScriptSemantics(expandParams: true) }, addSourceLocations: true);
		}

		[Test]
		public void ExpandedParamArrayIsFixedAtTheTopOfStatementLambda() {
			AssertCorrect(
@"delegate void D(int a, params object[] b);
void M() {
	// BEGIN
	D x = (int a, object[] b) => {};
	// END
}",
@"	// @(4, 2) - (4, 34)
	var $x = function($a) {
		// @(4, 31) - (4, 32)
		var $b = Array.prototype.slice.call(arguments, 1);
		// @(4, 32) - (4, 33)
	};
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = m => new DelegateScriptSemantics(expandParams: true) }, addSourceLocations: true);
		}

		[Test]
		public void ExpandedParamArrayIsFixedAtTheTopOfExpressionLambda() {
			AssertCorrect(
@"delegate int D(params object[] b);
void M() {
	// BEGIN
	D x = (object[] b) => 0;
	// END
}",
@"	// @(4, 2) - (4, 26)
	var $x = function() {
		// @(4, 24) - (4, 25)
		var $b = Array.prototype.slice.call(arguments, 0);
		return 0;
	};
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = m => new DelegateScriptSemantics(expandParams: true) }, addSourceLocations: true);
		}

		[Test]
		public void ExpandedParamArrayIsFixedAtTheTopOfConstructors() {
			AssertCorrectConstructor(
@"class B {}
class C : B {
	C(int a, string b, params object[] c) {
		int x = 0;
	}
}",
@"function($a, $b) {
	// @(3, 40) - (3, 41)
	var $c = Array.prototype.slice.call(arguments, 2);
	{sm_B}.call(this);
	// @(4, 3) - (4, 13)
	var $x = 0;
	// @(5, 2) - (5, 3)
}", "C", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(expandParams: c.ContainingType.Name == "C") });
		}
	}
}
