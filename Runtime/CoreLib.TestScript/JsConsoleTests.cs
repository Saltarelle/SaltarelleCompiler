using System;
using QUnit;

namespace CoreLib.TestScript
{
	[TestFixture]
	public class JsConsoleTests
	{
		// Without mocking console (which would prevent catching some potential 
		// errors in the polyfills etc.), we can only check here that the function 
		// calls don't trigger errors when the Javascript they produce runs
		[Test]
		public void CanCallLogWithAnObject()
		{
			JsConsole.Log(new { a = "abc", b = 7.5 });
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallLogWithMultipleObjects()
		{
			JsConsole.Log(new { a = "abc", b = 7.5 }, DateTime.Now);
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallInfoWithAnObject()
		{
			JsConsole.Info(new { a = "abc", b = 7.5 });
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallInfoWithMultipleObjects()
		{
			JsConsole.Info(new { a = "abc", b = 7.5 }, 99);
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallWarnWithAnObject()
		{
			JsConsole.Warn(new { a = "abc", b = 7.5 });
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallWarnWithMultipleObjects()
		{
			JsConsole.Warn(new { a = "abc", b = 7.5 }, TimeSpan.Zero);
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallErrorWithAnObject()
		{
			JsConsole.Error(new { a = "abc", b = 7.5 });
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallErrorWithMultipleObjects()
		{
			JsConsole.Error(new { a = "abc", b = 7.5 }, TimeSpan.Zero);
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallDirWithAnObject()
		{
			JsConsole.Dir(new { a = "abc", b = 7.5 });
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallGroup()
		{
			JsConsole.Group();
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallGroupCollapsed()
		{
			JsConsole.GroupCollapsed();
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallGroupEnd()
		{
			JsConsole.GroupEnd();
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallCountWithoutALabel()
		{
			JsConsole.Count();
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallCountWithALabel()
		{
			JsConsole.Count("label");
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallTimeWithALabel()
		{
			JsConsole.Time("label");
			Assert.OK(true, "Function call did not trigger an exception");
		}

		[Test]
		public void CanCallTimeEndWithALabel()
		{
			JsConsole.TimeEnd("label");
			Assert.OK(true, "Function call did not trigger an exception");
		}
	}
}