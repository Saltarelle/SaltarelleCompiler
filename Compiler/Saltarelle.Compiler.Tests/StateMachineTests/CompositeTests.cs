using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.StateMachineTests {
	[TestFixture]
	public class CompositeTests : StateMachineRewriterTestBase {
		[Test]
		public void CanRewriteGotoToStateMachine() {
			AssertCorrect(
@"{
	//@ 1
	a;
	//@ 2
	b;
lbl1:
	//@ 3
	if (c) {
		//@ 4
		// goto lbl2
	}
	//@ 5
	d;
lbl2:
	//@ 6
	e;
	//@ 7
	f;
}", 
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				a;
				//@ 2
				b;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ 3
				if (c) {
					//@ 4
					$state1 = 2;
					continue $loop1;
					//@ none
				}
				//@ 5
				d;
				//@ none
				$state1 = 2;
				continue $loop1;
				//@ none
			}
			case 2: {
				//@ 6
				e;
				//@ 7
				f;
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			default: {
				//@ none
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
		//@ 1
		a;
		try {
			//@ 2
			b;
			try {
				//@ 3
				c;
				lbl1:
				//@ 4
				d;
			}
			catch (e) {
				//@ 5
				f;
			}
			//@ 6
			g;
			lbl2:
			//@ 7
			h;
			try {
				//@ 8
				i;
				lbl3:
				//@ 9
				j;
			}
			catch (k) {
			}
		}
		catch (l) {
			//@ 10
			m;
		}
		//@ 11
		n;
		lbl3:
		//@ 12
		o;
	}
	catch (p) {
		//@ 13
		q;
		lbl4:
		//@ 14
		r;
	}
	finally {
		//@ 15
		s;
		lbl5:
		//@ 16
		t;
	}
}",
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ none
				$state1 = 1;
				try {
					//@ none
					$loop2:
					for (;;) {
						switch ($state1) {
							case 1: {
								//@ 1
								a;
								//@ none
								$state1 = 2;
								continue $loop2;
								//@ none
							}
							case 2: {
								//@ none
								$state1 = 3;
								try {
									//@ none
									$loop3:
									for (;;) {
										switch ($state1) {
											case 3: {
												//@ 2
												b;
												//@ none
												$state1 = 4;
												continue $loop3;
												//@ none
											}
											case 4: {
												//@ none
												$state1 = 5;
												try {
													//@ none
													$loop4:
													for (;;) {
														switch ($state1) {
															case 5: {
																//@ 3
																c;
																//@ none
																$state1 = 6;
																continue $loop4;
																//@ none
															}
															case 6: {
																//@ 4
																d;
																//@ none
																$state1 = -1;
																break $loop4;
																//@ none
															}
															default: {
																//@ none
																break $loop4;
															}
														}
													}
												}
												catch (e) {
													//@ 5
													f;
												}
												//@ 6
												g;
												//@ none
												$state1 = 7;
												continue $loop3;
												//@ none
											}
											case 7: {
												//@ 7
												h;
												//@ none
												$state1 = 8;
												continue $loop3;
												//@ none
											}
											case 8: {
												//@ none
												$state1 = 9;
												try {
													//@ none
													$loop5:
													for (;;) {
														switch ($state1) {
															case 9: {
																//@ 8
																i;
																//@ none
																$state1 = 10;
																continue $loop5;
																//@ none
															}
															case 10: {
																//@ 9
																j;
																//@ none
																$state1 = -1;
																break $loop5;
																//@ none
															}
															default: {
																//@ none
																break $loop5;
															}
														}
													}
												}
												catch (k) {
												}
												//@ none
												$state1 = -1;
												break $loop3;
												//@ none
											}
											default: {
												//@ none
												break $loop3;
											}
										}
									}
								}
								catch (l) {
									//@ 10
									m;
								}
								//@ 11
								n;
								//@ 12
								o;
								//@ none
								$state1 = -1;
								break $loop2;
								//@ none
							}
							default: {
								//@ none
								break $loop2;
							}
						}
					}
				}
				catch (p) {
					//@ none
					$state1 = 11;
					$loop6:
					for (;;) {
						switch ($state1) {
							case 11: {
								//@ 13
								q;
								//@ none
								$state1 = 12;
								continue $loop6;
								//@ none
							}
							case 12: {
								//@ 14
								r;
								//@ none
								$state1 = -1;
								break $loop6;
								//@ none
							}
							default: {
								//@ none
								break $loop6;
							}
						}
					}
				}
				finally {
					//@ none
					$state1 = 13;
					$loop7:
					for (;;) {
						switch ($state1) {
							case 13: {
								//@ 15
								s;
								//@ none
								$state1 = 14;
								continue $loop7;
								//@ none
							}
							case 14: {
								//@ 16
								t;
								//@ none
								$state1 = -1;
								break $loop7;
								//@ none
							}
							default: {
								//@ none
								break $loop7;
							}
						}
					}
				}
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			default: {
				//@ none
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
	//@ none
	var $state1 = 0, c;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ none
				a;
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ none
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
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			default: {
				//@ none
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
		//@ 1
		a;
		try {
			//@ 2
			b;
			lbl1:
			//@ 3
			// goto lbl2
		}
		catch (c) {
			//@ 4
			d;
			//@ 5
			// goto lbl2
		}
		//@ 6
		e;
		lbl2:
		//@ 7
		f;
	}
	catch (g) {
	}
}",
@"{
	//@ none
	var $state1 = 0;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ none
				$state1 = 1;
				try {
					//@ none
					$loop2:
					for (;;) {
						switch ($state1) {
							case 1: {
								//@ 1
								a;
								//@ none
								$state1 = 2;
								continue $loop2;
								//@ none
							}
							case 2: {
								//@ none
								$state1 = 3;
								try {
									//@ none
									$loop3:
									for (;;) {
										switch ($state1) {
											case 3: {
												//@ 2
												b;
												//@ none
												$state1 = 4;
												continue $loop3;
												//@ none
											}
											case 4: {
												//@ 3
												$state1 = 5;
												continue $loop2;
												//@ none
											}
											default: {
												//@ none
												break $loop3;
											}
										}
									}
								}
								catch (c) {
									//@ 4
									d;
									//@ 5
									$state1 = 5;
									continue $loop2;
									//@ none
								}
								//@ 6
								e;
								//@ none
								$state1 = 5;
								continue $loop2;
								//@ none
							}
							case 5: {
								//@ 7
								f;
								//@ none
								$state1 = -1;
								break $loop2;
								//@ none
							}
							default: {
								//@ none
								break $loop2;
							}
						}
					}
				}
				catch (g) {
				}
				//@ none
				$state1 = -1;
				break $loop1;
				//@ none
			}
			default: {
				//@ none
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
	//@ 1
	var a = 0, b = 0, c;
	//@ 2
	var d, e;
	//@ 3
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
	// goto lbl1
}", 
@"{
	//@ none
	var $state1 = 0, a, b, c, d, e, f, g, h, i, j, k, l;
	$loop1:
	for (;;) {
		switch ($state1) {
			case 0: {
				//@ 1
				a = 0, b = 0;
				//@ 2
				//@ 3
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
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			case 1: {
				//@ none
				$state1 = 1;
				continue $loop1;
				//@ none
			}
			default: {
				//@ none
				break $loop1;
			}
		}
	}
}
");
		}
	}
}
