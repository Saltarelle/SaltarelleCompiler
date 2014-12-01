using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests
{
	[TestFixture]
	public class IteratorBlockTests : StateMachineRewriterTestBase {
		[Test]
		public void IteratorBlockWithoutYieldBreak() {
			AssertCorrect(@"
{
	//@ 1
	a;
	//@ 2
	// yield return b
	//@ 3
	c;
	//@ 4
	// yield return d
lbl1:
	//@ 5
	e;
}", 
@"{
	var $state1 = 0;
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ 2
					setCurrent(b);
					$state1 = 1;
					return true;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 3
					c;
					//@ 4
					setCurrent(d);
					$state1 = 2;
					return true;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					//@ 5
					e;
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void IteratorBlockWithYieldBreak() {
			AssertCorrect(@"
{
	//@ 1
	a;
	//@ 2
	// yield return b
	//@ 3
	c;
	//@ 4
	// yield break
}", 
@"{
	var $state1 = 0;
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ 2
					setCurrent(b);
					$state1 = 1;
					return true;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 3
					c;
					//@ 4
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void IteratorBlockWithYieldBreakWithStuffFollowing() {
			AssertCorrect(@"
{
	//@ 1
	a;
	//@ 2
	// yield return b
	//@ 3
	c;
	//@ 4
	// yield break
	//@ 5
	d;
}", 
@"{
	var $state1 = 0;
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ 2
					setCurrent(b);
					$state1 = 1;
					return true;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 3
					c;
					//@ 4
					$state1 = -1;
					break $loop1;
					//@ 5
					d;
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldInTryWithFinally1() {
			AssertCorrect(
@"{
	//@ 1
	a;
	try {
		//@ 2
		b;
		//@ 3
		// yield return 1
		//@ 4
		c;
	}
	finally {
		//@ 5
		d;
	}
	//@ 6
	e;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 5
		d;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 2:
				case 3:
				case 4: {
					try {
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 2;
					//@ 2
					b;
					//@ 3
					setCurrent(1);
					$state1 = 4;
					return true;
					//@ none
				}
				case 4: {
					//@ none
					$state1 = 2;
					//@ 4
					c;
					//@ none
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 6
					e;
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldInTryWithFinally2() {
			AssertCorrect(
@"{
	//@ 1
	a;
	{
		try {
			//@ 2
			b;
			//@ 3
			// yield return 1
			//@ 4
			c;
		}
		finally {
			//@ 5
			d;
		}
	}
	//@ 6
	e;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 5
		d;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 2:
				case 3:
				case 4: {
					try {
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 2;
					//@ 2
					b;
					//@ 3
					setCurrent(1);
					$state1 = 4;
					return true;
					//@ none
				}
				case 4: {
					//@ none
					$state1 = 2;
					//@ 4
					c;
					//@ none
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 6
					e;
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldInTryWithFinally3() {
			AssertCorrect(
@"{
	//@ 1
	a;
	try {
		//@ 2
		b;
		//@ 3
		// yield return 1
		//@ 4
		c;
	}
	finally {
		//@ 5
		d;
	}
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 5
		d;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 1:
				case 2:
				case 3: {
					try {
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 1;
					//@ 2
					b;
					//@ 3
					setCurrent(1);
					$state1 = 3;
					return true;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = 1;
					//@ 4
					c;
					//@ none
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldInTryWithFinally4() {
			AssertCorrect(
@"{
	//@ 1
	a;
	try {
		//@ 2
		b;
		//@ 3
		// yield return 1
		//@ 4
		c;
	}
	finally {
		//@ 5
		d;
	}
	lbl2:
	//@ 6
	e;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 5
		d;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 2:
				case 3:
				case 4: {
					try {
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 2;
					//@ 2
					b;
					//@ 3
					setCurrent(1);
					$state1 = 4;
					return true;
					//@ none
				}
				case 4: {
					//@ none
					$state1 = 2;
					//@ 4
					c;
					//@ none
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 6
					e;
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void IteratorFinallyCanContainLabelAndGoto() {
			AssertCorrect(
@"{
	//@ 1
	a;
	try {
		//@ 2
		// yield return 0
	}
	finally {
		//@ 3
		b;
		lbl1:
		//@ 4
		c;
		//@ 5
		// goto lbl1
	}
	//@ 6
	d;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ none
		$state1 = 1;
		//@ none
		$loop2:
		for (;;) {
			switch ($state1) {
				case 1: {
					//@ none
					$state1 = -1;
					//@ 3
					b;
					//@ none
					$state1 = 2;
					continue $loop2;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					//@ 4
					c;
					//@ 5
					$state1 = 2;
					continue $loop2;
					//@ none
				}
				default: {
					//@ none
					break $loop2;
				}
			}
		}
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 4:
				case 5: {
					try {
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 4;
					//@ 2
					setCurrent(0);
					$state1 = 5;
					return true;
					//@ none
				}
				case 5: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
					//@ 6
					d;
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldBreakExecutesFinallyHandlers() {
			AssertCorrect(
@"{
	//@ 1
	a;
	try {
		//@ 2
		// yield return 0
		//@ 3
		b;
		//@ 4
		// yield break
		//@ 5
		c;
	}
	finally {
		//@ 6
		d;
	}
	//@ 7
	e;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 6
		d;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 2:
				case 3:
				case 4: {
					try {
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 2;
					//@ 2
					setCurrent(0);
					$state1 = 4;
					return true;
					//@ none
				}
				case 4: {
					//@ none
					$state1 = 2;
					//@ 3
					b;
					//@ 4
					$state1 = -1;
					$finally1.call(this);
					$state1 = -1;
					break $loop1;
					//@ 5
					c;
					//@ none
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 7
					e;
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldBreakExecutesFinallyHandlersNested1() {
			AssertCorrect(
@"{
	//@ 1
	a;
	try {
		//@ 2
		b;
		//@ 3
		// yield return 1
		//@ 4
		c;
		try {
			//@ 5
			d;
			//@ 6
			// yield return 2
			//@ 7
			e;
			try {
				//@ 8
				f;
				//@ 9
				// yield break
				//@ 10
				g;
			}
			finally {
				//@ 11
				h;
			}
			//@ 12
			i;
			//@ 13
			// yield return 4
			//@ 14
			j;
		}
		finally {
			//@ 15
			k;
		}
		//@ 16
		l;
		//@ 17
		// yield return 5
		//@ 18
		m;
	}
	finally {
		//@ 19
		n;
	}
	//@ 20
	o;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 19
		n;
	};
	$finally2 = function() {
		//@ 15
		k;
	};
	$finally3 = function() {
		//@ 11
		h;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
				case 13: {
					try {
						switch ($state1) {
							case 6:
							case 7:
							case 8:
							case 9:
							case 10:
							case 11:
							case 13: {
								try {
									switch ($state1) {
										case 10:
										case 11: {
											try {
												break;
											}
											finally {
												$finally3.call(this);
											}
										}
									}
									break;
								}
								finally {
									$finally2.call(this);
								}
							}
						}
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 2;
					//@ 2
					b;
					//@ 3
					setCurrent(1);
					$state1 = 4;
					return true;
					//@ none
				}
				case 4: {
					//@ none
					$state1 = 2;
					//@ 4
					c;
					//@ none
					$state1 = 6;
					//@ 5
					d;
					//@ 6
					setCurrent(2);
					$state1 = 8;
					return true;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 20
					o;
					//@ none
					$state1 = -1;
					break $loop1;
					//@ none
				}
				case 8: {
					//@ none
					$state1 = 6;
					//@ 7
					e;
					//@ none
					$state1 = 10;
					//@ 8
					f;
					//@ 9
					try {
						try {
							$state1 = 6;
							$finally3.call(this);
						}
						finally {
							$state1 = 2;
							$finally2.call(this);
						}
					}
					finally {
						$state1 = -1;
						$finally1.call(this);
					}
					$state1 = -1;
					break $loop1;
					//@ 10
					g;
					//@ none
					$state1 = 11;
					continue $loop1;
					//@ none
				}
				case 7: {
					//@ none
					$state1 = 2;
					$finally2.call(this);
					$state1 = 5;
					continue $loop1;
					//@ none
				}
				case 5: {
					//@ none
					$state1 = 2;
					//@ 16
					l;
					//@ 17
					setCurrent(5);
					$state1 = 12;
					return true;
					//@ none
				}
				case 11: {
					//@ none
					$state1 = 6;
					$finally3.call(this);
					$state1 = 9;
					continue $loop1;
					//@ none
				}
				case 9: {
					//@ none
					$state1 = 6;
					//@ 12
					i;
					//@ 13
					setCurrent(4);
					$state1 = 13;
					return true;
					//@ none
				}
				case 12: {
					//@ none
					$state1 = 2;
					//@ 18
					m;
					//@ none
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				case 13: {
					//@ none
					$state1 = 6;
					//@ 14
					j;
					//@ none
					$state1 = 7;
					continue $loop1;
					//@ none
				}
				default: {
					//@ none
					break $loop1;
				}
			}
		}
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldBreakExecutesFinallyHandlersNested2() {
			AssertCorrect(
@"{
	//@ 1
	a;
	try {
		//@ 2
		b;
		//@ 3
		// yield return 1
		//@ 4
		c;
		try {
			//@ 5
			d;
			//@ 6
			// yield return 2
			//@ 7
			e;
			try {
				//@ 8
				f1;
				//@ 9
				// yield break
				//@ 10
				g1;
			}
			catch (ex) {
				//@ 11
				f2;
				//@ 12
				// yield break
				//@ 13
				g2;
			}
			finally {
				//@ 14
				h;
			}
			//@ 15
			i;
			//@ 16
			// yield return 4
			//@ 17
			j;
		}
		finally {
			//@ 18
			k;
		}
		//@ 19
		l;
		//@ 20
		// yield return 5
		//@ 21
		m;
	}
	finally {
		//@ 22
		n;
	}
	//@ 23
	o;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 22
		n;
	};
	$finally2 = function() {
		//@ 18
		k;
	};
	$finally3 = function() {
		//@ 14
		h;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
				case 13: {
					try {
						switch ($state1) {
							case 6:
							case 7:
							case 8:
							case 9:
							case 10:
							case 11:
							case 13: {
								try {
									switch ($state1) {
										case 10:
										case 11: {
											try {
												break;
											}
											finally {
												$finally3.call(this);
											}
										}
									}
									break;
								}
								finally {
									$finally2.call(this);
								}
							}
						}
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 2;
					//@ 2
					b;
					//@ 3
					setCurrent(1);
					$state1 = 4;
					return true;
					//@ none
				}
				case 4: {
					//@ none
					$state1 = 2;
					//@ 4
					c;
					//@ none
					$state1 = 6;
					//@ 5
					d;
					//@ 6
					setCurrent(2);
					$state1 = 8;
					return true;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 23
					o;
					//@ none
					$state1 = -1;
					break $loop1;
					//@ none
				}
				case 8: {
					//@ none
					$state1 = 6;
					//@ 7
					e;
					//@ none
					$state1 = 10;
					//@ none
					try {
						//@ 8
						f1;
						//@ 9
						try {
							try {
								$state1 = 6;
								$finally3.call(this);
							}
							finally {
								$state1 = 2;
								$finally2.call(this);
							}
						}
						finally {
							$state1 = -1;
							$finally1.call(this);
						}
						$state1 = -1;
						break $loop1;
						//@ 10
						g1;
					}
					catch (ex) {
						//@ 11
						f2;
						//@ 12
						try {
							try {
								$state1 = 6;
								$finally3.call(this);
							}
							finally {
								$state1 = 2;
								$finally2.call(this);
							}
						}
						finally {
							$state1 = -1;
							$finally1.call(this);
						}
						$state1 = -1;
						break $loop1;
						//@ 13
						g2;
					}
					//@ none
					$state1 = 11;
					continue $loop1;
					//@ none
				}
				case 7: {
					//@ none
					$state1 = 2;
					$finally2.call(this);
					$state1 = 5;
					continue $loop1;
					//@ none
				}
				case 5: {
					//@ none
					$state1 = 2;
					//@ 19
					l;
					//@ 20
					setCurrent(5);
					$state1 = 12;
					return true;
					//@ none
				}
				case 11: {
					//@ none
					$state1 = 6;
					$finally3.call(this);
					$state1 = 9;
					continue $loop1;
					//@ none
				}
				case 9: {
					//@ none
					$state1 = 6;
					//@ 15
					i;
					//@ 16
					setCurrent(4);
					$state1 = 13;
					return true;
					//@ none
				}
				case 12: {
					//@ none
					$state1 = 2;
					//@ 21
					m;
					//@ none
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				case 13: {
					//@ none
					$state1 = 6;
					//@ 17
					j;
					//@ none
					$state1 = 7;
					continue $loop1;
					//@ none
				}
				default: {
					//@ none
					break $loop1;
				}
			}
		}
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void GotoOutOfTryBlockExecutesFinallyHandlers1() {
			AssertCorrect(
@"{
	//@ 1
	a;
	try {
		//@ 2
		b;
		//@ 3
		// yield return 1
		//@ 4
		c;
		try {
			//@ 5
			d;
			//@ 6
			// yield return 2
			//@ 7
			e;
			try {
				//@ 8
				f;
				//@ 9
				// goto lbl1
				//@ 10
				g;
			}
			finally {
				//@ 11
				h;
			}
			//@ 12
			i;
			//@ 13
			// yield return 4
			//@ 14
			j;
		}
		finally {
			//@ 15
			k;
		}
		//@ 16
		l;
		//@ 17
		// yield return 5
lbl1:
		//@ 18
		m;
	}
	finally {
		//@ 19
		n;
	}
	//@ 20
	o;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 19
		n;
	};
	$finally2 = function() {
		//@ 15
		k;
	};
	$finally3 = function() {
		//@ 11
		h;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
				case 13: {
					try {
						switch ($state1) {
							case 6:
							case 7:
							case 8:
							case 9:
							case 10:
							case 11:
							case 13: {
								try {
									switch ($state1) {
										case 10:
										case 11: {
											try {
												break;
											}
											finally {
												$finally3.call(this);
											}
										}
									}
									break;
								}
								finally {
									$finally2.call(this);
								}
							}
						}
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 2;
					//@ 2
					b;
					//@ 3
					setCurrent(1);
					$state1 = 4;
					return true;
					//@ none
				}
				case 4: {
					//@ none
					$state1 = 2;
					//@ 4
					c;
					//@ none
					$state1 = 6;
					//@ 5
					d;
					//@ 6
					setCurrent(2);
					$state1 = 8;
					return true;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 20
					o;
					//@ none
					$state1 = -1;
					break $loop1;
					//@ none
				}
				case 8: {
					//@ none
					$state1 = 6;
					//@ 7
					e;
					//@ none
					$state1 = 10;
					//@ 8
					f;
					//@ 9
					try {
						$state1 = 6;
						$finally3.call(this);
					}
					finally {
						$state1 = 2;
						$finally2.call(this);
					}
					$state1 = 12;
					continue $loop1;
					//@ 10
					g;
					//@ none
					$state1 = 11;
					continue $loop1;
					//@ none
				}
				case 7: {
					//@ none
					$state1 = 2;
					$finally2.call(this);
					$state1 = 5;
					continue $loop1;
					//@ none
				}
				case 5: {
					//@ none
					$state1 = 2;
					//@ 16
					l;
					//@ 17
					setCurrent(5);
					$state1 = 12;
					return true;
					//@ none
				}
				case 11: {
					//@ none
					$state1 = 6;
					$finally3.call(this);
					$state1 = 9;
					continue $loop1;
					//@ none
				}
				case 9: {
					//@ none
					$state1 = 6;
					//@ 12
					i;
					//@ 13
					setCurrent(4);
					$state1 = 13;
					return true;
					//@ none
				}
				case 12: {
					//@ none
					$state1 = 2;
					//@ 18
					m;
					//@ none
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				case 13: {
					//@ none
					$state1 = 6;
					//@ 14
					j;
					//@ none
					$state1 = 7;
					continue $loop1;
					//@ none
				}
				default: {
					//@ none
					break $loop1;
				}
			}
		}
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void GotoOutOfTryBlockExecutesFinallyHandlers2() {
			AssertCorrect(
@"{
	//@ 1
	a;
	try {
		//@ 2
		b;
		//@ 3
		// yield return 1
		//@ 4
		c;
		try {
			//@ 5
			d;
			//@ 6
			// yield return 2
			//@ 7
			e;
			try {
				//@ 8
				f1;
				//@ 9
				// goto lbl1
				//@ 10
				g1;
			}
			catch (ex) {
				//@ 11
				f2;
				//@ 12
				// goto lbl1
				//@ 13
				g2;
			}
			finally {
				//@ 14
				h;
			}
			//@ 15
			i;
			//@ 16
			// yield return 4
			//@ 17
			j;
		}
		finally {
			//@ 18
			k;
		}
		//@ 19
		l;
		//@ 20
		// yield return 5
lbl1:
		//@ 21
		m;
	}
	finally {
		//@ 22
		n;
	}
	//@ 23
	o;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 22
		n;
	};
	$finally2 = function() {
		//@ 18
		k;
	};
	$finally3 = function() {
		//@ 14
		h;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
				case 13: {
					try {
						switch ($state1) {
							case 6:
							case 7:
							case 8:
							case 9:
							case 10:
							case 11:
							case 13: {
								try {
									switch ($state1) {
										case 10:
										case 11: {
											try {
												break;
											}
											finally {
												$finally3.call(this);
											}
										}
									}
									break;
								}
								finally {
									$finally2.call(this);
								}
							}
						}
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 2;
					//@ 2
					b;
					//@ 3
					setCurrent(1);
					$state1 = 4;
					return true;
					//@ none
				}
				case 4: {
					//@ none
					$state1 = 2;
					//@ 4
					c;
					//@ none
					$state1 = 6;
					//@ 5
					d;
					//@ 6
					setCurrent(2);
					$state1 = 8;
					return true;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 23
					o;
					//@ none
					$state1 = -1;
					break $loop1;
					//@ none
				}
				case 8: {
					//@ none
					$state1 = 6;
					//@ 7
					e;
					//@ none
					$state1 = 10;
					//@ none
					try {
						//@ 8
						f1;
						//@ 9
						try {
							$state1 = 6;
							$finally3.call(this);
						}
						finally {
							$state1 = 2;
							$finally2.call(this);
						}
						$state1 = 12;
						continue $loop1;
						//@ 10
						g1;
					}
					catch (ex) {
						//@ 11
						f2;
						//@ 12
						try {
							$state1 = 6;
							$finally3.call(this);
						}
						finally {
							$state1 = 2;
							$finally2.call(this);
						}
						$state1 = 12;
						continue $loop1;
						//@ 13
						g2;
					}
					//@ none
					$state1 = 11;
					continue $loop1;
					//@ none
				}
				case 7: {
					//@ none
					$state1 = 2;
					$finally2.call(this);
					$state1 = 5;
					continue $loop1;
					//@ none
				}
				case 5: {
					//@ none
					$state1 = 2;
					//@ 19
					l;
					//@ 20
					setCurrent(5);
					$state1 = 12;
					return true;
					//@ none
				}
				case 11: {
					//@ none
					$state1 = 6;
					$finally3.call(this);
					$state1 = 9;
					continue $loop1;
					//@ none
				}
				case 9: {
					//@ none
					$state1 = 6;
					//@ 15
					i;
					//@ 16
					setCurrent(4);
					$state1 = 13;
					return true;
					//@ none
				}
				case 12: {
					//@ none
					$state1 = 2;
					//@ 21
					m;
					//@ none
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				case 13: {
					//@ none
					$state1 = 6;
					//@ 17
					j;
					//@ none
					$state1 = 7;
					continue $loop1;
					//@ none
				}
				default: {
					//@ none
					break $loop1;
				}
			}
		}
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void CanYieldBreakFromTryWithCatch() {
			AssertCorrect(
@"{
	//@ 1
	a;
	try {
		//@ 2
		b;
		//@ 3
		// yield break
	}
	catch (e) {
		//@ 4
		c;
		//@ 5
		// yield break
	}
}",
@"{
	var $state1 = 0;
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					try {
						//@ 2
						b;
						//@ 3
						$state1 = -1;
						break $loop1;
						//@ none
					}
					catch (e) {
						//@ 4
						c;
						//@ 5
						$state1 = -1;
						break $loop1;
						//@ none
					}
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldInTryWithFinallyNested() {
			AssertCorrect(
@"{
	//@ 1
	a;
	try {
		//@ 2
		b;
		//@ 3
		// yield return 1
		//@ 4
		c;
		try {
			//@ 5
			d;
			//@ 6
			// yield return 2
			//@ 7
			e;
			try {
				//@ 8
				f;
				//@ 9
				// yield return 3
				//@ 10
				g;
			}
			finally {
				//@ 11
				h;
			}
			//@ 12
			i;
			//@ 13
			// yield return 4
			//@ 14
			j;
		}
		finally {
			//@ 15
			k;
		}
		//@ 16
		l;
		//@ 17
		// yield return 5
		//@ 18
		m;
	}
	finally {
		//@ 19
		n;
	}
	//@ 20
	o;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 19
		n;
	};
	$finally2 = function() {
		//@ 15
		k;
	};
	$finally3 = function() {
		//@ 11
		h;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
				case 13:
				case 14: {
					try {
						switch ($state1) {
							case 6:
							case 7:
							case 8:
							case 9:
							case 10:
							case 11:
							case 12:
							case 14: {
								try {
									switch ($state1) {
										case 10:
										case 11:
										case 12: {
											try {
												break;
											}
											finally {
												$finally3.call(this);
											}
										}
									}
									break;
								}
								finally {
									$finally2.call(this);
								}
							}
						}
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 2;
					//@ 2
					b;
					//@ 3
					setCurrent(1);
					$state1 = 4;
					return true;
					//@ none
				}
				case 4: {
					//@ none
					$state1 = 2;
					//@ 4
					c;
					//@ none
					$state1 = 6;
					//@ 5
					d;
					//@ 6
					setCurrent(2);
					$state1 = 8;
					return true;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 20
					o;
					//@ none
					$state1 = -1;
					break $loop1;
					//@ none
				}
				case 8: {
					//@ none
					$state1 = 6;
					//@ 7
					e;
					//@ none
					$state1 = 10;
					//@ 8
					f;
					//@ 9
					setCurrent(3);
					$state1 = 12;
					return true;
					//@ none
				}
				case 7: {
					//@ none
					$state1 = 2;
					$finally2.call(this);
					$state1 = 5;
					continue $loop1;
					//@ none
				}
				case 5: {
					//@ none
					$state1 = 2;
					//@ 16
					l;
					//@ 17
					setCurrent(5);
					$state1 = 13;
					return true;
					//@ none
				}
				case 12: {
					//@ none
					$state1 = 10;
					//@ 10
					g;
					//@ none
					$state1 = 11;
					continue $loop1;
					//@ none
				}
				case 11: {
					//@ none
					$state1 = 6;
					$finally3.call(this);
					$state1 = 9;
					continue $loop1;
					//@ none
				}
				case 9: {
					//@ none
					$state1 = 6;
					//@ 12
					i;
					//@ 13
					setCurrent(4);
					$state1 = 14;
					return true;
					//@ none
				}
				case 13: {
					//@ none
					$state1 = 2;
					//@ 18
					m;
					//@ none
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				case 14: {
					//@ none
					$state1 = 6;
					//@ 14
					j;
					//@ none
					$state1 = 7;
					continue $loop1;
					//@ none
				}
				default: {
					//@ none
					break $loop1;
				}
			}
		}
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void IteratorWithOnlyYieldBreakWorks() {
			AssertCorrect(
@"{
	//@ 1
	// yield break
}",
@"{
	var $state1 = 0;
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ 1
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void IteratorWithYieldReturnAsLastStatement() {
			AssertCorrect(
@"{
	//@ 1
	a;
	//@ 2
	// yield return 1
}",
@"{
	var $state1 = 0;
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ 2
					setCurrent(1);
					$state1 = -1;
					return true;
					//@ none
				}
				default: {
					//@ none
					break $loop1;
				}
			}
		}
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void ForInIteratorBlock() {
			AssertCorrect(
@"{
	//@ 1
	a;
	//@ 2
	for (i = 0; i < b; i++) {
		//@ 3
		// yield return 1
	}
	//@ 4
	c;
}",
@"{
	var $state1 = 0;
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ 2
					i = 0;
					//@ none
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 2
					if (!(i < b)) {
						$state1 = 3;
						continue $loop1;
						//@ none
					}
					//@ 3
					setCurrent(1);
					$state1 = 2;
					return true;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					//@ 2
					i++;
					//@ none
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
					//@ 4
					c;
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void WhileInIteratorBlock() {
			AssertCorrect(
@"{
	//@ 1
	a;
	//@ 2
	while (b) {
		//@ 3
		// yield return c
	}
	//@ 4
	d;
}",
@"{
	var $state1 = 0;
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 2
					if (!b) {
						$state1 = 2;
						continue $loop1;
						//@ none
					}
					//@ 3
					setCurrent(c);
					$state1 = 1;
					return true;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					//@ 4
					d;
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void DoWhileInIteratorBlock() {
			AssertCorrect(
@"{
	//@ 1
	a;
	do {
		//@ 2
		// yield return b
		//@ 3
	} while (c);
	//@ 4
	d;
}",
@"{
	var $state1 = 0;
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					a;
					//@ none
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 2
					setCurrent(b);
					$state1 = 2;
					return true;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					//@ 3
					if (c) {
						$state1 = 1;
						continue $loop1;
						//@ none
					}
					//@ 4
					d;
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void ReturnToFirstStatementInsideTryAfterYieldReturn() {
			AssertCorrect(@"
{
	try {
		//@ 1
		while (a) {
			//@ 2
			// yield return 1
		}
	}
	finally {
		//@ 3
		b;
	}
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 3
		b;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 1:
				case 2:
				case 3: {
					try {
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = 1;
					//@ 1
					if (!a) {
						$state1 = 2;
						continue $loop1;
						//@ none
					}
					//@ 2
					setCurrent(1);
					$state1 = 3;
					return true;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void TwoTryBlocksAfterEachother() {
			AssertCorrect(@"
{
	try {
		try {
			//@ 1
			// yield return 1
			//@ 2
			a;
		}
		finally {
			//@ 3
			b;
		}
		try {
			//@ 4
			// yield return 2
			//@ 5
			c;
		}
		finally {
			//@ 6
			d;
		}
	}
	finally {
		//@ 7
		e;
	}
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 7
		e;
	};
	$finally2 = function() {
		//@ 3
		b;
	};
	$finally3 = function() {
		//@ 6
		d;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9: {
					try {
						switch ($state1) {
							case 4:
							case 5:
							case 6: {
								try {
									break;
								}
								finally {
									$finally2.call(this);
								}
							}
							case 7:
							case 8:
							case 9: {
								try {
									break;
								}
								finally {
									$finally3.call(this);
								}
							}
						}
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = 4;
					//@ 1
					setCurrent(1);
					$state1 = 6;
					return true;
					//@ none
				}
				case 6: {
					//@ none
					$state1 = 4;
					//@ 2
					a;
					//@ none
					$state1 = 5;
					continue $loop1;
					//@ none
				}
				case 5: {
					//@ none
					$state1 = 1;
					$finally2.call(this);
					$state1 = 3;
					continue $loop1;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = 7;
					//@ 4
					setCurrent(2);
					$state1 = 9;
					return true;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
					$state1 = -1;
					break $loop1;
					//@ none
				}
				case 9: {
					//@ none
					$state1 = 7;
					//@ 5
					c;
					//@ none
					$state1 = 8;
					continue $loop1;
					//@ none
				}
				case 8: {
					//@ none
					$state1 = 1;
					$finally3.call(this);
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				default: {
					//@ none
					break $loop1;
				}
			}
		}
		//@ none
		return false;
	}
}
", MethodType.Iterator);
		}

		[Test]
		public void ReturnToFirstStatementInsideTryAfterYieldReturnNested() {
			AssertCorrect(@"
{
	try {
		try {
			//@ 1
			while (a) {
				//@ 2
				// yield return 1
			}
		}
		finally {
			//@ 3
			b;
		}
	}
	finally {
		//@ 4
		c;
	}
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		//@ 4
		c;
	};
	$finally2 = function() {
		//@ 3
		b;
	};
	dispose = function() {
		//@ none
		try {
			switch ($state1) {
				case 1:
				case 2:
				case 3:
				case 4:
				case 5: {
					try {
						switch ($state1) {
							case 3:
							case 4:
							case 5: {
								try {
									break;
								}
								finally {
									$finally2.call(this);
								}
							}
						}
						break;
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$state1 = -1;
		}
	};
	{
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = 5;
					continue $loop1;
					//@ none
				}
				case 5: {
					//@ none
					$state1 = 3;
					//@ 1
					if (!a) {
						$state1 = 4;
						continue $loop1;
						//@ none
					}
					//@ 2
					setCurrent(1);
					$state1 = 5;
					return true;
					//@ none
				}
				case 4: {
					//@ none
					$state1 = 1;
					$finally2.call(this);
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					$finally1.call(this);
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
		//@ none
		return false;
	}
}
", methodType: MethodType.Iterator);
		}
	}
}
