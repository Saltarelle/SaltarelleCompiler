using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	public class DoWhileStatementTests : StateMachineRewriterTestBase {
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
				$tmp1 = -1;
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
				$tmp1 = -1;
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
				$tmp1 = -1;
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
				$tmp1 = -1;
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
			default: {
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
				$tmp1 = -1;
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
				$tmp1 = -1;
				break $loop1;
			}
			case 1: {
				if (c) {
					$tmp1 = 0;
					continue $loop1;
				}
				$tmp1 = -1;
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
				$tmp1 = -1;
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
