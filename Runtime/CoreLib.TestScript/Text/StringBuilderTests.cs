using QUnit;
using System.Text;

namespace CoreLib.TestScript.Text {
	[TestFixture]
	public class StringBuilderTests {
		private class SomeClass {
			public override string ToString() {
				return "some text";
			}
		}

		[Test]
		public void TypePropertiesAreCorrect() {
			var sb = new StringBuilder();
			Assert.AreEqual(typeof(StringBuilder).FullName, "ss.StringBuilder");
			Assert.IsTrue(typeof(StringBuilder).IsClass);
			Assert.IsTrue(sb is StringBuilder);
		}

		[Test]
		public void DefaultConstructorWorks() {
			var sb = new StringBuilder();
			Assert.AreEqual(sb.ToString(), "", "Text");
			Assert.AreEqual(sb.Length, 0, "Length");
		}

		[Test]
		public void ConstructorWithCapacityWorks() {
			var sb = new StringBuilder(55);
			Assert.AreEqual(sb.ToString(), "", "Text");
			Assert.AreEqual(sb.Length, 0, "Length");
		}

		[Test]
		public void InitialTextConstructorWorks() {
			var sb = new StringBuilder("some text");
			Assert.AreEqual(sb.ToString(), "some text", "Text");
			Assert.AreEqual(sb.Length, 9, "Length");
		}

		[Test]
		public void InitialTextConstructorWithCapacityWorks() {
			var sb = new StringBuilder("some text", 55);
			Assert.AreEqual(sb.ToString(), "some text", "Text");
			Assert.AreEqual(sb.Length, 9, "Length");
		}

		[Test]
		public void SubstringConstructorWorks() {
			var sb = new StringBuilder("some text", 5, 3, 55);
			Assert.AreEqual(sb.ToString(), "tex", "Text");
			Assert.AreEqual(sb.Length, 3, "Length");
		}

		[Test]
		public void AppendBoolWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.Append(true) == sb);
			Assert.AreEqual(sb.ToString(), "|true", "Text");
			Assert.AreEqual(sb.Length, 5, "Length");
		}

		[Test]
		public void AppendCharWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.Append('c') == sb);
			Assert.AreEqual(sb.ToString(), "|c", "Text");
			Assert.AreEqual(sb.Length, 2, "Length");
		}

		[Test]
		public void AppendIntWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.Append(123) == sb);
			Assert.AreEqual(sb.ToString(), "|123", "Text");
			Assert.AreEqual(sb.Length, 4, "Length");
		}

		[Test]
		public void AppendDoubleWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.Append(123.0) == sb);
			Assert.AreEqual(sb.ToString(), "|123", "Text");
			Assert.AreEqual(sb.Length, 4, "Length");
		}

		[Test]
		public void AppendObjectWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.Append(new SomeClass()) == sb);
			Assert.AreEqual(sb.ToString(), "|some text", "Text");
			Assert.AreEqual(sb.Length, 10, "Length");
		}

		[Test]
		public void AppendStringWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.Append("some text") == sb);
			Assert.AreEqual(sb.ToString(), "|some text", "Text");
			Assert.AreEqual(sb.Length, 10, "Length");
		}

		[Test]
		public void AppendLineWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine() == sb);
			Assert.AreEqual(sb.ToString(), "|\r\n", "Text");
			Assert.AreEqual(sb.Length, 3, "Length");
		}

		[Test]
		public void AppendLineBoolWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine(true) == sb);
			Assert.AreEqual(sb.ToString(), "|true\r\n", "Text");
			Assert.AreEqual(sb.Length, 7, "Length");
		}

		[Test]
		public void AppendLineCharWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine('c') == sb);
			Assert.AreEqual(sb.ToString(), "|c\r\n", "Text");
			Assert.AreEqual(sb.Length, 4, "Length");
		}

		[Test]
		public void AppendLineIntWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine(123) == sb);
			Assert.AreEqual(sb.ToString(), "|123\r\n", "Text");
			Assert.AreEqual(sb.Length, 6, "Length");
		}

		[Test]
		public void AppendLineDoubleWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine(123.0) == sb);
			Assert.AreEqual(sb.ToString(), "|123\r\n", "Text");
			Assert.AreEqual(sb.Length, 6, "Length");
		}

		[Test]
		public void AppendLineObjectWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine(new SomeClass()) == sb);
			Assert.AreEqual(sb.ToString(), "|some text\r\n", "Length");
			Assert.AreEqual(sb.Length, 12, "Length");
		}

		[Test]
		public void AppendLineStringWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine("some text") == sb);
			Assert.AreEqual(sb.ToString(), "|some text\r\n", "Text");
			Assert.AreEqual(sb.Length, 12, "Length");
		}

		[Test]
		public void ClearWorks() {
			var sb = new StringBuilder("some text");
			sb.Clear();
			Assert.AreEqual(sb.ToString(), "", "Text");
			Assert.AreEqual(sb.Length, 0, "Length");
		}

		[Test]
		public void ToStringWorks() {
			// Yes, this is tested by every other test as well. Included for completeness only
			var sb = new StringBuilder("some text");
			Assert.AreEqual(sb.ToString(), "some text");
		}

		[Test]
		public void LengthPropertyWorks() {
			// Yes, this is tested by every other test as well. Included for completeness only
			var sb = new StringBuilder("some text");
			Assert.AreEqual(sb.Length, 9);
		}

		[Test]
		public void IsEmptyPropertyWorks() {
			var sb = new StringBuilder("some text");
			Assert.IsFalse(sb.IsEmpty, "#1");
			sb.Clear();
			Assert.IsTrue(sb.IsEmpty, "#2");

			sb = new StringBuilder("");
			Assert.IsTrue(sb.IsEmpty, "#3");

			sb = new StringBuilder();
			Assert.IsTrue(sb.IsEmpty, "#4");
			sb.Append("");
			Assert.IsTrue(sb.IsEmpty, "#5");
			sb.Append("x");
			Assert.IsFalse(sb.IsEmpty, "#6");
		}
	}
}
