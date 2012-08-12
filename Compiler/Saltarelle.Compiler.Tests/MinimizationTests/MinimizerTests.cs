using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Minimization;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.Tests.MinimizationTests
{
	[TestFixture]
	public class MinimizerTests {
		private class OutputRewriter : RewriterVisitorBase<object> {
			private readonly Dictionary<Minimizer.Function, HashSet<string>> _locals;
			private readonly Dictionary<Minimizer.Function, HashSet<string>> _globals;

			public OutputRewriter(Dictionary<Minimizer.Function, HashSet<string>> locals, Dictionary<Minimizer.Function, HashSet<string>> globals)
			{
				_locals = locals;
				_globals = globals;
			}

			public JsStatement Process(JsBlockStatement stmt) {
				stmt = (JsBlockStatement)VisitStatement(stmt, null);
				return new JsBlockStatement(MakePrefix(new Minimizer.Function()).Concat(stmt.Statements));
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
				var body = (JsBlockStatement)VisitStatement(expression.Body, null);
				return new JsFunctionDefinitionExpression(expression.ParameterNames, new JsBlockStatement(MakePrefix(expression).Concat(body.Statements)), expression.Name);
			}

			public override JsStatement VisitFunctionStatement(JsFunctionStatement statement, object data) {
				var body = (JsBlockStatement)VisitStatement(statement.Body, null);
				return new JsFunctionStatement(statement.Name, statement.ParameterNames, new JsBlockStatement(MakePrefix(statement).Concat(body.Statements)));
			}

			private IEnumerable<JsStatement> MakePrefix(Minimizer.Function function) {
				var result = new List<JsStatement>();
				if (_locals != null)
					result.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Identifier("locals"), _locals[function].OrderBy(x => x).Select(JsExpression.Identifier))));
				if (_globals != null)
					result.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Identifier("globals"), _globals[function].OrderBy(x => x).Select(JsExpression.Identifier))));
				return result;
			}
		}

		[Test]
		public void GatheringIsCorrect() {
			var stmt = JsBlockStatement.MakeBlock(JavaScriptParser.Parser.ParseStatement(@"
{
	var a;
	(function() {
		var b;
		c;
		function d(p1, p2) {
			e;
			b;
			var f;
		}
		for (var g = 1;;) {
			for (var h in x) {
			}
			for (i in x) {
			}
		}
	});
	try {
	}
	catch (ex) {
		(function() {
			ex;
		});
	}
	j;
}"));
			var locals = Minimizer.LocalVariableGatherer.Analyze(stmt);
			var globals = Minimizer.ImplicitGlobalsGatherer.Analyze(stmt, locals);
			var result = new OutputRewriter(locals, globals).Process(stmt);

			string actual = OutputFormatter.Format(result);

			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(
@"{
	locals(a);
	globals(c, d, e, i, j, x);
	var a;
	(function() {
		locals(b, g, h);
		globals(c, d, e, i, x);
		var b;
		c;
		function d(p1, p2) {
			locals(f, p1, p2);
			globals(e);
			e;
			b;
			var f;
		}
		for (var g = 1;;) {
			for (var h in x) {
			}
			for (i in x) {
			}
		}
	});
	try {
	}
	catch (ex) {
		(function() {
			locals();
			globals();
			ex;
		});
	}
	j;
}
".Replace("\r\n", "\n")));
		}

		[Test]
		public void EncodeNumberWorks() {
			Assert.That(Enumerable.Range(0, 160).Select(Minimizer.EncodeNumber).ToList(), Is.EqualTo(new[] {
				"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
				"ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt", "bu", "bv", "bw", "bx", "by", "bz", "bA", "bB", "bC", "bD", "bE", "bF", "bG", "bH", "bI", "bJ", "bK", "bL", "bM", "bN", "bO", "bP", "bQ", "bR", "bS", "bT", "bU", "bV", "bW", "bX", "bY", "bZ",
				"ca", "cb", "cc", "cd", "ce", "cf", "cg", "ch", "ci", "cj", "ck", "cl", "cm", "cn", "co", "cp", "cq", "cr", "cs", "ct", "cu", "cv", "cw", "cx", "cy", "cz", "cA", "cB", "cC", "cD", "cE", "cF", "cG", "cH", "cI", "cJ", "cK", "cL", "cM", "cN", "cO", "cP", "cQ", "cR", "cS", "cT", "cU", "cV", "cW", "cX", "cY", "cZ",
				"da", "db", "dc", "dd"
			}));

			Enumerable.Range(0, 1000000).Select(Minimizer.EncodeNumber).Should().OnlyHaveUniqueItems();
		}

		[Test]
		public void GenerateNameGeneratesUniqueValidNames() {
			var usedNames = new HashSet<string>();
			for (int i = 0; i < 1000; i++) {
				var newName = Minimizer.GenerateName("x", usedNames);
				Assert.That(newName.IsValidJavaScriptIdentifier(), Is.True);
				Assert.That(usedNames.Contains(newName), Is.False);
				Assert.That(JSModel.Utils.IsJavaScriptReservedWord(newName), Is.False);
				usedNames.Add(newName);
			}
		}

		[Test]
		public void MinimizingIdentifiersWorks() {
			var stmt = JsBlockStatement.MakeBlock(JavaScriptParser.Parser.ParseStatement(@"
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
			var result = new Minimizer(true, false).Process(stmt);
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

		[Test]
		public void StrippingCommentsWorks() {
			var stmt = new JsBlockStatement(
				new JsComment("Comment 1"),
				new JsExpressionStatement(JsExpression.Identifier("a")),
				new JsComment("Comment 2"),
				new JsExpressionStatement(JsExpression.Identifier("b"))
			);

			var result = new Minimizer(false, true).Process(stmt);
			string actual = OutputFormatter.Format(result);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(
@"{
	a;
	b;
}
".Replace("\r\n", "\n")));
		}
	}
}
