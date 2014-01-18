using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Statements {
	[TestFixture]
	public class UsingStatementTests : MethodCompilerTestBase {
		[Test]
		public void UsingStatementWithSingleVariableDeclarationWithSimpleInitializerWorks() {
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
			if ($ReferenceNotEquals($d, $Default({def_IDisposable}))) {
				$d.$Dispose();
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
			$Upcast($Clone($d, {to_S}), {ct_IDisposable}).$Dispose();
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
			if ($ReferenceNotEquals($d, $Default(def_$InstantiateGenericType({Nullable}, {ga_S})))) {
				$Upcast($Clone($d, {to_S}), {ct_IDisposable}).$Dispose();
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
			if ($ReferenceNotEquals($d, $Default({def_IDisposable}))) {
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
			if ($ReferenceNotEquals($tmp1, $Default({def_S}))) {
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
			$Upcast($Clone($tmp1, {to_S}), {ct_IDisposable}).$Dispose();
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
			if ($ReferenceNotEquals($tmp1, $Default(def_$InstantiateGenericType({Nullable}, {ga_S})))) {
				$Upcast($Clone($tmp1, {to_S}), {ct_IDisposable}).$Dispose();
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
					if ($ReferenceNotEquals($d3, $Default({def_IDisposable}))) {
						$d3.$Dispose();
					}
				}
			}
			finally {
				if ($ReferenceNotEquals($d2, $Default({def_IDisposable}))) {
					$d2.$Dispose();
				}
			}
		}
		finally {
			if ($ReferenceNotEquals($d1, $Default({def_IDisposable}))) {
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
	}
}
