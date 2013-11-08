using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class RuntimeLibraryIntegrationTests : MethodCompilerTestBase {
		[Test]
		public void EnsureCanBeEvaluatedMultipleTimesWorks() {
			AssertCorrect(
@"bool? GetA() { return null; }
bool? GetB() { return null; }
void M() {
	bool? c = null, d = null;
	// BEGIN
	var x1 = c | d;
	var x2 = GetA() | d;
	var x3 = c | GetA();
	var x4 = GetA() | GetB();
	// END
}",
@"	var $x1 = $LiftedBooleanOr($c, $d);
	var $x2 = $LiftedBooleanOr(this.$GetA(), $d);
	var $tmp1 = this.$GetA();
	var $x3 = $LiftedBooleanOr($c, $tmp1);
	var $tmp2 = this.$GetA();
	var $tmp3 = this.$GetB();
	var $x4 = $LiftedBooleanOr($tmp2, $tmp3);
", runtimeLibrary: new MockRuntimeLibrary { LiftedBooleanOr = (a, b, c) => { var l = new[] { a }; b = c.EnsureCanBeEvaluatedMultipleTimes(b, l); return JsExpression.Invocation(JsExpression.Identifier("$LiftedBooleanOr"), l[0], b); } } );
		}
	}
}
