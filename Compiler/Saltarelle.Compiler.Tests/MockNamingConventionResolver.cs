using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests {
	public class MockNamingConventionResolver : INamingConventionResolver {
		public MockNamingConventionResolver() {
			GetTypeSemantics           = t => {
			                             	if (t.DeclaringTypeDefinition == null)
			                             		return TypeScriptSemantics.NormalType(t.FullName);
			                             	else
			                             		return TypeScriptSemantics.NormalType(GetTypeSemantics(t.DeclaringTypeDefinition).Name + "$" + t.Name);
			                             };
			GetTypeParameterName       = t => "$" + t.Name;
			GetMethodSemantics         = m => MethodScriptSemantics.NormalMethod(m.Name);
			GetConstructorSemantics    = c => {
			                             	if (c.DeclaringType.Kind == TypeKind.Anonymous)
			                             		return ConstructorScriptSemantics.Json(new IMember[0]);
			                             	else if (c.DeclaringType.GetConstructors().Count() == 1 || c.Parameters.Count == 0)
			                             		return ConstructorScriptSemantics.Unnamed();
			                             	else
			                             		return ConstructorScriptSemantics.Named("ctor$" + String.Join("$", c.Parameters.Select(p => p.Type.Name)));
			                             };
			GetPropertySemantics       = p => {
			                                 if (p.DeclaringType.Kind == TypeKind.Anonymous || (p.DeclaringType.FullName == "System.Array" && p.Name == "Length")) {
			                                     string name = p.Name.Replace("<>", "$");
			                                     return PropertyScriptSemantics.Field(name.StartsWith("$") ? name : ("$" + name));
			                                 }
			                                 else
			                                     return PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.NormalMethod("get_" + p.Name), MethodScriptSemantics.NormalMethod("set_" + p.Name));
			                             };
			GetAutoPropertyBackingFieldName = p => "$" + p.Name;
			GetFieldSemantics               = f => FieldScriptSemantics.Field("$" + f.Name);
			GetEventSemantics               = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.NormalMethod("add_" + e.Name), MethodScriptSemantics.NormalMethod("remove_" + e.Name));
			GetAutoEventBackingFieldName    = e => "$" + e.Name;
			GetVariableName                 = (v, used) => {
			                                      string baseName;
		                                          if (v != null) {
		                                              baseName = v.Name.Replace("<>", "$");
		                                              if (!baseName.StartsWith("$"))
		                                                  baseName = "$" + baseName;
		                                          }
		                                          else {
		                                              baseName = "$tmp";
		                                          }
			                                      if (v != null && !used.Contains(baseName))
			                                          return baseName;
			                                      int i = (v == null ? 1 : 2);
			                                      while (used.Contains(baseName + i.ToString(CultureInfo.InvariantCulture)))
			                                          i++;
			                                      return baseName + i.ToString(CultureInfo.InvariantCulture);
			                                  };
			ThisAlias                       = "$this";
		}

		public Func<ITypeDefinition, TypeScriptSemantics> GetTypeSemantics { get; set; }
		public Func<ITypeParameter, string> GetTypeParameterName { get; set; }
		public Func<IMethod, MethodScriptSemantics> GetMethodSemantics { get; set; }
		public Func<IMethod, ConstructorScriptSemantics> GetConstructorSemantics { get; set; }
		public Func<IProperty, PropertyScriptSemantics> GetPropertySemantics { get; set; }
		public Func<IProperty, string> GetAutoPropertyBackingFieldName { get; set; }
		public Func<IField, FieldScriptSemantics> GetFieldSemantics { get; set; }
		public Func<IEvent, EventScriptSemantics> GetEventSemantics { get; set; }
		public Func<IEvent, string> GetAutoEventBackingFieldName { get; set; }
		public Func<IVariable, ISet<string>, string> GetVariableName { get; set; }
		public string ThisAlias { get; set; }

		void INamingConventionResolver.Prepare(IEnumerable<ITypeDefinition> allTypes, IAssembly mainAssembly, IErrorReporter errorReporter) {
		}

		TypeScriptSemantics INamingConventionResolver.GetTypeSemantics(ITypeDefinition typeDefinition) {
			return GetTypeSemantics(typeDefinition);
		}

		string INamingConventionResolver.GetTypeParameterName(ITypeParameter typeDefinition) {
			return GetTypeParameterName(typeDefinition);
		}

		MethodScriptSemantics INamingConventionResolver.GetMethodSemantics(IMethod method) {
			return GetMethodSemantics(method);
		}

		ConstructorScriptSemantics INamingConventionResolver.GetConstructorSemantics(IMethod method) {
			return GetConstructorSemantics(method);
		}

		PropertyScriptSemantics INamingConventionResolver.GetPropertySemantics(IProperty property) {
			return GetPropertySemantics(property);
		}

		string INamingConventionResolver.GetAutoPropertyBackingFieldName(IProperty property) {
			return GetAutoPropertyBackingFieldName(property);
		}

		FieldScriptSemantics INamingConventionResolver.GetFieldSemantics(IField field) {
			return GetFieldSemantics(field);
		}

		EventScriptSemantics INamingConventionResolver.GetEventSemantics(IEvent evt) {
			return GetEventSemantics(evt);
		}

		string INamingConventionResolver.GetAutoEventBackingFieldName(IEvent evt) {
			return GetAutoEventBackingFieldName(evt);
		}

		string INamingConventionResolver.GetVariableName(IVariable variable, ISet<string> usedNames) {
			return GetVariableName(variable, usedNames);
		}

		string INamingConventionResolver.ThisAlias {
			get { return ThisAlias; }
		}
	}
}