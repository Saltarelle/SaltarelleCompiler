using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
    public interface INamingConventionResolver {
        string GetTypeName(ITypeDefinition typeDefinition);
        string GetTypeParameterName(ITypeParameter typeParameter);
    }
}
