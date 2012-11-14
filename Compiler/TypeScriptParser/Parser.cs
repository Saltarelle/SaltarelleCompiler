using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using TypeScriptModel;

namespace TypeScriptParser {
	public static class Parser {
		public static Globals Parse(string source) {
			var lex = new TypeScriptParserImpl.TypeScriptLexer(new ANTLRStringStream(source));
			CommonTokenStream tokens = new CommonTokenStream(lex);
			var parser = new TypeScriptParserImpl.TypeScriptParser(tokens);

			var r = parser.program();
			var tree = new TypeScriptParserImpl.TypeScriptWalker(new CommonTreeNodeStream(r.Tree));
			return tree.program();
		}
	}
}
