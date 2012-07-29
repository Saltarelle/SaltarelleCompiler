using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using JavaScriptParser.ParserImpl;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace JavaScriptParser {
	public static class Parser {
		public static JsExpression ParseExpression(string source) {
			JavaScriptLexer lex = new JavaScriptLexer(new ANTLRStringStream(source));
   			CommonTokenStream tokens = new CommonTokenStream(lex);
			var parser = new JavaScriptParser.ParserImpl.JavaScriptParser(tokens);

			var r = parser.expression();
			var tree = new JavaScriptTreeParser(new CommonTreeNodeStream(r.Tree));
			return tree.expression();
		}

		public static JsStatement ParseStatement(string source) {
			JavaScriptLexer lex = new JavaScriptLexer(new ANTLRStringStream(source));
   			CommonTokenStream tokens = new CommonTokenStream(lex);
			var parser = new JavaScriptParser.ParserImpl.JavaScriptParser(tokens);

			var r = parser.sourceElement();
			var tree = new JavaScriptTreeParser(new CommonTreeNodeStream(r.Tree));
			return tree.statement();
		}

		public static IList<JsStatement> ParseProgram(string source) {
			JavaScriptLexer lex = new JavaScriptLexer(new ANTLRStringStream(source));
   			CommonTokenStream tokens = new CommonTokenStream(lex);
			var parser = new JavaScriptParser.ParserImpl.JavaScriptParser(tokens);

			var r = parser.program();
			var tree = new JavaScriptTreeParser(new CommonTreeNodeStream(r.Tree));
			return tree.program();
		}
	}
}
