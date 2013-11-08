﻿using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class BinaryNonAssignmentOperatorTests : MethodCompilerTestBase {
		protected void AssertCorrectForBulkOperators(string csharp, string expected, bool includeEqualsAndNotEquals, IMetadataImporter metadataImporter = null) {
			// Bulk operators are all except for division, shift right, coalesce and the logical operators.
			foreach (var op in new[] { "*", "%", "+", "-", "<<", "<", ">", "<=", ">=", "&", "^", "|" }) {
				var jsOp = (op == "==" || op == "!=" ? op + "=" : op);	// Script should use strict equals (===) rather than normal equals (==)
				AssertCorrect(csharp.Replace("+", op), expected.Replace("+", jsOp), metadataImporter);
			}

			if (includeEqualsAndNotEquals) {
				AssertCorrect(csharp.Replace("+", "=="), expected.Replace("+", "==="), metadataImporter);
				AssertCorrect(csharp.Replace("+", "!="), expected.Replace("+", "!=="), metadataImporter);
			}
		}

		[Test]
		public void NonLiftedBulkOperatorsWork() {
			AssertCorrectForBulkOperators(
@"public void M() {
	int a = 0, b = 0;
	// BEGIN
	var c = a + b;
	// END
}",
@"	var $c = $a + $b;
", includeEqualsAndNotEquals: true);
		}

		[Test]
		public void LogicalAndWorks() {
			AssertCorrect(
@"public void M() {
	bool a = false, b = false;
	// BEGIN
	var c = a && b;
	// END
}",
@"	var $c = $a && $b;
");
		}

		[Test]
		public void LogicalAndWorksWhenTheSecondExpressionHasAdditionalStatements() {
			AssertCorrect(
@"public bool P { get; set; }

public void M() {
	bool a = false, b = false;
	// BEGIN
	var c = a && (P = b);
	// END
}",
@"	var $tmp1 = $a;
	if ($tmp1) {
		this.set_$P($b);
		$tmp1 = $b;
	}
	var $c = $tmp1;
");
		}


		[Test]
		public void LogicalOrWorks() {
			AssertCorrect(
@"public void M() {
	bool a = false, b = false;
	// BEGIN
	var c = a || b;
	// END
}",
@"	var $c = $a || $b;
");
		}

		[Test]
		public void LogicalOrWorksWhenTheSecondExpressionHasAdditionalStatements() {
			AssertCorrect(
@"public bool P { get; set; }

public void M() {
	bool a = false, b = false;
	// BEGIN
	var c = a || (P = b);
	// END
}",
@"	var $tmp1 = $a;
	if (!$tmp1) {
		this.set_$P($b);
		$tmp1 = $b;
	}
	var $c = $tmp1;
");
		}

		[Test]
		public void ArgumentsAreEvaluatedInTheCorrectOrder() {
			AssertCorrectForBulkOperators(
@"public int P { get; set; }
public void M() {
	int a = 0;
	// BEGIN
	var c = P + (P = a);
	// END
}",
@"	var $tmp1 = this.get_$P();
	this.set_$P($a);
	var $c = $tmp1 + $a;
", includeEqualsAndNotEquals: true);
		}

		[Test]
		public void SimpleOperatorWithTwoMethodCallsWorks() {
			AssertCorrect(
@"public int F1() { return 0; }
public int F2() { return 0; }
public void M() {
	// BEGIN
	var x = F1() + F2();
	// END
}",
@"	var $x = this.$F1() + this.$F2();
");
		}

		[Test]
		public void LiftedBulkOperatorsWork() {
			AssertCorrectForBulkOperators(
@"public void M() {
	int? a, b;
	// BEGIN
	var c = a + b;
	// END
}",
@"	var $c = $Lift($a + $b);
", includeEqualsAndNotEquals: false);
		}

		[Test]
		public void EqualsAndNotEqualsWithNullables() {
			AssertCorrect(@"
public void M<T>(T? x) where T : struct {
	int? y = 0;
	int z1 = 0;
	int? z2 = 0;
	// BEGIN
	bool b1 = x == null;
	bool b2 = x != null;
	bool b3 = y == null;
	bool b4 = y != null;
	bool b5 = y == z1;
	bool b6 = y != z1;
	bool b7 = y == z2;
	bool b8 = y != z2;
	// END
}",
@"	var $b1 = $ReferenceEquals($x, null);
	var $b2 = $ReferenceNotEquals($x, null);
	var $b3 = $ReferenceEquals($y, null);
	var $b4 = $ReferenceNotEquals($y, null);
	var $b5 = $y === $z1;
	var $b6 = $y !== $z1;
	var $b7 = $ReferenceEquals($y, $z2);
	var $b8 = $ReferenceNotEquals($y, $z2);
");
		}

		[Test]
		public void NonLiftedIntegerDivisionWorks() {
			DoForAllIntegerTypes(type =>
				AssertCorrect(
@"public void M() {
	type a = 0, b = 0;
	// BEGIN
	var c = a / b;
	// END
}".Replace("type", type),
@"	var $c = $IntDiv($a, $b);
"));
		}

		[Test]
		public void LiftedIntegerDivisionWorks() {
			DoForAllIntegerTypes(type =>
				AssertCorrect(
@"public void M() {
	type? a = 0, b = 0;
	// BEGIN
	var c = a / b;
	// END
}".Replace("type", type),
@"	var $c = $Lift($IntDiv($a, $b));
"));
		}

		[Test]
		public void NonLiftedFloatingPointDivisionWorks() {
			DoForAllFloatingPointTypes(type =>
				AssertCorrect(
@"public void M() {
	type a = 0, b = 0;
	// BEGIN
	var c = a / b;
	// END
}".Replace("type", type),
@"	var $c = $a / $b;
"));
		}

		[Test]
		public void LiftedFloatingPointDivisionWorks() {
			DoForAllFloatingPointTypes(type =>
				AssertCorrect(
@"public void M() {
	type? a = 0, b = 0;
	// BEGIN
	var c = a / b;
	// END
}".Replace("type", type),
@"	var $c = $Lift($a / $b);
"));
		}

		[Test]
		public void NonLiftedSignedRightShiftWorks() {
			AssertCorrect(
@"public void M() {
	int a = 0;
	int b = 0;
	// BEGIN
	var c = a >> b;
	// END
}",
@"	var $c = $a >> $b;
");

			AssertCorrect(
@"public void M() {
	long a = 0;
	int b = 0;
	// BEGIN
	var c = a >> b;
	// END
}",
@"	var $c = $a >> $b;
");
		}

		[Test]
		public void LiftedSignedRightShiftWorks() {
			AssertCorrect(
@"public void M() {
	int? a = 0;
	int b = 0;
	// BEGIN
	var c = a >> b;
	// END
}",
@"	var $c = $Lift($a >> $b);
");

			AssertCorrect(
@"public void M() {
	long? a = 0;
	int b = 0;
	// BEGIN
	var c = a >> b;
	// END
}",
@"	var $c = $Lift($a >> $b);
");
		}

		[Test]
		public void NonLiftedUnsignedRightShiftWorks() {
			AssertCorrect(
@"public void M() {
	uint a = 0;
	int b = 0;
	// BEGIN
	var c = a >> b;
	// END
}",
@"	var $c = $a >>> $b;
");

			AssertCorrect(
@"public void M() {
	ulong a = 0;
	int b = 0;
	// BEGIN
	var c = a >> b;
	// END
}",
@"	var $c = $a >>> $b;
");
		}

		[Test]
		public void LiftedUnsignedRightShiftWorks() {
			AssertCorrect(
@"public void M() {
	uint? a = 0;
	int b = 0;
	// BEGIN
	var c = a >> b;
	// END
}",
@"	var $c = $Lift($a >>> $b);
");

			AssertCorrect(
@"public void M() {
	ulong? a = 0;
	int b = 0;
	// BEGIN
	var c = a >> b;
	// END
}",
@"	var $c = $Lift($a >>> $b);
");
		}

		[Test]
		public void CoalesceWorksForObjectThatCannotBeFalsy() {
			AssertCorrect(
@"public void M() {
	int[] a = null, b = null;
	// BEGIN
	var c = a ?? b;
	// END
}",
@"	var $c = $a || $b;
");
		}

		[Test]
		public void CoalesceWorksForNumberWhenSecondOperandHasNoSideEffects() {
			AssertCorrect(
@"int? A { get; set; }
public void M() {
	int? b = null;
	// BEGIN
	var c = A ?? b;
	// END
}",
@"	var $c = $Coalesce(this.get_$A(), $b);
");
		}

		[Test]
		public void CoalesceWorksForNumbers() {
			DoForAllNumericTypes(type => 
				AssertCorrect(
@"type? A { get; set; }
type? B { get; set; }
type C { get; set; }
public void M() {
	// BEGIN
	var x = A ?? B;
	var y = A ?? C;
	// END
}".Replace("type", type),
@"	var $tmp1 = this.get_$A();
	if ($ReferenceEquals($tmp1, null)) {
		$tmp1 = this.get_$B();
	}
	var $x = $tmp1;
	var $tmp2 = this.get_$A();
	if ($ReferenceEquals($tmp2, null)) {
		$tmp2 = this.get_$C();
	}
	var $y = $tmp2;
"));
		}

		[Test]
		public void CoalesceWorksForString() {
			AssertCorrect(
@"string A { get; set; }
string B { get; set; }
public void M() {
	// BEGIN
	var x = A ?? B;
	// END
}",
@"	var $tmp1 = this.get_$A();
	if ($ReferenceEquals($tmp1, null)) {
		$tmp1 = this.get_$B();
	}
	var $x = $tmp1;
");
		}

		[Test]
		public void CoalesceWorksForBoolean() {
			AssertCorrect(
@"bool? A { get; set; }
bool? B { get; set; }
bool C { get; set; }
public void M() {
	// BEGIN
	var x = A ?? B;
	var y = A ?? C;
	// END
}",
@"	var $tmp1 = this.get_$A();
	if ($ReferenceEquals($tmp1, null)) {
		$tmp1 = this.get_$B();
	}
	var $x = $tmp1;
	var $tmp2 = this.get_$A();
	if ($ReferenceEquals($tmp2, null)) {
		$tmp2 = this.get_$C();
	}
	var $y = $tmp2;
");
		}

		[Test]
		public void CoalesceWorksForEnumTypes() {
			AssertCorrect(
@"enum E {}
E? A { get; set; }
E? B { get; set; }
E C { get; set; }
public void M() {
	// BEGIN
	var x = A ?? B;
	var y = A ?? C;
	// END
}",
@"	var $tmp1 = this.get_$A();
	if ($ReferenceEquals($tmp1, null)) {
		$tmp1 = this.get_$B();
	}
	var $x = $tmp1;
	var $tmp2 = this.get_$A();
	if ($ReferenceEquals($tmp2, null)) {
		$tmp2 = this.get_$C();
	}
	var $y = $tmp2;
");
		}

		[Test]
		public void CoalesceWorksForDynamicAndObjectAndValueTypeAndEnum() {
			foreach (var type in new[] { "dynamic", "object", "System.ValueType", "System.Enum" })
				AssertCorrect(
@"type A { get; set; }
type B { get; set; }
public void M() {
	// BEGIN
	var x = A ?? B;
	// END
}".Replace("type", type),
@"	var $tmp1 = this.get_$A();
	if ($ReferenceEquals($tmp1, null)) {
		$tmp1 = this.get_$B();
	}
	var $x = $tmp1;
");
		}

		[Test]
		public void CoalesceWorksForNonFalsyTypeWhenTheSecondExpressionHasAdditionalStatements() {
			AssertCorrect(
@"int[] P { get; set; }
public void M() {
	int[] a = null, b = null;
	// BEGIN
	var c = a ?? (P = b);
	// END
}",
@"	var $tmp1 = $a;
	if ($ReferenceEquals($tmp1, null)) {
		this.set_$P($b);
		$tmp1 = $b;
	}
	var $c = $tmp1;
");
		}

		[Test]
		public void CoalesceWorksForNumberWhenTheSecondExpressionHasAdditionalStatements() {
			AssertCorrect(
@"int? P { get; set; }
public void M() {
	int? a = null, b = null;
	// BEGIN
	var c = a ?? (P = b);
	// END
}",
@"	var $tmp1 = $a;
	if ($ReferenceEquals($tmp1, null)) {
		this.set_$P($b);
		$tmp1 = $b;
	}
	var $c = $tmp1;
");
		}

		[Test]
		public void NonLiftedBooleanAndWorks() {
			AssertCorrect(
@"public void M() {
	bool a = false, b = false;
	// BEGIN
	var c = a & b;
	// END
}",
@"	var $c = $a & $b;
");
		}

		[Test]
		public void NonLiftedBooleanOrWorks() {
			AssertCorrect(
@"public void M() {
	bool a = false, b = false;
	// BEGIN
	var c = a | b;
	// END
}",
@"	var $c = $a | $b;
");
		}

		[Test]
		public void LiftedBooleanAndWorks() {
			AssertCorrect(
@"public void M() {
	bool? a = false, b = false;
	// BEGIN
	var c = a & b;
	// END
}",
@"	var $c = $LiftedBooleanAnd($a, $b);
");
		}

		[Test]
		public void LiftedBooleanOrWorks() {
			AssertCorrect(
@"public void M() {
	bool? a = false, b = false;
	// BEGIN
	var c = a | b;
	// END
}",
@"	var $c = $LiftedBooleanOr($a, $b);
");
		}

		[Test]
		public void AddForDelegateTypeInvokesDelegateCombine() {
			AssertCorrect(
@"bool? P { get; set; }
public void M() {
	Action a = null, b = null;
	// BEGIN
	Action c = a + b;
	// END
}",
@"	var $c = {sm_Delegate}.$Combine($a, $b);
");
		}

		[Test]
		public void SubtractForDelegateTypeInvokesDelegateRemove() {
			AssertCorrect(
@"bool? P { get; set; }
public void M() {
	Action a = null, b = null;
	// BEGIN
	Action c = a - b;
	// END
}",
@"	var $c = {sm_Delegate}.$Remove($a, $b);
");
		}

		[Test]
		public void BinaryOperatorsWorkForDynamicMember() {
			AssertCorrectForBulkOperators(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d.someField + 123;
	// END
}",
@"	var $i = $d.someField + 123;
", includeEqualsAndNotEquals: false);

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d.someField == 123;
	// END
}",
@"	var $i = $ReferenceEquals($d.someField, 123);
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d.someField != 123;
	// END
}",
@"	var $i = $ReferenceNotEquals($d.someField, 123);
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d.someField / 123;
	// END
}",
@"	var $i = $d.someField / 123;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d.someField ?? 123;
	// END
}",
@"	var $i = $Coalesce($d.someField, 123);
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	bool b = false;
	// BEGIN
	var i = d.someField && $b;
	// END
}",
@"	var $i = $d.someField && $b;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	bool b = false;
	// BEGIN
	var i = d.someField || $b;
	// END
}",
@"	var $i = $d.someField || $b;
");
		}

		[Test]
		public void BinaryOperatorsWorkForDynamicObject() {
			AssertCorrectForBulkOperators(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d + 123;
	// END
}",
@"	var $i = $d + 123;
", includeEqualsAndNotEquals: false);

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d == 123;
	// END
}",
@"	var $i = $ReferenceEquals($d, 123);
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d != 123;
	// END
}",
@"	var $i = $ReferenceNotEquals($d, 123);
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d / 123;
	// END
}",
@"	var $i = $d / 123;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d ?? 123;
	// END
}",
@"	var $i = $Coalesce($d, 123);
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	bool b = false;
	// BEGIN
	var i = d && b;
	// END
}",
@"	var $i = $d && $b;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	bool b = false;
	// BEGIN
	var i = d || b;
	// END
}",
@"	var $i = $d || $b;
");
		}

		[Test]
		public void EqualityForReferenceTypesIsDelegatedToTheRuntimeLibrary() {
			AssertCorrect(
@"public void M() {
	object o1 = null, o2 = null;
	// BEGIN
	bool b = o1 == o2;
	// END
}",
@"	var $b = $ReferenceEquals($o1, $o2);
");

			AssertCorrect(
@"public void M() {
	System.Collections.JsDictionary o1 = null, o2 = null;
	// BEGIN
	bool b = o1 == o2;
	// END
}",
@"	var $b = $ReferenceEquals($Upcast($o1, {ct_Object}), $Upcast($o2, {ct_Object}));
");
		}

		[Test]
		public void InequalityForReferenceTypesIsDelegatedToTheRuntimeLibrary() {
			AssertCorrect(
@"public void M() {
	object o1 = null, o2 = null;
	// BEGIN
	bool b = o1 != o2;
	// END
}",
@"	var $b = $ReferenceNotEquals($o1, $o2);
");

			AssertCorrect(
@"using System.Collections;
public void M() {
	System.Collections.JsDictionary o1 = null, o2 = null;
	// BEGIN
	bool b = o1 != o2;
	// END
}",
@"	var $b = $ReferenceNotEquals($Upcast($o1, {ct_Object}), $Upcast($o2, {ct_Object}));
");
		}
	}
}
