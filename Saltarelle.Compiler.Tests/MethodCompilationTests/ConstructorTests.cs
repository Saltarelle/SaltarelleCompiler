using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests {
	public class ConstructorTests : CompilerTestBase {
        protected IMethod Constructor { get; private set; }
        protected MethodCompiler MethodCompiler { get; private set; }
        protected JsFunctionDefinitionExpression CompiledConstructor { get; private set; }

        protected void Compile(string source, INamingConventionResolver namingConvention = null, IRuntimeLibrary runtimeLibrary = null, IErrorReporter errorReporter = null) {
            Compile(new[] { source }, namingConvention, runtimeLibrary, errorReporter, (m, res, mc) => {
				if (m.IsConstructor && m.Attributes.Any()) {
					Constructor = m;
					MethodCompiler = mc;
					CompiledConstructor = res;
				}
            });

			Assert.That(Constructor, Is.Not.Null, "No constructors with attributes were compiled.");
        }

		protected void AssertCorrect(string csharp, string expected, INamingConventionResolver namingConvention = null) {
			Compile(csharp, namingConvention);
			string actual = OutputFormatter.Format(CompiledConstructor, allowIntermediates: true);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void SimpleUnnamedConstructorWorks() {
			AssertCorrect(
@"class C {
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() {
		M();
	}
}",
@"function() {
	this.M();
}");
		}

		[Test]
		public void SimpleStaticMethodConstructorWithoutBaseCallWorks() {
			AssertCorrect(
@"class C {
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() {
		this.M();
	}
}",
@"function() {
	var $this = {};
	$this.M();
}", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.StaticMethod("ctor") });
		}

		[Test]
		public void SimpleStaticMethodConstructorWithImplicitBaseCallWorks() {
			AssertCorrect(
@"class B {
}
class D : B {
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public D() : B() {
		this.M();
	}
}",
@"function() {
	var $this = {B}.ctor();
	$this.M();
}", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.StaticMethod("ctor") });
		}

		[Test]
		public void SimpleStaticMethodConstructorWithExplicitBaseCallWorks() {
			AssertCorrect(
@"class B {
}
class D : B {
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public D() {
		this.M();
	}
}",
@"function() {
	var $this = {B}.ctor();
	$this.M();
}", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.StaticMethod("ctor") });
		}

		[Test]
		public void ConstructorChainingToUnnamedConstructorWithoutArgumentsWorks() {
			AssertCorrect(
@"class C {
	public void M() {}

	public C() {
	}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C(int x) : this() {
		M();
	}
}",
@"function($x) {
	{C}.call(this);
	this.M();
}");
		}

		[Test]
		public void ConstructorChainingWithReorderedAndDefaultArgumentsWorks() {
			AssertCorrect(
@"class C {
	public void M() {}

	static int F1() { return 0; }
	static int F2() { return 0; }
	static int F3() { return 0; }
	static int F4() { return 0; }

	public C(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {
	}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() : this(d: F1(), g: F2(), f: F3(), b: F4()) {
		M();
	}
}",
@"function() {
	var $tmp1 = {C}.F1();
	var $tmp2 = {C}.F2();
	var $tmp3 = {C}.F3();
	{C}.call(this, 1, {C}.F4(), 3, $tmp1, 5, $tmp3, $tmp2);
	this.M();
}", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.Unnamed() });
		}

		[Test]
		public void ChainingToNamedConstructorWorks() {
			AssertCorrect(
@"class C {
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() : this(0) {
		M();
	}

	public C(int x) {
	}
}",
@"function() {
	{C}.ctor$Int32.call(this, 0);
	this.M();
}");
		}

		[Test, Ignore("TODO")]
		public void ChainingToConstructorImplementedAsInlineCodeWorks() {
			Assert.Fail("TODO");
		}

		[Test]
		public void ChainingToStaticMethodConstructorFromAnotherStaticMethodConstructorWorks() {
			AssertCorrect(
@"class C {
	static int F1() { return 0; }
	static int F2() { return 0; }
	static int F3() { return 0; }
	static int F4() { return 0; }

	public void M() {}

	public C(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {
	}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() : this(d: F1(), g: F2(), f: F3(), b: F4()) {
		M();
	}
}",
@"function() {
	var $tmp1 = {C}.F1();
	var $tmp2 = {C}.F2();
	var $tmp3 = {C}.F3();
	var $this = {C}.ctor$7(1, {C}.F4(), 3, $tmp1, 5, $tmp3, $tmp2);
	$this.M();
}", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.StaticMethod("ctor$" + c.Parameters.Count.ToString(CultureInfo.InvariantCulture)) });
		}

		[Test, Ignore("TODO")]
		public void ChainingToStaticMethodConstructorFromAnotherTypeOfConstructorIsAnError() {
			Assert.Fail("TODO");
		}

		[Test]
		public void ConstructorWithoutExplicitBaseInvokerInvokesBaseClassDefaultConstructorIfNotDerivingFromObject() {
			AssertCorrect(
@"class B {
	public B() {}
}

class D : B {
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public D() {
		M();
	}
}",
@"function() {
	{B}.call(this);
	this.M();
}");
		}

		[Test, Ignore("TODO")]
		public void ChainingToConstructorMarkedAsNotUsableFromScriptIsAnError() {
			Assert.Fail("TODO");
		}

		[Test, Ignore("TODO")]
		public void ChainingToAnonymousConstructorFromStaticMethodConstructorWorks() {
			AssertCorrect(
@"class C {
	static int F1() { return 0; }
	static int F2() { return 0; }
	static int F3() { return 0; }
	static int F4() { return 0; }

	public void M() {}

	public C(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {
	}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() : this(d: F1(), g: F2(), f: F3(), b: F4()) {
		M();
	}
}",
@"function() {
	var $tmp1 = {C}.F1();
	var $tmp2 = {C}.F2();
	var $tmp3 = {C}.F3();
	var $this = new {C}(1, {C}.F4(), 3, $tmp1, 5, $tmp3, $tmp2);
	$this.M();
}", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => c.Parameters.Count == 0 ? ConstructorImplOptions.StaticMethod("ctor") : ConstructorImplOptions.Unnamed() });
		}

		[Test]
		public void InvokingBaseConstructorWorks() {
			AssertCorrect(
@"class B {
}
class D : B {
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public D() : base() {
		this.M();
	}
}",
@"function() {
	{B}.call(this);
	this.M();
}");
		}

		[Test]
		public void InvokingBaseConstructorWithReorderedAndDefaultArgumentsWorks() {
			AssertCorrect(
@"class B {
	public B(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {
	}
}

class D : B {
	public void M() {}

	static int F1() { return 0; }
	static int F2() { return 0; }
	static int F3() { return 0; }
	static int F4() { return 0; }

	[System.Runtime.CompilerServices.CompilerGenerated]
	public D() : base(d: F1(), g: F2(), f: F3(), b: F4()) {
		M();
	}
}",
@"function() {
	var $tmp1 = {D}.F1();
	var $tmp2 = {D}.F2();
	var $tmp3 = {D}.F3();
	{B}.call(this, 1, {D}.F4(), 3, $tmp1, 5, $tmp3, $tmp2);
	this.M();
}", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.Unnamed() });
		}

		[Test, Ignore("TODO")]
		public void FieldsAreInitialized() {
			Assert.Fail("TODO");
		}

		[Test, Ignore("TODO")]
		public void FieldsAreNotInitializedWhenChainingConstructors() {
			Assert.Fail("TODO");
		}

		[Test, Ignore("TODO")]
		public void FieldInitializationWorksForStaticMethodConstructors() {
			Assert.Fail("TODO");
		}

		[Test, Ignore("TODO")]
		public void FieldsAreInitializedBeforeCallingBaseWhenBaseCallIsImplicit() {
			Assert.Fail("TODO");
		}

		[Test, Ignore("TODO")]
		public void FieldsAreInitializedBeforeCallingBaseWhenBaseCallIsExplicit() {
			Assert.Fail("TODO");
		}

		[Test, Ignore("TODO")]
		public void ImplicitlyDeclaredConstructorInvokesBaseWhenNotDerivedFromObject() {
		}

		[Test, Ignore("TODO")]
		public void ImplicitlyDeclaredConstructorDoesNotInvokeBaseWhenDerivedFromObject() {
		}

		[Test, Ignore("TODO")]
		public void ImplicitlyDeclaredConstructorInitializesFieldsBeforeInvokingBase() {
		}
	}
}
