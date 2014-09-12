using System.Linq;
using NUnit.Framework;

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
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.Name), Is.EquivalentTo(new[] { "A", "B" }));
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
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.Name), Is.EquivalentTo(new[] { "A" }));
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
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.Name), Is.EquivalentTo(new[] { "A", "C" }));
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
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.Name), Is.EquivalentTo(new[] { "A", "B" }));
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
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.Name), Is.EquivalentTo(new[] { "A", "C" }));
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
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.Name), Is.EquivalentTo(new[] { "A", "D" }));
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
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.Name), Is.EquivalentTo(new[] { "A", "B" }));
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
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.Name), Is.EquivalentTo(new[] { "A", "B" }));
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
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.Name), Is.EquivalentTo(new[] { "A", "B" }));
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
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.Name), Is.EquivalentTo(new[] { "A" }));
		}
	}
}
