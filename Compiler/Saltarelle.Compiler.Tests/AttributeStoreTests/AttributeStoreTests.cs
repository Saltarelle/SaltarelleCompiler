using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

#if false // This class cannot be in the app domain when running the driver tests
namespace System.Runtime.CompilerServices.Internal {
	public class ScriptSerializableAttribute : Attribute {
		public string TypeCheckCode { get; private set; }

		public ScriptSerializableAttribute(string typeCheckCode) {
			TypeCheckCode = typeCheckCode;
		}
	}
}
#endif

namespace Saltarelle.Compiler.Tests.AttributeStoreTests {
	[TestFixture]
	public class AttributeStoreTests {
		private static readonly Lazy<MetadataReference> _attributeReference = new Lazy<MetadataReference>(CompileAttributesAssembly);

		private static MetadataReference CompileAttributesAssembly() {
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(AttributeStoreTests), "TestAttributes.cs"))
			using (var reader = new StreamReader(stream)) {
				var c = Common.CreateCompilation(reader.ReadToEnd(), defineConstants: new[] { "TEST_ASSEMBLY" }, assemblyName: "TestAttributes");
				return c.ToMetadataReference();
			}
		}

		private AttributeList Process(string source, Func<Compilation, ISymbol> getSymbol, IErrorReporter errorReporter = null, IEnumerable<IAutomaticMetadataAttributeApplier> automaticMetadataAppliers = null) {
			bool defaultErrorHandling = errorReporter == null;
			errorReporter = errorReporter ?? new MockErrorReporter(true);
			var c = Common.CreateCompilation("using System; using Saltarelle.Compiler.Tests.AttributeStoreTests; " + source, new[] { Common.Mscorlib, _attributeReference.Value });
			var s = new AttributeStore(c, errorReporter, automaticMetadataAppliers ?? new IAutomaticMetadataAttributeApplier[0]);
			if (defaultErrorHandling) {
				Assert.That(((MockErrorReporter)errorReporter).AllMessages, Is.Empty, "Errors:" + Environment.NewLine + string.Join(Environment.NewLine, ((MockErrorReporter)errorReporter).AllMessages.Select(m => m.FormattedMessage)));
			}

			return s.AttributesFor(getSymbol(c));
		}

		[Test]
		public void CanReadAssemblyAttributes() {
			var attributes = Process("[assembly: Test1]", c => c.Assembly);

			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<Test1Attribute>());
		}

		[Test]
		public void CanReadTypeAttributes() {
			var attributes = Process("[Test1] class C {}", c => c.GetTypeByMetadataName("C"));
			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<Test1Attribute>());
		}

		[Test]
		public void CanReadNestedTypeAttributes() {
			var attributes = Process("class C { [Test1] class X {} }", c => c.GetTypeByMetadataName("C+X"));
			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<Test1Attribute>());
		}

		[Test]
		public void CanReadMethodAttributes() {
			var attributes = Process("class C { [Test1] void M() {} }", c => c.GetTypeByMetadataName("C").GetMembers("M").Single());
			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<Test1Attribute>());
		}

		[Test]
		public void CanReadFieldAttributes() {
			var attributes = Process("class C { [Test1] int f; }", c => c.GetTypeByMetadataName("C").GetMembers("f").Single());
			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<Test1Attribute>());
		}

		[Test]
		public void CanReadPropertyAndPropertyAccessorAttributes() {
			var attributes = Process("class C { [Test1] int P { get; set; } }", c => c.GetTypeByMetadataName("C").GetMembers("P").Single());
			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<Test1Attribute>());

			attributes = Process("class C { [Test1] int P { get; set; } }", c => ((IPropertySymbol)c.GetTypeByMetadataName("C").GetMembers("P").Single()).GetMethod);
			Assert.That(attributes, Is.Empty);

			attributes = Process("class C { [Test1] int P { get; set; } }", c => ((IPropertySymbol)c.GetTypeByMetadataName("C").GetMembers("P").Single()).SetMethod);
			Assert.That(attributes, Is.Empty);

			attributes = Process("class C { [Test1] int P { get { return 0; } } }", c => ((IPropertySymbol)c.GetTypeByMetadataName("C").GetMembers("P").Single()).GetMethod);
			Assert.That(attributes, Is.Empty);

			attributes = Process("class C { [Test1] int P { set {} } }", c => ((IPropertySymbol)c.GetTypeByMetadataName("C").GetMembers("P").Single()).SetMethod);
			Assert.That(attributes, Is.Empty);

			attributes = Process("class C { int P { [Test1] get; set; } }", c => c.GetTypeByMetadataName("C").GetMembers("P").Single());
			Assert.That(attributes, Is.Empty);

			attributes = Process("class C { int P { [Test1] get; set; } }", c => ((IPropertySymbol)c.GetTypeByMetadataName("C").GetMembers("P").Single()).GetMethod);
			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<Test1Attribute>());

			attributes = Process("class C { int P { get; [Test1] set; } }", c => c.GetTypeByMetadataName("C").GetMembers("P").Single());
			Assert.That(attributes, Is.Empty);

			attributes = Process("class C { int P { get; [Test1] set; } }", c => ((IPropertySymbol)c.GetTypeByMetadataName("C").GetMembers("P").Single()).SetMethod);
			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<Test1Attribute>());
		}

		[Test]
		public void CanReadEventAndEventAccessorAttributes() {
			var attributes = Process("class C { [Test1] event Action e; }", c => c.GetTypeByMetadataName("C").GetMembers("e").Single());
			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<Test1Attribute>());

			attributes = Process("class C { [Test1] event Action e; }", c => ((IEventSymbol)c.GetTypeByMetadataName("C").GetMembers("e").Single()).AddMethod);
			Assert.That(attributes, Is.Empty);

			attributes = Process("class C { [Test1] event Action e; }", c => ((IEventSymbol)c.GetTypeByMetadataName("C").GetMembers("e").Single()).RemoveMethod);
			Assert.That(attributes, Is.Empty);

			attributes = Process("class C { [Test1] event Action e { add {} remove {} } }", c => ((IEventSymbol)c.GetTypeByMetadataName("C").GetMembers("e").Single()).AddMethod);
			Assert.That(attributes, Is.Empty);

			attributes = Process("class C { [Test1] event Action e { add {} remove {} } }", c => ((IEventSymbol)c.GetTypeByMetadataName("C").GetMembers("e").Single()).RemoveMethod);
			Assert.That(attributes, Is.Empty);

			attributes = Process("class C { event Action e { [Test1] add {} remove {} } }", c => c.GetTypeByMetadataName("C").GetMembers("e").Single());
			Assert.That(attributes, Is.Empty);

			attributes = Process("class C { event Action e { [Test1] add {} remove {} } }", c => ((IEventSymbol)c.GetTypeByMetadataName("C").GetMembers("e").Single()).AddMethod);
			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<Test1Attribute>());

			attributes = Process("class C { event Action e { add {} [Test1] remove {} } }", c => c.GetTypeByMetadataName("C").GetMembers("e").Single());
			Assert.That(attributes, Is.Empty);

			attributes = Process("class C { event Action e { add {} [Test1] remove {} } }", c => ((IEventSymbol)c.GetTypeByMetadataName("C").GetMembers("e").Single()).RemoveMethod);
			Assert.That(attributes, Has.Count.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<Test1Attribute>());
		}

		[Test]
		public void CanUseConstructorArguments() {
			var attr = Process("[assembly: Test2(42)]", c => c.Assembly).Cast<Test2Attribute>().Single();
			Assert.That(attr.X, Is.EqualTo(42));
		}

		[Test]
		public void CanUseDefaultConstructorArguments() {
			var attr = Process("[assembly: Test2(\"Hello\")]", c => c.Assembly).Cast<Test2Attribute>().Single();
			Assert.That(attr.S, Is.EqualTo("Hello"));
			Assert.That(attr.X, Is.EqualTo(12));
		}

		[Test]
		public void CanUseCallerInformationInConstructorArguments() {
			var attr = Process(Environment.NewLine + Environment.NewLine + "[Test8] class C {}", c => c.GetTypeByMetadataName("C")).Cast<Test8Attribute>().Single();
			Assert.That(attr.Line, Is.EqualTo(3));
			Assert.That(attr.Path, Is.EqualTo("File0.cs"));
			Assert.That(attr.Member, Is.Null);

			attr = Process(Environment.NewLine + "class C { [Test8] void M() {} }", c => c.GetTypeByMetadataName("C").GetMembers("M").Single()).Cast<Test8Attribute>().Single();
			Assert.That(attr.Line, Is.EqualTo(2));
			Assert.That(attr.Path, Is.EqualTo("File0.cs"));
			Assert.That(attr.Member, Is.EqualTo("M"));
		}

		[Test]
		public void CanUseNamedArgumentsForProperty() {
			var attr = Process("[assembly: Test3(P1 = 42)]", c => c.Assembly).Cast<Test3Attribute>().Single();
			Assert.That(attr.P1, Is.EqualTo(42));
			Assert.That(attr.F1, Is.EqualTo(0));
		}

		[Test]
		public void CanUseNamedArgumentsForField() {
			var attr = Process("[assembly: Test3(F1 = 42)]", c => c.Assembly).Cast<Test3Attribute>().Single();
			Assert.That(attr.P1, Is.EqualTo(0));
			Assert.That(attr.F1, Is.EqualTo(42));
		}

		[Test]
		public void CanUsedConstructorAndNamedArguments() {
			var attr = Process("[assembly: Test3(\"Hello\", P1 = 42, F1 = 17)]", c => c.Assembly).Cast<Test3Attribute>().Single();
			Assert.That(attr.S, Is.EqualTo("Hello"));
			Assert.That(attr.P1, Is.EqualTo(42));
			Assert.That(attr.F1, Is.EqualTo(17));
		}

		[Test]
		public void CanUseNamedArgumentForBaseMember() {
			var attr = Process("[assembly: Test4(P1 = 42, F1 = 17, P2 = 38, F2 = 73)]", c => c.Assembly).Cast<Test4Attribute>().Single();
			Assert.That(attr.P1, Is.EqualTo(42));
			Assert.That(attr.F1, Is.EqualTo(17));
			Assert.That(attr.P2, Is.EqualTo(38));
			Assert.That(attr.F2, Is.EqualTo(73));
		}

		[Test]
		public void CanUseAllNonArrayNonTypeofLiterals() {
			var attr = Process("[assembly: Test5(d = 4.5, f = 6.5f, l = 53, ul = 234, i = 123, ui = 167, s = 3567, us = 23, b = 54, sb = -5, st = \"Hello, World\", e = Test5AttributeEnum.Test2, o = null)]", c => c.Assembly).Cast<Test5Attribute>().Single();
			Assert.That(attr.d, Is.EqualTo(4.5));
			Assert.That(attr.f, Is.EqualTo(6.5));
			Assert.That(attr.l, Is.EqualTo(53));
			Assert.That(attr.ul, Is.EqualTo(234));
			Assert.That(attr.i, Is.EqualTo(123));
			Assert.That(attr.ui, Is.EqualTo(167));
			Assert.That(attr.s, Is.EqualTo(3567));
			Assert.That(attr.us, Is.EqualTo(23));
			Assert.That(attr.b, Is.EqualTo(54));
			Assert.That(attr.sb, Is.EqualTo(-5));
			Assert.That(attr.st, Is.EqualTo("Hello, World"));
			Assert.That(attr.e, Is.EqualTo(Test5AttributeEnum.Test2));
			Assert.That(attr.o, Is.Null);
		}

		[Test]
		public void CanUseTypeOfForCommonType() {
			var attr = Process("[assembly: Test5(t = typeof(DateTime))]", c => c.Assembly).Cast<Test5Attribute>().Single();
			Assert.That(attr.t, Is.EqualTo(typeof(DateTime)));

			attr = Process("[assembly: Test5(t = typeof(System.Collections.Generic.List<int>))]", c => c.Assembly).Cast<Test5Attribute>().Single();
			Assert.That(attr.t, Is.EqualTo(typeof(List<int>)));

			attr = Process("[assembly: Test5(t = typeof(System.Collections.Generic.Dictionary<System.Collections.Generic.List<int>, System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>>>))]", c => c.Assembly).Cast<Test5Attribute>().Single();
			Assert.That(attr.t, Is.EqualTo(typeof(System.Collections.Generic.Dictionary<System.Collections.Generic.List<int>, System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>>>)));

			attr = Process("[assembly: Test5(t = typeof(System.Collections.Generic.List<>))]", c => c.Assembly).Cast<Test5Attribute>().Single();
			Assert.That(attr.t, Is.EqualTo(typeof(List<>)));
		}

		[Test]
		public void TypeOfNonCommonTypeGivesObject() {
			var attr = Process("[assembly: Test5(t = typeof(C))] class C {}", c => c.Assembly).Cast<Test5Attribute>().Single();
			Assert.That(attr.t, Is.EqualTo(typeof(object)));
		}

		[Test]
		public void CanUseArrayConstants() {
			var attr = Process("[assembly: Test6(ai = new[] { 3, 6, 7 })] class C {}", c => c.Assembly).Cast<Test6Attribute>().Single();
			Assert.That(attr.ai, Is.EqualTo(new[] { 3, 6, 7 }));
			Assert.That(attr.ai.Select(v => v.GetType()), Has.All.EqualTo(typeof(int)));

			attr = Process("[assembly: Test6(ao = new object[] { 3, (short)6, (byte)7, \"Hello\", null })] class C {}", c => c.Assembly).Cast<Test6Attribute>().Single();
			Assert.That(attr.ao, Is.EqualTo(new object[] { 3, (short)6, (byte)7, "Hello", null }));
			Assert.That(attr.ao.Select(v => (v != null ? v.GetType() : null)).ToList(), Is.EqualTo(new[] { typeof(int), typeof(short), typeof(byte), typeof(string), null }));

			attr = Process("[assembly: Test6(o = new object[] { 3, (short)6, 7 })] class C {}", c => c.Assembly).Cast<Test6Attribute>().Single();
			Assert.That(attr.o, Is.EqualTo(new object[] { 3, (short)6, 7 }));
			Assert.That(((object[])attr.o).Select(v => v.GetType()).ToList(), Is.EqualTo(new[] { typeof(int), typeof(short), typeof(int) }));
		}

		[Test]
		public void CanUseArrayConstantsNested() {
			var attr = Process("[assembly: Test6(ao = new object[] { new[] { 3, 6, 7 }, new[] { 2, 7 }, new[] { 52, 1 } })]", c => c.Assembly).Cast<Test6Attribute>().Single();
			Assert.That(attr.ao, Is.EqualTo(new[] { new[] { 3, 6, 7 }, new[] { 2, 7 }, new[] { 52, 1 } }));
			Assert.That(attr.ao.SelectMany(v => (int[])v).Select(v => v.GetType()), Has.All.EqualTo(typeof(int)));
		}

