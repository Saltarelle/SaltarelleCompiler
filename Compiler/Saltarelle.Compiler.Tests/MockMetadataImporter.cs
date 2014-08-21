using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests {
	public class MockMetadataImporter : IMetadataImporter {
		public MockMetadataImporter() {
			GetTypeSemantics                    = t => {
			                                               if (t.ContainingType == null)
			                                                   return TypeScriptSemantics.NormalType(t.Name);
			                                               else
			                                                   return TypeScriptSemantics.NormalType(GetTypeSemantics(t.ContainingType).Name + "$" + t.Name);
			                                           };
			GetMethodSemantics                  = m => MethodScriptSemantics.NormalMethod(m.Name);
			GetConstructorSemantics             = c => {
			                                               if (c.ContainingType.IsAnonymousType)
			                                                   return ConstructorScriptSemantics.Json(new ISymbol[0]);
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
			                                               else
			                                                   return PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name));
			                                           };
			GetDelegateSemantics                = d => new DelegateScriptSemantics();
			GetAutoPropertyBackingFieldName     = p => "$" + p.Name;
			ShouldGenerateAutoPropertyBackingField = p => true;
			GetFieldSemantics                   = f => FieldScriptSemantics.Field("$" + f.Name);
			GetEventSemantics                   = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name), MethodScriptSemantics.NormalMethod("remove_" + e.Name));
			GetAutoEventBackingFieldName        = e => "$" + e.Name;
			ShouldGenerateAutoEventBackingField = e => true;
		}

		public Func<ITypeSymbol, TypeScriptSemantics> GetTypeSemantics { get; set; }
		public Func<IMethodSymbol, MethodScriptSemantics> GetMethodSemantics { get; set; }
		public Func<IMethodSymbol, ConstructorScriptSemantics> GetConstructorSemantics { get; set; }
		public Func<IPropertySymbol, PropertyScriptSemantics> GetPropertySemantics { get; set; }
		public Func<ITypeSymbol, DelegateScriptSemantics> GetDelegateSemantics { get; set; }
		public Func<IPropertySymbol, string> GetAutoPropertyBackingFieldName { get; set; }
		public Func<IPropertySymbol, bool> ShouldGenerateAutoPropertyBackingField { get; set; }
		public Func<IFieldSymbol, FieldScriptSemantics> GetFieldSemantics { get; set; }
		public Func<IEventSymbol, EventScriptSemantics> GetEventSemantics { get; set; }
		public Func<IEventSymbol, string> GetAutoEventBackingFieldName { get; set; }
		public Func<IEventSymbol, bool> ShouldGenerateAutoEventBackingField { get; set; }

		void IMetadataImporter.Prepare(INamedTypeSymbol type) {
		}

		void IMetadataImporter.ReserveMemberName(INamedTypeSymbol type, string name, bool isStatic) {
		}

		bool IMetadataImporter.IsMemberNameAvailable(INamedTypeSymbol type, string name, bool isStatic) {
			return true;
		}

		void IMetadataImporter.SetMethodSemantics(IMethodSymbol method, MethodScriptSemantics semantics) {
		}

		void IMetadataImporter.SetConstructorSemantics(IMethodSymbol method, ConstructorScriptSemantics semantics) {
		}

		void IMetadataImporter.SetPropertySemantics(IPropertySymbol property, PropertyScriptSemantics semantics) {
		}

		void IMetadataImporter.SetFieldSemantics(IFieldSymbol field, FieldScriptSemantics semantics) {
		}

		void IMetadataImporter.SetEventSemantics(IEventSymbol evt,EventScriptSemantics semantics) {
		}

		TypeScriptSemantics IMetadataImporter.GetTypeSemantics(INamedTypeSymbol typeDefinition) {
			return GetTypeSemantics(typeDefinition);
		}

		MethodScriptSemantics IMetadataImporter.GetMethodSemantics(IMethodSymbol method) {
			if (method.AssociatedSymbol != null)
				throw new ArgumentException("GetMethodSemantics should not be called on the accessor " + method);
			return GetMethodSemantics(method);
		}

		ConstructorScriptSemantics IMetadataImporter.GetConstructorSemantics(IMethodSymbol method) {
			return GetConstructorSemantics(method);
		}

		PropertyScriptSemantics IMetadataImporter.GetPropertySemantics(IPropertySymbol property) {
			return GetPropertySemantics(property);
		}

		DelegateScriptSemantics IMetadataImporter.GetDelegateSemantics(INamedTypeSymbol delegateType) {
			return GetDelegateSemantics(delegateType);
		}

		string IMetadataImporter.GetAutoPropertyBackingFieldName(IPropertySymbol property) {
			return GetAutoPropertyBackingFieldName(property);
		}

		bool IMetadataImporter.ShouldGenerateAutoPropertyBackingField(IPropertySymbol property) {
			return ShouldGenerateAutoPropertyBackingField(property);
		}

		FieldScriptSemantics IMetadataImporter.GetFieldSemantics(IFieldSymbol field) {
			return GetFieldSemantics(field);
		}

		EventScriptSemantics IMetadataImporter.GetEventSemantics(IEventSymbol evt) {
			return GetEventSemantics(evt);
		}

		string IMetadataImporter.GetAutoEventBackingFieldName(IEventSymbol evt) {
			return GetAutoEventBackingFieldName(evt);
		}

		bool IMetadataImporter.ShouldGenerateAutoEventBackingField(IEventSymbol evt) {
			return ShouldGenerateAutoEventBackingField(evt);
		}
	}
}