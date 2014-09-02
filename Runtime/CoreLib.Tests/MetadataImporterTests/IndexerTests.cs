using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace CoreLib.Tests.MetadataImporterTests {
	[TestFixture]
	public class IndexerTests : MetadataImporterTestBase {
		[Test]
		public void IndexersImplementedAsGetAndSetMethodsWork() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class C1 {
	public int this[int x] { get { return 0; } set {} }
	public int this[int x, int y] { get { return 0; } set {} }
	public int this[int x, int y, int z] { get { return 0; } set {} }
}");

			var p1 = FindIndexer("C1", 1);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_item"));

			var p2 = FindIndexer("C1", 2);
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_item$1"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_item$1"));

			var p3 = FindIndexer("C1", 3);
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("get_item$2"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_item$2"));
		}

		[Test]
		public void NameIsPreservedForImportedTypes() {
			Prepare(
@"using System.Runtime.CompilerServices;

[Imported]
class C1 {
	int this[int x] { get { return 0; } set {} }
}");

			var p1 = FindIndexer("C1", 1);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_item"));
		}

		[Test]
		public void IndexerHidingBaseMemberGetsAUniqueName() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class B {
	public int this[int x] { get { return 0; } set {} }
}

public class D : B {
	public int this[int x] { get { return 0; } set {} }
}");

			var p1 = FindIndexer("D", 1);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_item$1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_item$1"));
		}

		public int this[int a] { get { return 0; } set {} }
		public int this[int a, int b] { get { return 0; } set {} }

		[Test]
		public void RenamingIndexersWithGetAndSetMethodsWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	[ScriptName(""Renamed"")]
	public int this[int x] { get { return 0; } set {} }
	[PreserveName]
	public int this[int x, int y] { get { return 0; } set {} }
	[PreserveCase]
	public int this[int x, int y, int z] { get { return 0; } set {} }
}");

			var p1 = FindIndexer("C1", 1);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_Renamed"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_Renamed"));

			var p2 = FindIndexer("C1", 2);
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_item"));

			var p3 = FindIndexer("C1", 3);
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("get_Item"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_Item"));
		}

		[Test]
		public void RenamingIndexerGettersAndSettersWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public int this[int x] { [ScriptName(""Renamed1"")] get { return 0; } [ScriptName(""Renamed2"")] set {} }
}");

			var p1 = FindIndexer("C1", 1);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("Renamed1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("Renamed2"));
		}

		[Test]
		public void SpecifyingInlineCodeForIndexerGettersAndSettersWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public int this[int x] { [InlineCode(""get_({this})"")] get { return 0; } [InlineCode(""set_({this})._({value})"")] set {} }
}");

			var p1 = FindIndexer("C1", 1);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.GetMethod.LiteralCode, Is.EqualTo("get_({this})"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.SetMethod.LiteralCode, Is.EqualTo("set_({this})._({value})"));
		}

		[Test]
		public void CannotSpecifyInlineCodeOnIndexerAccessorsThatOverrideBaseMembers() {
			Prepare(
@"using System.Runtime.CompilerServices;
class B {
	public virtual int this[int x] { get { return 0; } set {} }
}

class D : B {
	public sealed override int this[int x] { [InlineCode(""X"")] get { return 0; } [InlineCode(""X"")] set {} }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(2));
			Assert.That(AllErrorTexts.Any(m => m.Contains("D.get_Item") && m.Contains("InlineCodeAttribute") && m.Contains("overrides")));
			Assert.That(AllErrorTexts.Any(m => m.Contains("D.set_Item") && m.Contains("InlineCodeAttribute") && m.Contains("overrides")));
		}

		[Test]
		public void OverridingIndexerAccessorsGetTheirNameFromTheDefiningMember() {
			Prepare(
@"using System.Runtime.CompilerServices;

class A {
	public virtual int this[int x] { [ScriptName(""RenamedMethod1"")] get; [ScriptName(""RenamedMethod2"")] set; }
}

class B : A {
	public override int this[int x] { get { return 0; } set {} }
}
class C : B {
	public sealed override int this[int x] { get { return 0; } set {} }
}");

			var pb = FindIndexer("B", 1);
			Assert.That(pb.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(pb.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pb.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(pb.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pb.SetMethod.Name, Is.EqualTo("RenamedMethod2"));

			var pc = FindIndexer("C", 1);
			Assert.That(pc.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(pc.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(pc.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.SetMethod.Name, Is.EqualTo("RenamedMethod2"));
		}


		[Test]
		public void ImplicitInterfaceImplementationIndexerAccessorsGetTheirNameFromTheInterface() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	int this[int x] { [ScriptName(""RenamedMethod1"")] get { return 0; } [ScriptName(""RenamedMethod2"")] set {} }
}

[IncludeGenericArguments(true)]
interface I2<T> {
	T this[int x, int y] { [ScriptName(""RenamedMethod3"")] get { return default(T); } [ScriptName(""RenamedMethod4"")] set {} }
}

class C : I, I2<int> {
	public int this[int x] { get { return 0; } set; }
	public int this[int x, int y] { get { return 0; } set; }
}");

			var p1 = FindIndexer("C", 1);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("RenamedMethod2"));

			var p2 = FindIndexer("C", 2);
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("RenamedMethod3"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("RenamedMethod4"));
		}

		[Test]
		public void ExplicitInterfaceImplementationIndexerAccessorsGetTheirNameFromTheInterface() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	int this[int x] { [ScriptName(""RenamedMethod1"")] get; [ScriptName(""RenamedMethod2"")] set; }
}

[IncludeGenericArguments(true)]
interface I2<T> {
	T this[int x] { [ScriptName(""RenamedMethod3"")] get; [ScriptName(""RenamedMethod4"")] set; }
}

class C : I, I2<int> {
	int I.this[int x] { get { return 0; } set {} }
	int I2<int>.this[int x] { get { return 0; } set {} }
}");

			var p1 = Metadata.GetPropertySemantics(AllTypes["C"].GetProperties().Single(i => i.FindImplementedInterfaceMembers().Single().ContainingType.TypeArguments.IsEmpty));
			var p2 = Metadata.GetPropertySemantics(AllTypes["C"].GetProperties().Single(i => !i.FindImplementedInterfaceMembers().Single().ContainingType.TypeArguments.IsEmpty));

			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("RenamedMethod2"));

			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("RenamedMethod3"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("RenamedMethod4"));
		}

		[Test]
		public void NonScriptableAttributeCausesIndexerToNotBeUsableFromScript() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[NonScriptable]
	public int this[int x] { get { return 0; } set {} }
}
");

			var impl = FindIndexer("C1", 1);
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.NotUsableFromScript));
		}


		[Test]
		public void IntrinsicPropertyAttributeWorksForIndexers() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[IntrinsicProperty]
	public int this[int x] { get { return 0; } set {} }
}
");

			var impl = FindIndexer("C1", 1);
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(impl.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NativeIndexer));
			Assert.That(impl.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NativeIndexer));
		}

		[Test]
		public void IntrinsicPropertyAttributeWorksForReadOnlyIndexers() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[IntrinsicProperty]
	public int this[int x] { get { return 0; } }
}
");

			var impl = FindIndexer("C1", 1);
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(impl.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NativeIndexer));
			Assert.That(impl.SetMethod, Is.Null);
		}

		[Test]
		public void IntrinsicPropertyAttributeWorksForWriteOnlyIndexers() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[IntrinsicProperty]
	public int this[int x] { set {} }
}
");

			var impl = FindIndexer("C1", 1);
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(impl.GetMethod, Is.Null);
			Assert.That(impl.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NativeIndexer));
		}

		[Test]
		public void IndexerWithIntrinsicPropertyAttributeMustHaveExactlyOneArgument() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[IntrinsicProperty]
	public int this[int x, int y] { set {} }
}
", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("IntrinsicPropertyAttribute") && AllErrorTexts[0].Contains("indexer") && AllErrorTexts[0].Contains("exactly one parameter"));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnPropertiesImplementingInterfaceMembers() {
			Prepare(
@"using System.Runtime.CompilerServices;
interface I {
	int this[int x] { get; set; }
}

class C1 : I {
	[IntrinsicProperty]
	public int this[int x] { get { return 0; } set {} }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("indexer") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("interface member")));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnPropertiesThatOverrideBaseMembers() {
			Prepare(
@"using System.Runtime.CompilerServices;
class B {
	public virtual int this[int x] { get { return 0; } set {} }
}

class D1 : B {
	[IntrinsicProperty]
	public sealed override int this[int x] { get { return 0; } set {} }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("indexer") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("overrides")));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnInterfaceProperties() {
			Prepare(
@"using System.Runtime.CompilerServices;
interface I1 {
	[IntrinsicProperty]
	int this[int x] { get; set; }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("indexer") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("interface member")));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnOverridableProperties() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[IntrinsicProperty]
	public virtual int this[int x] { get { return 0; } set {} }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("indexer") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("overridable")));
		}

		[Test]
		public void NonPublicIndexersArePrefixedWithADollarIfSymbolsAreNotMinimized() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public int this[int x] { get { return 0; } set {} }
}

