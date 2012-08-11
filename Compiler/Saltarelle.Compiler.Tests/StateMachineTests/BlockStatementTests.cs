using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	public class BlockStatementTests : StateMachineRewriterTestBase {
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
				$tmp1 = 2;
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
