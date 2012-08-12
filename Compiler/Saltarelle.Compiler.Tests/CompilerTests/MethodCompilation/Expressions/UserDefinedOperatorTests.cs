using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class UserDefinedOperatorTests : MethodCompilerTestBase {
		[Test]
		public void UserDefinedBinaryOperatorsWork() {
			AssertCorrect(@"
class C1 {
	public static C1 operator+(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator-(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator*(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator/(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator%(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator&(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator|(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator^(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator<<(C1 c1, int i) { return default(C1); }
	public static C1 operator>>(C1 c1, int i) { return default(C1); }
}
void M() {
	C1 x = null, y = null, z;
	int i = 0;
	// BEGIN
	z = x +  y;
	z = x -  y;
	z = x *  y;
	z = x /  y;
	z = x %  y;
	z = x &  y;
	z = x |  y;
	z = x ^  y;
	z = x << i;
	z = x >> i;
	// END
}",
@"	$z = {C1}.$op_Addition($x, $y);
	$z = {C1}.$op_Subtraction($x, $y);
	$z = {C1}.$op_Multiply($x, $y);
	$z = {C1}.$op_Division($x, $y);
	$z = {C1}.$op_Modulus($x, $y);
	$z = {C1}.$op_BitwiseAnd($x, $y);
	$z = {C1}.$op_BitwiseOr($x, $y);
	$z = {C1}.$op_ExclusiveOr($x, $y);
	$z = {C1}.$op_LeftShift($x, $i);
	$z = {C1}.$op_RightShift($x, $i);
");
		}

		[Test]
		public void LiftedUserDefinedBinaryOperatorsWorks() {
			AssertCorrect(@"
struct C1 {
	public static C1 operator+(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator-(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator*(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator/(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator%(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator&(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator|(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator^(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator<<(C1 c1, int i) { return default(C1); }
	public static C1 operator>>(C1 c1, int i) { return default(C1); }
}
void M() {
	C1? x = null, y = null, z;
	int? i = null;
	// BEGIN
	z = x +  y;
	z = x -  y;
	z = x *  y;
	z = x /  y;
	z = x %  y;
	z = x &  y;
	z = x |  y;
	z = x ^  y;
	z = x << i;
	z = x >> i;
	// END
}",
@"	$z = $Lift({C1}.$op_Addition($x, $y));
	$z = $Lift({C1}.$op_Subtraction($x, $y));
	$z = $Lift({C1}.$op_Multiply($x, $y));
	$z = $Lift({C1}.$op_Division($x, $y));
	$z = $Lift({C1}.$op_Modulus($x, $y));
	$z = $Lift({C1}.$op_BitwiseAnd($x, $y));
	$z = $Lift({C1}.$op_BitwiseOr($x, $y));
	$z = $Lift({C1}.$op_ExclusiveOr($x, $y));
	$z = $Lift({C1}.$op_LeftShift($x, $i));
	$z = $Lift({C1}.$op_RightShift($x, $i));
");
		}

		[Test]
		public void CompoundAssignmentWithUserDefinedBinaryOperatorsWork() {
			AssertCorrect(@"
class C1 {
	public static C1 operator+(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator-(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator*(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator/(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator%(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator&(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator|(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator^(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator<<(C1 c1, int i) { return default(C1); }
	public static C1 operator>>(C1 c1, int i) { return default(C1); }
}
void M() {
	C1 x = null, y = null;
	int i = 0;
	// BEGIN
	x +=  y;
	x -=  y;
	x *=  y;
	x /=  y;
	x %=  y;
	x &=  y;
	x |=  y;
	x ^=  y;
	x <<= i;
	x >>= i;
	// END
}",
@"	$x = {C1}.$op_Addition($x, $y);
	$x = {C1}.$op_Subtraction($x, $y);
	$x = {C1}.$op_Multiply($x, $y);
	$x = {C1}.$op_Division($x, $y);
	$x = {C1}.$op_Modulus($x, $y);
	$x = {C1}.$op_BitwiseAnd($x, $y);
	$x = {C1}.$op_BitwiseOr($x, $y);
	$x = {C1}.$op_ExclusiveOr($x, $y);
	$x = {C1}.$op_LeftShift($x, $i);
	$x = {C1}.$op_RightShift($x, $i);
");
		}

		[Test]
		public void CompoundAssignmentWithLiftedUserDefinedBinaryOperatorsWork() {
			AssertCorrect(@"
struct C1 {
	public static C1 operator+(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator-(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator*(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator/(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator%(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator&(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator|(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator^(C1 c1, C1 c2) { return default(C1); }
	public static C1 operator<<(C1 c1, int i) { return default(C1); }
	public static C1 operator>>(C1 c1, int i) { return default(C1); }
}
void M() {
	C1? x = null, y = null;
	int i = 0;
	// BEGIN
	x +=  y;
	x -=  y;
	x *=  y;
	x /=  y;
	x %=  y;
	x &=  y;
	x |=  y;
	x ^=  y;
	x <<= i;
	x >>= i;
	// END
}",
@"	$x = $Lift({C1}.$op_Addition($x, $y));
	$x = $Lift({C1}.$op_Subtraction($x, $y));
	$x = $Lift({C1}.$op_Multiply($x, $y));
	$x = $Lift({C1}.$op_Division($x, $y));
	$x = $Lift({C1}.$op_Modulus($x, $y));
	$x = $Lift({C1}.$op_BitwiseAnd($x, $y));
	$x = $Lift({C1}.$op_BitwiseOr($x, $y));
	$x = $Lift({C1}.$op_ExclusiveOr($x, $y));
	$x = $Lift({C1}.$op_LeftShift($x, $i));
	$x = $Lift({C1}.$op_RightShift($x, $i));
");
		}

		[Test]
		public void UnaryNonAssigningOperatorsWork() {
			AssertCorrect(@"
class C1 {
	public static C1 operator+(C1 c) { return default(C1); }
	public static C1 operator-(C1 c) { return default(C1); }
	public static C1 operator!(C1 c) { return default(C1); }
	public static C1 operator~(C1 c) { return default(C1); }
}
void M() {
	C1 x = null, y;
	// BEGIN
	y = +x;
	y = -x;
	y = !x;
	y = ~x;
	// END
}",
@"	$y = {C1}.$op_UnaryPlus($x);
	$y = {C1}.$op_UnaryNegation($x);
	$y = {C1}.$op_LogicalNot($x);
	$y = {C1}.$op_OnesComplement($x);
");
		}

		[Test]
		public void LiftedUnaryNonAssigningOperatorsWork() {
			AssertCorrect(@"
struct C1 {
	public static C1 operator+(C1 c) { return default(C1); }
	public static C1 operator-(C1 c) { return default(C1); }
	public static C1 operator!(C1 c) { return default(C1); }
	public static C1 operator~(C1 c) { return default(C1); }
}
void M() {
	C1? x = null, y;
	// BEGIN
	y = +x;
	y = -x;
	y = !x;
	y = ~x;
	// END
}",
@"	$y = $Lift({C1}.$op_UnaryPlus($x));
	$y = $Lift({C1}.$op_UnaryNegation($x));
	$y = $Lift({C1}.$op_LogicalNot($x));
	$y = $Lift({C1}.$op_OnesComplement($x));
");
		}

		[Test, Ignore("Not yet supported")]
		public void OperatorsTrueAndFalseWork() {
			AssertCorrect(@"
class C1 {
	public static C1 operator &(C1 c1, C1 c2) { return null; }
	public static C1 operator |(C1 c1, C1 c2) { return null; }
	public static bool operator false(C1 c) { return false; }
	public static bool operator true(C1 c) { return false; }
}
void M() {
	C1 c1 = null, c2 = null, c3;
	// BEGIN
	c3 = c1 && c2;
	c3 = c1 || c2;
	// END
	}
}",
@"	// TODO: Not supported
");
		}

		[Test]
		public void IncrementAndDecrementOperatorsWorkOnVariablesWhenNotAssigningTheResult() {
			AssertCorrect(@"
class C1 {
	public static C1 operator++(C1 c) { return null; }
	public static C1 operator--(C1 c) { return null; }
}
void M() {
	C1 c = null;
	// BEGIN
	c++;
	++c;
	c--;
	--c;
	// END
}",
@"	$c = {C1}.$op_Increment($c);
	$c = {C1}.$op_Increment($c);
	$c = {C1}.$op_Decrement($c);
	$c = {C1}.$op_Decrement($c);
");
		}

		[Test]
		public void PreIncrementAndDecrementOperatorsWorkOnVariablesWhenAssigningTheResult() {
			AssertCorrect(@"
class C1 {
	public static C1 operator++(C1 c) { return null; }
	public static C1 operator--(C1 c) { return null; }
}
void M() {
	C1 c = null;
	// BEGIN
	C1 c1 = ++c;
	C1 c2 = --c;
	// END
}",
@"	var $c1 = $c = {C1}.$op_Increment($c);
	var $c2 = $c = {C1}.$op_Decrement($c);
");
		}

		[Test]
		public void PostIncrementAndDecrementOperatorsWorkOnVariablesWhenAssigningTheResult() {
			AssertCorrect(@"
class C1 {
	public static C1 operator++(C1 c) { return null; }
	public static C1 operator--(C1 c) { return null; }
}
void M() {
	C1 c = null;
	// BEGIN
	C1 c1 = c++;
	C1 c2 = c--;
	// END
}",
@"	var $tmp1 = $c;
	$c = {C1}.$op_Increment($tmp1);
	var $c1 = $tmp1;
	var $tmp2 = $c;
	$c = {C1}.$op_Decrement($tmp2);
	var $c2 = $tmp2;
");
		}

		[Test]
		public void IncrementAndDecrementOperatorsWorkOnPropertiesWhenNotAssigningTheResult() {
			AssertCorrect(@"
class C1 {
	public static C1 operator++(C1 c) { return null; }
	public static C1 operator--(C1 c) { return null; }
}
C1 P { get; set; }
void M() {
	// BEGIN
	P++;
	++P;
	P--;
	--P;
	// END
}",
@"	this.set_$P({C1}.$op_Increment(this.get_$P()));
	this.set_$P({C1}.$op_Increment(this.get_$P()));
	this.set_$P({C1}.$op_Decrement(this.get_$P()));
	this.set_$P({C1}.$op_Decrement(this.get_$P()));
");
		}

		[Test]
		public void PreIncrementAndDecrementOperatorsWorkOnPropertiesWhenAssigningTheResult() {
			AssertCorrect(@"
class C1 {
	public static C1 operator++(C1 c) { return null; }
	public static C1 operator--(C1 c) { return null; }
}
C1 P { get; set; }
void M() {
	// BEGIN
	C1 c1 = ++P;
	C1 c2 = --P;
	// END
}",
@"	var $tmp1 = {C1}.$op_Increment(this.get_$P());
	this.set_$P($tmp1);
	var $c1 = $tmp1;
	var $tmp2 = {C1}.$op_Decrement(this.get_$P());
	this.set_$P($tmp2);
	var $c2 = $tmp2;
");
		}

		[Test]
		public void PostIncrementAndDecrementOperatorsWorkOnPropertiesWhenAssigningTheResult() {
			AssertCorrect(@"
class C1 {
	public static C1 operator++(C1 c) { return null; }
	public static C1 operator--(C1 c) { return null; }
}
C1 P { get; set; }
void M() {
	// BEGIN
	C1 c1 = P++;
	C1 c2 = P--;
	// END
}",
@"	var $tmp1 = this.get_$P();
	this.set_$P({C1}.$op_Increment($tmp1));
	var $c1 = $tmp1;
	var $tmp2 = this.get_$P();
	this.set_$P({C1}.$op_Decrement($tmp2));
	var $c2 = $tmp2;
");
		}

		[Test]
		public void LiftedIncrementAndDecrementOperatorsWorkOnVariablesWhenNotAssigningTheResult() {
			AssertCorrect(@"
struct C1 {
	public static C1 operator++(C1 c) { return default(C1); }
	public static C1 operator--(C1 c) { return default(C1); }
}
void M() {
	C1? c = null;
	// BEGIN
	c++;
	++c;
	c--;
	--c;
	// END
}",
@"	$c = $Lift({C1}.$op_Increment($c));
	$c = $Lift({C1}.$op_Increment($c));
	$c = $Lift({C1}.$op_Decrement($c));
	$c = $Lift({C1}.$op_Decrement($c));
");
		}

		[Test]
		public void LiftedPreIncrementAndDecrementOperatorsWorkOnVariablesWhenAssigningTheResult() {
			AssertCorrect(@"
struct C1 {
	public static C1 operator++(C1 c) { return default(C1); }
	public static C1 operator--(C1 c) { return default(C1); }
}
void M() {
	C1? c = null;
	// BEGIN
	C1? c1 = ++c;
	C1? c2 = --c;
	// END
}",
@"	var $c1 = $c = $Lift({C1}.$op_Increment($c));
	var $c2 = $c = $Lift({C1}.$op_Decrement($c));
");
		}

		[Test]
		public void LiftedPostIncrementAndDecrementOperatorsWorkOnVariablesWhenAssigningTheResult() {
			AssertCorrect(@"
struct C1 {
	public static C1 operator++(C1 c) { return default(C1); }
	public static C1 operator--(C1 c) { return default(C1); }
}
void M() {
	C1? c = null;
	// BEGIN
	C1? c1 = c++;
	C1? c2 = c--;
	// END
}",
@"	var $tmp1 = $c;
	$c = $Lift({C1}.$op_Increment($tmp1));
	var $c1 = $tmp1;
	var $tmp2 = $c;
	$c = $Lift({C1}.$op_Decrement($tmp2));
	var $c2 = $tmp2;
");
		}

		[Test]
		public void LiftedIncrementAndDecrementOperatorsWorkOnPropertiesWhenNotAssigningTheResult() {
			AssertCorrect(@"
struct C1 {
	public static C1 operator++(C1 c) { return default(C1); }
	public static C1 operator--(C1 c) { return default(C1); }
}
C1? P { get; set; }
void M() {
	// BEGIN
	P++;
	++P;
	P--;
	--P;
	// END
}",
@"	this.set_$P($Lift({C1}.$op_Increment(this.get_$P())));
	this.set_$P($Lift({C1}.$op_Increment(this.get_$P())));
	this.set_$P($Lift({C1}.$op_Decrement(this.get_$P())));
	this.set_$P($Lift({C1}.$op_Decrement(this.get_$P())));
");
		}

		[Test]
		public void LiftedPreIncrementAndDecrementOperatorsWorkOnPropertiesWhenAssigningTheResult() {
			AssertCorrect(@"
struct C1 {
	public static C1 operator++(C1 c) { return default(C1); }
	public static C1 operator--(C1 c) { return default(C1); }
}
C1? P { get; set; }
void M() {
	// BEGIN
	C1? c1 = ++P;
	C1? c2 = --P;
	// END
}",
@"	var $tmp1 = $Lift({C1}.$op_Increment(this.get_$P()));
	this.set_$P($tmp1);
	var $c1 = $tmp1;
	var $tmp2 = $Lift({C1}.$op_Decrement(this.get_$P()));
	this.set_$P($tmp2);
	var $c2 = $tmp2;
");
		}

		[Test]
		public void LiftedPostIncrementAndDecrementOperatorsWorkOnPropertiesWhenAssigningTheResult() {
			AssertCorrect(@"
struct C1 {
	public static C1 operator++(C1 c) { return default(C1); }
	public static C1 operator--(C1 c) { return default(C1); }
}
C1? P { get; set; }
void M() {
	// BEGIN
	C1? c1 = P++;
	C1? c2 = P--;
	// END
}",
@"	var $tmp1 = this.get_$P();
	this.set_$P($Lift({C1}.$op_Increment($tmp1)));
	var $c1 = $tmp1;
	var $tmp2 = this.get_$P();
	this.set_$P($Lift({C1}.$op_Decrement($tmp2)));
	var $c2 = $tmp2;
");
		}

		[Test]
		public void UserDefinedOperatorImplementedAsNativeOperatorIsNotInvoked() {
			AssertCorrect(@"
class C1 {
	public static C1 operator+(C1 c1, C1 c2) {
		return null;
	}
}
void M() {
	C1 c1 = null, c2 = null;
	// BEGIN
	var c3 = c1 + c2;
	// END
}",
@"	var $c3 = $c1 + $c2;
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.IsOperator ? MethodScriptSemantics.NativeOperator() : MethodScriptSemantics.NormalMethod(m.Name) });
		}
	}
}


