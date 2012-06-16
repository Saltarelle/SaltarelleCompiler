using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class AnonymousTypeTests {
		private IType CreateType(ICompilation compilation) {
			var unresolvedProperties = new[] { "prop1", "Prop2" }
				.Select(name =>
					new DefaultUnresolvedProperty {
						Name = name,
						Accessibility = Accessibility.Public,
						ReturnType = KnownTypeReference.Int32,
						Getter = new DefaultUnresolvedMethod {
							Name = "get_" + name,
							Accessibility = Accessibility.Public,
							ReturnType = KnownTypeReference.Int32
						}
					})
				.Cast<IUnresolvedProperty>()
				.ToList();

			return new AnonymousType(compilation, unresolvedProperties);
		}

		[Test]
		public void ConstructorsAreReportedAsJsonConstructors() {
			var compilation = new SimpleCompilation(new CSharpProjectContent());
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var er = new MockErrorReporter(true);
			md.Prepare(new ITypeDefinition[0], compilation.MainAssembly, er);
			Assert.That(er.AllMessages, Is.Empty, "Prepare should not generate errors");

			var t = CreateType(compilation);

			var c = md.GetConstructorSemantics(DefaultResolvedMethod.GetDummyConstructor(compilation, t));
			Assert.That(c.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.Json));
		}

		[Test]
		public void PropertiesAreImplementedAsFieldsWithTheSameName() {
			var compilation = new SimpleCompilation(new CSharpProjectContent());
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var er = new MockErrorReporter(true);
			md.Prepare(new ITypeDefinition[0], compilation.MainAssembly, er);
			Assert.That(er.AllMessages, Is.Empty, "Prepare should not generate errors");

			var t = CreateType(compilation);

			var p1 = md.GetPropertySemantics(t.GetProperties().Single(p => p.Name == "prop1"));
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p1.FieldName, Is.EqualTo("prop1"));

			var p2 = md.GetPropertySemantics(t.GetProperties().Single(p => p.Name == "Prop2"));
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p2.FieldName, Is.EqualTo("Prop2"));
		}

		[Test]
		public void AnonymousTypePropertyNamesAreNotMinimized() {
			var compilation = new SimpleCompilation(new CSharpProjectContent());
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			var er = new MockErrorReporter(true);
			md.Prepare(new ITypeDefinition[0], compilation.MainAssembly, er);
			Assert.That(er.AllMessages, Is.Empty, "Prepare should not generate errors");

			var t = CreateType(compilation);

			var p1 = md.GetPropertySemantics(t.GetProperties().Single(p => p.Name == "prop1"));
			Assert.That(p1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p1.FieldName, Is.EqualTo("prop1"));

			var p2 = md.GetPropertySemantics(t.GetProperties().Single(p => p.Name == "Prop2"));
			Assert.That(p2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
			Assert.That(p2.FieldName, Is.EqualTo("Prop2"));
		}
	}
}
