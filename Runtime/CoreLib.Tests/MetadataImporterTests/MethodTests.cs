using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests.MetadataImporterTests {
	[TestFixture]
	public class MethodTests : MetadataImporterTestBase {
		[Test]
		public void NonOverloadedMethodIsCamelCased() {
			Prepare(
@"public class C {
	public void SomeMethod() {
	}
}");

			var impl = FindMethod("C.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod"));
			Assert.That(impl.IgnoreGenericArguments, Is.False);
			Assert.That(impl.GeneratedMethodName, Is.EqualTo(impl.Name));
		}

		[Test]
		public void OverloadedMethodsGetDifferentNames() {
			Prepare(
@"public class C {
	public void SomeMethod() {
	}
	public void SomeMethod(int x) {
	}
	public void SomeMethod(string s) {
	}
	public void SomeMethod(int a, int b) {
	}
}");

			var methods = FindMethods("C.SomeMethod");
			var m1 = methods.Single(x => x.Item1.Parameters.Count == 0).Item2;
			Assert.That(m1.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m1.Name, Is.EqualTo("someMethod"));
			Assert.That(m1.IgnoreGenericArguments, Is.False);
			Assert.That(m1.GeneratedMethodName, Is.EqualTo(m1.Name));

			var m2 = methods.Single(x => x.Item1.Parameters.Count == 1 && x.Item1.Parameters[0].Type.GetDefinition().KnownTypeCode == KnownTypeCode.Int32).Item2;
			Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m2.Name, Is.EqualTo("someMethod$1"));
			Assert.That(m2.IgnoreGenericArguments, Is.False);
			Assert.That(m2.GeneratedMethodName, Is.EqualTo(m2.Name));

			var m3 = methods.Single(x => x.Item1.Parameters.Count == 1 && x.Item1.Parameters[0].Type.GetDefinition().KnownTypeCode == KnownTypeCode.String).Item2;
			Assert.That(m3.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m3.Name, Is.EqualTo("someMethod$2"));
			Assert.That(m3.IgnoreGenericArguments, Is.False);
			Assert.That(m3.GeneratedMethodName, Is.EqualTo(m3.Name));

			var m4 = methods.Single(x => x.Item1.Parameters.Count == 2).Item2;
			Assert.That(m4.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m4.Name, Is.EqualTo("someMethod$3"));
			Assert.That(m4.IgnoreGenericArguments, Is.False);
			Assert.That(m4.GeneratedMethodName, Is.EqualTo(m4.Name));
		}

		[Test]
		public void NameIsPreservedForImportedTypes() {
			Prepare(@"
using System.Runtime.CompilerServices;
[Imported]
class C {
	void SomeMethod() {
	}
	[PreserveName]
	void SomeMethod(int x) {
	}
	[PreserveCase]
	void SomeMethod(int x, int y) {
	}
	[ScriptName(""Renamed"")]
	void SomeMethod(int x, int y, int z) {
	}
}");

			var methods = FindMethods("C.SomeMethod");
			var m1 = methods.Single(x => x.Item1.Parameters.Count == 0).Item2;
			Assert.That(m1.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m1.Name, Is.EqualTo("someMethod"));
			Assert.That(m1.GeneratedMethodName, Is.EqualTo(m1.Name));

			var m2 = methods.Single(x => x.Item1.Parameters.Count == 1).Item2;
			Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m2.Name, Is.EqualTo("someMethod"));
			Assert.That(m2.GeneratedMethodName, Is.EqualTo(m2.Name));

			var m3 = methods.Single(x => x.Item1.Parameters.Count == 2).Item2;
			Assert.That(m3.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m3.Name, Is.EqualTo("SomeMethod"));
			Assert.That(m3.GeneratedMethodName, Is.EqualTo(m3.Name));

			var m4 = methods.Single(x => x.Item1.Parameters.Count == 3).Item2;
			Assert.That(m4.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m4.Name, Is.EqualTo("Renamed"));
			Assert.That(m4.GeneratedMethodName, Is.EqualTo(m4.Name));
		}

		[Test]
		public void MethodShadowingBaseMethodGetsANewName() {
			Prepare(
@"public class A {
	public void SomeMethod() {
	}
}
public class B : A {
	public void SomeMethod(int x) {
	}
}

public class C : B {
	public new void SomeMethod() {
	}
}");

			var impl = FindMethod("A.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod"));
			Assert.That(impl.IgnoreGenericArguments, Is.False);
			Assert.That(impl.GeneratedMethodName, Is.EqualTo(impl.Name));

			impl = FindMethod("B.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod$1"));
			Assert.That(impl.IgnoreGenericArguments, Is.False);
			Assert.That(impl.GeneratedMethodName, Is.EqualTo(impl.Name));

			impl = FindMethod("C.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod$2"));
			Assert.That(impl.IgnoreGenericArguments, Is.False);
			Assert.That(impl.GeneratedMethodName, Is.EqualTo(impl.Name));
		}

		[Test]
		public void ScriptNameAttributeWorksOnMethods() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C {
	[ScriptName(""Renamed"")]
	public void SomeMethod() {
	}
}");

			var impl = FindMethod("C.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("Renamed"));
		}

		[Test]
		public void SameScriptNameCanBeSpecifiedOnManyOverloads() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class C {
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

			var methods = FindMethods("C.SomeMethod");
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
			Prepare(
@"using System.Runtime.CompilerServices;

public class C {
	public void SomeMethod() {
	}
	[PreserveName]
	void SomeMethod(int x) {
	}
	[PreserveName]
	public void SomeMethod(int a, int b) {
	}
}");

			var methods = FindMethods("C.SomeMethod");
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
			Prepare(
@"using System.Runtime.CompilerServices;

public class C {
	public void SomeMethod() {
	}
	[PreserveCase]
	void SomeMethod(int x) {
	}
	[PreserveCase]
	public void SomeMethod(int a, int b) {
	}
}");

			var methods = FindMethods("C.SomeMethod");
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
			Prepare(
@"using System.Runtime.CompilerServices;

class A {
	[ScriptName(""RenamedMethod"")]
	public virtual void SomeMethod() {}
}

class B : A {
	public override void SomeMethod() {}
}
class C : B {
	public sealed override void SomeMethod() {}
}");

			var mb = FindMethod("B.SomeMethod");
			Assert.That(mb.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(mb.Name, Is.EqualTo("RenamedMethod"));

			var mc = FindMethod("C.SomeMethod");
			Assert.That(mc.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(mc.Name, Is.EqualTo("RenamedMethod"));
		}


		[Test]
		public void ImplicitInterfaceImplementationMethodsGetTheirNameFromTheInterface() {
			Prepare(
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

			var m = FindMethod("C.SomeMethod");
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("RenamedMethod"));

			var m2 = FindMethod("C.SomeMethod2");
			Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m2.Name, Is.EqualTo("RenamedMethod2"));
		}

		[Test]
		public void ExplicitInterfaceImplementationMethodsGetTheirNameFromTheInterface() {
			Prepare(
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

			var m = FindMethod("C.SomeMethod");
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("RenamedMethod"));

			var m2 = FindMethod("C.SomeMethod2");
			Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m2.Name, Is.EqualTo("RenamedMethod2"));
		}

		[Test, Ignore("We currently don't allow this inheritance")]
		public void MethodCanImplementTwoInterfaceMethodsWithTheSameName() {
			Prepare(
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

			var m = FindMethod("C.SomeMethod");
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("RenamedMethod"));
		}

		[Test, Ignore("We currently don't allow this inheritance")]
		public void OverridingMethodCanImplementInterfaceMethodWithTheSameName() {
			Prepare(
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
	public sealed override void SomeMethod(int i) {}
}");

			var m = FindMethod("D.SomeMethod");
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("RenamedMethod"));
		}

		[Test]
		public void MethodCannotImplementTwoInterfaceMethodsIfTheNamesAreDifferent() {
			Prepare(
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
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("implement") && AllErrorTexts[0].Contains("differing script names") && AllErrorTexts[0].Contains("C.SomeMethod"));
		}

		[Test]
		public void OverridingMethodCannotImplementInterfaceMethodIfTheNamesDiffer() {
			Prepare(
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
	public sealed override void SomeMethod(int i) {
	}
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("implement") && AllErrorTexts[0].Contains("different script name") && AllErrorTexts[0].Contains("D.SomeMethod") && AllErrorTexts[0].Contains("I.SomeMethod"));
		}

		[Test, Ignore("We currently do not allow this inheritance")]
		public void BaseMethodCanImplementInterfaceMemberIfTheNamesAreTheSame() {
			Prepare(
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
			Prepare(
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
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.Fail("TODO: Assert message");
		}

		[Test]
		public void CannotSpecifyScriptNameAttributeOnMethodImplementingInterfaceMember() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	void SomeMethod(int i);
}

class C : I {
	[ScriptName(""RenamedMethod2"")]
	public void SomeMethod(int i) {}
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("ScriptName") && AllErrorTexts[0].Contains("C.SomeMethod") && AllErrorTexts[0].Contains("interface member"));
		}

		[Test]
		public void CannotSpecifyPreserveNameAttributeOnMethodImplementingInterfaceMember() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	void SomeMethod(int i);
}

class C : I {
	[PreserveName]
	public void SomeMethod(int i) {}
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("PreserveName") && AllErrorTexts[0].Contains("C.SomeMethod") && AllErrorTexts[0].Contains("interface member"));
		}

		[Test]
		public void CannotSpecifyPreserveCaseAttributeOnMethodImplementingInterfaceMember() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	void SomeMethod(int i);
}

