using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MemberConversionTests {
    [TestFixture]
    public class ConstructorConversionTests : CompilerTestBase {
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
        public void DefaultConstructorImplementedAsStaticMethodWorks() {
            var namingConvention = new MockNamingConventionResolver { GetConstructorImplementation = ctor => ConstructorImplOptions.StaticMethod("X") };
            Compile(new[] { "class C { }" }, namingConvention: namingConvention);
            FindStaticMethod("C.X").Should().NotBeNull();
            FindConstructor("C.X").Should().BeNull();
        }

        [Test]
        public void DefaultConstructorIsNotInsertedIfOtherConstructorIsDefined() {
            Compile(new[] { "class C { C(int i) {} }" });
            var cls = FindClass("C");
            cls.Constructors.Should().HaveCount(1);
            cls.Constructors[0].Name.Should().Be("ctor$Int32");
            cls.Constructors[0].Definition.Should().NotBeNull();
        }

        [Test]
        public void ConstructorsCanBeOverloadedWithDifferentImplementations() {
            var namingConvention = new MockNamingConventionResolver { GetConstructorImplementation = ctor => ctor.Parameters[0].Type.Name == "String" ? ConstructorImplOptions.Named("StringCtor") : ConstructorImplOptions.StaticMethod("IntCtor") };
            Compile(new[] { "class C { C(int i) {} C(string s) {} }" }, namingConvention: namingConvention);
            FindClass("C").Constructors.Should().HaveCount(1);
            FindClass("C").StaticMethods.Should().HaveCount(1);
            FindConstructor("C.StringCtor").Should().NotBeNull();
            FindStaticMethod("C.IntCtor").Should().NotBeNull();
        }

        [Test]
        public void ConstructorImplementedAsStaticMethodGetsAddedToTheStaticMethodsCollectionAndNotTheConstructors() {
            var namingConvention = new MockNamingConventionResolver { GetConstructorImplementation = ctor => ConstructorImplOptions.StaticMethod("X") };
            Compile(new[] { "class C { public C() {}" }, namingConvention: namingConvention);
            FindStaticMethod("C.X").Should().NotBeNull();
            FindConstructor("C.X").Should().BeNull();
        }

        [Test]
        public void ConstructorImplementedAsNotUsableFromScriptDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetConstructorImplementation = ctor => ConstructorImplOptions.NotUsableFromScript() };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            var m = FindInstanceMethod("C.X");
            m.Should().BeNull();
        }
    }
}
