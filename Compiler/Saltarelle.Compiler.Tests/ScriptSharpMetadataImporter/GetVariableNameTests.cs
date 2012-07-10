using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class GetVariableNameTests {
		[Test]
		public void ReturnsTheVariableNameWhenPossibleAndNotMinimizing() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			Assert.That(md.GetVariableName(new SimpleVariable(SpecialType.UnknownType, "variable", DomRegion.Empty), new HashSet<string>()), Is.EqualTo("variable"));
		}

		[Test]
		public void ReturnsTheVariableNameSuffixedWithAnIncrementingDigitWhenNotMinimizedAndTheNameIsAlreadyUsed() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			Assert.That(md.GetVariableName(new SimpleVariable(SpecialType.UnknownType, "variable", DomRegion.Empty), new HashSet<string> { "variable" }), Is.EqualTo("variable1"));
			Assert.That(md.GetVariableName(new SimpleVariable(SpecialType.UnknownType, "variable", DomRegion.Empty), new HashSet<string> { "variable", "variable1" }), Is.EqualTo("variable2"));
			Assert.That(md.GetVariableName(new SimpleVariable(SpecialType.UnknownType, "variable", DomRegion.Empty), new HashSet<string> { "variable", "variable1", "variable2" }), Is.EqualTo("variable3"));
		}

		[Test]
		public void ReturnsDollarTPlusAnIncrementingDigitWhenNoVariableWasSpecified() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(false);
			Assert.That(md.GetVariableName(null, new HashSet<string> { }), Is.EqualTo("$t1"));
			Assert.That(md.GetVariableName(null, new HashSet<string> { "$t1" }), Is.EqualTo("$t2"));
			Assert.That(md.GetVariableName(null, new HashSet<string> { "$t1", "$t2" }), Is.EqualTo("$t3"));
			Assert.That(md.GetVariableName(null, new HashSet<string> { "$t1", "$t2", "$t3" }), Is.EqualTo("$t4"));
		}

		[Test]
		public void ReturnsAShortUniqueNameWhenMinimizingNames() {
			var md = new MetadataImporter.ScriptSharpMetadataImporter(true);
			Assert.That(md.GetVariableName(new SimpleVariable(SpecialType.UnknownType, "variable", DomRegion.Empty), new HashSet<string>()), Is.EqualTo("a"));
			Assert.That(md.GetVariableName(new SimpleVariable(SpecialType.UnknownType, "variable", DomRegion.Empty), new HashSet<string> { "a" }), Is.EqualTo("b"));
			Assert.That(md.GetVariableName(new SimpleVariable(SpecialType.UnknownType, "variable", DomRegion.Empty), new HashSet<string> { "a", "b" }), Is.EqualTo("c"));
			Assert.That(md.GetVariableName(new SimpleVariable(SpecialType.UnknownType, "variable", DomRegion.Empty), new HashSet<string> { "a", "b", "c" }), Is.EqualTo("d"));

			Assert.That(md.GetVariableName(null, new HashSet<string>()), Is.EqualTo("a"));
			Assert.That(md.GetVariableName(null, new HashSet<string> { "a" }), Is.EqualTo("b"));
			Assert.That(md.GetVariableName(null, new HashSet<string> { "a", "b" }), Is.EqualTo("c"));
			Assert.That(md.GetVariableName(null, new HashSet<string> { "a", "b", "c" }), Is.EqualTo("d"));

			var u = new HashSet<string>();
			for (int i = 0; i < 1000; i++) {
				string name = md.GetVariableName(null, u);
				Assert.That(u.Contains(name), Is.False);
				Assert.That(name.Length <= 3);
				u.Add(name);
			}
		}
	}
}
