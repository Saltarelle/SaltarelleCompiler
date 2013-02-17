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
$MyEnum.prototype = { value1: 0, value2: 1, value3: 2 };
{Script}.registerEnum(global, 'MyEnum', $MyEnum);
");
		}

		[Test]
		public void EnumWithNamespaceWorks() {
			AssertCorrect(
@"namespace SomeNamespace.InnerNamespace {
	public enum MyEnum { Value1, Value2, Value3 }
}",
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyEnum
var $SomeNamespace_InnerNamespace_MyEnum = function() {
};
$SomeNamespace_InnerNamespace_MyEnum.prototype = { value1: 0, value2: 1, value3: 2 };
{Script}.registerEnum(global, 'SomeNamespace.InnerNamespace.MyEnum', $SomeNamespace_InnerNamespace_MyEnum);
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
$MyEnum.prototype = { value1: 0, value2: 1, value3: 2 };
{Script}.registerEnum(global, 'MyEnum', $MyEnum, { enumFlags: true });
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
$MyEnum.prototype = { value1: 'value1', value2: 'value2', value3: 'value3' };
{Script}.registerEnum(global, 'MyEnum', $MyEnum);
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
$$MyEnum.prototype = { $value1: 0, $value2: 1, $value3: 2 };
{Script}.registerEnum(null, '$MyEnum', $$MyEnum);
");
		}
	}
}
