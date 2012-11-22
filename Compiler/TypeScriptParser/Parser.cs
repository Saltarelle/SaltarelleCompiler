using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using TypeScriptModel;
using TypeScriptModel.Model;

namespace TypeScriptParser {
	public static class Parser {
		public static Globals Parse(string source, IErrorReporter errorReporter) {
			var lex = new TypeScriptParserImpl.TypeScriptLexer(new ANTLRStringStream(source)) { ErrorReporter = errorReporter };
			CommonTokenStream tokens = new CommonTokenStream(lex);
			var parser = new TypeScriptParserImpl.TypeScriptParser(tokens) { ErrorReporter = errorReporter };

			var r = parser.program();
			if (r.Tree == null)
				return new Globals(new Module[0], new Interface[0], new Member[0]);
			var tree = new TypeScriptParserImpl.TypeScriptWalker(new CommonTreeNodeStream(r.Tree)) { ErrorReporter = errorReporter };
			return tree.program();
		}
	}
}
