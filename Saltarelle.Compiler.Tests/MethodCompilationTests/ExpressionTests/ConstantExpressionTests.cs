using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class ConstantExpressionTests : MethodCompilerTestBase {
		[Test]
		public void LiteralTrueWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = true;
	// END
}",
@"	var $b = true;
");
		}

		[Test]
		public void LiteralFalseWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = false;
	// END
}",
@"	var $b = false;
");
		}

		[Test]
		public void LiteralByteWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = (byte)123;
	// END
}",
@"	var $b = 123;
");
		}

		[Test]
		public void LiteralSByteWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = (sbyte)123;
	// END
}",
@"	var $b = 123;
");
		}

		[Test]
		public void LiteralShortWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = (short)123;
	// END
}",
@"	var $b = 123;
");
		}

		[Test]
		public void LiteralUshortWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = (short)123;
	// END
}",
@"	var $b = 123;
");
		}

		[Test]
		public void LiteralIntWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = (int)123;
	// END
}",
@"	var $b = 123;
");
		}

		[Test]
		public void LiteralUIntWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = (uint)123;
	// END
}",
@"	var $b = 123;
");
		}

		[Test]
		public void LiteralUIntWithSuffixWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = 123U;
	// END
}",
@"	var $b = 123;
");
		}

		[Test]
		public void LiteralLongWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = (long)123;
	// END
}",
@"	var $b = 123;
");
		}

		[Test]
		public void LiteralLongWithSuffixWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = 123L;
	// END
}",
@"	var $b = 123;
");
		}

		[Test]
		public void LiteralULongWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = (ulong)123;
	// END
}",
@"	var $b = 123;
");
		}

		[Test]
		public void LiteralULongWithSuffixWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = 123UL;
	// END
}",
@"	var $b = 123;
");
		}

		[Test]
		public void LiteralFloatWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = (float)123.5;
	// END
}",
@"	var $b = 123.5;
");
		}

		[Test]
		public void LiteralFloatWithSuffixWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = 123.5F;
	// END
}",
@"	var $b = 123.5;
");
		}

		[Test]
		public void LiteralDoubleWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = 123.5;
	// END
}",
@"	var $b = 123.5;
");
		}

		[Test]
		public void LiteralDecimalWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = (decimal)123.5;
	// END
}",
@"	var $b = 123.5;
");
		}

		[Test]
		public void LiteralDecimalWithSuffixWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = 123.5M;
	// END
}",
@"	var $b = 123.5;
");
		}

		[Test]
		public void CharLiteralWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = 'A';
	// END
}",
@"	var $b = 65;
");
		}

		[Test]
		public void CharLiteralWorks2() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = '\xff34';
	// END
}",
@"	var $b = 65332;
");
		}

		[Test]
		public void LiteralStringWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = ""ABCD\nefgh"";
	// END
}",
@"	var $b = 'ABCD\nefgh';
");
		}

		[Test]
		public void NullLiteralWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	object o = null;
	// END
}",
@"	var $o = null;
");
		}

		[Test]
		public void DefaultWorksForReferenceType() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = default(object);
	// END
}",
@"	var $b = null;
");
		}

		[Test]
		public void DefaultWorksForNumericType() {
			DoForAllIntegerTypes(type =>
				AssertCorrect(
@"public void M() {
	// BEGIN
	var b = default(type);
	// END
}".Replace("type", type),
@"	var $b = 0;
"));

			DoForAllFloatingPointTypes(type =>
				AssertCorrect(
@"public void M() {
	// BEGIN
	var b = default(type);
	// END
}".Replace("type", type),
@"	var $b = 0;
"));
		}

		[Test]
		public void DefaultWorksForChar() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = default(char);
	// END
}",
@"	var $b = 0;
");
		}

		[Test]
		public void DefaultBoolWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var b = default(bool);
	// END
}",
@"	var $b = false;
");
		}

		[Test]
		public void DefaultWorksForTypeParameterWithoutConstraints() {
			AssertCorrect(
@"public void M<T>() {
	// BEGIN
	var b = default(T);
	// END
}",
@"	var $b = $Default($T);
");
		}

		[Test]
		public void DefaultWorksForTypeParameterConstrainedToReferenceType() {
			AssertCorrect(
@"public void M<T>() where T : class{
	// BEGIN
	var b = default(T);
	// END
}",
@"	var $b = null;
");
		}

		[Test]
		public void DefaultWorksForTypeParameterConstrainedToValueType() {
			AssertCorrect(
@"public void M<T>() where T : struct {
	// BEGIN
	var b = default(T);
	// END
}",
@"	var $b = $Default($T);
");
		}
	}
}
