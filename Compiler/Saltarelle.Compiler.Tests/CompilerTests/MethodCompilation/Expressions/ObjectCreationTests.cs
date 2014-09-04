using System.Linq;
using Microsoft.CodeAnalysis;
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
@"	var $c = new {sm_X}();
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
@"	var $t = new {sm_X}($a, $b, $c);
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
	var $x = new {sm_X}(1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
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
@"	var $c = new {sm_X}.$ctor2();
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
@"	var $t = new {sm_X}.$ctor2($a, $b, $c);
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
	var $x = new {sm_X}.$ctor2(1, this.$F4(), 3, $tmp1, 5, $tmp3, $tmp2);
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
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("create_" + c.ContainingType.Name) });
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
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("create_" + c.ContainingType.Name) });
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
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("create_" + c.ContainingType.Name), GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name) });
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
	var $x = __CreateX_(1)._(this.$F4())._(3)._($tmp1)._(5)._($tmp3)._($tmp2);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.InlineCode("__CreateX_({a})._({b})._({c})._({d})._({e})._({f})._({g})"), GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void InvokingConstructorImplementedAsInlineCodeWithExpandedParamArrayParameterInNonExpandedFormIsAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] {
@"class C1 {
	public C1(params int[] args) {}
	public void M() {
		var a = new[] { 1, 2, 3 };
		var c = new C1(a);
	}
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.InlineCode("_({*args})") }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.FormattedMessage.Contains("constructor") && msg.FormattedMessage.Contains("C1") && msg.FormattedMessage.Contains("params parameter expanded")));
		}

		[Test]
		public void InvokingConstructorImplementedAsInlineCodeInNonExpandedFormUsesTheNonExpandedCode() {
			AssertCorrect(
@"public C(params int[] args) {}
public void M() {
	int[] a = new[] { 1, 2, 3 };
	// BEGIN
	var c = new C(a);
	// END
}
",
@"	var $c = _2($a);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.InlineCode("_({*args})", nonExpandedFormLiteralCode: "_2({args})") });
		}

		[Test]
		public void InvokingConstructorImplementedAsInlineCodeWorksForGenericType() {
			AssertCorrect(
@"class X<T1, T2> {
	public X(int a, int b) {}
}
public void M() {
	// BEGIN
	var x = new X<string, int>(13, 42);
	// END
}",
@"	var $x = __CreateX_({ga_String})._({ga_Int32})._(13)._(42);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.InlineCode("__CreateX_({T1})._({T2})._({a})._({b})"), GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void UsingConstructorMarkedAsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { public Class() {} public void M() { var c = new Class(); } }" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.FormattedMessage.Contains("constructor")));
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
@"	var $tmp1 = new {sm_X}();
	$tmp1.$x = $i;
	$tmp1.set_$P($j);
	var $x = $tmp1;
");
		}

		[Test]
		public void CanUseObjectInitializersStruct() {
			AssertCorrect(
@"class X { public int x; public int P { get; set; } }
public void M() {
	int i = 0, j = 0;
	// BEGIN
	var x = new X { x = i, P = j };
	// END
}",
@"	var $tmp1 = new {sm_X}();
	$tmp1.$x = $Clone($i, {to_Int32});
	$tmp1.set_$P($Clone($j, {to_Int32}));
	var $x = $tmp1;
", mutableValueTypes: true);
		}

		[Test]
		public void CanUseCollectionInitializers1() {
			AssertCorrect(
@"class X : System.Collections.IEnumerable {
	public void Add(int a) {}
	public System.Collections.IEnumerator GetEnumerator() { return null; }
}
public void M() {
	int i = 0, j = 0;
	// BEGIN
	var x = new X { i, j };
	// END
}",
@"	var $tmp1 = new {sm_X}();
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
	public System.Collections.IEnumerator GetEnumerator() { return null; }
}
public void M() {
	int i = 0, j = 0;
	string s = null, t = null;
	// BEGIN
	var x = new X { { i, s }, { j, t } };
	// END
}",
@"	var $tmp1 = new {sm_X}();
	$tmp1.$Add($i, $s);
	$tmp1.$Add($j, $t);
	var $x = $tmp1;
");
		}

		[Test]
		public void CanUseCollectionInitializers2Struct() {
			AssertCorrect(
@"class X : System.Collections.IEnumerable {
	public void Add(int a, string b) {}
	public System.Collections.IEnumerator GetEnumerator() { return null; }
}
public void M() {
	int i = 0, j = 0;
	string s = null, t = null;
	// BEGIN
	var x = new X { { i, s }, { j, t } };
	// END
}",
@"	var $tmp1 = new {sm_X}();
	$tmp1.$Add($Clone($i, {to_Int32}), $s);
	$tmp1.$Add($Clone($j, {to_Int32}), $t);
	var $x = $tmp1;
", mutableValueTypes: true);
		}

		[Test, Category("Wait")]	// Roslyn bug
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
@"	var $tmp1 = new {sm_Test}();
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
@"	var $tmp1 = new {sm_Test}();
	var $tmp2 = new {sm_Point}();
	$tmp2.$X = 1;
	$tmp2.$Y = 2;
	var $tmp3 = new {sm_Color}();
	$tmp3.$R = 4;
	$tmp3.$G = 5;
	$tmp3.$B = 6;
	$tmp2.$Color = $tmp3;
	$tmp1.set_$Pos($tmp2);
	var $tmp4 = new (sm_$InstantiateGenericType({List}, {ga_String}))();
	$tmp4.$Add('Hello');
	$tmp4.$Add('World');
	$tmp1.set_$List($tmp4);
	var $tmp5 = new (sm_$InstantiateGenericType({Dictionary}, {ga_String}, {ga_Int32}))();
	$tmp5.$Add('A', 1);
	$tmp1.set_$Dict($tmp5);
	var $x = $tmp1;
