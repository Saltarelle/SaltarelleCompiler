using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.StateMachineRewrite;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler.Compiler {
	public class NestedFunctionContext {
		public ReadOnlySet<ISymbol> CapturedByRefVariables { get; private set; }

		public NestedFunctionContext(IEnumerable<ISymbol> capturedByRefVariables) {
			var crv = new HashSet<ISymbol>();
			foreach (var v in capturedByRefVariables)
				crv.Add(v);

			CapturedByRefVariables = new ReadOnlySet<ISymbol>(crv);
		}
	}

	public class ExpressionCompiler : CSharpSyntaxVisitor<JsExpression>, IRuntimeContext {
		private readonly SemanticModel _semanticModel;
		private readonly IMetadataImporter _metadataImporter;
		private readonly INamer _namer;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly IErrorReporter _errorReporter;
		private readonly IDictionary<ISymbol, VariableData> _variables;
		private readonly IDictionary<SyntaxNode, NestedFunctionData> _nestedFunctions;
		private readonly Func<ILocalSymbol> _createTemporaryVariable;
		private readonly Func<NestedFunctionContext, StatementCompiler> _createInnerCompiler;
		private readonly string _thisAlias;
		private readonly NestedFunctionContext _nestedFunctionContext;
		private readonly IMethodSymbol _methodBeingCompiled;
		private readonly INamedTypeSymbol _typeBeingCompiled;
		private readonly bool _returnMultidimArrayValueByReference;
		private bool _returnValueIsImportant;

		public ExpressionCompiler(SemanticModel semanticModel, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, IDictionary<ISymbol, VariableData> variables, IDictionary<SyntaxNode, NestedFunctionData> nestedFunctions, Func<ILocalSymbol> createTemporaryVariable, Func<NestedFunctionContext, StatementCompiler> createInnerCompiler, string thisAlias, NestedFunctionContext nestedFunctionContext, IMethodSymbol methodBeingCompiled, INamedTypeSymbol typeBeingCompiled, bool returnMultidimArrayValueByReference = false) {
			Require.ValidJavaScriptIdentifier(thisAlias, "thisAlias", allowNull: true);

			_semanticModel = semanticModel;
			_metadataImporter = metadataImporter;
			_namer = namer;
			_runtimeLibrary = runtimeLibrary;
			_errorReporter = errorReporter;
			_variables = variables;
			_nestedFunctions = nestedFunctions;
			_createTemporaryVariable = createTemporaryVariable;
			_createInnerCompiler = createInnerCompiler;
			_thisAlias = thisAlias;
			_nestedFunctionContext = nestedFunctionContext;
			_methodBeingCompiled = methodBeingCompiled;
			_typeBeingCompiled = typeBeingCompiled;
			_returnMultidimArrayValueByReference = returnMultidimArrayValueByReference;
		}

		private List<JsStatement> _additionalStatements;

		public ExpressionCompileResult Compile(ExpressionSyntax expression, bool returnValueIsImportant) {
			_additionalStatements = new List<JsStatement>();
			var result = Visit(expression, returnValueIsImportant);
			return new ExpressionCompileResult(result, _additionalStatements);
		}

		private JsExpression ProcessConversion(JsExpression js, ExpressionSyntax cs) {
			var typeInfo = _semanticModel.GetTypeInfo(cs);
			var conversion = _semanticModel.GetConversion(cs);
			return PerformConversion(js, conversion, typeInfo.Type, typeInfo.ConvertedType, cs);
		}

		public IList<JsStatement> CompileConstructorInitializer(IMethodSymbol method, IList<ExpressionSyntax> argumentsForCall, IList<int> argumentToParameterMap, IList<ExpressionSyntax> initializerStatements, bool currentIsStaticMethod) {
			return new JsStatement[0];
			//var impl = _metadataImporter.GetConstructorSemantics(method);
			//if (impl.SkipInInitializer) {
			//	if (currentIsStaticMethod)
			//		return new[] { JsStatement.Var(_thisAlias, JsExpression.ObjectLiteral()) };
			//	else
			//		return new JsStatement[0];
			//}
			//
			//_additionalStatements = new List<JsStatement>();
			//
			//if (currentIsStaticMethod) {
			//	_additionalStatements.Add(JsStatement.Var(_thisAlias, CompileConstructorInvocation(impl, method, argumentsForCall, argumentToParameterMap, initializerStatements)));
			//}
			//else if (impl.Type == ConstructorScriptSemantics.ImplType.Json) {
			//	_additionalStatements.Add(_runtimeLibrary.ShallowCopy(CompileJsonConstructorCall(method, impl, argumentsForCall, argumentToParameterMap, initializerStatements), CompileThis(), this));
			//}
			//else {
			//	string literalCode   = GetActualInlineCode(impl, argumentsForCall.Count > 0 && argumentsForCall[argumentsForCall.Count - 1] is ArrayCreateResolveResult);
			//	var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, literalCode, _runtimeLibrary.InstantiateType(method.ContainingType, this), false, argumentsForCall, argumentToParameterMap);
			//	var jsType           = thisAndArguments[0];
			//	thisAndArguments[0]  = CompileThis();	// Swap out the TypeResolveResult that we get as default.
			//
			//	switch (impl.Type) {
			//		case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
			//			_additionalStatements.Add(CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, jsType, impl.ExpandParams, true));
			//			break;
			//
			//		case ConstructorScriptSemantics.ImplType.NamedConstructor:
			//			_additionalStatements.Add(CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, JsExpression.Member(jsType, impl.Name), impl.ExpandParams, true));
			//			break;
			//
			//		case ConstructorScriptSemantics.ImplType.StaticMethod:
			//			_additionalStatements.Add(_runtimeLibrary.ShallowCopy(CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, JsExpression.Member(jsType, impl.Name), impl.ExpandParams, false), thisAndArguments[0], this));
			//			break;
			//
			//		case ConstructorScriptSemantics.ImplType.InlineCode:
			//			_additionalStatements.Add(_runtimeLibrary.ShallowCopy(CompileInlineCodeMethodInvocation(method, literalCode, null, thisAndArguments.Skip(1).ToList()), thisAndArguments[0], this));
			//			break;
			//
			//		default:
			//			_errorReporter.Message(Messages._7505);
			//			break;
			//	}
			//}
			//
			//var result = _additionalStatements;
			//_additionalStatements = null;	// Just so noone else messes with it by accident (shouldn't happen).
			//return result;
		}

		private ExpressionCompiler Clone(NestedFunctionContext nestedFunctionContext = null, bool returnMultidimArrayValueByReference = false) {
			return new ExpressionCompiler(_semanticModel, _metadataImporter, _namer, _runtimeLibrary, _errorReporter, _variables, _nestedFunctions, _createTemporaryVariable, _createInnerCompiler, _thisAlias, nestedFunctionContext ?? _nestedFunctionContext, _methodBeingCompiled, _typeBeingCompiled, returnMultidimArrayValueByReference);
		}

		private ExpressionCompileResult CloneAndCompile(ExpressionSyntax expression, bool returnValueIsImportant, NestedFunctionContext nestedFunctionContext = null, bool returnMultidimArrayValueByReference = false) {
			return Clone(nestedFunctionContext, returnMultidimArrayValueByReference).Compile(expression, returnValueIsImportant);
		}

		private ExpressionCompileResult CloneAndCompile(ArgumentForCall argument, bool returnValueIsImportant, NestedFunctionContext nestedFunctionContext = null, bool returnMultidimArrayValueByReference = false) {
			if (argument.Argument != null)
				return Clone(nestedFunctionContext, returnMultidimArrayValueByReference).Compile(argument.Argument, returnValueIsImportant);
			else if (argument.ParamArray != null) {
				var expressions = new List<JsExpression>();
				var additionalStatements = new List<JsStatement>();
				foreach (var init in argument.ParamArray.Item2) {
					var innerResult = CloneAndCompile(init, true);
					additionalStatements.AddRange(innerResult.AdditionalStatements);
					expressions.Add(MaybeCloneValueType(innerResult.Expression, init, argument.ParamArray.Item1));
				}
				return new ExpressionCompileResult(JsExpression.ArrayLiteral(expressions), additionalStatements);
			}
			else
				return new ExpressionCompileResult(JSModel.Utils.MakeConstantExpression(argument.DefaultValue), ImmutableArray<JsStatement>.Empty);
		}

		private void CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(IList<JsExpression> expressions, ExpressionCompileResult newExpressions) {
			Utils.CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(_additionalStatements, expressions, newExpressions, () => { var temp = _createTemporaryVariable(); return _variables[temp].Name; });
		}

		private void CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(IList<JsExpression> expressions, JsExpression newExpression) {
			CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(expressions, new ExpressionCompileResult(newExpression, new JsStatement[0]));
		}

		private JsExpression InnerCompile(ArgumentForCall argument, bool usedMultipleTimes, IList<JsExpression> expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression, bool returnMultidimArrayValueByReference = false) {
			var result = CloneAndCompile(argument, returnValueIsImportant: true, returnMultidimArrayValueByReference: returnMultidimArrayValueByReference);

			bool needsTemporary = usedMultipleTimes && IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(result.Expression);
			if (result.AdditionalStatements.Count > 0 || needsTemporary) {
				CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression, result);
			}

			_additionalStatements.AddRange(result.AdditionalStatements);

			if (needsTemporary) {
				var temp = _createTemporaryVariable();
				_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, result.Expression));
				return JsExpression.Identifier(_variables[temp].Name);
			}
			else {
				return result.Expression;
			}
		}

		private JsExpression InnerCompile(ArgumentForCall argument, bool usedMultipleTimes, ref JsExpression expressionThatHasToBeEvaluatedInOrderBeforeThisExpression, bool returnMultidimArrayValueByReference = false) {
			var l = new List<JsExpression>();
			if (expressionThatHasToBeEvaluatedInOrderBeforeThisExpression != null)
				l.Add(expressionThatHasToBeEvaluatedInOrderBeforeThisExpression);
			var r = InnerCompile(argument, usedMultipleTimes, l, returnMultidimArrayValueByReference);
			if (l.Count > 0)
				expressionThatHasToBeEvaluatedInOrderBeforeThisExpression = l[0];
			return r;
		}

		private JsExpression InnerCompile(ArgumentForCall argument, bool usedMultipleTimes, bool returnMultidimArrayValueByReference = false) {
			JsExpression _ = null;
			return InnerCompile(argument, usedMultipleTimes, ref _, returnMultidimArrayValueByReference);
		}

		private JsExpression InnerCompile(ExpressionSyntax node, bool usedMultipleTimes, IList<JsExpression> expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression, bool returnMultidimArrayValueByReference = false) {
			return InnerCompile(new ArgumentForCall(node), usedMultipleTimes, expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression, returnMultidimArrayValueByReference);
		}

		private JsExpression InnerCompile(ExpressionSyntax node, bool usedMultipleTimes, ref JsExpression expressionThatHasToBeEvaluatedInOrderBeforeThisExpression, bool returnMultidimArrayValueByReference = false) {
			return InnerCompile(new ArgumentForCall(node), usedMultipleTimes, ref expressionThatHasToBeEvaluatedInOrderBeforeThisExpression, returnMultidimArrayValueByReference);
		}

		private JsExpression InnerCompile(ExpressionSyntax node, bool usedMultipleTimes, bool returnMultidimArrayValueByReference = false) {
			return InnerCompile(new ArgumentForCall(node), usedMultipleTimes, returnMultidimArrayValueByReference);
		}

		public override JsExpression Visit(SyntaxNode node) {
			var expr = node as ExpressionSyntax;
			if (expr == null) {
				_errorReporter.InternalError("Unexpected node " + node);
				return JsExpression.Null;
			}

			var result = base.Visit(node);
			return ProcessConversion(result, expr);
		}

		private JsExpression Visit(SyntaxNode node, bool returnValueIsImportant) {
			var old = _returnValueIsImportant;
			_returnValueIsImportant = returnValueIsImportant;
			try {
				return Visit(node);
			}
			finally {
				_returnValueIsImportant = old;
			}
		}

		private bool IsIntegerType(ITypeSymbol type) {
			type = type.UnpackNullable();

			return type.SpecialType == SpecialType.System_Byte
			    || type.SpecialType == SpecialType.System_SByte
			    || type.SpecialType == SpecialType.System_Char
			    || type.SpecialType == SpecialType.System_Int16
			    || type.SpecialType == SpecialType.System_UInt16
			    || type.SpecialType == SpecialType.System_Int32
			    || type.SpecialType == SpecialType.System_UInt32
			    || type.SpecialType == SpecialType.System_Int64
			    || type.SpecialType == SpecialType.System_UInt64;
		}

		private bool IsUnsignedType(ITypeSymbol type) {
			type = type.UnpackNullable();

			return type.SpecialType == SpecialType.System_Byte
			    || type.SpecialType == SpecialType.System_UInt16
			    || type.SpecialType == SpecialType.System_UInt32
			    || type.SpecialType == SpecialType.System_UInt64;
		}

		private bool IsNullableBooleanType(ITypeSymbol type) {
			return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
			    && ((INamedTypeSymbol)type).TypeArguments[0].SpecialType == SpecialType.System_Boolean;
		}

		private bool IsAssignmentOperator(SyntaxNode node) {
			var kind = node.CSharpKind();
			return kind == SyntaxKind.AddAssignmentExpression
			    || kind == SyntaxKind.AndAssignmentExpression
			    || kind == SyntaxKind.DivideAssignmentExpression
			    || kind == SyntaxKind.ExclusiveOrAssignmentExpression
			    || kind == SyntaxKind.LeftShiftAssignmentExpression
			    || kind == SyntaxKind.ModuloAssignmentExpression
			    || kind == SyntaxKind.MultiplyAssignmentExpression
			    || kind == SyntaxKind.OrAssignmentExpression
			    || kind == SyntaxKind.RightShiftAssignmentExpression
			    || kind == SyntaxKind.SubtractAssignmentExpression;
		}

		private JsExpression CompileCompoundFieldAssignment(Func<bool, JsExpression> getTarget, ITypeSymbol type, ISymbol member, ExpressionSyntax otherOperand, string fieldName, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool returnValueBeforeChange) {
			var target = member.IsStatic ? _runtimeLibrary.InstantiateType(member.ContainingType, this) : getTarget(compoundFactory == null);
			var jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, ref target) : null);
			var access = JsExpression.Member(target, fieldName);
			if (compoundFactory != null) {
				if (returnValueIsImportant && IsMutableValueType(type)) {
					_additionalStatements.Add(JsExpression.Assign(access, MaybeCloneValueType(valueFactory(target, jsOtherOperand), otherOperand, type)));
					return access;
				}
				else {
					return compoundFactory(access, MaybeCloneValueType(jsOtherOperand, otherOperand, type));
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

		private JsExpression CompileArrayAccessCompoundAssignment(Func<bool, JsExpression> getArray, ArgumentForCall index, ExpressionSyntax otherOperand, ITypeSymbol elementType, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool returnValueBeforeChange) {
			var expressions = new List<JsExpression>();
			expressions.Add(getArray(compoundFactory == null));
			expressions.Add(InnerCompile(index, compoundFactory == null, expressions));
			var jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, expressions) : null);
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

		private bool IsMutableValueType(ITypeSymbol type) {
			return Utils.IsMutableValueType(type, _metadataImporter);
		}

		private JsExpression MaybeCloneValueType(JsExpression input, ExpressionSyntax csharpInput, ITypeSymbol type, bool forceClone = false) {
			return Utils.MaybeCloneValueType(input, csharpInput, type, _metadataImporter, _runtimeLibrary, this, forceClone);
		}

		private JsExpression CompileMemberAssignment(Func<bool, JsExpression> getTarget, bool isNonVirtualAccess, ITypeSymbol type, ISymbol member, ArgumentMap indexingArgumentMap, ExpressionSyntax otherOperand, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool isLifted, bool returnValueBeforeChange, bool oldValueIsImportant) {
			if (member is IPropertySymbol) {
				var property = member as IPropertySymbol;
				var impl = _metadataImporter.GetPropertySemantics(property);

				switch (impl.Type) {
					case PropertyScriptSemantics.ImplType.GetAndSetMethods: {
						if (impl.SetMethod.Type == MethodScriptSemantics.ImplType.NativeIndexer) {
							if (!property.IsIndexer || property.GetMethod.Parameters.Length != 1) {
								_errorReporter.Message(Messages._7506);
								return JsExpression.Null;
							}
							return CompileArrayAccessCompoundAssignment(getTarget, indexingArgumentMap.ArgumentsForCall[0], otherOperand, property.Type, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
						}
						else {
							List<JsExpression> thisAndArguments;
							if (property.Parameters.Length > 0) {
								thisAndArguments = CompileThisAndArgumentListForMethodCall(property.SetMethod, null, getTarget(oldValueIsImportant), oldValueIsImportant, indexingArgumentMap);
							}
							else {
								thisAndArguments = new List<JsExpression> { member.IsStatic ? _runtimeLibrary.InstantiateType(member.ContainingType, this) : getTarget(oldValueIsImportant) };
							}
							
							JsExpression oldValue, jsOtherOperand;
							if (oldValueIsImportant) {
								thisAndArguments.Add(MaybeCloneValueType(CompileMethodInvocation(impl.GetMethod, property.GetMethod, thisAndArguments, isNonVirtualAccess), otherOperand, type));
								jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, thisAndArguments) : null);
								oldValue = thisAndArguments[thisAndArguments.Count - 1];
								thisAndArguments.RemoveAt(thisAndArguments.Count - 1); // Remove the current value because it should not be an argument to the setter.
							}
							else {
								jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, thisAndArguments) : null);
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
								_additionalStatements.Add(CompileMethodInvocation(impl.SetMethod, property.SetMethod, thisAndArguments, isNonVirtualAccess));
								return valueToReturn;
							}
							else {
								thisAndArguments.Add(MaybeCloneValueType(valueFactory(oldValue, jsOtherOperand), otherOperand, type));
								return CompileMethodInvocation(impl.SetMethod, property.SetMethod, thisAndArguments, isNonVirtualAccess);
							}
						}
					}

					case PropertyScriptSemantics.ImplType.Field: {
						return CompileCompoundFieldAssignment(getTarget, type, member, otherOperand, impl.FieldName, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
					}

					default: {
						_errorReporter.Message(Messages._7507, property.FullyQualifiedName());
						return JsExpression.Null;
					}
				}
			}
			else if (member is IFieldSymbol) {
				var field = (IFieldSymbol)member;
				var impl = _metadataImporter.GetFieldSemantics(field);
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

		private JsExpression CompileCompoundAssignment(ExpressionSyntax target, ExpressionSyntax otherOperand, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool isLifted, bool returnValueBeforeChange = false, bool oldValueIsImportant = true) {
			if (isLifted) {
				compoundFactory = null;
				var oldVF       = valueFactory;
				valueFactory    = (a, b) => _runtimeLibrary.Lift(oldVF(a, b), this);
			}

			var targetSymbol = _semanticModel.GetSymbolInfo(target).Symbol;
			var targetType = _semanticModel.GetTypeInfo(target).Type;

			#warning TODO dynamic
/*			if (targetSymbol == null) {
				// Dynamic
				if (target is MemberAccessExpressionSyntax) {
					var mae = (MemberAccessExpressionSyntax)target;
					return CompileCompoundFieldAssignment(usedMultipleTimes => InnerCompile(mae.Expression, usedMultipleTimes), _semanticModel.GetTypeInfo(otherOperand).Type, null, otherOperand, mae.Name.Identifier.Text, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
				}
				else if (target is ElementAccessExpressionSyntax) {
					var eae = (ElementAccessExpressionSyntax)target;
					if (eae.ArgumentList.Arguments.Count != 1) {
						_errorReporter.Message(Messages._7528);
						return JsExpression.Null;
					}

					return CompileArrayAccessCompoundAssignment(usedMultipleTimes => InnerCompile(eae.Expression, usedMultipleTimes), eae.ArgumentList.Arguments[0].Expression, otherOperand, _semanticModel.GetTypeInfo(otherOperand).Type, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
				}
				else {
					_errorReporter.InternalError("Unexpected target for dynamic assignment " + target);
					return JsExpression.Null;
				}
			}*/
			if (targetSymbol == null && target is MemberAccessExpressionSyntax) {
				var mae = (MemberAccessExpressionSyntax)target;
				return CompileCompoundFieldAssignment(usedMultipleTimes => InnerCompile(mae.Expression, usedMultipleTimes), _semanticModel.GetTypeInfo(otherOperand).Type, null, otherOperand, mae.Name.Identifier.Text, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
			}
			if (target is IdentifierNameSyntax) {
				if (targetSymbol is ILocalSymbol || targetSymbol is IParameterSymbol) {
					JsExpression jsTarget, jsOtherOperand;
					jsTarget = InnerCompile(target, compoundFactory == null, returnMultidimArrayValueByReference: true);
					jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false) : null);	// If the variable is a by-ref variable we will get invalid reordering if we force the target to be evaluated before the other operand.

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
				else if (targetSymbol is IPropertySymbol || targetSymbol is IFieldSymbol || targetSymbol is IEventSymbol) {
					return CompileMemberAssignment(usedMultipleTimes => CompileThis(), false, targetType, targetSymbol, null, otherOperand, compoundFactory, valueFactory, returnValueIsImportant, isLifted, returnValueBeforeChange, oldValueIsImportant);
				}
				else {
					_errorReporter.InternalError("Unexpected symbol for " + target);
					return JsExpression.Null;
				}
			}
			else if (target is MemberAccessExpressionSyntax) {
				var mae = (MemberAccessExpressionSyntax)target;
				return CompileMemberAssignment(usedMultipleTimes => InnerCompile(mae.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), mae.IsNonVirtualAccess(), targetType, targetSymbol, null, otherOperand, compoundFactory, valueFactory, returnValueIsImportant, isLifted, returnValueBeforeChange, oldValueIsImportant);
			}
			else if (target is ElementAccessExpressionSyntax) {
				var eae = (ElementAccessExpressionSyntax)target;
				if (targetSymbol is IPropertySymbol) {
					return CompileMemberAssignment(usedMultipleTimes => InnerCompile(eae.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), eae.IsNonVirtualAccess(), targetType, targetSymbol, _semanticModel.GetArgumentMap(eae), otherOperand, compoundFactory, valueFactory, returnValueIsImportant, isLifted, returnValueBeforeChange, oldValueIsImportant);
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
						jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, expressions) : null);
						oldValue = expressions[expressions.Count - 1];
						expressions.RemoveAt(expressions.Count - 1); // Remove the current value because it should not be an argument to the setter.
					}
					else {
						jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, expressions) : null);
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
				var jsOtherOperand = otherOperand != null ? InnerCompile(otherOperand, false) : null;

				if (_methodBeingCompiled == null || _methodBeingCompiled.MethodKind != MethodKind.Constructor) {
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
						_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, MaybeCloneValueType(jsTarget, null, targetType, forceClone: true)));
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

		private JsExpression CompileBinaryNonAssigningOperator(ExpressionSyntax left, ExpressionSyntax right, Func<JsExpression, JsExpression, JsExpression> resultFactory, bool isLifted) {
			var jsLeft  = InnerCompile(left, false);
			var jsRight = InnerCompile(right, false, ref jsLeft);
			var result = resultFactory(jsLeft, jsRight);
			return isLifted ? _runtimeLibrary.Lift(result, this) : result;
		}

		private JsExpression CompileUnaryOperator(ExpressionSyntax operand, Func<JsExpression, JsExpression> resultFactory, bool isLifted) {
			var jsOperand = InnerCompile(operand, false);
			var result = resultFactory(jsOperand);
			return isLifted ? _runtimeLibrary.Lift(result, this) : result;
		}

		private JsExpression CompileConditionalOperator(ExpressionSyntax test, ExpressionSyntax truePath, ExpressionSyntax falsePath) {
			var jsTest      = Visit(test, true);
			var trueResult  = CloneAndCompile(truePath, true);
			var falseResult = CloneAndCompile(falsePath, true);

			if (trueResult.AdditionalStatements.Count > 0 || falseResult.AdditionalStatements.Count > 0) {
				var temp = _createTemporaryVariable();
				var trueBlock  = JsStatement.Block(trueResult.AdditionalStatements.Concat(new JsStatement[] { JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), trueResult.Expression) }));
				var falseBlock = JsStatement.Block(falseResult.AdditionalStatements.Concat(new JsStatement[] { JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), falseResult.Expression) }));
				_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, null));
				_additionalStatements.Add(JsStatement.If(jsTest, trueBlock, falseBlock));
				return JsExpression.Identifier(_variables[temp].Name);
			}
			else {
				return JsExpression.Conditional(jsTest, trueResult.Expression, falseResult.Expression);
			}
		}

		private JsExpression CompileCoalesce(ExpressionSyntax left, ExpressionSyntax right) {
			var jsLeft  = InnerCompile(left, false);
			var jsRight = CloneAndCompile(right, true);
			var leftType = _semanticModel.GetTypeInfo(left).Type;

			if (jsRight.AdditionalStatements.Count == 0 && !CanTypeBeFalsy(leftType)) {
				return JsExpression.LogicalOr(jsLeft, jsRight.Expression);
			}
			else if (jsRight.AdditionalStatements.Count == 0 && (jsRight.Expression.NodeType == ExpressionNodeType.Identifier || (jsRight.Expression.NodeType >= ExpressionNodeType.ConstantFirst && jsRight.Expression.NodeType <= ExpressionNodeType.ConstantLast))) {
				return _runtimeLibrary.Coalesce(jsLeft, jsRight.Expression, this);
			}
			else {
				var temp = _createTemporaryVariable();
				var nullBlock  = JsStatement.Block(jsRight.AdditionalStatements.Concat(new JsStatement[] { JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), jsRight.Expression) }));
				_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, jsLeft));
				_additionalStatements.Add(JsStatement.If(_runtimeLibrary.ReferenceEquals(JsExpression.Identifier(_variables[temp].Name), JsExpression.Null, this), nullBlock, null));
				return JsExpression.Identifier(_variables[temp].Name);
			}
		}

		private JsExpression CompileEventAddOrRemove(ExpressionSyntax target, IEventSymbol eventSymbol, ExpressionSyntax value, bool isAdd) {
			Func<bool, JsExpression> getTarget;
			if (eventSymbol.IsStatic) {
				getTarget = _ => _runtimeLibrary.InstantiateType(eventSymbol.ContainingType, this);
			}
			else if (target is MemberAccessExpressionSyntax) {
				getTarget = usedMultipleTimes => InnerCompile(((MemberAccessExpressionSyntax)target).Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true);
			}
			else if (target is IdentifierNameSyntax) {
				getTarget = _ => CompileThis();
			}
			else {
				_errorReporter.InternalError("Bad target node for event");
				return JsExpression.Null;
			}

			var impl = _metadataImporter.GetEventSemantics(eventSymbol);
			switch (impl.Type) {
				case EventScriptSemantics.ImplType.AddAndRemoveMethods: {
					var accessor = isAdd ? eventSymbol.AddMethod : eventSymbol.RemoveMethod;
					return CompileMethodInvocation2(isAdd ? impl.AddMethod : impl.RemoveMethod, accessor, getTarget, IsReadonlyField(target), ArgumentMap.CreateIdentity(value), target.IsNonVirtualAccess());
				}
				default:
					_errorReporter.Message(Messages._7511, eventSymbol.FullyQualifiedName());
					return JsExpression.Null;
			}
		}

		private bool CanTypeBeFalsy(ITypeSymbol type) {
			type = type.UnpackNullable();
			return IsIntegerType(type) || type.SpecialType == SpecialType.System_Single || type.SpecialType == SpecialType.System_Double || type.SpecialType == SpecialType.System_Decimal || type.SpecialType == SpecialType.System_Boolean || type.SpecialType == SpecialType.System_String // Numbers, boolean and string have falsy values that are not null...
			    || type.TypeKind == TypeKind.Enum || type.TypeKind == TypeKind.DynamicType // ... so do enum types...
			    || type.SpecialType == SpecialType.System_Object || type.SpecialType == SpecialType.System_ValueType || type.SpecialType == SpecialType.System_Enum; // These reference types might contain types that have falsy values, so we need to be safe.
		}

		private JsExpression CompileAndAlsoOrOrElse(ExpressionSyntax left, ExpressionSyntax right, bool isAndAlso) {
			var jsLeft  = InnerCompile(left, false);
			var jsRight = CloneAndCompile(right, true);
			if (jsRight.AdditionalStatements.Count > 0) {
				var temp = _createTemporaryVariable();
				var ifBlock = JsStatement.Block(jsRight.AdditionalStatements.Concat(new JsStatement[] { JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), jsRight.Expression) }));
				_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, jsLeft));
				JsExpression test = JsExpression.Identifier(_variables[temp].Name);
				if (!isAndAlso)
					test = JsExpression.LogicalNot(test);
				_additionalStatements.Add(JsStatement.If(test, ifBlock, null));
				return JsExpression.Identifier(_variables[temp].Name);
			}
			else {
				return isAndAlso ? JsExpression.LogicalAnd(jsLeft, jsRight.Expression) : JsExpression.LogicalOr(jsLeft, jsRight.Expression);
			}
		}

		private bool CanDoSimpleComparisonForEquals(ExpressionSyntax a, ExpressionSyntax b) {
			var tiA = _semanticModel.GetTypeInfo(a);
			var tiB = _semanticModel.GetTypeInfo(b);
			var typeA = tiA.ConvertedType;
			var typeB = tiB.ConvertedType;

			if (typeA != null && typeA.IsNullable()) {
				// in an expression such as myNullableInt == 3, an implicit nullable conversion is performed on the non-nullable value, but we can know for sure that it will never be null.
				var ca = _semanticModel.GetConversion(a);
				if (ca.IsNullable && ca.IsImplicit)
					typeA = tiA.Type;
			}

			if (typeB != null && typeB.IsNullable()) {
				var cb = _semanticModel.GetConversion(b);
				if (cb.IsNullable && cb.IsImplicit)
					typeB = tiB.Type;
			}
			
			bool aCanBeNull = typeA == null || !typeA.IsValueType || typeA.IsNullable();
			bool bCanBeNull = typeB == null || !typeB.IsValueType || typeB.IsNullable();
			return !aCanBeNull || !bCanBeNull;
		}

		public override JsExpression VisitBinaryExpression(BinaryExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

			if (symbol != null && symbol.MethodKind == MethodKind.UserDefinedOperator) {
				var impl = _metadataImporter.GetMethodSemantics(symbol);
				if (impl.Type != MethodScriptSemantics.ImplType.NativeOperator) {
					Func<JsExpression, JsExpression, JsExpression> invocation = (a, b) => CompileMethodInvocation(impl, symbol, new[] { _runtimeLibrary.InstantiateType(symbol.ContainingType, this), a, b }, false);
					if (IsAssignmentOperator(node))
						return CompileCompoundAssignment(node.Left, node.Right, null, invocation, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, invocation, _semanticModel.IsLiftedOperator(node));
				}
			}

			switch (node.CSharpKind()) {
				case SyntaxKind.SimpleAssignmentExpression:
					return CompileCompoundAssignment(node.Left, node.Right, JsExpression.Assign, (a, b) => b, _returnValueIsImportant, false, oldValueIsImportant: false);

				// Compound assignment operators

				case SyntaxKind.AddAssignmentExpression: {
					var leftSymbol = _semanticModel.GetSymbolInfo(node.Left).Symbol;
					if (leftSymbol is IEventSymbol) {
						return CompileEventAddOrRemove(node.Left, (IEventSymbol)leftSymbol, node.Right, true);
					}
					else {
						if (symbol.ContainingType.TypeKind == TypeKind.Delegate) {
							var del = _semanticModel.Compilation.GetSpecialType(SpecialType.System_Delegate);
							var combine = (IMethodSymbol)del.GetMembers("Combine").Single();
							var impl = _metadataImporter.GetMethodSemantics(combine);
							#warning TODO: perhaps replace with CompileDelegateCombineCall()
							return CompileCompoundAssignment(node.Left, node.Right, null, (a, b) => CompileMethodInvocation(impl, combine, new[] { _runtimeLibrary.InstantiateType(del, this), a, b }, false), _returnValueIsImportant, false);
						}
						else {
							return CompileCompoundAssignment(node.Left, node.Right, JsExpression.AddAssign, JsExpression.Add, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));
						}
					}
				}

				case SyntaxKind.AndAssignmentExpression:
					if (IsNullableBooleanType(_semanticModel.GetTypeInfo(node.Left).Type))
						return CompileCompoundAssignment(node.Left, node.Right, null, (a, b) => _runtimeLibrary.LiftedBooleanAnd(a, b, this), _returnValueIsImportant, false);
					else
						return CompileCompoundAssignment(node.Left, node.Right, JsExpression.BitwiseAndAssign, JsExpression.BitwiseAnd, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.DivideAssignmentExpression:
					if (IsIntegerType(_semanticModel.GetTypeInfo(node).Type))
						return CompileCompoundAssignment(node.Left, node.Right, null, (a, b) => _runtimeLibrary.IntegerDivision(a, b, this), _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));
					else
						return CompileCompoundAssignment(node.Left, node.Right, JsExpression.DivideAssign, JsExpression.Divide, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.ExclusiveOrAssignmentExpression:
					return CompileCompoundAssignment(node.Left, node.Right, JsExpression.BitwiseXorAssign, JsExpression.BitwiseXor, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.LeftShiftAssignmentExpression:
					return CompileCompoundAssignment(node.Left, node.Right, JsExpression.LeftShiftAssign, JsExpression.LeftShift, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.ModuloAssignmentExpression:
					return CompileCompoundAssignment(node.Left, node.Right, JsExpression.ModuloAssign, JsExpression.Modulo, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.MultiplyAssignmentExpression:
					return CompileCompoundAssignment(node.Left, node.Right, JsExpression.MultiplyAssign, JsExpression.Multiply, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.OrAssignmentExpression:
					if (IsNullableBooleanType(_semanticModel.GetTypeInfo(node.Left).Type))
						return CompileCompoundAssignment(node.Left, node.Right, null, (a, b) => _runtimeLibrary.LiftedBooleanOr(a, b, this), _returnValueIsImportant, false);
					else
						return CompileCompoundAssignment(node.Left, node.Right, JsExpression.BitwiseOrAssign, JsExpression.BitwiseOr, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.RightShiftAssignmentExpression:
					if (IsUnsignedType(_semanticModel.GetTypeInfo(node).Type))
						return CompileCompoundAssignment(node.Left, node.Right, JsExpression.RightShiftUnsignedAssign, JsExpression.RightShiftUnsigned, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));
					else
						return CompileCompoundAssignment(node.Left, node.Right, JsExpression.RightShiftSignedAssign, JsExpression.RightShiftSigned, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.SubtractAssignmentExpression: {
					var leftSymbol = _semanticModel.GetSymbolInfo(node.Left).Symbol;
					if (leftSymbol is IEventSymbol) {
						return CompileEventAddOrRemove(node.Left, (IEventSymbol)leftSymbol, node.Right, false);
					}
					else {
						if (symbol != null && symbol.ContainingType.TypeKind == TypeKind.Delegate) {
							var del = _semanticModel.Compilation.GetSpecialType(SpecialType.System_Delegate);
							var remove = (IMethodSymbol)del.GetMembers("Remove").Single();
							var impl = _metadataImporter.GetMethodSemantics(remove);
							return CompileCompoundAssignment(node.Left, node.Right, null, (a, b) => CompileMethodInvocation(impl, remove, new[] { _runtimeLibrary.InstantiateType(del, this), a, b }, false), _returnValueIsImportant, false);
						}
						else {
							return CompileCompoundAssignment(node.Left, node.Right, JsExpression.SubtractAssign, JsExpression.Subtract, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));
						}
					}
				}

				// Binary non-assigning operators

				case SyntaxKind.AddExpression:
					if (_semanticModel.GetTypeInfo(node.Left).Type.TypeKind == TypeKind.Delegate) {
						var del = _semanticModel.Compilation.GetSpecialType(SpecialType.System_Delegate);
						var combine = (IMethodSymbol)del.GetMembers("Combine").Single();
						var impl = _metadataImporter.GetMethodSemantics(combine);
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => CompileMethodInvocation(impl, combine, new[] { _runtimeLibrary.InstantiateType(del, this), a, b }, false), false);
					}
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Add, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.BitwiseAndExpression:
					if (IsNullableBooleanType(_semanticModel.GetTypeInfo(node.Left).Type))
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => _runtimeLibrary.LiftedBooleanAnd(a, b, this), false);	// We have already lifted it, so it should not be lifted again.
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.BitwiseAnd, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.LogicalAndExpression:
					return CompileAndAlsoOrOrElse(node.Left, node.Right, true);

				case SyntaxKind.CoalesceExpression:
					return CompileCoalesce(node.Left, node.Right);

				case SyntaxKind.DivideExpression:
					if (IsIntegerType(_semanticModel.GetTypeInfo(node).Type))
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => _runtimeLibrary.IntegerDivision(a, b, this), _semanticModel.IsLiftedOperator(node));
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Divide, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.ExclusiveOrExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.BitwiseXor, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.GreaterThanExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Greater, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.GreaterThanOrEqualExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.GreaterOrEqual, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.EqualsExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => CanDoSimpleComparisonForEquals(node.Left, node.Right) ? JsExpression.Same(a, b) : _runtimeLibrary.ReferenceEquals(a, b, this), false);

				case SyntaxKind.LeftShiftExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.LeftShift, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.LessThanExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Lesser, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.LessThanOrEqualExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.LesserOrEqual, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.ModuloExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Modulo, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.MultiplyExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Multiply, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.NotEqualsExpression:
					return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => CanDoSimpleComparisonForEquals(node.Left, node.Right) ? JsExpression.NotSame(a, b) : _runtimeLibrary.ReferenceNotEquals(a, b, this), false);

				case SyntaxKind.BitwiseOrExpression:
					if (IsNullableBooleanType(_semanticModel.GetTypeInfo(node.Left).Type))
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => _runtimeLibrary.LiftedBooleanOr(a, b, this), false);	// We have already lifted it, so it should not be lifted again.
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.BitwiseOr, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.LogicalOrExpression:
					return CompileAndAlsoOrOrElse(node.Left, node.Right, false);

				case SyntaxKind.RightShiftExpression:
					if (IsUnsignedType(_semanticModel.GetTypeInfo(node).Type))
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.RightShiftUnsigned, _semanticModel.IsLiftedOperator(node));
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.RightShiftSigned, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.SubtractExpression:
					if (_semanticModel.GetTypeInfo(node.Left).Type.TypeKind == TypeKind.Delegate) {
						var del = _semanticModel.Compilation.GetSpecialType(SpecialType.System_Delegate);
						var remove = (IMethodSymbol)del.GetMembers("Remove").Single();
						var impl = _metadataImporter.GetMethodSemantics(remove);
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, (a, b) => CompileMethodInvocation(impl, remove, new[] { _runtimeLibrary.InstantiateType(del, this), a, b }, false), false);
					}
					else
						return CompileBinaryNonAssigningOperator(node.Left, node.Right, JsExpression.Subtract, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.AsExpression:
					return _runtimeLibrary.TryDowncast(InnerCompile(node.Left, false), _semanticModel.GetTypeInfo(node.Left).Type, ((ITypeSymbol)_semanticModel.GetSymbolInfo(node.Right).Symbol).UnpackNullable(), this);

				case SyntaxKind.IsExpression:
					var targetType = ((ITypeSymbol)_semanticModel.GetSymbolInfo(node.Right).Symbol).UnpackNullable();
					return _runtimeLibrary.TypeIs(Visit(node.Left, true), _semanticModel.GetTypeInfo(node.Left).ConvertedType, targetType, this);

				default:
					_errorReporter.InternalError("Unsupported operator " + node.CSharpKind());
					return JsExpression.Null;
			}
		}

		public override JsExpression VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

			if (symbol != null && symbol.MethodKind == MethodKind.UserDefinedOperator) {
				var impl = _metadataImporter.GetMethodSemantics(symbol);
				if (impl.Type != MethodScriptSemantics.ImplType.NativeOperator) {
					if (node.CSharpKind() == SyntaxKind.PreIncrementExpression || node.CSharpKind() == SyntaxKind.PreDecrementExpression) {
						Func<JsExpression, JsExpression, JsExpression> invocation = (a, b) => CompileMethodInvocation(impl, symbol, new[] { _runtimeLibrary.InstantiateType(symbol.ContainingType, this), a }, false);
						return CompileCompoundAssignment(node.Operand, null, null, invocation, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node), false);
					}
					else {
						return CompileUnaryOperator(node.Operand, a => CompileMethodInvocation(impl, symbol, new[] { _runtimeLibrary.InstantiateType(symbol.ContainingType, this), a }, false), _semanticModel.IsLiftedOperator(node));
					}
				}
			}

			switch (node.CSharpKind()) {
				case SyntaxKind.PreIncrementExpression:
					return CompileCompoundAssignment(node.Operand, null, (a, b) => JsExpression.PrefixPlusPlus(a), (a, b) => JsExpression.Add(a, JsExpression.Number(1)), _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.PreDecrementExpression:
					return CompileCompoundAssignment(node.Operand, null, (a, b) => JsExpression.PrefixMinusMinus(a), (a, b) => JsExpression.Subtract(a, JsExpression.Number(1)), _returnValueIsImportant, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.UnaryMinusExpression:
					return CompileUnaryOperator(node.Operand, JsExpression.Negate, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.UnaryPlusExpression:
					return CompileUnaryOperator(node.Operand, JsExpression.Positive, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.LogicalNotExpression:
					return CompileUnaryOperator(node.Operand, JsExpression.LogicalNot, _semanticModel.IsLiftedOperator(node));

				case SyntaxKind.BitwiseNotExpression:
					return CompileUnaryOperator(node.Operand, JsExpression.BitwiseNot, _semanticModel.IsLiftedOperator(node));

				default:
					_errorReporter.InternalError("Unsupported operator " + node.OperatorToken.CSharpKind());
					return JsExpression.Null;
			}
		}

		public override JsExpression VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

			if (symbol != null && symbol.MethodKind == MethodKind.UserDefinedOperator) {
				var impl = _metadataImporter.GetMethodSemantics(symbol);
				if (impl.Type != MethodScriptSemantics.ImplType.NativeOperator) {
					if (node.CSharpKind() == SyntaxKind.PostIncrementExpression || node.CSharpKind() == SyntaxKind.PostDecrementExpression) {
						Func<JsExpression, JsExpression, JsExpression> invocation = (a, b) => CompileMethodInvocation(impl, symbol, new[] { _runtimeLibrary.InstantiateType(symbol.ContainingType, this), _returnValueIsImportant ? MaybeCloneValueType(a, null, symbol.Parameters[0].Type) : a }, false);
						return CompileCompoundAssignment(node.Operand, null, null, invocation, _returnValueIsImportant, _semanticModel.IsLiftedOperator(node), true);
					}
				}
			}

			switch (node.CSharpKind()) {
				case SyntaxKind.PostIncrementExpression:
					return CompileCompoundAssignment(node.Operand, null, (a, b) => JsExpression.PostfixPlusPlus(a), (a, b) => JsExpression.Add(a, JsExpression.Number(1)), _returnValueIsImportant, _semanticModel.IsLiftedOperator(node), returnValueBeforeChange: true);

				case SyntaxKind.PostDecrementExpression:
					return CompileCompoundAssignment(node.Operand, null, (a, b) => JsExpression.PostfixMinusMinus(a), (a, b) => JsExpression.Subtract(a, JsExpression.Number(1)), _returnValueIsImportant, _semanticModel.IsLiftedOperator(node), returnValueBeforeChange: true);

				default:
					_errorReporter.InternalError("Unsupported operator " + node.OperatorToken.CSharpKind());
					return JsExpression.Null;
			}
		}
		
		public override JsExpression VisitConditionalExpression(ConditionalExpressionSyntax node) {
			return CompileConditionalOperator(node.Condition, node.WhenTrue, node.WhenFalse);
		}

		public override JsExpression VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) {
			return Visit(node.Expression);
		}

		public JsExpression CompileDelegateCombineCall(JsExpression a, JsExpression b) {
			var del = _semanticModel.Compilation.GetSpecialType(SpecialType.System_Delegate);
			var combine = (IMethodSymbol)del.GetMembers("Combine").Single();
			var impl = _metadataImporter.GetMethodSemantics(combine);
			var thisAndArguments = (combine.IsStatic ? new[] { _runtimeLibrary.InstantiateType(del, this), a, b } : new[] { a, b });
			return CompileMethodInvocation(impl, combine, thisAndArguments, false);
		}

		public JsExpression CompileDelegateRemoveCall(JsExpression a, JsExpression b) {
			var del = _semanticModel.Compilation.GetSpecialType(SpecialType.System_Delegate);
			var remove = (IMethodSymbol)del.GetMembers("Remove").Single();
			var impl = _metadataImporter.GetMethodSemantics(remove);
			var thisAndArguments = (remove.IsStatic ? new[] { _runtimeLibrary.InstantiateType(del, this), a, b } : new[] { a, b });
			return CompileMethodInvocation(impl, remove, thisAndArguments, false);
		}

#if false

		FROM Invocations
			else if (member is IPropertySymbol) {
				var property = (IPropertySymbol)member;
				var impl = _metadataImporter.GetPropertySemantics(property);
				if (impl.Type != PropertyScriptSemantics.ImplType.GetAndSetMethods) {
					_errorReporter.InternalError("Cannot invoke property that does not have a get method.");
					return JsExpression.Null;
				}
				return CompileMethodInvocation(impl.GetMethod, property.Getter, targetResult, argumentsForCall, argumentToParameterMap, isVirtualCall);
			}
			else {
				_errorReporter.InternalError("Invocation of unsupported member " + member.ContainingType.FullName + "." + member.Name);
				return JsExpression.Null;
			}
		}

		if (method.IsConstructor) {
			if (method.ContainingType.Kind == TypeKind.Enum) {
				return JsExpression.Number(0);
			}
			else if (method.ContainingType.Kind == TypeKind.TypeParameter) {
				var activator = ReflectionHelper.ParseReflectionName("System.Activator").Resolve(_compilation);
				var createInstance = activator.GetMethods(m => m.Name == "CreateInstance" && m.IsStatic && m.TypeParameters.Count == 1 && m.Parameters.Count == 0).Single();
				var createInstanceSpec = new SpecializedMethod(createInstance, new TypeParameterSubstitution(EmptyList<ITypeSymbol>.Instance, new[] { method.ContainingType }));
				var createdObject = CompileMethodInvocation(_metadataImporter.GetMethodSemantics(createInstanceSpec), createInstanceSpec, new[] { _runtimeLibrary.InstantiateType(activator, this) }, false);
				return CompileInitializerStatements(createdObject, method.ContainingType, initializerStatements);
			}
			else {
				return CompileConstructorInvocation(_metadataImporter.GetConstructorSemantics(method), method, argumentsForCall, argumentToParameterMap, initializerStatements);
			}
		}
