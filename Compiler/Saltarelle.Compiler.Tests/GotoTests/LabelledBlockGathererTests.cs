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
		public void FirstStatementIsLabel() {
			AssertCorrect(@"
{
	lbl: a;
}", 
@"
--$0
goto lbl;

--lbl
a;
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
		public void DoWhileWithBreak1() {
			AssertCorrect(@"
{
	do {
		a;
		lbl1:
		b;
		break;
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
goto $2;

--$2
d;
goto $exit;

--lbl1
b;
goto $2;
");
		}

		[Test]
		public void DoWhileWithBreak2() {
			AssertCorrect(@"
{
	do {
		a;
		lbl1:
		b;
		break;
	} while (c);
	lbl2: d;
}", 
@"
--$0
a;
goto lbl1;

--$1
if (c) {
	goto $0;
}
goto lbl2;

--lbl1
b;
goto lbl2;

--lbl2
d;
goto $exit;
");
		}
	
		[Test]
		public void DoWhileWithBreak3() {
			AssertCorrect(@"
{
	do {
		a;
		lbl1:
		b;
		break;
	} while (c);
}", 
@"
--$0
a;
goto lbl1;

--$1
if (c) {
	goto $0;
}
goto $exit;

--lbl1
b;
goto $exit;
");
		}

		[Test]
		public void DoWhileWithContinue() {
			AssertCorrect(@"
{
	do {
		a;
		lbl1:
		b;
		continue;
		c;
	} while (c);
}", 
@"
--$0
a;
goto lbl1;

--$1
if (c) {
	goto $0;
}
goto $exit;

--lbl1
b;
goto $1;
c;
goto $1;
");
		}

		[Test]
		public void While1() {
			AssertCorrect(@"
{
	while (a) {
		b;
		lbl1:
		c;
	}
	d;
}", 
@"
--$0
if (!a) {
	goto $1;
}
b;
goto lbl1;

--$1
d;
goto $exit;

--lbl1
c;
goto $0;
");
		}

		[Test]
		public void While2() {
			AssertCorrect(@"
{
	a;
	while (b) {
		c;
		lbl1:
		d;
	}
	e;
}", 
@"
--$0
a;
goto $1;

--$1
if (!b) {
	goto $2;
}
c;
goto lbl1;

--$2
e;
goto $exit;

--lbl1
d;
goto $1;
");
		}

		[Test]
		public void While3() {
			AssertCorrect(@"
{
	while (a) {
		b;
		lbl1:
		c;
	}
}", 
@"
--$0
if (!a) {
	goto $exit;
}
b;
goto lbl1;

--lbl1
c;
goto $0;
");
		}

		[Test]
		public void While4() {
			AssertCorrect(@"
{
	while (a) {
		b;
		lbl1:
		c;
	}
	lbl2: d;
}", 
@"
--$0
if (!a) {
	goto lbl2;
}
b;
goto lbl1;

--lbl1
c;
goto $0;

--lbl2
d;
goto $exit;
");
		}

		[Test]
		public void WhileWithBreak() {
			AssertCorrect(@"
{
	while (a) {
		b;
		lbl1:
		c;
		break;
	}
	d;
}", 
@"
--$0
if (!a) {
	goto $1;
}
b;
goto lbl1;

--$1
d;
goto $exit;

--lbl1
c;
goto $1;
");
		}

		[Test]
		public void WhileWithContinue() {
			AssertCorrect(@"
{
	while (a) {
		b;
		lbl1:
		c;
		continue;
	}
	d;
}", 
@"
--$0
if (!a) {
	goto $1;
}
b;
goto lbl1;

--$1
d;
goto $exit;

--lbl1
c;
goto $0;
");
		}

		[Test]
		public void ForStatement1() {
			AssertCorrect(@"
{
	for (a; b; c) {
		d;
		lbl1:
		e;
	}
	f;
}", 
@"
--$0
a;
goto $1;

--$1
if (!b) {
	goto $3;
}
d;
goto lbl1;

--$2
c;
goto $1;

--$3
f;
goto $exit;

--lbl1
e;
goto $2;
");
		}

		[Test]
		public void ForStatement2() {
			AssertCorrect(@"
{
	for (a; b; c) {
		d;
		lbl1:
		e;
	}
}", 
@"
--$0
a;
goto $1;

--$1
if (!b) {
	goto $exit;
}
d;
goto lbl1;

--$2
c;
goto $1;

--lbl1
e;
goto $2;
");
		}

		[Test]
		public void ForStatement3() {
			AssertCorrect(@"
{
	for (a; b; c) {
		d;
		lbl1:
		e;
	}
	lbl2: f;
}", 
@"
--$0
a;
goto $1;

--$1
if (!b) {
	goto lbl2;
}
d;
goto lbl1;

--$2
c;
goto $1;

--lbl1
e;
goto $2;

--lbl2
f;
goto $exit;
");
		}

		[Test]
		public void ForStatementWithoutInitializer1() {
			AssertCorrect(@"
{
	a;
	for (; b; c) {
		d;
		lbl1:
		e;
	}
	f;
}", 
@"
--$0
a;
goto $1;

--$1
if (!b) {
	goto $3;
}
d;
goto lbl1;

--$2
c;
goto $1;

--$3
f;
goto $exit;

--lbl1
e;
goto $2;
");
		}

		[Test]
		public void ForStatementWithoutInitializer2() {
			AssertCorrect(@"
{
	for (; b; c) {
		d;
		lbl1:
		e;
	}
	f;
}", 
@"
--$0
if (!b) {
	goto $2;
}
d;
goto lbl1;

--$1
c;
goto $0;

--$2
f;
goto $exit;

--lbl1
e;
goto $1;
");
		}

		[Test]
		public void ForStatementWithoutInitializer3() {
			AssertCorrect(@"
{
	a;
	lbl2:
	for (; b; c) {
		d;
		lbl1:
		e;
	}
	f;
}", 
@"
--$0
a;
goto lbl2;

--$1
c;
goto lbl2;

--$2
f;
goto $exit;

--lbl1
e;
goto $1;

--lbl2
if (!b) {
	goto $2;
}
d;
goto lbl1;
");
		}

		[Test]
		public void ForStatementWithoutTest() {
			AssertCorrect(@"
{
	for (a; ; c) {
		d;
		lbl1:
		e;
	}
}", 
@"
--$0
a;
goto $1;

--$1
d;
goto lbl1;

--$2
c;
goto $1;

--lbl1
e;
goto $2;
");
		}

		[Test]
		public void ForStatementWithoutIncrementer() {
			AssertCorrect(@"
{
	for (a; b; ) {
		d;
		lbl1:
		e;
	}
}", 
@"
--$0
a;
goto $1;

--$1
if (!b) {
	goto $exit;
}
d;
goto lbl1;

--lbl1
e;
goto $1;
");
		}

		[Test]
		public void ForEverStatement() {
			AssertCorrect(@"
{
	for (;; ) {
		d;
		lbl1:
		e;
	}
}", 
@"
--$0
d;
goto lbl1;

--lbl1
e;
goto $0;
");
		}

		[Test]
		public void ContinueInForStatement() {
			AssertCorrect(@"
{
	for (a; b; c) {
		d;
		lbl1:
		e;
		continue;
	}
	f;
}", 
@"
--$0
a;
goto $1;

--$1
if (!b) {
	goto $3;
}
d;
goto lbl1;

--$2
c;
goto $1;

--$3
f;
goto $exit;

--lbl1
e;
goto $2;
");
		}

		[Test]
		public void ContinueInForStatementWitoutIncrementer() {
			AssertCorrect(@"
{
	for (a; b; ) {
		d;
		lbl1:
		e;
		continue;
	}
	f;
}", 
@"
--$0
a;
goto $1;

--$1
if (!b) {
	goto $2;
}
d;
goto lbl1;

--$2
f;
goto $exit;

--lbl1
e;
goto $1;
");
		}

		[Test]
		public void ContinueInForStatementWitoutTest() {
			AssertCorrect(@"
{
	for (a; ; c) {
		d;
		lbl1:
		e;
		continue;
	}
	f;
}", 
@"
--$0
a;
goto $1;

--$1
d;
goto lbl1;

--$2
c;
goto $1;

--$3
f;
goto $exit;

--lbl1
e;
goto $2;
");
		}

		[Test]
		public void ContinueInForEverStatement() {
			AssertCorrect(@"
{
	for (;;) {
		d;
		lbl1:
		e;
		continue;
	}
	f;
}", 
@"
--$0
d;
goto lbl1;

--$1
f;
goto $exit;

--lbl1
e;
goto $0;
");
		}

		[Test]
		public void BreakInForStatement() {
			AssertCorrect(@"
{
	for (a; b; c) {
		d;
		lbl1:
		e;
		break;
	}
	f;
}", 
@"
--$0
a;
goto $1;

--$1
if (!b) {
	goto $3;
}
d;
goto lbl1;

--$2
c;
goto $1;

--$3
f;
goto $exit;

--lbl1
e;
goto $3;
");
		}

		[Test]
		public void NestedForStatements() {
			AssertCorrect(@"
{
	for (a; b; c) {
		for (d; e; f) {
			g;
			lbl1:
			h;
		}
	}
	i;
}", 
@"
--$0
a;
goto $1;

--$1
if (!b) {
	goto $3;
}
d;
goto $4;

--$2
c;
goto $1;

--$3
i;
goto $exit;

--$4
if (!e) {
	goto $2;
}
g;
goto lbl1;

--$5
f;
goto $4;

--lbl1
h;
goto $5;
");
		}

		[Test]
		public void SwitchStatement1() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
		case c:
			d;
			lbl1:
			e;
			break;
		case f:
			g;
			break;
		case h:
		case i:
			j;
			break;
		case k:
			l;
			lbl2:
			m;
			break;
	}
	n;
}", 
@"
--$0
if (a === b || a === c) {
	d;
	goto lbl1;
}
else if (a === f) {
	g;
}
else if (a === h || a === i) {
	j;
}
else if (a === k) {
	l;
	goto lbl2;
}
goto $1;

--$1
n;
goto $exit;

--lbl1
e;
goto $1;

--lbl2
m;
goto $1;
");
		}

		[Test]
		public void SwitchStatement2() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
		case c:
			d;
			lbl1:
			e;
			break;
		case f:
			g;
			break;
		case h:
		case i:
			j;
			break;
		case k:
			l;
			lbl2:
			m;
			break;
	}
}", 
@"
--$0
if (a === b || a === c) {
	d;
	goto lbl1;
}
else if (a === f) {
	g;
}
else if (a === h || a === i) {
	j;
}
else if (a === k) {
	l;
	goto lbl2;
}
goto $exit;

--lbl1
e;
goto $exit;

--lbl2
m;
goto $exit;
");
		}

		[Test]
		public void SwitchStatement3() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
		case c:
			d;
			lbl1:
			e;
			break;
		case f:
			g;
			break;
		case h:
		case i:
			j;
			break;
		case k:
			l;
			lbl2:
			m;
			break;
	}
	lbl3: n;
}", 
@"
--$0
if (a === b || a === c) {
	d;
	goto lbl1;
}
else if (a === f) {
	g;
}
else if (a === h || a === i) {
	j;
}
else if (a === k) {
	l;
	goto lbl2;
}
goto lbl3;

--lbl1
e;
goto lbl3;

--lbl2
m;
goto lbl3;

--lbl3
n;
goto $exit;
");
		}
		
		[Test]
		public void SwitchStatementFallthrough1() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			c;
		case d:
			d;
			lbl1:
			e;
	}
	f;
}", 
@"
--$0
if (a === b) {
	c;
	goto $2;
}
else if (a === d) {
	goto $2;
}
goto $1;

--$1
f;
goto $exit;

--$2
d;
goto lbl1;

--lbl1
e;
goto $1;
");
		}

		[Test]
		public void SwitchStatementFallthrough2() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			c;
		case d:
			lbl2: d;
			lbl1:
			e;
	}
	f;
}", 
@"
--$0
if (a === b) {
	c;
	goto lbl2;
}
else if (a === d) {
	goto lbl2;
}
goto $1;

--$1
f;
goto $exit;

--lbl1
e;
goto $1;

--lbl2
d;
goto lbl1;
");
		}

		[Test]
		public void SwitchStatementFallthrough3() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			c;
			lbl1:
			d;
		case e:
			f;
	}
	g;
}", 
@"
--$0
if (a === b) {
	c;
	goto lbl1;
}
else if (a === e) {
	goto $2;
}
goto $1;

--$1
g;
goto $exit;

--$2
f;
goto $1;

--lbl1
d;
goto $2;
");
		}

		[Test]
		public void SwitchStatementWithBreakInTheMiddleOfSection() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			break;
			c;
			break;
		case d:
			d;
			lbl1:
			break;
			e;
			break;
	}
	f;
}", 
@"
--$0
if (a === b) {
	goto $1;
	c;
}
else if (a === d) {
	d;
	goto lbl1;
}
goto $1;

--$1
f;
goto $exit;

--lbl1
goto $1;
e;
goto $1;
");
		}

		[Test]
		public void SwitchStatementWithComplexExpression() {
			Assert.Fail("TODO");
		}

		[Test]
		public void SwitchStatementWithDefault() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			c;
			break;
		case d:
			e;
			lbl1:
			f;
			break;
		default:
			g;
			lbl2:
			h;
			break;
	}
	f;
}", 
@"
--$0
if (a === b) {
	c;
}
else if (a === d) {
	e;
	goto lbl1;
}
else {
	g;
	goto lbl2;
}
goto $1;

--$1
f;
goto $exit;

--lbl1
f;
goto $1;

--lbl2
h;
goto $1;
");
		}

		[Test]
		public void SwitchStatementWithDefaultGroupedWithOtherCases() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			c;
			break;
		case d:
			e;
			lbl1:
			f;
			break;
		case 1:
		default:
		case 2:
			g;
			lbl2:
			h;
			break;
	}
	f;
}", 
@"
--$0
if (a === b) {
	c;
}
else if (a === d) {
	e;
	goto lbl1;
}
else {
	g;
	goto lbl2;
}
goto $1;

--$1
f;
goto $exit;

--lbl1
f;
goto $1;

--lbl2
h;
goto $1;
");
		}

		[Test]
		public void SwitchStatementWithDefaultInTheMiddle() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			c;
			break;
		default:
			g;
			lbl2:
			h;
			break;
		case d:
			e;
			lbl1:
			f;
			break;
	}
	f;
}", 
@"
--$0
if (a === b) {
	c;
}
else if (a === d) {
	e;
	goto lbl1;
}
else {
	g;
	goto lbl2;
}
goto $1;

--$1
f;
goto $exit;

--lbl1
f;
goto $1;

--lbl2
h;
goto $1;
");
		}
	}
}
