using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	[TestFixture]
	public class LocalUsageGathererTests {
		private Tuple<SyntaxNode, SemanticModel> Compile(string source, bool addSkeleton = true) {
			if (addSkeleton)
				source = "using System; class C { " + source + " }";
			var c = Common.CreateCompilation(new[] { source });
			var st = c.SyntaxTrees.Single();
			return Tuple.Create(st.GetRoot(), c.GetSemanticModel(st));
		}

		[Test]
		public void ReferencedVariablesGatheringIsCorrect() {
			var c = Compile(
@"public void M(int a1, int a2) {
	int a3 = a1 + a2;
	Func<int> f = () => {
		Func<int, int> f2 = delegate(int a4) {
			return a4 + a2;
		};
		return f2(a1) + a3;
	};
}");
			var f = LocalUsageGatherer.GatherInfo(c.Item2, c.Item1.DescendantNodes().OfType<VariableDeclarationSyntax>().Single(d => d.Variables[0].Identifier.Text == "f"));
			Assert.That(f.DirectlyOrIndirectlyUsedLocals.Select(v => v.Name).ToList(), Is.EquivalentTo(new[] { "a1", "a2", "a3", "a4", "f2" }));
			Assert.That(f.DirectlyOrIndirectlyUsesThis, Is.False);

			var f2 = LocalUsageGatherer.GatherInfo(c.Item2, c.Item1.DescendantNodes().OfType<VariableDeclarationSyntax>().Single(d => d.Variables[0].Identifier.Text == "f2"));
			Assert.That(f2.DirectlyOrIndirectlyUsedLocals.Select(v => v.Name).ToList(), Is.EquivalentTo(new[] { "a2", "a4" }));
			Assert.That(f2.DirectlyOrIndirectlyUsesThis, Is.False);
		}

		[Test]
		public void InstanceFieldReferenceIsConsideredToUseThis() {
			var c = Compile(
@"private int i;
public void M() {
	Func<int> f = () => {
		return i;
	};
}");

			var f = LocalUsageGatherer.GatherInfo(c.Item2, c.Item1.DescendantNodes().OfType<VariableDeclarationSyntax>().Single(d => d.Variables[0].Identifier.Text == "f"));
			Assert.That(f.DirectlyOrIndirectlyUsesThis, Is.True);
		}

		[Test]
		public void InstancePropertyReferenceIsConsideredToUseThis() {
			var c = Compile(
@"private int i { get; set; }
public void M() {
	Func<int> f = () => {
		return i;
	};
}");

			var f = LocalUsageGatherer.GatherInfo(c.Item2, c.Item1.DescendantNodes().OfType<VariableDeclarationSyntax>().Single(d => d.Variables[0].Identifier.Text == "f"));
			Assert.That(f.DirectlyOrIndirectlyUsesThis, Is.True);
		}

		[Test]
		public void InstanceMethodInvocationIsConsideredToUseThis() {
			var c = Compile(
@"private void F() {}
public void M() {
	Func<int> f = () => {
		F();
		return 0;
	};
}");

			var f = LocalUsageGatherer.GatherInfo(c.Item2, c.Item1.DescendantNodes().OfType<VariableDeclarationSyntax>().Single(d => d.Variables[0].Identifier.Text == "f"));
			Assert.That(f.DirectlyOrIndirectlyUsesThis, Is.True);
		}

		[Test]
		public void InstanceMethodGroupConversionIsConsideredToUseThis() {
			var c = Compile(
@"private void F() {}
public void M() {
	Func<int> f = () => {
		F();
		return 0;
	};
}");

			var f = LocalUsageGatherer.GatherInfo(c.Item2, c.Item1.DescendantNodes().OfType<VariableDeclarationSyntax>().Single(d => d.Variables[0].Identifier.Text == "f"));
			Assert.That(f.DirectlyOrIndirectlyUsesThis, Is.True);
		}

		[Test]
		public void InstanceGenericMethodGroupConversionIsConsideredToUseThis() {
			var c = Compile(
@"private void F<T>() {}
public void M() {
	Func<int> f = () => {
		Action a = F<int>;
		return 0;
	};
}");

			var f = LocalUsageGatherer.GatherInfo(c.Item2, c.Item1.DescendantNodes().OfType<VariableDeclarationSyntax>().Single(d => d.Variables[0].Identifier.Text == "f"));
			Assert.That(f.DirectlyOrIndirectlyUsesThis, Is.True);
		}

		[Test]
		public void InstanceEventReferenceIsConsideredToUseThis() {
			var c = Compile(
@"private event Action e;
public void M() {
	Func<int> f = () => {
		e();
		return 0;
	};
}");

			var f = LocalUsageGatherer.GatherInfo(c.Item2, c.Item1.DescendantNodes().OfType<VariableDeclarationSyntax>().Single(d => d.Variables[0].Identifier.Text == "f"));
			Assert.That(f.DirectlyOrIndirectlyUsesThis, Is.True);
		}

		[Test]
		public void ConstructorInvocationIsNotConsideredToUseThis() {
			var c = Compile(
@"private static int i;
public void M() {
	Func<C> f = () => {
		return new C();
	};
}");

			var f = LocalUsageGatherer.GatherInfo(c.Item2, c.Item1.DescendantNodes().OfType<VariableDeclarationSyntax>().Single(d => d.Variables[0].Identifier.Text == "f"));
			Assert.That(f.DirectlyOrIndirectlyUsesThis, Is.False);
		}

		[Test]
		public void StaticReferenceIsNotConsideredToUseThis() {
			var c = Compile(
@"private static int i;
private static event Action e;
private static int P { get; set; }
private static void F1() {}
private static void F2<T>() {}
public void M() {
	Func<int> f = () => {
		var x1 = P;
		e();
		Action a1 = F1;
		F1();
		Action a = F2<int>;
		F2<int>();
		return i;
	};
}");

			var f = LocalUsageGatherer.GatherInfo(c.Item2, c.Item1.DescendantNodes().OfType<VariableDeclarationSyntax>().Single(d => d.Variables[0].Identifier.Text == "f"));
			Assert.That(f.DirectlyOrIndirectlyUsesThis, Is.False);
		}

		[Test]
		public void ThisQualifiedReferenceIsConsideredToUseThis() {
			var c = Compile(
@"private int i;
public void M() {
	Func<int> f = () => {
		return this.i;
	};
}");

			var f = LocalUsageGatherer.GatherInfo(c.Item2, c.Item1.DescendantNodes().OfType<VariableDeclarationSyntax>().Single(d => d.Variables[0].Identifier.Text == "f"));
			Assert.That(f.DirectlyOrIndirectlyUsesThis, Is.True);
		}

		[Test]
		public void BaseQualifiedReferenceIsConsideredToUseThis() {
			var c = Compile(
@" class B { public int F() { return 0; } }
class D : B {
	public void M() {
		System.Func<int> f = () => {
			return base.F();
		};
	}
}", addSkeleton: false);

			var f = LocalUsageGatherer.GatherInfo(c.Item2, c.Item1.DescendantNodes().OfType<VariableDeclarationSyntax>().Single(d => d.Variables[0].Identifier.Text == "f"));
			Assert.That(f.DirectlyOrIndirectlyUsesThis, Is.True);
		}
	}
}
