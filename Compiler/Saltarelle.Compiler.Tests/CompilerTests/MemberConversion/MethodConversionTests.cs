﻿using System;
using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MemberConversion {
	[TestFixture]
	public class MethodConversionTests : CompilerTestBase {
		[Test]
		public void SimpleInstanceMethodCanBeConverted() {
			Compile(new[] { "class C { public void M() {} }" });
			var m = FindInstanceMethod("C.M");
			m.Definition.Should().NotBeNull();
		}

		[Test]
		public void InterfaceMethodHasNullDefinition() {
			Compile(new[] { "interface I { void M(); }" });
			var m = FindInstanceMethod("I.M");
			m.Definition.Should().BeNull();
		}

		[Test]
		public void SimpleStaticMethodCanBeConverted() {
			Compile(new[] { "class C { public static void M() {} }" });
			var m = FindStaticMethod("C.M");
			m.Definition.Should().NotBeNull();
		}

		[Test]
		public void MethodImplementedAsInlineCodeDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = method => MethodScriptSemantics.InlineCode("X") };
			Compile(new[] { "class C { public static void M() {} }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
		}

		[Test]
		public void MethodImplementedAsInlineCodeWithGeneratedMethodNameDoesAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = method => MethodScriptSemantics.InlineCode("X", generatedMethodName: "someMethod") };
			Compile(new[] { "class C { public static void M() {} }" }, metadataImporter: metadataImporter);
			var m = FindInstanceMethod("C.someMethod");
			m.Definition.Should().NotBeNull();
		}

		[Test]
		public void MethodImplementedAsNotUsableFromScriptDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = method => MethodScriptSemantics.NotUsableFromScript() };
			Compile(new[] { "class C { public static void M() {} }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
		}

		[Test]
		public void InstanceMethodWithGenerateCodeSetToFalseDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = method => MethodScriptSemantics.NormalMethod("X", generateCode: false) };
			Compile(new[] { "class C { public static void M() {} }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
		}

		[Test]
		public void StaticMethodWithGenerateCodeSetToFalseDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = method => MethodScriptSemantics.NormalMethod("X", generateCode: false) };
			Compile(new[] { "class C { public static void M() {} }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
		}

		[Test]
		public void StaticMethodWithThisAsFirstArgumentAppearsOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = method => MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("X") };
			Compile(new[] { "class C { public static void M() {} }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindStaticMethod("C.X").Should().NotBeNull();
		}

		[Test]
		public void BaseMethodsAreNotIncludedInDerivedType() {
			Compile(new[] { "class B { public void X(); } class C : B { public void Y() {} }" });
			var cls = FindClass("C");
			cls.InstanceMethods.Should().HaveCount(1);
			cls.InstanceMethods[0].Name.Should().Be("Y");
		}

		[Test]
		public void ShadowingMethodsAreIncluded() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.DeclaringType.Name == "C" ? "XDerived" : m.Name) };
			Compile(new[] { "class B { public void X(); } class C : B { public new void X() {} }" }, metadataImporter: metadataImporter);
			var cls = FindClass("C");
			cls.InstanceMethods.Should().HaveCount(1);
			cls.InstanceMethods[0].Name.Should().Be("XDerived");
		}

		[Test]
		public void OverridingMethodsAreIncluded() {
			Compile(new[] { "class B { public virtual void X(); } class C : B { public override void X() {} }" });
			var cls = FindClass("C");
			cls.InstanceMethods.Should().HaveCount(1);
			cls.InstanceMethods[0].Name.Should().Be("X");
		}

		[Test]
		public void OperatorsWork() {
			Compile(new[] { "class C { public static bool operator==(C a, C b) {} }" });
			FindStaticMethod("C.op_Equality").Should().NotBeNull();
		}

		[Test]
		public void PartialMethodWithoutDefinitionIsNotImported() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = m => { throw new InvalidOperationException(); } };
			Compile(new[] { "partial class C { private partial void M(); }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void OverloadedPartialMethodsWork() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$M_" + m.Parameters.Count) };
			Compile(new[] { "partial class C { partial void M(); partial void M(int i); }", "partial class C { partial void M(int i) {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.$M_0"), Is.Null);
			Assert.That(FindInstanceMethod("C.$M_1"), Is.Not.Null);
		}

		[Test]
		public void PartialMethodWithDeclarationAndDefinitionIsImported() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$M") };
			Compile(new[] { "partial class C { partial void M(); }", "partial class C { partial void M() {} }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.$M").Should().NotBeNull();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void GenericMethodTypeArgumentsAreIncludedForInstanceMethods() {
			var namer = new MockNamer { GetTypeParameterName = tp => "$$" + tp.Name };
			Compile(new[] { "class C { public void X<U, V>() {} }" }, namer: namer);
			FindInstanceMethod("C.X").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
		}

		[Test]
		public void GenericMethodTypeArgumentsAreIgnoredForInstanceMethodsIfTheMethodImplOptionsSaySo() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("X", ignoreGenericArguments: true) };
			var namer = new MockNamer { GetTypeParameterName = tp => "$$" + tp.Name };
			Compile(new[] { "class C { public void X<U, V>() {} }" }, metadataImporter: metadataImporter, namer: namer);
			FindInstanceMethod("C.X").TypeParameterNames.Should().BeEmpty();
		}

		[Test]
		public void GenericMethodTypeArgumentsAreIncludedForStaticMethods() {
			var namer = new MockNamer { GetTypeParameterName = tp => "$$" + tp.Name };
			Compile(new[] { "class C { public static void X<U, V>() {} }" }, namer: namer);
			FindStaticMethod("C.X").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
		}

		[Test]
		public void GenericMethodTypeArgumentsAreIgnoredForStaticMethodsIfTheMethodImplOptionsSaySo() {
			var metadataImporter = new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("X", ignoreGenericArguments: true) };
			var namer = new MockNamer { GetTypeParameterName = tp => "$$" + tp.Name };
			Compile(new[] { "class C { public static void X<U, V>() {} }" }, metadataImporter: metadataImporter, namer: namer);
			FindStaticMethod("C.X").TypeParameterNames.Should().BeEmpty();
		}

		[Test]
		public void AbstractMethodHasANullDefinition() {
			Compile(new[] { "abstract class C { public abstract void M(); }" });
			var m = FindInstanceMethod("C.M");
			m.Definition.Should().BeNull();
		}
	}
}
