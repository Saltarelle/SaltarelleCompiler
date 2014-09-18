using System.Linq;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
#warning TODO
#if false

	[TestFixture]
	public class NestedFunctionGathererTests : MethodCompilerTestBase {
		[Test]
		public void TheGatheredStructureIsCorrect() {
			CompileMethod(
@"public void M() {
	Func<int> f = () => {
		Func<string, double> f2 = delegate(string s) {
			Func<int, int, int> f3 = (int i, int j) => (i + j);
			Action<double> a1 = x => {};
			return f3(1, 4);
		};
		Action<string> f4 = s2 => {
			Func<int, string> f5 = y => y.ToString();
		};
		return 0;
	};
	Action<Type> a = delegate {};
}");

			// Verify nested function counts
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions, Has.Count.EqualTo(2));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions, Has.Count.EqualTo(2));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions, Has.Count.EqualTo(2));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[0].NestedFunctions, Is.Empty);
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[1].NestedFunctions, Is.Empty);
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].NestedFunctions, Has.Count.EqualTo(1));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].NestedFunctions[0].NestedFunctions, Is.Empty);
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[1].NestedFunctions, Is.Empty);

			// Verify definitions
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].DefinitionNode.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(1, 15)));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].DefinitionNode.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(2, 28)));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[0].DefinitionNode.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(3, 28)));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[1].DefinitionNode.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(4, 23)));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].DefinitionNode.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(7, 22)));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].NestedFunctions[0].DefinitionNode.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(8, 26)));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[1].DefinitionNode.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(12, 18)));

			// Verify parents
			Assert.That(MethodCompiler.nestedFunctionsRoot.Parent, Is.Null);
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].Parent, Is.SameAs(MethodCompiler.nestedFunctionsRoot));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].Parent, Is.SameAs(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0]));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[0].Parent, Is.SameAs(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0]));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[1].Parent, Is.SameAs(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0]));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].Parent, Is.SameAs(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0]));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].NestedFunctions[0].Parent, Is.SameAs(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1]));
			Assert.That(MethodCompiler.nestedFunctionsRoot.NestedFunctions[1].Parent, Is.SameAs(MethodCompiler.nestedFunctionsRoot));
		}

		[Test]
		public void ReferencedVariablesGatheringIsCorrect() {
			CompileMethod(
@"public void M(int a1, int a2) {
	int a3 = a1 + a2;
	Func<int> f = () => {
		Func<int, int> f2 = delegate(int a4) {
			return a4 + a2;
		};
		return f2(a1) + a3;
	};
}");

			var f = MethodCompiler.nestedFunctionsRoot.NestedFunctions[0];
			var f2 = f.NestedFunctions[0];
			
			Assert.That(f.DirectlyUsedVariables.Select(v => MethodCompiler.variables[v].Name).ToList(), Is.EquivalentTo(new[] { "$a1", "$a3", "$f2" }));
			Assert.That(f2.DirectlyUsedVariables.Select(v => MethodCompiler.variables[v].Name).ToList(), Is.EquivalentTo(new[] { "$a2", "$a4" }));
		}

		[Test]
		public void DeclaredVariablesGatheringIsCorrect() {
			CompileMethod(
@"public void M(int a1, int a2) {
	int a3 = a1 + a2;
	Func<int> f = () => {
		Func<int, int> f2 = delegate(int a4) {
			return a4 + a2;
		};
		return f2(a1) + a3;
	};
}");

			var r = MethodCompiler.nestedFunctionsRoot;
			var f = r.NestedFunctions[0];
			var f2 = f.NestedFunctions[0];
			
			Assert.That(r.DirectlyDeclaredVariables.Select(v => MethodCompiler.variables[v].Name).ToList(), Is.EquivalentTo(new[] { "$a1", "$a2", "$a3", "$f" }));
			Assert.That(f.DirectlyDeclaredVariables.Select(v => MethodCompiler.variables[v].Name).ToList(), Is.EquivalentTo(new[] { "$f2" }));
			Assert.That(f2.DirectlyDeclaredVariables.Select(v => MethodCompiler.variables[v].Name).ToList(), Is.EquivalentTo(new[] { "$a4" }));
		}

		[Test]
		public void InstanceFieldReferenceIsConsideredToUseThis() {
			CompileMethod(
@"private int i;
public void M() {
	Func<int> f = () => {
		return i;
	};
}");

			var f = MethodCompiler.nestedFunctionsRoot.NestedFunctions[0];
			Assert.That(f.DirectlyUsesThis, Is.True);
		}

		[Test]
		public void StaticFieldReferenceIsNotConsideredToUseThis() {
			CompileMethod(
@"private static int i;
public void M() {
	Func<int> f = () => {
		return i;
	};
}");

			var f = MethodCompiler.nestedFunctionsRoot.NestedFunctions[0];
			Assert.That(f.DirectlyUsesThis, Is.False);
		}

		[Test]
		public void ThisQualifiedReferenceIsConsideredToUseThis() {
			CompileMethod(
@"private int i;
public void M() {
	Func<int> f = () => {
		return this.i;
	};
}");

			var f = MethodCompiler.nestedFunctionsRoot.NestedFunctions[0];
			Assert.That(f.DirectlyUsesThis, Is.True);
		}

		[Test]
		public void BaseQualifiedReferenceIsConsideredToUseThis() {
			CompileMethod(
@" class B { public int F() { return 0; } }
class D : B {
	public void M() {
		System.Func<int> f = () => {
			return base.F();
		};
	}
}", addSkeleton: false);

			var f = MethodCompiler.nestedFunctionsRoot.NestedFunctions[0];
			Assert.That(f.DirectlyUsesThis, Is.True);
		}
	}
#endif
}
