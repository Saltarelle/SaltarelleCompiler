using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {

	[TestFixture]
	public class MiscTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void MinimizationOfClassesGeneratesMembersThatAreUniqueInTheHierarchyButDoesNotTouchPublicSymbols() {
			Prepare(
@"class A {
	public void SomeMethod() {}
	public static void SomeStaticMethod() {}
	public void SomeMethod2() {}
	private void SomeMethod(int x) {}
	public virtual void VirtualMethod() {}

	public virtual int Prop1 { get; set; }
	private int Prop2 { get; set; }

	public int this[int x] { get { return 0; } set {} }

	public int Field1;
	internal int Field2;

	public A() {}
	public A(int i) {}
	public A(int i, int j) {}

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

	public static void SomeStaticMethod2() {}
	public static void SomeStaticMethod3() {}

	public B() {}
	public B(int i) {}
	public B(int i, int j) {}
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

	public static void SomeStaticMethod4() {}

	public C() {}
	public C(int i) {}
	public C(int i, int j) {}
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

	public D() {}
	internal D(int i) {}
	private D(int i, int j) {}
	public A(int i, int j, int k) {}
}
");

			Assert.That(FindMethods("A.SomeMethod").Single(m => m.Item1.Parameters.Count == 0).Item2.Name, Is.EqualTo("$0"));
			Assert.That(FindMethod("A.SomeMethod2").Name, Is.EqualTo("$2"));
			Assert.That(FindMethods("A.SomeMethod").Single(m => m.Item1.Parameters.Count == 1).Item2.Name, Is.EqualTo("$1"));
			Assert.That(FindMethod("A.VirtualMethod").Name, Is.EqualTo("$3"));
			Assert.That(FindIndexer("A", 1).GetMethod.Name, Is.EqualTo("$4"));
			Assert.That(FindIndexer("A", 1).SetMethod.Name, Is.EqualTo("$5"));
			Assert.That(FindProperty("A.Prop1").GetMethod.Name, Is.EqualTo("$6"));
			Assert.That(FindProperty("A.Prop1").SetMethod.Name, Is.EqualTo("$7"));
			Assert.That(FindProperty("A.Prop2").GetMethod.Name, Is.EqualTo("$8"));
			Assert.That(FindProperty("A.Prop2").SetMethod.Name, Is.EqualTo("$9"));
			Assert.That(FindField("A.Field1").Name, Is.EqualTo("$A"));
			Assert.That(FindField("A.Field2").Name, Is.EqualTo("$B"));
			Assert.That(FindEvent("A.Evt1").AddMethod.Name, Is.EqualTo("$C"));
			Assert.That(FindEvent("A.Evt1").RemoveMethod.Name, Is.EqualTo("$D"));
			Assert.That(FindEvent("A.Evt2").AddMethod.Name, Is.EqualTo("$E"));
			Assert.That(FindEvent("A.Evt2").RemoveMethod.Name, Is.EqualTo("$F"));
			Assert.That(FindMethod("B.OtherMethodB").Name, Is.EqualTo("$G"));
			Assert.That(FindMethod("B.VirtualMethod").Name, Is.EqualTo("$3"));
			Assert.That(FindProperty("B.Prop3").GetMethod.Name, Is.EqualTo("$H"));
			Assert.That(FindProperty("B.Prop3").SetMethod.Name, Is.EqualTo("$I"));
			Assert.That(FindField("B.Field3").Name, Is.EqualTo("$J"));
			Assert.That(FindEvent("B.Evt2").AddMethod.Name, Is.EqualTo("$E"));
			Assert.That(FindEvent("B.Evt2").RemoveMethod.Name, Is.EqualTo("$F"));
			Assert.That(FindEvent("B.Evt3").AddMethod.Name, Is.EqualTo("$K"));
			Assert.That(FindEvent("B.Evt3").RemoveMethod.Name, Is.EqualTo("$L"));
			Assert.That(FindMethod("C.OtherMethodC").Name, Is.EqualTo("$M"));
			Assert.That(FindMethod("C.SomeMethod").Name, Is.EqualTo("$N"));
			Assert.That(FindMethod("C.VirtualMethod").Name, Is.EqualTo("$3"));
			Assert.That(FindIndexer("C", 1).GetMethod.Name, Is.EqualTo("$O"));
			Assert.That(FindIndexer("C", 1).SetMethod.Name, Is.EqualTo("$P"));
			Assert.That(FindProperty("C.Prop1").GetMethod.Name, Is.EqualTo("$6"));
			Assert.That(FindProperty("C.Prop1").SetMethod.Name, Is.EqualTo("$7"));
			Assert.That(FindProperty("C.Prop2").GetMethod.Name, Is.EqualTo("$Q"));
			Assert.That(FindProperty("C.Prop2").SetMethod.Name, Is.EqualTo("$R"));
			Assert.That(FindField("C.Field1").Name, Is.EqualTo("$S"));
			Assert.That(FindEvent("C.Evt1").AddMethod.Name, Is.EqualTo("$T"));
			Assert.That(FindEvent("C.Evt1").RemoveMethod.Name, Is.EqualTo("$U"));
			Assert.That(FindEvent("C.Evt2").AddMethod.Name, Is.EqualTo("$E"));
			Assert.That(FindEvent("C.Evt2").RemoveMethod.Name, Is.EqualTo("$F"));
			Assert.That(FindMethod("D.PublicMethod").Name, Is.EqualTo("publicMethod"));
			Assert.That(FindMethod("D.ProtectedMethod").Name, Is.EqualTo("protectedMethod"));
			Assert.That(FindMethod("D.ProtectedInternalMethod").Name, Is.EqualTo("protectedInternalMethod"));
			Assert.That(FindMethod("D.PrivateMethod").Name, Is.EqualTo("$0"));
			Assert.That(FindProperty("D.Prop1").GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(FindProperty("D.Prop1").SetMethod.Name, Is.EqualTo("set_prop1"));
			Assert.That(FindProperty("D.Prop2").GetMethod.Name, Is.EqualTo("$1"));
			Assert.That(FindProperty("D.Prop2").SetMethod.Name, Is.EqualTo("$2"));
			Assert.That(FindIndexer("D", 1).GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(FindIndexer("D", 1).SetMethod.Name, Is.EqualTo("set_item"));
			Assert.That(FindField("D.Field1").Name, Is.EqualTo("field1"));
			Assert.That(FindField("D.Field2").Name, Is.EqualTo("field2"));
			Assert.That(FindEvent("D.Evt1").AddMethod.Name, Is.EqualTo("add_evt1"));
			Assert.That(FindEvent("D.Evt1").RemoveMethod.Name, Is.EqualTo("remove_evt1"));

			Assert.That(FindConstructor("A", 0).Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(FindMethod("A.SomeStaticMethod").Name, Is.EqualTo("$0"));
			Assert.That(FindConstructor("A", 1).Name, Is.EqualTo("$1"));
			Assert.That(FindConstructor("A", 2).Name, Is.EqualTo("$2"));
			Assert.That(FindConstructor("B", 0).Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(FindMethod("B.SomeStaticMethod2").Name, Is.EqualTo("$0"));
			Assert.That(FindMethod("B.SomeStaticMethod3").Name, Is.EqualTo("$1"));
			Assert.That(FindConstructor("B", 1).Name, Is.EqualTo("$2"));
			Assert.That(FindConstructor("B", 2).Name, Is.EqualTo("$3"));
			Assert.That(FindConstructor("C", 0).Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(FindMethod("C.SomeStaticMethod4").Name, Is.EqualTo("$0"));
			Assert.That(FindConstructor("C", 1).Name, Is.EqualTo("$1"));
			Assert.That(FindConstructor("C", 2).Name, Is.EqualTo("$2"));

			Assert.That(FindConstructor("D", 0).Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(FindConstructor("D", 1).Name, Is.EqualTo("$0"));
			Assert.That(FindConstructor("D", 2).Name, Is.EqualTo("$1"));
			Assert.That(FindConstructor("D", 3).Name, Is.EqualTo("$ctor1"));
		}

		[Test]
		public void MinimizationOfInterfacesGeneratesNamesThatAreUniqueWithinTheAssemblyButDoesNotTouchPublicInterfaces() {
			Prepare(
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
			var minimized =         FindMethods("I1.SomeMethod")
			                .Concat(FindMethods("I1.SomeMethod2"))
			                .Concat(FindMethods("I2.SomeMethod"))
			                .Concat(FindMethods("I2.SomeMethod2"))
			                .Select(m => m.Item2.Name)
			                .Concat(new[] { FindProperty("I1.Prop1").GetMethod.Name, FindProperty("I1.Prop1").SetMethod.Name,
			                                FindProperty("I1.Prop2").GetMethod.Name, FindProperty("I1.Prop2").SetMethod.Name,
			                                FindIndexer("I1", 1).GetMethod.Name, FindIndexer("I1", 1).SetMethod.Name,
			                                FindEvent("I1.Evt1").AddMethod.Name, FindEvent("I1.Evt1").RemoveMethod.Name,
			                                FindEvent("I1.Evt2").AddMethod.Name, FindEvent("I1.Evt2").RemoveMethod.Name,
			                                FindProperty("I2.Prop1").GetMethod.Name, FindProperty("I2.Prop1").SetMethod.Name,
			                                FindIndexer("I2", 1).GetMethod.Name, FindIndexer("I2", 1).SetMethod.Name,
			                                FindEvent("I2.Evt1").AddMethod.Name, FindEvent("I2.Evt1").RemoveMethod.Name,
			                                FindEvent("I2.Evt3").AddMethod.Name, FindEvent("I2.Evt3").RemoveMethod.Name,
			                        })
			                .OrderBy(x => x)
			                .ToList();

			Assert.That(minimized, Is.EquivalentTo(new[] { "$I1", "$I2", "$I3", "$I4", "$I5", "$I6", "$I7", "$I8", "$I9", "$IA", "$IB", "$IC", "$ID", "$IE", "$IF", "$IG", "$IH", "$II", "$IJ", "$IK", "$IL", "$IM", "$IN", "$IO" }));

			Assert.That(FindMethod("I3.SomeMethod").Name, Is.EqualTo("someMethod"));
			Assert.That(FindProperty("I3.Prop1").GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(FindProperty("I3.Prop1").SetMethod.Name, Is.EqualTo("set_prop1"));
			Assert.That(FindIndexer("I3", 1).GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(FindIndexer("I3", 1).SetMethod.Name, Is.EqualTo("set_item"));
			Assert.That(FindEvent("I3.Evt1").AddMethod.Name, Is.EqualTo("add_evt1"));
			Assert.That(FindEvent("I3.Evt1").RemoveMethod.Name, Is.EqualTo("remove_evt1"));

			Assert.That(FindMethod("I4.SomeMethod").Name, Is.EqualTo("someMethod"));
			Assert.That(FindMethod("I4.SomeMethod").Name, Is.EqualTo("someMethod"));
			Assert.That(FindProperty("I4.Prop1").GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(FindProperty("I4.Prop1").SetMethod.Name, Is.EqualTo("set_prop1"));
			Assert.That(FindIndexer("I4", 1).GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(FindIndexer("I4", 1).SetMethod.Name, Is.EqualTo("set_item"));
			Assert.That(FindEvent("I4.Evt1").AddMethod.Name, Is.EqualTo("add_evt1"));
			Assert.That(FindEvent("I4.Evt1").RemoveMethod.Name, Is.EqualTo("remove_evt1"));
		}
	}
}
