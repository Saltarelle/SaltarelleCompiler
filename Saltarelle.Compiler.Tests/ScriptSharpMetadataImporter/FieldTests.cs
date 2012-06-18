using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class FieldTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void FieldsWork() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class C1 {
	public int Field1;
}");

			var f1 = FindField("C1.Field1");
			Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f1.Name, Is.EqualTo("field1"));
		}

		[Test]
		public void NameIsPreservedForImportedTypes() {
			Prepare(
@"using System.Runtime.CompilerServices;

[Imported]
class C1 {
	int Field1;
}");

			var f1 = FindField("C1.Field1");
			Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f1.Name, Is.EqualTo("field1"));
		}

		[Test]
		public void FieldHidingBaseMemberGetsAUniqueName() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class B {
	public int Field;
}

public class D : B {
	public new int Field;
}");

			var f1 = FindField("D.Field");
			Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f1.Name, Is.EqualTo("field$1"));
		}

		[Test]
		public void RenamingFieldWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	[ScriptName(""Renamed"")]
	public int Field1;
	[PreserveName]
	public int Field2;
	[PreserveCase]
	public int Field3;
}");

			var f1 = FindField("C1.Field1");
			Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f1.Name, Is.EqualTo("Renamed"));

			var f2 = FindField("C1.Field2");
			Assert.That(f2.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f2.Name, Is.EqualTo("field2"));

			var f3 = FindField("C1.Field3");
			Assert.That(f3.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f3.Name, Is.EqualTo("Field3"));
		}

		[Test]
		public void NonPublicFieldsArePrefixedWithADollarIfSymbolsAreNotMinimized() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public int Field1;
}

public class C2 {
	private int Field2;
	internal int Field3;
}", minimizeNames: false);

			var f1 = FindField("C1.Field1");
			Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f1.Name, Is.EqualTo("$field1"));

			var f2 = FindField("C2.Field2");
			Assert.That(f2.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f2.Name, Is.EqualTo("$field2"));

			var f3 = FindField("C2.Field3");
			Assert.That(f3.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f3.Name, Is.EqualTo("$field3"));
		}

		[Test]
		public void NonScriptableAttributeCausesFieldToNotBeUsableFromScript() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[NonScriptable]
	public int Field1;
}
");

			var impl = FindField("C1.Field1");
			Assert.That(impl.Type, Is.EqualTo(FieldScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void ScriptNameCannotBeBlank() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C {
	[ScriptName("""")]
	public int Field;
}", expectErrors: true);

			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors.Any(m => m.Contains("C.Field") && m.Contains("ScriptNameAttribute") && m.Contains("field") && m.Contains("cannot be empty")));
		}
	}
}