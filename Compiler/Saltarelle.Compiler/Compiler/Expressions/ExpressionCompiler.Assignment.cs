using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler.Expressions {
	partial class ExpressionCompiler {
		private class DummyRuntimeContext : IRuntimeContext {
			public JsExpression ResolveTypeParameter(ITypeParameterSymbol tp) {
				return JsExpression.Null;
			}

			public JsExpression EnsureCanBeEvaluatedMultipleTimes(JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore) {
				return JsExpression.Null;
			}

			public static readonly IRuntimeContext Instance = new DummyRuntimeContext();
		}

		private static readonly JsExpression _dummyExpression = JsExpression.Identifier("x");
		private bool TypeNeedsClip(ITypeSymbol type) {
			return _runtimeLibrary.ClipInteger(_dummyExpression, type, false, DummyRuntimeContext.Instance) != _dummyExpression;
		}

		private JsExpression CompileCompoundFieldAssignment(Func<bool, JsExpression> getTarget, ITypeSymbol type, ISymbol member, ArgumentForCall? otherOperand, string fieldName, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool returnValueBeforeChange) {
			var target = member != null && member.IsStatic ? InstantiateType(member.ContainingType) : getTarget(compoundFactory == null);
			var jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand.Value, false, ref target) : null);
			var access = JsExpression.Member(target, fieldName);
			if (compoundFactory != null) {
				if (returnValueIsImportant && IsMutableValueType(type)) {
					_additionalStatements.Add(JsExpression.Assign(access, MaybeCloneValueType(valueFactory(target, jsOtherOperand), otherOperand, type)));
					return access;
				}
				else {
					return compoundFactory(access, otherOperand != null ? MaybeCloneValueType(jsOtherOperand, otherOperand, type) : null);
				}
			}
			else {
				if (returnValueIsImportant && returnValueBeforeChange) {
					var temp = _createTemporaryVariable();
					_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, access));
					_additionalStatements.Add(JsExpression.Assign(access, MaybeCloneValueType(valueFactory(JsExpression.Identifier(_variables[temp].Name), jsOtherOperand), otherOperand, type)));
					return JsExpression.Identifier(_variables[temp].Name);
				}
				else {
					if (returnValueIsImportant && IsMutableValueType(type)) {
						_additionalStatements.Add(JsExpression.Assign(access, MaybeCloneValueType(valueFactory(access, jsOtherOperand), otherOperand, type)));
						return access;
					}
					else {
						return JsExpression.Assign(access, MaybeCloneValueType(valueFactory(access, jsOtherOperand), otherOperand, type));
					}
				}
			}
		}

		private JsExpression CompileArrayAccessCompoundAssignment(Func<bool, JsExpression> getArray, ArgumentForCall index, ArgumentForCall? otherOperand, ITypeSymbol elementType, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool returnValueBeforeChange) {
			var expressions = new List<JsExpression>();
			expressions.Add(getArray(compoundFactory == null));
			expressions.Add(InnerCompile(index, compoundFactory == null, expressions));
			var jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand.Value, false, expressions) : null);
			var access = JsExpression.Index(expressions[0], expressions[1]);

			if (compoundFactory != null) {
				if (returnValueIsImportant && IsMutableValueType(elementType)) {
					_additionalStatements.Add(JsExpression.Assign(access, MaybeCloneValueType(valueFactory(access, jsOtherOperand), otherOperand, elementType)));
					return access;
				}
				else {
					return compoundFactory(access, MaybeCloneValueType(jsOtherOperand, otherOperand, elementType));
				}
			}
			else {
				if (returnValueIsImportant && returnValueBeforeChange) {
					var temp = _createTemporaryVariable();
					_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, access));
					_additionalStatements.Add(JsExpression.Assign(access, MaybeCloneValueType(valueFactory(JsExpression.Identifier(_variables[temp].Name), jsOtherOperand), otherOperand, elementType)));
					return JsExpression.Identifier(_variables[temp].Name);
				}
				else {
					if (returnValueIsImportant && IsMutableValueType(elementType)) {
						_additionalStatements.Add(JsExpression.Assign(access, MaybeCloneValueType(valueFactory(access, jsOtherOperand), otherOperand, elementType)));
						return access;
					}
					else {
						return JsExpression.Assign(access, MaybeCloneValueType(valueFactory(access, jsOtherOperand), otherOperand, elementType));
					}
				}
			}
		}

		private JsExpression CompileMemberAssignment(Func<bool, JsExpression> getTarget, bool isNonVirtualAccess, ITypeSymbol type, ISymbol member, ArgumentMap indexingArgumentMap, ArgumentForCall? otherOperand, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool returnValueBeforeChange, bool oldValueIsImportant) {
			if (member is IPropertySymbol) {
				var property = member as IPropertySymbol;
				var impl = _metadataImporter.GetPropertySemantics(property.OriginalDefinition);

				if (impl.Type != PropertyScriptSemantics.ImplType.GetAndSetMethods && impl.Type != PropertyScriptSemantics.ImplType.Field) {
					_errorReporter.Message(Messages._7507, property.FullyQualifiedName());
					return JsExpression.Null;
				}

				if (!isNonVirtualAccess && impl.Type == PropertyScriptSemantics.ImplType.Field) {
					return CompileCompoundFieldAssignment(getTarget, type, member, otherOperand, impl.FieldName, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
				}

				if (impl.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods && impl.SetMethod.Type == MethodScriptSemantics.ImplType.NativeIndexer) {
					if (!property.IsIndexer || property.GetMethod.Parameters.Length != 1) {
						_errorReporter.Message(Messages._7506);
						return JsExpression.Null;
					}
					return CompileArrayAccessCompoundAssignment(getTarget, indexingArgumentMap.ArgumentsForCall[0], otherOperand, property.Type, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
				}
				else {
					List<JsExpression> thisAndArguments;
					if (property.Parameters.Length > 0) {
						thisAndArguments = CompileThisAndArgumentListForMethodCall(property.SetMethod, null, getTarget(oldValueIsImportant), oldValueIsImportant, indexingArgumentMap, null);
					}
					else {
						thisAndArguments = new List<JsExpression> { member.IsStatic ? InstantiateType(member.ContainingType) : getTarget(oldValueIsImportant) };
					}
							
					JsExpression oldValue, jsOtherOperand;
					if (oldValueIsImportant) {
						if (impl.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods)
							thisAndArguments.Add(MaybeCloneValueType(CompileMethodInvocation(impl.GetMethod, property.GetMethod, thisAndArguments, isNonVirtualAccess), otherOperand, type));
						else
							thisAndArguments.Add(MaybeCloneValueType(_runtimeLibrary.GetBasePropertyValue(property, thisAndArguments[0], this), otherOperand, type));
						jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand.Value, false, thisAndArguments) : null);
						oldValue = thisAndArguments[thisAndArguments.Count - 1];
						thisAndArguments.RemoveAt(thisAndArguments.Count - 1); // Remove the current value because it should not be an argument to the setter.
					}
					else {
						jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand.Value, false, thisAndArguments) : null);
						oldValue = null;
					}
							
					if (returnValueIsImportant) {
						var valueToReturn = (returnValueBeforeChange ? oldValue : valueFactory(oldValue, jsOtherOperand));
						if (IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(valueToReturn)) {
							// Must be a simple assignment, if we got the value from a getter we would already have created a temporary for it.
							CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(thisAndArguments, valueToReturn);
							var temp = _createTemporaryVariable();
							_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, valueToReturn));
							valueToReturn = JsExpression.Identifier(_variables[temp].Name);
						}
							
						var newValue = (returnValueBeforeChange ? valueFactory(valueToReturn, jsOtherOperand) : valueToReturn);
							
						thisAndArguments.Add(MaybeCloneValueType(newValue, otherOperand, type, forceClone: true));
						if (impl.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods)
							_additionalStatements.Add(CompileMethodInvocation(impl.SetMethod, property.SetMethod, thisAndArguments, isNonVirtualAccess));
						else
							_additionalStatements.Add(_runtimeLibrary.SetBasePropertyValue(property, thisAndArguments[0], thisAndArguments[1], this));
						return valueToReturn;
					}
					else {
						thisAndArguments.Add(MaybeCloneValueType(valueFactory(oldValue, jsOtherOperand), otherOperand, type));
						if (impl.Type == PropertyScriptSemantics.ImplType.GetAndSetMethods)
							return CompileMethodInvocation(impl.SetMethod, property.SetMethod, thisAndArguments, isNonVirtualAccess);
						else
							return _runtimeLibrary.SetBasePropertyValue(property, thisAndArguments[0], thisAndArguments[1], this);
					}
				}
			}
			else if (member is IFieldSymbol) {
				var field = (IFieldSymbol)member;
				var impl = _metadataImporter.GetFieldSemantics(field.OriginalDefinition);
				switch (impl.Type) {
					case FieldScriptSemantics.ImplType.Field:
						return CompileCompoundFieldAssignment(getTarget, type, member, otherOperand, impl.Name, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
					case FieldScriptSemantics.ImplType.Constant:
						_errorReporter.Message(Messages._7508, field.FullyQualifiedName());
						return JsExpression.Null;
					default:
						_errorReporter.Message(Messages._7509, field.FullyQualifiedName());
						return JsExpression.Null;
				}
			}
			else if (member is IEventSymbol) {
				var evt = (IEventSymbol)member;
				var evtField = _metadataImporter.GetAutoEventBackingFieldName(evt);
				return CompileCompoundFieldAssignment(getTarget, type, member, otherOperand, evtField, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
			}
			else {
				_errorReporter.InternalError("Target " + member.FullyQualifiedName() + " of compound assignment is neither a property nor a field nor an event.");
				return JsExpression.Null;
			}
		}

		private JsExpression CompileLateBoundIndexerAssignmentWithCandidateSymbols(Func<bool, JsExpression> getTarget, bool isNonVirtualAccess, ITypeSymbol type, ImmutableArray<ISymbol> candidateMembers, IReadOnlyCollection<ArgumentSyntax> arguments, ArgumentForCall? otherOperand, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool returnValueBeforeChange, bool oldValueIsImportant) {
			// We need to validate getters as well as setters because we don't know here whether it is a compound assignment

			var allSemantics = candidateMembers.Select(m => _metadataImporter.GetPropertySemantics((IPropertySymbol)m.OriginalDefinition)).ToList();
			if (allSemantics.Any(s => s.Type != PropertyScriptSemantics.ImplType.GetAndSetMethods)) {
				return JsExpression.Null;
			}

			var getSemantics = allSemantics.Select(s => s.GetMethod).FirstOrDefault(m => m != null);
			var setSemantics = allSemantics.Select(s => s.SetMethod).FirstOrDefault(m => m != null);

			if (getSemantics != null) {
				if (allSemantics.Any(s => s.GetMethod != null && s.GetMethod.Type != getSemantics.Type)) {
					_errorReporter.Message(Messages._7532);
					return JsExpression.Null;
				}

				switch (getSemantics.Type) {
					case MethodScriptSemantics.ImplType.NormalMethod:
						if (allSemantics.Any(s => s.GetMethod != null && s.GetMethod.Name != getSemantics.Name)) {
							_errorReporter.Message(Messages._7532);
							return JsExpression.Null;
						}
						break;

					case MethodScriptSemantics.ImplType.NativeIndexer:
						break;

					default:
						_errorReporter.Message(Messages._7532);
						return JsExpression.Null;
					
				}
			}

			if (setSemantics != null) {
				if (allSemantics.Any(s => s.SetMethod != null && s.SetMethod.Type != setSemantics.Type)) {
					_errorReporter.Message(Messages._7532);
					return JsExpression.Null;
				}

				switch (setSemantics.Type) {
					case MethodScriptSemantics.ImplType.NormalMethod:
						if (allSemantics.Any(s => s.SetMethod != null && s.SetMethod.Name != setSemantics.Name)) {
							_errorReporter.Message(Messages._7532);
							return JsExpression.Null;
						}
						break;

					case MethodScriptSemantics.ImplType.NativeIndexer:
						break;

					default:
						_errorReporter.Message(Messages._7532);
						return JsExpression.Null;
					
				}
			}

			foreach (var arg in arguments) {
				if (arg.NameColon != null) {
					_errorReporter.Message(Messages._7526);
					return JsExpression.Null;
				}
			}

			return CompileMemberAssignment(getTarget, isNonVirtualAccess, type, candidateMembers[0], ArgumentMap.CreateIdentity(arguments.Select(a => a.Expression)), otherOperand, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange, oldValueIsImportant);
		}

		private JsExpression CompileCompoundAssignment(ExpressionSyntax target, SyntaxKind op, ArgumentForCall? otherOperand, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool isLifted, bool returnValueBeforeChange = false, bool oldValueIsImportant = true) {
			var type = _semanticModel.GetTypeInfo(target).Type;

			if (isLifted) {
				compoundFactory = null;
				var old         = valueFactory;
				valueFactory    = (a, b) => _runtimeLibrary.Lift(old(a, b), this);
			}

			var underlyingType = type.UnpackEnum();

			var specialType = underlyingType.UnpackNullable().SpecialType;
			bool isBitwiseOperator = (op == SyntaxKind.LeftShiftAssignmentExpression || op == SyntaxKind.RightShiftAssignmentExpression || op == SyntaxKind.AndAssignmentExpression || op == SyntaxKind.OrAssignmentExpression || op == SyntaxKind.ExclusiveOrAssignmentExpression);
			if (op != SyntaxKind.SimpleAssignmentExpression && IsIntegerType(underlyingType)) {
				if (   (isBitwiseOperator && specialType == SpecialType.System_Int32)
				    || (op == SyntaxKind.RightShiftAssignmentExpression && specialType == SpecialType.System_UInt32)
				    || ((op == SyntaxKind.PreIncrementExpression || op == SyntaxKind.PostIncrementExpression) && specialType == SpecialType.System_UInt64)
				    || ((op == SyntaxKind.PreIncrementExpression || op == SyntaxKind.PostIncrementExpression || op == SyntaxKind.PreDecrementExpression || op == SyntaxKind.PostDecrementExpression) && specialType == SpecialType.System_Int64))
				{
					// Don't need to check even in checked context and don't need to clip
				}
				else if (isBitwiseOperator) {
					// Always clip, never check
					compoundFactory = null;
					var old = valueFactory;
					valueFactory = (a, b) => _runtimeLibrary.ClipInteger(old(a, b), underlyingType, false, this);
				}
				else if (_semanticModel.IsInCheckedContext(target)) {
					compoundFactory = null;
					var old = valueFactory;
					valueFactory = (a, b) => _runtimeLibrary.CheckInteger(old(a, b), underlyingType, this);
				}
				else if (TypeNeedsClip(underlyingType)) {
					compoundFactory = null;
					var old = valueFactory;
					valueFactory = (a, b) => _runtimeLibrary.ClipInteger(old(a, b), underlyingType, false, this);
				}
			}

			var targetSymbol = _semanticModel.GetSymbolInfo(target);
			var targetType = _semanticModel.GetTypeInfo(target).Type;

			if (target is IdentifierNameSyntax) {
				if (targetSymbol.Symbol is ILocalSymbol || targetSymbol.Symbol is IParameterSymbol) {
					JsExpression jsTarget, jsOtherOperand;
					jsTarget = InnerCompile(target, compoundFactory == null, returnMultidimArrayValueByReference: true);
					jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand.Value, false) : null);	// If the variable is a by-ref variable we will get invalid reordering if we force the target to be evaluated before the other operand.

					if (compoundFactory != null) {
						if (returnValueIsImportant && IsMutableValueType(targetType)) {
							_additionalStatements.Add(JsExpression.Assign(jsTarget, MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, targetType)));
							return jsTarget;
						}
						else {
							return compoundFactory(jsTarget, MaybeCloneValueType(jsOtherOperand, otherOperand, targetType));
						}
					}
					else {
						if (returnValueIsImportant && returnValueBeforeChange) {
							var temp = _createTemporaryVariable();
							_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, jsTarget));
							_additionalStatements.Add(JsExpression.Assign(jsTarget, valueFactory(JsExpression.Identifier(_variables[temp].Name), jsOtherOperand)));
							return JsExpression.Identifier(_variables[temp].Name);
						}
						else {
							if (returnValueIsImportant && IsMutableValueType(targetType)) {
								_additionalStatements.Add(JsExpression.Assign(jsTarget, MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, targetType)));
								return jsTarget;
							}
							else {
								return JsExpression.Assign(jsTarget, MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, targetType));
							}
						}
					}
				}
				else if (targetSymbol.Symbol is IPropertySymbol || targetSymbol.Symbol is IFieldSymbol || targetSymbol.Symbol is IEventSymbol) {
					return CompileMemberAssignment(usedMultipleTimes => CompileThis(), false, targetType, targetSymbol.Symbol, null, otherOperand, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange, oldValueIsImportant);
				}
				else {
					_errorReporter.InternalError("Unexpected symbol for " + target);
					return JsExpression.Null;
				}
			}
			else if (target is MemberAccessExpressionSyntax) {
				var mae = (MemberAccessExpressionSyntax)target;
				if (_semanticModel.GetTypeInfo(mae.Expression).ConvertedType.TypeKind == TypeKind.Dynamic) {
					return CompileCompoundFieldAssignment(usedMultipleTimes => InnerCompile(mae.Expression, usedMultipleTimes), otherOperand != null && otherOperand.Value.Argument != null ? _semanticModel.GetTypeInfo(otherOperand.Value.Argument).Type : _compilation.DynamicType, null, otherOperand, mae.Name.Identifier.Text, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
				}
				else {
					return CompileMemberAssignment(usedMultipleTimes => InnerCompile(mae.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), mae.IsNonVirtualAccess(), targetType, targetSymbol.Symbol, null, otherOperand, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange, oldValueIsImportant);
				}
			}
			else if (target is ElementAccessExpressionSyntax) {
				var eae = (ElementAccessExpressionSyntax)target;

				if (targetSymbol.Symbol == null && targetSymbol.CandidateReason == CandidateReason.LateBound) {
					if (targetSymbol.CandidateSymbols.Length > 0) {
						return CompileLateBoundIndexerAssignmentWithCandidateSymbols(usedMultipleTimes => InnerCompile(eae.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), eae.IsNonVirtualAccess(), targetType, targetSymbol.CandidateSymbols, eae.ArgumentList.Arguments, otherOperand, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange, oldValueIsImportant);
					}
					else {
						if (eae.ArgumentList.Arguments.Count > 1) {
							_errorReporter.Message(Messages._7528);
							return JsExpression.Null;
						}

						return CompileArrayAccessCompoundAssignment(usedMultipleTimes => InnerCompile(eae.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), new ArgumentForCall(eae.ArgumentList.Arguments[0].Expression), otherOperand, targetType, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
					}
				}
				else if (targetSymbol.Symbol is IPropertySymbol) {
					return CompileMemberAssignment(usedMultipleTimes => InnerCompile(eae.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), eae.IsNonVirtualAccess(), targetType, targetSymbol.Symbol, _semanticModel.GetArgumentMap(eae), otherOperand, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange, oldValueIsImportant);
				}
				else if (eae.ArgumentList.Arguments.Count == 1) {
					return CompileArrayAccessCompoundAssignment(usedMultipleTimes => InnerCompile(eae.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), new ArgumentForCall(eae.ArgumentList.Arguments[0].Expression), otherOperand, targetType, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
				}
				else {
					var expressions = new List<JsExpression>();
					expressions.Add(InnerCompile(eae.Expression, oldValueIsImportant, returnMultidimArrayValueByReference: true));
					foreach (var argument in eae.ArgumentList.Arguments)
						expressions.Add(InnerCompile(argument.Expression, oldValueIsImportant, expressions));

					JsExpression oldValue, jsOtherOperand;
					if (oldValueIsImportant) {
						expressions.Add(_runtimeLibrary.GetMultiDimensionalArrayValue(expressions[0], expressions.Skip(1), this));
						jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand.Value, false, expressions) : null);
						oldValue = expressions[expressions.Count - 1];
						expressions.RemoveAt(expressions.Count - 1); // Remove the current value because it should not be an argument to the setter.
					}
					else {
						jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand.Value, false, expressions) : null);
						oldValue = null;
					}

					if (returnValueIsImportant) {
						var valueToReturn = (returnValueBeforeChange ? oldValue : valueFactory(oldValue, jsOtherOperand));
						if (IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(valueToReturn)) {
							// Must be a simple assignment, if we got the value from a getter we would already have created a temporary for it.
							CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(expressions, valueToReturn);
							var temp = _createTemporaryVariable();
							_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, valueToReturn));
							valueToReturn = JsExpression.Identifier(_variables[temp].Name);
						}

						var newValue = MaybeCloneValueType(returnValueBeforeChange ? valueFactory(valueToReturn, jsOtherOperand) : valueToReturn, otherOperand, targetType);

						_additionalStatements.Add(_runtimeLibrary.SetMultiDimensionalArrayValue(expressions[0], expressions.Skip(1), newValue, this));
						return valueToReturn;
					}
					else {
						return _runtimeLibrary.SetMultiDimensionalArrayValue(expressions[0], expressions.Skip(1), MaybeCloneValueType(valueFactory(oldValue, jsOtherOperand), otherOperand, targetType), this);
					}
				}
			}
			else if (target is InstanceExpressionSyntax) {
				var jsTarget = CompileThis();
				var jsOtherOperand = otherOperand != null ? InnerCompile(otherOperand.Value, false) : null;

				var containingMethod = GetContainingMethod(target);
				if (containingMethod != null && containingMethod.MethodKind != MethodKind.Constructor) {
					var typesem = _metadataImporter.GetTypeSemantics((INamedTypeSymbol)targetType.OriginalDefinition);
					if (typesem.Type != TypeScriptSemantics.ImplType.MutableValueType) {
						_errorReporter.Message(Messages._7538);
						return JsExpression.Null;
					}
				}

				if (compoundFactory != null) {
					if (returnValueIsImportant) {
						_additionalStatements.Add(_runtimeLibrary.ShallowCopy(MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, targetType), jsTarget, this));
						return jsTarget;
					}
					else {
						return _runtimeLibrary.ShallowCopy(MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, targetType), jsTarget, this);
					}
				}
				else {
					if (returnValueIsImportant && returnValueBeforeChange) {
						var temp = _createTemporaryVariable();
						_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, MaybeCloneValueType(jsTarget, targetType, forceClone: true)));
						_additionalStatements.Add(_runtimeLibrary.ShallowCopy(valueFactory(JsExpression.Identifier(_variables[temp].Name), jsOtherOperand), jsTarget, this));
						return JsExpression.Identifier(_variables[temp].Name);
					}
					else {
						if (returnValueIsImportant) {
							_additionalStatements.Add(_runtimeLibrary.ShallowCopy(MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, targetType), jsTarget, this));
							return jsTarget;
						}
						else {
							return _runtimeLibrary.ShallowCopy(MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, targetType), jsTarget, this);
						}
					}
				}
			}
			else {
				_errorReporter.InternalError("Unsupported target of assignment: " + target);
				return JsExpression.Null;
			}
		}
	}
}
