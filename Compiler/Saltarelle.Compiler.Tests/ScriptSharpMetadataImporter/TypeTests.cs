using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class TypeTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void TopLevelClassWithoutAttributesWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	public class SomeType {
	}
}");
			var type = FindType("TestNamespace.SomeType");
			Assert.That(type.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(type.Name, Is.EqualTo("TestNamespace.SomeType"));
		}

		[Test]
		public void NestedClassWithoutAttributesWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	public class Outer {
		public class SomeType {
		}
	}
}");
			var type = FindType("TestNamespace.Outer+SomeType");
			Assert.That(type.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(type.Name, Is.EqualTo("TestNamespace.Outer$SomeType"));
		}

		[Test]
		public void EnumWithoutAttributesWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	public enum SomeEnum {
	}
}");
			var type = FindType("TestNamespace.SomeEnum");
			Assert.That(type.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(type.Name, Is.EqualTo("TestNamespace.SomeEnum"));
			Assert.That(type.GenerateCode, Is.True);
		}

		[Test]
		public void ImportedAttributeWorksOnEnum() {
			Prepare(
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	[Imported]
	public enum SomeEnum {
	}
}");
			var type = FindType("TestNamespace.SomeEnum");
			Assert.That(type.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(type.Name, Is.EqualTo("TestNamespace.SomeEnum"));
			Assert.That(type.GenerateCode, Is.False);
		}

		[Test]
		public void MultipleNestingWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	public class Outer {
		public class Inner {
			public class SomeType {
			}
		}
	}
}");
			var type = FindType("TestNamespace.Outer+Inner+SomeType");
			Assert.That(type.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(type.Name, Is.EqualTo("TestNamespace.Outer$Inner$SomeType"));
		}

		[Test]
		public void ScriptNameAttributeCanChangeTheNameOfATopLevelClass() {
			Prepare(
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	[ScriptName(""Renamed"")]
	public class SomeType {
	}
}");

			var type = FindType("TestNamespace.SomeType");
			Assert.That(type.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(type.Name, Is.EqualTo("TestNamespace.Renamed"));
		}

		[Test]
		public void ScriptNameAttributeCanChangeTheNameOfANestedClass() {
			Prepare(
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	[ScriptName(""RenamedOuter"")]
	public class Outer {
		[ScriptName(""Renamed"")]
		public class SomeType {
		}
	}
}");
			
			var type = FindType("TestNamespace.Outer+SomeType");
			Assert.That(type.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(type.Name, Is.EqualTo("TestNamespace.Renamed"));
		}

		[Test]
		public void ClassOutsideNamespaceWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class SomeType {
}
");

			var type = FindType("SomeType");
			Assert.That(type.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(type.Name, Is.EqualTo("SomeType"));
		}

		[Test]
		public void ClassOutsideNamespaceWithScriptNameAttributeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

[ScriptName(""Renamed"")]
public class SomeType {
}
");

			var type = FindType("SomeType");
			Assert.That(type.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(type.Name, Is.EqualTo("Renamed"));
		}

		[Test]
		public void GenericTypeWithoutScriptNameAttributeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class SomeType<T1, T2> {
}
");

			var type = FindType("SomeType`2");
			Assert.That(type.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(type.Name, Is.EqualTo("SomeType$2"));
			Assert.That(type.IgnoreGenericArguments, Is.False);
		}

		[Test]
		public void GenericTypeWithScriptNameAttributeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

[ScriptName(""Renamed"")]
public class SomeType<T1, T2> {
}
");

			var type = FindType("SomeType`2");
			Assert.That(type.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(type.Name, Is.EqualTo("Renamed"));
			Assert.That(type.IgnoreGenericArguments, Is.False);
		}

		[Test]
		public void MultipleGenericNestedNamesAreCorrect() {
			Prepare(
@"using System.Runtime.CompilerServices;

namespace TestNamespace {
	public class Outer<T1,T2> {
		public class Inner<T3> {
			public class SomeType<T4,T5> {
			}
		}
	}
}");

			var type = FindType("TestNamespace.Outer`2+Inner`1+SomeType`2");
			Assert.That(type.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(type.Name, Is.EqualTo("TestNamespace.Outer$2$Inner$1$SomeType$2"));
			Assert.That(type.IgnoreGenericArguments, Is.False);
		}

		[Test]
		public void TypeNamesAreMinimizedForNonPublicTypesIfTheMinimizeFlagIsSet() {
			Prepare(
@"class C1 {}
internal class C2 {}
public class C3 {}
public class C4 { internal class C5 { public class C6 {} } }
internal class C7 { public class C8 { public class C9 {} } }
public class C10 { private class C11 {} protected class C12 {} protected internal class C13 {} }
");

			Assert.That(FindType("C1").Name, Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(FindType("C2").Name, Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(FindType("C3").Name, Is.EqualTo("C3"));
			Assert.That(FindType("C4").Name, Is.EqualTo("C4"));
			Assert.That(FindType("C4+C5").Name, Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(FindType("C4+C5+C6").Name, Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(FindType("C7").Name, Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(FindType("C7+C8").Name, Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(FindType("C7+C8+C9").Name, Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(FindType("C10+C11").Name, Is.StringMatching("^\\$[0-9]+$"));
			Assert.That(FindType("C10+C12").Name, Is.EqualTo("C10$C12"));
			Assert.That(FindType("C10+C13").Name, Is.EqualTo("C10$C13"));
		}

		[Test]
		public void MinimizedTypeNamesAreUniquePerNamespace() {
			Prepare(
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

			Assert.That(new[] { "C1", "C2", "C2+C3" }.Select(s => FindType(s).Name).ToList(), Is.EquivalentTo(new[] { "$0", "$1", "$2" }));
			Assert.That(new[] { "X.C4", "X.C5", "X.C5+C6" }.Select(s => FindType(s).Name).ToList(), Is.EquivalentTo(new[] { "X.$0", "X.$1", "X.$2" }));
			Assert.That(new[] { "X.Y.C7", "X.Y.C8", "X.Y.C8+C9" }.Select(s => FindType(s).Name).ToList(), Is.EquivalentTo(new[] { "X.Y.$0", "X.Y.$1", "X.Y.$2" }));
		}

		[Test]
		public void ScriptNameAttributePreventsMinimizationOfTypeNames() {
			Prepare(
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

			Assert.That(FindType("C1").Name, Is.EqualTo("Renamed1"));
			Assert.That(FindType("X.C2").Name, Is.EqualTo("X.Renamed2"));
			Assert.That(FindType("X.C2+C3").Name, Is.EqualTo("X.Renamed3"));
			Assert.That(FindType("X.C4").Name, Is.EqualTo("X.$0"));
			Assert.That(FindType("X.C4+C5").Name, Is.EqualTo("X.Renamed5"));
		}

		[Test]
		public void InternalTypesWithoutPreserveNameAttributeArePrefixedWithADollarSignIfTheMinimizeFlagIsNotSet() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {}
internal class C2 {}
public class C3 {}
public class C4 { internal class C5 { public class C6 {} } }
internal class C7 { public class C8 { public class C9 {} } }
public class C10 { private class C11 {} protected class C12 {} protected internal class C13 {} }
[PreserveName] internal class C14 {}
", false);

			Assert.That(FindType("C1").Name, Is.EqualTo("$C1"));
			Assert.That(FindType("C2").Name, Is.EqualTo("$C2"));
			Assert.That(FindType("C3").Name, Is.EqualTo("C3"));
			Assert.That(FindType("C4").Name, Is.EqualTo("C4"));
			Assert.That(FindType("C4+C5").Name, Is.EqualTo("$C4$C5"));
			Assert.That(FindType("C4+C5+C6").Name, Is.EqualTo("$C4$C5$C6"));
			Assert.That(FindType("C7").Name, Is.EqualTo("$C7"));
			Assert.That(FindType("C7+C8").Name, Is.EqualTo("$C7$C8"));
			Assert.That(FindType("C7+C8+C9").Name, Is.EqualTo("$C7$C8$C9"));
			Assert.That(FindType("C10+C11").Name, Is.EqualTo("$C10$C11"));
			Assert.That(FindType("C10+C12").Name, Is.EqualTo("C10$C12"));
			Assert.That(FindType("C10+C13").Name, Is.EqualTo("C10$C13"));
			Assert.That(FindType("C14").Name, Is.EqualTo("C14"));
		}

		[Test]
		public void ScriptNamespaceAttributeCanBeUsedToChangeNamespaceOfTypes() {
			Prepare(
@"using System.Runtime.CompilerServices;
[ScriptNamespace(""Some.Namespace"")] public class C1 {}
namespace X {
	[ScriptNamespace(""OtherNamespace"")]
	public class C2 {}
}");

			var t1 = FindType("C1");
			Assert.That(t1.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t1.Name, Is.EqualTo("Some.Namespace.C1"));

			var t2 = FindType("X.C2");
			Assert.That(t2.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t2.Name, Is.EqualTo("OtherNamespace.C2"));
		}

		[Test]
		public void EmptyScriptNamespaceAttributeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
[ScriptNamespace("""")] public class C1 {}
namespace X {
	[ScriptNamespace("""")]
	public class C2 {}
}");

			var t1 = FindType("C1");
			Assert.That(t1.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t1.Name, Is.EqualTo("C1"));

			var t2 = FindType("X.C2");
			Assert.That(t2.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t2.Name, Is.EqualTo("C2"));
		}

		[Test]
		public void ScriptNamespaceOnTheAssemblySetsTheDefaultButCanBeOverridden() {
			Prepare(
@"using System.Runtime.CompilerServices;
[assembly: ScriptNamespace(""my.ns"")]

namespace SomeNamespace { public class Class1 {} [ScriptNamespace(""otherns"")] public class Class5 {} }
namespace SomeNamespace.Nested.Something { public class Class2 {} }
namespace Something.Entirely.Different { public class Class3 {} }
public class Class4 {}
");

			var t1 = FindType("SomeNamespace.Class1");
			Assert.That(t1.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t1.Name, Is.EqualTo("my.ns.Class1"));

			var t2 = FindType("SomeNamespace.Nested.Something.Class2");
			Assert.That(t2.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t2.Name, Is.EqualTo("my.ns.Class2"));

			var t3 = FindType("Something.Entirely.Different.Class3");
			Assert.That(t3.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t3.Name, Is.EqualTo("my.ns.Class3"));

			var t4 = FindType("Class4");
			Assert.That(t4.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t4.Name, Is.EqualTo("my.ns.Class4"));

			var t5 = FindType("SomeNamespace.Class5");
			Assert.That(t5.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t5.Name, Is.EqualTo("otherns.Class5"));
		}

		[Test]
		public void EmptyScriptNamespaceAttributeOnAssemblyWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
[assembly: ScriptNamespace("""")]
public class C1 {}
namespace X { public class C2 {} }");

			var t1 = FindType("C1");
			Assert.That(t1.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t1.Name, Is.EqualTo("C1"));

			var t2 = FindType("X.C2");
			Assert.That(t2.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t2.Name, Is.EqualTo("C2"));
		}

		[Test]
		public void InvalidIdentifierInAssemblyScriptNamespaceAttributeIsAnError() {
			Prepare(@"using System.Runtime.CompilerServices; [assembly: ScriptNamespace(""invalid-identifier"")] public class Class1 {}", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code, Is.EqualTo(7002));
			Assert.That(AllErrors[0].Args[0], Is.EqualTo("assembly"));
		}

		[Test]
		public void IgnoreNamespaceAttributeWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
[IgnoreNamespace] public class C1 {}
namespace X {
	[IgnoreNamespace]
	public class C2 {}
}");

			var t1 = FindType("C1");
			Assert.That(t1.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t1.Name, Is.EqualTo("C1"));

			var t2 = FindType("X.C2");
			Assert.That(t2.Type, Is.EqualTo(TypeScriptSemantics.ImplType.NormalType));
			Assert.That(t2.Name, Is.EqualTo("C2"));
		}

		[Test]
		public void ScriptNamespaceAttributeCannotBeAppliedToNestedTypes() {
			Prepare(
@"using System.Runtime.CompilerServices;
namespace X {
	public class C1 {
		[ScriptNamespace(""X"")]
		public class C2 {}
	}
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("nested type") && AllErrorTexts[0].Contains("X.C1.C2") && AllErrorTexts[0].Contains("ScriptNamespace"));
		}

		[Test]
		public void IgnoreNamespaceAttributeCannotBeAppliedToNestedTypes() {
			Prepare(
@"using System.Runtime.CompilerServices;
namespace X {
	public class C1 {
		[IgnoreNamespace]
		public class C2 {}
	}
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("nested type") && AllErrorTexts[0].Contains("X.C1.C2") && AllErrorTexts[0].Contains("IgnoreNamespace"));
		}

		[Test]
		public void CannotApplyBothIgnoreNamespaceAndScriptNamespaceToTheSameClass() {
			Prepare(
@"using System.Runtime.CompilerServices;
namespace X {
	[IgnoreNamespace, ScriptNamespace(""X"")]
	public class C1 {
		public class C2 {}
	}
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("X.C1") && AllErrorTexts[0].Contains("IgnoreNamespace") && AllErrorTexts[0].Contains("ScriptNamespace"));
		}

		[Test]
		public void ScriptNameAttributeOnTypeMustBeAValidJSIdentifier() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; [ScriptName("""")] public class C1 {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1") && AllErrorTexts[0].Contains("ScriptName") && AllErrorTexts[0].Contains("must be a valid JavaScript identifier"));

			er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; [ScriptName(""X.Y"")] public class C1 {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1") && AllErrorTexts[0].Contains("ScriptName") && AllErrorTexts[0].Contains("must be a valid JavaScript identifier"));

			er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; [ScriptName(""a b"")] public class C1 {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1") && AllErrorTexts[0].Contains("ScriptName") && AllErrorTexts[0].Contains("must be a valid JavaScript identifier"));
		}

		[Test]
		public void ScriptNamespaceAttributeArgumentMustBeAValidJSQualifiedIdentifierOrBeEmpty() {
			var er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; [ScriptNamespace(""a b"")] public class C1 {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1") && AllErrorTexts[0].Contains("ScriptNamespace") && AllErrorTexts[0].Contains("must be a valid JavaScript qualified identifier"));

			er = new MockErrorReporter(false);
			Prepare(@"using System.Runtime.CompilerServices; [ScriptNamespace("" "")] public class C1 {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1") && AllErrorTexts[0].Contains("ScriptNamespace") && AllErrorTexts[0].Contains("must be a valid JavaScript qualified identifier"));
		}

		[Test]
		public void ScriptNamespaceAndIgnoreNamespaceAttributesAreConsideredWhenMinimizingNames() {
			Prepare(
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

			Assert.That(new[] { "C1", "X.C4", "X.Y.C7" }.Select(s => FindType(s).Name).ToList(), Is.EquivalentTo(new[] { "$0", "$1", "$2" }));
			Assert.That(new[] { "C2", "X.C5", "X.Y.C9" }.Select(s => FindType(s).Name).ToList(), Is.EquivalentTo(new[] { "X.$0", "X.$1", "X.$2" }));
			Assert.That(new[] { "C3", "X.C6", "X.Y.C8" }.Select(s => FindType(s).Name).ToList(), Is.EquivalentTo(new[] { "X.Y.$0", "X.Y.$1", "X.Y.$2" }));
		}

		[Test]
		public void PreserveNameAttributePreventsMinimization() {
			Prepare(
@"using System.Runtime.CompilerServices;
[PreserveName] class C1 {}
[PreserveName] internal class C2 {}
[PreserveName] public class C3 {}
[PreserveName] public class C4 { [PreserveName] internal class C5 { [PreserveName] public class C6 {} } }
[PreserveName] internal class C7 { [PreserveName] public class C8 { [PreserveName] public class C9 {} } }
[PreserveName] public class C10 { [PreserveName] private class C11 {} [PreserveName] protected class C12 {} [PreserveName] protected internal class C13 {} }
");

			Assert.That(FindType("C1").Name, Is.EqualTo("C1"));
			Assert.That(FindType("C2").Name, Is.EqualTo("C2"));
			Assert.That(FindType("C3").Name, Is.EqualTo("C3"));
			Assert.That(FindType("C4").Name, Is.EqualTo("C4"));
			Assert.That(FindType("C4+C5").Name, Is.EqualTo("C4$C5"));
			Assert.That(FindType("C4+C5+C6").Name, Is.EqualTo("C4$C5$C6"));
			Assert.That(FindType("C7").Name, Is.EqualTo("C7"));
			Assert.That(FindType("C7+C8").Name, Is.EqualTo("C7$C8"));
			Assert.That(FindType("C7+C8+C9").Name, Is.EqualTo("C7$C8$C9"));
			Assert.That(FindType("C10+C11").Name, Is.EqualTo("C10$C11"));
			Assert.That(FindType("C10+C12").Name, Is.EqualTo("C10$C12"));
			Assert.That(FindType("C10+C13").Name, Is.EqualTo("C10$C13"));
		}

		[Test]
		public void GlobalMethodsAttributeCausesAllMethodsToBeGlobalAndPreventsMinimization() {
			Prepare(
@"using System.Runtime.CompilerServices;

[GlobalMethods]
static class C1 {
	[PreserveName]
	static void Method1() {
	}

	[PreserveCase]
	static void Method2() {
	}

	[ScriptName(""Renamed"")]
	static void Method3() {
	}

	static void Method4() {
	}
}");

			var m1 = FindMethod("C1.Method1");
			Assert.That(m1.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m1.Name, Is.EqualTo("method1"));
			Assert.That(m1.IsGlobal, Is.True);

			var m2 = FindMethod("C1.Method2");
			Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m2.Name, Is.EqualTo("Method2"));
			Assert.That(m2.IsGlobal, Is.True);

			var m3 = FindMethod("C1.Method3");
			Assert.That(m3.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m3.Name, Is.EqualTo("Renamed"));
			Assert.That(m3.IsGlobal, Is.True);

			var m4 = FindMethod("C1.Method4");
			Assert.That(m4.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
			Assert.That(m4.Name, Is.EqualTo("method4"));
			Assert.That(m4.IsGlobal, Is.True);
		}

		[Test]
		public void FieldOrPropertyOrEventInGlobalMethodsClassGivesAnError() {
			Prepare(@"using System.Runtime.CompilerServices; [GlobalMethods] static class C1 { static int i; }", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1") && AllErrorTexts[0].Contains("GlobalMethodsAttribute") && AllErrorTexts[0].Contains("fields"));

			Prepare(@"using System.Runtime.CompilerServices; [GlobalMethods] static class C1 { static event System.EventHandler e; }", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1") && AllErrorTexts[0].Contains("GlobalMethodsAttribute") && AllErrorTexts[0].Contains("events"));

			Prepare(@"using System.Runtime.CompilerServices; [GlobalMethods] static class C1 { static int P { get; set; } }", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1") && AllErrorTexts[0].Contains("GlobalMethodsAttribute") && AllErrorTexts[0].Contains("properties"));
		}

		[Test]
		public void GlobalMethodsAttributeCannotBeAppliedToNonStaticClass() {
			Prepare(@"using System.Runtime.CompilerServices; [GlobalMethods] class C1 { static int i; }", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts[0].Contains("C1") && AllErrorTexts[0].Contains("GlobalMethodsAttribute") && AllErrorTexts[0].Contains("must be static"));
		}

		[Test]
		public void ImportedAttributeCausesCodeNotToBeGeneratedForATypeAndActsAsPreserveName() {
			Prepare(
@"using System.Runtime.CompilerServices;
[Imported]
class C1 {
}");

			var t = FindType("C1");
			Assert.That(t.Type == TypeScriptSemantics.ImplType.NormalType);
			Assert.That(t.Name, Is.EqualTo("C1"));
			Assert.That(t.GenerateCode, Is.False);

			Prepare(
@"using System.Runtime.CompilerServices;
[Imported]
class C1 {
}", minimizeNames: false);

			t = FindType("C1");
			Assert.That(t.Type == TypeScriptSemantics.ImplType.NormalType);
			Assert.That(t.Name, Is.EqualTo("C1"));
			Assert.That(t.GenerateCode, Is.False);
		}

		[Test]
		public void NonScriptableOnATypeCausesTheTypeAndAnyNestedTypesAndAllMembersToBeNotUsableFromScript() {
			Prepare(
@"using System.Runtime.CompilerServices;
[NonScriptable]
class C1 {
	class C2 {
		int F;
		int P { get; set; }
		event System.EventHandler E;
		int this[int x] { get { return 0; } set {} }
		void M() {}
		C2() {}
	}

	int F;
	int P { get; set; }
	event System.EventHandler E;
	int this[int x] { get { return 0; } set {} }
	void M() {}
	C2() {}
}");

			Assert.That(FindType("C1").Type == TypeScriptSemantics.ImplType.NotUsableFromScript);
			Assert.That(FindField("C1.F").Type, Is.EqualTo(FieldScriptSemantics.ImplType.NotUsableFromScript));
			Assert.That(FindProperty("C1.P").Type, Is.EqualTo(PropertyScriptSemantics.ImplType.NotUsableFromScript));
			Assert.That(FindEvent("C1.E").Type, Is.EqualTo(EventScriptSemantics.ImplType.NotUsableFromScript));
			Assert.That(FindIndexer("C1", 1).Type, Is.EqualTo(PropertyScriptSemantics.ImplType.NotUsableFromScript));
			Assert.That(FindMethod("C1.M").Type, Is.EqualTo(MethodScriptSemantics.ImplType.NotUsableFromScript));
			Assert.That(FindConstructor("C1", 0).Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NotUsableFromScript));

			Assert.That(FindType("C1+C2").Type == TypeScriptSemantics.ImplType.NotUsableFromScript);
			Assert.That(FindField("C1+C2.F").Type, Is.EqualTo(FieldScriptSemantics.ImplType.NotUsableFromScript));
			Assert.That(FindProperty("C1+C2.P").Type, Is.EqualTo(PropertyScriptSemantics.ImplType.NotUsableFromScript));
			Assert.That(FindEvent("C1+C2.E").Type, Is.EqualTo(EventScriptSemantics.ImplType.NotUsableFromScript));
			Assert.That(FindIndexer("C1+C2", 1).Type, Is.EqualTo(PropertyScriptSemantics.ImplType.NotUsableFromScript));
			Assert.That(FindMethod("C1+C2.M").Type, Is.EqualTo(MethodScriptSemantics.ImplType.NotUsableFromScript));
			Assert.That(FindConstructor("C1+C2", 0).Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void IgnoreGenericArgumentsAttributeOnTypeCausesGenericArgumentsToBeIgnored() {
			Prepare(@"using System.Runtime.CompilerServices; [IgnoreGenericArguments] public class C1<T1, T2> {}");
			var t = FindType("C1`2");
			Assert.That(t.Name, Is.EqualTo("C1"));
			Assert.That(t.IgnoreGenericArguments, Is.True);
		}

		[Test]
		public void DelegateTypesAreReturnedAsFunction() {
			Prepare(
@"using System.Runtime.CompilerServices;
delegate int MyDelegate(int a);
delegate TResult Func<T1, T2, TResult>(T1 t1, T2 t2);
}");
			
			Assert.That(FindType("MyDelegate").Type == TypeScriptSemantics.ImplType.NormalType);
			Assert.That(FindType("MyDelegate").Name == "Function");
			Assert.That(FindType("Func`3").Type == TypeScriptSemantics.ImplType.NormalType);
			Assert.That(FindType("Func`3").Name == "Function");
		}

		[Test]
		public void ResourcesAttributeCanOnlyBeAppliedToNonGenericStaticClassesWithOnlyConstFields() {
			Prepare(
@"using System.Runtime.CompilerServices;
[Resources]
public static class C {
	public const int F1 = 12;
	public const string F2 = ""X"";
}");
			// No error is good enough

			Prepare(
@"using System.Runtime.CompilerServices;
[Resources]
public class C {
	public const int F1 = 12;
	public const string F2 = ""X"";
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("ResourcesAttribute") && m.Contains("static")));

			Prepare(
@"using System.Runtime.CompilerServices;
[Resources]
public static class C {
	public static int F1;
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("ResourcesAttribute") && m.Contains("not const fields")));

			Prepare(
@"using System.Runtime.CompilerServices;
[Resources]
public static class C {
	public static int P { get; set; }
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("ResourcesAttribute") && m.Contains("not const fields")));

			Prepare(
@"using System.Runtime.CompilerServices;
[Resources]
public static class C {
	static C() {}
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("ResourcesAttribute") && m.Contains("not const fields")));

			Prepare(
@"using System.Runtime.CompilerServices;
[Resources]
public static class C {
	public static void M() {}
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("ResourcesAttribute") && m.Contains("not const fields")));

			Prepare(
@"using System.Runtime.CompilerServices;
[Resources]
public static class C<T> {
	public const string F1 = ""X"";
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("ResourcesAttribute") && m.Contains("generic")));
		}

		[Test]
		public void MixinAttributeActsAsPerserveName() {
			Prepare(
@"using System.Runtime.CompilerServices;
[Mixin(""$.fn"")]
public static class C {
	[PreserveName]
	static void Method1(int i) {}

	[PreserveCase]
	static void Method2(int i, int j) {}

	[ScriptName(""Renamed"")]
	static void Method3() {}

	static void Method4() {}
}");
			Assert.That(FindMethod("C.Method1").Name, Is.EqualTo("method1"));
			Assert.That(FindMethod("C.Method2").Name, Is.EqualTo("Method2"));
			Assert.That(FindMethod("C.Method3").Name, Is.EqualTo("Renamed"));
			Assert.That(FindMethod("C.Method4").Name, Is.EqualTo("method4"));
		}

		[Test]
		public void MixinAttributeCanOnlyBeAppliedToNonGenericStaticClassesWithOnlyMethods() {
			Prepare(
@"using System.Runtime.CompilerServices;
[Mixin(""$.fn"")]
public class C {
	public static void M() {}
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("MixinAttribute") && m.Contains("static")));

			Prepare(
@"using System.Runtime.CompilerServices;
[Mixin(""$.fn"")]
public static class C {
	public static int F1;
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("MixinAttribute") && m.Contains("only methods")));

			Prepare(
@"using System.Runtime.CompilerServices;
[Mixin(""$.fn"")]
public static class C {
	public static int P { get; set; }
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("MixinAttribute") && m.Contains("only methods")));

			Prepare(
@"using System.Runtime.CompilerServices;
[Mixin(""$.fn"")]
public static class C {
	static C() {}
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("MixinAttribute") && m.Contains("only methods")));

			Prepare(
@"using System.Runtime.CompilerServices;
[Mixin(""$.fn"")]
public static class C<T> {
	public void M() {}
}", expectErrors: true);
			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("MixinAttribute") && m.Contains("generic")));
		}

		[Test]
		public void CannotImplementTwoInterfacesWithTheSameMethodName() {
			Prepare(@"
public interface I1 { void SomeMethod(); }
public interface I2 { void SomeMethod(int x); }
public class C1 : I1, I2 {}", expectErrors: true);

			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code, Is.EqualTo(7018));
			Assert.That(AllErrors[0].Args[0], Is.EqualTo("C1"));
			Assert.That(new[] { AllErrors[0].Args[1], AllErrors[0].Args[2] }, Is.EquivalentTo(new[] { "I1", "I2" }));
			Assert.That(AllErrors[0].Args[3], Is.EqualTo("someMethod"));
		}

		[Test]
		public void CanImplementInterfaceThatDerivesFromAnotherInterface() {
			Prepare(@"
public interface I1 { void SomeMethod(); }
public interface I2 : I1 { void SomeMethod(int x); }
public class C1 : I1, I2 {}", expectErrors: false);

			// No errors is good enough.
		}

		[Test]
		public void CanImplementTwoInterfacesThatDeriveFromTheSameInterface() {
			Prepare(@"
public interface I1 { void SomeMethod(); }
public interface I2 : I1 { void SomeMethod2(); }
public interface I3 : I1 { void SomeMethod3(); }
public class C1 : I2, I3 {}", expectErrors: false);

			// No errors is good enough.

			Prepare(@"
public interface I1 { void SomeMethod(); }
public interface I2 : I1 { void SomeMethod2(); }
public interface I3 : I1 { void SomeMethod3(); }
public class C1 : I1, I2, I3 {}", expectErrors: false);

			// No errors is good enough.
		}

		[Test]
		public void CannotDeriveFromBaseClassAndImplementInterfaceWithTheSameMethodName() {
			Prepare(@"
public class B1 { public void SomeMethod(); }
public interface I1 { void SomeMethod(int x); }
public class C1 : B1, I1 {}", expectErrors: true);

			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code, Is.EqualTo(7018));
			Assert.That(AllErrors[0].Args[0], Is.EqualTo("C1"));
			Assert.That(new[] { AllErrors[0].Args[1], AllErrors[0].Args[2] }, Is.EquivalentTo(new[] { "B1", "I1" }));
			Assert.That(AllErrors[0].Args[3], Is.EqualTo("someMethod"));
		}

		[Test]
		public void CanDeriveFromBaseClassAndImplementInterfaceWhichTheBaseClassImplements() {
			Prepare(@"
public interface I1 { void SomeMethod(int x); }
public class B1 : I1 { void SomeMethod(); }
public class C1 : B1, I1 {}", expectErrors: false);

			// No errors is good enough.
		}

		[Test]
		public void IsNamedValuesMethodWorks() {
			Prepare(@"enum E1 {} [System.Runtime.CompilerServices.NamedValues] enum E2 {}");
			Assert.That(Metadata.IsNamedValues(AllTypes["E1"]), Is.False);
			Assert.That(Metadata.IsNamedValues(AllTypes["E2"]), Is.True);
		}

		[Test]
		public void IsResourcesMethodWorks() {
			Prepare(@"static class C1 {} [System.Runtime.CompilerServices.Resources] static class C2 {}");
			Assert.That(Metadata.IsResources(AllTypes["C1"]), Is.False);
			Assert.That(Metadata.IsResources(AllTypes["C2"]), Is.True);
		}

		[Test]
		public void IsGlobalMethodsMethodWorks() {
			Prepare(@"static class C1 {} [System.Runtime.CompilerServices.GlobalMethods] static class C2 {}");
			Assert.That(Metadata.IsGlobalMethods(AllTypes["C1"]), Is.False);
			Assert.That(Metadata.IsGlobalMethods(AllTypes["C2"]), Is.True);
		}

		[Test]
		public void GetMixinArgMethodWorks() {
			Prepare(@"using System.Runtime.CompilerServices; static class C1 {} [Mixin(null)] static class C2 {} [Mixin("""")] static class C3 {} [Mixin(""$.fn"")] static class C4 {}");
			Assert.That(Metadata.GetMixinArg(AllTypes["C1"]), Is.Null);
			Assert.That(Metadata.GetMixinArg(AllTypes["C2"]), Is.EqualTo(""));
			Assert.That(Metadata.GetMixinArg(AllTypes["C3"]), Is.EqualTo(""));
			Assert.That(Metadata.GetMixinArg(AllTypes["C4"]), Is.EqualTo("$.fn"));
		}

		[Test]
		public void IsRealTypeMethodWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
[Imported] class C1 {}
[Imported(IsRealType = false)] class C2 {}
[Imported(IsRealType = true)] class C3 {}
class C4 {}
");
			Assert.That(Metadata.IsRealType(AllTypes["C1"]), Is.False);
			Assert.That(Metadata.IsRealType(AllTypes["C2"]), Is.False);
			Assert.That(Metadata.IsRealType(AllTypes["C3"]), Is.True);
			Assert.That(Metadata.IsRealType(AllTypes["C4"]), Is.True);
		}
	}
}
