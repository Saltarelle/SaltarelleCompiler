using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Minification;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.Tests.MinificationTests {
	[TestFixture]
	public class MinifierTests {
		private class OutputRewriter : RewriterVisitorBase<object> {
			private readonly Dictionary<JsDeclarationScope, HashSet<string>> _locals;
			private readonly Dictionary<JsDeclarationScope, HashSet<string>> _globals;

			public OutputRewriter(Dictionary<JsDeclarationScope, HashSet<string>> locals, Dictionary<JsDeclarationScope, HashSet<string>> globals) {
				_locals = locals;
				_globals = globals;
			}

			public JsStatement Process(JsBlockStatement stmt) {
				stmt = (JsBlockStatement)VisitStatement(stmt, null);
				return JsStatement.Block(MakePrefix(JsDeclarationScope.Root).Concat(stmt.Statements));
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
				var body = (JsBlockStatement)VisitStatement(expression.Body, null);
				return JsExpression.FunctionDefinition(expression.ParameterNames, JsStatement.Block(MakePrefix(expression).Concat(body.Statements)), expression.Name);
			}

			public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
				var body = (JsBlockStatement)VisitStatement(statement.Body, null);
				return JsStatement.Function(statement.Name, statement.ParameterNames, JsStatement.Block(MakePrefix(statement).Concat(body.Statements)));
			}

			private IEnumerable<JsStatement> MakePrefix(JsDeclarationScope scope) {
				var result = new List<JsStatement>();
				if (_locals != null)
					result.Add(JsExpression.Invocation(JsExpression.Identifier("locals"), _locals[scope].OrderBy(x => x).Select(JsExpression.Identifier)));
				if (_globals != null)
					result.Add(JsExpression.Invocation(JsExpression.Identifier("globals"), _globals[scope].OrderBy(x => x).Select(JsExpression.Identifier)));
				return result;
			}
		}

		[Test]
		public void EncodeNumberWorks() {
			Assert.That(Enumerable.Range(0, 160).Select(Minifier.EncodeNumber).ToList(), Is.EqualTo(new[] {
				"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
				"ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt", "bu", "bv", "bw", "bx", "by", "bz", "bA", "bB", "bC", "bD", "bE", "bF", "bG", "bH", "bI", "bJ", "bK", "bL", "bM", "bN", "bO", "bP", "bQ", "bR", "bS", "bT", "bU", "bV", "bW", "bX", "bY", "bZ",
				"ca", "cb", "cc", "cd", "ce", "cf", "cg", "ch", "ci", "cj", "ck", "cl", "cm", "cn", "co", "cp", "cq", "cr", "cs", "ct", "cu", "cv", "cw", "cx", "cy", "cz", "cA", "cB", "cC", "cD", "cE", "cF", "cG", "cH", "cI", "cJ", "cK", "cL", "cM", "cN", "cO", "cP", "cQ", "cR", "cS", "cT", "cU", "cV", "cW", "cX", "cY", "cZ",
				"da", "db", "dc", "dd"
			}));

			Enumerable.Range(0, 1000000).Select(Minifier.EncodeNumber).Should().OnlyHaveUniqueItems();
		}

		[Test]
		public void GenerateNameGeneratesUniqueValidNames() {
			var usedNames = new HashSet<string>();
			for (int i = 0; i < 1000; i++) {
				var newName = Minifier.GenerateName("x", usedNames);
				Assert.That(newName.IsValidJavaScriptIdentifier(), Is.True);
				Assert.That(usedNames.Contains(newName), Is.False);
				Assert.That(JSModel.Utils.IsJavaScriptReservedWord(newName), Is.False);
				usedNames.Add(newName);
			}
		}

		[Test]
		public void MinimizingIdentifiersWorks() {
			var stmt = JsStatement.EnsureBlock(JavaScriptParser.Parser.ParseStatement(@"
{
	var variable1;
	(function() {
		var variable2;
		a;
		function d(p1, p2) {
			e;
			b;
			p1;
			variable1;
			var variable3;
		}
		for (var variable4 = 1;;) {
			for (var variable5 in d) {
			}
			for (f in x) {
			}
		}
	});
	try {
		ex;
	}
	catch (ex) {
		(function() {
			a;
			ex;
		});
	}
	j;
}"));
			var result = Minifier.Process(stmt);
			string actual = OutputFormatter.Format(result);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(
@"{
	var c;
	(function() {
		var g;
		a;
		function d(a, d) {
			e;
			b;
			a;
			c;
			var f;
		}
		for (var h = 1;;) {
			for (var i in d) {
			}
			for (f in x) {
			}
		}
	});
	try {
		ex;
	}
	catch (g) {
		(function() {
			a;
			g;
		});
	}
	j;
}
".Replace("\r\n", "\n")));
		}
	}
}
