using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class PropertyTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void PropertiesImplementedAsGetAndSetMethodsWork() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

public class C1 {
	public int Prop1 { get { return 0; } set {} }
	public int Prop2 { get { return 0; } }
	public int Prop3 { set {} }
}");

			var p1 = FindProperty(types, "C1.Prop1", md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_prop1"));

			var p2 = FindProperty(types, "C1.Prop2", md);
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_prop2"));
			Assert.That(p2.SetMethod, Is.Null);

			var p3 = FindProperty(types, "C1.Prop3", md);
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod, Is.Null);
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_prop3"));
		}

		[Test]
		public void PropertyHidingBaseMemberGetsAUniqueName() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

public class B {
	public int Prop { get; set; }
}

public class D : B {
	public new int Prop { get; set; }
}");

			var p1 = FindProperty(types, "D.Prop", md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_prop$1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_prop$1"));
		}

		[Test]
		public void RenamingPropertiesWithGetAndSetMethodsWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C1 {
	[ScriptName(""Renamed"")]
	public int Prop1 { get { return 0; } set {} }
	[PreserveName]
	public int Prop2 { get { return 0; } set {} }
	[PreserveCase]
	public int Prop3 { get { return 0; } set {} }
}");

			var p1 = FindProperty(types, "C1.Prop1", md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_Renamed"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_Renamed"));

			var p2 = FindProperty(types, "C1.Prop2", md);
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_prop2"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_prop2"));

			var p3 = FindProperty(types, "C1.Prop3", md);
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("get_Prop3"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_Prop3"));
		}

		[Test]
		public void RenamingPropertyGettersAndSettersWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C1 {
	public int Prop1 { [ScriptName(""Renamed1"")] get { return 0; } [ScriptName(""Renamed2"")] set {} }
}");

			var p1 = FindProperty(types, "C1.Prop1", md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("Renamed1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("Renamed2"));
		}

		[Test]
		public void SpecifyingInlineCodeForPropertyGettersAndSettersWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C1 {
	public int Prop1 { [InlineCode(""|some code|"")] get { return 0; } [InlineCode(""|setter|{value}"")] set {} }
}");

			var p1 = FindProperty(types, "C1.Prop1", md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.GetMethod.LiteralCode, Is.EqualTo("|some code|"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.SetMethod.LiteralCode, Is.EqualTo("|setter|{value}"));
		}

		[Test]
		public void SpecifyingScriptSkipForPropertyGetterWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C1 {
	public int Prop1 { [ScriptSkip] get { return 0; } }
}");

			var p1 = FindProperty(types, "C1.Prop1", md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.GetMethod.LiteralCode, Is.EqualTo("{this}"));
			Assert.That(p1.SetMethod, Is.Null);
		}

		[Test]
		public void CannotSpecifyInlineCodeOnPropertyAccessorsImplementingInterfaceMembers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
interface I {
	int Prop { get; set; }
}

class C : I {
	public int Prop { [InlineCode(""|some code|"")] get { return 0; } [InlineCode(""|setter|{value}"")] set {} }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(2));
			Assert.That(er.AllMessages.Any(m => m.Contains("C.get_Prop") && m.Contains("InlineCodeAttribute") && m.Contains("interface member")));
			Assert.That(er.AllMessages.Any(m => m.Contains("C.set_Prop") && m.Contains("InlineCodeAttribute") && m.Contains("interface member")));
		}

		[Test]
		public void CannotSpecifyScriptSkipOnPropertyAccessorsImplementingInterfaceMembers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
interface I {
	int Prop { get; set; }
}

class C : I {
	public int Prop { [ScriptSkip] get { return 0; } [ScriptSkip] set {} }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(2));
			Assert.That(er.AllMessages.Any(m => m.Contains("C.get_Prop") && m.Contains("ScriptSkipAttribute") && m.Contains("interface member")));
			Assert.That(er.AllMessages.Any(m => m.Contains("C.set_Prop") && m.Contains("ScriptSkipAttribute") && m.Contains("interface member")));
		}

		[Test]
		public void CannotSpecifyInlineCodeOnPropertyAccessorsThatOverrideBaseMembers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
class B {
	public virtual int Prop { get; set; }
}

