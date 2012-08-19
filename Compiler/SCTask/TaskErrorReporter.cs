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

		public void Message(MessageSeverity severity, int code, DomRegion region, string message, params object[] args) {
			if (severity == MessageSeverity.Error) {
				_log.LogError(null, string.Format("CS{0:0000}", code), null, region.FileName, region.BeginLine, region.BeginColumn, region.EndLine, region.EndColumn, message, args);
			}
			else {
				_log.LogWarning(null, string.Format("CS{0:0000}", code), null, region.FileName, region.BeginLine, region.BeginColumn, region.EndLine, region.EndColumn, message, args);
			}
		}

		public void InternalError(string text, DomRegion region) {
			this.Message(7999, region, text);
		}

		public void InternalError(Exception ex, DomRegion region, string additionalText = null) {
			this.Message(7999, region, (additionalText != null ? additionalText + ": " : "") + ex.ToString());
		}
	}
}
