using System;
using System.IO;
using System.Linq;
using CoreLib.Plugin;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests.OOPEmulatorTests {
	[TestFixture]
	public class OverallStructureTests : OOPEmulatorTestBase {
		private class Resource : IAssemblyResource {
			public string Name { get; private set; }
			public AssemblyResourceType Type { get; private set; }
			public string LinkedFileName { get; private set; }
			public bool IsPublic { get; private set; }
			private readonly byte[] _data;

			public Resource(AssemblyResourceType type, string name, bool isPublic, string linkedFileName = null, byte[] data = null) {
				_data = data;
				Type = type;
				Name = name;
				IsPublic = isPublic;
				LinkedFileName = linkedFileName;
			}

			public Stream GetResourceStream() {
				return new MemoryStream(_data);
			}
		}

		[Test]
		public void TheOverallStructureIsCorrect() {
			AssertCorrect(
@"namespace OuterNamespace {
	namespace InnerNamespace {
		public class SomeType {
			public SomeType(int a) { a = 0; }
			public void Method1(int b) { b = 0; }
			public void Method2(int c) { c = 0; }
			public static void StaticMethod(int d) { d = 0; }
		}
		public class SomeType2 {
			public SomeType2(int a1) { a1 = 0; }
			public void Method1(int b1) { b1 = 0; }
			public static void OtherStaticMethod(int c1) { c1 = 0; }
			static SomeType2() {
				int d1 = 0;
			}
		}
		public enum SomeEnum {
			Value1 = 1,
			Value2 = 2,
			Value3 = 3,
		}
	}
	namespace InnerNamespace2 {
		public class OtherType : InnerNamespace.SomeType2 {
			public OtherType(int a2) : base(a2) { a2 = 0; }
			public void Method2(int b2) { b2 = 0; }
			static OtherType() {
				int c2 = 0;
			}
		}
		public interface OtherInterface {
			void InterfaceMethod(int a3);
		}
	}
}
",
@"global.OuterNamespace = global.OuterNamespace || {};
global.OuterNamespace.InnerNamespace = global.OuterNamespace.InnerNamespace || {};
global.OuterNamespace.InnerNamespace2 = global.OuterNamespace.InnerNamespace2 || {};
{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace.SomeEnum
var $OuterNamespace_InnerNamespace_SomeEnum = function() {
};
$OuterNamespace_InnerNamespace_SomeEnum.__typeName = 'OuterNamespace.InnerNamespace.SomeEnum';
global.OuterNamespace.InnerNamespace.SomeEnum = $OuterNamespace_InnerNamespace_SomeEnum;
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace.SomeType
var $OuterNamespace_InnerNamespace_SomeType = function(a) {
	a = 0;
};
$OuterNamespace_InnerNamespace_SomeType.__typeName = 'OuterNamespace.InnerNamespace.SomeType';
$OuterNamespace_InnerNamespace_SomeType.staticMethod = function(d) {
	d = 0;
};
global.OuterNamespace.InnerNamespace.SomeType = $OuterNamespace_InnerNamespace_SomeType;
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace.SomeType2
var $OuterNamespace_InnerNamespace_SomeType2 = function(a1) {
	a1 = 0;
};
$OuterNamespace_InnerNamespace_SomeType2.__typeName = 'OuterNamespace.InnerNamespace.SomeType2';
$OuterNamespace_InnerNamespace_SomeType2.otherStaticMethod = function(c1) {
	c1 = 0;
};
global.OuterNamespace.InnerNamespace.SomeType2 = $OuterNamespace_InnerNamespace_SomeType2;
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace2.OtherInterface
var $OuterNamespace_InnerNamespace2_OtherInterface = function() {
};
$OuterNamespace_InnerNamespace2_OtherInterface.__typeName = 'OuterNamespace.InnerNamespace2.OtherInterface';
global.OuterNamespace.InnerNamespace2.OtherInterface = $OuterNamespace_InnerNamespace2_OtherInterface;
////////////////////////////////////////////////////////////////////////////////
// OuterNamespace.InnerNamespace2.OtherType
var $OuterNamespace_InnerNamespace2_OtherType = function(a2) {
	{SomeType2}.call(this, a2);
	a2 = 0;
};
$OuterNamespace_InnerNamespace2_OtherType.__typeName = 'OuterNamespace.InnerNamespace2.OtherType';
global.OuterNamespace.InnerNamespace2.OtherType = $OuterNamespace_InnerNamespace2_OtherType;
{Script}.initEnum($OuterNamespace_InnerNamespace_SomeEnum, $asm, { value1: 1, value2: 2, value3: 3 });
{Script}.initClass($OuterNamespace_InnerNamespace_SomeType, $asm, {
	method1: function(b) {
		b = 0;
	},
	method2: function(c) {
		c = 0;
	}
});
{Script}.initClass($OuterNamespace_InnerNamespace_SomeType2, $asm, {
	method1: function(b1) {
		b1 = 0;
	}
});
{Script}.initInterface($OuterNamespace_InnerNamespace2_OtherInterface, $asm, { interfaceMethod: null });
{Script}.initClass($OuterNamespace_InnerNamespace2_OtherType, $asm, {
	method2: function(b2) {
		b2 = 0;
	}
}, {SomeType2});
var d1 = 0;
var c2 = 0;
");
		}

		[Test]
		public void TypesAppearInTheCorrectOrder() {
			var names = new[] {
				"SomeType",
				"SomeType2",
				"SomeNamespace.SomeType",
				"SomeNamespace.InnerNamespace.OtherType1",
				"SomeNamespace.InnerNamespace.OtherType2",
				"SomeNamespace.SomeType.StrangeType1",
				"SomeNamespace.SomeType.StrangeType2",
				"SomeType2.SomeType3",
				"SomeTYpe2.SomeType3.SomeType4",
			};

			var rnd = new Random(3);
			var unorderedNames = names.Select(n => new { n, r = rnd.Next() }).OrderBy(x => x.r).Select(x => x.n).ToArray();
			
			var output = Process(string.Join(" ", unorderedNames.Select(n => {
			                                                                var parts = n.Split('.');
			                                                                string result = "public class " + parts[parts.Length - 1] + "{}";
			                                                                for (int i = parts.Length - 2; i >= 0; i--)
			                                                                    result = "namespace " + parts[i] + "{" + result + "}";
			                                                                return result;
			                                                            })));

			var actual = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Where(l => l.StartsWith("// ")).Select(l => l.Substring(3)).ToList();

			Assert.That(actual, Is.EqualTo(names));
		}

		[Test]
		public void BaseTypesAreRegisteredBeforeDerivedTypes() {
			AssertCorrect(
@"public class C3 {}
public interface I1 {}
public class C2 : C3 {}
public class C1 : C2, I1 {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// C1
var $C1 = function() {
	{C2}.call(this);
};
$C1.__typeName = 'C1';
global.C1 = $C1;
////////////////////////////////////////////////////////////////////////////////
// C2
var $C2 = function() {
	{C3}.call(this);
};
$C2.__typeName = 'C2';
global.C2 = $C2;
////////////////////////////////////////////////////////////////////////////////
// C3
var $C3 = function() {
};
$C3.__typeName = 'C3';
global.C3 = $C3;
////////////////////////////////////////////////////////////////////////////////
// I1
var $I1 = function() {
};
$I1.__typeName = 'I1';
global.I1 = $I1;
{Script}.initClass($C3, $asm, {});
{Script}.initClass($C2, $asm, {}, {C3});
{Script}.initInterface($I1, $asm, {});
{Script}.initClass($C1, $asm, {}, {C2}, [{I1}]);
");
		}

		[Test]
		public void BaseTypesAreRegisteredBeforeDerivedTypesGeneric() {
			AssertCorrect(
@"using System.Runtime.CompilerServices;
[IncludeGenericArguments(false)] public class B<T> {}
[IncludeGenericArguments(false)] public interface I<T> {}
public class A : B<int>, I<int> {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// A
var $A = function() {
	{B}.call(this);
};
$A.__typeName = 'A';
global.A = $A;
////////////////////////////////////////////////////////////////////////////////
// B
var $B = function() {
};
$B.__typeName = 'B';
global.B = $B;
////////////////////////////////////////////////////////////////////////////////
// I
var $I = function() {
};
$I.__typeName = 'I';
global.I = $I;
{Script}.initClass($B, $asm, {});
{Script}.initInterface($I, $asm, {});
{Script}.initClass($A, $asm, {}, {B}, [{I}]);
");
		}

		[Test]
		public void BaseTypesAreRegisteredBeforeDerivedTypesGeneric2() {
			var output = Process(@"
public abstract class Base {}
public abstract class EBase : Base { }
public abstract class GenericBase<T> : EBase { }
public sealed class C : GenericBase<object> {}");
			
			var actual = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Where(l => l.StartsWith("{Script}.initClass")).ToList();

			Assert.That(actual, Is.EqualTo(new[] { "{Script}.initClass($Base, $asm, {});", "{Script}.initClass($EBase, $asm, {}, {Base});", "{Script}.initClass($C, $asm, {}, {Script}.makeGenericType({GenericBase}, [{Object}]));" }));
		}

		[Test]
		public void ByNamespaceComparerOrdersTypesCorrectly() {
			var orig = new[] { "A", "B", "C", "A.B", "A.BA", "A.C", "A.BAA.A", "B.A", "B.B", "B.C", "B.A.A", "B.A.B", "B.B.A" };
			var rnd = new Random(42);
			var shuffled = orig.Select(n => new { n, r = rnd.Next() }).OrderBy(x => x.r).Select(x => x.n).ToList();
			var actual = OOPEmulator.OrderByNamespace(shuffled, s => s).ToList();
			Assert.That(actual, Is.EqualTo(orig));
		}

		[Test]
		public void ProgramWithEntryPointWorks() {
			AssertCorrect(
@"public class MyClass {
	[System.Runtime.CompilerServices.ScriptName(""theEntryPoint"")]
	public static void Main() { int x = 0; }
	static MyClass() { int a = 0; }
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.__typeName = 'MyClass';
$MyClass.theEntryPoint = function() {
	var x = 0;
};
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {});
var a = 0;
{MyClass}.theEntryPoint();
", entryPoint: "MyClass.Main");
		}

		[Test]
		public void AssemblyAttributesAreAssignedBeforeStaticInitializerStatements() {
			AssertCorrect(
@"[assembly: MyAttribute(42)]
public class MyAttribute : System.Attribute {
	public MyAttribute(int x) {}
	static MyAttribute() { int a = 0; }
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyAttribute
var $MyAttribute = function(x) {
};
$MyAttribute.__typeName = 'MyAttribute';
global.MyAttribute = $MyAttribute;
{Script}.initClass($MyAttribute, $asm, {});
$asm.attr = [new {MyAttribute}(42)];
var a = 0;
");
		}

		[Test]
		public void AnErrorIsIssuedIfTheMainMethodHasParameters() {
			var er = new MockErrorReporter();
			Process(
@"public class MyClass {
	public static void Main(string[] args) {}
}", entryPoint: "MyClass.Main", errorReporter: er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7800 && (string)m.Args[0] == "MyClass.Main"));
		}

		[Test]
		public void AnErrorIsIssuedIfTheMainMethodIsNotImplementedAsANormalMethod() {
			var er = new MockErrorReporter();
			Process(
@"public class MyClass {
	[System.Runtime.CompilerServices.InlineCode(""X"")]
	public static void Main() {}
}", entryPoint: "MyClass.Main", errorReporter: er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7801 && (string)m.Args[0] == "MyClass.Main"));
		}

		[Test]
		public void BothPublicAndPrivateEmbeddedResourcesAreIncludedInTheInitAssemblyCallButThisExcludesPluginDllsAndLinkedResources() {
			AssertCorrect("",
			              "{Script}.initAssembly($asm, 'x', { 'Resource.Name': 'LQYHBA==', 'Some.Private.Resource': 'BQMH' });\n",
			              resources: new[] { new Resource(AssemblyResourceType.Embedded, "Resource.Name", true, data: new byte[] { 45, 6, 7, 4 }),
			                                 new Resource(AssemblyResourceType.Linked, "Other.Resource", true, linkedFileName: "some-file.txt"),
			                                 new Resource(AssemblyResourceType.Embedded, "Some.Private.Resource", false, data: new byte[] { 5, 3, 7 }),
			                                 new Resource(AssemblyResourceType.Embedded, "Namespace.Plugin.dll", true, data: new byte[] { 5, 3, 7 }),
			                                 new Resource(AssemblyResourceType.Embedded, "Plugin.dll", true, data: new byte[] { 5, 3, 7 }) });
		}
	}
}
