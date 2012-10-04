using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.MetadataImporter;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.ReferenceImporter {
	/// <summary>
	/// This reference importer assumes that root namespaces and types are global objects.
	/// </summary>
	public class GlobalNamespaceReferenceImporter : IReferenceImporter {
		private class ImportVisitor : RewriterVisitorBase<object> {
			private readonly IScriptSharpMetadataImporter _metadataImporter;

			public override JsExpression VisitTypeReferenceExpression(JsTypeReferenceExpression expression, object data) {
				var sem = _metadataImporter.GetTypeSemantics(expression.Type);
				if (sem.Type != TypeScriptSemantics.ImplType.NormalType)
					throw new ArgumentException("The type " + expression.Type.FullName + " appears in the output stage but is not a normal type.");

				var parts = sem.Name.Split('.');
				JsExpression result = JsExpression.Identifier(parts[0]);
				for (int i = 1; i < parts.Length; i++)
					result = JsExpression.MemberAccess(result, parts[i]);
				return result;
			}

			public JsStatement Process(JsStatement stmt) {
				return VisitStatement(stmt, null);
			}

			public ImportVisitor(IScriptSharpMetadataImporter metadataImporter) {
				_metadataImporter = metadataImporter;
			}
		}

		private readonly IScriptSharpMetadataImporter _metadataImporter;

		public GlobalNamespaceReferenceImporter(IScriptSharpMetadataImporter metadataImporter) {
			_metadataImporter = metadataImporter;
		}

		public IList<JsStatement> ImportReferences(IList<JsStatement> statements) {
			return statements.Select(new ImportVisitor(_metadataImporter).Process).ToList();
		}
	}
}
