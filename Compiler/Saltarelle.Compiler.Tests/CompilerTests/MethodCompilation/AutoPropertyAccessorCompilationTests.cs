using System;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	[TestFixture]
	public class AutoPropertyAccessorCompilationTests : CompilerTestBase {
		[Test]
		public void InstanceAutoPropertyAccessorsImplementedAsInstanceMethodsAreCorrectlyCompiled() {
            Compile(new[] { "using System; class C { public int MyProperty { get; set; } }" });

			var getter = FindInstanceMethod("C.get_MyProperty");
			var setter = FindInstanceMethod("C.set_MyProperty");

			AssertCorrect(getter.Definition,
@"function() {
	return this.$MyProperty;
}");

			AssertCorrect(setter.Definition,
@"function($value) {
	this.$MyProperty = $value;
}");

			AssertCorrect(FindClass("C").UnnamedConstructor,
@"function() {
	this.$MyProperty = 0;
}");
		}

		[Test]
		public void InstanceAutoPropertyAccessorsImplementedAsStaticMethodsAreCorrectlyCompiled() {
            Compile(new[] { "using System; class C { public int MyProperty { get; set; } }" }, metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("get_" + p.Name), MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("set_" + p.Name)) });

			var getter = FindStaticMethod("C.get_MyProperty");
			var setter = FindStaticMethod("C.set_MyProperty");

			AssertCorrect(getter.Definition,
@"function($this) {
	return $this.$MyProperty;
}");

			AssertCorrect(setter.Definition,
@"function($this, $value) {
	$this.$MyProperty = $value;
}");

			AssertCorrect(FindClass("C").UnnamedConstructor,
@"function() {
	this.$MyProperty = 0;
}");
		}

		[Test]
		public void StaticAutoPropertyAccessorsAreCorrectlyCompiled() {
            Compile(new[] { "using System; class C { public static int MyProperty { get; set; } }" });

			var getter = FindStaticMethod("C.get_MyProperty");
			var setter = FindStaticMethod("C.set_MyProperty");

			AssertCorrect(getter.Definition,
@"function() {
	return {C}.$MyProperty;
}");

			AssertCorrect(setter.Definition,
@"function($value) {
	{C}.$MyProperty = $value;
}");

			var c = FindClass("C");
			Assert.That(c.StaticInitStatements, Has.Count.EqualTo(1));
			AssertCorrect(c.StaticInitStatements[0], "{C}.$MyProperty = 0;" + Environment.NewLine);
		}

		[Test]
		public void StaticAutoPropertyAccessorsAreCorrectlyCompiledForGenericClasses() {
            Compile(new[] { "using System; class C<T> { public static int MyProperty { get; set; } }" });

			var getter = FindStaticMethod("C.get_MyProperty");
			var setter = FindStaticMethod("C.set_MyProperty");

			AssertCorrect(getter.Definition,
@"function() {
	return $InstantiateGenericType({C}, $T).$MyProperty;
}");

			AssertCorrect(setter.Definition,
@"function($value) {
	$InstantiateGenericType({C}, $T).$MyProperty = $value;
}");

			var c = FindClass("C");
			Assert.That(c.StaticInitStatements, Has.Count.EqualTo(1));
			Assert.That(OutputFormatter.Format(c.StaticInitStatements[0], allowIntermediates: true), Is.EqualTo("$InstantiateGenericType({C}, $T).$MyProperty = 0;" + Environment.NewLine));
		}
	}
}