class C : I {
	[PreserveCase]
	public void SomeMethod(int i) {}
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("PreserveCase") && AllErrorTexts[0].Contains("C.SomeMethod") && AllErrorTexts[0].Contains("interface member"));
		}

		[Test]
		public void CannotSpecifyScriptNameAttributeOnOverridingMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;

class B {
	public virtual void SomeMethod(int i) {}
}

class D : B {
	[ScriptName(""RenamedMethod"")]
	public sealed override void SomeMethod(int i) {}
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("ScriptName") && AllErrorTexts[0].Contains("D.SomeMethod") && AllErrorTexts[0].Contains("overrides"));
		}

		[Test]
		public void CannotSpecifyPreserveNameAttributeOnOverridingMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;

class B {
	public virtual void SomeMethod(int i) {}
}

class D : B {
	[PreserveName]
	public sealed override void SomeMethod(int i) {}
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("ScriptName") && AllErrorTexts[0].Contains("D.SomeMethod") && AllErrorTexts[0].Contains("overrides"));
		}

		[Test]
		public void CannotSpecifyPreserveCaseAttributeOnOverridingMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;

class B {
	public virtual void SomeMethod(int i) {}
}

class D : B {
	[PreserveCase]
	public sealed override void SomeMethod(int i) {}
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("PreserveCase") && AllErrorTexts[0].Contains("D.SomeMethod") && AllErrorTexts[0].Contains("overrides"));
		}

		[Test]
		public void ScriptNameCanBeSpecifiedOnInterfaceMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	[ScriptName(""RenamedMethod"")]
	void SomeMethod();
}");

			var m = FindMethod("I.SomeMethod");
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("RenamedMethod"));
		}

		[Test]
		public void PreserveNameCanBeSpecifiedOnInterfaceMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	[PreserveName]
	void SomeMethod();
}");

			var m = FindMethod("I.SomeMethod");
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("someMethod"));
		}

		[Test]
		public void PreserveCaseCanBeSpecifiedOnInterfaceMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;

