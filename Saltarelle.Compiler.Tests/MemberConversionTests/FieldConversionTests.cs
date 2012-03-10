using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MemberConversionTests {
    [TestFixture]
    public class FieldConversionTests : CompilerTestBase {
        [Test]
        public void InstanceFieldsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetFieldImplementation = f => FieldImplOptions.Field("$SomeProp") };
            Compile(new[] { "class C { public int SomeField; }" }, namingConvention: namingConvention);
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").StaticFields.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void StaticFieldsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetFieldImplementation = f => FieldImplOptions.Field("$SomeProp") };
            Compile(new[] { "class C { public static int SomeField; }" }, namingConvention: namingConvention);
            FindStaticField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceFields.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void FieldsThatAreNotUsableFromScriptAreNotImported() {
            var namingConvention = new MockNamingConventionResolver { GetFieldImplementation = f => FieldImplOptions.NotUsableFromScript() };
            Compile(new[] { "class C { public int SomeField; }" }, namingConvention: namingConvention);
            FindClass("C").InstanceFields.Should().BeEmpty();
            FindClass("C").StaticFields.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ImportingMultipleFieldsInTheSameDeclarationWorks() {
            var namingConvention = new MockNamingConventionResolver { GetFieldImplementation = f => FieldImplOptions.Field("$" + f.Name) };
            Compile(new[] { "class C { public int Field1, Field2; }" }, namingConvention: namingConvention);
            FindInstanceField("C.$Field1").Should().NotBeNull();
            FindInstanceField("C.$Field2").Should().NotBeNull();
            FindClass("C").StaticFields.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }


        [Test]
        public void FieldWithoutInitializerIsInitializedToDefault() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void FieldInitializersAreCorrectlyImported() {
            Assert.Inconclusive("TODO");
        }
    }
}
