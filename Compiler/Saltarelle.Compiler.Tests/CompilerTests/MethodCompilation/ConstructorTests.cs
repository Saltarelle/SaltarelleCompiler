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
					CompiledConstructor = (JsFunctionDefinitionExpression)SourceLocationsInserter.Process(res);
				}
			});

			Assert.That(Constructor, Is.Not.Null, "No constructors with attributes were compiled.");
		}

		protected void AssertCorrect(string csharp, string expected, IMetadataImporter metadataImporter = null, bool useFirstConstructor = false) {
			Compile(csharp, metadataImporter, useFirstConstructor: useFirstConstructor);
			string actual = OutputFormatter.Format(CompiledConstructor, allowIntermediates: true);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
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
	// @(5, 13) - (5, 14)
	{sm_Object}.call(this);
	// @(6, 3) - (6, 7)
	this.M();
	// @(7, 2) - (7, 3)
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
	// @(5, 13) - (5, 14)
	var $this = {};
	// @(6, 3) - (6, 12)
	$this.M();
	// @(7, 2) - (7, 3)
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor", skipInInitializer: true) });
		}

		[Test]
		public void SimpleStaticMethodConstructorWithExplicitBaseCallWorks() {
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
	// @(7, 22) - (7, 23)
	var $this = {sm_B}.ctor();
	// @(8, 3) - (8, 12)
	$this.M();
	// @(9, 2) - (9, 3)
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
	public D() {
		this.M();
	}
}",
@"function() {
	// @(7, 13) - (7, 14)
	var $this = {sm_B}.ctor();
	// @(8, 3) - (8, 12)
	$this.M();
	// @(9, 2) - (9, 3)
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
	// @(5, 13) - (5, 14)
	var $this = {sm_Object}.ctor();
	// @(6, 3) - (6, 13)
	if (false) {
		// @(7, 4) - (7, 41)
		var $a = function($i) {
			// @(7, 35) - (7, 40)
			return $i + 1;
		};
		// @(8, 4) - (8, 11)
		return $this;
	}
	// @(10, 3) - (10, 12)
	$this.M();
	// @(11, 3) - (11, 10)
	return $this;
	// @(12, 2) - (12, 3)
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
	// @(8, 27) - (8, 28)
	{sm_C}.call(this);
	// @(9, 3) - (9, 7)
	this.M();
	// @(10, 2) - (10, 3)
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
	// @(13, 56) - (13, 57)
	var $tmp1 = {sm_C}.F1();
	var $tmp2 = {sm_C}.F2();
	var $tmp3 = {sm_C}.F3();
	{sm_C}.call(this, 1, {sm_C}.F4(), 3, $tmp1, 5, $tmp3, $tmp2);
	// @(14, 3) - (14, 7)
	this.M();
	// @(15, 2) - (15, 3)
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
	// @(5, 23) - (5, 24)
	{sm_C}.ctor$Int32.call(this, 0);
	// @(6, 3) - (6, 7)
	this.M();
	// @(7, 2) - (7, 3)
}");
		}

		[Test]
		public void ChainingToConstructorImplementedAsInlineCodeFromUnnamedConstructorWorks() {
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
	// @(6, 33) - (6, 34)
	{sm_C}.set_P(42);
	$ShallowCopy(_(42)._('X'), this);
	// @(7, 3) - (7, 7)
	this.M();
	// @(8, 2) - (8, 3)
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
	// @(9, 33) - (9, 34)
	{sm_C}.set_P(42);
	$ShallowCopy({ $X: 42, $S: 'X' }, this);
	// @(10, 3) - (10, 7)
	this.M();
	// @(11, 2) - (11, 3)
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
	// @(5, 28) - (5, 29)
	var $this = __Literal_(0)._X__;
	// @(6, 3) - (6, 7)
	$this.M();
	// @(7, 2) - (7, 3)
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
	// @(13, 56) - (13, 57)
	var $tmp1 = {sm_C}.F1();
	var $tmp2 = {sm_C}.F2();
	var $tmp3 = {sm_C}.F3();
	var $this = {sm_C}.ctor$7(1, {sm_C}.F4(), 3, $tmp1, 5, $tmp3, $tmp2);
	// @(14, 3) - (14, 7)
	$this.M();
	// @(15, 2) - (15, 3)
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
	// @(15, 57) - (15, 58)
	var $this = { $D: {sm_C1}.F1(), $G: {sm_C1}.F2(), $F: {sm_C1}.F3(), $B: {sm_C1}.F4(), $A: 1, $C: 3, $E: 5 };
	// @(16, 3) - (16, 7)
	$this.M();
	// @(17, 2) - (17, 3)
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
	// @(4, 27) - (4, 28)
	{sm_C}.set_P(0);
	$ShallowCopy({sm_C}.ctor(0), this);
	// @(5, 2) - (5, 3)
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
	// @(7, 27) - (7, 28)
	{sm_D}.set_P(1);
	$ShallowCopy({sm_B}.ctor(1), this);
	// @(7, 28) - (7, 29)
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.ContainingType.Name == "D" ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.StaticMethod("ctor") });
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
	// @(9, 13) - (9, 14)
	{sm_B}.call(this);
	// @(10, 3) - (10, 7)
	this.M();
	// @(11, 2) - (11, 3)
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
	// @(10, 3) - (10, 7)
	this.M();
	// @(11, 2) - (11, 3)
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
			Assert.That(rpt.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.FormattedMessage.IndexOf("cannot be used", StringComparison.InvariantCultureIgnoreCase) >= 0));
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
			Assert.That(rpt.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.FormattedMessage.IndexOf("cannot be used", StringComparison.InvariantCultureIgnoreCase) >= 0));
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
	// @(13, 56) - (13, 57)
	var $tmp1 = {sm_C}.F1();
	var $tmp2 = {sm_C}.F2();
	var $tmp3 = {sm_C}.F3();
	var $this = new {sm_C}(1, {sm_C}.F4(), 3, $tmp1, 5, $tmp3, $tmp2);
	// @(14, 3) - (14, 7)
	$this.M();
	// @(15, 2) - (15, 3)
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
	// @(7, 22) - (7, 23)
	{sm_B}.call(this);
	// @(8, 3) - (8, 12)
	this.M();
	// @(9, 2) - (9, 3)
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
	// @(8, 3) - (8, 12)
	this.M();
	// @(9, 2) - (9, 3)
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
	// @(7, 22) - (7, 23)
	var $this = {};
	// @(8, 3) - (8, 12)
	$this.M();
	// @(9, 2) - (9, 3)
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("construct_" + c.ContainingType.Name, skipInInitializer: c.ContainingType.Name == "B") });
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
	// @(15, 56) - (15, 57)
	var $tmp1 = {sm_D}.F1();
	var $tmp2 = {sm_D}.F2();
	var $tmp3 = {sm_D}.F3();
	{sm_B}.call(this, 1, {sm_D}.F4(), 3, $tmp1, 5, $tmp3, $tmp2);
	// @(16, 3) - (16, 7)
	this.M();
	// @(17, 2) - (17, 3)
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
	// @(4, 6) - (4, 7)
	$Init(this, '$f1', 1);
	$Init(this, '$f2', 'X');
	{sm_Object}.call(this);
	// @(5, 2) - (5, 3)
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
	// @(9, 27) - (9, 28)
	{sm_C}.call(this);
	// @(10, 3) - (10, 7)
	this.M();
	// @(11, 2) - (11, 3)
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
	// @(6, 13) - (6, 14)
	var $this = {sm_Object}.ctor();
	$Init($this, '$x', 1);
	// @(7, 3) - (7, 7)
	$this.M();
	// @(8, 2) - (8, 3)
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
	// @(8, 13) - (8, 14)
	var $this = {sm_B}.ctor();
	$Init($this, '$x', 1);
	// @(9, 3) - (9, 12)
	$this.M();
	// @(10, 2) - (10, 3)
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
	// @(8, 13) - (8, 14)
	$Init(this, '$i', 1);
	{sm_B}.call(this);
	// @(9, 3) - (9, 12)
	this.M();
	// @(10, 2) - (10, 3)
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
	// @(8, 22) - (8, 23)
	$Init(this, '$i', 1);
	{sm_B}.call(this);
	// @(9, 3) - (9, 12)
	this.M();
	// @(10, 2) - (10, 3)
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
		// @(2, 37) - (2, 40)
		return 'A';
	});
	$Init(this, '$f2', function($x) {
		// @(3, 43) - (3, 46)
		return 'B';
	});
	$Init(this, '$f3', function($x) {
		// @(4, 50) - (4, 61)
		return 'C';
		// @(4, 62) - (4, 63)
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
	// @(5, 38) - (5, 39)
	{sm_C1}.call(this, 4, 8, [59, 12, 4]);
	// @(5, 39) - (5, 40)
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
	// @(5, 47) - (5, 48)
	{sm_C1}.call(this, 4, 8, [59, 12, 4]);
	// @(5, 48) - (5, 49)
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
	// @(5, 38) - (5, 39)
	{sm_C1}.call(this, 4, 8, 59, 12, 4);
	// @(5, 39) - (5, 40)
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
	// @(6, 33) - (6, 34)
	{sm_C1}.apply(this, [4, 8].concat({sm_C1}.$args));
	// @(6, 34) - (6, 35)
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Named("x") : ConstructorScriptSemantics.Unnamed(expandParams: true) });

			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, new[] { 59, 12, 4 }) {}
}",
@"function() {
	// @(5, 48) - (5, 49)
	{sm_C1}.call(this, 4, 8, 59, 12, 4);
	// @(5, 49) - (5, 50)
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
	// @(5, 38) - (5, 39)
	{sm_C1}.x.call(this, 4, 8, [59, 12, 4]);
	// @(5, 39) - (5, 40)
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
	// @(5, 47) - (5, 48)
	{sm_C1}.x.call(this, 4, 8, [59, 12, 4]);
	// @(5, 48) - (5, 49)
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
	// @(5, 38) - (5, 39)
	{sm_C1}.x.call(this, 4, 8, 59, 12, 4);
	// @(5, 39) - (5, 40)
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
	// @(6, 33) - (6, 34)
	{sm_C1}.x.apply(this, [4, 8].concat({sm_C1}.$args));
	// @(6, 34) - (6, 35)
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Named("x", expandParams: true) });

			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, new[] { 59, 12, 4 }) {}
}",
@"function() {
	// @(5, 48) - (5, 49)
	{sm_C1}.x.call(this, 4, 8, 59, 12, 4);
	// @(5, 49) - (5, 50)
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
	// @(5, 38) - (5, 39)
	var $this = {sm_C1}.ctor$3(4, 8, [59, 12, 4]);
	// @(5, 39) - (5, 40)
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
	// @(5, 47) - (5, 48)
	var $this = {sm_C1}.ctor$3(4, 8, [59, 12, 4]);
	// @(5, 48) - (5, 49)
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
	// @(5, 38) - (5, 39)
	var $this = {sm_C1}.ctor$3(4, 8, 59, 12, 4);
	// @(5, 39) - (5, 40)
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
	// @(6, 33) - (6, 34)
	var $this = {sm_C1}.ctor$3.apply(null, [4, 8].concat({sm_C1}.$args));
	// @(6, 34) - (6, 35)
	return $this;
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("ctor$" + c.Parameters.Length.ToString(CultureInfo.InvariantCulture), expandParams: true) });

			AssertCorrect(
@"class C1 {
	public C1(int x, int y, params int[] args) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C1() : this(4, 8, new[] { 59, 12, 4 }) {}
}",
@"function() {
	// @(5, 48) - (5, 49)
	var $this = {sm_C1}.ctor$3(4, 8, 59, 12, 4);
	// @(5, 49) - (5, 50)
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
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.FormattedMessage.Contains("constructor") && msg.FormattedMessage.Contains("C1") && msg.FormattedMessage.Contains("params parameter expanded")));
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
	// @(5, 23) - (5, 24)
	$ShallowCopy(_2({sm_C}.$a), this);
	// @(6, 2) - (6, 3)
}", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 1 ? ConstructorScriptSemantics.InlineCode("_({*args})", nonExpandedFormLiteralCode: "_2({args})") : ConstructorScriptSemantics.Unnamed() });
		}

		[Test]
		public void CanCompileCallsToBaseConstructorWithOnlyDefaultArguments() {
			AssertCorrect(
@"class B {
	public B(int x = 42) {}
	public B(string s) {}
}
class C : B {
	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() {
	}
}
",
@"function() {
	// @(7, 13) - (7, 14)
	{sm_B}.ctor$Int32.call(this, 42);
	// @(8, 2) - (8, 3)
}");
		}

		[Test]
		public void ParameterlessBaseConstructorTakesPrecedenceOverOneWithDefaultArgumentsInImplicitInvocation() {
			AssertCorrect(
@"class B {
	public B() {}
	public B(int x = 0) {}
}
class C : B {
	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() {
	}
}",
@"function() {
	// @(7, 13) - (7, 14)
	{sm_B}.call(this);
	// @(8, 2) - (8, 3)
}");
		}

		[Test]
		public void CallerInformationWorksInConstructorChaining() {
			AssertCorrect(
@"using System.Runtime.CompilerServices;
class C {
	public C(int x, [CallerLineNumber] int p1 = 0, [CallerFilePath] string p2 = null, [CallerMemberName] string p3 = null) {}

	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() : this(42) {}
}",
@"function() {
	// @(6, 24) - (6, 25)
	{sm_C}.ctor$Int32$Int32$String$String.call(this, 42, 6, 'File0.cs', '.ctor');
	// @(6, 25) - (6, 26)
}");
		}

		[Test]
		public void CallerInformationWorksInBaseConstructorInvocation() {
			AssertCorrect(
@"using System.Runtime.CompilerServices;
class B {
	public B([CallerLineNumber] int p1 = 0, [CallerFilePath] string p2 = null, [CallerMemberName] string p3 = null) {}
}

class C : B {
	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() : base() {}
}",
@"function() {
	// @(8, 22) - (8, 23)
	{sm_B}.call(this, 8, 'File0.cs', '.ctor');
	// @(8, 23) - (8, 24)
}");
		}

		[Test]
		public void CallerInformationIsNotFilledInInImplicitBaseConstructorCall() {	// Believe it or not, this is what the spec says
			AssertCorrect(
@"using System.Runtime.CompilerServices;
class B {
	public B([CallerLineNumber] int p1 = 0, [CallerFilePath] string p2 = null, [CallerMemberName] string p3 = null) {}
}

class C : B {
	[System.Runtime.CompilerServices.CompilerGenerated]
	public C() {}
}",
@"function() {
	// @(8, 13) - (8, 14)
	{sm_B}.call(this, 0, null, null);
	// @(8, 14) - (8, 15)
}");
		}

		[Test]
		public void OmitUnspecifiedArgumentsFromWorksWhenChainingConstructors() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Named("otherCtor", omitUnspecifiedArgumentsFrom: 3) };

			AssertCorrect(
@"class C {
	public C(int a, int b, int c = -1, int d = -2, int f = -3, int g = -4) {}
	[System.Runtime.CompilerServices.CompilerGenerated] public C() : this(1, 2, 3, 4) {} }",
