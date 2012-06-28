using System.Collections.Generic;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler.Compiler {
	public interface ICompiler {
		PreparedCompilation CreateCompilation(IEnumerable<ISourceFile> sourceFiles, IEnumerable<IAssemblyReference> references, IList<string> defineConstants);
		IEnumerable<JsType> Compile(PreparedCompilation compilation);
	}
}