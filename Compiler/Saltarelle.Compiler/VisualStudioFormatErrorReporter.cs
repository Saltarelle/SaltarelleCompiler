using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;

namespace Saltarelle.Compiler {
	public class VisualStudioFormatErrorReporter : IErrorReporter {
		private readonly TextWriter _writer;

		public VisualStudioFormatErrorReporter(TextWriter writer) {
			_writer = writer;
		}

		public void Message(MessageSeverity severity, int code, string file, TextLocation location, string message, params object[] args) {
			_writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}({1},{2}): {3} CS{4:0000}: {5}", file, location.Line, location.Column, GetSeverityText(severity), code, string.Format(message, args)));
		}

		public void InternalError(string text, string file, TextLocation location) {
			this.Message(7999, file, location, text);
		}

		public void InternalError(Exception ex, string file, TextLocation location, string additionalText = null) {
			this.Message(7999, file, location, (additionalText != null ? additionalText + ": " : "") + ex.ToString());
		}

		private static string GetSeverityText(MessageSeverity severity) {
			return severity == MessageSeverity.Error ? "error" : "warning";
		}
	}
}
