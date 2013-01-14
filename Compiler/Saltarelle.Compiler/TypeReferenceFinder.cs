using System.Collections.Generic;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler {
	public class TypeReferenceFinder : RewriterVisitorBase<HashSet<ITypeDefinition>> {
		public override JsExpression VisitTypeReferenceExpression(JsTypeReferenceExpression expression, HashSet<ITypeDefinition> data) {
			data.Add(expression.Type);
			return base.VisitTypeReferenceExpression(expression, data);
		}

		private TypeReferenceFinder() {
		}

		public static ISet<ITypeDefinition> Analyze(IEnumerable<JsStatement> statements) {
			var obj = new TypeReferenceFinder();
			var result = new HashSet<ITypeDefinition>();
			foreach (var s in statements)
				obj.VisitStatement(s, result);
			return result;
		}

		public static ISet<ITypeDefinition> Analyze(JsExpression expression) {
			var obj = new TypeReferenceFinder();
			var result = new HashSet<ITypeDefinition>();
			obj.VisitExpression(expression, result);
			return result;
		}
	}
}