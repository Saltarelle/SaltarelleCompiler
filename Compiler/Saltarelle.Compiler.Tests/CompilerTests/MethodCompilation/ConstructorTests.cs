using System;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	public class ConstructorTests : CompilerTestBase {
		protected IMethodSymbol Constructor { get; private set; }
		protected MethodCompiler MethodCompiler { get; private set; }
		protected JsFunctionDefinitionExpression CompiledConstructor { get; private set; }

		protected void Compile(string source, IMetadataImporter metadataImporter = null, IRuntimeLibrary runtimeLibrary = null, IErrorReporter errorReporter = null, bool useFirstConstructor = false) {
			Compile(new[] { source }, metadataImporter: metadataImporter, runtimeLibrary: runtimeLibrary, errorReporter: errorReporter, methodCompiled: (m, res, mc) => {
				if (m.MethodKind == MethodKind.Constructor && (m.GetAttributes().Any() || useFirstConstructor)) {
					Constructor = m;
					MethodCompiler = mc;
					CompiledConstructor = res;
				}
			});

			Assert.That(Constructor, Is.Not.Null, "No constructors with attributes were compiled.");
		}

		protected void AssertCorrect(string csharp, string expected, IMetadataImporter metadataImporter = null, bool useFirstConstructor = false) {
			Compile(csharp, metadataImporter, useFirstConstructor: useFirstConstructor);
			string actual = OutputFormatter.Format(CompiledConstructor, allowIntermediates: true);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
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
	{sm_Object}.call(this);
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
	var $this = {sm_Object}.ctor();
	$this.M();
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor") });
		}

		[Test]
		public void SimpleStaticMethodConstructorWithImplicitBaseCallWorks() {
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
	var $this = {sm_B}.ctor();
	$this.M();
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor") });
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
	var $this = {sm_B}.ctor();
	$this.M();
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor") });
		}

		[Test]
		public void ReturningFromAStaticMethodConstructorReturnsTheCreatedObject() {
			AssertCorrect(
@"class C {
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() {
		if (false) {
			System.Func<int, int> a = i => i + 1;
			return;
		}
		this.M();
		return;
	}
}",
@"function() {
	var $this = {sm_Object}.ctor();
	if (false) {
		var $a = function($i) {
			return $i + 1;
		};
		return $this;
	}
	$this.M();
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor") });
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
	{sm_C}.call(this);
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
	var $tmp1 = {sm_C}.F1();
	var $tmp2 = {sm_C}.F2();
	var $tmp3 = {sm_C}.F3();
	{sm_C}.call(this, 1, {sm_C}.F4(), 3, $tmp1, 5, $tmp3, $tmp2);
	this.M();
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Named("ctor1") : ConstructorScriptSemantics.Unnamed() });
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
	{sm_C}.ctor$Int32.call(this, 0);
	this.M();
}");
		}

		[Test]
		public void ChainingToConstructorImplementedAsInlineCodeFromUnnamedConstructorWprks() {
			AssertCorrect(
@"class C {
	static int P { get; set; }
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() : this(P = 42, ""X"") {
		M();
	}

	public C(int x, string s) {
	}
}",
@"function() {
	{sm_C}.set_P(42);
	$ShallowCopy(_(42)._('X'), this);
	this.M();
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.InlineCode("_({x})._({s})") });
		}

		[Test]
		public void ChainingToConstructorImplementedAsJsonFromUnnamedConstructorWorks() {
			AssertCorrect(
@"class C {
	public int X;
	public string S;

	static int P { get; set; }
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() : this(P = 42, ""X"") {
		M();
	}

	public C(int x, string s) {
	}
}",
@"function() {
	{sm_C}.set_P(42);
	$ShallowCopy({ $X: 42, $S: 'X' }, this);
	this.M();
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Json(c.Parameters.Select(p => c.ContainingType.GetMembers().Where(m => m.Kind == SymbolKind.Field).Single(x => x.Name.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase)))) });
		}

		[Test]
		public void ChainingToConstructorImplementedAsInlineCodeFromStaticMethodConstructorWorks() {
			AssertCorrect(
@"class C {
	public void M() {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() : this(0, ""X"") {
		M();
	}

	public C(int x, string s) {
	}
}",
@"function() {
	var $this = __Literal_(0)._X__;
	$this.M();
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.StaticMethod("M") : ConstructorScriptSemantics.InlineCode("__Literal_({x})._{@s}__") });
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
	var $tmp1 = {sm_C}.F1();
	var $tmp2 = {sm_C}.F2();
	var $tmp3 = {sm_C}.F3();
	var $this = {sm_C}.ctor$7(1, {sm_C}.F4(), 3, $tmp1, 5, $tmp3, $tmp2);
	$this.M();
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor$" + c.Parameters.Length.ToString(CultureInfo.InvariantCulture)) });
		}

		[Test]
		public void ChainingToJsonConstructorFromStaticMethodConstructorWorks() {
			AssertCorrect(
@"class C1 {
	static int F1() { return 0; }
	static int F2() { return 0; }
	static int F3() { return 0; }
	static int F4() { return 0; }

	public int A, B, C, D, E, F, G;

	public void M() {}

	public C1(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {
	}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(d: F1(), g: F2(), f: F3(), b: F4()) {
		M();
	}
}",
@"function() {
	var $this = { $D: {sm_C1}.F1(), $G: {sm_C1}.F2(), $F: {sm_C1}.F3(), $B: {sm_C1}.F4(), $A: 1, $C: 3, $E: 5 };
	$this.M();
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.StaticMethod("X") : ConstructorScriptSemantics.Json(c.Parameters.Select(p => c.ContainingType.GetMembers().Where(m => m.Kind == SymbolKind.Field).Single(x => x.Name.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase)))) });
		}

		[Test]
		public void ChainingToStaticMethodConstructorFromAnotherTypeOfConstructorWorks() {
			AssertCorrect(
@"class C {
	public static int P { get; set; }
	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() : this(P = 0) {
	}
	public C(int x) {
	}
}",
@"function() {
	{sm_C}.set_P(0);
	$ShallowCopy({sm_C}.ctor(0), this);
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.StaticMethod("ctor") });
		}

		[Test]
		public void InvokingBaseStaticMethodConstructorFromAnotherTypeOfConstructorWorks() {
			AssertCorrect(
@"class B {
	public B(int x) {}
}
class D : B {
	public static int P { get; set; }
	[System.Runtime.CompilerServices.CompilerGenerated]
	public D() : base(P = 1) {}
}",
@"function() {
	{sm_D}.set_P(1);
	$ShallowCopy({sm_B}.ctor(1), this);
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.ContainingType.Name == "D" ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.StaticMethod("ctor") });
		}

		[Test, Category("Wait")]
		public void DynamicChainingIsAnError() {
			var er = new MockErrorReporter();
			Compile(new[] {
@"class B {
}
class C {
	public C(int x) {}
	public C(string x) {}

	public void M() {}

	private static dynamic x;

	[System.Runtime.CompilerServices.CompilerGenerated]
	public D() : this(x) {
		this.M();
	}
}" }, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7998 && (string)m.Args[0] == "dynamic constructor chaining"));
		}

		[Test]
		public void ConstructorWithoutExplicitBaseInvokerInvokesBaseClassDefaultConstructorIfNotMarkedAsSkipInInitializer() {
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
	{sm_B}.call(this);
	this.M();
}");
		}

		[Test]
		public void ConstructorWithoutExplicitBaseInvokerDoesNotInvokeBaseClassDefaultConstructorIfMarkedAsSkipInInitializer() {
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
	this.M();
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.ContainingType.Name == "B") });
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
}" }, errorReporter: rpt, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.NotUsableFromScript() });
			Assert.That(rpt.AllMessages.Any(msg => msg.Severity == MessageSeverity.Error && msg.FormattedMessage.IndexOf("cannot be used", StringComparison.InvariantCultureIgnoreCase) >= 0));
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
}" }, errorReporter: rpt, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.ContainingType.Name == "D" ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.NotUsableFromScript() });
			Assert.That(rpt.AllMessages.Any(msg => msg.Severity == MessageSeverity.Error && msg.FormattedMessage.IndexOf("cannot be used", StringComparison.InvariantCultureIgnoreCase) >= 0));
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
	var $tmp1 = {sm_C}.F1();
	var $tmp2 = {sm_C}.F2();
	var $tmp3 = {sm_C}.F3();
	var $this = new {sm_C}(1, {sm_C}.F4(), 3, $tmp1, 5, $tmp3, $tmp2);
	$this.M();
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.ContainingType.Name == "C" && c.Parameters.Length == 0 ? ConstructorScriptSemantics.StaticMethod("ctor") : ConstructorScriptSemantics.Unnamed() });
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
	{sm_B}.call(this);
	this.M();
}");
		}

		[Test]
		public void InvokingBaseConstructorMarkedAsSkipInInitializerDoesNothing() {
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
	this.M();
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.ContainingType.Name == "B") });
		}

		[Test]
		public void InvokingStaticMethodBaseConstructorMarkedAsSkipInInitializerCreatesAnEmptyObjectLiteral() {
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
	var $this = {};
	$this.M();
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("construct_" + c.ContainingType.Name, skipInInitializer: c.ContainingType.Name == "B") });
		}

		[Test, Category("Wait")]
		public void DynamicInvocationOfBaseConstructorIsAnError() {
			var er = new MockErrorReporter();
			Compile(new[] {
@"class B {
	public B(int x) {}
	public B(string x) {}
}
class D : B {
	public void M() {}

	private static dynamic x;

	[System.Runtime.CompilerServices.CompilerGenerated]
	public D() : base(x) {
		this.M();
	}
}" }, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7998 && (string)m.Args[0] == "dynamic invocation of base constructor"));
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
	var $tmp1 = {sm_D}.F1();
	var $tmp2 = {sm_D}.F2();
	var $tmp3 = {sm_D}.F3();
	{sm_B}.call(this, 1, {sm_D}.F4(), 3, $tmp1, 5, $tmp3, $tmp2);
	this.M();
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed() });
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
	$Init(this, '$f1', 1);
	$Init(this, '$f2', 'X');
	{sm_Object}.call(this);
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
	$Init(this, '$f1', 1);
	$Init(this, '$f2', 'X');
	{sm_Object}.call(this);
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
	{sm_C}.call(this);
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
	var $this = {sm_Object}.ctor();
	$Init($this, '$x', 1);
	$this.M();
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor") });
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
	var $this = {sm_B}.ctor();
	$Init($this, '$x', 1);
	$this.M();
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor") });
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
	$Init(this, '$i', 1);
	{sm_B}.call(this);
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
	$Init(this, '$i', 1);
	{sm_B}.call(this);
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
	{sm_B}.call(this);
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
	$Init(this, '$i', 1);
	{sm_B}.call(this);
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
	var $tmp1 = new {sm_X}();
	$tmp1.set_P(10);
	$Init(this, '$x', $tmp1);
	{sm_Object}.call(this);
}", useFirstConstructor: true);
		}

		[Test]
		public void InstanceFieldWithoutInitializerIsInitializedToDefault() {
			AssertCorrect(
@"class C {
	int i;
	int i2 = 1;
	string s;
	object o;
}",
@"function() {
	$Init(this, '$i', $Default({def_Int32}));
	$Init(this, '$i2', 1);
	$Init(this, '$s', null);
	$Init(this, '$o', null);
	{sm_Object}.call(this);
}", useFirstConstructor: true);
		}

		[Test]
		public void InstanceFieldInitializedToDefaultValueForTypeParamWorks() {
			AssertCorrect(
@"class C<T> {
	T t1, t2 = default(T);
}",
@"function() {
	$Init(this, '$t1', $Default($T));
	$Init(this, '$t2', $Default($T));
	{sm_Object}.call(this);
}", useFirstConstructor: true);
		}

		[Test]
		public void InstanceFieldInitializedToDefaultValueForReferenceTypeParamWorks() {
			AssertCorrect(
@"class C<T> where T : class {
	T t1, t2 = default(T);
}",
@"function() {
	$Init(this, '$t1', null);
	$Init(this, '$t2', null);
	{sm_Object}.call(this);
}", useFirstConstructor: true);
		}

		[Test]
		public void InitializingAnInstanceFieldToALambdaWorks() {
			AssertCorrect(
@"class C {
	System.Func<int, string> f1 = x => ""A"";
	System.Func<int, string> f2 = (int x) => ""B"";
	System.Func<int, string> f3 = delegate(int x) { return ""C""; };
}",
@"function() {
	$Init(this, '$f1', function($x) {
		return 'A';
	});
	$Init(this, '$f2', function($x) {
		return 'B';
	});
	$Init(this, '$f3', function($x) {
		return 'C';
	});
	{sm_Object}.call(this);
}", useFirstConstructor: true);
		}

		[Test]
		public void ChainingToUnnamedParamArrayConstructorThatDoesNotExpandArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, 59, 12, 4) {}
}",
@"function() {
	{sm_C1}.call(this, 4, 8, [59, 12, 4]);
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Named("x") : ConstructorScriptSemantics.Unnamed() });
		}

		[Test]
		public void ChainingToUnnamedParamArrayConstructorThatDoesNotExpandArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8,new[] { 59, 12, 4 }) {}
}",
@"function() {
	{sm_C1}.call(this, 4, 8, [59, 12, 4]);
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Named("x") : ConstructorScriptSemantics.Unnamed() });
		}

		[Test]
		public void ChainingToUnnamedParamArrayConstructorThatExpandsArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, 59, 12, 4) {}
}",
@"function() {
	{sm_C1}.call(this, 4, 8, 59, 12, 4);
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Named("x") : ConstructorScriptSemantics.Unnamed(expandParams: true) });
		}

		[Test]
		public void ChainingToUnnamedParamArrayConstructorThatExpandsArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"class C1 {
	static int[] args = new[] { 59, 12, 4 };
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, args) {}
}",
@"function() {
	{sm_C1}.apply(this, [4, 8].concat({sm_C1}.$args));
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Named("x") : ConstructorScriptSemantics.Unnamed(expandParams: true) });

			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, new[] { 59, 12, 4 }) {}
}",
@"function() {
	{sm_C1}.call(this, 4, 8, 59, 12, 4);
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Named("x") : ConstructorScriptSemantics.Unnamed(expandParams: true) });
		}

		[Test]
		public void ChainingToNamedParamArrayConstructorThatDoesNotExpandArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, 59, 12, 4) {}
}",
@"function() {
	{sm_C1}.x.call(this, 4, 8, [59, 12, 4]);
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Named("x") });
		}

		[Test]
		public void ChainingToNamedParamArrayConstructorThatDoesNotExpandArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8,new[] { 59, 12, 4 }) {}
}",
@"function() {
	{sm_C1}.x.call(this, 4, 8, [59, 12, 4]);
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Named("x") });
		}

		[Test]
		public void ChainingToNamedParamArrayConstructorThatExpandsArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, 59, 12, 4) {}
}",
@"function() {
	{sm_C1}.x.call(this, 4, 8, 59, 12, 4);
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Named("x", expandParams: true) });
		}

		[Test]
		public void ChainingToNamedParamArrayConstructorThatExpandsArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"class C1 {
	static int[] args = new[] { 59, 12, 4 };
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, args) {}
}",
@"function() {
	{sm_C1}.x.apply(this, [4, 8].concat({sm_C1}.$args));
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Named("x", expandParams: true) });

			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, new[] { 59, 12, 4 }) {}
}",
@"function() {
	{sm_C1}.x.call(this, 4, 8, 59, 12, 4);
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Named("x", expandParams: true) });
		}

		[Test]
		public void ChainingToStaticMethodParamArrayConstructorThatDoesNotExpandArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, 59, 12, 4) {}
}",
@"function() {
	var $this = {sm_C1}.ctor$3(4, 8, [59, 12, 4]);
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor$" + c.Parameters.Length.ToString(CultureInfo.InvariantCulture)) });
		}

		[Test]
		public void ChainingToStaticMethodParamArrayConstructorThatDoesNotExpandArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8,new[] { 59, 12, 4 }) {}
}",
@"function() {
	var $this = {sm_C1}.ctor$3(4, 8, [59, 12, 4]);
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor$" + c.Parameters.Length.ToString(CultureInfo.InvariantCulture)) });
		}

		[Test]
		public void ChainingToStaticMethodParamArrayConstructorThatExpandsArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, 59, 12, 4) {}
}",
@"function() {
	var $this = {sm_C1}.ctor$3(4, 8, 59, 12, 4);
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor$" + c.Parameters.Length.ToString(CultureInfo.InvariantCulture), expandParams: true) });
		}

		[Test]
		public void ChainingToStaticMethodParamArrayConstructorThatExpandsArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"class C1 {
	static int[] args = new[] { 59, 12, 4 };
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, args) {}
}",
@"function() {
	var $this = {sm_C1}.ctor$3.apply(null, [4, 8].concat({sm_C1}.$args));
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor$" + c.Parameters.Length.ToString(CultureInfo.InvariantCulture), expandParams: true) });

			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, new[] { 59, 12, 4 }) {}
}",
@"function() {
	var $this = {sm_C1}.ctor$3(4, 8, 59, 12, 4);
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor$" + c.Parameters.Length.ToString(CultureInfo.InvariantCulture), expandParams: true) });
		}

		[Test]
		public void ChainingToInlineCodeConstructorThatUsesExpandedParameterPlaceholderInNonExpandedFormIsAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] {
@"class C1 {
	public static int[] a = null;

	public C1(params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(a) {
	}
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 1 ? ConstructorScriptSemantics.InlineCode("_({*args})") : ConstructorScriptSemantics.Unnamed() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == MessageSeverity.Error && msg.FormattedMessage.Contains("constructor") && msg.FormattedMessage.Contains("C1") && msg.FormattedMessage.Contains("params parameter expanded")));
		}

		[Test]
		public void ChainingToInlineCodeConstructorInNonExpandedFormUsesNonExpandedPattern() {
			AssertCorrect(
@"class C {
	public static int[] a = null;
	public C(params int[] args) {}
	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() : this(a) {
	}
}",
@"function() {
	$ShallowCopy(_2({sm_C}.$a), this);
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 1 ? ConstructorScriptSemantics.InlineCode("_({*args})", nonExpandedFormLiteralCode: "_2({args})") : ConstructorScriptSemantics.Unnamed() });
		}
	}
}
