using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MemberConversion {
    [TestFixture]
    public class FieldConversionTests : CompilerTestBase {
        [Test]
        public void InstanceFieldsAreCorrectlyImported() {
            var metadataImporter = new MockMetadataImporter { GetFieldSemantics = f => FieldScriptSemantics.Field("$SomeProp") };
            Compile(new[] { "class C { public int SomeField; }" }, metadataImporter: metadataImporter);
            FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").StaticInitStatements.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void StaticFieldsAreCorrectlyImported() {
            var metadataImporter = new MockMetadataImporter { GetFieldSemantics = f => FieldScriptSemantics.Field("$SomeProp") };
            Compile(new[] { "class C { public static int SomeField; }" }, metadataImporter: metadataImporter);
            FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void FieldsThatAreNotUsableFromScriptAreNotImported() {
            var metadataImporter = new MockMetadataImporter { GetFieldSemantics = f => FieldScriptSemantics.NotUsableFromScript() };
            Compile(new[] { "class C { public int SomeField; }" }, metadataImporter: metadataImporter);
            FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
            FindClass("C").StaticInitStatements.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ImportingMultipleFieldsInTheSameDeclarationWorks() {
            var metadataImporter = new MockMetadataImporter { GetFieldSemantics = f => FieldScriptSemantics.Field("$" + f.Name) };
            Compile(new[] { "class C { public int Field1, Field2; }" }, metadataImporter: metadataImporter);
            FindInstanceFieldInitializer("C.$Field1").Should().NotBeNull();
            FindInstanceFieldInitializer("C.$Field2").Should().NotBeNull();
            FindClass("C").StaticInitStatements.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }
    }
}
