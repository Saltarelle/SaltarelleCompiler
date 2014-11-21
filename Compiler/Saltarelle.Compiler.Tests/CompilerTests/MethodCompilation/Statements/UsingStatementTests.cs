using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class UsingStatementTests : MethodCompilerTestBase {
		[Test]
		public void UsingStatementWithSingleVariableDeclarationWithSimpleInitializerWorksIDisposable() {
			AssertCorrect(
@"public void M() {
	IDisposable a = null;
	// BEGIN
	using (IDisposable d = a) {
		int x = 0;
	}
	// END
}",
@"	// @(4, 2) - (4, 27)
	var $d = $a;
	try {
		// @(5, 3) - (5, 13)
		var $x = 0;
	}
	finally {
		// @(6, 2) - (6, 3)
		if ($ReferenceNotEquals($d, null)) {
			$d.$Dispose();
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void UsingStatementWithSingleVariableDeclarationWithSimpleInitializerWorksClass() {
			AssertCorrect(
@"class C2 : IDisposable { public void Dispose() {} }
public void M() {
	C2 a = null;
	// BEGIN
	using (C2 d = a) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 18)
	var $d = $a;
	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	finally {
		// @(7, 2) - (7, 3)
		if ($ReferenceNotEquals($d, null)) {
			$d.$Dispose();
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void UsingStatementWithSingleVariableDeclarationWithSimpleInitializerWorksInterface() {
			AssertCorrect(
@"interface I2 : IDisposable {}
public void M() {
	I2 a = null;
	// BEGIN
	using (I2 d = a) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 18)
	var $d = $a;
	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	finally {
		// @(7, 2) - (7, 3)
		if ($ReferenceNotEquals($d, null)) {
			$Upcast($d, {ct_IDisposable}).$Dispose();
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void UsingStatementWithSingleVariableDeclarationWithSimpleInitializerWorksStruct() {
			AssertCorrect(
@"struct S : System.IDisposable { public void Dispose() {} }
public void M() {
	S a = default(S);
	// BEGIN
	using (var d = a) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 19)
	var $d = $Clone($a, {to_S});
	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	finally {
		// @(7, 2) - (7, 3)
		$Clone($d, {to_S}).$Dispose();
	}
", mutableValueTypes: true, addSourceLocations: true);
		}

		[Test]
		public void UsingStatementWithSingleVariableDeclarationWithSimpleInitializerWorksNullableStruct() {
			AssertCorrect(
@"struct S : System.IDisposable { public void Dispose() {} }
public void M() {
	S? a = null;
	// BEGIN
	using (var d = a) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 19)
	var $d = $Clone($a, {to_S});
	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	finally {
		// @(7, 2) - (7, 3)
		if ($ReferenceNotEquals($d, null)) {
			$Clone($d, {to_S}).$Dispose();
		}
	}
", mutableValueTypes: true, addSourceLocations: true);
		}

		[Test]
		public void UsingStatementWithSingleVariableDeclarationWorks() {
			AssertCorrect(
@"IDisposable MyProperty { get; set; }
public void M() {
	IDisposable a = null;
	// BEGIN
	using (IDisposable d = (MyProperty = a)) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 42)
	this.set_$MyProperty($a);
	var $d = $a;
	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	finally {
		// @(7, 2) - (7, 3)
		if ($ReferenceNotEquals($d, null)) {
			$d.$Dispose();
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void UsingStatementWithoutVariableDeclarationWorks() {
			AssertCorrect(
@"class S : IDisposable { public void Dispose() {} }
S MyProperty { get; set; }
public void M() {
	S a = null;
	// BEGIN
	using (MyProperty = a) {
		int x = 0;
	}
	// END
}",
@"	// @(6, 2) - (6, 24)
	this.set_$MyProperty($a);
	var $tmp1 = $a;
	try {
		// @(7, 3) - (7, 13)
		var $x = 0;
	}
	finally {
		// @(8, 2) - (8, 3)
		if ($ReferenceNotEquals($tmp1, null)) {
			$tmp1.$Dispose();
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void UsingStatementWithoutVariableDeclarationWorksInterface() {
			AssertCorrect(
@"interface I : IDisposable {}
I MyProperty { get; set; }
public void M() {
	I a = null;
	// BEGIN
	using (MyProperty = a) {
		int x = 0;
	}
	// END
}",
@"	// @(6, 2) - (6, 24)
	this.set_$MyProperty($a);
	var $tmp1 = $a;
	try {
		// @(7, 3) - (7, 13)
		var $x = 0;
	}
	finally {
		// @(8, 2) - (8, 3)
		if ($ReferenceNotEquals($tmp1, null)) {
			$Upcast($tmp1, {ct_IDisposable}).$Dispose();
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void UsingStatementWithoutVariableDeclarationWorksStruct() {
			AssertCorrect(
@"struct S : System.IDisposable { public void Dispose() {} }
public void M() {
	S a = default(S), d;
	// BEGIN
	using (d = a) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 15)
	$d = $Clone($a, {to_S});
	var $tmp1 = $Clone($d, {to_S});
	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	finally {
		// @(7, 2) - (7, 3)
		$Clone($tmp1, {to_S}).$Dispose();
	}
", mutableValueTypes: true, addSourceLocations: true);
		}

		[Test]
		public void UsingStatementWithoutVariableDeclarationWorksNullableStruct() {
			AssertCorrect(
@"struct S : System.IDisposable { public void Dispose() {} }
public void M() {
	S? a = null, d;
	// BEGIN
	using (d = a) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 15)
	$d = $Clone($a, {to_S});
	var $tmp1 = $Clone($d, {to_S});
	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	finally {
		// @(7, 2) - (7, 3)
		if ($ReferenceNotEquals($tmp1, null)) {
			$Clone($tmp1, {to_S}).$Dispose();
		}
	}
", mutableValueTypes: true, addSourceLocations: true);
		}

		[Test]
		public void UsingStatementWithMultipleVariableDeclarationsWork() {
			AssertCorrect(
@"IDisposable P1 { get; set; }
IDisposable P2 { get; set; }
IDisposable P3 { get; set; }
public void M() {
	IDisposable a = null, b = null, c = null;
	// BEGIN
	using (IDisposable d1 = (P1 = a), d2 = (P2 = b), d3 = (P3 = c)) {
		int x = 0;
	}
	// END
}",
@"	// @(7, 21) - (7, 34)
	this.set_$P1($a);
	var $d1 = $a;
	try {
		// @(7, 36) - (7, 49)
		this.set_$P2($b);
		var $d2 = $b;
		try {
			// @(7, 51) - (7, 64)
			this.set_$P3($c);
			var $d3 = $c;
			try {
				// @(8, 3) - (8, 13)
				var $x = 0;
			}
			finally {
				// @(9, 2) - (9, 3)
				if ($ReferenceNotEquals($d3, null)) {
					$d3.$Dispose();
				}
			}
		}
		finally {
			// @(9, 2) - (9, 3)
			if ($ReferenceNotEquals($d2, null)) {
				$d2.$Dispose();
			}
		}
	}
	finally {
		// @(9, 2) - (9, 3)
		if ($ReferenceNotEquals($d1, null)) {
			$d1.$Dispose();
		}
	}
", addSourceLocations: true);
		}

		[Test]
		public void UsingStatementWithDynamicResourceWorks() {
			AssertCorrect(
@"IDisposable MyProperty { get; set; }
public void M() {
	IDisposable a = null;
	// BEGIN
	using (dynamic d = (MyProperty = a)) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 38)
	this.set_$MyProperty($a);
	var $d = $a;
	var $tmp1 = $Cast($d, {ct_IDisposable});
	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	finally {
		// @(7, 2) - (7, 3)
		$tmp1.$Dispose();
	}
", addSourceLocations: true);
		}


		[Test]
		public void InlineCodeDisposeMethod() {
			AssertCorrect(
@"class MyDisposable : IDisposable { public void Dispose() {} }
public void M() {
	MyDisposable a = null;
	// BEGIN
	using (MyDisposable d = a) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 28)
	var $d = $a;
	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	finally {
		// @(7, 2) - (7, 3)
		if ($ReferenceNotEquals($d, null)) {
			dispose_it($d);
			additional;
		}
	}
", new MockMetadataImporter { GetMethodSemantics = m => m.ContainingType.Name == "MyDisposable" && m.Name == "Dispose" ? MethodScriptSemantics.InlineCode("dispose_it({this}); additional;") : MethodScriptSemantics.NormalMethod("$" + m.Name) }, addSourceLocations: true);
		}

		[Test]
		public void InlineCodeDisposeMethodStruct() {
			AssertCorrect(
@"class MyDisposable : IDisposable { public void Dispose() {} }
public void M() {
	MyDisposable a = null;
	// BEGIN
	using (MyDisposable d = a) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 28)
	var $d = $a;
	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	finally {
		// @(7, 2) - (7, 3)
		if ($ReferenceNotEquals($d, null)) {
			dispose_it($d);
			additional;
		}
	}
", new MockMetadataImporter { GetMethodSemantics = m => m.ContainingType.Name == "MyDisposable" && m.Name == "Dispose" ? MethodScriptSemantics.InlineCode("dispose_it({this}); additional;") : MethodScriptSemantics.NormalMethod("$" + m.Name) }, addSourceLocations: true);
		}

		[Test]
		public void UsingStatementWithDeclaredVariablePerformsConversionInTheBeginning() {
			AssertCorrect(
@"class MyDisposable : IDisposable { public void Dispose() {} }
public void M() {
	MyDisposable a = null;
	// BEGIN
	using (IDisposable d = a) {
		int x = 0;
	}
	// END
}",
@"	// @(5, 2) - (5, 27)
	var $d = $Upcast($a, {ct_IDisposable});
	try {
		// @(6, 3) - (6, 13)
		var $x = 0;
	}
	finally {
		// @(7, 2) - (7, 3)
		if ($ReferenceNotEquals($d, null)) {
			$d.$Dispose();
		}
	}
", new MockMetadataImporter { GetMethodSemantics = m => m.ContainingType.Name == "MyDisposable" && m.Name == "Dispose" ? MethodScriptSemantics.InlineCode("dispose_it({this});") : MethodScriptSemantics.NormalMethod("$" + m.Name) }, addSourceLocations: true);
		}
	}
}
