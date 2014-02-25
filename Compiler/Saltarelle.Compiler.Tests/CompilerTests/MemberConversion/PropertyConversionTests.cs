using System;
using FluentAssertions;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MemberConversion {
	[TestFixture]
	public class PropertyConversionTests : CompilerTestBase {
		[Test]
		public void InstanceAutoPropertiesWithGetSetMethodsAndFieldAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name)),
			                                                  GetAutoPropertyBackingFieldName = p => "$" + p.Name
			                                                };

			Compile(new[] { "class C { public string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
			FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
			FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
		}

		[Test]
		public void InterfacePropertyAccessorsHaveNullDefinition() {
			Compile(new[] { "interface I { int P { get; set; } }" });
			FindInstanceMethod("I.get_P").Definition.Should().BeNull();
			FindInstanceMethod("I.get_P").Definition.Should().BeNull();
		}

		[Test]
		public void InstanceAutoPropertiesWithGetSetMethodsWithNoCodeAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.DeclaringType.IsKnownType(KnownTypeCode.Object)),
			                                                  GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name, generateCode: false), MethodScriptSemantics.NormalMethod("set_" + p.Name, generateCode: false)),
			                                                };

			Compile(new[] { "class C { public string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
		}

		[Test]
		public void InstanceAutoPropertiesThatShouldNotGenerateBackingFieldsDoNotGenerateBackingFields() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.DeclaringType.IsKnownType(KnownTypeCode.Object)),
			                                                  ShouldGenerateAutoPropertyBackingField = p => false,
			                                                };
			Compile(new[] { "class C { public string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);

			Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Not.Null);
			FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
		}

		[Test]
		public void InstanceAutoPropertiesWithGetSetMethodsStaticWithNoCodeAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.DeclaringType.IsKnownType(KnownTypeCode.Object)),
			                                                  GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("get_" + p.Name, generateCode: false), MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("set_" + p.Name, generateCode: false)),
			                                                };
			Compile(new[] { "class C { public string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);

			Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Null);
			Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Null);
			Assert.That(FindStaticMethod("C.get_SomeProp"), Is.Null);
			Assert.That(FindStaticMethod("C.set_SomeProp"), Is.Null);
			FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
		}

		[Test]
		public void InstanceAutoPropertiesAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name)),
			                                                  GetAutoPropertyBackingFieldName = p => "$" + p.Name
			                                                };
			Compile(new[] { "class C { public string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
			FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
			FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
			FindClass("C").StaticInitStatements.Should().BeEmpty();
		}

		[Test]
		public void InstanceAutoPropertiesThatShouldBeInstanceFieldsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$" + p.Name) };
			Compile(new[] { "class C { public string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
			FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
		}

		[Test]
		public void StaticAutoPropertiesWithGetSetMethodsAndFieldAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name)),
			                                                  GetAutoPropertyBackingFieldName = p => "$" + p.Name
			                                                };

			Compile(new[] { "class C { public static string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
			FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
			FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
		}

		[Test]
		public void StaticAutoPropertiesThatShouldBeFieldsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$" + p.Name) };
			Compile(new[] { "class C { public static string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
			FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
		}

		[Test]
		public void InstanceAutoPropertyBackingFieldIsCorrectlyInitialized() {
			Compile(new[] { "class C<T> { public int P1 { get; set; } public string P2 { get; set; } public T P3 { get; set; } }" });
			FindInstanceFieldInitializer("C.$P1").Should().Be("$Default({def_Int32})");
			FindInstanceFieldInitializer("C.$P2").Should().Be("$Default({def_String})");
			FindInstanceFieldInitializer("C.$P3").Should().Be("$Default($T)");
		}

		[Test]
		public void StaticAutoPropertyBackingFieldIsCorrectlyInitialized() {
			Compile(new[] { "class C<T> { public static int P1 { get; set; } public static string P2 { get; set; } public static T P3 { get; set; } }" });
			FindStaticFieldInitializer("C.$P1").Should().Be("$Default({def_Int32})");
			FindStaticFieldInitializer("C.$P2").Should().Be("$Default({def_String})");
			FindStaticFieldInitializer("C.$P3").Should().Be("$Default($T)");
		}

		[Test]
		public void ManuallyImplementedInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_SomeProp"), MethodScriptSemantics.NormalMethod("set_SomeProp")) };
			Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
			FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void ManuallyImplementedInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$SomeProp") };
			Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void ManuallyImplementedReadOnlyInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_SomeProp"), null) };
			Compile(new[] { "class C { public int SomeProp { get { return 0; } } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
			FindInstanceMethod("C.set_SomeProp").Should().BeNull();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void ManuallyImplementedReadOnlyInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$SomeProp") };
			Compile(new[] { "class C { public int SomeProp { get { return 0; } } }" }, metadataImporter: metadataImporter);
			FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void ManuallyImplementedWriteOnlyInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(null, MethodScriptSemantics.NormalMethod("set_SomeProp")) };
			Compile(new[] { "class C { public int SomeProp { set {} } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.get_SomeProp").Should().BeNull();
			FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void ManuallyImplementedWriteOnlyInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$SomeProp") };
			Compile(new[] { "class C { public int SomeProp { set {} } }" }, metadataImporter: metadataImporter);
			FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void ManuallyImplementedStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_SomeProp"), MethodScriptSemantics.NormalMethod("set_SomeProp")) };
			Compile(new[] { "class C { public static int SomeProp { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
			FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
			FindClass("C").InstanceMethods.Should().BeEmpty();
		}

		[Test]
		public void ManuallyImplementedStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$SomeProp") };
			Compile(new[] { "class C { public static int SomeProp { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void ManuallyImplementedReadOnlyStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_SomeProp"), null) };
			Compile(new[] { "class C { public static int SomeProp { get { return 0; } } }" }, metadataImporter: metadataImporter);
			FindStaticMethod("C.get_SomeProp").Should().NotBeNull();
			FindStaticMethod("C.set_SomeProp").Should().BeNull();
			FindClass("C").InstanceMethods.Should().BeEmpty();
		}

		[Test]
		public void ManuallyImplementedReadOnlyStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$SomeProp") };
			Compile(new[] { "class C { public static int SomeProp { get { return 0; } } }" }, metadataImporter: metadataImporter);
			FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void ManuallyImplementedWriteOnlyStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(null, MethodScriptSemantics.NormalMethod("set_SomeProp")) };
			Compile(new[] { "class C { public static int SomeProp { set {} } }" }, metadataImporter: metadataImporter);
			FindStaticMethod("C.get_SomeProp").Should().BeNull();
			FindStaticMethod("C.set_SomeProp").Should().NotBeNull();
			FindClass("C").InstanceMethods.Should().BeEmpty();
		}

		[Test]
		public void ManuallyImplementedWriteOnlyStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$SomeProp") };
			Compile(new[] { "class C { public static int SomeProp { set {} } }" }, metadataImporter: metadataImporter);
			FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
			FindClass("C").InstanceMethods.Should().BeEmpty();
			FindClass("C").StaticMethods.Should().BeEmpty();
		}

		[Test]
		public void AbstractPropertyIsNotAnAutoProperty() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name)),
			                                                  GetAutoPropertyBackingFieldName = p => "$" + p.Name
			                                                };

			Compile(new[] { "abstract class C { public abstract string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			FindInstanceMethod("C.get_SomeProp").Should().NotBeNull();
			FindInstanceMethod("C.get_SomeProp").Definition.Should().BeNull();
			FindInstanceMethod("C.set_SomeProp").Should().NotBeNull();
			FindInstanceMethod("C.set_SomeProp").Definition.Should().BeNull();
			FindInstanceFieldInitializer("C.$SomeProp").Should().BeNull();
		}
	}
}
