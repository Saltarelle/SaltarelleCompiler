using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	public enum MessageSeverity {
		Error,
		Warning
	}

    public interface IErrorReporter {
		void Message(MessageSeverity severity, int code, DomRegion region, string message, params object[] args);
		void InternalError(string text, DomRegion region);
		void InternalError(Exception ex, DomRegion region, string additionalText = null);
    }

	public static class ErrorReporterExtensions {
		public static void Message(this IErrorReporter reporter, int code, DomRegion region, params object[] args) {
			var msg = Messages.Get(code);
			if (msg == null)
				reporter.InternalError("Message " + code + " does not exist" + (args.Length > 0 ? " (arguments were " + string.Join(", ", args) + ")" : "") + ".", region);
			reporter.Message(msg.Item1, code, region, msg.Item2, args);
		}

		public static void InternalError(this IErrorReporter reporter, Exception ex, DomRegion region, string additionalText = null) {
			reporter.InternalError(ex, region, additionalText);
		}

		public static void InternalError(this IErrorReporter reporter, string text, DomRegion region) {
			reporter.InternalError(text, region);
		}
	}
}