", addSkeleton: false);
		}

		[Test, Category("Wait")]	// Roslyn bug, GetCollectionInitializerSymbolInfo returns null for nested initializers
		public void NestingObjectAndCollectionInitializersWorks2() {
			AssertCorrect(
@"using System;
using System.Collections.Generic;
class Color { public int R, G, B; }
class Point { public int X, Y; public Color Color; public List<string> List { get; set; } public Dictionary<string, int> Dict { get; set; } }
class Test {
	public Point Pos { get; set; }

	void M() {
		// BEGIN
		var x = new Test {
			Pos = {
				X = 1,
				Y = 2,
				Color = {
					R = 4,
					G = 5,
					B = 6
				},
				List = { ""Hello"", ""World"" },
				Dict = { { ""A"", 1 } }
			},
		};
		// END
	}
}",
@"	TODO
", addSkeleton: false);
		}

		[Test]
		public void UsingCollectionInitializerWithInlineCodeConstructorWorks() {
			AssertCorrect(
@"class X : System.Collections.IEnumerable {
	public void Add(int a) {}
	public System.Collections.IEnumerator GetEnumerator() { return null; }
}
public void M() {
	int i = 0, j = 0;
	// BEGIN
	var x = new X { i, j };
	// END
}",
@"	var $tmp1 = __X__;
	$tmp1.Add($i);
	$tmp1.Add($j);
	var $x = $tmp1;
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.InlineCode("__X__") });
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
		public void CreatingDelegateWorks3() {
			AssertCorrect(
@"private int F() { return 0; }
public void M() {
	// BEGIN
	var f = new Func<int>(F);
	// END
}",
@"	var $f = $Bind(this.$F, this);
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

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("not usable from script") && er.AllMessages[0].FormattedMessage.Contains("instance") && er.AllMessages[0].FormattedMessage.Contains("C1"));

			er = new MockErrorReporter(false);
			Compile(new[] {
@"class C1 {}
class C2<T> {}
class C {
	public void M() {
		var x = new C2<C2<C1>>();
	}
}" }, metadataImporter: nc, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("not usable from script") && er.AllMessages[0].FormattedMessage.Contains("type argument") && er.AllMessages[0].FormattedMessage.Contains("C1") && er.AllMessages[0].FormattedMessage.Contains("C2"));
		}

		[Test]
		public void CannotUseMutableValueTypeAsTypeArgument() {
			var md = new MockMetadataImporter { GetTypeSemantics = t => t.TypeKind == TypeKind.Struct ? TypeScriptSemantics.MutableValueType(t.Name) : TypeScriptSemantics.NormalType(t.Name) };
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"struct S1 {}
class C1<T> {}
class C {
	public void M() {
		var c = new C1<S1>();
	}
}" }, metadataImporter: md, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].Code == 7539 && er.AllMessages[0].FormattedMessage.Contains("mutable value type") && er.AllMessages[0].FormattedMessage.Contains("S1"));

			er = new MockErrorReporter(false);

			Compile(new[] {
@"struct S1 {}
class C1<T> {}
class C {
	public void M() {
		var c = new C1<C1<S1>>();
	}
}" }, metadataImporter: md, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].Code == 7539 && er.AllMessages[0].FormattedMessage.Contains("mutable value type") && er.AllMessages[0].FormattedMessage.Contains("S1"));
		}

		[Test]
		public void InvokingUnnamedParamArrayConstructorThatDoesNotExpandArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 { public C1(int x, int y, params int[] args) {} }
public void M() {
	// BEGIN
	var c = new C1(4, 8, 59, 12, 4);
	// END
}",
@"	var $c = new {sm_C1}(4, 8, [59, 12, 4]);
");
		}

		[Test]
		public void InvokingUnnamedParamArrayConstructorThatDoesNotExpandArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"class C1 { public C1(int x, int y, params int[] args) {} }
public void M() {
	// BEGIN
	var c = new C1(4, 8, new[] { 59, 12, 4 });
	// END
}",
@"	var $c = new {sm_C1}(4, 8, [59, 12, 4]);
");
		}

		[Test]
		public void InvokingUnnamedParamArrayConstructorThatExpandsArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 { public C1(int x, int y, params int[] args) {} }
public void M() {
	// BEGIN
	var c = new C1(4, 8, 59, 12, 4);
	// END
}",
@"	var $c = new {sm_C1}(4, 8, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(expandParams: true) });
		}

		[Test]
		public void InvokingUnnamedParamArrayConstructorThatExpandsArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"public C(int x, int y, params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	var c1 = new C(4, 8, args);
	var c2 = new C(4, 8, new[] { 59, 12, 4 });
	// END
}",
@"	var $c1 = $ApplyConstructor({sm_C}, [4, 8].concat($args));
	var $c2 = new {sm_C}(4, 8, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(expandParams: true) });

			AssertCorrect(
@"public C(int x, params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	var c = new C(4, args);
	// END
}",
@"	var $c = $ApplyConstructor({sm_C}, [4].concat($args));
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(expandParams: true) });

			AssertCorrect(
@"public C(params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	var c = new C(args);
	// END
}",
@"	var $c = $ApplyConstructor({sm_C}, $args);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(expandParams: true) });
		}

		[Test]
		public void InvokingNamedParamArrayConstructorThatDoesNotExpandArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 { public C1(int x, int y, params int[] args) {} }
