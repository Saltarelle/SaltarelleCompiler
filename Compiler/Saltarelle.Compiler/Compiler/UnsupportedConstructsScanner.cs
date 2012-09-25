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

		public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration) {
			if (typeDeclaration.ClassType == ClassType.Struct && !_isCorelibCompilation) {
				_errorReporter.Region = typeDeclaration.GetRegion();
				_errorReporter.Message(7998, "user-defined value type (struct)");
				_result = false;
			}
			else {
				base.VisitTypeDeclaration(typeDeclaration);
			}
		}
	}
}
