using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CoreLib.Plugin;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests.MetadataImporterTests {
	[TestFixture]
	public class ChainingTests {
		private Dictionary<string, INamedTypeSymbol> AllTypes { get; set; }
		private IMetadataImporter Metadata { get; set; }

		private void Prepare(string source, Action preparer) {
			var compilation = PreparedCompilation.CreateCompilation("Test", OutputKind.DynamicallyLinkedLibrary, new[] { new MockSourceFile("File.cs", source) }, new[] { Files.Mscorlib }, null);
			AllTypes = compilation.Assembly.GetAllTypes().ToDictionary(t => t.MetadataName);

			var er = new MockErrorReporter(true);
			var s = new AttributeStore(compilation, er);
			Metadata = new MetadataImporter(er, compilation, s, new CompilerOptions());
			preparer();
			Metadata.Prepare(compilation.GetAllTypes());
			Assert.That(er.AllMessages, Is.Empty, "Should not generate errrors");
		}

		private IEnumerable<ISymbol> FindMembers(string name) {
			var lastDot = name.LastIndexOf('.');
			return AllTypes[name.Substring(0, lastDot)].GetMembers().Where(m => m.Name == name.Substring(lastDot + 1));
		}

		private IPropertySymbol FindProperty(string name) {
			return FindMembers(name).Cast<IPropertySymbol>().Single();
		}

		private IFieldSymbol FindField(string name) {
			return FindMembers(name).Cast<IFieldSymbol>().Single();
		}

		private IEventSymbol FindEvent(string name) {
			return FindMembers(name).Cast<IEventSymbol>().Single();
		}

		private List<IMethodSymbol> FindMethods(string name) {
			return FindMembers(name).Cast<IMethodSymbol>().Where(m => m.MethodKind != MethodKind.Constructor).ToList();
		}

		private IMethodSymbol FindMethod(string name) {
			return FindMethods(name).Single(m => m.ExplicitInterfaceImplementations.IsEmpty);
		}

		private IMethodSymbol FindMethod(string name, int parameterCount) {
			return FindMethods(name).Single(m => m.ExplicitInterfaceImplementations.IsEmpty && m.Parameters.Length == parameterCount);

		}

		private List<IMethodSymbol> FindConstructors(string type) {
			return AllTypes[type].GetConstructors().ToList();
		}

		private IMethodSymbol FindConstructor(string type, int parameterCount) {
			return FindConstructors(type).Single(m => m.Parameters.Length == parameterCount);
		}

		[Test]
		public void ReserveInstanceMemberNameCausesANameToBeUnusableForATypeAndAllDerivedTypes() {
			Prepare(@"
				public class B {
					public int SomeName() {}
				}
				public interface I {
					public int InterfaceName();
				}
				public class C : B, I {
					public int SomeName() {}
					int I.InterfaceName() {}
					public int InterfaceName() {}
				}
				public class D : C {
					public new int SomeName() {}
				}
			", () => {
				Metadata.ReserveMemberName(AllTypes["B"], "someName", false);
				Metadata.ReserveMemberName(AllTypes["I"], "interfaceName", false);
			});

			Assert.That(Metadata.GetMethodSemantics(FindMethod("B.SomeName")).Name, Is.EqualTo("someName$1"));
			Assert.That(Metadata.GetMethodSemantics(FindMethod("C.SomeName")).Name, Is.EqualTo("someName$2"));
			Assert.That(Metadata.GetMethodSemantics(FindMethod("D.SomeName")).Name, Is.EqualTo("someName$3"));
			Assert.That(Metadata.GetMethodSemantics(FindMethod("I.InterfaceName")).Name, Is.EqualTo("interfaceName$1"));
			Assert.That(Metadata.GetMethodSemantics(FindMethod("C.InterfaceName")).Name, Is.EqualTo("interfaceName$2"));
		}

		[Test]
		public void ReserveStaticMemberNameCausesANameToBeUnusableForATypeButNotDerivedTypes() {
			Prepare(@"
				public class B {
					public static int SomeName() {}
				}
				public class C : B {
					public static int SomeName() {}
				}
			", () => {
				Metadata.ReserveMemberName(AllTypes["B"], "someName", true);
			});

			Assert.That(Metadata.GetMethodSemantics(FindMethod("B.SomeName")).Name, Is.EqualTo("someName$1"));
			Assert.That(Metadata.GetMethodSemantics(FindMethod("C.SomeName")).Name, Is.EqualTo("someName"));
		}

		[Test]
		public void IsNameAvailableReturnsFalseForReservedMembers() {
			Prepare(@"public class C {}", () => {});
			var c = AllTypes["C"];
			Assert.That(Metadata.IsMemberNameAvailable(c, "constructor", true), Is.False);
			Assert.That(Metadata.IsMemberNameAvailable(c, "constructor", false), Is.False);
			Assert.That(Metadata.IsMemberNameAvailable(c, "prototype", true), Is.False);
			Assert.That(Metadata.IsMemberNameAvailable(c, "prototype", false), Is.True);
			Assert.That(Metadata.IsMemberNameAvailable(c, "for", true), Is.False);
			Assert.That(Metadata.IsMemberNameAvailable(c, "for", false), Is.False);
			Assert.That(Metadata.IsMemberNameAvailable(c, "something", true), Is.True);
			Assert.That(Metadata.IsMemberNameAvailable(c, "something", false), Is.True);
		}

		[Test]
		public void SetMethodSemanticsWorks() {
			Prepare(@"public class C { public void TheMethod(int i) {} public void TheMethod(int i, int j) {} }", () => {
				Metadata.SetMethodSemantics(FindMethod("C.TheMethod", 1), MethodScriptSemantics.NormalMethod("__something_else__"));
			});
			Assert.AreEqual(Metadata.GetMethodSemantics(FindMethod("C.TheMethod", 1)).Name, "__something_else__");
			Assert.AreEqual(Metadata.GetMethodSemantics(FindMethod("C.TheMethod", 2)).Name, "theMethod");
		}

		[Test]
		public void SetConstructorSemanticsWorks() {
			Prepare(@"public class C { public C() {} public C(int i) {} }", () => {
				Metadata.SetConstructorSemantics(FindConstructor("C", 0), ConstructorScriptSemantics.Named("__something_else__"));
			});
			Assert.AreEqual(Metadata.GetConstructorSemantics(FindConstructor("C", 0)).Type, ConstructorScriptSemantics.ImplType.NamedConstructor);
			Assert.AreEqual(Metadata.GetConstructorSemantics(FindConstructor("C", 0)).Name, "__something_else__");
			Assert.AreEqual(Metadata.GetConstructorSemantics(FindConstructor("C", 1)).Type, ConstructorScriptSemantics.ImplType.UnnamedConstructor);
		}

		[Test]
		public void SetPropertySemanticsWorks() {
			Prepare(@"public class C { public int TheProperty { get; set; } } public class D : C { public new int TheProperty { get; set; } }", () => {
				Metadata.SetPropertySemantics(FindProperty("C.TheProperty"), PropertyScriptSemantics.Field("__something_else__"));
			});
			Assert.AreEqual(Metadata.GetPropertySemantics(FindProperty("C.TheProperty")).Type, PropertyScriptSemantics.ImplType.Field);
			Assert.AreEqual(Metadata.GetPropertySemantics(FindProperty("C.TheProperty")).FieldName, "__something_else__");
			Assert.AreEqual(Metadata.GetPropertySemantics(FindProperty("D.TheProperty")).Type, PropertyScriptSemantics.ImplType.GetAndSetMethods);
			Assert.AreEqual(Metadata.GetPropertySemantics(FindProperty("D.TheProperty")).GetMethod.Name, "get_theProperty");
		}

		[Test]
		public void SetFieldSemanticsWorks() {
			Prepare(@"public class C { public int TheField; } public class D : C { public new int TheField; }", () => {
				Metadata.SetFieldSemantics(FindField("C.TheField"), FieldScriptSemantics.Field("__something_else__"));
			});
			Assert.AreEqual(Metadata.GetFieldSemantics(FindField("C.TheField")).Name, "__something_else__");
			Assert.AreEqual(Metadata.GetFieldSemantics(FindField("D.TheField")).Name, "theField");
		}

		[Test]
		public void SetEventSemanticsWorks() {
			Prepare(@"public class C { public event System.EventHandler TheEvent; } public class D : C { public new event System.EventHandler TheEvent; }", () => {
				Metadata.SetEventSemantics(FindEvent("C.TheEvent"), EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("addIt"), MethodScriptSemantics.NormalMethod("removeIt")));
			});
			Assert.AreEqual(Metadata.GetEventSemantics(FindEvent("C.TheEvent")).Type, EventScriptSemantics.ImplType.AddAndRemoveMethods);
			Assert.AreEqual(Metadata.GetEventSemantics(FindEvent("C.TheEvent")).AddMethod.Name, "addIt");
			Assert.AreEqual(Metadata.GetEventSemantics(FindEvent("D.TheEvent")).Type, EventScriptSemantics.ImplType.AddAndRemoveMethods);
			Assert.AreEqual(Metadata.GetEventSemantics(FindEvent("D.TheEvent")).AddMethod.Name, "add_theEvent");
		}
	}
}
