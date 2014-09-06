using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.Roslyn;

namespace CoreLib.Tests.MetadataImporterTests {
	[TestFixture]
	public class BackingFieldTests : MetadataImporterTestBase {
		[Test]
		public void PropertyBackingFieldIsNamedFromHierarchyDepthAndPropertyNameWhenNotMinimizing() {
			Prepare("class C { int Prop1 { get; set; } int Prop2 { get; set; } }", minimizeNames: false);
			var f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop1"));
			var f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop2"));

			Assert.That(f1, Is.EqualTo("$1$Prop1Field"));
			Assert.That(f2, Is.EqualTo("$1$Prop2Field"));

			// Verify that we can call the method again and get the same results.
			Assert.That(Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop1")), Is.EqualTo(f1));
			Assert.That(Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop2")), Is.EqualTo(f2));

			Prepare("class B {} interface I {} class D : B, I { int Prop1 { get; set; } int Prop2 { get; set; } }", minimizeNames: false);

			f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["D"].GetProperties().Single(p => p.Name == "Prop1"));
			f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["D"].GetProperties().Single(p => p.Name == "Prop2"));

			Assert.That(f1, Is.EqualTo("$2$Prop1Field"));
			Assert.That(f2, Is.EqualTo("$2$Prop2Field"));

			Prepare("class A {} class B : A {} interface I1 {} interface I2 {} class C : B { int Prop1 { get; set; } int Prop2 { get; set; } }", minimizeNames: false);

			f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop1"));
			f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop2"));

			Assert.That(f1, Is.EqualTo("$3$Prop1Field"));
			Assert.That(f2, Is.EqualTo("$3$Prop2Field"));
		}

		[Test]
		public void PropertyBackingFieldGetsAUniqueNameBasedOnTheHierarchyDepthWhenMinimizing() {
			Prepare("class C { int Prop1 { get; set; } int Prop2 { get; set; } }", minimizeNames: true);
			var f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop1"));
			var f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop2"));

			Assert.That(f1, Is.EqualTo("$1$1"));
			Assert.That(f2, Is.EqualTo("$1$2"));

			// Verify that we can call the method again and get the same results.
			Assert.That(Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop1")), Is.EqualTo(f1));
			Assert.That(Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop2")), Is.EqualTo(f2));

			Prepare("class B {} interface I {} class D : B, I { int Prop1 { get; set; } int Prop2 { get; set; } }", minimizeNames: true);

			f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["D"].GetProperties().Single(p => p.Name == "Prop1"));
			f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["D"].GetProperties().Single(p => p.Name == "Prop2"));

			Assert.That(f1, Is.EqualTo("$2$1"));
			Assert.That(f2, Is.EqualTo("$2$2"));

			Prepare("class A {} class B : A {} interface I1 {} interface I2 {} class C : B { int Prop1 { get; set; } int Prop2 { get; set; } }", minimizeNames: true);

			f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop1"));
			f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop2"));

			Assert.That(f1, Is.EqualTo("$3$1"));
			Assert.That(f2, Is.EqualTo("$3$2"));
		}

		[Test]
		public void BackingFieldNameAttributeOnPropertySetsTheNameOfTheBackingField() {
			Prepare("public class C { [System.Runtime.CompilerServices.BackingFieldName(\"newName\")] public int P { get; set; } }");
			Assert.That(Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single()), Is.EqualTo("newName"));
		}

