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
        string GetTypeName(ITypeResolveContext context, ITypeDefinition typeDefinition);
        string GetTypeParameterName(ITypeResolveContext context, ITypeParameter typeParameter);

        /// <summary>
        /// Gets the implementation of a method. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// If this returns null, the method is not usable from script.
        /// </summary>
        MethodImplOptions GetMethodImplementation(ITypeResolveContext context, IMethod method);

        /// <summary>
        /// Returns the name of a constructor. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// If this returns null, the constructor is not usable from script.
        /// </summary>
        ConstructorImplOptions GetConstructorImplementation(ITypeResolveContext context, IMethod method);
    }
}
