using ICSharpCode.NRefactory.CSharp;

namespace Saltarelle.Compiler.Compiler {
	public class UnsupportedConstructsScanner : DepthFirstAstVisitor {
		private readonly IErrorReporter _errorReporter;
		private readonly bool _isCorelibCompilation;
		private bool _result;

		public UnsupportedConstructsScanner(IErrorReporter errorReporter, bool isCorelibCompilation) {
			this._errorReporter = errorReporter;
			_isCorelibCompilation = isCorelibCompilation;
		}

		public bool ProcessAndReturnTrueIfEverythingIsSupported(SyntaxTree syntaxTree) {
			_result = true;
			syntaxTree.AcceptVisitor(this);
			return _result;
		}

		public override void VisitYieldReturnStatement(YieldReturnStatement yieldReturnStatement) {
			_errorReporter.Message(7998, yieldReturnStatement.GetRegion(), "yield return");
			_result = false;
		}

		public override void VisitYieldBreakStatement(YieldBreakStatement yieldBreakStatement) {
			_errorReporter.Message(7998, yieldBreakStatement.GetRegion(), "yield break");
			_result = false;
		}

		public override void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression) {
			if (unaryOperatorExpression.Operator == UnaryOperatorType.Await) {
				_errorReporter.Message(7998, unaryOperatorExpression.GetRegion(), "await");
				_result = false;
			}
			else {
				base.VisitUnaryOperatorExpression(unaryOperatorExpression);
			}
		}

		public override void VisitQueryExpression(QueryExpression queryExpression) {
			_errorReporter.Message(7998, queryExpression.GetRegion(), "query expression");
			_result = false;
		}

		public override void VisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement) {
			_errorReporter.Message(7998, gotoCaseStatement.GetRegion(), "goto case");
			_result = false;
		}

		public override void VisitGotoDefaultStatement(GotoDefaultStatement gotoDefaultStatement) {
			_errorReporter.Message(7998, gotoDefaultStatement.GetRegion(), "goto default");
			_result = false;
		}

		public override void VisitGotoStatement(GotoStatement gotoStatement) {
			_errorReporter.Message(7998, gotoStatement.GetRegion(), "goto");
			_result = false;
		}

		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration) {
			if (typeDeclaration.ClassType == ClassType.Struct && !_isCorelibCompilation) {
				_errorReporter.Message(7998, typeDeclaration.GetRegion(), "user-defined value type (struct)");
				_result = false;
			}
			else {
				base.VisitTypeDeclaration(typeDeclaration);
			}
		}

	}
}
