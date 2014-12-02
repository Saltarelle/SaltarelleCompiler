using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Saltarelle.Compiler.JSModel.SourceMaps {
	public class SourceMapGenerator : ISourceMapRecorder {
		private readonly Dictionary<string, string> _aliases; 
		private readonly SourceMapBuilder _sourceMapBuilder;

		public SourceMapGenerator(string scriptPath, string sourceRoot, IEnumerable<KeyValuePair<string, string>> sourceFileAliases) {
			string scriptFileName = Path.GetFileName(scriptPath);
			_aliases = sourceFileAliases.ToDictionary(a => a.Key, a => a.Value != null ? a.Value.Replace("\\", "/") : null);
			_sourceMapBuilder = new SourceMapBuilder(scriptFileName, sourceRoot);
		}

		public void RecordLocation(int scriptLine, int scriptCol, string sourcePath, int sourceLine, int sourceCol) {
			string alias = null;
			if (sourcePath != null) {
				if (!_aliases.TryGetValue(sourcePath, out alias)) {
					throw new Exception("Could not find the file " + sourcePath + " in the list of source files");
				}
			}

			SourceLocation sourceLocation;
			if (alias == null) {
				sourceLocation = new SourceLocation("no-source-location", "", 0, 0);
			}
			else {
				sourceLocation = new SourceLocation(alias, "", sourceLine - 1, sourceCol - 1);    // convert line and column to 0-based
			}

			_sourceMapBuilder.AddMapping(scriptLine - 1, scriptCol - 1, sourceLocation);
		}

		public void WriteSourceMap(StreamWriter target) {
			string mapFileContent = _sourceMapBuilder.Build();
			target.Write(mapFileContent);
		}
	}
}
