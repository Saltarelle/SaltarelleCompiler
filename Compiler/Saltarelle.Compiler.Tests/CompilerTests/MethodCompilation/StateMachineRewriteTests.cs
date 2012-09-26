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
	}
}
