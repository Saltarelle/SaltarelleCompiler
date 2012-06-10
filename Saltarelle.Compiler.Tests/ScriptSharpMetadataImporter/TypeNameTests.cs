using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class TypeNameTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void TopLevelClassWithoutAttributesWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter();

			var result = Process(md,
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	public class SomeType {
	}
}");
			var type = result["TestNamespace.SomeType"];
			Assert.That(md.GetTypeName(type), Is.EqualTo("TestNamespace.SomeType"));
		}

		[Test]
		public void NestedClassWithoutAttributesWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter();

			var result = Process(md,
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	public class Outer {
		public class SomeType {
		}
	}
}");
			var type = result["TestNamespace.Outer+SomeType"];
			Assert.That(md.GetTypeName(type), Is.EqualTo("TestNamespace.Outer$SomeType"));
		}

		[Test]
		public void MultipleNestingWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter();

			var result = Process(md,
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	public class Outer {
		public class Inner {
			public class SomeType {
			}
		}
	}
}");
			var type = result["TestNamespace.Outer+Inner+SomeType"];
			Assert.That(md.GetTypeName(type), Is.EqualTo("TestNamespace.Outer$Inner$SomeType"));
		}

		[Test]
		public void ScriptNameAttributeCanChangeTheNameOfATopLevelClass() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter();

			var result = Process(md,
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	[ScriptName(""Renamed"")]
	public class SomeType {
	}
}");
			var type = result["TestNamespace.SomeType"];
			Assert.That(md.GetTypeName(type), Is.EqualTo("TestNamespace.Renamed"));
		}

		[Test]
		public void ScriptNameAttributeCanChangeTheNameOfANestedClass() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter();

			var result = Process(md,
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	[ScriptName(""RenamedOuter"")]
	public class Outer {
		[ScriptName(""Renamed"")]
		public class SomeType {
		}
	}
}");
			var type = result["TestNamespace.Outer+SomeType"];
			Assert.That(md.GetTypeName(type), Is.EqualTo("TestNamespace.Renamed"));
		}

		[Test]
		public void ClassOutsideNamespaceWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter();

			var result = Process(md,
@"using System.Runtime.CompilerServices;

public class SomeType {
}
");
			var type = result["SomeType"];
			Assert.That(md.GetTypeName(type), Is.EqualTo("SomeType"));
		}

		[Test]
		public void ClassOutsideNamespaceWithScriptNameAttributeWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter();

			var result = Process(md,
@"using System.Runtime.CompilerServices;

[ScriptName(""Renamed"")]
public class SomeType {
}
");
			var type = result["SomeType"];
			Assert.That(md.GetTypeName(type), Is.EqualTo("Renamed"));
		}

		[Test]
		public void GenericTypeWithoutScriptNameAttributeWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter();

			var result = Process(md,
@"using System.Runtime.CompilerServices;

public class SomeType<T1, T2> {
}
");
			var type = result["SomeType`2"];
			Assert.That(md.GetTypeName(type), Is.EqualTo("SomeType$2"));
		}

		[Test]
		public void GenericTypeWithScriptNameAttributeWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter();

			var result = Process(md,
@"using System.Runtime.CompilerServices;

[ScriptName(""Renamed"")]
public class SomeType<T1, T2> {
}
");
			var type = result["SomeType`2"];
			Assert.That(md.GetTypeName(type), Is.EqualTo("Renamed"));
		}

		[Test]
		public void MultipleGenericNestedNamesAreCorrect() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter();

			var result = Process(md,
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	public class Outer<T1,T2> {
		public class Inner<T3> {
			public class SomeType<T4,T5> {
			}
		}
	}
}");
			var type = result["TestNamespace.Outer`2+Inner`1+SomeType`2"];
			Assert.That(md.GetTypeName(type), Is.EqualTo("TestNamespace.Outer$2$Inner$1$SomeType$2"));
		}
	}
}
