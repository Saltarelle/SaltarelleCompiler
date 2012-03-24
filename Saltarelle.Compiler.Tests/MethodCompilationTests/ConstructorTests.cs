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

        protected void Compile(string source, INamingConventionResolver namingConvention = null, IRuntimeLibrary runtimeLibrary = null, IErrorReporter errorReporter = null, bool useFirstConstructor = false) {
            Compile(new[] { source }, namingConvention, runtimeLibrary, errorReporter, (m, res, mc) => {
				if (m.IsConstructor && (m.Attributes.Any() || useFirstConstructor)) {
					Constructor = m;
					MethodCompiler = mc;
					CompiledConstructor = res;
				}
            });

			Assert.That(Constructor, Is.Not.Null, "No constructors with attributes were compiled.");
        }

		protected void AssertCorrect(string csharp, string expected, INamingConventionResolver namingConvention = null, bool useFirstConstructor = false) {
			Compile(csharp, namingConvention, useFirstConstructor: useFirstConstructor);
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
}", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => c.Parameters.Count == 0 ? ConstructorImplOptions.Named("ctor1") : ConstructorImplOptions.Unnamed() });
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

		[Test]
		public void ChainingToStaticMethodConstructorFromAnotherTypeOfConstructorIsAnError() {
			var rpt = new MockErrorReporter(false);
			Compile(new[] {
@"class C {
	public C() : this(0) {
	}
	public C(int x) {
	}
}" }, errorReporter: rpt, namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => c.Parameters.Count == 0 ? ConstructorImplOptions.Unnamed() : ConstructorImplOptions.StaticMethod("ctor") });
			Assert.That(rpt.AllMessages.Any(msg => msg.StartsWith("Error", StringComparison.InvariantCultureIgnoreCase) && msg.IndexOf("static method", StringComparison.InvariantCultureIgnoreCase) >= 0));
		}

		[Test]
		public void InvokingBaseStaticMethodConstructorFromAnotherTypeOfConstructorIsAnError() {
			var rpt = new MockErrorReporter(false);
			Compile(new[] {
@"class B {
	public B() {}
}
class D : B {
	public D() {}
}" }, errorReporter: rpt, namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => c.DeclaringType.Name == "D" ? ConstructorImplOptions.Unnamed() : ConstructorImplOptions.StaticMethod("ctor") });
			Assert.That(rpt.AllMessages.Any(msg => msg.StartsWith("Error", StringComparison.InvariantCultureIgnoreCase) && msg.IndexOf("static method", StringComparison.InvariantCultureIgnoreCase) >= 0));
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

		[Test]
		public void ChainingToConstructorMarkedAsNotUsableFromScriptIsAnError() {
			var rpt = new MockErrorReporter(false);
			Compile(new[] {
@"class C {
	public C() : this(0) {
	}
	public C(int x) {
	}
}" }, errorReporter: rpt, namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => c.Parameters.Count == 0 ? ConstructorImplOptions.Unnamed() : ConstructorImplOptions.NotUsableFromScript() });
			Assert.That(rpt.AllMessages.Any(msg => msg.StartsWith("Error", StringComparison.InvariantCultureIgnoreCase) && msg.IndexOf("cannot be used", StringComparison.InvariantCultureIgnoreCase) >= 0));
		}

		[Test]
		public void InvokingBaseConstructorMarkedAsNotUsableFromScriptIsAnError() {
			var rpt = new MockErrorReporter(false);
			Compile(new[] {
@"class B {
	public B() {}
}
class D : B {
	public D() {}
}" }, errorReporter: rpt, namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => c.DeclaringType.Name == "D" ? ConstructorImplOptions.Unnamed() : ConstructorImplOptions.NotUsableFromScript() });
			Assert.That(rpt.AllMessages.Any(msg => msg.StartsWith("Error", StringComparison.InvariantCultureIgnoreCase) && msg.IndexOf("cannot be used", StringComparison.InvariantCultureIgnoreCase) >= 0));
		}

		[Test]
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

		[Test]
		public void FieldsAreInitialized() {
			AssertCorrect(
@"class C {
	int f1 = 1;
	[System.Runtime.CompilerServices.CompilerGenerated]
	C() {
	}
	string f2 = ""X"";
}",
@"function() {
	this.$f1 = 1;
	this.$f2 = 'X';
}");
		}

		[Test]
		public void FieldsAreInitializedInImplicitlyDefinedConstructor() {
			AssertCorrect(
@"class C {
	int f1 = 1;
	string f2 = ""X"";
}",
@"function() {
	this.$f1 = 1;
	this.$f2 = 'X';
}", useFirstConstructor: true);
		}

		[Test]
		public void FieldsAreNotInitializedWhenChainingConstructors() {
			AssertCorrect(
@"class C {
	int x = 1;
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
		public void FieldInitializationWorksForStaticMethodConstructors() {
			AssertCorrect(
@"class C {
	int x = 1;
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() {
		M();
	}
}",
@"function() {
	var $this = {};
	$this.$x = 1;
	$this.M();
}", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.StaticMethod("ctor") });
		}

		[Test]
		public void FieldInitializationWorksForStaticMethodConstructorsWhenCallingBase() {
			AssertCorrect(
@"class B {
}
class D : B {
	int x = 1;
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public D() {
		this.M();
	}
}",
@"function() {
	var $this = {B}.ctor();
	$this.$x = 1;
	$this.M();
}", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.StaticMethod("ctor") });
		}

		[Test]
		public void FieldsAreInitializedBeforeCallingBaseWhenBaseCallIsImplicit() {
			AssertCorrect(
@"class B {
}
class D : B {
	int i = 1;
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public D() {
		this.M();
	}
}",
@"function() {
	this.$i = 1;
	{B}.call(this);
	this.M();
}");
		}

		[Test]
		public void FieldsAreInitializedBeforeCallingBaseWhenBaseCallIsExplicit() {
			AssertCorrect(
@"class B {
}
class D : B {
	int i = 1;
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public D() : base() {
		this.M();
	}
}",
@"function() {
	this.$i = 1;
	{B}.call(this);
	this.M();
}");
		}

		[Test]
		public void ImplicitlyDeclaredConstructorInvokesBaseWhenNotDerivedFromObject() {
			AssertCorrect(
@"class B {
}
class D : B {
}",
@"function() {
	{B}.call(this);
}", useFirstConstructor: true);
		}

		[Test]
		public void ImplicitlyDeclaredConstructorDoesNotInvokeBaseWhenDerivedFromObject() {
			AssertCorrect(
@"class C {
}",
@"function() {
}", useFirstConstructor: true);
		}

		[Test]
		public void ImplicitlyDeclaredConstructorInitializesFieldsBeforeInvokingBase() {
			AssertCorrect(
@"class B {
}
class D : B {
	int i = 1;
}",
@"function() {
	this.$i = 1;
	{B}.call(this);
}", useFirstConstructor: true);
		}

		[Test]
		public void FieldInitializersWithMultipleStatementsWork() {
			AssertCorrect(
@"class X {
	public int P { get; set; }
}
class C {
	X x = new X { P = 10 };
}",
@"function() {
	var $tmp1 = new {X}();
	$tmp1.set_P(10);
	this.$x = $tmp1;
}", useFirstConstructor: true);
		}

        [Test, Ignore("TODO")]
        public void InstanceFieldWithoutInitializerIsInitializedToDefault() {
            Assert.Fail("TODO");
        }
	}
}
