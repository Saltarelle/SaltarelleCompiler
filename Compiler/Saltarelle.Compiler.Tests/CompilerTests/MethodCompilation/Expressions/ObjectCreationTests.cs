using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class ObjectCreationTests : MethodCompilerTestBase {
		[Test]
		public void CanCallAnonymousConstructorWithNoArguments() {
			AssertCorrect(
@"class X {
}
public void M() {
	// BEGIN
	var c = new X();
	// END
}",
@"	var $c = new {inst_X}();
");
		}

		[Test]
		public void CanCallAnonymousConstructorWithArguments() {
			AssertCorrect(
@"class X {
	public X(int x, int y, int z) {}
}
public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	var t = new X(a, b, c);
	// END
}",
@"	var $t = new {inst_X}($a, $b, $c);
");
		}

		[Test]
		public void CanCallAnonymousConstructorWithReorderedAndDefaultArguments() {
			AssertCorrect(
@"class X {
	public X(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {}
}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }

public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	var x = new X(d: F1(), g: F2(), f: F3(), b: F4());
	// END
}",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	var $x = new {inst_X}(1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
");
		}

		[Test]
		public void CanCallNamedConstructorWithNoArguments() {
			AssertCorrect(
@"class X {
}
public void M() {
	// BEGIN
	var c = new X();
	// END
}",
@"	var $c = new {inst_X}.$ctor2();
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("$ctor2") });
		}

		[Test]
		public void CanCallNamedConstructorWithArguments() {
			AssertCorrect(
@"class X {
	public X(int x, int y, int z) {}
}
public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	var t = new X(a, b, c);
	// END
}",
@"	var $t = new {inst_X}.$ctor2($a, $b, $c);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("$ctor2") });
		}

		[Test]
		public void CanCallNamedConstructorWithReorderedAndDefaultArguments() {
			AssertCorrect(
@"class X {
	public X(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {}
}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }

public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	var x = new X(d: F1(), g: F2(), f: F3(), b: F4());
	// END
}",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	var $x = new {inst_X}.$ctor2(1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("$ctor2"), GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void CanCallStaticMethodConstructorWithNoArguments() {
			AssertCorrect(
@"class X {
}
public void M() {
	// BEGIN
	var c = new X();
	// END
}",
@"	var $c = {sm_X}.create_X();
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("create_" + c.DeclaringType.Name) });
		}

		[Test]
		public void CanCallStaticMethodConstructorWithArguments() {
			AssertCorrect(
@"class X {
	public X(int x, int y, int z) {}
}
public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	var t = new X(a, b, c);
	// END
}",
@"	var $t = {sm_X}.create_X($a, $b, $c);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("create_" + c.DeclaringType.Name) });
		}

		[Test]
		public void CanCallStaticMethodConstructorWithReorderedAndDefaultArguments() {
			AssertCorrect(
@"class X {
	public X(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {}
}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }

public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	var x = new X(d: F1(), g: F2(), f: F3(), b: F4());
	// END
}",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	var $x = {sm_X}.create_X(1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("create_" + c.DeclaringType.Name), GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void CanCallGlobalStaticMethodConstructor() {
			AssertCorrect(
@"class X {
	public X(int x, int y, int z) {}
}
public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	var t = new X(a, b, c);
	// END
}",
@"	var $t = create_X($a, $b, $c);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("create_" + c.DeclaringType.Name, isGlobal: true) });
		}


		[Test]
		public void InvokingConstructorImplementedAsInlineCodeWorks() {
			AssertCorrect(
@"class X {
	public X(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {}
}
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }

public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	var x = new X(d: F1(), g: F2(), f: F3(), b: F4());
	// END
}",
@"	var $tmp1 = this.$F1();
	var $tmp2 = this.$F2();
	var $tmp3 = this.$F3();
	var $x = __CreateX_1_this.$F4()_3_$tmp1_5_$tmp3_$tmp2__;
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.InlineCode("__CreateX_{a}_{b}_{c}_{d}_{e}_{f}_{g}__"), GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void UsingConstructorMarkedAsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { public Class() {} public void M() { var c = new Class(); } }" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("constructor")));
		}

		[Test]
		public void CanUseObjectInitializers() {
			AssertCorrect(
@"class X { public int x; public int P { get; set; } }
public void M() {
	int i = 0, j = 0;
	// BEGIN
	var x = new X { x = i, P = j };
	// END
}",
@"	var $tmp1 = new {inst_X}();
	$tmp1.$x = $i;
	$tmp1.set_$P($j);
	var $x = $tmp1;
");
		}

		[Test]
		public void CanUseCollectionInitializers1() {
			AssertCorrect(
@"class X : System.Collections.IEnumerable {
	public void Add(int a) {}
	public IEnumerator GetEnumerator() { return null; }
}
public void M() {
	int i = 0, j = 0;
	// BEGIN
	var x = new X { i, j };
	// END
}",
@"	var $tmp1 = new {inst_X}();
	$tmp1.$Add($i);
	$tmp1.$Add($j);
	var $x = $tmp1;
");
		}

		[Test]
		public void CanUseCollectionInitializers2() {
			AssertCorrect(
@"class X : System.Collections.IEnumerable {
	public void Add(int a, string b) {}
	public IEnumerator GetEnumerator() { return null; }
}
public void M() {
	int i = 0, j = 0;
	string s = null, t = null;
	// BEGIN
	var x = new X { { i, s }, { j, t } };
	// END
}",
@"	var $tmp1 = new {inst_X}();
	$tmp1.$Add($i, $s);
	$tmp1.$Add($j, $t);
	var $x = $tmp1;
");
		}

		[Test]
		public void ComplexObjectAndCollectionInitializersWork() {
			AssertCorrect(
@"using System;
using System.Collections.Generic;
class Point { public int X, Y; }
class Test {
	public Point Pos { get; set; }
	public List<string> List { get; set; }
	public Dictionary<string, int> Dict { get; set; }
	
	void M() {
		// BEGIN
		var x = new Test {
			Pos = { X = 1, Y = 2 },
			List = { ""Hello"", ""World"" },
			Dict = { { ""A"", 1 } }
		};
		// END
	}
}",
@"	var $tmp1 = new {inst_Test}();
	$tmp1.get_$Pos().$X = 1;
	$tmp1.get_$Pos().$Y = 2;
	$tmp1.get_$List().$Add('Hello');
	$tmp1.get_$List().$Add('World');
	$tmp1.get_$Dict().$Add('A', 1);
	var $x = $tmp1;
", addSkeleton: false);
		}

		[Test]
		public void NestingObjectAndCollectionInitializersWorks() {
			AssertCorrect(
@"using System;
using System.Collections.Generic;
class Color { public int R, G, B; }
class Point { public int X, Y; public Color Color; }
class Test {
	public Point Pos { get; set; }
	public List<string> List { get; set; }
	public Dictionary<string, int> Dict { get; set; }
	
	void M() {
		// BEGIN
		var x = new Test {
			Pos = new Point() { X = 1, Y = 2, Color = new Color { R = 4, G = 5, B = 6 } },
			List = new List<string> { ""Hello"", ""World"" },
			Dict = new Dictionary<string, int> { { ""A"", 1 } }
		};
		// END
	}
}",
@"	var $tmp1 = new {inst_Test}();
	var $tmp2 = new {inst_Point}();
	$tmp2.$X = 1;
	$tmp2.$Y = 2;
	var $tmp3 = new {inst_Color}();
	$tmp3.$R = 4;
	$tmp3.$G = 5;
	$tmp3.$B = 6;
	$tmp2.$Color = $tmp3;
	$tmp1.set_$Pos($tmp2);
	var $tmp4 = new (inst_$InstantiateGenericType({List}, {ga_String}))();
	$tmp4.$Add('Hello');
	$tmp4.$Add('World');
	$tmp1.set_$List($tmp4);
	var $tmp5 = new (inst_$InstantiateGenericType({Dictionary}, {ga_String}, {ga_Int32}))();
	$tmp5.$Add('A', 1);
	$tmp1.set_$Dict($tmp5);
	var $x = $tmp1;
", addSkeleton: false);
		}

		[Test]
		public void CreatingDelegateWorks1() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	var f = new Func<int>(() => 0);
	// END
}",
@"	var $f = function() {
		return 0;
	};
");
		}

		[Test]
		public void CreatingDelegateWorks2() {
			AssertCorrect(
@"int x;
public void M() {
	// BEGIN
	var f = new Func<int>(() => x);
	// END
}",
@"	var $f = $Bind(function() {
		return this.$x;
	}, this);
