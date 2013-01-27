using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class SizeOfTests : MethodCompilerTestBase {
		[Test]
		public void SizeOfIntegralTypesWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	int x1 = sizeof(sbyte);
	int x2 = sizeof(byte);
	int x3 = sizeof(short);
	int x4 = sizeof(ushort);
	int x5 = sizeof(int);
	int x6 = sizeof(uint);
	int x7 = sizeof(long);
	int x8 = sizeof(ulong);
	int x9 = sizeof(char);
	int xa = sizeof(float);
	int xb = sizeof(double);
	int xc = sizeof(bool);
	// END
}
",
@"	var $x1 = 1;
	var $x2 = 1;
	var $x3 = 2;
	var $x4 = 2;
	var $x5 = 4;
	var $x6 = 4;
	var $x7 = 8;
	var $x8 = 8;
	var $x9 = 2;
	var $xa = 4;
	var $xb = 8;
	var $xc = 1;
");
		}

		[Test]
		public void SizeOfEnumWorks() {
			AssertCorrect(@"
enum Enm1 {}
enum Enm2 : short {}
public void M() {
	// BEGIN
	int x1 = sizeof(Enm1);
	int x2 = sizeof(Enm2);
	// END
}",
@"	var $x1 = 4;
	var $x2 = 2;
");
		}
	}
}
