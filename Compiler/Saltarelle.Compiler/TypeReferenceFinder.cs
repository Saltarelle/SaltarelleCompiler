using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler {
	public class TypeReferenceFinder : RewriterVisitorBase<HashSet<INamedTypeSymbol>> {
		public override JsExpression VisitTypeReferenceExpression(JsTypeReferenceExpression expression, HashSet<INamedTypeSymbol> data) {
			data.Add(expression.Type);
			return base.VisitTypeReferenceExpression(expression, data);
		}

		private TypeReferenceFinder() {
		}

		public static ISet<INamedTypeSymbol> Analyze(IEnumerable<JsStatement> statements) {
			var obj = new TypeReferenceFinder();
			var result = new HashSet<INamedTypeSymbol>();
			foreach (var s in statements)
				obj.VisitStatement(s, result);
			return result;
		}

		public static ISet<INamedTypeSymbol> Analyze(JsExpression expression) {
			var obj = new TypeReferenceFinder();
			var result = new HashSet<INamedTypeSymbol>();
			obj.VisitExpression(expression, result);
			return result;
		}
	}
}