using System;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler.Expressions {
	partial class ExpressionCompiler {
		private JsExpression HandleMemberRead(Func<bool, JsExpression> getTarget, ISymbol member, bool isNonVirtualAccess, bool targetIsReadOnlyField) {
			if (member is IPropertySymbol) {
				var property = (IPropertySymbol)member;
				var impl = _metadataImporter.GetPropertySemantics(property.OriginalDefinition);
				switch (impl.Type) {
					case PropertyScriptSemantics.ImplType.GetAndSetMethods: {
						var getter = property.GetMethod;
						return CompileNonExtensionMethodInvocationWithSemantics(impl.GetMethod, getter, getTarget, targetIsReadOnlyField, ArgumentMap.Empty, isNonVirtualAccess);	// We know we have no arguments because indexers are treated as invocations.
					}
					case PropertyScriptSemantics.ImplType.Field: {
						if (isNonVirtualAccess) {
							return MaybeCloneValueType(_runtimeLibrary.GetBasePropertyValue(property, getTarget(false), this), property.Type, forceClone: true);
						}
						else {
							return JsExpression.Member(member.IsStatic ? InstantiateType(member.ContainingType) : getTarget(false), impl.FieldName);
						}
					}
					default: {
						_errorReporter.Message(Messages._7512, member.FullyQualifiedName());
						return JsExpression.Null;
					}
				}
			}
			else if (member is IFieldSymbol) {
				var impl = _metadataImporter.GetFieldSemantics((IFieldSymbol)member.OriginalDefinition);
				switch (impl.Type) {
					case FieldScriptSemantics.ImplType.Field:
						return JsExpression.Member(member.IsStatic ? InstantiateType(member.ContainingType) : getTarget(false), impl.Name);
					case FieldScriptSemantics.ImplType.Constant:
						return JSModel.Utils.MakeConstantExpression(impl.Value);
					default:
						_errorReporter.Message(Messages._7509, member.FullyQualifiedName());
						return JsExpression.Null;
				}
			}
			else if (member is IEventSymbol) {
				var impl = _metadataImporter.GetEventSemantics((IEventSymbol)member.OriginalDefinition);
				if (impl.Type == EventScriptSemantics.ImplType.NotUsableFromScript) {
					_errorReporter.Message(Messages._7511, member.FullyQualifiedName());
					return JsExpression.Null;
				}

				var fname = _metadataImporter.GetAutoEventBackingFieldName((IEventSymbol)member.OriginalDefinition);
				return JsExpression.Member(member.IsStatic ? InstantiateType(member.ContainingType) : getTarget(false), fname);
			}
			else if (member is IMethodSymbol) {
				var impl = _metadataImporter.GetMethodSemantics((IMethodSymbol)member.OriginalDefinition);
				if (impl.Type == MethodScriptSemantics.ImplType.NotUsableFromScript) {
					_errorReporter.Message(Messages._7511, member.FullyQualifiedName());
					return JsExpression.Null;
				}

				return JsExpression.Member(member.IsStatic ? InstantiateType(member.ContainingType) : getTarget(false), impl.Name);
			}
			else {
				_errorReporter.InternalError("Invalid member " + member);
				return JsExpression.Null;
			}
		}
	}
}
