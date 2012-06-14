using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {

	[TestFixture]
	public class MiscTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void MinimizationOfClassesGeneratesMembersThatAreUniqueInTheHierarchyButDoesNotTouchPublicSymbols() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"class A {
	public void SomeMethod() {}
	public static void SomeMethod2() {}
	private void SomeMethod(int x) {}
	public virtual void VirtualMethod() {}

	public virtual int Prop1 { get; set; }
	private int Prop2 { get; set; }

	public int this[int x] { get { return 0; } set {} }

	public int Field1;
	internal int Field2;

	public event System.EventHandler Evt1;
	public virtual event System.EventHandler Evt2;
}

class B : A {
	public void OtherMethodB() {}

	public override void VirtualMethod() {}

	public int Prop3 { get; set; }

	private int Field3;

	private event System.EventHandler Evt3;
	public override event System.EventHandler Evt2;
}

class C : B {
	private void OtherMethodC() {}
	public new void SomeMethod() {}
	public sealed override void VirtualMethod() {}

	public override int Prop1 { get; set; }
	public new int Prop2 { get; set; }

	public new int this[int x] { get { return 0; } set {} }

	public new int Field1;

	public new event System.EventHandler Evt1;
	public override event System.EventHandler Evt2;
}

public class D {
	public void PublicMethod() {}
	protected void ProtectedMethod() {}
	protected internal void ProtectedInternalMethod() {}
	private void PrivateMethod() {}

	public int Prop1 { get; set; }
	private int Prop2 { get; set; }

	public int this[int x] { get { return 0; } set {} }

	public int Field1;
	public int Field2;

