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
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $MyEnum = function() {
};
$MyEnum.__typeName = 'MyEnum';
var $MyEnum$$members = { value1: 0, value2: 1, value3: 2 };
global.MyEnum = $MyEnum;
{Script}.initEnum($MyEnum, $MyEnum$$members);
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
////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyEnum
var $SomeNamespace_InnerNamespace_MyEnum = function() {
};
$SomeNamespace_InnerNamespace_MyEnum.__typeName = 'SomeNamespace.InnerNamespace.MyEnum';
var $SomeNamespace_InnerNamespace_MyEnum$$members = { value1: 0, value2: 1, value3: 2 };
global.SomeNamespace.InnerNamespace.MyEnum = $SomeNamespace_InnerNamespace_MyEnum;
{Script}.initEnum($SomeNamespace_InnerNamespace_MyEnum, $SomeNamespace_InnerNamespace_MyEnum$$members);
");
		}

		[Test]
		public void FlagsAttributeWorks() {
			AssertCorrect(
@"[System.Flags] public enum MyEnum { Value1, Value2, Value3 }
",
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $MyEnum = function() {
};
$MyEnum.__typeName = 'MyEnum';
var $MyEnum$$members = { value1: 0, value2: 1, value3: 2 };
global.MyEnum = $MyEnum;
{Script}.initEnum($MyEnum, $MyEnum$$members);
{Script}.setMetadata($MyEnum, { enumFlags: true });
");
		}

		[Test]
		public void NamedValuesAttributeWorks() {
			AssertCorrect(
@"[System.Runtime.CompilerServices.NamedValues] public enum MyEnum { Value1, Value2, Value3 }
",
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $MyEnum = function() {
};
$MyEnum.__typeName = 'MyEnum';
var $MyEnum$$members = { value1: 'value1', value2: 'value2', value3: 'value3' };
global.MyEnum = $MyEnum;
{Script}.initEnum($MyEnum, $MyEnum$$members);
");
		}

		[Test]
		public void InternalEnumIsNotExported() {
			AssertCorrect(
@"internal enum MyEnum { Value1, Value2, Value3 }
",
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $$MyEnum = function() {
};
$$MyEnum.__typeName = '$MyEnum';
var $$MyEnum$$members = { $value1: 0, $value2: 1, $value3: 2 };
{Script}.initEnum($$MyEnum, $$MyEnum$$members);
");
		}
	}
}
