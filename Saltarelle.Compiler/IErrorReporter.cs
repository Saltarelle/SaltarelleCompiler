using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	public enum MessageSeverity {
		Info,
		Error,
		Warning
	}

    public interface IErrorReporter {
		void Message(int code, string file, TextLocation location, params object[] args);
		void InternalError(string text, string file, TextLocation location);
    }

	public static class ErrorReporterExtensions {
		public static void Message(this IErrorReporter reporter, int code, DomRegion region, params object[] args) {
			reporter.Message(code, region.FileName, region.Begin, args);
		}

		public static void InternalError(this IErrorReporter reporter, string text, DomRegion region) {
			reporter.InternalError(text, region.FileName, region.Begin);
		}
	}
}