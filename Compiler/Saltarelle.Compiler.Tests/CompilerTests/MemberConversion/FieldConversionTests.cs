using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MemberConversion {
	[TestFixture]
	public class FieldConversionTests : CompilerTestBase {
		[Test]
		public void InstanceFieldsAreCorrectlyImported() {
			Compile(new[] { "class C { public int SomeField; }" });
			Assert.That(FindInstanceFieldInitializer("C.$SomeField"), Is.Not.Null);
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void StaticFieldsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.ContainingType.SpecialType == SpecialType.System_Object) };
			Compile(new[] { "class C { public static int SomeField; }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticFieldInitializer("C.$SomeField"), Is.Not.Null);
			Assert.That(FindClass("C").UnnamedConstructor.Body.Statements, Is.Empty);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void FieldsThatAreNotUsableFromScriptAreNotImported() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.ContainingType.SpecialType == SpecialType.System_Object), GetFieldSemantics = f => FieldScriptSemantics.NotUsableFromScript() };
			Compile(new[] { "class C { public int SomeField; }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").UnnamedConstructor.Body.Statements, Is.Empty);
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void ImportingMultipleFieldsInTheSameDeclarationWorks() {
			Compile(new[] { "class C { public int Field1, Field2; }" });
			Assert.That(FindInstanceFieldInitializer("C.$Field1"), Is.Not.Null);
			Assert.That(FindInstanceFieldInitializer("C.$Field2"), Is.Not.Null);
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void RuntimeLibraryCanReturnNullFromInitializeFieldToPreventInitializationCodeFromAppearing() {
			Compile(new[] { "class C { public int f1 = 2, f2; public static int f3 = 3, f4; }" }, runtimeLibrary: new MockRuntimeLibrary { InitializeField = (t, n, m, v, c) => null });
			Assert.That(FindClass("C").UnnamedConstructor.Body.Statements, Has.Count.EqualTo(1));	// Only the base constructor call.
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
		}
	}
}
