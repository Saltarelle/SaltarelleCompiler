using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MemberConversionTests {
    [TestFixture]
    public class EventConversionTests : CompilerTestBase {
        [Test]
        public void InstanceAutoEventsWithAddRemoveMethodsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = f => EventImplOptions.AddAndRemoveMethods(MethodImplOptions.InstanceMethod("add_" + f.Name), MethodImplOptions.InstanceMethod("remove_" + f.Name)),
                                                                      GetAutoEventBackingFieldImplementation = f => FieldImplOptions.Instance("$" + f.Name)
                                                                    };

            Compile(new[] { "class C { public event System.EventHandler SomeProp; }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.add_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.remove_SomeProp").Should().NotBeNull();
            FindInstanceField("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void InstanceAutoEventsWithAddRemoveMethodsWithNoCodeAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = f => EventImplOptions.AddAndRemoveMethods(MethodImplOptions.InstanceMethod("add_" + f.Name, generateCode: false), MethodImplOptions.InstanceMethod("remove_" + f.Name, generateCode: false)),
                                                                      GetAutoEventBackingFieldImplementation = f => FieldImplOptions.NotUsableFromScript()
                                                                    };
            Compile(new[] { "class C { public event System.EventHandler SomeProp; }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").InstanceFields.Should().BeEmpty();
        }

        [Test]
        public void StaticAutoEventsWithAddRemoveMethodsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = f => EventImplOptions.AddAndRemoveMethods(MethodImplOptions.StaticMethod("add_" + f.Name), MethodImplOptions.StaticMethod("remove_" + f.Name)),
                                                                      GetAutoEventBackingFieldImplementation = f => FieldImplOptions.Static("$" + f.Name)
                                                                    };

            Compile(new[] { "class C { public event System.EventHandler SomeProp; }" }, namingConvention: namingConvention);
            FindStaticMethod("C.add_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.remove_SomeProp").Should().NotBeNull();
            FindStaticField("C.$SomeProp").Should().NotBeNull();
        }


        [Test]
        public void InstanceManualEventsWithAddRemoveMethodsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = f => EventImplOptions.AddAndRemoveMethods(MethodImplOptions.InstanceMethod("add_" + f.Name), MethodImplOptions.InstanceMethod("remove_" + f.Name)) };

            Compile(new[] { "class C { public event System.EventHandler SomeProp { add {} remove{} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.add_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.remove_SomeProp").Should().NotBeNull();
        }

        [Test]
        public void InstanceManualEventsWithAddRemoveMethodsWithNoCodeAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = f => EventImplOptions.AddAndRemoveMethods(MethodImplOptions.InstanceMethod("add_" + f.Name, generateCode: false), MethodImplOptions.InstanceMethod("remove_" + f.Name, generateCode: false)) };
            Compile(new[] { "class C { public event System.EventHandler SomeProp { add {} remove{} } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").InstanceFields.Should().BeEmpty();
        }

        [Test]
        public void StaticManualEventsWithAddRemoveMethodsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = f => EventImplOptions.AddAndRemoveMethods(MethodImplOptions.StaticMethod("add_" + f.Name), MethodImplOptions.StaticMethod("remove_" + f.Name)) };

            Compile(new[] { "class C { public event System.EventHandler SomeProp { add {} remove{} } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.add_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.remove_SomeProp").Should().NotBeNull();
        }

        [Test]
        public void ImportingMultipleEventsInTheSameDeclarationWorks() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = f => EventImplOptions.AddAndRemoveMethods(MethodImplOptions.InstanceMethod("add_" + f.Name), MethodImplOptions.InstanceMethod("remove_" + f.Name)),
                                                                      GetAutoEventBackingFieldImplementation = f => FieldImplOptions.Instance("$" + f.Name)
                                                                    };
            Compile(new[] { "class C { public event System.EventHandler Event1, Event2; }" }, namingConvention: namingConvention);
            FindInstanceField("C.$Event1").Should().NotBeNull();
            FindInstanceField("C.$Event2").Should().NotBeNull();
            FindInstanceMethod("C.add_Event1").Should().NotBeNull();
            FindInstanceMethod("C.remove_Event1").Should().NotBeNull();
            FindInstanceMethod("C.add_Event2").Should().NotBeNull();
            FindInstanceMethod("C.remove_Event2").Should().NotBeNull();
            FindClass("C").StaticFields.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void BackingFieldsForAutoEventsWithInitializerUseThatInitializer() {
            Assert.Inconclusive("TODO");
        }

        [Test]
        public void BackingFieldsForAutoEventsWithNoInitializerGetInitializedToDefault() {
            Assert.Inconclusive("TODO");
        }
    }
}