#if false // Uses the ScriptSerializableAttribute which cannot be in the app domain when running the compiler driver tests
		[Test]
		public void SerializableAttributeIsConvertedToScriptSerializableAttribute() {
			var attributes = Process("[Serializable(TypeCheckCode = \"X\")] class C {}", c => c.GetTypeByMetadataName("C"));
			Assert.That(attributes.Count, Is.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<System.Runtime.CompilerServices.Internal.ScriptSerializableAttribute>());
			Assert.That(((System.Runtime.CompilerServices.Internal.ScriptSerializableAttribute)attributes[0]).TypeCheckCode, Is.EqualTo("X"));

			attributes = Process("[Serializable] class C {}", c => c.GetTypeByMetadataName("C"));
			Assert.That(attributes.Count, Is.EqualTo(1));
			Assert.That(attributes[0], Is.InstanceOf<System.Runtime.CompilerServices.Internal.ScriptSerializableAttribute>());
			Assert.That(((System.Runtime.CompilerServices.Internal.ScriptSerializableAttribute)attributes[0]).TypeCheckCode, Is.Null);
		}
#endif

		[Test]
		public void AttributeCodeIsRun() {
			Test7Attribute.AllApplications.Clear();
			Process(@"
[assembly: Test7(""asm"")]
[Test7(""C_attr"")]
class C {
	[Test7(""ctor_attr"")]
	public C() {}

	[Test7(""M_attr"")]
	public void M() {}

	[Test7(""F_attr"")]
	public int F;

	[Test7(""P_attr"")]
	public int P { [Test7(""get_P_attr"")] get; [Test7(""set_P_attr"")] set; }

	[Test7(""E_attr"")]
	public event Action E { [Test7(""add_E_attr"")] add {} [Test7(""remove_E_attr"")] remove {} }
}
			", c => c.Assembly);

			var actual = string.Join("\n", Test7Attribute.AllApplications.Select(a => a.Item1.Name + ": " + a.Item2));
			Assert.AreEqual(
@".ctor: ctor_attr
M: M_attr
F: F_attr
P: P_attr
get_P: get_P_attr
set_P: set_P_attr
E: E_attr
add_E: add_E_attr
remove_E: remove_E_attr
C: C_attr
Test: asm"
				.Replace("\r\n", "\n"),
				actual
			);
		}

		private class TestAutomaticMetadataApplier : IAutomaticMetadataAttributeApplier {
			private readonly string _data;

			public TestAutomaticMetadataApplier(string data) {
				_data = data;
			}

			public void Process(IAssemblySymbol assembly, IAttributeStore attributeStore) {
				Test7Attribute.AllApplications.Add(Tuple.Create((ISymbol)assembly, _data));
			}

			public void Process(INamedTypeSymbol type, IAttributeStore attributeStore) {
				Test7Attribute.AllApplications.Add(Tuple.Create((ISymbol)type, _data));
			}
		}

		[Test]
		public void AutomaticMetadataAppliersAreRunBeforeAttributeCode() {
			Test7Attribute.AllApplications.Clear();
			Process(@"
[assembly: Test7(""asm"")]
[Test7(""C_attr"")]
class C {
	[Test7(""ctor_attr"")]
	public C() {}
}", c => c.Assembly, automaticMetadataAppliers: new[] { new TestAutomaticMetadataApplier("applier") });

			var actual = string.Join("\n", Test7Attribute.AllApplications.Select(a => a.Item1.Name + ": " + a.Item2));
			Assert.AreEqual(
@"C: applier
Test: applier
.ctor: ctor_attr
C: C_attr
Test: asm"
				.Replace("\r\n", "\n"),
				actual
			);
		}

		[Test]
		public void AttributeCodeIsOnlyRunForTheMainAssembly() {
			Test7Attribute.AllApplications.Clear();
			var reference = Common.CreateCompilation("using System; using Saltarelle.Compiler.Tests.AttributeStoreTests; [assembly: Test7(\"Should not be run\")] [Test7(\"Should not be run\")] class B { [Test7(\"Should not be run\")] public B() { } }", new[] { Common.Mscorlib, _attributeReference.Value }, assemblyName: "Ref");
			var compilation = Common.CreateCompilation("using System; using Saltarelle.Compiler.Tests.AttributeStoreTests; [assembly: Test7(\"asm\")] [Test7(\"C\")] class C { [Test7(\"ctor\")] public C() { } }", new[] { Common.Mscorlib, _attributeReference.Value, reference.ToMetadataReference() });
			var er = new MockErrorReporter();

#pragma warning disable 168
			var s = new AttributeStore(compilation, er, new IAutomaticMetadataAttributeApplier[0]);
#pragma warning restore 168
			Assert.That(er.AllMessages, Is.Empty, "Should not have errors");

			var actual = string.Join("\n", Test7Attribute.AllApplications.Select(a => a.Item1.Name + ": " + a.Item2));
			Assert.AreEqual(
@".ctor: ctor
C: C
Test: asm"
				.Replace("\r\n", "\n"),
				actual
			);
		}

		[Test]
		public void AutomaticMetadataAppliersAreOnlyRunForTheMainAssembly() {
			Test7Attribute.AllApplications.Clear();
			var reference = Common.CreateCompilation("using System; using Saltarelle.Compiler.Tests.AttributeStoreTests; [assembly: Test7(\"Should not be run\")] [Test7(\"Should not be run\")] class B { [Test7(\"Should not be run\")] public B() { } }", new[] { Common.Mscorlib, _attributeReference.Value }, assemblyName: "Ref");
			var compilation = Common.CreateCompilation("using System; using Saltarelle.Compiler.Tests.AttributeStoreTests; [assembly: Test7(\"asm\")] [Test7(\"C\")] class C { [Test7(\"ctor\")] public C() { } }", new[] { Common.Mscorlib, _attributeReference.Value, reference.ToMetadataReference() });
			var er = new MockErrorReporter();

#pragma warning disable 168
			var s = new AttributeStore(compilation, er, new[] { new TestAutomaticMetadataApplier("applier") });
#pragma warning restore 168
			Assert.That(er.AllMessages, Is.Empty, "Should not have errors");

			var actual = string.Join("\n", Test7Attribute.AllApplications.Select(a => a.Item1.Name + ": " + a.Item2));
			Assert.AreEqual(
@"C: applier
Test: applier
.ctor: ctor
C: C
Test: asm"
				.Replace("\r\n", "\n"),
				actual
			);
		}
	}
}
