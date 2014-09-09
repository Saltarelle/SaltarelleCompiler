using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler.Tests.CompilerTests.MemberConversion {
	[TestFixture]
	public class AttributeTests : CompilerTestBase {
		private void AssertCorrect(string attributeClass, string application, string expected, IMetadataImporter metadataImporter = null) {
			var source = "using System; " + attributeClass + " [" + application + "] class C {}";
			var compilation = Common.CreateCompilation(source);
			var c = compilation.GetTypeByMetadataName("C");
			var attr = c.GetAttributes().Single();

			var errorReporter = new MockErrorReporter(true);
			int tempCount = 0;
			var variables = new Dictionary<ISymbol, VariableData>();
			var expressionCompiler = new ExpressionCompiler(compilation.GetSemanticModel(compilation.SyntaxTrees.Single()), metadataImporter ?? new MockMetadataImporter(), new MockNamer(), new MockRuntimeLibrary(), errorReporter, variables, new Dictionary<SyntaxNode, NestedFunctionData>(), () => { var v = new SimpleVariable("tmp" + (++tempCount).ToString(CultureInfo.InvariantCulture), Location.None); variables[v] = new VariableData("$" + v.Name, null, false); return v; }, _ => { throw new NotSupportedException(); }, "this", null);
			Assert.That(errorReporter.AllMessages, Is.Empty);
			var compileResult = expressionCompiler.CompileAttributeConstruction(attr);
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
		public void AttributeWithEnumConstructorArgumentWorks() {
			AssertCorrect(
@"enum E { A, B } class MyAttribute : Attribute { public MyAttribute(E e) {} }",
@"My(E.A)",
@"TODO new {sm_MyAttribute}(42, ""x"");
");
		}

		[Test]
		public void AttributeWithTypeConstructorArgumentsWorks() {
			AssertCorrect(
@"class C {} class MyAttribute : Attribute { public MyAttribute(Type t) {} }",
@"My(typeof(C))",
@"new {sm_MyAttribute}(42, ""x"");
");
		}

		[Test]
		public void AttributeWithConstructorArrayArgumentWorks() {
			Assert.Fail("TODO");
		}

		[Test]
		public void AttributeWithConstructorArrayArgumentNestedWorks() {
			Assert.Fail("TODO");
		}

		[Test]
		public void AttributeWithNamedAndDefaultArguments() {
			Assert.Fail("TODO");
		}
	}
}
