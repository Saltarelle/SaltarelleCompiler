using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	public enum MessageSeverity {
		Error,
		Warning
	}

    public interface IErrorReporter {
		DomRegion Region { get; set; }
		void Message(MessageSeverity severity, int code, string message, params object[] args);
		void InternalError(string text);
		void InternalError(Exception ex, string additionalText = null);
    }

	public static class ErrorReporterExtensions {
		public static void Message(this IErrorReporter reporter, int code, params object[] args) {
			var msg = Messages.Get(code);
			if (msg == null)
				reporter.InternalError("Message " + code + " does not exist" + (args.Length > 0 ? " (arguments were " + string.Join(", ", args) + ")" : "") + ".");
			reporter.Message(msg.Item1, code, msg.Item2, args);
		}

		public static void InternalError(this IErrorReporter reporter, Exception ex, string additionalText = null) {
			reporter.InternalError(ex, additionalText);
		}

		public static void InternalError(this IErrorReporter reporter, string text) {
			reporter.InternalError(text);
		}
	}
}