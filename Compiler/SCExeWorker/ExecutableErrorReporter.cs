using System;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler.SCExe {
	public class ExecutableErrorReporter : IErrorReporter {
		private readonly TextWriter _writer;

		public ExecutableErrorReporter(TextWriter writer) {
			_writer = writer;
		}

		public Location Location { get; set; }

		public void Message(MessageSeverity severity, int code, string message, params object[] args) {
			var loc = Location != null ? Location.GetMappedLineSpan() : default(FileLinePositionSpan);
			_writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}({1},{2}): {3} CS{4:0000}: {5}", loc.Path, loc.StartLinePosition.Line, loc.StartLinePosition.Character, GetSeverityText(severity), code, string.Format(message, args)));
		}

		public void InternalError(string text) {
			this.Message(Messages.InternalError, text);
		}

		public void InternalError(Exception ex, string additionalText = null) {
			this.Message(Messages.InternalError, (additionalText != null ? additionalText + ": " : "") + ex.ToString());
		}

		private static string GetSeverityText(MessageSeverity severity) {
			return severity == MessageSeverity.Error ? "error" : "warning";
		}
	}
}
