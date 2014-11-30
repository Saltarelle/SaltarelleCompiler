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
	//@ 1
	x;
	//@ 2
	// await a:onCompleted1
	//@ 3
	y;
	//@ 4
	// await b:onCompleted2
	//@ 5
	z;
}", 
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					x;
					//@ 2
					$state1 = 1;
					a.onCompleted1($sm);
					return;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 3
					y;
					//@ 4
					$state1 = 2;
					b.onCompleted2($sm);
					return;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					//@ 5
					z;
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
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void VoidMethodWithAwaitAsLastStatement() {
			AssertCorrect(@"
{
	//@ 1
	x;
	//@ 2
	// await a:onCompleted1
	//@ 3
	y;
	//@ 4
	// await b:onCompleted2
}", 
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					x;
					//@ 2
					$state1 = 1;
					a.onCompleted1($sm);
					return;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 3
					y;
					//@ 4
					$state1 = 2;
					b.onCompleted2($sm);
					return;
					//@ none
				}
				case 2: {
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
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void VoidMethodWithLabelAfterAwait() {
			AssertCorrect(@"
{
	//@ 1
	x;
	//@ 2
	// await a:onCompleted1
	//@ 3
	y;
	//@ 4
	// await b:onCompleted2
lbl:
	//@ 5
	z;
}", 
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					x;
					//@ 2
					$state1 = 1;
					a.onCompleted1($sm);
					return;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 3
					y;
					//@ 4
					$state1 = 2;
					b.onCompleted2($sm);
					return;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					//@ 5
					z;
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
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodReturningTask() {
			AssertCorrect(@"
{
	//@ 1
	x;
	//@ 2
	// await a:onCompleted1
	//@ 3
	y;
	//@ 4
	// await b:onCompleted2
	//@ 5
	z;
}", 
@"{
	//@ none
	var $state1 = 0, $tcs = new TaskCompletionSource();
	var $sm = function() {
		//@ none
		try {
			$loop1:
			for (;;) {
				switch ($state1) {
					case 0: {
						//@ none
						$state1 = -1;
						//@ 1
						x;
						//@ 2
						$state1 = 1;
						a.onCompleted1($sm);
						return;
						//@ none
					}
					case 1: {
						//@ none
						$state1 = -1;
						//@ 3
						y;
						//@ 4
						$state1 = 2;
						b.onCompleted2($sm);
						return;
						//@ none
					}
					case 2: {
						//@ none
						$state1 = -1;
						//@ 5
						z;
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
			$tcs.setResult('<<null>>');
		}
		catch ($tmp1) {
			//@ none
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
	//@ 1
	return x;
	//@ 2
	// await a:onCompleted1
	//@ 3
	return;
	//@ 4
	// await b:onCompleted2
	//@ 5
	if (c) {
		//@ 6
		return y;
		//@ 7
		z;
		//@ 8
		return;
	}
	//@ 9
	return u;
}", 
@"{
	//@ none
	var $state1 = 0, $tcs = new TaskCompletionSource();
	var $sm = function() {
		//@ none
		try {
			$loop1:
			for (;;) {
				switch ($state1) {
					case 0: {
						//@ none
						$state1 = -1;
						//@ 1
						$tcs.setResult(x);
						return;
						//@ 2
						$state1 = 1;
						a.onCompleted1($sm);
						return;
						//@ none
					}
					case 1: {
						//@ none
						$state1 = -1;
						//@ 3
						$tcs.setResult('<<null>>');
						return;
						//@ 4
						$state1 = 2;
						b.onCompleted2($sm);
						return;
						//@ none
					}
					case 2: {
						//@ none
						$state1 = -1;
						//@ 5
						if (c) {
							//@ 6
							$tcs.setResult(y);
							return;
							//@ 7
							z;
							//@ 8
							$tcs.setResult('<<null>>');
							return;
						}
						//@ 9
						$tcs.setResult(u);
						return;
						//@ none
					}
					default: {
						//@ none
						break $loop1;
					}
				}
			}
		}
		catch ($tmp1) {
			//@ none
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
		//@ 1
		return x;
	}
	catch (ex) {
		//@ 2
		return y;
	}
}", 
@"{
	//@ none
	var $state1 = 0, $tcs = new TaskCompletionSource();
	var $sm = function() {
		//@ none
		try {
			$loop1:
			for (;;) {
				switch ($state1) {
					case 0: {
						//@ none
						$state1 = -1;
						try {
							//@ 1
							$tcs.setResult(x);
							return;
						}
						catch (ex) {
							//@ 2
							$tcs.setResult(y);
							return;
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
			$tcs.setResult('<<null>>');
		}
		catch ($tmp1) {
			//@ none
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
	//@ 1
	return;
	//@ 2
	// await a:onCompleted1
	//@ 3
	return;
	//@ 4
	// await b:onCompleted2
	//@ 5
	if (c) {
		//@ 6
		return;
		//@ 7
		z;
		//@ 8
		return;
	}
}",
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					return;
					//@ 2
					$state1 = 1;
					a.onCompleted1($sm);
					return;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 3
					return;
					//@ 4
					$state1 = 2;
					b.onCompleted2($sm);
					return;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					//@ 5
					if (c) {
						//@ 6
						return;
						//@ 7
						z;
						//@ 8
						return;
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
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodReturningTaskEndingWithReturn() {
			AssertCorrect(@"
{
	//@ 1
	x;
	//@ 2
	// await a:onCompleted1
	//@ 3
	y;
	//@ 4
	// await b:onCompleted2
	//@ 5
	return z;
}", 
@"{
	//@ none
	var $state1 = 0, $tcs = new TaskCompletionSource();
	var $sm = function() {
		//@ none
		try {
			$loop1:
			for (;;) {
				switch ($state1) {
					case 0: {
						//@ none
						$state1 = -1;
						//@ 1
						x;
						//@ 2
						$state1 = 1;
						a.onCompleted1($sm);
						return;
						//@ none
					}
					case 1: {
						//@ none
						$state1 = -1;
						//@ 3
						y;
						//@ 4
						$state1 = 2;
						b.onCompleted2($sm);
						return;
						//@ none
					}
					case 2: {
						//@ none
						$state1 = -1;
						//@ 5
						$tcs.setResult(z);
						return;
						//@ none
					}
					default: {
						//@ none
						break $loop1;
					}
				}
			}
		}
		catch ($tmp1) {
			//@ none
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
	//@ 1
	if (a) {
		//@ 2
		// await x:onCompleted1
	}
}",
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					if (a) {
						//@ 2
						$state1 = 1;
						x.onCompleted1($sm);
						return;
						//@ none
					}
					//@ none
					$state1 = -1;
					break $loop1;
					//@ none
				}
				case 1: {
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
		//@ 1
		a;
		//@ 2
		// await x:onCompleted1
		//@ 3
		b;
		try {
			//@ 4
			c;
		}
		finally {
			//@ 5
			d;
		}
	}
	finally {
		//@ 6
		e;
	}

	//@ 7
	// await y:onCompleted2

	try {
		//@ 8
		f;
	}
	finally {
		//@ 9
		g;
	}
}", 
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
		//@ none
		var $doFinally = true;
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 1:
				case 2: {
					//@ none
					if ($state1 === 0) {
						//@ none
						$state1 = 1;
					}
					try {
						//@ none
						$loop2:
						for (;;) {
							switch ($state1) {
								case 1: {
									//@ none
									$state1 = -1;
									//@ 1
									a;
									//@ 2
									$state1 = 2;
									x.onCompleted1($sm);
									$doFinally = false;
									return;
									//@ none
								}
								case 2: {
									//@ none
									$state1 = -1;
									//@ 3
									b;
									try {
										//@ 4
										c;
									}
									finally {
										//@ 5
										d;
									}
									//@ none
									$state1 = -1;
									break $loop2;
									//@ none
								}
								default: {
									//@ none
									break $loop2;
								}
							}
						}
					}
					finally {
						//@ none
						if ($doFinally) {
							//@ 6
							e;
						}
					}
					//@ 7
					$state1 = 3;
					y.onCompleted2($sm);
					$doFinally = false;
					return;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
					try {
						//@ 8
						f;
					}
					finally {
						//@ 9
						g;
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
		//@ 1
		a;
		//@ 2
		// await b:x
	}
	catch (c) {
		//@ 3
		d;
	}
	//@ 4
	// await e:x
	//@ 5
	f;
}", 
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 1:
				case 2: {
					//@ none
					if ($state1 === 0) {
						//@ none
						$state1 = 1;
					}
					try {
						//@ none
						$loop2:
						for (;;) {
							switch ($state1) {
								case 1: {
									//@ none
									$state1 = -1;
									//@ 1
									a;
									//@ 2
									$state1 = 2;
									b.x($sm);
									return;
									//@ none
								}
								case 2: {
									//@ none
									$state1 = -1;
									break $loop2;
									//@ none
								}
								default: {
									//@ none
									break $loop2;
								}
							}
						}
					}
					catch (c) {
						//@ 3
						d;
					}
					//@ 4
					$state1 = 3;
					e.x($sm);
					return;
					//@ none
				}
				case 3: {
					//@ none
					$state1 = -1;
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
		//@ 1
		// await a:x
	}
	finally {
		//@ 2
		b;
		lbl1:
		//@ 3
		c;
	}
}", 
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
		//@ none
		var $doFinally = true;
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0:
				case 1:
				case 2: {
					//@ none
					if ($state1 === 0) {
						//@ none
						$state1 = 1;
					}
					try {
						//@ none
						$loop2:
						for (;;) {
							switch ($state1) {
								case 1: {
									//@ 1
									$state1 = 2;
									a.x($sm);
									$doFinally = false;
									return;
									//@ none
								}
								case 2: {
									//@ none
									$state1 = -1;
									break $loop2;
									//@ none
								}
								default: {
									//@ none
									break $loop2;
								}
							}
						}
					}
					finally {
						//@ none
						if ($doFinally) {
							//@ none
							$state1 = 3;
							$loop3:
							for (;;) {
								switch ($state1) {
									case 3: {
										//@ none
										$state1 = -1;
										//@ 2
										b;
										//@ none
										$state1 = 4;
										continue $loop3;
										//@ none
									}
									case 4: {
										//@ none
										$state1 = -1;
										//@ 3
										c;
										//@ none
										$state1 = -1;
										break $loop3;
										//@ none
									}
									default: {
										//@ none
										break $loop3;
									}
								}
							}
						}
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
		//@ 1
		// await a:x
		try {
			//@ 2
			// await b:x
			try {
				//@ 3
				// await c:x
				//@ 4
				d;
			}
			catch (e) {
				//@ 5
				f;
			}
			//@ 6
			// await g:x
			//@ 7
			h;
			try {
				//@ 8
				i;
				//@ 9
				// await j:x
			}
			catch (k) {
			}
		}
		catch (l) {
			//@ 10
			m;
		}
		//@ 11
		// await n:x
		//@ 12
		o;
	}
	catch (p) {
		//@ 13
		q;
		lbl4:
		//@ 14
		r;
	}
	finally {
		//@ 15
		s;
		lbl5:
		//@ 16
		t;
	}
}",
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
		//@ none
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
				case 11: {
					//@ none
					if ($state1 === 0) {
						//@ none
						$state1 = 1;
					}
					try {
						//@ none
						$loop2:
						for (;;) {
							switch ($state1) {
								case 1: {
									//@ 1
									$state1 = 2;
									a.x($sm);
									$doFinally = false;
									return;
									//@ none
								}
								case 2:
								case 3:
								case 4:
								case 5:
								case 6:
								case 7:
								case 8:
								case 9:
								case 10: {
									//@ none
									if ($state1 === 2) {
										//@ none
										$state1 = 3;
									}
									try {
										//@ none
										$loop3:
										for (;;) {
											switch ($state1) {
												case 3: {
													//@ 2
													$state1 = 4;
													b.x($sm);
													$doFinally = false;
													return;
													//@ none
												}
												case 4:
												case 5:
												case 6: {
													//@ none
													if ($state1 === 4) {
														//@ none
														$state1 = 5;
													}
													try {
														//@ none
														$loop4:
														for (;;) {
															switch ($state1) {
																case 5: {
																	//@ 3
																	$state1 = 6;
																	c.x($sm);
																	$doFinally = false;
																	return;
																	//@ none
																}
																case 6: {
																	//@ none
																	$state1 = -1;
																	//@ 4
																	d;
																	//@ none
																	$state1 = -1;
																	break $loop4;
																	//@ none
																}
																default: {
																	//@ none
																	break $loop4;
																}
															}
														}
													}
													catch (e) {
														//@ 5
														f;
													}
													//@ 6
													$state1 = 7;
													g.x($sm);
													$doFinally = false;
													return;
													//@ none
												}
												case 7: {
													//@ none
													$state1 = -1;
													//@ 7
													h;
													//@ none
													$state1 = 8;
													continue $loop3;
													//@ none
												}
												case 8:
												case 9:
												case 10: {
													//@ none
													if ($state1 === 8) {
														//@ none
														$state1 = 9;
													}
													try {
														//@ none
														$loop5:
														for (;;) {
															switch ($state1) {
																case 9: {
																	//@ none
																	$state1 = -1;
																	//@ 8
																	i;
																	//@ 9
																	$state1 = 10;
																	j.x($sm);
																	$doFinally = false;
																	return;
																	//@ none
																}
																case 10: {
																	//@ none
																	$state1 = -1;
																	break $loop5;
																	//@ none
																}
																default: {
																	//@ none
																	break $loop5;
																}
															}
														}
													}
													catch (k) {
													}
													//@ none
													$state1 = -1;
													break $loop3;
													//@ none
												}
												default: {
													//@ none
													break $loop3;
												}
											}
										}
									}
									catch (l) {
										//@ 10
										m;
									}
									//@ 11
									$state1 = 11;
									n.x($sm);
									$doFinally = false;
									return;
									//@ none
								}
								case 11: {
									//@ none
									$state1 = -1;
									//@ 12
									o;
									//@ none
									$state1 = -1;
									break $loop2;
									//@ none
								}
								default: {
									//@ none
									break $loop2;
								}
							}
						}
					}
					catch (p) {
						//@ none
						$state1 = 12;
						$loop6:
						for (;;) {
							switch ($state1) {
								case 12: {
									//@ none
									$state1 = -1;
									//@ 13
									q;
									//@ none
									$state1 = 13;
									continue $loop6;
									//@ none
								}
								case 13: {
									//@ none
									$state1 = -1;
									//@ 14
									r;
									//@ none
									$state1 = -1;
									break $loop6;
									//@ none
								}
								default: {
									//@ none
									break $loop6;
								}
							}
						}
					}
					finally {
						//@ none
						if ($doFinally) {
							//@ none
							$state1 = 14;
							$loop7:
							for (;;) {
								switch ($state1) {
									case 14: {
										//@ none
										$state1 = -1;
										//@ 15
										s;
										//@ none
										$state1 = 15;
										continue $loop7;
										//@ none
									}
									case 15: {
										//@ none
										$state1 = -1;
										//@ 16
										t;
										//@ none
										$state1 = -1;
										break $loop7;
										//@ none
									}
									default: {
										//@ none
										break $loop7;
									}
								}
							}
						}
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
	};
	$sm();
}
", MethodType.AsyncVoid);
		}

		[Test]
		public void TwoTryBlocksInARow() {
			AssertCorrect(
@"{
	//@ 1
	a;
	try {
		//@ 2
		// await b:x
		//@ 3
		b.getResult();
	}
	catch (c) {
		//@ 4
		d;
	}

	//@ 5
	e;
	try {
		//@ 6
		// await f:x
		//@ 7
		g.getResult();
	}
	catch (h) {
		//@ 8
		i;
	}
}",
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
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
				case 1:
				case 2:
				case 3: {
					//@ none
					if ($state1 === 1) {
						//@ none
						$state1 = 2;
					}
					try {
						//@ none
						$loop2:
						for (;;) {
							switch ($state1) {
								case 2: {
									//@ 2
									$state1 = 3;
									b.x($sm);
									return;
									//@ none
								}
								case 3: {
									//@ none
									$state1 = -1;
									//@ 3
									b.getResult();
									//@ none
									$state1 = -1;
									break $loop2;
									//@ none
								}
								default: {
									//@ none
									break $loop2;
								}
							}
						}
					}
					catch (c) {
						//@ 4
						d;
					}
					//@ 5
					e;
					//@ none
					$state1 = 4;
					continue $loop1;
					//@ none
				}
				case 4:
				case 5:
				case 6: {
					//@ none
					if ($state1 === 4) {
						//@ none
						$state1 = 5;
					}
					try {
						//@ none
						$loop3:
						for (;;) {
							switch ($state1) {
								case 5: {
									//@ 6
									$state1 = 6;
									f.x($sm);
									return;
									//@ none
								}
								case 6: {
									//@ none
									$state1 = -1;
									//@ 7
									g.getResult();
									//@ none
									$state1 = -1;
									break $loop3;
									//@ none
								}
								default: {
									//@ none
									break $loop3;
								}
							}
						}
					}
					catch (h) {
						//@ 8
						i;
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
	};
	$sm();
}
", MethodType.AsyncVoid);
		}

		[Test]
		public void AsyncMethodThatUsesThis() {
			AssertCorrect(@"
{
	//@ 1
	x;
	//@ 2
	// await a:onCompleted1
	//@ 3
	this.y;
	//@ 4
	// await b:onCompleted2
	//@ 5
	z;
}", 
@"{
	//@ none
	var $state1 = 0;
	var $sm = $Bind(function() {
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					x;
					//@ 2
					$state1 = 1;
					a.onCompleted1($sm);
					return;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 3
					this.y;
					//@ 4
					$state1 = 2;
					b.onCompleted2($sm);
					return;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					//@ 5
					z;
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
	}, this);
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void TryBlockAsFirstStatementInsideIf() {
			AssertCorrect(@"
{
	//@ 1
	a;
	//@ 2
	// await b:onCompleted1
	//@ 3
	c;
	//@ 4
	if (d) {
		try {
			//@ 5
			e;
			//@ 6
			// await f:onCompleted2
			//@ 7
			g;
		}
		catch (h) {
		}
	}
	//@ 8
	i;
}", 
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
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
					$state1 = 1;
					b.onCompleted1($sm);
					return;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 3
					c;
					//@ 4
					if (d) {
						//@ none
						$state1 = 3;
						continue $loop1;
						//@ none
					}
					//@ none
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				case 3:
				case 4:
				case 5: {
					//@ none
					if ($state1 === 3) {
						//@ none
						$state1 = 4;
					}
					try {
						//@ none
						$loop2:
						for (;;) {
							switch ($state1) {
								case 4: {
									//@ none
									$state1 = -1;
									//@ 5
									e;
									//@ 6
									$state1 = 5;
									f.onCompleted2($sm);
									return;
									//@ none
								}
								case 5: {
									//@ none
									$state1 = -1;
									//@ 7
									g;
									//@ none
									$state1 = -1;
									break $loop2;
									//@ none
								}
								default: {
									//@ none
									break $loop2;
								}
							}
						}
					}
					catch (h) {
					}
					//@ none
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					//@ 8
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
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void AwaitInIfWithEmptyElse() {
			AssertCorrect(@"
{
	//@ 1
	if (a) {
		//@ 2
		// await b:c
		//@ 3
		d;
	}
	else {
	}
	//@ 4
	e;
}", 
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					if (a) {
						//@ 2
						$state1 = 2;
						b.c($sm);
						return;
						//@ none
					}
					else {
					}
					//@ none
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					//@ 3
					d;
					//@ none
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 4
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
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}

		[Test]
		public void AwaitInElseWithEmptyIf() {
			AssertCorrect(@"
{
	//@ 1
	if (a) {
	}
	else {
		//@ 2
		// await b:c
		//@ 3
		d;
	}
	//@ 4
	e;
}", 
@"{
	//@ none
	var $state1 = 0;
	var $sm = function() {
		//@ none
		$loop1:
		for (;;) {
			switch ($state1) {
				case 0: {
					//@ none
					$state1 = -1;
					//@ 1
					if (a) {
					}
					else {
						//@ 2
						$state1 = 2;
						b.c($sm);
						return;
						//@ none
					}
					//@ none
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 2: {
					//@ none
					$state1 = -1;
					//@ 3
					d;
					//@ none
					$state1 = 1;
					continue $loop1;
					//@ none
				}
				case 1: {
					//@ none
					$state1 = -1;
					//@ 4
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
	};
	$sm();
}
", methodType: MethodType.AsyncVoid);
		}
	}
}
