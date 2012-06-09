using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.MetadataImporter {
	public class ScriptSharpMetadataImporter : INamingConventionResolver {
		public string GetTypeName(ITypeDefinition typeDefinition) {
			throw new NotImplementedException();
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
