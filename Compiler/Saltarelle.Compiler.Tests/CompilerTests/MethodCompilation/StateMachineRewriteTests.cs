using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	[TestFixture]
	public class StateMachineRewriteTests : MethodCompilerTestBase {
		[Test]
		public void MethodCanUseGoto() {
			AssertCorrect(@"
public void M() {
	int a = 0, b = 0, c = 0;
	lbl1:
	if (a == 1)
		goto lbl2;
	else
		goto lbl3;
	lbl2:
	b = 0;
	goto lbl3;
	lbl3:
	c = 0;
	goto lbl1;
}",
@"function() {
	// @ none
	var $state = 0, $a, $b, $c;
	$loop1:
	for (;;) {
		switch ($state) {
			case 0: {
				// @(3, 2) - (3, 26)
				$a = 0, $b = 0, $c = 0;
				// @ none
				$state = 1;
				continue $loop1;
				// @ none
			}
			case 1: {
				// @(5, 2) - (5, 13)
				if ($a === 1) {
					// @(6, 3) - (6, 13)
					$state = 2;
					continue $loop1;
					// @ none
				}
				else {
					// @(8, 3) - (8, 13)
					$state = 3;
					continue $loop1;
					// @ none
				}
			}
			case 2: {
				// @(10, 2) - (10, 8)
				$b = 0;
				// @(11, 2) - (11, 12)
				$state = 3;
				continue $loop1;
				// @ none
			}
			case 3: {
				// @(13, 2) - (13, 8)
				$c = 0;
				// @(14, 2) - (14, 12)
				$state = 1;
				continue $loop1;
				// @ none
			}
			default: {
				// @ none
				break $loop1;
			}
		}
	}
}", addSourceLocations: true);
		}

		[Test]
		public void ConstructorCanUseGoto() {
			AssertCorrect(@"
public C() {
	int a = 0, b = 0, c = 0;
	lbl1:
	if (a == 1)
		goto lbl2;
	else
		goto lbl3;
	lbl2:
	b = 0;
	goto lbl3;
	lbl3:
	c = 0;
	goto lbl1;
}",
@"function() {
	// @ none
	var $state = 0, $a, $b, $c;
	$loop1:
	for (;;) {
		switch ($state) {
			case 0: {
				// @(2, 12) - (2, 13)
				{sm_Object}.call(this);
				// @(3, 2) - (3, 26)
				$a = 0, $b = 0, $c = 0;
				// @ none
				$state = 1;
				continue $loop1;
				// @ none
			}
			case 1: {
				// @(5, 2) - (5, 13)
				if ($a === 1) {
					// @(6, 3) - (6, 13)
					$state = 2;
					continue $loop1;
					// @ none
				}
				else {
					// @(8, 3) - (8, 13)
					$state = 3;
					continue $loop1;
					// @ none
				}
			}
			case 2: {
				// @(10, 2) - (10, 8)
				$b = 0;
				// @(11, 2) - (11, 12)
				$state = 3;
				continue $loop1;
				// @ none
			}
			case 3: {
				// @(13, 2) - (13, 8)
				$c = 0;
				// @(14, 2) - (14, 12)
				$state = 1;
				continue $loop1;
				// @ none
			}
			default: {
				// @ none
				break $loop1;
			}
		}
	}
}", methodName: ".ctor", addSourceLocations: true);
		}

		[Test]
		public void IteratorBlockReturningIEnumeratorWorks() {
			AssertCorrect(@"
public System.Collections.IEnumerator M() {
	try {
		yield return 1;
	}
	finally {
		var a = 1;
	}
}",
@"function() {
	// @ none
	var $result, $state = 0;
	var $finally = function() {
		// @(7, 3) - (7, 13)
		var $a = 1;
	};
	return $MakeEnumerator({ga_Object}, function() {
		// @ none
		$loop1:
		for (;;) {
			switch ($state) {
				case 0: {
					// @ none
					$state = 1;
					// @(4, 3) - (4, 18)
					$result = $Upcast(1, {ct_Object});
					$state = 2;
					return true;
					// @ none
				}
				case 2: {
					// @ none
					$state = -1;
					$finally.call(this);
					$state = -1;
					break $loop1;
					// @ none
				}
				default: {
					// @ none
					break $loop1;
				}
			}
		}
		// @ none
		return false;
	}, function() {
		// @ none
		return $result;
	}, function() {
		// @ none
		try {
			switch ($state) {
				case 1:
				case 2: {
					try {
						break;
					}
					finally {
						$finally.call(this);
					}
				}
			}
		}
		finally {
			$state = -1;
		}
	});
}", addSourceLocations: true);
		}

		[Test]
		public void IteratorBlockReturningIEnumeratorOfTWorks() {
			AssertCorrect(@"
public System.Collections.Generic.IEnumerator<int> M(int x) {
	try {
		yield return 1;
	}
	finally {
		var a = 1;
	}
}",
@"function($x) {
	// @ none
	var $result, $state = 0;
	var $finally = function() {
		// @(7, 3) - (7, 13)
		var $a = 1;
	};
	return $MakeEnumerator({ga_Int32}, function() {
		// @ none
		$loop1:
		for (;;) {
			switch ($state) {
				case 0: {
					// @ none
					$state = 1;
					// @(4, 3) - (4, 18)
					$result = 1;
					$state = 2;
					return true;
					// @ none
				}
				case 2: {
					// @ none
					$state = -1;
					$finally.call(this);
					$state = -1;
					break $loop1;
					// @ none
				}
				default: {
					// @ none
					break $loop1;
				}
			}
		}
		// @ none
		return false;
	}, function() {
		// @ none
		return $result;
	}, function() {
		// @ none
		try {
			switch ($state) {
				case 1:
				case 2: {
					try {
						break;
					}
					finally {
						$finally.call(this);
					}
				}
			}
		}
		finally {
			$state = -1;
		}
	});
}", addSourceLocations: true);
		}

		[Test]
		public void IteratorBlockReturningIEnumerableWorks() {
			AssertCorrect(@"
public System.Collections.IEnumerable M() {
	try {
		yield return 1;
	}
	finally {
		var a = 1;
	}
}",
@"function() {
	// @ none
	return $MakeEnumerable({ga_Object}, function() {
		return (function() {
			// @ none
			var $result, $state = 0;
			var $finally = function() {
				// @(7, 3) - (7, 13)
				var $a = 1;
			};
			return $MakeEnumerator({ga_Object}, function() {
				// @ none
				$loop1:
				for (;;) {
					switch ($state) {
						case 0: {
							// @ none
							$state = 1;
							// @(4, 3) - (4, 18)
							$result = $Upcast(1, {ct_Object});
							$state = 2;
							return true;
							// @ none
						}
						case 2: {
							// @ none
							$state = -1;
							$finally.call(this);
							$state = -1;
							break $loop1;
							// @ none
						}
						default: {
							// @ none
							break $loop1;
						}
					}
				}
				// @ none
				return false;
			}, function() {
				// @ none
				return $result;
			}, function() {
				// @ none
				try {
					switch ($state) {
						case 1:
						case 2: {
							try {
								break;
							}
							finally {
								$finally.call(this);
							}
						}
					}
				}
				finally {
					$state = -1;
				}
			});
		}).call(this);
	});
}", addSourceLocations: true);
		}

		[Test]
		public void IteratorBlockReturningIEnumerableOfTWorks() {
			AssertCorrect(@"
public System.Collections.Generic.IEnumerable<int> M(int x, int y) {
	try {
		yield return 1;
	}
	finally {
		var a = 1;
	}
}",
@"function($x, $y) {
	// @ none
	return $MakeEnumerable({ga_Int32}, function() {
		return (function($x, $y) {
			// @ none
			var $result, $state = 0;
			var $finally = function() {
				// @(7, 3) - (7, 13)
				var $a = 1;
			};
			return $MakeEnumerator({ga_Int32}, function() {
				// @ none
				$loop1:
				for (;;) {
					switch ($state) {
						case 0: {
							// @ none
							$state = 1;
							// @(4, 3) - (4, 18)
							$result = 1;
							$state = 2;
							return true;
							// @ none
						}
						case 2: {
							// @ none
							$state = -1;
							$finally.call(this);
							$state = -1;
							break $loop1;
							// @ none
						}
						default: {
							// @ none
							break $loop1;
						}
					}
				}
				// @ none
				return false;
			}, function() {
				// @ none
				return $result;
			}, function() {
				// @ none
				try {
					switch ($state) {
						case 1:
						case 2: {
							try {
								break;
							}
							finally {
								$finally.call(this);
							}
						}
					}
				}
				finally {
					$state = -1;
				}
			});
		}).call(this, $x, $y);
	});
}", addSourceLocations: true);
		}

		[Test]
		public void BlockLambdaCanUseGoto() {
			AssertCorrect(@"
public void M() {
	Action x = () => {
		int a = 0, b = 0, c = 0;
		lbl1:
		if (a == 1)
			goto lbl2;
		else
			goto lbl3;
		lbl2:
		b = 0;
		goto lbl3;
		lbl3:
		c = 0;
		goto lbl1;
	};
}",
@"function() {
	// @(3, 2) - (16, 4)
	var $x = function() {
		// @ none
		var $state = 0, $a, $b, $c;
		$loop1:
		for (;;) {
			switch ($state) {
				case 0: {
					// @(4, 3) - (4, 27)
					$a = 0, $b = 0, $c = 0;
					// @ none
					$state = 1;
					continue $loop1;
					// @ none
				}
				case 1: {
					// @(6, 3) - (6, 14)
					if ($a === 1) {
						// @(7, 4) - (7, 14)
						$state = 2;
						continue $loop1;
						// @ none
					}
					else {
						// @(9, 4) - (9, 14)
						$state = 3;
						continue $loop1;
						// @ none
					}
				}
				case 2: {
					// @(11, 3) - (11, 9)
					$b = 0;
					// @(12, 3) - (12, 13)
					$state = 3;
					continue $loop1;
					// @ none
				}
				case 3: {
					// @(14, 3) - (14, 9)
					$c = 0;
					// @(15, 3) - (15, 13)
					$state = 1;
					continue $loop1;
					// @ none
				}
				default: {
					// @ none
					break $loop1;
				}
			}
		}
	};
	// @(17, 1) - (17, 2)
}", addSourceLocations: true);
		}

		[Test]
		public void AnonymousDelegateCanUseGoto() {
			AssertCorrect(@"
public void M() {
	Action<int> x = delegate(int a) {
		int b = 0, c = 0;
		lbl1:
		if (a == 1)
			goto lbl2;
		else
			goto lbl3;
		lbl2:
		b = 0;
		goto lbl3;
		lbl3:
		c = 0;
		goto lbl1;
	};
}",
@"function() {
	// @(3, 2) - (16, 4)
	var $x = function($a) {
		// @ none
		var $state = 0, $b, $c;
		$loop1:
		for (;;) {
			switch ($state) {
				case 0: {
					// @(4, 3) - (4, 20)
					$b = 0, $c = 0;
					// @ none
					$state = 1;
					continue $loop1;
					// @ none
				}
				case 1: {
					// @(6, 3) - (6, 14)
					if ($a === 1) {
						// @(7, 4) - (7, 14)
						$state = 2;
						continue $loop1;
						// @ none
					}
					else {
						// @(9, 4) - (9, 14)
						$state = 3;
						continue $loop1;
						// @ none
					}
				}
				case 2: {
					// @(11, 3) - (11, 9)
					$b = 0;
					// @(12, 3) - (12, 13)
					$state = 3;
					continue $loop1;
					// @ none
				}
				case 3: {
					// @(14, 3) - (14, 9)
					$c = 0;
					// @(15, 3) - (15, 13)
					$state = 1;
					continue $loop1;
					// @ none
				}
				default: {
					// @ none
					break $loop1;
				}
			}
		}
	};
	// @(17, 1) - (17, 2)
}", addSourceLocations: true);
		}

		[Test]
		public void AsyncVoidMethodWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class MyAwaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public async void M() {
		var a = new MyAwaitable();
		await a;
		int i = 0;
	}
}",
@"function() {
	// @ none
	var $state = 0, $a, $tmp1, $i;
	var $sm = function() {
		// @ none
		$loop1:
		for (;;) {
			switch ($state) {
				case 0: {
					// @ none
					$state = -1;
					// @(14, 3) - (14, 29)
					$a = new {sm_MyAwaitable}();
					// @(15, 3) - (15, 11)
					$tmp1 = $a.$GetAwaiter();
					$state = 1;
					$tmp1.$OnCompleted($sm);
					return;
					// @ none
				}
				case 1: {
					// @ none
					$state = -1;
					// @(15, 3) - (15, 11)
					$tmp1.$GetResult();
					// @(16, 3) - (16, 13)
					$i = 0;
					// @ none
					$state = -1;
					break $loop1;
					// @ none
				}
				default: {
					// @ none
					break $loop1;
				}
			}
		}
	};
	$sm();
}", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void AsyncNonGenericTaskMethodWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class MyAwaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public async Task M() {
		var a = new MyAwaitable();
		int i = await a + 1;
	}
}",
@"function() {
	// @ none
	var $state = 0, $tcs = $CreateTaskCompletionSource('non-generic'), $a, $tmp1, $i;
	var $sm = function() {
		// @ none
		try {
			$loop1:
			for (;;) {
				switch ($state) {
					case 0: {
						// @ none
						$state = -1;
						// @(14, 3) - (14, 29)
						$a = new {sm_MyAwaitable}();
						// @(15, 3) - (15, 23)
						$tmp1 = $a.$GetAwaiter();
						$state = 1;
						$tmp1.$OnCompleted($sm);
						return;
						// @ none
					}
					case 1: {
						// @ none
						$state = -1;
						// @(15, 3) - (15, 23)
						$i = $tmp1.$GetResult() + 1;
						// @ none
						$state = -1;
						break $loop1;
						// @ none
					}
					default: {
						// @ none
						break $loop1;
					}
				}
			}
			// @ none
			$SetAsyncResult($tcs, '<<null>>');
		}
		catch ($tmp2) {
			// @ none
			$SetAsyncException($tcs, $tmp2);
		}
	};
	$sm();
	return $GetTask($tcs);
}", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void AsyncGenericTaskMethodWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class MyAwaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public async Task<int> M() {
		var a = new MyAwaitable();
		return await a + 1;
	}
}",
@"function() {
	// @ none
	var $state = 0, $tcs = $CreateTaskCompletionSource({ga_Int32}), $a, $tmp1;
	var $sm = function() {
		// @ none
		try {
			$loop1:
			for (;;) {
				switch ($state) {
					case 0: {
						// @ none
						$state = -1;
						// @(14, 3) - (14, 29)
						$a = new {sm_MyAwaitable}();
						// @(15, 3) - (15, 22)
						$tmp1 = $a.$GetAwaiter();
						$state = 1;
						$tmp1.$OnCompleted($sm);
						return;
						// @ none
					}
					case 1: {
						// @ none
						$state = -1;
						// @(15, 3) - (15, 22)
						$SetAsyncResult($tcs, $tmp1.$GetResult() + 1);
						return;
						// @ none
					}
					default: {
						// @ none
						break $loop1;
					}
				}
			}
		}
		catch ($tmp2) {
			// @ none
			$SetAsyncException($tcs, $tmp2);
		}
	};
	$sm();
	return $GetTask($tcs);
}", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void AsyncVoidStatementLambdaWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class MyAwaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public void M() {
		var a = new MyAwaitable();
		Action x = async () => {
			await a;
		};
	}
}",
@"function() {
	// @(14, 3) - (14, 29)
	var $a = new {sm_MyAwaitable}();
	// @(15, 3) - (17, 5)
	var $x = function() {
		// @ none
		var $state = 0, $tmp1;
		var $sm = function() {
			// @ none
			$loop1:
			for (;;) {
				switch ($state) {
					case 0: {
						// @ none
						$state = -1;
						// @(16, 4) - (16, 12)
						$tmp1 = $a.$GetAwaiter();
						$state = 1;
						$tmp1.$OnCompleted($sm);
						return;
						// @ none
					}
					case 1: {
						// @ none
						$state = -1;
						// @(16, 4) - (16, 12)
						$tmp1.$GetResult();
						// @ none
						$state = -1;
						break $loop1;
						// @ none
					}
					default: {
						// @ none
						break $loop1;
					}
				}
			}
		};
		$sm();
	};
	// @(18, 2) - (18, 3)
}", addSkeleton: false, addSourceLocations: true);
		}

		[Test, Ignore("Roslyn bug #400")]
		public void AsyncVoidExpressionLambdaWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class MyAwaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public void M() {
		var a = new MyAwaitable();
		Action x = async () => await a;
	}
}",
@"function() {
	var $a = new {sm_MyAwaitable}();
	var $x = function() {
		var $state = 0, $tmp1;
		var $sm = function() {
			$loop1:
			for (;;) {
				switch ($state) {
					case 0: {
						$state = -1;
						$tmp1 = $a.$GetAwaiter();
						$state = 1;
						$tmp1.$OnCompleted($sm);
						return;
					}
					case 1: {
						$state = -1;
						$tmp1.$GetResult();
						$state = -1;
						break $loop1;
					}
					default: {
						break $loop1;
					}
				}
			}
		};
		$sm();
	};
}", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void AsyncNonGenericTaskStatementLambdaWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class MyAwaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public void M() {
		var a = new MyAwaitable();
		Func<Task> x = async() => {
			await a;
		};
	}
}",
@"function() {
	// @(14, 3) - (14, 29)
	var $a = new {sm_MyAwaitable}();
	// @(15, 3) - (17, 5)
	var $x = function() {
		// @ none
		var $state = 0, $tcs = $CreateTaskCompletionSource('non-generic'), $tmp1;
		var $sm = function() {
			// @ none
			try {
				$loop1:
				for (;;) {
					switch ($state) {
						case 0: {
							// @ none
							$state = -1;
							// @(16, 4) - (16, 12)
							$tmp1 = $a.$GetAwaiter();
							$state = 1;
							$tmp1.$OnCompleted($sm);
							return;
							// @ none
						}
						case 1: {
							// @ none
							$state = -1;
							// @(16, 4) - (16, 12)
							$tmp1.$GetResult();
							// @ none
							$state = -1;
							break $loop1;
							// @ none
						}
						default: {
							// @ none
							break $loop1;
						}
					}
				}
				// @ none
				$SetAsyncResult($tcs, '<<null>>');
			}
			catch ($tmp2) {
				// @ none
				$SetAsyncException($tcs, $tmp2);
			}
		};
		$sm();
		return $GetTask($tcs);
	};
	// @(18, 2) - (18, 3)
}", addSkeleton: false, addSourceLocations: true);
		}

		[Test, Ignore("Roslyn bug #400")]
		public void AsyncNonGenericTaskExpressionLambdaWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class MyAwaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public void M() {
		var a = new MyAwaitable();
		Func<Task> x = async() => await a;
	}
}",
@"function() {
	var $a = new {sm_MyAwaitable}();
	var $x = function() {
		var $state = 0, $tcs = $CreateTaskCompletionSource('non-generic'), $tmp1;
		var $sm = function() {
			try {
				$loop1:
				for (;;) {
					switch ($state) {
						case 0: {
							$state = -1;
							$tmp1 = $a.$GetAwaiter();
							$state = 1;
							$tmp1.$OnCompleted($sm);
							return;
						}
						case 1: {
							$state = -1;
							$tmp1.$GetResult();
							$state = -1;
							break $loop1;
						}
						default: {
							break $loop1;
						}
					}
				}
				$SetAsyncResult($tcs, '<<null>>');
			}
			catch ($tmp2) {
				$SetAsyncException($tcs, $tmp2);
			}
		};
		$sm();
		return $GetTask($tcs);
	};
}", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void AsyncGenericTaskStatementLambdaWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class MyAwaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public void M() {
		var a = new MyAwaitable();
		Func<Task<int>> x = async() => {
			return await a + 1;
		};
	}
}",
@"function() {
	// @(14, 3) - (14, 29)
	var $a = new {sm_MyAwaitable}();
	// @(15, 3) - (17, 5)
	var $x = function() {
		// @ none
		var $state = 0, $tcs = $CreateTaskCompletionSource({ga_Int32}), $tmp1;
		var $sm = function() {
			// @ none
			try {
				$loop1:
				for (;;) {
					switch ($state) {
						case 0: {
							// @ none
							$state = -1;
							// @(16, 4) - (16, 23)
							$tmp1 = $a.$GetAwaiter();
							$state = 1;
							$tmp1.$OnCompleted($sm);
							return;
							// @ none
						}
						case 1: {
							// @ none
							$state = -1;
							// @(16, 4) - (16, 23)
							$SetAsyncResult($tcs, $tmp1.$GetResult() + 1);
							return;
							// @ none
						}
						default: {
							// @ none
							break $loop1;
						}
					}
				}
			}
			catch ($tmp2) {
				// @ none
				$SetAsyncException($tcs, $tmp2);
			}
		};
		$sm();
		return $GetTask($tcs);
	};
	// @(18, 2) - (18, 3)
}", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void AsyncGenericTaskExpressionLambdaWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class MyAwaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public void M() {
		var a = new MyAwaitable();
		Func<Task<int>> x = async() => await a + 1;
	}
}",
@"function() {
	// @(14, 3) - (14, 29)
	var $a = new {sm_MyAwaitable}();
	// @(15, 3) - (15, 46)
	var $x = function() {
		// @ none
		var $state = 0, $tcs = $CreateTaskCompletionSource({ga_Int32}), $tmp1;
		var $sm = function() {
			// @ none
			try {
				$loop1:
				for (;;) {
					switch ($state) {
						case 0: {
							// @ none
							$state = -1;
							// @(15, 34) - (15, 45)
							$tmp1 = $a.$GetAwaiter();
							$state = 1;
							$tmp1.$OnCompleted($sm);
							return;
							// @ none
						}
						case 1: {
							// @ none
							$state = -1;
							// @(15, 34) - (15, 45)
							$SetAsyncResult($tcs, $tmp1.$GetResult() + 1);
							return;
							// @ none
						}
						default: {
							// @ none
							break $loop1;
						}
					}
				}
			}
			catch ($tmp2) {
				// @ none
				$SetAsyncException($tcs, $tmp2);
			}
		};
		$sm();
		return $GetTask($tcs);
	};
	// @(16, 2) - (16, 3)
}", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void AsyncVoidAnonymousDelegateWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class MyAwaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public void M() {
		var a = new MyAwaitable();
		Action x = async delegate() {
			await a;
		};
	}
}",
@"function() {
	// @(14, 3) - (14, 29)
	var $a = new {sm_MyAwaitable}();
	// @(15, 3) - (17, 5)
	var $x = function() {
		// @ none
		var $state = 0, $tmp1;
		var $sm = function() {
			// @ none
			$loop1:
			for (;;) {
				switch ($state) {
					case 0: {
						// @ none
						$state = -1;
						// @(16, 4) - (16, 12)
						$tmp1 = $a.$GetAwaiter();
						$state = 1;
						$tmp1.$OnCompleted($sm);
						return;
						// @ none
					}
					case 1: {
						// @ none
						$state = -1;
						// @(16, 4) - (16, 12)
						$tmp1.$GetResult();
						// @ none
						$state = -1;
						break $loop1;
						// @ none
					}
					default: {
						// @ none
						break $loop1;
					}
				}
			}
		};
		$sm();
	};
	// @(18, 2) - (18, 3)
}", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void AsyncNonGenericTaskAnonymousDelegateWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class MyAwaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public void M() {
		var a = new MyAwaitable();
		Func<Task> x = async delegate() {
			await a;
		};
	}
}",
@"function() {
	// @(14, 3) - (14, 29)
	var $a = new {sm_MyAwaitable}();
	// @(15, 3) - (17, 5)
	var $x = function() {
		// @ none
		var $state = 0, $tcs = $CreateTaskCompletionSource('non-generic'), $tmp1;
		var $sm = function() {
			// @ none
			try {
				$loop1:
				for (;;) {
					switch ($state) {
						case 0: {
							// @ none
							$state = -1;
							// @(16, 4) - (16, 12)
							$tmp1 = $a.$GetAwaiter();
							$state = 1;
							$tmp1.$OnCompleted($sm);
							return;
							// @ none
						}
						case 1: {
							// @ none
							$state = -1;
							// @(16, 4) - (16, 12)
							$tmp1.$GetResult();
							// @ none
							$state = -1;
							break $loop1;
							// @ none
						}
						default: {
							// @ none
							break $loop1;
						}
					}
				}
				// @ none
				$SetAsyncResult($tcs, '<<null>>');
			}
			catch ($tmp2) {
				// @ none
				$SetAsyncException($tcs, $tmp2);
			}
		};
		$sm();
		return $GetTask($tcs);
	};
	// @(18, 2) - (18, 3)
}", addSkeleton: false, addSourceLocations: true);
		}

		[Test]
		public void AsyncGenericTaskAnonymousDelegateWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class MyAwaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public void M() {
		var a = new MyAwaitable();
		Func<Task<int>> x = async delegate() {
			return await a + 1;
		};
	}
}",
@"function() {
	// @(14, 3) - (14, 29)
	var $a = new {sm_MyAwaitable}();
	// @(15, 3) - (17, 5)
	var $x = function() {
		// @ none
		var $state = 0, $tcs = $CreateTaskCompletionSource({ga_Int32}), $tmp1;
		var $sm = function() {
			// @ none
			try {
				$loop1:
				for (;;) {
					switch ($state) {
						case 0: {
							// @ none
							$state = -1;
							// @(16, 4) - (16, 23)
							$tmp1 = $a.$GetAwaiter();
							$state = 1;
							$tmp1.$OnCompleted($sm);
							return;
							// @ none
						}
						case 1: {
							// @ none
							$state = -1;
							// @(16, 4) - (16, 23)
							$SetAsyncResult($tcs, $tmp1.$GetResult() + 1);
							return;
							// @ none
						}
						default: {
							// @ none
							break $loop1;
						}
					}
				}
			}
			catch ($tmp2) {
				// @ none
				$SetAsyncException($tcs, $tmp2);
			}
		};
		$sm();
		return $GetTask($tcs);
	};
	// @(18, 2) - (18, 3)
}", addSkeleton: false, addSourceLocations: true);
		}
	}
}