class D : B {
	public sealed override int Prop { [InlineCode(""X"")] get { return 0; } [InlineCode(""X"")] set {} }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(2));
			Assert.That(er.AllMessages.Any(m => m.Contains("D.get_Prop") && m.Contains("InlineCodeAttribute") && m.Contains("overrides")));
			Assert.That(er.AllMessages.Any(m => m.Contains("D.set_Prop") && m.Contains("InlineCodeAttribute") && m.Contains("overrides")));
		}

		[Test]
		public void CannotSpecifyScriptSkipOnPropertyAccessorsThatOverrideBaseMembers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
class B {
	public virtual int Prop { get; set; }
}

class D : B {
	public sealed override int Prop { [ScriptSkip] get { return 0; } [ScriptSkip] set {} }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(2));
			Assert.That(er.AllMessages.Any(m => m.Contains("D.get_Prop") && m.Contains("ScriptSkipAttribute") && m.Contains("overrides")));
			Assert.That(er.AllMessages.Any(m => m.Contains("D.set_Prop") && m.Contains("ScriptSkipAttribute") && m.Contains("overrides")));
		}

		[Test]
		public void CannotSpecifyInlineCodeOnOverridablePropertyAccessors() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
class C {
	public virtual int Prop { [InlineCode(""X"")] get; [InlineCode(""X"")] set; }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(2));
			Assert.That(er.AllMessages.Any(m => m.Contains("C.get_Prop") && m.Contains("InlineCodeAttribute") && m.Contains("overridable")));
			Assert.That(er.AllMessages.Any(m => m.Contains("C.set_Prop") && m.Contains("InlineCodeAttribute") && m.Contains("overridable")));
		}

		[Test]
		public void CannotSpecifyScriptSkipOnOverridablePropertyAccessors() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
class C {
	public virtual int Prop { [ScriptSkip] get; [ScriptSkip] set; }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(2));
			Assert.That(er.AllMessages.Any(m => m.Contains("C.get_Prop") && m.Contains("ScriptSkipAttribute") && m.Contains("overridable")));
			Assert.That(er.AllMessages.Any(m => m.Contains("C.set_Prop") && m.Contains("ScriptSkipAttribute") && m.Contains("overridable")));
		}

		[Test]
		public void OverridingPropertyAccessorsGetTheirNameFromTheDefiningMember() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class A {
	public virtual int P { [ScriptName(""RenamedMethod1"")] get; [ScriptName(""RenamedMethod2"")] set; }
}

class B : A {
	public override int P { get; set; }
}
class C : B {
	public sealed override int P { get; set; }
}");

			var pb = FindProperty(types, "B.P", md);
			Assert.That(pb.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(pb.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pb.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(pb.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pb.SetMethod.Name, Is.EqualTo("RenamedMethod2"));

			var pc = FindProperty(types, "C.P", md);
			Assert.That(pc.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(pc.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(pc.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.SetMethod.Name, Is.EqualTo("RenamedMethod2"));
		}


		[Test]
		public void ImplicitInterfaceImplementationPropertyAccessorsGetTheirNameFromTheInterface() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	int P1 { [ScriptName(""RenamedMethod1"")] get; [ScriptName(""RenamedMethod2"")] set; }
}

interface I2<T> {
	T P2 { [ScriptName(""RenamedMethod3"")] get; [ScriptName(""RenamedMethod4"")] set; }
}

class C : I, I2<int> {
	int P1 { get; set; }
	int P2 { get; set; }
}");

			var p1 = FindProperty(types, "C.P1", md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("RenamedMethod2"));

			var p2 = FindProperty(types, "C.P2", md);
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("RenamedMethod3"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("RenamedMethod4"));
		}

		[Test]
		public void ExplicitInterfaceImplementationPropertyAccessorsGetTheirNameFromTheInterface() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	int P1 { [ScriptName(""RenamedMethod1"")] get; [ScriptName(""RenamedMethod2"")] set; }
}

interface I2<T> {
	T P2 { [ScriptName(""RenamedMethod3"")] get; [ScriptName(""RenamedMethod4"")] set; }
}

