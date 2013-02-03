using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
	public class JsEnum : JsType {
		public JsEnum(ITypeDefinition csharpTypeDefinition) : base(csharpTypeDefinition) {
		}
	}
}
