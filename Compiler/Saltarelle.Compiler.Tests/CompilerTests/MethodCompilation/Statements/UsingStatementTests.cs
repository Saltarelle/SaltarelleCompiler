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
@"	{
		var $d = $a;
		try {
			var $x = 0;
		}
		finally {
			if ($ReferenceNotEquals($d, null)) {
				$d.$Dispose();
			}
		}
	}
");
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
@"	{
		var $d = $a;
		try {
			var $x = 0;
		}
		finally {
			if ($ReferenceNotEquals($d, null)) {
				$d.$Dispose();
			}
		}
	}
");
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
@"	{
		var $d = $a;
		try {
			var $x = 0;
		}
		finally {
			if ($ReferenceNotEquals($d, null)) {
				$Upcast($d, {ct_IDisposable}).$Dispose();
			}
		}
	}
");
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
@"	{
		var $d = $Clone($a, {to_S});
		try {
			var $x = $Clone(0, {to_Int32});
		}
		finally {
			$Clone($d, {to_S}).$Dispose();
		}
	}
", mutableValueTypes: true);
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
@"	{
		var $d = $Clone($a, {to_S});
		try {
			var $x = $Clone(0, {to_Int32});
		}
		finally {
			if ($ReferenceNotEquals($d, null)) {
				$Clone($d, {to_S}).$Dispose();
			}
		}
	}
", mutableValueTypes: true);
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
@"	{
		this.set_$MyProperty($a);
		var $d = $a;
		try {
			var $x = 0;
		}
		finally {
			if ($ReferenceNotEquals($d, null)) {
				$d.$Dispose();
			}
		}
	}
");
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
@"	{
		this.set_$MyProperty($a);
		var $tmp1 = $a;
		try {
			var $x = 0;
		}
		finally {
			if ($ReferenceNotEquals($tmp1, null)) {
				$tmp1.$Dispose();
			}
		}
	}
");
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
@"	{
		this.set_$MyProperty($a);
		var $tmp1 = $a;
		try {
			var $x = 0;
		}
		finally {
			if ($ReferenceNotEquals($tmp1, null)) {
				$Upcast($tmp1, {ct_IDisposable}).$Dispose();
			}
		}
	}
");
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
@"	{
		$d = $Clone($a, {to_S});
		var $tmp1 = $Clone($d, {to_S});
		try {
			var $x = $Clone(0, {to_Int32});
		}
		finally {
			$Clone($tmp1, {to_S}).$Dispose();
		}
	}
", mutableValueTypes: true);
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
@"	{
		$d = $Clone($a, {to_S});
		var $tmp1 = $Clone($d, {to_S});
		try {
			var $x = $Clone(0, {to_Int32});
		}
		finally {
			if ($ReferenceNotEquals($tmp1, null)) {
				$Clone($tmp1, {to_S}).$Dispose();
			}
		}
	}
", mutableValueTypes: true);
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
@"	{
		this.set_$P1($a);
		var $d1 = $a;
		try {
			this.set_$P2($b);
			var $d2 = $b;
			try {
				this.set_$P3($c);
				var $d3 = $c;
				try {
					var $x = 0;
				}
				finally {
					if ($ReferenceNotEquals($d3, null)) {
						$d3.$Dispose();
					}
				}
			}
			finally {
				if ($ReferenceNotEquals($d2, null)) {
					$d2.$Dispose();
				}
			}
		}
		finally {
			if ($ReferenceNotEquals($d1, null)) {
				$d1.$Dispose();
			}
		}
	}
");
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
@"	{
		this.set_$MyProperty($a);
		var $d = $a;
		var $tmp1 = $Cast($d, {ct_IDisposable});
		try {
			var $x = 0;
		}
		finally {
			$tmp1.$Dispose();
		}
	}
");
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
@"	{
		var $d = $a;
		try {
			var $x = 0;
		}
		finally {
			if ($ReferenceNotEquals($d, null)) {
				dispose_it($d);
				additional;
			}
		}
	}
", new MockMetadataImporter { GetMethodSemantics = m => m.ContainingType.Name == "MyDisposable" && m.Name == "Dispose" ? MethodScriptSemantics.InlineCode("dispose_it({this}); additional;") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
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
@"	{
		var $d = $a;
		try {
			var $x = 0;
		}
		finally {
			if ($ReferenceNotEquals($d, null)) {
				dispose_it($d);
				additional;
			}
		}
	}
", new MockMetadataImporter { GetMethodSemantics = m => m.ContainingType.Name == "MyDisposable" && m.Name == "Dispose" ? MethodScriptSemantics.InlineCode("dispose_it({this}); additional;") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
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
@"	{
		var $d = $Upcast($a, {ct_IDisposable});
		try {
			var $x = 0;
		}
		finally {
			if ($ReferenceNotEquals($d, null)) {
				$d.$Dispose();
			}
		}
	}
", new MockMetadataImporter { GetMethodSemantics = m => m.ContainingType.Name == "MyDisposable" && m.Name == "Dispose" ? MethodScriptSemantics.InlineCode("dispose_it({this});") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}
	}
}
