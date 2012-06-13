using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class MemberImportTests : ScriptSharpMetadataImporterTestBase {
		private IEnumerable<IMember> FindMembers(IDictionary<string, ITypeDefinition> types, string name) {
            var lastDot = name.LastIndexOf('.');
			return types[name.Substring(0, lastDot)].Members.Where(m => m.Name == name.Substring(lastDot + 1));
		}

		private List<Tuple<IMethod, MethodScriptSemantics>> FindMethods(IDictionary<string, ITypeDefinition> types, string name, INamingConventionResolver md) {
			return FindMembers(types, name).Cast<IMethod>().Select(m => Tuple.Create(m, md.GetMethodImplementation(m))).ToList();
		}

		private MethodScriptSemantics FindMethod(IDictionary<string, ITypeDefinition> types, string name, INamingConventionResolver md) {
			return FindMethods(types, name, md).Single().Item2;
		}
		
		[Test]
		public void NonOverloadedMethodIsCamelCased() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"class C {
	public void SomeMethod() {
	}
}");

			var impl = FindMethod(types, "C.SomeMethod", md);
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod"));
			Assert.That(impl.IgnoreGenericArguments, Is.False);
			Assert.That(impl.GenerateCode, Is.True);
		}

		[Test]
		public void OverloadedMethodsGetDifferentNames() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"class C {
	public void SomeMethod() {
	}
	public void SomeMethod(int x) {
	}
	public void SomeMethod(string s) {
	}
	public void SomeMethod(int a, int b) {
	}
}");

			var methods = FindMethods(types, "C.SomeMethod", md);
			var m1 = methods.Single(x => x.Item1.Parameters.Count == 0).Item2;
			Assert.That(m1.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m1.Name, Is.EqualTo("someMethod"));
			Assert.That(m1.IgnoreGenericArguments, Is.False);
			Assert.That(m1.GenerateCode, Is.True);

			var m2 = methods.Single(x => x.Item1.Parameters.Count == 1 && x.Item1.Parameters[0].Type.GetDefinition().KnownTypeCode == KnownTypeCode.Int32).Item2;
			Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m2.Name, Is.EqualTo("someMethod$1"));
			Assert.That(m2.IgnoreGenericArguments, Is.False);
			Assert.That(m2.GenerateCode, Is.True);

			var m3 = methods.Single(x => x.Item1.Parameters.Count == 1 && x.Item1.Parameters[0].Type.GetDefinition().KnownTypeCode == KnownTypeCode.String).Item2;
			Assert.That(m3.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m3.Name, Is.EqualTo("someMethod$2"));
			Assert.That(m3.IgnoreGenericArguments, Is.False);
			Assert.That(m3.GenerateCode, Is.True);

			var m4 = methods.Single(x => x.Item1.Parameters.Count == 2).Item2;
			Assert.That(m4.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m4.Name, Is.EqualTo("someMethod$3"));
			Assert.That(m4.IgnoreGenericArguments, Is.False);
			Assert.That(m4.GenerateCode, Is.True);
		}

		[Test]
		public void MethodShadowingBaseMethodGetsANewName() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"class A {
	public void SomeMethod() {
	}
}
class B : A {
	public void SomeMethod(int x) {
	}
}

class C : B {
	public new void SomeMethod() {
	}
}");

			var impl = FindMethod(types, "A.SomeMethod", md);
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod"));
			Assert.That(impl.IgnoreGenericArguments, Is.False);
			Assert.That(impl.GenerateCode, Is.True);

			impl = FindMethod(types, "B.SomeMethod", md);
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod$1"));
			Assert.That(impl.IgnoreGenericArguments, Is.False);
			Assert.That(impl.GenerateCode, Is.True);

			impl = FindMethod(types, "C.SomeMethod", md);
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod$2"));
			Assert.That(impl.IgnoreGenericArguments, Is.False);
			Assert.That(impl.GenerateCode, Is.True);
		}

		[Test]
		public void ScriptNameAttributeWorksOnMethods() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;
