using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace CoreLib.Tests.MetadataImporterTests {
	[TestFixture]
	public class PropertyTests : MetadataImporterTestBase {
		[Test]
		public void PropertiesImplementedAsGetAndSetMethodsWork() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class C1 {
	public int Prop1 { get { return 0; } set {} }
	public int Prop2 { get { return 0; } }
	public int Prop3 { set {} }
}");

			var p1 = FindProperty("C1.Prop1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_prop1"));

			var p2 = FindProperty("C1.Prop2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_prop2"));
			Assert.That(p2.SetMethod, Is.Null);

			var p3 = FindProperty("C1.Prop3");
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod, Is.Null);
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_prop3"));
		}

		[Test]
		public void NameIsPreservedForImportedTypes() {
			Prepare(
@"using System.Runtime.CompilerServices;

[Imported]
class C1 {
	int Prop1 { get; set; }
	[IntrinsicProperty]
	int Prop2 { get; set; }
}");
			
			var p1 = FindProperty("C1.Prop1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_prop1"));

			var p2 = FindProperty("C1.Prop2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p2.FieldName, Is.EqualTo("prop2"));
		}

		[Test]
		public void PropertyHidingBaseMemberGetsAUniqueName() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class B {
	public int Prop { get; set; }
}

public class D : B {
	public new int Prop { get; set; }
}");

			var p1 = FindProperty("D.Prop");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_prop$1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_prop$1"));
		}

		[Test]
		public void RenamingPropertiesWithGetAndSetMethodsWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	[ScriptName(""Renamed"")]
	public int Prop1 { get { return 0; } set {} }
	[PreserveName]
	public int Prop2 { get { return 0; } set {} }
	[PreserveCase]
	public int Prop3 { get { return 0; } set {} }
}");

			var p1 = FindProperty("C1.Prop1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_Renamed"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_Renamed"));

			var p2 = FindProperty("C1.Prop2");
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_prop2"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_prop2"));

			var p3 = FindProperty("C1.Prop3");
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("get_Prop3"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_Prop3"));
		}

		[Test]
		public void RenamingPropertyGettersAndSettersWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public int Prop1 { [ScriptName(""Renamed1"")] get { return 0; } [ScriptName(""Renamed2"")] set {} }
}");

			var p1 = FindProperty("C1.Prop1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("Renamed1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("Renamed2"));
		}

		[Test]
		public void SpecifyingInlineCodeForPropertyGettersAndSettersWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public int Prop1 { [InlineCode(""get_({this})"")] get { return 0; } [InlineCode(""set_({this})._({value})"")] set {} }
}");

			var p1 = FindProperty("C1.Prop1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.GetMethod.LiteralCode, Is.EqualTo("get_({this})"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.SetMethod.LiteralCode, Is.EqualTo("set_({this})._({value})"));
		}

		[Test]
		public void SpecifyingScriptSkipForPropertyGetterWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public int Prop1 { [ScriptSkip] get { return 0; } }
}");

			var p1 = FindProperty("C1.Prop1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.GetMethod.LiteralCode, Is.EqualTo("{this}"));
			Assert.That(p1.SetMethod, Is.Null);
		}

		[Test]
		public void CannotSpecifyScriptSkipOnPropertyAccessorsImplementingInterfaceMembers() {
			Prepare(
@"using System.Runtime.CompilerServices;
interface I {
	int Prop { get; set; }
}

class C : I {
	public int Prop { [ScriptSkip] get { return 0; } [ScriptSkip] set {} }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(2));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.get_Prop") && m.Contains("ScriptSkipAttribute") && m.Contains("interface member")));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.set_Prop") && m.Contains("ScriptSkipAttribute") && m.Contains("interface member")));
		}

		[Test]
		public void CannotSpecifyInlineCodeOnPropertyAccessorsThatOverrideBaseMembers() {
			Prepare(
@"using System.Runtime.CompilerServices;
class B {
	public virtual int Prop { get; set; }
}

class D : B {
	public sealed override int Prop { [InlineCode(""X"")] get { return 0; } [InlineCode(""X"")] set {} }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(2));
			Assert.That(AllErrorTexts.Any(m => m.Contains("D.get_Prop") && m.Contains("InlineCodeAttribute") && m.Contains("overrides")));
			Assert.That(AllErrorTexts.Any(m => m.Contains("D.set_Prop") && m.Contains("InlineCodeAttribute") && m.Contains("overrides")));
		}

		[Test]
		public void CannotSpecifyScriptSkipOnPropertyAccessorsThatOverrideBaseMembers() {
			Prepare(
@"using System.Runtime.CompilerServices;
class B {
	public virtual int Prop { get; set; }
}

class D : B {
	public sealed override int Prop { [ScriptSkip] get { return 0; } [ScriptSkip] set {} }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(2));
			Assert.That(AllErrorTexts.Any(m => m.Contains("D.get_Prop") && m.Contains("ScriptSkipAttribute") && m.Contains("overrides")));
			Assert.That(AllErrorTexts.Any(m => m.Contains("D.set_Prop") && m.Contains("ScriptSkipAttribute") && m.Contains("overrides")));
		}

		[Test]
		public void CannotSpecifyInlineCodeOnOverridablePropertyAccessors() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C {
	public virtual int Prop { [InlineCode(""X"")] get; [InlineCode(""X"")] set; }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(2));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.get_Prop") && m.Contains("InlineCodeAttribute") && m.Contains("overridable")));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.set_Prop") && m.Contains("InlineCodeAttribute") && m.Contains("overridable")));
		}

		[Test]
		public void CannotSpecifyScriptSkipOnOverridablePropertyAccessors() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C {
	public virtual int Prop { [ScriptSkip] get; [ScriptSkip] set; }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(2));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.get_Prop") && m.Contains("ScriptSkipAttribute") && m.Contains("overridable")));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.set_Prop") && m.Contains("ScriptSkipAttribute") && m.Contains("overridable")));
		}

		[Test]
		public void OverridingPropertyAccessorsGetTheirNameFromTheDefiningMember() {
			Prepare(
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

			var pb = FindProperty("B.P");
			Assert.That(pb.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(pb.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pb.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(pb.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pb.SetMethod.Name, Is.EqualTo("RenamedMethod2"));

			var pc = FindProperty("C.P");
			Assert.That(pc.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(pc.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(pc.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.SetMethod.Name, Is.EqualTo("RenamedMethod2"));
		}

		[Test]
		public void ImplicitInterfaceImplementationPropertyAccessorsGetTheirNameFromTheInterface() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	int P1 { [ScriptName(""RenamedMethod1"")] get; [ScriptName(""RenamedMethod2"")] set; }
}

[IncludeGenericArguments(true)]
interface I2<T> {
	T P2 { [ScriptName(""RenamedMethod3"")] get; [ScriptName(""RenamedMethod4"")] set; }
}

public class C : I, I2<int> {
	public int P1 { get; set; }
	public int P2 { get; set; }
}");

			var p1 = FindProperty("C.P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("RenamedMethod2"));

			var p2 = FindProperty("C.P2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("RenamedMethod3"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("RenamedMethod4"));
		}

		[Test]
		public void ExplicitInterfaceImplementationPropertyAccessorsGetTheirNameFromTheInterface() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	int P1 { [ScriptName(""RenamedMethod1"")] get; [ScriptName(""RenamedMethod2"")] set; }
}

[IncludeGenericArguments(true)]
interface I2<T> {
	T P2 { [ScriptName(""RenamedMethod3"")] get; [ScriptName(""RenamedMethod4"")] set; }
}

class C : I, I2<int> {
	int I.P1 { get; set; }
	int I2<int>.P2 { get; set; }
}");

			var p1 = FindProperty("C.P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("RenamedMethod2"));

			var p2 = FindProperty("C.P2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("RenamedMethod3"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("RenamedMethod4"));
		}

		[Test]
		public void CannotSpecfifyNameForGetAndSetMethodPropertyImplementingInterfaceMember() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	int P1 { get; set; }
}

class C : I {
	[ScriptName(""X"")]
	public int P1 { get; set; }
}", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7135 && e.FormattedMessage.Contains("C.P1")));
		}

		[Test]
		public void CannotSpecfifyNameForPropertyAccessorsForGetAndSetMethodPropertyImplementingInterfaceMember() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	int P1 { get; set; }
}

class C : I {
	public int P1 { [ScriptName(""Renamed"")] get; set; }
}", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7135 && e.FormattedMessage.Contains("C.get_P1")));

			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	int P1 { get; set; }
}

class C : I {
	public int P1 { get; [ScriptName(""Renamed"")] set; }
}", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7135 && e.FormattedMessage.Contains("C.set_P1")));
		}

		[Test]
		public void CanSpecifyNameForAccessorMethodOfPropertyImplentingAnInterfaceMemberWhenTheAccessorDoesNotImplementAnInterfaceMember() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	int P1 { set; }
}

