using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Minimization;
using Saltarelle.Compiler.JSModel.Statements;

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
	globals(c, e, i, j, x);
	var a;
	(function() {
		locals(b, d, g, h);
		globals(c, e, i, x);
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
	}
}
