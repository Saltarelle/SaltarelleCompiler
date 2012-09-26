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
		}
		catch ($tmp1) {
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
						break $loop1;
					}
					default: {
						break $loop1;
					}
				}
			}
		}
		catch ($tmp1) {
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
		}
		catch ($tmp1) {
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
	return y;
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
						$tcs.setResult(y);
						return;
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
		public void AsyncMethodWithTryFinally() {
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
		try {
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
										$state1 = -1;
										a;
										$state1 = 2;
										x.onCompleted1($sm);
										$doFinally = false;
										return;
									}
									case 2: {
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
						$state1 = 3;
						y.onCompleted2($sm);
						$doFinally = false;
						return;
					}
					case 3: {
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
		}
		catch ($tmp1) {
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
		try {
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
										$state1 = -1;
										a;
										$state1 = 2;
										b.x($sm);
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
						catch (c) {
							d;
						}
						$state1 = 3;
						e.x($sm);
						return;
					}
					case 3: {
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
		}
		catch ($tmp1) {
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
		try {
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
		}
		catch ($tmp1) {
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
		try {
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
					case 10: {
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
									case 3:
									case 4:
									case 5:
									case 6:
									case 7:
									case 8:
									case 9: {
										if ($state1 === 2) {
											$state1 = 3;
										}
										try {
											$loop3:
											for (;;) {
												switch ($state1) {
													case 3: {
														$state1 = 4;
														b.x($sm);
														$doFinally = false;
														return;
													}
													case 4:
													case 5:
													case 6: {
														if ($state1 === 4) {
															$state1 = 5;
														}
														try {
															$loop4:
															for (;;) {
																switch ($state1) {
																	case 5: {
																		$state1 = 6;
																		c.x($sm);
																		$doFinally = false;
																		return;
																	}
																	case 6: {
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
														$state1 = 7;
														g.x($sm);
														$doFinally = false;
														return;
													}
													case 7:
													case 8:
													case 9: {
														if ($state1 === 7) {
															$state1 = -1;
															h;
															$state1 = 8;
														}
														try {
															$loop5:
															for (;;) {
																switch ($state1) {
																	case 8: {
																		$state1 = -1;
																		i;
																		$state1 = 9;
																		j.x($sm);
																		$doFinally = false;
																		return;
																	}
																	case 9: {
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
										$state1 = 10;
										n.x($sm);
										$doFinally = false;
										return;
									}
									case 10: {
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
							$state1 = 11;
							$loop6:
							for (;;) {
								switch ($state1) {
									case 11: {
										$state1 = -1;
										q;
										$state1 = 12;
										continue $loop6;
									}
									case 12: {
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
								$state1 = 13;
								$loop7:
								for (;;) {
									switch ($state1) {
										case 13: {
											$state1 = -1;
											s;
											$state1 = 14;
											continue $loop7;
										}
										case 14: {
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
		}
		catch ($tmp1) {
		}
	};
	$sm();
}
", MethodType.AsyncVoid);
		}
	}
}
