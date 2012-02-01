using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MemberConversionTests {
    [TestFixture]
    public class IndexerConversionTests : CompilerTestBase {
        [Test]
        public void IndexerThatIsNotUsableFromScriptIsNotImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.NotUsableFromScript() };
            Compile(new[] { "class C { public int this[int i] { get {} set {} } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void IndexerWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InstanceMethod("get_SomeProp"), MethodImplOptions.StaticMethod("set_SomeProp", addThisAsFirstArgument: true)) };
            Compile(new[] { "class C { public int this[int i] { get {} set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
        }

        [Test]
        public void IndexerWithGetAndSetMethodsWithNoCodeIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InstanceMethod("get_SomeProp", generateCode: false), MethodImplOptions.StaticMethod("set_SomeProp", generateCode: false)) };
            Compile(new[] { "class C { public int this[int i] { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().BeNull();
            FindInstanceMethod("C.set_SomeProp").Should().BeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void NativeIndexerIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.NativeIndexer() };
            Compile(new[] { "class C { public int this[int i] { get { return 0; } set {} } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ReadOnlyIndexerWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InstanceMethod("get_SomeProp"), MethodImplOptions.InstanceMethod("set_SomeProp")) };
            Compile(new[] { "class C { public int this[int i] { get { return 0; } } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
            FindInstanceMethod("C.set_SomeProp").Should().BeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ReadOnlyNativeIndexerIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.NativeIndexer() };
            Compile(new[] { "class C { public int this[int i] { get { return 0; } } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void WriteOnlyIndexerWithGetAndSetMethodsIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InstanceMethod("get_SomeProp"), MethodImplOptions.InstanceMethod("set_SomeProp")) };
            Compile(new[] { "class C { public int this[int i] { set {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.get_SomeProp").Should().BeNull();
            FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void WriteOnlyNativeIndexerIsCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.NativeIndexer() };
            Compile(new[] { "class C { public int this[int i] { set {} } }" }, namingConvention: namingConvention);
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }
    }
}
