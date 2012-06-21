using System.Collections.Generic;
using System.Text;

namespace Saltarelle.Compiler {
	public class CodeBuilder {
		private readonly Dictionary<int, string> _indents = new Dictionary<int, string>() { { 0, "" } };
		private int _indentLevel = 0;
		private readonly StringBuilder _sb;
		private bool _atLineStart;
		
		public CodeBuilder() : this(new StringBuilder()) {
		}
		
		public CodeBuilder(StringBuilder sb) {
			this._sb = sb;
			_atLineStart = true;
		}
		
		internal int IndentLevel { get { return _indentLevel; } }

		public CodeBuilder Indent() {
			_indentLevel++;
			if (!_indents.ContainsKey(_indentLevel))
				_indents.Add(_indentLevel, new string('\t', _indentLevel));
			return this;
		}
		
		public CodeBuilder Outdent() {
			_indentLevel--;
			return this;
		}

		public CodeBuilder Append(string value) {
			if (_atLineStart)
				_sb.Append(_indents[_indentLevel]);
			_atLineStart = false;
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
			_sb.AppendLine();
			_atLineStart = true;
			return this;
		}
		
		public CodeBuilder PreventIndent() {
			_atLineStart = false;
			return this;
		}

		public override string ToString() {
			return _sb.ToString();
		}
	}
}
