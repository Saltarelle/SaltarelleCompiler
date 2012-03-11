using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MemberConversionTests {
    [TestFixture]
    public class PropertyConversionTests : CompilerTestBase {
        [Test]
        public void InstanceAutoPropertiesWithGetSetMethodsAndFieldAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.NormalMethod("get_" + p.Name), MethodImplOptions.NormalMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldImplOptions.Field("$" + p.Name)
                                                                    };

            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void InstanceAutoPropertiesWithGetSetMethodsWithNoCodeAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.NormalMethod("get_" + p.Name, generateCode: false), MethodImplOptions.NormalMethod("set_" + p.Name, generateCode: false)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldImplOptions.NotUsableFromScript()
            };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").InstanceFields.Should().BeEmpty();
        }

        [Test]
        public void InstanceAutoPropertiesWithGetSetMethodsStaticWithNoCodeAreCorrectlyImported()
        {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.NormalMethod("get_" + p.Name, generateCode: false), MethodImplOptions.NormalMethod("set_" + p.Name, generateCode: false)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldImplOptions.NotUsableFromScript()
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
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.NormalMethod("get_" + p.Name), MethodImplOptions.NormalMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldImplOptions.Field("$" + p.Name)
                                                                    };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").StaticFields.Should().BeEmpty();
        }

        [Test]
        public void InstanceAutoPropertiesThatShouldBeInstanceFieldsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.Field("$" + p.Name) };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
        }

        [Test, Ignore("NRefactory bug")]
        public void StaticAutoPropertiesWithGetSetMethodsAndFieldAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.NormalMethod("get_" + p.Name), MethodImplOptions.NormalMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldImplOptions.Field("$" + p.Name)
                                                                    };

            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
        }

        [Test, Ignore("NRefactory bug")]
        public void StaticAutoPropertiesWithGetSetMethodsButNoFieldAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.NormalMethod("get_" + p.Name), MethodImplOptions.NormalMethod("set_" + p.Name)),
                                                                      GetAutoPropertyBackingFieldImplementation = p => FieldImplOptions.NotUsableFromScript()
                                                                    };
            Compile(new[] { "class C { public string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").InstanceFields.Should().BeEmpty();
        }

        [Test]
        public void StaticAutoPropertiesThatShouldBeFieldsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.Field("$" + p.Name) };
            Compile(new[] { "class C { public static string SomeProp { get; set; } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
            FindStaticField("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void AutoPropertyBackingFieldIsCorrectlyInitialized() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void ManuallyImplementedInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.NormalMethod("get_SomeProp"), MethodImplOptions.NormalMethod("set_SomeProp")) };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.Field("$SomeProp") };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedReadOnlyInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.NormalMethod("get_SomeProp"), null) };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().BeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedReadOnlyInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.Field("$SomeProp") };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } }" }, namingConvention: namingConvention);
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedWriteOnlyInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(null, MethodImplOptions.NormalMethod("set_SomeProp")) };
            Compile(new[] { "class C { public int SomeProp { set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().BeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedWriteOnlyInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.Field("$SomeProp") };
            Compile(new[] { "class C { public int SomeProp { set {} }" }, namingConvention: namingConvention);
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test, Ignore("NRefactory bug")]
        public void ManuallyImplementedStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.NormalMethod("get_SomeProp"), MethodImplOptions.NormalMethod("set_SomeProp")) };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.Field("$SomeProp") };
            Compile(new[] { "class C { public static int SomeProp { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindStaticField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test, Ignore("NRefactory bug")]
        public void ManuallyImplementedReadOnlyStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.NormalMethod("get_SomeProp"), null) };
            Compile(new[] { "class C { public int SomeProp { get { return 0; } } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().BeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedReadOnlyStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.Field("$SomeProp") };
            Compile(new[] { "class C { public static int SomeProp { get { return 0; } }" }, namingConvention: namingConvention);
            FindStaticField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test, Ignore("NRefactory bug")]
        public void ManuallyImplementedWriteOnlyStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(null, MethodImplOptions.NormalMethod("set_SomeProp")) };
            Compile(new[] { "class C { public static int SomeProp { set {} } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.get_SomeProp").Should().BeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
        }

        [Test]
        public void ManuallyImplementedWriteOnlyStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.Field("$SomeProp") };
            Compile(new[] { "class C { public static int SomeProp { set {} }" }, namingConvention: namingConvention);
            FindStaticField("C.$SomeProp").Should().NotBeNull();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }
    }
}