#endif

		private JsExpression HandleMemberAccess(Func<bool, JsExpression> getTarget, ISymbol member, bool isNonVirtualAccess, bool targetIsReadOnlyField) {
			if (member is IPropertySymbol) {
				var property = (IPropertySymbol)member;
				var impl = _metadataImporter.GetPropertySemantics(property);
				switch (impl.Type) {
					case PropertyScriptSemantics.ImplType.GetAndSetMethods: {
						var getter = property.GetMethod;
						return CompileMethodInvocation2(impl.GetMethod, getter, getTarget, targetIsReadOnlyField, ArgumentMap.Empty, isNonVirtualAccess);	// We know we have no arguments because indexers are treated as invocations.
					}
					case PropertyScriptSemantics.ImplType.Field: {
						return JsExpression.Member(member.IsStatic ? _runtimeLibrary.InstantiateType(member.ContainingType, this) : getTarget(false), impl.FieldName);
					}
					default: {
						_errorReporter.Message(Messages._7512, member.FullyQualifiedName());
						return JsExpression.Null;
					}
				}
			}
			else if (member is IFieldSymbol) {
				var impl = _metadataImporter.GetFieldSemantics((IFieldSymbol)member);
				switch (impl.Type) {
					case FieldScriptSemantics.ImplType.Field:
						return JsExpression.Member(member.IsStatic ? _runtimeLibrary.InstantiateType(member.ContainingType, this) : getTarget(false), impl.Name);
					case FieldScriptSemantics.ImplType.Constant:
						return JSModel.Utils.MakeConstantExpression(impl.Value);
					default:
						_errorReporter.Message(Messages._7509, member.FullyQualifiedName());
						return JsExpression.Null;
				}
			}
			else if (member is IEventSymbol) {
				var impl = _metadataImporter.GetEventSemantics((IEventSymbol)member);
				if (impl.Type == EventScriptSemantics.ImplType.NotUsableFromScript) {
					_errorReporter.Message(Messages._7511, member.FullyQualifiedName());
					return JsExpression.Null;
				}

				var fname = _metadataImporter.GetAutoEventBackingFieldName((IEventSymbol)member);
				return JsExpression.Member(member.IsStatic ? _runtimeLibrary.InstantiateType(member.ContainingType, this) : getTarget(false), fname);
			}
			else if (member is IMethodSymbol) {
				var impl = _metadataImporter.GetMethodSemantics((IMethodSymbol)member);
				if (impl.Type == MethodScriptSemantics.ImplType.NotUsableFromScript) {
					_errorReporter.Message(Messages._7511, member.FullyQualifiedName());
					return JsExpression.Null;
				}

				return JsExpression.Member(member.IsStatic ? _runtimeLibrary.InstantiateType(member.ContainingType, this) : getTarget(false), impl.Name);
			}
			else {
				_errorReporter.InternalError("Invalid member " + member);
				return JsExpression.Null;
			}
		}

		public override JsExpression VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
			var conversion = _semanticModel.GetConversion(node);
			if (conversion.IsMethodGroup) {
				var targetType = _semanticModel.GetTypeInfo(node).ConvertedType;
				return PerformMethodGroupConversion(usedMultipleTimes => InnerCompile(node.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), (INamedTypeSymbol)targetType, (IMethodSymbol)symbol, node.IsNonVirtualAccess());
			}
			else {
				return HandleMemberAccess(usedMultipleTimes => InnerCompile(node.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), _semanticModel.GetSymbolInfo(node).Symbol, node.IsNonVirtualAccess(), IsReadonlyField(node));
			}
		}

		private int FindIndexInTokens(IList<InlineCodeToken> tokens, int parameterIndex) {
			for (int i = 0; i < tokens.Count; i++) {
				if (parameterIndex == -1) {
					if (tokens[i].Type == InlineCodeToken.TokenType.This)
						return i;
				}
				else {
					if ((tokens[i].Type == InlineCodeToken.TokenType.Parameter) && tokens[i].Index == parameterIndex)
						return i;
				}
			}
			return -1;
		}

		private IList<int> CreateInlineCodeExpressionToOrderMap(IList<InlineCodeToken> tokens, int argumentCount, IList<int> argumentToParameterMap) {
			var dict = Enumerable.Range(-1, argumentCount + 1).OrderBy(x => FindIndexInTokens(tokens, x)).Select((i, n) => new { i, n }).ToDictionary(x => x.i, x => x.n);
			return new[] { -1 }.Concat(argumentToParameterMap).Select(x => dict[x]).ToList();
		}

		private void CreateTemporariesForExpressionsAndAllRequiredExpressionsLeftOfIt(List<JsExpression> expressions, int index) {
			for (int i = 0; i < index; i++) {
				if (ExpressionOrderer.DoesOrderMatter(expressions[i], expressions[index])) {
					CreateTemporariesForExpressionsAndAllRequiredExpressionsLeftOfIt(expressions, i);
				}
			}
			var temp = _createTemporaryVariable();
			_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, expressions[index]));
			expressions[index] = JsExpression.Identifier(_variables[temp].Name);
		}

		private List<JsExpression> CompileThisAndArgumentListForMethodCall(IMethodSymbol member, string literalCode, JsExpression target, bool argumentsUsedMultipleTimes, ArgumentMap argumentMap) {
			member = member.UnReduceIfExtensionMethod();
			IList<InlineCodeToken> tokens = null;
			var expressions = new List<JsExpression>() { target };
			if (literalCode != null) {
				bool hasError = false;
				tokens = InlineCodeMethodCompiler.Tokenize((IMethodSymbol)member, literalCode, s => hasError = true);
				if (hasError)
					tokens = null;
			}

			if (tokens != null && target != null && !member.IsStatic && member.MethodKind != MethodKind.Constructor) {
				int thisUseCount = tokens.Count(t => t.Type == InlineCodeToken.TokenType.This);
				if (thisUseCount > 1 && IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(target)) {
					// Create a temporary for {this}, if required.
					var temp = _createTemporaryVariable();
					_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, expressions[0]));
					expressions[0] = JsExpression.Identifier(_variables[temp].Name);
				}
				else if (thisUseCount == 0 && DoesJsExpressionHaveSideEffects.Analyze(target)) {
					// Ensure that 'this' is evaluated if required, even if not used by the inline code.
					_additionalStatements.Add(target);
					expressions[0] = JsExpression.Null;
				}
			}

			bool hasCreatedParamArray = false;

			// Compile the arguments left to right
			foreach (var i in argumentMap.ArgumentToParameterMap) {
				if (member.Parameters[i].IsParams) {
					if (hasCreatedParamArray)
						continue;
					hasCreatedParamArray = true;
				}

				var a = argumentMap.ArgumentsForCall[i];
				if (member.Parameters[i].RefKind != RefKind.None) {
					var symbol = _semanticModel.GetSymbolInfo(a.Argument).Symbol;
					if (symbol is ILocalSymbol || symbol is IParameterSymbol) {
						expressions.Add(CompileLocal(symbol, true));
					}
					else {
						_errorReporter.Message(Messages._7513);
						expressions.Add(JsExpression.Null);
					}
				}
				else {
					int useCount = (tokens != null ? tokens.Count(t => t.Type == InlineCodeToken.TokenType.Parameter && t.Index == i) : 1);
					bool usedMultipleTimes = argumentsUsedMultipleTimes || useCount > 1;
					if (useCount >= 1) {
						expressions.Add(InnerCompile(a, usedMultipleTimes, expressions));
					}
					else if (tokens != null && tokens.Count(t => t.Type == InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier && t.Index == i) > 0) {
						var result = CloneAndCompile(a, false);
						expressions.Add(result.Expression);	// Will later give an error if the result is not a literal string.
					}
					else {
						var result = CloneAndCompile(a, false);
						if (result.AdditionalStatements.Count > 0 || DoesJsExpressionHaveSideEffects.Analyze(result.Expression)) {
							CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(expressions, result);
							_additionalStatements.AddRange(result.AdditionalStatements);
							_additionalStatements.Add(result.Expression);
						}
						expressions.Add(JsExpression.Null);	// Will be ignored later, anyway
					}
				}
			}

			// Ensure that expressions are evaluated left-to-right in the resulting script.
			var expressionToOrderMap = tokens == null ? new[] { 0 }.Concat(argumentMap.ArgumentToParameterMap.Select(x => x + 1)).ToList() : CreateInlineCodeExpressionToOrderMap(tokens, argumentMap.ArgumentsForCall.Length, argumentMap.ArgumentToParameterMap);
			for (int i = 0; i < expressions.Count; i++) {
				var haveToBeEvaluatedBefore = Enumerable.Range(i + 1, expressions.Count - i - 1).Where(x => expressionToOrderMap[x] < expressionToOrderMap[i]);
				if (haveToBeEvaluatedBefore.Any(other => ExpressionOrderer.DoesOrderMatter(expressions[i], expressions[other]))) {
					CreateTemporariesForExpressionsAndAllRequiredExpressionsLeftOfIt(expressions, i);
				}
			}

			// Rearrange the arguments so they appear in the order the method expects them to.
			if ((argumentMap.ArgumentToParameterMap.Length != argumentMap.ArgumentsForCall.Length || argumentMap.ArgumentToParameterMap.Select((i, n) => new { i, n }).Any(t => t.i != t.n))) {	// If we have an argument to parameter map and it actually performs any reordering.			// Ensure that expressions are evaluated left-to-right in case arguments are reordered
				var newExpressions = new List<JsExpression>() { expressions[0] };
				for (int i = 0; i < argumentMap.ArgumentsForCall.Length; i++) {
					int specifiedIndex = argumentMap.ArgumentToParameterMap.IndexOf(i);
					newExpressions.Add(specifiedIndex != -1 ? expressions[specifiedIndex + 1] : InnerCompile(argumentMap.ArgumentsForCall[i], false));	// If the argument was not specified, use the value in argumentsForCall, which has to be constant.
				}
				expressions = newExpressions;
			}

			for (int i = 1; i < expressions.Count; i++) {
				if ((i - 1) >= member.Parameters.Length || member.Parameters[i - 1].RefKind == RefKind.None) {
					expressions[i] = MaybeCloneValueType(expressions[i], argumentMap.ArgumentsForCall[i - 1].Argument, member.Parameters[Math.Min(i - 1, member.Parameters.Length - 1)].Type);	// Math.Min() because the last parameter might be an expanded param array.
				}
			}

			return expressions;
		}

		private string GetActualInlineCode(MethodScriptSemantics sem, bool isNonVirtualInvocationOfVirtualMethod, bool isParamArrayExpanded) {
			if (sem.Type == MethodScriptSemantics.ImplType.InlineCode) {
				if (isNonVirtualInvocationOfVirtualMethod)
					return sem.NonVirtualInvocationLiteralCode;
				else if (!isParamArrayExpanded)
					return sem.NonExpandedFormLiteralCode;
				else
					return sem.LiteralCode;
			}
			else {
				return null;
			}
		}

		private string GetActualInlineCode(ConstructorScriptSemantics sem, bool isParamArrayExpanded) {
			if (sem.Type == ConstructorScriptSemantics.ImplType.InlineCode) {
				if (!isParamArrayExpanded)
					return sem.NonExpandedFormLiteralCode;
				else
					return sem.LiteralCode;
			}
			else {
				return null;
			}
		}

		private bool IsReadonlyField(ExpressionSyntax r) {
			for (;;) {
				var sym = _semanticModel.GetSymbolInfo(r).Symbol as IFieldSymbol;
				if (sym == null || sym.Type.TypeKind != TypeKind.Struct)
					return false;
				if (sym.IsReadOnly)
					return true;

				var mr = r as MemberAccessExpressionSyntax;
				if (mr == null)
					return false;
				r = mr.Expression;
			}
		}

		// Renamed because last parameter swapped meaning
		private JsExpression CompileMethodInvocation2(MethodScriptSemantics sem, IMethodSymbol method, Func<bool, JsExpression> getTarget, bool targetIsReadOnlyField, ArgumentMap argumentMap, bool isNonVirtualInvocation) {
			method = method.UnReduceIfExtensionMethod();
			isNonVirtualInvocation &= method.IsOverridable();
			bool targetUsedMultipleTimes = sem != null && ((!sem.IgnoreGenericArguments && method.TypeParameters.Length > 0) || (sem.ExpandParams && !argumentMap.CanBeTreatedAsExpandedForm));
			string literalCode = GetActualInlineCode(sem, isNonVirtualInvocation, argumentMap.CanBeTreatedAsExpandedForm);

			var jsTarget = method.IsStatic ? _runtimeLibrary.InstantiateType(method.ContainingType, this) : getTarget(targetUsedMultipleTimes);
			if (IsMutableValueType(method.ContainingType) && targetIsReadOnlyField) {
				jsTarget = MaybeCloneValueType(jsTarget, null, method.ContainingType, forceClone: true);
			}

			var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, literalCode, jsTarget, false, argumentMap);
			return CompileMethodInvocation(sem, method, thisAndArguments, isNonVirtualInvocation);
		}

		private JsExpression CompileConstructorInvocationWithPotentialExpandParams(IList<JsExpression> arguments, JsExpression constructor, bool expandParams) {
			if (expandParams) {
				if (arguments[arguments.Count - 1] is JsArrayLiteralExpression) {
					var args = arguments.Take(arguments.Count - 1).Concat(((JsArrayLiteralExpression)arguments[arguments.Count - 1]).Elements);
					return JsExpression.New(constructor, args);
				}
				else {
					return _runtimeLibrary.ApplyConstructor(constructor, arguments.Count == 1 ? arguments[0] : JsExpression.Invocation(JsExpression.Member(JsExpression.ArrayLiteral(arguments.Take(arguments.Count - 1)), "concat"), arguments[arguments.Count - 1]), this);
				}
			}
			else {
				return JsExpression.New(constructor, arguments);
			}
		}

		private JsExpression CompileMethodInvocationWithPotentialExpandParams(IList<JsExpression> thisAndArguments, JsExpression method, bool expandParams, bool needCall) {
			if (expandParams) {
				if (thisAndArguments[thisAndArguments.Count - 1] is JsArrayLiteralExpression) {
					var args = thisAndArguments.Take(thisAndArguments.Count - 1).Concat(((JsArrayLiteralExpression)thisAndArguments[thisAndArguments.Count - 1]).Elements);
					return needCall ? JsExpression.Invocation(JsExpression.Member(method, "call"), args) : JsExpression.Invocation(method, args.Skip(1));
				}
				else {
					return JsExpression.Invocation(JsExpression.Member(method, "apply"), thisAndArguments[0], thisAndArguments.Count == 2 ? thisAndArguments[1] : JsExpression.Invocation(JsExpression.Member(JsExpression.ArrayLiteral(thisAndArguments.Skip(1).Take(thisAndArguments.Count - 2)), "concat"), thisAndArguments[thisAndArguments.Count - 1]));
				}
			}
			else {
				return needCall ? JsExpression.Invocation(JsExpression.Member(method, "call"), thisAndArguments) : JsExpression.Invocation(method, thisAndArguments.Skip(1));
			}
		}

		private JsExpression CompileMethodInvocation(MethodScriptSemantics impl, IMethodSymbol method, IList<JsExpression> thisAndArguments, bool isNonVirtualInvocation) {
			isNonVirtualInvocation &= method.IsOverridable();
			var errors = Utils.FindGenericInstantiationErrors(method.TypeArguments, _metadataImporter);
			if (errors.HasErrors) {
				foreach (var ut in errors.UsedUnusableTypes)
					_errorReporter.Message(Messages._7515, ut.Name, method.FullyQualifiedName());
				foreach (var t in errors.MutableValueTypesBoundToTypeArguments)
					_errorReporter.Message(Messages._7539, t.Name);
				return JsExpression.Null;
			}

			var typeArguments = (impl != null && !impl.IgnoreGenericArguments ? method.TypeArguments : ImmutableArray<ITypeSymbol>.Empty);

			switch (impl.Type) {
				case MethodScriptSemantics.ImplType.NormalMethod: {
					if (isNonVirtualInvocation) {
						return _runtimeLibrary.CallBase(method, thisAndArguments, this);
					}
					else {
						var jsMethod = JsExpression.Member(thisAndArguments[0], impl.Name);
						if (method.IsStatic)
							thisAndArguments = new[] { JsExpression.Null }.Concat(thisAndArguments.Skip(1)).ToList();

						if (typeArguments.Length > 0) {
							return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments, this), impl.ExpandParams, true);
						}
						else
							return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, jsMethod, impl.ExpandParams, false);
					}
				}

				case MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument: {
					var jsMethod = JsExpression.Member(_runtimeLibrary.InstantiateType(method.ContainingType, this), impl.Name);
					thisAndArguments.Insert(0, JsExpression.Null);
					if (typeArguments.Length > 0) {
						return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments, this), impl.ExpandParams, true);
					}
					else {
						return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, jsMethod, impl.ExpandParams, false);
					}
				}

				case MethodScriptSemantics.ImplType.InlineCode:
					return CompileInlineCodeMethodInvocation(method, GetActualInlineCode(impl, isNonVirtualInvocation, thisAndArguments[thisAndArguments.Count - 1] is JsArrayLiteralExpression), method.IsStatic ? null : thisAndArguments[0], thisAndArguments.Skip(1).ToList());

				case MethodScriptSemantics.ImplType.NativeIndexer:
					return JsExpression.Index(thisAndArguments[0], thisAndArguments[1]);

				default: {
					_errorReporter.Message(Messages._7516, method.FullyQualifiedName());
					return JsExpression.Null;
				}
			}
		}

		private JsExpression ResolveTypeForInlineCode(string typeName) {
			var type = _semanticModel.Compilation.GetTypeByMetadataName(typeName);
			if (type == null) {
				_errorReporter.Message(Messages._7525, "Unknown type '" + typeName + "' specified in inline implementation");
				return JsExpression.Null;
			}
			else {
				return _runtimeLibrary.InstantiateType(type, this);
			}
		}

		private JsExpression CompileInlineCodeMethodInvocation(IMethodSymbol method, string code, JsExpression @this, IList<JsExpression> arguments) {
			var tokens = InlineCodeMethodCompiler.Tokenize(method, code, s => _errorReporter.Message(Messages._7525, s));
			if (tokens == null) {
				return JsExpression.Null;
			}
			if (tokens.Any(t => t.Type == InlineCodeToken.TokenType.Parameter && t.IsExpandedParamArray) && !(arguments[arguments.Count - 1] is JsArrayLiteralExpression)) {
				_errorReporter.Message(Messages._7525, string.Format("The {0} can only be invoked with its params parameter expanded", method.MethodKind == MethodKind.Constructor ? "constructor " + method.ContainingType.FullyQualifiedName() : ("method " + method.FullyQualifiedName())));
				return JsExpression.Null;
			}
			if (method.ReturnType.SpecialType == SpecialType.System_Void && method.MethodKind != MethodKind.Constructor) {
				_additionalStatements.AddRange(InlineCodeMethodCompiler.CompileStatementListInlineCodeMethodInvocation(method, tokens, @this, arguments, ResolveTypeForInlineCode, t => _runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, this), s => _errorReporter.Message(Messages._7525, s)));
				return JsExpression.Null;
			}
			else {
				return InlineCodeMethodCompiler.CompileExpressionInlineCodeMethodInvocation(method, tokens, @this, arguments, ResolveTypeForInlineCode, t => _runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, this), s => _errorReporter.Message(Messages._7525, s));
			}
		}

		private string GetMemberNameForJsonConstructor(ISymbol member) {
			if (member is IPropertySymbol) {
				var currentImpl = _metadataImporter.GetPropertySemantics((IPropertySymbol)member);
				if (currentImpl.Type == PropertyScriptSemantics.ImplType.Field) {
					return currentImpl.FieldName;
				}
				else {
					_errorReporter.Message(Messages._7517, member.FullyQualifiedName());
					return null;
				}
			}
			else if (member is IFieldSymbol) {
				var currentImpl = _metadataImporter.GetFieldSemantics((IFieldSymbol)member);
				if (currentImpl.Type == FieldScriptSemantics.ImplType.Field) {
					return currentImpl.Name;
				}
				else {
					_errorReporter.Message(Messages._7518, member.FullyQualifiedName());
					return null;
				}
			}
			else {
				_errorReporter.InternalError("Unsupported member " + member + " in object initializer.");
				return null;
			}
		}

		private JsExpression CompileJsonConstructorCall(IMethodSymbol constructor, ConstructorScriptSemantics impl, ArgumentMap argumentMap, IReadOnlyList<ExpressionSyntax> initializerStatements) {
			var jsPropertyNames = new List<string>();
			var expressions = new List<JsExpression>();
			// Add initializers for specified arguments.
			for (int i = 0; i < argumentMap.ArgumentToParameterMap.Length; i++) {
				var m = impl.ParameterToMemberMap[argumentMap.ArgumentToParameterMap[i]];
				string name = GetMemberNameForJsonConstructor(m);
				if (name != null) {
					jsPropertyNames.Add(name);
					expressions.Add(InnerCompile(argumentMap.ArgumentsForCall[argumentMap.ArgumentToParameterMap[i]], false, expressions));
				}
			}
			// Add initializers for initializer statements
			foreach (var init in initializerStatements) {
				var be = init as BinaryExpressionSyntax;
				if (be != null && be.CSharpKind() == SyntaxKind.SimpleAssignmentExpression && be.Left is SimpleNameSyntax) {
					var member = _semanticModel.GetSymbolInfo(be.Left).Symbol;
					string name = GetMemberNameForJsonConstructor(member);
					if (name != null) {
						if (jsPropertyNames.Contains(name)) {
							_errorReporter.Message(Messages._7527, member.Name);
						}
						else {
							jsPropertyNames.Add(name);
							expressions.Add(InnerCompile(be.Right, false, expressions));
						}
					}
				}
				else {
					_errorReporter.InternalError("Expected an assignment to an identifier, got " + init);
				}
			}
			// Add initializers for unspecified arguments
			for (int i = 0; i < argumentMap.ArgumentsForCall.Length; i++) {
				if (!argumentMap.ArgumentToParameterMap.Contains(i)) {
					string name = GetMemberNameForJsonConstructor(impl.ParameterToMemberMap[i]);
					if (name != null && !jsPropertyNames.Contains(name)) {
						jsPropertyNames.Add(name);
						expressions.Add(InnerCompile(argumentMap.ArgumentsForCall[i], false, expressions));
					}
				}
			}

			var jsProperties = new List<JsObjectLiteralProperty>();
			for (int i = 0; i < expressions.Count; i++)
				jsProperties.Add(new JsObjectLiteralProperty(jsPropertyNames[i], expressions[i]));
			return JsExpression.ObjectLiteral(jsProperties);
		}

		private void CompileInitializerStatementsInner(Func<JsExpression> getTarget, IReadOnlyList<ExpressionSyntax> initializerStatements) {
			foreach (var init in initializerStatements) {
				var collectionInitializer = (IMethodSymbol)_semanticModel.GetCollectionInitializerSymbolInfo(init).Symbol;
				if (collectionInitializer != null) {
					var impl = _metadataImporter.GetMethodSemantics(collectionInitializer);
					var arguments = init.CSharpKind() == SyntaxKind.ComplexElementInitializerExpression ? ((InitializerExpressionSyntax)init).Expressions : (IReadOnlyList<ExpressionSyntax>)new[] { init };

					var js = CompileMethodInvocation2(impl, collectionInitializer, _ => getTarget(), false, ArgumentMap.CreateIdentity(arguments), false);
					_additionalStatements.Add(js);
				}
				else {
					var be = init as BinaryExpressionSyntax;
					if (be != null && be.CSharpKind() == SyntaxKind.SimpleAssignmentExpression && be.Left is SimpleNameSyntax) {
						var member = _semanticModel.GetSymbolInfo(be.Left).Symbol;
						var nestedInitializer = be.Right as InitializerExpressionSyntax;
						if (nestedInitializer != null) {
							var apa = init.DescendantNodes().OfType<ExpressionSyntax>().Select(n => _semanticModel.GetCollectionInitializerSymbolInfo(n)).ToList();
							CompileInitializerStatementsInner(() => HandleMemberAccess(_ => getTarget(), member, false, false), nestedInitializer.Expressions);
						}
						else {
							var type = _semanticModel.GetTypeInfo(be).Type;
							var js = CompileMemberAssignment(_ => getTarget(), false, type, member, null, be.Right, null, (a, b) => b, false, false, false, false);
							_additionalStatements.Add(js);
						}
					}
					else {
						_errorReporter.InternalError("Expected an assignment to an identifier, got " + init);
					}
				}
			}
		}

		private JsExpression CompileInitializerStatements(JsExpression objectBeingInitialized, IReadOnlyList<ExpressionSyntax> initializerStatements) {
			if (initializerStatements != null && initializerStatements.Count > 0) {
				var tempVar = _createTemporaryVariable();
				var tempName = _variables[tempVar].Name;
				_additionalStatements.Add(JsStatement.Var(tempName, objectBeingInitialized));
				CompileInitializerStatementsInner(() => JsExpression.Identifier(tempName), initializerStatements);
				return JsExpression.Identifier(tempName);
			}
			else {
				return objectBeingInitialized;
			}
		}

		private JsExpression CompileConstructorInvocation(ConstructorScriptSemantics impl, IMethodSymbol method, ArgumentMap argumentMap, IReadOnlyList<ExpressionSyntax> initializerStatements) {
			var typeToConstruct = method.ContainingType;
			var typeToConstructDef = typeToConstruct.ConstructedFrom;
			if (typeToConstructDef != null && _metadataImporter.GetTypeSemantics(typeToConstructDef).Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_errorReporter.Message(Messages._7519, typeToConstruct.FullyQualifiedName());
				return JsExpression.Null;
			}
			if (typeToConstruct.TypeArguments.Length > 0) {
				var errors = Utils.FindGenericInstantiationErrors(typeToConstruct.TypeArguments, _metadataImporter);
				if (errors.HasErrors) {
					foreach (var ut in errors.UsedUnusableTypes)
						_errorReporter.Message(Messages._7520, ut.FullyQualifiedName(), typeToConstructDef.FullyQualifiedName());
					foreach (var t in errors.MutableValueTypesBoundToTypeArguments)
						_errorReporter.Message(Messages._7539, t.FullyQualifiedName());
					return JsExpression.Null;
				}
			}

			if (impl.Type == ConstructorScriptSemantics.ImplType.Json) {
				return CompileJsonConstructorCall(method, impl, argumentMap, initializerStatements);
			}
			else {
				string literalCode = GetActualInlineCode(impl, argumentMap.CanBeTreatedAsExpandedForm);
				var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, literalCode, _runtimeLibrary.InstantiateType(method.ContainingType, this), false, argumentMap);

				JsExpression constructorCall;

				switch (impl.Type) {
					case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
						constructorCall = CompileConstructorInvocationWithPotentialExpandParams(thisAndArguments.Skip(1).ToList(), thisAndArguments[0], impl.ExpandParams);
						break;

					case ConstructorScriptSemantics.ImplType.NamedConstructor:
						constructorCall = CompileConstructorInvocationWithPotentialExpandParams(thisAndArguments.Skip(1).ToList(), JsExpression.Member(thisAndArguments[0], impl.Name), impl.ExpandParams);
						break;

					case ConstructorScriptSemantics.ImplType.StaticMethod:
						constructorCall = CompileMethodInvocationWithPotentialExpandParams(new[] { JsExpression.Null }.Concat(thisAndArguments.Skip(1)).ToList(), JsExpression.Member(thisAndArguments[0], impl.Name), impl.ExpandParams, false);
						break;

					case ConstructorScriptSemantics.ImplType.InlineCode:
						constructorCall = CompileInlineCodeMethodInvocation(method, literalCode, null , thisAndArguments.Skip(1).ToList());
						break;

					default:
						_errorReporter.Message(Messages._7505);
						return JsExpression.Null;
				}

				return CompileInitializerStatements(constructorCall, initializerStatements);
			}
		}

		public override JsExpression VisitObjectCreationExpression(ObjectCreationExpressionSyntax node) {
			var type = _semanticModel.GetTypeInfo(node).Type;

			if (type.TypeKind == TypeKind.Enum) {
				#warning TODO: What about [NamedValues]
				return JsExpression.Number(0);
			}
			else if (type.TypeKind == TypeKind.TypeParameter) {
				var activator = _semanticModel.Compilation.GetTypeByMetadataName(typeof(System.Activator).FullName);
				var createInstance = activator.GetMembers("CreateInstance").OfType<IMethodSymbol>().Single(m => m.IsStatic && m.TypeParameters.Length == 1 && m.Parameters.Length == 0);
				var createInstanceSpec = createInstance.Construct(type);
				var createdObject = CompileMethodInvocation(_metadataImporter.GetMethodSemantics(createInstanceSpec), createInstanceSpec, new[] { _runtimeLibrary.InstantiateType(activator, this) }, false);
				return CompileInitializerStatements(createdObject, node.Initializer != null ? (IReadOnlyList<ExpressionSyntax>)node.Initializer.Expressions : ImmutableArray<ExpressionSyntax>.Empty);
			}
			else if (type.TypeKind == TypeKind.Delegate && node.ArgumentList != null && node.ArgumentList.Arguments.Count == 1) {
				var arg = node.ArgumentList.Arguments[0].Expression;
				var conversion = _semanticModel.GetConversion(arg);
				if (conversion.IsAnonymousFunction || conversion.IsMethodGroup) {
					return Visit(arg);
				}
				else {
					var sourceType = _semanticModel.GetTypeInfo(arg).Type;
					var targetSem = _metadataImporter.GetDelegateSemantics((INamedTypeSymbol)type.OriginalDefinition);
					var sourceSem = _metadataImporter.GetDelegateSemantics((INamedTypeSymbol)sourceType.OriginalDefinition);
					if (targetSem.BindThisToFirstParameter != sourceSem.BindThisToFirstParameter) {
						_errorReporter.Message(Messages._7533, type.FullyQualifiedName(), sourceType.FullyQualifiedName());
						return JsExpression.Null;
					}
					if (targetSem.ExpandParams != sourceSem.ExpandParams) {
						_errorReporter.Message(Messages._7537, type.FullyQualifiedName(), sourceType.FullyQualifiedName());
						return JsExpression.Null;
					}

					if (sourceType.TypeKind == TypeKind.Delegate) {
						return _runtimeLibrary.CloneDelegate(Visit(arg), sourceType, type, this);
					}
					else {
						_errorReporter.InternalError("Unexpected delegate construction " + node);
						return JsExpression.Null;
					}
				}
			}
			else {
				var ctor = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
				return CompileConstructorInvocation(_metadataImporter.GetConstructorSemantics(ctor), ctor, _semanticModel.GetArgumentMap(node), node.Initializer != null ? (IReadOnlyList<ExpressionSyntax>)node.Initializer.Expressions : ImmutableArray<ExpressionSyntax>.Empty);
			}
		}

		public override JsExpression VisitInvocationExpression(InvocationExpressionSyntax node) {
			var method = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
			if (method == null) {
				_errorReporter.InternalError("Invocation of non-method");
			}

			if (method.ContainingType.TypeKind == TypeKind.Delegate && method.Name == "Invoke") {
				var sem = _metadataImporter.GetDelegateSemantics(method.ContainingType);
			
				var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, null, InnerCompile(node.Expression, usedMultipleTimes: false, returnMultidimArrayValueByReference: true), false, _semanticModel.GetArgumentMap(node));
				var methodExpr = thisAndArguments[0];
				thisAndArguments = thisAndArguments.Skip(1).ToList();
			
				if (sem.BindThisToFirstParameter) {
					return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, methodExpr, sem.ExpandParams, true);
				}
				else {
					thisAndArguments.Insert(0, JsExpression.Null);
					return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, methodExpr, sem.ExpandParams, false);
				}
			}

			if (node.Expression is MemberAccessExpressionSyntax) {
				var mae = (MemberAccessExpressionSyntax)node.Expression;
				return CompileMethodInvocation2(_metadataImporter.GetMethodSemantics(method), method, usedMultipleTimes => InnerCompile(mae.Expression, usedMultipleTimes, returnMultidimArrayValueByReference: true), IsReadonlyField(mae.Expression), _semanticModel.GetArgumentMap(node), node.Expression.IsNonVirtualAccess());
			}
			else {
				return CompileMethodInvocation2(_metadataImporter.GetMethodSemantics(method), method, usedMultipleTimes => CompileThis(), IsReadonlyField(node.Expression), _semanticModel.GetArgumentMap(node), node.Expression.IsNonVirtualAccess());
			}
		}

		public override JsExpression VisitLiteralExpression(LiteralExpressionSyntax node) {
			var value = _semanticModel.GetConstantValue(node);
			if (!value.HasValue) {
				_errorReporter.InternalError("Literal does not have constant value");
				return JsExpression.Null;
			}
			return JSModel.Utils.MakeConstantExpression(value.Value);
		}

		public override JsExpression VisitDefaultExpression(DefaultExpressionSyntax node) {
			return _runtimeLibrary.Default(_semanticModel.GetTypeInfo(node).Type, this);
		}

		private JsExpression CompileThis() {
			if (_thisAlias != null) {
				return JsExpression.Identifier(_thisAlias);
			}
			else if (_nestedFunctionContext != null && _nestedFunctionContext.CapturedByRefVariables.Count != 0) {
				return JsExpression.Member(JsExpression.This, _namer.ThisAlias);
			}
			else {
				return JsExpression.This;
			}
		}

		public override JsExpression VisitThisExpression(ThisExpressionSyntax node) {
			return CompileThis();
		}

		public override JsExpression VisitBaseExpression(BaseExpressionSyntax node) {
			return CompileThis();
		}

		private JsExpression CompileLocal(ISymbol variable, bool returnReference) {
			var data = _variables[variable];
			if (data.UseByRefSemantics) {
				var target = _nestedFunctionContext != null && _nestedFunctionContext.CapturedByRefVariables.Contains(variable)
				           ? (JsExpression)JsExpression.Member(JsExpression.This, data.Name)	// If using a captured by-ref variable, we access it using this.name.$
				           : (JsExpression)JsExpression.Identifier(data.Name);

				return returnReference ? target : JsExpression.Member(target, "$");
			}
			else {
				return JsExpression.Identifier(_variables[variable].Name);
			}
		}

		public override JsExpression VisitIdentifierName(IdentifierNameSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
			var conversion = _semanticModel.GetConversion(node);
			if (conversion.IsMethodGroup) {
				var targetType = _semanticModel.GetTypeInfo(node).ConvertedType;
				return PerformMethodGroupConversion(_ => symbol.IsStatic ? _runtimeLibrary.InstantiateType(_typeBeingCompiled, this) : CompileThis(), (INamedTypeSymbol)targetType, (IMethodSymbol)symbol, false);
			}
			else {
				if (symbol is ILocalSymbol || symbol is IParameterSymbol) {
					return CompileLocal(_semanticModel.GetSymbolInfo(node).Symbol, false);
				}
				else if (symbol is IMethodSymbol || symbol is IPropertySymbol || symbol is IFieldSymbol || symbol is IEventSymbol) {
					return HandleMemberAccess(usedMultipleTimes => CompileThis(), _semanticModel.GetSymbolInfo(node).Symbol, false, IsReadonlyField(node));
				}
				else {
					_errorReporter.InternalError("Cannot handle identifier " + node);
					return JsExpression.Null;
				}
			}
		}

		public override JsExpression VisitGenericName(GenericNameSyntax node) {
			var conversion = _semanticModel.GetConversion(node);
			if (conversion.IsMethodGroup) {
				var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
				var targetType = _semanticModel.GetTypeInfo(node).ConvertedType;
				return PerformMethodGroupConversion(_ => symbol.IsStatic ? _runtimeLibrary.InstantiateType(_typeBeingCompiled, this) : CompileThis(), (INamedTypeSymbol)targetType, (IMethodSymbol)symbol, false);
			}
			else {
				_errorReporter.InternalError("Unexpected generic name " + node);
				return JsExpression.Null;
			}
		}

		public override JsExpression VisitTypeOfExpression(TypeOfExpressionSyntax node) {
			var type = (ITypeSymbol)_semanticModel.GetSymbolInfo(node.Type).Symbol;
			var errors = Utils.FindTypeUsageErrors(new[] { type }, _metadataImporter);
			if (errors.HasErrors) {
				foreach (var ut in errors.UsedUnusableTypes)
					_errorReporter.Message(Messages._7522, ut.FullyQualifiedName());
				foreach (var t in errors.MutableValueTypesBoundToTypeArguments)
					_errorReporter.Message(Messages._7539, t.FullyQualifiedName());

				return JsExpression.Null;
			}
			else
				return _runtimeLibrary.TypeOf(type, this);
		}

		public override JsExpression VisitElementAccessExpression(ElementAccessExpressionSyntax node) {
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
			var type = _semanticModel.GetTypeInfo(node).Type;

			if (symbol is IPropertySymbol) {
				var property = (IPropertySymbol)symbol;
				var impl = _metadataImporter.GetPropertySemantics(property);
				if (impl.Type != PropertyScriptSemantics.ImplType.GetAndSetMethods) {
					_errorReporter.InternalError("Cannot invoke property that does not have a get method.");
					return JsExpression.Null;
				}
				return CompileMethodInvocation2(impl.GetMethod, property.GetMethod, usedMultipleTimes => InnerCompile(node.Expression, usedMultipleTimes), IsReadonlyField(node.Expression), _semanticModel.GetArgumentMap(node), node.IsNonVirtualAccess());
			}
			else {
				var expressions = new List<JsExpression>();
				expressions.Add(InnerCompile(node.Expression, false, returnMultidimArrayValueByReference: true));
				foreach (var i in node.ArgumentList.Arguments)
					expressions.Add(InnerCompile(i.Expression, false, expressions));

				if (node.ArgumentList.Arguments.Count == 1) {
					return JsExpression.Index(expressions[0], expressions[1]);
				}
				else {
					var result = _runtimeLibrary.GetMultiDimensionalArrayValue(expressions[0], expressions.Skip(1), this);
					if (!_returnMultidimArrayValueByReference) {
						type = type.UnpackNullable();
						if (IsMutableValueType(type)) {
							result = _runtimeLibrary.CloneValueType(result, type, this);
						}
					}
					return result;
				}
			}
		}

		private JsExpression HandleArrayCreation(IArrayTypeSymbol arrayType, InitializerExpressionSyntax initializer, IReadOnlyList<ArrayRankSpecifierSyntax> rankSpecifiers) {
			if (arrayType.Rank == 1) {
				if (initializer != null && initializer.Expressions.Count > 0) {
					var expressions = new List<JsExpression>();
					foreach (var init in initializer.Expressions)
						expressions.Add(MaybeCloneValueType(InnerCompile(init, false, expressions), init, arrayType.ElementType));
					return JsExpression.ArrayLiteral(expressions);
				}
				else {
					var rank = _semanticModel.GetConstantValue(rankSpecifiers[0].Sizes[0]);
					if ((initializer != null && initializer.Expressions.Count == 0) || (rank.HasValue && Convert.ToInt64(rank.Value) == 0)) {
						return JsExpression.ArrayLiteral();
					}
					else {
						return _runtimeLibrary.CreateArray(arrayType.ElementType, new[] { InnerCompile(rankSpecifiers[0].Sizes[0], false) }, this);
					}
				}
			}
			else {
				var sizes = new List<JsExpression>();
				foreach (var a in rankSpecifiers[0].Sizes)
					sizes.Add(InnerCompile(a, false, sizes));
				var result = _runtimeLibrary.CreateArray(arrayType.ElementType, sizes, this);

				if (initializer != null && initializer.Expressions.Count > 0) {
					var temp = _createTemporaryVariable();
					_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, result));
					result = JsExpression.Identifier(_variables[temp].Name);

					var expressions = new List<JsExpression>();
					foreach (var ie in initializer.Expressions)
						expressions.Add(InnerCompile(ie, false, expressions));

					var indices = new JsExpression[rankSpecifiers[0].Sizes.Count];
					#warning TODO
					//for (int i = 0; i < rr.InitializerElements.Count; i++) {
					//	int remainder = i;
					//	for (int j = indices.Length - 1; j >= 0; j--) {
					//		int arg = Convert.ToInt32(rr.SizeArguments[j].ConstantValue);
					//		indices[j] = JsExpression.Number(remainder % arg);
					//		remainder /= arg;
					//	}
					//
					//	_additionalStatements.Add(_runtimeLibrary.SetMultiDimensionalArrayValue(result, indices, MaybeCloneValueType(expressions[i], rr.InitializerElements[i], arrayType.ElementType), this));
					//}
				}

				return result;
			}

		}

		public override JsExpression VisitArrayCreationExpression(ArrayCreationExpressionSyntax node) {
			return HandleArrayCreation((IArrayTypeSymbol)_semanticModel.GetTypeInfo(node).Type, node.Initializer, node.Type.RankSpecifiers);
		}

		public override JsExpression VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node) {
			return HandleArrayCreation((IArrayTypeSymbol)_semanticModel.GetTypeInfo(node).Type, node.Initializer, null);
		}

