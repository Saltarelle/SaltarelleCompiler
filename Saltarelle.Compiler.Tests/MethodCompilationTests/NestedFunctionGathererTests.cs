using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;
using NUnit.Framework;
using FluentAssertions;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests {

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
            MethodCompiler.nestedFunctions.Should().HaveCount(2);
            MethodCompiler.nestedFunctions[0].NestedFunctions.Should().HaveCount(2);
            MethodCompiler.nestedFunctions[0].NestedFunctions[0].NestedFunctions.Should().HaveCount(2);
            MethodCompiler.nestedFunctions[0].NestedFunctions[0].NestedFunctions[0].NestedFunctions.Should().HaveCount(0);
            MethodCompiler.nestedFunctions[0].NestedFunctions[0].NestedFunctions[1].NestedFunctions.Should().HaveCount(0);
            MethodCompiler.nestedFunctions[0].NestedFunctions[1].NestedFunctions.Should().HaveCount(1);
            MethodCompiler.nestedFunctions[0].NestedFunctions[1].NestedFunctions[0].NestedFunctions.Should().HaveCount(0);
            MethodCompiler.nestedFunctions[1].NestedFunctions.Should().HaveCount(0);

            // Verify definitions
            MethodCompiler.nestedFunctions[0].DefinitionNode.StartLocation.Should().Be(new TextLocation(2, 19));
            MethodCompiler.nestedFunctions[0].NestedFunctions[0].DefinitionNode.StartLocation.Should().Be(new TextLocation(3, 35));
            MethodCompiler.nestedFunctions[0].NestedFunctions[0].NestedFunctions[0].DefinitionNode.StartLocation.Should().Be(new TextLocation(4, 38));
            MethodCompiler.nestedFunctions[0].NestedFunctions[0].NestedFunctions[1].DefinitionNode.StartLocation.Should().Be(new TextLocation(5, 33));
            MethodCompiler.nestedFunctions[0].NestedFunctions[1].DefinitionNode.StartLocation.Should().Be(new TextLocation(8, 29));
            MethodCompiler.nestedFunctions[0].NestedFunctions[1].NestedFunctions[0].DefinitionNode.StartLocation.Should().Be(new TextLocation(9, 36));
            MethodCompiler.nestedFunctions[1].DefinitionNode.StartLocation.Should().Be(new TextLocation(13, 22));

            // Verify bodies
            MethodCompiler.nestedFunctions[0].BodyNode.StartLocation.Should().Be(new TextLocation(2, 25));
            MethodCompiler.nestedFunctions[0].NestedFunctions[0].BodyNode.StartLocation.Should().Be(new TextLocation(3, 54));
            MethodCompiler.nestedFunctions[0].NestedFunctions[0].NestedFunctions[0].BodyNode.StartLocation.Should().Be(new TextLocation(4, 56));
            MethodCompiler.nestedFunctions[0].NestedFunctions[0].NestedFunctions[1].BodyNode.StartLocation.Should().Be(new TextLocation(5, 38));
            MethodCompiler.nestedFunctions[0].NestedFunctions[1].BodyNode.StartLocation.Should().Be(new TextLocation(8, 35));
            MethodCompiler.nestedFunctions[0].NestedFunctions[1].NestedFunctions[0].BodyNode.StartLocation.Should().Be(new TextLocation(9, 41));
            MethodCompiler.nestedFunctions[1].BodyNode.StartLocation.Should().Be(new TextLocation(13, 31));

            // Verify resolve results
            MethodCompiler.nestedFunctions[0].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new string[0]);
            MethodCompiler.nestedFunctions[0].NestedFunctions[0].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new[] { "s" });
            MethodCompiler.nestedFunctions[0].NestedFunctions[0].NestedFunctions[0].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new[] { "i", "j" });
            MethodCompiler.nestedFunctions[0].NestedFunctions[0].NestedFunctions[1].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new[] { "x" });
            MethodCompiler.nestedFunctions[0].NestedFunctions[1].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new[] { "s2" });
            MethodCompiler.nestedFunctions[0].NestedFunctions[1].NestedFunctions[0].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new[] { "y" });
            MethodCompiler.nestedFunctions[1].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new string[0]);
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

            var f = MethodCompiler.nestedFunctions[0];
            var f2 = f.NestedFunctions[0];

            Assert.That(f.DirectlyUsedVariables.Select(v => v.Name).ToList(), Is.EquivalentTo(new[] { "a1", "a3", "f2" }));
            Assert.That(f2.DirectlyUsedVariables.Select(v => v.Name).ToList(), Is.EquivalentTo(new[] { "a2", "a4" }));
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

            var f = MethodCompiler.nestedFunctions[0];
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

            var f = MethodCompiler.nestedFunctions[0];
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

            var f = MethodCompiler.nestedFunctions[0];
            Assert.That(f.DirectlyUsesThis, Is.True);
        }

        [Test]
        public void BaseQualifiedReferenceIsConsideredToUseThis() {
            CompileMethod(
@"public void M() {
    Func<int> f = () => {
        return base.GetHashCode();
    };
}");

            var f = MethodCompiler.nestedFunctions[0];
            Assert.That(f.DirectlyUsesThis, Is.True);
        }
	}
}
