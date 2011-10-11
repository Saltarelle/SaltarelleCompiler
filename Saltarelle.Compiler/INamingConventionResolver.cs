using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler {
    public interface INamingConventionResolver {
        /// <summary>
        /// Returns the name of a type as it should appear in the script. If null is included the class, and any nested class, will not appear in the output.
        /// </summary>
        string GetTypeName(ITypeDefinition typeDefinition);
        string GetTypeParameterName(ITypeParameter typeParameter);

        /// <summary>
        /// Gets the name of a method. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// </summary>
        MethodImplOptions GetMethodImplementation(IMethod method);
    }
}
