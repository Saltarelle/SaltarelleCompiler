using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests {
    [TestFixture]
    public class MemberConversionTests : CompilerTestBase {
        [Test]
        public void SimpleInstanceMethodCanBeConverted() {
            var types = Compile(new[] { "class C { public void M() {} }" });
            var cls = FindClass(types, "C");
            cls.InstanceMembers.Should().Contain(m => m.Name == "M");
        }
    }
}
