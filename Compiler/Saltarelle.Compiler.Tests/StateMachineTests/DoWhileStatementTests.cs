using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	public class DoWhileStatementTests : StateMachineRewriterTestBase {
		[Test]
		public void DoWhile1() {
			AssertCorrect(@"
{
	//@ 1
	do {
		//@ 2
		a;
		lbl1:
		//@ 3
		b;
		//@ 4
	} while (c);
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
				//@ 2
				a;
				//@ none
				$state1 = 2;
				continue $loop1;
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
				//@ 4
				if (c) {
					$state1 = 0;
					continue $loop1;
					//@ none
				}
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
		public void DoWhile2() {
			AssertCorrect(@"
{
	//@ 1
	x;
	do {
		//@ 2
		a;
		lbl1:
		//@ 3
		b;
		//@ 4
	} while (c);
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
				x;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 2
				a;
				//@ none
				$state1 = 3;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 3
				b;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 4
				if (c) {
					$state1 = 1;
					continue $loop1;
					//@ none
				}
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
		public void DoWhile3() {
			AssertCorrect(@"
{
	//@ 1
	x;
	before:
	//@ 2
	do {
		//@ 3
		a;
		lbl1:
		//@ 4
		b;
		//@ 5
	} while (c);
	//@ 6
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
				x;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 3
				a;
				//@ none
				$state1 = 3;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 4
				b;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 5
				if (c) {
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				//@ 6
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
		public void DoWhileWithBreak1() {
			AssertCorrect(@"
{
	do {
		//@ 1
		a;
		lbl1:
		//@ 2
		b;
		//@ 3
		break;
		//@ 4
	} while (c);
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
				a;
				//@ none
				$state1 = 3;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 2
				b;
				//@ 3
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 5
				d;
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			case 1: {
				//@ 4
				if (c) {
					$state1 = 0;
					continue $loop1;
					//@ none
				}
				//@ none
				$state1 = 2;
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
		public void DoWhileWithBreak2() {
			AssertCorrect(@"
{
	do {
		//@ 1
		a;
		lbl1:
		//@ 2
		b;
		//@ 3
		break;
		//@ 4
	} while (c);
	lbl2:
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
				a;
				//@ none
				$state1 = 3;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 2
				b;
				//@ 3
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 4
				if (c) {
					$state1 = 0;
					continue $loop1;
					//@ none
				}
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
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
		public void DoWhileWithBreak3() {
			AssertCorrect(@"
{
	do {
		//@ 1
		a;
		lbl1:
		//@ 2
		b;
		//@ 3
		break;
		//@ 4
	} while (c);
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
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 2
				b;
				//@ 3
				$state1 = -1;
				break $loop1;
				//@ none
			}
			case 1: {
				//@ 4
				if (c) {
					$state1 = 0;
					continue $loop1;
					//@ none
				}
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
		public void DoWhileWithContinue() {
			AssertCorrect(@"
{
	do {
		//@ 1
		a;
		lbl1:
		//@ 2
		b;
		//@ 3
		continue;
		//@ 4
		c;
		//@ 5
	} while (c);
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
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 2
				b;
				//@ 3
				$state1 = 1;
				continue $loop1;
				//@ 4
				c;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 5
				if (c) {
					$state1 = 0;
					continue $loop1;
					//@ none
				}
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
