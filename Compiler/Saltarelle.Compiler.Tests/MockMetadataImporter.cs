using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests {
	public class MockMetadataImporter : IMetadataImporter {
		public MockMetadataImporter() {
			GetTypeSemantics                = t => {
			                                           if (t.DeclaringTypeDefinition == null)
			                                               return TypeScriptSemantics.NormalType(t.FullName);
			                                           else
			                                               return TypeScriptSemantics.NormalType(GetTypeSemantics(t.DeclaringTypeDefinition).Name + "$" + t.Name);
			                                       };
			GetMethodSemantics              = m => MethodScriptSemantics.NormalMethod(m.Name);
			GetConstructorSemantics         = c => {
			                                           if (c.DeclaringType.Kind == TypeKind.Anonymous)
			                                               return ConstructorScriptSemantics.Json(new IMember[0]);
			                                           else if (c.DeclaringType.GetConstructors().Count() == 1 || c.Parameters.Count == 0)
			                                               return ConstructorScriptSemantics.Unnamed();
			                                           else
			                                               return ConstructorScriptSemantics.Named("ctor$" + String.Join("$", c.Parameters.Select(p => p.Type.Name)));
			                                       };
			GetPropertySemantics            = p => {
			                                           if (p.DeclaringType.Kind == TypeKind.Anonymous || (p.DeclaringType.FullName == "System.Array" && p.Name == "Length")) {
			                                               string name = p.Name.Replace("<>", "$");
			                                               return PropertyScriptSemantics.Field(name.StartsWith("$") ? name : ("$" + name));
			                                           }
			                                           else
			                                               return PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name));
			                                       };
			GetDelegateSemantics            = d => new DelegateScriptSemantics();
			GetAutoPropertyBackingFieldName = p => "$" + p.Name;
			GetFieldSemantics               = f => FieldScriptSemantics.Field("$" + f.Name);
			GetEventSemantics               = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name), MethodScriptSemantics.NormalMethod("remove_" + e.Name));
			GetAutoEventBackingFieldName    = e => "$" + e.Name;
		}

		public Func<ITypeDefinition, TypeScriptSemantics> GetTypeSemantics { get; set; }
		public Func<IMethod, MethodScriptSemantics> GetMethodSemantics { get; set; }
		public Func<IMethod, ConstructorScriptSemantics> GetConstructorSemantics { get; set; }
		public Func<IProperty, PropertyScriptSemantics> GetPropertySemantics { get; set; }
		public Func<ITypeDefinition, DelegateScriptSemantics> GetDelegateSemantics { get; set; }
		public Func<IProperty, string> GetAutoPropertyBackingFieldName { get; set; }
		public Func<IField, FieldScriptSemantics> GetFieldSemantics { get; set; }
		public Func<IEvent, EventScriptSemantics> GetEventSemantics { get; set; }
		public Func<IEvent, string> GetAutoEventBackingFieldName { get; set; }

		void IMetadataImporter.Prepare(IEnumerable<ITypeDefinition> allTypes, bool minimizeNames, IAssembly mainAssembly) {
		}

		TypeScriptSemantics IMetadataImporter.GetTypeSemantics(ITypeDefinition typeDefinition) {
			return GetTypeSemantics(typeDefinition);
		}

		MethodScriptSemantics IMetadataImporter.GetMethodSemantics(IMethod method) {
			return GetMethodSemantics(method);
		}

		ConstructorScriptSemantics IMetadataImporter.GetConstructorSemantics(IMethod method) {
			return GetConstructorSemantics(method);
		}

		PropertyScriptSemantics IMetadataImporter.GetPropertySemantics(IProperty property) {
			return GetPropertySemantics(property);
		}

		DelegateScriptSemantics IMetadataImporter.GetDelegateSemantics(ITypeDefinition delegateType) {
			return GetDelegateSemantics(delegateType);
		}

		string IMetadataImporter.GetAutoPropertyBackingFieldName(IProperty property) {
			return GetAutoPropertyBackingFieldName(property);
		}

		FieldScriptSemantics IMetadataImporter.GetFieldSemantics(IField field) {
			return GetFieldSemantics(field);
		}

		EventScriptSemantics IMetadataImporter.GetEventSemantics(IEvent evt) {
			return GetEventSemantics(evt);
		}

		string IMetadataImporter.GetAutoEventBackingFieldName(IEvent evt) {
			return GetAutoEventBackingFieldName(evt);
		}
	}
}