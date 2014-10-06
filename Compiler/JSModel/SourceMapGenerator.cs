using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Saltarelle.Compiler.JSModel {
	public class SourceMapGenerator : ISourceMapRecorder {
		private class Location {
			public int ScriptLine { get; set; }
			public int ScriptCol { get; set; }
			public string SourcePath { get; set; }
			public int SourceLine { get; set; }
			public int SourceCol { get; set; }

			public Location(int scriptLine, int scriptCol, string sourcePath, int sourceLine, int sourceCol) {
				ScriptLine = scriptLine;
				ScriptCol = scriptCol;
				SourcePath = sourcePath;
				SourceLine = sourceLine;
				SourceCol = sourceCol;
			}
		}

		private List<Location> _locations = new List<Location>();

		public void RecordLocation(int scriptLine, int scriptCol, string sourcePath, int sourceLine, int sourceCol) {
			_locations.Add(new Location(scriptLine, scriptCol, sourcePath, sourceLine, sourceCol));
		}

		public void WriteSourceMap(Stream target) {
			using (var writer = new StreamWriter(target, Encoding.UTF8, bufferSize: 8192, leaveOpen: true)) {
				foreach (var l in _locations) {
					writer.WriteLine("({0},{1}) -> ({2}, {3}, {4})", l.ScriptLine, l.ScriptCol, l.SourcePath, l.SourceLine, l.SourceCol);
				}
			}
		}
	}
}
