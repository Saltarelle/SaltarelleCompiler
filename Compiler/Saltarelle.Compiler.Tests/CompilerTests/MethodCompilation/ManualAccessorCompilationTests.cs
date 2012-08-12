using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	[TestFixture]
	public class ManualAccessorCompilationTests : CompilerTestBase {
		[Test]
		public void InstanceManualPropertyAccessorsCanBeCompiled() {
            Compile(new[] { "using System; class C { public int myField; public int MyProperty { get { return myField; } set { myField = value; } } }" });

			var getter = FindInstanceMethod("C.get_MyProperty");
			var setter = FindInstanceMethod("C.set_MyProperty");

			AssertCorrect(getter.Definition,
@"function() {
	return this.$myField;
}");

			AssertCorrect(setter.Definition,
@"function($value) {
	this.$myField = $value;
}");
		}

		[Test]
		public void StaticManualPropertyAccessorsCanBeCompiled() {
            Compile(new[] { "using System; class C { public static int myField; public static int MyProperty { get { return myField; } set { myField = value; } } }" });

			var getter = FindStaticMethod("C.get_MyProperty");
			var setter = FindStaticMethod("C.set_MyProperty");

			AssertCorrect(getter.Definition,
@"function() {
	return {C}.$myField;
}");

			AssertCorrect(setter.Definition,
@"function($value) {
	{C}.$myField = $value;
}");
		}

		[Test]
		public void IndexerAccessorsCanBeCompiled() {
            Compile(new[] { "using System; class C { private int x; public int this[int i, int j] { get { return i + j; } set { x = i + j + value; } } }" });

			var getter = FindInstanceMethod("C.get_Item");
			var setter = FindInstanceMethod("C.set_Item");

			AssertCorrect(getter.Definition,
@"function($i, $j) {
	return $i + $j;
}");

			AssertCorrect(setter.Definition,
@"function($i, $j, $value) {
	this.$x = $i + $j + $value;
}");
		}

		[Test]
		public void InstanceManuaEventAccessorsCanBeCompiled() {
            Compile(new[] { "using System; class C { private EventHandler myField; public event EventHandler MyEvent { add { myField += value; } remove { myField -= value; } } }" });

			var getter = FindInstanceMethod("C.add_MyEvent");
			var setter = FindInstanceMethod("C.remove_MyEvent");

			AssertCorrect(getter.Definition,
@"function($value) {
	this.$myField = {Delegate}.Combine(this.$myField, $value);
}");

			AssertCorrect(setter.Definition,
@"function($value) {
	this.$myField = {Delegate}.Remove(this.$myField, $value);
}");
		}

		[Test]
		public void StaticManuaEventAccessorsCanBeCompiled() {
            Compile(new[] { "using System; class C { private static EventHandler myField; public static event EventHandler MyEvent { add { myField += value; } remove { myField -= value; } } }" });

			var adder = FindStaticMethod("C.add_MyEvent");
			var remover = FindStaticMethod("C.remove_MyEvent");

			AssertCorrect(adder.Definition,
@"function($value) {
	{C}.$myField = {Delegate}.Combine({C}.$myField, $value);
}");

			AssertCorrect(remover.Definition,
@"function($value) {
	{C}.$myField = {Delegate}.Remove({C}.$myField, $value);
}");
		}
	}
}
