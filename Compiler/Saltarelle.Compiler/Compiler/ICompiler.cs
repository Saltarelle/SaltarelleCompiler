using System.Collections.Generic;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler.Compiler {
	public interface ICompiler {
		IEnumerable<JsType> Compile(PreparedCompilation compilation);
	}
}