using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

	// System

	[TestFixture]
	public class ActivatorTests : CoreLibTestBase {}

	[TestFixture]
	public class ArgumentsTests : CoreLibTestBase {}

	[TestFixture]
	public class ArrayTests : CoreLibTestBase {}

	[TestFixture]
	public class AsyncTests : CoreLibTestBase {}

	[TestFixture]
	public class BooleanTests : CoreLibTestBase {}

	[TestFixture]
	public class ByteTests : CoreLibTestBase {}

	[TestFixture]
	public class CharTests : CoreLibTestBase {}

	[TestFixture]
	public class DateTimeTests : CoreLibTestBase {}

	[TestFixture]
	public class DecimalTests : CoreLibTestBase {}

	[TestFixture]
	public class DelegateTests : CoreLibTestBase {}

	[TestFixture]
	public class DoubleTests : CoreLibTestBase {}

	[TestFixture]
	public class EnumTests : CoreLibTestBase {}

	[TestFixture]
	public class EqualityComparerTests : CoreLibTestBase {}

	[TestFixture]
	public class IComparableTests : CoreLibTestBase {}

	[TestFixture]
	public class IEquatableTests : CoreLibTestBase {}

	[TestFixture]
	public class Int16Tests : CoreLibTestBase {}

	[TestFixture]
	public class Int32Tests : CoreLibTestBase {}

	[TestFixture]
	public class Int64Tests : CoreLibTestBase {}

	[TestFixture]
	public class IteratorBlockTests : CoreLibTestBase {}

	[TestFixture]
	public class JsDateTests : CoreLibTestBase {}

	[TestFixture]
	public class LazyTests : CoreLibTestBase {}

	[TestFixture]
	public class MathTests : CoreLibTestBase {}

	[TestFixture]
	public class MultidimArrayTests : CoreLibTestBase {}

	[TestFixture]
	public class NullableTests : CoreLibTestBase {}

	[TestFixture]
	public class ObjectTests : CoreLibTestBase {}

	[TestFixture]
	public class PromiseTests : CoreLibTestBase {
        private static readonly Lazy<string> _simplePromiseScript = new Lazy<string>(() => File.ReadAllText(@"SimplePromise.js"));
		internal static string SimplePromiseScript { get { return _simplePromiseScript.Value; } }

		protected override IEnumerable<string> ScriptSources {
			get {
				return base.ScriptSources.Concat(new[] { SimplePromiseScript });
			}
		}
	}

	[TestFixture]
	public class SByteTests : CoreLibTestBase {}

	[TestFixture]
	public class ScriptTests : CoreLibTestBase {}

	[TestFixture]
	public class SingleTests : CoreLibTestBase {}

	[TestFixture]
	public class StringTests : CoreLibTestBase {}

	[TestFixture]
	public class TaskTests : CoreLibTestBase {}

	[TestFixture]
	public class TupleTests : CoreLibTestBase {}

	[TestFixture]
	public class UInt16Tests : CoreLibTestBase {}

	[TestFixture]
	public class UInt32Tests : CoreLibTestBase {}

	[TestFixture]
	public class UInt64Tests : CoreLibTestBase {}

	// System.Collections

	[TestFixture]
	public class JsDictionaryTests : CoreLibTestBase {}

	// System.Collections.Generic

	[TestFixture]
	public class ListTests : CoreLibTestBase {}

	[TestFixture]
	public class GenericDictionaryTests : CoreLibTestBase {}

	[TestFixture]
	public class GenericJsDictionaryTests : CoreLibTestBase {}

	[TestFixture]
	public class StackTests : CoreLibTestBase {}

	[TestFixture]
	public class QueueTests : CoreLibTestBase {}

	// System.Serialization

	[TestFixture]
	public class JsonTests : CoreLibTestBase {}

	// System.Text

	[TestFixture]
	public class RegexTests : CoreLibTestBase {}

	[TestFixture]
	public class StringBuilderTests : CoreLibTestBase {}
}
