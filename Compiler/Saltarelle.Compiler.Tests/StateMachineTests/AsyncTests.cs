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
						$state1 = -1;
						b.onCompleted2($sm);
						return;
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
						$state1 = -1;
						try {
							$state1 = 1;
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
		public void AsyncMethodWithNestedTryBlocks() {
			Assert.Fail("TODO. Test multiple nesting, and multiple inner blocks and nested statemachines in both try, catch and finally");
		}
	}
}
