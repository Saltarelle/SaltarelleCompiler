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

	public interface IMyInterface<T, T2> {
		T MyProperty { get; set; }
	}

	public class ClassWithAttributedExplicitlyImplementedProperty : IMyInterface<int, string> {
		[MyAttribute("This property has an attribute")] int IMyInterface<int, string>.MyProperty { [MyAttribute("This getter has an attribute")] get { return 0; } [MyAttribute("This setter has an attribute")] set {} }
	}

	public class ClassWithAttributedIndexers {
		[MyAttribute("Indexer(int)")]
		public int this[int a] { [MyAttribute("Indexer(int) getter")] get { return 0; } [MyAttribute("Indexer(int) setter")] set {} }
		[MyAttribute("Indexer(int,int)")]
		public int this[int a, int b] { [MyAttribute("Indexer(int,int) getter")] get { return 0; } [MyAttribute("Indexer(int,int) setter")] set {} }
		[MyAttribute("Indexer(int,string)")]
		public int this[int a, string b] { [MyAttribute("Indexer(int,string) getter")] get { return 0; } [MyAttribute("Indexer(int,string) setter")] set {} }
	}
}
