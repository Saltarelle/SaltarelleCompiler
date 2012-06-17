using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class MethodTests : ScriptSharpMetadataImporterTestBase {
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
			Assert.That(impl.GenerateCode, Is.True);
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
			Assert.That(impl.GenerateCode, Is.True);

			impl = FindMethod("B.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod$1"));
			Assert.That(impl.IgnoreGenericArguments, Is.False);
			Assert.That(impl.GenerateCode, Is.True);

			impl = FindMethod("C.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(impl.Name, Is.EqualTo("someMethod$2"));
			Assert.That(impl.IgnoreGenericArguments, Is.False);
			Assert.That(impl.GenerateCode, Is.True);
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

		[Test]
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

		[Test]
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

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("implement") && AllErrors[0].Contains("differing script names") && AllErrors[0].Contains("C.SomeMethod"));
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

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("implement") && AllErrors[0].Contains("different script name") && AllErrors[0].Contains("D.SomeMethod") && AllErrors[0].Contains("I.SomeMethod"));
		}

		[Test]
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

			Assert.That(AllErrors, Has.Count.EqualTo(1));
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

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("ScriptName") && AllErrors[0].Contains("C.SomeMethod") && AllErrors[0].Contains("interface member"));
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

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("PreserveName") && AllErrors[0].Contains("C.SomeMethod") && AllErrors[0].Contains("interface member"));
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

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("PreserveCase") && AllErrors[0].Contains("C.SomeMethod") && AllErrors[0].Contains("interface member"));
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

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("ScriptName") && AllErrors[0].Contains("D.SomeMethod") && AllErrors[0].Contains("overrides"));
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

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("ScriptName") && AllErrors[0].Contains("D.SomeMethod") && AllErrors[0].Contains("overrides"));
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

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("PreserveCase") && AllErrors[0].Contains("D.SomeMethod") && AllErrors[0].Contains("overrides"));
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
		public void ScriptNameOnMethodMustBeValidIdentifierOrBeEmpty() {
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptName(""a b"")] public void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1") && AllErrors[0].Contains("ScriptName") && AllErrors[0].Contains("must be a valid JavaScript identifier"));

			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptName(""a b"")] public void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1") && AllErrors[0].Contains("ScriptName") && AllErrors[0].Contains("must be a valid JavaScript identifier"));
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
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("I1.M") && AllErrors[0].Contains("ScriptName") && AllErrors[0].Contains("interface method") && AllErrors[0].Contains("empty name"));
		}

		[Test]
		public void EmptyScriptNameCannotBeSpecifiedOnVirtualOrAbstractMethod() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptName("""")] public virtual void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.M") && AllErrors[0].Contains("ScriptName") && AllErrors[0].Contains("overridable") && AllErrors[0].Contains("empty name"));

			er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptName("""")] public abstract void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.M") && AllErrors[0].Contains("ScriptName") && AllErrors[0].Contains("overridable") && AllErrors[0].Contains("empty name"));
		}

		[Test]
		public void EmptyScriptNameCannotBeSpecifiedOnStaticMethod() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptName("""")] public static void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.M") && AllErrors[0].Contains("ScriptName") && AllErrors[0].Contains("static") && AllErrors[0].Contains("empty name"));
		}

		[Test]
		public void ScriptSkipAttributeCannotBeSpecifiedOnInterfaceMethod() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public interface I1 { [ScriptSkip] void M(); }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("I1.M") && AllErrors[0].Contains("ScriptSkipAttribute") && AllErrors[0].Contains("interface method"));
		}

		[Test]
		public void ScriptSkipAttributeCannotBeSpecifiedOnVirtualOrAbstractMethod() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] public virtual void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.M") && AllErrors[0].Contains("ScriptSkipAttribute") && AllErrors[0].Contains("overridable"));

			er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] public abstract void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.M") && AllErrors[0].Contains("ScriptSkipAttribute") && AllErrors[0].Contains("overridable"));
		}

		[Test]
		public void ScriptSkipAttributeCannotBeSpecifiedOnMethodImplementingInterfaceMember() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public interface I { void M(); } public class C : I { [ScriptSkip] public void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C.M") && AllErrors[0].Contains("ScriptSkipAttribute") && AllErrors[0].Contains("implements"));
		}

		[Test]
		public void ScriptSkipAttributeCannotBeSpecifiedOnMethodThatOverridesABaseMember() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class B { public virtual void M() {} } public class D : B { [ScriptSkip] public sealed override void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("D.M") && AllErrors[0].Contains("ScriptSkipAttribute") && AllErrors[0].Contains("overrides"));
		}

		[Test]
		public void StaticMethodWithScriptSkipAttributeMustHaveExactlyOneParameter() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] static void M(); }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.M") && AllErrors[0].Contains("ScriptSkipAttribute") && AllErrors[0].Contains("one parameter"));

			er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] static void M(int i, int j); }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.M") && AllErrors[0].Contains("ScriptSkipAttribute") && AllErrors[0].Contains("one parameter"));
		}

		[Test]
		public void InstanceMethodWithScriptSkipAttributeCannotHaveParameters() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] void M(int i); }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.M") && AllErrors[0].Contains("ScriptSkipAttribute") && AllErrors[0].Contains("no parameters"));

			er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [ScriptSkip] void M(int i, int j); }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.M") && AllErrors[0].Contains("ScriptSkipAttribute") && AllErrors[0].Contains("no parameters"));
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
			Assert.That(impl.LiteralCode, Is.EqualTo("{0}"));
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
			Assert.That(methods.All(m => m.Item2.GenerateCode == (m.Item1.Parameters.Count == 2)));
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
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.SomeMethod") && AllErrors[0].Contains("AlternateSignatureAttribute") && AllErrors[0].Contains("same name"));

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
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.SomeMethod") && AllErrors[0].Contains("AlternateSignatureAttribute") && AllErrors[0].Contains("same name"));
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
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.SomeMethod") && AllErrors[0].Contains("ScriptAliasAttribute") && AllErrors[0].Contains("must be static"));
		}

		[Test]
		public void InlineCodeAttributeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1<T1> {
	class C2<T2> {
		[InlineCode(""Some.[].Strange{ }'thing' {T1} {T2} {T3} {T4} {x} {y} {this}"")]
		public void SomeMethod<T3, T4>(int x, string y) {}
	}
}");

			var impl = FindMethod("C1`1+C2`1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
			Assert.That(impl.LiteralCode, Is.EqualTo("Some.[].Strange{ }'thing' {T1} {T2} {T3} {T4} {x} {y} {this}"));
		}

		[Test]
		public void InlineCodeAttributeWithUnknownArgumentsIsAnError() {
			Prepare(@"using System.Runtime.CompilerServices; class C1 { [InlineCode(""{this}"")] public static void SomeMethod() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.SomeMethod") && AllErrors[0].Contains("inline code") && AllErrors[0].Contains("{this}"));

			Prepare(@"using System.Runtime.CompilerServices; class C1 { [InlineCode(""{x}"")] public void SomeMethod() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.SomeMethod") && AllErrors[0].Contains("inline code") && AllErrors[0].Contains("{x}"));
		}

		[Test]
		public void InlineCodeAttributeCannotBeSpecifiedOnInterfaceMethod() {
			Prepare(@"using System.Runtime.CompilerServices; public interface I1 { [InlineCode(""X"")] void M(); }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("I1.M") && AllErrors[0].Contains("InlineCodeAttribute") && AllErrors[0].Contains("interface method"));
		}

		[Test]
		public void InlineCodeAttributeCannotBeSpecifiedOnOverridableMethod() {
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [InlineCode(""X"")] public virtual void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.M") && AllErrors[0].Contains("InlineCodeAttribute") && AllErrors[0].Contains("overridable"));

			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [InlineCode(""X"")] public abstract void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.M") && AllErrors[0].Contains("InlineCodeAttribute") && AllErrors[0].Contains("overridable"));
		}

		[Test]
		public void InlineCodeAttributeCannotBeSpecifiedOnMethodImplementingInterfaceMember() {
			Prepare(@"using System.Runtime.CompilerServices; public interface I { void M(); } public class C : I { [InlineCode(""X"")] public void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C.M") && AllErrors[0].Contains("InlineCodeAttribute") && AllErrors[0].Contains("implements"));
		}

		[Test]
		public void InlineCodeAttributeCannotBeSpecifiedOnMethodThatOverridesABaseMember() {
			Prepare(@"using System.Runtime.CompilerServices; public class B { public virtual void M() {} } public class D : B { [InlineCode(""X"")] public sealed override void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("D.M") && AllErrors[0].Contains("InlineCodeAttribute") && AllErrors[0].Contains("overrides"));
		}

		[Test]
		public void InstanceMethodOnFirstArgumentAttributeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[InstanceMethodOnFirstArgument]
	[PreserveCase]
	public static void SomeMethod() {
	}

	[InstanceMethodOnFirstArgument]
	[ScriptName(""RenamedMethod"")]
	public static void SomeMethod2() {
	}

	[InstanceMethodOnFirstArgument]
	public static void SomeMethod3() {
	}

	[InstanceMethodOnFirstArgument]
	[PreserveName]
	public static void SomeMethod4() {
	}
}
");

			var impl = FindMethod("C1.SomeMethod");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InstanceMethodOnFirstArgument));
			Assert.That(impl.Name, Is.EqualTo("SomeMethod"));

			impl = FindMethod("C1.SomeMethod2");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InstanceMethodOnFirstArgument));
			Assert.That(impl.Name, Is.EqualTo("RenamedMethod"));

			impl = FindMethod("C1.SomeMethod3");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InstanceMethodOnFirstArgument));
			Assert.That(impl.Name, Is.EqualTo("someMethod3"));

			impl = FindMethod("C1.SomeMethod4");
			Assert.That(impl.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InstanceMethodOnFirstArgument));
			Assert.That(impl.Name, Is.EqualTo("someMethod4"));
		}

		[Test]
		public void InstanceMethodOnFirstArgumentAttributeCannotBeSpecifiedOnInstanceMember() {
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [InstanceMethodOnFirstArgument] public void M() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.M") && AllErrors[0].Contains("InstanceMethodOnFirstArgumentAttribute") && AllErrors[0].Contains("static"));
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

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("IgnoreGenericArgumentsAttribute") && AllErrors[0].Contains("D.SomeMethod") && AllErrors[0].Contains("overrides"));
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
		public void MethodsOtherThanInvokeCannotBeUsedOnDelegateTypes() {
			Prepare("public delegate void Del();");
			var del = AllTypes["Del"];

			Assert.That(del.Methods.Where(m => !m.IsConstructor).Select(m => new { m.Name, Impl = Metadata.GetMethodSemantics(m) }).All(m => m.Impl.Type == (m.Name == "Invoke" ? MethodScriptSemantics.ImplType.NormalMethod : MethodScriptSemantics.ImplType.NotUsableFromScript)));
		}
	}
}
