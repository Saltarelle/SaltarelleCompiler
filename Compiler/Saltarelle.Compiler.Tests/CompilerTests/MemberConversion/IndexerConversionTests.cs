using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MemberConversion {
	[TestFixture]
	public class IndexerConversionTests : CompilerTestBase {
		[Test]
		public void IndexerThatIsNotUsableFromScriptIsNotImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.NotUsableFromScript() };
			Compile(new[] { "class C { public int this[int i] { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void IndexerWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
			Compile(new[] { "class C { public int this[int i] { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.get_Item"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.set_Item"), Is.Not.Null);
		}

		[Test]
		public void IndexerAccessorsInInterfaceHaveNullDefinition() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
			Compile(new[] { "interface I { int this[int i] { get; set; } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("I.get_Item"), Is.Not.Null);
			Assert.That(FindInstanceMethod("I.set_Item"), Is.Not.Null);
		}

		[Test]
		public void IndexerWithGetAndSetMethodsWithNoCodeIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item", generateCode: false), MethodScriptSemantics.NormalMethod("set_Item", generateCode: false)) };
			Compile(new[] { "class C { public int this[int i] { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.get_Item"), Is.Null);
			Assert.That(FindInstanceMethod("C.set_Item"), Is.Null);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void NativeIndexerIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.NativeIndexer() };
			Compile(new[] { "class C { public int this[int i] { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void ReadOnlyIndexerWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
			Compile(new[] { "class C { public int this[int i] { get { return 0; } } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.get_Item"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.set_Item"), Is.Null);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void ReadOnlyNativeIndexerIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.NativeIndexer() };
			Compile(new[] { "class C { public int this[int i] { get { return 0; } } }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void WriteOnlyIndexerWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
			Compile(new[] { "class C { public int this[int i] { set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.get_Item"), Is.Null);
			Assert.That(FindInstanceMethod("C.set_Item"), Is.Not.Null);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void WriteOnlyNativeIndexerIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.NativeIndexer() };
			Compile(new[] { "class C { public int this[int i] { set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void AbstractIndexerHasANullDefinition() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
			Compile(new[] { "abstract class C { public abstract int this[int i] { get; set; } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.get_Item").Definition, Is.Null);
			Assert.That(FindInstanceMethod("C.set_Item").Definition, Is.Null);
		}
	}
}
