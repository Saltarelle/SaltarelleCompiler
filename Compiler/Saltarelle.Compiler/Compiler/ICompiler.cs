using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler.Compiler {
	public interface ICompiler {
		IEnumerable<JsType> Compile(CSharpCompilation compilation);
	}
}