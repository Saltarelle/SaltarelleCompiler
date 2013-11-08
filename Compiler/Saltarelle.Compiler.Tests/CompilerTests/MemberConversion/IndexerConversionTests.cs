﻿using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MemberConversion {
	[TestFixture]
	public class IndexerConversionTests : CompilerTestBase {
		[Test]
		public void IndexerThatIsNotUsableFromScriptIsNotImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.NotUsableFromScript() };
			Compile(new[] { "class C { public int this[int i] { get {} set {} } }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void IndexerWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
			Compile(new[] { "class C { public int this[int i] { get {} set {} } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.get_Item").Should().NotBeNull();
			FindInstanceMethod("C.set_Item").Should().NotBeNull();
		}

		[Test]
		public void IndexerAccessorsInInterfaceHaveNullDefinition() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
			Compile(new[] { "interface I { int this[int i] { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("I.get_Item").Should().NotBeNull();
			FindInstanceMethod("I.set_Item").Should().NotBeNull();
		}

		[Test]
		public void IndexerWithGetAndSetMethodsWithNoCodeIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item", generateCode: false), MethodScriptSemantics.NormalMethod("set_Item", generateCode: false)) };
			Compile(new[] { "class C { public int this[int i] { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.get_Item").Should().BeNull();
			FindInstanceMethod("C.set_Item").Should().BeNull();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void NativeIndexerIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.NativeIndexer() };
			Compile(new[] { "class C { public int this[int i] { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void ReadOnlyIndexerWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
			Compile(new[] { "class C { public int this[int i] { get { return 0; } } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.get_Item").Should().NotBeNull();
			FindInstanceMethod("C.set_Item").Should().BeNull();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void ReadOnlyNativeIndexerIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.NativeIndexer() };
			Compile(new[] { "class C { public int this[int i] { get { return 0; } } }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void WriteOnlyIndexerWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
			Compile(new[] { "class C { public int this[int i] { set {} } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.get_Item").Should().BeNull();
			FindInstanceMethod("C.set_Item").Should().NotBeNull();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void WriteOnlyNativeIndexerIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.NativeIndexer() };
			Compile(new[] { "class C { public int this[int i] { set {} } }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void AbstractIndexerHasANullDefinition() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
			Compile(new[] { "abstract class C { public abstract int this[int i] { get; set; } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.get_Item").Definition.Should().BeNull();
			FindInstanceMethod("C.set_Item").Definition.Should().BeNull();
		}
	}
}
