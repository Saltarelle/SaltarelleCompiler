using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler {
    public interface IMetadataImporter {
		/// <summary>
		/// Prepare to handle the specified types.
		/// </summary>
		/// <param name="allTypes">All types in the compilation.</param>
		/// <param name="mainAssembly">Main assembly for the compilation.</param>
		/// <param name="errorReporter">Error reporter to use to report errors.</param>
		void Prepare(IEnumerable<ITypeDefinition> allTypes, IAssembly mainAssembly, IErrorReporter errorReporter);

		/// <summary>
        /// Returns how a type should be implemented in script.
        /// </summary>
        TypeScriptSemantics GetTypeSemantics(ITypeDefinition typeDefinition);

        /// <summary>
        /// Gets the implementation of a method. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// Must not return null.
        /// </summary>
        MethodScriptSemantics GetMethodSemantics(IMethod method);

        /// <summary>
        /// Returns the implementation of a constructor. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// Must not return null.
        /// </summary>
        ConstructorScriptSemantics GetConstructorSemantics(IMethod method);

        /// <summary>
        /// Returns the implementation of an auto-implemented property. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// Must not return null.
        /// </summary>
        PropertyScriptSemantics GetPropertySemantics(IProperty property);

        /// <summary>
        /// Returns the name of the backing field for the specified property. Must not return null.
        /// </summary>
        string GetAutoPropertyBackingFieldName(IProperty property);

        /// <summary>
        /// Returns how a field is implemented. Might store away the returned implementation in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// </summary>
        FieldScriptSemantics GetFieldSemantics(IField property);

        /// <summary>
        /// Returns how an event is implemented. Might store away the returned implementation in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// </summary>
        EventScriptSemantics GetEventSemantics(IEvent evt);
        
        /// <summary>
        /// Returns the name of the backing field for the specified property. Must not return null.
        /// </summary>
        string GetAutoEventBackingFieldName(IEvent evt);
    }
}
