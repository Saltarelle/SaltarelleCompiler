using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class IndexerTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void IndexersImplementedAsGetAndSetMethodsWork() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

public class C1 {
	public int this[int x] { get { return 0; } set {} }
	public int this[int x, int y] { get { return 0; } set {} }
	public int this[int x, int y, int z] { get { return 0; } set {} }
}");

			var p1 = FindIndexer(types, "C1", 1, md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_item"));

			var p2 = FindIndexer(types, "C1", 2, md);
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_item$1"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_item$1"));

			var p3 = FindIndexer(types, "C1", 3, md);
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("get_item$2"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_item$2"));
		}

		[Test]
		public void IndexerHidingBaseMemberGetsAUniqueName() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

public class B {
	public int this[int x] { get { return 0; } set {} }
}

public class D : B {
	public int this[int x] { get { return 0; } set {} }
}");

			var p1 = FindIndexer(types, "D", 1, md);
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
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C1 {
	[ScriptName(""Renamed"")]
	public int this[int x] { get { return 0; } set {} }
	[PreserveName]
	public int this[int x, int y] { get { return 0; } set {} }
	[PreserveCase]
	public int this[int x, int y, int z] { get { return 0; } set {} }
}");

			var p1 = FindIndexer(types, "C1", 1, md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_Renamed"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_Renamed"));

			var p2 = FindIndexer(types, "C1", 2, md);
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_item"));

			var p3 = FindIndexer(types, "C1", 3, md);
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("get_Item"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_Item"));
		}

		[Test]
		public void RenamingIndexerGettersAndSettersWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C1 {
	public int this[int x] { [ScriptName(""Renamed1"")] get { return 0; } [ScriptName(""Renamed2"")] set {} }
}");

			var p1 = FindIndexer(types, "C1", 1, md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("Renamed1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("Renamed2"));
		}

		[Test]
		public void SpecifyingInlineCodeForIndexerGettersAndSettersWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C1 {
	public int this[int x] { [InlineCode(""|some code|"")] get { return 0; } [InlineCode(""|setter|{value}"")] set {} }
}");

			var p1 = FindIndexer(types, "C1", 1, md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.GetMethod.LiteralCode, Is.EqualTo("|some code|"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(p1.SetMethod.LiteralCode, Is.EqualTo("|setter|{value}"));
		}

		[Test]
		public void CannotSpecifyInlineCodeOnIndexerAccessorsImplementingInterfaceMembers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
interface I {
	int this[int x] { get; set; }
}

class C : I {
	public int this[int x] { [InlineCode(""|some code|"")] get { return 0; } [InlineCode(""|setter|{value}"")] set {} }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(2));
			Assert.That(er.AllMessages.Any(m => m.Contains("C.get_Item") && m.Contains("InlineCodeAttribute") && m.Contains("interface member")));
			Assert.That(er.AllMessages.Any(m => m.Contains("C.set_Item") && m.Contains("InlineCodeAttribute") && m.Contains("interface member")));
		}

		[Test]
		public void CannotSpecifyInlineCodeOnIndexerAccessorsThatOverrideBaseMembers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
class B {
	public virtual int this[int x] { get { return 0; } set {} }
}

class D : B {
	public sealed override int this[int x] { [InlineCode(""X"")] get { return 0; } [InlineCode(""X"")] set {} }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(2));
			Assert.That(er.AllMessages.Any(m => m.Contains("D.get_Item") && m.Contains("InlineCodeAttribute") && m.Contains("overrides")));
			Assert.That(er.AllMessages.Any(m => m.Contains("D.set_Item") && m.Contains("InlineCodeAttribute") && m.Contains("overrides")));
		}

		[Test]
		public void OverridingIndexerAccessorsGetTheirNameFromTheDefiningMember() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
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

			var pb = FindIndexer(types, "B", 1, md);
			Assert.That(pb.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(pb.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pb.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(pb.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pb.SetMethod.Name, Is.EqualTo("RenamedMethod2"));

			var pc = FindIndexer(types, "C", 1, md);
			Assert.That(pc.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(pc.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(pc.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(pc.SetMethod.Name, Is.EqualTo("RenamedMethod2"));
		}


		[Test]
		public void ImplicitInterfaceImplementationIndexerAccessorsGetTheirNameFromTheInterface() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	int this[int x] { [ScriptName(""RenamedMethod1"")] get { return 0; } [ScriptName(""RenamedMethod2"")] set {} }
}

interface I2<T> {
	T this[int x, int y] { [ScriptName(""RenamedMethod3"")] get { return default(T); } [ScriptName(""RenamedMethod4"")] set {} }
}

class C : I, I2<int> {
	int this[int x] { get { return 0; } set; }
	int this[int x, int y] { get { return 0; } set; }
}");

			var p1 = FindIndexer(types, "C", 1, md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("RenamedMethod1"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("RenamedMethod2"));

			var p2 = FindIndexer(types, "C", 2, md);
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("RenamedMethod3"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("RenamedMethod4"));
		}

		[Test]
		public void ExplicitInterfaceImplementationIndexerAccessorsGetTheirNameFromTheInterface() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	int this[int x] { [ScriptName(""RenamedMethod1"")] get { return 0; } [ScriptName(""RenamedMethod2"")] set {} }
}

interface I2<T> {
	T this[int x] { [ScriptName(""RenamedMethod3"")] get { return default(T); } [ScriptName(""RenamedMethod4"")] set {} }
}

class C : I, I2<int> {
	int I.this[int x] { get { return 0; } set {} }
	int I2<int>.this[int x] { get { return 0; } set {} }
}");

			var p1 = md.GetPropertyImplementation(types["C"].Members.OfType<IProperty>().Single(i => !(i.ImplementedInterfaceMembers[0].DeclaringType is ParameterizedType)));
			var p2 = md.GetPropertyImplementation(types["C"].Members.OfType<IProperty>().Single(i => i.ImplementedInterfaceMembers[0].DeclaringType is ParameterizedType));
			
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
		public void IntrinsicPropertyAttributeWorksForIndexers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[IntrinsicProperty]
	public int this[int x] { get { return 0; } set {} }
}
");

			var impl = FindIndexer(types, "C1", 1, md);
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(impl.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NativeIndexer));
			Assert.That(impl.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NativeIndexer));
		}

		[Test]
		public void IntrinsicPropertyAttributeWorksForReadOnlyIndexers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[IntrinsicProperty]
	public int this[int x] { get { return 0; } }
}
");

			var impl = FindIndexer(types, "C1", 1, md);
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(impl.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NativeIndexer));
			Assert.That(impl.SetMethod, Is.Null);
		}

		[Test]
		public void IntrinsicPropertyAttributeWorksForWriteOnlyIndexers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[IntrinsicProperty]
	public int this[int x] { set {} }
}
");

			var impl = FindIndexer(types, "C1", 1, md);
			Assert.That(impl.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(impl.GetMethod, Is.Null);
			Assert.That(impl.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NativeIndexer));
		}

		[Test]
		public void IndexerWithIntrinsicPropertyAttributeMustHaveExactlyOneArgument() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[IntrinsicProperty]
	public int this[int x, int y] { set {} }
}
", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1") && er.AllMessages[0].Contains("IntrinsicPropertyAttribute") && er.AllMessages[0].Contains("indexer") && er.AllMessages[0].Contains("exactly one parameter"));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnPropertiesImplementingInterfaceMembers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
interface I {
	int this[int x] { get; set; }
}

