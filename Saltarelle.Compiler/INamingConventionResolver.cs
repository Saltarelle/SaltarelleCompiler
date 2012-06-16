using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler {
    public interface INamingConventionResolver {
		/// <summary>
		/// Prepare to handle the specified types.
		/// </summary>
		/// <param name="allTypes">All types in the compilation.</param>
		/// <param name="mainAssembly">Main assembly for the compilation.</param>
		/// <param name="errorReporter">Error reporter to use to report errors.</param>
		void Prepare(IEnumerable<ITypeDefinition> allTypes, IAssembly mainAssembly, IErrorReporter errorReporter);

		/// <summary>
        /// Returns the name of a type as it should appear in the script. If null is included the class, and any nested class, will not appear in the output.
        /// </summary>
        TypeScriptSemantics GetTypeSemantics(ITypeDefinition typeDefinition);

        /// <summary>
        /// Gets the name by which a type parameter should be known.
        /// </summary>
		string GetTypeParameterName(ITypeParameter typeParameter);

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

        /// <summary>
        /// Gets the name of a variable, constrained to not used any of the names in the supplied set. Must not return null.
        /// </summary>
        /// <param name="variable">Variable to get the name for. Can be null in order to get a name for a temporary variable.</param>
        /// <param name="usedNames">All names that are used, and thus not possible to use as the variable name.</param>
        string GetVariableName(IVariable variable, ISet<string> usedNames);

		/// <summary>
		/// Returns the alias for "this" whenever it has to be aliased, eg. inside a static method with this as first argument.
		/// </summary>
		string ThisAlias { get; }
    }
}