#if false
		public override JsExpression VisitArrayCreateResolveResult(ArrayCreateResolveResult rr, bool returnValueIsImportant) {
			var at = (ArrayType)rr.Type;

			if (at.Dimensions == 1) {
				if (rr.InitializerElements != null && rr.InitializerElements.Count > 0) {
					// return CreateArray(rr.InitializerElements)
					var expressions = new List<JsExpression>();
					foreach (var init in rr.InitializerElements)
						expressions.Add(MaybeCloneValueType(InnerCompile(init, false, expressions), init, at.ElementType));
					return JsExpression.ArrayLiteral(expressions);
				}
				else if (rr.SizeArguments[0].IsCompileTimeConstant && Convert.ToInt64(rr.SizeArguments[0].ConstantValue) == 0) {
					return JsExpression.ArrayLiteral();
				}
				else {
					return _runtimeLibrary.CreateArray(at.ElementType, new[] { InnerCompile(rr.SizeArguments[0], false) }, this);
				}
			}
			else {
				var sizes = new List<JsExpression>();
				foreach (var a in rr.SizeArguments)
					sizes.Add(InnerCompile(a, false, sizes));
				var result = _runtimeLibrary.CreateArray(at.ElementType, sizes, this);

				if (rr.InitializerElements != null && rr.InitializerElements.Count > 0) {
					var temp = _createTemporaryVariable(rr.Type);
					_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, result));
					result = JsExpression.Identifier(_variables[temp].Name);

					var expressions = new List<JsExpression>();
					foreach (var ie in rr.InitializerElements)
						expressions.Add(InnerCompile(ie, false, expressions));

					var indices = new JsExpression[rr.SizeArguments.Count];
					for (int i = 0; i < rr.InitializerElements.Count; i++) {
						int remainder = i;
						for (int j = indices.Length - 1; j >= 0; j--) {
							int arg = Convert.ToInt32(rr.SizeArguments[j].ConstantValue);
							indices[j] = JsExpression.Number(remainder % arg);
							remainder /= arg;
						}

						_additionalStatements.Add(_runtimeLibrary.SetMultiDimensionalArrayValue(result, indices, MaybeCloneValueType(expressions[i], rr.InitializerElements[i], at.ElementType), this));
					}
				}

				return result;
			}
		}

		public override JsExpression VisitTypeIsResolveResult(TypeIsResolveResult rr, bool returnValueIsImportant) {
			var targetType = UnpackNullable(rr.TargetType);
			return _runtimeLibrary.TypeIs(VisitResolveResult(rr.Input, returnValueIsImportant), rr.Input.Type, targetType, this);
		}

		public override JsExpression VisitByReferenceResolveResult(ByReferenceResolveResult rr, bool returnValueIsImportant) {
			_errorReporter.InternalError("Resolve result " + rr.ToString() + " should have been handled in method call.");
			return JsExpression.Null;
		}

		public override JsExpression VisitDefaultResolveResult(ResolveResult rr, bool returnValueIsImportant) {
			if (rr.Type.Kind == TypeKind.Null)
				return JsExpression.Null;
			_errorReporter.InternalError("Resolve result " + rr + " is not handled.");
			return JsExpression.Null;
		}
