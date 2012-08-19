using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	public class ExecutableErrorReporter : IErrorReporter {
		private readonly TextWriter _writer;

		public ExecutableErrorReporter(TextWriter writer) {
			_writer = writer;
		}

		public void Message(MessageSeverity severity, int code, DomRegion region, string message, params object[] args) {
			_writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}({1},{2}): {3} CS{4:0000}: {5}", region.FileName, region.BeginLine, region.BeginColumn, GetSeverityText(severity), code, string.Format(message, args)));
		}

		public void InternalError(string text, DomRegion region) {
			this.Message(7999, region, text);
		}

		public void InternalError(Exception ex, DomRegion region, string additionalText = null) {
			this.Message(7999, region, (additionalText != null ? additionalText + ": " : "") + ex.ToString());
		}

		private static string GetSeverityText(MessageSeverity severity) {
			return severity == MessageSeverity.Error ? "error" : "warning";
		}
	}
}
