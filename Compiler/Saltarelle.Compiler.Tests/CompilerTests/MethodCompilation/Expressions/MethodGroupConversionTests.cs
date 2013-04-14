using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class MethodGroupConversionTests : MethodCompilerTestBase {
		[Test]
		public void ReadingMethodGroupWithOneMethodWorks() {
			AssertCorrect(
@"void F(int x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F;
	// END
}",
@"	$f = $Bind(this.$F, this);
");
		}

		[Test]
		public void CombiningDeclarationAndAssignmentWorks() {
			AssertCorrect(
@"void F(int x) {}
public void M() {
	// BEGIN
	System.Action<int> f = F;
	// END
}",
@"	var $f = $Bind(this.$F, this);
");
		}

		[Test]
		public void ReadingStaticMethodGroupWorks() {
			AssertCorrect(
@"static void F(int x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F;
	// END
}",
@"	$f = {sm_C}.$F;
");
		}

		[Test]
		public void ReadingMethodGroupWithOverloadsWorks() {
			AssertCorrect(
@"void F(int x) {}
void F(string x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F;
	// END
}",
@"	$f = $Bind(this.F_Int32, this);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.Parameters.Count > 0 ? m.Name + "_" + m.Parameters[0].Type.Name : m.Name) });
		}

		[Test]
		public void ReadingMethodGroupWithAnotherTargetWorks() {
			AssertCorrect(
@"class X { public void F(int x) {} public void F(string x) {} }
public void M() {
	Action<int> f;
	var x = new X();
	// BEGIN
	f = x.F;
	// END
}
",
@"	$f = $Bind($x.F_Int32, $x);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.Parameters.Count > 0 ? m.Name + "_" + m.Parameters[0].Type.Name : m.Name) });
		}

		[Test]
		public void ReadingMethodGroupWithAnotherTargetOnlyInvokesTheTargetOnce() {
			AssertCorrect(
@"class X { public void F(int x) {} public void F(string x) {} }
X F2() { return null; }
public void M() {
	Action<int> f;
	var x = new X();
	// BEGIN
	f = F2().F;
	// END
}
",
@"	var $tmp1 = this.F2();
	$f = $Bind($tmp1.F_Int32, $tmp1);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod(m.Parameters.Count > 0 ? m.Name + "_" + m.Parameters[0].Type.Name : m.Name) });
		}

		[Test]
		public void MethodGroupConversionCanInstantiateGenericMethod() {
			AssertCorrect(
@"void F<T>(T x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F<int>;
	// END
}",
@"	$f = $Bind($InstantiateGenericMethod(this.$F, {ga_Int32}), this);
");
		}

		[Test]
		public void MethodGroupConversionCanInstantiateGenericMethodWhenTheGenericArgumentIsNotExplicitlySpecified() {
			AssertCorrect(
@"void F<T>(T x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F;
	// END
}",
@"	$f = $Bind($InstantiateGenericMethod(this.$F, {ga_Int32}), this);
");
		}

		[Test]
		public void MethodGroupConversionDoesNotInstantiateGenericMethodIfIgnoreGenericArgumentsIsSet() {
			AssertCorrect(
@"void F<T>(T x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F<int>;
	// END
}",
@"	$f = $Bind(this.$F, this);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, ignoreGenericArguments: true) });
		}


		[Test]
		public void MethodGroupConversionCanInstantiateGenericStaticMethod() {
			AssertCorrect(
@"static void F<T>(T x) {}
public void M() {
	System.Action<int> f;
	// BEGIN
	f = F<int>;
	// END
}",
@"	$f = $InstantiateGenericMethod({sm_C}.$F, {ga_Int32});
");
		}

		[Test]
		public void UsingAMethodMarkedAsNotUsableFromScriptGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableMethod() {} public void M() { System.Func<int> f; f = UnusableMethod; } }" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "UnusableMethod" ? MethodScriptSemantics.NotUsableFromScript() : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == MessageSeverity.Error && msg.FormattedMessage.Contains("Class.UnusableMethod")));
		}

		[Test]
		public void MethodGroupConversionForBaseVersionOfOverriddenMethodWorks() {
			AssertCorrect(
@"
class B {
	public virtual void F(int x, int y) {}
}
class D : B {
	public override void F(int x, int y) {}
	public void M() {
		System.Action<int, int> a;
		// BEGIN
		a = this.F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind(this.$F, this);
	$a = $BindBaseCall({bind_B}, '$F', [], this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionForMethodOverriddenFromGenericClassWorks() {
			AssertCorrect(
@"class B<T> {
	public virtual void F(T x, int y) {}
}
class D : B<string> {
	public override void F(string x, int y) {}
	public void M() {
		System.Action<string, int> a;
		// BEGIN
		a = this.F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind(this.$F, this);
	$a = $BindBaseCall(bind_$InstantiateGenericType({B}, {ga_String}), '$F', [], this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionForMethodOverriddenFromGenericClassWorks2() {
			AssertCorrect(
@"class B<T> {
	public virtual void F(T x, int y) {}
}
class D<T2> : B<T2> {
	public override void F(T2 x, int y) {}
	public void M() {
		System.Action<T2, int> a;
		// BEGIN
		a = this.F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind(this.$F, this);
	$a = $BindBaseCall(bind_$InstantiateGenericType({B}, $T2), '$F', [], this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionForGenericOverriddenMethodWorks() {
			AssertCorrect(
@"class B {
	public virtual void F<T>(T x, int y) {}
}
class D : B {
	public override void F<U>(U x, int y) {}
	public void M() {
		System.Action<int, int> a;
		// BEGIN
		a = this.F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind($InstantiateGenericMethod(this.$F, {ga_Int32}), this);
	$a = $BindBaseCall({bind_B}, '$F', [{ga_Int32}], this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionForGenericMethodOverriddenFromGenericClassWorks2() {
			AssertCorrect(
@"class B<T> {
	public virtual void F<U>(U x, int y) {}
}
class D<T2> : B<T2> {
	public override void F<S>(S x, int y) {}
	public void M() {
		System.Action<int, int> a;
		// BEGIN
		a = F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind($InstantiateGenericMethod(this.$F, {ga_Int32}), this);
	$a = $BindBaseCall(bind_$InstantiateGenericType({B}, $T2), '$F', [{ga_Int32}], this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionForBaseVersionOfMethodInheritedFromGrandParentWorks() {
			AssertCorrect(
@"class B {
	public virtual void F(int x, int y) {}
}
class D : B {
}
class D2 : D {
	public override void F(int x, int y) {}

	public void M() {
		System.Action<int, int> a;
		// BEGIN
		a = this.F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind(this.$F, this);
	$a = $BindBaseCall({bind_B}, '$F', [], this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionForBaseVersionOfMethodDefinedInGrandParentAndOverriddenInParentWorks() {
			AssertCorrect(
@"class B {
	public virtual void F(int x, int y) {}
}
class D : B {
	public virtual void F(int x, int y) {}
}
class D2 : D {
	public override void F(int x, int y) {}

	public void M() {
		System.Action<int, int> a;
		// BEGIN
		a = this.F;
		a = base.F;
		// END
	}
}
",
@"	$a = $Bind(this.$F, this);
	$a = $BindBaseCall({bind_D}, '$F', [], this);
", addSkeleton: false);
		}

		[Test]
		public void CannotPerformMethodGroupConversionOnNormalMethodThatExpandsParamsToDelegateThatDoesNot() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {
	public void F(int x, int y, params int[] args) {}
	public void M() {
		System.Action<int, int, int[]> a = F;
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name == "F") }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("C1.F") && er.AllMessages[0].FormattedMessage.Contains("System.Action") && er.AllMessages[0].FormattedMessage.Contains("expand") && er.AllMessages[0].FormattedMessage.Contains("param array"));
		}

		[Test]
		public void CanPerformMethodGroupConversionOnMethodThatExpandsParamsToDelegateThatAlsoDoes() {
			AssertCorrect(
@"public void F(int x, int y, params int[] args) {}
public void M() {
	System.Action<int, int, int[]> f;
	// BEGIN
	f = F;
	// END
}",
@"	$f = $Bind(this.$F, this);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: m.Name == "F"), GetDelegateSemantics = d => new DelegateScriptSemantics(expandParams: true) });
		}

		[Test]
		public void BindFirstParameterToThisWorks() {
			AssertCorrect(
@"private int i;
public int F(int _this, int b) { return 0; }
public void M() {
	// BEGIN
	Func<int, int, int> f = F;
	// END
}
",
@"	var $f = $BindFirstParameterToThis($Bind(this.F, this));
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(bindThisToFirstParameter: true) });
		}

		[Test]
		public void CanPerformMethodGroupConversionOnInlineCodeMethodWithReturnValue() {
			AssertCorrect(
@"private int i;
public int F<T>(int a, int b) { return 0; }
public void M() {
	// BEGIN
	Func<int, int, int> f = F<string>;
	// END
}
",
@"	var $f = function($tmp1, $tmp2) {
		return _($tmp1)._($tmp2)._({ga_String});
	};
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({a})._({b})._({T})") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void CanPerformMethodGroupConversionOnInlineCodeMethodWithoutReturnValue() {
			AssertCorrect(
@"private int i;
public void F<T>(int a, int b) {}
public void M() {
	// BEGIN
	Action<int, int> f = F<string>;
	// END
}
",
@"	var $f = function($tmp1, $tmp2) {
		_($tmp1)._($tmp2)._({ga_String});
	};
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({a})._({b})._({T})") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void CanPerformMethodGroupConversionOnInlineCodeMethodThatUsesThis() {
			AssertCorrect(
@"private int i;
public int F<T>(int a, int b) { return 0; }
public void M() {
	// BEGIN
	Func<int, int, int> f = F<string>;
	// END
}
",
@"	var $f = $Bind(function($tmp1, $tmp2) {
		return _(this)._($tmp1)._($tmp2)._({ga_String});
	}, this);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({this})._({a})._({b})._({T})") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void MethodGroupConversionOnInlineCodeMethodOnAnotherTargetWorks() {
			AssertCorrect(
@"private int i;
public int F<T>(int a, int b) { return 0; }
public void M() {
	C c = null;
	// BEGIN
	Func<int, int, int> f = c.F<string>;
	// END
}
",
@"	var $f = function($tmp1, $tmp2) {
		return _($c)._($tmp1)._($tmp2)._({ga_String});
	};
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({this})._({a})._({b})._({T})") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void CanPerformMethodGroupConversionOnInlineCodeMethodUsingNonVirtualCall() {
			AssertCorrect(
@"class B {
	public virtual int F<T>(int a, int b) { return 0; }
}
class C : B {
	public override int F<T>(int a, int b) { return 0; }
	public void M() {
		// BEGIN
		System.Func<int, int, int> f = base.F<string>;
		// END
	}
}
",
@"	var $f = $Bind(function($tmp1, $tmp2) {
		return _(this)._($tmp1)._($tmp2)._({ga_String});
	}, this);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("X", nonVirtualInvocationLiteralCode: "_({this})._({a})._({b})._({T})") : MethodScriptSemantics.NormalMethod(m.Name) }, addSkeleton: false);
		}

		[Test]
		public void CanPerformMethodGroupConversionOnInlineCodeMethodWhenDelegateTypeUsesBindThisToFirstParameter() {
			AssertCorrect(
@"public int F<T>(int _this, int b) { return 0; }
public void M() {
	// BEGIN
	Func<int, int, int> f = F<string>;
	// END
}
",
@"	var $f = $BindFirstParameterToThis($Bind(function($tmp1, $tmp2) {
		return _(this)._($tmp1)._($tmp2)._({ga_String});
	}, this));
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(bindThisToFirstParameter: true), GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({this})._({_this})._({b})._({T})") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void CanPerformMethodGroupConversionOnInlineCodeMethodWhenDelegateTypeExpandsParamArray() {
			AssertCorrect(
@"public delegate void D(int x, int y, params object[] z);
public void F<T>(int a, int b, object[] c) {}
public void M() {
	// BEGIN
	D f = F<string>;
	// END
}",
@"	var $f = $Bind(function($tmp1, $tmp2) {
		_(this)._($tmp1)._($tmp2)._(Array.prototype.slice.call(arguments, 2));
	}, this);
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(expandParams: true), GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({this})._({a})._({b})._({c})_({T})") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void CannotPerformMethodGroupConversionOnInlineCodeMethodThatIncludesAnExpandedParameter() {
			var er = new MockErrorReporter(false);
			Compile(new[] {
@"class C1 {
	public int F1(params object[] a) { return 0; }
	public void M() {
		System.Func<object[], int> f = F1;
	}
}
" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F1" ? MethodScriptSemantics.InlineCode("_({*a})") : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(msg => msg.Severity == MessageSeverity.Error && msg.Code == 7523 && msg.FormattedMessage.Contains("C1.F1") && msg.FormattedMessage.Contains("expanded param array")));
		}

		[Test]
		public void CannotPerformMethodGroupConversionOnInlineCodeMethodThatIncludesAParameterAsLiteralText() {
			var er = new MockErrorReporter(false);
			Compile(new[] {
@"class C1 {
	public int F1(string a) { return 0; }
	public void M() {
		System.Func<string, int> f = F1;
	}
}
" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F1" ? MethodScriptSemantics.InlineCode("_({@a})") : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(msg => msg.Severity == MessageSeverity.Error && msg.Code == 7523 && msg.FormattedMessage.Contains("C1.F1") && msg.FormattedMessage.Contains("literal string as code")));
		}

		[Test]
		public void CanPerformMethodGroupConversionOnStaticMethodWithThisAsFirstArgumentWithReturnValue() {
			AssertCorrect(
@"private int i;
public int F(int a, int b) { return 0; }
public void M() {
	// BEGIN
	Func<int, int, int> f = F;
	// END
}
",
@"	var $f = $Bind(function($tmp1, $tmp2) {
		return {sm_C}.$F(this, $tmp1, $tmp2);
	}, this);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$F") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void CanPerformMethodGroupConversionOnStaticMethodWithThisAsFirstArgumentWithoutReturnValue() {
			AssertCorrect(
@"public void F(int a, int b) { return 0; }
public void M() {
	// BEGIN
	Action<int, int> f = F;
	// END
}
",
@"	var $f = $Bind(function($tmp1, $tmp2) {
		{sm_C}.$F(this, $tmp1, $tmp2);
	}, this);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$F") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void CanPerformMethodGroupConversionOnStaticMethodWithThisAsFirstArgumentInGenericType() {
			AssertCorrect(
@"class C<T1, T2> {
	public int F(int a, int b) { return 0; }
	public void M() {
		// BEGIN
		System.Func<int, int, int> f = F;
		// END
	}
}
",
@"	var $f = $Bind(function($tmp1, $tmp2) {
		return sm_$InstantiateGenericType({C}, $T1, $T2).$F(this, $tmp1, $tmp2);
	}, this);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$F") : MethodScriptSemantics.NormalMethod(m.Name) }, addSkeleton: false);
		}
		
		[Test]
		public void MethodGroupConversionOnStaticMethodWithThisAsFirstArgumentOnAnotherTargetWorks() {
			AssertCorrect(
@"public void F(int a, int b) { return 0; }
public void M() {
	C c;
	// BEGIN
	Action<int, int> f = c.F;
	// END
}
",
@"	var $f = $Bind(function($tmp1, $tmp2) {
		{sm_C}.$F(this, $tmp1, $tmp2);
	}, $c);
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$F") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void CanPerformMethodGroupConversionOnStaticMethodWithThisAsFirstArgumentWhenDelegateTypeUsesBindThisToFirstParameter() {
			AssertCorrect(
@"public void F(int a, int b) { return 0; }
public void M() {
	// BEGIN
	Action<int, int> f = F;
	// END
}
",
@"	var $f = $BindFirstParameterToThis($Bind(function($tmp1, $tmp2) {
		{sm_C}.$F(this, $tmp1, $tmp2);
	}, this));
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(bindThisToFirstParameter: true), GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$F") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void CanPerformMethodGroupConversionOnStaticMethodWithThisAsFirstArgumentWhenBothTheMethodAndTheDelegateTypeExpandsParamArray() {
			AssertCorrect(
@"public void F(int a, int b) { return 0; }
public void M() {
	// BEGIN
	Action<int, int> f = F;
	// END
}
",
@"	var $f = $Bind(function() {
		{sm_C}.$F.apply(null, [this].concat(Array.prototype.slice.call(arguments)));
	}, this);
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(expandParams: true), GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$F", expandParams: true) : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void CannotPerformMethodGroupConversionOnStaticMethodWithThisAsFirstArgumentThatExpandsParamsToDelegateThatDoesNot() {
			var er = new MockErrorReporter(false);

			Compile(new[] {
@"class C1 {
	public void F(int x, int y, params int[] args) {}
	public void M() {
		System.Action<int, int, int[]> a = F;
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$F", expandParams: true) : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("C1.F") && er.AllMessages[0].FormattedMessage.Contains("System.Action") && er.AllMessages[0].FormattedMessage.Contains("expand") && er.AllMessages[0].FormattedMessage.Contains("param array"));
		}

		[Test]
		public void MethodGroupConversionOnExtensionMethodImplementedAsNormalMethod() {
			AssertCorrect(
@"using System;
static class Ext {
	public static void F(this string s, int a, params int[] b) {}
}
class C {
	public void M() {
		string s = null;
		// BEGIN
		Action<int, int[]> f = s.F;
		// END
	}
}
",
@"	var $f = function($tmp1, $tmp2) {
		{sm_Ext}.$F($s, $tmp1, $tmp2);
	};
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionOnExtensionMethodImplementedAsNormalMethod_WithReturnValue() {
			AssertCorrect(
@"using System;
static class Ext {
	public static int F(this string s, int a, params int[] b) { return 0; }
}
class C {
	public void M() {
		string s = null;
		// BEGIN
		Func<int, int[], int> f = s.F;
		// END
	}
}
",
@"	var $f = function($tmp1, $tmp2) {
		return {sm_Ext}.$F($s, $tmp1, $tmp2);
	};
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionOnExtensionMethodImplementedAsNormalMethod_ExpandParams() {
			AssertCorrect(
@"using System;
static class Ext {
	public static void F(this string s, int a, int b) {}
}
class C {
	public void M() {
		string s = null;
		// BEGIN
		Action<int, int> f = s.F;
		// END
	}
}
",
@"	var $f = function() {
		{sm_Ext}.$F.apply(null, [$s].concat(Array.prototype.slice.call(arguments)));
	};
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.NormalMethod("$" + m.Name, expandParams: true), GetDelegateSemantics = d => new DelegateScriptSemantics(expandParams: true) });
		}

		[Test]
		public void MethodGroupConversionOnExtensionMethodImplementedAsNormalMethodAppliedToThis() {
			AssertCorrect(
@"using System;
static class Ext {
	public static void F(this C c, int a, int b) {}
}
class C {
	public void M() {
		// BEGIN
		Action<int, int> f = this.F;
		// END
	}
}
",
@"	var $f = $Bind(function($tmp1, $tmp2) {
		{sm_Ext}.$F(this, $tmp1, $tmp2);
	}, this);
", addSkeleton: false);
		}

		[Test]
		public void MethodGroupConversionOnExtensionMethodImplementedAsInlineCode() {
			AssertCorrect(
@"using System;
static class Ext {
	public static void F(this string s, int a, int b) {}
}
class C {
	public void M() {
		string s = null;
		// BEGIN
		Action<int, int> f = s.F;
		// END
	}
}
",
@"	var $f = function($tmp1, $tmp2) {
		_($s)._($tmp1)._($tmp2);
	};
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({s})._({a})._({b})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void MethodGroupConversionOnExtensionMethodImplementedAsInlineCode_WithReturnValue() {
			AssertCorrect(
@"using System;
static class Ext {
	public static int F(this string s, int a, params int[] b) { return 0; }
}
class C {
	public void M() {
		string s = null;
		// BEGIN
		Func<int, int[], int> f = s.F;
		// END
	}
}
",
@"	var $f = function($tmp1, $tmp2) {
		return _($s)._($tmp1)._($tmp2);
	};
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({s})._({a})._({b})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
		}

		[Test]
		public void MethodGroupConversionOnExtensionMethodImplementedAsInlineCode_ExpandParams() {
			AssertCorrect(
@"using System;
static class Ext {
	public static void F(this string s, int a, int[] b) {}
}
class C {
	public void M() {
		string s = null;
		// BEGIN
		Action<int, int[]> f = s.F;
		// END
	}
}
",
@"	var $f = function($tmp1) {
		_($s)._($tmp1)._(Array.prototype.slice.call(arguments, 1));
	};
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({s})._({a})._({b})") : MethodScriptSemantics.NormalMethod("$" + m.Name), GetDelegateSemantics = d => new DelegateScriptSemantics(expandParams: true) });
		}

		[Test]
		public void MethodGroupConversionOnExtensionMethodImplementedAsInlineCodeAppliedToThis() {
			AssertCorrect(
@"using System;
static class Ext {
	public static void F(this C c, int a, int b) {}
}
class C {
	public void M() {
		// BEGIN
		Action<int, int> f = this.F;
		// END
	}
}
",
@"	var $f = $Bind(function($tmp1, $tmp2) {
		_(this)._($tmp1)._($tmp2);
	}, this);
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("_({c})._({a})._({b})") : MethodScriptSemantics.NormalMethod("$" + m.Name) });

		}
	}
}