");
		}

		[Test]
		public void CannotUseNotUsableTypeAsATypeArgument() {
			var nc = new MockMetadataImporter { GetTypeSemantics = t => t.Name == "C1" ? TypeScriptSemantics.NotUsableFromScript() : TypeScriptSemantics.NormalType(t.Name) };
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {}
class C {
	public void M() {
		var c = new C1();
	}
}" }, metadataImporter: nc, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("not usable from script") && er.AllMessagesText[0].Contains("instance") && er.AllMessagesText[0].Contains("C1"));

			er = new MockErrorReporter(false);
			Compile(new[] {
@"class C1 {}
class C2<T> {}
class C {
	public void M() {
		var x = new C2<C2<C1>>();
	}
}" }, metadataImporter: nc, errorReporter: er);
			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("not usable from script") && er.AllMessagesText[0].Contains("type argument") && er.AllMessagesText[0].Contains("C1") && er.AllMessagesText[0].Contains("C2"));
		}

		[Test]
		public void InvokingParamArrayConstructorThatDoesNotExpandArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 { public C1(int x, int y, params int[] args) {} }
public void M() {
	// BEGIN
	var c = new C1(4, 8, 59, 12, 4);
	// END
}",
@"	var $c = new {inst_C1}(4, 8, [59, 12, 4]);
");
		}

		[Test]
		public void InvokingParamArrayConstructorThatDoesNotExpandArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"class C1 { public C1(int x, int y, params int[] args) {} }