class C {
	[ScriptName(""Renamed"")]
	public void SomeMethod() {
	}
}");

			var impl = FindMethod(types, "C.SomeMethod", md);
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("Renamed"));
		}

		[Test]
		public void SameScriptNameCanBeSpecifiedOnManyOverloads() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C {
	[ScriptName(""Renamed1"")]
	public void SomeMethod() {
	}
	[ScriptName(""Renamed1"")]
	public void SomeMethod(int x) {
	}
	[ScriptName(""Renamed2"")]
	public void SomeMethod(string s) {
	}
	public void SomeMethod(int a, int b) {
	}
}");

			var methods = FindMethods(types, "C.SomeMethod", md);
			var m1 = methods.Single(x => x.Item1.Parameters.Count == 0).Item2;
			Assert.That(m1.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m1.Name, Is.EqualTo("Renamed1"));

			var m2 = methods.Single(x => x.Item1.Parameters.Count == 1 && x.Item1.Parameters[0].Type.GetDefinition().KnownTypeCode == KnownTypeCode.Int32).Item2;
			Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m2.Name, Is.EqualTo("Renamed1"));

			var m3 = methods.Single(x => x.Item1.Parameters.Count == 1 && x.Item1.Parameters[0].Type.GetDefinition().KnownTypeCode == KnownTypeCode.String).Item2;
			Assert.That(m3.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m3.Name, Is.EqualTo("Renamed2"));

			var m4 = methods.Single(x => x.Item1.Parameters.Count == 2).Item2;
			Assert.That(m4.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m4.Name, Is.EqualTo("someMethod"));
		}

		[Test]
		public void PreserveNameWorksOnMethods() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C {
	public void SomeMethod() {
	}
	[PreserveName]
	public void SomeMethod(int x) {
	}
	[PreserveName]
	public void SomeMethod(int a, int b) {
	}
}");

			var methods = FindMethods(types, "C.SomeMethod", md);
			var m1 = methods.Single(x => x.Item1.Parameters.Count == 0).Item2;
			Assert.That(m1.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m1.Name, Is.EqualTo("someMethod$1"));

			var m2 = methods.Single(x => x.Item1.Parameters.Count == 1).Item2;
			Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m2.Name, Is.EqualTo("someMethod"));

			var m3 = methods.Single(x => x.Item1.Parameters.Count == 2).Item2;
			Assert.That(m3.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m3.Name, Is.EqualTo("someMethod"));
		}

		[Test]
		public void PreserveCaseWorksOnMethods() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class C {
	public void SomeMethod() {
	}
	[PreserveCase]
	public void SomeMethod(int x) {
	}
	[PreserveCase]
	public void SomeMethod(int a, int b) {
	}
}");

			var methods = FindMethods(types, "C.SomeMethod", md);
			var m1 = methods.Single(x => x.Item1.Parameters.Count == 0).Item2;
			Assert.That(m1.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m1.Name, Is.EqualTo("someMethod"));

			var m2 = methods.Single(x => x.Item1.Parameters.Count == 1).Item2;
			Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m2.Name, Is.EqualTo("SomeMethod"));

			var m3 = methods.Single(x => x.Item1.Parameters.Count == 2).Item2;
			Assert.That(m3.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m3.Name, Is.EqualTo("SomeMethod"));
		}

		[Test]
		public void OverridingMembersGetTheirNameFromTheDefiningMember() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

class A {
	[ScriptName(""RenamedMethod"")]
	public virtual void SomeMethod() {}
}

class B : A {
	public override void SomeMethod() {}
}
class C : B {
	public override void SomeMethod() {}
}");

			var mb = FindMethod(types, "B.SomeMethod", md);
			Assert.That(mb.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(mb.Name, Is.EqualTo("RenamedMethod"));

			var mc = FindMethod(types, "C.SomeMethod", md);
			Assert.That(mc.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(mc.Name, Is.EqualTo("RenamedMethod"));
		}


		[Test]
		public void ImplicitInterfaceImplementationMethodsGetTheirNameFromTheInterface() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	[ScriptName(""RenamedMethod"")]
	void SomeMethod();
}

interface I2<T> {
	[ScriptName(""RenamedMethod2"")]
	void SomeMethod2();
}

class C : I, I2<int> {
	public void SomeMethod() {}

