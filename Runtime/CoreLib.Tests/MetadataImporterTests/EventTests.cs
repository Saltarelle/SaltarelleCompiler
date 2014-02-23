using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace CoreLib.Tests.MetadataImporterTests {
	[TestFixture]
	public class EventTests : MetadataImporterTestBase {
		[Test]
		public void EventsWork() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class C1 {
	public event System.EventHandler Evt1;
}");

			var e1 = FindEvent("C1.Evt1");
			Assert.That(e1.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(e1.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.AddMethod.Name, Is.EqualTo("add_evt1"));
			Assert.That(e1.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.RemoveMethod.Name, Is.EqualTo("remove_evt1"));
		}

		[Test]
		public void NameIsPreservedForImportedTypes() {
			Prepare(
@"using System.Runtime.CompilerServices;

[Imported]
class C1 {
	event System.EventHandler Evt1;
}");

			var e1 = FindEvent("C1.Evt1");
			Assert.That(e1.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(e1.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.AddMethod.Name, Is.EqualTo("add_evt1"));
			Assert.That(e1.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.RemoveMethod.Name, Is.EqualTo("remove_evt1"));
		}

		[Test]
		public void EventHidingBaseMemberGetsAUniqueName() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class B {
	public event System.EventHandler Evt;
}

public class D : B {
	public new event System.EventHandler Evt;
}");

			var e1 = FindEvent("D.Evt");
			Assert.That(e1.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(e1.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.AddMethod.Name, Is.EqualTo("add_evt$1"));
			Assert.That(e1.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.RemoveMethod.Name, Is.EqualTo("remove_evt$1"));
		}

		[Test]
		public void RenamingEventsWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	[ScriptName(""Renamed"")]
	event System.EventHandler Evt1;
	[PreserveName]
	event System.EventHandler Evt2;
	[PreserveCase]
	event System.EventHandler Evt3;
}");

			var e1 = FindEvent("C1.Evt1");
			Assert.That(e1.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(e1.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.AddMethod.Name, Is.EqualTo("add_Renamed"));
			Assert.That(e1.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.RemoveMethod.Name, Is.EqualTo("remove_Renamed"));

			var e2 = FindEvent("C1.Evt2");
			Assert.That(e2.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e2.AddMethod.Name, Is.EqualTo("add_evt2"));
			Assert.That(e2.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e2.RemoveMethod.Name, Is.EqualTo("remove_evt2"));

			var e3 = FindEvent("C1.Evt3");
			Assert.That(e3.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e3.AddMethod.Name, Is.EqualTo("add_Evt3"));
			Assert.That(e3.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e3.RemoveMethod.Name, Is.EqualTo("remove_Evt3"));
		}

		[Test]
		public void RenamingEventAddAndRemoveAccessorsWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public event System.EventHandler Evt { [ScriptName(""Renamed1"")] add {} [ScriptName(""Renamed2"")] remove {} }
}");

			var e1 = FindEvent("C1.Evt");
			Assert.That(e1.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(e1.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.AddMethod.Name, Is.EqualTo("Renamed1"));
			Assert.That(e1.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.RemoveMethod.Name, Is.EqualTo("Renamed2"));
		}

		[Test]
		public void SpecifyingInlineCodeForEventAddAndRemoveAccessorsWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public event System.EventHandler Evt { [InlineCode(""add_({this})._({value})"")] add {} [InlineCode(""remove_({this})._({value})"")] remove {} }
}");

			var e1 = FindEvent("C1.Evt");
			Assert.That(e1.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(e1.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(e1.AddMethod.LiteralCode, Is.EqualTo("add_({this})._({value})"));
			Assert.That(e1.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(e1.RemoveMethod.LiteralCode, Is.EqualTo("remove_({this})._({value})"));
		}

		[Test]
		public void CannotSpecifyInlineCodeOnEventAccessorsThatOverrideBaseMembers() {
			Prepare(
@"using System.Runtime.CompilerServices;
class B {
	public virtual event System.EventHandler Evt;
}

class D : B {
	public sealed override event System.EventHandler Evt { [InlineCode(""X"")] add {} [InlineCode(""X"")] remove {} }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(2));
			Assert.That(AllErrorTexts.Any(m => m.Contains("D.add_Evt") && m.Contains("InlineCodeAttribute") && m.Contains("overrides")));
			Assert.That(AllErrorTexts.Any(m => m.Contains("D.remove_Evt") && m.Contains("InlineCodeAttribute") && m.Contains("overrides")));
		}

		[Test]
		public void CannotSpecifyInlineCodeOnOverridableEventAccessors() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C {
	public virtual event System.EventHandler Evt { [InlineCode(""X"")] add {} [InlineCode(""X"")] remove {} }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(2));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.add_Evt") && m.Contains("InlineCodeAttribute") && m.Contains("overridable")));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.remove_Evt") && m.Contains("InlineCodeAttribute") && m.Contains("overridable")));
		}

		[Test]
		public void OverridingEventAccessorsGetTheirNameFromTheDefiningMember() {
			Prepare(
@"using System.Runtime.CompilerServices;

class A {
	public virtual event System.EventHandler Evt { [ScriptName(""RenamedMethod1"")] add {} [ScriptName(""RenamedMethod2"")] remove {} }
}

class B : A {
	public override event System.EventHandler Evt;
}
class C : B {
	public sealed override event System.EventHandler Evt;
}");

			var eb = FindEvent("B.Evt");
			Assert.That(eb.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(eb.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(eb.AddMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(eb.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(eb.RemoveMethod.Name, Is.EqualTo("RenamedMethod2"));

			var ec = FindEvent("C.Evt");
			Assert.That(ec.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(ec.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(ec.AddMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(ec.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(ec.RemoveMethod.Name, Is.EqualTo("RenamedMethod2"));
		}

		[Test]
		public void NonScriptableAttributeCausesEventToNotBeUsableFromScript() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[NonScriptable]
	public event System.EventHandler Evt;
}
");

			var impl = FindEvent("C1.Evt");
			Assert.That(impl.Type, Is.EqualTo(EventScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void NonPublicEventsArePrefixedWithADollarIfSymbolsAreNotMinimized() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public event System.EventHandler Evt1;
}

public class C2 {
	private event System.EventHandler Evt2;
	internal event System.EventHandler Evt3;
}", minimizeNames: false);

			var e1 = FindEvent("C1.Evt1");
			Assert.That(e1.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(e1.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.AddMethod.Name, Is.EqualTo("add_$evt1"));
			Assert.That(e1.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.RemoveMethod.Name, Is.EqualTo("remove_$evt1"));

			var e2 = FindEvent("C2.Evt2");
			Assert.That(e2.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(e2.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e2.AddMethod.Name, Is.EqualTo("add_$evt2"));
			Assert.That(e2.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e2.RemoveMethod.Name, Is.EqualTo("remove_$evt2"));

			var e3 = FindEvent("C2.Evt3");
			Assert.That(e3.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(e3.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e3.AddMethod.Name, Is.EqualTo("add_$evt3"));
			Assert.That(e3.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e3.RemoveMethod.Name, Is.EqualTo("remove_$evt3"));
		}

		[Test]
		public void ScriptNameCannotBeBlank() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C {
	[ScriptName("""")]
	public event System.EventHandler Evt;
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.Evt") && m.Contains("ScriptNameAttribute") && m.Contains("event") && m.Contains("cannot be empty")));
		}

		[Test]
		public void CustomInitializationAttributeOnAutoEventIsNotAnError() {
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"{$System.DateTime} + {value} + {T} + {this}\")] public event System.Action<T> e; }");
			// No error is good enough
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"\")] public event System.Action<T> e; }");
			// No error is good enough
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(null)] public event System.Action<T> e; }");
			// No error is good enough
		}

		[Test]
		public void CustomInitializationAttributeOnManualEventIsAnError() {
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"null\")] public event System.Action<T> e1 { add {} remove {} } }", expectErrors: true);

			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7165 && AllErrors[0].FormattedMessage.Contains("C1.e1") && AllErrors[0].FormattedMessage.Contains("manual"));
		}

		[Test]
		public void ErrorInCustomInitializationAttributeCodeIsAnError() {
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"{x}\")] public event System.Action<T> e1; }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7163 && AllErrors[0].FormattedMessage.Contains("C1.e1"));

			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"{this}\")] public static event System.Action<T> e1; }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7163 && AllErrors[0].FormattedMessage.Contains("C1.e1"));

			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"a b\")] public event System.Action<T> e1; }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7163 && AllErrors[0].FormattedMessage.Contains("C1.e1"));
		}


		[Test]
		public void DontGenerateAttributeOnAccessorCausesCodeNotToBeGeneratedForTheMethod() {
			Prepare(@"
using System;
using System.Runtime.CompilerServices;
public class C {
	public event Action E1 { [DontGenerate] add {} remove {} }
	public event Action E2 { add {} [DontGenerate] remove {} }
");

			var e1 = FindEvent("C.E1");
			Assert.That(e1.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(e1.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.AddMethod.Name, Is.EqualTo("add_e1"));
			Assert.That(e1.AddMethod.GeneratedMethodName, Is.Null);
			Assert.That(e1.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e1.RemoveMethod.Name, Is.EqualTo("remove_e1"));
			Assert.That(e1.RemoveMethod.GeneratedMethodName, Is.EqualTo("remove_e1"));

			var e2 = FindEvent("C.E2");
			Assert.That(e2.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(e2.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e2.AddMethod.Name, Is.EqualTo("add_e2"));
			Assert.That(e2.AddMethod.GeneratedMethodName, Is.EqualTo("add_e2"));
			Assert.That(e2.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(e2.RemoveMethod.Name, Is.EqualTo("remove_e2"));
			Assert.That(e2.RemoveMethod.GeneratedMethodName, Is.Null);
		}


		[Test]
		public void ScriptNameForAccessorCanIncludeTheOwnerPlaceholder() {
			Prepare(@"
using System;
using System.Runtime.CompilerServices;
public class B {
	[IntrinsicProperty]
	public int E { get; set; }
}
public class C : B {
	public new event Action E { [ScriptName(""add{owner}"")] add {} [ScriptName(""remove{owner}"")] remove {} }
}
public class D : C {
	[IntrinsicProperty]
	public new int E { get; set; }
}
");
			var pc = FindEvent("C.E");
			Assert.That(pc.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(pc.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.AddMethod.Name, Is.EqualTo("adde$1"));
			Assert.That(pc.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.RemoveMethod.Name, Is.EqualTo("removee$1"));

			var pd = FindProperty("D.E");
			Assert.That(pd.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(pd.FieldName, Is.EqualTo("e$2"));
		}
	}
}
