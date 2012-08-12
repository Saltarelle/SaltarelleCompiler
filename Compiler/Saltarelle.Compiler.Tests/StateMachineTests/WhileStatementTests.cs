using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	public class WhileStatementTests : StateMachineRewriterTestBase {
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (!a) {
					$state1 = 1;
					continue $loop1;
				}
				b;
				$state1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$state1 = 0;
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (!a) {
					$state1 = 1;
					continue $loop1;
				}
				b;
				$state1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$state1 = 0;
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				a;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					$state1 = 2;
					continue $loop1;
				}
				c;
				$state1 = 3;
				continue $loop1;
			}
			case 3: {
				d;
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (!a) {
					$state1 = -1;
					break $loop1;
				}
				b;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				c;
				$state1 = 0;
				continue $loop1;
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (!a) {
					$state1 = 1;
					continue $loop1;
				}
				b;
				$state1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$state1 = 0;
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (!a) {
					$state1 = 1;
					continue $loop1;
				}
				b;
				$state1 = 2;
				continue $loop1;
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (!a) {
					$state1 = 1;
					continue $loop1;
				}
				b;
				$state1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$state1 = 0;
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
	}
}
