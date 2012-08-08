using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	public class ForStatementTests : StateMachineRewriterTestBase {
		[Test]
		public void ForStatement1() {
			AssertCorrect(@"
{
	for (a; b; c) {
		d;
		lbl1:
		e;
	}
	f;
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
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ForStatement2() {
			AssertCorrect(@"
{
	{
		for (a; b; c) {
			d;
			lbl1:
			e;
		}
	}
	f;
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
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ForStatement3() {
			AssertCorrect(@"
{
	for (a; b; c) {
		d;
		lbl1:
		e;
	}
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
					break $loop1;
				}
				d;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ForStatement4() {
			AssertCorrect(@"
{
	for (a; b; c) {
		d;
		lbl1:
		e;
	}
	lbl2: f;
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
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ForStatementWithoutInitializer1() {
			AssertCorrect(@"
{
	a;
	for (; b; c) {
		d;
		lbl1:
		e;
	}
	f;
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
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ForStatementWithoutInitializer2() {
			AssertCorrect(@"
{
	for (; b; c) {
		d;
		lbl1:
		e;
	}
	f;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (!b) {
					$tmp1 = 2;
					continue $loop1;
				}
				d;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				c;
				$tmp1 = 0;
				continue $loop1;
			}
			case 2: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ForStatementWithoutInitializer3() {
			AssertCorrect(@"
{
	a;
	lbl2:
	for (; b; c) {
		d;
		lbl1:
		e;
	}
	f;
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
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ForStatementWithoutTest() {
			AssertCorrect(@"
{
	for (a; ; c) {
		d;
		lbl1:
		e;
	}
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
				d;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ForStatementWithoutIncrementer() {
			AssertCorrect(@"
{
	for (a; b; ) {
		d;
		lbl1:
		e;
	}
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
					break $loop1;
				}
				d;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ForEverStatement() {
			AssertCorrect(@"
{
	for (;; ) {
		d;
		lbl1:
		e;
	}
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				d;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				e;
				$tmp1 = 0;
				continue $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ContinueInForStatement() {
			AssertCorrect(@"
{
	for (a; b; c) {
		d;
		lbl1:
		e;
		continue;
	}
	f;
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
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ContinueInForStatementWitoutIncrementer() {
			AssertCorrect(@"
{
	for (a; b; ) {
		d;
		lbl1:
		e;
		continue;
	}
	f;
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
				d;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ContinueInForStatementWitoutTest() {
			AssertCorrect(@"
{
	for (a; ; c) {
		d;
		lbl1:
		e;
		continue;
	}
	f;
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
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void ContinueInForEverStatement() {
			AssertCorrect(@"
{
	for (;;) {
		d;
		lbl1:
		e;
		continue;
	}
	f;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				d;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				e;
				$tmp1 = 0;
				continue $loop1;
			}
			case 1: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void BreakInForStatement() {
			AssertCorrect(@"
{
	for (a; b; c) {
		d;
		lbl1:
		e;
		break;
	}
	f;
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
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				e;
				$tmp1 = 3;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void NestedForStatements() {
			AssertCorrect(@"
{
	for (a; b; c) {
		for (d; e; f) {
			g;
			lbl1:
			h;
		}
	}
	i;
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
					$tmp1 = 3;
					continue $loop1;
				}
				d;
				$tmp1 = 4;
				continue $loop1;
			}
			case 4: {
				if (!e) {
					$tmp1 = 2;
					continue $loop1;
				}
				g;
				$tmp1 = 6;
				continue $loop1;
			}
			case 2: {
				c;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				i;
				break $loop1;
			}
			case 6: {
				h;
				$tmp1 = 5;
				continue $loop1;
			}
			case 5: {
				f;
				$tmp1 = 4;
				continue $loop1;
			}
		}
	}
}
");
		}
	}
}
