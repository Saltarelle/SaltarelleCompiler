using NUnit.Framework;

namespace CoreLib.Tests.Core {
	[TestFixture]
	public class TypeSystemLanguageSupportTests : CoreLibTestBase {
		[Test]
		public void ConversionToAndFromDynamicWorks() {
			SourceVerifier.AssertSourceCorrect(@"
public class C {
	private void M() {
		int i = 0;
		object o = null;
		dynamic d = null;
		// BEGIN
		d = i;
		i = d;
		d = o;
		o = d;
		// END
	}
}",
@"			d = i;
			i = ss.Nullable.unbox(Type.cast(d, ss.Int32));
			d = o;
			o = d;
");
		}
	}
}