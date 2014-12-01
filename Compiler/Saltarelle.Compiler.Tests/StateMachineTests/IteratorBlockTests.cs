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
	var $state1 = 0;
	{
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					setCurrent(b);
					$state1 = 1;
					return true;
				}
				case 1: {
					$state1 = -1;
					c;
					setCurrent(d);
					$state1 = 2;
					return true;
				}
				case 2: {
					$state1 = -1;
					e;
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
", methodType: MethodType.Iterator);
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
	var $state1 = 0;
	{
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					setCurrent(b);
					$state1 = 1;
					return true;
				}
				case 1: {
					$state1 = -1;
					c;
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
", methodType: MethodType.Iterator);
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
	var $state1 = 0;
	{
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					setCurrent(b);
					$state1 = 1;
					return true;
				}
				case 1: {
					$state1 = -1;
					c;
					$state1 = -1;
					break $loop1;
					d;
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
", methodType: MethodType.Iterator);
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
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		d;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 2;
					b;
					setCurrent(1);
					$state1 = 4;
					return true;
				}
				case 4: {
					$state1 = 2;
					c;
					$state1 = 3;
					continue $loop1;
				}
				case 3: {
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					e;
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
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldInTryWithFinally2() {
			AssertCorrect(
@"{
	a;
	{
		try {
			b;
			yield return 1;
			c;
		}
		finally {
			d;
		}
	}
	e;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		d;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 2;
					b;
					setCurrent(1);
					$state1 = 4;
					return true;
				}
				case 4: {
					$state1 = 2;
					c;
					$state1 = 3;
					continue $loop1;
				}
				case 3: {
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					e;
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
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldInTryWithFinally3() {
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
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		d;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 1;
					b;
					setCurrent(1);
					$state1 = 3;
					return true;
				}
				case 3: {
					$state1 = 1;
					c;
					$state1 = 2;
					continue $loop1;
				}
				case 2: {
					$state1 = -1;
					$finally1.call(this);
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
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldInTryWithFinally4() {
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
	lbl2: e;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		d;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 2;
					b;
					setCurrent(1);
					$state1 = 4;
					return true;
				}
				case 4: {
					$state1 = 2;
					c;
					$state1 = 3;
					continue $loop1;
				}
				case 3: {
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					e;
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
", methodType: MethodType.Iterator);
		}

		[Test]
		public void IteratorFinallyCanContainLabelAndGoto() {
			AssertCorrect(
@"{
	a;
	try {
		yield return 0;
	}
	finally {
		b;
		lbl1:
		c;
		goto lbl1;
	}
	d;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		$state1 = 1;
		$loop2:
		for (;;) {
			switch ($state1) {
				case 1: {
					$state1 = -1;
					b;
					$state1 = 2;
					continue $loop2;
				}
				case 2: {
					$state1 = -1;
					c;
					$state1 = 2;
					continue $loop2;
				}
				default: {
					break $loop2;
				}
			}
		}
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 4;
					setCurrent(0);
					$state1 = 5;
					return true;
				}
				case 5: {
					$state1 = -1;
					$finally1.call(this);
					$state1 = 3;
					continue $loop1;
				}
				case 3: {
					$state1 = -1;
					d;
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
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldBreakExecutesFinallyHandlers() {
			AssertCorrect(
@"{
	a;
	try {
		yield return 0;
		b;
		yield break;
		c;
	}
	finally {
		d;
	}
	e;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		d;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 2;
					setCurrent(0);
					$state1 = 4;
					return true;
				}
				case 4: {
					$state1 = 2;
					b;
					$state1 = -1;
					$finally1.call(this);
					$state1 = -1;
					break $loop1;
					c;
					$state1 = 3;
					continue $loop1;
				}
				case 3: {
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					e;
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
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldBreakExecutesFinallyHandlersNested1() {
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
				yield break;
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
	var $state1 = 0;
	$finally1 = function() {
		n;
	};
	$finally2 = function() {
		k;
	};
	$finally3 = function() {
		h;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 2;
					b;
					setCurrent(1);
					$state1 = 4;
					return true;
				}
				case 4: {
					$state1 = 2;
					c;
					$state1 = 6;
					d;
					setCurrent(2);
					$state1 = 8;
					return true;
				}
				case 3: {
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					o;
					$state1 = -1;
					break $loop1;
				}
				case 8: {
					$state1 = 6;
					e;
					$state1 = 10;
					f;
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
					g;
					$state1 = 11;
					continue $loop1;
				}
				case 7: {
					$state1 = 2;
					$finally2.call(this);
					$state1 = 5;
					continue $loop1;
				}
				case 5: {
					$state1 = 2;
					l;
					setCurrent(5);
					$state1 = 12;
					return true;
				}
				case 11: {
					$state1 = 6;
					$finally3.call(this);
					$state1 = 9;
					continue $loop1;
				}
				case 9: {
					$state1 = 6;
					i;
					setCurrent(4);
					$state1 = 13;
					return true;
				}
				case 12: {
					$state1 = 2;
					m;
					$state1 = 3;
					continue $loop1;
				}
				case 13: {
					$state1 = 6;
					j;
					$state1 = 7;
					continue $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void YieldBreakExecutesFinallyHandlersNested2() {
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
				f1;
				yield break;
				g1;
			}
			catch (ex) {
				f2;
				yield break;
				g2;
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
	var $state1 = 0;
	$finally1 = function() {
		n;
	};
	$finally2 = function() {
		k;
	};
	$finally3 = function() {
		h;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 2;
					b;
					setCurrent(1);
					$state1 = 4;
					return true;
				}
				case 4: {
					$state1 = 2;
					c;
					$state1 = 6;
					d;
					setCurrent(2);
					$state1 = 8;
					return true;
				}
				case 3: {
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					o;
					$state1 = -1;
					break $loop1;
				}
				case 8: {
					$state1 = 6;
					e;
					$state1 = 10;
					try {
						f1;
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
						g1;
					}
					catch (ex) {
						f2;
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
						g2;
					}
					$state1 = 11;
					continue $loop1;
				}
				case 7: {
					$state1 = 2;
					$finally2.call(this);
					$state1 = 5;
					continue $loop1;
				}
				case 5: {
					$state1 = 2;
					l;
					setCurrent(5);
					$state1 = 12;
					return true;
				}
				case 11: {
					$state1 = 6;
					$finally3.call(this);
					$state1 = 9;
					continue $loop1;
				}
				case 9: {
					$state1 = 6;
					i;
					setCurrent(4);
					$state1 = 13;
					return true;
				}
				case 12: {
					$state1 = 2;
					m;
					$state1 = 3;
					continue $loop1;
				}
				case 13: {
					$state1 = 6;
					j;
					$state1 = 7;
					continue $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void GotoOutOfTryBlockExecutesFinallyHandlers1() {
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
				goto lbl1;
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
lbl1:
		m;
	}
	finally {
		n;
	}
	o;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		n;
	};
	$finally2 = function() {
		k;
	};
	$finally3 = function() {
		h;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 2;
					b;
					setCurrent(1);
					$state1 = 4;
					return true;
				}
				case 4: {
					$state1 = 2;
					c;
					$state1 = 6;
					d;
					setCurrent(2);
					$state1 = 8;
					return true;
				}
				case 3: {
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					o;
					$state1 = -1;
					break $loop1;
				}
				case 8: {
					$state1 = 6;
					e;
					$state1 = 10;
					f;
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
					g;
					$state1 = 11;
					continue $loop1;
				}
				case 7: {
					$state1 = 2;
					$finally2.call(this);
					$state1 = 5;
					continue $loop1;
				}
				case 5: {
					$state1 = 2;
					l;
					setCurrent(5);
					$state1 = 12;
					return true;
				}
				case 11: {
					$state1 = 6;
					$finally3.call(this);
					$state1 = 9;
					continue $loop1;
				}
				case 9: {
					$state1 = 6;
					i;
					setCurrent(4);
					$state1 = 13;
					return true;
				}
				case 12: {
					$state1 = 2;
					m;
					$state1 = 3;
					continue $loop1;
				}
				case 13: {
					$state1 = 6;
					j;
					$state1 = 7;
					continue $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void GotoOutOfTryBlockExecutesFinallyHandlers2() {
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
				f1;
				goto lbl1;
				g1;
			}
			catch (ex) {
				f2;
				goto lbl1;
				g2;
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
lbl1:
		m;
	}
	finally {
		n;
	}
	o;
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		n;
	};
	$finally2 = function() {
		k;
	};
	$finally3 = function() {
		h;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 2;
					b;
					setCurrent(1);
					$state1 = 4;
					return true;
				}
				case 4: {
					$state1 = 2;
					c;
					$state1 = 6;
					d;
					setCurrent(2);
					$state1 = 8;
					return true;
				}
				case 3: {
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					o;
					$state1 = -1;
					break $loop1;
				}
				case 8: {
					$state1 = 6;
					e;
					$state1 = 10;
					try {
						f1;
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
						g1;
					}
					catch (ex) {
						f2;
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
						g2;
					}
					$state1 = 11;
					continue $loop1;
				}
				case 7: {
					$state1 = 2;
					$finally2.call(this);
					$state1 = 5;
					continue $loop1;
				}
				case 5: {
					$state1 = 2;
					l;
					setCurrent(5);
					$state1 = 12;
					return true;
				}
				case 11: {
					$state1 = 6;
					$finally3.call(this);
					$state1 = 9;
					continue $loop1;
				}
				case 9: {
					$state1 = 6;
					i;
					setCurrent(4);
					$state1 = 13;
					return true;
				}
				case 12: {
					$state1 = 2;
					m;
					$state1 = 3;
					continue $loop1;
				}
				case 13: {
					$state1 = 6;
					j;
					$state1 = 7;
					continue $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void CanYieldBreakFromTryWithCatch() {
			AssertCorrect(
@"{
	a;
	try {
		b;
		yield break;
	}
	catch (e) {
		c;
		yield break;
	}
}",
@"{
	var $state1 = 0;
	{
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					try {
						b;
						$state1 = -1;
						break $loop1;
					}
					catch (e) {
						c;
						$state1 = -1;
						break $loop1;
					}
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
", methodType: MethodType.Iterator);
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
	var $state1 = 0;
	$finally1 = function() {
		n;
	};
	$finally2 = function() {
		k;
	};
	$finally3 = function() {
		h;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 2;
					b;
					setCurrent(1);
					$state1 = 4;
					return true;
				}
				case 4: {
					$state1 = 2;
					c;
					$state1 = 6;
					d;
					setCurrent(2);
					$state1 = 8;
					return true;
				}
				case 3: {
					$state1 = -1;
					$finally1.call(this);
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					o;
					$state1 = -1;
					break $loop1;
				}
				case 8: {
					$state1 = 6;
					e;
					$state1 = 10;
					f;
					setCurrent(3);
					$state1 = 12;
					return true;
				}
				case 7: {
					$state1 = 2;
					$finally2.call(this);
					$state1 = 5;
					continue $loop1;
				}
				case 5: {
					$state1 = 2;
					l;
					setCurrent(5);
					$state1 = 13;
					return true;
				}
				case 12: {
					$state1 = 10;
					g;
					$state1 = 11;
					continue $loop1;
				}
				case 11: {
					$state1 = 6;
					$finally3.call(this);
					$state1 = 9;
					continue $loop1;
				}
				case 9: {
					$state1 = 6;
					i;
					setCurrent(4);
					$state1 = 14;
					return true;
				}
				case 13: {
					$state1 = 2;
					m;
					$state1 = 3;
					continue $loop1;
				}
				case 14: {
					$state1 = 6;
					j;
					$state1 = 7;
					continue $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void IteratorWithOnlyYieldBreakWorks() {
			AssertCorrect(
@"{
	yield break;
}",
@"{
	var $state1 = 0;
	{
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
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
", methodType: MethodType.Iterator);
		}

		[Test]
		public void IteratorWithYieldReturnAsLastStatement() {
			AssertCorrect(
@"{
	a;
	yield return 1;
}",
@"{
	var $state1 = 0;
	{
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					setCurrent(1);
					$state1 = -1;
					return true;
				}
				default: {
					break $loop1;
				}
			}
		}
		return false;
	}
}
", methodType: MethodType.Iterator);
		}

		[Test]
		public void ForInIteratorBlock() {
			AssertCorrect(
@"{
	a;
	for (i = 0; i < b; i++)
		yield return 1;
	c;
}",
@"{
	var $state1 = 0;
	{
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					i = 0;
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					if (!(i < b)) {
						$state1 = 3;
						continue $loop1;
					}
					setCurrent(1);
					$state1 = 2;
					return true;
				}
				case 2: {
					$state1 = -1;
					i++;
					$state1 = 1;
					continue $loop1;
				}
				case 3: {
					$state1 = -1;
					c;
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
", methodType: MethodType.Iterator);
		}

		[Test]
		public void WhileInIteratorBlock() {
			AssertCorrect(
@"{
	a;
	while (b) {
		yield return c;
	}
	d;
}",
@"{
	var $state1 = 0;
	{
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					if (!b) {
						$state1 = 2;
						continue $loop1;
					}
					setCurrent(c);
					$state1 = 1;
					return true;
				}
				case 2: {
					$state1 = -1;
					d;
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
", methodType: MethodType.Iterator);
		}

		[Test]
		public void DoWhileInIteratorBlock() {
			AssertCorrect(
@"{
	a;
	do {
		yield return b;
	} while (c);
	d;
}",
@"{
	var $state1 = 0;
	{
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					setCurrent(b);
					$state1 = 2;
					return true;
				}
				case 2: {
					$state1 = -1;
					if (c) {
						$state1 = 1;
						continue $loop1;
					}
					d;
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
", methodType: MethodType.Iterator);
		}

		[Test]
		public void ReturnToFirstStatementInsideTryAfterYieldReturn() {
			AssertCorrect(@"
{
	try {
		while (a) {
			yield return 1;
		}
	}
	finally {
		b;
	}
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		b;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = 3;
					continue $loop1;
				}
				case 3: {
					$state1 = 1;
					if (!a) {
						$state1 = 2;
						continue $loop1;
					}
					setCurrent(1);
					$state1 = 3;
					return true;
				}
				case 2: {
					$state1 = -1;
					$finally1.call(this);
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
", methodType: MethodType.Iterator);
		}

		[Test]
		public void TwoTryBlocksAfterEachother() {
			AssertCorrect(@"
{
	try {
		try {
			yield return 1;
			a;
		}
		finally {
			b;
		}
		try {
			yield return 2;
			c;
		}
		finally {
			d;
		}
	}
	finally {
		e;
	}
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		e;
	};
	$finally2 = function() {
		b;
	};
	$finally3 = function() {
		d;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = 4;
					setCurrent(1);
					$state1 = 6;
					return true;
				}
				case 6: {
					$state1 = 4;
					a;
					$state1 = 5;
					continue $loop1;
				}
				case 5: {
					$state1 = 1;
					$finally2.call(this);
					$state1 = 3;
					continue $loop1;
				}
				case 3: {
					$state1 = 7;
					setCurrent(2);
					$state1 = 9;
					return true;
				}
				case 2: {
					$state1 = -1;
					$finally1.call(this);
					$state1 = -1;
					break $loop1;
				}
				case 9: {
					$state1 = 7;
					c;
					$state1 = 8;
					continue $loop1;
				}
				case 8: {
					$state1 = 1;
					$finally3.call(this);
					$state1 = 2;
					continue $loop1;
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

		[Test]
		public void ReturnToFirstStatementInsideTryAfterYieldReturnNested() {
			AssertCorrect(@"
{
	try {
		try {
			while (a) {
				yield return 1;
			}
		}
		finally {
			b;
		}
	}
	finally {
		c;
	}
}",
@"{
	var $state1 = 0;
	$finally1 = function() {
		c;
	};
	$finally2 = function() {
		b;
	};
	dispose = function() {
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
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = 5;
					continue $loop1;
				}
				case 5: {
					$state1 = 3;
					if (!a) {
						$state1 = 4;
						continue $loop1;
					}
					setCurrent(1);
					$state1 = 5;
					return true;
				}
				case 4: {
					$state1 = 1;
					$finally2.call(this);
					$state1 = 2;
					continue $loop1;
				}
				case 2: {
					$state1 = -1;
					$finally1.call(this);
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
", methodType: MethodType.Iterator);
		}
	}
}
