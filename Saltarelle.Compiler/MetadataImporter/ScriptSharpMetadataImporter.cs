using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.MetadataImporter {
	// To handle:
	// [NonScriptable] (Class | Struct | Enum | Interface | Delegate | Constructor | Method | Property)
	// [Imported] (Class | Interface | Enum | Delegate | Struct)
	// [ScriptAssembly] (Assembly) ?
	// [ScriptQualifier] (Assembly)
	// [IgnoreNamespace] (Class | Enum | Delegate | Interface | Struct)
	// [ScriptNamespaceAttribute] (Assembly | Type)
	// [Resources] (Class) ?
	// [GlobalMethods] (Class) - Needs better support in the compiler
	// [Mixin] (Class) ?
	// [NamedValues] (Enum) - Needs better support in the compiler
	// [NumericValues] (Enum)
	// [AlternateSignature] (Constructor | Method)
	// [IntrinsicProperty] (Property (/indexer))
	// [ScriptName] (Class | Interface | Struct | Enum | Field | Property | Method | Event)
	// [PreserveCase] (Method | Property | AttributeTargets.Event | Field)
	// [PreserveName] (Class | Method | AttributeTargets.Property | Event | Field)
	// [ScriptAlias] (Method | Property) = Literal code ?
	// [ScriptSkip] (Method) ?
	// Record
	// New attributes:
	// [InstanceMethodOnFirstArgument]

	public class ScriptSharpMetadataImporter : INamingConventionResolver {
		private Dictionary<ITypeDefinition, string> typeNames;

		private string GetDefaultTypeName(ITypeDefinition def) {
			int outerCount = (def.DeclaringTypeDefinition != null ? def.DeclaringTypeDefinition.TypeParameters.Count : 0);
			return def.Name + (def.TypeParameterCount != outerCount ? "$" + (def.TypeParameterCount - outerCount).ToString(CultureInfo.InvariantCulture) : "");
		}

		public string DetermineTypeName(ITypeDefinition typeDefinition) {
			var scriptNameAttr = typeDefinition.Attributes.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.ScriptNameAttribute");
			string typeName, nmspace;
			if (scriptNameAttr != null) {
				typeName = (string)scriptNameAttr.PositionalArguments[0].ConstantValue;
				nmspace = typeDefinition.Namespace;
			}
			else {
				typeName = GetDefaultTypeName(typeDefinition);
				if (typeDefinition.DeclaringTypeDefinition != null) {
					typeName = GetTypeName(typeDefinition.DeclaringTypeDefinition) + "$" + typeName;
					nmspace = "";
				}
				else {
					nmspace = typeDefinition.Namespace;
				}
			}

			return !string.IsNullOrEmpty(nmspace) ? nmspace + "." + typeName : typeName;
		}

		public void Prepare(IEnumerable<ITypeDefinition> types) {
			var l = types.ToList();
			typeNames = new Dictionary<ITypeDefinition, string>();
			foreach (var t in l) {
				typeNames[t] = DetermineTypeName(t);
			}
		}

		public string GetTypeName(ITypeDefinition typeDefinition) {
			return typeNames[typeDefinition];
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
