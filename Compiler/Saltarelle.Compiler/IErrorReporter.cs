using System;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler {
	public interface IErrorReporter {
		Location Location { get; set; }
		void Message(DiagnosticSeverity severity, string code, string message, params object[] args);
		void InternalError(string text);
		void InternalError(Exception ex, string additionalText = null);
	}

	public static class ErrorReporterExtensions {
		public static void Message(this IErrorReporter reporter, Tuple<int, DiagnosticSeverity, string> message, params object[] args) {
			reporter.Message(message.Item2, string.Format(CultureInfo.InvariantCulture, "CS{0:0000}", message.Item1), message.Item3, args);
		}

		public static void InternalError(this IErrorReporter reporter, Exception ex, string additionalText = null) {
			reporter.InternalError(ex, additionalText);
		}

		public static void InternalError(this IErrorReporter reporter, string text) {
			reporter.InternalError(text);
		}
	}
}