	public void SomeMethod2() {}
}");

			var m = FindMethod(types, "C.SomeMethod", md);
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("RenamedMethod"));

			var m2 = FindMethod(types, "C.SomeMethod2", md);
			Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m2.Name, Is.EqualTo("RenamedMethod2"));
		}

		[Test]
		public void ExplicitInterfaceImplementationMethodsGetTheirNameFromTheInterface() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	[ScriptName(""RenamedMethod"")]
	void SomeMethod();
}

interface I2<T> {
	[ScriptName(""RenamedMethod2"")]
	void SomeMethod2();
}

class C : I, I2<int> {
	void I.SomeMethod() {
	}

	void I2<int>.SomeMethod2() {
	}
}");

			var m = FindMethod(types, "C.SomeMethod", md);
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("RenamedMethod"));

			var m2 = FindMethod(types, "C.SomeMethod2", md);
			Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m2.Name, Is.EqualTo("RenamedMethod2"));
		}

		[Test]
		public void MethodCanImplementTwoInterfaceMethodsWithTheSameName() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	[ScriptName(""RenamedMethod"")]
	void SomeMethod(int i);
}

interface I2<T> {
	[ScriptName(""RenamedMethod"")]
	void SomeMethod2(T i);
}

class C : I, I2<int> {
	public void SomeMethod(int i) {}
}");

			var m = FindMethod(types, "C.SomeMethod", md);
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("RenamedMethod"));
		}

		[Test]
		public void OverridingMethodCanImplementInterfaceMethodWithTheSameName() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	[ScriptName(""RenamedMethod"")]
	void SomeMethod(int i);
}

class B {
	[ScriptName(""RenamedMethod"")]
	public virtual void SomeMethod(int i) {}
}

class D : B, I {
	public override void SomeMethod(int i) {}
}");

			var m = FindMethod(types, "D.SomeMethod", md);
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("RenamedMethod"));
		}

		[Test]
		public void MethodCannotImplementTwoInterfaceMethodsIfTheNamesAreDifferent() {
			var er = new MockErrorReporter(false);
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	[ScriptName(""RenamedMethod"")]
	void SomeMethod(int i);
}

interface I2<T> {
	[ScriptName(""RenamedMethod2"")]
	void SomeMethod(T i);
}

class C : I, I2<int> {
	public void SomeMethod(int i) {
	}
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("implement") && er.AllMessages[0].Contains("differing script names") && er.AllMessages[0].Contains("C.SomeMethod"));
		}

		[Test]
		public void OverridingMethodCannotImplementInterfaceMethodIfTheNamesDiffer() {
			var er = new MockErrorReporter(false);
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	[ScriptName(""RenamedMethod"")]
	void SomeMethod(int i);
}

class B {
	[ScriptName(""RenamedMethod2"")]
	public virtual void SomeMethod(int i) {}
}

class D : B, I {
	public override void SomeMethod(int i) {
	}
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("implement") && er.AllMessages[0].Contains("different script name") && er.AllMessages[0].Contains("D.SomeMethod") && er.AllMessages[0].Contains("I.SomeMethod"));
		}

		[Test]
		public void BaseMethodCanImplementInterfaceMemberIfTheNamesAreTheSame() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	[ScriptName(""RenamedMethod"")]
	void SomeMethod(int i);
}

class B {
	[ScriptName(""RenamedMethod"")]
	public void SomeMethod(int i) {}
}

class D : B, I {
}");

			// The only thing we need to assert in this test is that there was no error message.
		}

		[Test, Ignore("No NRefactory support")]
		public void BaseMethodCannotImplementInterfaceMemberIfTheNamesDiffer() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var er = new MockErrorReporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	[ScriptName(""RenamedMethod"")]
	void SomeMethod(int i);
}

class B {
	[ScriptName(""RenamedMethod2"")]
	public void SomeMethod(int i) {}
}

class D : B, I {
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.Fail("TODO: Assert message");
		}

		[Test]
		public void CannotSpecifyScriptNameAttributeOnMethodImplementingInterfaceMember() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	void SomeMethod(int i);
}

