using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Moq;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler.Tests.ScriptSharpOOPEmulatorTests {
	[TestFixture]
	public class EnumTests : ScriptSharpOOPEmulatorTestBase {
		[Test]
		public void EnumWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyEnum
var $SomeNamespace_InnerNamespace_MyEnum = function() {
};
$SomeNamespace_InnerNamespace_MyEnum.prototype = { value1: 1, value2: 2, value3: 3 };
{Type}.registerEnum(global, 'SomeNamespace.InnerNamespace.MyEnum', $SomeNamespace_InnerNamespace_MyEnum, false);
",			new JsEnum(CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyEnum"), new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
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
",			new JsEnum(CreateMockTypeDefinition("MyEnum"), new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
		}

		[Test]
		public void FlagsAttributeWorks() {
			var typeDef = Common.CreateTypeMock("SomeNamespace.InnerNamespace.MyEnum");
			typeDef.SetupGet(_ => _.Accessibility).Returns(Accessibility.Public);
			var attr = new Mock<IAttribute>(MockBehavior.Strict);
			var attrType = new Mock<ITypeDefinition>();
			typeDef.SetupGet(_ => _.Attributes).Returns(new[] { attr.Object });
			attr.Setup(_ => _.AttributeType).Returns(attrType.Object);
			attr.Setup(_ => _.PositionalArguments).Returns(new ResolveResult[0]);
			attrType.SetupGet(_ => _.FullName).Returns("System.FlagsAttribute");

			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyEnum
var $SomeNamespace_InnerNamespace_MyEnum = function() {
};
$SomeNamespace_InnerNamespace_MyEnum.prototype = { value1: 1, value2: 2, value3: 3 };
{Type}.registerEnum(global, 'SomeNamespace.InnerNamespace.MyEnum', $SomeNamespace_InnerNamespace_MyEnum, true);
",			new JsEnum(typeDef.Object, new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
		}

		[Test]
		public void NamedValuesAttributeWorks() {
			Assert.Inconclusive("Type must have NamedValuesAttribute");
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyEnum
var $SomeNamespace_InnerNamespace_MyEnum = function() {
};
$SomeNamespace_InnerNamespace_MyEnum.prototype = { value1: 'value1', value2: 'value2', value3: 'value3' };
{Type}.registerEnum(global, 'SomeNamespace.InnerNamespace.MyEnum', $SomeNamespace_InnerNamespace_MyEnum, false);
",			new JsEnum(CreateMockTypeDefinition("SomeNamespace.InnerNamespace.MyEnum"), new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
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
",			new JsEnum(CreateMockTypeDefinition("MyEnum", Accessibility.Internal), new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
		}
	}
}