	public event System.EventHandler Evt1;
}
");

			Assert.That(FindMethods(types, "A.SomeMethod", md).Single(m => m.Item1.Parameters.Count == 0).Item2.Name, Is.EqualTo("$0"));
			Assert.That(FindMethod(types, "A.SomeMethod2", md).Name, Is.EqualTo("$2"));
			Assert.That(FindMethods(types, "A.SomeMethod", md).Single(m => m.Item1.Parameters.Count == 1).Item2.Name, Is.EqualTo("$1"));
			Assert.That(FindMethod(types, "A.VirtualMethod", md).Name, Is.EqualTo("$3"));
			Assert.That(FindIndexer(types, "A", 1, md).GetMethod.Name, Is.EqualTo("$4"));
			Assert.That(FindIndexer(types, "A", 1, md).SetMethod.Name, Is.EqualTo("$5"));
			Assert.That(FindProperty(types, "A.Prop1", md).GetMethod.Name, Is.EqualTo("$6"));
			Assert.That(FindProperty(types, "A.Prop1", md).SetMethod.Name, Is.EqualTo("$7"));
			Assert.That(FindProperty(types, "A.Prop2", md).GetMethod.Name, Is.EqualTo("$8"));
			Assert.That(FindProperty(types, "A.Prop2", md).SetMethod.Name, Is.EqualTo("$9"));
			Assert.That(FindField(types, "A.Field1", md).Name, Is.EqualTo("$A"));
			Assert.That(FindField(types, "A.Field2", md).Name, Is.EqualTo("$B"));
			Assert.That(FindEvent(types, "A.Evt1", md).AddMethod.Name, Is.EqualTo("$C"));
			Assert.That(FindEvent(types, "A.Evt1", md).RemoveMethod.Name, Is.EqualTo("$D"));
			Assert.That(FindEvent(types, "A.Evt2", md).AddMethod.Name, Is.EqualTo("$E"));
			Assert.That(FindEvent(types, "A.Evt2", md).RemoveMethod.Name, Is.EqualTo("$F"));
			Assert.That(FindMethod(types, "B.OtherMethodB", md).Name, Is.EqualTo("$G"));
			Assert.That(FindMethod(types, "B.VirtualMethod", md).Name, Is.EqualTo("$3"));
			Assert.That(FindProperty(types, "B.Prop3", md).GetMethod.Name, Is.EqualTo("$H"));
			Assert.That(FindProperty(types, "B.Prop3", md).SetMethod.Name, Is.EqualTo("$I"));
			Assert.That(FindField(types, "B.Field3", md).Name, Is.EqualTo("$J"));
			Assert.That(FindEvent(types, "B.Evt2", md).AddMethod.Name, Is.EqualTo("$E"));
			Assert.That(FindEvent(types, "B.Evt2", md).RemoveMethod.Name, Is.EqualTo("$F"));
			Assert.That(FindEvent(types, "B.Evt3", md).AddMethod.Name, Is.EqualTo("$K"));
			Assert.That(FindEvent(types, "B.Evt3", md).RemoveMethod.Name, Is.EqualTo("$L"));
			Assert.That(FindMethod(types, "C.OtherMethodC", md).Name, Is.EqualTo("$M"));
			Assert.That(FindMethod(types, "C.SomeMethod", md).Name, Is.EqualTo("$N"));
			Assert.That(FindMethod(types, "C.VirtualMethod", md).Name, Is.EqualTo("$3"));
			Assert.That(FindIndexer(types, "C", 1, md).GetMethod.Name, Is.EqualTo("$O"));
			Assert.That(FindIndexer(types, "C", 1, md).SetMethod.Name, Is.EqualTo("$P"));
			Assert.That(FindProperty(types, "C.Prop1", md).GetMethod.Name, Is.EqualTo("$6"));
			Assert.That(FindProperty(types, "C.Prop1", md).SetMethod.Name, Is.EqualTo("$7"));
			Assert.That(FindProperty(types, "C.Prop2", md).GetMethod.Name, Is.EqualTo("$Q"));
			Assert.That(FindProperty(types, "C.Prop2", md).SetMethod.Name, Is.EqualTo("$R"));
			Assert.That(FindField(types, "C.Field1", md).Name, Is.EqualTo("$S"));
			Assert.That(FindEvent(types, "C.Evt1", md).AddMethod.Name, Is.EqualTo("$T"));
			Assert.That(FindEvent(types, "C.Evt1", md).RemoveMethod.Name, Is.EqualTo("$U"));
			Assert.That(FindEvent(types, "C.Evt2", md).AddMethod.Name, Is.EqualTo("$E"));
			Assert.That(FindEvent(types, "C.Evt2", md).RemoveMethod.Name, Is.EqualTo("$F"));
			Assert.That(FindMethod(types, "D.PublicMethod", md).Name, Is.EqualTo("publicMethod"));
			Assert.That(FindMethod(types, "D.ProtectedMethod", md).Name, Is.EqualTo("protectedMethod"));
			Assert.That(FindMethod(types, "D.ProtectedInternalMethod", md).Name, Is.EqualTo("protectedInternalMethod"));
			Assert.That(FindMethod(types, "D.PrivateMethod", md).Name, Is.EqualTo("$0"));
			Assert.That(FindProperty(types, "D.Prop1", md).GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(FindProperty(types, "D.Prop1", md).SetMethod.Name, Is.EqualTo("set_prop1"));
			Assert.That(FindProperty(types, "D.Prop2", md).GetMethod.Name, Is.EqualTo("$1"));
			Assert.That(FindProperty(types, "D.Prop2", md).SetMethod.Name, Is.EqualTo("$2"));
			Assert.That(FindIndexer(types, "D", 1, md).GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(FindIndexer(types, "D", 1, md).SetMethod.Name, Is.EqualTo("set_item"));
			Assert.That(FindField(types, "D.Field1", md).Name, Is.EqualTo("field1"));
			Assert.That(FindField(types, "D.Field2", md).Name, Is.EqualTo("field2"));
			Assert.That(FindEvent(types, "D.Evt1", md).AddMethod.Name, Is.EqualTo("add_evt1"));
			Assert.That(FindEvent(types, "D.Evt1", md).RemoveMethod.Name, Is.EqualTo("remove_evt1"));

			Assert.Inconclusive("TODO: Test constructors");
		}

		[Test]
		public void MinimizationOfInterfacesGeneratesNamesThatAreUniqueWithinTheAssemblyButDoesNotTouchPublicInterfaces() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"interface I1 {
	void SomeMethod();
	void SomeMethod(int x);
	void SomeMethod2();

	int Prop1 { get; set; }
	int Prop2 { get; set; }
	int this[int x] { get; set; }

	event System.EventHandler Evt1;
	event System.EventHandler Evt2;
}

interface I2 {
	void SomeMethod();
	void SomeMethod(int x);
	void SomeMethod2();

