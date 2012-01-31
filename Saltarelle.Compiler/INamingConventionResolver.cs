﻿using System.Collections.Generic;
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
        /// Gets the implementation of a method. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// Must not return null.
        /// </summary>
        MethodImplOptions GetMethodImplementation(IMethod method);

        /// <summary>
        /// Returns the implementation of a constructor. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// Must not return null.
        /// </summary>
        ConstructorImplOptions GetConstructorImplementation(IMethod method);

        /// <summary>
        /// Returns the implementation of an auto-implemented property. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// Must not return null.
        /// </summary>
        PropertyImplOptions GetPropertyImplementation(IProperty property);

        /// <summary>
        /// Returns the name of the backing field for the specified property. Must not return null, and must also not return NotUsableFromScript.
        /// </summary>
        FieldImplOptions GetAutoPropertyBackingFieldImplementation(IProperty property);

        /// <summary>
        /// Returns how a field is implemented.
        /// </summary>
        FieldImplOptions GetFieldImplementation(IField property);
    }
}