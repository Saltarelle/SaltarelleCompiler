using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib.Plugin;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests;
using Saltarelle.Compiler.Roslyn;

namespace CoreLib.Tests.MetadataImporterTests {
	[TestFixture]
	public class AnonymousTypeTests {
		private Tuple<CSharpCompilation, INamedTypeSymbol> CreateType(params string[] propertyNames) {
			var source = @"class C { public void M() { var x = new { " + string.Join(", ", propertyNames.Select(m => m + " = 0")) + "} } }";
			var syntaxTree = CSharpSyntaxTree.ParseText(source);
			var compilation = CSharpCompilation.Create("Test", new[] { syntaxTree }, new[] { Common.Mscorlib });
			var expr = syntaxTree.GetRoot().DescendantNodes().OfType<AnonymousObjectCreationExpressionSyntax>().Single();
			var semanticModel = compilation.GetSemanticModel(syntaxTree);
			return Tuple.Create(compilation, (INamedTypeSymbol)semanticModel.GetTypeInfo(expr).Type);
		}

		private MetadataImporter CreateMetadataImporter(CSharpCompilation compilation, INamedTypeSymbol type, CompilerOptions compilerOptions) {
			var er = new MockErrorReporter(true);
			var s = new AttributeStore(compilation, er, new IAutomaticMetadataAttributeApplier[0]);
			var md = new MetadataImporter(Common.ReferenceMetadataImporter, er, compilation, s, compilerOptions);
			md.Prepare(compilation.GetAllTypes());
			if (er.AllMessages.Count > 0) {
				Assert.Fail("Errors:" + Environment.NewLine + string.Join(Environment.NewLine, er.AllMessages));
			}

			return md;
		}

		[Test]
		public void PropertiesAreImplementedAsFieldsWithTheSameName() {
			var t = CreateType("prop1", "Prop2");
			var md = CreateMetadataImporter(t.Item1, t.Item2, new CompilerOptions());

			var p1 = md.GetPropertySemantics((IPropertySymbol)t.Item2.GetMembers("prop1").Single());
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p1.FieldName, Is.EqualTo("prop1"));

			var p2 = md.GetPropertySemantics((IPropertySymbol)t.Item2.GetMembers("Prop2").Single());
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p2.FieldName, Is.EqualTo("Prop2"));
		}

		[Test]
		public void AnonymousTypePropertyNamesAreNotMinimized() {
			var t = CreateType("prop1", "Prop2");
			var md = CreateMetadataImporter(t.Item1, t.Item2, new CompilerOptions { MinimizeScript = true });

			var p1 = md.GetPropertySemantics((IPropertySymbol)t.Item2.GetMembers("prop1").Single());
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p1.FieldName, Is.EqualTo("prop1"));

			var p2 = md.GetPropertySemantics((IPropertySymbol)t.Item2.GetMembers("Prop2").Single());
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p2.FieldName, Is.EqualTo("Prop2"));
		}
	}
}
