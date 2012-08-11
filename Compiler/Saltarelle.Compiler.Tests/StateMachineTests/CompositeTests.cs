using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests {
	[TestFixture]
	public class CompositeTests : StateMachineRewriterTestBase {
		[Test]
		public void CanRewriteGotoToStateMachine() {
			AssertCorrect(
@"{
	a;
	b;
lbl1:
	if (c)
		goto lbl2;
	d;
lbl2:
	e;
	f;
}", 
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a;
				b;
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				if (c) {
					$tmp1 = 2;
					continue $loop1;
				}
				d;
				$tmp1 = 2;
				continue $loop1;
			}
			case 2: {
				e;
				f;
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void TryCatchFinallyAreRewrittenByThemselves() {
			// Note: This generates an extra, unnecessary, state machine. It could be removed, but that would further increase the complexity of the most complex part of the compiler.
			AssertCorrect(
@"{
	try {
		a;
		try {
			b;
			try {
				c;
				lbl1:
				d;
			}
			catch (e) {
				f;
			}
			g;
			lbl2:
			h;
		}
		catch (i) {
			j;
		}
		k;
		lbl3:
		l;
	}
	catch (m) {
		n;
		lbl4:
		o;
	}
	finally {
		p;
		lbl5:
		q;
	}
}",
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				try {
					$tmp1 = 1;
					$loop2:
					for (;;) {
						switch ($tmp1) {
							case 1: {
								a;
								try {
									$tmp1 = 2;
									$loop3:
									for (;;) {
										switch ($tmp1) {
											case 2: {
												b;
												try {
													$tmp1 = 3;
													$loop4:
													for (;;) {
														switch ($tmp1) {
															case 3: {
																c;
																$tmp1 = 4;
																continue $loop4;
															}
															case 4: {
																d;
																break $loop4;
															}
														}
													}
												}
												catch (e) {
													f;
												}
												g;
												$tmp1 = 5;
												continue $loop3;
											}
											case 5: {
												h;
												break $loop3;
											}
										}
									}
								}
								catch (i) {
									j;
								}
								k;
								$tmp1 = 6;
								continue $loop2;
							}
							case 6: {
								l;
								break $loop2;
							}
						}
					}
				}
				catch (m) {
					$tmp1 = 7;
					$loop5:
					for (;;) {
						switch ($tmp1) {
							case 7: {
								n;
								$tmp1 = 8;
								continue $loop5;
							}
							case 8: {
								o;
								break $loop5;
							}
						}
					}
				}
				finally {
					$tmp1 = 9;
					$loop6:
					for (;;) {
						switch ($tmp1) {
							case 9: {
								p;
								$tmp1 = 10;
								continue $loop6;
							}
							case 10: {
								q;
								break $loop6;
							}
						}
					}
				}
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void NestedFunctionsAreRewrittenByThemselves() {
			AssertCorrect(
@"{
	a;
	lbl1:
	b;
	var c = function() {
		d;
		lbl2:
		e;
		var f = function() {
			g;
			lbl3:
			h;
		};
		i;
	};
	j;
}",
@"{
	var $tmp3 = 0, c;
	$loop3:
	for (;;) {
		switch ($tmp3) {
			case 0: {
				a;
				$tmp3 = 1;
				continue $loop3;
			}
			case 1: {
				b;
				c = function() {
					var $tmp2 = 0, f;
					$loop2:
					for (;;) {
						switch ($tmp2) {
							case 0: {
								d;
								$tmp2 = 1;
								continue $loop2;
							}
							case 1: {
								e;
								f = function() {
									var $tmp1 = 0;
									$loop1:
									for (;;) {
										switch ($tmp1) {
											case 0: {
												g;
												$tmp1 = 1;
												continue $loop1;
											}
											case 1: {
												h;
												break $loop1;
											}
										}
									}
								};
								i;
								break $loop2;
							}
						}
					}
				};
				j;
				break $loop3;
			}
		}
	}
}
");
		}

		[Test]
		public void CanGotoOuterLabelFromInnerTryBlock() {
			AssertCorrect(
@"{
	try {
		a;
		try {
			b;
			lbl1:
			goto lbl2;
		}
		catch (c) {
			d;
			goto lbl2;
		}
		e;
		lbl2:
		f;
	}
	catch (g) {
	}
}",
@"{
	var $tmp1 = 0;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				try {
					$tmp1 = 1;
					$loop2:
					for (;;) {
						switch ($tmp1) {
							case 1: {
								a;
								try {
									$tmp1 = 2;
									$loop3:
									for (;;) {
										switch ($tmp1) {
											case 2: {
												b;
												$tmp1 = 3;
												continue $loop3;
											}
											case 3: {
												$tmp1 = 4;
												continue $loop2;
											}
										}
									}
								}
								catch (c) {
									d;
									$tmp1 = 4;
									continue $loop2;
								}
								e;
								$tmp1 = 4;
								continue $loop2;
							}
							case 4: {
								f;
								break $loop2;
							}
						}
					}
				}
				catch (g) {
				}
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void VariablesInSimpleStateMachineAreDeclaredBeforeTheLoop() {
			AssertCorrect(
@"{
	var a = 0, b = 0, c;
	var d, e;
	for (var f = 0, g = 1, h; f < g; f++) {
		for (var i = 0, j; i < 0; i++) {
			for (var k; k < 0; k++) {
			}
		}
	}
	for (var l in x) {
	}
	for (m in x) {
	}
lbl1:
	goto lbl1;
}", 
@"{
	var $tmp1 = 0, a, b, c, d, e, f, g, h, i, j, k, l;
	$loop1:
	for (;;) {
		switch ($tmp1) {
			case 0: {
				a = 0, b = 0;
				for (f = 0, g = 1; f < g; f++) {
					for (i = 0; i < 0; i++) {
						for (; k < 0; k++) {
						}
					}
				}
				for (l in x) {
				}
				for (m in x) {
				}
				$tmp1 = 1;
				continue $loop1;
			}
			case 1: {
				$tmp1 = 1;
				continue $loop1;
			}
		}
	}
}
");
		}
	}
}
