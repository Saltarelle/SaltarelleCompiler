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
		/// <param name="minimizeNames">Whether names should be minimized by default (the importer is free to use/ignore this flag however it wants).</param>
		/// <param name="mainAssembly">Main assembly for the compilation.</param>
		void Prepare(IEnumerable<ITypeDefinition> allTypes, bool minimizeNames, IAssembly mainAssembly);

		/// <summary>
        /// Returns how a type should be implemented in script. Must not return null.
        /// </summary>
        TypeScriptSemantics GetTypeSemantics(ITypeDefinition typeDefinition);

        /// <summary>
        /// Gets the semantics of a method. Must not return null.
        /// </summary>
        MethodScriptSemantics GetMethodSemantics(IMethod method);

        /// <summary>
        /// Returns the semantics of a constructor. Must not return null.
        /// </summary>
        ConstructorScriptSemantics GetConstructorSemantics(IMethod method);

        /// <summary>
        /// Returns the semantics of a property. Must not return null.
        /// </summary>
        PropertyScriptSemantics GetPropertySemantics(IProperty property);

		/// <summary>
		/// Returns the semantics of a delegate. Must not return null.
		/// </summary>
		/// <param name="delegateType"></param>
		/// <returns></returns>
		DelegateScriptSemantics GetDelegateSemantics(ITypeDefinition delegateType);

        /// <summary>
        /// Returns the name of the backing field for the specified property. Must not return null.
        /// </summary>
        string GetAutoPropertyBackingFieldName(IProperty property);

        /// <summary>
        /// Returns the semantics of a field. Must not return null.
        /// </summary>
        FieldScriptSemantics GetFieldSemantics(IField property);

        /// <summary>
        /// Returns the semantics of an event. Must not return null.
        /// </summary>
        EventScriptSemantics GetEventSemantics(IEvent evt);
        
        /// <summary>
        /// Returns the name of the backing field for the specified event. Must not return null.
        /// </summary>
        string GetAutoEventBackingFieldName(IEvent evt);
    }
}
