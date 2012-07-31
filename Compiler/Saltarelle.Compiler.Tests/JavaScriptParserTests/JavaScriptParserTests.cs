using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests.JavaScriptParserTests {
	[TestFixture]
	public class JavaScriptParserTests {
		private T ParseExpression<T>(string source) where T : JsExpression {
			var expr = JavaScriptParser.Parser.ParseExpression("(" + source + ")");
			Assert.That(expr, Is.InstanceOf<T>());
			return (T)expr;
		}

		private T ParseStatement<T>(string source) where T : JsStatement {
			var stmt = JavaScriptParser.Parser.ParseStatement(source);
			Assert.That(stmt, Is.InstanceOf<T>());
			return (T)stmt;
		}

		private void RoundtripExpression(string source, string expected = null) {
			var expr = JavaScriptParser.Parser.ParseExpression(source);
			Assert.That(OutputFormatter.Format(expr).Replace("\r\n", "\n"), Is.EqualTo((expected ?? source).Replace("\r\n", "\n")));
		}

		private void RoundtripStatement(string source, string expected = null) {
			var stmt = JavaScriptParser.Parser.ParseStatement(source);
			Assert.That(OutputFormatter.Format(stmt).Replace("\r\n", "\n"), Is.EqualTo((expected ?? source).Replace("\r\n", "\n")));
		}

		[Test]
		public void Null() {
			var expr = ParseExpression<JsConstantExpression>("null");
			Assert.That(expr.NodeType, Is.EqualTo(ExpressionNodeType.Null));
		}

		[Test]
		public void Identifier() {
			var expr = ParseExpression<JsIdentifierExpression>("myIdentifier");
			Assert.That(expr.Name, Is.EqualTo("myIdentifier"));
		}

		[Test]
		public void Number() {
			var expr = ParseExpression<JsConstantExpression>("123");
			Assert.That(expr.NumberValue, Is.EqualTo(123));
			expr = ParseExpression<JsConstantExpression>("0xff");
			Assert.That(expr.NumberValue, Is.EqualTo(255));
			expr = ParseExpression<JsConstantExpression>("1.375");
			Assert.That(expr.NumberValue, Is.EqualTo(1.375));
		}

		[Test]
		public void String() {
			var expr = ParseExpression<JsConstantExpression>("'XYZ'");
			Assert.That(expr.StringValue, Is.EqualTo("XYZ"));
			expr = ParseExpression<JsConstantExpression>("\"XYZ\"");
			Assert.That(expr.StringValue, Is.EqualTo("XYZ"));
			expr = ParseExpression<JsConstantExpression>("\"X\\\"YZ\"");
			Assert.That(expr.StringValue, Is.EqualTo("X\"YZ"));
		}

		[Test]
		public void Boolean() {
			var exprT = ParseExpression<JsConstantExpression>("true");
			Assert.That(exprT.BooleanValue, Is.True);
			var exprF = ParseExpression<JsConstantExpression>("false");
			Assert.That(exprF.BooleanValue, Is.False);
		}

		[Test, Ignore("Not supported")]
		public void Regex() {
			var expr = ParseExpression<JsConstantExpression>("/a/");
			Assert.That(expr.RegexpValue.Pattern, Is.EqualTo("a"));
			Assert.That(expr.RegexpValue.Options, Is.EqualTo(""));
			expr = ParseExpression<JsConstantExpression>("/b/i");
			Assert.That(expr.RegexpValue.Pattern, Is.EqualTo("b"));
			Assert.That(expr.RegexpValue.Options, Is.EqualTo("i"));
		}

		[Test]
		public void This() {
			ParseExpression<JsThisExpression>("this");
		}

		[Test]
		public void Unary() {
			foreach (var t in new[] { Tuple.Create("typeof x", ExpressionNodeType.TypeOf),
			                          Tuple.Create("!x", ExpressionNodeType.LogicalNot),
			                          Tuple.Create("-x", ExpressionNodeType.Negate),
			                          Tuple.Create("+x", ExpressionNodeType.Positive),
			                          Tuple.Create("++x", ExpressionNodeType.PrefixPlusPlus),
			                          Tuple.Create("--x", ExpressionNodeType.PrefixMinusMinus),
			                          Tuple.Create("x++", ExpressionNodeType.PostfixPlusPlus),
			                          Tuple.Create("x--", ExpressionNodeType.PostfixMinusMinus),
			                          Tuple.Create("delete x", ExpressionNodeType.Delete),
			                          Tuple.Create("void x", ExpressionNodeType.Void),
			                          Tuple.Create("~x", ExpressionNodeType.BitwiseNot),
			                        }
			) {
				var expr = ParseExpression<JsUnaryExpression>(t.Item1);
				Assert.That(expr.NodeType, Is.EqualTo(t.Item2));
				Assert.That(expr.Operand.NodeType == ExpressionNodeType.Identifier && ((JsIdentifierExpression)expr.Operand).Name == "x");
			}
		}

		[Test]
		public void Binary() {
			foreach (var t in new[] { Tuple.Create("x && y", ExpressionNodeType.LogicalAnd),
			                          Tuple.Create("x || y", ExpressionNodeType.LogicalOr),
			                          Tuple.Create("x != y", ExpressionNodeType.NotEqual),
			                          Tuple.Create("x <= y", ExpressionNodeType.LesserOrEqual),
			                          Tuple.Create("x >= y", ExpressionNodeType.GreaterOrEqual),
			                          Tuple.Create("x < y", ExpressionNodeType.Lesser),
			                          Tuple.Create("x > y", ExpressionNodeType.Greater),
			                          Tuple.Create("x == y", ExpressionNodeType.Equal),
			                          Tuple.Create("x - y", ExpressionNodeType.Subtract),
			                          Tuple.Create("x + y", ExpressionNodeType.Add),
			                          Tuple.Create("x % y", ExpressionNodeType.Modulo),
			                          Tuple.Create("x / y", ExpressionNodeType.Divide),
			                          Tuple.Create("x * y", ExpressionNodeType.Multiply),
			                          Tuple.Create("x & y", ExpressionNodeType.BitwiseAnd),
			                          Tuple.Create("x | y", ExpressionNodeType.BitwiseOr),
			                          Tuple.Create("x ^ y", ExpressionNodeType.BitwiseXor),
			                          Tuple.Create("x === y", ExpressionNodeType.Same),
			                          Tuple.Create("x !== y", ExpressionNodeType.NotSame),
			                          Tuple.Create("x << y", ExpressionNodeType.LeftShift),
			                          Tuple.Create("x >> y", ExpressionNodeType.RightShiftSigned),
			                          Tuple.Create("x >>> y", ExpressionNodeType.RightShiftUnsigned),
			                          Tuple.Create("x instanceof y", ExpressionNodeType.InstanceOf),
			                          Tuple.Create("x in y", ExpressionNodeType.In),
			                          Tuple.Create("x = y", ExpressionNodeType.Assign),
			                          Tuple.Create("x *= y", ExpressionNodeType.MultiplyAssign),
			                          Tuple.Create("x /= y", ExpressionNodeType.DivideAssign),
			                          Tuple.Create("x %= y", ExpressionNodeType.ModuloAssign),
			                          Tuple.Create("x += y", ExpressionNodeType.AddAssign),
			                          Tuple.Create("x -= y", ExpressionNodeType.SubtractAssign),
			                          Tuple.Create("x <<= y", ExpressionNodeType.LeftShiftAssign),
			                          Tuple.Create("x >>= y", ExpressionNodeType.RightShiftSignedAssign),
			                          Tuple.Create("x >>>= y", ExpressionNodeType.RightShiftUnsignedAssign),
			                          Tuple.Create("x &= y", ExpressionNodeType.BitwiseAndAssign),
			                          Tuple.Create("x |= y", ExpressionNodeType.BitwiseOrAssign),
			                          Tuple.Create("x ^= y", ExpressionNodeType.BitwiseXOrAssign),
			                        }
			) {
				var expr = ParseExpression<JsBinaryExpression>(t.Item1);
				Assert.That(expr.NodeType, Is.EqualTo(t.Item2));
				Assert.That(expr.Left.NodeType == ExpressionNodeType.Identifier && ((JsIdentifierExpression)expr.Left).Name == "x");
				Assert.That(expr.Right.NodeType == ExpressionNodeType.Identifier && ((JsIdentifierExpression)expr.Right).Name == "y");
			}
		}

		[Test]
		public void Comma() {
			RoundtripExpression("a, b, c");
		}

		[Test]
		public void Conditional() {
			RoundtripExpression("a ? b : c", "(a ? b : c)");
		}

		[Test]
		public void Invocation() {
			RoundtripExpression("f()");
			RoundtripExpression("f(a, b)");
		}

		[Test]
		public void InvocationWithSuffix() {
			RoundtripExpression("f(a)(b)[c].d", "(f(a)(b)[c]).d");
		}

		[Test]
		public void ObjectCreation() {
			RoundtripExpression("new f()");
			RoundtripExpression("new f(a, b)");
		}

		[Test]
		public void FunctionDefinitionExpression1() {
			RoundtripExpression(
@"(function f() {
})()");
		}

		[Test]
		public void FunctionDefinitionExpression2() {
			RoundtripExpression(
@"(function f(a, b) {
	c;
})()");
		}

		[Test]
		public void FunctionDefinitionExpression3() {
			RoundtripExpression(
@"(function(a, b) {
	c;
})()");
		}

		[Test]
		public void MembersAndIndexing() {
			RoundtripExpression("a[b].c[d].e", "((a[b]).c[d]).e");
			RoundtripExpression("(new a()).b");
		}

		[Test]
		public void ArrayLiteral() {
			RoundtripExpression("[1, 2, 3]");
		}

		[Test]
		public void ObjectLiteral() {
			RoundtripExpression("{ a: b, 'c': d, 1: e }", "{ a: b, c: d, '1': e }");
			var expr = ParseExpression<JsObjectLiteralExpression>("{}");
			Assert.That(expr.Values, Is.Empty);
		}

		[Test]
		public void ExpressionStatement() {
			var stmt = ParseStatement<JsExpressionStatement>("x;");
			Assert.That(stmt.Expression, Is.InstanceOf<JsIdentifierExpression>());
			Assert.That(((JsIdentifierExpression)stmt.Expression).Name, Is.EqualTo("x"));
		}

		[Test]
		public void BlockStatement() {
			var stmt = ParseStatement<JsBlockStatement>("{\n}");
			Assert.That(stmt.Statements, Is.Empty);

			stmt = ParseStatement<JsBlockStatement>("{x;}");
			Assert.That(stmt.Statements, Has.Count.EqualTo(1));
			Assert.That(stmt.Statements[0], Is.InstanceOf<JsExpressionStatement>());
		}

		[Test]
		public void EmptyStatement() {
			ParseStatement<JsEmptyStatement>(";");
		}

		[Test]
		public void LabelledStatement() {
			var stmt = ParseStatement<JsBlockStatement>("{lbl: x;}");
			Assert.That(stmt.Statements.Count, Is.EqualTo(1));
			Assert.That(stmt.Statements[0], Is.InstanceOf<JsLabelledStatement>());
			Assert.That(((JsLabelledStatement)stmt.Statements[0]).Label, Is.EqualTo("lbl"));
			Assert.That(((JsLabelledStatement)stmt.Statements[0]).Statement, Is.InstanceOf<JsExpressionStatement>());
			Assert.That(((JsExpressionStatement)((JsLabelledStatement)stmt.Statements[0]).Statement).Expression, Is.InstanceOf<JsIdentifierExpression>());
		}

		[Test]
		public void VariableDeclaration() {
			RoundtripStatement("var i;\n");
			RoundtripStatement("var i = 0;\n");
			RoundtripStatement("var i, j, k;\n");
			RoundtripStatement("var i = 0, j = 1, k = 2;\n");
		}

		[Test]
		public void IfStatement() {
			var stmt = ParseStatement<JsIfStatement>("if (x) { y; } else { z; }");
			Assert.That(OutputFormatter.Format(stmt.Test), Is.EqualTo("x"));
			Assert.That(OutputFormatter.Format(stmt.Then).Replace("\r\n", "\n"), Is.EqualTo("{\n\ty;\n}\n"));
			Assert.That(OutputFormatter.Format(stmt.Else).Replace("\r\n", "\n"), Is.EqualTo("{\n\tz;\n}\n"));

			stmt = ParseStatement<JsIfStatement>("if (x) { y; }");
			Assert.That(OutputFormatter.Format(stmt.Test), Is.EqualTo("x"));
			Assert.That(OutputFormatter.Format(stmt.Then).Replace("\r\n", "\n"), Is.EqualTo("{\n\ty;\n}\n"));
			Assert.That(stmt.Else, Is.Null);

			stmt = ParseStatement<JsIfStatement>("if (x) { y; } else if (z) { a; } else { b; }");
			Assert.That(OutputFormatter.Format(stmt.Test), Is.EqualTo("x"));
			Assert.That(OutputFormatter.Format(stmt.Then).Replace("\r\n", "\n"), Is.EqualTo("{\n\ty;\n}\n"));
			var jsElse = stmt.Else.Statements[0] as JsIfStatement;
			Assert.That(jsElse, Is.Not.Null);
			Assert.That(OutputFormatter.Format(jsElse.Test), Is.EqualTo("z"));
			Assert.That(OutputFormatter.Format(jsElse.Then).Replace("\r\n", "\n"), Is.EqualTo("{\n\ta;\n}\n"));
			Assert.That(OutputFormatter.Format(jsElse.Else).Replace("\r\n", "\n"), Is.EqualTo("{\n\tb;\n}\n"));
		}

		[Test]
		public void WhileStatement() {
			var stmt = ParseStatement<JsWhileStatement>("while (x) { y; }");
			Assert.That(OutputFormatter.Format(stmt.Condition), Is.EqualTo("x"));
			Assert.That(OutputFormatter.Format(stmt.Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\ty;\n}\n"));
		}

		[Test]
		public void DoWhileStatement() {
			var stmt = ParseStatement<JsDoWhileStatement>("do { y; } while (x);");
			Assert.That(OutputFormatter.Format(stmt.Condition), Is.EqualTo("x"));
			Assert.That(OutputFormatter.Format(stmt.Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\ty;\n}\n"));
		}

		[Test]
		public void ForStatement() {
			var stmt = ParseStatement<JsForStatement>("for (x = 0; x < y; x++) { z; }");
			Assert.That(stmt.InitStatement, Is.InstanceOf<JsExpressionStatement>());
			Assert.That(OutputFormatter.Format(stmt.InitStatement).Replace("\r\n", "\n"), Is.EqualTo("x = 0;\n"));
			Assert.That(OutputFormatter.Format(stmt.ConditionExpression), Is.EqualTo("x < y"));
			Assert.That(OutputFormatter.Format(stmt.IteratorExpression), Is.EqualTo("x++"));
			Assert.That(OutputFormatter.Format(stmt.Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\tz;\n}\n"));

			stmt = ParseStatement<JsForStatement>("for (var x = 0, y = 0; x < y; x++) { z; }");
			Assert.That(stmt.InitStatement, Is.InstanceOf<JsVariableDeclarationStatement>());
			Assert.That(OutputFormatter.Format(stmt.InitStatement).Replace("\r\n", "\n"), Is.EqualTo("var x = 0, y = 0;\n"));
			Assert.That(OutputFormatter.Format(stmt.ConditionExpression), Is.EqualTo("x < y"));
			Assert.That(OutputFormatter.Format(stmt.IteratorExpression), Is.EqualTo("x++"));
			Assert.That(OutputFormatter.Format(stmt.Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\tz;\n}\n"));

			stmt = ParseStatement<JsForStatement>("for (; x < y; x++) { z; }");
			Assert.That(stmt.InitStatement, Is.InstanceOf<JsEmptyStatement>());
			Assert.That(OutputFormatter.Format(stmt.ConditionExpression), Is.EqualTo("x < y"));
			Assert.That(OutputFormatter.Format(stmt.IteratorExpression), Is.EqualTo("x++"));
			Assert.That(OutputFormatter.Format(stmt.Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\tz;\n}\n"));

			stmt = ParseStatement<JsForStatement>("for (x = 0; ; x++) { z; }");
			Assert.That(stmt.InitStatement, Is.InstanceOf<JsExpressionStatement>());
			Assert.That(OutputFormatter.Format(stmt.InitStatement).Replace("\r\n", "\n"), Is.EqualTo("x = 0;\n"));
			Assert.That(stmt.ConditionExpression, Is.Null);
			Assert.That(OutputFormatter.Format(stmt.IteratorExpression), Is.EqualTo("x++"));
			Assert.That(OutputFormatter.Format(stmt.Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\tz;\n}\n"));

			stmt = ParseStatement<JsForStatement>("for (x = 0; x < y; ) { z; }");
			Assert.That(stmt.InitStatement, Is.InstanceOf<JsExpressionStatement>());
			Assert.That(OutputFormatter.Format(stmt.InitStatement).Replace("\r\n", "\n"), Is.EqualTo("x = 0;\n"));
			Assert.That(OutputFormatter.Format(stmt.ConditionExpression), Is.EqualTo("x < y"));
			Assert.That(stmt.IteratorExpression, Is.Null);
			Assert.That(OutputFormatter.Format(stmt.Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\tz;\n}\n"));
		}

		[Test]
		public void ForEachInStatement() {
			var stmt = ParseStatement<JsForEachInStatement>("for (x in y) { z; }");
			Assert.That(stmt.LoopVariableName, Is.EqualTo("x"));
			Assert.That(stmt.IsLoopVariableDeclared, Is.False);
			Assert.That(OutputFormatter.Format(stmt.ObjectToIterateOver), Is.EqualTo("y"));
			Assert.That(OutputFormatter.Format(stmt.Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\tz;\n}\n"));

			stmt = ParseStatement<JsForEachInStatement>("for (var x in y) { z; }");
			Assert.That(stmt.LoopVariableName, Is.EqualTo("x"));
			Assert.That(stmt.IsLoopVariableDeclared, Is.True);
			Assert.That(OutputFormatter.Format(stmt.ObjectToIterateOver), Is.EqualTo("y"));
			Assert.That(OutputFormatter.Format(stmt.Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\tz;\n}\n"));
		}

		[Test]
		public void ContinueStatement() {
			var stmt = ParseStatement<JsContinueStatement>("continue;");
			Assert.That(stmt.TargetLabel, Is.Null);

			stmt = ParseStatement<JsContinueStatement>("continue lbl;");
			Assert.That(stmt.TargetLabel, Is.EqualTo("lbl"));
		}

		[Test]
		public void BreakStatement() {
			var stmt = ParseStatement<JsBreakStatement>("break;");
			Assert.That(stmt.TargetLabel, Is.Null);

			stmt = ParseStatement<JsBreakStatement>("break lbl;");
			Assert.That(stmt.TargetLabel, Is.EqualTo("lbl"));
		}

		[Test]
		public void ReturnStatement() {
			var stmt = ParseStatement<JsReturnStatement>("return;");
			Assert.That(stmt.Value, Is.Null);

			stmt = ParseStatement<JsReturnStatement>("return x;");
			Assert.That(OutputFormatter.Format(stmt.Value), Is.EqualTo("x"));
		}

		[Test]
		public void WithStatement() {
			var stmt = ParseStatement<JsWithStatement>("with (x) { y; }");
			Assert.That(OutputFormatter.Format(stmt.Object), Is.EqualTo("x"));
			Assert.That(OutputFormatter.Format(stmt.Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\ty;\n}\n"));
		}

		[Test]
		public void TryStatement() {
			var stmt = ParseStatement<JsTryCatchFinallyStatement>("try { a; } catch (b) { c; } finally { d; }");
			Assert.That(OutputFormatter.Format(stmt.GuardedStatement).Replace("\r\n", "\n"), Is.EqualTo("{\n\ta;\n}\n"));
			Assert.That(stmt.Catch.Identifier, Is.EqualTo("b"));
			Assert.That(OutputFormatter.Format(stmt.Catch.Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\tc;\n}\n"));
			Assert.That(OutputFormatter.Format(stmt.Finally).Replace("\r\n", "\n"), Is.EqualTo("{\n\td;\n}\n"));

			stmt = ParseStatement<JsTryCatchFinallyStatement>("try { a; } catch (b) { c; }");
			Assert.That(OutputFormatter.Format(stmt.GuardedStatement).Replace("\r\n", "\n"), Is.EqualTo("{\n\ta;\n}\n"));
			Assert.That(stmt.Catch.Identifier, Is.EqualTo("b"));
			Assert.That(OutputFormatter.Format(stmt.Catch.Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\tc;\n}\n"));
			Assert.That(stmt.Finally, Is.Null);

			stmt = ParseStatement<JsTryCatchFinallyStatement>("try { a; } finally { d; }");
			Assert.That(OutputFormatter.Format(stmt.GuardedStatement).Replace("\r\n", "\n"), Is.EqualTo("{\n\ta;\n}\n"));
			Assert.That(stmt.Catch, Is.Null);
			Assert.That(OutputFormatter.Format(stmt.Finally).Replace("\r\n", "\n"), Is.EqualTo("{\n\td;\n}\n"));
		}

		[Test]
		public void ThrowStatement() {
			var stmt = ParseStatement<JsThrowStatement>("throw x;");
			Assert.That(OutputFormatter.Format(stmt.Expression), Is.EqualTo("x"));
		}

		[Test]
		public void FunctionDeclarationStatement() {
			RoundtripStatement("function f() {\n\tx;\n}\n");
			RoundtripStatement("function f(a, b, c) {\n\tx;\n}\n");
		}

		[Test]
		public void SwitchStatement() {
			var stmt = ParseStatement<JsSwitchStatement>("switch(a) { case b: x; case c: y; default: z; }");
			Assert.That(OutputFormatter.Format(stmt.Expression), Is.EqualTo("a"));
			Assert.That(stmt.Clauses.Count, Is.EqualTo(3));
			Assert.That(stmt.Clauses[0].Values.Select(v => OutputFormatter.Format(v)), Is.EqualTo(new[] { "b" }));
			Assert.That(OutputFormatter.Format(stmt.Clauses[0].Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\tx;\n}\n"));
			Assert.That(stmt.Clauses[1].Values.Select(v => OutputFormatter.Format(v)), Is.EqualTo(new[] { "c" }));
			Assert.That(OutputFormatter.Format(stmt.Clauses[1].Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\ty;\n}\n"));
			Assert.That(stmt.Clauses[2].Values, Is.EqualTo(new object[] { null }));
			Assert.That(OutputFormatter.Format(stmt.Clauses[2].Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\tz;\n}\n"));
		}

		[Test]
		public void SwitchStatementWithMultipleLabelsPerBlock() {
			var stmt = ParseStatement<JsSwitchStatement>("switch(a) { case b: case c: x; case d: default: y; }");
			Assert.That(OutputFormatter.Format(stmt.Expression), Is.EqualTo("a"));
			Assert.That(stmt.Clauses.Count, Is.EqualTo(2));
			Assert.That(stmt.Clauses[0].Values.Select(v => OutputFormatter.Format(v)), Is.EqualTo(new[] { "b", "c" }));
			Assert.That(OutputFormatter.Format(stmt.Clauses[0].Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\tx;\n}\n"));
			Assert.That(stmt.Clauses[1].Values.Count, Is.EqualTo(2));
			Assert.That(OutputFormatter.Format(stmt.Clauses[1].Values[0]), Is.EqualTo("d"));
			Assert.That(stmt.Clauses[1].Values[1], Is.Null);
			Assert.That(OutputFormatter.Format(stmt.Clauses[1].Body).Replace("\r\n", "\n"), Is.EqualTo("{\n\ty;\n}\n"));
		}

		[Test]
		public void EmptySwitchStatement() {
			var stmt = ParseStatement<JsSwitchStatement>("switch(a) {}");
			Assert.That(OutputFormatter.Format(stmt.Expression), Is.EqualTo("a"));
			Assert.That(stmt.Clauses.Count, Is.EqualTo(0));
		}

		[Test]
		public void SwitchStatementWithEmptyClause() {
			var stmt = ParseStatement<JsSwitchStatement>("switch(a) { case b: }");
			Assert.That(OutputFormatter.Format(stmt.Expression), Is.EqualTo("a"));
			Assert.That(stmt.Clauses.Count, Is.EqualTo(1));
			Assert.That(stmt.Clauses[0].Values.Select(v => OutputFormatter.Format(v)), Is.EqualTo(new[] { "b" }));
			Assert.That(stmt.Clauses[0].Body.Statements[0], Is.InstanceOf<JsEmptyStatement>());
		}

		[Test]
		public void Program() {
			var stmts = JavaScriptParser.Parser.ParseProgram("x;y;");
			Assert.That(stmts.Count, Is.EqualTo(2));
			Assert.That(OutputFormatter.Format(stmts[0]).Replace("\r\n", "\n"), Is.EqualTo("x;\n"));
			Assert.That(OutputFormatter.Format(stmts[1]).Replace("\r\n", "\n"), Is.EqualTo("y;\n"));
		}

		[Test]
		public void GotoStatement() {
			var stmt = ParseStatement<JsGotoStatement>("goto lbl;");
			Assert.That(stmt.TargetLabel, Is.EqualTo("lbl"));
		}
	}
}
