using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests.MethodCompilationTests;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests.ExpressionTests {
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
@"	var $c = new {X}();
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
@"	var $t = new {X}($a, $b, $c);
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
	var $x = new {X}(1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
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
@"	var $c = new ({X}.$ctor2)();
", namingConvention: new MockNamingConventionResolver { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("$ctor2") });
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
@"	var $t = new ({X}.$ctor2)($a, $b, $c);
", namingConvention: new MockNamingConventionResolver { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("$ctor2") });
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
	var $x = new ({X}.$ctor2)(1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
", namingConvention: new MockNamingConventionResolver { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("$ctor2"), GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name) });
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
@"	var $c = {X}.create_X();
", namingConvention: new MockNamingConventionResolver { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("create_" + c.DeclaringType.Name) });
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
@"	var $t = {X}.create_X($a, $b, $c);
", namingConvention: new MockNamingConventionResolver { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("create_" + c.DeclaringType.Name) });
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
	var $x = {X}.create_X(1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
", namingConvention: new MockNamingConventionResolver { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("create_" + c.DeclaringType.Name), GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name) });
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
", namingConvention: new MockNamingConventionResolver { GetConstructorSemantics = c => ConstructorScriptSemantics.InlineCode("__CreateX_{a}_{b}_{c}_{d}_{e}_{f}_{g}__"), GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void UsingConstructorMarkedAsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { public Class() {} public void M() { var c = new Class(); } }" }, namingConvention: new MockNamingConventionResolver { GetConstructorSemantics = c => ConstructorScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(m => m.StartsWith("Error:") && m.Contains("constructor")));
		}

		[Test]
		public void CanUseObjectInitializers() {
			AssertCorrect(
@"class C { public int x; public int P { get; set; } }
public void M() {
	int i = 0, j = 0;
	// BEGIN
	var c = new C { x = i, P = j };
	// END
}",
@"	var $tmp1 = new {C}();
	$tmp1.$x = $i;
	$tmp1.set_$P($j);
	var $c = $tmp1;
");
		}

		[Test]
		public void CanUseCollectionInitializers1() {
			AssertCorrect(
@"class C : System.Collections.IEnumerable {
	public void Add(int a) {}
	public IEnumerator GetEnumerator() { return null; }
}
public void M() {
	int i = 0, j = 0;
	// BEGIN
	var c = new C { i, j };
	// END
}",
@"	var $tmp1 = new {C}();
	$tmp1.$Add($i);
	$tmp1.$Add($j);
	var $c = $tmp1;
");
		}

		[Test]
		public void CanUseCollectionInitializers2() {
			AssertCorrect(
@"class C : System.Collections.IEnumerable {
	public void Add(int a, string b) {}
	public IEnumerator GetEnumerator() { return null; }
}
public void M() {
	int i = 0, j = 0;
	string s = null, t = null;
	// BEGIN
	var c = new C { { i, s }, { j, t } };
	// END
}",
@"	var $tmp1 = new {C}();
	$tmp1.$Add($i, $s);
	$tmp1.$Add($j, $t);
	var $c = $tmp1;
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
@"	var $tmp1 = new {Test}();
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
@"	var $tmp1 = new {Test}();
	var $tmp2 = new {Point}();
	$tmp2.$X = 1;
	$tmp2.$Y = 2;
	var $tmp3 = new {Color}();
	$tmp3.$R = 4;
	$tmp3.$G = 5;
	$tmp3.$B = 6;
	$tmp2.$Color = $tmp3;
	$tmp1.set_$Pos($tmp2);
	var $tmp4 = new ($InstantiateGenericType({List}, {String}))();
	$tmp4.$Add('Hello');
	$tmp4.$Add('World');
	$tmp1.set_$List($tmp4);
	var $tmp5 = new ($InstantiateGenericType({Dictionary}, {String}, {Int32}))();
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
	}
}
