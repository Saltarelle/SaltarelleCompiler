using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	public class SwitchStatementTests : StateMachineRewriterTestBase {
		[Test]
		public void SwitchStatement1() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
		case c:
			d;
			lbl1:
			e;
			break;
		case f:
			g;
			break;
		case h:
		case i:
			j;
			break;
		case k:
			l;
			lbl2:
			m;
			break;
	}
	n;
}", 
@"{
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (a === b || a === c) {
					d;
					$state1 = 2;
					continue $loop1;
				}
				else if (a === f) {
					g;
					$state1 = 1;
					continue $loop1;
				}
				else if (a === h || a === i) {
					j;
					$state1 = 1;
					continue $loop1;
				}
				else if (a === k) {
					l;
					$state1 = 3;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$state1 = 1;
				continue $loop1;
			}
			case 3: {
				m;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				n;
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
		public void SwitchStatement2() {
			AssertCorrect(@"
{
	{
		switch (a) {
			case b:
			case c:
				d;
				lbl1:
				e;
				break;
			case f:
				g;
				break;
			case h:
			case i:
				j;
				break;
			case k:
				l;
				lbl2:
				m;
				break;
		}
	}
	n;
}", 
@"{
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (a === b || a === c) {
					d;
					$state1 = 2;
					continue $loop1;
				}
				else if (a === f) {
					g;
					$state1 = 1;
					continue $loop1;
				}
				else if (a === h || a === i) {
					j;
					$state1 = 1;
					continue $loop1;
				}
				else if (a === k) {
					l;
					$state1 = 3;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$state1 = 1;
				continue $loop1;
			}
			case 3: {
				m;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				n;
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
		public void SwitchStatement3() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
		case c:
			d;
			lbl1:
			e;
			break;
		case f:
			g;
			break;
		case h:
		case i:
			j;
			break;
		case k:
			l;
			lbl2:
			m;
			break;
	}
}",
@"{
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (a === b || a === c) {
					d;
					$state1 = 1;
					continue $loop1;
				}
				else if (a === f) {
					g;
					$state1 = -1;
					break $loop1;
				}
				else if (a === h || a === i) {
					j;
					$state1 = -1;
					break $loop1;
				}
				else if (a === k) {
					l;
					$state1 = 2;
					continue $loop1;
				}
				$state1 = -1;
				break $loop1;
			}
			case 1: {
				e;
				$state1 = -1;
				break $loop1;
			}
			case 2: {
				m;
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
		public void SwitchStatement4() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
		case c:
			d;
			lbl1:
			e;
			break;
		case f:
			g;
			break;
		case h:
		case i:
			j;
			break;
		case k:
			l;
			lbl2:
			m;
			break;
	}
	lbl3: n;
}", 
@"{
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (a === b || a === c) {
					d;
					$state1 = 2;
					continue $loop1;
				}
				else if (a === f) {
					g;
					$state1 = 1;
					continue $loop1;
				}
				else if (a === h || a === i) {
					j;
					$state1 = 1;
					continue $loop1;
				}
				else if (a === k) {
					l;
					$state1 = 3;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$state1 = 1;
				continue $loop1;
			}
			case 3: {
				m;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				n;
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
		public void SwitchStatementFallthrough1() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			c;
		case d:
			d;
			lbl1:
			e;
	}
	f;
}", 
@"{
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (a === b) {
					c;
					$state1 = 2;
					continue $loop1;
				}
				else if (a === d) {
					$state1 = 2;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				d;
				$state1 = 3;
				continue $loop1;
			}
			case 1: {
				f;
				$state1 = -1;
				break $loop1;
			}
			case 3: {
				e;
				$state1 = 1;
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
		public void SwitchStatementFallthrough2() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			c;
		case d:
			lbl2: d;
			lbl1:
			e;
	}
	f;
}", 
@"{
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (a === b) {
					c;
					$state1 = 2;
					continue $loop1;
				}
				else if (a === d) {
					$state1 = 2;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				d;
				$state1 = 3;
				continue $loop1;
			}
			case 1: {
				f;
				$state1 = -1;
				break $loop1;
			}
			case 3: {
				e;
				$state1 = 1;
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
		public void SwitchStatementFallthrough3() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			c;
			lbl1:
			d;
		case e:
			f;
	}
	g;
}", 
@"{
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (a === b) {
					c;
					$state1 = 3;
					continue $loop1;
				}
				else if (a === e) {
					$state1 = 2;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 3: {
				d;
				$state1 = 2;
				continue $loop1;
			}
			case 2: {
				f;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				g;
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
		public void SwitchStatementWithBreakInTheMiddleOfSection() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			break;
			c;
			break;
		case d:
			d;
			lbl1:
			break;
			e;
			break;
	}
	f;
}", 
@"{
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (a === b) {
					$state1 = 1;
					continue $loop1;
					c;
					$state1 = 1;
					continue $loop1;
				}
				else if (a === d) {
					d;
					$state1 = 2;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				$state1 = 1;
				continue $loop1;
				e;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				f;
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
		public void SwitchStatementWithComplexExpression() {
			AssertCorrect(@"
{
	switch (a + b) {
		case c:
			d;
			lbl1:
			e;
			break;
		case f:
		case g:
			h;
			break;
	}
	n;
}", 
@"{
	var $state1 = 0, $tmp1;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				$tmp1 = a + b;
				if ($tmp1 === c) {
					d;
					$state1 = 2;
					continue $loop1;
				}
				else if ($tmp1 === f || $tmp1 === g) {
					h;
					$state1 = 1;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				n;
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
		public void SwitchStatementWithDefault() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			c;
			break;
		case d:
			e;
			lbl1:
			f;
			break;
		default:
			g;
			lbl2:
			h;
			break;
	}
	i;
}", 
@"{
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (a === b) {
					c;
					$state1 = 1;
					continue $loop1;
				}
				else if (a === d) {
					e;
					$state1 = 2;
					continue $loop1;
				}
				else {
					g;
					$state1 = 3;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				f;
				$state1 = 1;
				continue $loop1;
			}
			case 3: {
				h;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				i;
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
		public void SwitchStatementWithDefaultGroupedWithOtherCases() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			c;
			break;
		case d:
			e;
			lbl1:
			f;
			break;
		case 1:
		default:
		case 2:
			g;
			lbl2:
			h;
			break;
	}
	i;
}", 
@"{
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (a === b) {
					c;
					$state1 = 1;
					continue $loop1;
				}
				else if (a === d) {
					e;
					$state1 = 2;
					continue $loop1;
				}
				else {
					g;
					$state1 = 3;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				f;
				$state1 = 1;
				continue $loop1;
			}
			case 3: {
				h;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				i;
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
		public void SwitchStatementWithDefaultInTheMiddle() {
			AssertCorrect(@"
{
	switch (a) {
		case b:
			c;
			break;
		default:
			g;
			lbl2:
			h;
			break;
		case d:
			e;
			lbl1:
			f;
			break;
	}
	i;
}", 
@"{
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				if (a === b) {
					c;
					$state1 = 1;
					continue $loop1;
				}
				else if (a === d) {
					e;
					$state1 = 3;
					continue $loop1;
				}
				else {
					g;
					$state1 = 2;
					continue $loop1;
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				h;
				$state1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				i;
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
