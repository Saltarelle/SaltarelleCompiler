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
	var $state = 0, $a, $b, $c;
	$loop1:
	for (;;) {
		switch ($state) {
			case 0: {
				$a = 0, $b = 0, $c = 0;
				$state = 1;
				continue $loop1;
			}
			case 1: {
				if ($a === 1) {
					$state = 2;
					continue $loop1;
				}
				else {
					$state = 3;
					continue $loop1;
				}
			}
			case 2: {
				$b = 0;
				$state = 3;
				continue $loop1;
			}
			case 3: {
				$c = 0;
				$state = 1;
				continue $loop1;
			}
			default: {
				break $loop1;
			}
		}
	}
}");
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
	var $state = 0, $a, $b, $c;
	$loop1:
	for (;;) {
		switch ($state) {
			case 0: {
				$a = 0, $b = 0, $c = 0;
				$state = 1;
				continue $loop1;
			}
			case 1: {
				if ($a === 1) {
					$state = 2;
					continue $loop1;
				}
				else {
					$state = 3;
					continue $loop1;
				}
			}
			case 2: {
				$b = 0;
				$state = 3;
				continue $loop1;
			}
			case 3: {
				$c = 0;
				$state = 1;
				continue $loop1;
			}
			default: {
				break $loop1;
			}
		}
	}
}", methodName: ".ctor");
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
	var $result, $state = 0;
	$finally = function() {
		var $a = 1;
	};
	return $MakeEnumerator({ga_Object}, function() {
		$loop1:
		for (;;) {
			switch ($state) {
				case 0: {
					$state = 1;
					$result = $Upcast(1, {ct_Object});
					$state = 2;
					return true;
				}
				case 2: {
					$state = -1;
					$finally.call(this);
					$state = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
		return false;
	}, function() {
		return $result;
	}, function() {
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
}");
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
	var $result, $state = 0;
	$finally = function() {
		var $a = 1;
	};
	return $MakeEnumerator({ga_Int32}, function() {
		$loop1:
		for (;;) {
			switch ($state) {
				case 0: {
					$state = 1;
					$result = 1;
					$state = 2;
					return true;
				}
				case 2: {
					$state = -1;
					$finally.call(this);
					$state = -1;
					break $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
		return false;
	}, function() {
		return $result;
	}, function() {
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
}");
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
	return $MakeEnumerable({ga_Object}, function() {
		return (function() {
			var $result, $state = 0;
			$finally = function() {
				var $a = 1;
			};
			return $MakeEnumerator({ga_Object}, function() {
				$loop1:
				for (;;) {
					switch ($state) {
						case 0: {
							$state = 1;
							$result = $Upcast(1, {ct_Object});
							$state = 2;
							return true;
						}
						case 2: {
							$state = -1;
							$finally.call(this);
							$state = -1;
							break $loop1;
						}
						default: {
							break $loop1;
						}
					}
				}
				return false;
			}, function() {
				return $result;
			}, function() {
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
}");
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
	return $MakeEnumerable({ga_Int32}, function() {
		return (function($x, $y) {
			var $result, $state = 0;
			$finally = function() {
				var $a = 1;
			};
			return $MakeEnumerator({ga_Int32}, function() {
				$loop1:
				for (;;) {
					switch ($state) {
						case 0: {
							$state = 1;
							$result = 1;
							$state = 2;
							return true;
						}
						case 2: {
							$state = -1;
							$finally.call(this);
							$state = -1;
							break $loop1;
						}
						default: {
							break $loop1;
						}
					}
				}
				return false;
			}, function() {
				return $result;
			}, function() {
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
}");
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
	var $x = function() {
		var $state = 0, $a, $b, $c;
		$loop1:
		for (;;) {
			switch ($state) {
				case 0: {
					$a = 0, $b = 0, $c = 0;
					$state = 1;
					continue $loop1;
				}
				case 1: {
					if ($a === 1) {
						$state = 2;
						continue $loop1;
					}
					else {
						$state = 3;
						continue $loop1;
					}
				}
				case 2: {
					$b = 0;
					$state = 3;
					continue $loop1;
				}
				case 3: {
					$c = 0;
					$state = 1;
					continue $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
}");
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
	var $x = function($a) {
		var $state = 0, $b, $c;
		$loop1:
		for (;;) {
			switch ($state) {
				case 0: {
					$b = 0, $c = 0;
					$state = 1;
					continue $loop1;
				}
				case 1: {
					if ($a === 1) {
						$state = 2;
						continue $loop1;
					}
					else {
						$state = 3;
						continue $loop1;
					}
				}
				case 2: {
					$b = 0;
					$state = 3;
					continue $loop1;
				}
				case 3: {
					$c = 0;
					$state = 1;
					continue $loop1;
				}
				default: {
					break $loop1;
				}
			}
		}
	};
}");
		}

		[Test]
		public void AsyncVoidMethodWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() {}
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
	var $state = 0, $a, $tmp1, $i;
	var $sm = function() {
		$loop1:
		for (;;) {
			switch ($state) {
				case 0: {
					$state = -1;
					$a = new {inst_MyAwaitable}();
					$tmp1 = $a.$GetAwaiter();
					$state = 1;
					$tmp1.$OnCompleted($sm);
					return;
				}
				case 1: {
					$state = -1;
					$tmp1.$GetResult();
					$i = 0;
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
}", addSkeleton: false);
		}

		[Test]
		public void AsyncNonGenericTaskMethodWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() {}
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
	var $state = 0, $tcs = $CreateTaskCompletionSource('non-generic'), $a, $tmp1, $i;
	var $sm = function() {
		try {
			$loop1:
			for (;;) {
				switch ($state) {
					case 0: {
						$state = -1;
						$a = new {inst_MyAwaitable}();
						$tmp1 = $a.$GetAwaiter();
						$state = 1;
						$tmp1.$OnCompleted($sm);
						return;
					}
					case 1: {
						$state = -1;
						$i = $tmp1.$GetResult() + 1;
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
}", addSkeleton: false);
		}

		[Test]
		public void AsyncGenericTaskMethodWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() {}
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
	var $state = 0, $tcs = $CreateTaskCompletionSource({ga_Int32}), $a, $tmp1;
	var $sm = function() {
		try {
			$loop1:
			for (;;) {
				switch ($state) {
					case 0: {
						$state = -1;
						$a = new {inst_MyAwaitable}();
						$tmp1 = $a.$GetAwaiter();
						$state = 1;
						$tmp1.$OnCompleted($sm);
						return;
					}
					case 1: {
						$state = -1;
						$SetAsyncResult($tcs, $tmp1.$GetResult() + 1);
						return;
					}
					default: {
						break $loop1;
					}
				}
			}
		}
		catch ($tmp2) {
			$SetAsyncException($tcs, $tmp2);
		}
	};
	$sm();
	return $GetTask($tcs);
}", addSkeleton: false);
		}

		[Test]
		public void AsyncVoidLambdaWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() {}
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
	var $a = new {inst_MyAwaitable}();
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
}", addSkeleton: false);
		}

		[Test]
		public void AsyncNonGenericTaskLambdaWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() {}
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
	var $a = new {inst_MyAwaitable}();
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
}", addSkeleton: false);
		}

		[Test]
		public void AsyncGenericTaskLambdaWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() {}
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
	var $a = new {inst_MyAwaitable}();
	var $x = function() {
		var $state = 0, $tcs = $CreateTaskCompletionSource({ga_Int32}), $tmp1;
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
							$SetAsyncResult($tcs, $tmp1.$GetResult() + 1);
							return;
						}
						default: {
							break $loop1;
						}
					}
				}
			}
			catch ($tmp2) {
				$SetAsyncException($tcs, $tmp2);
			}
		};
		$sm();
		return $GetTask($tcs);
	};
}", addSkeleton: false);
		}







		[Test]
		public void AsyncVoidAnonymousDelegateWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() {}
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
	var $a = new {inst_MyAwaitable}();
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
}", addSkeleton: false);
		}

		[Test]
		public void AsyncNonGenericTaskAnonymousDelegateWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() {}
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
	var $a = new {inst_MyAwaitable}();
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
}", addSkeleton: false);
		}

		[Test]
		public void AsyncGenericTaskAnonymousDelegateWorks() {
			AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() {}
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
	var $a = new {inst_MyAwaitable}();
	var $x = function() {
		var $state = 0, $tcs = $CreateTaskCompletionSource({ga_Int32}), $tmp1;
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
							$SetAsyncResult($tcs, $tmp1.$GetResult() + 1);
							return;
						}
						default: {
							break $loop1;
						}
					}
				}
			}
			catch ($tmp2) {
				$SetAsyncException($tcs, $tmp2);
			}
		};
		$sm();
		return $GetTask($tcs);
	};
}", addSkeleton: false);
		}
	}
}
