using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.GotoTests
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
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!a) {
					$tmp1 = 1;
					continue $loop1;
				}
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 0;
				continue $loop1;
			}
			case 1: {
				d;
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
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!a) {
					$tmp1 = 1;
					continue $loop1;
				}
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 0;
				continue $loop1;
			}
			case 1: {
				d;
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
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (!b) {
					$tmp1 = 2;
					continue $loop1;
				}
				c;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				d;
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
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
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!a) {
					break $loop1;
				}
				b;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				c;
				$tmp1 = 0;
				continue $loop1;
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
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!a) {
					$tmp1 = 1;
					continue $loop1;
				}
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 0;
				continue $loop1;
			}
			case 1: {
				d;
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
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!a) {
					$tmp1 = 1;
					continue $loop1;
				}
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				d;
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
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!a) {
					$tmp1 = 1;
					continue $loop1;
				}
				b;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 0;
				continue $loop1;
			}
			case 1: {
				d;
				break $loop1;
			}
		}
	}
}
");
		}
	}
}
