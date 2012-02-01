using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests {
    [TestFixture]
    public class AllTests : CompilerTestBase {
        [Test]
        public void ParameterGetCorrectNamesForSimpleMethods() {
            var namingConvention = new MockNamingConventionResolver() {
                                       GetVariableName = (v, used) => {
                                           switch (v.Name) {
                                               case "i":
                                                   used.Should().BeEmpty();
                                                   return "$i";
                                               case "s":
                                                   used.Should().BeEquivalentTo(new object[] { "$i" });
                                                   return "$x";
                                               case "i2":
                                                   used.Should().BeEquivalentTo(new object[] { "$i", "$x" });
                                                   return "$i2";
                                               default:
                                                   Assert.Fail("Unexpected name");
                                                   return null;
                                           }
                                       }
                                   };
            Compile(new[] { "class C { public void M(int i, string s, int i2) {} }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.M").Definition.ParameterNames.Should().Equal(new object[] { "$i", "$x", "$i2" });
        }

        [Test]
        public void TypeParametersAreConsideredUsedDuringParameterNameDetermination() {
            var namingConvention = new MockNamingConventionResolver() { GetTypeParameterName = p => "$" + p.Name,
                                                                        GetVariableName = (v, used) => { used.Should().BeEquivalentTo(new[] { "$P1", "$P2", "$P3" }); return "$i"; } };

            Compile(new[] { "class C<P1> { public class C2<P2> { public void M<P3>(int i) {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C+C2.M").Definition.ParameterNames.Should().Equal(new object[] { "$i" });
        }
    }
}
