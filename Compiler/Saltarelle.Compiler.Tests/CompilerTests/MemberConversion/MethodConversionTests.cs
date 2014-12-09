using System;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MemberConversion {
	[TestFixture]
	public class MethodConversionTests : CompilerTestBase {
		[Test]
		public void SimpleInstanceMethodCanBeConverted() {
			Compile(new[] { "class C { public void M() {} }" });
			var m = FindInstanceMethod("C.M");
			Assert.That(m.Definition, Is.Not.Null);
		}

		[Test]
		public void SimpleStaticMethodCanBeConverted() {
			Compile(new[] { "class C { public static void M() {} }" });
			var m = FindStaticMethod("C.M");
			Assert.That(m.Definition, Is.Not.Null);
		}

		[Test]
		public void MethodImplementedAsInlineCodeDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = method => MethodScriptSemantics.InlineCode("X") };
			Compile(new[] { "class C { public static void M() {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
		}

		[Test]
		public void MethodImplementedAsInlineCodeWithGeneratedMethodNameDoesAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = method => MethodScriptSemantics.InlineCode("X", generatedMethodName: "someMethod") };
			Compile(new[] { "class C { public static void M() {} }" }, metadataImporter: metadataImporter);
			var m = FindInstanceMethod("C.someMethod");
			Assert.That(m.Definition, Is.Not.Null);
		}

		[Test]
		public void MethodImplementedAsNotUsableFromScriptDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = method => MethodScriptSemantics.NotUsableFromScript() };
			Compile(new[] { "class C { public static void M() {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
		}

		[Test]
		public void InstanceMethodWithGenerateCodeSetToFalseDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = method => MethodScriptSemantics.NormalMethod("X", generateCode: false) };
			Compile(new[] { "class C { public static void M() {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
		}

		[Test]
		public void StaticMethodWithGenerateCodeSetToFalseDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = method => MethodScriptSemantics.NormalMethod("X", generateCode: false) };
			Compile(new[] { "class C { public static void M() {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
		}

		[Test]
		public void StaticMethodWithThisAsFirstArgumentAppearsOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = method => MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("X") };
			Compile(new[] { "class C { public static void M() {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindStaticMethod("C.X"), Is.Not.Null);
		}

		[Test]
		public void BaseMethodsAreNotIncludedInDerivedType() {
			Compile(new[] { "class B { public void X() {} } class C : B { public void Y() {} }" });
			var cls = FindClass("C");
			Assert.That(cls.InstanceMethods, Has.Count.EqualTo(1));
			Assert.That(cls.InstanceMethods[0].Name, Is.EqualTo("Y"));
		}

		[Test]
		public void ShadowingMethodsAreIncluded() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.ContainingType.Name == "C" ? "XDerived" : m.Name) };
			Compile(new[] { "class B { public void X() {} } class C : B { public new void X() {} }" }, metadataImporter: metadataImporter);
			var cls = FindClass("C");
			Assert.That(cls.InstanceMethods, Has.Count.EqualTo(1));
			Assert.That(cls.InstanceMethods[0].Name, Is.EqualTo("XDerived"));
		}

		[Test]
		public void OverridingMethodsAreIncluded() {
			Compile(new[] { "class B { public virtual void X() {} } class C : B { public override void X() {} }" });
			var cls = FindClass("C");
			Assert.That(cls.InstanceMethods, Has.Count.EqualTo(1));
			Assert.That(cls.InstanceMethods[0].Name, Is.EqualTo("X"));
		}

		[Test]
		public void OperatorsWork() {
			Compile(new[] { "class C { public static bool operator==(C a, C b) { return true; } public static bool operator!=(C a, C b) { return false; } }" });
			Assert.That(FindStaticMethod("C.op_Equality"), Is.Not.Null);
			Assert.That(FindStaticMethod("C.op_Inequality"), Is.Not.Null);
		}

		[Test]
		public void ConversionOperatorsWork() {
			Compile(new[] { "class C { public static explicit operator bool(C a) { return false; } public static implicit operator int(C a) { return 0; } }" });
			Assert.That(FindStaticMethod("C.op_Explicit"), Is.Not.Null);
			Assert.That(FindStaticMethod("C.op_Implicit"), Is.Not.Null);
		}

		[Test]
		public void PartialMethodWithoutDefinitionIsNotImported() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = m => { throw new InvalidOperationException(); } };
			Compile(new[] { "partial class C { partial void M(); }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void OverloadedPartialMethodsWork() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$M_" + m.Parameters.Length) };
			Compile(new[] { "partial class C { partial void M(); partial void M(int i); }", "partial class C { partial void M(int i) {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.$M_0"), Is.Null);
			Assert.That(FindInstanceMethod("C.$M_1"), Is.Not.Null);
		}

		[Test]
		public void PartialMethodWithDeclarationAndDefinitionIsImported() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$M") };
			Compile(new[] { "partial class C { partial void M(); }", "partial class C { partial void M() {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.$M"), Is.Not.Null);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void GenericMethodTypeArgumentsAreIncludedForInstanceMethods() {
			var namer = new MockNamer { GetTypeParameterName = tp => "$$" + tp.Name };
			Compile(new[] { "class C { public void X<U, V>() {} }" }, namer: namer);
			Assert.That(FindInstanceMethod("C.X").TypeParameterNames, Is.EqualTo(new[] { "$$U", "$$V" }));
		}

		[Test]
		public void GenericMethodTypeArgumentsAreIgnoredForInstanceMethodsIfTheMethodImplOptionsSaySo() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("X", ignoreGenericArguments: true) };
			var namer = new MockNamer { GetTypeParameterName = tp => "$$" + tp.Name };
			Compile(new[] { "class C { public void X<U, V>() {} }" }, metadataImporter: metadataImporter, namer: namer);
			Assert.That(FindInstanceMethod("C.X").TypeParameterNames, Is.Empty);
		}

		[Test]
		public void GenericMethodTypeArgumentsAreIncludedForStaticMethods() {
			var namer = new MockNamer { GetTypeParameterName = tp => "$$" + tp.Name };
			Compile(new[] { "class C { public static void X<U, V>() {} }" }, namer: namer);
			Assert.That(FindStaticMethod("C.X").TypeParameterNames, Is.EqualTo(new[] { "$$U", "$$V" }));
		}

		[Test]
		public void GenericMethodTypeArgumentsAreIgnoredForStaticMethodsIfTheMethodImplOptionsSaySo() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("X", ignoreGenericArguments: true) };
			var namer = new MockNamer { GetTypeParameterName = tp => "$$" + tp.Name };
			Compile(new[] { "class C { public static void X<U, V>() {} }" }, metadataImporter: metadataImporter, namer: namer);
			Assert.That(FindStaticMethod("C.X").TypeParameterNames, Is.Empty);
		}

		[Test]
		public void AbstractMethodIsNotImported() {
			Compile(new[] { "abstract class C { public abstract void M(); }" });
			var m = FindInstanceMethod("C.M");
			Assert.That(m, Is.Null);
		}
	}
}
