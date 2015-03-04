using System;
using QUnit;

namespace CoreLib.TestScript
{
	[TestFixture]
	public class ConsoleTests
	{
		// Without mocking console (which would prevent catching some potential 
		// errors in the polyfills etc.), we can only check here that the function 
		// calls don't trigger errors when the Javascript they produce runs
		[Test]
		public void CanCallLogWithAnObject()
		{
			Console.Log(new { a = "abc", b = 7.5 });
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallLogWithMultipleObjects()
		{
			Console.Log(new { a = "abc", b = 7.5 }, DateTime.Now);
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallInfoWithAnObject()
		{
			Console.Info(new { a = "abc", b = 7.5 });
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallInfoWithMultipleObjects()
		{
			Console.Info(new { a = "abc", b = 7.5 }, 99);
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallWarnWithAnObject()
		{
			Console.Warn(new { a = "abc", b = 7.5 });
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallWarnWithMultipleObjects()
		{
			Console.Warn(new { a = "abc", b = 7.5 }, TimeSpan.Zero);
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallErrorWithAnObject()
		{
			Console.Error(new { a = "abc", b = 7.5 });
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallErrorWithMultipleObjects()
		{
			Console.Error(new { a = "abc", b = 7.5 }, TimeSpan.Zero);
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallDirWithAnObject()
		{
			Console.Dir(new { a = "abc", b = 7.5 });
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallGroup()
		{
			Console.Group();
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallGroupCollapsed()
		{
			Console.GroupCollapsed();
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallGroupEnd()
		{
			Console.GroupEnd();
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallCountWithoutALabel()
		{
			Console.Count();
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallCountWithALabel()
		{
			Console.Count("label");
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallTimeWithALabel()
		{
			Console.Time("label");
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallTimeEndWithALabel()
		{
			Console.TimeEnd("label");
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallTrace()
		{
			Console.Trace();
			Assert.OK(true, "Function call did not trigger an exception");
		}
	}
}