#endif

		private JsExpression PerformConversion(JsExpression input, Conversion c, ITypeSymbol fromType, ITypeSymbol toType, ExpressionSyntax csharpInput) {
			if (c.IsIdentity) {
				return input;
			}
			else if (c.IsMethodGroup || c.IsAnonymousFunction) {
				return input;	// Conversion should have been performed as part of processing the converted expression
			}
			else if (c.IsReference) {
				if (fromType == null)
					return input;	// Null literal (Isn't this a NullLiteral conversion? Roslyn bug?)
				if (toType.TypeKind == TypeKind.ArrayType && fromType.TypeKind == TypeKind.ArrayType)	// Array covariance / contravariance.
					return input;
				else if (toType.TypeKind == TypeKind.DynamicType)
					return input;
				else if (toType.TypeKind == TypeKind.Delegate && fromType.TypeKind == TypeKind.Delegate && toType.SpecialType != SpecialType.System_MulticastDelegate && fromType.SpecialType != SpecialType.System_MulticastDelegate)
					return input;	// Conversion between compatible delegate types.
				else if (c.IsImplicit)
					return _runtimeLibrary.Upcast(input, fromType, toType, this);
				else
					return _runtimeLibrary.Downcast(input, fromType, toType, this);
			}
			else if (c.IsNumeric || c.IsNullable) {
				var result = input;
				if (fromType.IsNullable() && !toType.IsNullable())
					result = _runtimeLibrary.FromNullable(result, this);

				if (!IsIntegerType(fromType) && fromType.TypeKind != TypeKind.Enum && IsIntegerType(toType)) {
					result = _runtimeLibrary.FloatToInt(result, this);

					if (fromType.IsNullable() && toType.IsNullable()) {
						result = _runtimeLibrary.Lift(result, this);
					}
				}
				return result;
			}
			else if (c.IsDynamic) {
				JsExpression result;
				if (toType.IsNullable()) {
					// Unboxing to nullable type.
					result = _runtimeLibrary.Downcast(input, fromType, toType.UnpackNullable(), this);
				}
				else if (toType.TypeKind == TypeKind.Struct) {
					// Unboxing to non-nullable type.
					result = _runtimeLibrary.FromNullable(_runtimeLibrary.Downcast(input, fromType, toType, this), this);
				}
				else {
					// Converting to a boring reference type.
					result = _runtimeLibrary.Downcast(input, fromType, toType, this);
				}
				return MaybeCloneValueType(result, null, toType, forceClone: true);
			}
			else if (c.IsEnumeration) {
				if (fromType.IsNullable() && !toType.IsNullable())
					return _runtimeLibrary.FromNullable(input, this);
				return input;
			}
			else if (c.IsBoxing) {
				var box = MaybeCloneValueType(input, null, fromType);

				// Conversion between type parameters are classified as boxing conversions, so it's sometimes an upcast, sometimes a downcast.
				if (toType.TypeKind == TypeKind.DynamicType) {
					return box;
				}
				else if (fromType.UnpackNullable().GetAllBaseTypes().Contains(toType)) {
					return _runtimeLibrary.Upcast(box, fromType, toType, this);
				}
				else {
					return _runtimeLibrary.Downcast(box, fromType, toType, this);
				}
			}
			else if (c.IsUnboxing) {
				JsExpression result;
				if (toType.IsNullable()) {
					result = _runtimeLibrary.Downcast(input, fromType, toType.UnpackNullable(), this);
				}
				else {
					result = _runtimeLibrary.Downcast(input, fromType, toType, this);
					if (toType.TypeKind == TypeKind.Struct)
						result = _runtimeLibrary.FromNullable(result, this);	// hidden gem in the C# spec: conversions involving type parameter which are not known to not be unboxing are considered unboxing conversions.
				}
				return MaybeCloneValueType(result, null, toType, forceClone: true);
			}
			else if (c.IsUserDefined) {
				input = PerformConversion(input, c.UserDefinedFromConversion(), fromType, c.MethodSymbol.Parameters[0].Type, csharpInput);
				var impl = _metadataImporter.GetMethodSemantics(c.MethodSymbol);
				var result = CompileMethodInvocation(impl, c.MethodSymbol, new[] { _runtimeLibrary.InstantiateType(c.MethodSymbol.ContainingType, this), input }, false);
				if (_semanticModel.IsLiftedConversion(c, csharpInput))
					result = _runtimeLibrary.Lift(result, this);
				result = PerformConversion(result, c.UserDefinedToConversion(), c.MethodSymbol.ReturnType, toType, null);
				return result;
			}
			else if (c.IsNullLiteral || c.IsConstantExpression) {
				return input;
			}
			else {
				_errorReporter.InternalError("Conversion " + c + " is not implemented");
				return JsExpression.Null;
			}
		}

		private JsExpression PerformMethodGroupConversionOnNormalMethod(IMethodSymbol method, ITypeSymbol delegateType, bool isBaseCall, bool isExtensionMethodGroupConversion, Func<bool, JsExpression> getTarget, MethodScriptSemantics methodSemantics, DelegateScriptSemantics delegateSemantics) {
			if (methodSemantics.ExpandParams != delegateSemantics.ExpandParams) {
				_errorReporter.Message(Messages._7524, method.FullyQualifiedName(), delegateType.FullyQualifiedName());
				return JsExpression.Null;
			}

			var typeArguments = methodSemantics.IgnoreGenericArguments ? ImmutableArray<ITypeSymbol>.Empty : method.TypeArguments;

			JsExpression result;

			if (isBaseCall) {
				// base.Method
				var jsTarget = getTarget(true);
				result = _runtimeLibrary.BindBaseCall(method, jsTarget, this);
			}
			else if (isExtensionMethodGroupConversion) {
				IList<string> parameters;
				JsExpression body;
				var jsTarget = getTarget(true);
				if (methodSemantics.ExpandParams) {
					parameters = ImmutableArray<string>.Empty;
					body = JsExpression.Invocation(JsExpression.Member(JsExpression.Member(_runtimeLibrary.InstantiateType(method.ContainingType, this), methodSemantics.Name), "apply"), JsExpression.Null, JsExpression.Invocation(JsExpression.Member(JsExpression.ArrayLiteral(jsTarget), "concat"), JsExpression.Invocation(JsExpression.Member(JsExpression.Member(JsExpression.Member(JsExpression.Identifier("Array"), "prototype"), "slice"), "call"), JsExpression.Identifier("arguments"))));
				}
				else {
					parameters = new string[method.Parameters.Length - 1];
					for (int i = 0; i < parameters.Count; i++)
						parameters[i] = _variables[_createTemporaryVariable()].Name;
					body = CompileMethodInvocation(methodSemantics, method, new[] { _runtimeLibrary.InstantiateType(method.ContainingType, this), jsTarget }.Concat(parameters.Select(JsExpression.Identifier)).ToList(), false);
				}
				result = JsExpression.FunctionDefinition(parameters, method.ReturnType.SpecialType == SpecialType.System_Void ? (JsStatement)body : JsStatement.Return(body));
				if (UsesThisVisitor.Analyze(body))
					result = _runtimeLibrary.Bind(result, JsExpression.This, this);
			}
			else {
				JsExpression jsTarget, jsMethod;

				if (method.IsStatic) {
					jsTarget = null;
					jsMethod = JsExpression.Member(_runtimeLibrary.InstantiateType(method.ContainingType, this), methodSemantics.Name);
				}
				else {
					jsTarget = getTarget(true);
					jsMethod = JsExpression.Member(jsTarget, methodSemantics.Name);
				}

				if (typeArguments.Length > 0) {
					jsMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments, this);
				}

				result = jsTarget != null ? _runtimeLibrary.Bind(jsMethod, jsTarget, this) : jsMethod;
			}

			if (delegateSemantics.BindThisToFirstParameter)
				result = _runtimeLibrary.BindFirstParameterToThis(result, this);

			return result;
		}

		private JsExpression PerformMethodGroupConversionOnInlineCodeMethod(IMethodSymbol method, ITypeSymbol delegateType, bool isBaseCall, bool isExtensionMethodGroupConversion, Func<bool, JsExpression> getTarget, MethodScriptSemantics methodSemantics, DelegateScriptSemantics delegateSemantics) {
			string code = isBaseCall ? methodSemantics.NonVirtualInvocationLiteralCode : methodSemantics.NonExpandedFormLiteralCode;
			var tokens = InlineCodeMethodCompiler.Tokenize(method, code, s => _errorReporter.Message(Messages._7525, s));
			if (tokens == null) {
				return JsExpression.Null;
			}
			else if (tokens.Any(t => t.Type == InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier)) {
				_errorReporter.Message(Messages._7523, method.FullyQualifiedName(), "it uses a literal string as code ({@arg})");
				return JsExpression.Null;
			}
			else if (tokens.Any(t => t.Type == InlineCodeToken.TokenType.Parameter && t.IsExpandedParamArray)) {
				_errorReporter.Message(Messages._7523, method.FullyQualifiedName(), "it has an expanded param array parameter ({*arg})");
				return JsExpression.Null;
			}

			var parameters = new string[method.Parameters.Length - (delegateSemantics.ExpandParams ? 1 : 0) - (isExtensionMethodGroupConversion ? 1 : 0)];
			for (int i = 0; i < parameters.Length; i++)
				parameters[i] = _variables[_createTemporaryVariable()].Name;

			var jsTarget = method.IsStatic && !isExtensionMethodGroupConversion ? JsExpression.Null : getTarget(tokens.Count(t => t.Type == InlineCodeToken.TokenType.This) > 1);
			var arguments = new List<JsExpression>();
			if (isExtensionMethodGroupConversion)
				arguments.Add(jsTarget);
			arguments.AddRange(parameters.Select(p => (JsExpression)JsExpression.Identifier(p)));
			if (delegateSemantics.ExpandParams)
				arguments.Add(JsExpression.Invocation(JsExpression.Member(JsExpression.Member(JsExpression.Member(JsExpression.Identifier("Array"), "prototype"), "slice"), "call"), JsExpression.Identifier("arguments"), JsExpression.Number(parameters.Length)));

			bool usesThis;
			JsExpression result;
			if (method.ReturnType.SpecialType == SpecialType.System_Void) {
				var list = InlineCodeMethodCompiler.CompileStatementListInlineCodeMethodInvocation(method,
				                                                                                   tokens,
				                                                                                   method.IsStatic ? null : jsTarget,
				                                                                                   arguments,
				                                                                                   ResolveTypeForInlineCode,
				                                                                                   t => _runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, this),
				                                                                                   s => _errorReporter.Message(Messages._7525, s));
				var body = JsStatement.Block(list);
				result = JsExpression.FunctionDefinition(parameters, body);
				usesThis = UsesThisVisitor.Analyze(body);
			}
			else {
				var body = InlineCodeMethodCompiler.CompileExpressionInlineCodeMethodInvocation(method,
				                                                                                tokens,
				                                                                                method.IsStatic ? null : jsTarget,
				                                                                                arguments,
				                                                                                ResolveTypeForInlineCode,
				                                                                                t => _runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, this),
				                                                                                s => _errorReporter.Message(Messages._7525, s));
				result = JsExpression.FunctionDefinition(parameters, JsStatement.Return(body));
				usesThis = UsesThisVisitor.Analyze(body);
			}

			if (usesThis)
				result = _runtimeLibrary.Bind(result, JsExpression.This, this);
			if (delegateSemantics.BindThisToFirstParameter)
				result = _runtimeLibrary.BindFirstParameterToThis(result, this);
			return result;
		}

		private JsExpression PerformMethodGroupConversionOnStaticMethodWithThisAsFirstArgument(IMethodSymbol method, ITypeSymbol delegateType, bool isBaseCall, Func<bool, JsExpression> getTarget, MethodScriptSemantics methodSemantics, DelegateScriptSemantics delegateSemantics) {
			if (methodSemantics.ExpandParams != delegateSemantics.ExpandParams) {
				_errorReporter.Message(Messages._7524, method.FullyQualifiedName(), delegateType.FullyQualifiedName());
				return JsExpression.Null;
			}

			JsExpression result;
			if (methodSemantics.ExpandParams) {
				var body = JsExpression.Invocation(JsExpression.Member(JsExpression.Member(_runtimeLibrary.InstantiateType(method.ContainingType, this), methodSemantics.Name), "apply"), JsExpression.Null, JsExpression.Invocation(JsExpression.Member(JsExpression.ArrayLiteral(JsExpression.This), "concat"), JsExpression.Invocation(JsExpression.Member(JsExpression.Member(JsExpression.Member(JsExpression.Identifier("Array"), "prototype"), "slice"), "call"), JsExpression.Identifier("arguments"))));
				result = JsExpression.FunctionDefinition(new string[0], method.ReturnType.SpecialType == SpecialType.System_Void ? (JsStatement)body : JsStatement.Return(body));
			}
			else {
				var parameters = new string[method.Parameters.Length];
				for (int i = 0; i < method.Parameters.Length; i++)
					parameters[i] = _variables[_createTemporaryVariable()].Name;

				var body = JsExpression.Invocation(JsExpression.Member(_runtimeLibrary.InstantiateType(method.ContainingType, this), methodSemantics.Name), new[] { JsExpression.This }.Concat(parameters.Select(p => (JsExpression)JsExpression.Identifier(p))));
				result = JsExpression.FunctionDefinition(parameters, method.ReturnType.SpecialType == SpecialType.System_Void ? (JsStatement)body : JsStatement.Return(body));
			}

			result = _runtimeLibrary.Bind(result, getTarget(false), this);
			if (delegateSemantics.BindThisToFirstParameter)
				result = _runtimeLibrary.BindFirstParameterToThis(result, this);
			return result;
		}

		public override JsExpression VisitCastExpression(CastExpressionSyntax node) {
			var info = _semanticModel.GetCastInfo(node);
			var input = Visit(node.Expression, true);
			return PerformConversion(input, info.Conversion, info.FromType, info.ToType, node.Expression);
		}

		private JsExpression PerformMethodGroupConversion(Func<bool, JsExpression> getTarget, INamedTypeSymbol targetType, IMethodSymbol symbol, bool isNonVirtualLookup) {
			var methodSemantics = _metadataImporter.GetMethodSemantics(symbol);
			var delegateSemantics = _metadataImporter.GetDelegateSemantics(targetType);
			switch (methodSemantics.Type) {
				case MethodScriptSemantics.ImplType.NormalMethod:
					return PerformMethodGroupConversionOnNormalMethod(symbol, targetType, symbol.IsOverridable() && isNonVirtualLookup, symbol.ReducedFrom != null, getTarget, methodSemantics, delegateSemantics);
				case MethodScriptSemantics.ImplType.InlineCode:
					return PerformMethodGroupConversionOnInlineCodeMethod(symbol, targetType, symbol.IsOverridable() && isNonVirtualLookup, symbol.ReducedFrom != null, getTarget, methodSemantics, delegateSemantics);
				case MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument:
					return PerformMethodGroupConversionOnStaticMethodWithThisAsFirstArgument(symbol, targetType, symbol.IsOverridable() && isNonVirtualLookup, getTarget, methodSemantics, delegateSemantics);
				default:
					_errorReporter.Message(Messages._7523, symbol.FullyQualifiedName(), "it is not a normal method");
					return JsExpression.Null;
			}
		}

		private JsExpression PerformExpressionTreeLambdaConversion(ExpressionSyntax body) {
			var tree = new ExpressionTreeBuilder(_semanticModel,
					                             _metadataImporter,
					                             () => { var v = _createTemporaryVariable(); return _variables[v].Name; },
					                             (m, t, a) => {
					                                 var c = Clone();
					                                 c._additionalStatements = new List<JsStatement>();
					                                 var sem = _metadataImporter.GetMethodSemantics(m);
					                                 if (sem.Type == MethodScriptSemantics.ImplType.InlineCode) {
					                                     var tokens = InlineCodeMethodCompiler.Tokenize(m, sem.LiteralCode, _ => {});
					                                     if (tokens != null) {
					                                         for (int i = 0; i < a.Length; i++) {
					                                             if (tokens.Count(k => k.Type == InlineCodeToken.TokenType.Parameter && k.Index == i) > 1) {
					                                                 if (IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(a[i])) {
					                                                     var temp = _createTemporaryVariable();
					                                                     c._additionalStatements.Add(JsStatement.Var(_variables[temp].Name, a[i]));
					                                                     a[i] = JsExpression.Identifier(_variables[temp].Name);
					                                                 }
					                                             }
					                                         }
					                                     }
					                                 }
					                                 var e = c.CompileMethodInvocation(_metadataImporter.GetMethodSemantics(m), m, new[] { m.IsStatic ? _runtimeLibrary.InstantiateType(m.ContainingType, this) : t }.Concat(a).ToList(), false);
					                                 return new ExpressionCompileResult(e, c._additionalStatements);
					                             },
					                             t => _runtimeLibrary.InstantiateType(t, this),
					                             t => _runtimeLibrary.Default(t, this),
					                             m => _runtimeLibrary.GetMember(m, this),
					                             v => _runtimeLibrary.GetExpressionForLocal(v.Name, CompileLocal(v, false), v.Type, this),
					                             CompileThis()
					                            ).BuildExpressionTree(body);
			_additionalStatements.AddRange(tree.AdditionalStatements);
			return tree.Expression;
		}

		private JsExpression CompileLambda(SyntaxNode lambdaNode, IReadOnlyList<IParameterSymbol> lambdaParameters, SyntaxNode body, bool isAsync, INamedTypeSymbol delegateType, DelegateScriptSemantics semantics) {
			var methodType = delegateType.DelegateInvokeMethod;
			var f = _nestedFunctions[lambdaNode];

			var capturedByRefVariables = f.DirectlyOrIndirectlyUsedVariables.Where(v => _variables[v].UseByRefSemantics).ToList();
			if (capturedByRefVariables.Count > 0) {
				var allParents = f.AllParents;
				capturedByRefVariables.RemoveAll(v => !allParents.Any(p => p.DirectlyDeclaredVariables.Contains(v)));	// Remove used byref variables that were declared in this method or any nested method.
			}

			bool captureThis = (_thisAlias == null && f.DirectlyOrIndirectlyUsesThis);
			var newContext = new NestedFunctionContext(capturedByRefVariables);

			JsFunctionDefinitionExpression def;
			if (body is StatementSyntax) {
				StateMachineType smt = StateMachineType.NormalMethod;
				ITypeSymbol taskGenericArgument = null;
				if (isAsync) {
					smt = methodType.ReturnsVoid ? StateMachineType.AsyncVoid : StateMachineType.AsyncTask;
					taskGenericArgument = methodType.TypeArguments.Length > 0 ? methodType.TypeArguments[0] : null;
				}

				def = _createInnerCompiler(newContext).CompileMethod(lambdaParameters, _variables, (BlockSyntax)body, false, semantics.ExpandParams, smt, taskGenericArgument);
			}
			else {
				var innerResult = CloneAndCompile((ExpressionSyntax)body, !methodType.ReturnsVoid, nestedFunctionContext: newContext);
				var lastStatement = methodType.ReturnsVoid ? (JsStatement)innerResult.Expression : JsStatement.Return(MaybeCloneValueType(innerResult.Expression, (ExpressionSyntax)body, methodType.ReturnType));
				var jsBody = JsStatement.Block(MethodCompiler.PrepareParameters(lambdaParameters, _variables, expandParams: semantics.ExpandParams, staticMethodWithThisAsFirstArgument: false).Concat(innerResult.AdditionalStatements).Concat(new[] { lastStatement }));
				def = JsExpression.FunctionDefinition(lambdaParameters.Where((p, i) => i != lambdaParameters.Count - 1 || !semantics.ExpandParams).Select(p => _variables[p].Name), jsBody);
			}

			JsExpression captureObject;
			if (newContext.CapturedByRefVariables.Count > 0) {
				var toCapture = newContext.CapturedByRefVariables.Select(v => new JsObjectLiteralProperty(_variables[v].Name, CompileLocal(v, true))).ToList();
				if (captureThis)
					toCapture.Add(new JsObjectLiteralProperty(_namer.ThisAlias, CompileThis()));
				captureObject = JsExpression.ObjectLiteral(toCapture);
			}
			else if (captureThis) {
				captureObject = CompileThis();
			}
			else {
				captureObject = null;
			}

			var result = captureObject != null ? _runtimeLibrary.Bind(def, captureObject, this) : def;
			if (semantics.BindThisToFirstParameter)
				result = _runtimeLibrary.BindFirstParameterToThis(result, this);
			return result;
		}

		public override JsExpression VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) {
			var targetType = (INamedTypeSymbol)_semanticModel.GetTypeInfo(node).ConvertedType;
			if (targetType.ContainingNamespace.Name == typeof(System.Linq.Expressions.Expression).Namespace && targetType.ContainingNamespace.Name == typeof(System.Linq.Expressions.Expression).Name) {
				return PerformExpressionTreeLambdaConversion((ExpressionSyntax)node.Body);
			}
			else {
				var sem = _metadataImporter.GetDelegateSemantics(targetType);
				var lambdaSymbol = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
				return CompileLambda(node, lambdaSymbol.Parameters, node.Body, node.AsyncKeyword.CSharpKind() != SyntaxKind.None, targetType, sem);
			}
		}

		public override JsExpression VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) {
			var targetType = (INamedTypeSymbol)_semanticModel.GetTypeInfo(node).ConvertedType;
			if (targetType.ContainingNamespace.Name == typeof(System.Linq.Expressions.Expression).Namespace && targetType.ContainingNamespace.Name == typeof(System.Linq.Expressions.Expression).Name) {
				return PerformExpressionTreeLambdaConversion((ExpressionSyntax)node.Body);
			}
			else {
				var sem = _metadataImporter.GetDelegateSemantics(targetType);
				var lambdaSymbol = (IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol;
				return CompileLambda(node, lambdaSymbol.Parameters, node.Body, node.AsyncKeyword.CSharpKind() != SyntaxKind.None, targetType, sem);
			}
		}

		public override JsExpression VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) {
			var targetType = (INamedTypeSymbol)_semanticModel.GetTypeInfo(node).ConvertedType;
			var sem = _metadataImporter.GetDelegateSemantics(targetType);
			var parameters = node.ParameterList != null ? ((IMethodSymbol)_semanticModel.GetSymbolInfo(node).Symbol).Parameters : ImmutableArray<IParameterSymbol>.Empty;
			return CompileLambda(node, parameters, node.Block, node.AsyncKeyword.CSharpKind() != SyntaxKind.None, targetType, sem);
		}

