using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.GotoTests
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
	}
}
