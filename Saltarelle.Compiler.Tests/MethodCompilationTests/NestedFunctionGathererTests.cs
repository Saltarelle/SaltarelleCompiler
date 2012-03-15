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
            MethodCompiler.nestedFunctionsRoot.NestedFunctions.Should().HaveCount(2);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions.Should().HaveCount(2);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions.Should().HaveCount(2);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[0].NestedFunctions.Should().HaveCount(0);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[1].NestedFunctions.Should().HaveCount(0);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].NestedFunctions.Should().HaveCount(1);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].NestedFunctions[0].NestedFunctions.Should().HaveCount(0);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[1].NestedFunctions.Should().HaveCount(0);

            // Verify definitions
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].DefinitionNode.StartLocation.Should().Be(new TextLocation(2, 19));
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].DefinitionNode.StartLocation.Should().Be(new TextLocation(3, 35));
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[0].DefinitionNode.StartLocation.Should().Be(new TextLocation(4, 38));
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[1].DefinitionNode.StartLocation.Should().Be(new TextLocation(5, 33));
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].DefinitionNode.StartLocation.Should().Be(new TextLocation(8, 29));
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].NestedFunctions[0].DefinitionNode.StartLocation.Should().Be(new TextLocation(9, 36));
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[1].DefinitionNode.StartLocation.Should().Be(new TextLocation(13, 22));

            // Verify bodies
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].BodyNode.StartLocation.Should().Be(new TextLocation(2, 25));
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].BodyNode.StartLocation.Should().Be(new TextLocation(3, 54));
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[0].BodyNode.StartLocation.Should().Be(new TextLocation(4, 56));
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[1].BodyNode.StartLocation.Should().Be(new TextLocation(5, 38));
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].BodyNode.StartLocation.Should().Be(new TextLocation(8, 35));
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].NestedFunctions[0].BodyNode.StartLocation.Should().Be(new TextLocation(9, 41));
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[1].BodyNode.StartLocation.Should().Be(new TextLocation(13, 31));

            // Verify resolve results
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new string[0]);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new[] { "s" });
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[0].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new[] { "i", "j" });
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[1].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new[] { "x" });
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new[] { "s2" });
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].NestedFunctions[0].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new[] { "y" });
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[1].ResolveResult.Parameters.Select(p => p.Name).Should().Equal(new string[0]);

            // Verify parents
			MethodCompiler.nestedFunctionsRoot.Parent.Should().BeNull();
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].Parent.Should().Be(MethodCompiler.nestedFunctionsRoot);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].Parent.Should().Be(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0]);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[0].Parent.Should().Be(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0]);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0].NestedFunctions[1].Parent.Should().Be(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[0]);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].Parent.Should().Be(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0]);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1].NestedFunctions[0].Parent.Should().Be(MethodCompiler.nestedFunctionsRoot.NestedFunctions[0].NestedFunctions[1]);
            MethodCompiler.nestedFunctionsRoot.NestedFunctions[1].Parent.Should().Be(MethodCompiler.nestedFunctionsRoot);
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
@"public void M() {
    Func<int> f = () => {
        return base.GetHashCode();
    };
}");

            var f = MethodCompiler.nestedFunctionsRoot.NestedFunctions[0];
            Assert.That(f.DirectlyUsesThis, Is.True);
        }
	}
}
