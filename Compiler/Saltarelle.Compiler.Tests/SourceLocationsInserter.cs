using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Tests {
	public class SourceLocationsInserter : RewriterVisitorBase<object> {
		public override JsStatement VisitSequencePoint(JsSequencePoint sequencePoint, object data) {
			if (sequencePoint.Location != null) {
				var location = sequencePoint.Location.GetMappedLineSpan();
				return JsStatement.Comment(" @(" + (location.StartLinePosition.Line + 1) + ", " + (location.StartLinePosition.Character + 1) + ") - (" + (location.EndLinePosition.Line + 1) + ", " + (location.EndLinePosition.Character + 1) + ")");
			}
			else {
				return JsStatement.Comment(" @ none");
			}
		}

		private SourceLocationsInserter() {
		}

		private static readonly SourceLocationsInserter _instance = new SourceLocationsInserter();
		public static JsExpression Process(JsExpression stmt) {
			return stmt.Accept(_instance, null);
		}
	}
}