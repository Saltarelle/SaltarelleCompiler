using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.GotoTests
{
	[TestFixture]
	class IfStatementTests : StateMachineRewriterTestBase {
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
	}
}
