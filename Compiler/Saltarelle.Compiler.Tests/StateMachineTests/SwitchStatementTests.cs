using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	public class SwitchStatementTests : StateMachineRewriterTestBase {
		[Test]
		public void SwitchStatement1() {
			AssertCorrect(@"
{
	//@ 1
	switch (a) {
		case b:
		case c:
			//@ 2
			d;
			lbl1:
			//@ 3
			e;
			//@ 4
			break;
		case f:
			//@ 5
			g;
			//@ 6
			break;
		case h:
		case i:
			//@ 7
			j;
			//@ 8
			break;
		case k:
			//@ 9
			l;
			lbl2:
			//@ 10
			m;
			//@ 11
			break;
	}
	//@ 12
	n;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				switch (a) {
					case b:
					case c: {
						//@ 2
						d;
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
					case f: {
						//@ 5
						g;
						//@ 6
						$state1 = 1;
						continue $loop1;
						//@ none
					}
					case h:
					case i: {
						//@ 7
						j;
						//@ 8
						$state1 = 1;
						continue $loop1;
						//@ none
					}
					case k: {
						//@ 9
						l;
						//@ none
						$state1 = 3;
						continue $loop1;
						//@ none
					}
				}
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 3
				e;
				//@ 4
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 10
				m;
				//@ 11
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 12
				n;
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
		public void SwitchStatement2() {
			AssertCorrect(@"
{
	{
		//@ 1
		switch (a) {
			case b:
			case c:
				//@ 2
				d;
				lbl1:
				//@ 3
				e;
				//@ 4
				break;
			case f:
				//@ 5
				g;
				//@ 6
				break;
			case h:
			case i:
				//@ 7
				j;
				//@ 8
				break;
			case k:
				//@ 9
				l;
				lbl2:
				//@ 10
				m;
				//@ 11
				break;
		}
	}
	//@ 12
	n;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				switch (a) {
					case b:
					case c: {
						//@ 2
						d;
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
					case f: {
						//@ 5
						g;
						//@ 6
						$state1 = 1;
						continue $loop1;
						//@ none
					}
					case h:
					case i: {
						//@ 7
						j;
						//@ 8
						$state1 = 1;
						continue $loop1;
						//@ none
					}
					case k: {
						//@ 9
						l;
						//@ none
						$state1 = 3;
						continue $loop1;
						//@ none
					}
				}
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 3
				e;
				//@ 4
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 10
				m;
				//@ 11
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 12
				n;
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
		public void SwitchStatement3() {
			AssertCorrect(@"
{
	//@ 1
	switch (a) {
		case b:
		case c:
			//@ 2
			d;
			lbl1:
			//@ 3
			e;
			//@ 4
			break;
		case f:
			//@ 5
			g;
			//@ 6
			break;
		case h:
		case i:
			//@ 7
			j;
			//@ 8
			break;
		case k:
			//@ 9
			l;
			lbl2:
			//@ 10
			m;
			//@ 11
			break;
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
				switch (a) {
					case b:
					case c: {
						//@ 2
						d;
						//@ none
						$state1 = 1;
						continue $loop1;
						//@ none
					}
					case f: {
						//@ 5
						g;
						//@ 6
						$state1 = -1;
						break $loop1;
						//@ none
					}
					case h:
					case i: {
						//@ 7
						j;
						//@ 8
						$state1 = -1;
						break $loop1;
						//@ none
					}
					case k: {
						//@ 9
						l;
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
				}
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			case 1: {
				//@ 3
				e;
				//@ 4
				$state1 = -1;
				break $loop1;
				//@ none
			}
			case 2: {
				//@ 10
				m;
				//@ 11
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
		public void SwitchStatement4() {
			AssertCorrect(@"
{
	//@ 1
	switch (a) {
		case b:
		case c:
			//@ 2
			d;
			lbl1:
			//@ 3
			e;
			//@ 4
			break;
		case f:
			//@ 5
			g;
			//@ 6
			break;
		case h:
		case i:
			//@ 7
			j;
			//@ 8
			break;
		case k:
			//@ 9
			l;
			lbl2:
			//@ 10
			m;
			//@ 11
			break;
	}
	lbl3:
	//@ 12
	n;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				switch (a) {
					case b:
					case c: {
						//@ 2
						d;
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
					case f: {
						//@ 5
						g;
						//@ 6
						$state1 = 1;
						continue $loop1;
						//@ none
					}
					case h:
					case i: {
						//@ 7
						j;
						//@ 8
						$state1 = 1;
						continue $loop1;
						//@ none
					}
					case k: {
						//@ 9
						l;
						//@ none
						$state1 = 3;
						continue $loop1;
						//@ none
					}
				}
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 3
				e;
				//@ 4
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 10
				m;
				//@ 11
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 12
				n;
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
		public void SwitchStatementFallthrough1() {
			AssertCorrect(@"
{
	//@ 1
	switch (a) {
		case b:
			//@ 2
			c;
		case d:
			//@ 3
			e;
			lbl1:
			//@ 4
			f;
	}
	//@ 5
	g;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				switch (a) {
					case b: {
						//@ 2
						c;
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
					case d: {
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
				}
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 3
				e;
				//@ none
				$state1 = 3;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 5
				g;
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			case 3: {
				//@ 4
				f;
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
		public void SwitchStatementFallthrough2() {
			AssertCorrect(@"
{
	//@ 1
	switch (a) {
		case b:
			//@ 2
			c;
		case d:
			lbl2:
			//@ 3
			e;
			lbl1:
			//@ 4
			f;
	}
	//@ 5
	g;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				switch (a) {
					case b: {
						//@ 2
						c;
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
					case d: {
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
				}
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 3
				e;
				//@ none
				$state1 = 3;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 5
				g;
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			case 3: {
				//@ 4
				f;
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
		public void SwitchStatementFallthrough3() {
			AssertCorrect(@"
{
	//@ 1
	switch (a) {
		case b:
			//@ 2
			c;
			lbl1:
			//@ 3
			d;
		case e:
			//@ 4
			f;
	}
	//@ 5
	g;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				switch (a) {
					case b: {
						//@ 2
						c;
						//@ none
						$state1 = 3;
						continue $loop1;
						//@ none
					}
					case e: {
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
				}
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 3
				d;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 4
				f;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 5
				g;
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
		public void SwitchStatementWithBreakInTheMiddleOfSection() {
			AssertCorrect(@"
{
	//@ 1
	switch (a) {
		case b:
			//@ 2
			break;
			//@ 3
			c;
			//@ 4
			break;
		case d:
			//@ 5
			d;
			lbl1:
			//@ 6
			break;
			//@ 7
			e;
			//@ 8
			break;
	}
	//@ 9
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
				switch (a) {
					case b: {
						//@ 2
						$state1 = 1;
						continue $loop1;
						//@ 3
						c;
						//@ 4
						$state1 = 1;
						continue $loop1;
						//@ none
					}
					case d: {
						//@ 5
						d;
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
				}
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 6
				$state1 = 1;
				continue $loop1;
				//@ 7
				e;
				//@ 8
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 9
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
		public void SwitchStatementWithDefault() {
			AssertCorrect(@"
{
	//@ 1
	switch (a) {
		case b:
			//@ 2
			c;
			//@ 3
			break;
		case d:
			//@ 4
			e;
			lbl1:
			//@ 5
			f;
			//@ 6
			break;
		default:
			//@ 7
			g;
			lbl2:
			//@ 8
			h;
			//@ 9
			break;
	}
	//@ 10
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
				switch (a) {
					case b: {
						//@ 2
						c;
						//@ 3
						$state1 = 1;
						continue $loop1;
						//@ none
					}
					case d: {
						//@ 4
						e;
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
					default: {
						//@ 7
						g;
						//@ none
						$state1 = 3;
						continue $loop1;
						//@ none
					}
				}
			}
			case 2: {
				//@ 5
				f;
				//@ 6
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 8
				h;
				//@ 9
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 10
				i;
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
		public void SwitchStatementWithDefaultGroupedWithOtherCases() {
			AssertCorrect(@"
{
	//@ 1
	switch (a) {
		case b:
			//@ 2
			c;
			//@ 3
			break;
		case d:
			//@ 4
			e;
			lbl1:
			//@ 5
			f;
			//@ 6
			break;
		case 1:
		default:
		case 2:
			//@ 7
			g;
			lbl2:
			//@ 8
			h;
			//@ 9
			break;
	}
	//@ 10
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
				switch (a) {
					case b: {
						//@ 2
						c;
						//@ 3
						$state1 = 1;
						continue $loop1;
						//@ none
					}
					case d: {
						//@ 4
						e;
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
					case 1:
					default:
					case 2: {
						//@ 7
						g;
						//@ none
						$state1 = 3;
						continue $loop1;
						//@ none
					}
				}
			}
			case 2: {
				//@ 5
				f;
				//@ 6
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 8
				h;
				//@ 9
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 10
				i;
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
		public void SwitchStatementWithDefaultInTheMiddle() {
			AssertCorrect(@"
{
	//@ 1
	switch (a) {
		case b:
			//@ 2
			c;
			//@ 3
			break;
		default:
			//@ 4
			g;
			lbl2:
			//@ 5
			h;
			//@ 6
			break;
		case d:
			//@ 7
			e;
			lbl1:
			//@ 8
			f;
			//@ 9
			break;
	}
	//@ 10
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
				switch (a) {
					case b: {
						//@ 2
						c;
						//@ 3
						$state1 = 1;
						continue $loop1;
						//@ none
					}
					default: {
						//@ 4
						g;
						//@ none
						$state1 = 2;
						continue $loop1;
						//@ none
					}
					case d: {
						//@ 7
						e;
						//@ none
						$state1 = 3;
						continue $loop1;
						//@ none
					}
				}
			}
			case 2: {
				//@ 5
				h;
				//@ 6
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 3: {
				//@ 8
				f;
				//@ 9
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 10
				i;
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
		public void NestedSwitchStatemtent() {
			AssertCorrect(@"
{
	//@ 1
	switch (a) {
		default:
			//@ 2
			switch (b) {
				default:
					//@ 3
					c;
					lbl:
					//@ 4
					d;
					//@ 5
					break;
			}
			//@ 6
			e;
			//@ 7
			break;
	}
	//@ 8
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
				switch (a) {
					default: {
						//@ 2
						switch (b) {
							default: {
								//@ 3
								c;
								//@ none
								$state1 = 3;
								continue $loop1;
								//@ none
							}
						}
					}
				}
			}
			case 3: {
				//@ 4
				d;
				//@ 5
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 6
				e;
				//@ 7
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 8
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
	}
}
