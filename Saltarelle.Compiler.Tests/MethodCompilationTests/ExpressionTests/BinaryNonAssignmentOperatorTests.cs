using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class BinaryNonAssignmentOperatorTests : MethodCompilerTestBase {
		protected void AssertCorrectForBulkOperators(string csharp, string expected, bool includeEqualsAndNotEquals, INamingConventionResolver namingConvention = null) {
			// Bulk operators are all except for division, shift right, coalesce and the logical operators.
			foreach (var op in new[] { "*", "%", "+", "-", "<<", "<", ">", "<=", ">=", "&", "^", "|" }) {
				var jsOp = (op == "==" || op == "!=" ? op + "=" : op);	// Script should use strict equals (===) rather than normal equals (==)
				AssertCorrect(csharp.Replace("+", op), expected.Replace("+", jsOp), namingConvention);
			}

			if (includeEqualsAndNotEquals) {
				AssertCorrect(csharp.Replace("+", "=="), expected.Replace("+", "==="), namingConvention);
				AssertCorrect(csharp.Replace("+", "!="), expected.Replace("+", "!=="), namingConvention);
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
		public void LogicalOperatorsWork() {
			AssertCorrect(
@"public void M() {
	bool a = false, b = false;
	// BEGIN
	var c = a && b;
	// END
}",
@"	var $c = $a && $b;
");

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
		public void ArgumentsAreEvaluatedInTheCorrectOrder() {
			AssertCorrectForBulkOperators(
@"public int P { get; set; }
public void M() {
	int a = 0;
	// BEGIN
	var c = P + (P = a);
	// END
}",
@"	var $tmp1 = this.get_P();
	this.set_P($a);
	var $c = $tmp1 + $a;
", includeEqualsAndNotEquals: true);
		}

		[Test]
		public void LiftedBulkOperatorsExceptForEqualsAndNotEqualsWork() {
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
		public void LiftedEqualsAndNotEqualsAreTheSameAsNonLiftedVersions() {
			AssertCorrect(
@"public void M() {
	int? a, b;
	// BEGIN
	var c = a == b;
	// END
}",
@"	var $c = $a === $b;
");

			AssertCorrect(
@"public void M() {
	int? a, b;
	// BEGIN
	var c = a != b;
	// END
}",
@"	var $c = $a !== $b;
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
		public void CoalesceWorks() {
			AssertCorrect(
@"public void M() {
	int? a = 0, b = 0;
	// BEGIN
	var c = a ?? b;
	// END
}",
@"	var $c = $Coalesce($a, $b);
");
		}
	}
}
