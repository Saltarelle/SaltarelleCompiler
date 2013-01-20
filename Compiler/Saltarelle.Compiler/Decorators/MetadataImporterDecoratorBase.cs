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

		public virtual void Prepare(IEnumerable<ITypeDefinition> allTypes, bool minimizeNames, IAssembly mainAssembly) {
			_prev.Prepare(allTypes, minimizeNames, mainAssembly);
		}

		public virtual TypeScriptSemantics GetTypeSemantics(ITypeDefinition typeDefinition) {
			return _prev.GetTypeSemantics(typeDefinition);
		}

		public virtual MethodScriptSemantics GetMethodSemantics(IMethod method) {
			return _prev.GetMethodSemantics(method);
		}

		public virtual ConstructorScriptSemantics GetConstructorSemantics(IMethod method) {
			return _prev.GetConstructorSemantics(method);
		}

		public virtual PropertyScriptSemantics GetPropertySemantics(IProperty property) {
			return _prev.GetPropertySemantics(property);
		}

		public virtual DelegateScriptSemantics GetDelegateSemantics(ITypeDefinition delegateType) {
			return _prev.GetDelegateSemantics(delegateType);
		}

		public virtual string GetAutoPropertyBackingFieldName(IProperty property) {
			return _prev.GetAutoPropertyBackingFieldName(property);
		}

		public virtual FieldScriptSemantics GetFieldSemantics(IField field) {
			return _prev.GetFieldSemantics(field);
		}

		public virtual EventScriptSemantics GetEventSemantics(IEvent evt) {
			return _prev.GetEventSemantics(evt);
		}

		public virtual string GetAutoEventBackingFieldName(IEvent evt) {
			return _prev.GetAutoEventBackingFieldName(evt);
		}
	}
}
