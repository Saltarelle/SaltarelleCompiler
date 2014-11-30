using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	public class BlockStatementTests : StateMachineRewriterTestBase {
		[Test]
		public void SimpleBlockStatementWorks() {
			AssertCorrect(@"
{
	//@ 1
	a;
	//@ 2
	b;
lbl1:
	//@ 3
	c;
	//@ 4
	d;
lbl2:
	//@ 5
	e;
	//@ 6
	f;
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
				//@ 4
				d;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 5
				e;
				//@ 6
				f;
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
		public void FirstStatementIsLabel() {
			AssertCorrect(@"
{
	lbl:
	//@ 1
	a;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 1
				a;
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
		public void BlockEndingWithGotoIsNotDoubleConnected() {
			AssertCorrect(@"
{
	//@ 1
	a;
	//@ 2
	b;
	//@ 3
	// goto lbl2
lbl1:
	//@ 4
	c;
	//@ 5
	d;
lbl2:
	//@ 6
	e;
	//@ 7
	f;
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
				//@ 2
				b;
				//@ 3
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 4
				c;
				//@ 5
				d;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 6
				e;
				//@ 7
				f;
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
		public void BlockEndingWithThrowIsNotDoubleConnected() {
			AssertCorrect(@"
{
	//@ 1
	a;
	//@ 2
	b;
	//@ 3
	throw c;
lbl1:
	//@ 4
	d;
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
				//@ 2
				b;
				//@ 3
				throw c;
			}
			case 1: {
				//@ 4
				d;
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
		public void BlockEndingWithReturnIsNotDoubleConnected() {
			AssertCorrect(@"
{
	//@ 1
	a;
	//@ 2
	b;
	//@ 3
	return;
lbl1:
	//@ 4
	c;
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
				//@ 2
				b;
				//@ 3
				return;
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
		public void NestingBlockStatementsWorks() {
			AssertCorrect(@"
{
	//@ 1
	a;
	{
		//@ 2
		b;
		lbl1: {
			//@ 3
			c;
		}
		//@ 4
		d;
		lbl2:
		//@ 5
		e;
	}
	{
		//@ 6
		f;
		{
			//@ 7
			g;
			//@ 8
			// goto lbl4
		}
	}
	lbl4:
	//@ 9
	h;
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
				//@ 2
				b;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ none
				{
					//@ 3
					c;
				}
				//@ 4
				d;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 5
				e;
				{
					//@ 6
					f;
					{
						//@ 7
						g;
						//@ 8
						$state1 = 3;
						continue $loop1;
						//@ none
					}
				}
			}
			case 3: {
				//@ 9
				h;
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
