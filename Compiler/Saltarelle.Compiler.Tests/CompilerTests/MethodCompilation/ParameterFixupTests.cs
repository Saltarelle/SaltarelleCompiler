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
					compiledConstructor = res;
				}
			}, metadataImporter: metadataImporter);

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
		public void ParametersUsingByRefSemanticsAreConvertedAtTheTopOfStatementLambda() {
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
	{sm_B}.call(this);
	$c.$ = 0;
	{sm_C}.F($a);
	{sm_C}.F($b);
	{sm_C}.F($c);
	{sm_C}.F($d);
}", "C");
		}

		[Test]
		public void ExpandedParamArrayIsFixedAtTheTopOfMethods() {
			AssertCorrect(
@"void M(int a, int b, params int[] c) {}",
@"function($a, $b) {
	var $c = Array.prototype.slice.call(arguments, 2);
}", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: true) });
		}

		[Test]
		public void ExpandedParamArrayIsFixedAtTheTopOfMethodsForStaticMethodsWithThisAsFirstArgument() {
			AssertCorrect(
@"void M(int a, int b, params int[] c) {}",
@"function($this, $a, $b) {
	var $c = Array.prototype.slice.call(arguments, 3);
}", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name, expandParams: true) });
		}

		[Test]
		public void ExpandedParamArrayIsFixedAtTheTopOfAnonymousDelegateExpressions() {
			AssertCorrect(
@"delegate void D(int a, int b, string c, params object[] d);
void M() {
	D x = delegate(int a, int b, string c, object[] d) {};
}",
@"function() {
	var $x = function($a, $b, $c) {
		var $d = Array.prototype.slice.call(arguments, 3);
	};
}", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = m => new DelegateScriptSemantics(expandParams: true) });
		}

		[Test]
		public void ExpandedParamArrayIsFixedAtTheTopOfStatementLambda() {
			AssertCorrect(
@"delegate void D(int a, params object[] b);
void M() {
	D x = (int a, object[] b) => {};
}",
@"function() {
	var $x = function($a) {
		var $b = Array.prototype.slice.call(arguments, 1);
	};
}", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = m => new DelegateScriptSemantics(expandParams: true) });
		}

		[Test]
		public void ExpandedParamArrayIsFixedAtTheTopOfExpressionLambda() {
			AssertCorrect(
@"delegate int D(params object[] b);
void M() {
	D x = (object[] b) => 0;
}",
@"function() {
	var $x = function() {
		var $b = Array.prototype.slice.call(arguments, 0);
		return 0;
	};
}", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = m => new DelegateScriptSemantics(expandParams: true) });
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
	var $c = Array.prototype.slice.call(arguments, 2);
	{sm_B}.call(this);
	var $x = 0;
}", "C", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(expandParams: c.ContainingType.Name == "C") });
		}
	}
}
