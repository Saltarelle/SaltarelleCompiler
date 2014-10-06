using System.Collections.Concurrent;
using System.Text;

namespace Saltarelle.Compiler.JSModel {
	public class CodeBuilder {
		private static readonly ConcurrentDictionary<int, string> _indents = new ConcurrentDictionary<int, string>();
		private int _indentLevel = 0;
		private readonly StringBuilder _sb;

		public int CurrentLine { get; private set; }
		public int CurrentCol { get; private set; }

		private string GetIndent() {
			string result;
			if (_indents.TryGetValue(_indentLevel, out result))
				return result;
			result = new string('\t', _indentLevel);
			_indents.TryAdd(_indentLevel, result);
			return result;
		}
		
		public CodeBuilder(int indentLevel = 0) {
			_sb = new StringBuilder();
			_indentLevel = indentLevel;
			CurrentLine = 1;
			CurrentCol  = 1;
		}
		
		public CodeBuilder Indent() {
			_indentLevel++;
			return this;
		}
		
		public CodeBuilder Outdent() {
			_indentLevel--;
			return this;
		}

		public CodeBuilder Append(string value) {
			EnsureIndented();
			for (int i = 0; i < value.Length; i++) {
				if (value[i] == '\n') {
					CurrentLine++;
					CurrentCol = 1;
				}
				else
					CurrentCol++;
			}
			_sb.Append(value);
			return this;
		}
		
		public CodeBuilder AppendFormat(string format, params object[] args) {
			Append(string.Format(format, args));
			return this;
		}

		public CodeBuilder AppendLine(string value) {
			Append(value);
			AppendLine();
			return this;
		}
		
		public CodeBuilder AppendLine() {
			_sb.Append("\n");
			CurrentLine++;
			CurrentCol = 1;
			return this;
		}
		
		public CodeBuilder EnsureIndented() {
			if (CurrentCol == 1) {
				string indent = GetIndent();
				_sb.Append(indent);
				CurrentCol += indent.Length;
			}
			return this;
		}

		public override string ToString() {
			return _sb.ToString();
		}
	}
}
