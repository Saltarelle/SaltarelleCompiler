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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				a;
				b;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				if (c) {
					$state1 = 2;
					continue $loop1;
				}
				d;
				$state1 = 2;
				continue $loop1;
			}
			case 2: {
				e;
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
			try {
				i;
				lbl3:
				j;
			}
			catch (k) {
			}
		}
		catch (l) {
			m;
		}
		n;
		lbl4:
		o;
	}
	catch (p) {
		q;
		lbl5:
		r;
	}
	finally {
		s;
		lbl6:
		t;
	}
}",
@"{
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				$state1 = 1;
				try {
					$loop2:
					for (;;) {
						switch ($state1) {
							case 1: {
								a;
								$state1 = 2;
								continue $loop2;
							}
							case 2: {
								$state1 = 4;
								try {
									$loop3:
									for (;;) {
										switch ($state1) {
											case 4: {
												b;
												$state1 = 5;
												continue $loop3;
											}
											case 5: {
												$state1 = 7;
												try {
													$loop4:
													for (;;) {
														switch ($state1) {
															case 7: {
																c;
																$state1 = 8;
																continue $loop4;
															}
															case 8: {
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
												g;
												$state1 = 9;
												continue $loop3;
											}
											case 9: {
												h;
												$state1 = 10;
												continue $loop3;
											}
											case 10: {
												$state1 = 11;
												try {
													$loop5:
													for (;;) {
														switch ($state1) {
															case 11: {
																i;
																$state1 = 12;
																continue $loop5;
															}
															case 12: {
																j;
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
								n;
								$state1 = 13;
								continue $loop2;
							}
							case 13: {
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
								q;
								$state1 = 15;
								continue $loop6;
							}
							case 15: {
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
					$state1 = 16;
					$loop7:
					for (;;) {
						switch ($state1) {
							case 16: {
								s;
								$state1 = 17;
								continue $loop7;
							}
							case 17: {
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
				$state1 = -1;
				break $loop1;
			}
			default: {
				break $loop1;
			}
		}
	}
}
");
		}

		[Test]
		public void NestedFunctionsAreNotTouched() {
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
	var $state1 = 0, c;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				a;
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				b;
				c = function() {
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
				$state1 = -1;
				break $loop1;
			}
			default: {
				break $loop1;
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
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				$state1 = 1;
				try {
					$loop2:
					for (;;) {
						switch ($state1) {
							case 1: {
								a;
								$state1 = 2;
								continue $loop2;
							}
							case 2: {
								$state1 = 4;
								try {
									$loop3:
									for (;;) {
										switch ($state1) {
											case 4: {
												b;
												$state1 = 5;
												continue $loop3;
											}
											case 5: {
												$state1 = 6;
												continue $loop2;
											}
											default: {
												break $loop3;
											}
										}
									}
								}
								catch (c) {
									d;
									$state1 = 6;
									continue $loop2;
								}
								$state1 = 3;
								continue $loop2;
							}
							case 3: {
								e;
								$state1 = 6;
								continue $loop2;
							}
							case 6: {
								f;
								$state1 = -1;
								break $loop2;
							}
							default: {
								break $loop2;
							}
						}
					}
				}
				catch (g) {
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
	var $state1 = 0, a, b, c, d, e, f, g, h, i, j, k, l;
	$loop1:
	for (;;) {
		switch ($state1) {
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
				$state1 = 1;
				continue $loop1;
			}
			case 1: {
				$state1 = 1;
				continue $loop1;
			}
			default: {
				break $loop1;
			}
		}
	}
}
");
		}
	}
}
