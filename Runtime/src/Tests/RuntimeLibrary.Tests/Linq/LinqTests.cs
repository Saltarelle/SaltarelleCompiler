using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace RuntimeLibrary.Tests.Linq {
#warning TODO: Move to separate test project
	[TestFixture]
	public class LinqTests : TestBase {
		private static readonly Lazy<string> _linqJSScript = new Lazy<string>(() => File.ReadAllText(@"..\..\..\..\bin\Script\linq.js"));
		internal static string LinqJSScript { get { return _linqJSScript.Value; } }

		private static readonly Lazy<string> _testsScript = new Lazy<string>(() => File.ReadAllText(@"..\..\LinqJSTests\bin\LinqJSTests.js"));
		internal static string TestsScript { get { return _testsScript.Value; } }

		protected override IEnumerable<string> ScriptSources {
			get { return new[] { LinqJSScript, TestsScript }; }
		}

		protected override string TestClassName {
			get { return "LinqJSTests.Tests"; }
		}
	}
}
