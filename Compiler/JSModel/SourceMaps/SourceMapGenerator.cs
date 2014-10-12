using System.IO;

namespace Saltarelle.Compiler.JSModel.SourceMaps {
	public class SourceMapGenerator : ISourceMapRecorder {
		private readonly SourceMapBuilder _sourceMapBuilder;

		// TODO: make it configurable by the user
		private string _sourceRoot = @"../sources/";

		public SourceMapGenerator(string scriptPath, string mapPath) {
			string scriptUri    = Path.GetFileName(scriptPath);
			string sourceMapUri = Path.GetFileName(mapPath);         
			_sourceMapBuilder = new SourceMapBuilder(sourceMapUri, scriptUri, _sourceRoot);
		}

		public void RecordLocation(int scriptLine, int scriptCol, string sourcePath, int sourceLine, int sourceCol) {
			// patch MSDOS-like path separator
			var path = sourcePath.Replace(@"\","/");

			var sourceLocation = new SourceLocation(path, "", sourceLine - 1, sourceCol - 1);    // convert line and column to 0-based
			_sourceMapBuilder.AddMapping(scriptLine - 1, scriptCol - 1, sourceLocation);
		}

		public void WriteSourceMap(StreamWriter target) {
			string mapFileContent = _sourceMapBuilder.Build();
			target.Write(mapFileContent);
		}
	}
}
