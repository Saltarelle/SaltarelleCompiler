using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler {
	public interface IOOPEmulator {
		IList<JsStatement> Rewrite(IEnumerable<JsType> types, Func<ITypeReference, JsTypeReferenceExpression> createTypeReferenceExpression, IAssembly currentAssembly);
	}
}
