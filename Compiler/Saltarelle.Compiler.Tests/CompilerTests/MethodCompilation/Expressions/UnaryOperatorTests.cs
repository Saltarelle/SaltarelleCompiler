using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class UnaryOperatorTests : MethodCompilerTestBase {
		[Test]
		public void NonLiftedUnaryPlusWorks() {
			AssertCorrect(
@"public void M() {
	int a = 0;
	// BEGIN
	var b = +a;
	// END
}",
@"	var $b = +$a;
");
		}

		[Test]
		public void NonLiftedUnaryMinusWorks() {
			AssertCorrect(
@"public void M() {
	int a = 0;
	// BEGIN
	var b = -a;
	// END
}",
@"	var $b = -$a;
");
		}

		[Test]
		public void NonLiftedLogicalNotWorks() {
			AssertCorrect(
@"public void M() {
	bool a = false;
	// BEGIN
	var b = !a;
	// END
}",
@"	var $b = !$a;
");
		}

		[Test]
		public void NonLiftedBitwiseNotWorks() {
			AssertCorrect(
@"public void M() {
	int a = 0;
	// BEGIN
	var b = ~a;
	// END
}",
@"	var $b = ~$a;
");
		}

		[Test]
		public void LiftedUnaryPlusWorks() {
			AssertCorrect(
@"public void M() {
	int? a = 0;
	// BEGIN
	var b = +a;
	// END
}",
@"	var $b = $Lift(+$a);
");
		}

		[Test]
		public void LiftedUnaryMinusWorks() {
			AssertCorrect(
@"public void M() {
	int? a = 0;
	// BEGIN
	var b = -a;
	// END
}",
@"	var $b = $Lift(-$a);
");
		}

		[Test]
		public void LiftedLogicalNotWorks() {
			AssertCorrect(
@"public void M() {
	bool? a = false;
	// BEGIN
	var b = !a;
	// END
}",
@"	var $b = $Lift(!$a);
");
		}

		[Test]
		public void LiftedBitwiseNotWorks() {
			AssertCorrect(
@"public void M() {
	int? a = 0;
	// BEGIN
	var b = ~a;
	// END
}",
@"	var $b = $Lift(~$a);
");
		}

		[Test]
		public void UnaryOperatorsWorkForDynamicMembers() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = +d.someMember;
	// END
}",
@"	var $i = +$d.someMember;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = -d.someMember;
	// END
}",
@"	var $i = -$d.someMember;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = !d.someMember;
	// END
}",
@"	var $i = !$d.someMember;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = ~d.someMember;
	// END
}",
@"	var $i = ~$d.someMember;
");
		}

		[Test]
		public void UnaryOperatorsWorkForDynamicObjects() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = +d;
	// END
}",
@"	var $i = +$d;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = -d;
	// END
}",
@"	var $i = -$d;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = !d;
	// END
}",
@"	var $i = !$d;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	var i = ~d;
	// END
}",
@"	var $i = ~$d;
");
		}

		[Test]
		public void BitwiseOperationOnLongAndULongIsAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class C { public void M() { long v = 0; var v2 = ~v; } }" }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.Code == 7540));

			er = new MockErrorReporter(false);
			Compile(new[] { "class C { public void M() { ulong v = 0; var v2 = ~v; } }" }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.Code == 7540));
		}

		[Test]
		public void BitwiseOperationOnNullableLongAndULongIsAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class C { public void M() { long? v = 0; var v2 = ~v; } }" }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.Code == 7540));

			er = new MockErrorReporter(false);
			Compile(new[] { "class C { public void M() { ulong? v = 0; var v2 = ~v; } }" }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.Code == 7540));
		}

		[Test]
		public void AllOperatorsWorkForSByte() {
			AssertCorrect(
@"public void M() {
	sbyte i = 0;
	int x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = +$i;
		$x = -$i;
		$x = ~$i;
	}
	{
		$x = +$i;
		$x = -$i;
		$x = ~$i;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForByte() {
			AssertCorrect(
@"public void M() {
	byte i = 0;
	int x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = +$i;
		$x = -$i;
		$x = ~$i;
	}
	{
		$x = +$i;
		$x = -$i;
		$x = ~$i;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForShort() {
			AssertCorrect(
@"public void M() {
	short i = 0;
	int x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = +$i;
		$x = -$i;
		$x = ~$i;
	}
	{
		$x = +$i;
		$x = -$i;
		$x = ~$i;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForUShort() {
			AssertCorrect(
@"public void M() {
	ushort i = 0;
	int x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = +$i;
		$x = -$i;
		$x = ~$i;
	}
	{
		$x = +$i;
		$x = -$i;
		$x = ~$i;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForInt() {
			AssertCorrect(
@"public void M() {
	int i = 0, x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = +$i;
		$x = -$i;
		$x = ~$i;
	}
	{
		$x = +$i;
		$x = $Check(-$i, {ct_Int32});
		$x = ~$i;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForUInt() {
			AssertCorrect(
@"public void M() {
	uint i = 0;
	long x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = +$i;
		$x = -$i;
		$x = $Clip(~$i, {ct_UInt32});
	}
	{
		$x = +$i;
		$x = -$i;
		$x = $Clip(~$i, {ct_UInt32});
	}
");
		}

		[Test]
		public void AllNonBitwiseOperatorsWorkForLong() {
			AssertCorrect(
@"public void M() {
	long i = 0, x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
	}
	checked {
		x = +i;
		x = -i;
	}
	// END
}",
@"	{
		$x = +$i;
		$x = -$i;
	}
	{
		$x = +$i;
		$x = -$i;
	}
");
		}

		[Test]
		public void AllNonBitwiseOperatorsWorkForULong() {
			AssertCorrect(
@"public void M() {
	ulong i = 0, x;
	// BEGIN
	unchecked {
		x = +i;
	}
	checked {
		x = +i;
	}
	// END
}",
@"	{
		$x = +$i;
	}
	{
		$x = +$i;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForChar() {
			AssertCorrect(
@"public void M() {
	char i = '0';
	int x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = +$i;
		$x = -$i;
		$x = ~$i;
	}
	{
		$x = +$i;
		$x = -$i;
		$x = ~$i;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForFloat() {
			AssertCorrect(
@"public void M() {
	float i = 0, x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
	}
	checked {
		x = +i;
		x = -i;
	}
	// END
}",
@"	{
		$x = +$i;
		$x = -$i;
	}
	{
		$x = +$i;
		$x = -$i;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForDouble() {
			AssertCorrect(
@"public void M() {
	double i = 0, x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
	}
	checked {
		x = +i;
		x = -i;
	}
	// END
}",
@"	{
		$x = +$i;
		$x = -$i;
	}
	{
		$x = +$i;
		$x = -$i;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForDecimal() {
			AssertCorrect(
@"public void M() {
	decimal i = 0, x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
	}
	checked {
		x = +i;
		x = -i;
	}
	// END
}",
@"	{
		$x = +$i;
		$x = -$i;
	}
	{
		$x = +$i;
		$x = -$i;
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableSByte() {
			AssertCorrect(
@"public void M() {
	sbyte? i = 0;
	int? x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Lift(~$i);
	}
	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Lift(~$i);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableByte() {
			AssertCorrect(
@"public void M() {
	byte? i = 0;
	int? x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Lift(~$i);
	}
	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Lift(~$i);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableShort() {
			AssertCorrect(
@"public void M() {
	short? i = 0;
	int? x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Lift(~$i);
	}
	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Lift(~$i);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableUShort() {
			AssertCorrect(
@"public void M() {
	ushort? i = 0;
	int? x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Lift(~$i);
	}
	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Lift(~$i);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableInt() {
			AssertCorrect(
@"public void M() {
	int? i = 0, x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Lift(~$i);
	}
	{
		$x = $Lift(+$i);
		$x = $Check($Lift(-$i), ct_$InstantiateGenericType({Nullable}, {ga_Int32}));
		$x = $Lift(~$i);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableUInt() {
			AssertCorrect(
@"public void M() {
	uint? i = 0;
	long? x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Clip($Lift(~$i), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
	}
	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Clip($Lift(~$i), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
	}
");
		}

		[Test]
		public void AllNonBitwiseOperatorsWorkForNullableLong() {
			AssertCorrect(
@"public void M() {
	long? i = 0, x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
	}
	checked {
		x = +i;
		x = -i;
	}
	// END
}",
@"	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
	}
	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
	}
");
		}

		[Test]
		public void AllNonBitwiseOperatorsWorkForNullableULong() {
			AssertCorrect(
@"public void M() {
	ulong? i = 0, x;
	// BEGIN
	unchecked {
		x = +i;
	}
	checked {
		x = +i;
	}
	// END
}",
@"	{
		$x = $Lift(+$i);
	}
	{
		$x = $Lift(+$i);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableChar() {
			AssertCorrect(
@"public void M() {
	char? i = '0';
	int? x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
		x = ~i;
	}
	checked {
		x = +i;
		x = -i;
		x = ~i;
	}
	// END
}",
@"	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Lift(~$i);
	}
	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
		$x = $Lift(~$i);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableFloat() {
			AssertCorrect(
@"public void M() {
	float? i = 0, x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
	}
	checked {
		x = +i;
		x = -i;
	}
	// END
}",
@"	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
	}
	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableDouble() {
			AssertCorrect(
@"public void M() {
	double? i = 0, x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
	}
	checked {
		x = +i;
		x = -i;
	}
	// END
}",
@"	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
	}
	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
	}
");
		}

		[Test]
		public void AllOperatorsWorkForNullableDecimal() {
			AssertCorrect(
@"public void M() {
	decimal? i = 0, x;
	// BEGIN
	unchecked {
		x = +i;
		x = -i;
	}
	checked {
		x = +i;
		x = -i;
	}
	// END
}",
@"	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
	}
	{
		$x = $Lift(+$i);
		$x = $Lift(-$i);
	}
");
		}

		[Test]
		public void OperationsOnEnumsActAsOperationsOnTheUnderlyingType() {
			AssertCorrect(
@"enum E1 : uint {}
enum E2 : int {}
void M() {
	E1 e1 = default(E1), x1;
	E2 e2 = default(E2), x2;

	// BEGIN
	checked {
		x1 = ~e1;
		x2 = ~e2;
	}
	// END
}",
@"	{
		$x1 = $Clip(~$e1, {ct_UInt32});
		$x2 = ~$e2;
	}
");
		}

		[Test]
		public void OperationsOnNullableEnumsActAsOperationsOnTheUnderlyingType() {
			AssertCorrect(
@"enum E1 : uint {}
enum E2 : int {}
void M() {
	E1? e1 = default(E1), x1;
	E2? e2 = default(E2), x2;

	// BEGIN
	checked {
		x1 = ~e1;
		x2 = ~e2;
	}
	// END
}",
@"	{
		$x1 = $Clip($Lift(~$e1), ct_$InstantiateGenericType({Nullable}, {ga_UInt32}));
		$x2 = $Lift(~$e2);
	}
");
		}
	}
}