public void M() {
	// BEGIN
	var c = new C1(4, 8, new[] { 59, 12, 4 });
	// END
}",
@"	var $c = new {inst_C1}(4, 8, [59, 12, 4]);
");
		}

		[Test]
		public void InvokingParamArrayConstructorThatExpandsArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 { public C1(int x, int y, params int[] args) {} }
public void M() {
	// BEGIN
	var c = new C1(4, 8, 59, 12, 4);
	// END
}",
@"	var $c = new {inst_C1}(4, 8, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(expandParams: true) });
		}

		[Test]
		public void InvokingParamArrayConstructorThatExpandsArgumentsInNonExpandedFormIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {
	public C1(int x, int y, params int[] args) {}
	public void M() {
	// BEGIN
	var c = new C1(4, 8, new[] { 59, 12, 4 });
	// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(expandParams: true) }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("C1") && er.AllMessagesText[0].Contains("constructor") && er.AllMessagesText[0].Contains("expanded form"));
		}

		[Test]
		public void CreatingObjectWithJsonConstructorWorks() {
			AssertCorrect(
@"class C1 { public int a; public string b; }
public int F() { return 0; }
public int P { get; set; }

public void M() {
	// BEGIN
	var c = new C1 { a = (P = F()), b = ""X"" };
	// END
}",
@"	var $tmp1 = this.F();
	this.set_P($tmp1);
	var $c = { $a: $tmp1, $b: 'X' };
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Json(new IMember[0]) });
		}

		[Test]
		public void JsonConstructorWithParameterToMemberMapWorks() {
			AssertCorrect(
@"class C1 { public C1(int a, int b) {} public int a2, int b2; }
public int F1() { return 0; }
public int F2() { return 0; }
public int P { get; set; }

public void M() {
	// BEGIN
	var c = new C1(F1(), P = F2());
	// END
}",
@"	var $tmp2 = this.F1();
	var $tmp1 = this.F2();
	this.set_P($tmp1);
	var $c = { $a2: $tmp2, $b2: $tmp1 };
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.DeclaringType.Name == "C1" ? ConstructorScriptSemantics.Json(new[] { c.DeclaringType.GetFields().Single(f => f.Name == "a2"), c.DeclaringType.GetFields().Single(f => f.Name == "b2") }) : ConstructorScriptSemantics.Unnamed() });
		}

		[Test]
		public void JsonConstructorWithParameterToMemberMapWorksWithReorderedAndDefaultArguments() {
			AssertCorrect(
@"class X {
	public int a2, b2, c2, d2, e2, f2, g2, h2;
	public X(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {}
}
int P { get; set; }
int F1() { return 0; }
int F2() { return 0; }
int F3() { return 0; }
int F4() { return 0; }

public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	var x = new X(d: F1(), g: (P = F2()), f: F3(), b: F4());
	// END
}",
@"	var $tmp2 = this.F1();
	var $tmp1 = this.F2();
	this.set_P($tmp1);
	var $x = { $d2: $tmp2, $g2: $tmp1, $f2: this.F3(), $b2: this.F4(), $a2: 1, $c2: 3, $e2: 5 };
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Json(c.Parameters.Select(p => c.DeclaringType.GetFields().Single(f => f.Name == p.Name + "2"))) });
		}

		[Test]
		public void JsonConstructorWithParameterToMemberMapWorksWithObjectInitializers() {
			AssertCorrect(
@"class X {
	public int a2, b2, c2, d2;
	public X(int a, int b) {}
}

public void M() {
	// BEGIN
	var x = new X(123, 456) { c2 = 789, d2 = 987 };
	// END
}",
@"	var $x = { $a2: 123, $b2: 456, $c2: 789, $d2: 987 };
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Json(c.Parameters.Select(p => c.DeclaringType.GetFields().Single(f => f.Name == p.Name + "2"))) });
		}

		[Test]
		public void MemberCorrespondingToOptionalNonSpecifiedArgumentToJsonConstructorCanBeInitialized() {
			AssertCorrect(
@"class X {
	public int a2, b2, c2, d2;
	public X(int a, int b = 0) {}
}

public void M() {
	// BEGIN
	var x = new X(123) { c2 = 789, b2 = 987 };
	// END
}",
@"	var $x = { $a2: 123, $c2: 789, $b2: 987 };
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Json(c.Parameters.Select(p => c.DeclaringType.GetFields().Single(f => f.Name == p.Name + "2"))) });
		}

		[Test]
		public void InitializingMemberThatIsAlsoInitializedWithParameterToMemberMapIsAnError() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class X {
	public int a2;
	public X(int a) {}
}
class C {
	public void M() {
		// BEGIN
		var x = new X(123) { a2 = 789 };
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Json(c.Parameters.Select(p => c.DeclaringType.GetFields().Single(f => f.Name == p.Name + "2"))) }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("a2") && er.AllMessagesText[0].Contains("initializer") && er.AllMessagesText[0].Contains("constructor call"));
		}

		[Test]
		public void CreatingANewDelegateFromAnOldOneWorks1() {
			AssertCorrect(
@"delegate void D1(int i);
delegate void D2(int i);

public void M() {
	D1 d1 = i => {};
	// BEGIN
	D2 d2 = new D2(d1);
	// END
}",
@"	var $d2 = $CloneDelegate($d1);
");
		}

		[Test]
		public void CreatingANewDelegateFromAnOldOneWorks2() {
			AssertCorrect(@"
using System;
using System.Runtime.CompilerServices;

public class C {
	delegate int D(int a);
	public void F(D f) {
	}
	int x;
	private void M() {
		object o = null;
		// BEGIN
		F(new D((Func<int, int>)(a => x)));
		// END
	}
}",
@"	this.$F($CloneDelegate($Bind(function($a) {
		return this.$x;
	}, this)));
