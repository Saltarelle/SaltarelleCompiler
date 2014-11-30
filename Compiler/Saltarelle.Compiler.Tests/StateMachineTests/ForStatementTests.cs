using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	public class ForStatementTests : StateMachineRewriterTestBase {
		[Test]
		public void ForStatement1() {
			AssertCorrect(@"
{
	//@ 1
	for (a; b; c) {
		//@ 2
		d;
		lbl1:
		//@ 3
		e;
	}
	//@ 4
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
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 1
				if (!b) {
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				//@ 2
				d;
				//@ none
				$state1 = 4;
				continue $loop1;
				//@ none
			}
			case 4: {
				//@ 3
				e;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 1
				c;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 4
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
		public void ForStatement2() {
			AssertCorrect(@"
{
	{
		//@ 1
		for (a; b; c) {
			//@ 2
			d;
			lbl1:
			//@ 3
			e;
		}
	}
	//@ 4
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
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 1
				if (!b) {
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				//@ 2
				d;
				//@ none
				$state1 = 4;
				continue $loop1;
				//@ none
			}
			case 4: {
				//@ 3
				e;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 1
				c;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 4
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
		public void ForStatement3() {
			AssertCorrect(@"
{
	//@ 1
	for (a; b; c) {
		//@ 2
		d;
		lbl1:
		//@ 3
		e;
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
				a;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 1
				if (!b) {
					$state1 = -1;
					break $loop1;
					//@ none
				}
				//@ 2
				d;
				//@ none
				$state1 = 3;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 3
				e;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 1
				c;
				//@ none
				$state1 = 1;
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
		public void ForStatement4() {
			AssertCorrect(@"
{
	//@ 1
	for (a; b; c) {
		//@ 2
		d;
		lbl1:
		//@ 3
		e;
	}
	lbl2:
	//@ 4
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
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 1
				if (!b) {
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				//@ 2
				d;
				//@ none
				$state1 = 4;
				continue $loop1;
				//@ none
			}
			case 4: {
				//@ 3
				e;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 1
				c;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 4
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
		public void ForStatementWithoutInitializer1() {
			AssertCorrect(@"
{
	//@ 1
	a;
	//@ 2
	for (; b; c) {
		//@ 3
		d;
		lbl1:
		//@ 4
		e;
	}
	//@ 5
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
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 2
				if (!b) {
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				//@ 3
				d;
				//@ none
				$state1 = 4;
				continue $loop1;
				//@ none
			}
			case 4: {
				//@ 4
				e;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 2
				c;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 5
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
		public void ForStatementWithoutInitializer2() {
			AssertCorrect(@"
{
	//@ 1
	for (; b; c) {
		//@ 2
		d;
		lbl1:
		//@ 3
		e;
	}
	//@ 4
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
				if (!b) {
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				//@ 2
				d;
				//@ none
				$state1 = 3;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 3
				e;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 1
				c;
				//@ none
				$state1 = 0;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 4
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
		public void ForStatementWithoutInitializer3() {
			AssertCorrect(@"
{
	//@ 1
	a;
	lbl2:
	//@ 2
	for (; b; c) {
		//@ 3
		d;
		lbl1:
		//@ 4
		e;
	}
	//@ 5
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
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 2
				if (!b) {
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				//@ 3
				d;
				//@ none
				$state1 = 4;
				continue $loop1;
				//@ none
			}
			case 4: {
				//@ 4
				e;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 2
				c;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 5
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
		public void ForStatementWithoutTest() {
			AssertCorrect(@"
{
	//@ 1
	for (a; ; c) {
		//@ 2
		d;
		lbl1:
		//@ 3
		e;
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
				a;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 2
				d;
				//@ none
				$state1 = 3;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 3
				e;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 1
				c;
				//@ none
				$state1 = 1;
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
		public void ForStatementWithoutIncrementer() {
			AssertCorrect(@"
{
	//@ 1
	for (a; b; ) {
		//@ 2
		d;
		lbl1:
		//@ 3
		e;
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
				a;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 1
				if (!b) {
					$state1 = -1;
					break $loop1;
					//@ none
				}
				//@ 2
				d;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 3
				e;
				//@ none
				$state1 = 1;
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
		public void ForEverStatement() {
			AssertCorrect(@"
{
	//@ 1
	for (;; ) {
		//@ 2
		d;
		lbl1:
		//@ 3
		e;
	}
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 2
				d;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 3
				e;
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
		public void ContinueInForStatement() {
			AssertCorrect(@"
{
	//@ 1
	for (a; b; c) {
		//@ 2
		d;
		lbl1:
		//@ 3
		e;
		//@ 4
		continue;
	}
	//@ 5
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
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 1
				if (!b) {
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				//@ 2
				d;
				//@ none
				$state1 = 4;
				continue $loop1;
				//@ none
			}
			case 4: {
				//@ 3
				e;
				//@ 4
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 1
				c;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 5
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
		public void ContinueInForStatementWitoutIncrementer() {
			AssertCorrect(@"
{
	//@ 1
	for (a; b; ) {
		//@ 2
		d;
		lbl1:
		//@ 3
		e;
		//@ 4
		continue;
	}
	//@ 5
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
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 1
				if (!b) {
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				//@ 2
				d;
				//@ none
				$state1 = 3;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 3
				e;
				//@ 4
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 5
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
		public void ContinueInForStatementWitoutTest() {
			AssertCorrect(@"
{
	//@ 1
	for (a; ; c) {
		//@ 2
		d;
		lbl1:
		//@ 3
		e;
		//@ 4
		continue;
	}
	//@ 5
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
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 2
				d;
				//@ none
				$state1 = 4;
				continue $loop1;
				//@ none
			}
			case 4: {
				//@ 3
				e;
				//@ 4
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 1
				c;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 5
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
		public void ContinueInForEverStatement() {
			AssertCorrect(@"
{
	//@ 1
	for (;;) {
		//@ 2
		d;
		lbl1:
		//@ 3
		e;
		//@ 4
		continue;
	}
	//@ 5
	f;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 2
				d;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 3
				e;
				//@ 4
				$state1 = 0;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 5
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
		public void BreakInForStatement() {
			AssertCorrect(@"
{
	//@ 1
	for (a; b; c) {
		//@ 2
		d;
		lbl1:
		//@ 3
		e;
		//@ 4
		break;
	}
	//@ 5
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
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 1
				if (!b) {
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				//@ 2
				d;
				//@ none
				$state1 = 4;
				continue $loop1;
				//@ none
			}
			case 4: {
				//@ 3
				e;
				//@ 4
				$state1 = 3;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 1
				c;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 5
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
		public void NestedForStatements() {
			AssertCorrect(@"
{
	//@ 1
	for (a; b; c) {
		//@ 2
		for (d; e; f) {
			//@ 3
			g;
			lbl1:
			//@ 4
			h;
		}
	}
	//@ 5
	i;
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
				//@ 1
				if (!b) {
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				//@ 2
				d;
				//@ none
				$state1 = 4;
				continue $loop1;
				//@ none
			}
			case 4: {
				//@ 2
				if (!e) {
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				//@ 3
				g;
				//@ none
				$state1 = 6;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 1
				c;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 5
				i;
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			case 6: {
				//@ 4
				h;
				//@ none
				$state1 = 5;
				continue $loop1;
				//@ none
			}
			case 5: {
				//@ 2
				f;
				//@ none
				$state1 = 4;
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
	}
}
