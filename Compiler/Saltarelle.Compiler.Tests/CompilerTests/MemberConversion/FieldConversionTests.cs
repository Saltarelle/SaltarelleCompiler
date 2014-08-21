using FluentAssertions;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MemberConversion {
	[TestFixture]
	public class FieldConversionTests : CompilerTestBase {
		[Test]
		public void InstanceFieldsAreCorrectlyImported() {
			Compile(new[] { "class C { public int SomeField; }" });
			FindInstanceFieldInitializer("C.$SomeField").Should().NotBeNull();
			FindClass("C").StaticInitStatements.Should().BeEmpty();
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void StaticFieldsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.ContainingType.SpecialType == SpecialType.System_Object) };
			Compile(new[] { "class C { public static int SomeField; }" }, metadataImporter: metadataImporter);
			FindStaticFieldInitializer("C.$SomeField").Should().NotBeNull();
			FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void FieldsThatAreNotUsableFromScriptAreNotImported() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.ContainingType.SpecialType == SpecialType.System_Object), GetFieldSemantics = f => FieldScriptSemantics.NotUsableFromScript() };
			Compile(new[] { "class C { public int SomeField; }" }, metadataImporter: metadataImporter);
			FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
			FindClass("C").StaticInitStatements.Should().BeEmpty();
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void ImportingMultipleFieldsInTheSameDeclarationWorks() {
			Compile(new[] { "class C { public int Field1, Field2; }" });
			FindInstanceFieldInitializer("C.$Field1").Should().NotBeNull();
			FindInstanceFieldInitializer("C.$Field2").Should().NotBeNull();
			FindClass("C").StaticInitStatements.Should().BeEmpty();
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void RuntimeLibraryCanReturnNullFromInitializeFieldToPreventInitializationCodeFromAppearing() {
			Compile(new[] { "class C { public int f1 = 2, f2; public static int f3 = 3, f4; }" }, runtimeLibrary: new MockRuntimeLibrary { InitializeField = (t, n, m, v, c) => null });
			FindClass("C").UnnamedConstructor.Body.Statements.Should().HaveCount(1);	// Only the base constructor call.
			FindClass("C").StaticInitStatements.Should().BeEmpty();
		}
	}
}
