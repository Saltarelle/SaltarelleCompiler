using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.MetadataImporter {
	// Done:
	// [ScriptName] (Type)
	// [IgnoreNamespace] (Type)
	// [ScriptNamespaceAttribute] (Type)
	// [PreserveName] (Type)

	// To handle:
	// [NonScriptable] (Type | Constructor | Method | Property | Event) (event missed in ScriptSharp)
	// [Imported] (Type | Struct)
	// [ScriptAssembly] (Assembly) ?
	// [ScriptQualifier] (Assembly)
	// [ScriptNamespaceAttribute] (Assembly)
	// [Resources] (Class) ?
	// [GlobalMethods] (Class) - Needs better support in the compiler
	// [Mixin] (Class) ?
	// [NamedValues] (Enum) - Needs better support in the compiler
	// [NumericValues] (Enum)
	// [AlternateSignature] (Constructor | Method)
	// [IntrinsicProperty] (Property (/indexer))
	// [ScriptName] (Field | Property | Method | Event)
	// [PreserveCase] (Method | Property | Event | Field)
	// [PreserveName] (Method | Property | Event | Field)
	// [ScriptAlias] (Method | Property) = Literal code ?
	// [ScriptSkip] (Method) ?
	// Record
	// New attributes:
	// [InstanceMethodOnFirstArgument]

	public class ScriptSharpMetadataImporter : INamingConventionResolver {
		private Dictionary<ITypeDefinition, string> _typeNames;
		private Dictionary<string, string> _errors;
		private bool _minimizeNames;

		public ScriptSharpMetadataImporter(bool minimizeNames) {
			_minimizeNames = minimizeNames;
		}

		private string GetDefaultTypeName(ITypeDefinition def) {
			int outerCount = (def.DeclaringTypeDefinition != null ? def.DeclaringTypeDefinition.TypeParameters.Count : 0);
			return def.Name + (def.TypeParameterCount != outerCount ? "$" + (def.TypeParameterCount - outerCount).ToString(CultureInfo.InvariantCulture) : "");
		}

		private IList<object> GetAttributePositionalArgs(IEntity entity, string attributeName) {
			attributeName = "System.Runtime.CompilerServices." + attributeName;
			var attr = entity.Attributes.FirstOrDefault(a => a.AttributeType.FullName == attributeName);
			return attr != null ? attr.PositionalArguments.Select(arg => arg.ConstantValue).ToList() : null;
		}

		private string DetermineNamespace(ITypeDefinition typeDefinition) {
			while (typeDefinition.DeclaringTypeDefinition != null) {
				typeDefinition = typeDefinition.DeclaringTypeDefinition;
			}

			var ina = GetAttributePositionalArgs(typeDefinition, "IgnoreNamespaceAttribute");
			var sna = GetAttributePositionalArgs(typeDefinition, "ScriptNamespaceAttribute");
			if (ina != null) {
				if (sna != null) {
					_errors[typeDefinition.FullName + ":Namespace"] = "The type " + typeDefinition.FullName + " has both [IgnoreNamespace] and [ScriptNamespace] specified. At most one of these attributes can be specified for a type.";
					return typeDefinition.FullName;
				}
				else {
					return "";
				}
			}
			else {
				if (sna != null) {
					string arg = (string)sna[0];
					if (arg == null || (arg != "" && !arg.IsValidNestedJavaScriptIdentifier()))
						_errors[typeDefinition.FullName + ":Namespace"] = typeDefinition.FullName + ": The argument for [ScriptNamespace], when applied to a type, must be a valid JavaScript qualified identifier.";
					return arg;
				}
				else {
					return typeDefinition.Namespace;
				}
			}
		}

		private Tuple<string, string> SplitName(string typeName) {
			int dot = typeName.LastIndexOf('.');
			return dot > 0 ? Tuple.Create(typeName.Substring(0, dot), typeName.Substring(dot + 1)) : Tuple.Create("", typeName);
		}

		private string DetermineTypeName(ITypeDefinition typeDefinition) {
			var scriptNameAttr = GetAttributePositionalArgs(typeDefinition, "ScriptNameAttribute");
			string typeName, nmspace;
			if (scriptNameAttr != null && scriptNameAttr[0] != null && ((string)scriptNameAttr[0]).IsValidJavaScriptIdentifier()) {
				typeName = (string)scriptNameAttr[0];
				nmspace = DetermineNamespace(typeDefinition);
			}
			else {
				if (scriptNameAttr != null) {
					_errors[typeDefinition.FullName + ":Name"] = typeDefinition.FullName + ": The argument for [ScriptName], when applied to a type, must be a valid JavaScript identifier.";
				}

				if (_minimizeNames && !IsPublic(typeDefinition) && GetAttributePositionalArgs(typeDefinition, "PreserveNameAttribute") == null) {
					nmspace = DetermineNamespace(typeDefinition);
					int index = _typeNames.Values.Select(tn => SplitName(tn)).Count(tn => tn.Item1 == nmspace && tn.Item2.StartsWith("$"));
					typeName = "$" + index.ToString(CultureInfo.InvariantCulture);
				}
				else {
					typeName = GetDefaultTypeName(typeDefinition);
					if (typeDefinition.DeclaringTypeDefinition != null) {
						if (GetAttributePositionalArgs(typeDefinition, "IgnoreNamespaceAttribute") != null || GetAttributePositionalArgs(typeDefinition, "ScriptNamespaceAttribute") != null) {
							_errors[typeDefinition.FullName + ":Namespace"] = "[IgnoreNamespace] or [ScriptNamespace] cannot be specified for the nested type " + typeDefinition.FullName + ".";
						}

						typeName = GetTypeName(typeDefinition.DeclaringTypeDefinition) + "$" + typeName;
						nmspace = "";
					}
					else {
						nmspace = DetermineNamespace(typeDefinition);
					}
				}
			}

			return !string.IsNullOrEmpty(nmspace) ? nmspace + "." + typeName : typeName;
		}

		private bool IsPublic(ITypeDefinition type) {
            // A type is public if the type and all its declaring types are public or protected (or protected internal).
            while (type != null) {
                bool isPublic = (type.Accessibility == Accessibility.Public || type.Accessibility == Accessibility.Protected || type.Accessibility == Accessibility.ProtectedOrInternal);
                if (!isPublic)
                    return false;
                type = type.DeclaringTypeDefinition;
            }
            return true;
		}

		public bool Prepare(IEnumerable<ITypeDefinition> types, IAssembly mainAssembly, IErrorReporter errorReporter) {
			_errors = new Dictionary<string, string>();
			var l = types.ToList();
			_typeNames = new Dictionary<ITypeDefinition, string>();
			foreach (var t in l.Where(t => t.ParentAssembly == mainAssembly || IsPublic(t))) {
				_typeNames[t] = DetermineTypeName(t);
			}

			foreach (var e in _errors.Values)
				errorReporter.Error(e);
			return _errors.Count == 0;
		}

		public string GetTypeName(ITypeDefinition typeDefinition) {
			return _typeNames[typeDefinition];
		}

		public string GetTypeParameterName(ITypeParameter typeParameter) {
			throw new NotImplementedException();
		}

		public MethodScriptSemantics GetMethodImplementation(IMethod method) {
			throw new NotImplementedException();
		}

		public ConstructorScriptSemantics GetConstructorImplementation(IMethod method) {
			throw new NotImplementedException();
		}

		public PropertyScriptSemantics GetPropertyImplementation(IProperty property) {
			throw new NotImplementedException();
		}

		public string GetAutoPropertyBackingFieldName(IProperty property) {
			throw new NotImplementedException();
		}

		public FieldScriptSemantics GetFieldImplementation(IField property) {
			throw new NotImplementedException();
		}

		public EventScriptSemantics GetEventImplementation(IEvent evt) {
			throw new NotImplementedException();
		}

		public string GetAutoEventBackingFieldName(IEvent evt) {
			throw new NotImplementedException();
		}

		public string GetEnumValueName(IField value) {
			throw new NotImplementedException();
		}

		public string GetVariableName(IVariable variable, ISet<string> usedNames) {
			throw new NotImplementedException();
		}

		public string GetTemporaryVariableName(int index) {
			throw new NotImplementedException();
		}

		public string ThisAlias {
			get { throw new NotImplementedException(); }
		}
	}
}