public class C : I {
	public int P1 { [ScriptName(""Renamed"")] get; set; }
}");

			var p1 = FindProperty("C.P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("Renamed"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_$p1"));

			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	int P1 { get; }
}

public class C : I {
	public int P1 { get; [ScriptName(""Renamed"")] set; }
}");

			p1 = FindProperty("C.P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_$p1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("Renamed"));
		}

		[Test]
		public void PropertyImplementedAsGetAndSetMethodsCanImplementMultipleInterfacePropertiesWithTheSameScriptName() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I1 {
	int P1 { get; set; }
}

interface I2<T> {
	T P1 { get; set; }
}

class C : I1, I2<int> {
	public int P1 { get; set; }
}");

			var p1 = FindProperty("C.P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_$p1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_$p1"));
		}

		[Test]
		public void PropertyImplementedAsGetAndSetMethodsCannotImplementMultipleInterfacePropertiesWithDifferentNames() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I1 {
	int P1 { get; }
}

interface I2<T> {
	[ScriptName(""Renamed"")]
	T P1 { get; }
}

class C : I1, I2<int> {
	public int P1 { get; set; }
}", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7136 && e.FormattedMessage.Contains("C.get_P1") && (e.FormattedMessage.Contains("I1.get_P1") || e.FormattedMessage.Contains("I2<System.Int32>.get_P1")) && e.FormattedMessage.Contains("get_$p1") && e.FormattedMessage.Contains("Renamed")));

			Prepare(
@"using System.Runtime.CompilerServices;

interface I1 {
	int P1 { set; }
}

interface I2<T> {
	[ScriptName(""Renamed"")]
	T P1 { set; }
}

class C : I1, I2<int> {
	public int P1 { get; set; }
}", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7136 && e.FormattedMessage.Contains("C.set_P1") && (e.FormattedMessage.Contains("I1.set_P1") || e.FormattedMessage.Contains("I2<System.Int32>.set_P1")) && e.FormattedMessage.Contains("set_$p1") && e.FormattedMessage.Contains("Renamed")));
		}

		[Test]
		public void BasePropertyImplementedAsGetAndSetMethodsCanImplementInterfacePropertyWithTheCorrectName() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	int P1 { get; set; }
}

