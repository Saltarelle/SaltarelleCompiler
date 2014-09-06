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

		public void Message(DiagnosticSeverity severity, string code, string message, params object[] args) {
			var loc = Location != null ? Location.GetMappedLineSpan() : default(FileLinePositionSpan);
			if (severity == DiagnosticSeverity.Error) {
				_log.LogError(null, code, null, loc.Path, loc.StartLinePosition.Line + 1, loc.StartLinePosition.Character + 1, loc.EndLinePosition.Line + 1, loc.EndLinePosition.Character + 1, message, args);
			}
			else {
				_log.LogWarning(null, code, null, loc.Path, loc.StartLinePosition.Line + 1, loc.StartLinePosition.Character + 1, loc.EndLinePosition.Line + 1, loc.EndLinePosition.Character + 1, message, args);
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
