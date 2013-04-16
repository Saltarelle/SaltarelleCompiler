using QUnit;
using System.Text.RegularExpressions;

namespace CoreLib.TestScript.Text.RegularExpressions {
	[TestFixture]
	public class RegexTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			var re = new Regex("");
			Assert.AreEqual(typeof(Regex).FullName, "RegExp");
			Assert.AreEqual(typeof(Regex).IsClass, true);
			Assert.IsTrue(re is Regex);
		}

		[Test]
		public void StringOnlyConstructorWorks() {
			var re = new Regex("test123");
			Assert.AreEqual(re.Source, "test123");
			Assert.IsFalse(re.Global);
		}

		[Test]
		public void ConstructorWithFlagsWorks() {
			var re = new Regex("test123", "g");
			Assert.AreEqual(re.Source, "test123");
			Assert.IsTrue(re.Global);
		}

		[Test]
		public void GlobalFlagWorks() {
			Assert.IsFalse(new Regex("x", "").Global);
			Assert.IsTrue(new Regex("x", "g").Global);
		}

		[Test]
		public void IgnoreCaseFlagWorks() {
			Assert.IsFalse(new Regex("x", "").IgnoreCase);
			Assert.IsTrue(new Regex("x", "i").IgnoreCase);
		}

		[Test]
		public void MultilineFlagWorks() {
			Assert.IsFalse(new Regex("x", "").Multiline);
			Assert.IsTrue(new Regex("x", "m").Multiline);
		}

		[Test]
		public void PatternPropertyWorks() {
			Assert.AreEqual(new Regex("test123", "").Pattern, "test123");
		}

		[Test]
		public void SourcePropertyWorks() {
			Assert.AreEqual(new Regex("test123", "").Source, "test123");
		}

		[Test]
		public void ExecWorks() {
			var re = new Regex("a|b", "g");
			var m = re.Exec("xaybz");
			Assert.AreEqual(m.Index, 1);
			Assert.AreEqual(m.Length, 1);
			Assert.AreEqual(m[0], "a");
		}

		[Test]
		public void LastIndexWorks() {
			var re = new Regex("a|b", "g");
			re.Exec("xaybz");
			Assert.AreEqual(re.LastIndex, 2);
		}

		[Test]
		public void TestWorks() {
			Assert.IsTrue(new Regex("a|b").Test("xaybz"));
			Assert.IsFalse(new Regex("c").Test("xaybz"));
		}
	}
}
