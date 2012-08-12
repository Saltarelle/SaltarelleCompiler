using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporterTests {

	[TestFixture]
	public class MinimizedNamesTests : ScriptSharpMetadataImporterTestBase {
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
			Assert.That(FindField("A.Field1").Name, Is.EqualTo("$a"));
			Assert.That(FindField("A.Field2").Name, Is.EqualTo("$b"));
			Assert.That(FindEvent("A.Evt1").AddMethod.Name, Is.EqualTo("$c"));
			Assert.That(FindEvent("A.Evt1").RemoveMethod.Name, Is.EqualTo("$d"));
			Assert.That(FindEvent("A.Evt2").AddMethod.Name, Is.EqualTo("$e"));
			Assert.That(FindEvent("A.Evt2").RemoveMethod.Name, Is.EqualTo("$f"));
			Assert.That(FindMethod("B.OtherMethodB").Name, Is.EqualTo("$g"));
			Assert.That(FindMethod("B.VirtualMethod").Name, Is.EqualTo("$3"));
			Assert.That(FindProperty("B.Prop3").GetMethod.Name, Is.EqualTo("$h"));
			Assert.That(FindProperty("B.Prop3").SetMethod.Name, Is.EqualTo("$i"));
			Assert.That(FindField("B.Field3").Name, Is.EqualTo("$j"));
			Assert.That(FindEvent("B.Evt2").AddMethod.Name, Is.EqualTo("$e"));
			Assert.That(FindEvent("B.Evt2").RemoveMethod.Name, Is.EqualTo("$f"));
			Assert.That(FindEvent("B.Evt3").AddMethod.Name, Is.EqualTo("$k"));
			Assert.That(FindEvent("B.Evt3").RemoveMethod.Name, Is.EqualTo("$l"));
			Assert.That(FindMethod("C.OtherMethodC").Name, Is.EqualTo("$m"));
			Assert.That(FindMethod("C.SomeMethod").Name, Is.EqualTo("$n"));
			Assert.That(FindMethod("C.VirtualMethod").Name, Is.EqualTo("$3"));
			Assert.That(FindIndexer("C", 1).GetMethod.Name, Is.EqualTo("$o"));
			Assert.That(FindIndexer("C", 1).SetMethod.Name, Is.EqualTo("$p"));
			Assert.That(FindProperty("C.Prop1").GetMethod.Name, Is.EqualTo("$6"));
			Assert.That(FindProperty("C.Prop1").SetMethod.Name, Is.EqualTo("$7"));
			Assert.That(FindProperty("C.Prop2").GetMethod.Name, Is.EqualTo("$q"));
			Assert.That(FindProperty("C.Prop2").SetMethod.Name, Is.EqualTo("$r"));
			Assert.That(FindField("C.Field1").Name, Is.EqualTo("$s"));
			Assert.That(FindEvent("C.Evt1").AddMethod.Name, Is.EqualTo("$t"));
			Assert.That(FindEvent("C.Evt1").RemoveMethod.Name, Is.EqualTo("$u"));
			Assert.That(FindEvent("C.Evt2").AddMethod.Name, Is.EqualTo("$e"));
			Assert.That(FindEvent("C.Evt2").RemoveMethod.Name, Is.EqualTo("$f"));
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
		public void MinimizationHasNoEffectOnInterfaces() {
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

public interface I2 {
	void SomeMethod();

	int Prop1 { get; set; }
	int this[int x] { get; set; }

	event System.EventHandler Evt1;
}

public interface I3 {
	void SomeMethod();

	int Prop1 { get; set; }
	int this[int x] { get; set; }

	event System.EventHandler Evt1;
}");


			Assert.That(FindMethod("I1.SomeMethod", 0).Name, Is.EqualTo("$someMethod"));
			Assert.That(FindMethod("I1.SomeMethod", 1).Name, Is.EqualTo("$someMethod$1"));
			Assert.That(FindMethod("I1.SomeMethod2").Name, Is.EqualTo("$someMethod2"));

			Assert.That(FindProperty("I1.Prop1").GetMethod.Name, Is.EqualTo("get_$prop1"));
			Assert.That(FindProperty("I1.Prop1").SetMethod.Name, Is.EqualTo("set_$prop1"));
			Assert.That(FindProperty("I1.Prop2").GetMethod.Name, Is.EqualTo("get_$prop2"));
			Assert.That(FindProperty("I1.Prop2").SetMethod.Name, Is.EqualTo("set_$prop2"));
			Assert.That(FindIndexer("I1", 1).GetMethod.Name, Is.EqualTo("get_$item"));
			Assert.That(FindIndexer("I1", 1).SetMethod.Name, Is.EqualTo("set_$item"));

			Assert.That(FindEvent("I1.Evt1").AddMethod.Name, Is.EqualTo("add_$evt1"));
			Assert.That(FindEvent("I1.Evt1").RemoveMethod.Name, Is.EqualTo("remove_$evt1"));
			Assert.That(FindEvent("I1.Evt2").AddMethod.Name, Is.EqualTo("add_$evt2"));
			Assert.That(FindEvent("I1.Evt2").RemoveMethod.Name, Is.EqualTo("remove_$evt2"));

			Assert.That(FindMethod("I2.SomeMethod").Name, Is.EqualTo("someMethod"));
			Assert.That(FindProperty("I2.Prop1").GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(FindProperty("I2.Prop1").SetMethod.Name, Is.EqualTo("set_prop1"));
			Assert.That(FindIndexer("I2", 1).GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(FindIndexer("I2", 1).SetMethod.Name, Is.EqualTo("set_item"));
			Assert.That(FindEvent("I2.Evt1").AddMethod.Name, Is.EqualTo("add_evt1"));
			Assert.That(FindEvent("I2.Evt1").RemoveMethod.Name, Is.EqualTo("remove_evt1"));

			Assert.That(FindMethod("I3.SomeMethod").Name, Is.EqualTo("someMethod"));
			Assert.That(FindMethod("I3.SomeMethod").Name, Is.EqualTo("someMethod"));
			Assert.That(FindProperty("I3.Prop1").GetMethod.Name, Is.EqualTo("get_prop1"));
			Assert.That(FindProperty("I3.Prop1").SetMethod.Name, Is.EqualTo("set_prop1"));
			Assert.That(FindIndexer("I3", 1).GetMethod.Name, Is.EqualTo("get_item"));
			Assert.That(FindIndexer("I3", 1).SetMethod.Name, Is.EqualTo("set_item"));
			Assert.That(FindEvent("I3.Evt1").AddMethod.Name, Is.EqualTo("add_evt1"));
			Assert.That(FindEvent("I3.Evt1").RemoveMethod.Name, Is.EqualTo("remove_evt1"));
		}
	}
}
