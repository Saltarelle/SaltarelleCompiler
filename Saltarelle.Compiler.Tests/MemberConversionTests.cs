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
            var m = FindInstanceMember("C.M");
            m.Initializer.Should().BeOfType<JsFunctionDefinitionExpression>();
        }

        [Test]
        public void SimpleStaticMethodCanBeConverted() {
            Compile(new[] { "class C { public static void M() {} }" });
            var m = FindStaticMember("C.M");
            m.Initializer.Should().BeOfType<JsFunctionDefinitionExpression>();
        }

        [Test]
        public void NamingConventionIsUsedToDetermineWhetherMethodNameAndStaticity() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.InstanceMethod("X") };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            var m = FindInstanceMember("C.X");
            m.Should().NotBeNull();

            namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.StaticMethod("Y") };
            Compile(new[] { "class C { public void M() {}" }, namingConvention: namingConvention);
            m = FindInstanceMember("C.Y");
            m.Should().NotBeNull();
        }

        [Test]
        public void MethodImplementedAsInlineCodeDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.InlineCode("X") };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            var m = FindInstanceMember("C.X");
            m.Should().BeNull();
        }

        [Test]
        public void MethodImplementedAsInstanceMethodOnFirstArgumentDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.InstanceMethodOnFirstArgument("X") };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            var m = FindInstanceMember("C.X");
            m.Should().BeNull();
        }

        [Test]
        public void DefaultConstructorIsInsertedIfNoConstructorIsDefined() {
            Compile(new[] { "class C {}" });
            var cls = FindClass("C");
            cls.Constructors.Should().HaveCount(1);
            cls.Constructors[0].Name.Should().BeNull();
            cls.Constructors[0].Initializer.Should().BeOfType<JsFunctionDefinitionExpression>();
            ((JsFunctionDefinitionExpression)cls.Constructors[0].Initializer).ParameterNames.Should().HaveCount(0);
        }

        [Test]
        public void DefaultConstructorIsNotInsertedIfOtherConstructorIsDefined() {
            Compile(new[] { "class C { C(int i) {} }" });
            var cls = FindClass("C");
            cls.Constructors.Should().HaveCount(1);
            cls.Constructors[0].Name.Should().BeNull();
            cls.Constructors[0].Initializer.Should().BeOfType<JsFunctionDefinitionExpression>();
            ((JsFunctionDefinitionExpression)cls.Constructors[0].Initializer).ParameterNames.Should().HaveCount(1);
        }

        [Test]
        public void ConstructorsCanBeOverloaded() {
            Compile(new[] { "class C { C(int i) {} C(string s) {} }" });
            Assert.Inconclusive("Implement");
        }

        [Test]
        public void BaseMethodsAreNotIncludedInDerivedType() {
            Compile(new[] { "class B { public void X(); } class C : B { public void Y() {} }" });
            var cls = FindClass("C");
            cls.InstanceMembers.Should().HaveCount(1);
            cls.InstanceMembers[0].Name.Should().Be("Y");
        }

        [Test]
        public void OverridingMethodsGetTheirNameFromTheBase() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.InstanceMethod(m.DeclaringType.Name == "C" ? "XDerived" : m.Name) };
            Compile(new[] { "class B { public virtual void X(); } class C : B { public override void X() {} }" });
            var cls = FindClass("C");
            cls.InstanceMembers.Should().HaveCount(1);
            cls.InstanceMembers[0].Name.Should().Be("X");
        }

        [Test]
        public void ShadowingMethodsAreIncluded() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.InstanceMethod(m.DeclaringType.Name == "C" ? "XDerived" : m.Name) };
            Compile(new[] { "class B { public void X(); } class C : B { public new void X() {} }" });
            var cls = FindClass("C");
            cls.InstanceMembers.Should().HaveCount(1);
            cls.InstanceMembers[0].Name.Should().Be("XDerived");
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
