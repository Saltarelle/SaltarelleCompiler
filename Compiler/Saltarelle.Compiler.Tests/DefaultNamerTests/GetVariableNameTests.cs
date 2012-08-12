using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;

namespace Saltarelle.Compiler.Tests.DefaultNamerTests {
	[TestFixture]
	public class GetVariableNameTests {
		[Test]
		public void ReturnsTheVariableNameWhenPossible() {
			var n = new DefaultNamer();
			Assert.That(n.GetVariableName("variable", new HashSet<string>()), Is.EqualTo("variable"));
		}

		[Test]
		public void ReturnsTheVariableNameSuffixedWithAnIncrementingDigitWhenTheNameIsAlreadyUsed() {
			var n = new DefaultNamer();
			Assert.That(n.GetVariableName("variable", new HashSet<string> { "variable" }), Is.EqualTo("variable1"));
			Assert.That(n.GetVariableName("variable", new HashSet<string> { "variable", "variable1" }), Is.EqualTo("variable2"));
			Assert.That(n.GetVariableName("variable", new HashSet<string> { "variable", "variable1", "variable2" }), Is.EqualTo("variable3"));
		}

		[Test]
		public void ReturnsDollarTPlusAnIncrementingDigitWhenNoDesiredNameWasSpecified() {
			var n = new DefaultNamer();
			Assert.That(n.GetVariableName(null, new HashSet<string> { }), Is.EqualTo("$t1"));
			Assert.That(n.GetVariableName(null, new HashSet<string> { "$t1" }), Is.EqualTo("$t2"));
			Assert.That(n.GetVariableName(null, new HashSet<string> { "$t1", "$t2" }), Is.EqualTo("$t3"));
			Assert.That(n.GetVariableName(null, new HashSet<string> { "$t1", "$t2", "$t3" }), Is.EqualTo("$t4"));
		}

		[Test]
		public void DoesNotReturnKeywords() {
			var n = new DefaultNamer();
			Assert.That(n.GetVariableName("finally", new HashSet<string>()), Is.EqualTo("finally1"));
		}

		[Test]
		public void ReturnsAValidJavaScriptIdentifierWithNameStartingWithDollarForTransparentIdentifers() {
			var n = new DefaultNamer();
			Assert.That(n.GetVariableName("<>identifier", new HashSet<string>()), Is.EqualTo("$identifier"));
		}
	}
}
