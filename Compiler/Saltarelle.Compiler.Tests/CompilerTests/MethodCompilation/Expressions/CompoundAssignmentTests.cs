using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class CompoundAssignmentTests : MethodCompilerTestBase {
		protected void AssertCorrectForBulkOperators(string csharp, string expected, IMetadataImporter metadataImporter = null, bool addSkeleton = true) {
			// Bulk operators are all except for division and shift right.
			foreach (var op in new[] { "+", "*", "%", "-", "<<", "&", "|", "^" }) {
				AssertCorrect(csharp.Replace("+", op), expected.Replace("+", op), metadataImporter, addSkeleton: addSkeleton);
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
@"	this.set_$P(this.get_$P() + $i);
");
		}

		[Test]
		public void CompoundAssignmentToPropertyWithMethodsOnlyInvokesTheTargetOnce() {
			AssertCorrectForBulkOperators(
@"public class X { public int P { get; set; } }
public X F() { return null; }
public void M() {
	int i = 0;
	// BEGIN
	F().P += i;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.set_$P($tmp1.get_$P() + $i);
");
		}

		[Test]
		public void CompoundAssignmentToPropertyWithMethodsEvaluatesTheArgumentsInCorrectOrder() {
			AssertCorrectForBulkOperators(
@"public class X { public int P { get; set; } }
public X F1() { return null; }
public int F2() { return 0; }
public int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F1().P += (P = F2());
	// END
}",
@"	var $tmp1 = this.$F1();
	var $tmp3 = $tmp1.get_$P();
	var $tmp2 = this.$F2();
	this.set_$P($tmp2);
	$tmp1.set_$P($tmp3 + $tmp2);
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
@"	var $tmp2 = this.get_$P1();
	var $tmp1 = this.get_$P2() + $i;
	this.set_$P2($tmp1);
	this.set_$P1($tmp2 + $tmp1);
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
@"	var $tmp2 = this.get_$P1();
	var $tmp1 = this.get_$P2() + this.$F();
	this.set_$P2($tmp1);
	var $tmp3 = $tmp2 + $tmp1;
	this.set_$P1($tmp3);
	if ($tmp3 < 0) {
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
@"	this.$F += $i;
");
		}

		[Test]
		public void CompoundAssignmentToPropertyWithFieldImplementationDoesNotGenerateTemporary() {
			AssertCorrectForBulkOperators(
@"class X { public int F { get; set; } }
X F() { return null; }
public void M() {
	int i = 0;
	// BEGIN
	F().F += i;
	// END
}",
@"	this.$F().$F += $i;
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
@"	this.$F1 += this.$F2 += $i;
");
		}

		[Test]
		public void CompoundAssignmentToPropertyWithFieldImplementationCorrectlyOrdersExpressions() {
			AssertCorrectForBulkOperators(
@"class X { public int F { get; set; } }
X F() { return null; }
public int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	F().F += (P = i);
	// END
}",
@"	var $tmp1 = this.$F();
	this.set_$P($i);
	$tmp1.$F += $i;
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
@"	{sm_C}.set_$P({sm_C}.get_$P() + $i);
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
@"	{sm_C}.$F += $i;
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
@"	this.set_$Item($i, $j, this.get_$Item($i, $j) + $k);
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
@"	var $tmp1 = this.get_$Item($i, $j) + $k;
	this.set_$Item($i, $j, $tmp1);
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
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	this.set_$Item($tmp1, $tmp2, this.get_$Item($tmp1, $tmp2) + this.$F3());
");
		}

		[Test]
		public void CompoundAssigningToIndexerWorksWhenReorderingArguments() {
			AssertCorrectForBulkOperators(
@"int this[int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7] { get { return 0; } set {} }
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	this[d: F1(), g: F2(), f: F3(), b: F4()] += i;
	// END
}
",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	var $tmp4 = this.$F4();
	this.set_$Item(1, $tmp4, 3, $tmp1, 5, $tmp3, $tmp2, this.get_$Item(1, $tmp4, 3, $tmp1, 5, $tmp3, $tmp2) + $i);
");
		}

		[Test]
		public void CompoundAssigningToIndexerImplementedAsInlineCodeWorks() {
			AssertCorrectForBulkOperators(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int i = 0, j = 1, k = 2;
	// BEGIN
	this[i, j] += k;
	// END
}",
@"	set_(this)._($i)._($j)._(get_(this)._($i)._($j) + $k);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("get_({this})._({x})._({y})"), MethodScriptSemantics.InlineCode("set_({this})._({x})._({y})._({value})")) : PropertyScriptSemantics.Field(p.Name) });
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
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void CompoundAssigningToPropertyWithSetMethodImplementedAsLiteralCodeWorks() {
			AssertCorrectForBulkOperators(
@"int P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P += i;
	// END
}",
@"	set_(this)._(get_(this) + $i);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("get_({this})"), MethodScriptSemantics.InlineCode("set_({this})._({value})")) });
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
@"	{sm_C}.$a += {sm_C}.$b += $i;
");
		}

		[Test]
		public void DivisionCompoundAssignmentWorksForLocalIntegralVariables() {
			AssertCorrect(
@"public void M() {
	byte i = 0;
	byte j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int32}, $i, $j);
");

			AssertCorrect(
@"public void M() {
	sbyte i = 0;
	sbyte j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int32}, $i, $j);
");

			AssertCorrect(
@"public void M() {
	short i = 0;
	byte j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int32}, $i, $j);
");

			AssertCorrect(
@"public void M() {
	ushort i = 0;
	byte j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int32}, $i, $j);
");

			AssertCorrect(
@"public void M() {
	int i = 0;
	byte j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int32}, $i, $j);
");

			AssertCorrect(
@"public void M() {
	uint i = 0;
	byte j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_UInt32}, $i, $j);
");

			AssertCorrect(
@"public void M() {
	long i = 0;
	byte j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int64}, $i, $j);
");

			AssertCorrect(
@"public void M() {
	ulong i = 0;
	byte j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_UInt64}, $i, $j);