public void M() {
	// BEGIN
	var c = new C1(4, 8, 59, 12, 4);
	// END
}",
@"	var $c = new {sm_C1}.X(4, 8, [59, 12, 4]);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("X") });
		}

		[Test]
		public void InvokingNamedParamArrayConstructorThatDoesNotExpandArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"class C1 { public C1(int x, int y, params int[] args) {} }
public void M() {
	// BEGIN
	var c = new C1(4, 8, new[] { 59, 12, 4 });
	// END
}",
@"	var $c = new {sm_C1}.X(4, 8, [59, 12, 4]);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("X") });
		}

		[Test]
		public void InvokingNamedParamArrayConstructorThatExpandsArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 { public C1(int x, int y, params int[] args) {} }
public void M() {
	// BEGIN
	var c = new C1(4, 8, 59, 12, 4);
	// END
}",
@"	var $c = new {sm_C1}.X(4, 8, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("X", expandParams: true) });
		}

		[Test]
		public void InvokingNamedParamArrayConstructorThatExpandsArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"public C(int x, int y, params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	var c1 = new C(4, 8, args);
	var c2 = new C(4, 8, new[] { 59, 12, 4 });
	// END
}",
@"	var $c1 = $ApplyConstructor({sm_C}.X, [4, 8].concat($args));
	var $c2 = new {sm_C}.X(4, 8, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("X", expandParams: true) });

			AssertCorrect(
@"public C(int x, params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	var c = new C(4, args);
	// END
}",
@"	var $c = $ApplyConstructor({sm_C}.X, [4].concat($args));
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("X", expandParams: true) });

			AssertCorrect(
@"public C(params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	var c = new C(args);
	// END
}",
@"	var $c = $ApplyConstructor({sm_C}.X, $args);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("X", expandParams: true) });
		}

		[Test]
		public void InvokingStaticMethodParamArrayConstructorThatDoesNotExpandArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 { public C1(int x, int y, params int[] args) {} }
