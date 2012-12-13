using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler {
	public interface IJSTypeSystemRewriter {
		IEnumerable<JsType> Rewrite(IEnumerable<JsType> types);
	}
}