class C : I {
	[ScriptName(""RenamedMethod2"")]
	public void SomeMethod(int i) {}
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("ScriptName") && er.AllMessages[0].Contains("C.SomeMethod") && er.AllMessages[0].Contains("interface member"));
		}

		[Test]
		public void CannotSpecifyPreserveNameAttributeOnMethodImplementingInterfaceMember() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	void SomeMethod(int i);
}

class C : I {
	[PreserveName]
	public void SomeMethod(int i) {}
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("PreserveName") && er.AllMessages[0].Contains("C.SomeMethod") && er.AllMessages[0].Contains("interface member"));
		}

		[Test]
		public void CannotSpecifyPreserveCaseAttributeOnMethodImplementingInterfaceMember() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	void SomeMethod(int i);
}

class C : I {
	[PreserveCase]
	public void SomeMethod(int i) {}
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("PreserveCase") && er.AllMessages[0].Contains("C.SomeMethod") && er.AllMessages[0].Contains("interface member"));
		}

		[Test]
		public void CannotSpecifyScriptNameAttributeOnOverridingMethod() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;

class B {
	public virtual void SomeMethod(int i) {}
}

class D : B {
	[ScriptName(""RenamedMethod"")]
	public override void SomeMethod(int i) {}
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("ScriptName") && er.AllMessages[0].Contains("D.SomeMethod") && er.AllMessages[0].Contains("overrides"));
		}

		[Test]
		public void CannotSpecifyPreserveNameAttributeOnOverridingMethod() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;

class B {
	public virtual void SomeMethod(int i) {}
}

class D : B {
	[PreserveName]
	public override void SomeMethod(int i) {}
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("ScriptName") && er.AllMessages[0].Contains("D.SomeMethod") && er.AllMessages[0].Contains("overrides"));
		}

		[Test]
		public void CannotSpecifyPreserveCaseAttributeOnOverridingMethod() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;

class B {
	public virtual void SomeMethod(int i) {}
}

class D : B {
	[PreserveCase]
	public override void SomeMethod(int i) {}
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("PreserveCase") && er.AllMessages[0].Contains("D.SomeMethod") && er.AllMessages[0].Contains("overrides"));
		}

		[Test]
		public void ScriptNameCanBeSpecifiedOnInterfaceMethod() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	[ScriptName(""RenamedMethod"")]
	void SomeMethod();
}");

			var m = FindMethod(types, "I.SomeMethod", md);
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("RenamedMethod"));
		}

		[Test]
		public void PreserveNameCanBeSpecifiedOnInterfaceMethod() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	[PreserveName]
	void SomeMethod();
}");

			var m = FindMethod(types, "I.SomeMethod", md);
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("someMethod"));
		}

		[Test]
		public void PreserveCaseCanBeSpecifiedOnInterfaceMethod() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"using System.Runtime.CompilerServices;

interface I {
	[PreserveCase]
	void SomeMethod();
}");

			var m = FindMethod(types, "I.SomeMethod", md);
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("SomeMethod"));
		}

		[Test]
		public void MinimizationOfClassesGeneratesMembersThatAreUniqueInTheHierarchyButDoesNotTouchPublicSymbols() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"class A {
	public void SomeMethod() {}
	public static void SomeMethod2() {}
	public void SomeMethod(int x) {}
	public virtual void VirtualMethod() {}
}

class B : A {
	public void OtherMethodB() {}
	public override void VirtualMethod();
}

class C : B {
	public void OtherMethodC() {}
	public new void SomeMethod() {}
	public override void VirtualMethod() {}
}

