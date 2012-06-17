using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Moq;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler.Tests.ScriptSharpOOPEmulator {
	[TestFixture]
	public class EnumTests : ScriptSharpOOPEmulatorTestBase {
		[Test]
		public void EnumWorks() {
			AssertCorrect(
@"{Type}.registerNamespace('SomeNamespace.InnerNamespace');
////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyEnum
{MyEnum} = function() {
};
{MyEnum}.prototype = { value1: 1, value2: 2, value3: 3 };
{MyEnum}.registerEnum('SomeNamespace.InnerNamespace.MyEnum', false);
",			new JsEnum(CreateMockType(), "SomeNamespace.InnerNamespace.MyEnum", new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
		}

		[Test]
		public void EnumWithoutNamespaceWorks() {
			AssertCorrect(
@"////////////////////////////////////////////////////////////////////////////////
// MyEnum
{MyEnum} = function() {
};
{MyEnum}.prototype = { value1: 1, value2: 2, value3: 3 };
{MyEnum}.registerEnum('MyEnum', false);
",			new JsEnum(CreateMockType(), "MyEnum", new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
		}

		[Test]
		public void FlagsAttributeWorks() {
			var typeDef = new Mock<ITypeDefinition>(MockBehavior.Strict);
			var attr = new Mock<IAttribute>(MockBehavior.Strict);
			var attrType = new Mock<ITypeDefinition>();
			typeDef.SetupGet(_ => _.Attributes).Returns(new[] { attr.Object });
			attr.Setup(_ => _.AttributeType).Returns(attrType.Object);
			attr.Setup(_ => _.PositionalArguments).Returns(new ResolveResult[0]);
			attrType.SetupGet(_ => _.FullName).Returns("System.FlagsAttribute");

			AssertCorrect(
@"{Type}.registerNamespace('SomeNamespace.InnerNamespace');
////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyEnum
{MyEnum} = function() {
};
{MyEnum}.prototype = { value1: 1, value2: 2, value3: 3 };
{MyEnum}.registerEnum('SomeNamespace.InnerNamespace.MyEnum', true);
",			new JsEnum(typeDef.Object, "SomeNamespace.InnerNamespace.MyEnum", new[] { new JsEnumValue("value1", 1), new JsEnumValue("value2", 2), new JsEnumValue("value3", 3) }));
		}
	}
}
