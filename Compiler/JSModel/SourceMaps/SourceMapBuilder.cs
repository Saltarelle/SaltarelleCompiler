using System.Collections.Generic;
using System.Text;

namespace Saltarelle.Compiler.JSModel.SourceMaps {  
	public class SourceMapBuilder {
		private const int VLQBaseShift = 5;
		private const int VLQBaseMask = (1 << 5) - 1;
		private const int VLQContinuationBit = 1 << 5;
		private const int VLQContinuationMask = 1 << 5;
		private const string Base64Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

		private readonly string _sourceMapPath;
		private readonly string _scriptPath; 
		private readonly string _sourceRoot; 

		private readonly List<SourceMapEntry> _entries;

		private readonly Map<string, int> _sourceUrlMap;
		private readonly List<string> _sourceUrlList;
		private readonly Map<string, int> _sourceNameMap;
		private readonly List<string> _sourceNameList;

		private int _previousTargetLine;
		private int _previousTargetColumn;
		private int _previousSourceUrlIndex;
		private int _previousSourceLine;
		private int _previousSourceColumn;
		private int _previousSourceNameIndex;
		private bool _firstEntryInLine;

		public SourceMapBuilder(string sourceMapUri, string scriptUri, string sourceRoot) {
			this._sourceMapPath = sourceMapUri;
			this._scriptPath = scriptUri;
			this._sourceRoot = sourceRoot;

			_entries = new List<SourceMapEntry>();

			_sourceUrlMap = new Map<string, int>();
			_sourceUrlList = new List<string>();
			_sourceNameMap = new Map<string, int>();
			_sourceNameList = new List<string>();

			_previousTargetLine = 0;
			_previousTargetColumn = 0;
			_previousSourceUrlIndex = 0;
			_previousSourceLine = 0;
			_previousSourceColumn = 0;
			_previousSourceNameIndex = 0;
			_firstEntryInLine = true;
		}

		public void AddMapping(int scriptLine, int scriptColumn, SourceLocation sourceLocation) {
			if (!_entries.IsEmpty() && (scriptLine == _entries.Last().ScriptLine)) {
				if (SameAsPreviousLocation(sourceLocation)) {
					// The entry points to the same source location as the previous entry in the same line, hence it is not needed for the source map.
					return;
				}
			}

			if (sourceLocation != null) {
				UpdatePreviousSourceLocation(sourceLocation);
			}
			_entries.Add(new SourceMapEntry(sourceLocation, scriptLine, scriptColumn));
		}

		public string Build() {
			ResetPreviousSourceLocation();
			var mappingsBuffer = new StringBuilder();
			_entries.ForEach(entry => WriteEntry(entry, mappingsBuffer));
			var buffer = new StringBuilder();
			buffer.Append("{\n");
			buffer.Append("  \u0022version\u0022: 3,\n");
			if (_sourceMapPath != null && _scriptPath != null) {
				buffer.Append(string.Format("  \u0022file\u0022: \u0022{0}\u0022,\n", /*uri.MakeRelativeUri(scriptUri).ToString())*/ _scriptPath) );
			}
			buffer.Append("  \u0022sourceRoot\u0022: \u0022"+_sourceRoot+"\u0022,\n");
			buffer.Append("  \u0022sources\u0022: ");
			if(_sourceMapPath != null) {
				for(int t = 0; t < _sourceUrlList.Count; t++) {
					_sourceUrlList[t] = _sourceUrlList[t];
				}
			}
			PrintStringListOn(_sourceUrlList, buffer);
			buffer.Append(",\n");
			buffer.Append("  \u0022names\u0022: ");
			PrintStringListOn(_sourceNameList, buffer);
			buffer.Append(",\n");
			buffer.Append("  \u0022mappings\u0022: \u0022");
			buffer.Append(mappingsBuffer);
			buffer.Append("\u0022\n}\n");
			return buffer.ToString();
		}

