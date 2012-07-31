using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.GotoRewrite;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests.GotoTests {
	[TestFixture]
	public class LabelledBlockGathererTests {
		private void AssertCorrect(string orig, string expected) {
			var stmt = JsBlockStatement.MakeBlock(JavaScriptParser.Parser.ParseStatement(orig));
			var blocks = new LabelledBlockGatherer().Gather(stmt);
			var actual = string.Join("", blocks.OrderBy(b => b.Name).Select(b => Environment.NewLine + "--" + b.Name + Environment.NewLine + b.Statements.Aggregate("", (old, s) => old + OutputFormatter.Format(s))));
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		[Test]
		public void SimpleBlockStatementWorks() {
			AssertCorrect(@"
{
	a;
	b;
lbl1:
	c;
	d;
lbl2:
	e;
	f;
}", 
@"
--$0
a;
b;
goto lbl1;

--lbl1
c;
d;
goto lbl2;

--lbl2
e;
f;
goto $exit;
");
		}

		[Test]
		public void BlockEndingWithGotoIsNotDoubleConnected() {
			AssertCorrect(@"
{
	a;
	b;
	goto lbl1;
lbl1:
	c;
	d;
}", 
@"
--$0
a;
b;
goto lbl1;

--lbl1
c;
d;
goto $exit;
");
		}

		[Test]
		public void BlockEndingWithThrowIsNotDoubleConnected() {
			AssertCorrect(@"
{
	a;
	b;
	throw c;
}", 
@"
--$0
a;
b;
throw c;
");
		}
		
		[Test]
		public void BlockEndingWithReturnIsNotDoubleConnected() {
			AssertCorrect(@"
{
	a;
	b;
	return;
}", 
@"
--$0
a;
b;
return;
");
		}

		[Test]
		public void NestingBlockStatementsWorks() {
			AssertCorrect(@"
{
	a;
	{
		b;
		lbl1: {
			c;
		}
		d;
		lbl2:
		e;
	}
	{
		f;
		{
			g;
			goto lbl4;
		}
	}
	lbl4:
	h;
}", 
@"
--$0
a;
b;
goto lbl1;

--lbl1
{
	c;
}
d;
goto lbl2;

--lbl2
e;
{
	f;
	{
		g;
		goto lbl4;
	}
}

--lbl4
h;
goto $exit;
");
		}

		[Test]
		public void IfStatement1() {
			AssertCorrect(@"
if (x) {
	a;
	b;
lbl1:
	c;
	d;
}", 
@"
--$0
if (x) {
	a;
	b;
	goto lbl1;
}
goto $exit;

--lbl1
c;
d;
goto $exit;
");
		}

		[Test]
		public void IfElse() {
			AssertCorrect(@"
if (x) {
	a;
lbl1:
	b;
}
else {
	c;
lbl2:
	d;
}
e;
", 
@"
--$0
if (x) {
	a;
	goto lbl1;
}
else {
	c;
	goto lbl2;
}

--lbl1
b;
goto $1;

--lbl2
d;
goto $1;

--$1
e;
goto $exit;
");
		}

		[Test]
		public void NestedIfElse() {
			// No this assertion is not really correct.
			AssertCorrect(@"
if (x) {
	a;
	if (y) {
		b;
lbl1:
		c;
	}
	else {
		d;
lbl2:
		e;
	}
	f;
}
else {
	g;
	if (z) {
		h;
lbl3:
		i;
	}
	else {
		j;
lbl4:
		k;
	}
	l;
}
m;
", 
@"TODO");
		}

		[Test]
		public void NestedIfElse2() {
			// No this assertion is not really correct.
			AssertCorrect(@"
if (x) {
	a;
lbl1:
	if (y) {
		b;
lbl2:
		c;
	}
	else {
		d;
lbl3:
		e;
	}
	f;
}
else {
	g;
lbl4:
	if (z) {
		h;
lbl5:
		i;
	}
	else {
		j;
lbl6:
		k;
	}
	l;
}
m;
", 
@"
--$0
if (x) {
	a;
	goto lbl1;
}
else {
	g;
	goto lbl4;
}

--lbl1
if (y) {
	b;
	goto lbl2;
}
else {
	d;
	goto lbl3;
}

--lbl2
c;
goto $1;

--lbl3
e;
goto $1;

--$1
f;
goto $2;

--$2
m;
goto $exit;

--lbl1
b;
goto $1;

--lbl2
d;
goto $1;

--$1
e;
goto $exit;
");
		}
	}
}