public void M() {
	// BEGIN
	var c = new C1(4, 8, 59, 12, 4);
	// END
}",
@"	var $c = {sm_C1}.X(4, 8, [59, 12, 4]);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("X") });
		}

		[Test]
		public void InvokingStaticMethodParamArrayConstructorThatDoesNotExpandArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"class C1 { public C1(int x, int y, params int[] args) {} }
public void M() {
	// BEGIN
	var c = new C1(4, 8, new[] { 59, 12, 4 });
	// END
}",
@"	var $c = {sm_C1}.X(4, 8, [59, 12, 4]);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("X") });
		}

		[Test]
		public void InvokingStaticMethodParamArrayConstructorThatExpandsArgumentsInExpandedFormWorks() {
			AssertCorrect(
@"class C1 { public C1(int x, int y, params int[] args) {} }
public void M() {
	// BEGIN
	var c = new C1(4, 8, 59, 12, 4);
	// END
}",
@"	var $c = {sm_C1}.X(4, 8, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("X", expandParams: true) });
		}

		[Test]
		public void InvokingStaticMethodParamArrayConstructorThatExpandsArgumentsInNonExpandedFormWorks() {
			AssertCorrect(
@"public C(int x, int y, params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	var c1 = new C(4, 8, args);
	var c2 = new C(4, 8, new[] { 59, 12, 4 });
	// END
}",
@"	var $c1 = {sm_C}.X.apply(null, [4, 8].concat($args));
	var $c2 = {sm_C}.X(4, 8, 59, 12, 4);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("X", expandParams: true) });

			AssertCorrect(
@"public C(int x, params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	var c = new C(4, args);
	// END
}",
@"	var $c = {sm_C}.X.apply(null, [4].concat($args));
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("X", expandParams: true) });

			AssertCorrect(
@"public C(params int[] args) {}
public void M() {
	var args = new[] { 59, 12, 4 };
	// BEGIN
	var c = new C(args);
	// END
}",
@"	var $c = {sm_C}.X.apply(null, $args);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.StaticMethod("X", expandParams: true) });
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
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Json(new ISymbol[0]) });
		}

		[Test]
		public void JsonConstructorWithParameterToMemberMapWorks() {
			AssertCorrect(
@"class C1 { public C1(int a, int b) {} public int a2; int b2; }
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
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.ContainingType.Name == "C1" ? ConstructorScriptSemantics.Json(new[] { c.ContainingType.GetMembers().Where(m => m.Kind == SymbolKind.Field).Single(f => f.Name == "a2"), c.ContainingType.GetMembers().Where(m => m.Kind == SymbolKind.Field).Single(f => f.Name == "b2") }) : ConstructorScriptSemantics.Unnamed() });
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
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Json(c.Parameters.Select(p => c.ContainingType.GetMembers().Where(m => m.Kind == SymbolKind.Field).Single(f => f.Name == p.Name + "2"))) });
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
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Json(c.Parameters.Select(p => c.ContainingType.GetMembers().Where(m => m.Kind == SymbolKind.Field).Single(f => f.Name == p.Name + "2"))) });
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
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Json(c.Parameters.Select(p => c.ContainingType.GetMembers().Where(m => m.Kind == SymbolKind.Field).Single(f => f.Name == p.Name + "2"))) });
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
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Json(c.Parameters.Select(p => c.ContainingType.GetMembers().Where(m => m.Kind == SymbolKind.Field).Single(f => f.Name == p.Name + "2"))) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("a2") && er.AllMessages[0].FormattedMessage.Contains("initializer") && er.AllMessages[0].FormattedMessage.Contains("constructor call"));
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
	public delegate int D(int a);
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
		public void CannotCreateADelegateThatBindsThisToFirstParameterFromOneThatDoesNot() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"delegate void D1(int i, int j);
delegate void D2(int a, int b);

class C {
	public void M() {
		D1 d1 = null;
		D2 d2 = new D2(d1);
	}
}" }, metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(bindThisToFirstParameter: d.Name == "D1") }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(e => e.FormattedMessage.Contains("D1") && e.FormattedMessage.Contains("D2") && e.FormattedMessage.Contains("differ in whether the Javascript 'this'")));
		}

		[Test]
		public void CannotCreateADelegateThatExpandsParamsFromOneThatDoesNot() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"delegate void D1(int i, int j);
delegate void D2(int a, int b);