public class C2 {
	private int this[int x] { get { return 0; } set {} }
	internal int this[int x, int y] { get { return 0; } set {} }
}", minimizeNames: false);

			var p1 = FindIndexer("C1", 1);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_$item"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_$item"));

			var p2 = FindIndexer("C2", 1);
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_$item$1"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_$item$1"));

			var p3 = FindIndexer("C2", 2);
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("get_$item"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_$item"));
		}

		[Test]
		public void ScriptAliasAttributeCannotBeSpecifiedOnIndexer() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	[ScriptAlias(""$"")]
	public int this[int x] { get { return 0; } set {} }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("Indexer") && AllErrorTexts[0].Contains("ScriptAliasAttribute"));
		}

		[Test]
		public void ScriptNameCannotBeBlank() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C {
	[ScriptName("""")]
	public int this[int x] { get { return 0; } set {} }
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C") && m.Contains("ScriptNameAttribute") && m.Contains("indexer") && m.Contains("cannot be empty")));
		}

		[Test]
		public void ScriptNameForAccessorCanIncludeTheOwnerPlaceholder() {
			Prepare(@"
using System.Runtime.CompilerServices;
public class B {
	[IntrinsicProperty]
	public int Item { get; set; }
}
public class C : B {
	public new int this[int x] { [ScriptName(""get{owner}"")] get { return 0; } [ScriptName(""set{owner}"")] set {} }
}
public class D : C {
	[IntrinsicProperty]
	public new int Item { get; set; }
}
");
			var pc = FindIndexer("C", 1);
			Assert.That(pc.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(pc.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.GetMethod.Name, Is.EqualTo("getitem$1"));
			Assert.That(pc.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.SetMethod.Name, Is.EqualTo("setitem$1"));

			var pd = FindProperty("D.Item");
			Assert.That(pd.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(pd.FieldName, Is.EqualTo("item$2"));
		}
	}
}
