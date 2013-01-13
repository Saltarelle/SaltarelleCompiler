using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Decorators {
	public abstract class MetadataImporterDecoratorBase : IMetadataImporter {
		private readonly IMetadataImporter _prev;

		protected MetadataImporterDecoratorBase(IMetadataImporter prev) {
			this._prev = prev;
		}

		public void Prepare(IEnumerable<ITypeDefinition> allTypes, bool minimizeNames, IAssembly mainAssembly) {
			_prev.Prepare(allTypes, minimizeNames, mainAssembly);
		}

		public TypeScriptSemantics GetTypeSemantics(ITypeDefinition typeDefinition) {
			return _prev.GetTypeSemantics(typeDefinition);
		}

		public MethodScriptSemantics GetMethodSemantics(IMethod method) {
			return _prev.GetMethodSemantics(method);
		}

		public ConstructorScriptSemantics GetConstructorSemantics(IMethod method) {
			return _prev.GetConstructorSemantics(method);
		}

		public PropertyScriptSemantics GetPropertySemantics(IProperty property) {
			return _prev.GetPropertySemantics(property);
		}

		public DelegateScriptSemantics GetDelegateSemantics(ITypeDefinition delegateType) {
			return _prev.GetDelegateSemantics(delegateType);
		}

		public string GetAutoPropertyBackingFieldName(IProperty property) {
			return _prev.GetAutoPropertyBackingFieldName(property);
		}

		public FieldScriptSemantics GetFieldSemantics(IField field) {
			return _prev.GetFieldSemantics(field);
		}

		public EventScriptSemantics GetEventSemantics(IEvent evt) {
			return _prev.GetEventSemantics(evt);
		}

		public string GetAutoEventBackingFieldName(IEvent evt) {
			return _prev.GetAutoEventBackingFieldName(evt);
		}
	}
}
