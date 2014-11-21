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

			AssertCorrect(SourceLocationsInserter.Process(getter.Definition),
@"function() {
	// @(1, 49) - (1, 52)
	return this.$MyProperty;
}");

			AssertCorrect(SourceLocationsInserter.Process(setter.Definition),
@"function($value) {
	// @(1, 54) - (1, 57)
	this.$MyProperty = $value;
}");

			AssertCorrect(FindClass("C").UnnamedConstructor,
@"function() {
	$Init(this, '$MyProperty', $Default({def_Int32}));
	{sm_Object}.call(this);
}");
		}

		[Test]
		public void InstanceAutoPropertyAccessorsImplementedAsStaticMethodsAreCorrectlyCompiled() {
			Compile(new[] { "using System; class C { public int MyProperty { get; set; } }" }, metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("get_" + p.Name), MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("set_" + p.Name)) });

			var getter = FindStaticMethod("C.get_MyProperty");
			var setter = FindStaticMethod("C.set_MyProperty");

			AssertCorrect(SourceLocationsInserter.Process(getter.Definition),
@"function($this) {
	// @(1, 49) - (1, 52)
	return $this.$MyProperty;
}");

			AssertCorrect(SourceLocationsInserter.Process(setter.Definition),
@"function($this, $value) {
	// @(1, 54) - (1, 57)
	$this.$MyProperty = $value;
}");

			AssertCorrect(FindClass("C").UnnamedConstructor,
@"function() {
	$Init(this, '$MyProperty', $Default({def_Int32}));
	{sm_Object}.call(this);
}");
		}

		[Test]
		public void StaticAutoPropertyAccessorsAreCorrectlyCompiled() {
			Compile(new[] { "using System; class C { public static int MyProperty { get; set; } }" });

			var getter = FindStaticMethod("C.get_MyProperty");
			var setter = FindStaticMethod("C.set_MyProperty");

			AssertCorrect(SourceLocationsInserter.Process(getter.Definition),
@"function() {
	// @(1, 56) - (1, 59)
	return {sm_C}.$MyProperty;
}");

			AssertCorrect(SourceLocationsInserter.Process(setter.Definition),
@"function($value) {
	// @(1, 61) - (1, 64)
	{sm_C}.$MyProperty = $value;
}");

			var c = FindClass("C");
			Assert.That(c.StaticInitStatements, Has.Count.EqualTo(1));
			AssertCorrect(c.StaticInitStatements[0], "$Init({sm_C}, '$MyProperty', $Default({def_Int32}));" + Environment.NewLine);
		}

		[Test]
		public void StaticAutoPropertyAccessorsAreCorrectlyCompiledForGenericClasses() {
			Compile(new[] { "using System; class C<T> { public static int MyProperty { get; set; } }" });

			var getter = FindStaticMethod("C.get_MyProperty");
			var setter = FindStaticMethod("C.set_MyProperty");

			AssertCorrect(SourceLocationsInserter.Process(getter.Definition),
@"function() {
	// @(1, 59) - (1, 62)
	return sm_$InstantiateGenericType({C}, $T).$MyProperty;
}");

			AssertCorrect(SourceLocationsInserter.Process(setter.Definition),
@"function($value) {
	// @(1, 64) - (1, 67)
	sm_$InstantiateGenericType({C}, $T).$MyProperty = $value;
}");

			var c = FindClass("C");
			Assert.That(c.StaticInitStatements, Has.Count.EqualTo(1));
			Assert.That(OutputFormatter.Format(c.StaticInitStatements[0], allowIntermediates: true), Is.EqualTo("$Init(sm_$InstantiateGenericType({C}, $T), '$MyProperty', $Default({def_Int32}));" + Environment.NewLine));
		}
	}
}
