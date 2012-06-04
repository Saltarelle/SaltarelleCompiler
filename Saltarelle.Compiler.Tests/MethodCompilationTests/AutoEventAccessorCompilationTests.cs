using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests {
	[TestFixture]
	public class AutoEventAccessorCompilationTests : CompilerTestBase {
		[Test]
		public void InstanceAutoEventAccessorsImplementedAsInstanceMethodsAreCorrectlyCompiled() {
            Compile(new[] { "using System; class C { public event System.EventHandler MyEvent; }" });

			var adder   = FindInstanceMethod("C.add_MyEvent");
			var remover = FindInstanceMethod("C.remove_MyEvent");

			Assert.That(OutputFormatter.Format(adder.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	this.$MyEvent = {Delegate}.Combine(this.$MyEvent, $value);
}"));

			Assert.That(OutputFormatter.Format(remover.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	this.$MyEvent = {Delegate}.Remove(this.$MyEvent, $value);
}"));

			Assert.That(OutputFormatter.Format(FindClass("C").UnnamedConstructor), Is.EqualTo(
@"function() {
	this.$MyEvent = null;
}"));
		}

		[Test]
		public void InstanceAutoEventAccessorsImplementedAsStaticMethodsAreCorrectlyCompiled() {
            Compile(new[] { "using System; class C { public event System.EventHandler MyEvent; }" }, namingConvention: new MockNamingConventionResolver { GetEventImplementation = e => EventImplOptions.AddAndRemoveMethods(MethodImplOptions.StaticMethodWithThisAsFirstArgument("add_" + e.Name), MethodImplOptions.StaticMethodWithThisAsFirstArgument("remove_" + e.Name)) });

			var adder   = FindStaticMethod("C.add_MyEvent");
			var remover = FindStaticMethod("C.remove_MyEvent");

			Assert.That(OutputFormatter.Format(adder.Definition, allowIntermediates: true), Is.EqualTo(
@"function($this, $value) {
	$this.$MyEvent = {Delegate}.Combine($this.$MyEvent, $value);
}"));

			Assert.That(OutputFormatter.Format(remover.Definition, allowIntermediates: true), Is.EqualTo(
@"function($this, $value) {
	$this.$MyEvent = {Delegate}.Remove($this.$MyEvent, $value);
}"));

			Assert.That(OutputFormatter.Format(FindClass("C").UnnamedConstructor), Is.EqualTo(
@"function() {
	this.$MyEvent = null;
}"));
		}

		[Test]
		public void StaticAutoEventAccessorsAreCorrectlyCompiled() {
            Compile(new[] { "using System; class C { public static event System.EventHandler MyEvent; }" });

			var adder   = FindStaticMethod("C.add_MyEvent");
			var remover = FindStaticMethod("C.remove_MyEvent");

			Assert.That(OutputFormatter.Format(adder.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	{C}.$MyEvent = {Delegate}.Combine({C}.$MyEvent, $value);
}"));

			Assert.That(OutputFormatter.Format(remover.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	{C}.$MyEvent = {Delegate}.Remove({C}.$MyEvent, $value);
}"));

			var c = FindClass("C");
			Assert.That(c.StaticInitStatements, Has.Count.EqualTo(1));
			Assert.That(OutputFormatter.Format(c.StaticInitStatements[0], allowIntermediates: true), Is.EqualTo("{C}.$MyEvent = null;" + Environment.NewLine));
		}

		[Test]
		public void StaticAutoEventAccessorsAreCorrectlyCompiledForGenericClasses() {
            Compile(new[] { "using System; class C<T> { public static event System.EventHandler MyEvent; }" });

			var adder   = FindStaticMethod("C.add_MyEvent");
			var remover = FindStaticMethod("C.remove_MyEvent");

			Assert.That(OutputFormatter.Format(adder.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	$InstantiateGenericType({C}, $T).$MyEvent = {Delegate}.Combine($InstantiateGenericType({C}, $T).$MyEvent, $value);
}"));

			Assert.That(OutputFormatter.Format(remover.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	$InstantiateGenericType({C}, $T).$MyEvent = {Delegate}.Remove($InstantiateGenericType({C}, $T).$MyEvent, $value);
}"));

			var c = FindClass("C");
			Assert.That(c.StaticInitStatements, Has.Count.EqualTo(1));
			Assert.That(OutputFormatter.Format(c.StaticInitStatements[0], allowIntermediates: true), Is.EqualTo("{C}.$MyEvent = null;" + Environment.NewLine));
		}
	}
}