class B {
	public int P1 { get; set; }
}

class C : B {
}
");

			// No error is good enough
		}

		[Test]
		public void BasePropertyImplementedAsGetAndSetMethodsCannotImplementInterfacePropertyWithTheWrongName() {
			Prepare(
@"using System.Runtime.CompilerServices;

public interface I {
	int P1 { get; set; }
}

public class B {
	public int P1 { [ScriptName(""Renamed"")] get; set; }
}

class C : B, I {
}
", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7171 && e.FormattedMessage.Contains("B.get_P1") && e.FormattedMessage.Contains("I.get_P1") && e.FormattedMessage.Contains("get_p1") && e.FormattedMessage.Contains("Renamed")));

			Prepare(
@"using System.Runtime.CompilerServices;

public interface I {
	int P1 { get; set; }
}

public class B {
	public int P1 { get; [ScriptName(""Renamed"")] set; }
}

class C : B, I {
}
", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7171 && e.FormattedMessage.Contains("B.set_P1") && e.FormattedMessage.Contains("I.set_P1") && e.FormattedMessage.Contains("set_p1") && e.FormattedMessage.Contains("Renamed")));
		}

		[Test]
		public void PropertyImplementedAsGetAndSetMethodsCanImplementDifferentPropertiesOnDifferentAccessors() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I1 {
	[ScriptName(""Name1"")]
	int P1 { get; }
}

interface I2<T> {
	[ScriptName(""Name2"")]
	T P1 { set; }
}

class C : I1, I2<int> {
	public int P1 { get; set; }
}");

			var p1 = FindProperty("C.P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_Name1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_Name2"));
		}

		[Test]
		public void IntrinsicPropertyAttributeForAutoPropertiesCreatesAFieldWithoutGeneratedAccessors() {
			Prepare(
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

			var impl = FindProperty("C1.Prop1");
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("Prop1"));
			Assert.That(impl.GenerateAccessors, Is.False);

			impl = FindProperty("C1.Prop2");
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("RenamedProperty"));
			Assert.That(impl.GenerateAccessors, Is.False);

			impl = FindProperty("C1.Prop3");
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("$0"));
			Assert.That(impl.GenerateAccessors, Is.False);

			impl = FindProperty("C1.Prop4");
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("prop4"));
			Assert.That(impl.GenerateAccessors, Is.False);
		}

		[Test]
		public void IntrinsicPropertyAttributeForManualPropertiesCausesAccessorsToBeGenerated() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[IntrinsicProperty]
	[PreserveCase]
	public int Prop1 { get { return 0; } set {} }

	[IntrinsicProperty]
	[ScriptName(""RenamedProperty"")]
	public int Prop2 { get { return 0; } set {} }

	[IntrinsicProperty]
	public int Prop3 { get { return 0; } set {} }

	[IntrinsicProperty]
	[PreserveName]
	public int Prop4 { get { return 0; } set {} }

	[IntrinsicProperty]
	[PreserveName]
	public int Prop5 { get { return 0; } }

	[IntrinsicProperty]
	[PreserveName]
	public int Prop6 { set {} }
}
");

			var impl = FindProperty("C1.Prop1");
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("Prop1"));
			Assert.That(impl.GenerateAccessors, Is.True);

			impl = FindProperty("C1.Prop2");
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("RenamedProperty"));
			Assert.That(impl.GenerateAccessors, Is.True);

			impl = FindProperty("C1.Prop3");
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("$0"));
			Assert.That(impl.GenerateAccessors, Is.True);

			impl = FindProperty("C1.Prop4");
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("prop4"));
			Assert.That(impl.GenerateAccessors, Is.True);

			impl = FindProperty("C1.Prop5");
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("prop5"));
			Assert.That(impl.GenerateAccessors, Is.True);

			impl = FindProperty("C1.Prop6");
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("prop6"));
			Assert.That(impl.GenerateAccessors, Is.True);
		}

		[Test]
		public void IntrinsicPropertyGeneratesAUniqueName() {
			Prepare(
@"using System.Runtime.CompilerServices;
public class B {
	[IntrinsicProperty]
	public int P { get; set; }
}
public class C : B {
	[IntrinsicProperty]
	public new int P { get; set; }
}
");

			var impl = FindProperty("C.P");
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(impl.FieldName, Is.EqualTo("p$1"));
		}

		[Test]
		public void AutoPropertyImplementingFieldLikeInterfacePropertyIsImplementedAsField() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I {
	int Prop1 { get; set; }
	int Prop2 { get; }
	int Prop3 { set; }
	int Prop4 { get; set; }
	[ScriptName(""Renamed"")]
	int Prop5 { get; set; }
}

class C : I {
	public int Prop1 { get; set; }
	public int Prop2 { get; set; }
	public int Prop3 { get; set; }
	[IntrinsicProperty]
	public int Prop4 { get; set; }
	public int Prop5 { get; set; }
}");

			var p1 = FindProperty("C.Prop1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p1.FieldName, Is.EqualTo("prop1"));
			Assert.That(p1.GenerateAccessors, Is.False);

			var p2 = FindProperty("C.Prop2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p2.FieldName, Is.EqualTo("prop2"));
			Assert.That(p2.GenerateAccessors, Is.False);

			var p3 = FindProperty("C.Prop3");
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p3.FieldName, Is.EqualTo("prop3"));
			Assert.That(p3.GenerateAccessors, Is.False);

			var p4 = FindProperty("C.Prop4");
			Assert.That(p4.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p4.FieldName, Is.EqualTo("prop4"));
			Assert.That(p4.GenerateAccessors, Is.False);

			var p5 = FindProperty("C.Prop5");
			Assert.That(p5.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p5.FieldName, Is.EqualTo("Renamed"));
			Assert.That(p5.GenerateAccessors, Is.False);
		}

		[Test]
		public void ManualPropertyImplementingFieldLikeInterfacePropertyIsImplementedAsFieldWithGeneratedAccessors() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I {
	int Prop1 { get; set; }
	int Prop2 { get; }
	int Prop3 { set; }
	int Prop4 { get; set; }
	[ScriptName(""Renamed"")]
	int Prop5 { get; set; }
}

class C : I {
	public int Prop1 { get { return 0; } set {} }
	public int Prop2 { get { return 0; } set {} }
	public int Prop3 { get { return 0; } set {} }
	[IntrinsicProperty]
	public int Prop4 { get { return 0; } set {} }
	public int Prop5 { get { return 0; } set {} }
}");

			var p1 = FindProperty("C.Prop1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p1.FieldName, Is.EqualTo("prop1"));
			Assert.That(p1.GenerateAccessors, Is.True);

			var p2 = FindProperty("C.Prop2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p2.FieldName, Is.EqualTo("prop2"));
			Assert.That(p2.GenerateAccessors, Is.True);

			var p3 = FindProperty("C.Prop3");
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p3.FieldName, Is.EqualTo("prop3"));
			Assert.That(p3.GenerateAccessors, Is.True);

			var p4 = FindProperty("C.Prop4");
			Assert.That(p4.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p4.FieldName, Is.EqualTo("prop4"));
			Assert.That(p4.GenerateAccessors, Is.True);

			var p5 = FindProperty("C.Prop5");
			Assert.That(p5.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p5.FieldName, Is.EqualTo("Renamed"));
			Assert.That(p5.GenerateAccessors, Is.True);
		}

		[Test]
		public void VirtualPropertyImplementingFieldLikeInterfacePropertyIsImplementedAsFieldWithGeneratedAccessors() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I {
	int Prop1 { get; set; }
	int Prop2 { get; }
	int Prop3 { set; }
	int Prop4 { get; set; }
	[ScriptName(""Renamed"")]
	int Prop5 { get; set; }
}

class C : I {
	public virtual int Prop1 { get; set; }
	public virtual int Prop2 { get; set; }
	public virtual int Prop3 { get; set; }
	[IntrinsicProperty]
	public virtual int Prop4 { get; set; }
	public virtual int Prop5 { get; set; }
}");

			var p1 = FindProperty("C.Prop1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p1.FieldName, Is.EqualTo("prop1"));
			Assert.That(p1.GenerateAccessors, Is.True);

			var p2 = FindProperty("C.Prop2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p2.FieldName, Is.EqualTo("prop2"));
			Assert.That(p2.GenerateAccessors, Is.True);

			var p3 = FindProperty("C.Prop3");
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p3.FieldName, Is.EqualTo("prop3"));
			Assert.That(p3.GenerateAccessors, Is.True);

			var p4 = FindProperty("C.Prop4");
			Assert.That(p4.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p4.FieldName, Is.EqualTo("prop4"));
			Assert.That(p4.GenerateAccessors, Is.True);

			var p5 = FindProperty("C.Prop5");
			Assert.That(p5.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p5.FieldName, Is.EqualTo("Renamed"));
			Assert.That(p5.GenerateAccessors, Is.True);
		}

		[Test]
		public void CannotSpecfifyNameForFieldLikePropertyImplementingInterfaceMember() {
			Prepare(
@"using System.Runtime.CompilerServices;

[System.Serializable]
interface I {
	int P1 { get; set; }
}

class C : I {
	[ScriptName(""X"")]
	public int P1 { get; set; }
}", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7135 && e.FormattedMessage.Contains("C.P1")));
		}

		[Test]
		public void FieldLikePropertyImplementingInterfaceMemberReservesName() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I {
	[ScriptName(""x"")]
	int P { get; set; }
}

public class C : I {
	public int P { get; set; }
	public void X() {}
}");

			var x = FindMethod("C.X");
			Assert.That(x.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(x.Name, Is.EqualTo("x$1"));
		}

		[Test]
		public void PropertyCanImplementTwoFieldLikePropertiesWithTheSameName() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I1 {
	int Prop1 { get; set; }
}

[Serializable]
interface I2<T> {
	T Prop1 { get; set; }
}

class C : I1, I2<int> {
	public int Prop1 { get; set; }
}");

			var p1 = FindProperty("C.Prop1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p1.FieldName, Is.EqualTo("prop1"));
		}

		[Test]
		public void PropertyCanNotImplementTwoFieldLikePropertiesWithDifferentNames() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I1 {
	int Prop1 { get; set; }
}

[Serializable]
interface I2 {
	[ScriptName(""Renamed"")]
	int Prop1 { get; set; }
}

class C : I1, I2 {
	public int Prop1 { get; set; }
}", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
		}

		[Test]
		public void FieldLikeBasePropertyCanImplementFieldLikeInterfacePropertyWithTheSameFieldName() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I1 {
	int Prop1 { get; set; }
}

public class B {
	[IntrinsicProperty]
	public int Prop1 { get; set; }
}

public class C : B, I1 {
}");
			// No error is good enough

			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I1 {
	int Prop1 { get; set; }
}

public class B {
	[IntrinsicProperty]
	public virtual int Prop1 { get; set; }
}

public class C : B, I1 {
}");
			// No error is good enough
		}

		[Test]
		public void FieldLikeBasePropertyWithDifferentFieldNameCannotImplementFieldLikeInterfaceProperty() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I1 {
	int Prop1 { get; set; }
}

