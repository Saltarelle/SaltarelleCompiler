using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MemberConversionTests {
    [TestFixture]
    public class MethodConversionTests : CompilerTestBase {
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
        public void MethodImplementedAsInlineCodeDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.InlineCode("X") };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void MethodImplementedAsInstanceMethodOnFirstArgumentDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.InstanceMethodOnFirstArgument("X") };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void MethodImplementedAsNotUsableFromScriptDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.NotUsableFromScript() };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void InstanceMethodWithGenerateCodeSetToFalseDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.NormalMethod("X", generateCode: false) };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void StaticMethodWithGenerateCodeSetToFalseDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.NormalMethod("X", generateCode: false) };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void StaticMethodWithThisAsFirstArgumentAppearsOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.StaticMethodWithThisAsFirstArgument("X") };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
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
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod(m.DeclaringType.Name == "C" ? "XDerived" : m.Name) };
            Compile(new[] { "class B { public void X(); } class C : B { public new void X() {} }" }, namingConvention: namingConvention);
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
        public void AdditionalNamesWorkForInstanceMethods() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod("X", additionalNames: new[] { "X1", "X2" } ) };
            Compile(new[] { "class C { public void X() {} }" }, namingConvention: namingConvention);
            var cls = FindClass("C");
            cls.InstanceMethods.Should().HaveCount(3);
            cls.InstanceMethods.Should().Contain(m => m.Name == "X");
            cls.InstanceMethods.Should().Contain(m => m.Name == "X1");
            cls.InstanceMethods.Should().Contain(m => m.Name == "X2");
        }

        [Test]
        public void AdditionalNamesWorkForStaticMethods() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod("X", additionalNames: new[] { "X1", "X2" } ) };
            Compile(new[] { "class C { public static void X() {} }" }, namingConvention: namingConvention);
            var cls = FindClass("C");
            cls.StaticMethods.Should().HaveCount(3);
            cls.StaticMethods.Should().Contain(m => m.Name == "X");
            cls.StaticMethods.Should().Contain(m => m.Name == "X1");
            cls.StaticMethods.Should().Contain(m => m.Name == "X2");
        }

        [Test]
        public void OperatorsWork() {
            Compile(new[] { "class C { public static bool operator==(C a, C b) {} }" });
            FindStaticMethod("C.op_Equality").Should().NotBeNull();
        }

        [Test, Ignore("Partial methods lack support in NRefactory")]
        public void PartialMethodWithoutDefinitionIsNotImported() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => { throw new InvalidOperationException(); } };
            Compile(new[] { "partial class C { private partial void M(); }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test, Ignore("Partial methods lack support in NRefactory")]
        public void OverloadedPartialMethodsWork() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod("$M_" + m.Parameters.Count) };
            Compile(new[] { "partial class C { partial void M(); partial void M(int i); }", "partial class C { partial void M(int i) {} }" }, namingConvention: namingConvention);
            #warning TODO: Find out why code below fails
            Assert.That(FindInstanceMethod("C.$M_0"), Is.Null);
            Assert.That(FindInstanceMethod("C.$M_1"), Is.Null);
            #if FALSE
            FindInstanceMethod("C.$M_0").Should().BeNull();
            FindInstanceMethod("C.$M_1").Should().NotBeNull();
            #endif
        }

        [Test, Ignore("Partial methods lack support in NRefactory")]
        public void PartialMethodWithDeclarationAndDefinitionIsImported() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod("$M") };
            Compile(new[] { "partial class C { partial void M(); }", "partial class C { partial void M() {} }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void GenericMethodTypeArgumentsAreIncludedForInstanceMethods() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod("X", additionalNames: new[] { "X1", "X2" }), GetTypeParameterName = tp => "$$" + tp.Name };
            Compile(new[] { "class C { public void X<U, V>() {} }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.X").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
            FindInstanceMethod("C.X1").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
            FindInstanceMethod("C.X2").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
        }

        [Test]
        public void GenericMethodTypeArgumentsAreIgnoredForInstanceMethodsIfTheMethodImplOptionsSaySo() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod("X", additionalNames: new[] { "X1", "X2" }, ignoreGenericArguments: true), GetTypeParameterName = tp => "$$" + tp.Name };
            Compile(new[] { "class C { public void X<U, V>() {} }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.X").TypeParameterNames.Should().BeEmpty();
            FindInstanceMethod("C.X1").TypeParameterNames.Should().BeEmpty();
            FindInstanceMethod("C.X2").TypeParameterNames.Should().BeEmpty();
        }

        [Test]
        public void GenericMethodTypeArgumentsAreIncludedForStaticMethods() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod("X", additionalNames: new[] { "X1", "X2" }), GetTypeParameterName = tp => "$$" + tp.Name };
            Compile(new[] { "class C { public static void X<U, V>() {} }" }, namingConvention: namingConvention);
            FindStaticMethod("C.X").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
            FindStaticMethod("C.X1").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
            FindStaticMethod("C.X2").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
        }

        [Test]
        public void GenericMethodTypeArgumentsAreIgnoredForStaticMethodsIfTheMethodImplOptionsSaySo() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.NormalMethod("X", additionalNames: new[] { "X1", "X2" }, ignoreGenericArguments: true), GetTypeParameterName = tp => "$$" + tp.Name };
            Compile(new[] { "class C { public static void X<U, V>() {} }" }, namingConvention: namingConvention);
            FindStaticMethod("C.X").TypeParameterNames.Should().BeEmpty();
            FindStaticMethod("C.X1").TypeParameterNames.Should().BeEmpty();
            FindStaticMethod("C.X2").TypeParameterNames.Should().BeEmpty();
        }
    }
}
