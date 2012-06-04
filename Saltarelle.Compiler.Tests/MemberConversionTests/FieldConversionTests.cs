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
            FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").StaticInitStatements.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void StaticFieldsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetFieldImplementation = f => FieldImplOptions.Field("$SomeProp") };
            Compile(new[] { "class C { public static int SomeField; }" }, namingConvention: namingConvention);
            FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void FieldsThatAreNotUsableFromScriptAreNotImported() {
            var namingConvention = new MockNamingConventionResolver { GetFieldImplementation = f => FieldImplOptions.NotUsableFromScript() };
            Compile(new[] { "class C { public int SomeField; }" }, namingConvention: namingConvention);
            FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
            FindClass("C").StaticInitStatements.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ImportingMultipleFieldsInTheSameDeclarationWorks() {
            var namingConvention = new MockNamingConventionResolver { GetFieldImplementation = f => FieldImplOptions.Field("$" + f.Name) };
            Compile(new[] { "class C { public int Field1, Field2; }" }, namingConvention: namingConvention);
            FindInstanceFieldInitializer("C.$Field1").Should().NotBeNull();
            FindInstanceFieldInitializer("C.$Field2").Should().NotBeNull();
            FindClass("C").StaticInitStatements.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }
    }
}
