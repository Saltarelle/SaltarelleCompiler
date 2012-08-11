using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests {
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
	var $tmp1, $a, $b, $c;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				$a = 0, $b = 0, $c = 0;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if ($a === 1) {
					$tmp1 = 2;
					continue $loop1;
				}
				else {
					$tmp1 = 3;
					continue $loop1;
				}
			}
			case 2: {
				$b = 0;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				$c = 0;
				$tmp1 = 1;
				continue $loop1;
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
	var $tmp1, $a, $b, $c;
	$tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				$a = 0, $b = 0, $c = 0;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if ($a === 1) {
					$tmp1 = 2;
					continue $loop1;
				}
				else {
					$tmp1 = 3;
					continue $loop1;
				}
			}
			case 2: {
				$b = 0;
				$tmp1 = 3;
				continue $loop1;
			}
			case 3: {
				$c = 0;
				$tmp1 = 1;
				continue $loop1;
			}
		}
	}
}", methodName: ".ctor");
		}

		[Test]
		public void IteratorBlockReturningIEnumeratorWorks() {
			AssertCorrect(@"
public System.Collections.Generic.IEnumerator M() {
	try {
		yield return 1;
	}
	finally {
		var a = 1;
	}
}",
@"function() {
	var $result, $tmp1;
	$finally1 = function() {
		var $a = 1;
	};
	return $MakeEnumerator({Object}, function() {
		$tmp1 = 0;
		$loop1:
		for (;;) {
			switch ($tmp1) {
				case 0: {
					$tmp1 = 1;
					$result = 1;
					$tmp1 = 2;
					return true;
				}
				case 2: {
					$tmp1 = -1;
					$finally1.call(this);
					break $loop1;
				}
			}
		}
		return false;
	}, function() {
		return $result;
	}, function() {
		try {
			switch ($tmp1) {
				case 1:
				case 2: {
					try {
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$tmp1 = -1;
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
	var $result, $tmp1;
	$finally1 = function() {
		var $a = 1;
	};
	return $MakeEnumerator({Int32}, function() {
		$tmp1 = 0;
		$loop1:
		for (;;) {
			switch ($tmp1) {
				case 0: {
					$tmp1 = 1;
					$result = 1;
					$tmp1 = 2;
					return true;
				}
				case 2: {
					$tmp1 = -1;
					$finally1.call(this);
					break $loop1;
				}
			}
		}
		return false;
	}, function() {
		return $result;
	}, function() {
		try {
			switch ($tmp1) {
				case 1:
				case 2: {
					try {
					}
					finally {
						$finally1.call(this);
					}
				}
			}
		}
		finally {
			$tmp1 = -1;
		}
	});
}");
		}
	}
}
