using FluentAssertions;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.Compiler.MemberConversionTests {
    [TestFixture]
    public class EnumValuesConversionTests : CompilerTestBase {
        [Test]
        public void EmptyEnumIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEnumValueName = v => "$" + v.Name };
            Compile(new[] { "enum MyEnum {}" }, namingConvention: namingConvention);
            FindEnum("MyEnum").Values.Should().BeEmpty();
        }

        [Test]
        public void EnumWithValuesIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEnumValueName = v => "$" + v.Name };
            Compile(new[] { "enum MyEnum { Member1, Member2 }" }, namingConvention: namingConvention);
            FindEnum("MyEnum").Values.Should().HaveCount(2);
            FindEnumValue("MyEnum.$Member1").Value.Should().Be(0);
            FindEnumValue("MyEnum.$Member2").Value.Should().Be(1);
        }

        [Test]
        public void EnumWithExplicitValuesIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEnumValueName = v => "$" + v.Name };
            Compile(new[] { "enum MyEnum { Member1 = 2, Member2 }" }, namingConvention: namingConvention);
            FindEnumValue("MyEnum.$Member1").Value.Should().Be(2);
            FindEnumValue("MyEnum.$Member2").Value.Should().Be(3);
        }

        [Test]
        public void EnumWithOtherBaseTypeIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEnumValueName = v => "$" + v.Name };
            Compile(new[] { "enum MyEnum : short { Member1, Member2 }" }, namingConvention: namingConvention);
            FindEnum("MyEnum").Values.Should().HaveCount(2);
        }

        [Test]
        public void EnumValueInitialiserCanReferenceOtherMember() {
            var namingConvention = new MockNamingConventionResolver { GetEnumValueName = v => "$" + v.Name };
            Compile(new[] { "enum MyEnum { Member1 = 2, Member2 = Member1 }" }, namingConvention: namingConvention);
            FindEnumValue("MyEnum.$Member1").Value.Should().Be(2);
            FindEnumValue("MyEnum.$Member2").Value.Should().Be(2);
        }

        [Test]
        public void EnumValueInitialiserCanUseComplexExpressions() {
            var namingConvention = new MockNamingConventionResolver { GetEnumValueName = v => "$" + v.Name };
            Compile(new[] { "enum MyEnum { Member1 = 2, Member2 = (int)(2 * (Member1 + (float)3) - 2) }" }, namingConvention: namingConvention);
            FindEnumValue("MyEnum.$Member1").Value.Should().Be(2);
            FindEnumValue("MyEnum.$Member2").Value.Should().Be(8);
        }

        [Test]
        public void EnumValueInitialiserCanReferenceLaterValue() {
            var namingConvention = new MockNamingConventionResolver { GetEnumValueName = v => "$" + v.Name };
            Compile(new[] { "enum MyEnum { Member1 = Member2, Member2 = 4 }" }, namingConvention: namingConvention);
            FindEnumValue("MyEnum.$Member1").Value.Should().Be(4);
            FindEnumValue("MyEnum.$Member2").Value.Should().Be(4);
        }
    }
}
