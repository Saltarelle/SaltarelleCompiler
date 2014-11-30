using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	public class WhileStatementTests : StateMachineRewriterTestBase {
		[Test]
		public void While1() {
			AssertCorrect(@"
{
	//@ 1
	while (a) {
		//@ 2
		b;
		lbl1:
		//@ 3
		c;
	}
	//@ 4
	d;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				if (!a) {
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				//@ 2
				b;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 3
				c;
				//@ none
				$state1 = 0;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 4
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
		public void While2() {
			AssertCorrect(@"
{
	{
		//@ 1
		while (a) {
			//@ 2
			b;
			lbl1:
			//@ 3
			c;
		}
	}
	//@ 4
	d;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				if (!a) {
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				//@ 2
				b;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 3
				c;
				//@ none
				$state1 = 0;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 4
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
		public void While3() {
			AssertCorrect(@"
{
	//@ 1
	a;
	//@ 2
	while (b) {
		//@ 3
		c;
		lbl1:
		//@ 4
		d;
	}
	//@ 5
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
				a;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 2
				if (!b) {
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				//@ 3
				c;
				//@ none
				$state1 = 3;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 4
				d;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 5
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
		public void While4() {
			AssertCorrect(@"
{
	//@ 1
	while (a) {
		//@ 2
		b;
		lbl1:
		//@ 3
		c;
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
				if (!a) {
					$state1 = -1;
					break $loop1;
					//@ none
				}
				//@ 2
				b;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 3
				c;
				//@ none
				$state1 = 0;
				continue $loop1;
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
		public void While5() {
			AssertCorrect(@"
{
	//@ 1
	while (a) {
		//@ 2
		b;
		lbl1:
		//@ 3
		c;
	}
	lbl2:
	//@ 4
	d;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				if (!a) {
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				//@ 2
				b;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 3
				c;
				//@ none
				$state1 = 0;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 4
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
		public void WhileWithBreak() {
			AssertCorrect(@"
{
	//@ 1
	while (a) {
		//@ 2
		b;
		lbl1:
		//@ 3
		c;
		//@ 4
		break;
	}
	//@ 5
	d;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				if (!a) {
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				//@ 2
				b;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 3
				c;
				//@ 4
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
		public void WhileWithContinue() {
			AssertCorrect(@"
{
	//@ 1
	while (a) {
		//@ 2
		b;
		lbl1:
		//@ 3
		c;
		//@ 4
		continue;
	}
	//@ 5
	d;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				if (!a) {
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				//@ 2
				b;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 3
				c;
				//@ 4
				$state1 = 0;
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
	}
}