class C {
	public void M() {
		D1 d1 = null;
		D2 d2 = new D2(d1);
	}
}" }, metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(expandParams: d.Name == "D1") }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(e => e.FormattedMessage.Contains("D1") && e.FormattedMessage.Contains("D2") && e.FormattedMessage.Contains("differ in whether the param array")));
		}

		[Test, Category("Wait")]
		public void CreatingEnumDelegatesToTheRuntimeLibrary() {
			AssertCorrect(
@"enum E {}

public void M() {
	// BEGIN
	var e = new E();
	// END
}",
@"	TODO, this behavior should be changed
");
		}

		[Test, Category("Wait")]
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
@"	var $c = new {sm_C1}($d);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(generateCode: false) });
		}

		[Test, Category("Wait")]
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
@"	var $c = new {sm_C1}.X($d);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Named("X", generateCode: false) });
		}

		[Test, Category("Wait")]
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

		[Test, Category("Wait")]
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
	var c = new C1(d) { P = i };
	// END
}",
@"	var $tmp1 = new {sm_C1}($d);
	$tmp1.set_P($i);
	var $c = $tmp1;
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(generateCode: false) });
		}

		[Test, Category("Wait")]
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

		[Test, Category("Wait")]
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
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length == 0 || c.Parameters[0].Type.Name == "Int32" ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Named("X") }, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7526));
		}

		[Test, Category("Wait")]
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
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length > 0 ? ConstructorScriptSemantics.Named("C$" + c.Parameters[0].Type.Name) : ConstructorScriptSemantics.Unnamed() }, errorReporter: er);
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
}" }, metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.Parameters.Length > 0 ? ConstructorScriptSemantics.StaticMethod("C$" + c.Parameters[0].Type.Name) : ConstructorScriptSemantics.Unnamed() }, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7531));
		}

		[Test, Category("Wait")]
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

		[Test]
		public void CreatingAnInstanceOfATypeParameterWithADefaultConstructorConstraintInvokesGenericActivatorCreateInstance() {
			AssertCorrect(
@"public class C1 {
	public C1(int x) {}
	public C1(string x) {}
	public int P { get; set; }
}

public void M<TMyType>() where TMyType : new() {
	// BEGIN
	var c = new TMyType();
	// END
}",
@"	var $c = $InstantiateGenericMethod({sm_Activator}.$CreateInstance, $TMyType).call(null);
");
		}

		[Test]
		public void ObjectInitializerAssignedToByReferenceVariable() {
			AssertCorrect(
@"public int P1;
public void M(out C c) {
	// BEGIN
	c = new C { P1 = 123 };
	// END
}
",
@"	var $tmp1 = new {sm_C}();
	$tmp1.$P1 = 123;
	$c.$ = $tmp1;
");
		}

		[Test]
		public void ObjectInitializerAssignedToField() {
			AssertCorrect(
@"public C F() { return null; }
public C X;

public int P1;
public void M() {
	// BEGIN
	X = new C { P1 = 123 };
	F().X = new C { P1 = 123 };
	// END
}
",
@"	var $tmp1 = new {sm_C}();
	$tmp1.$P1 = 123;
	this.$X = $tmp1;
	var $tmp3 = this.$F();
	var $tmp2 = new {sm_C}();
	$tmp2.$P1 = 123;
	$tmp3.$X = $tmp2;
");
		}

		[Test]
		public void ObjectInitializerAssignedToProperty() {
			AssertCorrect(
@"public C F() { return null; }
public C X { get; set; }

public int P1;
public void M() {
	// BEGIN
	X = new C { P1 = 123 };
	F().X = new C { P1 = 123 };
	// END
}
",
@"	var $tmp1 = new {sm_C}();
	$tmp1.$P1 = 123;
	this.set_$X($tmp1);
	var $tmp3 = this.$F();
	var $tmp2 = new {sm_C}();
	$tmp2.$P1 = 123;
	$tmp3.set_$X($tmp2);
");
		}

		[Test]
		public void ObjectInitializersWithTypeParameters() {
			AssertCorrect(
@"public class X {
	public string A;
}
public void M<T>(string a) where T : X, new() {
	// BEGIN
	var x = new T { A = a };
	// END
}",
@"	var $tmp1 = $InstantiateGenericMethod({sm_Activator}.$CreateInstance, $T).call(null);
	$tmp1.$A = $a;
	var $x = $tmp1;
");
		}
	}
}