class B {
	[IntrinsicProperty, ScriptName(""Renamed"")]
	public int Prop1 { get; set; }
}

class C : B, I1 {
}", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7171 && e.FormattedMessage.Contains("B.Prop1") && e.FormattedMessage.Contains("I1.Prop1") && e.FormattedMessage.Contains("prop1") && e.FormattedMessage.Contains("Renamed")));
		}

		[Test]
		public void MethodBasedBasePropertyCannotImplementFieldLikeInterfaceProperty() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I1 {
	int Prop1 { get; set; }
}

class B {
	public int Prop1 { get; set; }
}

class C : B, I1 {
}", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7174 && e.FormattedMessage.Contains("B.Prop1") && e.FormattedMessage.Contains("I1.Prop1") && e.FormattedMessage.Contains("a field") && e.FormattedMessage.Contains("get and set methods")));
		}

		[Test]
		public void PropertyCannotImplementBothMethodBasedAndFieldLikeInterfaceProperties() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I1 {
	int Prop1 { get; set; }
}

interface I2 {
	int Prop1 { get; set; }
}

class C : I1, I2 {
	public int Prop1 { get; set; }
}", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7175 && e.FormattedMessage.Contains("C.Prop1") && (e.FormattedMessage.Contains("I1.Prop1") || e.FormattedMessage.Contains("I2.Prop1")) && e.FormattedMessage.Contains("a field") && e.FormattedMessage.Contains("get and set methods")));
		}

		[Test]
		public void AnErrorIsIssuedIfFieldLikeInterfacePropertyNameIsNotAvailableForNonImportedType() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I1 {
	int Prop1 { get; set; }
}

