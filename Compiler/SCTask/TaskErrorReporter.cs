using System;
using ICSharpCode.NRefactory;
using Microsoft.Build.Utilities;

namespace Saltarelle.Compiler.SCTask {
	public class TaskErrorReporter : IErrorReporter {
		private readonly TaskLoggingHelper _log;

		public TaskErrorReporter(TaskLoggingHelper log) {
			_log = log;
		}

		public void Message(MessageSeverity severity, int code, string file, TextLocation location, string message, params object[] args) {
			if (severity == MessageSeverity.Error) {
				_log.LogError(null, string.Format("CS{0:0000}", code), null, file, location.Line, location.Column, location.Line, location.Column, message, args);
			}
			else {
				_log.LogWarning(null, string.Format("CS{0:0000}", code), null, file, location.Line, location.Column, location.Line, location.Column, message, args);
			}
		}

		public void InternalError(string text, string file, TextLocation location) {
			this.Message(7999, file, location, text);
		}

		public void InternalError(Exception ex, string file, TextLocation location, string additionalText = null) {
			this.Message(7999, file, location, (additionalText != null ? additionalText + ": " : "") + ex.ToString());
		}
	}
}
