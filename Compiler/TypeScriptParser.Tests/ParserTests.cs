using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TypeScriptModel;

namespace TypeScriptParser.Tests {
	[TestFixture]
    public class ParserTests {
		private void Roundtrip(string s) {
			var model = Parser.Parse(s);
			var actual = OutputFormatter.Format(model);
			Assert.AreEqual(s.Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"));
		}

		[Test]
		public void VariableWithoutType() {
			Roundtrip("declare var myVariable;");
		}

		[Test]
		public void VariableWithType() {
			Roundtrip("declare var myVariable: SomeType;");
		}

		[Test]
		public void EmptyInterface() {
			Roundtrip(
@"interface IFace {
}");
		}
    }
}
