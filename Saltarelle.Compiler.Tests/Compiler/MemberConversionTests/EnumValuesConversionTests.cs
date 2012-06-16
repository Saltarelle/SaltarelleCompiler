using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.Compiler.MemberConversionTests {
    [TestFixture]
    public class EnumValuesConversionTests : CompilerTestBase {
        [Test]
        public void EmptyEnumIsCorrectlyImported() {
            Compile(new[] { "enum MyEnum {}" });
            FindEnum("MyEnum").Values.Should().BeEmpty();
        }

        [Test]
        public void EnumWithValuesIsCorrectlyImported() {
            Compile(new[] { "enum MyEnum { Member1, Member2 }" });
            FindEnum("MyEnum").Values.Should().HaveCount(2);
            FindEnumValue("MyEnum.$Member1").Value.Should().Be(0);
            FindEnumValue("MyEnum.$Member2").Value.Should().Be(1);
        }

        [Test]
        public void EnumWithExplicitValuesIsCorrectlyImported() {
            Compile(new[] { "enum MyEnum { Member1 = 2, Member2 }" });
            FindEnumValue("MyEnum.$Member1").Value.Should().Be(2);
            FindEnumValue("MyEnum.$Member2").Value.Should().Be(3);
        }

        [Test]
        public void EnumWithOtherBaseTypeIsCorrectlyImported() {
            Compile(new[] { "enum MyEnum : short { Member1, Member2 }" });
            FindEnum("MyEnum").Values.Should().HaveCount(2);
        }

        [Test]
        public void EnumValueInitialiserCanReferenceOtherMember() {
            Compile(new[] { "enum MyEnum { Member1 = 2, Member2 = Member1 }" });
            FindEnumValue("MyEnum.$Member1").Value.Should().Be(2);
            FindEnumValue("MyEnum.$Member2").Value.Should().Be(2);
        }

        [Test]
        public void EnumValueInitialiserCanUseComplexExpressions() {
            Compile(new[] { "enum MyEnum { Member1 = 2, Member2 = (int)(2 * (Member1 + (float)3) - 2) }" });
            FindEnumValue("MyEnum.$Member1").Value.Should().Be(2);
            FindEnumValue("MyEnum.$Member2").Value.Should().Be(8);
        }

        [Test]
        public void EnumValueInitialiserCanReferenceLaterValue() {
            Compile(new[] { "enum MyEnum { Member1 = Member2, Member2 = 4 }" });
            FindEnumValue("MyEnum.$Member1").Value.Should().Be(4);
            FindEnumValue("MyEnum.$Member2").Value.Should().Be(4);
        }

        [Test]
        public void EnumValueWhichIsNotUsableFromScriptIsNotImported() {
			var nc = new MockNamingConventionResolver { GetFieldSemantics = f => f.Name == "Member2" ? FieldScriptSemantics.NotUsableFromScript() : FieldScriptSemantics.Field("$" + f.Name) };
            Compile(new[] { "enum MyEnum { Member1, Member2, Member3 }" }, namingConvention: nc);
			FindEnum("MyEnum").Values.Select(v => v.Name).Should().BeEquivalentTo(new[] { "$Member1", "$Member3" });
        }
    }
}
