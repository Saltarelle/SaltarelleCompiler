using System;
using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.Compiler.MemberConversionTests {
    [TestFixture]
    public class EventConversionTests : CompilerTestBase {
        [Test]
        public void InstanceAutoEventsWithAddRemoveMethodsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name), MethodScriptSemantics.NormalMethod("remove_" + e.Name)),
                                                                      GetAutoEventBackingFieldName = e => "$" + e.Name
                                                                    };

            Compile(new[] { "class C { public event System.EventHandler SomeProp; }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.add_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.remove_SomeProp").Should().NotBeNull();
            FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
        }

        [Test]
        public void InstanceAutoEventsWithAddRemoveMethodsWithNoCodeAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name, generateCode: false), MethodScriptSemantics.NormalMethod("remove_" + e.Name, generateCode: false)),
                                                                      GetAutoEventBackingFieldName = e => { throw new InvalidOperationException(); }
                                                                    };
            Compile(new[] { "class C { public event System.EventHandler SomeProp; }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
        }

        [Test]
        public void StaticAutoEventsWithAddRemoveMethodsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name), MethodScriptSemantics.NormalMethod("remove_" + e.Name)),
                                                                      GetAutoEventBackingFieldName = e => "$" + e.Name
                                                                    };

            Compile(new[] { "class C { public static event System.EventHandler SomeProp; }" }, namingConvention: namingConvention);
            FindStaticMethod("C.add_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.remove_SomeProp").Should().NotBeNull();
            FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
        }


        [Test]
        public void InstanceManualEventsWithAddRemoveMethodsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = f => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + f.Name), MethodScriptSemantics.NormalMethod("remove_" + f.Name)) };

            Compile(new[] { "class C { public event System.EventHandler SomeProp { add {} remove{} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.add_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.remove_SomeProp").Should().NotBeNull();
        }

        [Test]
        public void InstanceManualEventsWithAddRemoveMethodsWithNoCodeAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = f => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + f.Name, generateCode: false), MethodScriptSemantics.NormalMethod("remove_" + f.Name, generateCode: false)) };
            Compile(new[] { "class C { public event System.EventHandler SomeProp { add {} remove{} } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
        }

        [Test]
        public void StaticManualEventsWithAddRemoveMethodsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = f => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + f.Name), MethodScriptSemantics.NormalMethod("remove_" + f.Name)) };

            Compile(new[] { "class C { public static event System.EventHandler SomeProp { add {} remove{} } }" }, namingConvention: namingConvention);
            FindStaticMethod("C.add_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.remove_SomeProp").Should().NotBeNull();
        }

        [Test]
        public void ImportingMultipleEventsInTheSameDeclarationWorks() {
            var namingConvention = new MockNamingConventionResolver { GetEventImplementation = f => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + f.Name), MethodScriptSemantics.NormalMethod("remove_" + f.Name)),
                                                                      GetAutoEventBackingFieldName = f => "$" + f.Name
                                                                    };
            Compile(new[] { "class C { public event System.EventHandler Event1, Event2; }" }, namingConvention: namingConvention);
            FindInstanceFieldInitializer("C.$Event1").Should().NotBeNull();
            FindInstanceFieldInitializer("C.$Event2").Should().NotBeNull();
            FindInstanceMethod("C.add_Event1").Should().NotBeNull();
            FindInstanceMethod("C.remove_Event1").Should().NotBeNull();
            FindInstanceMethod("C.add_Event2").Should().NotBeNull();
            FindInstanceMethod("C.remove_Event2").Should().NotBeNull();
            FindClass("C").StaticInitStatements.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void BackingFieldsForInstanceAutoEventsWithInitializerUseThatInitializer() {
            Compile(new[] { "class C { public static System.EventHandler GetHandler() { return null; } public event System.EventHandler Event1 = null, Event2 = GetHandler(), Event3 = (s, e) => {}; }" });
            FindInstanceFieldInitializer("C.$Event1").Should().Be("null");
            FindInstanceFieldInitializer("C.$Event2").Should().Be("{C}.GetHandler()");
            FindInstanceFieldInitializer("C.$Event3").Should().Be("function($s, $e) {\r\n}");
        }

        [Test]
        public void BackingFieldsForInstanceAutoEventsWithNoInitializerGetInitializedToDefault() {
            Compile(new[] { "class C { public event System.EventHandler Event1; }" });
            FindInstanceFieldInitializer("C.$Event1").Should().Be("null");
        }

        [Test]
        public void BackingFieldsForStaticAutoEventsWithInitializerUseThatInitializer() {
            Compile(new[] { "class C { public static System.EventHandler GetHandler() { return null; } public static event System.EventHandler Event1 = null, Event2 = GetHandler(), Event3 = (s, e) => {}; }" });
            FindStaticFieldInitializer("C.$Event1").Should().Be("null");
            FindStaticFieldInitializer("C.$Event2").Should().Be("{C}.GetHandler()");
            FindStaticFieldInitializer("C.$Event3").Should().Be("function($s, $e) {\r\n}");
        }

        [Test]
        public void BackingFieldsForStaticAutoEventsWithNoInitializerGetInitializedToDefault() {
            Compile(new[] { "class C { public static event System.EventHandler Event1; }" });
            FindStaticFieldInitializer("C.$Event1").Should().Be("null");
        }
    }
}
