using System;
using System.Runtime.CompilerServices;

namespace QUnit {
    /// <summary>
    /// This attribute indicates that a class is a QUnit test fixture, and can contain test methods (methods decorated with a <see cref="TestAttribute"/> or <see cref="AsyncTestAttribute"/>.
    /// </summary>
    #if !PLUGIN
	[Imported]
    [NonScriptable]
	#endif
	[AttributeUsage(AttributeTargets.Class)]
    public sealed class TestFixtureAttribute : Attribute {
    }

	/// <summary>
	/// This attribute specifies that a method is a QUnit test. This means that instead of a normal method, a QUnit.test() call will be generated in the (generated) runTests method of the declaring class.
	/// </summary>
    #if !PLUGIN
	[Imported]
    [NonScriptable]
	#endif
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class TestAttribute : Attribute {
		public string Description { get; private set; }
		public string Category { get; set; }
		public int ExpectedAssertionCount { get; set; }

		public TestAttribute() {
			ExpectedAssertionCount = -1;
		}

		public TestAttribute(string description) {
			Description = description;
			ExpectedAssertionCount = -1;
		}
	}

	/// <summary>
	/// This attribute specifies that a method is a QUnit test. This means that instead of a normal method, a QUnit.asyncTest() call will be generated in the (generated) runTests method of the declaring class.
	/// </summary>
    #if !PLUGIN
	[Imported]
    [NonScriptable]
	#endif
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class AsyncTestAttribute : Attribute {
		public string Description { get; private set; }
		public string Category { get; set; }
		public int ExpectedAssertionCount { get; set; }

		public AsyncTestAttribute() {
			ExpectedAssertionCount = -1;
		}

		public AsyncTestAttribute(string description) {
			Description = description;
			ExpectedAssertionCount = -1;
		}
	}
}
