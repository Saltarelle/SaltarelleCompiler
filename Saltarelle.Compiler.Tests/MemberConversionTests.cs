using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using ICSharpCode.NRefactory.TypeSystem;
using Moq;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Tests {
    [TestFixture]
    public class MemberConversionTests : CompilerTestBase {
        [Test]
        public void SimpleInstanceMethodCanBeConverted() {
            Compile(new[] { "class C { public void M() {} }" });
            var m = FindInstanceMethod("C.M");
            m.Definition.Should().NotBeNull();
        }

        [Test]
        public void SimpleStaticMethodCanBeConverted() {
            Compile(new[] { "class C { public static void M() {} }" });
            var m = FindStaticMethod("C.M");
            m.Definition.Should().NotBeNull();
        }

        [Test]
        public void NamingConventionIsUsedToDetermineMethodNameAndStaticity() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = (_, method) => MethodImplOptions.InstanceMethod("X") };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            var m = FindInstanceMethod("C.X");
            m.Should().NotBeNull();

            namingConvention = new MockNamingConventionResolver { GetMethodImplementation = (_, method) => MethodImplOptions.StaticMethod("Y") };
            Compile(new[] { "class C { public void M() {}" }, namingConvention: namingConvention);
            m = FindStaticMethod("C.Y");
            m.Should().NotBeNull();
        }

        [Test]
        public void MethodImplementedAsInlineCodeDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = (_, method) => MethodImplOptions.InlineCode("X") };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void MethodImplementedAsInstanceMethodOnFirstArgumentDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = (_, method) => MethodImplOptions.InstanceMethodOnFirstArgument("X") };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void MethodImplementedAsNotUsableFromScriptDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = (_, method) => MethodImplOptions.NotUsableFromScript() };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void InstanceMethodWithGenerateCodeSetToFalseDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = (_, method) => MethodImplOptions.InstanceMethod("X", generateCode: false) };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void StaticMethodWithGenerateCodeSetToFalseDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = (_, method) => MethodImplOptions.StaticMethod("X", generateCode: false) };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void DefaultConstructorIsInsertedIfNoConstructorIsDefined() {
            Compile(new[] { "class C {}" });
            var cls = FindClass("C");
            cls.Constructors.Should().HaveCount(1);
            cls.Constructors[0].Name.Should().BeNull();
            cls.Constructors[0].Definition.Should().NotBeNull();
            cls.Constructors[0].Definition.ParameterNames.Should().HaveCount(0);
        }

        [Test]
        public void DefaultConstructorIsNotInsertedIfOtherConstructorIsDefined() {
            Compile(new[] { "class C { C(int i) {} }" });
            var cls = FindClass("C");
            cls.Constructors.Should().HaveCount(1);
            cls.Constructors[0].Name.Should().Be("ctor$Int32");
            cls.Constructors[0].Definition.Should().NotBeNull();
            cls.Constructors[0].Definition.ParameterNames.Should().HaveCount(1);
        }

        [Test]
        public void ConstructorsCanBeOverloadedWithDifferentImplementations() {
            var namingConvention = new MockNamingConventionResolver { GetConstructorImplementation = (_, ctor) => ctor.Parameters[0].Type.Equals(KnownTypeReference.String) ? ConstructorImplOptions.Named("StringCtor") : ConstructorImplOptions.StaticMethod("IntCtor") };
            Compile(new[] { "class C { C(int i) {} C(string s) {} }" }, namingConvention: namingConvention);
            FindClass("C").Constructors.Should().HaveCount(1);
            FindClass("C").StaticMethods.Should().HaveCount(1);
            FindConstructor("C.StringCtor").Should().NotBeNull();
            FindStaticMethod("C.IntCtor").Should().NotBeNull();
        }

        [Test]
        public void ConstructorImplementedAsStaticMethodGetsAddedToTheStaticMethodsCollectionAndNotTheConstructors() {
            var namingConvention = new MockNamingConventionResolver { GetConstructorImplementation = (_, ctor) => ConstructorImplOptions.StaticMethod("X") };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            FindStaticMethod("C.X").Should().NotBeNull();
            FindConstructor("C.X").Should().BeNull();
        }

        [Test]
        public void ConstructorImplementedAsNotUsableFromScriptDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetConstructorImplementation = (_, ctor) => ConstructorImplOptions.NotUsableFromScript() };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            var m = FindInstanceMethod("C.X");
            m.Should().BeNull();
        }

        [Test]
        public void BaseMethodsAreNotIncludedInDerivedType() {
            Compile(new[] { "class B { public void X(); } class C : B { public void Y() {} }" });
            var cls = FindClass("C");
            cls.InstanceMethods.Should().HaveCount(1);
            cls.InstanceMethods[0].Name.Should().Be("Y");
        }

        [Test]
        public void OverridingMethodsGetTheirNameFromTheBase() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = (_, m) => MethodImplOptions.InstanceMethod(m.DeclaringType.Name == "C" ? "XDerived" : m.Name) };
            Compile(new[] { "class B { public virtual void X(); } class C : B { public override void X() {} }" }, namingConvention: namingConvention);
            var cls = FindClass("C");
            cls.InstanceMethods.Should().HaveCount(1);
            cls.InstanceMethods[0].Name.Should().Be("X");
        }

        [Test]
        public void ShadowingMethodsAreIncluded() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = (_, m) => MethodImplOptions.InstanceMethod(m.DeclaringType.Name == "C" ? "XDerived" : m.Name) };
            Compile(new[] { "class B { public void X(); } class C : B { public new void X() {} }" }, namingConvention: namingConvention);
            var cls = FindClass("C");
            cls.InstanceMethods.Should().HaveCount(1);
            cls.InstanceMethods[0].Name.Should().Be("XDerived");
        }

        [Test]
        public void GenericMethodTypeArgumentsAreIncluded() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void GenericMethodTypeArgumentsAreIgnoredIfTheMethodImplOptionsSaySo() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void ImplicitlyImplementedEventsAreCorrectlyImported() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void ExplicitlyImplementedEventsAreCorrectlyImported() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void ClassFieldsAreCorrectlyImported() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void EnumValuesAreCorrectlyImported() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void ImplicitlyImplementedPropertiesAreCorrectlyImported() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void ExplicitlyImplementedPropertiesAreCorrectlyImported() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void IndexersAreCorrectlyImported() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void OverridingIndexersGetTheirNameFromTherDefiningMethod() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void ExplicitInterfaceImplementationWorks() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void MethodCanImplementMoreThanOneInterfaceMethod() {
            Assert.Inconclusive("TODO");
        }
    }
}
