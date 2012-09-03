using System;
using System.Collections.Generic;
using System.Testing;
using System.Text;

namespace CoreLibTests {
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
			Assert.AreEqual(sb.ToString(), "");
		}

		[Test]
		public void InitialTextConstructorWorks() {
			var sb = new StringBuilder("some text");
			Assert.AreEqual(sb.ToString(), "some text");
		}

		[Test]
		public void AppendBoolWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.Append(true) == sb);
			Assert.AreEqual(sb.ToString(), "|true");
		}

		[Test]
		public void AppendCharWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.Append('c') == sb);
			Assert.AreEqual(sb.ToString(), "|c");
		}

		[Test]
		public void AppendIntWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.Append(123) == sb);
			Assert.AreEqual(sb.ToString(), "|123");
		}

		[Test]
		public void AppendDoubleWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.Append(123.0) == sb);
			Assert.AreEqual(sb.ToString(), "|123");
		}

		[Test]
		public void AppendObjectWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.Append(new SomeClass()) == sb);
			Assert.AreEqual(sb.ToString(), "|some text");
		}

		[Test]
		public void AppendStringWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.Append("some text") == sb);
			Assert.AreEqual(sb.ToString(), "|some text");
		}

		[Test]
		public void AppendLineWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine() == sb);
			Assert.AreEqual(sb.ToString(), "|\r\n");
		}

		[Test]
		public void AppendLineBoolWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine(true) == sb);
			Assert.AreEqual(sb.ToString(), "|true\r\n");
		}

		[Test]
		public void AppendLineCharWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine('c') == sb);
			Assert.AreEqual(sb.ToString(), "|c\r\n");
		}

		[Test]
		public void AppendLineIntWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine(123) == sb);
			Assert.AreEqual(sb.ToString(), "|123\r\n");
		}

		[Test]
		public void AppendLineDoubleWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine(123.0) == sb);
			Assert.AreEqual(sb.ToString(), "|123\r\n");
		}

		[Test]
		public void AppendLineObjectWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine(new SomeClass()) == sb);
			Assert.AreEqual(sb.ToString(), "|some text\r\n");
		}

		[Test]
		public void AppendLineStringWorks() {
			var sb = new StringBuilder("|");
			Assert.IsTrue(sb.AppendLine("some text") == sb);
			Assert.AreEqual(sb.ToString(), "|some text\r\n");
		}

		[Test]
		public void ClearWorks() {
			var sb = new StringBuilder("some text");
			sb.Clear();
			Assert.AreEqual(sb.ToString(), "");
		}

		[Test]
		public void ToStringWorks() {
			// Yes, this is tested by every other test as well. Included for completeness only
			var sb = new StringBuilder("some text");
			Assert.AreEqual(sb.ToString(), "some text");
		}
	}
}
