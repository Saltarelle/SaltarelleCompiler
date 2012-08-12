using System.Linq;
using NUnit.Framework;
using FluentAssertions;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporterTests {
	[TestFixture]
	public class MiscTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void EncodeNumberWorksWhenAllowingDigitFirst() {
			Assert.That(Enumerable.Range(0, 190).Select(i => MetadataImporter.ScriptSharpMetadataImporter.EncodeNumber(i, false)).ToList(), Is.EqualTo(new[] {
				"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
				"10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "1a", "1b", "1c", "1d", "1e", "1f", "1g", "1h", "1i", "1j", "1k", "1l", "1m", "1n", "1o", "1p", "1q", "1r", "1s", "1t", "1u", "1v", "1w", "1x", "1y", "1z", "1A", "1B", "1C", "1D", "1E", "1F", "1G", "1H", "1I", "1J", "1K", "1L", "1M", "1N", "1O", "1P", "1Q", "1R", "1S", "1T", "1U", "1V", "1W", "1X", "1Y", "1Z",
				"20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "2a", "2b", "2c", "2d", "2e", "2f", "2g", "2h", "2i", "2j", "2k", "2l", "2m", "2n", "2o", "2p", "2q", "2r", "2s", "2t", "2u", "2v", "2w", "2x", "2y", "2z", "2A", "2B", "2C", "2D", "2E", "2F", "2G", "2H", "2I", "2J", "2K", "2L", "2M", "2N", "2O", "2P", "2Q", "2R", "2S", "2T", "2U", "2V", "2W", "2X", "2Y", "2Z",
				"30", "31", "32", "33"
			}));

			Assert.That(Enumerable.Range(3844, 190).Select(i => MetadataImporter.ScriptSharpMetadataImporter.EncodeNumber(i, false)).ToList(), Is.EqualTo(new[] {
				"100", "101", "102", "103", "104", "105", "106", "107", "108", "109", "10a", "10b", "10c", "10d", "10e", "10f", "10g", "10h", "10i", "10j", "10k", "10l", "10m", "10n", "10o", "10p", "10q", "10r", "10s", "10t", "10u", "10v", "10w", "10x", "10y", "10z", "10A", "10B", "10C", "10D", "10E", "10F", "10G", "10H", "10I", "10J", "10K", "10L", "10M", "10N", "10O", "10P", "10Q", "10R", "10S", "10T", "10U", "10V", "10W", "10X", "10Y", "10Z",
				"110", "111", "112", "113", "114", "115", "116", "117", "118", "119", "11a", "11b", "11c", "11d", "11e", "11f", "11g", "11h", "11i", "11j", "11k", "11l", "11m", "11n", "11o", "11p", "11q", "11r", "11s", "11t", "11u", "11v", "11w", "11x", "11y", "11z", "11A", "11B", "11C", "11D", "11E", "11F", "11G", "11H", "11I", "11J", "11K", "11L", "11M", "11N", "11O", "11P", "11Q", "11R", "11S", "11T", "11U", "11V", "11W", "11X", "11Y", "11Z",
				"120", "121", "122", "123", "124", "125", "126", "127", "128", "129", "12a", "12b", "12c", "12d", "12e", "12f", "12g", "12h", "12i", "12j", "12k", "12l", "12m", "12n", "12o", "12p", "12q", "12r", "12s", "12t", "12u", "12v", "12w", "12x", "12y", "12z", "12A", "12B", "12C", "12D", "12E", "12F", "12G", "12H", "12I", "12J", "12K", "12L", "12M", "12N", "12O", "12P", "12Q", "12R", "12S", "12T", "12U", "12V", "12W", "12X", "12Y", "12Z",
				"130", "131", "132", "133"
			}));

			Assert.That(Enumerable.Range(7688, 190).Select(i => MetadataImporter.ScriptSharpMetadataImporter.EncodeNumber(i, false)).ToList(), Is.EqualTo(new[] {
				"200", "201", "202", "203", "204", "205", "206", "207", "208", "209", "20a", "20b", "20c", "20d", "20e", "20f", "20g", "20h", "20i", "20j", "20k", "20l", "20m", "20n", "20o", "20p", "20q", "20r", "20s", "20t", "20u", "20v", "20w", "20x", "20y", "20z", "20A", "20B", "20C", "20D", "20E", "20F", "20G", "20H", "20I", "20J", "20K", "20L", "20M", "20N", "20O", "20P", "20Q", "20R", "20S", "20T", "20U", "20V", "20W", "20X", "20Y", "20Z",
				"210", "211", "212", "213", "214", "215", "216", "217", "218", "219", "21a", "21b", "21c", "21d", "21e", "21f", "21g", "21h", "21i", "21j", "21k", "21l", "21m", "21n", "21o", "21p", "21q", "21r", "21s", "21t", "21u", "21v", "21w", "21x", "21y", "21z", "21A", "21B", "21C", "21D", "21E", "21F", "21G", "21H", "21I", "21J", "21K", "21L", "21M", "21N", "21O", "21P", "21Q", "21R", "21S", "21T", "21U", "21V", "21W", "21X", "21Y", "21Z",
				"220", "221", "222", "223", "224", "225", "226", "227", "228", "229", "22a", "22b", "22c", "22d", "22e", "22f", "22g", "22h", "22i", "22j", "22k", "22l", "22m", "22n", "22o", "22p", "22q", "22r", "22s", "22t", "22u", "22v", "22w", "22x", "22y", "22z", "22A", "22B", "22C", "22D", "22E", "22F", "22G", "22H", "22I", "22J", "22K", "22L", "22M", "22N", "22O", "22P", "22Q", "22R", "22S", "22T", "22U", "22V", "22W", "22X", "22Y", "22Z",
				"230", "231", "232", "233"
			}));

			Assert.That(Enumerable.Range(238328, 190).Select(i => MetadataImporter.ScriptSharpMetadataImporter.EncodeNumber(i, false)).ToList(), Is.EqualTo(new[] {
				"1000", "1001", "1002", "1003", "1004", "1005", "1006", "1007", "1008", "1009", "100a", "100b", "100c", "100d", "100e", "100f", "100g", "100h", "100i", "100j", "100k", "100l", "100m", "100n", "100o", "100p", "100q", "100r", "100s", "100t", "100u", "100v", "100w", "100x", "100y", "100z", "100A", "100B", "100C", "100D", "100E", "100F", "100G", "100H", "100I", "100J", "100K", "100L", "100M", "100N", "100O", "100P", "100Q", "100R", "100S", "100T", "100U", "100V", "100W", "100X", "100Y", "100Z",
				"1010", "1011", "1012", "1013", "1014", "1015", "1016", "1017", "1018", "1019", "101a", "101b", "101c", "101d", "101e", "101f", "101g", "101h", "101i", "101j", "101k", "101l", "101m", "101n", "101o", "101p", "101q", "101r", "101s", "101t", "101u", "101v", "101w", "101x", "101y", "101z", "101A", "101B", "101C", "101D", "101E", "101F", "101G", "101H", "101I", "101J", "101K", "101L", "101M", "101N", "101O", "101P", "101Q", "101R", "101S", "101T", "101U", "101V", "101W", "101X", "101Y", "101Z",
				"1020", "1021", "1022", "1023", "1024", "1025", "1026", "1027", "1028", "1029", "102a", "102b", "102c", "102d", "102e", "102f", "102g", "102h", "102i", "102j", "102k", "102l", "102m", "102n", "102o", "102p", "102q", "102r", "102s", "102t", "102u", "102v", "102w", "102x", "102y", "102z", "102A", "102B", "102C", "102D", "102E", "102F", "102G", "102H", "102I", "102J", "102K", "102L", "102M", "102N", "102O", "102P", "102Q", "102R", "102S", "102T", "102U", "102V", "102W", "102X", "102Y", "102Z",
				"1030", "1031", "1032", "1033"
			}));

			Enumerable.Range(0, 1000000).Select(i => MetadataImporter.ScriptSharpMetadataImporter.EncodeNumber(i, false)).Should().OnlyHaveUniqueItems();
		}

		[Test]
		public void EncodeNumberWorksWhenNotAllowingDigitFirst() {
			Assert.That(Enumerable.Range(0, 160).Select(i => MetadataImporter.ScriptSharpMetadataImporter.EncodeNumber(i, true)).ToList(), Is.EqualTo(new[] {
				"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
				"ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt", "bu", "bv", "bw", "bx", "by", "bz", "bA", "bB", "bC", "bD", "bE", "bF", "bG", "bH", "bI", "bJ", "bK", "bL", "bM", "bN", "bO", "bP", "bQ", "bR", "bS", "bT", "bU", "bV", "bW", "bX", "bY", "bZ",
				"ca", "cb", "cc", "cd", "ce", "cf", "cg", "ch", "ci", "cj", "ck", "cl", "cm", "cn", "co", "cp", "cq", "cr", "cs", "ct", "cu", "cv", "cw", "cx", "cy", "cz", "cA", "cB", "cC", "cD", "cE", "cF", "cG", "cH", "cI", "cJ", "cK", "cL", "cM", "cN", "cO", "cP", "cQ", "cR", "cS", "cT", "cU", "cV", "cW", "cX", "cY", "cZ",
				"da", "db", "dc", "dd"
			}));

			Enumerable.Range(0, 1000000).Select(i => MetadataImporter.ScriptSharpMetadataImporter.EncodeNumber(i, true)).Should().OnlyHaveUniqueItems();
			foreach (var s in Enumerable.Range(0, 1000000).Select(i => MetadataImporter.ScriptSharpMetadataImporter.EncodeNumber(i, true))) {
				Assert.That(s[0], Is.Not.InRange('0', '9'), "Invalid item " + s);
				Assert.That(s, Is.Not.EqualTo("in"), "Found keyword in returned list.");
			}
		}
	}
}
