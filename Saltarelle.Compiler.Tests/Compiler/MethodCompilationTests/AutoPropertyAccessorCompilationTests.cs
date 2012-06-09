using System;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests {
	[TestFixture]
	public class AutoPropertyAccessorCompilationTests : CompilerTestBase {
		[Test]
		public void InstanceAutoPropertyAccessorsImplementedAsInstanceMethodsAreCorrectlyCompiled() {
            Compile(new[] { "using System; class C { public int MyProperty { get; set; } }" });

			var getter = FindInstanceMethod("C.get_MyProperty");
			var setter = FindInstanceMethod("C.set_MyProperty");

			Assert.That(OutputFormatter.Format(getter.Definition), Is.EqualTo(
@"function() {
	return this.$MyProperty;
}"));

			Assert.That(OutputFormatter.Format(setter.Definition), Is.EqualTo(
@"function($value) {
	this.$MyProperty = $value;
}"));

			Assert.That(OutputFormatter.Format(FindClass("C").UnnamedConstructor), Is.EqualTo(
@"function() {
	this.$MyProperty = 0;
}"));
		}

		[Test]
		public void InstanceAutoPropertyAccessorsImplementedAsStaticMethodsAreCorrectlyCompiled() {
            Compile(new[] { "using System; class C { public int MyProperty { get; set; } }" }, namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("get_" + p.Name), MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("set_" + p.Name)) });

			var getter = FindStaticMethod("C.get_MyProperty");
			var setter = FindStaticMethod("C.set_MyProperty");

			Assert.That(OutputFormatter.Format(getter.Definition), Is.EqualTo(
@"function($this) {
	return $this.$MyProperty;
}"));

			Assert.That(OutputFormatter.Format(setter.Definition), Is.EqualTo(
@"function($this, $value) {
	$this.$MyProperty = $value;
}"));

			Assert.That(OutputFormatter.Format(FindClass("C").UnnamedConstructor), Is.EqualTo(
@"function() {
	this.$MyProperty = 0;
}"));
		}

		[Test]
		public void StaticAutoPropertyAccessorsAreCorrectlyCompiled() {
            Compile(new[] { "using System; class C { public static int MyProperty { get; set; } }" });

			var getter = FindStaticMethod("C.get_MyProperty");
			var setter = FindStaticMethod("C.set_MyProperty");

			Assert.That(OutputFormatter.Format(getter.Definition, allowIntermediates: true), Is.EqualTo(
@"function() {
	return {C}.$MyProperty;
}"));

			Assert.That(OutputFormatter.Format(setter.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	{C}.$MyProperty = $value;
}"));

			var c = FindClass("C");
			Assert.That(c.StaticInitStatements, Has.Count.EqualTo(1));
			Assert.That(OutputFormatter.Format(c.StaticInitStatements[0], allowIntermediates: true), Is.EqualTo("{C}.$MyProperty = 0;" + Environment.NewLine));
		}

		[Test]
		public void StaticAutoPropertyAccessorsAreCorrectlyCompiledForGenericClasses() {
            Compile(new[] { "using System; class C<T> { public static int MyProperty { get; set; } }" });

			var getter = FindStaticMethod("C.get_MyProperty");
			var setter = FindStaticMethod("C.set_MyProperty");

			Assert.That(OutputFormatter.Format(getter.Definition, allowIntermediates: true), Is.EqualTo(
@"function() {
	return $InstantiateGenericType({C}, $T).$MyProperty;
}"));

			Assert.That(OutputFormatter.Format(setter.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	$InstantiateGenericType({C}, $T).$MyProperty = $value;
}"));

			var c = FindClass("C");
			Assert.That(c.StaticInitStatements, Has.Count.EqualTo(1));
			Assert.That(OutputFormatter.Format(c.StaticInitStatements[0], allowIntermediates: true), Is.EqualTo("{C}.$MyProperty = 0;" + Environment.NewLine));
		}
	}
}