	int Prop1 { get; set; }
	int this[int x] { get; set; }

	event System.EventHandler Evt1;
	event System.EventHandler Evt3;
}

public interface I3 {
	void SomeMethod();

	int Prop1 { get; set; }
	int this[int x] { get; set; }

	event System.EventHandler Evt1;
}

public interface I4 {
	void SomeMethod();

	int Prop1 { get; set; }
	int this[int x] { get; set; }

	event System.EventHandler Evt1;
}");
			var minimized =         FindMethods(types, "I1.SomeMethod", md)
			                .Concat(FindMethods(types, "I1.SomeMethod2", md))
			                .Concat(FindMethods(types, "I2.SomeMethod", md))
			                .Concat(FindMethods(types, "I2.SomeMethod2", md))
			                .Select(m => m.Item2.Name)
			                .Concat(new[] { FindProperty(types, "I1.Prop1", md).GetMethod.Name, FindProperty(types, "I1.Prop1", md).SetMethod.Name,
			                                FindProperty(types, "I1.Prop2", md).GetMethod.Name, FindProperty(types, "I1.Prop2", md).SetMethod.Name,
			                                FindIndexer(types, "I1", 1, md).GetMethod.Name, FindIndexer(types, "I1", 1, md).SetMethod.Name,
			                                FindEvent(types, "I1.Evt1", md).AddMethod.Name, FindEvent(types, "I1.Evt1", md).RemoveMethod.Name,
			                                FindEvent(types, "I1.Evt2", md).AddMethod.Name, FindEvent(types, "I1.Evt2", md).RemoveMethod.Name,
			                                FindProperty(types, "I2.Prop1", md).GetMethod.Name, FindProperty(types, "I2.Prop1", md).SetMethod.Name,
			                                FindIndexer(types, "I2", 1, md).GetMethod.Name, FindIndexer(types, "I2", 1, md).SetMethod.Name,
			                                FindEvent(types, "I2.Evt1", md).AddMethod.Name, FindEvent(types, "I2.Evt1", md).RemoveMethod.Name,
			                                FindEvent(types, "I2.Evt3", md).AddMethod.Name, FindEvent(types, "I2.Evt3", md).RemoveMethod.Name,
			                        })
			                .OrderBy(x => x)
			                .ToList();

			Assert.That(minimized, Is.EquivalentTo(new[] { "$I1", "$I2", "$I3", "$I4", "$I5", "$I6", "$I7", "$I8", "$I9", "$IA", "$IB", "$IC", "$ID", "$IE", "$IF", "$IG", "$IH", "$II", "$IJ", "$IK", "$IL", "$IM", "$IN", "$IO" }));

			Assert.That(FindMethod(types, "I3.SomeMethod", md).Name, Is.EqualTo("someMethod"));
			Assert.That(FindProperty(types, "I3.Prop1", md).GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(FindProperty(types, "I3.Prop1", md).SetMethod.Name, Is.EqualTo("set_prop1"));
			Assert.That(FindIndexer(types, "I3", 1, md).GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(FindIndexer(types, "I3", 1, md).SetMethod.Name, Is.EqualTo("set_item"));
			Assert.That(FindEvent(types, "I3.Evt1", md).AddMethod.Name, Is.EqualTo("add_evt1"));
			Assert.That(FindEvent(types, "I3.Evt1", md).RemoveMethod.Name, Is.EqualTo("remove_evt1"));

			Assert.That(FindMethod(types, "I4.SomeMethod", md).Name, Is.EqualTo("someMethod"));
			Assert.That(FindMethod(types, "I4.SomeMethod", md).Name, Is.EqualTo("someMethod"));
			Assert.That(FindProperty(types, "I4.Prop1", md).GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(FindProperty(types, "I4.Prop1", md).SetMethod.Name, Is.EqualTo("set_prop1"));
			Assert.That(FindIndexer(types, "I4", 1, md).GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(FindIndexer(types, "I4", 1, md).SetMethod.Name, Is.EqualTo("set_item"));
			Assert.That(FindEvent(types, "I4.Evt1", md).AddMethod.Name, Is.EqualTo("add_evt1"));
			Assert.That(FindEvent(types, "I4.Evt1", md).RemoveMethod.Name, Is.EqualTo("remove_evt1"));
		}
	}
}
