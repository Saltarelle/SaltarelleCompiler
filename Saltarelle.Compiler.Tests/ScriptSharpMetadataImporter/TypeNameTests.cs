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
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

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
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

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
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

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
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

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
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

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
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

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
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

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
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

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
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

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
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

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

		[Test]
		public void TypeNamesAreMinimizedForNonPublicTypesIfTheMinimizeFlagIsSet() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var result = Process(md,
@"class C1 {}
internal class C2 {}
public class C3 {}
public class C4 { internal class C5 { public class C6 {} } }
internal class C7 { public class C8 { public class C9 {} } }
public class C10 { private class C11 {} protected class C12 {} protected internal class C13 {} }
");

			var names = new[] { "C1", "C2", "C3", "C4", "C4+C5", "C4+C5+C6", "C7", "C7+C8", "C7+C8+C9", "C10+C11", "C10+C12", "C10+C13" }.ToDictionary(s => s, s => md.GetTypeName(result[s]));

			Assert.That(names["C1"], Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(names["C2"], Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(names["C3"], Is.EqualTo("C3"));
			Assert.That(names["C4"], Is.EqualTo("C4"));
			Assert.That(names["C4+C5"], Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(names["C4+C5+C6"], Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(names["C7"], Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(names["C7+C8"], Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(names["C7+C8+C9"], Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(names["C10+C11"], Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(names["C10+C12"], Is.EqualTo("C10$C12"));
			Assert.That(names["C10+C13"], Is.EqualTo("C10$C13"));
		}

		[Test]
		public void MinimizedTypeNamesAreUniquePerNamespace() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var result = Process(md,
@"class C1 {}
class C2 { class C3 {} }

namespace X {
	class C4 {}
	class C5 { class C6 {} }
}

namespace X.Y {
	class C7 {}
	class C8 { class C9 {} }
}");

			Assert.That(new[] { "C1", "C2", "C2+C3" }.Select(s => md.GetTypeName(result[s])).ToList(), Is.EquivalentTo(new[] { "$0", "$1", "$2" }));
			Assert.That(new[] { "X.C4", "X.C5", "X.C5+C6" }.Select(s => md.GetTypeName(result[s])).ToList(), Is.EquivalentTo(new[] { "X.$0", "X.$1", "X.$2" }));
			Assert.That(new[] { "X.Y.C7", "X.Y.C8", "X.Y.C8+C9" }.Select(s => md.GetTypeName(result[s])).ToList(), Is.EquivalentTo(new[] { "X.Y.$0", "X.Y.$1", "X.Y.$2" }));
		}

		[Test]
		public void ScriptNameAttributePreventsMinimizationOfTypeNames() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var result = Process(md,
@"using System.Runtime.CompilerServices;
[ScriptName(""Renamed1"")] class C1 {}
namespace X {
	[ScriptName(""Renamed2"")]
	class C2 {
		[ScriptName(""Renamed3"")]
		class C3 {}
	}
	class C4 {
		[ScriptName(""Renamed5"")]
		class C5 {
		}
	}
}");

			Assert.That(md.GetTypeName(result["C1"]), Is.EqualTo("Renamed1"));
			Assert.That(md.GetTypeName(result["X.C2"]), Is.EqualTo("X.Renamed2"));
			Assert.That(md.GetTypeName(result["X.C2+C3"]), Is.EqualTo("X.Renamed3"));
			Assert.That(md.GetTypeName(result["X.C4"]), Is.EqualTo("X.$0"));
			Assert.That(md.GetTypeName(result["X.C4+C5"]), Is.EqualTo("X.Renamed5"));
		}

		[Test]
		public void TypeNamesAreNotMinimizedForNonPublicTypesIfTheMinimizeFlagNotIsSet() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var result = Process(md, "internal class C1 {}");

			Assert.That(md.GetTypeName(result["C1"]), Is.EqualTo("C1"));
		}

		[Test]
		public void ScriptNamespaceAttributeCanBeUsedToChangeNamespaceOfTypes() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);

			var result = Process(md,
@"using System.Runtime.CompilerServices;
[ScriptNamespace(""Some.Namespace"")] class C1 {}
namespace X {
	[ScriptNamespace(""OtherNamespace"")]
	class C2 {}
}");

			Assert.That(md.GetTypeName(result["C1"]), Is.EqualTo("Some.Namespace.C1"));
			Assert.That(md.GetTypeName(result["X.C2"]), Is.EqualTo("OtherNamespace.C2"));
		}

		[Test]
		public void EmptyScriptNamespaceAttributeWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var result = Process(md,
@"using System.Runtime.CompilerServices;
[ScriptNamespace("""")] public class C1 {}
namespace X {
	[ScriptNamespace("""")]
	public class C2 {}
}");

			Assert.That(md.GetTypeName(result["C1"]), Is.EqualTo("C1"));
			Assert.That(md.GetTypeName(result["X.C2"]), Is.EqualTo("C2"));
		}

		[Test]
		public void IgnoreNamespaceAttributeWorks() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var result = Process(md,
@"using System.Runtime.CompilerServices;
[IgnoreNamespace] public class C1 {}
namespace X {
	[IgnoreNamespace]
	public class C2 {}
}");

			Assert.That(md.GetTypeName(result["C1"]), Is.EqualTo("C1"));
			Assert.That(md.GetTypeName(result["X.C2"]), Is.EqualTo("C2"));
		}

		[Test]
		public void ScriptNamespaceAttributeCannotBeAppliedToNestedTypes() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
namespace X {
	public class C1 {
		[ScriptNamespace(""X"")]
		public class C2 {}
	}
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("nested type") && er.AllMessages[0].Contains("X.C1.C2") && er.AllMessages[0].Contains("ScriptNamespace"));
		}

		[Test]
		public void IgnoreNamespaceAttributeCannotBeAppliedToNestedTypes() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
namespace X {
	public class C1 {
		[IgnoreNamespace]
		public class C2 {}
	}
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("nested type") && er.AllMessages[0].Contains("X.C1.C2") && er.AllMessages[0].Contains("IgnoreNamespace"));
		}

		[Test]
		public void CannotApplyBothIgnoreNamespaceAndScriptNamespaceToTheSameClass() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			var er = new MockErrorReporter(false);

			Process(md,
@"using System.Runtime.CompilerServices;
namespace X {
	[IgnoreNamespace, ScriptNamespace(""X"")]
	public class C1 {
		public class C2 {}
	}
}", er);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("X.C1") && er.AllMessages[0].Contains("IgnoreNamespace") && er.AllMessages[0].Contains("ScriptNamespace"));
		}

		[Test]
		public void ScriptNameAttributeOnTypeMustBeAValidJSIdentifier() {
			var er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; [ScriptName("""")] public class C1 {}", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1") && er.AllMessages[0].Contains("ScriptName") && er.AllMessages[0].Contains("must be a valid JavaScript identifier"));

			er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; [ScriptName(""X.Y"")] public class C1 {}", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1") && er.AllMessages[0].Contains("ScriptName") && er.AllMessages[0].Contains("must be a valid JavaScript identifier"));

			er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; [ScriptName(""a b"")] public class C1 {}", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1") && er.AllMessages[0].Contains("ScriptName") && er.AllMessages[0].Contains("must be a valid JavaScript identifier"));
		}

		[Test]
		public void ScriptNamespaceAttributeArgumentMustBeAValidJSQualifiedIdentifierOrBeEmpty() {
			var er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; [ScriptNamespace(""a b"")] public class C1 {}", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1") && er.AllMessages[0].Contains("ScriptNamespace") && er.AllMessages[0].Contains("must be a valid JavaScript qualified identifier"));

			er = new MockErrorReporter(false);
			Process(new MetadataImporter.ScriptSharpMetadataImporter(true), @"using System.Runtime.CompilerServices; [ScriptNamespace("" "")] public class C1 {}", er);
			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages[0].Contains("C1") && er.AllMessages[0].Contains("ScriptNamespace") && er.AllMessages[0].Contains("must be a valid JavaScript qualified identifier"));
		}

		[Test]
		public void ScriptNamespaceAndIgnoreNamespaceAttributesAreConsideredWhenMinimizingNames() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var result = Process(md,
@"using System.Runtime.CompilerServices;

class C1 {}
[ScriptNamespace(""X"")] class C2 {}
[ScriptNamespace(""X.Y"")] class C3 {}

namespace X {
	[ScriptNamespace("""")] class C4 {}
	class C5 {}
	[ScriptNamespace(""X.Y"")] class C6 {}
}

namespace X.Y {
	[IgnoreNamespace] class C7 {}
	class C8 {}
	[ScriptNamespace(""X"")] class C9 {}
}");

			Assert.That(new[] { "C1", "X.C4", "X.Y.C7" }.Select(s => md.GetTypeName(result[s])).ToList(), Is.EquivalentTo(new[] { "$0", "$1", "$2" }));
			Assert.That(new[] { "C2", "X.C5", "X.Y.C9" }.Select(s => md.GetTypeName(result[s])).ToList(), Is.EquivalentTo(new[] { "X.$0", "X.$1", "X.$2" }));
			Assert.That(new[] { "C3", "X.C6", "X.Y.C8" }.Select(s => md.GetTypeName(result[s])).ToList(), Is.EquivalentTo(new[] { "X.Y.$0", "X.Y.$1", "X.Y.$2" }));
		}

		[Test]
		public void PreserveNameAttributePreventsMinimization() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);

			var result = Process(md,
@"using System.Runtime.CompilerServices;
[PreserveName] class C1 {}
[PreserveName] internal class C2 {}
[PreserveName] public class C3 {}
[PreserveName] public class C4 { [PreserveName] internal class C5 { [PreserveName] public class C6 {} } }
[PreserveName] internal class C7 { [PreserveName] public class C8 { [PreserveName] public class C9 {} } }
[PreserveName] public class C10 { [PreserveName] private class C11 {} [PreserveName] protected class C12 {} [PreserveName] protected internal class C13 {} }
");

			var names = new[] { "C1", "C2", "C3", "C4", "C4+C5", "C4+C5+C6", "C7", "C7+C8", "C7+C8+C9", "C10+C11", "C10+C12", "C10+C13" }.ToDictionary(s => s, s => md.GetTypeName(result[s]));

			Assert.That(md.GetTypeName(result["C1"]), Is.EqualTo("C1"));
			Assert.That(md.GetTypeName(result["C2"]), Is.EqualTo("C2"));
			Assert.That(md.GetTypeName(result["C3"]), Is.EqualTo("C3"));
			Assert.That(md.GetTypeName(result["C4"]), Is.EqualTo("C4"));
			Assert.That(md.GetTypeName(result["C4+C5"]), Is.EqualTo("C4$C5"));
			Assert.That(md.GetTypeName(result["C4+C5+C6"]), Is.EqualTo("C4$C5$C6"));
			Assert.That(md.GetTypeName(result["C7"]), Is.EqualTo("C7"));
			Assert.That(md.GetTypeName(result["C7+C8"]), Is.EqualTo("C7$C8"));
			Assert.That(md.GetTypeName(result["C7+C8+C9"]), Is.EqualTo("C7$C8$C9"));
			Assert.That(md.GetTypeName(result["C10+C11"]), Is.EqualTo("C10$C11"));
			Assert.That(md.GetTypeName(result["C10+C12"]), Is.EqualTo("C10$C12"));
			Assert.That(md.GetTypeName(result["C10+C13"]), Is.EqualTo("C10$C13"));
		}
	}
}
