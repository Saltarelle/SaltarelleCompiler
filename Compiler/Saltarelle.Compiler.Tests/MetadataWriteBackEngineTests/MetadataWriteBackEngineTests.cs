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

		private Tuple<IMetadataWriteBackEngine, ICompilation> Prepare() {
            IProjectContent project = new CSharpProjectContent();

            project = project.AddAssemblyReferences(new[] { Common.Mscorlib, CurrentAsm });

			var compilation = project.CreateCompilation();

			var asm = AssemblyDefinition.ReadAssembly(typeof(MetadataWriteBackEngineTests).Assembly.Location);
			var eng = new CecilMetadataWriteBackEngine(asm);

			return Tuple.Create((IMetadataWriteBackEngine)eng, compilation);
		}

		[Test]
		public void CanGetAttributesOfType() {
			var p = Prepare();
			var attrs = p.Item1.GetAttributes((ITypeDefinition)ReflectionHelper.ParseReflectionName("Saltarelle.Compiler.Tests.MetadataWriteBackEngineTests.MetadataWriteBackEngineTestCase.ClassWithAttribute").Resolve(p.Item2));
			Assert.That(attrs.Count, Is.EqualTo(1));
			var attr = attrs.ElementAt(0);
			Assert.That(attr.AttributeType, Is.EqualTo(ReflectionHelper.ParseReflectionName(typeof(ObsoleteAttribute).FullName).Resolve(p.Item2)));
			Assert.That(attr.PositionalArguments, Is.EqualTo(new[] { "This class has an attribute" }));
		}

		[Test]
		public void CanGetAttributesOfField() {
			Assert.Fail("TODO");
		}

		[Test]
		public void CanGetAttributesOfProperty() {
			Assert.Fail("TODO");
		}

		[Test]
		public void CanGetAttributesOfPropertyWhichIsAnExplicitInterfaceImplementation() {
			Assert.Fail("TODO");
		}

		[Test]
		public void CanGetAttributesOfPropertyGetter() {
			Assert.Fail("TODO");
		}

		[Test]
		public void CanGetAttributesOfPropertySetter() {
			Assert.Fail("TODO");
		}

		[Test]
		public void CanGetAttributesOfIndexer() {
			Assert.Fail("TODO, including overloads");
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
	}
}
