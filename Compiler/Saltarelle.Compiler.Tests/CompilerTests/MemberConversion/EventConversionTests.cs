using System;
using Microsoft.CodeAnalysis;
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
			Assert.That(FindInstanceMethod("C.add_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.remove_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Not.Null);
		}

		[Test]
		public void InterfaceEventAccessorsHaveNullDefinition() {
			Compile(new[] { "interface I { event System.EventHandler E; }" });
			Assert.That(FindInstanceMethod("I.add_E").Definition, Is.Null);
			Assert.That(FindInstanceMethod("I.remove_E").Definition, Is.Null);
		}

		[Test]
		public void InstanceAutoEventsWithAddRemoveMethodsWithNoCodeAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.ContainingType.SpecialType == SpecialType.System_Object),
			                                                  GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name, generateCode: false), MethodScriptSemantics.NormalMethod("remove_" + e.Name, generateCode: false)),
			                                                };
			Compile(new[] { "class C { public event System.EventHandler SomeProp; }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Not.Null);
		}

		[Test]
		public void InstanceAutoEventsThatShouldNotGenerateBackingFieldsDoNotGenerateBackingFields() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.ContainingType.SpecialType == SpecialType.System_Object),
			                                                  ShouldGenerateAutoEventBackingField = e => false,
			                                                };
			Compile(new[] { "class C { public event System.EventHandler SomeProp; }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.add_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.remove_SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").UnnamedConstructor.Body.Statements, Is.Empty);
		}

		[Test]
		public void StaticAutoEventsWithAddRemoveMethodsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name), MethodScriptSemantics.NormalMethod("remove_" + e.Name)),
			                                                  GetAutoEventBackingFieldName = e => "$" + e.Name
			                                                };

			Compile(new[] { "class C { public static event System.EventHandler SomeProp; }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticMethod("C.add_SomeProp"), Is.Not.Null);
			Assert.That(FindStaticMethod("C.remove_SomeProp"), Is.Not.Null);
			Assert.That(FindStaticFieldInitializer("C.$SomeProp"), Is.Not.Null);
		}


		[Test]
		public void InstanceManualEventsWithAddRemoveMethodsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = f => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + f.Name), MethodScriptSemantics.NormalMethod("remove_" + f.Name)) };

			Compile(new[] { "class C { public event System.EventHandler SomeProp { add {} remove{} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.add_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.remove_SomeProp"), Is.Not.Null);
		}

		[Test]
		public void InstanceManualEventsWithAddRemoveMethodsWithNoCodeAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.ContainingType.SpecialType == SpecialType.System_Object), GetEventSemantics = f => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + f.Name, generateCode: false), MethodScriptSemantics.NormalMethod("remove_" + f.Name, generateCode: false)) };
			Compile(new[] { "class C { public event System.EventHandler SomeProp { add {} remove{} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").UnnamedConstructor.Body.Statements, Is.Empty);
		}

		[Test]
		public void StaticManualEventsWithAddRemoveMethodsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = f => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + f.Name), MethodScriptSemantics.NormalMethod("remove_" + f.Name)) };

			Compile(new[] { "class C { public static event System.EventHandler SomeProp { add {} remove{} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticMethod("C.add_SomeProp"), Is.Not.Null);
			Assert.That(FindStaticMethod("C.remove_SomeProp"), Is.Not.Null);
		}

		[Test]
		public void ImportingMultipleEventsInTheSameDeclarationWorks() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = f => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + f.Name), MethodScriptSemantics.NormalMethod("remove_" + f.Name)),
			                                                  GetAutoEventBackingFieldName = f => "$" + f.Name
			                                                };
			Compile(new[] { "class C { public event System.EventHandler Event1, Event2; }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceFieldInitializer("C.$Event1"), Is.Not.Null);
			Assert.That(FindInstanceFieldInitializer("C.$Event2"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.add_Event1"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.remove_Event1"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.add_Event2"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.remove_Event2"), Is.Not.Null);
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void BackingFieldsForInstanceAutoEventsWithInitializerUseThatInitializer() {
			Compile(new[] { "class C { public static System.EventHandler GetHandler() { return null; } public event System.EventHandler Event1 = null, Event2 = GetHandler(), Event3 = (s, e) => {}; }" });
			Assert.That(FindInstanceFieldInitializer("C.$Event1"), Is.EqualTo("null"));
			Assert.That(FindInstanceFieldInitializer("C.$Event2"), Is.EqualTo("{sm_C}.GetHandler()"));
			Assert.That(FindInstanceFieldInitializer("C.$Event3").Replace("\r\n", "\n"), Is.EqualTo("function($s, $e) {\n}"));
		}

		[Test]
		public void BackingFieldsForInstanceAutoEventsWithNoInitializerGetInitializedToDefault() {
			Compile(new[] { "class C { public event System.EventHandler Event1; }" });
			Assert.That(FindInstanceFieldInitializer("C.$Event1"), Is.EqualTo("null"));
		}

		[Test]
		public void BackingFieldsForStaticAutoEventsWithInitializerUseThatInitializer() {
			Compile(new[] { "class C { public static System.EventHandler GetHandler() { return null; } public static event System.EventHandler Event1 = null, Event2 = GetHandler(), Event3 = (s, e) => {}; }" });
			Assert.That(FindStaticFieldInitializer("C.$Event1"), Is.EqualTo("null"));
			Assert.That(FindStaticFieldInitializer("C.$Event2"), Is.EqualTo("{sm_C}.GetHandler()"));
			Assert.That(FindStaticFieldInitializer("C.$Event3").Replace("\r\n", "\n"), Is.EqualTo("function($s, $e) {\n}"));
		}

		[Test]
		public void BackingFieldsForStaticAutoEventsWithNoInitializerGetInitializedToDefault() {
			Compile(new[] { "class C { public static event System.EventHandler Event1; }" });
			Assert.That(FindStaticFieldInitializer("C.$Event1"), Is.EqualTo("null"));
		}

		[Test]
		public void AbstractEventIsNotAnAutoEvent() {
			var metadataImporter = new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name), MethodScriptSemantics.NormalMethod("remove_" + e.Name)),
			                                                  GetAutoEventBackingFieldName = e => "$" + e.Name
			                                                };

			Compile(new[] { "abstract class C { public abstract event System.EventHandler SomeProp; }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.add_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.add_SomeProp").Definition, Is.Null);
			Assert.That(FindInstanceMethod("C.remove_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.remove_SomeProp").Definition, Is.Null);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Null);
		}
	}
}
