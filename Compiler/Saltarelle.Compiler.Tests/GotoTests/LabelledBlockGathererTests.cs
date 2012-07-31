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
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
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
	goto lbl2;
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
goto lbl2;

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
		public void BlockEndingWithThrowIsNotDoubleConnected() {
			AssertCorrect(@"
{
	a;
	b;
	throw c;
lbl1:
	d;
	e;
}", 
@"
--$0
a;
b;
throw c;

--lbl1
d;
e;
goto $exit;
");
		}
		
		[Test]
		public void BlockEndingWithReturnIsNotDoubleConnected() {
			AssertCorrect(@"
{
	a;
	b;
	return;
lbl1:
	c;
	d;
}", 
@"
--$0
a;
b;
return;

--lbl1
c;
d;
goto $exit;
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
		public void IfStatement2() {
			AssertCorrect(@"
{
	if (x) {
		a;
		b;
	lbl1:
		c;
		d;
	}
	e;
}", 
@"
--$0
if (x) {
	a;
	b;
	goto lbl1;
}
goto $1;

--$1
e;
goto $exit;

--lbl1
c;
d;
goto $1;
");
		}

		[Test]
		public void IfStatement3() {
			AssertCorrect(@"
{
	if (x) {
		a;
		b;
	lbl1:
		c;
		d;
	}
	lbl2: e;
}", 
@"
--$0
if (x) {
	a;
	b;
	goto lbl1;
}
goto lbl2;

--lbl1
c;
d;
goto lbl2;

--lbl2
e;
goto $exit;
");
		}

		[Test]
		public void IfElse() {
			AssertCorrect(@"
{
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
}
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

--$1
e;
goto $exit;

--lbl1
b;
goto $1;

--lbl2
d;
goto $1;
");
		}

		[Test]
		public void IfElseWithNoLabelInThen() {
			AssertCorrect(@"
{
	if (x) {
		a;
	}
	else {
		b;
	lbl1:
		c;
	}
	d;
}
", 
@"
--$0
if (x) {
	a;
	goto $1;
}
else {
	b;
	goto lbl1;
}

--$1
d;
goto $exit;

--lbl1
c;
goto $1;
");
		}

		[Test]
		public void IfElseWithNoLabelInElse() {
			AssertCorrect(@"
{
	if (x) {
		a;
	lbl1:
		b;
	}
	else {
		c;
	}
	d;
}
", 
@"
--$0
if (x) {
	a;
	goto lbl1;
}
else {
	c;
	goto $1;
}

--$1
d;
goto $exit;

--lbl1
b;
goto $1;
");
		}

		[Test]
		public void NestedIfElse() {
			// No this assertion is not really correct.
			AssertCorrect(@"
{
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
}", 
@"
--$0
if (x) {
	a;
	if (y) {
		b;
		goto lbl1;
	}
	else {
		d;
		goto lbl2;
	}
}
else {
	g;
	if (z) {
		h;
		goto lbl3;
	}
	else {
		j;
		goto lbl4;
	}
}

--$1
m;
goto $exit;

--$2
f;
goto $1;

--$3
l;
goto $1;

--lbl1
c;
goto $2;

--lbl2
e;
goto $2;

--lbl3
i;
goto $3;

--lbl4
k;
goto $3;
");
		}

		[Test]
		public void DoWhile1() {
			AssertCorrect(@"
{
	do {
		a;
		lbl1:
		b;
	} while (c);
	d;
}", 
@"
--$0
a;
goto lbl1;

--$1
if (c) {
	goto $0;
}
d;
goto $exit;

--lbl1
b;
goto $1;
");
		}

		[Test]
		public void DoWhile2() {
			AssertCorrect(@"
{
	x;
	do {
		a;
		lbl1:
		b;
	} while (c);
	d;
}", 
@"
--$0
x;
goto $1;

--$1
a;
goto lbl1;

--$2
if (c) {
	goto $1;
}
d;
goto $exit;

--lbl1
b;
goto $2;
");
		}

		[Test]
		public void DoWhile3() {
			AssertCorrect(@"
{
	x;
	before:
	do {
		a;
		lbl1:
		b;
	} while (c);
	d;
}", 
@"
--$0
x;
goto before;

--$1
if (c) {
	goto before;
}
d;
goto $exit;

--before
a;
goto lbl1;

--lbl1
b;
goto $1;
");
		}

		[Test]
		public void DoWhileWithBreak() {
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
	}
}
