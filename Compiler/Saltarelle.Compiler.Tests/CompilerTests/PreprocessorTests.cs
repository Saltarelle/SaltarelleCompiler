using System.Linq;
using NUnit.Framework;
using FluentAssertions;

namespace Saltarelle.Compiler.Tests.CompilerTests {
    [TestFixture]
    public class PreprocessorTests : CompilerTestBase {
        [Test]
        public void TakenIfWorks() {
            Compile(new[] {
@"
class A {}
#if true
class B {}
#endif
" });
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] { "A", "B" });
        }

        [Test]
        public void NotTakenIfWorks() {
            Compile(new[] {
@"
class A {}
#if false
class B {}
#endif
" });
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] { "A" });
        }

        [Test]
        public void TakenElifWorks() {
            Compile(new[] {
@"
class A {}
#if false
class B {}
#elif true
class C {}
#endif
" });
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] { "A", "C" });
        }

        [Test]
        public void NotTakenElifWorks() {
            Compile(new[] {
@"
class A {}
#if true
class B {}
#elif true
class C {}
#elif false
class D {}
#endif
" });
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] { "A", "B" });
        }

        [Test]
        public void TakenElseWorks() {
            Compile(new[] {
@"
class A {}
#if false
class B {}
#else
class C {}
#endif
" });
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] { "A", "C" });
        }

        [Test]
        public void TakenElseWorksWithElif() {
            Compile(new[] {
@"
class A {}
#if false
class B {}
#elif false
class C {}
#else
class D {}
#endif
" });
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] { "A", "D" });
        }

        [Test]
        public void NotTakenElseWorks() {
            Compile(new[] {
@"
class A {}
#if true
class B {}
#else
class C {}
#endif
" });
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] { "A", "B" });
        }

        [Test]
        public void PassingDefineConstantsWorks() {
            Compile(new[] {
@"
class A {}
#if MY_SYMBOL1
class B {}
#endif
#if MY_SYMBOL2
class B {}
#endif
" }, defineConstants: new[] { "MY_SYMBOL1" });
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] { "A", "B" });
        }

        [Test]
        public void DefineWorks() {
            Compile(new[] {
@"
#define MY_SYMBOL
class A {}
#if MY_SYMBOL
class B {}
#endif
" });
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] { "A", "B" });
        }

        [Test]
        public void UndefWorks() {
            Compile(new[] {
@"
#undef MY_SYMBOL
class A {}
#if MY_SYMBOL
class B {}
#endif
" }, defineConstants: new[] { "MY_SYMBOL" });
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] { "A" });
        }
    }
}
