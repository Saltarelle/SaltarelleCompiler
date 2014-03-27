using System.Collections.Generic;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.OOPEmulation;

namespace Saltarelle.Compiler {
	/// <summary>
	/// This interface can be implemented to support transforming <see cref="JsType"/>s to actual JavaScript.
	/// </summary>
	public interface IOOPEmulator {
		IEnumerable<JsStatement> GetCodeBeforeFirstType(IEnumerable<JsType> types);
		TypeOOPEmulation EmulateType(JsType type);
		IEnumerable<JsStatement> GetCodeAfterLastType(IEnumerable<JsType> types);
		IEnumerable<JsStatement> GetStaticInitStatements(JsClass type);
	}
}
