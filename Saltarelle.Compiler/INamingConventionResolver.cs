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
		void Prepare(IEnumerable<ITypeDefinition> allTypes);

		/// <summary>
        /// Returns the name of a type as it should appear in the script. If null is included the class, and any nested class, will not appear in the output.
        /// </summary>
        [Obsolete("Must create a TypeScriptSemantics type for this. Properties should include GenerateCode, IsGlobal, etc.")]
        string GetTypeName(ITypeDefinition typeDefinition);

        string GetTypeParameterName(ITypeParameter typeParameter);

        /// <summary>
        /// Gets the implementation of a method. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// Must not return null.
        /// </summary>
        MethodScriptSemantics GetMethodImplementation(IMethod method);

        /// <summary>
        /// Returns the implementation of a constructor. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// Must not return null.
        /// </summary>
        ConstructorScriptSemantics GetConstructorImplementation(IMethod method);

        /// <summary>
        /// Returns the implementation of an auto-implemented property. Might store away the returned name in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// Must not return null.
        /// </summary>
        PropertyScriptSemantics GetPropertyImplementation(IProperty property);

        /// <summary>
        /// Returns the name of the backing field for the specified property. Must not return null.
        /// </summary>
        string GetAutoPropertyBackingFieldName(IProperty property);

        /// <summary>
        /// Returns how a field is implemented. Might store away the returned implementation in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// </summary>
        FieldScriptSemantics GetFieldImplementation(IField property);

        /// <summary>
        /// Returns how an event is implemented. Might store away the returned implementation in some kind of cache (eg. to ensure that multiple calls to the same overloaded method return the exact same name).
        /// </summary>
        EventScriptSemantics GetEventImplementation(IEvent evt);
        
        /// <summary>
        /// Returns the name of the backing field for the specified property. Must not return null.
        /// </summary>
        string GetAutoEventBackingFieldName(IEvent evt);

        /// <summary>
        /// Returns the name by which an enum field is to be known in script. May not return null.
        /// </summary>
        string GetEnumValueName(IField value);

        /// <summary>
        /// Gets the name of a variable, constrained to not used any of the names in the supplied set. Must not return null.
        /// </summary>
        string GetVariableName(IVariable variable, ISet<string> usedNames);

		/// <summary>
		/// Gets the name of a temporary variable.
		/// </summary>
		/// <param name="index">The first temporary variable in a method will have index 0, the second will have index 1, and so on.</param>
		string GetTemporaryVariableName(int index);

		/// <summary>
		/// Returns the alias for "this" whenever it has to be aliased, eg. inside a static method with this as first argument.
		/// </summary>
		string ThisAlias { get; }
    }
}
