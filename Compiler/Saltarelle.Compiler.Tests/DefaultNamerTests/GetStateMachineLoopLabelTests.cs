using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;

namespace Saltarelle.Compiler.Tests.DefaultNamerTests {
	[TestFixture]
	public class GetStateMachineLoopLabelTests {
		[Test]
		public void ReturnsUniqueLoopLabels() {
			var n = new DefaultNamer();
			Assert.That(n.GetStateMachineLoopLabel(new HashSet<string>()), Is.EqualTo("$sm1"));
			Assert.That(n.GetStateMachineLoopLabel(new HashSet<string> { "$sm1" }), Is.EqualTo("$sm2"));
			Assert.That(n.GetStateMachineLoopLabel(new HashSet<string> { "$sm1", "$sm2" }), Is.EqualTo("$sm3"));
		}
	}
}