class C : I, I2<int> {
	int I.P1 { get; set; }
	int I2<int>.P2 { get; set; }
}");

			var p1 = FindProperty(types, "C.P1", md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("RenamedMethod2"));

			var p2 = FindProperty(types, "C.P2", md);
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("RenamedMethod3"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("RenamedMethod4"));
		}

		[Test]
		public void IntrinsicPropertyAttributeWorksForNonIndexers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[IntrinsicProperty]
	[PreserveCase]
	public int Prop1 { get; set; }

	[IntrinsicProperty]
	[ScriptName(""RenamedProperty"")]
	public int Prop2 { get; set; }

	[IntrinsicProperty]
	public int Prop3 { get; set; }

	[IntrinsicProperty]
	[PreserveName]
	public int Prop4 { get; set; }
}
");

			var impl = FindProperty(types, "C1.Prop1", md);
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("Prop1"));

			impl = FindProperty(types, "C1.Prop2", md);
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("RenamedProperty"));

			impl = FindProperty(types, "C1.Prop3", md);
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("prop3"));

			impl = FindProperty(types, "C1.Prop4", md);
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("prop4"));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnPropertiesImplementingInterfaceMembers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
interface I {
	int Prop { get; set; }
}

class C : I {
	[IntrinsicProperty]
	public int Prop { get; set; }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Contains("C.Prop") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("interface member")));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnPropertiesThatOverrideBaseMembers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
class B {
	public virtual int Prop { get; set; }
}

class D : B {
	[IntrinsicProperty]
	public sealed override int Prop { get; set; }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Contains("D.Prop") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("overrides")));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnInterfaceProperties() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
interface I {
	[IntrinsicProperty]
	int Prop { get; set; }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Contains("I.Prop") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("interface member")));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnOverridableProperties() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
class C {
	[IntrinsicProperty]
	public virtual int Prop { get; set; }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Contains("C.Prop") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("overridable")));
		}

		[Test]
		public void NonScriptableAttributeCausesPropertyToNotBeUsableFromScript() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[NonScriptable]
	public int Prop1 { get; set; }
}
");

			var impl = FindProperty(types, "C1.Prop1", md);
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void ScriptAliasAttributeCannotBeSpecifiedOnInstanceProperty() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;

class C1 {
	[ScriptAlias(""$"")]
	public int Prop { get; set; }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1.Prop") && er.AllMessages[0].Contains("instance member") && er.AllMessages[0].Contains("ScriptAliasAttribute"));
		}

		[Test]
		public void ScriptAliasAttributeWorksOnStaticProperty() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C1 {
	[ScriptAlias(""$"")]
	public static int Prop1 { get; set; }

	[ScriptAlias(""$(this)"")]
	public static int Prop2 { get; }

	[ScriptAlias(""$3"")]
	public static int Prop3 { set; }

	[ScriptAlias(""$4"")]
	[IntrinsicProperty]
	public static int Prop4 { get; set; }
}");

			var p1 = FindProperty(types, "C1.Prop1", md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.GetMethod.LiteralCode, Is.EqualTo("$"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.SetMethod.LiteralCode, Is.EqualTo("$"));

			var p2 = FindProperty(types, "C1.Prop2", md);
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p2.GetMethod.LiteralCode, Is.EqualTo("$(this)"));
			Assert.That(p2.SetMethod, Is.Null);

			var p3 = FindProperty(types, "C1.Prop3", md);
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod, Is.Null);
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p3.SetMethod.LiteralCode, Is.EqualTo("$3"));

			var p4 = FindProperty(types, "C1.Prop4", md);
			Assert.That(p4.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p4.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p4.GetMethod.LiteralCode, Is.EqualTo("$4"));
			Assert.That(p4.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p4.SetMethod.LiteralCode, Is.EqualTo("$4"));
		}

		[Test]
		public void NonPublicPropertiesArePrefixedWithADollarIfSymbolsAreNotMinimized() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C1 {
	public int Prop1 { get; set; }
}

public class C2 {
	private int Prop2 { get; set; }
	internal int Prop3 { get; set; }
}");

			var p1 = FindProperty(types, "C1.Prop1", md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_$prop1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_$prop1"));

			var p2 = FindProperty(types, "C2.Prop2", md);
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_$prop2"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_$prop2"));

			var p3 = FindProperty(types, "C2.Prop3", md);
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("get_$prop3"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_$prop3"));
		}
	}
}