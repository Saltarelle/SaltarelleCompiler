using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class TypeIsTests : MethodCompilerTestBase {
		[Test]
		public void TypeIsWorksForReferenceTypes() {
			AssertCorrect(
@"class X<T> {
}
void M() {
	object o = null;
	// BEGIN
	bool b = o is X<int>;
	// END
}
",
@"	var $b = $TypeIs($o, $InstantiateGenericType({X}, {Int32}));
");
		}

		[Test]
		public void TypeIsWorksForUnboxingConversions() {
			AssertCorrect(
@"class X<T> {
}
void M() {
	object o = null;
	// BEGIN
	bool b = o is int;
	// END
}
",
@"	var $b = $TypeIs($o, {Int32});
");
		}

		[Test]
		public void TypeIsWorksWithNullableTypes() {
			AssertCorrect(
@"class X<T> {
}
void M() {
	object o = null;
	// BEGIN
	bool b = o is int?;
	// END
}
",
@"	var $b = $TypeIs($o, {Int32});
");
		}
	}
}
