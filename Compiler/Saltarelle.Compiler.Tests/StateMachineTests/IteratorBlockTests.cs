using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	public class IteratorBlockTests : StateMachineRewriterTestBase {
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

		[Test]
		public void YieldInTryWithFinally1() {
			AssertCorrect(
@"{
	a;
	try {
		b;
		yield return 1;
		c;
	}
	finally {
		d;
	}
	e;
}
}",
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				$tmp1 = -1;
				a;
				$tmp1 = 2;
				b;
				setCurrent(1);
				$tmp1 = 4;
				return true;
			}
			case 4: {
				$tmp1 = 2;
				c;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				$tmp1 = -1;
				$finally1();
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
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
		public void YieldInTryWithFinallyNested() {
			AssertCorrect(
@"{
	a;
	try {
		b;
		yield return 1;
		c;
		try {
			d;
			yield return 2;
			e;
			try {
				f;
				yield return 3;
				g;
			}
			finally {
				h;
			}
			i;
			yield return 4;
			j;
		}
		finally {
			k;
		}
		l;
		yield return 5;
		m;
	}
	finally {
		n;
	}
	o;
}",
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				$tmp1 = -1;
				a;
				$tmp1 = 1;
				b;
				setCurrent(1);
				$tmp1 = 3;
				return true;
			}
			case 3: {
				$tmp1 = 1;
				c;
				$tmp1 = 4;
				d;
				setCurrent(2);
				$tmp1 = 6;
				return true;
			}
			case 6: {
				$tmp1 = 4;
				e;
				$tmp1 = 7;
				f;
				setCurrent(3);
				$tmp1 = 9;
				return true;
			}
			case 9: {
				$tmp1 = 7;
				g;
				$tmp1 = 8;
				continue $loop1;
			}
		}
	}
	return false;
}
TODO: More
", isIteratorBlock: true);
		}
	}
}
