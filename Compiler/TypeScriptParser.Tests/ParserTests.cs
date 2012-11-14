using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TypeScriptParser.Tests {
	[TestFixture]
    public class ParserTests {
		[Test]
		public void VariableWithoutType() {
			var actual = Parser.Parse("declare var myVariable");
		}
    }
}
