using System.Linq;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporterTests {
	[TestFixture]
	public class BackingFieldTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void PropertyBackingFieldIsNamedFromHierarchyDepthAndPropertyNameWhenNotMinimizing() {
			Prepare("class C { int Prop1 { get; set; } int Prop2 { get; set; } }", minimizeNames: false);
			var f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop1"));
			var f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop2"));

			Assert.That(f1, Is.EqualTo("$1$Prop1Field"));
			Assert.That(f2, Is.EqualTo("$1$Prop2Field"));

			// Verify that we can call the method again and get the same results.
			Assert.That(Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop1")), Is.EqualTo(f1));
			Assert.That(Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop2")), Is.EqualTo(f2));

			Prepare("class B {} interface I {} class D : B, I { int Prop1 { get; set; } int Prop2 { get; set; } }", minimizeNames: false);

			f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["D"].Properties.Single(p => p.Name == "Prop1"));
			f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["D"].Properties.Single(p => p.Name == "Prop2"));

			Assert.That(f1, Is.EqualTo("$2$Prop1Field"));
			Assert.That(f2, Is.EqualTo("$2$Prop2Field"));

			Prepare("class A {} class B : A {} interface I1 {} interface I2 {} class C : B { int Prop1 { get; set; } int Prop2 { get; set; } }", minimizeNames: false);

			f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop1"));
			f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop2"));

			Assert.That(f1, Is.EqualTo("$3$Prop1Field"));
			Assert.That(f2, Is.EqualTo("$3$Prop2Field"));
		}

		[Test]
		public void PropertyBackingFieldGetsAUniqueNameBasedOnTheHierarchyDepthWhenMinimizing() {
			Prepare("class C { int Prop1 { get; set; } int Prop2 { get; set; } }", minimizeNames: true);
			var f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop1"));
			var f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop2"));

			Assert.That(f1, Is.EqualTo("$1$1"));
			Assert.That(f2, Is.EqualTo("$1$2"));

			// Verify that we can call the method again and get the same results.
			Assert.That(Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop1")), Is.EqualTo(f1));
			Assert.That(Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop2")), Is.EqualTo(f2));

			Prepare("class B {} interface I {} class D : B, I { int Prop1 { get; set; } int Prop2 { get; set; } }", minimizeNames: true);

			f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["D"].Properties.Single(p => p.Name == "Prop1"));
			f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["D"].Properties.Single(p => p.Name == "Prop2"));

			Assert.That(f1, Is.EqualTo("$2$1"));
			Assert.That(f2, Is.EqualTo("$2$2"));

			Prepare("class A {} class B : A {} interface I1 {} interface I2 {} class C : B { int Prop1 { get; set; } int Prop2 { get; set; } }", minimizeNames: true);

			f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop1"));
			f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop2"));

			Assert.That(f1, Is.EqualTo("$3$1"));
			Assert.That(f2, Is.EqualTo("$3$2"));
		}

		[Test]
		public void EventBackingFieldIsNamedFromHierarchyDepthAndPropertyNameWhenNotMinimizing() {
			Prepare("class C { event System.EventHandler Evt1, Evt2; }", minimizeNames: false);
			var f1 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt1"));
			var f2 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt2"));

			Assert.That(f1, Is.EqualTo("$1$Evt1Field"));
			Assert.That(f2, Is.EqualTo("$1$Evt2Field"));

			// Verify that we can call the method again and get the same results.
			Assert.That(Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt1")), Is.EqualTo(f1));
			Assert.That(Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt2")), Is.EqualTo(f2));

			Prepare("class B {} interface I {} class D : B, I { event System.EventHandler Evt1, Evt2; }", minimizeNames: false);

			f1 = Metadata.GetAutoEventBackingFieldName(AllTypes["D"].Events.Single(e => e.Name == "Evt1"));
			f2 = Metadata.GetAutoEventBackingFieldName(AllTypes["D"].Events.Single(e => e.Name == "Evt2"));

			Assert.That(f1, Is.EqualTo("$2$Evt1Field"));
			Assert.That(f2, Is.EqualTo("$2$Evt2Field"));

			Prepare("class A {} class B : A {} interface I1 {} interface I2 {} class C : B { event System.EventHandler Evt1, Evt2; }", minimizeNames: false);

			f1 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt1"));
			f2 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt2"));

			Assert.That(f1, Is.EqualTo("$3$Evt1Field"));
			Assert.That(f2, Is.EqualTo("$3$Evt2Field"));
		}

		[Test]
		public void EventBackingFieldGetsAUniqueNameBasedOnTheHierarchyDepthWhenMinimizing() {
			Prepare("class C { event System.EventHandler Evt1, Evt2; }", minimizeNames: true);
			var f1 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt1"));
			var f2 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt2"));

			Assert.That(f1, Is.EqualTo("$1$1"));
			Assert.That(f2, Is.EqualTo("$1$2"));

			// Verify that we can call the method again and get the same results.
			Assert.That(Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt1")), Is.EqualTo(f1));
			Assert.That(Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt2")), Is.EqualTo(f2));

			Prepare("class B {} interface I {} class D : B, I { event System.EventHandler Evt1, Evt2; }", minimizeNames: true);

			f1 = Metadata.GetAutoEventBackingFieldName(AllTypes["D"].Events.Single(e => e.Name == "Evt1"));
			f2 = Metadata.GetAutoEventBackingFieldName(AllTypes["D"].Events.Single(e => e.Name == "Evt2"));

			Assert.That(f1, Is.EqualTo("$2$1"));
			Assert.That(f2, Is.EqualTo("$2$2"));

			Prepare("class A {} class B : A {} interface I1 {} interface I2 {} class C : B { event System.EventHandler Evt1, Evt2; }", minimizeNames: true);

			f1 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt1"));
			f2 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt2"));

			Assert.That(f1, Is.EqualTo("$3$1"));
			Assert.That(f2, Is.EqualTo("$3$2"));
		}

		[Test]
		public void PropertyAndEventBackingFieldsDoNotCollideWhenMinimizing() {
			Prepare("class C { int Prop1 { get; set; } int Prop2 { get; set; } event System.EventHandler Evt1, Evt2; }", minimizeNames: true);
			var f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop1"));
			var f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].Properties.Single(p => p.Name == "Prop2"));
			var f3 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt1"));
			var f4 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].Events.Single(e => e.Name == "Evt2"));

			Assert.That(new[] { f1, f2, f3, f4 }, Is.EquivalentTo(new[] { "$1$1", "$1$2", "$1$3", "$1$4" }));
		}
	}
}
