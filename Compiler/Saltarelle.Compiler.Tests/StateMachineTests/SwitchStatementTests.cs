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
	var $tmp1;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b || a === c) {
					d;
					$tmp1 = 2;
					continue $loop1;
				}
				else if (a === f) {
					g;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === h || a === i) {
					j;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === k) {
					l;
					$tmp1 = 3;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				m;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				n;
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
	var $tmp1;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b || a === c) {
					d;
					$tmp1 = 2;
					continue $loop1;
				}
				else if (a === f) {
					g;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === h || a === i) {
					j;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === k) {
					l;
					$tmp1 = 3;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				m;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				n;
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
	var $tmp1;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b || a === c) {
					d;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === f) {
					g;
					break $loop1;
				}
				else if (a === h || a === i) {
					j;
					break $loop1;
				}
				else if (a === k) {
					l;
					$tmp1 = 2;
					continue $loop1;
				}
				break $loop1;
			}
			case 1: {
				e;
				break $loop1;
			}
			case 2: {
				m;
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
	var $tmp1;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b || a === c) {
					d;
					$tmp1 = 2;
					continue $loop1;
				}
				else if (a === f) {
					g;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === h || a === i) {
					j;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === k) {
					l;
					$tmp1 = 3;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				m;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				n;
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
	var $tmp1;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					c;
					$tmp1 = 2;
					continue $loop1;
				}
				else if (a === d) {
					$tmp1 = 2;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				d;
				$tmp1 = 3;
				continue $loop1;
			}
			case 1: {
				f;
				break $loop1;
			}
			case 3: {
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
	var $tmp1;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					c;
					$tmp1 = 2;
					continue $loop1;
				}
				else if (a === d) {
					$tmp1 = 2;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				d;
				$tmp1 = 3;
				continue $loop1;
			}
			case 1: {
				f;
				break $loop1;
			}
			case 3: {
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
	var $tmp1;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					c;
					$tmp1 = 3;
					continue $loop1;
				}
				else if (a === e) {
					$tmp1 = 2;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				d;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				f;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				g;
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
	var $tmp1;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					$tmp1 = 1;
					continue $loop1;
					c;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === d) {
					d;
					$tmp1 = 2;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				$tmp1 = 1;
				continue $loop1;
				e;
				$tmp1 = 1;
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
	var $tmp1, $tmp2;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				$tmp2 = a + b;
				if ($tmp2 === c) {
					d;
					$tmp1 = 2;
					continue $loop1;
				}
				else if ($tmp2 === f || $tmp2 === g) {
					h;
					$tmp1 = 1;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				n;
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
	var $tmp1;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					c;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === d) {
					e;
					$tmp1 = 2;
					continue $loop1;
				}
				else {
					g;
					$tmp1 = 3;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				f;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				h;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				i;
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
	var $tmp1;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					c;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === d) {
					e;
					$tmp1 = 2;
					continue $loop1;
				}
				else {
					g;
					$tmp1 = 3;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				f;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				h;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				i;
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
	var $tmp1;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				if (a === b) {
					c;
					$tmp1 = 1;
					continue $loop1;
				}
				else if (a === d) {
					e;
					$tmp1 = 3;
					continue $loop1;
				}
				else {
					g;
					$tmp1 = 2;
					continue $loop1;
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 2: {
				h;
				$tmp1 = 1;
				continue $loop1;
			}
			case 3: {
				f;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				i;
				break $loop1;
			}
		}
	}
}
");
		}
	}
}
