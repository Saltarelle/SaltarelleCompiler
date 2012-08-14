using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace RuntimeLibrary.Tests.Core {
	public abstract class CoreLibTestBase : TestBase {
        private static readonly Lazy<string> _testsScript = new Lazy<string>(() => File.ReadAllText(@"..\..\CoreLibTests\bin\CoreLibTests.js"));
		internal static string TestsScript { get { return _testsScript.Value; } }

		protected override IEnumerable<string> ScriptSources {
			get { return new[] { TestsScript }; }
		}

		protected override string TestClassName {
			get { return "CoreLibTests." + GetType().Name; }
		}
	}

	[TestFixture]
	public class DateTimeTests : CoreLibTestBase {
	}

	[TestFixture]
	public class MutableDateTimeTests : CoreLibTestBase {
	}
}
