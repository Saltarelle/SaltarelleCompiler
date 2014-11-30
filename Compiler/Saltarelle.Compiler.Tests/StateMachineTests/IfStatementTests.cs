using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	class IfStatementTests : StateMachineRewriterTestBase {
		[Test]
		public void IfStatement1() {
			AssertCorrect(@"
{
	//@ 1
	if (x) {
		//@ 2
		a;
		//@ 3
		b;
	lbl1:
		//@ 4
		c;
		//@ 5
		d;
	}
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				if (x) {
					//@ 2
					a;
					//@ 3
					b;
					//@ none
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			case 1: {
				//@ 4
				c;
				//@ 5
				d;
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			default: {
				//@ none
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
	//@ 1
	if (x) {
		//@ 2
		a;
		//@ 3
		b;
	lbl1:
		//@ 4
		c;
		//@ 5
		d;
	}
	//@ 6
	e;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				if (x) {
					//@ 2
					a;
					//@ 3
					b;
					//@ none
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 4
				c;
				//@ 5
				d;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 6
				e;
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			default: {
				//@ none
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
	//@ 1
	if (x) {
		//@ 2
		a;
		//@ 3
		b;
	lbl1:
		//@ 4
		c;
		//@ 5
		d;
	}
	lbl2:
	//@ 6
	e;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				if (x) {
					//@ 2
					a;
					//@ 3
					b;
					//@ none
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 4
				c;
				//@ 5
				d;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 6
				e;
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			default: {
				//@ none
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
	//@ 1
	if (x) {
		//@ 2
		a;
	lbl1:
		//@ 3
		b;
	}
	else {
		//@ 4
		c;
	lbl2:
		//@ 5
		d;
	}
	//@ 6
	e;
}
", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				if (x) {
					//@ 2
					a;
					//@ none
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				else {
					//@ 4
					c;
					//@ none
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				//@ none
			}
			case 2: {
				//@ 3
				b;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 5
				d;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 6
				e;
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			default: {
				//@ none
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
	//@ 1
	if (x) {
		//@ 2
		a;
	}
	else {
		//@ 3
		b;
	lbl1:
		//@ 4
		c;
	}
	//@ 5
	d;
}
", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				if (x) {
					//@ 2
					a;
					//@ none
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				else {
					//@ 3
					b;
					//@ none
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				//@ none
			}
			case 2: {
				//@ 4
				c;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 5
				d;
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			default: {
				//@ none
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
	//@ 1
	if (x) {
		//@ 2
		a;
	lbl1:
		//@ 3
		b;
	}
	else {
		//@ 4
		c;
	}
	//@ 5
	d;
}
", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				if (x) {
					//@ 2
					a;
					//@ none
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				else {
					//@ 4
					c;
					//@ none
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				//@ none
			}
			case 2: {
				//@ 3
				b;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 5
				d;
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			default: {
				//@ none
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
	//@ 1
	if (x) {
		//@ 2
		a;
		//@ 3
		if (y) {
			//@ 4
			b;
			lbl1:
			//@ 5
			c;
		}
		else {
			//@ 6
			d;
			lbl2:
			//@ 7
			e;
		}
		//@ 8
		f;
	}
	else {
		//@ 9
		g;
		//@ 10
		if (z) {
			//@ 11
			h;
			lbl3:
			//@ 12
			i;
		}
		else {
			//@ 13
			j;
			lbl4:
			//@ 14
			k;
		}
		//@ 15
		l;
	}
	//@ 16
	m;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				if (x) {
					//@ 2
					a;
					//@ 3
					if (y) {
						//@ 4
						b;
						//@ none
						$state1 = 3;
						continue $loop1;
						//@ none
					}
					else {
						//@ 6
						d;
						//@ none
						$state1 = 4;
						continue $loop1;
						//@ none
					}
					//@ none
				}
				else {
					//@ 9
					g;
					//@ 10
					if (z) {
						//@ 11
						h;
						//@ none
						$state1 = 6;
						continue $loop1;
						//@ none
					}
					else {
						//@ 13
						j;
						//@ none
						$state1 = 7;
						continue $loop1;
						//@ none
					}
					//@ none
				}
				//@ none
			}
			case 3: {
				//@ 5
				c;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 4: {
				//@ 7
				e;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 8
				f;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 6: {
				//@ 12
				i;
				//@ none
				$state1 = 5;
				continue $loop1;
				//@ none
			}
			case 7: {
				//@ 14
				k;
				//@ none
				$state1 = 5;
				continue $loop1;
				//@ none
			}
			case 5: {
				//@ 15
				l;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 16
				m;
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			default: {
				//@ none
				break $loop1;
			}
		}
	}
}
");
		}
	}
}