@"function() {
	// @(3, 84) - (3, 85)
	{sm_C}.otherCtor.call(this, 1, 2, 3, 4);
	// @(3, 85) - (3, 86)
}", metadataImporter: metadataImporter);

			AssertCorrect(
@"class C {
	public C(int a, int b, int c = -1, int d = -2, int f = -3, int g = -4) {}
	[System.Runtime.CompilerServices.CompilerGenerated] public C() : this(1, 2, 3) {}
}",
@"function() {
	// @(3, 81) - (3, 82)
	{sm_C}.otherCtor.call(this, 1, 2, 3);
	// @(3, 82) - (3, 83)
}", metadataImporter: metadataImporter);

			AssertCorrect(
@"class C {
	public C(int a, int b, int c = -1, int d = -2, int f = -3, int g = -4) {}
	[System.Runtime.CompilerServices.CompilerGenerated] public C() : this(1, 2) {}
}",
@"function() {
	// @(3, 78) - (3, 79)
	{sm_C}.otherCtor.call(this, 1, 2, -1);
	// @(3, 79) - (3, 80)
}", metadataImporter: metadataImporter);

			AssertCorrect(
@"class C {
	public C(int a, int b, int c = -1, int d = -2, int f = -3, int g = -4) {}
	[System.Runtime.CompilerServices.CompilerGenerated] public C() : this(2, 5, f: 4) {}
}",
@"function() {
	// @(3, 84) - (3, 85)
	{sm_C}.otherCtor.call(this, 2, 5, -1, -2, 4);
	// @(3, 85) - (3, 86)
}", metadataImporter: metadataImporter);
		}

		[Test]
		public void OmitUnspecifiedArgumentsFromWorksWhenInvokingBaseConstructor() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(omitUnspecifiedArgumentsFrom: 3) };

			AssertCorrect(
@"class B { public B(int a, int b, int c = -1, int d = -2, int f = -3, int g = -4) {} }
class C : B { [System.Runtime.CompilerServices.CompilerGenerated] public C() : base(1, 2, 3, 4) {} }",
@"function() {
	// @(2, 97) - (2, 98)
	{sm_B}.call(this, 1, 2, 3, 4);
	// @(2, 98) - (2, 99)
}", metadataImporter: metadataImporter);

			AssertCorrect(
@"class B { public B(int a, int b, int c = -1, int d = -2, int f = -3, int g = -4) {} }
class C : B { [System.Runtime.CompilerServices.CompilerGenerated] public C() : base(1, 2, 3) {} }",
@"function() {
	// @(2, 94) - (2, 95)
	{sm_B}.call(this, 1, 2, 3);
	// @(2, 95) - (2, 96)
}", metadataImporter: metadataImporter);

			AssertCorrect(
@"class B { public B(int a, int b, int c = -1, int d = -2, int f = -3, int g = -4) {} }
class C : B { [System.Runtime.CompilerServices.CompilerGenerated] public C() : base(1, 2) {} }",
@"function() {
	// @(2, 91) - (2, 92)
	{sm_B}.call(this, 1, 2, -1);
	// @(2, 92) - (2, 93)
}", metadataImporter: metadataImporter);

			AssertCorrect(
@"class B { public B(int a, int b, int c = -1, int d = -2, int f = -3, int g = -4) {} }
class C : B { [System.Runtime.CompilerServices.CompilerGenerated] public C() : base(2, 5, f: 4) {} }",
@"function() {
	// @(2, 97) - (2, 98)
	{sm_B}.call(this, 2, 5, -1, -2, 4);
	// @(2, 98) - (2, 99)
}", metadataImporter: metadataImporter);
		}

		[Test]
		public void OmitUnspecifiedArgumentsFromWorksWhenInvokingBaseConstructorImplicit() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(omitUnspecifiedArgumentsFrom: 3) };

			AssertCorrect(
@"class B {
	public B(int a = -1, int b = -2, int c = -3, int d = -4, int f = -5, int g = -6) {}
}
class C : B { [System.Runtime.CompilerServices.CompilerGenerated] public C() {} }",
@"function() {
	// @(4, 78) - (4, 79)
	{sm_B}.call(this, -1, -2, -3);
	// @(4, 79) - (4, 80)
}", metadataImporter: metadataImporter);
		}
	}
}
