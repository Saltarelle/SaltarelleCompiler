using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.OOPEmulator;

namespace Saltarelle.Compiler.Tests {
	[TestFixture]
	public class TarjanTests {
		private List<string> RunTest(int numNodes, params string[] edges) {
			var result = TopologicalSorter.FindAndTopologicallySortStronglyConnectedComponents(Enumerable.Range('a', numNodes).Select(c => ((char)c)), x => edges.Where(e => e[0] == x).Select(e => e[1]));
			return result.Select(x => new string(x.ToArray())).ToList();
		}

		[Test]
		public void TestAlgorithm() {
			var result = RunTest(12, "ab", "bc", "be", "bf", "cd", "cg", "dc", "dh", "ea", "ef", "fg", "gf", "hd", "hg", "ii", "kl");
			Assert.That(result, Has.Count.EqualTo(7));
			var idx1 = result.FindIndex(s => s.Length == 3 && s.Contains("a") && s.Contains("b") && s.Contains("e"));
			var idx2 = result.FindIndex(s => s.Length == 3 && s.Contains("c") && s.Contains("d") && s.Contains("h"));
			var idx3 = result.FindIndex(s => s.Length == 2 && s.Contains("f") && s.Contains("g"));
			var idx4 = result.FindIndex(s => s.Length == 1 && s.Contains("i"));
			var idx5 = result.FindIndex(s => s.Length == 1 && s.Contains("j"));
			var idx6 = result.FindIndex(s => s.Length == 1 && s.Contains("k"));
			var idx7 = result.FindIndex(s => s.Length == 1 && s.Contains("l"));
			Assert.That(idx1, Is.GreaterThanOrEqualTo(0), "Component abe not found");
			Assert.That(idx2, Is.GreaterThanOrEqualTo(0), "Component cdh not found");
			Assert.That(idx3, Is.GreaterThanOrEqualTo(0), "Component fg not found");
			Assert.That(idx4, Is.GreaterThanOrEqualTo(0), "Component i not found");
			Assert.That(idx5, Is.GreaterThanOrEqualTo(0), "Component j not found");
			Assert.That(idx6, Is.GreaterThanOrEqualTo(0), "Component k not found");
			Assert.That(idx7, Is.GreaterThanOrEqualTo(0), "Component l not found");

			Assert.That(idx3, Is.LessThanOrEqualTo(idx1));
			Assert.That(idx3, Is.LessThanOrEqualTo(idx2));
			Assert.That(idx2, Is.LessThanOrEqualTo(idx1));
			Assert.That(idx7, Is.LessThanOrEqualTo(idx6));
		}
	}
}
