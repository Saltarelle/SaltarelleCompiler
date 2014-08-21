using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.ScriptSemantics;
using TopologicalSort;

namespace Saltarelle.Compiler {
	public interface IMetadataImporter {
		/// <summary>
		/// This method will be called for all types in an assembly. The compiler will ensure that the method is called for a type after it has been called for all the type's base types and outer types.
		/// </summary>
		/// <param name="type">All types in the compilation.</param>
		void Prepare(INamedTypeSymbol type);

		/// <summary>
		/// Reserve a name in a type. This means that the name will afterwards be reported as not available by the <see cref="IsMemberNameAvailable"/> method
		/// </summary>
		/// <param name="type">Type in which to register the member.</param>
		/// <param name="name">Name of the member to register.</param>
		/// <param name="isStatic">Whether to register a static (true) or instance (false) member.</param>
		void ReserveMemberName(INamedTypeSymbol type, string name, bool isStatic);

		/// <summary>
		/// Determines whether a member name is available for a type. For instance members, a member name is not considered available if it is used by any base type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <param name="name">Name to check.</param>
		/// <param name="isStatic">Whether to check a static (true) or instance (false) member.</param>
		bool IsMemberNameAvailable(INamedTypeSymbol type, string name, bool isStatic);

		/// <summary>
		/// Sets the semantics for a method. Should be called before Prepare. Note that this will NOT reserve the name.
		/// </summary>
		void SetMethodSemantics(IMethodSymbol method, MethodScriptSemantics semantics);

		/// <summary>
		/// Sets the semantics for a constructor. Should be called before Prepare. Note that this will NOT reserve the name.
		/// </summary>
		void SetConstructorSemantics(IMethodSymbol method, ConstructorScriptSemantics semantics);

		/// <summary>
		/// Sets the semantics for a property. Should be called before Prepare. Note that this will NOT reserve the name.
		/// </summary>
		void SetPropertySemantics(IPropertySymbol property, PropertyScriptSemantics semantics);

		/// <summary>
		/// Sets the semantics for a field. Should be called before Prepare. Note that this will NOT reserve the name.
		/// </summary>
		void SetFieldSemantics(IFieldSymbol field, FieldScriptSemantics semantics);

		/// <summary>
		/// Sets the semantics for an event. Should be called before Prepare. Note that this will NOT reserve the name.
		/// </summary>
		void SetEventSemantics(IEventSymbol evt,EventScriptSemantics semantics);

		/// <summary>
		/// Returns how a type should be implemented in script. Must not return null.
		/// </summary>
		TypeScriptSemantics GetTypeSemantics(INamedTypeSymbol typeDefinition);

		/// <summary>
		/// Gets the semantics of a method. Must not return null.
		/// </summary>
		MethodScriptSemantics GetMethodSemantics(IMethodSymbol method);

		/// <summary>
		/// Returns the semantics of a constructor. Must not return null.
		/// </summary>
		ConstructorScriptSemantics GetConstructorSemantics(IMethodSymbol method);

		/// <summary>
		/// Returns the semantics of a property. Must not return null.
		/// </summary>
		PropertyScriptSemantics GetPropertySemantics(IPropertySymbol property);

		/// <summary>
		/// Returns the semantics of a delegate. Must not return null.
		/// </summary>
		/// <param name="delegateType"></param>
		/// <returns></returns>
		DelegateScriptSemantics GetDelegateSemantics(INamedTypeSymbol delegateType);

		/// <summary>
		/// Returns the name of the backing field for the specified property. Must not return null.
		/// </summary>
		string GetAutoPropertyBackingFieldName(IPropertySymbol property);

		/// <summary>
		/// Returns whether a backing field should be generated for the specified auto-property.
		/// </summary>
		bool ShouldGenerateAutoPropertyBackingField(IPropertySymbol property);

		/// <summary>
		/// Returns the semantics of a field. Must not return null.
		/// </summary>
		FieldScriptSemantics GetFieldSemantics(IFieldSymbol field);

		/// <summary>
		/// Returns the semantics of an event. Must not return null.
		/// </summary>
		EventScriptSemantics GetEventSemantics(IEventSymbol evt);
		
		/// <summary>
		/// Returns the name of the backing field for the specified event. Must not return null.
		/// </summary>
		string GetAutoEventBackingFieldName(IEventSymbol evt);

		/// <summary>
		/// Returns whether a backing field should be generated for the specified auto-property.
		/// </summary>
		bool ShouldGenerateAutoEventBackingField(IEventSymbol evt);
	}

	public static class MetadataImporterExtensions {
		private static IEnumerable<INamedTypeSymbol> GetBaseAndOuterTypeDefinitions(INamedTypeSymbol t) {
			if (t.BaseType != null)
				yield return t.BaseType;
			foreach (var b in t.Interfaces)
				yield return b;
			if (t.ContainingType != null)
				yield return t.ContainingType;
		}

		public static void Prepare(this IMetadataImporter md, IEnumerable<INamedTypeSymbol> types) {
			var l = types.ToList();
			foreach (var t in TopologicalSorter.TopologicalSort(l, l.SelectMany(GetBaseAndOuterTypeDefinitions, Edge.Create)))
				md.Prepare(t);
		}
	}
}
