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

		public virtual void Prepare(ITypeDefinition type) {
			_prev.Prepare(type);
		}

		public virtual void ReserveMemberName(ITypeDefinition type, string name, bool isStatic) {
			_prev.ReserveMemberName(type, name, isStatic);
		}

		public virtual bool IsMemberNameAvailable(ITypeDefinition type, string name, bool isStatic) {
			return _prev.IsMemberNameAvailable(type, name, isStatic);
		}

		public virtual void SetMethodSemantics(IMethod method, MethodScriptSemantics semantics) {
			_prev.SetMethodSemantics(method, semantics);
		}

		public virtual void SetConstructorSemantics(IMethod method, ConstructorScriptSemantics semantics) {
			_prev.SetConstructorSemantics(method, semantics);
		}

		public virtual void SetPropertySemantics(IProperty property, PropertyScriptSemantics semantics) {
			_prev.SetPropertySemantics(property, semantics);
		}

		public virtual void SetFieldSemantics(IField field, FieldScriptSemantics semantics) {
			_prev.SetFieldSemantics(field, semantics);
		}

		public virtual void SetEventSemantics(IEvent evt,EventScriptSemantics semantics) {
			_prev.SetEventSemantics(evt, semantics);
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
