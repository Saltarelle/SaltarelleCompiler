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
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.InstanceMethod("X") };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            var m = FindInstanceMethod("C.X");
            m.Should().NotBeNull();

            namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.StaticMethod("Y") };
            Compile(new[] { "class C { public void M() {}" }, namingConvention: namingConvention);
            m = FindStaticMethod("C.Y");
            m.Should().NotBeNull();
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
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.InstanceMethod("X", generateCode: false) };
            Compile(new[] { "class C { public static void M() {}" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void StaticMethodWithGenerateCodeSetToFalseDoesNotAppearOnTheType() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = method => MethodImplOptions.StaticMethod("X", generateCode: false) };
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

        [Test]
        public void BaseMethodsAreNotIncludedInDerivedType() {
            Compile(new[] { "class B { public void X(); } class C : B { public void Y() {} }" });
            var cls = FindClass("C");
            cls.InstanceMethods.Should().HaveCount(1);
            cls.InstanceMethods[0].Name.Should().Be("Y");
        }

        [Test]
        public void ShadowingMethodsAreIncluded() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.InstanceMethod(m.DeclaringType.Name == "C" ? "XDerived" : m.Name) };
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
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.InstanceMethod("X", additionalNames: new[] { "X1", "X2" } ) };
            Compile(new[] { "class C { public void X() {} }" }, namingConvention: namingConvention);
            var cls = FindClass("C");
            cls.InstanceMethods.Should().HaveCount(3);
            cls.InstanceMethods.Should().Contain(m => m.Name == "X");
            cls.InstanceMethods.Should().Contain(m => m.Name == "X1");
            cls.InstanceMethods.Should().Contain(m => m.Name == "X2");
        }

        [Test]
        public void AdditionalNamesWorkForStaticMethods() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.StaticMethod("X", additionalNames: new[] { "X1", "X2" } ) };
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

        [Test]
        public void GenericMethodTypeArgumentsAreIncludedForInstanceMethods() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.InstanceMethod("X", additionalNames: new[] { "X1", "X2" }), GetTypeParameterName = tp => "$$" + tp.Name };
            Compile(new[] { "class C { public void X<U, V>() {} }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.X").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
            FindInstanceMethod("C.X1").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
            FindInstanceMethod("C.X2").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
        }

        [Test]
        public void GenericMethodTypeArgumentsAreIgnoredForInstanceMethodsIfTheMethodImplOptionsSaySo() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.InstanceMethod("X", additionalNames: new[] { "X1", "X2" }, ignoreGenericArguments: true), GetTypeParameterName = tp => "$$" + tp.Name };
            Compile(new[] { "class C { public void X<U, V>() {} }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.X").TypeParameterNames.Should().BeEmpty();
            FindInstanceMethod("C.X1").TypeParameterNames.Should().BeEmpty();
            FindInstanceMethod("C.X2").TypeParameterNames.Should().BeEmpty();
        }

        [Test]
        public void GenericMethodTypeArgumentsAreIncludedForStaticMethods() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.StaticMethod("X", additionalNames: new[] { "X1", "X2" }), GetTypeParameterName = tp => "$$" + tp.Name };
            Compile(new[] { "class C { public void X<U, V>() {} }" }, namingConvention: namingConvention);
            FindStaticMethod("C.X").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
            FindStaticMethod("C.X1").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
            FindStaticMethod("C.X2").TypeParameterNames.Should().Equal(new[] { "$$U", "$$V" });
        }

        [Test]
        public void GenericMethodTypeArgumentsAreIgnoredForStaticMethodsIfTheMethodImplOptionsSaySo() {
            var namingConvention = new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.StaticMethod("X", additionalNames: new[] { "X1", "X2" }, ignoreGenericArguments: true), GetTypeParameterName = tp => "$$" + tp.Name };
            Compile(new[] { "class C { public void X<U, V>() {} }" }, namingConvention: namingConvention);
            FindStaticMethod("C.X").TypeParameterNames.Should().BeEmpty();
            FindStaticMethod("C.X1").TypeParameterNames.Should().BeEmpty();
            FindStaticMethod("C.X2").TypeParameterNames.Should().BeEmpty();
        }

        [Test]
        public void InstanceAutoPropertiesWithGetSetMethodsAndFieldAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InstanceMethod("get_" + p.Name), MethodImplOptions.InstanceMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldOptions.Instance("$" + p.Name)
                                                                    };

            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void InstanceAutoPropertiesWithGetSetMethodsButNoFieldAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InstanceMethod("get_" + p.Name), MethodImplOptions.InstanceMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldOptions.NotUsableFromScript()
                                                                    };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").InstanceFields.Should().BeEmpty();
        }

        [Test]
        public void InstanceAutoPropertiesWithGetSetMethodsWithNoCodeAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InstanceMethod("get_" + p.Name, generateCode: false), MethodImplOptions.InstanceMethod("set_" + p.Name, generateCode: false)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldOptions.NotUsableFromScript()
                                                                    };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").InstanceFields.Should().BeEmpty();
        }

        [Test]
        public void InstanceAutoPropertiesWithGetSetMethodsStaticAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.StaticMethod("get_" + p.Name), MethodImplOptions.StaticMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldOptions.NotUsableFromScript()
                                                                    };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);

            Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Null);
            Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Null);
            Assert.That(FindStaticMethod("C.get_SomeProp"), Is.Not.Null);
            Assert.That(FindStaticMethod("C.set_SomeProp"), Is.Not.Null);
            Assert.That(FindClass("C").InstanceFields, Is.Empty);
            Assert.That(FindClass("C").StaticFields, Is.Empty);