", addSkeleton: false);
		}

		[Test]
		public void CreatingEnumGivesAZeroConstant() {
			AssertCorrect(
@"enum E {}

public void M() {
	// BEGIN
	var e = new E();
	// END
}",
@"	var $e = 0;
");
		}

		[Test]
		public void CreatingObjectWithDynamicArgumentWorksWhenAllCandidatesAreUnnamedConstructors() {
			AssertCorrect(
@"public class C1 {
	public C1(int x) {}
	public C1(string x) {}
}

public void M() {
	dynamic d = null;
	// BEGIN
	var c = new C1(d);
	// END
}",
@"	var $c = new {inst_C1}($d);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(generateCode: false) });
		}

		[Test]
		public void CreatingObjectWithDynamicArgumentWorksWhenAllCandidatesAreNamedConstructorsWithTheSameName() {
			AssertCorrect(
@"public class C1 {
	public C1(int x) {}
	public C1(string x) {}
}

public void M() {
	dynamic d = null;
	// BEGIN
	var c = new C1(d);
	// END
}",
@"	var $c = new {inst_C1}.X($d);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("X", generateCode: false) });
		}

		[Test]
		public void CreatingObjectWithDynamicArgumentWorksWhenAllCandidatesAreStaticMethodsWithTheSameName() {
			AssertCorrect(
@"public class C1 {
	public C1(int x) {}
	public C1(string x) {}
}

public void M() {
	dynamic d = null;
	// BEGIN
	var c = new C1(d);
	// END
}",
@"	var $c = {sm_C1}.X($d);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("X", generateCode: false) });
		}

		[Test]
		public void CreatingObjectWithDynamicArgumentAndInitializerStatementsWorks() {
			AssertCorrect(
@"public class C1 {
	public C1(int x) {}
	public C1(string x) {}
	public int P { get; set; }
}

public void M() {
	dynamic d = null;
	int i = 0;
	// BEGIN
	var c = new C1(d) { P = i; };
	// END
}",
@"	var $tmp1 = new {inst_C1}($d);
	$tmp1.set_P($i);
	var $c = $tmp1;
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(generateCode: false) });
		}

		[Test]
		public void UsingNamedArgumentWithDynamicConstructorInvocationIsAnError() {
			var er = new MockErrorReporter();
			Compile(new[] {
@"public class C1 {
	public C1(int x) {}
	public C1(string x) {}
}

public class C {
	public void M() {
		dynamic d = null;
		// BEGIN
		var c = new C1(x: d);
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(generateCode: false) }, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7526));
		}

		[Test]
		public void CreatingObjectWithDynamicArgumentGivesAnErrorWhenTheSemanticsDifferBetweenApplicableMethods() {
			var er = new MockErrorReporter();
			Compile(new[] {
@"public class C1 {
	public C1(int x) {}
	public C1(string x) {}
}

public class C {
	public void M() {
		dynamic d = null;
		// BEGIN
		var c = new C1(x: d);
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Count == 0 || c.Parameters[0].Type.Name == "Int32" ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Named("X") }, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7526));
		}

		[Test]
		public void CreatingObjectWithDynamicArgumentGivesAnErrorWhenNamesDifferBetweenApplicableMethods() {
			var er = new MockErrorReporter();
			Compile(new[] {
@"public class C1 {
	public C1(int x) {}
	public C1(string x) {}
}

public class C {
	public void M() {
		dynamic d = null;
		// BEGIN
		var c = new C1(d);
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Count > 0 ? ConstructorScriptSemantics.Named("C$" + c.Parameters[0].Type.Name) : ConstructorScriptSemantics.Unnamed() }, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7531));

			er = new MockErrorReporter();
			Compile(new[] {
@"public class C1 {
	public C1(int x) {}
	public C1(string x) {}
}

public class C {
	public void M() {
		dynamic d = null;
		// BEGIN
		var c = new C1(d);
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Count > 0 ? ConstructorScriptSemantics.StaticMethod("C$" + c.Parameters[0].Type.Name) : ConstructorScriptSemantics.Unnamed() }, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7531));
		}

		[Test]
		public void CreatingObjectWithDynamicArgumentGivesAnErrorWhenTheApplicableMethodsUseInlineCode() {
			var er = new MockErrorReporter();
			Compile(new[] {
@"public class C1 {
	public C1(int x) {}
	public C1(string x) {}
}

public class C {
	public void M() {
		dynamic d = null;
		// BEGIN
		var c = new C1(d);
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.InlineCode("X") }, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7531));
		}
	}
}
