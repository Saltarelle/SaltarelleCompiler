using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace CoreLib.Tests.OOPEmulatorTests {
	[TestFixture]
	public class EnumTests : OOPEmulatorTestBase {
		[Test]
		public void EnumWorks() {
			AssertCorrectEmulation(
@"public enum MyEnum { Value1, Value2, Value3 }
",
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $MyEnum = function() {
};
$MyEnum.__typeName = 'MyEnum';
global.MyEnum = $MyEnum;
-
{Script}.initEnum($MyEnum, $asm, { value1: 0, value2: 1, value3: 2 });
", "MyEnum");
		}

		[Test]
		public void EnumWithNamespaceWorks() {
			AssertCorrectEmulation(
@"namespace SomeNamespace.InnerNamespace {
	public enum MyEnum { Value1, Value2, Value3 }
}",
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyEnum
var $SomeNamespace_InnerNamespace_MyEnum = function() {
};
$SomeNamespace_InnerNamespace_MyEnum.__typeName = 'SomeNamespace.InnerNamespace.MyEnum';
global.SomeNamespace.InnerNamespace.MyEnum = $SomeNamespace_InnerNamespace_MyEnum;
-
{Script}.initEnum($SomeNamespace_InnerNamespace_MyEnum, $asm, { value1: 0, value2: 1, value3: 2 });
", "SomeNamespace.InnerNamespace.MyEnum");
		}

		[Test]
		public void FlagsAttributeWorks() {
			AssertCorrectEmulation(
@"[System.Flags] public enum MyEnum { Value1, Value2, Value3 }
",
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $MyEnum = function() {
};
$MyEnum.__typeName = 'MyEnum';
global.MyEnum = $MyEnum;
-
{Script}.initEnum($MyEnum, $asm, { value1: 0, value2: 1, value3: 2 });
-
{Script}.setMetadata($MyEnum, { enumFlags: true });
", "MyEnum");
		}

		[Test]
		public void NamedValuesAttributeWorks() {
			AssertCorrectEmulation(
@"[System.Runtime.CompilerServices.NamedValues] public enum MyEnum { Value1, Value2, Value3 }
",
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $MyEnum = function() {
};
$MyEnum.__typeName = 'MyEnum';
global.MyEnum = $MyEnum;
-
{Script}.initEnum($MyEnum, $asm, { value1: 'value1', value2: 'value2', value3: 'value3' }, true);
", "MyEnum");
		}

		[Test]
		public void InternalEnumIsNotExported() {
			AssertCorrectEmulation(
@"internal enum MyEnum { Value1, Value2, Value3 }
",
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $$MyEnum = function() {
};
$$MyEnum.__typeName = '$MyEnum';
-
{Script}.initEnum($$MyEnum, $asm, { $value1: 0, $value2: 1, $value3: 2 });
", "MyEnum");
		}
	}
}
