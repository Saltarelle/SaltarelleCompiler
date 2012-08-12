using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (x) {
					a;
					b;
					$state1 = 1;
					continue $loop1;
				}
				$state1 = -1;
				break $loop1;
			}
			case 1: {
				c;
				d;
				$state1 = -1;
				break $loop1;
			}
			default: {
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (x) {
					a;
					b;
					$state1 = 2;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				c;
				d;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				e;
				$state1 = -1;
				break $loop1;
			}
			default: {
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (x) {
					a;
					b;
					$state1 = 2;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				c;
				d;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				e;
				$state1 = -1;
				break $loop1;
			}
			default: {
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (x) {
					a;
					$state1 = 2;
					continue $loop1;
				}
				else {
					c;
					$state1 = 3;
					continue $loop1;
				}
			}
			case 2: {
				b;
				$state1 = 1;
				continue $loop1;
			}
			case 3: {
				d;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				e;
				$state1 = -1;
				break $loop1;
			}
			default: {
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (x) {
					a;
					$state1 = 1;
					continue $loop1;
				}
				else {
					b;
					$state1 = 2;
					continue $loop1;
				}
			}
			case 2: {
				c;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				d;
				$state1 = -1;
				break $loop1;
			}
			default: {
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (x) {
					a;
					$state1 = 2;
					continue $loop1;
				}
				else {
					c;
					$state1 = 1;
					continue $loop1;
				}
			}
			case 2: {
				b;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				d;
				$state1 = -1;
				break $loop1;
			}
			default: {
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (x) {
					a;
					if (y) {
						b;
						$state1 = 3;
						continue $loop1;
					}
					else {
						d;
						$state1 = 4;
						continue $loop1;
					}
				}
				else {
					g;
					if (z) {
						h;
						$state1 = 6;
						continue $loop1;
					}
					else {
						j;
						$state1 = 7;
						continue $loop1;
					}
				}
			}
			case 3: {
				c;
				$state1 = 2;
				continue $loop1;
			}
			case 4: {
				e;
				$state1 = 2;
				continue $loop1;
			}
			case 2: {
				f;
				$state1 = 1;
				continue $loop1;
			}
			case 6: {
				i;
				$state1 = 5;
				continue $loop1;
			}
			case 7: {
				k;
				$state1 = 5;
				continue $loop1;
			}
			case 5: {
				l;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				m;
				$state1 = -1;
				break $loop1;
			}
			default: {
				break $loop1;
			}
		}
	}
}
");
		}
	}
}
