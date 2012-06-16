using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class GetTypeParameterNameTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void GetTypeParameterNameWorksWhenNotMinimizingNames() {
			Prepare(
@"class C1<T1, T2> {
	class C2<T3, T4> {
		void M1<T5, T6>(T5 a, T6 b) {}
	}
	class C3<T3> {
		void M2<T4, T5>(T5 a, T6 b) {}
	}
);", minimizeNames: false);

			var c1 = AllTypes["C1`2"];
			Assert.That(c1.TypeParameters.Select(Metadata.GetTypeParameterName), Is.EqualTo(new[] { "T1", "T2" }));

			var c2 = AllTypes["C1`2+C2`2"];
			Assert.That(c2.TypeParameters.Select(Metadata.GetTypeParameterName), Is.EqualTo(new[] { "T1", "T2", "T3", "T4" }));

			var m1 = c2.Methods.Single(m => m.Name == "M1");
			Assert.That(m1.TypeParameters.Select(Metadata.GetTypeParameterName), Is.EqualTo(new[] { "T5", "T6" }));

			var c3 = AllTypes["C1`2+C3`1"];
			Assert.That(c3.TypeParameters.Select(Metadata.GetTypeParameterName), Is.EqualTo(new[] { "T1", "T2", "T3" }));

			var m2 = c3.Methods.Single(m => m.Name == "M2");
			Assert.That(m2.TypeParameters.Select(Metadata.GetTypeParameterName), Is.EqualTo(new[] { "T4", "T5" }));
		}

		[Test]
		public void GetTypeParameterNameWorksWhenMinimizingNames() {
			Prepare(
@"class C1<T1, T2> {
	class C2<T3, T4> {
		void M1<T5, T6>(T5 a, T6 b) {}
	}
	class C3<T3> {
		void M2<T4, T5>(T5 a, T6 b) {}
	}
);", minimizeNames: true);

			var c1 = AllTypes["C1`2"];
			Assert.That(c1.TypeParameters.Select(Metadata.GetTypeParameterName), Is.EqualTo(new[] { "a", "b" }));

			var c2 = AllTypes["C1`2+C2`2"];
			Assert.That(c2.TypeParameters.Select(Metadata.GetTypeParameterName), Is.EqualTo(new[] { "a", "b", "c", "d" }));

			var m1 = c2.Methods.Single(m => m.Name == "M1");
			Assert.That(m1.TypeParameters.Select(Metadata.GetTypeParameterName), Is.EqualTo(new[] { "e", "f" }));

			var c3 = AllTypes["C1`2+C3`1"];
			Assert.That(c3.TypeParameters.Select(Metadata.GetTypeParameterName), Is.EqualTo(new[] { "a", "b", "c" }));

			var m2 = c3.Methods.Single(m => m.Name == "M2");
			Assert.That(m2.TypeParameters.Select(Metadata.GetTypeParameterName), Is.EqualTo(new[] { "d", "e" }));
		}
	}
}
