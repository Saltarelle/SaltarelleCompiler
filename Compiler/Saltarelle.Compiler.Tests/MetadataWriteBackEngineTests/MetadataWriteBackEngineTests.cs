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

		private void RunTest(Action<CecilMetadataWriteBackEngine, ICompilation> asserter) {
			RunTest((e, c, a) => asserter(e, c));
		}

		private void RunTest(Action<CecilMetadataWriteBackEngine, ICompilation, AssemblyDefinition> asserter) {
            IProjectContent project = new CSharpProjectContent();

            project = project.AddAssemblyReferences(new[] { Common.Mscorlib, CurrentAsm });

			var compilation = project.CreateCompilation();

			var asm = AssemblyDefinition.ReadAssembly(typeof(MetadataWriteBackEngineTests).Assembly.Location);
			var eng = new CecilMetadataWriteBackEngine(asm, compilation);

			asserter(eng, compilation, asm);
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
			RunTest((engine, compilation) => {
				var indexer1 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 1);
				var attrs = engine.GetAttributes(indexer1);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int)" }));

				var indexer2 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.Int32");
				attrs = engine.GetAttributes(indexer2);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int,int)" }));

				var indexer3 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.String");
				attrs = engine.GetAttributes(indexer3);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int,string)" }));
			});
		}

		[Test]
		public void CanGetAttributesOfIndexerGetter() {
			RunTest((engine, compilation) => {
				var indexer1 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 1).Getter;
				var attrs = engine.GetAttributes(indexer1);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int) getter" }));

				var indexer2 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.Int32").Getter;
				attrs = engine.GetAttributes(indexer2);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int,int) getter" }));

				var indexer3 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.String").Getter;
				attrs = engine.GetAttributes(indexer3);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int,string) getter" }));
			});
		}

		[Test]
		public void CanGetAttributesOfIndexerGetterWhichIsAnExplicitInterfaceImplementation() {
			RunTest((engine, compilation) => {
				var indexer1 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 1).Getter;
				var attrs = engine.GetAttributes(indexer1);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int) getter" }));

				var indexer2 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.Int32").Getter;
				attrs = engine.GetAttributes(indexer2);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int,int) getter" }));

				var indexer3 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.String").Getter;
				attrs = engine.GetAttributes(indexer3);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int,string) getter" }));
			});
		}

		[Test]
		public void CanGetAttributesOfIndexerSetter() {
			RunTest((engine, compilation) => {
				var indexer1 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 1).Setter;
				var attrs = engine.GetAttributes(indexer1);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int) setter" }));

				var indexer2 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.Int32").Setter;
				attrs = engine.GetAttributes(indexer2);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int,int) setter" }));

				var indexer3 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.String").Setter;
				attrs = engine.GetAttributes(indexer3);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int,string) setter" }));
			});
		}

		[Test]
		public void CanGetAttributesOfIndexerSetterWhichIsAnExplicitInterfaceImplementation() {
			RunTest((engine, compilation) => {
				var indexer1 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 1).Setter;
				var attrs = engine.GetAttributes(indexer1);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int) setter" }));

				var indexer2 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.Int32").Setter;
				attrs = engine.GetAttributes(indexer2);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int,int) setter" }));

				var indexer3 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedIndexers).FullName).Resolve(compilation).GetProperties().Single(p => p.Name == "Item" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.String").Setter;
				attrs = engine.GetAttributes(indexer3);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Indexer(int,string) setter" }));
			});
		}

		[Test]
		public void CanGetAttributesOfEvent() {
			RunTest((engine, compilation) => {
				var prop = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedEvent).FullName).Resolve(compilation).GetEvents().Single(e => e.Name == "MyEvent");
				var attrs = engine.GetAttributes(prop);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This event has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfEventAdder() {
			RunTest((engine, compilation) => {
				var prop = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedEvent).FullName).Resolve(compilation).GetEvents().Single(e => e.Name == "MyEvent").AddAccessor;
				var attrs = engine.GetAttributes(prop);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This event adder has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfEventRemover() {
			RunTest((engine, compilation) => {
				var prop = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedEvent).FullName).Resolve(compilation).GetEvents().Single(e => e.Name == "MyEvent").RemoveAccessor;
				var attrs = engine.GetAttributes(prop);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This event remover has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfEventWhichIsAnExplicitInterfaceImplementation() {
			RunTest((engine, compilation) => {
				var prop = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedEventAccessors).FullName).Resolve(compilation).GetEvents().Single(e => e.Name == "MyEvent");
				var attrs = engine.GetAttributes(prop);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This event has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfEventAdderWhichIsAnExplicitInterfaceImplementation() {
			RunTest((engine, compilation) => {
				var prop = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedEventAccessors).FullName).Resolve(compilation).GetEvents().Single(e => e.Name == "MyEvent").AddAccessor;
				var attrs = engine.GetAttributes(prop);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This event adder has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfEventRemoverWhichIsAnExplicitInterfaceImplementation() {
			RunTest((engine, compilation) => {
				var prop = ReflectionHelper.ParseReflectionName(typeof(ClassWithAttributedExplicitlyImplementedEventAccessors).FullName).Resolve(compilation).GetEvents().Single(e => e.Name == "MyEvent").RemoveAccessor;
				var attrs = engine.GetAttributes(prop);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "This event remover has an attribute" }));
			});
		}

		[Test]
		public void CanGetAttributesOfMethod() {
			RunTest((engine, compilation) => {
				var method1 = ReflectionHelper.ParseReflectionName(typeof(ClassWithMethods).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "MyMethod" && p.Parameters.Count == 0);
				var attrs = engine.GetAttributes(method1);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "MyMethod()" }));

				var method2 = ReflectionHelper.ParseReflectionName(typeof(ClassWithMethods).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "MyMethod" && p.Parameters.Count == 1);
				attrs = engine.GetAttributes(method2);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "MyMethod(int)" }));

				var method3 = ReflectionHelper.ParseReflectionName(typeof(ClassWithMethods).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "MyMethod" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.Int32");
				attrs = engine.GetAttributes(method3);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "MyMethod(int,int)" }));

				var method4 = ReflectionHelper.ParseReflectionName(typeof(ClassWithMethods).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "MyMethod" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.String");
				attrs = engine.GetAttributes(method4);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "MyMethod(int,string)" }));
			});
		}

		[Test]
		public void CanGetAttributesOfMethodWhichIsAnExplicitInterfaceImplementation() {
			RunTest((engine, compilation) => {
				var method1 = ReflectionHelper.ParseReflectionName(typeof(ClassWithMethods).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "MyMethod" && p.Parameters.Count == 0);
				var attrs = engine.GetAttributes(method1);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "MyMethod()" }));

				var method2 = ReflectionHelper.ParseReflectionName(typeof(ClassWithMethods).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "MyMethod" && p.Parameters.Count == 1);
				attrs = engine.GetAttributes(method2);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "MyMethod(int)" }));

				var method3 = ReflectionHelper.ParseReflectionName(typeof(ClassWithMethods).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "MyMethod" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.Int32");
				attrs = engine.GetAttributes(method3);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "MyMethod(int,int)" }));

				var method4 = ReflectionHelper.ParseReflectionName(typeof(ClassWithMethods).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "MyMethod" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.String");
				attrs = engine.GetAttributes(method4);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "MyMethod(int,string)" }));
			});
		}

		[Test]
		public void CanGetAttributesOfMethodWhichIsAnExplicitInterfaceImplementationInAGenericClass() {
			RunTest((engine, compilation) => {
				var method1 = ReflectionHelper.ParseReflectionName(typeof(GenericClassWithAttributedExplicitlyImplementedMethods<>).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "MyMethod" && p.Parameters.Count == 0);
				var attrs = engine.GetAttributes(method1);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "MyMethod()" }));

				var method2 = ReflectionHelper.ParseReflectionName(typeof(GenericClassWithAttributedExplicitlyImplementedMethods<>).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "MyMethod" && p.Parameters.Count == 1);
				attrs = engine.GetAttributes(method2);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "MyMethod(int)" }));

				var method3 = ReflectionHelper.ParseReflectionName(typeof(GenericClassWithAttributedExplicitlyImplementedMethods<>).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "MyMethod" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.Int32");
				attrs = engine.GetAttributes(method3);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "MyMethod(int,int)" }));

				var method4 = ReflectionHelper.ParseReflectionName(typeof(GenericClassWithAttributedExplicitlyImplementedMethods<>).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "MyMethod" && p.Parameters.Count == 2 && p.Parameters[1].Type.FullName == "System.String");
				attrs = engine.GetAttributes(method4);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "MyMethod(int,string)" }));
			});
		}

		[Test]
		public void CanGetAttributesOfOperator() {
			RunTest((engine, compilation) => {
				var operator1 = ReflectionHelper.ParseReflectionName(typeof(ClassWithOperators).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "op_Addition" && p.Parameters[1].Type.FullName != "System.Int32");
				var attrs = engine.GetAttributes(operator1);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Add class instances" }));

				var operator2 = ReflectionHelper.ParseReflectionName(typeof(ClassWithOperators).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "op_Addition" && p.Parameters[1].Type.FullName == "System.Int32");
				attrs = engine.GetAttributes(operator2);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Add class and int" }));

				var operator3 = ReflectionHelper.ParseReflectionName(typeof(ClassWithOperators).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "op_Implicit" && p.ReturnType.FullName == "System.Int32");
				attrs = engine.GetAttributes(operator3);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Convert to int" }));

				var operator4 = ReflectionHelper.ParseReflectionName(typeof(ClassWithOperators).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "op_Implicit" && p.ReturnType.FullName == "System.Single");
				attrs = engine.GetAttributes(operator4);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Convert to float" }));

				var operator5 = ReflectionHelper.ParseReflectionName(typeof(ClassWithOperators).FullName).Resolve(compilation).GetMethods().Single(p => p.Name == "op_Explicit" && p.ReturnType.FullName == "System.String");
				attrs = engine.GetAttributes(operator5);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Convert to string" }));
			});
		}

		[Test]
		public void CanGetAttributesOfConstructor() {
			RunTest((engine, compilation) => {
				var ctor1 = ReflectionHelper.ParseReflectionName(typeof(ClassWithConstructors).FullName).Resolve(compilation).GetConstructors().Single(c => c.Parameters.Count == 0);
				var attrs = engine.GetAttributes(ctor1);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Constructor()" }));

				var ctor2 = ReflectionHelper.ParseReflectionName(typeof(ClassWithConstructors).FullName).Resolve(compilation).GetConstructors().Single(c => c.Parameters.Count == 1);
				attrs = engine.GetAttributes(ctor2);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Constructor(int)" }));

				var ctor3 = ReflectionHelper.ParseReflectionName(typeof(ClassWithConstructors).FullName).Resolve(compilation).GetConstructors().Single(c => c.Parameters.Count == 2 && c.Parameters[1].Type.FullName == "System.Int32");
				attrs = engine.GetAttributes(ctor3);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Constructor(int,int)" }));

				var ctor4 = ReflectionHelper.ParseReflectionName(typeof(ClassWithConstructors).FullName).Resolve(compilation).GetConstructors().Single(c => c.Parameters.Count == 2 && c.Parameters[1].Type.FullName == "System.String");
				attrs = engine.GetAttributes(ctor4);
				Assert.That(attrs.Count, Is.EqualTo(1));
				attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(MyAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments.Select(a => a.ConstantValue), Is.EqualTo(new[] { "Constructor(int,string)" }));
			});
		}

		[Test]
		public void ConstructorCanBeResolved() {
			RunTest((engine, compilation) => {
				var ctor1 = ReflectionHelper.ParseReflectionName(typeof(ClassWithAConstructorWithAComplexAttribute).FullName).Resolve(compilation).GetConstructors().Single();
				var attrs = engine.GetAttributes(ctor1);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(ComplexAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.Constructor.Parameters.Select(p => p.Type), Is.EqualTo(new[] { compilation.FindType(KnownTypeCode.Byte), compilation.FindType(KnownTypeCode.String) }));
			});
		}

		[Test]
		public void PositionalArgumentsWork() {
			RunTest((engine, compilation) => {
				var ctor = ReflectionHelper.ParseReflectionName(typeof(ClassWithAConstructorWithAComplexAttribute).FullName).Resolve(compilation).GetConstructors().Single();
				var attrs = engine.GetAttributes(ctor);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(ComplexAttribute).FullName).Resolve(compilation)));
				Assert.That(attr.PositionalArguments[0].Type, Is.EqualTo(compilation.FindType(KnownTypeCode.Byte)));
				Assert.That(attr.PositionalArguments[0].ConstantValue, Is.InstanceOf<byte>());
				Assert.That(attr.PositionalArguments[0].ConstantValue, Is.EqualTo(42));
				Assert.That(attr.PositionalArguments[1].Type, Is.EqualTo(compilation.FindType(KnownTypeCode.String)));
				Assert.That(attr.PositionalArguments[1].ConstantValue, Is.InstanceOf<string>());
				Assert.That(attr.PositionalArguments[1].ConstantValue, Is.EqualTo("Some value"));
			});
		}

		[Test]
		public void NamedArgumentsWork() {
			RunTest((engine, compilation) => {
				var ctor = ReflectionHelper.ParseReflectionName(typeof(ClassWithAConstructorWithAComplexAttribute).FullName).Resolve(compilation).GetConstructors().Single();
				var attrs = engine.GetAttributes(ctor);
				Assert.That(attrs.Count, Is.EqualTo(1));
				var attr = attrs.ElementAt(0);
				Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(ComplexAttribute).FullName).Resolve(compilation)));
				var namedArgs = attr.NamedArguments.ToDictionary(x => x.Key.Name);
				Assert.That(namedArgs.Keys, Is.EquivalentTo(new[] { "Property1", "Property2", "Property3", "Field1" }));

				var p1 = namedArgs["Property1"].Value;
				Assert.That(p1.Type, Is.EqualTo(compilation.FindType(KnownTypeCode.String)));
				Assert.That(p1.ConstantValue, Is.InstanceOf<string>());
				Assert.That(p1.ConstantValue, Is.EqualTo("Property 1 value"));

				var p2 = namedArgs["Property2"].Value;
				Assert.That(p2.Type, Is.EqualTo(compilation.FindType(KnownTypeCode.Int32)));
				Assert.That(p2.ConstantValue, Is.InstanceOf<int>());
				Assert.That(p2.ConstantValue, Is.EqualTo(347));

				var p3 = namedArgs["Property3"].Value;
				Assert.That(p3.Type, Is.EqualTo(compilation.FindType(KnownTypeCode.String)));
				Assert.That(p3.ConstantValue, Is.Null);

				var f1 = namedArgs["Field1"].Value;
				Assert.That(f1.Type, Is.EqualTo(compilation.FindType(KnownTypeCode.Byte)));
				Assert.That(f1.ConstantValue, Is.InstanceOf<byte>());
				Assert.That(f1.ConstantValue, Is.EqualTo(12));
			});
		}

		[Test]
		public void CreateAttributeCanCreateAnAttributeWithNamedAndPositionalArguments() {
			RunTest((engine, compilation) => {
				var a = engine.CreateAttribute(compilation.ReferencedAssemblies[1], typeof(ComplexAttribute).FullName, new[] { Tuple.Create(compilation.FindType(KnownTypeCode.Int32), (object)352), Tuple.Create(compilation.FindType(KnownTypeCode.String), (object)"Test attribute") }, new[] { Tuple.Create("Property2", (object)567), Tuple.Create("Field1", (object)(byte)34) });
				var attrType = ReflectionHelper.ParseReflectionName(typeof(ComplexAttribute).FullName).Resolve(compilation);
				Assert.That(a.AttributeType, Is.EqualTo(attrType));
				Assert.That(a.Constructor, Is.EqualTo(attrType.GetConstructors().Single(c => c.Parameters.Count == 2 && c.Parameters[0].Type == compilation.FindType(KnownTypeCode.Int32) && c.Parameters[1].Type == compilation.FindType(KnownTypeCode.String))));
				Assert.That(a.PositionalArguments[0].Type, Is.EqualTo(compilation.FindType(KnownTypeCode.Int32)));
				Assert.That(a.PositionalArguments[0].ConstantValue, Is.InstanceOf<int>());
				Assert.That(a.PositionalArguments[0].ConstantValue, Is.EqualTo(352));
				Assert.That(a.PositionalArguments[1].Type, Is.EqualTo(compilation.FindType(KnownTypeCode.String)));
				Assert.That(a.PositionalArguments[1].ConstantValue, Is.InstanceOf<string>());
				Assert.That(a.PositionalArguments[1].ConstantValue, Is.EqualTo("Test attribute"));

				var namedArgs = a.NamedArguments.ToDictionary(na => na.Key.Name);
				Assert.That(namedArgs.Keys, Is.EquivalentTo(new[] { "Property2", "Field1" }));
				Assert.That(namedArgs["Property2"].Key, Is.EqualTo(attrType.GetProperties().Single(p => p.Name == "Property2")));
				Assert.That(namedArgs["Property2"].Value.Type, Is.EqualTo(compilation.FindType(KnownTypeCode.Int32)));
				Assert.That(namedArgs["Property2"].Value.ConstantValue, Is.InstanceOf<int>());
				Assert.That(namedArgs["Property2"].Value.ConstantValue, Is.EqualTo(567));
				Assert.That(namedArgs["Field1"].Key, Is.EqualTo(attrType.GetFields().Single(p => p.Name == "Field1")));
				Assert.That(namedArgs["Field1"].Value.Type, Is.EqualTo(compilation.FindType(KnownTypeCode.Byte)));
				Assert.That(namedArgs["Field1"].Value.ConstantValue, Is.InstanceOf<byte>());
				Assert.That(namedArgs["Field1"].Value.ConstantValue, Is.EqualTo(34));
			});
		}

		[Test]
		public void CreateAttributeSearchesAllAssembliesWhenTheSuppliedAssemblyIsNull() {
			RunTest((engine, compilation) => {
				var a = engine.CreateAttribute(null, typeof(ComplexAttribute).FullName, new Tuple<IType, object>[0], new Tuple<string, object>[0]);
				var attrType = ReflectionHelper.ParseReflectionName(typeof(ComplexAttribute).FullName).Resolve(compilation);
				Assert.That(a.AttributeType, Is.EqualTo(attrType));
				Assert.That(a.Constructor, Is.EqualTo(attrType.GetConstructors().Single(c => c.Parameters.Count == 0)));
			});
		}

		[Test]
		public void CreateAttributeWorksWhenNamedOrPositionalArgIsNull() {
			RunTest((engine, compilation) => {
				var a = engine.CreateAttribute(compilation.ReferencedAssemblies[1], typeof(ComplexAttribute).FullName, new[] { Tuple.Create(compilation.FindType(KnownTypeCode.Int32), (object)352), Tuple.Create(compilation.FindType(KnownTypeCode.String), (object)null) }, new[] { Tuple.Create("Property1", (object)null) });
				var attrType = ReflectionHelper.ParseReflectionName(typeof(ComplexAttribute).FullName).Resolve(compilation);
				Assert.That(a.AttributeType, Is.EqualTo(attrType));
				Assert.That(a.Constructor, Is.EqualTo(attrType.GetConstructors().Single(c => c.Parameters.Count == 2 && c.Parameters[0].Type == compilation.FindType(KnownTypeCode.Int32) && c.Parameters[1].Type == compilation.FindType(KnownTypeCode.String))));
				Assert.That(a.PositionalArguments[0].Type, Is.EqualTo(compilation.FindType(KnownTypeCode.Int32)));
				Assert.That(a.PositionalArguments[0].ConstantValue, Is.InstanceOf<int>());
				Assert.That(a.PositionalArguments[0].ConstantValue, Is.EqualTo(352));
				Assert.That(a.PositionalArguments[1].Type, Is.EqualTo(compilation.FindType(KnownTypeCode.String)));
				Assert.That(a.PositionalArguments[1].ConstantValue, Is.Null);

				var namedArgs = a.NamedArguments.ToDictionary(na => na.Key.Name);
				Assert.That(namedArgs.Keys, Is.EquivalentTo(new[] { "Property1" }));
				Assert.That(namedArgs["Property1"].Key, Is.EqualTo(attrType.GetProperties().Single(p => p.Name == "Property1")));
				Assert.That(namedArgs["Property1"].Value.Type, Is.EqualTo(compilation.FindType(KnownTypeCode.String)));
				Assert.That(namedArgs["Property1"].Value.ConstantValue, Is.Null);
			});
		}

		[Test]
		public void CreateAttributeCanLookupTheCorrectConstructorWithOverloadResolution() {
			RunTest((engine, compilation) => {
				var a = engine.CreateAttribute(compilation.ReferencedAssemblies[1], typeof(ComplexAttribute).FullName, new[] { Tuple.Create(compilation.FindType(KnownTypeCode.Int16), (object)(short)352), Tuple.Create(compilation.FindType(KnownTypeCode.String), (object)null) }, null);
				var attrType = ReflectionHelper.ParseReflectionName(typeof(ComplexAttribute).FullName).Resolve(compilation);
				Assert.That(a.AttributeType, Is.EqualTo(attrType));
				Assert.That(a.Constructor, Is.EqualTo(attrType.GetConstructors().Single(c => c.Parameters.Count == 2 && c.Parameters[0].Type == compilation.FindType(KnownTypeCode.Int32) && c.Parameters[1].Type == compilation.FindType(KnownTypeCode.String))));
				Assert.That(a.PositionalArguments[0].Type, Is.EqualTo(compilation.FindType(KnownTypeCode.Int32)));
				Assert.That(a.PositionalArguments[0].ConstantValue, Is.InstanceOf<int>());
				Assert.That(a.PositionalArguments[0].ConstantValue, Is.EqualTo(352));
				Assert.That(a.PositionalArguments[1].Type, Is.EqualTo(compilation.FindType(KnownTypeCode.String)));
				Assert.That(a.PositionalArguments[1].ConstantValue, Is.Null);
			});
		}

		[Test]
		public void CreateAttributeCanPerformImplicitConversionOnNamedArguments() {
			RunTest((engine, compilation) => {
				var a = engine.CreateAttribute(compilation.ReferencedAssemblies[1], typeof(ComplexAttribute).FullName, null, new[] { Tuple.Create("Property2", (object)(short)567) });
				var attrType = ReflectionHelper.ParseReflectionName(typeof(ComplexAttribute).FullName).Resolve(compilation);
				Assert.That(a.AttributeType, Is.EqualTo(attrType));
				Assert.That(a.Constructor, Is.EqualTo(attrType.GetConstructors().Single(c => c.Parameters.Count == 0)));
				Assert.That(a.PositionalArguments, Is.Empty);

				var namedArgs = a.NamedArguments.ToDictionary(na => na.Key.Name);
				Assert.That(namedArgs.Keys, Is.EquivalentTo(new[] { "Property2" }));
				Assert.That(namedArgs["Property2"].Key, Is.EqualTo(attrType.GetProperties().Single(p => p.Name == "Property2")));
				Assert.That(namedArgs["Property2"].Value.Type, Is.EqualTo(compilation.FindType(KnownTypeCode.Int32)));
				Assert.That(namedArgs["Property2"].Value.ConstantValue, Is.InstanceOf<int>());
				Assert.That(namedArgs["Property2"].Value.ConstantValue, Is.EqualTo(567));
			});
		}

		[Test]
		public void WritingTheModifiedAssemblyWorks() {
			try {
				RunTest((engine, compilation, assembly) => {
					var a1 = engine.CreateAttribute(compilation.ReferencedAssemblies[1], typeof(ComplexAttribute).FullName, new[] { Tuple.Create(compilation.FindType(KnownTypeCode.Int32), (object)352), Tuple.Create(compilation.FindType(KnownTypeCode.String), (object)"Class attribute") }, new[] { Tuple.Create("Property2", (object)567), Tuple.Create("Field1", (object)(byte)34) });
					var typeAttrs = engine.GetAttributes((ITypeDefinition)ReflectionHelper.ParseReflectionName(typeof(UnattributedClass).FullName).Resolve(compilation));
					typeAttrs.Add(a1);

					var a2 = engine.CreateAttribute(compilation.ReferencedAssemblies[1], typeof(ComplexAttribute).FullName, new[] { Tuple.Create(compilation.FindType(KnownTypeCode.Byte), (object)(byte)34), Tuple.Create(compilation.FindType(KnownTypeCode.String), (object)"Method attribute") }, new[] { Tuple.Create("Property2", (object)2), Tuple.Create("Field2", (object)"String value") });
					var methodAttrs = engine.GetAttributes(ReflectionHelper.ParseReflectionName(typeof(UnattributedClass).FullName).Resolve(compilation).GetMethods(options: GetMemberOptions.IgnoreInheritedMembers).Single());
					methodAttrs.Add(a2);

					engine.Apply();

					assembly.Write(Path.GetFullPath("Written.dll"));

					var loadedAsm = AssemblyDefinition.ReadAssembly(Path.GetFullPath("Written.dll"));
					var loadedClass = loadedAsm.Modules.SelectMany(m => m.Types).Single(t => t.FullName == typeof(UnattributedClass).FullName);
					var loadedMethod = loadedClass.Methods.Single(m => !m.IsConstructor);

					Assert.That(loadedClass.CustomAttributes, Has.Count.EqualTo(1));
					var classAttr = loadedClass.CustomAttributes[0];
					Assert.That(classAttr.AttributeType.FullName, Is.EqualTo(typeof(ComplexAttribute).FullName));
					Assert.That(classAttr.Constructor.Parameters.Count, Is.EqualTo(2));
					Assert.That(classAttr.ConstructorArguments.Select(arg => arg.Value), Is.EqualTo(new object[] { 352, "Class attribute" }));
					Assert.That(classAttr.Properties.Select(arg => new { arg.Name, arg.Argument.Value }), Is.EqualTo(new[] { new { Name = "Property2", Value = (object)567 } }));
					Assert.That(classAttr.Fields.Select(arg => new { arg.Name, arg.Argument.Value }), Is.EqualTo(new[] { new { Name = "Field1", Value = (object)(byte)34 } }));

					Assert.That(loadedMethod.CustomAttributes, Has.Count.EqualTo(1));
					var methodAttr = loadedMethod.CustomAttributes[0];
					Assert.That(methodAttr.AttributeType.FullName, Is.EqualTo(typeof(ComplexAttribute).FullName));
					Assert.That(methodAttr.Constructor.Parameters.Count, Is.EqualTo(2));
					Assert.That(methodAttr.ConstructorArguments.Select(arg => arg.Value), Is.EqualTo(new object[] { 34, "Method attribute" }));
					Assert.That(methodAttr.Properties.Select(arg => new { arg.Name, arg.Argument.Value }), Is.EqualTo(new[] { new { Name = "Property2", Value = (object)2 } }));
					Assert.That(methodAttr.Fields.Select(arg => new { arg.Name, arg.Argument.Value }), Is.EqualTo(new[] { new { Name = "Field2", Value = (object)"String value" } }));
				});
			}
			finally {
				try { File.Delete(Path.GetFullPath("Written.dll")); } catch {}
			}
		}
	}
}
