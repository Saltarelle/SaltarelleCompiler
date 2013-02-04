using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CoreLib.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class CallBaseTests {
		[Test]
		public void SimpleBaseCallWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class B {
	public virtual void F(int x) {}
}
public class C : B {
	public override void F(int x) {}
	public void M() {
		// BEGIN
		base.F(42);
		// END
	}
}
",
@"			$B.prototype.f.call(this, 42);
");
		}

		[Test]
		public void BaseCallWithGenericBaseTypeWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class B<T1, T2> {
	public virtual void F(int x) {}
}
public class C : B<int, string> {
	public override void F(int x) {}
	public void M() {
		// BEGIN
		base.F(42);
		// END
	}
}
",
@"			ss.makeGenericType($B$2, [ss.Int32, String]).prototype.f.call(this, 42);
");
		}

		[Test]
		public void BaseCallWithGenericMethodWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class B {
	public virtual void F<T1, T2>(int x) {}
}
public class C : B {
	public override void F<T1, T2>(int x) {}
	public void M() {
		// BEGIN
		base.F<int, string>(42);
		// END
	}
}
",
@"			$B.prototype.f(ss.Int32, String).call(this, 42);
");
		}

		[Test]
		public void InvokingParamArrayMethodThatDoesNotExpandArgumentsInExpandedFormWorks() {
			SourceVerifier.AssertSourceCorrect(
@"public class B { public virtual void F(int x, int y, params int[] args) {} }
public class C : B {
	public override void F(int x, int y, params int[] args) {}
	public void M() {
		// BEGIN
		base.F(4, 8, 59, 12, 4);
		// END
	}
}",
@"			$B.prototype.f.call(this, 4, 8, [59, 12, 4]);
");
		}

		[Test]
		public void InvokingParamArrayMethodThatDoesNotExpandArgumentsInNonExpandedFormWorks() {
			SourceVerifier.AssertSourceCorrect(
@"public class B { public virtual void F(int x, int y, params int[] args) {} }
public class C : B {
	public override void F(int x, int y, params int[] args) {}
	public void M() {
		// BEGIN
		base.F(4, 8, new[] { 59, 12, 4 });
		// END
	}
}",
@"			$B.prototype.f.call(this, 4, 8, [59, 12, 4]);
");
		}

		[Test]
		public void InvokingParamArrayMethodThatExpandsArgumentsInExpandedFormWorks() {
			SourceVerifier.AssertSourceCorrect(
@"using System.Runtime.CompilerServices;
public class B { [ExpandParams] public virtual void F(int x, int y, params int[] args) {} }
public class C : B {
	[ExpandParams] public override void F(int x, int y, params int[] args) {}
	public void M() {
		// BEGIN
		base.F(4, 8, 59, 12, 4);
		// END
	}
}",
@"			$B.prototype.f.call(this, 4, 8, 59, 12, 4);
");
		}

		[Test]
		public void InvokingParamArrayMethodThatExpandsArgumentsInNonExpandedFormWorks() {
			SourceVerifier.AssertSourceCorrect(
@"using System.Runtime.CompilerServices;
public class B {
	[ExpandParams] public virtual void F1(int x, int y, params int[] args) {}
	[ExpandParams] public virtual void F2(int x, params int[] args) {}
	[ExpandParams] public virtual void F3(params int[] args) {}
}
public class C : B {
	[ExpandParams] public override void F1(int x, int y, params int[] args) {}
	[ExpandParams] public override void F2(int x, params int[] args) {}
	[ExpandParams] public override void F3(params int[] args) {}
	public void M() {
		C c = null;
		var args = new[] { 59, 12, 4 };
		// BEGIN
		base.F1(4, 8, args);
		base.F2(42, args);
		base.F3(args);
		base.F1(4, 8, new[] { 59, 12, 4 });
		// END
	}
}",
@"			$B.prototype.f1.apply(this, [4, 8].concat(args));
			$B.prototype.f2.apply(this, [42].concat(args));
			$B.prototype.f3.apply(this, args);
			$B.prototype.f1.call(this, 4, 8, 59, 12, 4);
");
		}

		[Test]
		public void InvokingParamArrayGenericMethodThatDoesNotExpandArgumentsInExpandedFormWorks() {
			SourceVerifier.AssertSourceCorrect(
@"public class B { public virtual void F<T>(int x, int y, params int[] args) {} }
public class C : B {
	public override void F<T>(int x, int y, params int[] args) {}
	public void M() {
		// BEGIN
		base.F<int>(4, 8, 59, 12, 4);
		// END
	}
}",
@"			$B.prototype.f(ss.Int32).call(this, 4, 8, [59, 12, 4]);
");
		}

		[Test]
		public void InvokingParamArrayGenericMethodThatDoesNotExpandArgumentsInNonExpandedFormWorks() {
			SourceVerifier.AssertSourceCorrect(
@"public class B { public virtual void F<T>(int x, int y, params int[] args) {} }
public class C : B {
	public override void F<T>(int x, int y, params int[] args) {}
	public void M() {
		// BEGIN
		base.F<int>(4, 8, new[] { 59, 12, 4 });
		// END
	}
}",
@"			$B.prototype.f(ss.Int32).call(this, 4, 8, [59, 12, 4]);
");
		}

		[Test]
		public void InvokingParamArrayGenericMethodThatExpandsArgumentsInExpandedFormWorks() {
			SourceVerifier.AssertSourceCorrect(
@"using System.Runtime.CompilerServices;
public class B { [ExpandParams] public virtual void F<T>(int x, int y, params int[] args) {} }
public class C : B {
	[ExpandParams] public override void F<T>(int x, int y, params int[] args) {}
	public void M() {
		// BEGIN
		base.F<int>(4, 8, 59, 12, 4);
		// END
	}
}",
@"			$B.prototype.f(ss.Int32).call(this, 4, 8, 59, 12, 4);
");
		}

		[Test]
		public void InvokingParamArrayGenericMethodThatExpandsArgumentsInNonExpandedFormWorks() {
			SourceVerifier.AssertSourceCorrect(
@"using System.Runtime.CompilerServices;
public class B {
	[ExpandParams] public virtual void F1<T>(int x, int y, params int[] args) {}
	[ExpandParams] public virtual void F2<T>(int x, params int[] args) {}
	[ExpandParams] public virtual void F3<T>(params int[] args) {}
}
public class C : B {
	[ExpandParams] public override void F1<T>(int x, int y, params int[] args) {}
	[ExpandParams] public override void F2<T>(int x, params int[] args) {}
	[ExpandParams] public override void F3<T>(params int[] args) {}
	public void M() {
		C c = null;
		var args = new[] { 59, 12, 4 };
		// BEGIN
		base.F1<int>(4, 8, args);
		base.F2<int>(42, args);
		base.F3<int>(args);
		base.F1<int>(4, 8, new[] { 59, 12, 4 });
		// END
	}
}",
@"			$B.prototype.f1(ss.Int32).apply(this, [4, 8].concat(args));
			$B.prototype.f2(ss.Int32).apply(this, [42].concat(args));
			$B.prototype.f3(ss.Int32).apply(this, args);
			$B.prototype.f1(ss.Int32).call(this, 4, 8, 59, 12, 4);
");
		}
	}
}
