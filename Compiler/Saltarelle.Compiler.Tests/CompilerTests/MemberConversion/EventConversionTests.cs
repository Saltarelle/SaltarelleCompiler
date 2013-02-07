using System;
using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MemberConversion {
	[TestFixture]
	public class EventConversionTests : CompilerTestBase {
		[Test]
		public void InstanceAutoEventsWithAddRemoveMethodsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name), MethodScriptSemantics.NormalMethod("remove_" + e.Name)),
			                                                  GetAutoEventBackingFieldName = e => "$" + e.Name
			                                                };

			Compile(new[] { "class C { public event System.EventHandler SomeProp; }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.add_SomeProp").Should().NotBeNull();
			FindInstanceMethod("C.remove_SomeProp").Should().NotBeNull();
			FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
		}

		[Test]
		public void InterfaceEventAccessorsHaveNullDefinition() {
			Compile(new[] { "interface I { event System.EventHandler E; }" });
			FindInstanceMethod("I.add_E").Definition.Should().BeNull();
			FindInstanceMethod("I.remove_E").Definition.Should().BeNull();
		}

		[Test]
		public void InstanceAutoEventsWithAddRemoveMethodsWithNoCodeAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name, generateCode: false), MethodScriptSemantics.NormalMethod("remove_" + e.Name, generateCode: false)),
			                                                  GetAutoEventBackingFieldName = e => { throw new InvalidOperationException(); }
			                                                };
			Compile(new[] { "class C { public event System.EventHandler SomeProp; }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
		}

		[Test]
		public void StaticAutoEventsWithAddRemoveMethodsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name), MethodScriptSemantics.NormalMethod("remove_" + e.Name)),
			                                                  GetAutoEventBackingFieldName = e => "$" + e.Name
			                                                };

			Compile(new[] { "class C { public static event System.EventHandler SomeProp; }" }, metadataImporter: metadataImporter);
			FindStaticMethod("C.add_SomeProp").Should().NotBeNull();
			FindStaticMethod("C.remove_SomeProp").Should().NotBeNull();
			FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
		}


		[Test]
		public void InstanceManualEventsWithAddRemoveMethodsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = f => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + f.Name), MethodScriptSemantics.NormalMethod("remove_" + f.Name)) };

			Compile(new[] { "class C { public event System.EventHandler SomeProp { add {} remove{} } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.add_SomeProp").Should().NotBeNull();
			FindInstanceMethod("C.remove_SomeProp").Should().NotBeNull();
		}

		[Test]
		public void InstanceManualEventsWithAddRemoveMethodsWithNoCodeAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = f => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + f.Name, generateCode: false), MethodScriptSemantics.NormalMethod("remove_" + f.Name, generateCode: false)) };
			Compile(new[] { "class C { public event System.EventHandler SomeProp { add {} remove{} } }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
		}

		[Test]
		public void StaticManualEventsWithAddRemoveMethodsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = f => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + f.Name), MethodScriptSemantics.NormalMethod("remove_" + f.Name)) };

			Compile(new[] { "class C { public static event System.EventHandler SomeProp { add {} remove{} } }" }, metadataImporter: metadataImporter);
			FindStaticMethod("C.add_SomeProp").Should().NotBeNull();
			FindStaticMethod("C.remove_SomeProp").Should().NotBeNull();
		}

		[Test]
		public void ImportingMultipleEventsInTheSameDeclarationWorks() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = f => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + f.Name), MethodScriptSemantics.NormalMethod("remove_" + f.Name)),
			                                                  GetAutoEventBackingFieldName = f => "$" + f.Name
			                                                };
			Compile(new[] { "class C { public event System.EventHandler Event1, Event2; }" }, metadataImporter: metadataImporter);
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
			FindInstanceFieldInitializer("C.$Event2").Should().Be("{sm_C}.GetHandler()");
			FindInstanceFieldInitializer("C.$Event3").Replace("\r\n", "\n").Should().Be("function($s, $e) {\n}");
		}

		[Test]
		public void BackingFieldsForInstanceAutoEventsWithNoInitializerGetInitializedToDefault() {
			Compile(new[] { "class C { public event System.EventHandler Event1; }" });
			FindInstanceFieldInitializer("C.$Event1").Should().Be("$Default({def_EventHandler})");
		}

		[Test]
		public void BackingFieldsForStaticAutoEventsWithInitializerUseThatInitializer() {
			Compile(new[] { "class C { public static System.EventHandler GetHandler() { return null; } public static event System.EventHandler Event1 = null, Event2 = GetHandler(), Event3 = (s, e) => {}; }" });
			FindStaticFieldInitializer("C.$Event1").Should().Be("null");
			FindStaticFieldInitializer("C.$Event2").Should().Be("{sm_C}.GetHandler()");
			FindStaticFieldInitializer("C.$Event3").Replace("\r\n", "\n").Should().Be("function($s, $e) {\n}");
		}

		[Test]
		public void BackingFieldsForStaticAutoEventsWithNoInitializerGetInitializedToDefault() {
			Compile(new[] { "class C { public static event System.EventHandler Event1; }" });
			FindStaticFieldInitializer("C.$Event1").Should().Be("$Default({def_EventHandler})");
		}

		[Test]
		public void AbstractEventIsNotAnAutoEvent() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name), MethodScriptSemantics.NormalMethod("remove_" + e.Name)),
			                                                  GetAutoEventBackingFieldName = e => "$" + e.Name
			                                                };

			Compile(new[] { "abstract class C { public abstract event System.EventHandler SomeProp; }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.add_SomeProp").Should().NotBeNull();
			FindInstanceMethod("C.add_SomeProp").Definition.Should().BeNull();
			FindInstanceMethod("C.remove_SomeProp").Should().NotBeNull();
			FindInstanceMethod("C.remove_SomeProp").Definition.Should().BeNull();
			FindInstanceFieldInitializer("C.$SomeProp").Should().BeNull();
		}
	}
}
