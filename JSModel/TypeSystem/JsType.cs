using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public abstract class JsType : IFreezable {
        public string Name { get; private set; }

        protected bool Frozen { get; private set; }
    	public ITypeDefinition CSharpTypeDefinition { get; private set; }

    	protected JsType(ITypeDefinition csharpTypeDefinition, string name) {
            Require.ValidJavaScriptNestedIdentifier(name, "name");
			CSharpTypeDefinition = csharpTypeDefinition;
            Name = name;
        }

        public virtual void Freeze() {
            Frozen = true;
        }
    }
}