public class D {
	public void PublicMethod() {}
	protected void ProtectedMethod() {}
	protected internal void ProtectedInternalMethod() {}
	private void PrivateMethod() {}
}
");

			Assert.That(FindMethods(types, "A.SomeMethod", md).Single(m => m.Item1.Parameters.Count == 0).Item2.Name, Is.EqualTo("$0"));
			Assert.That(FindMethod(types, "A.SomeMethod2", md).Name, Is.EqualTo("$2"));
			Assert.That(FindMethods(types, "A.SomeMethod", md).Single(m => m.Item1.Parameters.Count == 1).Item2.Name, Is.EqualTo("$1"));
			Assert.That(FindMethod(types, "A.VirtualMethod", md).Name, Is.EqualTo("$3"));
			Assert.That(FindMethod(types, "B.OtherMethodB", md).Name, Is.EqualTo("$4"));
			Assert.That(FindMethod(types, "B.VirtualMethod", md).Name, Is.EqualTo("$3"));
			Assert.That(FindMethod(types, "C.OtherMethodC", md).Name, Is.EqualTo("$5"));
			Assert.That(FindMethod(types, "C.SomeMethod", md).Name, Is.EqualTo("$6"));
			Assert.That(FindMethod(types, "C.VirtualMethod", md).Name, Is.EqualTo("$3"));
			Assert.That(FindMethod(types, "D.PublicMethod", md).Name, Is.EqualTo("publicMethod"));
			Assert.That(FindMethod(types, "D.ProtectedMethod", md).Name, Is.EqualTo("protectedMethod"));
			Assert.That(FindMethod(types, "D.ProtectedInternalMethod", md).Name, Is.EqualTo("protectedInternalMethod"));
			Assert.That(FindMethod(types, "D.PrivateMethod", md).Name, Is.EqualTo("$0"));

			Assert.Inconclusive("TODO: Test properties, events, constructors, fields");
		}

		[Test]
		public void MinimizationOfInterfacesGeneratesNamesThatAreUniqueWithinTheAssemblyButDoesNotTouchPublicInterfaces() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var types = Process(md,
@"interface I1 {
	void SomeMethod();
	void SomeMethod(int x);
	void SomeMethod2();
}

interface I2 {
	void SomeMethod();
	void SomeMethod(int x);
	void SomeMethod2();
}

public interface I3 {
	void SomeMethod();
}

