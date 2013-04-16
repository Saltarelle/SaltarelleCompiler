using NUnit.Framework;

namespace CoreLib.Tests.Core.Reflection {
	[TestFixture]
	public class TypeSystemTests : CoreLibTestBase {
		[Test]
		public void CastToSerializableTypeIsANoOp() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
using System.Runtime.CompilerServices;

[Serializable]
sealed class R {}

public class C {
	private void M() {
		object o = null;
		// BEGIN
		var v1 = (R)o;
		// END
	}
}",
@"			var v1 = o;
");
		}

		[Test]
		public void CastToImportedInterfaceIsANoOp() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
using System.Runtime.CompilerServices;

[Imported]
interface I {}

public class C {
	private void M() {
		object o = null;
		// BEGIN
		var v1 = (I)o;
		// END
	}
}",
@"			var v1 = o;
");
		}

		[Test]
		public void CastToImportedGenericClassIsANoOp() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
using System.Runtime.CompilerServices;

[Imported]
class C1<T> {}

public class C {
	private void M() {
		object o = null;
		// BEGIN
		var v1 = (C1<int>)o;
		// END
	}
}",
@"			var v1 = o;
");
		}

		[Test]
		public void CastBetweenTypesWithTheSameScriptNameIsANoOp() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
using System.Runtime.CompilerServices;

[ScriptName(""X"")]
class C1 {}
[ScriptName(""X"")]
class C2 : C1 {}

public class C {
	private void M() {
		C1 o = null;
		// BEGIN
		var v1 = (C2)o;
		var v2 = o as C2;
		var v3 = o is C2;
		// END
	}
}",
@"			var v1 = o;
			var v2 = o;
			var v3 = ss.isValue(o);
");
		}

		[Test]
		public void ComparingObjectToNullIsCallToIsNullOrUndefined() {
			SourceVerifier.AssertSourceCorrect(@"
using System;

public class C {
	private void M() {
		object o = null;
		// BEGIN
		var v1 = o == null;
		var v2 = null == o;
		var v3 = o != null;
		var v4 = null != o;
		// END
	}
}",
@"			var v1 = ss.isNullOrUndefined(o);
			var v2 = ss.isNullOrUndefined(o);
			var v3 = ss.isValue(o);
			var v4 = ss.isValue(o);
");
		}

		[Test]
		public void ComparingWithLiteralStringIsNotCallToIsNullOrUndefined() {
			SourceVerifier.AssertSourceCorrect(@"
using System;

public class C {
	private void M() {
		object o = null;
		// BEGIN
		var v1 = o == ""X"";
		var v2 = ""X"" == o;
		var v3 = o != ""X"";
		var v4 = ""X"" != o;
		// END
	}
}",
@"			var v1 = o === 'X';
			var v2 = 'X' === o;
			var v3 = o !== 'X';
			var v4 = 'X' !== o;
");
		}

		[Test]
		public void ComparingObjectsIsCallToReferenceEquals() {
			SourceVerifier.AssertSourceCorrect(@"
using System;

public class C {
	private void M() {
		object o1 = null, o2 = null;
		// BEGIN
		var v1 = o1 == o2;
		var v2 = o1 != o2;
		// END
	}
}",
@"			var v1 = ss.referenceEquals(o1, o2);
			var v2 = !ss.referenceEquals(o1, o2);
");
		}

		[Test]
		public void ConvertingDynamicToBoolUsesDoubleNegation() {
			SourceVerifier.AssertSourceCorrect(@"
using System;

public class C {
	private void M() {
		dynamic d = null;
		// BEGIN
		bool b = d;
		// END
	}
}",
@"			var b = !!d;
");
		}
	}
}