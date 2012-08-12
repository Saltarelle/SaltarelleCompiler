using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporterTests {
	[TestFixture]
	public class EventTests : ScriptSharpMetadataImporterTestBase {
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
	public event System.EventHandler Evt { [InlineCode(""|add|{value}"")] add {} [InlineCode(""|remove|{value}"")] remove {} }
}");

			var e1 = FindEvent("C1.Evt");
			Assert.That(e1.Type, Is.EqualTo(EventScriptSemantics.ImplType.AddAndRemoveMethods));
			Assert.That(e1.AddMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(e1.AddMethod.LiteralCode, Is.EqualTo("|add|{value}"));
			Assert.That(e1.RemoveMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(e1.RemoveMethod.LiteralCode, Is.EqualTo("|remove|{value}"));
		}

		[Test]
		public void CannotSpecifyInlineCodeOnEventAccessorsImplementingInterfaceMembers() {
			Prepare(
@"using System.Runtime.CompilerServices;
interface I {
	event System.EventHandler Evt;
}

class C : I {
	public event System.EventHandler Evt { [InlineCode(""|some code|"")] add {} [InlineCode(""|some code|"")] remove {} }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(2));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.add_Evt") && m.Contains("InlineCodeAttribute") && m.Contains("interface member")));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.remove_Evt") && m.Contains("InlineCodeAttribute") && m.Contains("interface member")));
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
	}
}
