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
	public void SomeMethod(int x) {}
	public virtual void VirtualMethod() {}

	public virtual int Prop1 { get; set; }
	public int Prop2 { get; set; }

	public int this[int x] { get { return 0; } set {} }

	public int Field1;
	public int Field2;
}

class B : A {
	public void OtherMethodB() {}

	public override void VirtualMethod();

	public int Prop3 { get; set; }

	public int Field3;
}

class C : B {
	public void OtherMethodC() {}
	public new void SomeMethod() {}
	public sealed override void VirtualMethod() {}

	public override int Prop1 { get; set; }
	public new int Prop2 { get; set; }

	public new int this[int x] { get { return 0; } set {} }

	public new int Field1;
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
			Assert.That(FindMethod(types, "B.OtherMethodB", md).Name, Is.EqualTo("$C"));
			Assert.That(FindMethod(types, "B.VirtualMethod", md).Name, Is.EqualTo("$3"));
			Assert.That(FindProperty(types, "B.Prop3", md).GetMethod.Name, Is.EqualTo("$D"));
			Assert.That(FindProperty(types, "B.Prop3", md).SetMethod.Name, Is.EqualTo("$E"));
			Assert.That(FindField(types, "B.Field3", md).Name, Is.EqualTo("$F"));
			Assert.That(FindMethod(types, "C.OtherMethodC", md).Name, Is.EqualTo("$G"));
			Assert.That(FindMethod(types, "C.SomeMethod", md).Name, Is.EqualTo("$H"));
			Assert.That(FindMethod(types, "C.VirtualMethod", md).Name, Is.EqualTo("$3"));
			Assert.That(FindIndexer(types, "C", 1, md).GetMethod.Name, Is.EqualTo("$I"));
			Assert.That(FindIndexer(types, "C", 1, md).SetMethod.Name, Is.EqualTo("$J"));
			Assert.That(FindProperty(types, "C.Prop1", md).GetMethod.Name, Is.EqualTo("$6"));
			Assert.That(FindProperty(types, "C.Prop1", md).SetMethod.Name, Is.EqualTo("$7"));
			Assert.That(FindProperty(types, "C.Prop2", md).GetMethod.Name, Is.EqualTo("$K"));
			Assert.That(FindProperty(types, "C.Prop2", md).SetMethod.Name, Is.EqualTo("$L"));
			Assert.That(FindField(types, "C.Field1", md).Name, Is.EqualTo("$M"));
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

			Assert.Inconclusive("TODO: Test events, constructors");
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
}

interface I2 {
	void SomeMethod();
	void SomeMethod(int x);
	void SomeMethod2();

	int Prop1 { get; set; }
	int this[int x] { get; set; }
}

public interface I3 {
	void SomeMethod();

	int Prop1 { get; set; }
	int this[int x] { get; set; }
}

public interface I4 {
	void SomeMethod();

	int Prop1 { get; set; }
	int this[int x] { get; set; }
}");
			var minimized =         FindMethods(types, "I1.SomeMethod", md)
			                .Concat(FindMethods(types, "I1.SomeMethod2", md))
			                .Concat(FindMethods(types, "I2.SomeMethod", md))
			                .Concat(FindMethods(types, "I2.SomeMethod2", md))
			                .Select(m => m.Item2.Name)
			                .Concat(new[] { FindProperty(types, "I1.Prop1", md).GetMethod.Name, FindProperty(types, "I1.Prop1", md).SetMethod.Name,
			                                FindProperty(types, "I1.Prop2", md).GetMethod.Name, FindProperty(types, "I1.Prop2", md).SetMethod.Name,
			                                FindIndexer(types, "I1", 1, md).GetMethod.Name, FindIndexer(types, "I1", 1, md).SetMethod.Name,
			                                FindProperty(types, "I2.Prop1", md).GetMethod.Name, FindProperty(types, "I2.Prop1", md).SetMethod.Name,
			                                FindIndexer(types, "I2", 1, md).GetMethod.Name, FindIndexer(types, "I2", 1, md).SetMethod.Name,
			                        })
			                .OrderBy(x => x)
			                .ToList();

			Assert.That(minimized, Is.EquivalentTo(new[] { "$I1", "$I2", "$I3", "$I4", "$I5", "$I6", "$I7", "$I8", "$I9", "$IA", "$IB", "$IC", "$ID", "$IE", "$IF", "$IG" }));

			Assert.That(FindMethod(types, "I3.SomeMethod", md).Name, Is.EqualTo("someMethod"));
			Assert.That(FindProperty(types, "I3.Prop1", md).GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(FindProperty(types, "I3.Prop1", md).SetMethod.Name, Is.EqualTo("set_prop1"));
			Assert.That(FindIndexer(types, "I3", 1, md).GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(FindIndexer(types, "I3", 1, md).SetMethod.Name, Is.EqualTo("set_item"));

			Assert.That(FindMethod(types, "I4.SomeMethod", md).Name, Is.EqualTo("someMethod"));
			Assert.That(FindMethod(types, "I4.SomeMethod", md).Name, Is.EqualTo("someMethod"));
			Assert.That(FindProperty(types, "I4.Prop1", md).GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(FindProperty(types, "I4.Prop1", md).SetMethod.Name, Is.EqualTo("set_prop1"));
			Assert.That(FindIndexer(types, "I4", 1, md).GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(FindIndexer(types, "I4", 1, md).SetMethod.Name, Is.EqualTo("set_item"));

			Assert.Inconclusive("TODO: events");
		}
	}
}
