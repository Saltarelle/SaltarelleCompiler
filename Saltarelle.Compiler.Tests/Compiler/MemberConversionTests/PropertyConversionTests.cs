using System;
using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.Compiler.MemberConversionTests {
    [TestFixture]
    public class PropertyConversionTests : CompilerTestBase {
        [Test]
        public void InstanceAutoPropertiesWithGetSetMethodsAndFieldAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldName = p => "$" + p.Name
                                                                    };

            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void InstanceAutoPropertiesWithGetSetMethodsWithNoCodeAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name, generateCode: false), MethodScriptSemantics.NormalMethod("set_" + p.Name, generateCode: false)),
                                                                      GetAutoPropertyBackingFieldName = p => { throw new InvalidOperationException("Shouldn't be called"); }
            };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
        }

        [Test]
        public void InstanceAutoPropertiesWithGetSetMethodsStaticWithNoCodeAreCorrectlyImported()
        {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("get_" + p.Name, generateCode: false), MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("set_" + p.Name, generateCode: false)),
                                                                      GetAutoPropertyBackingFieldName = p => { throw new InvalidOperationException("Shouldn't be called"); }
            };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);

            Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Null);
            Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Null);
            Assert.That(FindStaticMethod("C.get_SomeProp"), Is.Null);
            Assert.That(FindStaticMethod("C.set_SomeProp"), Is.Null);
            FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
            Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
        }

        [Test]
        public void InstanceAutoPropertiesAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldName = p => "$" + p.Name
                                                                    };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").StaticInitStatements.Should().BeEmpty();
        }

        [Test]
        public void InstanceAutoPropertiesThatShouldBeInstanceFieldsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.Field("$" + p.Name) };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
            FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void StaticAutoPropertiesWithGetSetMethodsAndFieldAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldName = p => "$" + p.Name
                                                                    };

            Compile(new[] { "class C { public static string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void StaticAutoPropertiesThatShouldBeFieldsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.Field("$" + p.Name) };
            Compile(new[] { "class C { public static string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
            FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void InstanceAutoPropertyBackingFieldIsCorrectlyInitialized() {
            Compile(new[] { "class C<T> { public int P1 { get; set; } public string P2 { get; set; } public T P3 { get; set; } }" });
            FindInstanceFieldInitializer("C.$P1").Should().Be("0");
            FindInstanceFieldInitializer("C.$P2").Should().Be("null");
            FindInstanceFieldInitializer("C.$P3").Should().Be("$Default($T)");
        }

        [Test]
        public void StaticAutoPropertyBackingFieldIsCorrectlyInitialized() {
            Compile(new[] { "class C<T> { public static int P1 { get; set; } public static string P2 { get; set; } public static T P3 { get; set; } }" });
            FindStaticFieldInitializer("C.$P1").Should().Be("0");
            FindStaticFieldInitializer("C.$P2").Should().Be("null");
            FindStaticFieldInitializer("C.$P3").Should().Be("$Default($T)");
        }

        [Test]
        public void ManuallyImplementedInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_SomeProp"), MethodScriptSemantics.NormalMethod("set_SomeProp")) };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.Field("$SomeProp") };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedReadOnlyInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_SomeProp"), null) };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().BeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedReadOnlyInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.Field("$SomeProp") };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } } }" }, namingConvention: namingConvention);
            FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedWriteOnlyInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.GetAndSetMethods(null, MethodScriptSemantics.NormalMethod("set_SomeProp")) };
            Compile(new[] { "class C { public int SomeProp { set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().BeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedWriteOnlyInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.Field("$SomeProp") };
            Compile(new[] { "class C { public int SomeProp { set {} } }" }, namingConvention: namingConvention);
            FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_SomeProp"), MethodScriptSemantics.NormalMethod("set_SomeProp")) };
            Compile(new[] { "class C { public static int SomeProp { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.Field("$SomeProp") };
            Compile(new[] { "class C { public static int SomeProp { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedReadOnlyStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_SomeProp"), null) };
            Compile(new[] { "class C { public static int SomeProp { get { return 0; } } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().BeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedReadOnlyStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.Field("$SomeProp") };
            Compile(new[] { "class C { public static int SomeProp { get { return 0; } } }" }, namingConvention: namingConvention);
            FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedWriteOnlyStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.GetAndSetMethods(null, MethodScriptSemantics.NormalMethod("set_SomeProp")) };
            Compile(new[] { "class C { public static int SomeProp { set {} } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().BeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedWriteOnlyStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.Field("$SomeProp") };
            Compile(new[] { "class C { public static int SomeProp { set {} } }" }, namingConvention: namingConvention);
            FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }
    }
}
