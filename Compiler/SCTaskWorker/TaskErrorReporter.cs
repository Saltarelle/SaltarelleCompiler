using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;
using Microsoft.Build.Utilities;

namespace Saltarelle.Compiler.SCTask {
	public class TaskErrorReporter : IErrorReporter {
		private readonly TaskLoggingHelper _log;

		public TaskErrorReporter(TaskLoggingHelper log) {
			_log = log;
		}

		public DomRegion Region { get; set; }

		public void Message(MessageSeverity severity, int code, string message, params object[] args) {
			if (severity == MessageSeverity.Error) {
				_log.LogError(null, string.Format("CS{0:0000}", code), null, Region.FileName, Region.BeginLine, Region.BeginColumn, Region.EndLine, Region.EndColumn, message, args);
			}
			else {
				_log.LogWarning(null, string.Format("CS{0:0000}", code), null, Region.FileName, Region.BeginLine, Region.BeginColumn, Region.EndLine, Region.EndColumn, message, args);
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
