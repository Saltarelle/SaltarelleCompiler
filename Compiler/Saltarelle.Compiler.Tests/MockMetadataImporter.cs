using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests {
	public class MockMetadataImporter : IMetadataImporter {
		public MockMetadataImporter() {
			GetTypeSemantics                    = t => {
			                                               if (t.ContainingType == null)
			                                                   return TypeScriptSemantics.NormalType(t.FullyQualifiedName());
			                                               else
			                                                   return TypeScriptSemantics.NormalType(GetTypeSemantics(t.ContainingType).Name + "$" + t.Name);
			                                           };
			GetMethodSemantics                  = m => {
			                                               return MethodScriptSemantics.NormalMethod(m.Name);
			                                           };
			GetConstructorSemantics             = c => {
			                                               if (c.ContainingType.IsAnonymousType)
			                                                   throw new ArgumentException("Should not call GetConstructorSemantics for anonymous types");
			                                               else if (c.ContainingType.GetMembers().OfType<IMethodSymbol>().Count(m => m.MethodKind == MethodKind.Constructor) == 1 || c.Parameters.Length == 0)
			                                                   return ConstructorScriptSemantics.Unnamed();
			                                               else
			                                                   return ConstructorScriptSemantics.Named("ctor$" + String.Join("$", c.Parameters.Select(p => p.Type.Name)));
			                                           };
			GetPropertySemantics                = p => {
			                                               if (p.ContainingType.IsAnonymousType || (p.ContainingType.SpecialType == SpecialType.System_Array && p.Name == "Length")) {
			                                                   string name = p.Name.Replace("<>", "$");
			                                                   return PropertyScriptSemantics.Field(name.StartsWith("$") ? name : ("$" + name));
			                                               }
			                                               else {
			                                                   string name = p.IsIndexer ? "Item" : p.Name;
			                                                   return PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + name), MethodScriptSemantics.NormalMethod("set_" + name));
			                                               }
			                                           };
			GetDelegateSemantics                = d => new DelegateScriptSemantics();
			GetAutoPropertyBackingFieldName     = p => "$" + p.Name;
			ShouldGenerateAutoPropertyBackingField = p => true;
			GetFieldSemantics                   = f => FieldScriptSemantics.Field("$" + f.Name);
			GetEventSemantics                   = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name), MethodScriptSemantics.NormalMethod("remove_" + e.Name));
			GetAutoEventBackingFieldName        = e => "$" + e.Name;
			ShouldGenerateAutoEventBackingField = e => true;
			GetUsedInstanceMemberNames          = t => ImmutableArray<string>.Empty;
		}

		public bool AllowGetSemanticsForAccessorMethods { get; set; }
		public Func<ITypeSymbol, TypeScriptSemantics> GetTypeSemantics { get; set; }
		public Func<IMethodSymbol, MethodScriptSemantics> GetMethodSemantics { get; set; }
		public Func<IMethodSymbol, ConstructorScriptSemantics> GetConstructorSemantics { get; set; }
		public Func<IPropertySymbol, PropertyScriptSemantics> GetPropertySemantics { get; set; }
		public Func<INamedTypeSymbol, DelegateScriptSemantics> GetDelegateSemantics { get; set; }
		public Func<IPropertySymbol, string> GetAutoPropertyBackingFieldName { get; set; }
		public Func<IPropertySymbol, bool> ShouldGenerateAutoPropertyBackingField { get; set; }
		public Func<IFieldSymbol, FieldScriptSemantics> GetFieldSemantics { get; set; }
		public Func<IEventSymbol, EventScriptSemantics> GetEventSemantics { get; set; }
		public Func<IEventSymbol, string> GetAutoEventBackingFieldName { get; set; }
		public Func<IEventSymbol, bool> ShouldGenerateAutoEventBackingField { get; set; }
		public Func<INamedTypeSymbol, IReadOnlyList<string>> GetUsedInstanceMemberNames { get; set; }

		private void EnsureOriginalDefinition(ISymbol symbol) {
			if (!Equals(symbol, symbol.OriginalDefinition))
				throw new Exception("Symbol " + symbol + " is not the original definition");
		}

		void IMetadataImporter.Prepare(INamedTypeSymbol type) {
			EnsureOriginalDefinition(type);
		}

		void IMetadataImporter.ReserveMemberName(INamedTypeSymbol type, string name, bool isStatic) {
			EnsureOriginalDefinition(type);
		}

		bool IMetadataImporter.IsMemberNameAvailable(INamedTypeSymbol type, string name, bool isStatic) {
			EnsureOriginalDefinition(type);
			return true;
		}

		void IMetadataImporter.SetMethodSemantics(IMethodSymbol method, MethodScriptSemantics semantics) {
			EnsureOriginalDefinition(method);
		}

		void IMetadataImporter.SetConstructorSemantics(IMethodSymbol method, ConstructorScriptSemantics semantics) {
			EnsureOriginalDefinition(method);
		}

		void IMetadataImporter.SetPropertySemantics(IPropertySymbol property, PropertyScriptSemantics semantics) {
			EnsureOriginalDefinition(property);
		}

		void IMetadataImporter.SetFieldSemantics(IFieldSymbol field, FieldScriptSemantics semantics) {
			EnsureOriginalDefinition(field);
		}

		void IMetadataImporter.SetEventSemantics(IEventSymbol evt,EventScriptSemantics semantics) {
			EnsureOriginalDefinition(evt);
		}

		TypeScriptSemantics IMetadataImporter.GetTypeSemantics(INamedTypeSymbol typeDefinition) {
			return GetTypeSemantics(typeDefinition);
		}

		MethodScriptSemantics IMetadataImporter.GetMethodSemantics(IMethodSymbol method) {
			EnsureOriginalDefinition(method);
			if (method.ReducedFrom != null)
				throw new ArgumentException("Should not call GetMethodSemantics() on reduced extension method " + method, "method");

			if (!AllowGetSemanticsForAccessorMethods && method.AssociatedSymbol != null)
				throw new ArgumentException("GetMethodSemantics should not be called on the accessor " + method);
			return GetMethodSemantics(method);
		}

		ConstructorScriptSemantics IMetadataImporter.GetConstructorSemantics(IMethodSymbol method) {
			EnsureOriginalDefinition(method);
			return GetConstructorSemantics(method);
		}

		PropertyScriptSemantics IMetadataImporter.GetPropertySemantics(IPropertySymbol property) {
			EnsureOriginalDefinition(property);
			return GetPropertySemantics(property);
		}

		DelegateScriptSemantics IMetadataImporter.GetDelegateSemantics(INamedTypeSymbol delegateType) {
			EnsureOriginalDefinition(delegateType);
			return GetDelegateSemantics(delegateType);
		}

		string IMetadataImporter.GetAutoPropertyBackingFieldName(IPropertySymbol property) {
			EnsureOriginalDefinition(property);
			return GetAutoPropertyBackingFieldName(property);
		}

		bool IMetadataImporter.ShouldGenerateAutoPropertyBackingField(IPropertySymbol property) {
			EnsureOriginalDefinition(property);
			return ShouldGenerateAutoPropertyBackingField(property);
		}

		FieldScriptSemantics IMetadataImporter.GetFieldSemantics(IFieldSymbol field) {
			EnsureOriginalDefinition(field);
			return GetFieldSemantics(field);
		}

		EventScriptSemantics IMetadataImporter.GetEventSemantics(IEventSymbol evt) {
			EnsureOriginalDefinition(evt);
			return GetEventSemantics(evt);
		}

		string IMetadataImporter.GetAutoEventBackingFieldName(IEventSymbol evt) {
			EnsureOriginalDefinition(evt);
			return GetAutoEventBackingFieldName(evt);
		}

		bool IMetadataImporter.ShouldGenerateAutoEventBackingField(IEventSymbol evt) {
			EnsureOriginalDefinition(evt);
			return ShouldGenerateAutoEventBackingField(evt);
		}

		IReadOnlyList<string> IMetadataImporter.GetUsedInstanceMemberNames(INamedTypeSymbol type) {
			EnsureOriginalDefinition(type);
			return GetUsedInstanceMemberNames(type);
		}
	}
}