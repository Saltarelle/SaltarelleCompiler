using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.ReferenceImporter {
	/// <summary>
	/// This reference importer assumes that root namespaces and types are global objects.
	/// </summary>
	public class GlobalNamespaceReferenceImporter : IReferenceImporter {
		private class ImportVisitor : RewriterVisitorBase<object> {
			public override JsExpression VisitTypeReferenceExpression(JsTypeReferenceExpression expression, object data) {
				var parts = expression.TypeName.Split('.');
				JsExpression result = JsExpression.Identifier(parts[0]);
				for (int i = 1; i < parts.Length; i++)
					result = JsExpression.MemberAccess(result, parts[i]);
				return result;
			}

			public JsStatement Process(JsStatement stmt) {
				return VisitStatement(stmt, null);
			}

			private ImportVisitor() {
			}

			public static readonly ImportVisitor Instance = new ImportVisitor();
		}

		public IList<JsStatement> ImportReferences(IList<JsStatement> statements) {
			return statements.Select(ImportVisitor.Instance.Process).ToList();
		}
	}
}