		[Test]
		public void BackingFieldNameAttributeOnPropertyCausesThatNameToNotBeUsedInDerivedTypes() {
			Prepare(@"
public class C { [System.Runtime.CompilerServices.BackingFieldName(""newName"")] public int P { get; set; } }
public class D : C { public void NewName() {} }");
			Assert.That(FindMethod("D.NewName").Name, Is.EqualTo("newName$1"));
		}

		[Test]
		public void BackingFieldNameAttributeCanUseTheOwnerPlaceholderForProperty() {
			Prepare(@"
public class B { public int P() { return 0; } }
public class C : B { [System.Runtime.CompilerServices.BackingFieldName(""{owner}field"")] public int P { get; set; } }");

			Assert.That(Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single()), Is.EqualTo("p$1field"));
		}

		[Test]
		public void ArgumentToBackingFieldNameMustBeAValidIdentifierForProperty() {
			Prepare(@"public class C1 { [System.Runtime.CompilerServices.BackingFieldName(null)] public int P1 { get; set; } }", expectErrors: true);
			Assert.That(AllErrors[0].Code == 7168 && AllErrors[0].FormattedMessage.Contains("BackingFieldName") && AllErrors[0].FormattedMessage.Contains("C1.P1") && AllErrors[0].FormattedMessage.Contains("valid JavaScript identifier"));
			Prepare(@"public class C1 { [System.Runtime.CompilerServices.BackingFieldName("""")] public int P1 { get; set; } }", expectErrors: true);
			Assert.That(AllErrors[0].Code == 7168 && AllErrors[0].FormattedMessage.Contains("BackingFieldName") && AllErrors[0].FormattedMessage.Contains("C1.P1") && AllErrors[0].FormattedMessage.Contains("valid JavaScript identifier"));
			Prepare(@"public class C1 { [System.Runtime.CompilerServices.BackingFieldName(""a b"")] public int P1 { get; set; } }", expectErrors: true);
			Assert.That(AllErrors[0].Code == 7168 && AllErrors[0].FormattedMessage.Contains("BackingFieldName") && AllErrors[0].FormattedMessage.Contains("C1.P1") && AllErrors[0].FormattedMessage.Contains("valid JavaScript identifier"));
		}

		[Test]
		public void ShouldGenerateBackingFieldReturnsFalseForIntrinsicProperty() {
			Prepare("public class C { [System.Runtime.CompilerServices.IntrinsicProperty] public int P { get; set; } }");
			Assert.That(Metadata.ShouldGenerateAutoPropertyBackingField(AllTypes["C"].GetProperties().Single()), Is.False);
		}

		[Test]
		public void ShouldGenerateBackingFieldReturnsFalseForPropertyWhenCodeIsNotGeneratedForEitherAccessor() {
			Prepare("public class C { public int P { [System.Runtime.CompilerServices.DontGenerate] get; [System.Runtime.CompilerServices.DontGenerate] set; } }");
			Assert.That(Metadata.ShouldGenerateAutoPropertyBackingField(AllTypes["C"].GetProperties().Single()), Is.False);
		}

		[Test]
		public void ShouldGenerateBackingFieldReturnsTrueForPropertyWhenCodeIsNotGeneratedForTheGetAccessor() {
			Prepare("public class C { public int P { [System.Runtime.CompilerServices.DontGenerate] get; set; } }");
			Assert.That(Metadata.ShouldGenerateAutoPropertyBackingField(AllTypes["C"].GetProperties().Single()), Is.True);
		}

		[Test]
		public void ShouldGenerateBackingFieldReturnsTrueForPropertyWhenCodeIsNotGeneratedForTheSetAccessor() {
			Prepare("public class C { public int P { get; [System.Runtime.CompilerServices.DontGenerate] set; } }");
			Assert.That(Metadata.ShouldGenerateAutoPropertyBackingField(AllTypes["C"].GetProperties().Single()), Is.True);
		}

		[Test]
		public void ShouldGenerateBackingFieldReturnsTrueForPropertyWithBackingFieldNameAttributeEvenWhenNoCodeIsGenerated() {
			Prepare("public class C { [System.Runtime.CompilerServices.BackingFieldName(\"newName\")] public int P { [System.Runtime.CompilerServices.DontGenerate] get; [System.Runtime.CompilerServices.DontGenerate] set; } }");
			Assert.That(Metadata.ShouldGenerateAutoPropertyBackingField(AllTypes["C"].GetProperties().Single()), Is.True);
		}

		[Test]
		public void BackingFieldNameAttributeCannotBeSpecifiedOnManualProperty() {
			Prepare("public class C1 { [System.Runtime.CompilerServices.BackingFieldName(\"newName\")] public int P1 { get { return 0; } set {} } }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7167 && AllErrors[0].FormattedMessage.Contains("BackingFieldName") && AllErrors[0].FormattedMessage.Contains("C1.P1"));
		}

		[Test]
		public void EventBackingFieldIsNamedFromHierarchyDepthAndPropertyNameWhenNotMinimizing() {
			Prepare("class C { event System.EventHandler Evt1, Evt2; }", minimizeNames: false);
			var f1 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt1"));
			var f2 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt2"));

			Assert.That(f1, Is.EqualTo("$1$Evt1Field"));
			Assert.That(f2, Is.EqualTo("$1$Evt2Field"));

			// Verify that we can call the method again and get the same results.
			Assert.That(Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt1")), Is.EqualTo(f1));
			Assert.That(Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt2")), Is.EqualTo(f2));

			Prepare("class B {} interface I {} class D : B, I { event System.EventHandler Evt1, Evt2; }", minimizeNames: false);

			f1 = Metadata.GetAutoEventBackingFieldName(AllTypes["D"].GetEvents().Single(e => e.Name == "Evt1"));
			f2 = Metadata.GetAutoEventBackingFieldName(AllTypes["D"].GetEvents().Single(e => e.Name == "Evt2"));

			Assert.That(f1, Is.EqualTo("$2$Evt1Field"));
			Assert.That(f2, Is.EqualTo("$2$Evt2Field"));

			Prepare("class A {} class B : A {} interface I1 {} interface I2 {} class C : B { event System.EventHandler Evt1, Evt2; }", minimizeNames: false);

			f1 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt1"));
			f2 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt2"));

			Assert.That(f1, Is.EqualTo("$3$Evt1Field"));
			Assert.That(f2, Is.EqualTo("$3$Evt2Field"));
		}

		[Test]
		public void EventBackingFieldGetsAUniqueNameBasedOnTheHierarchyDepthWhenMinimizing() {
			Prepare("class C { event System.EventHandler Evt1, Evt2; }", minimizeNames: true);
			var f1 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt1"));
			var f2 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt2"));

			Assert.That(f1, Is.EqualTo("$1$1"));
			Assert.That(f2, Is.EqualTo("$1$2"));

			// Verify that we can call the method again and get the same results.
			Assert.That(Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt1")), Is.EqualTo(f1));
			Assert.That(Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt2")), Is.EqualTo(f2));

			Prepare("class B {} interface I {} class D : B, I { event System.EventHandler Evt1, Evt2; }", minimizeNames: true);

			f1 = Metadata.GetAutoEventBackingFieldName(AllTypes["D"].GetEvents().Single(e => e.Name == "Evt1"));
			f2 = Metadata.GetAutoEventBackingFieldName(AllTypes["D"].GetEvents().Single(e => e.Name == "Evt2"));

			Assert.That(f1, Is.EqualTo("$2$1"));
			Assert.That(f2, Is.EqualTo("$2$2"));

			Prepare("class A {} class B : A {} interface I1 {} interface I2 {} class C : B { event System.EventHandler Evt1, Evt2; }", minimizeNames: true);

			f1 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt1"));
			f2 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt2"));

			Assert.That(f1, Is.EqualTo("$3$1"));
			Assert.That(f2, Is.EqualTo("$3$2"));
		}

		[Test]
		public void BackingFieldNameAttributeOnEventSetsTheNameOfTheBackingField() {
			Prepare("public class C { [System.Runtime.CompilerServices.BackingFieldName(\"newName\")] public event System.Action P; }");
			Assert.That(Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single()), Is.EqualTo("newName"));
		}

		[Test]
		public void BackingFieldNameAttributeOnEventCausesThatNameToNotBeUsedInDerivedTypes() {
			Prepare(@"
public class C { [System.Runtime.CompilerServices.BackingFieldName(""newName"")] public event System.Action P; }
public class D : C { public void NewName() {} }");
			Assert.That(FindMethod("D.NewName").Name, Is.EqualTo("newName$1"));
		}

		[Test]
		public void BackingFieldNameAttributeCanUseTheOwnerPlaceholderForEvent() {
			Prepare(@"
public class B { public int P() { return 0; } }
public class C : B { [System.Runtime.CompilerServices.BackingFieldName(""{owner}field"")] public event System.Action P; }");

			Assert.That(Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single()), Is.EqualTo("p$1field"));
		}

		[Test]
		public void ArgumentToBackingFieldNameMustBeAValidIdentifierForEvent() {
			Prepare(@"public class C1 { [System.Runtime.CompilerServices.BackingFieldName(null)] public event System.Action P1; }", expectErrors: true);
			Assert.That(AllErrors[0].Code == 7170 && AllErrors[0].FormattedMessage.Contains("BackingFieldName") && AllErrors[0].FormattedMessage.Contains("C1.P1") && AllErrors[0].FormattedMessage.Contains("valid JavaScript identifier"));
			Prepare(@"public class C1 { [System.Runtime.CompilerServices.BackingFieldName("""")] public event System.Action P1; }", expectErrors: true);
			Assert.That(AllErrors[0].Code == 7170 && AllErrors[0].FormattedMessage.Contains("BackingFieldName") && AllErrors[0].FormattedMessage.Contains("C1.P1") && AllErrors[0].FormattedMessage.Contains("valid JavaScript identifier"));
			Prepare(@"public class C1 { [System.Runtime.CompilerServices.BackingFieldName(""a b"")] public event System.Action P1; }", expectErrors: true);
			Assert.That(AllErrors[0].Code == 7170 && AllErrors[0].FormattedMessage.Contains("BackingFieldName") && AllErrors[0].FormattedMessage.Contains("C1.P1") && AllErrors[0].FormattedMessage.Contains("valid JavaScript identifier"));
		}

		[Test]
		public void BackingFieldNameAttributeCannotBeSpecifiedOnManualEvent() {
			Prepare("public class C1 { [System.Runtime.CompilerServices.BackingFieldName(\"newName\")] public event System.Action P1 { add {} remove {} } }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7169 && AllErrors[0].FormattedMessage.Contains("BackingFieldName") && AllErrors[0].FormattedMessage.Contains("C1.P1"));
		}

		[Test]
		public void PropertyAndEventBackingFieldsDoNotCollideWhenMinimizing() {
			Prepare("class C { int Prop1 { get; set; } int Prop2 { get; set; } event System.EventHandler Evt1, Evt2; }", minimizeNames: true);
			var f1 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop1"));
			var f2 = Metadata.GetAutoPropertyBackingFieldName(AllTypes["C"].GetProperties().Single(p => p.Name == "Prop2"));
			var f3 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt1"));
			var f4 = Metadata.GetAutoEventBackingFieldName(AllTypes["C"].GetEvents().Single(e => e.Name == "Evt2"));

			Assert.That(new[] { f1, f2, f3, f4 }, Is.EquivalentTo(new[] { "$1$1", "$1$2", "$1$3", "$1$4" }));
		}
	}
}
