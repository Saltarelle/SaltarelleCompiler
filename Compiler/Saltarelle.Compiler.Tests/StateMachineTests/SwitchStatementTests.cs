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
				switch (a) {
					case b:
					case c: {
						d;
						$state1 = 2;
						continue $loop1;
					}
					case f: {
						g;
						$state1 = 1;
						continue $loop1;
					}
					case h:
					case i: {
						j;
						$state1 = 1;
						continue $loop1;
					}
					case k: {
						l;
						$state1 = 3;
						continue $loop1;
					}
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
				switch (a) {
					case b:
					case c: {
						d;
						$state1 = 2;
						continue $loop1;
					}
					case f: {
						g;
						$state1 = 1;
						continue $loop1;
					}
					case h:
					case i: {
						j;
						$state1 = 1;
						continue $loop1;
					}
					case k: {
						l;
						$state1 = 3;
						continue $loop1;
					}
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
				switch (a) {
					case b:
					case c: {
						d;
						$state1 = 1;
						continue $loop1;
					}
					case f: {
						g;
						$state1 = -1;
						break $loop1;
					}
					case h:
					case i: {
						j;
						$state1 = -1;
						break $loop1;
					}
					case k: {
						l;
						$state1 = 2;
						continue $loop1;
					}
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
				switch (a) {
					case b:
					case c: {
						d;
						$state1 = 2;
						continue $loop1;
					}
					case f: {
						g;
						$state1 = 1;
						continue $loop1;
					}
					case h:
					case i: {
						j;
						$state1 = 1;
						continue $loop1;
					}
					case k: {
						l;
						$state1 = 3;
						continue $loop1;
					}
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
			e;
			lbl1:
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
				switch (a) {
					case b: {
						c;
						$state1 = 2;
						continue $loop1;
					}
					case d: {
						$state1 = 2;
						continue $loop1;
					}
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$state1 = 3;
				continue $loop1;
			}
			case 1: {
				g;
				$state1 = -1;
				break $loop1;
			}
			case 3: {
				f;
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
			lbl2:
			e;
			lbl1:
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
				switch (a) {
					case b: {
						c;
						$state1 = 2;
						continue $loop1;
					}
					case d: {
						$state1 = 2;
						continue $loop1;
					}
				}
				$state1 = 1;
				continue $loop1;
			}
			case 2: {
				e;
				$state1 = 3;
				continue $loop1;
			}
			case 1: {
				g;
				$state1 = -1;
				break $loop1;
			}
			case 3: {
				f;
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
				switch (a) {
					case b: {
						c;
						$state1 = 3;
						continue $loop1;
					}
					case e: {
						$state1 = 2;
						continue $loop1;
					}
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
				switch (a) {
					case b: {
						$state1 = 1;
						continue $loop1;
						c;
						$state1 = 1;
						continue $loop1;
					}
					case d: {
						d;
						$state1 = 2;
						continue $loop1;
					}
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
				switch (a) {
					case b: {
						c;
						$state1 = 1;
						continue $loop1;
					}
					case d: {
						e;
						$state1 = 2;
						continue $loop1;
					}
					default: {
						g;
						$state1 = 3;
						continue $loop1;
					}
				}
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
				switch (a) {
					case b: {
						c;
						$state1 = 1;
						continue $loop1;
					}
					case d: {
						e;
						$state1 = 2;
						continue $loop1;
					}
					case 1:
					default:
					case 2: {
						g;
						$state1 = 3;
						continue $loop1;
					}
				}
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
				switch (a) {
					case b: {
						c;
						$state1 = 1;
						continue $loop1;
					}
					default: {
						g;
						$state1 = 2;
						continue $loop1;
					}
					case d: {
						e;
						$state1 = 3;
						continue $loop1;
					}
				}
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

		[Test]
		public void NestedSwitchStatemtent() {
			AssertCorrect(@"
{
	switch (a) {
		default:
			switch (b) {
				default:
					// yield return 1
					break;
			}
			a;
			break;
	}
	b;
}", 
@"{
	var $state1 = 0;
	{
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					switch (a) {
						default: {
							switch (b) {
								default: {
									setCurrent(1);
									$state1 = 3;
									return true;
								}
							}
						}
					}
				}
				case 3: {
					$state1 = 2;
					continue $loop1;
				}
				case 2: {
					$state1 = -1;
					a;
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					b;
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
		return false;
	}
}
", MethodType.Iterator);
		}
	}
}
