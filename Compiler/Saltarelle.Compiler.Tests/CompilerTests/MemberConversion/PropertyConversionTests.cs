using System;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.TypeSystem;
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
			Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Not.Null);
		}

		[Test]
		public void InstanceAutoPropertiesWithGetSetMethodsWithNoCodeAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.ContainingType.SpecialType == SpecialType.System_Object),
			                                                  GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name, generateCode: false), MethodScriptSemantics.NormalMethod("set_" + p.Name, generateCode: false)),
			                                                };

			Compile(new[] { "class C { public string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Not.Null);
		}

		[Test]
		public void InstanceAutoPropertiesThatShouldNotGenerateBackingFieldsDoNotGenerateBackingFields() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.ContainingType.SpecialType == SpecialType.System_Object),
			                                                  ShouldGenerateAutoPropertyBackingField = p => false,
			                                                };
			Compile(new[] { "class C { public string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);

			Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").UnnamedConstructor.Body.Statements, Is.Empty);
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
		}

		[Test]
		public void InstanceAutoPropertiesWithGetSetMethodsStaticWithNoCodeAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = c => ConstructorScriptSemantics.Unnamed(skipInInitializer: c.ContainingType.SpecialType == SpecialType.System_Object),
			                                                  GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("get_" + p.Name, generateCode: false), MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("set_" + p.Name, generateCode: false)),
			                                                };
			Compile(new[] { "class C { public string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);

			Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Null);
			Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Null);
			Assert.That(FindStaticMethod("C.get_SomeProp"), Is.Null);
			Assert.That(FindStaticMethod("C.set_SomeProp"), Is.Null);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
		}

		[Test]
		public void InstanceAutoPropertiesAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name)),
			                                                  GetAutoPropertyBackingFieldName = p => "$" + p.Name
			                                                };
			Compile(new[] { "class C { public string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
		}

		[Test]
		public void InstancePropertiesWithGeneratedAccessorsAreCorreclyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field(p.Name, generateAccessors: true) };
			Compile(new[] { "class C { public string SomeProp { get { return null; } set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.SomeProp", JsMethodKind.GetAccessor), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.SomeProp", JsMethodKind.SetAccessor), Is.Not.Null);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Null);
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
		}

		[Test]
		public void InstanceFieldPropertiesWithGeneratedAccessorsAndOnlyGetAccessorsAreCorreclyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field(p.Name, generateAccessors: true) };
			Compile(new[] { "class C { public string SomeProp { get { return null; } } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.SomeProp", JsMethodKind.GetAccessor), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.SomeProp", JsMethodKind.SetAccessor), Is.Null);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Null);
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
		}

		[Test]
		public void InstanceFieldPropertiesWithGeneratedAccessorsAndOnlySetAccessorsAreCorreclyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field(p.Name, generateAccessors: true) };
			Compile(new[] { "class C { public string SomeProp { set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.SomeProp", JsMethodKind.GetAccessor), Is.Null);
			Assert.That(FindInstanceMethod("C.SomeProp", JsMethodKind.SetAccessor), Is.Not.Null);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Null);
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
		}

		[Test]
		public void InstanceFieldAutoPropertiesWithGeneratedAccessorsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field(p.Name, generateAccessors: true) };
			Compile(new[] { "class C { public int SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.SomeProp", JsMethodKind.GetAccessor), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.SomeProp", JsMethodKind.SetAccessor), Is.Not.Null);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.EqualTo("$Default({def_Int32})"));
			Assert.That(FindClass("C").StaticInitStatements, Is.Empty);
		}

		[Test]
		public void InstanceAutoPropertiesThatShouldBeInstanceFieldsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$" + p.Name) };
			Compile(new[] { "class C { public string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Not.Null);
		}

		[Test]
		public void StaticAutoPropertiesWithGetSetMethodsAndFieldAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name)),
			                                                  GetAutoPropertyBackingFieldName = p => "$" + p.Name
			                                                };

			Compile(new[] { "class C { public static string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticMethod("C.get_SomeProp"), Is.Not.Null);
			Assert.That(FindStaticMethod("C.set_SomeProp"), Is.Not.Null);
			Assert.That(FindStaticFieldInitializer("C.$SomeProp"), Is.Not.Null);
		}

		[Test]
		public void StaticAutoPropertiesThatShouldBeFieldsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$" + p.Name) };
			Compile(new[] { "class C { public static string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
			Assert.That(FindStaticFieldInitializer("C.$SomeProp"), Is.Not.Null);
		}

		[Test]
		public void StaticFieldPropertiesWithGeneratedAccessorsAreCorreclyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field(p.Name, generateAccessors: true) };
			Compile(new[] { "class C { public static string SomeProp { get { return null; } set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticMethod("C.SomeProp", JsMethodKind.GetAccessor), Is.Not.Null);
			Assert.That(FindStaticMethod("C.SomeProp", JsMethodKind.SetAccessor), Is.Not.Null);
			Assert.That(FindStaticFieldInitializer("C.$SomeProp"), Is.Null);
		}

		[Test]
		public void StaticFieldPropertiesWithGeneratedAccessorsWithOnlyGetAccessorsAreCorreclyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field(p.Name, generateAccessors: true) };
			Compile(new[] { "class C { public static string SomeProp { get { return null; } } }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticMethod("C.SomeProp", JsMethodKind.GetAccessor), Is.Not.Null);
			Assert.That(FindStaticMethod("C.SomeProp", JsMethodKind.SetAccessor), Is.Null);
			Assert.That(FindStaticFieldInitializer("C.$SomeProp"), Is.Null);
		}

		[Test]
		public void StaticFieldPropertiesWithGeneratedAccessorsWithOnlySetAccessorsAreCorreclyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field(p.Name, generateAccessors: true) };
			Compile(new[] { "class C { public static string SomeProp { set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticMethod("C.SomeProp", JsMethodKind.GetAccessor), Is.Null);
			Assert.That(FindStaticMethod("C.SomeProp", JsMethodKind.SetAccessor), Is.Not.Null);
			Assert.That(FindStaticFieldInitializer("C.$SomeProp"), Is.Null);
		}

		[Test]
		public void StaticFieldAutoPropertiesWithGeneratedAccessorsAreCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field(p.Name, generateAccessors: true) };
			Compile(new[] { "class C { public static int SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticMethod("C.SomeProp", JsMethodKind.GetAccessor), Is.Not.Null);
			Assert.That(FindStaticMethod("C.SomeProp", JsMethodKind.SetAccessor), Is.Not.Null);
			Assert.That(FindStaticFieldInitializer("C.$SomeProp"), Is.EqualTo("$Default({def_Int32})"));
		}

		[Test]
		public void InstanceAutoPropertyBackingFieldIsCorrectlyInitialized() {
			Compile(new[] { "class C<T> { public int P1 { get; set; } public string P2 { get; set; } public T P3 { get; set; } }" });
			Assert.That(FindInstanceFieldInitializer("C.$P1"), Is.EqualTo("$Default({def_Int32})"));
			Assert.That(FindInstanceFieldInitializer("C.$P2"), Is.EqualTo("null"));
			Assert.That(FindInstanceFieldInitializer("C.$P3"), Is.EqualTo("$Default($T)"));
		}

		[Test]
		public void StaticAutoPropertyBackingFieldIsCorrectlyInitialized() {
			Compile(new[] { "class C<T> { public static int P1 { get; set; } public static string P2 { get; set; } public static T P3 { get; set; } }" });
			Assert.That(FindStaticFieldInitializer("C.$P1"), Is.EqualTo("$Default({def_Int32})"));
			Assert.That(FindStaticFieldInitializer("C.$P2"), Is.EqualTo("null"));
			Assert.That(FindStaticFieldInitializer("C.$P3"), Is.EqualTo("$Default($T)"));
		}

		[Test]
		public void ManuallyImplementedInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_SomeProp"), MethodScriptSemantics.NormalMethod("set_SomeProp")) };
			Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void ManuallyImplementedInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$SomeProp") };
			Compile(new[] { "class C { public int SomeProp { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void ManuallyImplementedReadOnlyInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_SomeProp"), null) };
			Compile(new[] { "class C { public int SomeProp { get { return 0; } } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Not.Null);
			Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Null);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void ManuallyImplementedReadOnlyInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$SomeProp") };
			Compile(new[] { "class C { public int SomeProp { get { return 0; } } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void ManuallyImplementedWriteOnlyInstancePropertyWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(null, MethodScriptSemantics.NormalMethod("set_SomeProp")) };
			Compile(new[] { "class C { public int SomeProp { set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Null);
			Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void ManuallyImplementedWriteOnlyInstancePropertyThatShouldBeAFieldIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$SomeProp") };
			Compile(new[] { "class C { public int SomeProp { set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void ManuallyImplementedStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_SomeProp"), MethodScriptSemantics.NormalMethod("set_SomeProp")) };
			Compile(new[] { "class C { public static int SomeProp { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticMethod("C.get_SomeProp"), Is.Not.Null);
			Assert.That(FindStaticMethod("C.set_SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
		}

		[Test]
		public void ManuallyImplementedStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$SomeProp") };
			Compile(new[] { "class C { public static int SomeProp { get { return 0; } set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticFieldInitializer("C.$SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void ManuallyImplementedReadOnlyStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_SomeProp"), null) };
			Compile(new[] { "class C { public static int SomeProp { get { return 0; } } }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticMethod("C.get_SomeProp"), Is.Not.Null);
			Assert.That(FindStaticMethod("C.set_SomeProp"), Is.Null);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
		}

		[Test]
		public void ManuallyImplementedReadOnlyStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$SomeProp") };
			Compile(new[] { "class C { public static int SomeProp { get { return 0; } } }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticFieldInitializer("C.$SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void ManuallyImplementedWriteOnlyStaticPropertyWithGetAndSetMethodsIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(null, MethodScriptSemantics.NormalMethod("set_SomeProp")) };
			Compile(new[] { "class C { public static int SomeProp { set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticMethod("C.get_SomeProp"), Is.Null);
			Assert.That(FindStaticMethod("C.set_SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
		}

		[Test]
		public void ManuallyImplementedWriteOnlyStaticPropertyThatShouldBeAFieldIsCorrectlyImported() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$SomeProp") };
			Compile(new[] { "class C { public static int SomeProp { set {} } }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticFieldInitializer("C.$SomeProp"), Is.Not.Null);
			Assert.That(FindClass("C").InstanceMethods, Is.Empty);
			Assert.That(FindClass("C").StaticMethods, Is.Empty);
		}

		[Test]
		public void AbstractPropertyIsNotConverted() {
			var metadataImporter = new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name)),
			                                                  GetAutoPropertyBackingFieldName = p => "$" + p.Name
			                                                };

			Compile(new[] { "abstract class C { public abstract string SomeProp { get; set; } }" }, metadataImporter: metadataImporter);
			Assert.That(FindInstanceMethod("C.get_SomeProp"), Is.Null);
			Assert.That(FindInstanceMethod("C.set_SomeProp"), Is.Null);
			Assert.That(FindInstanceFieldInitializer("C.$SomeProp"), Is.Null);
		}
	}
}
