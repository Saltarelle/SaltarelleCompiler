using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

// ReSharper disable CheckNamespace
namespace Saltarelle.Compiler.Tests.MetadataWriteBackEngineTests.MetadataWriteBackEngineTestCase {
// ReSharper restore CheckNamespace

	[AttributeUsage(AttributeTargets.All)]
	public sealed class MyAttribute : Attribute {
		public MyAttribute(string text) {
		}
	}

	[MyAttribute("This class has an attribute")]
	public class ClassWithAttribute {
	}

	public class ClassWithAttributedField {
		[MyAttribute("This field has an attribute")] public int MyField;
	}

	public class ClassWithAttributedProperty {
		[MyAttribute("This property has an attribute")] public int MyProperty { [MyAttribute("This getter has an attribute")] get { return 0; } [MyAttribute("This setter has an attribute")] set {} }
	}

	public interface IInterfaceWithProperty<T, T2> {
		T MyProperty { get; set; }
	}

	public class ClassWithAttributedExplicitlyImplementedProperty : IInterfaceWithProperty<int, string> {
		[MyAttribute("This property has an attribute")] int IInterfaceWithProperty<int, string>.MyProperty { [MyAttribute("This getter has an attribute")] get { return 0; } [MyAttribute("This setter has an attribute")] set {} }
	}

	public class ClassWithAttributedIndexers {
		[MyAttribute("Indexer(int)")]
		public int this[int a] { [MyAttribute("Indexer(int) getter")] get { return 0; } [MyAttribute("Indexer(int) setter")] set {} }
		[MyAttribute("Indexer(int,int)")]
		public int this[int a, int b] { [MyAttribute("Indexer(int,int) getter")] get { return 0; } [MyAttribute("Indexer(int,int) setter")] set {} }
		[MyAttribute("Indexer(int,string)")]
		public int this[int a, string b] { [MyAttribute("Indexer(int,string) getter")] get { return 0; } [MyAttribute("Indexer(int,string) setter")] set {} }
	}

	public interface IInterfaceWithIndexers<T1, T2, T3> {
		int this[T1 a] { get; set; }
		int this[T1 a, T2 b] { get; set; }
		int this[T2 a, T3 b] { get; set; }
	}

	public class ClassWithAttributedExplicitlyImplementedIndexers : IInterfaceWithIndexers<int, int, string> {
		[MyAttribute("Indexer(int)")]
		int IInterfaceWithIndexers<int, int, string>.this[int a] { [MyAttribute("Indexer(int) getter")] get { return 0; } [MyAttribute("Indexer(int) setter")] set {} }
		[MyAttribute("Indexer(int,int)")]
		int IInterfaceWithIndexers<int, int, string>.this[int a, int b] { [MyAttribute("Indexer(int,int) getter")] get { return 0; } [MyAttribute("Indexer(int,int) setter")] set {} }
		[MyAttribute("Indexer(int,string)")]
		int IInterfaceWithIndexers<int, int, string>.this[int a, string b] { [MyAttribute("Indexer(int,string) getter")] get { return 0; } [MyAttribute("Indexer(int,string) setter")] set {} }
	}

	public class ClassWithAttributedEvent {
		[MyAttribute("This event has an attribute")]
		public event EventHandler MyEvent { [MyAttribute("This event adder has an attribute")] add {} [MyAttribute("This event remover has an attribute")] remove {} }
	}

	public interface IInterfaceWithEvent<T> {
		event EventHandler MyEvent;
	}

	public class ClassWithAttributedExplicitlyImplementedEventAccessors : IInterfaceWithEvent<int> {
		[MyAttribute("This event has an attribute")]
		public event EventHandler MyEvent { [MyAttribute("This event adder has an attribute")] add {} [MyAttribute("This event remover has an attribute")] remove {} }
	}

	public class ClassWithMethods {
		[MyAttribute("MyMethod()")]
		public void MyMethod() {}

		[MyAttribute("MyMethod(int)")]
		public void MyMethod(int a) {}

		[MyAttribute("MyMethod(int,int)")]
		public void MyMethod(int a, int b) {}

		[MyAttribute("MyMethod(int,string)")]
		public void MyMethod(int a, string b) {}
	}

	public interface IInterfaceWithMethods<T1, T2, T3> {
		void MyMethod();
		void MyMethod(T1 a);
		void MyMethod(T1 a, T2 b);
		void MyMethod(T2 a, T3 b);
	}

	public class ClassWithAttributedExplicitlyImplementedMethods : IInterfaceWithMethods<int, int, string> {
		[MyAttribute("MyMethod()")]
		void IInterfaceWithMethods<int, int, string>.MyMethod() {}

		[MyAttribute("MyMethod(int)")]
		void IInterfaceWithMethods<int, int, string>.MyMethod(int a) {}

		[MyAttribute("MyMethod(int,int)")]
		void IInterfaceWithMethods<int, int, string>.MyMethod(int a, int b) {}

		[MyAttribute("MyMethod(int,string)")]
		void IInterfaceWithMethods<int, int, string>.MyMethod(int a, string b) {}
	}

	public class GenericClassWithAttributedExplicitlyImplementedMethods<T> : IInterfaceWithMethods<T, int, string> {
		[MyAttribute("MyMethod()")]
		void IInterfaceWithMethods<T, int, string>.MyMethod() {}

		[MyAttribute("MyMethod(int)")]
		void IInterfaceWithMethods<T, int, string>.MyMethod(T a) {}

		[MyAttribute("MyMethod(int,int)")]
		void IInterfaceWithMethods<T, int, string>.MyMethod(T a, int b) {}

		[MyAttribute("MyMethod(int,string)")]
		void IInterfaceWithMethods<T, int, string>.MyMethod(int a, string b) {}
	}

	public class ClassWithOperators {
		[MyAttribute("Add class instances")]
		public static ClassWithOperators operator+(ClassWithOperators a, ClassWithOperators b) {
			return null;
		}

		[MyAttribute("Add class and int")]
		public static ClassWithOperators operator+(ClassWithOperators a, int b) {
			return null;
		}

		[MyAttribute("Convert to int")]
		public static implicit operator int(ClassWithOperators a) {
			return 0;
		}

		[MyAttribute("Convert to float")]
		public static implicit operator float(ClassWithOperators a) {
			return 0;
		}

		[MyAttribute("Convert to string")]
		public static explicit operator string(ClassWithOperators a) {
			return null;
		}
	}

	public class ClassWithConstructors {
		[MyAttribute("Constructor()")]
		ClassWithConstructors() {}

		[MyAttribute("Constructor(int)")]
		ClassWithConstructors(int a) {}

		[MyAttribute("Constructor(int,int)")]
		ClassWithConstructors(int a, int b) {}

		[MyAttribute("Constructor(int,string)")]
		ClassWithConstructors(int a, string b) {}
	}

	public class ComplexAttribute : Attribute {
		public ComplexAttribute() {}
		public ComplexAttribute(int a) {}
		public ComplexAttribute(byte a) {}
		public ComplexAttribute(int a, string b) {}
		public ComplexAttribute(byte a, string b) {}

		public string Property1 { get; set; }
		public int Property2 { get; set; }
		public string Property3 { get; set; }
		public string Property4 { get; set; }
		public byte Field1;
		public string Field2;
	}

	public class ClassWithAConstructorWithAComplexAttribute {
		[ComplexAttribute((byte)42, "Some value", Property1 = "Property 1 value", Property2 = 347, Property3 = null, Field1 = 12)]
		public ClassWithAConstructorWithAComplexAttribute() {}
	}

	public class UnattributedClass {
		public void SomeMethod() {}
	}
}
