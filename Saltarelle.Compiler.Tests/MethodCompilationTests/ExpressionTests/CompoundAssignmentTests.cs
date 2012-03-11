using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class CompoundAssignmentTests : MethodCompilerTestBase {
		protected new void AssertCorrectForBulkOperators(string csharp, string expected, INamingConventionResolver namingConvention = null) {
			// Bulk operators are all except for division and shift right.
			foreach (var op in new[] { "+", "*", "%", "-", "<<", "&", "|", "^" }) {
				AssertCorrect(csharp.Replace("+", op), expected.Replace("+", op), namingConvention);
			}
		}

		[Test]
		public void CompoundAssignmentWorksForLocalVariables() {
			AssertCorrectForBulkOperators(
@"public void M() {
	int i = 0, j = 1;
	// BEGIN
	i += j;
	// END
}
",
@"	$i += $j;
");
		}

		[Test]
		public void CompoundAssignmentChainWorksForlLocalVariables() {
			AssertCorrectForBulkOperators(
@"public void M() {
	int i = 0, j = 1, k = 2;;
	// BEGIN
	i += j += k;
	// END
}
",
@"	$i += $j += $k;
");
		}

		[Test]
		public void CompoundAssignmentToPropertyWithMethodsWorks() {
			AssertCorrectForBulkOperators(
@"public int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P += i;
	// END
}",
@"	this.set_P(this.get_P() + $i);
");
		}

		[Test]
		public void CompoundAssignmentToPropertyWithMethodsOnlyInvokesTheTargetOnce() {
			AssertCorrectForBulkOperators(
@"class X { public int P { get; set; } }
public X F() { return null; }
public void M() {
	int i = 0;
	// BEGIN
	F().P += i;
	// END
}",
@"	var $tmp1 = this.F();
	$tmp1.set_P($tmp1.get_P() + $i);
");
		}

		[Test]
		public void CompoundAssignmentToPropertyWithMethodsEvaluatesTheArgumentsInCorrectOrder() {
			AssertCorrectForBulkOperators(
@"class X { public int P { get; set; } }
public X F1() { return null; }
public int F2() { return null; }
public int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F1().P += (P = F2());
	// END
}",
@"	var $tmp1 = this.F1();
	var $tmp3 = $tmp1.get_P();
	var $tmp2 = this.F2();
	this.set_P($tmp2);
	$tmp1.set_P($tmp3 + $tmp2);
");
		}

		[Test]
		public void CompoundAssignmentChainForPropertiesWithMethodsWorksWithSimpleArgument() {
			AssertCorrectForBulkOperators(
@"public int P1 { get; set; }
public int P2 { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P1 += P2 += i;
	// END
}",
@"	var $tmp2 = this.get_P1();
	var $tmp1 = this.get_P2() + $i;
	this.set_P2($tmp1);
	this.set_P1($tmp2 + $tmp1);
");
		}

		[Test]
		public void CompoundAssignmentChainForPropertiesWithMethodsWorksWhenReturnValueUsed() {
			AssertCorrectForBulkOperators(
@"public int P1 { get; set; }
public int P2 { get; set; }
public int F() { return 0; }
public void M() {
	// BEGIN
	if ((P1 += P2 += F()) < 0) {
	}
	// END
}",
@"	var $tmp3 = this.get_P1();
	var $tmp1 = this.get_P2();
	var $tmp2 = $tmp1 + this.F();
	this.set_P2($tmp2);
	var $tmp4 = $tmp3 + $tmp2;
	this.set_P1($tmp4);
	if ($tmp4 < 0) {
	}
");
		}

		[Test]
		public void CompoundAssigningToPropertyWithFieldImplementationWorks() {
			AssertCorrectForBulkOperators(
@"public int F { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F += i;
	// END
}",
@"	this.F += $i;
");
		}

		[Test]
		public void CompoundAssignmentToPropertyWithFieldImplementationDoesNotGenerateTemporary() {
			AssertCorrectForBulkOperators(
@"class X { public int F { get; set; } }
public X F() { return null; }
public void M() {
	int i = 0;
	// BEGIN
	F().F += i;
	// END
}",
@"	this.F().F += $i;
");
		}

		[Test]
		public void CompoundAssignmentChainForPropertiesWithFieldImplementationWorks() {
			AssertCorrectForBulkOperators(
@"public int F1 { get; set; }
public int F2 { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F1 += F2 += i;
	// END
}",
@"	this.F1 += this.F2 += $i;
");
		}

		[Test]
		public void CompoundAssignmentToPropertyWithFieldImplementationCorrectlyOrdersExpressions() {
			AssertCorrectForBulkOperators(
@"class X { public int F { get; set; } }
public X F() { return null; }
public int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F().F += (P = i);
	// END
}",
@"	var $tmp1 = this.F();
	this.set_P($i);
	$tmp1.F += $i;
");
		}

		[Test]
		public void CompoundAssigningToStaticPropertyWithSetMethodWorks() {
			AssertCorrectForBulkOperators(
@"static int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P += i;
	// END
}",
@"	{C}.set_P({C}.get_P() + $i);
");
		}

		[Test]
		public void CompoundAssigningToStaticPropertyWithFieldImplementationWorks() {
			AssertCorrectForBulkOperators(
@"static int F { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F += i;
	// END
}",
@"	{C}.F += $i;
");
		}

		[Test]
		public void CompoundAssigningToIndexerWithSetMethodWorks() {
			AssertCorrectForBulkOperators(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2;
	// BEGIN
	this[i, j] += k;
	// END
}",
@"	this.set_Item($i, $j, this.get_Item($i, $j) + $k);
");
		}

		[Test]
		public void CompoundAssigningToIndexerWithMethodsWorksWhenUsingTheReturnValue() {
			AssertCorrectForBulkOperators(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2, l;
	// BEGIN
	l = this[i, j] += k;
	// END
}",
@"	var $tmp1 = this.get_Item($i, $j) + $k;
	this.set_Item($i, $j, $tmp1);
	$l = $tmp1;
");
		}

		[Test]
		public void CompoundAssigningToIndexerWithMethodsOnlyInvokesIndexingArgumentsOnceAndInTheCorrectOrder() {
			AssertCorrectForBulkOperators(
@"int this[int x, int y] { get { return 0; } set {} }
public int F1() { return 0; }
public int F2() { return 0; }
public int F3() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	this[F1(), F2()] += F3();
	// END
}",
@"	var $tmp1 = this.F1();
	var $tmp2 = this.F2();
	var $tmp3 = this.get_Item($tmp1, $tmp2);
	this.set_Item($tmp1, $tmp2, $tmp3 + this.F3());
");
		}

		[Test]
		public void CompoundAssigningToPropertyImplementedAsNativeIndexerWorks() {
			AssertCorrectForBulkOperators(
@"int this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2, l;
	// BEGIN
	l = this[i] += k;
	// END
}",
@"	$l = this[$i] += $k;
", namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => p.IsIndexer ? PropertyImplOptions.NativeIndexer() : PropertyImplOptions.Field(p.Name) });
		}

		[Test]
		public void CompoundAssigningToInstanceFieldWorks() {
			AssertCorrectForBulkOperators(
@"int a, b;
public void M() {
	int i = 0;
	// BEGIN
	a += b += i;
	// END
}",
@"	this.$a += this.$b += $i;
");
		}

		[Test]
		public void CompoundAssigningToStaticFieldWorks() {
			AssertCorrectForBulkOperators(
@"static int a, b;
public void M() {
	int i = 0;
	// BEGIN
	a += b += i;
	// END
}",
@"	{C}.$a += {C}.$b += $i;
");
		}

		[Test]
		public void DivisionCompoundAssignmentWorksForLocalDoubles() {
			AssertCorrectForBulkOperators(
@"public void M() {
	int i = 0, j = 1;
	// BEGIN
	i += j;
	// END
}
",
@"	$i += $j;
");
		}

		[Test]
		public void DivisionCompoundAssignmentWorksForLocalIntegralVariables() {
			DoForAllIntegerTypes(type =>
				AssertCorrect(
@"public void M() {
	type i = 0, j = 1;
	// BEGIN
	i /= j;
	// END
}
".Replace("type", type),
@"	$i = $IntDiv($i, $j);
"));
		}

		[Test]
		public void DivisionCompoundAssignmentWorksForLocalFloatingPointVariables() {
			DoForAllFloatingPointTypes(type =>
				AssertCorrect(
@"public void M() {
	type i = 0, j = 1;
	// BEGIN
	i /= j;
	// END
}
".Replace("type", type),
@"	$i /= $j;
"));
		}

		[Test]
		public void DivisionCompoundAssignmentWorksForIntegralPropertiesImplementedWithMethods() {
			DoForAllIntegerTypes(type =>
				AssertCorrect(
@"public type P { get; set; }
public void M() {
	type i = 0;
	// BEGIN
	P /= i;
	// END
}
".Replace("type", type),
@"	this.set_P($IntDiv(this.get_P(), $i));
"));
		}

		[Test]
		public void DivisionCompoundAssignmentWorksForFloatingPointPropertiesImplementedWithMethods() {
			DoForAllFloatingPointTypes(type =>
				AssertCorrect(
@"public type P { get; set; }
public void M() {
	type i = 0;
	// BEGIN
	P /= i;
	// END
}
".Replace("type", type),
@"	this.set_P(this.get_P() / $i);
"));
		}

		[Test]
		public void DivisionCompoundAssignmentWorksForIntegralPropertiesImplementedAsFields() {
			DoForAllIntegerTypes(type =>
				AssertCorrect(
@"public type F { get; set; }
public void M() {
	type i = 0;
	// BEGIN
	F /= i;
	// END
}
".Replace("type", type),
@"	this.F = $IntDiv(this.F, $i);
"));
		}

		[Test]
		public void DivisionCompoundAssignmentOnlyInvokesTargetOnceForIntegralPropertiesImplementedAsFields() {
			DoForAllIntegerTypes(type =>
				AssertCorrect(
@"class X { public type F { get; set; } }
public X F1() { return null; }
public void M() {
	type i = 0;
	// BEGIN
	F1().F /= i;
	// END
}
".Replace("type", type),
@"	var $tmp1 = this.F1();
	$tmp1.F = $IntDiv($tmp1.F, $i);
"));
		}

		[Test]
		public void DivisionCompoundAssignmentWorksForFloatingPointPropertiesImplementedAsFields() {
			DoForAllFloatingPointTypes(type =>
				AssertCorrect(
@"public type F { get; set; }
public void M() {
	type i = 0;
	// BEGIN
	F /= i;
	// END
}
".Replace("type", type),
@"	this.F /= $i;
"));
		}

		[Test]
		public void DivisionCompoundAssignmentWorksForIntegralPropertiesImplementedAsNativeIndexers() {
			DoForAllIntegerTypes(type =>
				AssertCorrect(
@"public type this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	type j = 0;
	// BEGIN
	this[i] /= j;
	// END
}
".Replace("type", type),
@"	this[$i] = $IntDiv(this[$i], $j);
", namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => p.IsIndexer ? PropertyImplOptions.NativeIndexer() : PropertyImplOptions.Field(p.Name) }));
		}

		[Test]
		public void DivisionCompoundAssignmentOnlyInvokesTargetOnceForIntegralPropertiesImplementedAsNativeIndexers() {
			DoForAllIntegerTypes(type =>
				AssertCorrect(
@"class X { public type this[int x] { get { return 0; } set {} } }
public X F1() { return null; }
public void M() {
	int i = 0;
	type j = 0;
	// BEGIN
	F1()[i] /= j;
	// END
}
".Replace("type", type),
@"	var $tmp1 = this.F1();
	$tmp1[$i] = $IntDiv($tmp1[$i], $j);
", namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => p.IsIndexer ? PropertyImplOptions.NativeIndexer() : PropertyImplOptions.Field(p.Name) }));
		}

		[Test]
		public void DivisionCompoundAssignmentWorksForFloatingPointPropertiesImplementedAsNativeIndexers() {
			DoForAllFloatingPointTypes(type =>
				AssertCorrect(
@"public type this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	type j = 0;
	// BEGIN
	this[i] /= j;
	// END
}
".Replace("type", type),
@"	this[$i] /= $j;
", namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => p.IsIndexer ? PropertyImplOptions.NativeIndexer() : PropertyImplOptions.Field(p.Name) }));
		}

		[Test]
		public void DivisionCompoundAssignmentToInstanceFieldOnlyInvokesTheTargetOnceForIntegralFields() {
			DoForAllIntegerTypes(type =>
				AssertCorrect(
@"class X { public type a; }
X F() { return null; }
public void M() {
	type i = 0;
	// BEGIN
	F().a /= i;
	// END
}".Replace("type", type),
@"	var $tmp1 = this.F();
	$tmp1.$a = $IntDiv($tmp1.$a, $i);
"));
		}

		[Test]
		public void DivisionCompoundAssignmentToLocalIntegralVariableWorks() {
			DoForAllIntegerTypes(type =>
				AssertCorrect(
@"public void M() {
	type i = 0, j = 0;
	// BEGIN
	i /= j;
	// END
}".Replace("type", type),
@"	$i = $IntDiv($i, $j);
"));
		}

		[Test]
		public void DivisionCompoundAssignmentToLocalFloatingPointVariableWorks() {
			DoForAllFloatingPointTypes(type =>
				AssertCorrect(
@"public void M() {
	type i = 0, j = 0;
	// BEGIN
	i /= j;
	// END
}".Replace("type", type),
@"	$i /= $j;
"));
		}

		[Test]
		public void ShiftRightCompoundAssignmentForUnsignedTypesVariableWorksWhenResultIsCompoundAssignment() {
			DoForAllUnsignedIntegerTypes(type =>
				AssertCorrect(
@"public void M() {
	type i = 0;
	int j = 0;
	// BEGIN
	i >>= j;
	// END
}".Replace("type", type),
@"	$i >>>= $j;
"));
		}

		[Test]
		public void ShiftRightCompoundAssignmentForUnsignedTypesVariableWorksWhenResultIsNormalAssignment() {
			DoForAllUnsignedIntegerTypes(type =>
				AssertCorrect(
@"public type P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P >>= i;
	// END
}".Replace("type", type),
@"	this.set_P(this.get_P() >>> $i);
"));
		}

		[Test]
		public void ShiftRightCompoundAssignmentForSignedTypesVariableWorksWhenResultIsCompoundAssignment() {
			foreach (var type in new[] { "sbyte", "short", "int", "long" }) {
				AssertCorrect(
@"public void M() {
	type i = 0;
	int j = 0;
	// BEGIN
	i >>= j;
	// END
}".Replace("type", type),
@"	$i >>= $j;
");
			}
		}

		[Test]
		public void ShiftRightCompoundAssignmentForSignedTypesVariableWorksWhenResultIsNormalAssignment() {
			DoForAllSignedIntegerTypes(type =>
				AssertCorrect(
@"public type P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P >>= i;
	// END
}".Replace("type", type),
@"	this.set_P(this.get_P() >> $i);
"));
		}

		[Test]
		public void LiftedOperatorsExceptForRightShiftAndDivisionWork() {
			AssertCorrectForBulkOperators(
@"public void M() {
	int? i = 0, j = 1;
	// BEGIN
	i += j;
	// END
}
",
@"	$i = $Lift($i + $j);
");
		}

		[Test]
		public void LiftedIntegerDivisionWorks() {
			DoForAllIntegerTypes(type =>
				AssertCorrect(
@"public void M() {
	type? i = 0, j = 0;
	// BEGIN
	i /= j;
	// END
}".Replace("type", type),
@"	$i = $Lift($IntDiv($i, $j));
"));
		}

		[Test]
		public void LiftedFloatingPointDivisionWorks() {
			DoForAllFloatingPointTypes(type =>
				AssertCorrect(
@"public void M() {
	type? i = 0, j = 0;
	// BEGIN
	i /= j;
	// END
}".Replace("type", type),
@"	$i = $Lift($i / $j);
"));
		}

		[Test]
		public void LiftedSignedRightShiftWorks() {
			DoForAllSignedIntegerTypes(type =>
				AssertCorrect(
@"public void M() {
	type? i = 0;
	int? j = 0;
	// BEGIN
	i >>= j;
	// END
}".Replace("type", type),
@"	$i = $Lift($i >> $j);
"));
		}

		[Test]
		public void LiftedUnsignedRightShiftWorks() {
			DoForAllUnsignedIntegerTypes(type =>
				AssertCorrect(
@"public void M() {
	type? i = 0;
	int? j = 0;
	// BEGIN
	i >>= j;
	// END
}".Replace("type", type),
@"	$i = $Lift($i >>> $j);
"));
		}

		[Test]
		public void UsingPropertyThatIsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableProperty { get; set; } public void M() { UnusableProperty += 0; } }" }, namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableProperty")));
		}
	}
}
