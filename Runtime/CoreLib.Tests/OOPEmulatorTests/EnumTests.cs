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
var $MyEnum = global.MyEnum = {Script}.mkEnum($asm, 'MyEnum', { value1: 0, value2: 1, value3: 2 });
-
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
var $SomeNamespace_InnerNamespace_MyEnum = global.SomeNamespace.InnerNamespace.MyEnum = {Script}.mkEnum($asm, 'SomeNamespace.InnerNamespace.MyEnum', { value1: 0, value2: 1, value3: 2 });
-
", "SomeNamespace.InnerNamespace.MyEnum");
		}

		[Test]
		public void FlagsAttributeWorks() {
			AssertCorrectEmulation(
@"[System.Flags] public enum MyEnum { Value1, Value2, Value3 }
",
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $MyEnum = global.MyEnum = {Script}.mkEnum($asm, 'MyEnum', { value1: 0, value2: 1, value3: 2 });
-
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
var $MyEnum = global.MyEnum = {Script}.mkEnum($asm, 'MyEnum', { value1: 'value1', value2: 'value2', value3: 'value3' }, true);
-
", "MyEnum");
		}

		[Test]
		public void InternalEnumIsNotExported() {
			AssertCorrectEmulation(
@"internal enum MyEnum { Value1, Value2, Value3 }
",
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $$MyEnum = {Script}.mkEnum($asm, '$MyEnum', { $value1: 0, $value2: 1, $value3: 2 });
-
", "MyEnum");
		}
	}
}
