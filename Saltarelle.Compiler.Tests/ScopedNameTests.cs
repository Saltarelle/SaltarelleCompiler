using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.TypeSystem;
using FluentAssertions;

namespace Saltarelle.Compiler.Tests {
    [TestFixture]
    public class ScopedNameTests {
        private void AssertEqual(ScopedName n1, ScopedName n2) {
            n1.Should().Be(n2);
            n1.GetHashCode().Should().Be(n2.GetHashCode());
        }

        private void AssertNotEqual(ScopedName n1, ScopedName n2) {
            n1.Should().NotBe(n2);
            n1.GetHashCode().Should().NotBe(n2.GetHashCode());
        }

        [Test]
        public void GlobalNameShouldNotBeEqualToNestedName() {
            AssertNotEqual(ScopedName.Nested(ScopedName.Global(null, "Parent"), "TestName"), ScopedName.Global(null, "TestName"));
        }

        [Test]
        public void GlobalNamesShouldBeEqualIfAndOnlyIfNamespacesAndNamesAreTheSame() {
            AssertEqual(ScopedName.Global(null, "TestName"), ScopedName.Global(null, "TestName"));
            AssertEqual(ScopedName.Global("Nmspace", "TestName"), ScopedName.Global("Nmspace", "TestName"));
            AssertEqual(ScopedName.Global("Nmspace.Nested", "TestName"), ScopedName.Global("Nmspace.Nested", "TestName"));

            AssertNotEqual(ScopedName.Global(null, "TestName"), ScopedName.Global(null, "TestName2"));
            AssertNotEqual(ScopedName.Global("Nmspace", "TestName"), ScopedName.Global("Nmspace", "TestName2"));
            AssertNotEqual(ScopedName.Global("Nmspace", "TestName"), ScopedName.Global("Nmspace2", "TestName"));
        }

        [Test]
        public void NamespacePartsShouldBeEmptyForNonNamespacedTypes() {
            ScopedName.Global(null, "TestName").NamespaceParts.Should().BeEmpty();
        }

        [Test]
        public void NamespacePartsShouldWorkForNamespacedTypes() {
            ScopedName.Global("Nmspace1", "TestName").NamespaceParts.Should().Equal(new[] { "Nmspace1" });
            ScopedName.Global("Nmspace1.Nmspace2", "TestName").NamespaceParts.Should().Equal(new[] { "Nmspace1", "Nmspace2" });
            ScopedName.Global("Nmspace1.Nmspace2.Nmspace3", "TestName").NamespaceParts.Should().Equal(new[] { "Nmspace1", "Nmspace2", "Nmspace3" });
        }

        [Test]
        public void TypesNestedInEqualButNonSameTypesShouldBeConsideredEqual() {
            AssertEqual(ScopedName.Nested(ScopedName.Global(null, "Container"), "TestName"), ScopedName.Nested(ScopedName.Global(null, "Container"), "TestName"));
            AssertEqual(ScopedName.Nested(ScopedName.Nested(ScopedName.Global(null, "Container"), "Container2"), "TestName"), ScopedName.Nested(ScopedName.Nested(ScopedName.Global(null, "Container"), "Container2"), "TestName"));
        }
    }
}
