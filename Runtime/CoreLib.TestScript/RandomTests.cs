using System;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class RandomTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			var rand = new Random();
			Assert.AreEqual(typeof(Random).FullName, "ss.Random");
			Assert.IsTrue(typeof(Random).IsClass);
			Assert.IsTrue(rand is Random);
		}

#pragma warning disable 219
		[Test(ExpectedAssertionCount = 0)]
		public void DefaultConstructorWorks() {
			var rand = new Random();
		}

		[Test(ExpectedAssertionCount = 0)]
		public void SeedConstructorWorks() {
			var rand = new Random(854);
		}
#pragma warning restore 219

		[Test]
		public void NextWorks() {
			var rand = new Random();
			for (var i = 0; i < 10; i++) {
				int randomNumber = rand.Next();
				Assert.IsTrue(randomNumber >= 0, randomNumber + " is greater or equal to 0");
			}
		}

		[Test]
		public void NextWithMaxWorks() {
			var rand = new Random();
			for (var i = 0; i < 10; i++) {
				int randomNumber = rand.Next(5);
				Assert.IsTrue(randomNumber >= 0, randomNumber + " is greater or equal to 0");
				Assert.IsTrue(randomNumber < 5, randomNumber + " is smaller than 5");
			}
		}

		[Test]
		public void NextWithMinAndMaxWorks() {
			var rand = new Random();
			for (var i = 0; i < 10; i++) {
				int randomNumber = rand.Next(5, 10);
				Assert.IsTrue(randomNumber >= 5, randomNumber + " is greater or equal to 5");
				Assert.IsTrue(randomNumber < 10, randomNumber + " is smaller than 10");
			}
		}

		[Test]
		public void NextDoubleWorks() {
			var rand = new Random();
			for (var i = 0; i < 10; i++) {
				double randomNumber = rand.NextDouble();
				Assert.IsTrue(randomNumber >= 0.0, randomNumber + " is greater or equal to 0.0");
				Assert.IsTrue(randomNumber < 1.0, randomNumber + " is smaller than 1.0");
			}
		}

		[Test]
		public void NextBytesWorks() {
			var rand = new Random();
			var bytes = new byte[150];
			rand.NextBytes(bytes);
			for (var i = 0; i < bytes.Length; i++) {
				Assert.IsTrue(bytes[i] >= byte.MinValue, bytes[i] + " is greater or equal to " + byte.MinValue);
				Assert.IsTrue(bytes[i] < byte.MaxValue, bytes[i] + " is smaller than " + byte.MaxValue);
			}
		}
	}
}