public interface I4 {
	void SomeMethod();
}");

			Assert.That(        FindMethods(types, "I1.SomeMethod", md)
			            .Concat(FindMethods(types, "I1.SomeMethod2", md))
			            .Concat(FindMethods(types, "I2.SomeMethod", md))
			            .Concat(FindMethods(types, "I2.SomeMethod2", md))
						.Select(m => m.Item2.Name).ToList(),
						Is.EquivalentTo(new[] { "$I0", "$I1", "$I2", "$I3", "$I4", "$I5" }));

			Assert.That(FindMethod(types, "I3.SomeMethod", md).Name, Is.EqualTo("someMethod"));
			Assert.That(FindMethod(types, "I4.SomeMethod", md).Name, Is.EqualTo("someMethod"));

			Assert.Inconclusive("TODO: Test properties, events, constructors");
		}

		[Test]
		public void ScriptNameOnMethodMustBeValidIdentifierOrBeEmpty() {
			var er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; public class C1 { [ScriptName(""a b"")] public void M() {} }", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1") && er.AllMessages[0].Contains("ScriptName") && er.AllMessages[0].Contains("must be a valid JavaScript identifier"));

			er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; public class C1 { [ScriptName(""a b"")] public void M() {} }", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1") && er.AllMessages[0].Contains("ScriptName") && er.AllMessages[0].Contains("must be a valid JavaScript identifier"));
		}

		[Test]
		public void EmptyScriptNameOnMethodResultsInLiteralCodeImplementation() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[ScriptName("""")]
	public void SomeMethod() {
	}
}
class C2 {
	[ScriptName("""")]
	public void SomeMethod(int x) {
	}
}
class C3 {
	[ScriptName("""")]
	public void SomeMethod(int x, string y) {
	}
}");

			var impl = FindMethod(types, "C1.SomeMethod", md);
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{this}()"));

			impl = FindMethod(types, "C2.SomeMethod", md);
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{this}({x})"));

			impl = FindMethod(types, "C3.SomeMethod", md);
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{this}({x}, {y})"));
		}

		[Test]
		public void EmptyScriptNameCannotBeSpecifiedOnInterfaceMethod() {
			var er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; public interface I1 { [ScriptName("""")] void M(); }", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("I1.M") && er.AllMessages[0].Contains("ScriptName") && er.AllMessages[0].Contains("interface method") && er.AllMessages[0].Contains("empty name"));
		}

		[Test]
		public void EmptyScriptNameCannotBeSpecifiedOnVirtualOrAbstractMethod() {
			var er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; public class C1 { [ScriptName("""")] public virtual void M() {} }", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1.M") && er.AllMessages[0].Contains("ScriptName") && er.AllMessages[0].Contains("overridable") && er.AllMessages[0].Contains("empty name"));

			er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; public class C1 { [ScriptName("""")] public abstract void M() {} }", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1.M") && er.AllMessages[0].Contains("ScriptName") && er.AllMessages[0].Contains("overridable") && er.AllMessages[0].Contains("empty name"));
		}

		[Test]
		public void ScriptSkipAttributeCannotBeSpecifiedOnInterfaceMethod() {
			var er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; public interface I1 { [ScriptSkip] void M(); }", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("I1.M") && er.AllMessages[0].Contains("ScriptSkipAttribute") && er.AllMessages[0].Contains("interface method"));
		}

		[Test]
		public void ScriptSkipAttributeCannotBeSpecifiedOnVirtualOrAbstractMethod() {
			var er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] public virtual void M() {} }", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1.M") && er.AllMessages[0].Contains("ScriptSkipAttribute") && er.AllMessages[0].Contains("overridable"));

			er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] public abstract void M() {} }", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1.M") && er.AllMessages[0].Contains("ScriptSkipAttribute") && er.AllMessages[0].Contains("overridable"));
		}

		[Test]
		public void StaticMethodWithScriptSkipAttributeMustHaveExactlyOneParameter() {
			var er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] static void M(); }", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1.M") && er.AllMessages[0].Contains("ScriptSkipAttribute") && er.AllMessages[0].Contains("one parameter"));

			er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] static void M(int i, int j); }", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1.M") && er.AllMessages[0].Contains("ScriptSkipAttribute") && er.AllMessages[0].Contains("one parameter"));
		}

		[Test]
		public void InstanceMethodWithScriptSkipAttributeCannotHaveParameters() {
			var er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] void M(int i); }", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1.M") && er.AllMessages[0].Contains("ScriptSkipAttribute") && er.AllMessages[0].Contains("no parameters"));

			er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] void M(int i, int j); }", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1.M") && er.AllMessages[0].Contains("ScriptSkipAttribute") && er.AllMessages[0].Contains("no parameters"));
		}

		[Test]
		public void ScriptSkipOnStaticMethodWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[ScriptSkip]
	public static void SomeMethod(int x) {
	}
}");

			var impl = FindMethod(types, "C1.SomeMethod", md);
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{0}"));
		}

		[Test]
		public void ScriptSkipOnInstanceMethodWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[ScriptSkip]
	public void SomeMethod() {
	}
}");

			var impl = FindMethod(types, "C1.SomeMethod", md);
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{this}"));
		}

		[Test]
		public void AlternateSignatureAttributeWorksOnMethods() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var types = Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[AlternateSignature]
	public void SomeMethod() {
	}


	[AlternateSignature]
	public void SomeMethod(int x) {
	}

	[ScriptName(""RenamedMethod"")]
	public void SomeMethod(int x, int y) {
	}
}");

			var methods = FindMethods(types, "C1.SomeMethod", md);
			Assert.That(methods.All(m => m.Item2.Name == "RenamedMethod"));
		}

		[Test]
		public void IfAnyMethodInAMethodGroupHasAnAlternateSignatureAttributeThenExactlyOneMethodMustNotHaveIt() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[AlternateSignature]
	public void SomeMethod() {
	}

	public void SomeMethod(int x) {
	}

	public void SomeMethod(int x, int y) {
	}
}", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1.SomeMethod") && er.AllMessages[0].Contains("AlternateSignatureAttribute") && er.AllMessages[0].Contains("same name"));


			md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
class C1 {
	[AlternateSignature]
	public void SomeMethod() {
	}

	[AlternateSignature]
	public void SomeMethod(int x) {
	}

	[AlternateSignature]
	public void SomeMethod(int x, int y) {
	}
}", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1.SomeMethod") && er.AllMessages[0].Contains("AlternateSignatureAttribute") && er.AllMessages[0].Contains("same name"));
		}
	}
}