#warning Determine why code below does not work.
#if FALSE
            FindInstanceMethod("C.get_SomeProp").Should().BeNull();
            FindInstanceMethod("C.set_SomeProp").Should().BeNull();
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").InstanceFields.Should().BeEmpty();
            FindClass("C").StaticFields.Should().BeEmpty();
#endif
        }

        [Test]
        public void InstanceAutoPropertiesWithGetSetMethodsStaticWithNoCodeAreCorrectlyImported()
        {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.StaticMethod("get_" + p.Name, generateCode: false), MethodImplOptions.StaticMethod("set_" + p.Name, generateCode: false)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldOptions.NotUsableFromScript()
            };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);

            Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Null);
            Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Null);
            Assert.That(FindStaticMethod("C.get_SomeProp"), Is.Null);
            Assert.That(FindStaticMethod("C.set_SomeProp"), Is.Null);
            Assert.That(FindClass("C").InstanceFields, Is.Empty);
            Assert.That(FindClass("C").StaticFields, Is.Empty);
#warning Determine why code below does not work.
#if FALSE
            FindInstanceMethod("C.get_SomeProp").Should().BeNull();
            FindInstanceMethod("C.set_SomeProp").Should().BeNull();
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").InstanceFields.Should().BeEmpty();
            FindClass("C").StaticFields.Should().BeEmpty();
#endif
        }

        [Test]
        public void InstanceAutoPropertiesWithInstanceFieldAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InstanceMethod("get_" + p.Name), MethodImplOptions.InstanceMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldOptions.Instance("$" + p.Name)
                                                                    };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").StaticFields.Should().BeEmpty();
        }


        [Test]
        public void InstanceAutoPropertiesWithStaticFieldAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InstanceMethod("get_" + p.Name), MethodImplOptions.InstanceMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldOptions.Static("$" + p.Name)
                                                                    };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").InstanceFields.Should().BeEmpty();
            FindStaticField("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void InstanceAutoPropertiesThatShouldBeInstanceFieldsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.InstanceField("$" + p.Name) };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void InstanceAutoPropertiesThatShouldBeStaticFieldsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.StaticField("$" + p.Name) };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
            FindStaticField("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void StaticAutoPropertiesWithGetSetMethodsAndFieldAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.StaticMethod("get_" + p.Name), MethodImplOptions.StaticMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldOptions.Instance("$" + p.Name)
                                                                    };

            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void StaticAutoPropertiesWithGetSetMethodsButNoFieldAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.StaticMethod("get_" + p.Name), MethodImplOptions.StaticMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldOptions.NotUsableFromScript()
                                                                    };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").InstanceFields.Should().BeEmpty();
        }

        [Test]
        public void StaticAutoPropertiesThatShouldBeFieldsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.StaticField("$" + p.Name) };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
            FindStaticField("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void ManuallyImplementedInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InstanceMethod("get_SomeProp"), MethodImplOptions.InstanceMethod("set_SomeProp")) };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.InstanceField("$SomeProp") };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedReadOnlyInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InstanceMethod("get_SomeProp"), null) };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().BeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedReadOnlyInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.InstanceField("$SomeProp") };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } }" }, namingConvention: namingConvention);
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedWriteOnlyInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(null, MethodImplOptions.InstanceMethod("set_SomeProp")) };
            Compile(new[] { "class C { public int SomeProp { set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().BeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedWriteOnlyInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.InstanceField("$SomeProp") };
            Compile(new[] { "class C { public int SomeProp { set {} }" }, namingConvention: namingConvention);
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.StaticMethod("get_SomeProp"), MethodImplOptions.StaticMethod("set_SomeProp")) };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.StaticField("$SomeProp") };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindStaticField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedReadOnlyStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.StaticMethod("get_SomeProp"), null) };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().BeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedReadOnlyStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.StaticField("$SomeProp") };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } }" }, namingConvention: namingConvention);
            FindStaticField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedWriteOnlyStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(null, MethodImplOptions.StaticMethod("set_SomeProp")) };
            Compile(new[] { "class C { public int SomeProp { set {} } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().BeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedWriteOnlyStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.StaticField("$SomeProp") };
            Compile(new[] { "class C { public int SomeProp { set {} }" }, namingConvention: namingConvention);
            FindStaticField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ClassFieldsAreCorrectlyImported() {
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
        public void EnumValuesAreCorrectlyImported() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void IndexersAreCorrectlyImported() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void ParameterNamesAreCorrect() {
            Assert.Inconclusive("TODO, for all kinds of methods");
        }

        [Test]
        public void PartialMethodsWork() {
            Assert.Inconclusive("TODO, both with and without definition");
        }
    }
}
