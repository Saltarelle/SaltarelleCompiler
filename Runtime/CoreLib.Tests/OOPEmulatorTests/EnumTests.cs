using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace CoreLib.Tests.OOPEmulatorTests {
	[TestFixture]
	public class EnumTests : OOPEmulatorTestBase {
		[Test]
		public void EnumWorks() {
			AssertCorrect(
@"public enum MyEnum { Value1, Value2, Value3 }
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $MyEnum = function() {
};
$MyEnum.__typeName = 'MyEnum';
global.MyEnum = $MyEnum;
{Script}.initEnum($MyEnum, $asm, { value1: 0, value2: 1, value3: 2 });
");
		}

		[Test]
		public void EnumWithNamespaceWorks() {
			AssertCorrect(
@"namespace SomeNamespace.InnerNamespace {
	public enum MyEnum { Value1, Value2, Value3 }
}",
@"global.SomeNamespace = global.SomeNamespace || {};
global.SomeNamespace.InnerNamespace = global.SomeNamespace.InnerNamespace || {};
{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyEnum
var $SomeNamespace_InnerNamespace_MyEnum = function() {
};
$SomeNamespace_InnerNamespace_MyEnum.__typeName = 'SomeNamespace.InnerNamespace.MyEnum';
global.SomeNamespace.InnerNamespace.MyEnum = $SomeNamespace_InnerNamespace_MyEnum;
{Script}.initEnum($SomeNamespace_InnerNamespace_MyEnum, $asm, { value1: 0, value2: 1, value3: 2 });
");
		}

		[Test]
		public void FlagsAttributeWorks() {
			AssertCorrect(
@"[System.Flags] public enum MyEnum { Value1, Value2, Value3 }
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $MyEnum = function() {
};
$MyEnum.__typeName = 'MyEnum';
global.MyEnum = $MyEnum;
{Script}.initEnum($MyEnum, $asm, { value1: 0, value2: 1, value3: 2 });
{Script}.setMetadata($MyEnum, { enumFlags: true });
");
		}

		[Test]
		public void NamedValuesAttributeWorks() {
			AssertCorrect(
@"[System.Runtime.CompilerServices.NamedValues] public enum MyEnum { Value1, Value2, Value3 }
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $MyEnum = function() {
};
$MyEnum.__typeName = 'MyEnum';
global.MyEnum = $MyEnum;
{Script}.initEnum($MyEnum, $asm, { value1: 'value1', value2: 'value2', value3: 'value3' });
");
		}

		[Test]
		public void InternalEnumIsNotExported() {
			AssertCorrect(
@"internal enum MyEnum { Value1, Value2, Value3 }
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $$MyEnum = function() {
};
$$MyEnum.__typeName = '$MyEnum';
{Script}.initEnum($$MyEnum, $asm, { $value1: 0, $value2: 1, $value3: 2 });
");
		}
	}
}
