using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests {
	[TestFixture]
	public class PostProcessingTests : MethodCompilerTestBase {
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
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				var $a = 0, $b = 0, $c = 0;
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
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				var $a = 0, $b = 0, $c = 0;
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
	}
}