");

			AssertCorrect(
@"public void M() {
	char i = '0';
	char j = '1';
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int32}, $i, $j);
");
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
			AssertCorrect(
@"public sbyte P { get; set; }
public void M() {
	sbyte i = 0;
	// BEGIN
	P /= i;
	// END
}",
@"	this.set_$P($IntDiv({ct_Int32}, this.get_$P(), $i));
");

			AssertCorrect(
@"public byte P { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	P /= i;
	// END
}",
@"	this.set_$P($IntDiv({ct_Int32}, this.get_$P(), $i));
");

			AssertCorrect(
@"public short P { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	P /= i;
	// END
}",
@"	this.set_$P($IntDiv({ct_Int32}, this.get_$P(), $i));
");

			AssertCorrect(
@"public ushort P { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	P /= i;
	// END
}",
@"	this.set_$P($IntDiv({ct_Int32}, this.get_$P(), $i));
");

			AssertCorrect(
@"public int P { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	P /= i;
	// END
}",
@"	this.set_$P($IntDiv({ct_Int32}, this.get_$P(), $i));
");

			AssertCorrect(
@"public uint P { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	P /= i;
	// END
}",
@"	this.set_$P($IntDiv({ct_UInt32}, this.get_$P(), $i));
");

			AssertCorrect(
@"public long P { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	P /= i;
	// END
}",
@"	this.set_$P($IntDiv({ct_Int64}, this.get_$P(), $i));
");

			AssertCorrect(
@"public ulong P { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	P /= i;
	// END
}",
@"	this.set_$P($IntDiv({ct_UInt64}, this.get_$P(), $i));
");

			AssertCorrect(
@"public char P { get; set; }
public void M() {
	char i = '0';
	// BEGIN
	P /= i;
	// END
}",
@"	this.set_$P($IntDiv({ct_Int32}, this.get_$P(), $i));
");
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
@"	this.set_$P(this.get_$P() / $i);
"));
		}

		[Test]
		public void DivisionCompoundAssignmentWorksForIntegralPropertiesImplementedAsFields() {
			AssertCorrect(
@"public sbyte F { get; set; }
public void M() {
	sbyte i = 0;
	// BEGIN
	F /= i;
	// END
}",
@"	this.$F = $IntDiv({ct_Int32}, this.$F, $i);
");

			AssertCorrect(
@"public byte F { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	F /= i;
	// END
}",
@"	this.$F = $IntDiv({ct_Int32}, this.$F, $i);
");

			AssertCorrect(
@"public short F { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	F /= i;
	// END
}",
@"	this.$F = $IntDiv({ct_Int32}, this.$F, $i);
");

			AssertCorrect(
@"public ushort F { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	F /= i;
	// END
}",
@"	this.$F = $IntDiv({ct_Int32}, this.$F, $i);
");

			AssertCorrect(
@"public int F { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	F /= i;
	// END
}",
@"	this.$F = $IntDiv({ct_Int32}, this.$F, $i);
");

			AssertCorrect(
@"public uint F { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	F /= i;
	// END
}",
@"	this.$F = $IntDiv({ct_UInt32}, this.$F, $i);
");

			AssertCorrect(
@"public long F { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	F /= i;
	// END
}",
@"	this.$F = $IntDiv({ct_Int64}, this.$F, $i);
");

			AssertCorrect(
@"public ulong F { get; set; }
public void M() {
	byte i = 0;
	// BEGIN
	F /= i;
	// END
}",
@"	this.$F = $IntDiv({ct_UInt64}, this.$F, $i);
");

			AssertCorrect(
@"public char F { get; set; }
public void M() {
	char i = '0';
	// BEGIN
	F /= i;
	// END
}",
@"	this.$F = $IntDiv({ct_Int32}, this.$F, $i);
");
		}

		[Test]
		public void DivisionCompoundAssignmentOnlyInvokesTargetOnceForIntegralPropertiesImplementedAsFields() {
			AssertCorrect(
@"class X { public sbyte F { get; set; } }
X F1() { return null; }
public void M() {
	sbyte i = 0;
	// BEGIN
	F1().F /= i;
	// END
}
",
@"	var $tmp1 = this.$F1();
	$tmp1.$F = $IntDiv({ct_Int32}, $tmp1.$F, $i);
");

			AssertCorrect(
@"class X { public byte F { get; set; } }
X F1() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F1().F /= i;
	// END
}
",
@"	var $tmp1 = this.$F1();
	$tmp1.$F = $IntDiv({ct_Int32}, $tmp1.$F, $i);
");

			AssertCorrect(
@"class X { public short F { get; set; } }
X F1() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F1().F /= i;
	// END
}
",
@"	var $tmp1 = this.$F1();
	$tmp1.$F = $IntDiv({ct_Int32}, $tmp1.$F, $i);
");

			AssertCorrect(
@"class X { public ushort F { get; set; } }
X F1() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F1().F /= i;
	// END
}
",
@"	var $tmp1 = this.$F1();
	$tmp1.$F = $IntDiv({ct_Int32}, $tmp1.$F, $i);
");

			AssertCorrect(
@"class X { public int F { get; set; } }
X F1() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F1().F /= i;
	// END
}
",
@"	var $tmp1 = this.$F1();
	$tmp1.$F = $IntDiv({ct_Int32}, $tmp1.$F, $i);
");

			AssertCorrect(
@"class X { public uint F { get; set; } }
X F1() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F1().F /= i;
	// END
}
",
@"	var $tmp1 = this.$F1();
	$tmp1.$F = $IntDiv({ct_UInt32}, $tmp1.$F, $i);
");

			AssertCorrect(
@"class X { public long F { get; set; } }
X F1() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F1().F /= i;
	// END
}
",
@"	var $tmp1 = this.$F1();
	$tmp1.$F = $IntDiv({ct_Int64}, $tmp1.$F, $i);
");

			AssertCorrect(
@"class X { public ulong F { get; set; } }
X F1() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F1().F /= i;
	// END
}
",
@"	var $tmp1 = this.$F1();
	$tmp1.$F = $IntDiv({ct_UInt64}, $tmp1.$F, $i);
");

			AssertCorrect(
@"class X { public char F { get; set; } }
X F1() { return null; }
public void M() {
	char i = '\0';
	// BEGIN
	F1().F /= i;
	// END
}
",
@"	var $tmp1 = this.$F1();
	$tmp1.$F = $IntDiv({ct_Int32}, $tmp1.$F, $i);
");
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
@"	this.$F /= $i;
"));
		}

		[Test]
		public void DivisionCompoundAssignmentWorksForIntegralPropertiesImplementedAsNativeIndexers() {
			AssertCorrect(
@"public sbyte this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	sbyte j = 0;
	// BEGIN
	this[i] /= j;
	// END
}",
@"	this[$i] = $IntDiv({ct_Int32}, this[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"public byte this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	byte j = 0;
	// BEGIN
	this[i] /= j;
	// END
}",
@"	this[$i] = $IntDiv({ct_Int32}, this[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"public short this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	byte j = 0;
	// BEGIN
	this[i] /= j;
	// END
}",
@"	this[$i] = $IntDiv({ct_Int32}, this[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"public ushort this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	byte j = 0;
	// BEGIN
	this[i] /= j;
	// END
}",
@"	this[$i] = $IntDiv({ct_Int32}, this[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"public int this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	byte j = 0;
	// BEGIN
	this[i] /= j;
	// END
}",
@"	this[$i] = $IntDiv({ct_Int32}, this[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"public uint this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	byte j = 0;
	// BEGIN
	this[i] /= j;
	// END
}",
@"	this[$i] = $IntDiv({ct_UInt32}, this[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"public long this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	byte j = 0;
	// BEGIN
	this[i] /= j;
	// END
}",
@"	this[$i] = $IntDiv({ct_Int64}, this[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"public ulong this[int x] { get { return 0; } set {} }
public void M() {
	int i = 0;
	byte j = 0;
	// BEGIN
	this[i] /= j;
	// END
}",
@"	this[$i] = $IntDiv({ct_UInt64}, this[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"public char this[int x] { get { return '0'; } set {} }
public void M() {
	int i = 0;
	char j = '0';
	// BEGIN
	this[i] /= j;
	// END
}",
@"	this[$i] = $IntDiv({ct_Int32}, this[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
		}

		[Test]
		public void DivisionCompoundAssignmentOnlyInvokesTargetOnceForIntegralPropertiesImplementedAsNativeIndexers() {
			AssertCorrect(
@"class X { public sbyte this[int x] { get { return 0; } set {} } }
X F1() { return null; }
public void M() {
	int i = 0;
	sbyte j = 0;
	// BEGIN
	F1()[i] /= j;
	// END
}
",
@"	var $tmp1 = this.F1();
	$tmp1[$i] = $IntDiv({ct_Int32}, $tmp1[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"class X { public byte this[int x] { get { return 0; } set {} } }
X F1() { return null; }
public void M() {
	int i = 0;
	byte j = 0;
	// BEGIN
	F1()[i] /= j;
	// END
}
",
@"	var $tmp1 = this.F1();
	$tmp1[$i] = $IntDiv({ct_Int32}, $tmp1[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"class X { public short this[int x] { get { return 0; } set {} } }
X F1() { return null; }
public void M() {
	int i = 0;
	byte j = 0;
	// BEGIN
	F1()[i] /= j;
	// END
}
",
@"	var $tmp1 = this.F1();
	$tmp1[$i] = $IntDiv({ct_Int32}, $tmp1[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"class X { public ushort this[int x] { get { return 0; } set {} } }
X F1() { return null; }
public void M() {
	int i = 0;
	byte j = 0;
	// BEGIN
	F1()[i] /= j;
	// END
}
",
@"	var $tmp1 = this.F1();
	$tmp1[$i] = $IntDiv({ct_Int32}, $tmp1[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"class X { public int this[int x] { get { return 0; } set {} } }
X F1() { return null; }
public void M() {
	int i = 0;
	int j = 0;
	// BEGIN
	F1()[i] /= j;
	// END
}
",
@"	var $tmp1 = this.F1();
	$tmp1[$i] = $IntDiv({ct_Int32}, $tmp1[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"class X { public uint this[int x] { get { return 0; } set {} } }
X F1() { return null; }
public void M() {
	int i = 0;
	uint j = 0;
	// BEGIN
	F1()[i] /= j;
	// END
}
",
@"	var $tmp1 = this.F1();
	$tmp1[$i] = $IntDiv({ct_UInt32}, $tmp1[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"class X { public long this[int x] { get { return 0; } set {} } }
X F1() { return null; }
public void M() {
	int i = 0;
	long j = 0;
	// BEGIN
	F1()[i] /= j;
	// END
}
",
@"	var $tmp1 = this.F1();
	$tmp1[$i] = $IntDiv({ct_Int64}, $tmp1[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"class X { public ulong this[int x] { get { return 0; } set {} } }
X F1() { return null; }
public void M() {
	int i = 0;
	ulong j = 0;
	// BEGIN
	F1()[i] /= j;
	// END
}
",
@"	var $tmp1 = this.F1();
	$tmp1[$i] = $IntDiv({ct_UInt64}, $tmp1[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });

			AssertCorrect(
@"class X { public char this[int x] { get { return '0'; } set {} } }
X F1() { return null; }
public void M() {
	int i = 0;
	char j = '0';
	// BEGIN
	F1()[i] /= j;
	// END
}
",
@"	var $tmp1 = this.F1();
	$tmp1[$i] = $IntDiv({ct_Int32}, $tmp1[$i], $j);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
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
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) }));
		}

		[Test]
		public void DivisionCompoundAssignmentToInstanceFieldOnlyInvokesTheTargetOnceForIntegralFields() {
			AssertCorrect(
@"class X { public sbyte a; }
X F() { return null; }
public void M() {
	sbyte i = 0;
	// BEGIN
	F().a /= i;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.$a = $IntDiv({ct_Int32}, $tmp1.$a, $i);
");

			AssertCorrect(
@"class X { public byte a; }
X F() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F().a /= i;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.$a = $IntDiv({ct_Int32}, $tmp1.$a, $i);
");

			AssertCorrect(
@"class X { public short a; }
X F() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F().a /= i;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.$a = $IntDiv({ct_Int32}, $tmp1.$a, $i);
");

			AssertCorrect(
@"class X { public ushort a; }
X F() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F().a /= i;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.$a = $IntDiv({ct_Int32}, $tmp1.$a, $i);
");

			AssertCorrect(
@"class X { public int a; }
X F() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F().a /= i;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.$a = $IntDiv({ct_Int32}, $tmp1.$a, $i);
");

			AssertCorrect(
@"class X { public uint a; }
X F() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F().a /= i;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.$a = $IntDiv({ct_UInt32}, $tmp1.$a, $i);
");

			AssertCorrect(
@"class X { public long a; }
X F() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F().a /= i;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.$a = $IntDiv({ct_Int64}, $tmp1.$a, $i);
");

			AssertCorrect(
@"class X { public ulong a; }
X F() { return null; }
public void M() {
	byte i = 0;
	// BEGIN
	F().a /= i;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.$a = $IntDiv({ct_UInt64}, $tmp1.$a, $i);
");

			AssertCorrect(
@"class X { public char a; }
X F() { return null; }
public void M() {
	char i = '0';
	// BEGIN
	F().a /= i;
	// END
}",
@"	var $tmp1 = this.$F();
	$tmp1.$a = $IntDiv({ct_Int32}, $tmp1.$a, $i);
");
		}

		[Test]
		public void DivisionCompoundAssignmentToLocalIntegralVariableWorks() {
				AssertCorrect(
@"public void M() {
	sbyte i = 0;
	sbyte j = 0;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int32}, $i, $j);
");

				AssertCorrect(
@"public void M() {
	byte i = 0;
	byte j = 0;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int32}, $i, $j);
");

				AssertCorrect(
@"public void M() {
	short i = 0;
	byte j = 0;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int32}, $i, $j);
");

				AssertCorrect(
@"public void M() {
	ushort i = 0;
	byte j = 0;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int32}, $i, $j);
");

				AssertCorrect(
@"public void M() {
	int i = 0;
	byte j = 0;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int32}, $i, $j);
");

				AssertCorrect(
@"public void M() {
	uint i = 0;
	byte j = 0;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_UInt32}, $i, $j);
");

				AssertCorrect(
@"public void M() {
	long i = 0;
	byte j = 0;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int64}, $i, $j);
");

				AssertCorrect(
@"public void M() {
	ulong i = 0;
	byte j = 0;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_UInt64}, $i, $j);
");

				AssertCorrect(
@"public void M() {
	char i = '0';
	char j = '0';
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv({ct_Int32}, $i, $j);
");
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
			foreach (var type in new[] { "byte", "ushort", "uint" }) {
				AssertCorrect(
@"public void M() {
	type i = 0;
	int j = 0;
	// BEGIN
	i >>= j;
	// END
}".Replace("type", type),
@"	$i >>>= $j;
");
			}
		}

		[Test]
		public void ShiftRightCompoundAssignmentForUnsignedTypesVariableWorksWhenResultIsNormalAssignment() {
			foreach (var type in new[] { "byte", "ushort", "uint" }) {
				AssertCorrect(
@"public type P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P >>= i;
	// END
}".Replace("type", type),
@"	this.set_$P(this.get_$P() >>> $i);
");
			}
		}

		[Test]
		public void ShiftRightCompoundAssignmentForSignedTypesVariableWorksWhenResultIsCompoundAssignment() {
			foreach (var type in new[] { "sbyte", "short", "int" }) {
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
			foreach (var type in new[] { "sbyte", "short", "int" }) {
				AssertCorrect(
@"public type P { get; set; }
public void M() {
	int i = 0;
	// BEGIN
	P >>= i;
	// END
}".Replace("type", type),
@"	this.set_$P(this.get_$P() >> $i);
");
			}
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
			AssertCorrect(
@"public void M() {
	byte? i = 0;
	byte? j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
");

			AssertCorrect(
@"public void M() {
	sbyte? i = 0;
	sbyte? j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
");

			AssertCorrect(
@"public void M() {
	short? i = 0;
	byte? j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
");

			AssertCorrect(
@"public void M() {
	ushort? i = 0;
	byte? j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
");

			AssertCorrect(
@"public void M() {
	int? i = 0;
	byte? j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
");

			AssertCorrect(
@"public void M() {
	uint? i = 0;
	byte? j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_UInt32}), $i, $j);
");

			AssertCorrect(
@"public void M() {
	long? i = 0;
	byte? j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int64}), $i, $j);
");

			AssertCorrect(
@"public void M() {
	ulong? i = 0;
	byte? j = 1;
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_UInt64}), $i, $j);
");

			AssertCorrect(
@"public void M() {
	char? i = '0';
	char? j = '1';
	// BEGIN
	i /= j;
	// END
}",
@"	$i = $IntDiv(ct_$InstantiateGenericType({Nullable}, {ga_Int32}), $i, $j);
");
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
			foreach (var type in new[] { "sbyte", "short", "int" }) {
				AssertCorrect(
@"public void M() {
	type? i = 0;
	int? j = 0;
	// BEGIN
	i >>= j;
	// END
}".Replace("type", type),
@"	$i = $Lift($i >> $j);
");
			}
		}

		[Test]
		public void LiftedUnsignedRightShiftWorks() {
			foreach (var type in new[] { "byte", "ushort", "uint" }) {
				AssertCorrect(
@"public void M() {
	type? i = 0;
	int? j = 0;
	// BEGIN
	i >>= j;
	// END
}".Replace("type", type),
@"	$i = $Lift($i >>> $j);
");
			}
		}

		[Test]
		public void FieldConstantInLiftedOperation() {
			AssertCorrect(@"
public void M() {
	bool b;
	double? d = 0;
	// BEGIN
	d += double.PositiveInfinity;
	d -= double.PositiveInfinity;
	d *= double.PositiveInfinity;
	d /= double.PositiveInfinity;
	// END
}",
@"	$d = $Lift($d + {sm_Double}.$PosInf);
	$d = $Lift($d - {sm_Double}.$PosInf);
	$d = $Lift($d * {sm_Double}.$PosInf);
	$d = $Lift($d / {sm_Double}.$PosInf);
", metadataImporter: new MockMetadataImporter { GetFieldSemantics = f => FieldScriptSemantics.Field("$PosInf") });
		}

		[Test]
		public void CanCompoundAssignToArrayElement() {
			AssertCorrect(
@"public void M() {
	int[] arr = null;
	int i = 0;
	// BEGIN
	arr[0] = i;
	// END
}",
@"	$arr[0] = $i;
");
		}

		[Test]
		public void CompoundAssignToArrayElementEvaluatesArgumentsInTheCorrectOrder() {
			AssertCorrect(
@"int[] F1() { return null; }
int F2() { return 0; }
int F3() { return 0; }
int P { get; set; }
public void M() {
	int i;
	// BEGIN
	F1()[F2()] += (P = F3());
	// END
}",
@"	var $tmp2 = this.$F1();
	var $tmp3 = this.$F2();
	var $tmp1 = this.$F3();
	this.set_$P($tmp1);
	$tmp2[$tmp3] += $tmp1;
");
		}

		[Test]
		public void LiftedCompoundAssignToArrayElementOnlyEvaluatesArgumentsOnce() {
			AssertCorrect(
@"int?[] F1() { return null; }
int F2() { return 0; }
int F3() { return 0; }
public void M() {
	// BEGIN
	F1()[F2()] += F3();
	// END
}",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	$tmp1[$tmp2] = $Lift($tmp1[$tmp2] + this.$F3());
");
		}

		[Test]
		public void CompoundAssigningToMultiDimensionalArrayWorks() {
			AssertCorrectForBulkOperators(
@"public void M() {
	int[,] arr = null;
	int i = 0, j = 1, k = 2;
	// BEGIN
	arr[i, j] += k;
	// END
}",
@"	$MultidimArraySet($arr, $i, $j, $MultidimArrayGet($arr, $i, $j) + $k);
");
		}

		[Test]
		public void CompoundAssigningToMultiDimensionalArrayWorksWhenUsingTheReturnValue() {
			AssertCorrectForBulkOperators(
@"public void M() {
	int[,] arr = null;
	int i = 0, j = 1, k = 2, l;
	// BEGIN
	l = arr[i, j] += k;
	// END
}",
@"	var $tmp1 = $MultidimArrayGet($arr, $i, $j) + $k;
	$MultidimArraySet($arr, $i, $j, $tmp1);
	$l = $tmp1;
");
		}

		[Test]
		public void CompoundAssigningToMultiDimensionalArrayOnlyInvokesIndexingArgumentsOnceAndInTheCorrectOrder() {
			AssertCorrectForBulkOperators(
@"public int[,] A() { return null; }
public int F1() { return 0; }
public int F2() { return 0; }
public int F3() { return 0; }
public void M() {
	int i = 0;
	// BEGIN
	A()[F1(), F2()] += F3();
	// END
}",
@"	var $tmp1 = this.$A();
	var $tmp2 = this.$F1();
	var $tmp3 = this.$F2();
	$MultidimArraySet($tmp1, $tmp2, $tmp3, $MultidimArrayGet($tmp1, $tmp2, $tmp3) + this.$F3());
");
		}

		[Test]
		public void CompoundAssigningToByRefLocalWorks() {
			AssertCorrectForBulkOperators(
@"int[] arr;
int i;
int F() { return 0; }
public void M(ref int i) {
	// BEGIN
	i += 1;
	// END
}",
@"	$i.$ += 1;
");
		}

		[Test]
		public void NonLiftedBooleanAndWorksForLocalVariables() {
			foreach (var type in new[] { "bool", "int?", "int" }) {
				AssertCorrect(
@"public void M() {
	type a = default(type), b = default(type);
	// BEGIN
	a &= b;
	// END
}".Replace("type", type),
type.EndsWith("?")
? @"	$a = $Lift($a & $b);
"
: @"	$a &= $b;
");
			}
		}

		[Test]
		public void NonLiftedBooleanOrWorksForLocalVariables() {
			foreach (var type in new[] { "bool", "int?", "int" }) {
				AssertCorrect(
@"public void M() {
	type a = default(type), b = default(type);
	// BEGIN
	a |= b;
	// END
}".Replace("type", type),
type.EndsWith("?")
? @"	$a = $Lift($a | $b);
"
: @"	$a |= $b;
");
			}
		}

		[Test]
		public void LiftedBooleanAndWorksForLocalVariables() {
			AssertCorrect(
@"public void M() {
	bool? a = false, b = false;
	// BEGIN
	a &= b;
	// END
}",
@"	$a = $LiftedBooleanAnd($a, $b);
");
		}

		[Test]
		public void LiftedBooleanOrWorksForLocalVariables() {
			AssertCorrect(
@"public void M() {
	bool? a = false, b = false;
	// BEGIN
	a |= b;
	// END
}",
@"	$a = $LiftedBooleanOr($a, $b);
");
		}

		[Test]
		public void NonLiftedBooleanAndWorksForMethodProperties() {
			foreach (var type in new[] { "bool", "int?", "int" }) {
				AssertCorrect(
@"type P { get; set; }
public void M() {
	type a = default(type);
	// BEGIN
	P &= a;
	// END
}".Replace("type", type),
type.EndsWith("?")
? @"	this.set_$P($Lift(this.get_$P() & $a));
"
: @"	this.set_$P(this.get_$P() & $a);
");
			}
		}

		[Test]
		public void NonLiftedBooleanOrWorksForMethodProperties() {
			foreach (var type in new[] { "bool", "int?", "int" }) {
				AssertCorrect(
@"type P { get; set; }
public void M() {
	type a = default(type);
	// BEGIN
	P |= a;
	// END
}".Replace("type", type),
type.EndsWith("?")
? @"	this.set_$P($Lift(this.get_$P() | $a));
"
: @"	this.set_$P(this.get_$P() | $a);
");
			}
		}

		[Test]
		public void LiftedBooleanAndWorksForMethodProperties() {
			AssertCorrect(
@"bool? P { get; set; }
public void M() {
	bool a = false;
	// BEGIN
	P &= a;
	// END
}",
@"	this.set_$P($LiftedBooleanAnd(this.get_$P(), $a));
");
		}

		[Test]
		public void LiftedBooleanOrWorksForMethodProperties() {
			AssertCorrect(
@"bool? P { get; set; }
public void M() {
	bool a = false;
	// BEGIN
	P |= a;
	// END
}",
@"	this.set_$P($LiftedBooleanOr(this.get_$P(), $a));
");
		}

		[Test]
		public void UsingPropertyThatIsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableProperty { get; set; } public void M() { UnusableProperty += 0; } }" }, metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.FormattedMessage.Contains("Class.UnusableProperty")));
		}

		[Test]
		public void CompoundAddAssignForDelegateTypeInvokesDelegateCombine() {
			AssertCorrect(
@"bool? P { get; set; }
public void M() {
	Action a = null, b = null;
	// BEGIN
	a += b;
	// END
}",
@"	$a = {sm_Delegate}.$Combine($a, $b);
");
		}

		[Test]
		public void CompoundSubtractAssignForDelegateTypeInvokesDelegateRemove() {
			AssertCorrect(
@"bool? P { get; set; }
public void M() {
	Action a = null, b = null;
	// BEGIN
	a -= b;
	// END
}",
@"	$a = {sm_Delegate}.$Remove($a, $b);
");
		}

		[Test]
		public void NonVirtualCompoundAssignToBasePropertyWorks() {
			AssertCorrectForBulkOperators(
@"class B {
	public virtual int P { get; set; }
}
class D : B {
	public override int P { get; set; }
	public void M() {
		// BEGIN
		base.P += 10;
		// END
	}
}",
@"	$CallBase({bind_B}, '$set_P', [], [this, $CallBase({bind_B}, '$get_P', [], [this]) + 10]);
", addSkeleton: false);
		}

		[Test]
		public void CompoundAssignmentToDynamicMemberWorks() {
			AssertCorrectForBulkOperators(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d.someField += 123;
	// END
}",
@"	$d.someField += 123;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d.someField /= 123;
	// END
}",
@"	$d.someField /= 123;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d.someField >>= 123;
	// END
}",
@"	$d.someField >>= 123;
");
		}

		[Test]
		public void CompoundAssignmentToDynamicObjectWorks() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d += 123;
	// END
}",
@"	$d += 123;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d /= 123;
	// END
}",
@"	$d /= 123;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d >>= 123;
	// END
}",
@"	$d >>= 123;
");
		}

		[Test]
		public void CompoundAssignmentToDynamicIndexingWorks() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d[""X""] += 123;
	// END
}",
@"	$d['X'] += 123;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d[""X""] /= 123;
	// END
}",
@"	$d['X'] /= 123;
");

			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	d[""X""] >>= 123;
	// END
}",
@"	$d['X'] >>= 123;
");
		}

		[Test]
		public void CompoundAssignmentToIndexerWithDynamicArgumentWorks() {
			AssertCorrect(
@"public int this[int a] { get { return 0; } set {} }
public void M() {
	dynamic d = null;
	// BEGIN
	this[d] += 123;
	// END
}",
@"	this.set_$Item($d, this.get_$Item($d) + 123);
");
		}

		[Test]
		public void CompoundAssignmentToIndexerWithTwoDynamicArgumentsWorks() {
			AssertCorrect(
@"public int this[int a] { get { return 0; } set {} }
public int this[int a, string b] { get { return 0; } set {} }
public void M() {
	dynamic d1 = null, d2 = null;
	// BEGIN
	this[d1, d2] += 123;
	// END
}",
@"	this.set_$Item($d1, $d2, this.get_$Item($d1, $d2) + 123);
");
		}

		[Test]
		public void CompoundAssignmentToIndexerWithDynamicArgumentWorksWhenTwoMethodsWithTheSameNameAreApplicable() {
			AssertCorrect(
@"public int this[int a, string b] { get { return 0; } set {} }
public int this[string a, string b] { get { return 0; } set {} }
public void M() {
	dynamic d1 = null, d2 = null;
	// BEGIN
	this[d1, d2] += 123;
	// END
}",
@"	this.set_$Item($d1, $d2, this.get_$Item($d1, $d2) + 123);
");
		}

		[Test]
		public void CompoundAssignmentToIndexerWithDynamicArgumentWorksWhenTwoNativeIndexersAreApplicable() {
			AssertCorrect(
@"public int this[int a] { get { return 0; } set {} }
public int this[string b] { get { return 0; } set {} }
public void M() {
	dynamic d1 = null;
	// BEGIN
	this[d1] += 123;
	// END
}",
@"	this[$d1] += 123;
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NativeIndexer(), MethodScriptSemantics.NativeIndexer()) });
		}

		[Test]
		public void CompoundAssignmentToIndexerWithDynamicArgumentGivesTheCorrectErrorWhenMethodsWithDifferentImplementationAreApplicable() {
			var er = new MockErrorReporter();
			Compile(new[] {
@"class C {
	public int this[int a] { get { return 0; } set {} }
	public int this[string b] { get { return 0; } set {} }
	public void M() {
		dynamic d1 = null;
		// BEGIN
		this[d1] += 123;
		// END
	}
}" }, errorReporter: er, metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.Parameters.Length == 1 && p.Parameters[0].Type.SpecialType == SpecialType.System_String ? PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NativeIndexer(), MethodScriptSemantics.NativeIndexer()) : PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("$" + p.GetMethod.Name), MethodScriptSemantics.NormalMethod("$" + p.SetMethod.Name)) });

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7532));
		}

		[Test]
		public void CompoundAssignmentToIndexerGivesTheCorrectErrorWhenMethodsWithDifferentImplementationAreApplicable() {
			var er = new MockErrorReporter();
			Compile(new[] {
@"class C {
	public int this[int a] { get { return 0; } set {} }
	public int this[string b] { get { return 0; } set {} }
	public void M() {
		dynamic d1 = null;
		// BEGIN
		this[d1] += 123;
		// END
	}
}" }, errorReporter: er, metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.Parameters.Length == 1 && p.Parameters[0].Type.SpecialType == SpecialType.System_String ? PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NativeIndexer(), MethodScriptSemantics.NativeIndexer()) : PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("$" + p.GetMethod.Name), MethodScriptSemantics.NormalMethod("$" + p.SetMethod.Name)) });

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7532));
		}

		[Test]
		public void CompoundAssignmentToIndexerWithDynamicArgumentCannotUseNamedArguments() {
			var er = new MockErrorReporter();
			Compile(new[] {
@"class C {
	public int this[int a, string b] { get { return 0; } set {} }
	public int this[string a, string b] { get { return 0; } set {} }
	public void M() {
		dynamic d1 = null, d2 = null;
		// BEGIN
		this[a: d1, b: d2] += 123;
		// END
	}
}" }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7526));
		}

		[Test]
		public void CompoundAssignmentToDynamicPropertyOfNonDynamicObject() {
			AssertCorrectForBulkOperators(@"
public class SomeClass {
	public dynamic Value { get; set; }
}

class C {
	public void M() {
		var c = new SomeClass();
		// BEGIN
		c.Value += 1;
		// END
	}
}",
@"	$c.set_$Value($c.get_$Value() + 1);
", addSkeleton: false);
		}

		[Test]
		public void CompoundAssignmentToDynamicFieldOfNonDynamicObject() {
			AssertCorrectForBulkOperators(@"
public class SomeClass {
	public dynamic Value;
}

class C {
	public void M() {
		var c = new SomeClass();
		// BEGIN
		c.Value += 1;
		// END
	}
}",
@"	$c.$Value += 1;
", addSkeleton: false);
		}

		[Test]
		public void BitwiseOperationOnLongAndULongIsAnError() {
			foreach (var oper in new[] { "<<", ">>", "|", "&" }) {
				var er = new MockErrorReporter(false);
				Compile(new[] { "class C { public void M() { long v = 0; v OPER= 1; } }".Replace("OPER", oper) }, errorReporter: er);
				Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.Code == 7540));
			}

			foreach (var oper in new[] { "<<", ">>", "|", "&" }) {
				var er = new MockErrorReporter(false);
				Compile(new[] { "class C { public void M() { ulong v = 0; v OPER= 1; } }".Replace("OPER", oper) }, errorReporter: er);
				Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.Code == 7540));
			}
		}

		[Test]
		public void BitwiseOperationOnNullableLongAndULongIsAnError() {
			foreach (var oper in new[] { "<<", ">>", "|", "&" }) {
				var er = new MockErrorReporter(false);
				Compile(new[] { "class C { public void M() { long? v = 0; v OPER= 1; } }".Replace("OPER", oper) }, errorReporter: er);
				Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.Code == 7540));
			}

			foreach (var oper in new[] { "<<", ">>", "|", "&" }) {
				var er = new MockErrorReporter(false);
				Compile(new[] { "class C { public void M() { ulong? v = 0; v OPER= 1; } }".Replace("OPER", oper) }, errorReporter: er);
				Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.Code == 7540));
			}
		}
	}
}