class C1 : I {
	[IntrinsicProperty]
	public int this[int x] { get { return 0; } set {} }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Contains("C1") && m.Contains("indexer") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("interface member")));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnPropertiesThatOverrideBaseMembers() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
class B {
	public virtual int this[int x] { get { return 0; } set {} }
}

class D1 : B {
	[IntrinsicProperty]
	public sealed override int this[int x] { get { return 0; } set {} }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Contains("D1") && m.Contains("indexer") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("overrides")));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnInterfaceProperties() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
interface I1 {
	[IntrinsicProperty]
	int this[int x] { get; set; }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Contains("I1") && m.Contains("indexer") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("interface member")));
		}

		[Test]
		public void CannotSpecifyIntrinsicPropertyAttributeOnOverridableProperties() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[IntrinsicProperty]
	public virtual int this[int x] { get { return 0; } set {} }
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Contains("C1") && m.Contains("indexer") && m.Contains("IntrinsicPropertyAttribute") && m.Contains("overridable")));
		}

		[Test]
		public void NonPublicIndexersArePrefixedWithADollarIfSymbolsAreNotMinimized() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C1 {
	public int this[int x] { get { return 0; } set {} }
}

public class C2 {
	private int this[int x] { get { return 0; } set {} }
	internal int this[int x, int y] { get { return 0; } set {} }
}");

			var p1 = FindIndexer(types, "C1", 1, md);
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.GetMethod.Name, Is.EqualTo("get_$item"));
			Assert.That(p1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p1.SetMethod.Name, Is.EqualTo("set_$item"));

			var p2 = FindIndexer(types, "C2", 1, md);
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.GetMethod.Name, Is.EqualTo("get_$item"));
			Assert.That(p2.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p2.SetMethod.Name, Is.EqualTo("set_$item"));

			var p3 = FindIndexer(types, "C2", 2, md);
			Assert.That(p3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
			Assert.That(p3.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.GetMethod.Name, Is.EqualTo("get_$item$1"));
			Assert.That(p3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(p3.SetMethod.Name, Is.EqualTo("set_$item$1"));
		}
	}
}
