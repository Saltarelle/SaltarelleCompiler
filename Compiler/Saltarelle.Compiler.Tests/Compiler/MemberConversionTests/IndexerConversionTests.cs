using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.Compiler.MemberConversionTests {
    [TestFixture]
    public class IndexerConversionTests : CompilerTestBase {
        [Test]
        public void IndexerThatIsNotUsableFromScriptIsNotImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertySemantics = p => PropertyScriptSemantics.NotUsableFromScript() };
            Compile(new[] { "class C { public int this[int i] { get {} set {} } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void IndexerWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
            Compile(new[] { "class C { public int this[int i] { get {} set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_Item").Should().NotBeNull();
            FindInstanceMethod("C.set_Item").Should().NotBeNull();
        }

		[Test]
		public void IndexerAccessorsInInterfaceHaveNullDefinition() {
            var namingConvention = new MockNamingConventionResolver { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
            Compile(new[] { "interface I { int this[int i] { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("I.get_Item").Should().NotBeNull();
            FindInstanceMethod("I.set_Item").Should().NotBeNull();
		}

        [Test]
        public void IndexerWithGetAndSetMethodsWithNoCodeIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item", generateCode: false), MethodScriptSemantics.NormalMethod("set_Item", generateCode: false)) };
            Compile(new[] { "class C { public int this[int i] { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_Item").Should().BeNull();
            FindInstanceMethod("C.set_Item").Should().BeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void NativeIndexerIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertySemantics = p => PropertyScriptSemantics.NativeIndexer() };
            Compile(new[] { "class C { public int this[int i] { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ReadOnlyIndexerWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
            Compile(new[] { "class C { public int this[int i] { get { return 0; } } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_Item").Should().NotBeNull();
            FindInstanceMethod("C.set_Item").Should().BeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ReadOnlyNativeIndexerIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertySemantics = p => PropertyScriptSemantics.NativeIndexer() };
            Compile(new[] { "class C { public int this[int i] { get { return 0; } } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void WriteOnlyIndexerWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
            Compile(new[] { "class C { public int this[int i] { set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_Item").Should().BeNull();
            FindInstanceMethod("C.set_Item").Should().NotBeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void WriteOnlyNativeIndexerIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertySemantics = p => PropertyScriptSemantics.NativeIndexer() };
            Compile(new[] { "class C { public int this[int i] { set {} } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

		[Test]
		public void AbstractIndexerHasANullDefinition() {
            var namingConvention = new MockNamingConventionResolver { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_Item"), MethodScriptSemantics.NormalMethod("set_Item")) };
            Compile(new[] { "abstract class C { public abstract int this[int i] { get; set; } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_Item").Definition.Should().BeNull();
            FindInstanceMethod("C.set_Item").Definition.Should().BeNull();
		}
    }
}
