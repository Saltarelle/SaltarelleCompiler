using System;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class ConvertTests {
		private byte[] GetTestArr() {
			var result = new byte[64*3];
			for (int i = 0; i < 64; i++) {
				result[i * 3] = (byte)(i << 2);
				result[i * 3 + 1] = 0;
				result[i * 3 + 2] = 0;
			}
			return result;
		}

		[Test]
		public void ToBase64StringWithOnlyArrayWorks() {
			var testArr = GetTestArr();

			Assert.AreEqual(Convert.ToBase64String(testArr), "AAAABAAACAAADAAAEAAAFAAAGAAAHAAAIAAAJAAAKAAALAAAMAAANAAAOAAAPAAAQAAARAAASAAATAAAUAAAVAAAWAAAXAAAYAAAZAAAaAAAbAAAcAAAdAAAeAAAfAAAgAAAhAAAiAAAjAAAkAAAlAAAmAAAnAAAoAAApAAAqAAArAAAsAAAtAAAuAAAvAAAwAAAxAAAyAAAzAAA0AAA1AAA2AAA3AAA4AAA5AAA6AAA7AAA8AAA9AAA+AAA/AAA");
			Assert.AreEqual(Convert.ToBase64String(new byte[] { 1, 2, 3 }), "AQID");
			Assert.AreEqual(Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }), "AQIDBA==");
			Assert.AreEqual(Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }), "AQIDBAU=");
			Assert.AreEqual(Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6 }), "AQIDBAUG");
			Assert.AreEqual(Convert.ToBase64String(new byte[0]), "");
		}

		[Test]
		public void ToBase64StringWithArrayAndFormattingOptionsWorks() {
			var testArr = GetTestArr();
			Assert.AreEqual(Convert.ToBase64String(testArr, Base64FormattingOptions.None), "AAAABAAACAAADAAAEAAAFAAAGAAAHAAAIAAAJAAAKAAALAAAMAAANAAAOAAAPAAAQAAARAAASAAATAAAUAAAVAAAWAAAXAAAYAAAZAAAaAAAbAAAcAAAdAAAeAAAfAAAgAAAhAAAiAAAjAAAkAAAlAAAmAAAnAAAoAAApAAAqAAArAAAsAAAtAAAuAAAvAAAwAAAxAAAyAAAzAAA0AAA1AAA2AAA3AAA4AAA5AAA6AAA7AAA8AAA9AAA+AAA/AAA");
			Assert.AreEqual(Convert.ToBase64String(testArr, Base64FormattingOptions.InsertLineBreaks), "AAAABAAACAAADAAAEAAAFAAAGAAAHAAAIAAAJAAAKAAALAAAMAAANAAAOAAAPAAAQAAARAAASAAA\n" + 
			                                                                                           "TAAAUAAAVAAAWAAAXAAAYAAAZAAAaAAAbAAAcAAAdAAAeAAAfAAAgAAAhAAAiAAAjAAAkAAAlAAA\n" +
			                                                                                           "mAAAnAAAoAAApAAAqAAArAAAsAAAtAAAuAAAvAAAwAAAxAAAyAAAzAAA0AAA1AAA2AAA3AAA4AAA\n" +
			                                                                                           "5AAA6AAA7AAA8AAA9AAA+AAA/AAA");
		}

		[Test]
		public void ToBase64StringWithArrayAndOffsetAndLengthWorks() {
			var arr = GetTestArr();
			Assert.AreEqual(Convert.ToBase64String(arr, 100, 90), "AACIAACMAACQAACUAACYAACcAACgAACkAACoAACsAACwAAC0AAC4AAC8AADAAADEAADIAADMAADQAADUAADYAADcAADgAADkAADoAADsAADwAAD0AAD4AAD8");
		}

		[Test]
		public void ToBase64StringWithArrayAndOffsetAndLengthAndFormattingOptionsWorks() {
			var arr = GetTestArr();
			Assert.AreEqual(Convert.ToBase64String(arr, 100, 90, Base64FormattingOptions.None), "AACIAACMAACQAACUAACYAACcAACgAACkAACoAACsAACwAAC0AAC4AAC8AADAAADEAADIAADMAADQAADUAADYAADcAADgAADkAADoAADsAADwAAD0AAD4AAD8");
			Assert.AreEqual(Convert.ToBase64String(arr, 100, 90, Base64FormattingOptions.InsertLineBreaks), "AACIAACMAACQAACUAACYAACcAACgAACkAACoAACsAACwAAC0AAC4AAC8AADAAADEAADIAADMAADQ\n" +
			                                                                                                "AADUAADYAADcAADgAADkAADoAADsAADwAAD0AAD4AAD8");
			Assert.AreEqual(Convert.ToBase64String(arr, 70, 114, Base64FormattingOptions.InsertLineBreaks), "AABgAABkAABoAABsAABwAAB0AAB4AAB8AACAAACEAACIAACMAACQAACUAACYAACcAACgAACkAACo\n" +
			                                                                                                "AACsAACwAAC0AAC4AAC8AADAAADEAADIAADMAADQAADUAADYAADcAADgAADkAADoAADsAADwAAD0");
		}

		[Test]
		public void FromBase64StringWorks() {
			Assert.AreEqual(Convert.FromBase64String("AAAABAAACAAADAAAEAAAFAAAGAAAHAAAIAAAJAAAKAAALAAAMAAANAAAOAAAPAAAQAAARAAASAAATAAAUAAAVAAAWAAAXAAAYAAAZAAAaAAAbAAAcAAAdAAAeAAAfAAAgAAAhAAAiAAAjAAAkAAAlAAAmAAAnAAAoAAApAAAqAAArAAAsAAAtAAAuAAAvAAAwAAAxAAAyAAAzAAA0AAA1AAA2AAA3AAA4AAA5AAA6AAA7AAA8AAA9AAA+AAA/AAA"), GetTestArr());
			Assert.AreEqual(Convert.FromBase64String("AQID"), new byte[] { 1, 2, 3 });
			Assert.AreEqual(Convert.FromBase64String("AQIDBA=="), new byte[] { 1, 2, 3, 4 });
			Assert.AreEqual(Convert.FromBase64String("AQIDBAU="), new byte[] { 1, 2, 3, 4, 5 });
			Assert.AreEqual(Convert.FromBase64String("AQIDBAUG"), new byte[] { 1, 2, 3, 4, 5, 6 });
			Assert.AreEqual(Convert.FromBase64String("AQIDBAU="), new byte[] { 1, 2, 3, 4, 5 });
			Assert.AreEqual(Convert.FromBase64String("A Q\nI\tD"), new byte[] { 1, 2, 3 });
			Assert.AreEqual(Convert.FromBase64String(""), new byte[0]);
		}
	}
}
