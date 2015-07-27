using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests {
	[TestFixture]
	public class AsyncTests : StateMachineRewriterTestBase {
		[Test]
		public void VoidMethodWithAwait() {
			AssertCorrect(@"
{
	x;
	await a:onCompleted1;
	y;
	await b:onCompleted2;
	z;
}", 
@"{
	var $state1 = 0;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					x;
					$state1 = 1;
					a.onCompleted1($sm);
					return;
				}
				case 1: {
					$state1 = -1;
					y;
					$state1 = 2;
					b.onCompleted2($sm);
					return;
				}
				case 2: {
					$state1 = -1;
					z;
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void VoidMethodWithAwaitAsLastStatement() {
			AssertCorrect(@"
{
	x;
	await a:onCompleted1;
	y;
	await b:onCompleted2;
}", 
@"{
	var $state1 = 0;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					x;
					$state1 = 1;
					a.onCompleted1($sm);
					return;
				}
				case 1: {
					$state1 = -1;
					y;
					$state1 = 2;
					b.onCompleted2($sm);
					return;
				}
				case 2: {
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void VoidMethodWithLabelAfterAwait() {
			AssertCorrect(@"
{
	x;
	await a:onCompleted1;
	y;
	await b:onCompleted2;
lbl: z;
}", 
@"{
	var $state1 = 0;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					x;
					$state1 = 1;
					a.onCompleted1($sm);
					return;
				}
				case 1: {
					$state1 = -1;
					y;
					$state1 = 2;
					b.onCompleted2($sm);
					return;
				}
				case 2: {
					$state1 = -1;
					z;
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodReturningTask() {
			AssertCorrect(@"
{
	x;
	await a:onCompleted1;
	y;
	await b:onCompleted2;
	z;
}", 
@"{
	var $state1 = 0, $tcs = new TaskCompletionSource();
	var $sm = function() {
		try {
			$loop1:
			for (;;) {
				switch ($state1) {
					case 0: {
						$state1 = -1;
						x;
						$state1 = 1;
						a.onCompleted1($sm);
						return;
					}
					case 1: {
						$state1 = -1;
						y;
						$state1 = 2;
						b.onCompleted2($sm);
						return;
					}
					case 2: {
						$state1 = -1;
						z;
						$state1 = -1;
						break $loop1;
					}
					default: {
						break $loop1;
					}
				}
			}
			$tcs.setResult('<<null>>');
		}
		catch ($tmp1) {
			$tcs.setException($tmp1);
		}
	};
	$sm();
	return $tcs.getTask();
}
", methodType: MethodType.AsyncTask);
		}

		[Test]
		public void AsyncMethodReturningTaskWithReturnStatements() {
			AssertCorrect(@"
{
	return x;
	await a:onCompleted1;
	return;
	await b:onCompleted2;
	if (c) {
		return y;
		z;
		return;
	}
	return u;
}", 
@"{
	var $state1 = 0, $tcs = new TaskCompletionSource();
	var $sm = function() {
		try {
			$loop1:
			for (;;) {
				switch ($state1) {
					case 0: {
						$state1 = -1;
						$tcs.setResult(x);
						return;
						$state1 = 1;
						a.onCompleted1($sm);
						return;
					}
					case 1: {
						$state1 = -1;
						$tcs.setResult('<<null>>');
						return;
						$state1 = 2;
						b.onCompleted2($sm);
						return;
					}
					case 2: {
						$state1 = -1;
						if (c) {
							$tcs.setResult(y);
							return;
							z;
							$tcs.setResult('<<null>>');
							return;
						}
						$tcs.setResult(u);
						return;
					}
					default: {
						break $loop1;
					}
				}
			}
		}
		catch ($tmp1) {
			$tcs.setException($tmp1);
		}
	};
	$sm();
	return $tcs.getTask();
}
", methodType: MethodType.AsyncTask);
		}

		[Test]
		public void AsyncMethodReturningTaskWithReturnStatementsInsideTryCatch() {
			AssertCorrect(@"
{
	try {
		return x;
	}
	catch (ex) {
		return y;
	}
}", 
@"{
	var $state1 = 0, $tcs = new TaskCompletionSource();
	var $sm = function() {
		try {
			$loop1:
			for (;;) {
				switch ($state1) {
					case 0: {
						$state1 = -1;
						try {
							$tcs.setResult(x);
							return;
						}
						catch (ex) {
							$tcs.setResult(y);
							return;
						}
						$state1 = -1;
						break $loop1;
					}
					default: {
						break $loop1;
					}
				}
			}
			$tcs.setResult('<<null>>');
		}
		catch ($tmp1) {
			$tcs.setException($tmp1);
		}
	};
	$sm();
	return $tcs.getTask();
}
", methodType: MethodType.AsyncTask);
		}

		[Test]
		public void AsyncMethodReturningVoidWithReturnStatements() {
			AssertCorrect(@"
{
	return;
	await a:onCompleted1;
	return;
	await b:onCompleted2;
	if (c) {
		return;
		z;
		return;
	}
}",
@"{
	var $state1 = 0;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					return;
					$state1 = 1;
					a.onCompleted1($sm);
					return;
				}
				case 1: {
					$state1 = -1;
					return;
					$state1 = 2;
					b.onCompleted2($sm);
					return;
				}
				case 2: {
					$state1 = -1;
					if (c) {
						return;
						z;
						return;
					}
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodReturningTaskEndingWithReturn() {
			AssertCorrect(@"
{
	x;
	await a:onCompleted1;
	y;
	await b:onCompleted2;
	return z;
}", 
@"{
	var $state1 = 0, $tcs = new TaskCompletionSource();
	var $sm = function() {
		try {
			$loop1:
			for (;;) {
				switch ($state1) {
					case 0: {
						$state1 = -1;
						x;
						$state1 = 1;
						a.onCompleted1($sm);
						return;
					}
					case 1: {
						$state1 = -1;
						y;
						$state1 = 2;
						b.onCompleted2($sm);
						return;
					}
					case 2: {
						$state1 = -1;
						$tcs.setResult(z);
						return;
					}
					default: {
						break $loop1;
					}
				}
			}
		}
		catch ($tmp1) {
			$tcs.setException($tmp1);
		}
	};
	$sm();
	return $tcs.getTask();
}
", methodType: MethodType.AsyncTask);
		}

		[Test]
		public void AwaitInsideIf() {
			AssertCorrect(@"
{
	if (a) {
		await x:onCompleted1;
	}
}",
@"{
	var $state1 = 0;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					if (a) {
						$state1 = 1;
						x.onCompleted1($sm);
						return;
					}
					$state1 = -1;
					break $loop1;
				}
				case 1: {
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodWithTryFinally() {
			AssertCorrect(@"
{
	try {
		a;
		await x:onCompleted1;
		b;
	}
	finally {
		c;
	}
	d;
}",
@"{
	var $state1 = 0;
	var $sm = function() {
		var $doFinally = true;
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 2:
				case 3: {
					if ($state1 === 0) {
						$state1 = 2;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 2: {
									$state1 = -1;
									a;
									$state1 = 3;
									x.onCompleted1($sm);
									$doFinally = false;
									return;
								}
								case 3: {
									$state1 = -1;
									b;
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					finally {
						if ($doFinally) {
							c;
						}
					}
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
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
	};
	$sm();
}
", MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodWithTryFinallyNested() {
			AssertCorrect(@"
{
	try {
		a;
		await x:onCompleted1;
		b;
		try {
			c;
			await y:onCompleted2;
		}
		finally {
			d;
		}
	}
	finally {
		e;
	}
	f;
}",
@"{
	var $state1 = 0;
	var $sm = function() {
		var $doFinally = true;
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6: {
					if ($state1 === 0) {
						$state1 = 2;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 2: {
									$state1 = -1;
									a;
									$state1 = 3;
									x.onCompleted1($sm);
									$doFinally = false;
									return;
								}
								case 3: {
									$state1 = -1;
									b;
									$state1 = 4;
									continue $loop2;
								}
								case 4:
								case 5:
								case 6: {
									if ($state1 === 4) {
										$state1 = 5;
									}
									try {
										$loop3:
										for (;;) {
											switch ($state1) {
												case 5: {
													$state1 = -1;
													c;
													$state1 = 6;
													y.onCompleted2($sm);
													$doFinally = false;
													return;
												}
												case 6: {
													$state1 = -1;
													break $loop3;
												}
												default: {
													break $loop3;
												}
											}
										}
									}
									finally {
										if ($doFinally) {
											d;
										}
									}
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					finally {
						if ($doFinally) {
							e;
						}
					}
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					f;
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodWithTryCatch() {
			AssertCorrect(@"
{
	try {
		a;
		await x:onCompleted1;
		b;
	}
	catch (ex) {
		c;
	}
	d;
}",
@"{
	var $state1 = 0;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 2:
				case 3: {
					if ($state1 === 0) {
						$state1 = 2;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 2: {
									$state1 = -1;
									a;
									$state1 = 3;
									x.onCompleted1($sm);
									return;
								}
								case 3: {
									$state1 = -1;
									b;
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					catch (ex) {
						c;
					}
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
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
	};
	$sm();
}
", MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodWithTryCatchNested() {
			AssertCorrect(@"
{
	try {
		a;
		await x:onCompleted1;
		b;
		try {
			c;
			await y:onCompleted2;
		}
		catch (ex1) {
			d;
		}
	}
	catch (ex2) {
		e;
	}
	f;
}",
@"{
	var $state1 = 0;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6: {
					if ($state1 === 0) {
						$state1 = 2;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 2: {
									$state1 = -1;
									a;
									$state1 = 3;
									x.onCompleted1($sm);
									return;
								}
								case 3: {
									$state1 = -1;
									b;
									$state1 = 4;
									continue $loop2;
								}
								case 4:
								case 5:
								case 6: {
									if ($state1 === 4) {
										$state1 = 5;
									}
									try {
										$loop3:
										for (;;) {
											switch ($state1) {
												case 5: {
													$state1 = -1;
													c;
													$state1 = 6;
													y.onCompleted2($sm);
													return;
												}
												case 6: {
													$state1 = -1;
													break $loop3;
												}
												default: {
													break $loop3;
												}
											}
										}
									}
									catch (ex1) {
										d;
									}
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					catch (ex2) {
						e;
					}
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					f;
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodWithTryCatchFinally() {
			AssertCorrect(@"
{
	try {
		a;
		await x:onCompleted1;
		b;
	}
	catch (ex) {
		c;
	}
	finally {
		d;
	}
	e;
}",
@"{
	var $state1 = 0;
	var $sm = function() {
		var $doFinally = true;
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 2:
				case 3: {
					if ($state1 === 0) {
						$state1 = 2;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 2: {
									$state1 = -1;
									a;
									$state1 = 3;
									x.onCompleted1($sm);
									$doFinally = false;
									return;
								}
								case 3: {
									$state1 = -1;
									b;
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					catch (ex) {
						c;
					}
					finally {
						if ($doFinally) {
							d;
						}
					}
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
	};
	$sm();
}
", MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodWithTryCatchFinallyNested() {
			AssertCorrect(@"
{
	try {
		a;
		await x:onCompleted1;
		b;
		try {
			c;
			await y:onCompleted2;
		}
		catch (ex1) {
			d;
		}
		finally {
			e;
		}
	}
	catch (ex2) {
		f;
	}
	finally {
		g;
	}
	h;
}",
@"{
	var $state1 = 0;
	var $sm = function() {
		var $doFinally = true;
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6: {
					if ($state1 === 0) {
						$state1 = 2;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 2: {
									$state1 = -1;
									a;
									$state1 = 3;
									x.onCompleted1($sm);
									$doFinally = false;
									return;
								}
								case 3: {
									$state1 = -1;
									b;
									$state1 = 4;
									continue $loop2;
								}
								case 4:
								case 5:
								case 6: {
									if ($state1 === 4) {
										$state1 = 5;
									}
									try {
										$loop3:
										for (;;) {
											switch ($state1) {
												case 5: {
													$state1 = -1;
													c;
													$state1 = 6;
													y.onCompleted2($sm);
													$doFinally = false;
													return;
												}
												case 6: {
													$state1 = -1;
													break $loop3;
												}
												default: {
													break $loop3;
												}
											}
										}
									}
									catch (ex1) {
										d;
									}
									finally {
										if ($doFinally) {
											e;
										}
									}
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					catch (ex2) {
						f;
					}
					finally {
						if ($doFinally) {
							g;
						}
					}
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					h;
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", MethodType.AsyncVoid);
		}

		[Test]
		public void ComplexAsyncMethodWithTryFinally() {
			AssertCorrect(@"
{
	try {
		a;
		await x:onCompleted1;
		b;
		try {
			c;
		}
		finally {
			d;
		}
	}
	finally {
		e;
	}

	await y:onCompleted2;

	try {
		f;
	}
	finally {
		g;
	}
}", 
@"{
	var $state1 = 0;
	var $sm = function() {
		var $doFinally = true;
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 2:
				case 3: {
					if ($state1 === 0) {
						$state1 = 2;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 2: {
									$state1 = -1;
									a;
									$state1 = 3;
									x.onCompleted1($sm);
									$doFinally = false;
									return;
								}
								case 3: {
									$state1 = -1;
									b;
									try {
										c;
									}
									finally {
										d;
									}
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					finally {
						if ($doFinally) {
							e;
						}
					}
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = 4;
					y.onCompleted2($sm);
					$doFinally = false;
					return;
				}
				case 4: {
					$state1 = -1;
					try {
						f;
					}
					finally {
						g;
					}
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void AwaitAsLastStatementInTry() {
			AssertCorrect(@"
{
	try {
		a;
		await b:x;
	}
	catch (c) {
		d;
	}
	await e:x;
	f;
}", 
@"{
	var $state1 = 0;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 2:
				case 3: {
					if ($state1 === 0) {
						$state1 = 2;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 2: {
									$state1 = -1;
									a;
									$state1 = 3;
									b.x($sm);
									return;
								}
								case 3: {
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					catch (c) {
						d;
					}
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = 4;
					e.x($sm);
					return;
				}
				case 4: {
					$state1 = -1;
					f;
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodWithLabelInFinally() {
			AssertCorrect(@"
{
	try {
		await a:x;
	}
	finally {
		b;
		lbl1:
		c;
	}
}", 
@"{
	var $state1 = 0;
	var $sm = function() {
		var $doFinally = true;
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 1:
				case 2: {
					if ($state1 === 0) {
						$state1 = 1;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 1: {
									$state1 = 2;
									a.x($sm);
									$doFinally = false;
									return;
								}
								case 2: {
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					finally {
						if ($doFinally) {
							$state1 = 3;
							$loop3:
							for (;;) {
								switch ($state1) {
									case 3: {
										$state1 = -1;
										b;
										$state1 = 4;
										continue $loop3;
									}
									case 4: {
										$state1 = -1;
										c;
										$state1 = -1;
										break $loop3;
									}
									default: {
										break $loop3;
									}
								}
							}
						}
					}
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodWithNestedTryBlocks() {
			// Note: This generates an extra, unnecessary, state machine. It could be removed, but that would further increase the complexity of the most complex part of the compiler.
			AssertCorrect(
@"{
	try {
		await a:x;
		try {
			await b:x;
			try {
				await c:x;
				d;
			}
			catch (e) {
				f;
			}
			await g:x;
			h;
			try {
				i;
				await j:x;
			}
			catch (k) {
			}
		}
		catch (l) {
			m;
		}
		await n:x;
		o;
	}
	catch (p) {
		q;
		lbl4:
		r;
	}
	finally {
		s;
		lbl5:
		t;
	}
}",
@"{
	var $state1 = 0;
	var $sm = function() {
		var $doFinally = true;
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 1:
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
					if ($state1 === 0) {
						$state1 = 1;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 1: {
									$state1 = 2;
									a.x($sm);
									$doFinally = false;
									return;
								}
								case 2:
								case 4:
								case 5:
								case 6:
								case 7:
								case 8:
								case 9:
								case 10:
								case 11:
								case 12: {
									if ($state1 === 2) {
										$state1 = 4;
									}
									try {
										$loop3:
										for (;;) {
											switch ($state1) {
												case 4: {
													$state1 = 5;
													b.x($sm);
													$doFinally = false;
													return;
												}
												case 5:
												case 7:
												case 8: {
													if ($state1 === 5) {
														$state1 = 7;
													}
													try {
														$loop4:
														for (;;) {
															switch ($state1) {
																case 7: {
																	$state1 = 8;
																	c.x($sm);
																	$doFinally = false;
																	return;
																}
																case 8: {
																	$state1 = -1;
																	d;
																	$state1 = -1;
																	break $loop4;
																}
																default: {
																	break $loop4;
																}
															}
														}
													}
													catch (e) {
														f;
													}
													$state1 = 6;
													continue $loop3;
												}
												case 6: {
													$state1 = 9;
													g.x($sm);
													$doFinally = false;
													return;
												}
												case 9: {
													$state1 = -1;
													h;
													$state1 = 10;
													continue $loop3;
												}
												case 10:
												case 11:
												case 12: {
													if ($state1 === 10) {
														$state1 = 11;
													}
													try {
														$loop5:
														for (;;) {
															switch ($state1) {
																case 11: {
																	$state1 = -1;
																	i;
																	$state1 = 12;
																	j.x($sm);
																	$doFinally = false;
																	return;
																}
																case 12: {
																	$state1 = -1;
																	break $loop5;
																}
																default: {
																	break $loop5;
																}
															}
														}
													}
													catch (k) {
													}
													$state1 = -1;
													break $loop3;
												}
												default: {
													break $loop3;
												}
											}
										}
									}
									catch (l) {
										m;
									}
									$state1 = 3;
									continue $loop2;
								}
								case 3: {
									$state1 = 13;
									n.x($sm);
									$doFinally = false;
									return;
								}
								case 13: {
									$state1 = -1;
									o;
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					catch (p) {
						$state1 = 14;
						$loop6:
						for (;;) {
							switch ($state1) {
								case 14: {
									$state1 = -1;
									q;
									$state1 = 15;
									continue $loop6;
								}
								case 15: {
									$state1 = -1;
									r;
									$state1 = -1;
									break $loop6;
								}
								default: {
									break $loop6;
								}
							}
						}
					}
					finally {
						if ($doFinally) {
							$state1 = 16;
							$loop7:
							for (;;) {
								switch ($state1) {
									case 16: {
										$state1 = -1;
										s;
										$state1 = 17;
										continue $loop7;
									}
									case 17: {
										$state1 = -1;
										t;
										$state1 = -1;
										break $loop7;
									}
									default: {
										break $loop7;
									}
								}
							}
						}
					}
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", MethodType.AsyncVoid);
		}

		[Test]
		public void TwoTryBlocksInARow() {
			AssertCorrect(
@"{
	a;
	try {
		await b:x;
		b.getResult();
	}
	catch (c) {
		d;
	}

	e;
	try {
		await f:x;
		g.getResult();
	}
	catch (h) {
		i;
	}
}",
@"{
	var $state1 = 0;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 1;
					continue $loop1;
				}
				case 1:
				case 3:
				case 4: {
					if ($state1 === 1) {
						$state1 = 3;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 3: {
									$state1 = 4;
									b.x($sm);
									return;
								}
								case 4: {
									$state1 = -1;
									b.getResult();
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					catch (c) {
						d;
					}
					$state1 = 2;
					continue $loop1;
				}
				case 2: {
					$state1 = -1;
					e;
					$state1 = 5;
					continue $loop1;
				}
				case 5:
				case 6:
				case 7: {
					if ($state1 === 5) {
						$state1 = 6;
					}
					try {
						$loop3:
						for (;;) {
							switch ($state1) {
								case 6: {
									$state1 = 7;
									f.x($sm);
									return;
								}
								case 7: {
									$state1 = -1;
									g.getResult();
									$state1 = -1;
									break $loop3;
								}
								default: {
									break $loop3;
								}
							}
						}
					}
					catch (h) {
						i;
					}
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodThatUsesThis() {
			AssertCorrect(@"
{
	x;
	await a:onCompleted1;
	this.y;
	await b:onCompleted2;
	z;
}", 
@"{
	var $state1 = 0;
	var $sm = $Bind(function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					x;
					$state1 = 1;
					a.onCompleted1($sm);
					return;
				}
				case 1: {
					$state1 = -1;
					this.y;
					$state1 = 2;
					b.onCompleted2($sm);
					return;
				}
				case 2: {
					$state1 = -1;
					z;
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	}, this);
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void TryBlockAsFirstStatementInsideIf() {
			AssertCorrect(@"
{
	a;
	await b:onCompleted1;
	c;
	if (d) {
		try {
			e;
			await f:onCompleted2;
			g;
		}
		catch (h) {
		}
	}
	i;
}", 
@"{
	var $state1 = 0;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					a;
					$state1 = 1;
					b.onCompleted1($sm);
					return;
				}
				case 1: {
					$state1 = -1;
					c;
					if (d) {
						$state1 = 3;
						continue $loop1;
					}
					$state1 = 2;
					continue $loop1;
				}
				case 3:
				case 4:
				case 5: {
					if ($state1 === 3) {
						$state1 = 4;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 4: {
									$state1 = -1;
									e;
									$state1 = 5;
									f.onCompleted2($sm);
									return;
								}
								case 5: {
									$state1 = -1;
									g;
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					catch (h) {
					}
					$state1 = 2;
					continue $loop1;
				}
				case 2: {
					$state1 = -1;
					i;
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void AwaitInIfWithEmptyElse() {
			AssertCorrect(@"
{
	if (a) {
		await b:c;
		d
	}
	else {
	}
	e;
}", 
@"{
	var $state1 = 0;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					if (a) {
						$state1 = 2;
						b.c($sm);
						return;
					}
					else {
					}
					$state1 = 1;
					continue $loop1;
				}
				case 2: {
					$state1 = -1;
					d;
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
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void AwaitInElseWithEmptyIf() {
			AssertCorrect(@"
{
	if (a) {
	}
	else {
		await b:c;
		d
	}
	e;
}", 
@"{
	var $state1 = 0;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					$state1 = -1;
					if (a) {
					}
					else {
						$state1 = 2;
						b.c($sm);
						return;
					}
					$state1 = 1;
					continue $loop1;
				}
				case 2: {
					$state1 = -1;
					d;
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
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void TryFinallyWithoutAwaitAfterTryFinallyWithAwait() {
			AssertCorrect(@"
{
	try {
		await x:y;
		a;
	}
	finally {
		b;
	}
	try {
		c;
	}
	finally {
		d;
	}
}", 
@"{
	var $state1 = 0;
	var $sm = function() {
		var $doFinally = true;
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 2:
				case 3: {
					if ($state1 === 0) {
						$state1 = 2;
					}
					try {
						$loop2:
						for (;;) {
							switch ($state1) {
								case 2: {
									$state1 = 3;
									x.y($sm);
									$doFinally = false;
									return;
								}
								case 3: {
									$state1 = -1;
									a;
									$state1 = -1;
									break $loop2;
								}
								default: {
									break $loop2;
								}
							}
						}
					}
					finally {
						if ($doFinally) {
							b;
						}
					}
					$state1 = 1;
					continue $loop1;
				}
				case 1: {
					$state1 = -1;
					try {
						c;
					}
					finally {
						d;
					}
					$state1 = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}
	}
}
