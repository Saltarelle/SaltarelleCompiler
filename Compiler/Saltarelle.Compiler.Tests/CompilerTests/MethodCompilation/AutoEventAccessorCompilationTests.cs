using System;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	[TestFixture]
	public class AutoEventAccessorCompilationTests : CompilerTestBase {
		[Test]
		public void InstanceAutoEventAccessorsImplementedAsInstanceMethodsAreCorrectlyCompiled() {
			Compile(new[] { "using System; class C { public event System.EventHandler MyEvent; }" });

			var adder   = FindInstanceMethod("C.add_MyEvent");
			var remover = FindInstanceMethod("C.remove_MyEvent");

			AssertCorrect(adder.Definition,
@"function($value) {
	this.$MyEvent = {sm_Delegate}.Combine(this.$MyEvent, $value);
}");

			AssertCorrect(remover.Definition,
@"function($value) {
	this.$MyEvent = {sm_Delegate}.Remove(this.$MyEvent, $value);
}");

			AssertCorrect(FindClass("C").UnnamedConstructor,
@"function() {
	$Init(this, '$MyEvent', $Default({def_EventHandler}));
	{sm_Object}.call(this);
}");
		}

		[Test]
		public void InstanceAutoEventAccessorsImplementedAsStaticMethodsAreCorrectlyCompiled() {
			Compile(new[] { "using System; class C { public event System.EventHandler MyEvent; }" }, metadataImporter: new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("add_" + e.Name), MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("remove_" + e.Name)) });

			var adder   = FindStaticMethod("C.add_MyEvent");
			var remover = FindStaticMethod("C.remove_MyEvent");

			AssertCorrect(adder.Definition,
@"function($this, $value) {
	$this.$MyEvent = {sm_Delegate}.Combine($this.$MyEvent, $value);
}");

			AssertCorrect(remover.Definition,
@"function($this, $value) {
	$this.$MyEvent = {sm_Delegate}.Remove($this.$MyEvent, $value);
}");

			AssertCorrect(FindClass("C").UnnamedConstructor,
@"function() {
	$Init(this, '$MyEvent', $Default({def_EventHandler}));
	{sm_Object}.call(this);
}");
		}

		[Test]
		public void StaticAutoEventAccessorsAreCorrectlyCompiled() {
			Compile(new[] { "using System; class C { public static event System.EventHandler MyEvent; }" });

			var adder   = FindStaticMethod("C.add_MyEvent");
			var remover = FindStaticMethod("C.remove_MyEvent");

			AssertCorrect(adder.Definition,
@"function($value) {
	{sm_C}.$MyEvent = {sm_Delegate}.Combine({sm_C}.$MyEvent, $value);
}");

			AssertCorrect(remover.Definition,
@"function($value) {
	{sm_C}.$MyEvent = {sm_Delegate}.Remove({sm_C}.$MyEvent, $value);
}");

			var c = FindClass("C");
			Assert.That(c.StaticInitStatements, Has.Count.EqualTo(1));
			Assert.That(OutputFormatter.Format(c.StaticInitStatements[0], allowIntermediates: true), Is.EqualTo("$Init({sm_C}, '$MyEvent', $Default({def_EventHandler}));" + Environment.NewLine));
		}

		[Test]
		public void StaticAutoEventAccessorsAreCorrectlyCompiledForGenericClasses() {
			Compile(new[] { "using System; class C<T> { public static event System.EventHandler MyEvent; }" });

			var adder   = FindStaticMethod("C.add_MyEvent");
			var remover = FindStaticMethod("C.remove_MyEvent");

			AssertCorrect(adder.Definition,
@"function($value) {
	sm_$InstantiateGenericType({C}, $T).$MyEvent = {sm_Delegate}.Combine(sm_$InstantiateGenericType({C}, $T).$MyEvent, $value);
}");

			AssertCorrect(remover.Definition,
@"function($value) {
	sm_$InstantiateGenericType({C}, $T).$MyEvent = {sm_Delegate}.Remove(sm_$InstantiateGenericType({C}, $T).$MyEvent, $value);
}");

			var c = FindClass("C");
			Assert.That(c.StaticInitStatements, Has.Count.EqualTo(1));
			Assert.That(OutputFormatter.Format(c.StaticInitStatements[0], allowIntermediates: true), Is.EqualTo("$Init(sm_$InstantiateGenericType({C}, $T), '$MyEvent', $Default({def_EventHandler}));" + Environment.NewLine));
		}
	}
}
