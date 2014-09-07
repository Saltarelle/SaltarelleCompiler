using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Decorators {
	public abstract class MetadataImporterDecoratorBase : IMetadataImporter {
		private readonly IMetadataImporter _prev;

		protected MetadataImporterDecoratorBase(IMetadataImporter prev) {
			this._prev = prev;
		}

		public virtual void Prepare(INamedTypeSymbol type) {
			_prev.Prepare(type);
		}

		public virtual void ReserveMemberName(INamedTypeSymbol type, string name, bool isStatic) {
			_prev.ReserveMemberName(type, name, isStatic);
		}

		public virtual bool IsMemberNameAvailable(INamedTypeSymbol type, string name, bool isStatic) {
			return _prev.IsMemberNameAvailable(type, name, isStatic);
		}

		public virtual void SetMethodSemantics(IMethodSymbol method, MethodScriptSemantics semantics) {
			_prev.SetMethodSemantics(method, semantics);
		}

		public virtual void SetConstructorSemantics(IMethodSymbol method, ConstructorScriptSemantics semantics) {
			_prev.SetConstructorSemantics(method, semantics);
		}

		public virtual void SetPropertySemantics(IPropertySymbol property, PropertyScriptSemantics semantics) {
			_prev.SetPropertySemantics(property, semantics);
		}

		public virtual void SetFieldSemantics(IFieldSymbol field, FieldScriptSemantics semantics) {
			_prev.SetFieldSemantics(field, semantics);
		}

		public virtual void SetEventSemantics(IEventSymbol evt,EventScriptSemantics semantics) {
			_prev.SetEventSemantics(evt, semantics);
		}

		public virtual TypeScriptSemantics GetTypeSemantics(INamedTypeSymbol typeDefinition) {
			return _prev.GetTypeSemantics(typeDefinition);
		}

		public virtual MethodScriptSemantics GetMethodSemantics(IMethodSymbol method) {
			return _prev.GetMethodSemantics(method);
		}

		public virtual ConstructorScriptSemantics GetConstructorSemantics(IMethodSymbol method) {
			return _prev.GetConstructorSemantics(method);
		}

		public virtual PropertyScriptSemantics GetPropertySemantics(IPropertySymbol property) {
			return _prev.GetPropertySemantics(property);
		}

		public virtual DelegateScriptSemantics GetDelegateSemantics(INamedTypeSymbol delegateType) {
			return _prev.GetDelegateSemantics(delegateType);
		}

		public virtual string GetAutoPropertyBackingFieldName(IPropertySymbol property) {
			return _prev.GetAutoPropertyBackingFieldName(property);
		}

		public virtual bool ShouldGenerateAutoPropertyBackingField(IPropertySymbol property) {
			return _prev.ShouldGenerateAutoPropertyBackingField(property);
		}

		public virtual FieldScriptSemantics GetFieldSemantics(IFieldSymbol field) {
			return _prev.GetFieldSemantics(field);
		}

		public virtual EventScriptSemantics GetEventSemantics(IEventSymbol evt) {
			return _prev.GetEventSemantics(evt);
		}

		public virtual string GetAutoEventBackingFieldName(IEventSymbol evt) {
			return _prev.GetAutoEventBackingFieldName(evt);
		}

		public virtual bool ShouldGenerateAutoEventBackingField(IEventSymbol evt) {
			return _prev.ShouldGenerateAutoEventBackingField(evt);
		}

		public IReadOnlyList<string> GetUsedInstanceMemberNames(INamedTypeSymbol type) {
			return _prev.GetUsedInstanceMemberNames(type);
		}
	}
}