		private void ResetPreviousSourceLocation() {
			_previousSourceUrlIndex = 0;
			_previousSourceLine = 0;
			_previousSourceColumn = 0;
			_previousSourceNameIndex = 0;
		}

		private void UpdatePreviousSourceLocation(SourceLocation sourceLocation) {
			_previousSourceLine = sourceLocation.Line;
			_previousSourceColumn = sourceLocation.Column;
			string sourceUrl = sourceLocation.SourceUrl;
			_previousSourceUrlIndex = IndexOf(_sourceUrlList, sourceUrl, _sourceUrlMap);
			string sourceName = sourceLocation.SourceName;
			if (sourceName != null) {
				_previousSourceNameIndex = IndexOf(_sourceNameList, sourceName, _sourceNameMap);
			}
		}

		private bool SameAsPreviousLocation(SourceLocation sourceLocation) {
			if (sourceLocation == null) {
				return true;
			}

			int sourceUrlIndex = IndexOf(_sourceUrlList, sourceLocation.SourceUrl, _sourceUrlMap);

			return sourceUrlIndex == _previousSourceUrlIndex &&
			       sourceLocation.Line == _previousSourceLine &&
			       sourceLocation.Column == _previousSourceColumn;
		}

		private void PrintStringListOn(List<string> strings, StringBuilder buffer) {
			bool first = true;
			buffer.Append("[");
			foreach(string str in strings) {
				if (!first) buffer.Append(",");
				buffer.Append("\u0022");
				buffer.WriteJsonEscapedCharsOn(str);
				buffer.Append("\u0022");
				first = false;
			}
			buffer.Append("]");
		}

		private void WriteEntry(SourceMapEntry entry, StringBuilder output) {
			int targetLine = entry.ScriptLine;
			int targetColumn = entry.ScriptColumn;

			if (targetLine > _previousTargetLine) {
				for (int i = _previousTargetLine; i < targetLine; ++i) {
					output.Append(";");
				}
				_previousTargetLine = targetLine;
				_previousTargetColumn = 0;
				_firstEntryInLine = true;
			}

			if (!_firstEntryInLine) {
				output.Append(",");
			}
			_firstEntryInLine = false;

			EncodeVLQ(output, targetColumn - _previousTargetColumn);
			_previousTargetColumn = targetColumn;

			if (entry.SourceLocation == null)
				return;

			string sourceUrl = entry.SourceLocation.SourceUrl;
			int sourceLine = entry.SourceLocation.Line;
			int sourceColumn = entry.SourceLocation.Column;
			string sourceName = entry.SourceLocation.SourceName;

			int sourceUrlIndex = IndexOf(_sourceUrlList, sourceUrl, _sourceUrlMap);
			EncodeVLQ(output, sourceUrlIndex - _previousSourceUrlIndex);
			EncodeVLQ(output, sourceLine - _previousSourceLine);
			EncodeVLQ(output, sourceColumn - _previousSourceColumn);

			if (sourceName != null) {
				int sourceNameIndex = IndexOf(_sourceNameList, sourceName, _sourceNameMap);
				EncodeVLQ(output, sourceNameIndex - _previousSourceNameIndex);
			}

			// Update previous source location to ensure the next indices are relative
			// to those if [entry.sourceLocation].
			UpdatePreviousSourceLocation(entry.SourceLocation);
		}

		private static int IndexOf(List<string> list, string value, Map<string, int> map) {
			return map.PutIfAbsent(value, ()=> {
				int index = list.Count;
				list.Add(value);
				return index;
			});
		}

		private static void EncodeVLQ(StringBuilder output, int value) {
			int signBit = 0;
			if (value < 0) {
				signBit = 1;
				value = -value;
			}
			value = (value << 1) | signBit;
			do {
				int digit = value & VLQBaseMask;
				value >>= VLQBaseShift;
				if (value > 0) {
					digit |= VLQContinuationBit;
				}
				output.Append(Base64Digits[digit]);
			} while (value > 0);
		}
	}
}
