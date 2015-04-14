using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class BinaryNonAssignmentOperatorTests : MethodCompilerTestBase {
		protected void AssertCorrectForBulkOperators(string csharp, string expected, bool includeEqualsAndNotEquals, IMetadataImporter metadataImporter = null) {
			// Bulk operators are all except for division, modulo, shift right, coalesce and the logical operators.
			foreach (var op in new[] { "*", "+", "-", "<<", "<", ">", "<=", ">=", "&", "^", "|" }) {
				var jsOp = (op == "==" || op == "!=" ? op + "=" : op);	// Script should use strict equals (===) rather than normal equals (==)
				AssertCorrect(csharp.Replace("+", op), expected.Replace("+", jsOp), metadataImporter);
			}

			if (includeEqualsAndNotEquals) {
				AssertCorrect(csharp.Replace("+", "=="), expected.Replace("+", "==="), metadataImporter);
				AssertCorrect(csharp.Replace("+", "!="), expected.Replace("+", "!=="), metadataImporter);
			}
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
	int? a = 0, b = 0;
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
		public void FieldConstantInLiftedOperation() {
			AssertCorrect(@"
public void M() {
	bool b;
	double? d = 0, d2;
	// BEGIN
	b  = d == double.PositiveInfinity;
	b  = d != double.PositiveInfinity;
	b  = d >= double.PositiveInfinity;
	b  = d <= double.PositiveInfinity;
	b  = d > double.PositiveInfinity;
	b  = d < double.PositiveInfinity;
	d2 = d + double.PositiveInfinity;
	d2 = d - double.PositiveInfinity;
	d2 = d * double.PositiveInfinity;
	d2 = d / double.PositiveInfinity;
	// END
}",
@"	$b = $d === {sm_Double}.$PosInf;
	$b = $d !== {sm_Double}.$PosInf;
	$b = $Lift($d >= {sm_Double}.$PosInf);
	$b = $Lift($d <= {sm_Double}.$PosInf);
	$b = $Lift($d > {sm_Double}.$PosInf);
	$b = $Lift($d < {sm_Double}.$PosInf);
	$d2 = $Lift($d + {sm_Double}.$PosInf);
	$d2 = $Lift($d - {sm_Double}.$PosInf);
	$d2 = $Lift($d * {sm_Double}.$PosInf);
	$d2 = $Lift($d / {sm_Double}.$PosInf);
", metadataImporter: new MockMetadataImporter { GetFieldSemantics = f => FieldScriptSemantics.Field("$PosInf") });
		}

		[Test]
		public void DynamicDivisionWorks() {
			AssertCorrect(
@"public void M() {
	dynamic d = 0, x = 0;
	sbyte sb = 0;
	byte b = 0;
	short s = 0;
	ushort us = 0;
	int i = 0;
	uint ui = 0;
	long l = 0;
	ulong ul = 0;
	float f = 0;
	double o = 0;
	decimal e = 0;
	char c = '0';

	// BEGIN
	x = d  / sb;
	x = d  / b;
	x = d  / s;
	x = d  / us;
	x = d  / i;
	x = d  / ul;
	x = d  / l;
	x = d  / ul;
	x = d  / f;
	x = d  / o;
	x = d  / e;
	x = d  / c;
	x = sb / d;
	x = b  / d;
	x = s  / d;
	x = us / d;
	x = i  / d;
	x = ul / d;
	x = l  / d;
	x = ul / d;
	x = f  / d;
	x = o  / d;
	x = e  / d;
	x = c  / d;
	// END

}",
@"	$x = $d / $sb;
	$x = $d / $b;
	$x = $d / $s;
	$x = $d / $us;
	$x = $d / $i;
	$x = $d / $ul;
	$x = $d / $l;
	$x = $d / $ul;
	$x = $d / $f;
	$x = $d / $o;
	$x = $d / $e;
	$x = $d / $c;
	$x = $sb / $d;
	$x = $b / $d;
	$x = $s / $d;
	$x = $us / $d;
	$x = $i / $d;
	$x = $ul / $d;
	$x = $l / $d;
	$x = $ul / $d;
	$x = $f / $d;
	$x = $o / $d;
	$x = $e / $d;
	$x = $c / $d;
");
		}

		[Test]
		public void NonLiftedSignedRightShiftWithCastWorks() {
				AssertCorrect(
@"public void M() {
	byte a = 0;
	int b = 0;
	// BEGIN
	var c = (int)a >> b;
	// END
}",
@"	var $c = $a >> $b;
");

				AssertCorrect(
@"public void M() {
	ushort a = 0;
	int b = 0;
	// BEGIN
	var c = (int)a >> b;
	// END
}",
@"	var $c = $a >> $b;
");

			AssertCorrect(
@"public void M() {
	uint a = 0;
	int b = 0;
	// BEGIN
	var c = (int)a >> b;
	// END
}",
@"	var $c = $Narrow($a, {ct_Int32}) >> $b;
");
		}

		[Test]
		public void NonLiftedUnsignedRightShiftWithCastWorks() {
				AssertCorrect(
@"public void M() {
	int a = 0;
	int b = 0;
	// BEGIN
	uint c = (uint)a >> b;
	// END
}",
@"	var $c = $Narrow($a, {ct_UInt32}) >>> $b;
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
type C1 { get; set; }
public void M() {
	// BEGIN
	var x = A ?? B;
	var y = A ?? C1;
	// END
}".Replace("type", type),
@"	var $tmp1 = this.get_$A();
	if ($ReferenceEquals($tmp1, null)) {
		$tmp1 = this.get_$B();
	}
	var $x = $tmp1;
	var $tmp2 = this.get_$A();
	if ($ReferenceEquals($tmp2, null)) {
		$tmp2 = this.get_$C1();
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
bool C1 { get; set; }
public void M() {
	// BEGIN
	var x = A ?? B;
	var y = A ?? C1;
	// END
}",
@"	var $tmp1 = this.get_$A();
	if ($ReferenceEquals($tmp1, null)) {
		$tmp1 = this.get_$B();
	}
	var $x = $tmp1;
	var $tmp2 = this.get_$A();
	if ($ReferenceEquals($tmp2, null)) {
		$tmp2 = this.get_$C1();
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
E C1 { get; set; }
public void M() {
	// BEGIN
	var x = A ?? B;
	var y = A ?? C1;
	// END
}",
@"	var $tmp1 = this.get_$A();
	if ($ReferenceEquals($tmp1, null)) {
		$tmp1 = this.get_$B();
	}
	var $x = $tmp1;
	var $tmp2 = this.get_$A();
	if ($ReferenceEquals($tmp2, null)) {
		$tmp2 = this.get_$C1();
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
		public void DelegateEqualityInvokesOperatorEquality()
		{
			AssertCorrect(
@"public void M() {
	Action a = null, b = null;
	// BEGIN
	bool c = a == b;
	// END
}",
@"	var $c = {sm_Delegate}.$op_Equality($a, $b);
");
		}

		[Test]
		public void DelegateEqualityInvokesOperatorInequality()
		{
			AssertCorrect(
@"bool? P { get; set; }
public void M() {
	Action a = null, b = null;
	// BEGIN
	bool c = a != b;
	// END
}",
@"	var $c = {sm_Delegate}.$op_Inequality($a, $b);
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
	var i = d.someField == ""123"";
	// END
}",
@"	var $i = $ReferenceEquals($d.someField, '123');
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d.someField != ""123"";
	// END
}",
@"	var $i = $ReferenceNotEquals($d.someField, '123');
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
	var i = d.someField && b;
	// END
}",
@"	var $i = $d.someField && $b;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	bool b = false;
	// BEGIN
	var i = d.someField || b;
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
	var j = d == null;
	// END
}",
@"	var $i = $d === 123;
	var $j = $ReferenceEquals($d, null);
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = d != 123;
	var j = d != null;
	// END
}",
@"	var $i = $d !== 123;
	var $j = $ReferenceNotEquals($d, null);
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
@"public void M() {
	System.Collections.JsDictionary o1 = null, o2 = null;
	// BEGIN
	bool b = o1 != o2;
	// END
}",
@"	var $b = $ReferenceNotEquals($Upcast($o1, {ct_Object}), $Upcast($o2, {ct_Object}));
");
		}

		[Test]
		public void BitwiseOperationOnLongAndULongIsAnError() {
			foreach (var oper in new[] { "<<", ">>", "|", "&", "^" }) {
				var er = new MockErrorReporter(false);
				Compile(new[] { "class C { public void M() { long v = 0; var v2 = v OPER 1; } }".Replace("OPER", oper) }, errorReporter: er);
				Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.Code == 7540));
			}

			foreach (var oper in new[] { "<<", ">>", "|", "&", "^" }) {
				var er = new MockErrorReporter(false);
				Compile(new[] { "class C { public void M() { ulong v = 0; var v2 = v OPER 1; } }".Replace("OPER", oper) }, errorReporter: er);
				Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.Code == 7540));
			}
		}

		[Test]
		public void BitwiseOperationOnNullableLongAndULongIsAnError() {
			foreach (var oper in new[] { "<<", ">>", "|", "&", "^" }) {
				var er = new MockErrorReporter(false);
				Compile(new[] { "class C { public void M() { long? v = 0; var v2 = v OPER 1; } }".Replace("OPER", oper) }, errorReporter: er);
				Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.Code == 7540));
			}

			foreach (var oper in new[] { "<<", ">>", "|", "&", "^" }) {
				var er = new MockErrorReporter(false);
				Compile(new[] { "class C { public void M() { ulong? v = 0; var v2 = v OPER 1; } }".Replace("OPER", oper) }, errorReporter: er);
				Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.Code == 7540));
			}
		}

		[Test]
		public void AllOperatorsWorkForSByte() {
			AssertCorrect(
@"public void M() {
	sbyte i = 0, j = 1;
	int x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}",
@"	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $IntDiv({ct_Int32}, $i, $j);
		$x = $IntMod({ct_Int32}, $i, $j);
		$x = $i << $j;
		$x = $i >> $j;
		$x = $i & $j;
		$x = $i | $j;
		$x = $i ^ $j;
	}
	{
		$x = $Check($i + $j, {ct_Int32});
		$x = $Check($i - $j, {ct_Int32});
		$x = $Check($i * $j, {ct_Int32});
		$x = $Check($IntDiv({ct_Int32}, $i, $j), {ct_Int32});
		$x = $Check($IntMod({ct_Int32}, $i, $j), {ct_Int32});
		$x = $i << $j;
		$x = $i >> $j;
		$x = $i & $j;
		$x = $i | $j;
		$x = $i ^ $j;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForByte() {
			AssertCorrect(
@"public void M() {
	byte i = 0, j = 1;
	int x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}",
@"	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $IntDiv({ct_Int32}, $i, $j);
		$x = $IntMod({ct_Int32}, $i, $j);
		$x = $i << $j;
		$x = $i >>> $j;
		$x = $i & $j;
		$x = $i | $j;
		$x = $i ^ $j;
	}
	{
		$x = $Check($i + $j, {ct_Int32});
		$x = $Check($i - $j, {ct_Int32});
		$x = $Check($i * $j, {ct_Int32});
		$x = $Check($IntDiv({ct_Int32}, $i, $j), {ct_Int32});
		$x = $Check($IntMod({ct_Int32}, $i, $j), {ct_Int32});
		$x = $i << $j;
		$x = $i >>> $j;
		$x = $i & $j;
		$x = $i | $j;
		$x = $i ^ $j;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForShort() {
			AssertCorrect(
@"public void M() {
	short i = 0, j = 1;
	int x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}",
@"	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $IntDiv({ct_Int32}, $i, $j);
		$x = $IntMod({ct_Int32}, $i, $j);
		$x = $i << $j;
		$x = $i >> $j;
		$x = $i & $j;
		$x = $i | $j;
		$x = $i ^ $j;
	}
	{
		$x = $Check($i + $j, {ct_Int32});
		$x = $Check($i - $j, {ct_Int32});
		$x = $Check($i * $j, {ct_Int32});
		$x = $Check($IntDiv({ct_Int32}, $i, $j), {ct_Int32});
		$x = $Check($IntMod({ct_Int32}, $i, $j), {ct_Int32});
		$x = $i << $j;
		$x = $i >> $j;
		$x = $i & $j;
		$x = $i | $j;
		$x = $i ^ $j;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForUShort() {
			AssertCorrect(
@"public void M() {
	ushort i = 0, j = 1;
	int x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}",
@"	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $IntDiv({ct_Int32}, $i, $j);
		$x = $IntMod({ct_Int32}, $i, $j);
		$x = $i << $j;
		$x = $i >>> $j;
		$x = $i & $j;
		$x = $i | $j;
		$x = $i ^ $j;
	}
	{
		$x = $Check($i + $j, {ct_Int32});
		$x = $Check($i - $j, {ct_Int32});
		$x = $Check($i * $j, {ct_Int32});
		$x = $Check($IntDiv({ct_Int32}, $i, $j), {ct_Int32});
		$x = $Check($IntMod({ct_Int32}, $i, $j), {ct_Int32});
		$x = $i << $j;
		$x = $i >>> $j;
		$x = $i & $j;
		$x = $i | $j;
		$x = $i ^ $j;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForInt() {
			AssertCorrect(
@"public void M() {
	int i = 0, j = 1, x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}",
@"	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $IntDiv({ct_Int32}, $i, $j);
		$x = $IntMod({ct_Int32}, $i, $j);
		$x = $i << $j;
		$x = $i >> $j;
		$x = $i & $j;
		$x = $i | $j;
		$x = $i ^ $j;
	}
	{
		$x = $Check($i + $j, {ct_Int32});
		$x = $Check($i - $j, {ct_Int32});
		$x = $Check($i * $j, {ct_Int32});
		$x = $Check($IntDiv({ct_Int32}, $i, $j), {ct_Int32});
		$x = $Check($IntMod({ct_Int32}, $i, $j), {ct_Int32});
		$x = $i << $j;
		$x = $i >> $j;
		$x = $i & $j;
		$x = $i | $j;
		$x = $i ^ $j;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForUInt() {
			AssertCorrect(
@"public void M() {
	uint i = 0, j = 1, x;
	int k = 1;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << k;
		x = i >> k;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << k;
		x = i >> k;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}",
@"	{
		$x = $Clip($i + $j, {ct_UInt32});
		$x = $Clip($i - $j, {ct_UInt32});
		$x = $Clip($i * $j, {ct_UInt32});
		$x = $Clip($IntDiv({ct_UInt32}, $i, $j), {ct_UInt32});
		$x = $Clip($IntMod({ct_UInt32}, $i, $j), {ct_UInt32});
		$x = $Clip($i << $k, {ct_UInt32});
		$x = $i >>> $k;
		$x = $Clip($i & $j, {ct_UInt32});
		$x = $Clip($i | $j, {ct_UInt32});
		$x = $Clip($i ^ $j, {ct_UInt32});
	}
	{
		$x = $Check($i + $j, {ct_UInt32});
		$x = $Check($i - $j, {ct_UInt32});
		$x = $Check($i * $j, {ct_UInt32});
		$x = $Check($IntDiv({ct_UInt32}, $i, $j), {ct_UInt32});
		$x = $Check($IntMod({ct_UInt32}, $i, $j), {ct_UInt32});
		$x = $Clip($i << $k, {ct_UInt32});
		$x = $i >>> $k;
		$x = $Clip($i & $j, {ct_UInt32});
		$x = $Clip($i | $j, {ct_UInt32});
		$x = $Clip($i ^ $j, {ct_UInt32});
	}
");
		}

		[Test]
		public void AllNonBitwiseOperatorsWorkForLong() {
			AssertCorrect(
@"public void M() {
	long i = 0, j = 1, x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	// END
}",
@"	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $IntDiv({ct_Int64}, $i, $j);
		$x = $IntMod({ct_Int64}, $i, $j);
	}
	{
		$x = $Check($i + $j, {ct_Int64});
		$x = $Check($i - $j, {ct_Int64});
		$x = $Check($i * $j, {ct_Int64});
		$x = $Check($IntDiv({ct_Int64}, $i, $j), {ct_Int64});
		$x = $Check($IntMod({ct_Int64}, $i, $j), {ct_Int64});
	}
");
		}

		[Test]
		public void AllNonBitwiseOperatorsWorkForULong() {
			AssertCorrect(
@"public void M() {
	ulong i = 0, j = 1, x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	// END
}",
@"	{
		$x = $Clip($i + $j, {ct_UInt64});
		$x = $Clip($i - $j, {ct_UInt64});
		$x = $Clip($i * $j, {ct_UInt64});
		$x = $Clip($IntDiv({ct_UInt64}, $i, $j), {ct_UInt64});
		$x = $Clip($IntMod({ct_UInt64}, $i, $j), {ct_UInt64});
	}
	{
		$x = $Check($i + $j, {ct_UInt64});
		$x = $Check($i - $j, {ct_UInt64});
		$x = $Check($i * $j, {ct_UInt64});
		$x = $Check($IntDiv({ct_UInt64}, $i, $j), {ct_UInt64});
		$x = $Check($IntMod({ct_UInt64}, $i, $j), {ct_UInt64});
	}
");
		}

		[Test]
		public void AllOperatorsWorkForChar() {
			AssertCorrect(
@"public void M() {
	char i = '0', j = '1', y;
	int x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}
",
@"	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $IntDiv({ct_Int32}, $i, $j);
		$x = $IntMod({ct_Int32}, $i, $j);
		$x = $i << $j;
		$x = $i >> $j;
		$x = $i & $j;
		$x = $i | $j;
		$x = $i ^ $j;
	}
	{
		$x = $Check($i + $j, {ct_Int32});
		$x = $Check($i - $j, {ct_Int32});
		$x = $Check($i * $j, {ct_Int32});
		$x = $Check($IntDiv({ct_Int32}, $i, $j), {ct_Int32});
		$x = $Check($IntMod({ct_Int32}, $i, $j), {ct_Int32});
		$x = $i << $j;
		$x = $i >> $j;
		$x = $i & $j;
		$x = $i | $j;
		$x = $i ^ $j;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForFloat() {
			AssertCorrect(
@"public void M() {
	float i = 0, j = 1, x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	// END
}",
@"	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $i / $j;
		$x = $i % $j;
	}
	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $i / $j;
		$x = $i % $j;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForDouble() {
			AssertCorrect(
@"public void M() {
	double i = 0, j = 1, x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	// END
}",
@"	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $i / $j;
		$x = $i % $j;
	}
	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $i / $j;
		$x = $i % $j;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForDecimal() {
			AssertCorrect(
@"public void M() {
	decimal i = 0, j = 1, x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	// END
}",
@"	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $i / $j;
		$x = $i % $j;
	}
	{
		$x = $i + $j;
		$x = $i - $j;
		$x = $i * $j;
		$x = $i / $j;
		$x = $i % $j;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableSByte() {
			AssertCorrect(
@"public void M() {
	sbyte? i = 0, j = 1;
	int? x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}",
@"	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
		$x = $IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
		$x = $Lift($i << $j);
		$x = $Lift($i >> $j);
		$x = $Lift($i & $j);
		$x = $Lift($i | $j);
		$x = $Lift($i ^ $j);
	}
	{
		$x = $Check($Lift($i + $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($Lift($i - $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($Lift($i * $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Lift($i << $j);
		$x = $Lift($i >> $j);
		$x = $Lift($i & $j);
		$x = $Lift($i | $j);
		$x = $Lift($i ^ $j);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableByte() {
			AssertCorrect(
@"public void M() {
	byte? i = 0, j = 1;
	int? x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}",
@"	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
		$x = $IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
		$x = $Lift($i << $j);
		$x = $Lift($i >>> $j);
		$x = $Lift($i & $j);
		$x = $Lift($i | $j);
		$x = $Lift($i ^ $j);
	}
	{
		$x = $Check($Lift($i + $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($Lift($i - $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($Lift($i * $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Lift($i << $j);
		$x = $Lift($i >>> $j);
		$x = $Lift($i & $j);
		$x = $Lift($i | $j);
		$x = $Lift($i ^ $j);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableShort() {
			AssertCorrect(
@"public void M() {
	short? i = 0, j = 1;
	int? x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}",
@"	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
		$x = $IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
		$x = $Lift($i << $j);
		$x = $Lift($i >> $j);
		$x = $Lift($i & $j);
		$x = $Lift($i | $j);
		$x = $Lift($i ^ $j);
	}
	{
		$x = $Check($Lift($i + $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($Lift($i - $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($Lift($i * $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Lift($i << $j);
		$x = $Lift($i >> $j);
		$x = $Lift($i & $j);
		$x = $Lift($i | $j);
		$x = $Lift($i ^ $j);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableUShort() {
			AssertCorrect(
@"public void M() {
	ushort? i = 0, j = 1;
	int? x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}",
@"	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
		$x = $IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
		$x = $Lift($i << $j);
		$x = $Lift($i >>> $j);
		$x = $Lift($i & $j);
		$x = $Lift($i | $j);
		$x = $Lift($i ^ $j);
	}
	{
		$x = $Check($Lift($i + $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($Lift($i - $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($Lift($i * $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Lift($i << $j);
		$x = $Lift($i >>> $j);
		$x = $Lift($i & $j);
		$x = $Lift($i | $j);
		$x = $Lift($i ^ $j);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableInt() {
			AssertCorrect(
@"public void M() {
	int? i = 0, j = 1, x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}",
@"	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
		$x = $IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
		$x = $Lift($i << $j);
		$x = $Lift($i >> $j);
		$x = $Lift($i & $j);
		$x = $Lift($i | $j);
		$x = $Lift($i ^ $j);
	}
	{
		$x = $Check($Lift($i + $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($Lift($i - $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($Lift($i * $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Lift($i << $j);
		$x = $Lift($i >> $j);
		$x = $Lift($i & $j);
		$x = $Lift($i | $j);
		$x = $Lift($i ^ $j);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableUInt() {
			AssertCorrect(
@"public void M() {
	uint? i = 0, j = 1, x;
	int? k = 0;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << k;
		x = i >> k;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << k;
		x = i >> k;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}",
@"	{
		$x = $Clip($Lift($i + $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Clip($Lift($i - $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Clip($Lift($i * $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Clip($IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_UInt32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Clip($IntMod(ct_$InstantiateGenericType({Nullable}, {ga_UInt32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Clip($Lift($i << $k), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Lift($i >>> $k);
		$x = $Clip($Lift($i & $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Clip($Lift($i | $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Clip($Lift($i ^ $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
	}
	{
		$x = $Check($Lift($i + $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Check($Lift($i - $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Check($Lift($i * $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Check($IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_UInt32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Check($IntMod(ct_$InstantiateGenericType({Nullable}, {ga_UInt32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Clip($Lift($i << $k), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Lift($i >>> $k);
		$x = $Clip($Lift($i & $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Clip($Lift($i | $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x = $Clip($Lift($i ^ $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
	}
");
		}

		[Test]
		public void AllNonBitwiseOperatorsWorkForNullableLong() {
			AssertCorrect(
@"public void M() {
	long? i = 0, j = 1, x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	// END
}",
@"	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int64}), $i, $j);
		$x = $IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int64}), $i, $j);
	}
	{
		$x = $Check($Lift($i + $j), ct_$InstantiateGenericType({Nullable}, {ga_Int64}));
		$x = $Check($Lift($i - $j), ct_$InstantiateGenericType({Nullable}, {ga_Int64}));
		$x = $Check($Lift($i * $j), ct_$InstantiateGenericType({Nullable}, {ga_Int64}));
		$x = $Check($IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int64}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int64}));
		$x = $Check($IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int64}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int64}));
	}
");
		}

		[Test]
		public void AllNonBitwiseOperatorsWorkForNullableULong() {
			AssertCorrect(
@"public void M() {
	ulong? i = 0, j = 1, x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	// END
}",
@"	{
		$x = $Clip($Lift($i + $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
		$x = $Clip($Lift($i - $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
		$x = $Clip($Lift($i * $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
		$x = $Clip($IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_UInt64}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
		$x = $Clip($IntMod(ct_$InstantiateGenericType({Nullable}, {ga_UInt64}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
	}
	{
		$x = $Check($Lift($i + $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
		$x = $Check($Lift($i - $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
		$x = $Check($Lift($i * $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
		$x = $Check($IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_UInt64}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
		$x = $Check($IntMod(ct_$InstantiateGenericType({Nullable}, {ga_UInt64}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableChar() {
			AssertCorrect(
@"public void M() {
	char? i = '0', j = '1';
	int? x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
		x = i << j;
		x = i >> j;
		x = i & j;
		x = i | j;
		x = i ^ j;
	}
	// END
}
",
@"	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
		$x = $IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
		$x = $Lift($i << $j);
		$x = $Lift($i >> $j);
		$x = $Lift($i & $j);
		$x = $Lift($i | $j);
		$x = $Lift($i ^ $j);
	}
	{
		$x = $Check($Lift($i + $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($Lift($i - $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($Lift($i * $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Check($IntMod(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Lift($i << $j);
		$x = $Lift($i >> $j);
		$x = $Lift($i & $j);
		$x = $Lift($i | $j);
		$x = $Lift($i ^ $j);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableFloat() {
			AssertCorrect(
@"public void M() {
	float? i = 0, j = 1, x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	// END
}",
@"	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $Lift($i / $j);
		$x = $Lift($i % $j);
	}
	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $Lift($i / $j);
		$x = $Lift($i % $j);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableDouble() {
			AssertCorrect(
@"public void M() {
	double? i = 0, j = 1, x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	// END
}",
@"	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $Lift($i / $j);
		$x = $Lift($i % $j);
	}
	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $Lift($i / $j);
		$x = $Lift($i % $j);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableDecimal() {
			AssertCorrect(
@"public void M() {
	decimal? i = 0, j = 1, x;
	// BEGIN
	unchecked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	checked {
		x = i + j;
		x = i - j;
		x = i * j;
		x = i / j;
		x = i % j;
	}
	// END
}",
@"	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $Lift($i / $j);
		$x = $Lift($i % $j);
	}
	{
		$x = $Lift($i + $j);
		$x = $Lift($i - $j);
		$x = $Lift($i * $j);
		$x = $Lift($i / $j);
		$x = $Lift($i % $j);
	}
");
		}

		[Test]
		public void CastToSmallerTypeAfterOperatorCausesNestedChecksToNotBeEmitted() {
			AssertCorrect(
@"void M() {
	sbyte sb = 0;
	byte b = 0;
	short s = 0;
	ushort us = 0;
	int i = 0;
	uint ui = 0;
	long l = 0;
	ulong ul = 0;
	char c = '0';

	// BEGIN
	sb = (sbyte)(i * 2);
	sb = (sbyte)(ui * 2);
	sb = (sbyte)(l * 2);
	sb = (sbyte)(ul * 2);

	b = (byte)(i * 2);
	b = (byte)(ui * 2);
	b = (byte)(l * 2);
	b = (byte)(ul * 2);

	s = (sbyte)(i * 2);
	s = (sbyte)(ui * 2);
	s = (sbyte)(l * 2);
	s = (sbyte)(ul * 2);

	us = (ushort)(i * 2);
	us = (ushort)(ui * 2);
	us = (ushort)(l * 2);
	us = (ushort)(ul * 2);

	i = (int)(i * 2);
	i = (int)(ui * 2);
	i = (int)(l * 2);
	i = (int)(ul * 2);

	ui = (uint)(i * 2);
	ui = (uint)(ui * 2);
	ui = (uint)(l * 2);
	ui = (uint)(ul * 2);

	l = (long)(i * 2);
	l = (long)(ui * 2);
	l = (long)(l * 2);
	l = (long)(ul * 2);

	ul = (ulong)(i * 2);
	ul = (ulong)(ui * 2);
	ul = (ulong)(l * 2);
	ul = (ulong)(ul * 2);

	c = (char)(i * 2);
	c = (char)(ui * 2);
	c = (char)(l * 2);
	c = (char)(ul * 2);
	// END
}",
@"	$sb = $Narrow($i * 2, {ct_SByte});
	$sb = $Narrow($ui * 2, {ct_SByte});
	$sb = $Narrow($l * 2, {ct_SByte});
	$sb = $Narrow($ul * 2, {ct_SByte});
	$b = $Narrow($i * 2, {ct_Byte});
	$b = $Narrow($ui * 2, {ct_Byte});
	$b = $Narrow($l * 2, {ct_Byte});
	$b = $Narrow($ul * 2, {ct_Byte});
	$s = $Narrow($i * 2, {ct_SByte});
	$s = $Narrow($ui * 2, {ct_SByte});
	$s = $Narrow($l * 2, {ct_SByte});
	$s = $Narrow($ul * 2, {ct_SByte});
	$us = $Narrow($i * 2, {ct_UInt16});
	$us = $Narrow($ui * 2, {ct_UInt16});
	$us = $Narrow($l * 2, {ct_UInt16});
	$us = $Narrow($ul * 2, {ct_UInt16});
	$i = $i * 2;
	$i = $Narrow($ui * 2, {ct_Int32});
	$i = $Narrow($l * 2, {ct_Int32});
	$i = $Narrow($ul * 2, {ct_Int32});
	$ui = $Narrow($i * 2, {ct_UInt32});
	$ui = $Clip($ui * 2, {ct_UInt32});
	$ui = $Narrow($l * 2, {ct_UInt32});
	$ui = $Narrow($ul * 2, {ct_UInt32});
	$l = $i * 2;
	$l = $Clip($ui * 2, {ct_UInt32});
	$l = $l * 2;
	$l = $Narrow($ul * 2, {ct_Int64});
	$ul = $Narrow($i * 2, {ct_UInt64});
	$ul = $Clip($ui * 2, {ct_UInt32});
	$ul = $Narrow($l * 2, {ct_UInt64});
	$ul = $Clip($ul * 2, {ct_UInt64});
	$c = $Narrow($i * 2, {ct_Char});
	$c = $Narrow($ui * 2, {ct_Char});
	$c = $Narrow($l * 2, {ct_Char});
	$c = $Narrow($ul * 2, {ct_Char});
");
		}

		[Test]
		public void CastToSmallerTypeAfterOperatorCausesNestedChecksToNotBeEmittedNullable() {
			AssertCorrect(
@"void M() {
	sbyte? sb = 0;
	byte? b = 0;
	short? s = 0;
	ushort? us = 0;
	int? i = 0;
	uint? ui = 0;
	long? l = 0;
	ulong? ul = 0;
	char? c = '0';

	// BEGIN
	sb = (sbyte?)(i * 2);
	sb = (sbyte?)(ui * 2);
	sb = (sbyte?)(l * 2);
	sb = (sbyte?)(ul * 2);

	b = (byte?)(i * 2);
	b = (byte?)(ui * 2);
	b = (byte?)(l * 2);
	b = (byte?)(ul * 2);

	s = (sbyte?)(i * 2);
	s = (sbyte?)(ui * 2);
	s = (sbyte?)(l * 2);
	s = (sbyte?)(ul * 2);

	us = (ushort?)(i * 2);
	us = (ushort?)(ui * 2);
	us = (ushort?)(l * 2);
	us = (ushort?)(ul * 2);

	i = (int?)(i * 2);
	i = (int?)(ui * 2);
	i = (int?)(l * 2);
	i = (int?)(ul * 2);

	ui = (uint?)(i * 2);
	ui = (uint?)(ui * 2);
	ui = (uint?)(l * 2);
	ui = (uint?)(ul * 2);

	l = (long?)(i * 2);
	l = (long?)(ui * 2);
	l = (long?)(l * 2);
	l = (long?)(ul * 2);

	ul = (ulong?)(i * 2);
	ul = (ulong?)(ui * 2);
	ul = (ulong?)(l * 2);
	ul = (ulong?)(ul * 2);

	c = (char?)(i * 2);
	c = (char?)(ui * 2);
	c = (char?)(l * 2);
	c = (char?)(ul * 2);
	// END
}",
@"	$sb = $Narrow($Lift($i * 2), ct_$InstantiateGenericType({Nullable}, {ga_SByte}));
	$sb = $Narrow($Lift($ui * 2), ct_$InstantiateGenericType({Nullable}, {ga_SByte}));
	$sb = $Narrow($Lift($l * 2), ct_$InstantiateGenericType({Nullable}, {ga_SByte}));
	$sb = $Narrow($Lift($ul * 2), ct_$InstantiateGenericType({Nullable}, {ga_SByte}));
	$b = $Narrow($Lift($i * 2), ct_$InstantiateGenericType({Nullable}, {ga_Byte}));
	$b = $Narrow($Lift($ui * 2), ct_$InstantiateGenericType({Nullable}, {ga_Byte}));
	$b = $Narrow($Lift($l * 2), ct_$InstantiateGenericType({Nullable}, {ga_Byte}));
	$b = $Narrow($Lift($ul * 2), ct_$InstantiateGenericType({Nullable}, {ga_Byte}));
	$s = $Narrow($Lift($i * 2), ct_$InstantiateGenericType({Nullable}, {ga_SByte}));
	$s = $Narrow($Lift($ui * 2), ct_$InstantiateGenericType({Nullable}, {ga_SByte}));
	$s = $Narrow($Lift($l * 2), ct_$InstantiateGenericType({Nullable}, {ga_SByte}));
	$s = $Narrow($Lift($ul * 2), ct_$InstantiateGenericType({Nullable}, {ga_SByte}));
	$us = $Narrow($Lift($i * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt16}));
	$us = $Narrow($Lift($ui * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt16}));
	$us = $Narrow($Lift($l * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt16}));
	$us = $Narrow($Lift($ul * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt16}));
	$i = $Lift($i * 2);
	$i = $Narrow($Lift($ui * 2), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
	$i = $Narrow($Lift($l * 2), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
	$i = $Narrow($Lift($ul * 2), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
	$ui = $Narrow($Lift($i * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
	$ui = $Clip($Lift($ui * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
	$ui = $Narrow($Lift($l * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
	$ui = $Narrow($Lift($ul * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
	$l = $Lift($i * 2);
	$l = $Clip($Lift($ui * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
	$l = $Lift($l * 2);
	$l = $Narrow($Lift($ul * 2), ct_$InstantiateGenericType({Nullable}, {ga_Int64}));
	$ul = $Narrow($Lift($i * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
	$ul = $Clip($Lift($ui * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
	$ul = $Narrow($Lift($l * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
	$ul = $Clip($Lift($ul * 2), ct_$InstantiateGenericType({Nullable}, {ga_UInt64}));
	$c = $Narrow($Lift($i * 2), ct_$InstantiateGenericType({Nullable}, {ga_Char}));
	$c = $Narrow($Lift($ui * 2), ct_$InstantiateGenericType({Nullable}, {ga_Char}));
	$c = $Narrow($Lift($l * 2), ct_$InstantiateGenericType({Nullable}, {ga_Char}));
	$c = $Narrow($Lift($ul * 2), ct_$InstantiateGenericType({Nullable}, {ga_Char}));
");
		}

		[Test]
		public void OperationsOnEnumsActAsOperationsOnTheUnderlyingType() {
			AssertCorrect(
@"enum E1 : uint {}
enum E2 : int {}
void M() {
	E1 e11 = default(E1), e12 = default(E1), x1;
	E2 e21 = default(E2), e22 = default(E2), x2;
	uint y1;
	int y2;

	// BEGIN
	checked {
		x1 = e11 & e12;
		x1 = e11 | e12;
		x1 = e11 ^ e12;
		y1 = e11 - e12;

		x2 = e21 & e22;
		x2 = e21 | e22;
		x2 = e21 ^ e22;
		y2 = e21 - e22;
	}
	// END
}
",
@"	{
		$x1 = $Clip($e11 & $e12, {ct_UInt32});
		$x1 = $Clip($e11 | $e12, {ct_UInt32});
		$x1 = $Clip($e11 ^ $e12, {ct_UInt32});
		$y1 = $Check($e11 - $e12, {ct_UInt32});
		$x2 = $e21 & $e22;
		$x2 = $e21 | $e22;
		$x2 = $e21 ^ $e22;
		$y2 = $Check($e21 - $e22, {ct_Int32});
	}
");
		}

		[Test]
		public void OperationsOnNullableEnumsActAsOperationsOnTheUnderlyingType() {
			AssertCorrect(
@"enum E1 : uint {}
enum E2 : int {}
void M() {
	E1? e11 = default(E1), e12 = default(E1), x1;
	E2? e21 = default(E2), e22 = default(E2), x2;
	uint? y1;
	int? y2;

	// BEGIN
	checked {
		x1 = e11 & e12;
		x1 = e11 | e12;
		x1 = e11 ^ e12;
		y1 = e11 - e12;

		x2 = e21 & e22;
		x2 = e21 | e22;
		x2 = e21 ^ e22;
		y2 = e21 - e22;
	}
	// END
}
",
@"	{
		$x1 = $Clip($Lift($e11 & $e12), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x1 = $Clip($Lift($e11 | $e12), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x1 = $Clip($Lift($e11 ^ $e12), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$y1 = $Check($Lift($e11 - $e12), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x2 = $Lift($e21 & $e22);
		$x2 = $Lift($e21 | $e22);
		$x2 = $Lift($e21 ^ $e22);
		$y2 = $Check($Lift($e21 - $e22), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
	}
");
		}
	}
}
