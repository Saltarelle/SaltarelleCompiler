using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.Compiler.Expressions;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MemberConversion {
	[TestFixture]
	public class AttributeTests : CompilerTestBase {
		private Tuple<ExpressionCompileResult, MockErrorReporter> Compile(string attributeClass, string application, string expected, IMetadataImporter metadataImporter, bool expectErrors) {
			var source = "using System; " + attributeClass + " [" + application + "] class C {}";
			var compilation = Common.CreateCompilation(source);
			var c = compilation.GetTypeByMetadataName("C");
			var attr = c.GetAttributes().Single();

			var errorReporter = new MockErrorReporter(!expectErrors);
			int tempCount = 0;
			var variables = new Dictionary<ISymbol, VariableData>();
			var expressionCompiler = new ExpressionCompiler(compilation, compilation.GetSemanticModel(compilation.SyntaxTrees.Single()), metadataImporter ?? new MockMetadataImporter(), new MockNamer(), new MockRuntimeLibrary(), errorReporter, variables, () => { var v = new SimpleVariable("tmp" + (++tempCount).ToString(CultureInfo.InvariantCulture), Location.None); variables[v] = new VariableData("$" + v.Name, null, false); return v; }, _ => { throw new NotSupportedException(); }, "this", null, new Dictionary<IRangeVariableSymbol, JsExpression>());
			var compileResult = expressionCompiler.CompileAttributeConstruction(attr);
			if (expectErrors)
				Assert.That(errorReporter.AllMessages, Is.Not.Empty, "Compile should have generated errors");
			else
				Assert.That(errorReporter.AllMessages, Is.Empty, "Compile should not have generated errors");

			return Tuple.Create(compileResult, errorReporter);
		}

		private MockErrorReporter ExpectErrors(string attributeClass, string application, string expected, IMetadataImporter metadataImporter = null) {
			return Compile(attributeClass, application, expected, metadataImporter, true).Item2;
		}

		private void AssertCorrect(string attributeClass, string application, string expected, IMetadataImporter metadataImporter = null) {
			var compileResult = Compile(attributeClass, application, expected, metadataImporter, false).Item1;
			var actual = OutputFormatter.Format(compileResult.GetStatements(), allowIntermediates: true);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}

		[Test]
		public void SimpleAttributeWithDefaultConstructorCanBeApplied() {
			AssertCorrect(
@"class MyAttribute : Attribute {}",
@"My",
@"new {sm_MyAttribute}();
");
		}

		[Test]
		public void AttributeWithConstructorArgumentsWorks() {
			AssertCorrect(
@"class MyAttribute : Attribute { public MyAttribute(int a, string b, object c, string d) {} }",
@"My(42, ""x"", null, null)",
@"new {sm_MyAttribute}(42, 'x', null, null);
");
		}

		[Test]
		public void AttributeWithEnumConstructorArgumentWorksField() {
			AssertCorrect(
@"enum E { A, B } class MyAttribute : Attribute { public MyAttribute(E e) {} }",
@"My(E.A)",
@"new {sm_MyAttribute}({sm_E}.$A);
");
		}

		[Test]
		public void AttributeWithEnumConstructorArgumentWorksNonExistentField() {
			AssertCorrect(
@"enum E { A, B } class MyAttribute : Attribute { public MyAttribute(E e) {} }",
@"My((E)17)",
@"new {sm_MyAttribute}(17);
");
		}

		[Test]
		public void AttributeWithEnumConstructorArgumentWorksConstant() {
			AssertCorrect(
@"enum E { A, B } class MyAttribute : Attribute { public MyAttribute(E e) {} }",
@"My(E.A)",
@"new {sm_MyAttribute}('0');
", new MockMetadataImporter { GetFieldSemantics = f => FieldScriptSemantics.StringConstant(((int)f.ConstantValue).ToString(CultureInfo.InvariantCulture)) });
		}

		[Test]
		public void AttributeWithTypeConstructorArgumentsWorks() {
			AssertCorrect(
@"class X {} class MyAttribute : Attribute { public MyAttribute(Type t) {} }",
@"My(typeof(X))",
@"new {sm_MyAttribute}({sm_X});
");
		}

		[Test]
		public void AttributeWithConstructorArrayArgumentWorks() {
			AssertCorrect(
@"enum E { A, B } class MyAttribute : Attribute { public MyAttribute(object[] a, params int[] b) {} }",
@"My(new object[] { 1, ""2"", typeof(string), E.A, null }, 4, 5, 6)",
@"new {sm_MyAttribute}([1, '2', {sm_String}, {sm_E}.$A, null], [4, 5, 6]);
");
		}

		[Test]
		public void AttributeWithConstructorArrayArgumentNestedWorks() {
			AssertCorrect(
@"class MyAttribute : Attribute { public MyAttribute(object[] a) {} }",
@"My(new object[] { 1, 2, new object[] { 3, 4, new object[] { 5, 6, 7 }, new object[] { 8 } }, 9, new object[] { 10, 11 } })",
@"new {sm_MyAttribute}([1, 2, [3, 4, [5, 6, 7], [8]], 9, [10, 11]]);
");
		}

		[Test]
		public void AttributeWithNamedAndDefaultConstructorArguments() {
			AssertCorrect(
@"class MyAttribute : Attribute { public MyAttribute(int a = 1, int b = 2, int c = 3, int d = 4, int e = 5, int f = 6, int g = 7) {} }",
@"MyAttribute(d: 10, g: 11, f: 12, b: 13)",
@"new {sm_MyAttribute}(1, 13, 3, 10, 5, 12, 11);
");
		}

		[Test]
		public void AttributeWithNamedArguments() {
			AssertCorrect(
@"class MyAttribute : Attribute { public int F1; public int P1 { get; set; } public int P2 { get; set; } public int P3 { get; set; } }",
@"MyAttribute(F1 = 4, P1 = 5, P2 = 8, P3 = 7)",
@"var $tmp1 = new {sm_MyAttribute}();
$tmp1.$F1 = 4;
$tmp1.set_$P1(5);
$tmp1.$P2 = 8;
_($tmp1, 7);
$tmp1;
", new MockMetadataImporter { GetPropertySemantics = p => {
	switch (p.Name) {
		case "P1":
			return PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_$P1"), MethodScriptSemantics.NormalMethod("set_$P1"));
		case "P2":
			return PropertyScriptSemantics.Field("$P2");
		case "P3":
			return PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("_()"), MethodScriptSemantics.InlineCode("_({this}, {value})"));
		default:
			throw new NotSupportedException("Invalid property");
	}
}} );
		}

		[Test]
		public void AttributeWithComplexNamedArgumentArray() {
			AssertCorrect(
@"class MyAttribute : Attribute { public object F; }",
@"My(F = new object[] { 1, 2, new object[] { 3, 4, new object[] { 5, 6, 7 }, new object[] { 8 } }, 9, new object[] { 10, 11 } })",
@"var $tmp1 = new {sm_MyAttribute}();
$tmp1.$F = [1, 2, [3, 4, [5, 6, 7], [8]], 9, [10, 11]];
$tmp1;
", new MockMetadataImporter { GetPropertySemantics = p => {
	switch (p.Name) {
		case "P1":
			return PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_$P1"), MethodScriptSemantics.NormalMethod("set_$P1"));
		case "P2":
			return PropertyScriptSemantics.Field("$P2");
		case "P3":
			return PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("_()"), MethodScriptSemantics.InlineCode("_({this}, {value})"));
		default:
			throw new NotSupportedException("Invalid property");
	}
}} );
		}

		[Test]
		public void AttributeWithConstructorAndNamedArguments() {
			AssertCorrect(
@"class MyAttribute : Attribute { public MyAttribute(string x, string y) {} public int F1; public int P1 { get; set; } public int P2 { get; set; } public int P3 { get; set; } }",
@"MyAttribute(""a"", ""b"", F1 = 4, P1 = 5, P2 = 8, P3 = 7)",
@"var $tmp1 = new {sm_MyAttribute}('a', 'b');
$tmp1.$F1 = 4;
$tmp1.set_$P1(5);
$tmp1.$P2 = 8;
_($tmp1, 7);
$tmp1;
", new MockMetadataImporter { GetPropertySemantics = p => {
	switch (p.Name) {
		case "P1":
			return PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_$P1"), MethodScriptSemantics.NormalMethod("set_$P1"));
		case "P2":
			return PropertyScriptSemantics.Field("$P2");
		case "P3":
			return PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("_()"), MethodScriptSemantics.InlineCode("_({this}, {value})"));
		default:
			throw new NotSupportedException("Invalid property");
	}
}} );
		}

		[Test]
		public void AttributeWithNamedArgumentsCanAssignToInheritedMember() {
			AssertCorrect(
@"class BAttribute : Attribute { public int P1, P2; } class CAttribute : BAttribute { public int P1, P3, P4; } class DAttribute : CAttribute { public int P4, P5; }",
@"D(P1 = 1, P2 = 2, P3 = 3, P4 = 4, P5 = 5)",
@"var $tmp1 = new {sm_DAttribute}();
$tmp1.CAttribute$P1 = 1;
$tmp1.BAttribute$P2 = 2;
$tmp1.CAttribute$P3 = 3;
$tmp1.DAttribute$P4 = 4;
$tmp1.DAttribute$P5 = 5;
$tmp1;
", metadataImporter: new MockMetadataImporter { GetFieldSemantics = f => FieldScriptSemantics.Field(f.ContainingType.Name + "$" + f.Name) });
		}

		[Test]
		public void CanConstructAttributeWithInlineCodeConstructor() {
			AssertCorrect(
@"class MyAttribute : Attribute { public MyAttribute(int a, int b) {} }",
@"My(42, 37)",
@"_(42)._(37);
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.ContainingType.Name == "MyAttribute" ? ConstructorScriptSemantics.InlineCode("_({a})._({b})") : ConstructorScriptSemantics.Unnamed() });
		}

		[Test]
		public void ConstructingAttributeWithUnusableConstructorIsAnError() {
			var er = ExpectErrors(
@"class MyAttribute : Attribute { public MyAttribute(int a, int b) {} }",
@"My(42, 37)",
@"var $tmp1 = new {sm_DAttribute}();
$tmp1.CAttribute$P1 = 1;
$tmp1.BAttribute$P2 = 2;
$tmp1.CAttribute$P3 = 3;
$tmp1.DAttribute$P4 = 4;
$tmp1.DAttribute$P5 = 5;
$tmp1;
", metadataImporter: new MockMetadataImporter { GetConstructorSemantics = c => c.ContainingType.Name == "MyAttribute" ? ConstructorScriptSemantics.NotUsableFromScript() : ConstructorScriptSemantics.Unnamed() });

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7505));
		}

		[Test]
		public void AssigningUnusableMemberIsAnError() {
			var er = ExpectErrors(
@"class MyAttribute : Attribute { public int F; }",
@"My(F = 42)",
@"var $tmp1 = new {sm_DAttribute}();
$tmp1.CAttribute$P1 = 1;
$tmp1.BAttribute$P2 = 2;
$tmp1.CAttribute$P3 = 3;
$tmp1.DAttribute$P4 = 4;
$tmp1.DAttribute$P5 = 5;
$tmp1;
", metadataImporter: new MockMetadataImporter { GetFieldSemantics = f => f.ContainingType.Name == "MyAttribute" ? FieldScriptSemantics.NotUsableFromScript() : FieldScriptSemantics.Field("$" + f.Name) });

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(e => e.Severity == DiagnosticSeverity.Error && e.Code == 7509 && e.FormattedMessage.Contains("MyAttribute.F")));
		}
	}
}
