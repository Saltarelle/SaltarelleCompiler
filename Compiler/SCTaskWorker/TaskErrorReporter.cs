using System;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler.SCTask {
	public class TaskErrorReporter : IErrorReporter {
		private readonly TaskLoggingHelper _log;

		public TaskErrorReporter(TaskLoggingHelper log) {
			_log = log;
		}

		public Location Location { get; set; }

		public void Message(DiagnosticSeverity severity, int code, string message, params object[] args) {
			var loc = Location != null ? Location.GetMappedLineSpan() : default(FileLinePositionSpan);
			if (severity == DiagnosticSeverity.Error) {
				_log.LogError(null, string.Format("CS{0:0000}", code), null, loc.Path, loc.StartLinePosition.Line, loc.StartLinePosition.Character, loc.EndLinePosition.Line, loc.EndLinePosition.Character, message, args);
			}
			else {
				_log.LogWarning(null, string.Format("CS{0:0000}", code), null, loc.Path, loc.StartLinePosition.Line, loc.StartLinePosition.Character, loc.EndLinePosition.Line, loc.EndLinePosition.Character, message, args);
			}
		}

		public void InternalError(string text) {
			this.Message(Messages.InternalError, text);
		}

		public void InternalError(Exception ex, string additionalText = null) {
			this.Message(Messages.InternalError, (additionalText != null ? additionalText + ": " : "") + ex.ToString());
		}
	}
}
