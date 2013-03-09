using System.Collections.Generic;
using CoreLib.Plugin;
using NUnit.Framework;

namespace CoreLib.Tests.NamerTests {
	[TestFixture]
	public class GetStateMachineLoopLabelTests {
		[Test]
		public void ReturnsUniqueLoopLabels() {
			var n = new Namer();
			Assert.That(n.GetStateMachineLoopLabel(new HashSet<string>()), Is.EqualTo("$sm1"));
			Assert.That(n.GetStateMachineLoopLabel(new HashSet<string> { "$sm1" }), Is.EqualTo("$sm2"));
			Assert.That(n.GetStateMachineLoopLabel(new HashSet<string> { "$sm1", "$sm2" }), Is.EqualTo("$sm3"));
		}
	}
}
