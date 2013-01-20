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
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyEnum
var $SomeNamespace_InnerNamespace_MyEnum = function() {
};
$SomeNamespace_InnerNamespace_MyEnum.prototype = { value1: 1, value2: 2, value3: 3 };
{Type}.registerEnum(global, 'SomeNamespace.InnerNamespace.MyEnum', $SomeNamespace_InnerNamespace_MyEnum, false);
",			new JsEnum(Common.CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyEnum", Common.CreateMockAssembly()), new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
		}

		[Test]
		public void EnumWithoutNamespaceWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $MyEnum = function() {
};
$MyEnum.prototype = { value1: 1, value2: 2, value3: 3 };
{Type}.registerEnum(global, 'MyEnum', $MyEnum, false);
",			new JsEnum(Common.CreateMockTypeDefinition("MyEnum", Common.CreateMockAssembly()), new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
		}

		[Test]
		public void FlagsAttributeWorks() {
			var typeDef = Common.CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyEnum", Common.CreateMockAssembly(), attributes: new Expression<Func<Attribute>>[] { () => new FlagsAttribute() });

			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyEnum
var $SomeNamespace_InnerNamespace_MyEnum = function() {
};
$SomeNamespace_InnerNamespace_MyEnum.prototype = { value1: 1, value2: 2, value3: 3 };
{Type}.registerEnum(global, 'SomeNamespace.InnerNamespace.MyEnum', $SomeNamespace_InnerNamespace_MyEnum, true);
",			new JsEnum(typeDef, new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
		}

		[Test]
		public void NamedValuesAttributeWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyEnum
var $SomeNamespace_InnerNamespace_MyEnum = function() {
};
$SomeNamespace_InnerNamespace_MyEnum.prototype = { value1: 'value1', value2: 'value2', value3: 'value3' };
{Type}.registerEnum(global, 'SomeNamespace.InnerNamespace.MyEnum', $SomeNamespace_InnerNamespace_MyEnum, false);
",			new JsEnum(Common.CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyEnum", Common.CreateMockAssembly(), attributes: new Expression<Func<Attribute>>[] { () => new NamedValuesAttribute() }), new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
		}

		[Test]
		public void InternalEnumIsNotExported() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
var $MyEnum = function() {
};
$MyEnum.prototype = { value1: 1, value2: 2, value3: 3 };
{Type}.registerEnum(null, 'MyEnum', $MyEnum, false);
",			new JsEnum(Common.CreateMockTypeDefinition("MyEnum", Common.CreateMockAssembly(), Accessibility.Internal), new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
		}
	}
}
