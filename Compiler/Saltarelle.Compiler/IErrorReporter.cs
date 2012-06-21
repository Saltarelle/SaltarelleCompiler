using System;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	public enum MessageSeverity {
		Error,
		Warning
	}

    public interface IErrorReporter {
		void Message(MessageSeverity severity, int code, string file, TextLocation location, string message, params object[] args);
		void InternalError(string text, string file, TextLocation location);
		void InternalError(Exception ex, string file, TextLocation location, string additionalText = null);
    }

	public static class ErrorReporterExtensions {
		public static void Message(this IErrorReporter reporter, int code, string file, TextLocation location, params object[] args) {
			var msg = Messages.Get(code);
			if (msg == null)
				reporter.InternalError("Message " + code + " does not exist" + (args.Length > 0 ? " (arguments were " + string.Join(", ", args) + ")" : "") + ".", file, location);
			reporter.Message(msg.Item1, code, file, location, msg.Item2, args);
		}

		public static void Message(this IErrorReporter reporter, int code, DomRegion region, params object[] args) {
			reporter.Message(code, region.FileName, region.Begin, args);
		}

		public static void InternalError(this IErrorReporter reporter, Exception ex, DomRegion region, string additionalText = null) {
			reporter.InternalError(ex, region.FileName, region.Begin, additionalText);
		}

		public static void InternalError(this IErrorReporter reporter, string text, DomRegion region) {
			reporter.InternalError(text, region.FileName, region.Begin);
		}
	}
}