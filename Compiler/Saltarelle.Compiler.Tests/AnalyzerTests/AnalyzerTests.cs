using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Analyzers;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Minification;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.Tests.AnalyzerTests {
	[TestFixture]
	public class AnalyzerTests {
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
		public void GatheringIsCorrect() {
			var stmt = JsStatement.EnsureBlock(JavaScriptParser.Parser.ParseStatement(@"
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
			var locals = LocalVariableGatherer.Analyze(stmt);
			var globals = ImplicitGlobalsGatherer.Analyze(stmt, locals, reportGlobalsAsUsedInAllParentScopes: true);
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
		public void ImplicitGlobalsGathererWithNoReportInAllParentScopes() {
			var stmt = JsStatement.EnsureBlock(JavaScriptParser.Parser.ParseStatement(@"
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
			var locals = LocalVariableGatherer.Analyze(stmt);
			var globals = ImplicitGlobalsGatherer.Analyze(stmt, locals, reportGlobalsAsUsedInAllParentScopes: false);
			var result = new OutputRewriter(locals, globals).Process(stmt);

			string actual = OutputFormatter.Format(result);

			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(
@"{
	locals(a);
	globals(j);
	var a;
	(function() {
		locals(b, g, h);
		globals(c, d, i, x);
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
