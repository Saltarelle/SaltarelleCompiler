using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.GotoRewrite;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests.GotoTests {
	[TestFixture]
	public class StateMachineRewriterTests {
		private void AssertCorrect(string orig, string expected, bool isIteratorBlock = false) {
			int nextTempIndex = 0;
			var stmt = JsBlockStatement.MakeBlock(JavaScriptParser.Parser.ParseStatement(orig));
			var result = StateMachineRewriter.Rewrite(stmt, e => e.NodeType != ExpressionNodeType.Identifier, () => "$tmp" + (++nextTempIndex).ToString(CultureInfo.InvariantCulture), v => JsExpression.Invocation(JsExpression.Identifier("setCurrent"), v), isIteratorBlock: true);
			var actual = OutputFormatter.Format(result);
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				b;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				c;
				d;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				e;
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void FirstStatementIsLabel() {
			AssertCorrect(@"
{
	lbl: a;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				a;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				b;
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				c;
				d;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				e;
				f;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				b;
				throw c;
			}
			case 1: {
				d;
				e;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				b;
				return;
			}
			case 1: {
				c;
				d;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				b;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				{
					c;
				}
				d;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				e;
				{
					f;
					{
						g;
						$tmp1 = 3;
						continue $loop1;
					}
				}
			}
			case 3: {
				h;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (x) {
					a;
					b;
					$tmp1 = 1;
					continue $loop1;
				}
				break $loop1;
			}
			case 1: {
				c;
				d;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (x) {
					a;
					b;
					$tmp1 = 2;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				c;
				d;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				e;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (x) {
					a;
					b;
					$tmp1 = 2;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				c;
				d;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				e;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (x) {
					a;
					$tmp1 = 2;
					continue $loop1;
				}
				else {
					c;
					$tmp1 = 3;
					continue $loop1;
				}
			}
			case 2: {
				b;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				d;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				e;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (x) {
					a;
					$tmp1 = 1;
					continue $loop1;
				}
				else {
					b;
					$tmp1 = 2;
					continue $loop1;
				}
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				d;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (x) {
					a;
					$tmp1 = 2;
					continue $loop1;
				}
				else {
					c;
					$tmp1 = 1;
					continue $loop1;
				}
			}
			case 2: {
				b;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				d;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void NestedIfElse() {
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (x) {
					a;
					if (y) {
						b;
						$tmp1 = 3;
						continue $loop1;
					}
					else {
						d;
						$tmp1 = 4;
						continue $loop1;
					}
				}
				else {
					g;
					if (z) {
						h;
						$tmp1 = 6;
						continue $loop1;
					}
					else {
						j;
						$tmp1 = 7;
						continue $loop1;
					}
				}
			}
			case 3: {
				c;
				$tmp1 = 2;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				f;
				$tmp1 = 1;
				continue $loop1;
			}
			case 6: {
				i;
				$tmp1 = 5;
				continue $loop1;
			}
			case 7: {
				k;
				$tmp1 = 5;
				continue $loop1;
			}
			case 5: {
				l;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				m;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				b;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (c) {
					$tmp1 = 0;
					continue $loop1;
				}
				d;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				x;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				a;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				if (c) {
					$tmp1 = 1;
					continue $loop1;
				}
				d;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				x;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				a;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				if (c) {
					$tmp1 = 1;
					continue $loop1;
				}
				d;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				d;
				break $loop1;
			}
			case 1: {
				if (c) {
					$tmp1 = 0;
					continue $loop1;
				}
				$tmp1 = 2;
				continue $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 1: {
				if (c) {
					$tmp1 = 0;
					continue $loop1;
				}
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				d;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				b;
				break $loop1;
			}
			case 1: {
				if (c) {
					$tmp1 = 0;
					continue $loop1;
				}
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				b;
				$tmp1 = 1;
				continue $loop1;
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (c) {
					$tmp1 = 0;
					continue $loop1;
				}
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!a) {
					$tmp1 = 1;
					continue $loop1;
				}
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 0;
				continue $loop1;
			}
			case 1: {
				d;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void While2() {
			AssertCorrect(@"
{
	{
		while (a) {
			b;
			lbl1:
			c;
		}
	}
	d;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!a) {
					$tmp1 = 1;
					continue $loop1;
				}
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 0;
				continue $loop1;
			}
			case 1: {
				d;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void While3() {
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					$tmp1 = 2;
					continue $loop1;
				}
				c;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				d;
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				break $loop1;
			}
		}
	}
}
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
}",
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!a) {
					break $loop1;
				}
				b;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				c;
				$tmp1 = 0;
				continue $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void While5() {
			AssertCorrect(@"
{
	while (a) {
		b;
		lbl1:
		c;
	}
	lbl2: d;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!a) {
					$tmp1 = 1;
					continue $loop1;
				}
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 0;
				continue $loop1;
			}
			case 1: {
				d;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!a) {
					$tmp1 = 1;
					continue $loop1;
				}
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				d;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!a) {
					$tmp1 = 1;
					continue $loop1;
				}
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 0;
				continue $loop1;
			}
			case 1: {
				d;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ForStatement2() {
			AssertCorrect(@"
{
	{
		for (a; b; c) {
			d;
			lbl1:
			e;
		}
	}
	f;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
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
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					break $loop1;
				}
				d;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ForStatement4() {
			AssertCorrect(@"
{
	for (a; b; c) {
		d;
		lbl1:
		e;
	}
	lbl2: f;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!b) {
					$tmp1 = 2;
					continue $loop1;
				}
				d;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				c;
				$tmp1 = 0;
				continue $loop1;
			}
			case 2: {
				f;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				d;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					break $loop1;
				}
				d;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				d;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				e;
				$tmp1 = 0;
				continue $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					$tmp1 = 2;
					continue $loop1;
				}
				d;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				f;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				d;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				e;
				$tmp1 = 0;
				continue $loop1;
			}
			case 1: {
				f;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 3;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				if (!e) {
					$tmp1 = 2;
					continue $loop1;
				}
				g;
				$tmp1 = 6;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				i;
				break $loop1;
			}
			case 6: {
				h;
				$tmp1 = 5;
				continue $loop1;
			}
			case 5: {
				f;
				$tmp1 = 4;
				continue $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b || a === c) {
					d;
					$tmp1 = 2;
					continue $loop1;
				}
				else if (a === f) {
					g;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === h || a === i) {
					j;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === k) {
					l;
					$tmp1 = 3;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				m;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				n;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void SwitchStatement2() {
			AssertCorrect(@"
{
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
	}
	n;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b || a === c) {
					d;
					$tmp1 = 2;
					continue $loop1;
				}
				else if (a === f) {
					g;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === h || a === i) {
					j;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === k) {
					l;
					$tmp1 = 3;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				m;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				n;
				break $loop1;
			}
		}
	}
}
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
}",
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b || a === c) {
					d;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === f) {
					g;
					break $loop1;
				}
				else if (a === h || a === i) {
					j;
					break $loop1;
				}
				else if (a === k) {
					l;
					$tmp1 = 2;
					continue $loop1;
				}
				break $loop1;
			}
			case 1: {
				e;
				break $loop1;
			}
			case 2: {
				m;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void SwitchStatement4() {
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b || a === c) {
					d;
					$tmp1 = 2;
					continue $loop1;
				}
				else if (a === f) {
					g;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === h || a === i) {
					j;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === k) {
					l;
					$tmp1 = 3;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				m;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				n;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					c;
					$tmp1 = 2;
					continue $loop1;
				}
				else if (a === d) {
					$tmp1 = 2;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				d;
				$tmp1 = 3;
				continue $loop1;
			}
			case 1: {
				f;
				break $loop1;
			}
			case 3: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					c;
					$tmp1 = 2;
					continue $loop1;
				}
				else if (a === d) {
					$tmp1 = 2;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				d;
				$tmp1 = 3;
				continue $loop1;
			}
			case 1: {
				f;
				break $loop1;
			}
			case 3: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					c;
					$tmp1 = 3;
					continue $loop1;
				}
				else if (a === e) {
					$tmp1 = 2;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				d;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				f;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				g;
				break $loop1;
			}
		}
	}
}
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
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					$tmp1 = 1;
					continue $loop1;
					c;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === d) {
					d;
					$tmp1 = 2;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				$tmp1 = 1;
				continue $loop1;
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void SwitchStatementWithComplexExpression() {
			AssertCorrect(@"
{
	switch (a + b) {
		case c:
			d;
			lbl1:
			e;
			break;
		case f:
		case g:
			h;
			break;
	}
	n;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				var $tmp2 = a + b;
				if ($tmp2 === c) {
					d;
					$tmp1 = 2;
					continue $loop1;
				}
				else if ($tmp2 === f || $tmp2 === g) {
					h;
					$tmp1 = 1;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				n;
				break $loop1;
			}
		}
	}
}
");
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
	i;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					c;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === d) {
					e;
					$tmp1 = 2;
					continue $loop1;
				}
				else {
					g;
					$tmp1 = 3;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				f;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				h;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				i;
				break $loop1;
			}
		}
	}
}
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
	i;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					c;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === d) {
					e;
					$tmp1 = 2;
					continue $loop1;
				}
				else {
					g;
					$tmp1 = 3;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				f;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				h;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				i;
				break $loop1;
			}
		}
	}
}
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
	i;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					c;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === d) {
					e;
					$tmp1 = 3;
					continue $loop1;
				}
				else {
					g;
					$tmp1 = 2;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				h;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				i;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void IteratorBlockWithoutYieldBreak() {
			AssertCorrect(@"
{
	a;
	yield return b;
	c;
	yield return d;
lbl1:
	e;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				$tmp1 = -1;
				a;
				setCurrent(b);
				$tmp1 = 1;
				return true;
			}
			case 1: {
				$tmp1 = -1;
				c;
				setCurrent(d);
				$tmp1 = 2;
				return true;
			}
			case 2: {
				$tmp1 = -1;
				e;
				break $loop1;
			}
		}
	}
	return false;
}
", isIteratorBlock: true);
		}

		[Test]
		public void IteratorBlockWithYieldBreak() {
			AssertCorrect(@"
{
	a;
	yield return b;
	c;
	yield break;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				$tmp1 = -1;
				a;
				setCurrent(b);
				$tmp1 = 1;
				return true;
			}
			case 1: {
				$tmp1 = -1;
				c;
				return false;
			}
		}
	}
	return false;
}
", isIteratorBlock: true);
		}

		[Test]
		public void IteratorBlockWithYieldBreakWithStuffFollowing() {
			AssertCorrect(@"
{
	a;
	yield return b;
	c;
	yield break;
	d;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				$tmp1 = -1;
				a;
				setCurrent(b);
				$tmp1 = 1;
				return true;
			}
			case 1: {
				$tmp1 = -1;
				c;
				return false;
			}
			case 2: {
				$tmp1 = -1;
				d;
				break $loop1;
			}
		}
	}
	return false;
}
", isIteratorBlock: true);
		}
	}
}