class B {
	[ScriptName(""prop1""), IntrinsicProperty]
	int P { get; set; }
}

class C : B, I1 {
	public int Prop1 { get; set; }
}", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7172 && e.FormattedMessage.Contains("C.Prop1") && e.FormattedMessage.Contains("I1.Prop1") && e.FormattedMessage.Contains("prop1")));
		}

		[Test]
		public void AnErrorIsNotIssuedIfFieldLikeInterfacePropertyNameIsNotAvailableForImportedType() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Serializable]
interface I1 {
	int Prop1 { get; set; }
}

class B {
	[ScriptName(""prop1""), IntrinsicProperty]
	int P { get; set; }
}

[Imported]
class C : B, I1 {
	public int Prop1 { get; set; }
}", expectErrors: false);

			var prop1 = FindProperty("C.Prop1");
			Assert.That(prop1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(prop1.FieldName, Is.EqualTo("prop1"));
		}

		[Test, Ignore("Future improvement")]
		public void InlineCodeOnFieldLikePropertiesImplementingInterfaceMembers() {
			Assert.Fail("TODO: Must decide how (if) this is supposed to work.");
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnPropertiesImplementingInterfaceMembersImplementedAsGetAndSetMethods() {
			Prepare(
@"using System.Runtime.CompilerServices;
interface I {
	int Prop { get; set; }
}

class C : I {
	[IntrinsicProperty]
	public int Prop { get; set; }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.Prop") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("interface member")));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnPropertiesThatOverrideBaseMembers() {
			Prepare(
@"using System.Runtime.CompilerServices;
class B {
	public virtual int Prop { get; set; }
}

class D : B {
	[IntrinsicProperty]
	public sealed override int Prop { get; set; }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("D.Prop") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("overrides")));
		}

		[Test]
		public void IntrinsicPropertyAttributeOnInterfacePropertiesCausesFieldImplementation() {
			Prepare(
@"using System.Runtime.CompilerServices;
public interface I {
	[IntrinsicProperty]
	int Prop { get; set; }
}");

			var prop = FindProperty("I.Prop");
			Assert.That(prop.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(prop.FieldName, Is.EqualTo("prop"));
			Assert.That(prop.GenerateAccessors, Is.False);
		}

		[Test]
		public void IntrinsicPropertyAttributeOnOverridablePropertiesCausesFieldImplementationWithGeneratedAccessors() {
			Prepare(
@"using System.Runtime.CompilerServices;
public class C {
	[IntrinsicProperty]
	public virtual int Prop { get; set; }
}");

			var prop = FindProperty("C.Prop");
			Assert.That(prop.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(prop.FieldName, Is.EqualTo("prop"));
			Assert.That(prop.GenerateAccessors, Is.True);
		}

		[Test]
		public void NonScriptableAttributeCausesPropertyToNotBeUsableFromScript() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[NonScriptable]
	public int Prop1 { get; set; }
}
");

			var impl = FindProperty("C1.Prop1");
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void NonScriptableAttributeIsNotInheritedFromInterfaceMember() {
			Prepare(
@"using System.Runtime.CompilerServices;
public interface I { [NonScriptable] int Prop { get; set; } }
public class C1 : I {
	public int Prop { get; set; }
}");

			var p = FindProperty("C1.Prop");
			Assert.That(p.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p.GetMethod.Name, Is.EqualTo("get_prop"));
			Assert.That(p.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p.SetMethod.Name, Is.EqualTo("set_prop"));
		}

		[Test]
		public void NonScriptableAttributeIsInheritedForExplicitInterfaceImplementation() {
			Prepare(
@"using System.Runtime.CompilerServices;
public interface I { [NonScriptable] int Prop { get; set; } }
public class C1 : I {
	int I.Prop { get; set; }
}");

			var p = FindProperty("C1.Prop");
			Assert.That(p.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void CanSpecifyIntrinsicPropertyForPropertyImplementingUnusableInterfaceProperty() {
			Prepare(
@"using System.Runtime.CompilerServices;
public interface I { [NonScriptable] int Prop { get; set; } }
public class C1 : I {
	[IntrinsicProperty]
	public int Prop { get; set; }
}");

			var p = FindProperty("C1.Prop");
			Assert.That(p.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p.FieldName, Is.EqualTo("prop"));
			Assert.That(p.GenerateAccessors, Is.False);
		}

		[Test]
		public void NonScriptableAttributeIsNotInheritedFromUnusableBaseMember() {
			Prepare(
@"using System.Runtime.CompilerServices;
public class B { [NonScriptable] public virtual int Prop { get; set; } }
public class C1 : B {
	public sealed override int Prop { get; set; }
}");

			var p = FindProperty("C1.Prop");
			Assert.That(p.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
		}

		[Test]
		public void CanSpecifyIntrinsicPropertyForPropertyOverridingUnusableBaseMember() {
			Prepare(
@"using System.Runtime.CompilerServices;
public class B { [NonScriptable] public virtual int Prop { get; set; } }
public class C1 : B {
	[IntrinsicProperty]
	public sealed override int Prop { get; set; }
}");

			var p = FindProperty("C1.Prop");
			Assert.That(p.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p.FieldName, Is.EqualTo("prop"));
			Assert.That(p.GenerateAccessors, Is.False);
		}

		[Test]
		public void ScriptAliasAttributeCannotBeSpecifiedOnInstanceProperty() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	[ScriptAlias(""$"")]
	public int Prop { get; set; }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.Prop") && AllErrorTexts[0].Contains("instance member") && AllErrorTexts[0].Contains("ScriptAliasAttribute"));
		}

		[Test]
		public void ScriptAliasAttributeWorksOnStaticProperty() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	[ScriptAlias(""$"")]
	public static int Prop1 { get; set; }

	[ScriptAlias(""$(this)"")]
	public static int Prop2 { get { return 0; } }

	[ScriptAlias(""$3"")]
	public static int Prop3 { set {} }

	[ScriptAlias(""$4"")]
	[IntrinsicProperty]
	public static int Prop4 { get; set; }
}");

			var p1 = FindProperty("C1.Prop1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.GetMethod.LiteralCode, Is.EqualTo("$"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.SetMethod.LiteralCode, Is.EqualTo("$ = {value}"));

			var p2 = FindProperty("C1.Prop2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p2.GetMethod.LiteralCode, Is.EqualTo("$(this)"));
			Assert.That(p2.SetMethod, Is.Null);

			var p3 = FindProperty("C1.Prop3");
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod, Is.Null);
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p3.SetMethod.LiteralCode, Is.EqualTo("$3 = {value}"));

			var p4 = FindProperty("C1.Prop4");
			Assert.That(p4.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p4.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p4.GetMethod.LiteralCode, Is.EqualTo("$4"));
			Assert.That(p4.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p4.SetMethod.LiteralCode, Is.EqualTo("$4 = {value}"));
		}

		[Test]
		public void NonPublicPropertiesArePrefixedWithADollarIfSymbolsAreNotMinimized() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public int Prop1 { get; set; }
}

public class C2 {
	private int Prop2 { get; set; }
	internal int Prop3 { get; set; }
}", minimizeNames: false);

			var p1 = FindProperty("C1.Prop1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_$prop1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_$prop1"));

			var p2 = FindProperty("C2.Prop2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_$prop2"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_$prop2"));

			var p3 = FindProperty("C2.Prop3");
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("get_$prop3"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_$prop3"));
		}

		[Test]
		public void ScriptNameCannotBeBlank() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C {
	[ScriptName("""")]
	public int Prop { get; set; }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.Prop") && m.Contains("ScriptNameAttribute") && m.Contains("property") && m.Contains("cannot be empty")));
		}

		[Test]
		public void CustomInitializationAttributeOnAutoPropertyIsNotAnError() {
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"{$System.DateTime} + {value} + {T} + {this}\")] public T p { get; set; } }");
			// No error is good enough
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"\")] public T p { get; set; } }");
			// No error is good enough
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(null)] public T p { get; set; } }");
			// No error is good enough
		}

		[Test]
		public void CustomInitializationAttributeOnAutoPropertyIsNotAnErrorOnPropertyImplementingInterfaceMember() {
			Prepare("interface I1<T> { T p { get; set; } } public class C1<T> : I1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"{$System.DateTime} + {value} + {T} + {this}\")] public T p { get; set; } }");
			// No error is good enough
			Prepare("interface I1<T> { T p { get; set; } } public class C1<T> : I1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"\")] public T p { get; set; } }");
			// No error is good enough
			Prepare("interface I1<T> { T p { get; set; } } public class C1<T> : I1<T> { [System.Runtime.CompilerServices.CustomInitialization(null)] public T p { get; set; } }");
			// No error is good enough
		}

		[Test]
		public void CustomInitializationAttributeOnManualPropertyIsAnError() {
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"null\")] public T p1 { get { return default(T); } set {} } }", expectErrors: true);

			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7166 && AllErrors[0].FormattedMessage.Contains("C1<T>.p1") && AllErrors[0].FormattedMessage.Contains("manual"));
		}

		[Test]
		public void CustomInitializationAttributeOnManualPropertyIsAnErrorOnPropertyImplementingInterfaceMember() {
			Prepare("interface I1<T> { T p1 { get; set; } } public class C1<T> : I1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"null\")] public T p1 { get { return default(T); } set {} } }", expectErrors: true);

			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7166 && AllErrors[0].FormattedMessage.Contains("C1<T>.p1") && AllErrors[0].FormattedMessage.Contains("manual"));
		}

		[Test]
		public void ErrorInCustomInitializationAttributeCodeIsAnError() {
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"{x}\")] public T p1 { get; set; } }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7163 && AllErrors[0].FormattedMessage.Contains("C1<T>.p1"));

			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"{this}\")] public static T p1 { get; set; } }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7163 && AllErrors[0].FormattedMessage.Contains("C1<T>.p1"));

			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"a b\")] public T p1 { get; set; } }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7163 && AllErrors[0].FormattedMessage.Contains("C1<T>.p1"));
		}

		[Test]
		public void ErrorInCustomInitializationAttributeCodeIsAnErrorOnPropertyImplementingInterfaceMember() {
			Prepare("public interface I<T> { T p1 { get; set; } } public class C1<T> : I<T> { [System.Runtime.CompilerServices.CustomInitialization(\"{x}\")] public T p1 { get; set; } }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7163 && AllErrors[0].FormattedMessage.Contains("C1<T>.p1"));

			Prepare("public interface I<T> { T p1 { get; set; } } public class C1<T> : I<T> { [System.Runtime.CompilerServices.CustomInitialization(\"a b\")] public T p1 { get; set; } }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7163 && AllErrors[0].FormattedMessage.Contains("C1<T>.p1"));
		}

		[Test]
		public void DontGenerateAttributeOnAccessorCausesCodeNotToBeGeneratedForTheMethod() {
			Prepare(@"
using System.Runtime.CompilerServices;
public class C {
	public int P1 { [DontGenerate] get; set; }
	public int P2 { get; [DontGenerate] set; }
}");

			var p1 = FindProperty("C.P1");
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_p1"));
			Assert.That(p1.GetMethod.GeneratedMethodName, Is.Null);
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_p1"));
			Assert.That(p1.SetMethod.GeneratedMethodName, Is.EqualTo("set_p1"));

			var p2 = FindProperty("C.P2");
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_p2"));
			Assert.That(p2.GetMethod.GeneratedMethodName, Is.EqualTo("get_p2"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_p2"));
			Assert.That(p2.SetMethod.GeneratedMethodName, Is.Null);
		}

		[Test]
		public void ScriptNameForAccessorCanIncludeTheOwnerPlaceholder() {
			Prepare(@"
using System.Runtime.CompilerServices;
public class B {
	[IntrinsicProperty]
	public int P { get; set; }
}
public class C : B {
	public new int P { [ScriptName(""get{owner}"")] get; [ScriptName(""set{owner}"")] set; }
}
public class D : C {
	[IntrinsicProperty]
	public new int P { get; set; }
}
");
			var pc = FindProperty("C.P");
			Assert.That(pc.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(pc.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.GetMethod.Name, Is.EqualTo("getp$1"));
			Assert.That(pc.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.SetMethod.Name, Is.EqualTo("setp$1"));

			var pd = FindProperty("D.P");
			Assert.That(pd.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(pd.FieldName, Is.EqualTo("p$2"));
		}
	}
}