using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests {
	[TestFixture]
	public class ManualAccessorCompilationTests : CompilerTestBase {
		[Test]
		public void InstanceManualPropertyAccessorsCanBeCompiled() {
            Compile(new[] { "using System; class C { public int myField; public int MyProperty { get { return myField; } set { myField = value; } }" });

			var getter = FindInstanceMethod("C.get_MyProperty");
			var setter = FindInstanceMethod("C.set_MyProperty");

			Assert.That(OutputFormatter.Format(getter.Definition, allowIntermediates: true), Is.EqualTo(
@"function() {
	return this.$myField;
}"));

			Assert.That(OutputFormatter.Format(setter.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	this.$myField = $value;
}"));
		}

		[Test]
		public void StaticManualPropertyAccessorsCanBeCompiled() {
            Compile(new[] { "using System; class C { public static int myField; public static int MyProperty { get { return myField; } set { myField = value; } }" });

			var getter = FindStaticMethod("C.get_MyProperty");
			var setter = FindStaticMethod("C.set_MyProperty");

			Assert.That(OutputFormatter.Format(getter.Definition, allowIntermediates: true), Is.EqualTo(
@"function() {
	return {C}.$myField;
}"));

			Assert.That(OutputFormatter.Format(setter.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	{C}.$myField = $value;
}"));
		}

		[Test]
		public void IndexerAccessorsCanBeCompiled() {
            Compile(new[] { "using System; class C { private int x; public int this[int i, int j] { get { return i + j; } set { x = i + j + value; } }" });

			var getter = FindInstanceMethod("C.get_Item");
			var setter = FindInstanceMethod("C.set_Item");

			Assert.That(OutputFormatter.Format(getter.Definition, allowIntermediates: true), Is.EqualTo(
@"function($i, $j) {
	return $i + $j;
}"));

			Assert.That(OutputFormatter.Format(setter.Definition, allowIntermediates: true), Is.EqualTo(
@"function($i, $j, $value) {
	this.$x = $i + $j + $value;
}"));
		}

		[Test]
		public void InstanceManuaEventAccessorsCanBeCompiled() {
            Compile(new[] { "using System; class C { private EventHandler myField; public event EventHandler MyEvent { add { myField += value; } remove { myField -= value; } }" });

			var getter = FindInstanceMethod("C.add_MyEvent");
			var setter = FindInstanceMethod("C.remove_MyEvent");

			Assert.That(OutputFormatter.Format(getter.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	this.$myField = {Delegate}.Combine(this.$myField, $value);
}"));

			Assert.That(OutputFormatter.Format(setter.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	this.$myField = {Delegate}.Remove(this.$myField, $value);
}"));
		}

		[Test]
		public void StaticManuaEventAccessorsCanBeCompiled() {
            Compile(new[] { "using System; class C { private static EventHandler myField; public static event EventHandler MyEvent { add { myField += value; } remove { myField -= value; } }" });

			var adder = FindStaticMethod("C.add_MyEvent");
			var remover = FindStaticMethod("C.remove_MyEvent");

			Assert.That(OutputFormatter.Format(adder.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	{C}.$myField = {Delegate}.Combine({C}.$myField, $value);
}"));

			Assert.That(OutputFormatter.Format(remover.Definition, allowIntermediates: true), Is.EqualTo(
@"function($value) {
	{C}.$myField = {Delegate}.Remove({C}.$myField, $value);
}"));
		}
	}
}