interface I {
	[PreserveCase]
	void SomeMethod();
}");

			var m = FindMethod("I.SomeMethod");
			Assert.That(m.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m.Name, Is.EqualTo("SomeMethod"));
		}

		[Test]
		public void ScriptNameOnMethodCanBeInvalidIdentifier() {
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptName(""a b"")] public void M() {} }");
			var m = FindMethod("C1.M");
			Assert.That(m.Name, Is.EqualTo("a b"));
		}

		[Test]
		public void EmptyScriptNameOnMethodResultsInLiteralCodeImplementation() {
			Prepare(
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

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{this}()"));

			impl = FindMethod("C2.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{this}({x})"));

			impl = FindMethod("C3.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{this}({x}, {y})"));
		}

		[Test]
		public void EmptyScriptNameCannotBeSpecifiedOnInterfaceMethod() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public interface I1 { [ScriptName("""")] void M(); }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("I1.M") && AllErrorTexts[0].Contains("ScriptName") && AllErrorTexts[0].Contains("interface method") && AllErrorTexts[0].Contains("empty name"));
		}

		[Test]
		public void EmptyScriptNameCannotBeSpecifiedOnVirtualOrAbstractMethod() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptName("""")] public virtual void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("ScriptName") && AllErrorTexts[0].Contains("overridable") && AllErrorTexts[0].Contains("empty name"));

			er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptName("""")] public abstract void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("ScriptName") && AllErrorTexts[0].Contains("overridable") && AllErrorTexts[0].Contains("empty name"));
		}

		[Test]
		public void EmptyScriptNameCannotBeSpecifiedOnStaticMethod() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptName("""")] public static void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("ScriptName") && AllErrorTexts[0].Contains("static") && AllErrorTexts[0].Contains("empty name"));
		}

		[Test]
		public void ScriptSkipAttributeCannotBeSpecifiedOnInterfaceMethod() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public interface I1 { [ScriptSkip] void M(); }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("I1.M") && AllErrorTexts[0].Contains("ScriptSkipAttribute") && AllErrorTexts[0].Contains("interface method"));
		}

		[Test]
		public void ScriptSkipAttributeCannotBeSpecifiedOnVirtualOrAbstractMethod() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] public virtual void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("ScriptSkipAttribute") && AllErrorTexts[0].Contains("overridable"));

			er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] public abstract void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("ScriptSkipAttribute") && AllErrorTexts[0].Contains("overridable"));
		}

		[Test]
		public void ScriptSkipAttributeCannotBeSpecifiedOnMethodImplementingInterfaceMember() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public interface I { void M(); } public class C : I { [ScriptSkip] public void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C.M") && AllErrorTexts[0].Contains("ScriptSkipAttribute") && AllErrorTexts[0].Contains("implements"));
		}

		[Test]
		public void ScriptSkipAttributeCannotBeSpecifiedOnMethodThatOverridesABaseMember() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class B { public virtual void M() {} } public class D : B { [ScriptSkip] public sealed override void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("D.M") && AllErrorTexts[0].Contains("ScriptSkipAttribute") && AllErrorTexts[0].Contains("overrides"));
		}

		[Test]
		public void StaticMethodWithScriptSkipAttributeMustHaveExactlyOneParameter() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] static void M(); }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("ScriptSkipAttribute") && AllErrorTexts[0].Contains("one parameter"));

			er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] static void M(int i, int j); }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("ScriptSkipAttribute") && AllErrorTexts[0].Contains("one parameter"));
		}

		[Test]
		public void InstanceMethodWithScriptSkipAttributeCannotHaveParameters() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] void M(int i); }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("ScriptSkipAttribute") && AllErrorTexts[0].Contains("no parameters"));

			er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] void M(int i, int j); }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("ScriptSkipAttribute") && AllErrorTexts[0].Contains("no parameters"));
		}

		[Test]
		public void ScriptSkipOnStaticMethodWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[ScriptSkip]
	public static void SomeMethod(int x) {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{x}"));
		}

		[Test]
		public void ScriptSkipOnInstanceMethodWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[ScriptSkip]
	public void SomeMethod() {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{this}"));
		}

		[Test]
		public void AlternateSignatureAttributeWorksOnMethods() {
			Prepare(
@"using System.Runtime.CompilerServices;
public class C1 {
	[AlternateSignature]
	public void SomeMethod() {
	}

	[AlternateSignature]
	public void SomeMethod(int x) {
	}

	public void SomeMethod(int x, int y) {
	}
}");

			var methods = FindMethods("C1.SomeMethod");
			Assert.That(methods.All(m => m.Item2.Name == "someMethod"));
			Assert.That(methods.All(m => m.Item2.GeneratedMethodName == (m.Item1.Parameters.Count == 2 ? m.Item2.Name : null)));
		}

		[Test]
		public void AlternateSignatureAttributeWorksWhenTheMainMethodIsRenamed() {
			Prepare(
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

			var methods = FindMethods("C1.SomeMethod");
			Assert.That(methods.All(m => m.Item2.Name == "RenamedMethod"));
			Assert.That(methods.All(m => m.Item2.GeneratedMethodName == (m.Item1.Parameters.Count == 2 ? m.Item2.Name : null)));
		}

		[Test]
		public void AlternateSignatureAttributeDoesNotConsiderNonScriptableOrInlineCodeMethods() {
			Prepare(
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

	[InlineCode(""X"")]
	public void SomeMethod(int a, int b, int c) {
	}

	[NonScriptable]
	public void SomeMethod(int a, int b, int c, int d) {
	}
}");

			var methods = FindMethods("C1.SomeMethod");
			Assert.That(methods.Where(m => m.Item1.Parameters.Count < 3).All(m => m.Item2.Name == "RenamedMethod"));
			Assert.That(methods.Where(m => m.Item1.Parameters.Count < 3).All(m => m.Item2.GeneratedMethodName == (m.Item1.Parameters.Count == 2 ? m.Item2.Name : null)));
			Assert.That(FindMethod("C1.SomeMethod", 3).Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(FindMethod("C1.SomeMethod", 4).Type, Is.EqualTo(MethodScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void IfAnyMethodInAMethodGroupHasAnAlternateSignatureAttributeThenExactlyOneMethodMustNotHaveIt() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[AlternateSignature]
	public void SomeMethod() {
	}

	public void SomeMethod(int x) {
	}

	public void SomeMethod(int x, int y) {
	}
}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.SomeMethod") && AllErrorTexts[0].Contains("AlternateSignatureAttribute") && AllErrorTexts[0].Contains("same name"));

			Prepare(
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
}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(3));
			Assert.That(AllErrorTexts.All(m => m.Contains("C1.SomeMethod") && m.Contains("AlternateSignatureAttribute") && m.Contains("same name")));
		}

		[Test]
		public void ScriptAliasAttributeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[ScriptAlias(""Some.Thing.Somewhere"")]
	public static void SomeMethod() {
	}
}
class C2 {
	[ScriptAlias(""global[x].abc"")]
	public static void SomeMethod(int x) {
	}
}
class C3 {
	[ScriptAlias(""x.y"")]
	public static void SomeMethod(int x, string y) {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("Some.Thing.Somewhere()"));

			impl = FindMethod("C2.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("global[x].abc({x})"));

			impl = FindMethod("C3.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("x.y({x}, {y})"));
		}

		[Test]
		public void ScriptAliasAttributeCanOnlyBeSpecifiedOnStaticMethods() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[ScriptAlias(""x.y"")]
	public void SomeMethod() {
	}
}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.SomeMethod") && AllErrorTexts[0].Contains("ScriptAliasAttribute") && AllErrorTexts[0].Contains("must be static"));
		}

		[Test]
		public void InlineCodeAttributeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1<T1> {
	class C2<T2> {
		[InlineCode(""_({T1})._({T2})._({T3})._({T4})._({x})._({y})._({this})"")]
		public void SomeMethod<T3, T4>(int x, string y) {}
	}
}");

			var impl = FindMethod("C1`1+C2`1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("_({T1})._({T2})._({T3})._({T4})._({x})._({y})._({this})"));
			Assert.That(impl.NonVirtualInvocationLiteralCode, Is.EqualTo("_({T1})._({T2})._({T3})._({T4})._({x})._({y})._({this})"));
			Assert.That(impl.GeneratedMethodName, Is.Null);
		}

		[Test]
		public void InlineCodeWithGeneratedMethodNameWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[InlineCode(""X"", GeneratedMethodName = ""GeneratedName"")]
	public void SomeMethod(int x, string y) {}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("X"));
			Assert.That(impl.NonVirtualInvocationLiteralCode, Is.EqualTo("X"));
			Assert.That(impl.GeneratedMethodName, Is.EqualTo("GeneratedName"));
		}

		[Test]
		public void InlineCodeWithNonVirtualCodeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[InlineCode(""X"", NonVirtualCode = ""X({x}, {y})"")]
	public void SomeMethod(int x, string y) {}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("X"));
			Assert.That(impl.NonVirtualInvocationLiteralCode, Is.EqualTo("X({x}, {y})"));
			Assert.That(impl.GeneratedMethodName, Is.Null);
		}

		[Test]
		public void MethodDerivedFromInlineCodeWithGeneratedMethodIsNormalMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;
