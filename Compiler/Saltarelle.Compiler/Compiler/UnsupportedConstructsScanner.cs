using ICSharpCode.NRefactory.CSharp;

namespace Saltarelle.Compiler.Compiler {
	public class UnsupportedConstructsScanner : DepthFirstAstVisitor {
		private readonly IErrorReporter _errorReporter;
		private readonly bool _isCorelibCompilation;
		private bool _result;

		public UnsupportedConstructsScanner(IErrorReporter errorReporter, bool isCorelibCompilation) {
			_errorReporter = errorReporter;
			_isCorelibCompilation = isCorelibCompilation;
		}

		public bool ProcessAndReturnTrueIfEverythingIsSupported(SyntaxTree syntaxTree) {
			_result = true;
			syntaxTree.AcceptVisitor(this);
			return _result;
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
