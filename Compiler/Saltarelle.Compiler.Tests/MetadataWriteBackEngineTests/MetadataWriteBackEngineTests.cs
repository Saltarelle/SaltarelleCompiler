using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.Cecil;
using NUnit.Framework;
using Saltarelle.Compiler.MetadataWriteBackEngine;
using Saltarelle.Compiler.Tests.MetadataWriteBackEngineTests.MetadataWriteBackEngineTestCase;

namespace Saltarelle.Compiler.Tests.MetadataWriteBackEngineTests {
	[TestFixture]
	public class MetadataWriteBackEngineTests {
    	private static readonly Lazy<IAssemblyReference> _currentAsmLazy = new Lazy<IAssemblyReference>(() => new CecilLoader() { IncludeInternalMembers = true }.LoadAssemblyFile(typeof(MetadataWriteBackEngineTests).Assembly.Location));
        internal static IAssemblyReference CurrentAsm { get { return _currentAsmLazy.Value; } }

		private void RunTest(Action<IMetadataWriteBackEngine, ICompilation> asserter) {
            IProjectContent project = new CSharpProjectContent();

            project = project.AddAssemblyReferences(new[] { Common.Mscorlib, CurrentAsm });

			var compilation = project.CreateCompilation();

			var asm = AssemblyDefinition.ReadAssembly(typeof(MetadataWriteBackEngineTests).Assembly.Location);
			var eng = new CecilMetadataWriteBackEngine(asm, compilation);

			asserter(eng, compilation);
		}

		[Test]
		public void CanGetAttributesOfType() {
			RunTest((engine, compilation) => {
				var attrs = engine.GetAttributes((ITypeDefinition)ReflectionHelper.ParseReflectionName("Saltarelle.Compiler.Tests.MetadataWriteBackEngineTests.MetadataWriteBackEngineTestCase.ClassWithAttribute").Resolve(compilation));
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This class has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfField() {
			RunTest((engine, compilation) => {
				var fld = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedField).FullName).Resolve(compilation).GetFields().Single(f => f.Name == "MyField");
				var attrs = engine.GetAttributes(fld);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This field has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfProperty() {
			RunTest((engine, compilation) => {
				var prop = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedProperty).FullName).Resolve(compilation).GetProperties().Single(f => f.Name == "MyProperty");
				var attrs = engine.GetAttributes(prop);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This property has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfPropertyWhichIsAnExplicitInterfaceImplementation() {
			RunTest((engine, compilation) => {
				var prop = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedProperty).FullName).Resolve(compilation).GetProperties().Single(f => f.Name == "MyProperty");
				var attrs = engine.GetAttributes(prop);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This property has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfPropertyGetter() {
			RunTest((engine, compilation) => {
				var prop = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedProperty).FullName).Resolve(compilation).GetProperties().Single(f => f.Name == "MyProperty").Getter;
				var attrs = engine.GetAttributes(prop);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This getter has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfPropertyGetterWhichIsAnExplicitInterfaceImplementation() {
			RunTest((engine, compilation) => {
				var prop = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedProperty).FullName).Resolve(compilation).GetProperties().Single(f => f.Name == "MyProperty").Getter;
				var attrs = engine.GetAttributes(prop);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This getter has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfPropertySetter() {
			RunTest((engine, compilation) => {
				var prop = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedProperty).FullName).Resolve(compilation).GetProperties().Single(f => f.Name == "MyProperty").Setter;
				var attrs = engine.GetAttributes(prop);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This setter has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfPropertySetterWhichIsAnExplicitInterfaceImplementation() {
			RunTest((engine, compilation) => {
				var prop = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedProperty).FullName).Resolve(compilation).GetProperties().Single(f => f.Name == "MyProperty").Setter;
				var attrs = engine.GetAttributes(prop);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This setter has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfIndexer() {
			RunTest((engine, compilation) => {
				var indexer1 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 1);
				var attrs = engine.GetAttributes(indexer1);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int)" }));

				var indexer2 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.Int32");
				attrs = engine.GetAttributes(indexer2);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int,int)" }));

				var indexer3 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.String");
				attrs = engine.GetAttributes(indexer3);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int,string)" }));
			});
		}

		[Test]
		public void CanGetAttributesOfIndexerWhichIsAnExplicitInterfaceImplementation() {
			Assert.Fail("TODO, including overloads");
		}

		[Test]
		public void CanGetAttributesOfIndexerGetter() {
			Assert.Fail("TODO");
		}

		[Test]
		public void CanGetAttributesOfIndexerSetter() {
			Assert.Fail("TODO");
		}

		[Test]
		public void CanGetAttributesOfEvent() {
			Assert.Fail("TODO");
		}

		[Test]
		public void CanGetAttributesOfEventWhichIsAnExplicitInterfaceImplementation() {
			Assert.Fail("TODO");
		}

		[Test]
		public void CanGetAttributesOfEventAdder() {
			Assert.Fail("TODO");
		}

		[Test]
		public void CanGetAttributesOfEventRemover() {
			Assert.Fail("TODO");
		}

		[Test]
		public void CanGetAttributesOfMethod() {
			Assert.Fail("TODO, including overloads");
		}

		[Test]
		public void CanGetAttributesOfMethodWhichIsAnExplicitInterfaceImplementation() {
			Assert.Fail("TODO, including overloads");
		}

		[Test]
		public void CanGetAttributesOfOperator() {
			Assert.Fail("TODO, including overloads, op_Implicit/explicit");
		}

		[Test]
		public void CanGetAttributesOfConstructor() {
			Assert.Fail("TODO, including overloads");
		}

		[Test]
		public void PositionalArgumentsWork() {
			Assert.Fail("TODO");
		}

		[Test]
		public void NamedArgumentsWork() {
			Assert.Fail("TODO");
		}

		[Test]
		public void ConstructorCanBeResolved() {
			Assert.Fail("TODO");
		}
	}
}