class B {
	[InlineCode(""X"", GeneratedMethodName = ""GeneratedName"")]
	public virtual void SomeMethod(int x, string y) {}
}
class D : B {
	public override void SomeMethod(int x, string y) {}
}");

			var impl = FindMethod("D.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("GeneratedName"));
			Assert.That(impl.GeneratedMethodName, Is.EqualTo(impl.Name));
		}

		[Test]
		public void InlineCodeWithGeneratedMethodNameAvoidsNameClashes() {
			Prepare(
@"using System.Runtime.CompilerServices;
public class C {
	[InlineCode(""X"", GeneratedMethodName = ""someMethod"")]
	public void SomeMethod(int x, string y) {}

	public void SomeMethod() {}
}");

			var impl = FindMethod("C.SomeMethod", 0);
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod$1"));
			Assert.That(impl.GeneratedMethodName, Is.EqualTo(impl.Name));
		}

		[Test]
		public void MethodImplementingInlineCodeWithGeneratedMethodIsNormalMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;
interface I {
	[InlineCode(""X"", GeneratedMethodName = ""generatedName"")]
	public virtual void SomeMethod(int x, string y) {}
}
class C : I {
	public void SomeMethod(int x, string y) {}
}");

			var impl = FindMethod("C.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("generatedName"));
			Assert.That(impl.GeneratedMethodName, Is.EqualTo(impl.Name));
		}

		[Test]
		public void MethodImplementingAnInterfaceMemberCanSpecifyInlineCode() {
			Prepare(
@"using System.Runtime.CompilerServices;
interface I {
	[InlineCode(""X"", GeneratedMethodName = ""generatedName"")]
	public virtual void SomeMethod(int x, string y) {}
}
class C1 : I {
	[InlineCode(""Y"", GeneratedMethodName = ""generatedName"")]
	public void SomeMethod(int x, string y) {}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("Y"));
			Assert.That(impl.GeneratedMethodName, Is.EqualTo("generatedName"));
		}

		[Test]
		public void MethodImplementingInlineCodeWithGeneratedMethodNameAvoidsNameClashes() {
			Prepare(
@"using System.Runtime.CompilerServices;
interface I {
	[InlineCode(""X"", GeneratedMethodName = ""generatedName"")]
	public virtual void SomeMethod(int x, string y) {}
}
public class C : I {
	public void SomeMethod(int x, string y) {}

	public void GeneratedName(int x, string y) {}
}");

			var impl = FindMethod("C.GeneratedName");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("generatedName$1"));
			Assert.That(impl.GeneratedMethodName, Is.EqualTo(impl.Name));
		}

		[Test]
		public void InlineCodeAttributeWithUnknownArgumentsIsAnError() {
			Prepare(@"using System.Runtime.CompilerServices; class C1 { [InlineCode(""{this}"")] public static void SomeMethod() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.SomeMethod") && AllErrorTexts[0].Contains("inline code") && AllErrorTexts[0].Contains("{this}"));

			Prepare(@"using System.Runtime.CompilerServices; class C1 { [InlineCode(""{x}"")] public void SomeMethod() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.SomeMethod") && AllErrorTexts[0].Contains("inline code") && AllErrorTexts[0].Contains("{x}"));

			Prepare(@"using System.Runtime.CompilerServices; class C1 { [InlineCode(""X"", NonVirtualCode = ""{x}"")] public void SomeMethod() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.SomeMethod") && AllErrorTexts[0].Contains("inline code") && AllErrorTexts[0].Contains("{x}"));
		}

		[Test]
		public void InlineCodeAttributeReferencingUnknownTypeIsAnError() {
			Prepare(@"using System.Runtime.CompilerServices; class C1 { [InlineCode(""{$Some.Nonexistent.Type}"")] public static void SomeMethod() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.SomeMethod") && AllErrorTexts[0].Contains("inline code") && AllErrorTexts[0].Contains("Some.Nonexistent.Type"));
		}

		[Test]
		public void InlineCodeAttributeCannotBeSpecifiedOnInterfaceMethod() {
			Prepare(@"using System.Runtime.CompilerServices; public interface I1 { [InlineCode(""X"")] void M(); }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("I1.M") && AllErrorTexts[0].Contains("InlineCodeAttribute") && AllErrorTexts[0].Contains("interface method"));
		}

		[Test]
		public void InlineCodeAttributeCannotBeSpecifiedOnOverridableMethod() {
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [InlineCode(""X"")] public virtual void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("InlineCodeAttribute") && AllErrorTexts[0].Contains("overridable"));

			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [InlineCode(""X"")] public abstract void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("InlineCodeAttribute") && AllErrorTexts[0].Contains("overridable"));
		}

		[Test]
		public void InlineCodeAttributeCannotBeSpecifiedOnMethodThatOverridesABaseMember() {
			Prepare(@"using System.Runtime.CompilerServices; public class B { public virtual void M() {} } public class D : B { [InlineCode(""X"")] public sealed override void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("D.M") && AllErrorTexts[0].Contains("InlineCodeAttribute") && AllErrorTexts[0].Contains("overrides"));
		}

		[Test]
		public void InstanceMethodOnFirstArgumentAttributeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[InstanceMethodOnFirstArgument]
	[PreserveCase]
	public static void SomeMethod(int a) {
	}

	[InstanceMethodOnFirstArgument]
	[ScriptName(""RenamedMethod"")]
	public static void SomeMethod2(int a) {
	}

	[InstanceMethodOnFirstArgument]
	public static void SomeMethod3(int a, int x) {
	}

	[InstanceMethodOnFirstArgument]
	[PreserveName]
	public static void SomeMethod4(int b, string s1, string s2) {
	}
}
");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{a}.SomeMethod()"));

			impl = FindMethod("C1.SomeMethod2");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{a}.RenamedMethod()"));

			impl = FindMethod("C1.SomeMethod3");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{a}.someMethod3({x})"));

			impl = FindMethod("C1.SomeMethod4");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{b}.someMethod4({s1}, {s2})"));
		}

		[Test]
		public void InstanceMethodOnFirstArgumentAttributeWorksWithExpandParams() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[InstanceMethodOnFirstArgument]
	public static void SomeMethod1(int a, int b, params int[] c) {
	}

	[InstanceMethodOnFirstArgument]
	[ExpandParams]
	public static void SomeMethod2(int a, int b, params int[] c) {
	}
}
");

			var m1 = FindMethod("C1.SomeMethod1");
			Assert.That(m1.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(m1.LiteralCode, Is.EqualTo("{a}.someMethod1({b}, {c})"));

			var m2 = FindMethod("C1.SomeMethod2");
			Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(m2.LiteralCode, Is.EqualTo("{a}.someMethod2({b}, {*c})"));
		}

		[Test]
		public void InstanceMethodOnFirstArgumentAttributeCannotBeSpecifiedOnInstanceMember() {
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [InstanceMethodOnFirstArgument] public void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("InstanceMethodOnFirstArgumentAttribute") && AllErrorTexts[0].Contains("static"));
		}

		[Test]
		public void InstanceMethodOnFirstArgumentAttributeCannotBeSpecifiedOnMethodWithoutParameters() {
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [InstanceMethodOnFirstArgument] public static void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("InstanceMethodOnFirstArgumentAttribute") && AllErrorTexts[0].Contains("parameters"));
		}

		[Test]
		public void InstanceMethodOnFirstArgumentAttributeCannotBeSpecifiedOnMethodWithASingleParamsParameter() {
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [InstanceMethodOnFirstArgument] public static void M(params int[] args) {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("InstanceMethodOnFirstArgumentAttribute") && AllErrorTexts[0].Contains("params"));
		}

		[Test]
		public void ExpandParamsAttributeCanOnlyBeAppliedToMethodWithParamArrayIfInstanceMethodOnFirstArgument() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[ExpandParams]
	[InstanceMethodOnFirstArgument]
	public static void M2(int a, int b, int[] c) {}
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1.M2") && m.Contains("params") && m.Contains("ExpandParamsAttribute")));
		}

		[Test]
		public void NonScriptableAttributeCausesAMethodToBeNotUsableFromScript() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[NonScriptable]
	public static void SomeMethod() {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void NonScriptableAttributeIsNotInheritedFromInterfaceMember() {
			Prepare(
@"using System.Runtime.CompilerServices;
public interface I { [NonScriptable] void SomeMethod(); }
public class C1 : I {
	public void SomeMethod() {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod"));
		}

		[Test]
		public void NonScriptableAttributeIsInheritedForExplicitInterfaceImplementation() {
			Prepare(
@"using System.Runtime.CompilerServices;
public interface I { [NonScriptable] void SomeMethod(); }
public class C1 : I {
	void I.SomeMethod() {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void CanSpecifyNameForMethodImplementingUnusableInterfaceMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;
interface I { [NonScriptable] void SomeMethod(); }
class C1 : I {
	[ScriptName(""renamed"")]
	public void SomeMethod() {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("renamed"));
		}

		[Test]
		public void CanSpecifyInlineCodeForMethodImplementingUnusableInterfaceMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;
interface I { [NonScriptable] void SomeMethod(); }
class C1 : I {
	[InlineCode(""X"")]
	public void SomeMethod() {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("X"));
		}

		[Test]
		public void CanSpecifyScriptSkipForMethodImplementingUnusableInterfaceMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;
interface I { [NonScriptable] void SomeMethod(); }
class C1 : I {
	[ScriptSkip]
	public void SomeMethod() {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{this}"));
		}

		[Test]
		public void NonScriptableAttributeIsNotInheritedFromBaseMember() {
			Prepare(
@"using System.Runtime.CompilerServices;
public class B { [NonScriptable] public virtual void SomeMethod(); }
public class C1 : B {
	public override void SomeMethod() {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod"));
		}

		[Test]
		public void CanSpecifyNameForMethodOverridingUnusableBaseMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;
class B { [NonScriptable] public virtual void SomeMethod(); }
class C1 : B {
	[ScriptName(""renamed"")]
	public override void SomeMethod() {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("renamed"));
		}

		[Test]
		public void CanSpecifyInlineCodeForMethodOverridingUnusableBaseMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;
class B { [NonScriptable] public virtual void SomeMethod(); }
class C1 : B {
	[InlineCode(""X"")]
	public sealed override void SomeMethod() {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("X"));
		}

		[Test]
		public void CanSpecifyScriptSkipForMethodOverridingUnusableBaseMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;
class B { [NonScriptable] public virtual void SomeMethod(); }
class C1 : B {
	[ScriptSkip]
	public sealed override void SomeMethod() {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("{this}"));
		}

		[Test]
		public void IgnoreGenericArgumentsAttributeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[IgnoreGenericArguments]
	public void SomeMethod<T>() {
	}
}");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.IgnoreGenericArguments, Is.True);
		}

		[Test]
		public void IgnoreGenericArgumentsCannotBeSpecifiedOnOverridingMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;

class B {
	public virtual void SomeMethod<T>(T t) {}
}

class D : B {
	[IgnoreGenericArguments]
	public sealed override void SomeMethod<T>(T t) {}
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("IgnoreGenericArgumentsAttribute") && AllErrorTexts[0].Contains("D.SomeMethod") && AllErrorTexts[0].Contains("overrides"));
		}

		[Test]
		public void NonPublicMethodsArePrefixedWithADollarIfSymbolsAreNotMinimized() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public void SomeMethod() {}
}

public class C2 {
	private void SomeMethod1() {}
	internal void SomeMethod2() {}
}", minimizeNames: false);

			Assert.That(FindMethod("C1.SomeMethod").Name, Is.EqualTo("$someMethod"));
			Assert.That(FindMethod("C2.SomeMethod1").Name, Is.EqualTo("$someMethod1"));
			Assert.That(FindMethod("C2.SomeMethod2").Name, Is.EqualTo("$someMethod2"));
		}

		[Test]
		public void MethodsOnDelegateCannotBeUsed() {
			Prepare("public delegate void Del();");
			var del = AllTypes["Del"];

			Assert.That(del.Methods.Where(m => !m.IsConstructor).Select(m => new { m.Name, Impl = Metadata.GetMethodSemantics(m) }).All(m => m.Impl.Type == MethodScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void IntrinsicOperatorAttributeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	[IntrinsicOperator]
	public static C1 operator+(C1 a, C1 b) { return null; }
}");

			Assert.That(FindMethod("C1.op_Addition").Type, Is.EqualTo(MethodScriptSemantics.ImplType.NativeOperator));
		}

		[Test]
		public void IntrinsicOperatorAttributeCannotBeAppliedToNonOperatorMethod() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	[IntrinsicOperator]
	public static void M() {}
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1.M") && AllErrorTexts[0].Contains("IntrinsicOperatorAttribute") && AllErrorTexts[0].Contains("operator method"));
		}

		[Test]
		public void IntrinsicOperatorAttributeCannotBeAppliedToConversionOperator() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C2 {}
class C3 {}
class C1 {
	[IntrinsicOperator]
	public static implicit operator C2(C1 c) { return null; }
	[IntrinsicOperator]
	public static explicit operator C3(C1 c) { return null; }
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(2));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("IntrinsicOperatorAttribute") && m.Contains("conversion operator")));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("IntrinsicOperatorAttribute") && m.Contains("conversion operator")));
		}

		[Test]
		public void ExpandParamsAttributeCausesMethodToUseExpandParamsOption() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public void M1(int a, int b, params int[] c) {}
	[ExpandParams]
	public void M2(int a, int b, params int[] c) {}
}");

			Assert.That(FindMethod("C1.M1").ExpandParams, Is.False);
			Assert.That(FindMethod("C1.M2").ExpandParams, Is.True);
		}

		[Test]
		public void ExpandParamsAttributeCanOnlyBeAppliedToMethodWithParamArray() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[ExpandParams]
	public void M2(int a, int b, int[] c) {}
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1.M2") && m.Contains("params") && m.Contains("ExpandParamsAttribute")));
		}

		[Test]
		public void MethodInGlobalMethodsClassHonorsIgnoreGenericArgumentsAttribute() {
			Prepare(
@"using System.Runtime.CompilerServices;
[GlobalMethods]
static class C {
	[IgnoreGenericArguments]
	public static void M1<T>(T t) {}

	public static void M2<T>(T t) {}
}");
			Assert.That(FindMethod("C.M1").IgnoreGenericArguments, Is.True);
			Assert.That(FindMethod("C.M2").IgnoreGenericArguments, Is.False);
		}

		[Test]
		public void MethodInGlobalMethodsClassHonorsExpandParamsAttribute() {
			Prepare(
@"using System.Runtime.CompilerServices;
[GlobalMethods]
static class C {
	[ExpandParams]
	public static void M1(params object[] arg) {}

	public static void M2(params object[] arg) {}
}");
			Assert.That(FindMethod("C.M1").ExpandParams, Is.True);
			Assert.That(FindMethod("C.M2").ExpandParams, Is.False);
		}

		[Test]
		public void EnumerateAsArrayWorks() {
			Prepare(@"
using System.Runtime.CompilerServices;
using System.Collections.Generic;
public class C1 {
	[EnumerateAsArray]
	public IEnumerator<int> GetEnumerator() { return null; }
}
[Serializable]
public class C2 {
	[EnumerateAsArray]
	public IEnumerator<int> GetEnumerator() { return null; }
}
public class C3 {
	[EnumerateAsArray]
	[ScriptSkip]
	public IEnumerator<int> GetEnumerator() { return null; }
}
public class C4 {
	[EnumerateAsArray]
	[InlineCode(""X"")]
	public IEnumerator<int> GetEnumerator() { return null; }
}
public class C5 {
	[EnumerateAsArray]
	[ScriptName("""")]
	public IEnumerator<int> GetEnumerator() { return null; }
}
public class C6 : IEnumerable<int> {
	[EnumerateAsArray]
	public IEnumerator<int> GetEnumerator() { return null; }
}
public class B {
	public virtual IEnumerator<int> GetEnumerator() { return null; }
}
public class C7 : B {
	[EnumerateAsArray]
	public override IEnumerator<int> GetEnumerator() { return null; }
}
");
			Assert.That(FindMethod("C1.GetEnumerator").EnumerateAsArray, Is.True);
			Assert.That(FindMethod("C2.GetEnumerator").EnumerateAsArray, Is.True);
			Assert.That(FindMethod("C3.GetEnumerator").EnumerateAsArray, Is.True);
			Assert.That(FindMethod("C4.GetEnumerator").EnumerateAsArray, Is.True);
			Assert.That(FindMethod("C5.GetEnumerator").EnumerateAsArray, Is.True);
			Assert.That(FindMethod("C6.GetEnumerator").EnumerateAsArray, Is.True);
			Assert.That(FindMethod("C7.GetEnumerator").EnumerateAsArray, Is.True);
		}

		[Test]
		public void SpecifyingEnumerateAsArrayOnMethodThatIsNotAGetEnumeratorMethodIsAnError() {
			Prepare(
@"using System.Runtime.CompilerServices;
using System.Collections.Generic;
class C1 {
	[EnumerateAsArray]
	public IEnumerator<int> Something() { return null; }
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("EnumerateAsArrayAttribute") && m.Contains("GetEnumerator")));

			Prepare(
@"using System.Runtime.CompilerServices;
using System.Collections.Generic;
class C1 {
	[EnumerateAsArray]
	public static IEnumerator<int> GetEnumerator() { return null; }
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("EnumerateAsArrayAttribute") && m.Contains("GetEnumerator")));

			Prepare(
@"using System.Runtime.CompilerServices;
using System.Collections.Generic;
class C1 {
	[EnumerateAsArray]
	public IEnumerator<int> GetEnumerator<T>() { return null; }
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("EnumerateAsArrayAttribute") && m.Contains("GetEnumerator")));

			Prepare(
@"using System.Runtime.CompilerServices;
using System.Collections.Generic;
class C1 {
	[EnumerateAsArray]
	public IEnumerator<int> GetEnumerator(int i) { return null; }
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("EnumerateAsArrayAttribute") && m.Contains("GetEnumerator")));
		}
	}
}
