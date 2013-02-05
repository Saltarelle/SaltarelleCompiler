using System.Collections.Generic;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler.Compiler {
	public interface ICompiler {
		IEnumerable<JsType> Compile(PreparedCompilation compilation);
	}
}