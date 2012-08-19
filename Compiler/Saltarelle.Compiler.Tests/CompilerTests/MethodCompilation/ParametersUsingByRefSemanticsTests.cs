using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	[TestFixture]
	public class ParametersUsingByRefSemanticsTests : MethodCompilerTestBase {
        protected void AssertCorrectConstructor(string source, string expected, string className) {
			JsExpression compiledConstructor = null;
            Compile(new[] { source }, methodCompiled: (m, res, mc) => {
				if (m.IsConstructor && m.DeclaringType.FullName == className) {
					compiledConstructor = res;
				}
            });

			Assert.That(compiledConstructor, Is.Not.Null, "No constructor was compiled.");

			string actual = OutputFormatter.Format(compiledConstructor, allowIntermediates: true);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
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
	$a = { $: $a };
	$d = { $: $d };
	$c.$ = 0;
	this.$F($a);
	this.$F($b);
	this.$F($c);
	this.$F($d);
}");
		}

		[Test]
		public void ParametersUsingByRefSemanticsAreConvertedAtTheTopOfAnonymousDelegateExpressions() {
			AssertCorrect(
@"static void F(ref int x) {}
delegate void D(int a, ref int b, out int c, int d, int e);
void M() {
	D x = delegate(int a, ref int b, out int c, int d, int e) {
		c = 0;
		F(ref a);
		F(ref b);
		F(ref c);
		F(ref d);
	};
}",
@"function() {
	var $x = function($a, $b, $c, $d, $e) {
		$a = { $: $a };
		$d = { $: $d };
		$c.$ = 0;
		{sm_C}.$F($a);
		{sm_C}.$F($b);
		{sm_C}.$F($c);
		{sm_C}.$F($d);
	};
}");
		}

		[Test]
		public void ParametersUsingByRefSemanticsAreConvertedAtTheTopOfMultiExpressionLambda() {
			AssertCorrect(
@"static void F(ref int x) {}
delegate void D(int a, ref int b, out int c, int d, int e);
void M() {
	D x = (int a, ref int b, out int c, int d, int e) => {
		c = 0;
		F(ref a);
		F(ref b);
		F(ref c);
		F(ref d);
	};
}",
@"function() {
	var $x = function($a, $b, $c, $d, $e) {
		$a = { $: $a };
		$d = { $: $d };
		$c.$ = 0;
		{sm_C}.$F($a);
		{sm_C}.$F($b);
		{sm_C}.$F($c);
		{sm_C}.$F($d);
	};
}");
		}

		[Test]
		public void ParametersUsingByRefSemanticsAreConvertedAtTheTopOfExpressionLambda() {
			AssertCorrect(
@"static void F(ref int x) {}
delegate void D(int a);
void M() {
	D x = a => F(ref a);
}",
@"function() {
	var $x = function($a) {
		$a = { $: $a };
		{sm_C}.$F($a);
	};
}");
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
	$a = { $: $a };
	$d = { $: $d };
	{inst_B}.call(this);
	$c.$ = 0;
	{sm_C}.F($a);
	{sm_C}.F($b);
	{sm_C}.F($c);
	{sm_C}.F($d);
}", "C");
		}
	}
}