#if false
		public override JsExpression VisitDynamicMemberResolveResult(DynamicMemberResolveResult rr, bool data) {
			return JsExpression.Member(VisitResolveResult(rr.Target, true), rr.Member);
		}

		public override JsExpression VisitDynamicInvocationResolveResult(DynamicInvocationResolveResult rr, bool data) {
			if (rr.InvocationType == DynamicInvocationType.ObjectCreation) {
				if (rr.Arguments.Any(arg => arg is NamedArgumentResolveResult)) {
					_errorReporter.Message(Messages._7526);
					return JsExpression.Null;
				}
				var methods = ((MethodGroupResolveResult)rr.Target).Methods.ToList();
				var semantics = methods.Select(_metadataImporter.GetConstructorSemantics).ToList();

				if (semantics.Select(s => s.Type).Distinct().Count() > 1) {
					_errorReporter.Message(Messages._7531);
					return JsExpression.Null;
				}
				switch (semantics[0].Type) {
					case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
						break;

					case ConstructorScriptSemantics.ImplType.NamedConstructor:
					case ConstructorScriptSemantics.ImplType.StaticMethod:
						if (semantics.Select(s => s.Name).Distinct().Count() > 1) {
							_errorReporter.Message(Messages._7531);
							return JsExpression.Null;
						}
						break;

					default:
						_errorReporter.Message(Messages._7531);
						return JsExpression.Null;
				}

				return CompileConstructorInvocation(semantics[0], methods[0], rr.Arguments, null, rr.InitializerStatements);
			}
			else {
				if (rr.InvocationType == DynamicInvocationType.Indexing && rr.Arguments.Count != 1) {
					_errorReporter.Message(Messages._7528);
					return JsExpression.Null;
				}

				var expressions = new List<JsExpression>();
				if (rr.Target is MethodGroupResolveResult) {
					var mgrr = (MethodGroupResolveResult)rr.Target;
					var impl = mgrr.Methods.Select(_metadataImporter.GetMethodSemantics).ToList();
					if (impl.Any(x => x.Type != MethodScriptSemantics.ImplType.NormalMethod)) {
						_errorReporter.Message(Messages._7530);
						return JsExpression.Null;
					}
					if (impl.Any(x => x.Name != impl[0].Name)) {
						_errorReporter.Message(Messages._7529);
						return JsExpression.Null;
					}
					var target = mgrr.TargetResult is TypeResolveResult ? _runtimeLibrary.InstantiateType(mgrr.TargetResult.Type, this) : InnerCompile(mgrr.TargetResult, false);
					expressions.Add(JsExpression.Member(target, impl[0].Name));
				}
				else {
					expressions.Add(InnerCompile(rr.Target, false));
				}

				foreach (var arg in rr.Arguments) {
					if (arg is NamedArgumentResolveResult) {
						_errorReporter.Message(Messages._7526);
						return JsExpression.Null;
					}
					expressions.Add(InnerCompile(arg, false, expressions));
				}

				switch (rr.InvocationType) {
					case DynamicInvocationType.Indexing:
						return JsExpression.Index(expressions[0], expressions[1]);

					case DynamicInvocationType.Invocation:
						return JsExpression.Invocation(expressions[0], expressions.Skip(1));

					default:
						_errorReporter.InternalError("Unsupported dynamic invocation type " + rr.InvocationType);
						return JsExpression.Null;
				}
			}
		}

		public override JsExpression VisitAwaitResolveResult(AwaitResolveResult rr, bool returnValueIsImportant) {
			JsExpression operand;
			if (rr.GetAwaiterInvocation is DynamicInvocationResolveResult && ((DynamicInvocationResolveResult)rr.GetAwaiterInvocation).Target is DynamicMemberResolveResult) {
				// If the GetAwaiter call is dynamic, we need to camel-case it.
				operand = InnerCompile(((DynamicMemberResolveResult)((DynamicInvocationResolveResult)rr.GetAwaiterInvocation).Target).Target, false);
				operand = JsExpression.Invocation(JsExpression.Member(operand, "getAwaiter"));
				var temp = _createTemporaryVariable(SpecialType.Dynamic);
				_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, operand));
				operand = JsExpression.Identifier(_variables[temp].Name);
			}
			else {
				operand = InnerCompile(rr.GetAwaiterInvocation, true);
			}

			if (rr.GetAwaiterInvocation.Type.Kind == TypeKind.Dynamic) {
				_additionalStatements.Add(JsStatement.Await(operand, "onCompleted"));
				return JsExpression.Invocation(JsExpression.Member(operand, "getResult"));
			}
			else {
				var getResultMethodImpl   = _metadataImporter.GetMethodSemantics(rr.GetResultMethod);
				var onCompletedMethodImpl = _metadataImporter.GetMethodSemantics(rr.OnCompletedMethod);
	
				if (onCompletedMethodImpl.Type != MethodScriptSemantics.ImplType.NormalMethod) {
					_errorReporter.Message(Messages._7535);
					return JsExpression.Null;
				}
	
				_additionalStatements.Add(JsStatement.Await(operand, onCompletedMethodImpl.Name));
				return CompileMethodInvocation(getResultMethodImpl, rr.GetResultMethod, new[] { operand }, false);
			}
		}

		public override JsExpression VisitNamedArgumentResolveResult(NamedArgumentResolveResult rr, bool data) {
			return VisitResolveResult(rr.Argument, data);	// Argument names are ignored.
		}

		public override JsExpression VisitSizeOfResolveResult(SizeOfResolveResult rr, bool data) {
			if (rr.ConstantValue == null) {
				// This is an internal error because AFAIK, using sizeof() with anything that doesn't return a compile-time constant (with our enum extensions) can only be done in an unsafe context.
				_errorReporter.InternalError("Cannot take the size of type " + rr.ReferencedType.FullName);
				return JsExpression.Null;
			}
			return JSModel.Utils.MakeConstantExpression(rr.ConstantValue);
		}
#endif
		JsExpression IRuntimeContext.ResolveTypeParameter(ITypeParameterSymbol tp) {
			return ResolveTypeParameter(tp);
		}

		JsExpression IRuntimeContext.EnsureCanBeEvaluatedMultipleTimes(JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore) {
			return Utils.EnsureCanBeEvaluatedMultipleTimes(_additionalStatements, expression, expressionsThatMustBeEvaluatedBefore, () => { var temp = _createTemporaryVariable(); return _variables[temp].Name; });
		}

		private JsExpression ResolveTypeParameter(ITypeParameterSymbol tp) {
			return Utils.ResolveTypeParameter(tp, _typeBeingCompiled, _methodBeingCompiled, _metadataImporter, _errorReporter, _namer);
		}
	}
}
