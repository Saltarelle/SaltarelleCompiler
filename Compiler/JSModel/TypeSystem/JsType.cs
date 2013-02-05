using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
	public abstract class JsType : IFreezable {
		protected bool Frozen { get; private set; }
		public ITypeDefinition CSharpTypeDefinition { get; private set; }

		protected JsType(ITypeDefinition csharpTypeDefinition) {
			CSharpTypeDefinition = csharpTypeDefinition;
		}

		public virtual void Freeze() {
			Frozen = true;
		}
	}
}
