using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
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
", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.Named("$ctor2") });
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
", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.Named("$ctor2") });
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
", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.Named("$ctor2"), GetMethodImplementation = m => MethodImplOptions.NormalMethod("$" + m.Name) });
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
", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.StaticMethod("create_" + c.DeclaringType.Name) });
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
", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.StaticMethod("create_" + c.DeclaringType.Name) });
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
", namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.StaticMethod("create_" + c.DeclaringType.Name), GetMethodImplementation = m => MethodImplOptions.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void UsingConstructorMarkedAsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { public Class() {} public void M() { var c = new Class(); } }" }, namingConvention: new MockNamingConventionResolver { GetConstructorImplementation = c => ConstructorImplOptions.NotUsableFromScript() }, errorReporter: er);
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

class Point { public int X, Y; }
class Test {
	private Point _pos = new Point();
	private List<string> _list = new List<string>();
	public Point Pos {
		get { Console.WriteLine("Get pos"); return _pos; }
		set { Console.WriteLine("Set pos"); _pos = value; }
	}

	public List<string> List { get { Console.WriteLine("Get List"); return _list; } set { Console.WriteLine("Set list"); _list = value; } }
	public Dictionary<string, int> _dict = new Dictionary<string, int>();
	public Dictionary<string, int> Dict {
		get { Console.WriteLine("Get dict"); return _dict; }
		set { Console.WriteLine("Set dict"); _dict = value; }
	}



	public void M() {
		var x = new Test {
			Pos = { X = 1, Y = 2 },
			List = { "Hello", "World" },
			Dict = { { "A", 1 } }
		};
	}
}
		[Test]
		public void ComplexObjectAndCollectionInitializersWork() {
			new Test().M();
			AssertCorrect(
@"using System;
using System.Collections.Generic;
struct Point { public int X, Y; }
class Test {
	public Point Pos;
	public List<string> List = new List<string>();
	public Dictionary<string, int> Dict = new Dictionary<string, int>();
	
	void M() {
		var x = new Test {
			Pos = { X = 1, Y = 2 },
			List = { ""Hello"", ""World"" },
			Dict = { { ""A"", 1 } }
		};
	}
}",
@"
", addSkeleton: false);
		}

		[Test]
		public void NestingObjectAndCollectionInitializersWorks() {
			Assert.Fail("TODO");
		}
	}
}
