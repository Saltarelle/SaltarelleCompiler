using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler;
using Messages = CoreLib.Plugin.Messages;

namespace CoreLib.Tests {
	[TestFixture]
	public class StructureTests {
		[Test]
		public void MessagesHaveCorrectCodes() {
			var t = typeof(Messages);
			foreach (var f in t.GetFields().Where(f => f.Name.StartsWith("_"))) {
				int id = int.Parse(f.Name.Substring(1));
				var msg = (Tuple<int, DiagnosticSeverity, string>)f.GetValue(null);
				Assert.That(msg.Item1, Is.EqualTo(id), "Wrong code for message " + id);
			}
		}
		
	}
}
