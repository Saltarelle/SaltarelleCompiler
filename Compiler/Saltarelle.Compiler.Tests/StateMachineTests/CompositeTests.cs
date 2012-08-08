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
					var $tmp2 = 0;
					$loop2:
					for (;;) {
						switch ($tmp2) {
							case 0: {
								a;
								try {
									var $tmp3 = 0;
									$loop3:
									for (;;) {
										switch ($tmp3) {
											case 0: {
												b;
												try {
													var $tmp4 = 0;
													$loop4:
													for (;;) {
														switch ($tmp4) {
															case 0: {
																c;
																$tmp4 = 1;
																continue $loop4;
															}
															case 1: {
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
												$tmp3 = 1;
												continue $loop3;
											}
											case 1: {
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
								$tmp2 = 1;
								continue $loop2;
							}
							case 1: {
								l;
								break $loop2;
							}
						}
					}
				}
				catch (m) {
					var $tmp5 = 0;
					$loop5:
					for (;;) {
						switch ($tmp5) {
							case 0: {
								n;
								$tmp5 = 1;
								continue $loop5;
							}
							case 1: {
								o;
								break $loop5;
							}
						}
					}
				}
				finally {
					var $tmp6 = 0;
					$loop6:
					for (;;) {
						switch ($tmp6) {
							case 0: {
								p;
								$tmp6 = 1;
								continue $loop6;
							}
							case 1: {
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
	var $tmp3 = 0;
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
				var c = function() {
					var $tmp2 = 0;
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
								var f = function() {
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
					var $tmp2 = 0;
					$loop2:
					for (;;) {
						switch ($tmp2) {
							case 0: {
								a;
								try {
									var $tmp3 = 0;
									$loop3:
									for (;;) {
										switch ($tmp3) {
											case 0: {
												b;
												$tmp3 = 1;
												continue $loop3;
											}
											case 1: {
												$tmp2 = 1;
												continue $loop2;
											}
										}
									}
								}
								catch (c) {
									d;
									$tmp2 = 1;
									continue $loop2;
								}
								e;
								$tmp2 = 1;
								continue $loop2;
							}
							case 1: {
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
	}
}
