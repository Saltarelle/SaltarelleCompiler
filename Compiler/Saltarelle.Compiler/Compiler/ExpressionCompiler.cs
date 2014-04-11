using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.StateMachineRewrite;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler {
	public class NestedFunctionContext {
		public ReadOnlySet<IVariable> CapturedByRefVariables { get; private set; }

		public NestedFunctionContext(IEnumerable<IVariable> capturedByRefVariables) {
			var crv = new HashSet<IVariable>();
			foreach (var v in capturedByRefVariables)
				crv.Add(v);

			CapturedByRefVariables = new ReadOnlySet<IVariable>(crv);
		}
	}

	public class ExpressionCompiler : ResolveResultVisitor<JsExpression, bool>, IRuntimeContext {
		private readonly ICompilation _compilation;
		private readonly IMetadataImporter _metadataImporter;
		private readonly INamer _namer;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly IErrorReporter _errorReporter;
		private readonly IDictionary<IVariable, VariableData> _variables;
		private readonly IDictionary<LambdaResolveResult, NestedFunctionData> _nestedFunctions;
		private readonly Func<IType, IVariable> _createTemporaryVariable;
		private readonly Func<NestedFunctionContext, StatementCompiler> _createInnerCompiler;
		private readonly string _thisAlias;
		private readonly NestedFunctionContext _nestedFunctionContext;
		private readonly IMethod _methodBeingCompiled;
		private readonly ITypeDefinition _typeBeingCompiled;
		private readonly bool _returnMultidimArrayValueByReference;
		private IVariable _objectBeingInitialized;

		public ExpressionCompiler(ICompilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, IDictionary<IVariable, VariableData> variables, IDictionary<LambdaResolveResult, NestedFunctionData> nestedFunctions, Func<IType, IVariable> createTemporaryVariable, Func<NestedFunctionContext, StatementCompiler> createInnerCompiler, string thisAlias, NestedFunctionContext nestedFunctionContext, IVariable objectBeingInitialized, IMethod methodBeingCompiled, ITypeDefinition typeBeingCompiled, bool returnMultidimArrayValueByReference = false) {
			Require.ValidJavaScriptIdentifier(thisAlias, "thisAlias", allowNull: true);

			_compilation = compilation;
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
			_objectBeingInitialized = objectBeingInitialized;
			_methodBeingCompiled = methodBeingCompiled;
			_typeBeingCompiled = typeBeingCompiled;
			_returnMultidimArrayValueByReference = returnMultidimArrayValueByReference;
		}

		private List<JsStatement> _additionalStatements;

		public ExpressionCompileResult Compile(ResolveResult expression, bool returnValueIsImportant) {
			_additionalStatements = new List<JsStatement>();
			var expr = VisitResolveResult(expression, returnValueIsImportant);
			return new ExpressionCompileResult(expr, _additionalStatements);
		}

		public IList<JsStatement> CompileConstructorInitializer(IMethod method, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, IList<ResolveResult> initializerStatements, bool currentIsStaticMethod) {
			var impl = _metadataImporter.GetConstructorSemantics(method);
			if (impl.SkipInInitializer) {
				if (currentIsStaticMethod)
					return new[] { JsStatement.Var(_thisAlias, JsExpression.ObjectLiteral()) };
				else
					return EmptyList<JsStatement>.Instance;
			}

			_additionalStatements = new List<JsStatement>();

			if (currentIsStaticMethod) {
				_additionalStatements.Add(JsStatement.Var(_thisAlias, CompileConstructorInvocation(impl, method, argumentsForCall, argumentToParameterMap, initializerStatements)));
			}
			else if (impl.Type == ConstructorScriptSemantics.ImplType.Json) {
				_additionalStatements.Add(_runtimeLibrary.ShallowCopy(CompileJsonConstructorCall(method, impl, argumentsForCall, argumentToParameterMap, initializerStatements), CompileThis(), this));
			}
			else {
				string literalCode   = GetActualInlineCode(impl, argumentsForCall.Count > 0 && argumentsForCall[argumentsForCall.Count - 1] is ArrayCreateResolveResult);
				var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, literalCode, _runtimeLibrary.InstantiateType(method.DeclaringType, this), false, argumentsForCall, argumentToParameterMap);
				var jsType           = thisAndArguments[0];
				thisAndArguments[0]  = CompileThis();	// Swap out the TypeResolveResult that we get as default.

				switch (impl.Type) {
					case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
						_additionalStatements.Add(CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, jsType, impl.ExpandParams, true));
						break;

					case ConstructorScriptSemantics.ImplType.NamedConstructor:
						_additionalStatements.Add(CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, JsExpression.Member(jsType, impl.Name), impl.ExpandParams, true));
						break;

					case ConstructorScriptSemantics.ImplType.StaticMethod:
						_additionalStatements.Add(_runtimeLibrary.ShallowCopy(CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, JsExpression.Member(jsType, impl.Name), impl.ExpandParams, false), thisAndArguments[0], this));
						break;

					case ConstructorScriptSemantics.ImplType.InlineCode:
						_additionalStatements.Add(_runtimeLibrary.ShallowCopy(CompileInlineCodeMethodInvocation(method, literalCode, null, thisAndArguments.Skip(1).ToList()), thisAndArguments[0], this));
						break;

					default:
						_errorReporter.Message(Messages._7505);
						break;
				}
			}

			var result = _additionalStatements;
			_additionalStatements = null;	// Just so noone else messes with it by accident (shouldn't happen).
			return result;
		}

		private ExpressionCompiler Clone(NestedFunctionContext nestedFunctionContext = null, bool returnMultidimArrayValueByReference = false) {
			return new ExpressionCompiler(_compilation, _metadataImporter, _namer, _runtimeLibrary, _errorReporter, _variables, _nestedFunctions, _createTemporaryVariable, _createInnerCompiler, _thisAlias, nestedFunctionContext ?? _nestedFunctionContext, _objectBeingInitialized, _methodBeingCompiled, _typeBeingCompiled, returnMultidimArrayValueByReference);
		}

		private ExpressionCompileResult CloneAndCompile(ResolveResult expression, bool returnValueIsImportant, NestedFunctionContext nestedFunctionContext = null, bool returnMultidimArrayValueByReference = false) {
			return Clone(nestedFunctionContext, returnMultidimArrayValueByReference).Compile(expression, returnValueIsImportant);
		}

		private void CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(IList<JsExpression> expressions, ExpressionCompileResult newExpressions) {
			Utils.CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(_additionalStatements, expressions, newExpressions, () => { var temp = _createTemporaryVariable(SpecialType.UnknownType); return _variables[temp].Name; });
		}

		private void CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(IList<JsExpression> expressions, JsExpression newExpression) {
			CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(expressions, new ExpressionCompileResult(newExpression, new JsStatement[0]));
		}

		JsExpression IRuntimeContext.ResolveTypeParameter(ITypeParameter tp) {
			return ResolveTypeParameter(tp);
		}

		JsExpression IRuntimeContext.EnsureCanBeEvaluatedMultipleTimes(JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore) {
			return Utils.EnsureCanBeEvaluatedMultipleTimes(_additionalStatements, expression, expressionsThatMustBeEvaluatedBefore, () => { var temp = _createTemporaryVariable(SpecialType.UnknownType); return _variables[temp].Name; });
		}

		private JsExpression ResolveTypeParameter(ITypeParameter tp) {
			return Utils.ResolveTypeParameter(tp, _typeBeingCompiled, _methodBeingCompiled, _metadataImporter, _errorReporter, _namer);
		}

		private JsExpression InnerCompile(ResolveResult rr, bool usedMultipleTimes, IList<JsExpression> expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression, bool returnMultidimArrayValueByReference = false) {
			var result = CloneAndCompile(rr, returnValueIsImportant: true, returnMultidimArrayValueByReference: returnMultidimArrayValueByReference);

			bool needsTemporary = usedMultipleTimes && IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(result.Expression);
			if (result.AdditionalStatements.Count > 0 || needsTemporary) {
				CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression, result);
			}

			_additionalStatements.AddRange(result.AdditionalStatements);

			if (needsTemporary) {
				var temp = _createTemporaryVariable(rr.Type);
				_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, result.Expression));
				return JsExpression.Identifier(_variables[temp].Name);
			}
			else {
				return result.Expression;
			}
		}

		private JsExpression InnerCompile(ResolveResult rr, bool usedMultipleTimes, ref JsExpression expressionThatHasToBeEvaluatedInOrderBeforeThisExpression, bool returnMultidimArrayValueByReference = false) {
			var l = new List<JsExpression>();
			if (expressionThatHasToBeEvaluatedInOrderBeforeThisExpression != null)
				l.Add(expressionThatHasToBeEvaluatedInOrderBeforeThisExpression);
			var r = InnerCompile(rr, usedMultipleTimes, l, returnMultidimArrayValueByReference);
			if (l.Count > 0)
				expressionThatHasToBeEvaluatedInOrderBeforeThisExpression = l[0];
			return r;
		}

		private JsExpression InnerCompile(ResolveResult rr, bool usedMultipleTimes, bool returnMultidimArrayValueByReference = false) {
			JsExpression _ = null;
			return InnerCompile(rr, usedMultipleTimes, ref _, returnMultidimArrayValueByReference);
		}

		private bool IsIntegerType(IType type) {
			type = UnpackNullable(type);

			return type.Equals(_compilation.FindType(KnownTypeCode.Byte))
			    || type.Equals(_compilation.FindType(KnownTypeCode.SByte))
			    || type.Equals(_compilation.FindType(KnownTypeCode.Char))
			    || type.Equals(_compilation.FindType(KnownTypeCode.Int16))
			    || type.Equals(_compilation.FindType(KnownTypeCode.UInt16))
			    || type.Equals(_compilation.FindType(KnownTypeCode.Int32))
			    || type.Equals(_compilation.FindType(KnownTypeCode.UInt32))
			    || type.Equals(_compilation.FindType(KnownTypeCode.Int64))
			    || type.Equals(_compilation.FindType(KnownTypeCode.UInt64));
		}

		private bool IsUnsignedType(IType type) {
			type = UnpackNullable(type);

			return type.Equals(_compilation.FindType(KnownTypeCode.Byte))
			    || type.Equals(_compilation.FindType(KnownTypeCode.UInt16))
				|| type.Equals(_compilation.FindType(KnownTypeCode.UInt32))
				|| type.Equals(_compilation.FindType(KnownTypeCode.UInt64));
		}

		private IType UnpackNullable(IType type) {
			return type.IsKnownType(KnownTypeCode.NullableOfT) ? ((ParameterizedType)type).TypeArguments[0] : type;
		}

		private bool IsNullableBooleanType(IType type) {
			return Equals(type.GetDefinition(), _compilation.FindType(KnownTypeCode.NullableOfT))
			    && Equals(UnpackNullable(type), _compilation.FindType(KnownTypeCode.Boolean));
		}

		private bool IsAssignmentOperator(ExpressionType operatorType) {
			return operatorType == ExpressionType.AddAssign
			    || operatorType == ExpressionType.AndAssign
			    || operatorType == ExpressionType.DivideAssign
			    || operatorType == ExpressionType.ExclusiveOrAssign
			    || operatorType == ExpressionType.LeftShiftAssign
			    || operatorType == ExpressionType.ModuloAssign
			    || operatorType == ExpressionType.MultiplyAssign
			    || operatorType == ExpressionType.OrAssign
			    || operatorType == ExpressionType.PowerAssign
			    || operatorType == ExpressionType.RightShiftAssign
			    || operatorType == ExpressionType.SubtractAssign
			    || operatorType == ExpressionType.AddAssignChecked
			    || operatorType == ExpressionType.MultiplyAssignChecked
			    || operatorType == ExpressionType.SubtractAssignChecked;
		}

		private JsExpression CompileCompoundFieldAssignment(MemberResolveResult target, ResolveResult otherOperand, string fieldName, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool returnValueBeforeChange) {
			var jsTarget = target.Member.IsStatic ? _runtimeLibrary.InstantiateType(target.Member.DeclaringType, this) : InnerCompile(target.TargetResult, compoundFactory == null, returnMultidimArrayValueByReference: true);
			var jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, ref jsTarget) : null);
			var access = JsExpression.Member(jsTarget, fieldName);
			if (compoundFactory != null) {
				if (returnValueIsImportant && IsMutableValueType(target.Type)) {
					_additionalStatements.Add(JsExpression.Assign(access, MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, target.Type)));
					return access;
				}
				else {
					return compoundFactory(access, MaybeCloneValueType(jsOtherOperand, otherOperand, target.Type));
				}
			}
			else {
				if (returnValueIsImportant && returnValueBeforeChange) {
					var temp = _createTemporaryVariable(target.Type);
					_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, access));
					_additionalStatements.Add(JsExpression.Assign(access, MaybeCloneValueType(valueFactory(JsExpression.Identifier(_variables[temp].Name), jsOtherOperand), otherOperand, target.Type)));
					return JsExpression.Identifier(_variables[temp].Name);
				}
				else {
					if (returnValueIsImportant && IsMutableValueType(target.Type)) {
						_additionalStatements.Add(JsExpression.Assign(access, MaybeCloneValueType(valueFactory(access, jsOtherOperand), otherOperand, target.Type)));
						return access;
					}
					else {
						return JsExpression.Assign(access, MaybeCloneValueType(valueFactory(access, jsOtherOperand), otherOperand, target.Type));
					}
				}
			}
		}

		private JsExpression CompileArrayAccessCompoundAssignment(ResolveResult array, ResolveResult index, ResolveResult otherOperand, IType elementType, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool returnValueBeforeChange) {
			var expressions = new List<JsExpression>();
			expressions.Add(InnerCompile(array, compoundFactory == null, returnMultidimArrayValueByReference: true));
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
					var temp = _createTemporaryVariable(_compilation.FindType(KnownTypeCode.Object));
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

		private bool IsMutableValueType(IType type) {
			return Utils.IsMutableValueType(type, _metadataImporter);
		}

		private JsExpression MaybeCloneValueType(JsExpression input, ResolveResult csharpInput, IType type, bool forceClone = false) {
			return Utils.MaybeCloneValueType(input, csharpInput, type, _metadataImporter, _runtimeLibrary, this, forceClone);
		}

		private JsExpression CompileCompoundAssignment(ResolveResult target, ResolveResult otherOperand, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool isLifted, bool returnValueBeforeChange = false, bool oldValueIsImportant = true) {
			if (isLifted) {
				compoundFactory = null;
				var oldVF       = valueFactory;
				valueFactory    = (a, b) => _runtimeLibrary.Lift(oldVF(a, b), this);
			}

			if (target is LocalResolveResult || target is DynamicMemberResolveResult || target is DynamicInvocationResolveResult /* Dynamic indexing is an invocation */) {
				JsExpression jsTarget, jsOtherOperand;
				jsTarget = InnerCompile(target, compoundFactory == null, returnMultidimArrayValueByReference: true);
				if (target is LocalResolveResult || target is DynamicMemberResolveResult) {
					jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false) : null);	// If the variable is a by-ref variable we will get invalid reordering if we force the target to be evaluated before the other operand.
				}
				else {
					jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, ref jsTarget) : null);
				}

				if (compoundFactory != null) {
					if (returnValueIsImportant && IsMutableValueType(target.Type)) {
						_additionalStatements.Add(JsExpression.Assign(jsTarget, MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, target.Type)));
						return jsTarget;
					}
					else {
						return compoundFactory(jsTarget, MaybeCloneValueType(jsOtherOperand, otherOperand, target.Type));
					}
				}
				else {
					if (returnValueIsImportant && returnValueBeforeChange) {
						var temp = _createTemporaryVariable(target.Type);
						_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, jsTarget));
						_additionalStatements.Add(JsExpression.Assign(jsTarget, valueFactory(JsExpression.Identifier(_variables[temp].Name), jsOtherOperand)));
						return JsExpression.Identifier(_variables[temp].Name);
					}
					else {
						if (returnValueIsImportant && IsMutableValueType(target.Type)) {
							_additionalStatements.Add(JsExpression.Assign(jsTarget, MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, target.Type)));
							return jsTarget;
						}
						else {
							return JsExpression.Assign(jsTarget, MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, target.Type));
						}
					}
				}
			}
			else if (target is MemberResolveResult) {
				var mrr = (MemberResolveResult)target;

				if (mrr.Member is IProperty) {
					var property = ((MemberResolveResult)target).Member as IProperty;
					var impl = _metadataImporter.GetPropertySemantics(property);

					switch (impl.Type) {
						case PropertyScriptSemantics.ImplType.GetAndSetMethods: {
							if (impl.SetMethod.Type == MethodScriptSemantics.ImplType.NativeIndexer) {
								if (!property.IsIndexer || property.Getter.Parameters.Count != 1) {
									_errorReporter.Message(Messages._7506);
									return JsExpression.Null;
								}
								return CompileArrayAccessCompoundAssignment(mrr.TargetResult, ((CSharpInvocationResolveResult)mrr).Arguments[0], otherOperand, property.ReturnType, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
							}
							else {
								List<JsExpression> thisAndArguments;
								if (property.Parameters.Count > 0) {
									var invocation = (CSharpInvocationResolveResult)target;
									thisAndArguments = CompileThisAndArgumentListForMethodCall(property.Setter, null, InnerCompile(invocation.TargetResult, oldValueIsImportant, returnMultidimArrayValueByReference: true), oldValueIsImportant, invocation.GetArgumentsForCall(), invocation.GetArgumentToParameterMap());
								}
								else {
									thisAndArguments = new List<JsExpression> { mrr.Member.IsStatic ? _runtimeLibrary.InstantiateType(mrr.Member.DeclaringType, this) : InnerCompile(mrr.TargetResult, oldValueIsImportant, returnMultidimArrayValueByReference: true) };
								}

								JsExpression oldValue, jsOtherOperand;
								if (oldValueIsImportant) {
									thisAndArguments.Add(MaybeCloneValueType(CompileMethodInvocation(impl.GetMethod, property.Getter, thisAndArguments, mrr.Member.IsOverridable && !mrr.IsVirtualCall), otherOperand, target.Type));
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
										var temp = _createTemporaryVariable(target.Type);
										_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, valueToReturn));
										valueToReturn = JsExpression.Identifier(_variables[temp].Name);
									}

									var newValue = (returnValueBeforeChange ? valueFactory(valueToReturn, jsOtherOperand) : valueToReturn);

									thisAndArguments.Add(MaybeCloneValueType(newValue, otherOperand, target.Type, forceClone: true));
									_additionalStatements.Add(CompileMethodInvocation(impl.SetMethod, property.Setter, thisAndArguments, mrr.Member.IsOverridable && !mrr.IsVirtualCall));
									return valueToReturn;
								}
								else {
									thisAndArguments.Add(MaybeCloneValueType(valueFactory(oldValue, jsOtherOperand), otherOperand, target.Type));
									return CompileMethodInvocation(impl.SetMethod, property.Setter, thisAndArguments, mrr.Member.IsOverridable && !mrr.IsVirtualCall);
								}
							}
						}

						case PropertyScriptSemantics.ImplType.Field: {
							return CompileCompoundFieldAssignment(mrr, otherOperand, impl.FieldName, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
						}

						default: {
							_errorReporter.Message(Messages._7507, property.DeclaringType.FullName + "." + property.Name);
							return JsExpression.Null;
						}
					}
				}
				else if (mrr.Member is IField) {
					var field = (IField)mrr.Member;
					var impl = _metadataImporter.GetFieldSemantics(field);
					switch (impl.Type) {
						case FieldScriptSemantics.ImplType.Field:
							return CompileCompoundFieldAssignment(mrr, otherOperand, impl.Name, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
						case FieldScriptSemantics.ImplType.Constant:
							_errorReporter.Message(Messages._7508, field.DeclaringType.FullName + "." + field.Name);
							return JsExpression.Null;
						default:
							_errorReporter.Message(Messages._7509, field.DeclaringType.FullName + "." + field.Name);
							return JsExpression.Null;
					}
				}
				else if (mrr.Member is IEvent) {
					var evt = (IEvent)mrr.Member;
					var evtField = _metadataImporter.GetAutoEventBackingFieldName(evt);
					return CompileCompoundFieldAssignment(mrr, otherOperand, evtField, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
				}
				else {
					_errorReporter.InternalError("Target " + mrr.Member.DeclaringType.FullName + "." + mrr.Member.Name + " of compound assignment is neither a property nor a field nor an event.");
					return JsExpression.Null;
				}
			}
			else if (target is ArrayAccessResolveResult) {
				var arr = (ArrayAccessResolveResult)target;
				if (arr.Indexes.Count == 1) {
					return CompileArrayAccessCompoundAssignment(arr.Array, arr.Indexes[0], otherOperand, target.Type, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
				}
				else {
					var expressions = new List<JsExpression>();
					expressions.Add(InnerCompile(arr.Array, oldValueIsImportant, returnMultidimArrayValueByReference: true));
					foreach (var i in arr.Indexes)
						expressions.Add(InnerCompile(i, oldValueIsImportant, expressions));

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
							var temp = _createTemporaryVariable(target.Type);
							_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, valueToReturn));
							valueToReturn = JsExpression.Identifier(_variables[temp].Name);
						}

						var newValue = MaybeCloneValueType(returnValueBeforeChange ? valueFactory(valueToReturn, jsOtherOperand) : valueToReturn, otherOperand, target.Type);

						_additionalStatements.Add(_runtimeLibrary.SetMultiDimensionalArrayValue(expressions[0], expressions.Skip(1), newValue, this));
						return valueToReturn;
					}
					else {
						return _runtimeLibrary.SetMultiDimensionalArrayValue(expressions[0], expressions.Skip(1), MaybeCloneValueType(valueFactory(oldValue, jsOtherOperand), otherOperand, target.Type), this);
					}
				}
			}
			else if (target is ThisResolveResult) {
				var jsTarget = CompileThis();
				var jsOtherOperand = otherOperand != null ? InnerCompile(otherOperand, false) : null;

				if (_methodBeingCompiled == null || !_methodBeingCompiled.IsConstructor) {
					var typesem = _metadataImporter.GetTypeSemantics(target.Type.GetDefinition());
					if (typesem.Type != TypeScriptSemantics.ImplType.MutableValueType) {
						_errorReporter.Message(Messages._7538);
						return JsExpression.Null;
					}
				}

				if (compoundFactory != null) {
					if (returnValueIsImportant) {
						_additionalStatements.Add(_runtimeLibrary.ShallowCopy(MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, target.Type), jsTarget, this));
						return jsTarget;
					}
					else {
						return _runtimeLibrary.ShallowCopy(MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, target.Type), jsTarget, this);
					}
				}
				else {
					if (returnValueIsImportant && returnValueBeforeChange) {
						var temp = _createTemporaryVariable(target.Type);
						_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, MaybeCloneValueType(jsTarget, null, target.Type, forceClone: true)));
						_additionalStatements.Add(_runtimeLibrary.ShallowCopy(valueFactory(JsExpression.Identifier(_variables[temp].Name), jsOtherOperand), jsTarget, this));
						return JsExpression.Identifier(_variables[temp].Name);
					}
					else {
						if (returnValueIsImportant) {
							_additionalStatements.Add(_runtimeLibrary.ShallowCopy(MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, target.Type), jsTarget, this));
							return jsTarget;
						}
						else {
							return _runtimeLibrary.ShallowCopy(MaybeCloneValueType(valueFactory(jsTarget, jsOtherOperand), otherOperand, target.Type), jsTarget, this);
						}
					}
				}
			}
			else {
				_errorReporter.InternalError("Unsupported target of assignment: " + target);
				return JsExpression.Null;
			}
		}

		private JsExpression CompileBinaryNonAssigningOperator(ResolveResult left, ResolveResult right, Func<JsExpression, JsExpression, JsExpression> resultFactory, bool isLifted) {
			var jsLeft  = InnerCompile(left, false);
			var jsRight = InnerCompile(right, false, ref jsLeft);
			var result = resultFactory(jsLeft, jsRight);
			return isLifted ? _runtimeLibrary.Lift(result, this) : result;
		}

		private JsExpression CompileUnaryOperator(ResolveResult operand, Func<JsExpression, JsExpression> resultFactory, bool isLifted) {
			var jsOperand = InnerCompile(operand, false);
			var result = resultFactory(jsOperand);
			return isLifted ? _runtimeLibrary.Lift(result, this) : result;
		}

		private JsExpression CompileConditionalOperator(ResolveResult test, ResolveResult truePath, ResolveResult falsePath) {
			var jsTest      = VisitResolveResult(test, true);
			var trueResult  = CloneAndCompile(truePath, true);
			var falseResult = CloneAndCompile(falsePath, true);

			if (trueResult.AdditionalStatements.Count > 0 || falseResult.AdditionalStatements.Count > 0) {
				var temp = _createTemporaryVariable(truePath.Type);
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

		private JsExpression CompileCoalesce(IType resultType, ResolveResult left, ResolveResult right) {
			var jsLeft  = InnerCompile(left, false);
			var jsRight = CloneAndCompile(right, true);

			if (jsRight.AdditionalStatements.Count == 0 && !CanTypeBeFalsy(left.Type)) {
				return JsExpression.LogicalOr(jsLeft, jsRight.Expression);
			}
			else if (jsRight.AdditionalStatements.Count == 0 && (jsRight.Expression.NodeType == ExpressionNodeType.Identifier || (jsRight.Expression.NodeType >= ExpressionNodeType.ConstantFirst && jsRight.Expression.NodeType <= ExpressionNodeType.ConstantLast))) {
				return _runtimeLibrary.Coalesce(jsLeft, jsRight.Expression, this);
			}
			else {
				var temp = _createTemporaryVariable(resultType);
				var nullBlock  = JsStatement.Block(jsRight.AdditionalStatements.Concat(new JsStatement[] { JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), jsRight.Expression) }));
				_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, jsLeft));
				_additionalStatements.Add(JsStatement.If(_runtimeLibrary.ReferenceEquals(JsExpression.Identifier(_variables[temp].Name), JsExpression.Null, this), nullBlock, null));
				return JsExpression.Identifier(_variables[temp].Name);
			}
		}

		private JsExpression CompileEventAddOrRemove(MemberResolveResult target, ResolveResult value, bool isAdd) {
			var evt = (IEvent)target.Member;
			var impl = _metadataImporter.GetEventSemantics(evt);
			switch (impl.Type) {
				case EventScriptSemantics.ImplType.AddAndRemoveMethods: {
					var accessor = isAdd ? evt.AddAccessor : evt.RemoveAccessor;
					return CompileMethodInvocation(isAdd ? impl.AddMethod : impl.RemoveMethod, accessor, target.TargetResult, new[] { value }, new[] { 0 }, target.IsVirtualCall);
				}
				default:
					_errorReporter.Message(Messages._7511, evt.DeclaringType.FullName + "." + evt.Name);
					return JsExpression.Null;
			}
		}

		public override JsExpression VisitResolveResult(ResolveResult rr, bool data) {
			if (rr.IsError) {
				_errorReporter.InternalError("ResolveResult " + rr.ToString() + " is an error.");
				return JsExpression.Null;
			}
			else
				return base.VisitResolveResult(rr, data);
		}

		private bool CanTypeBeFalsy(IType type) {
			type = UnpackNullable(type);
			return IsIntegerType(type) || type.IsKnownType(KnownTypeCode.Single) || type.IsKnownType(KnownTypeCode.Double) || type.IsKnownType(KnownTypeCode.Decimal) || type.IsKnownType(KnownTypeCode.Boolean) || type.IsKnownType(KnownTypeCode.String) // Numbers, boolean and string have falsy values that are not null...
			    || type.Kind == TypeKind.Enum || type.Kind == TypeKind.Dynamic // ... so do enum types...
			    || type.IsKnownType(KnownTypeCode.Object) || type.IsKnownType(KnownTypeCode.ValueType) || type.IsKnownType(KnownTypeCode.Enum); // These reference types might contain types that have falsy values, so we need to be safe.
		}

		private JsExpression CompileAndAlsoOrOrElse(ResolveResult left, ResolveResult right, bool isAndAlso) {
			var jsLeft  = InnerCompile(left, false);
			var jsRight = CloneAndCompile(right, true);
			if (jsRight.AdditionalStatements.Count > 0) {
				var temp = _createTemporaryVariable(_compilation.FindType(KnownTypeCode.Boolean));
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

		private bool CanDoSimpleComparisonForEquals(ResolveResult a, ResolveResult b) {
			if (a.Type.IsKnownType(KnownTypeCode.NullableOfT) || b.Type.IsKnownType(KnownTypeCode.NullableOfT)) {
				// in an expression such as myNullableInt == 3, an implicit nullable conversion is performed on the non-nullable value, but we can know for sure that it will never be null.
				var ca = a as ConversionResolveResult;
				if (ca != null) {
					if (ca.Conversion.IsNullableConversion && ca.Conversion.IsImplicit)
						a = ca.Input;
				}

				var cb = b as ConversionResolveResult;
				if (cb != null) {
					if (cb.Conversion.IsNullableConversion && cb.Conversion.IsImplicit)
						b = cb.Input;
				}
			}

			bool aCanBeNull = a.Type.IsReferenceType != false || a.Type.IsKnownType(KnownTypeCode.NullableOfT);
			bool bCanBeNull = b.Type.IsReferenceType != false || b.Type.IsKnownType(KnownTypeCode.NullableOfT);
			return !aCanBeNull || !bCanBeNull;
		}

		public override JsExpression VisitOperatorResolveResult(OperatorResolveResult rr, bool returnValueIsImportant) {
			if (rr.UserDefinedOperatorMethod != null) {
				var impl = _metadataImporter.GetMethodSemantics(rr.UserDefinedOperatorMethod);
				if (impl.Type != MethodScriptSemantics.ImplType.NativeOperator) {
					switch (rr.Operands.Count) {
						case 1: {
							bool returnValueBeforeChange = true;
							switch (rr.OperatorType) {
								case ExpressionType.PreIncrementAssign:
								case ExpressionType.PreDecrementAssign:
									returnValueBeforeChange = false;
									goto case ExpressionType.PostIncrementAssign;
								case ExpressionType.PostIncrementAssign:
								case ExpressionType.PostDecrementAssign: {
									Func<JsExpression, JsExpression, JsExpression> invocation = (a, b) => CompileMethodInvocation(impl, rr.UserDefinedOperatorMethod, new[] { _runtimeLibrary.InstantiateType(rr.UserDefinedOperatorMethod.DeclaringType, this), returnValueIsImportant && returnValueBeforeChange ? MaybeCloneValueType(a, null, rr.Type) : a }, false);
									return CompileCompoundAssignment(rr.Operands[0], null, null, invocation, returnValueIsImportant, rr.IsLiftedOperator, returnValueBeforeChange);
								}
								default:
									return CompileUnaryOperator(rr.Operands[0], a => CompileMethodInvocation(impl, rr.UserDefinedOperatorMethod, new[] { _runtimeLibrary.InstantiateType(rr.UserDefinedOperatorMethod.DeclaringType, this), a }, false), rr.IsLiftedOperator);
							}
						}

						case 2: {
							Func<JsExpression, JsExpression, JsExpression> invocation = (a, b) => CompileMethodInvocation(impl, rr.UserDefinedOperatorMethod, new[] { _runtimeLibrary.InstantiateType(rr.UserDefinedOperatorMethod.DeclaringType, this), a, b }, false);
							if (IsAssignmentOperator(rr.OperatorType))
								return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, invocation, returnValueIsImportant, rr.IsLiftedOperator);
							else
								return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], invocation, rr.IsLiftedOperator);
						}
					}
					_errorReporter.InternalError("Could not compile call to user-defined operator " + rr.UserDefinedOperatorMethod.DeclaringType.FullName + "." + rr.UserDefinedOperatorMethod.Name);
					return JsExpression.Null;
				}
			}

			switch (rr.OperatorType) {
				case ExpressionType.Assign:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.Assign, (a, b) => b, returnValueIsImportant, false, oldValueIsImportant: false);

				// Compound assignment operators

				case ExpressionType.AddAssign:
				case ExpressionType.AddAssignChecked:
					if (rr.Operands[0] is MemberResolveResult && ((MemberResolveResult)rr.Operands[0]).Member is IEvent) {
						return CompileEventAddOrRemove((MemberResolveResult)rr.Operands[0], rr.Operands[1], true);
					}
					else if (rr.Operands[0].Type.Kind == TypeKind.Delegate) {
						var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
						var combine = del.GetMethods().Single(m => m.Name == "Combine" && m.Parameters.Count == 2);
						var impl = _metadataImporter.GetMethodSemantics(combine);
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => CompileMethodInvocation(impl, combine, new[] { _runtimeLibrary.InstantiateType(del, this), a, b }, false), returnValueIsImportant, false);
					}
					else {
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.AddAssign, JsExpression.Add, returnValueIsImportant, rr.IsLiftedOperator);
					}

				case ExpressionType.AndAssign:
					if (IsNullableBooleanType(rr.Operands[0].Type))
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => _runtimeLibrary.LiftedBooleanAnd(a, b, this), returnValueIsImportant, false);
					else
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseAndAssign, JsExpression.BitwiseAnd, returnValueIsImportant, rr.IsLiftedOperator);

				case ExpressionType.DivideAssign:
					if (IsIntegerType(rr.Type))
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => _runtimeLibrary.IntegerDivision(a, b, this), returnValueIsImportant, rr.IsLiftedOperator);
					else
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.DivideAssign, JsExpression.Divide, returnValueIsImportant, rr.IsLiftedOperator);

				case ExpressionType.ExclusiveOrAssign:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseXorAssign, JsExpression.BitwiseXor, returnValueIsImportant, rr.IsLiftedOperator);

				case ExpressionType.LeftShiftAssign:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.LeftShiftAssign, JsExpression.LeftShift, returnValueIsImportant, rr.IsLiftedOperator);

				case ExpressionType.ModuloAssign:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.ModuloAssign, JsExpression.Modulo, returnValueIsImportant, rr.IsLiftedOperator);

				case ExpressionType.MultiplyAssign:
				case ExpressionType.MultiplyAssignChecked:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.MultiplyAssign, JsExpression.Multiply, returnValueIsImportant, rr.IsLiftedOperator);

				case ExpressionType.OrAssign:
					if (IsNullableBooleanType(rr.Operands[0].Type))
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => _runtimeLibrary.LiftedBooleanOr(a, b, this), returnValueIsImportant, false);
					else
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseOrAssign, JsExpression.BitwiseOr, returnValueIsImportant, rr.IsLiftedOperator);

				case ExpressionType.RightShiftAssign:
					if (IsUnsignedType(rr.Type))
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.RightShiftUnsignedAssign, JsExpression.RightShiftUnsigned, returnValueIsImportant, rr.IsLiftedOperator);
					else
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.RightShiftSignedAssign, JsExpression.RightShiftSigned, returnValueIsImportant, rr.IsLiftedOperator);

				case ExpressionType.SubtractAssign:
				case ExpressionType.SubtractAssignChecked:
					if (rr.Operands[0] is MemberResolveResult && ((MemberResolveResult)rr.Operands[0]).Member is IEvent) {
						return CompileEventAddOrRemove((MemberResolveResult)rr.Operands[0], rr.Operands[1], false);
					}
					else if (rr.Operands[0].Type.Kind == TypeKind.Delegate) {
						var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
						var remove = del.GetMethods().Single(m => m.Name == "Remove" && m.Parameters.Count == 2);
						var impl = _metadataImporter.GetMethodSemantics(remove);
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => CompileMethodInvocation(impl, remove, new[] { _runtimeLibrary.InstantiateType(del, this), a, b }, false), returnValueIsImportant, false);
					}
					else {
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.SubtractAssign, JsExpression.Subtract, returnValueIsImportant, rr.IsLiftedOperator);
					}

				case ExpressionType.PreIncrementAssign:
					return CompileCompoundAssignment(rr.Operands[0], null, (a, b) => JsExpression.PrefixPlusPlus(a), (a, b) => JsExpression.Add(a, JsExpression.Number(1)), returnValueIsImportant, rr.IsLiftedOperator);

				case ExpressionType.PreDecrementAssign:
					return CompileCompoundAssignment(rr.Operands[0], null, (a, b) => JsExpression.PrefixMinusMinus(a), (a, b) => JsExpression.Subtract(a, JsExpression.Number(1)), returnValueIsImportant, rr.IsLiftedOperator);

				case ExpressionType.PostIncrementAssign:
					return CompileCompoundAssignment(rr.Operands[0], null, (a, b) => JsExpression.PostfixPlusPlus(a), (a, b) => JsExpression.Add(a, JsExpression.Number(1)), returnValueIsImportant, rr.IsLiftedOperator, returnValueBeforeChange: true);

				case ExpressionType.PostDecrementAssign:
					return CompileCompoundAssignment(rr.Operands[0], null, (a, b) => JsExpression.PostfixMinusMinus(a), (a, b) => JsExpression.Subtract(a, JsExpression.Number(1)), returnValueIsImportant, rr.IsLiftedOperator, returnValueBeforeChange: true);

				// Binary non-assigning operators

				case ExpressionType.Add:
				case ExpressionType.AddChecked:
					if (rr.Operands[0].Type.Kind == TypeKind.Delegate) {
						var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
						var combine = del.GetMethods().Single(m => m.Name == "Combine" && m.Parameters.Count == 2);
						var impl = _metadataImporter.GetMethodSemantics(combine);
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => CompileMethodInvocation(impl, combine, new[] { _runtimeLibrary.InstantiateType(del, this), a, b }, false), false);
					}
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Add, rr.IsLiftedOperator);

				case ExpressionType.And:
					if (IsNullableBooleanType(rr.Operands[0].Type))
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => _runtimeLibrary.LiftedBooleanAnd(a, b, this), false);	// We have already lifted it, so it should not be lifted again.
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseAnd, rr.IsLiftedOperator);

				case ExpressionType.AndAlso:
					return CompileAndAlsoOrOrElse(rr.Operands[0], rr.Operands[1], true);

				case ExpressionType.Coalesce:
					return CompileCoalesce(rr.Type, rr.Operands[0], rr.Operands[1]);

				case ExpressionType.Divide:
					if (IsIntegerType(rr.Type))
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => _runtimeLibrary.IntegerDivision(a, b, this), rr.IsLiftedOperator);
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Divide, rr.IsLiftedOperator);

				case ExpressionType.ExclusiveOr:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseXor, rr.IsLiftedOperator);

				case ExpressionType.GreaterThan:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Greater, rr.IsLiftedOperator);

				case ExpressionType.GreaterThanOrEqual:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.GreaterOrEqual, rr.IsLiftedOperator);

				case ExpressionType.Equal:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => CanDoSimpleComparisonForEquals(rr.Operands[0], rr.Operands[1]) ? JsExpression.Same(a, b) : _runtimeLibrary.ReferenceEquals(a, b, this), false);

				case ExpressionType.LeftShift:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.LeftShift, rr.IsLiftedOperator);

				case ExpressionType.LessThan:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Lesser, rr.IsLiftedOperator);

				case ExpressionType.LessThanOrEqual:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.LesserOrEqual, rr.IsLiftedOperator);

				case ExpressionType.Modulo:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Modulo, rr.IsLiftedOperator);

				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Multiply, rr.IsLiftedOperator);

				case ExpressionType.NotEqual:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => CanDoSimpleComparisonForEquals(rr.Operands[0], rr.Operands[1]) ? JsExpression.NotSame(a, b) : _runtimeLibrary.ReferenceNotEquals(a, b, this), false);

				case ExpressionType.Or:
					if (IsNullableBooleanType(rr.Operands[0].Type))
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => _runtimeLibrary.LiftedBooleanOr(a, b, this), false);	// We have already lifted it, so it should not be lifted again.
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseOr, rr.IsLiftedOperator);

				case ExpressionType.OrElse:
					return CompileAndAlsoOrOrElse(rr.Operands[0], rr.Operands[1], false);

				case ExpressionType.RightShift:
					if (IsUnsignedType(rr.Type))
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.RightShiftUnsigned, rr.IsLiftedOperator);
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.RightShiftSigned, rr.IsLiftedOperator);

				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
					if (rr.Operands[0].Type.Kind == TypeKind.Delegate) {
						var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
						var remove = del.GetMethods().Single(m => m.Name == "Remove" && m.Parameters.Count == 2);
						var impl = _metadataImporter.GetMethodSemantics(remove);
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => CompileMethodInvocation(impl, remove, new[] { _runtimeLibrary.InstantiateType(del, this), a, b }, false), false);
					}
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Subtract, rr.IsLiftedOperator);

				// Unary operators

				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
					return CompileUnaryOperator(rr.Operands[0], JsExpression.Negate, rr.IsLiftedOperator);

				case ExpressionType.UnaryPlus:
					return CompileUnaryOperator(rr.Operands[0], JsExpression.Positive, rr.IsLiftedOperator);

				case ExpressionType.Not:
					return CompileUnaryOperator(rr.Operands[0], JsExpression.LogicalNot, rr.IsLiftedOperator);

				case ExpressionType.OnesComplement:
					return CompileUnaryOperator(rr.Operands[0], JsExpression.BitwiseNot, rr.IsLiftedOperator);

				// Conditional operator

				case ExpressionType.Conditional:
					return CompileConditionalOperator(rr.Operands[0], rr.Operands[1], rr.Operands[2]);

				case ExpressionType.Power:
				case ExpressionType.PowerAssign:
				case ExpressionType.Increment:
				case ExpressionType.Decrement:
				default:
					_errorReporter.InternalError("Unsupported operator " + rr.OperatorType);
					return JsExpression.Null;
			}
		}

		public JsExpression CompileDelegateCombineCall(JsExpression a, JsExpression b) {
			var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
			var combine = del.GetMethods().Single(m => m.Name == "Combine" && m.Parameters.Count == 2);
			var impl = _metadataImporter.GetMethodSemantics(combine);
			var thisAndArguments = (combine.IsStatic ? new[] { _runtimeLibrary.InstantiateType(del, this), a, b } : new[] { a, b });
			return CompileMethodInvocation(impl, combine, thisAndArguments, false);
		}

		public JsExpression CompileDelegateRemoveCall(JsExpression a, JsExpression b) {
			var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
			var remove = del.GetMethods().Single(m => m.Name == "Remove" && m.Parameters.Count == 2);
			var impl = _metadataImporter.GetMethodSemantics(remove);
			var thisAndArguments = (remove.IsStatic ? new[] { _runtimeLibrary.InstantiateType(del, this), a, b } : new[] { a, b });
			return CompileMethodInvocation(impl, remove, thisAndArguments, false);
		}

		public override JsExpression VisitMethodGroupResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.MethodGroupResolveResult rr, bool returnValueIsImportant) {
			_errorReporter.InternalError("MethodGroupResolveResult should always be the target of a method group conversion, and is handled there");
			return JsExpression.Null;
		}

		public override JsExpression VisitLambdaResolveResult(LambdaResolveResult rr, bool returnValueIsImportant) {
			_errorReporter.InternalError("LambdaResolveResult should always be the target of an anonymous method conversion, and is handled there");
			return JsExpression.Null;
		}

		public override JsExpression VisitMemberResolveResult(MemberResolveResult rr, bool returnValueIsImportant) {
			if (rr.Member is IProperty) {
				var impl = _metadataImporter.GetPropertySemantics((IProperty)rr.Member);
				switch (impl.Type) {
					case PropertyScriptSemantics.ImplType.GetAndSetMethods: {
						var getter = ((IProperty)rr.Member).Getter;
						return CompileMethodInvocation(impl.GetMethod, getter, rr.TargetResult, new ResolveResult[0], new int[0], rr.IsVirtualCall);	// We know we have no arguments because indexers are treated as invocations.
					}
					case PropertyScriptSemantics.ImplType.Field: {
						return JsExpression.Member(rr.Member.IsStatic ? _runtimeLibrary.InstantiateType(rr.Member.DeclaringType, this) : InnerCompile(rr.TargetResult, false), impl.FieldName);
					}
					default: {
						_errorReporter.Message(Messages._7512, rr.Member.DeclaringType.FullName + "." + rr.Member.Name);
						return JsExpression.Null;
					}
				}
			}
			else if (rr.Member is IField) {
				var impl = _metadataImporter.GetFieldSemantics((IField)rr.Member);
				switch (impl.Type) {
					case FieldScriptSemantics.ImplType.Field:
						return JsExpression.Member(rr.Member.IsStatic ? _runtimeLibrary.InstantiateType(rr.Member.DeclaringType, this) : InnerCompile(rr.TargetResult, false, returnMultidimArrayValueByReference: true), impl.Name);
					case FieldScriptSemantics.ImplType.Constant:
						return JSModel.Utils.MakeConstantExpression(impl.Value);
					default:
						_errorReporter.Message(Messages._7509, rr.Member.DeclaringType.Name + "." + rr.Member.Name);
						return JsExpression.Null;
				}
			}
			else if (rr.Member is IEvent) {
				var eimpl = _metadataImporter.GetEventSemantics((IEvent)rr.Member);
				if (eimpl.Type == EventScriptSemantics.ImplType.NotUsableFromScript) {
					_errorReporter.Message(Messages._7511, rr.Member.DeclaringType.Name + "." + rr.Member.Name);
					return JsExpression.Null;
				}

				var fname = _metadataImporter.GetAutoEventBackingFieldName((IEvent)rr.Member);
				return JsExpression.Member(rr.Member.IsStatic ? _runtimeLibrary.InstantiateType(rr.Member.DeclaringType, this) : InnerCompile(rr.TargetResult, true, returnMultidimArrayValueByReference: true), fname);
			}
			else {
				_errorReporter.InternalError("Invalid member " + rr.Member.ToString());
				return JsExpression.Null;
			}
		}

		private static readonly ConcurrentDictionary<int, IList<int>> argumentToParameterMapCache = new ConcurrentDictionary<int, IList<int>>();
		private IList<int> CreateIdentityArgumentToParameterMap(int argCount) {
			IList<int> result;
			if (argumentToParameterMapCache.TryGetValue(argCount, out result))
				return result;
			result = Enumerable.Range(0, argCount).ToList();
			argumentToParameterMapCache.TryAdd(argCount, result);
			return result;
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
			var temp = _createTemporaryVariable(SpecialType.UnknownType);
			_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, expressions[index]));
			expressions[index] = JsExpression.Identifier(_variables[temp].Name);
		}

		private List<JsExpression> CompileThisAndArgumentListForMethodCall(IParameterizedMember member, string literalCode, JsExpression target, bool argumentsUsedMultipleTimes, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap) {
			IList<InlineCodeToken> tokens = null;
			var expressions = new List<JsExpression>() { target };
			if (literalCode != null) {
				bool hasError = false;
				tokens = InlineCodeMethodCompiler.Tokenize((IMethod)member, literalCode, s => hasError = true);
				if (hasError)
					tokens = null;
			}

			if (tokens != null && target != null && !member.IsStatic && member.SymbolKind != SymbolKind.Constructor) {
				int thisUseCount = tokens.Count(t => t.Type == InlineCodeToken.TokenType.This);
				if (thisUseCount > 1 && IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(target)) {
					// Create a temporary for {this}, if required.
					var temp = _createTemporaryVariable(member.DeclaringType);
					_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, expressions[0]));
					expressions[0] = JsExpression.Identifier(_variables[temp].Name);
				}
				else if (thisUseCount == 0 && DoesJsExpressionHaveSideEffects.Analyze(target)) {
					// Ensure that 'this' is evaluated if required, even if not used by the inline code.
					_additionalStatements.Add(target);
					expressions[0] = JsExpression.Null;
				}
			}

			argumentToParameterMap = argumentToParameterMap ?? CreateIdentityArgumentToParameterMap(argumentsForCall.Count);
			bool hasCreatedParamArray = false;

			// Compile the arguments left to right
			foreach (var i in argumentToParameterMap) {
				if (member.Parameters[i].IsParams) {
					if (hasCreatedParamArray)
						continue;
					hasCreatedParamArray = true;
				}

				var a = argumentsForCall[i];
				if (a is ByReferenceResolveResult) {
					var r = (ByReferenceResolveResult)a;
					if (r.ElementResult is LocalResolveResult) {
						expressions.Add(CompileLocal(((LocalResolveResult)r.ElementResult).Variable, true));
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
					else if (tokens != null && tokens.Count( t => t.Type == InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier && t.Index == i) > 0) {
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
			var expressionToOrderMap = tokens == null ? new[] { 0 }.Concat(argumentToParameterMap.Select(x => x + 1)).ToList() : CreateInlineCodeExpressionToOrderMap(tokens, argumentsForCall.Count, argumentToParameterMap);
			for (int i = 0; i < expressions.Count; i++) {
				var haveToBeEvaluatedBefore = Enumerable.Range(i + 1, expressions.Count - i - 1).Where(x => expressionToOrderMap[x] < expressionToOrderMap[i]);
				if (haveToBeEvaluatedBefore.Any(other => ExpressionOrderer.DoesOrderMatter(expressions[i], expressions[other]))) {
					CreateTemporariesForExpressionsAndAllRequiredExpressionsLeftOfIt(expressions, i);
				}
			}

			// Rearrange the arguments so they appear in the order the method expects them to.
			if ((argumentToParameterMap.Count != argumentsForCall.Count || argumentToParameterMap.Select((i, n) => new { i, n }).Any(t => t.i != t.n))) {	// If we have an argument to parameter map and it actually performs any reordering.			// Ensure that expressions are evaluated left-to-right in case arguments are reordered
				var newExpressions = new List<JsExpression>() { expressions[0] };
				for (int i = 0; i < argumentsForCall.Count; i++) {
					int specifiedIndex = argumentToParameterMap.IndexOf(i);
					newExpressions.Add(specifiedIndex != -1 ? expressions[specifiedIndex + 1] : VisitResolveResult(argumentsForCall[i], true));	// If the argument was not specified, use the value in argumentsForCall, which has to be constant.
				}
				expressions = newExpressions;
			}

			for (int i = 1; i < expressions.Count; i++) {
				expressions[i] = MaybeCloneValueType(expressions[i], argumentsForCall[i - 1], member.Parameters[Math.Min(i - 1, member.Parameters.Count - 1)].Type);	// Math.Min() because the last parameter might be an expanded param array.
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

		private bool IsReadonlyField(ResolveResult r) {
			for (;;) {
				var mr = r as MemberResolveResult;
				if (mr == null)
					return false;
				var f = mr.Member as IField;
				if (f == null || f.Type.Kind != TypeKind.Struct)
					return false;
				if (f.IsReadOnly)
					return true;
				r = mr.TargetResult;
			}
		}

		private JsExpression CompileMethodInvocation(MethodScriptSemantics sem, IMethod method, ResolveResult targetResult, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, bool isVirtualCall) {
			bool isNonVirtualInvocationOfVirtualMethod = method.IsOverridable && !isVirtualCall;
			bool isParamArrayExpanded = argumentsForCall.Count > 0 && argumentsForCall[argumentsForCall.Count - 1] is ArrayCreateResolveResult;
			bool targetUsedMultipleTimes = sem != null && ((!sem.IgnoreGenericArguments && method.TypeParameters.Count > 0) || (sem.ExpandParams && !isParamArrayExpanded));
			string literalCode = GetActualInlineCode(sem, isNonVirtualInvocationOfVirtualMethod, isParamArrayExpanded);

			var jsTarget = method.IsStatic ? _runtimeLibrary.InstantiateType(method.DeclaringType, this) : InnerCompile(targetResult, targetUsedMultipleTimes, returnMultidimArrayValueByReference: true);
			if (IsMutableValueType(targetResult.Type) && IsReadonlyField(targetResult)) {
				jsTarget = MaybeCloneValueType(jsTarget, null, targetResult.Type, forceClone: true);
			}

			var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, literalCode, jsTarget, false, argumentsForCall, argumentToParameterMap);
			return CompileMethodInvocation(sem, method, thisAndArguments, isNonVirtualInvocationOfVirtualMethod);
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

		private JsExpression CompileMethodInvocation(MethodScriptSemantics impl, IMethod method, IList<JsExpression> thisAndArguments, bool isNonVirtualInvocationOfVirtualMethod) {
			var typeArguments = (method is SpecializedMethod ? ((SpecializedMethod)method).TypeArguments : EmptyList<IType>.Instance);
			var errors = Utils.FindGenericInstantiationErrors(typeArguments, _metadataImporter);
			if (errors.HasErrors) {
				foreach (var ut in errors.UsedUnusableTypes)
					_errorReporter.Message(Messages._7515, ut.FullName, method.DeclaringType.FullName + "." + method.Name);
				foreach (var t in errors.MutableValueTypesBoundToTypeArguments)
					_errorReporter.Message(Messages._7539, t.FullName);
				return JsExpression.Null;
			}

			typeArguments = (impl != null && !impl.IgnoreGenericArguments ? typeArguments : new List<IType>());

			switch (impl.Type) {
				case MethodScriptSemantics.ImplType.NormalMethod: {
					if (isNonVirtualInvocationOfVirtualMethod) {
						return _runtimeLibrary.CallBase(method, thisAndArguments, this);
					}
					else {
						var jsMethod = JsExpression.Member(thisAndArguments[0], impl.Name);
						if (method.IsStatic)
							thisAndArguments = new[] { JsExpression.Null }.Concat(thisAndArguments.Skip(1)).ToList();

						if (typeArguments.Count > 0) {
							return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments, this), impl.ExpandParams, true);
						}
						else
							return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, jsMethod, impl.ExpandParams, false);
					}
				}

				case MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument: {
					var jsMethod = JsExpression.Member(_runtimeLibrary.InstantiateType(method.DeclaringType, this), impl.Name);
					thisAndArguments.Insert(0, JsExpression.Null);
					if (typeArguments.Count > 0) {
						return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments, this), impl.ExpandParams, true);
					}
					else {
						return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, jsMethod, impl.ExpandParams, false);
					}
				}

				case MethodScriptSemantics.ImplType.InlineCode:
					return CompileInlineCodeMethodInvocation(method, GetActualInlineCode(impl, isNonVirtualInvocationOfVirtualMethod, thisAndArguments[thisAndArguments.Count - 1] is JsArrayLiteralExpression), method.IsStatic ? null : thisAndArguments[0], thisAndArguments.Skip(1).ToList());

				case MethodScriptSemantics.ImplType.NativeIndexer:
					return JsExpression.Index(thisAndArguments[0], thisAndArguments[1]);

				default: {
					_errorReporter.Message(Messages._7516, method.DeclaringType.FullName + "." + method.Name);
					return JsExpression.Null;
				}
			}
		}

		private JsExpression ResolveTypeForInlineCode(string typeName) {
			var type = ReflectionHelper.ParseReflectionName(typeName).Resolve(_compilation);
			if (type.Kind == TypeKind.Unknown) {
				_errorReporter.Message(Messages._7525, "Unknown type '" + typeName + "' specified in inline implementation");
				return JsExpression.Null;
			}
			else {
				return _runtimeLibrary.InstantiateType(type, this);
			}
		}

		private JsExpression CompileInlineCodeMethodInvocation(IMethod method, string code, JsExpression @this, IList<JsExpression> arguments) {
			var tokens = InlineCodeMethodCompiler.Tokenize(method, code, s => _errorReporter.Message(Messages._7525, s));
			if (tokens == null) {
				return JsExpression.Null;
			}
			if (tokens.Any(t => t.Type == InlineCodeToken.TokenType.Parameter && t.IsExpandedParamArray) && !(arguments[arguments.Count - 1] is JsArrayLiteralExpression)) {
				_errorReporter.Message(Messages._7525, string.Format("The {0} can only be invoked with its params parameter expanded", method.IsConstructor ? "constructor " + method.DeclaringType.FullName : ("method " + method.FullName)));
				return JsExpression.Null;
			}
			if (method.ReturnType.Kind == TypeKind.Void && !method.IsConstructor) {
				_additionalStatements.AddRange(InlineCodeMethodCompiler.CompileStatementListInlineCodeMethodInvocation(method, tokens, @this, arguments, ResolveTypeForInlineCode, t => _runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, this), s => _errorReporter.Message(Messages._7525, s)));
				return JsExpression.Null;
			}
			else {
				return InlineCodeMethodCompiler.CompileExpressionInlineCodeMethodInvocation(method, tokens, @this, arguments, ResolveTypeForInlineCode, t => _runtimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(t, this), s => _errorReporter.Message(Messages._7525, s));
			}
		}

		private string GetMemberNameForJsonConstructor(IMember member) {
			if (member is IProperty) {
				var currentImpl = _metadataImporter.GetPropertySemantics((IProperty)member);
				if (currentImpl.Type == PropertyScriptSemantics.ImplType.Field) {
					return currentImpl.FieldName;
				}
				else {
					_errorReporter.Message(Messages._7517, member.DeclaringType.FullName + "." + member.Name);
					return null;
				}
			}
			else if (member is IField) {
				var currentImpl = _metadataImporter.GetFieldSemantics((IField)member);
				if (currentImpl.Type == FieldScriptSemantics.ImplType.Field) {
					return currentImpl.Name;
				}
				else {
					_errorReporter.Message(Messages._7518, member.DeclaringType.FullName + "." + member.Name);
					return null;
				}
			}
			else {
				_errorReporter.InternalError("Unsupported member " + member + " in anonymous object initializer.");
				return null;
			}
		}

		private JsExpression CompileJsonConstructorCall(IMethod constructor, ConstructorScriptSemantics impl, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, IList<ResolveResult> initializerStatements) {
			argumentToParameterMap = argumentToParameterMap ?? CreateIdentityArgumentToParameterMap(argumentsForCall.Count);

			var jsPropertyNames = new List<string>();
			var expressions = new List<JsExpression>();
			// Add initializers for specified arguments.
			for (int i = 0; i < argumentToParameterMap.Count; i++) {
				var m = impl.ParameterToMemberMap[argumentToParameterMap[i]];
				string name = GetMemberNameForJsonConstructor(m);
				if (name != null) {
					jsPropertyNames.Add(name);
					expressions.Add(InnerCompile(argumentsForCall[argumentToParameterMap[i]], false, expressions));
				}
			}
			// Add initializers for initializer statements
			foreach (var init in initializerStatements) {
				var orr = init as OperatorResolveResult;
				if (orr != null && orr.OperatorType == ExpressionType.Assign && orr.Operands[0] is MemberResolveResult && ((MemberResolveResult)orr.Operands[0]).TargetResult is InitializedObjectResolveResult) {
					var member = ((MemberResolveResult)orr.Operands[0]).Member;
					string name = GetMemberNameForJsonConstructor(member);
					if (name != null) {
						if (jsPropertyNames.Contains(name)) {
							_errorReporter.Message(Messages._7527, member.Name);
						}
						else {
							jsPropertyNames.Add(name);
							expressions.Add(InnerCompile(orr.Operands[1], false, expressions));
						}
					}
				}
				else {
					_errorReporter.InternalError("Expected an assignment to an InitializedObjectResolveResult, got " + orr);
				}
			}
			// Add initializers for unspecified arguments
			for (int i = 0; i < argumentsForCall.Count; i++) {
				if (!argumentToParameterMap.Contains(i)) {
					string name = GetMemberNameForJsonConstructor(impl.ParameterToMemberMap[i]);
					if (name != null && !jsPropertyNames.Contains(name)) {
						jsPropertyNames.Add(name);
						expressions.Add(InnerCompile(argumentsForCall[i], false, expressions));
					}
				}
			}

			var jsProperties = new List<JsObjectLiteralProperty>();
			for (int i = 0; i < expressions.Count; i++)
				jsProperties.Add(new JsObjectLiteralProperty(jsPropertyNames[i], expressions[i]));
			return JsExpression.ObjectLiteral(jsProperties);
		}

		private JsExpression CompileInitializerStatements(JsExpression objectBeingInitialized, IType objectType, IList<ResolveResult> initializerStatements) {
			if (initializerStatements != null && initializerStatements.Count > 0) {
				var obj = _createTemporaryVariable(objectType);
				var oldObjectBeingInitialized = _objectBeingInitialized;
				_objectBeingInitialized = obj;
				_additionalStatements.Add(JsStatement.Var(_variables[_objectBeingInitialized].Name, objectBeingInitialized));
				foreach (var init in initializerStatements) {
					var js = VisitResolveResult(init, false);
					_additionalStatements.Add(js);
				}
				_objectBeingInitialized = oldObjectBeingInitialized;

				return JsExpression.Identifier(_variables[obj].Name);
			}
			else {
				return objectBeingInitialized;
			}
		}

		private JsExpression CompileConstructorInvocation(ConstructorScriptSemantics impl, IMethod method, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, IList<ResolveResult> initializerStatements) {
			var typeToConstruct = method.DeclaringType;
			var typeToConstructDef = typeToConstruct.GetDefinition();
			if (typeToConstructDef != null && _metadataImporter.GetTypeSemantics(typeToConstructDef).Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_errorReporter.Message(Messages._7519, typeToConstruct.FullName);
				return JsExpression.Null;
			}
			if (typeToConstruct is ParameterizedType) {
				var errors = Utils.FindGenericInstantiationErrors(((ParameterizedType)typeToConstruct).TypeArguments, _metadataImporter);
				if (errors.HasErrors) {
					foreach (var ut in errors.UsedUnusableTypes)
						_errorReporter.Message(Messages._7520, ut.FullName, typeToConstructDef.FullName);
					foreach (var t in errors.MutableValueTypesBoundToTypeArguments)
						_errorReporter.Message(Messages._7539, t.FullName);
					return JsExpression.Null;
				}
			}

			if (impl.Type == ConstructorScriptSemantics.ImplType.Json) {
				return CompileJsonConstructorCall(method, impl, argumentsForCall, argumentToParameterMap, initializerStatements);
			}
			else {
				string literalCode = GetActualInlineCode(impl, argumentsForCall.Count > 0 && argumentsForCall[argumentsForCall.Count - 1] is ArrayCreateResolveResult);
				var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, literalCode, _runtimeLibrary.InstantiateType(method.DeclaringType, this), false, argumentsForCall, argumentToParameterMap);

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

				return CompileInitializerStatements(constructorCall, method.DeclaringType, initializerStatements);
			}
		}

		public override JsExpression VisitInitializedObjectResolveResult(InitializedObjectResolveResult rr, bool data) {
			return JsExpression.Identifier(_variables[_objectBeingInitialized].Name);
		}

		private JsExpression HandleInvocation(IParameterizedMember member, ResolveResult targetResult, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, IList<ResolveResult> initializerStatements, bool isVirtualCall) {
			if (member is IMethod) {
				if (member.DeclaringType.Kind == TypeKind.Delegate && member.Equals(member.DeclaringType.GetDelegateInvokeMethod())) {
					var sem = _metadataImporter.GetDelegateSemantics(member.DeclaringTypeDefinition);

					var thisAndArguments = CompileThisAndArgumentListForMethodCall(member, null, InnerCompile(targetResult, usedMultipleTimes: false, returnMultidimArrayValueByReference: true), false, argumentsForCall, argumentToParameterMap);
					var method = thisAndArguments[0];
					thisAndArguments = thisAndArguments.Skip(1).ToList();

					if (sem.BindThisToFirstParameter) {
						return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, method, sem.ExpandParams, true);
					}
					else {
						thisAndArguments.Insert(0, JsExpression.Null);
						return CompileMethodInvocationWithPotentialExpandParams(thisAndArguments, method, sem.ExpandParams, false);
					}
				}
				else {
					var method = (IMethod)member;
					if (method.IsConstructor) {
						if (method.DeclaringType.Kind == TypeKind.Enum) {
							return JsExpression.Number(0);
						}
						else if (method.DeclaringType.Kind == TypeKind.TypeParameter) {
							var activator = ReflectionHelper.ParseReflectionName("System.Activator").Resolve(_compilation);
							var createInstance = activator.GetMethods(m => m.Name == "CreateInstance" && m.IsStatic && m.TypeParameters.Count == 1 && m.Parameters.Count == 0).Single();
							var createInstanceSpec = new SpecializedMethod(createInstance, new TypeParameterSubstitution(EmptyList<IType>.Instance, new[] { method.DeclaringType }));
							var createdObject = CompileMethodInvocation(_metadataImporter.GetMethodSemantics(createInstanceSpec), createInstanceSpec, new[] { _runtimeLibrary.InstantiateType(activator, this) }, false);
							return CompileInitializerStatements(createdObject, method.DeclaringType, initializerStatements);
						}
						else {
							return CompileConstructorInvocation(_metadataImporter.GetConstructorSemantics(method), method, argumentsForCall, argumentToParameterMap, initializerStatements);
						}
					}
					else {
						return CompileMethodInvocation(_metadataImporter.GetMethodSemantics(method), method, targetResult, argumentsForCall, argumentToParameterMap, isVirtualCall);
					}
				}
			}
			else if (member is IProperty) {
				var property = (IProperty)member;
				var impl = _metadataImporter.GetPropertySemantics(property);
				if (impl.Type != PropertyScriptSemantics.ImplType.GetAndSetMethods) {
					_errorReporter.InternalError("Cannot invoke property that does not have a get method.");
					return JsExpression.Null;
				}
				return CompileMethodInvocation(impl.GetMethod, property.Getter, targetResult, argumentsForCall, argumentToParameterMap, isVirtualCall);
			}
			else {
				_errorReporter.InternalError("Invocation of unsupported member " + member.DeclaringType.FullName + "." + member.Name);
				return JsExpression.Null;
			}
		}

		public override JsExpression VisitInvocationResolveResult(InvocationResolveResult rr, bool data) {
			return HandleInvocation(rr.Member, rr.TargetResult, rr.Arguments, null, rr.InitializerStatements, rr.IsVirtualCall);
		}

		public override JsExpression VisitCSharpInvocationResolveResult(CSharpInvocationResolveResult rr, bool returnValueIsImportant) {
			return HandleInvocation(rr.Member, rr.TargetResult, rr.GetArgumentsForCall(), rr.GetArgumentToParameterMap(), rr.InitializerStatements, rr.IsVirtualCall);
		}

		public override JsExpression VisitConstantResolveResult(ConstantResolveResult rr, bool returnValueIsImportant) {
			if (rr.ConstantValue == null || (rr.Type.Kind == TypeKind.Enum) && rr.ConstantValue.Equals(0))
				return _runtimeLibrary.Default(rr.Type, this);
			else
				return JSModel.Utils.MakeConstantExpression(rr.ConstantValue);
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

		public override JsExpression VisitThisResolveResult(ThisResolveResult rr, bool returnValueIsImportant) {
			return CompileThis();
		}

		private JsExpression CompileLambda(LambdaResolveResult rr, IType returnType, DelegateScriptSemantics semantics) {
			var f = _nestedFunctions[rr];

			var capturedByRefVariables = f.DirectlyOrIndirectlyUsedVariables.Where(v => _variables[v].UseByRefSemantics).ToList();
			if (capturedByRefVariables.Count > 0) {
				var allParents = f.AllParents;
				capturedByRefVariables.RemoveAll(v => !allParents.Any(p => p.DirectlyDeclaredVariables.Contains(v)));	// Remove used byref variables that were declared in this method or any nested method.
			}

			bool captureThis = (_thisAlias == null && f.DirectlyOrIndirectlyUsesThis);
			var newContext = new NestedFunctionContext(capturedByRefVariables);

			JsFunctionDefinitionExpression def;
			if (f.BodyNode is Statement) {
				StateMachineType smt = StateMachineType.NormalMethod;
				IType taskGenericArgument = null;
				if (rr.IsAsync) {
					smt = returnType.IsKnownType(KnownTypeCode.Void) ? StateMachineType.AsyncVoid : StateMachineType.AsyncTask;
					taskGenericArgument = returnType is ParameterizedType ? ((ParameterizedType)returnType).TypeArguments[0] : null;
				}

				def = _createInnerCompiler(newContext).CompileMethod(rr.Parameters, _variables, (BlockStatement)f.BodyNode, false, semantics.ExpandParams, smt, taskGenericArgument);
			}
			else {
				var body = CloneAndCompile(rr.Body, !returnType.IsKnownType(KnownTypeCode.Void), nestedFunctionContext: newContext);
				var lastStatement = returnType.IsKnownType(KnownTypeCode.Void) ? (JsStatement)body.Expression : JsStatement.Return(MaybeCloneValueType(body.Expression, rr.Body, rr.ReturnType));
				var jsBody = JsStatement.Block(MethodCompiler.PrepareParameters(rr.Parameters, _variables, expandParams: semantics.ExpandParams, staticMethodWithThisAsFirstArgument: false).Concat(body.AdditionalStatements).Concat(new[] { lastStatement }));
				def = JsExpression.FunctionDefinition(rr.Parameters.Where((p, i) => i != rr.Parameters.Count - 1 || !semantics.ExpandParams).Select(p => _variables[p].Name), jsBody);
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

		private JsExpression CompileLocal(IVariable variable, bool returnReference) {
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

		public override JsExpression VisitLocalResolveResult(LocalResolveResult rr, bool returnValueIsImportant) {
			return CompileLocal(rr.Variable, false);
		}

		public override JsExpression VisitTypeOfResolveResult(TypeOfResolveResult rr, bool returnValueIsImportant) {
			var errors = Utils.FindTypeUsageErrors(new[] { rr.ReferencedType }, _metadataImporter);
			if (errors.HasErrors) {
				foreach (var ut in errors.UsedUnusableTypes)
					_errorReporter.Message(Messages._7522, ut.FullName);
				foreach (var t in errors.MutableValueTypesBoundToTypeArguments)
					_errorReporter.Message(Messages._7539, t.FullName);

				return JsExpression.Null;
			}
			else
				return _runtimeLibrary.TypeOf(rr.ReferencedType, this);
		}

		public override JsExpression VisitTypeResolveResult(TypeResolveResult rr, bool returnValueIsImportant) {
			throw new NotSupportedException(rr + " should be handled elsewhere");
		}

		public override JsExpression VisitArrayAccessResolveResult(ArrayAccessResolveResult rr, bool returnValueIsImportant) {
			var expressions = new List<JsExpression>();
			expressions.Add(InnerCompile(rr.Array, false, returnMultidimArrayValueByReference: true));
			foreach (var i in rr.Indexes)
				expressions.Add(InnerCompile(i, false, expressions));

			if (rr.Indexes.Count == 1) {
				return JsExpression.Index(expressions[0], expressions[1]);
			}
			else {
				var result = _runtimeLibrary.GetMultiDimensionalArrayValue(expressions[0], expressions.Skip(1), this);
				if (!_returnMultidimArrayValueByReference) {
					var type = NullableType.GetUnderlyingType(rr.Type);
					if (IsMutableValueType(type)) {
						result = _runtimeLibrary.CloneValueType(result, rr.Type, this);
					}
				}
				return result;
			}
		}

		public override JsExpression VisitArrayCreateResolveResult(ArrayCreateResolveResult rr, bool returnValueIsImportant) {
			var at = (ArrayType)rr.Type;

			if (at.Dimensions == 1) {
				if (rr.InitializerElements != null && rr.InitializerElements.Count > 0) {
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

		private JsExpression PerformConversion(JsExpression input, Conversion c, IType fromType, IType toType, ResolveResult csharpInput) {
			if (c.IsIdentityConversion) {
				return input;
			}
			else if (c.IsTryCast) {
				return _runtimeLibrary.TryDowncast(input, fromType, UnpackNullable(toType), this);
			}
			else if (c.IsReferenceConversion) {
				if (toType is ArrayType && fromType is ArrayType)	// Array covariance / contravariance.
					return input;
				else if (toType.Kind == TypeKind.Dynamic)
					return input;
				else if (toType.Kind == TypeKind.Delegate && fromType.Kind == TypeKind.Delegate && !toType.Equals(_compilation.FindType(KnownTypeCode.MulticastDelegate)) && !fromType.Equals(_compilation.FindType(KnownTypeCode.MulticastDelegate)))
					return input;	// Conversion between compatible delegate types.
				else if (c.IsImplicit)
					return _runtimeLibrary.Upcast(input, fromType, toType, this);
				else
					return _runtimeLibrary.Downcast(input, fromType, toType, this);
			}
			else if (c.IsNumericConversion) {
				var result = input;
				if (fromType.IsKnownType(KnownTypeCode.NullableOfT) && !toType.IsKnownType(KnownTypeCode.NullableOfT))
					result = _runtimeLibrary.FromNullable(result, this);

				if (!IsIntegerType(fromType) && IsIntegerType(toType)) {
					result = _runtimeLibrary.FloatToInt(result, this);

					if (fromType.IsKnownType(KnownTypeCode.NullableOfT) && toType.IsKnownType(KnownTypeCode.NullableOfT)) {
						result = _runtimeLibrary.Lift(result, this);
					}
				}
				return result;
			}
			else if (c.IsDynamicConversion) {
				JsExpression result;
				if (toType.IsKnownType(KnownTypeCode.NullableOfT)) {
					// Unboxing to nullable type.
					result = _runtimeLibrary.Downcast(input, fromType, UnpackNullable(toType), this);
				}
				else if (toType.Kind == TypeKind.Struct) {
					// Unboxing to non-nullable type.
					result = _runtimeLibrary.FromNullable(_runtimeLibrary.Downcast(input, fromType, toType, this), this);
				}
				else {
					// Converting to a boring reference type.
					result = _runtimeLibrary.Downcast(input, fromType, toType, this);
				}
				return MaybeCloneValueType(result, null, toType, forceClone: true);
			}
			else if (c.IsNullableConversion || c.IsEnumerationConversion) {
				if (fromType.IsKnownType(KnownTypeCode.NullableOfT) && !toType.IsKnownType(KnownTypeCode.NullableOfT))
					return _runtimeLibrary.FromNullable(input, this);
				return input;
			}
			else if (c.IsBoxingConversion) {
				var box = MaybeCloneValueType(input, null, fromType);

				// Conversion between type parameters are classified as boxing conversions, so it's sometimes an upcast, sometimes a downcast.
				if (toType.Kind == TypeKind.Dynamic) {
					return box;
				}
				else if (NullableType.GetUnderlyingType(fromType).GetAllBaseTypes().Contains(toType)) {
					return _runtimeLibrary.Upcast(box, fromType, toType, this);
				}
				else {
					return _runtimeLibrary.Downcast(box, fromType, toType, this);
				}
			}
			else if (c.IsUnboxingConversion) {
				JsExpression result;
				if (toType.IsKnownType(KnownTypeCode.NullableOfT)) {
					result = _runtimeLibrary.Downcast(input, fromType, UnpackNullable(toType), this);
				}
				else {
					result = _runtimeLibrary.Downcast(input, fromType, toType, this);
					if (toType.Kind == TypeKind.Struct)
						result = _runtimeLibrary.FromNullable(result, this);	// hidden gem in the C# spec: conversions involving type parameter which are not known to not be unboxing are considered unboxing conversions.
				}
				return MaybeCloneValueType(result, null, toType, forceClone: true);
			}
			else if (c.IsUserDefined) {
				input = PerformConversion(input, c.ConversionBeforeUserDefinedOperator, fromType, c.Method.Parameters[0].Type, ((ConversionResolveResult)csharpInput).Input);
				var impl = _metadataImporter.GetMethodSemantics(c.Method);
				var result = CompileMethodInvocation(impl, c.Method, new[] { _runtimeLibrary.InstantiateType(c.Method.DeclaringType, this), input }, false);
				if (c.IsLifted)
					result = _runtimeLibrary.Lift(result, this);
				result = PerformConversion(result, c.ConversionAfterUserDefinedOperator, c.Method.ReturnType, toType, null);
				return result;
			}
			else if (c.IsNullLiteralConversion || c.IsConstantExpressionConversion) {
				return input;
			}

			_errorReporter.InternalError("Conversion " + c + " is not implemented");
			return JsExpression.Null;
		}

		private JsExpression PerformMethodGroupConversionOnNormalMethod(IMethod method, IType delegateType, bool isBaseCall, bool isExtensionMethodGroupConversion, ResolveResult target, MethodScriptSemantics methodSemantics, DelegateScriptSemantics delegateSemantics) {
			if (methodSemantics.ExpandParams != delegateSemantics.ExpandParams) {
				_errorReporter.Message(Messages._7524, method.FullName, delegateType.FullName);
				return JsExpression.Null;
			}

			var typeArguments = (method is SpecializedMethod && !methodSemantics.IgnoreGenericArguments) ? ((SpecializedMethod)method).TypeArguments : new List<IType>();

			JsExpression result;

			if (isBaseCall) {
				// base.Method
				var jsTarget = InnerCompile(target, true);
				result = _runtimeLibrary.BindBaseCall(method, jsTarget, this);
			}
			else if (isExtensionMethodGroupConversion) {
				IList<string> parameters;
				JsExpression body;
				var jsTarget = InnerCompile(target, true);
				if (methodSemantics.ExpandParams) {
					parameters = EmptyList<string>.Instance;
					body = JsExpression.Invocation(JsExpression.Member(JsExpression.Member(_runtimeLibrary.InstantiateType(method.DeclaringType, this), methodSemantics.Name), "apply"), JsExpression.Null, JsExpression.Invocation(JsExpression.Member(JsExpression.ArrayLiteral(jsTarget), "concat"), JsExpression.Invocation(JsExpression.Member(JsExpression.Member(JsExpression.Member(JsExpression.Identifier("Array"), "prototype"), "slice"), "call"), JsExpression.Identifier("arguments"))));
				}
				else {
					parameters = new string[method.Parameters.Count - 1];
					for (int i = 0; i < parameters.Count; i++)
						parameters[i] = _variables[_createTemporaryVariable(method.Parameters[i].Type)].Name;
					body = CompileMethodInvocation(methodSemantics, method, new[] { _runtimeLibrary.InstantiateType(method.DeclaringType, this), jsTarget }.Concat(parameters.Select(JsExpression.Identifier)).ToList(), false);
				}
				result = JsExpression.FunctionDefinition(parameters, method.ReturnType.IsKnownType(KnownTypeCode.Void) ? (JsStatement)body : JsStatement.Return(body));
				if (UsesThisVisitor.Analyze(body))
					result = _runtimeLibrary.Bind(result, JsExpression.This, this);
			}
			else {
				JsExpression jsTarget, jsMethod;

				if (method.IsStatic) {
					jsTarget = null;
					jsMethod = JsExpression.Member(_runtimeLibrary.InstantiateType(method.DeclaringType, this), methodSemantics.Name);
				}
				else {
					jsTarget = InnerCompile(target, true);
					jsMethod = JsExpression.Member(jsTarget, methodSemantics.Name);
				}

				if (typeArguments.Count > 0) {
					jsMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments, this);
				}

				result = jsTarget != null ? _runtimeLibrary.Bind(jsMethod, jsTarget, this) : jsMethod;
			}

			if (delegateSemantics.BindThisToFirstParameter)
				result = _runtimeLibrary.BindFirstParameterToThis(result, this);

			return result;
		}

		private JsExpression PerformMethodGroupConversionOnInlineCodeMethod(IMethod method, IType delegateType, bool isBaseCall, bool isExtensionMethodGroupConversion, ResolveResult target, MethodScriptSemantics methodSemantics, DelegateScriptSemantics delegateSemantics) {
			string code = isBaseCall ? methodSemantics.NonVirtualInvocationLiteralCode : methodSemantics.NonExpandedFormLiteralCode;
			var tokens = InlineCodeMethodCompiler.Tokenize(method, code, s => _errorReporter.Message(Messages._7525, s));
			if (tokens == null) {
				return JsExpression.Null;
			}
			else if (tokens.Any(t => t.Type == InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier)) {
				_errorReporter.Message(Messages._7523, method.FullName, "it uses a literal string as code ({@arg})");
				return JsExpression.Null;
			}
			else if (tokens.Any(t => t.Type == InlineCodeToken.TokenType.Parameter && t.IsExpandedParamArray)) {
				_errorReporter.Message(Messages._7523, method.FullName, "it has an expanded param array parameter ({*arg})");
				return JsExpression.Null;
			}

			var parameters = new string[method.Parameters.Count - (delegateSemantics.ExpandParams ? 1 : 0) - (isExtensionMethodGroupConversion ? 1 : 0)];
			for (int i = 0; i < parameters.Length; i++)
				parameters[i] = _variables[_createTemporaryVariable(method.Parameters[i].Type)].Name;

			var jsTarget = method.IsStatic && !isExtensionMethodGroupConversion ? JsExpression.Null : InnerCompile(target, tokens.Count(t => t.Type == InlineCodeToken.TokenType.This) > 1);
			var arguments = new List<JsExpression>();
			if (isExtensionMethodGroupConversion)
				arguments.Add(jsTarget);
			arguments.AddRange(parameters.Select(p => (JsExpression)JsExpression.Identifier(p)));
			if (delegateSemantics.ExpandParams)
				arguments.Add(JsExpression.Invocation(JsExpression.Member(JsExpression.Member(JsExpression.Member(JsExpression.Identifier("Array"), "prototype"), "slice"), "call"), JsExpression.Identifier("arguments"), JsExpression.Number(parameters.Length)));

			bool usesThis;
			JsExpression result;
			if (method.ReturnType.IsKnownType(KnownTypeCode.Void)) {
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

		private JsExpression PerformMethodGroupConversionOnStaticMethodWithThisAsFirstArgument(IMethod method, IType delegateType, bool isBaseCall, ResolveResult target, MethodScriptSemantics methodSemantics, DelegateScriptSemantics delegateSemantics) {
			if (methodSemantics.ExpandParams != delegateSemantics.ExpandParams) {
				_errorReporter.Message(Messages._7524, method.FullName, delegateType.FullName);
				return JsExpression.Null;
			}

			JsExpression result;
			if (methodSemantics.ExpandParams) {
				var body = JsExpression.Invocation(JsExpression.Member(JsExpression.Member(_runtimeLibrary.InstantiateType(method.DeclaringType, this), methodSemantics.Name), "apply"), JsExpression.Null, JsExpression.Invocation(JsExpression.Member(JsExpression.ArrayLiteral(JsExpression.This), "concat"), JsExpression.Invocation(JsExpression.Member(JsExpression.Member(JsExpression.Member(JsExpression.Identifier("Array"), "prototype"), "slice"), "call"), JsExpression.Identifier("arguments"))));
				result = JsExpression.FunctionDefinition(new string[0], method.ReturnType.IsKnownType(KnownTypeCode.Void) ? (JsStatement)body : JsStatement.Return(body));
			}
			else {
				var parameters = new string[method.Parameters.Count];
				for (int i = 0; i < method.Parameters.Count; i++)
					parameters[i] = _variables[_createTemporaryVariable(method.Parameters[i].Type)].Name;

				var body = JsExpression.Invocation(JsExpression.Member(_runtimeLibrary.InstantiateType(method.DeclaringType, this), methodSemantics.Name), new[] { JsExpression.This }.Concat(parameters.Select(p => (JsExpression)JsExpression.Identifier(p))));
				result = JsExpression.FunctionDefinition(parameters, method.ReturnType.IsKnownType(KnownTypeCode.Void) ? (JsStatement)body : JsStatement.Return(body));
			}

			result = _runtimeLibrary.Bind(result, InnerCompile(target, false), this);
			if (delegateSemantics.BindThisToFirstParameter)
				result = _runtimeLibrary.BindFirstParameterToThis(result, this);
			return result;
		}

		public override JsExpression VisitConversionResolveResult(ConversionResolveResult rr, bool returnValueIsImportant) {
			if (rr.Conversion.IsAnonymousFunctionConversion) {
				if (rr.Type.FullName == typeof(System.Linq.Expressions.Expression).FullName && rr.Type.TypeParameterCount == 1) {
					var tree = new ExpressionTreeBuilder(_compilation,
					                                     _metadataImporter,
					                                     t => { var v = _createTemporaryVariable(t); return _variables[v].Name; },
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
					                                                             var temp = _createTemporaryVariable(rr.Type);
					                                                             c._additionalStatements = c._additionalStatements ?? new List<JsStatement>();
					                                                             c._additionalStatements.Add(JsStatement.Var(_variables[temp].Name, a[i]));
					                                                             a[i] = JsExpression.Identifier(_variables[temp].Name);
					                                                         }
					                                                     }
					                                                 }
					                                             }
					                                         }
					                                         var e = c.CompileMethodInvocation(_metadataImporter.GetMethodSemantics(m), m, new[] { m.IsStatic ? _runtimeLibrary.InstantiateType(m.DeclaringType, this) : t }.Concat(a).ToList(), false);
					                                         return new ExpressionCompileResult(e, c._additionalStatements);
					                                     },
					                                     t => _runtimeLibrary.InstantiateType(t, this),
					                                     t => _runtimeLibrary.Default(t, this),
					                                     m => _runtimeLibrary.GetMember(m, this),
					                                     v => _runtimeLibrary.GetExpressionForLocal(v.Name, CompileLocal(v, false), v.Type, this),
					                                     CompileThis()
					                                    ).BuildExpressionTree((LambdaResolveResult)rr.Input);
					_additionalStatements.AddRange(tree.AdditionalStatements);
					return tree.Expression;
				}
				else {
					var retType = rr.Type.GetDelegateInvokeMethod().ReturnType;
					return CompileLambda((LambdaResolveResult)rr.Input, retType, _metadataImporter.GetDelegateSemantics(rr.Type.GetDefinition()));
				}
			}
			else if (rr.Conversion.IsMethodGroupConversion) {
				var mgrr = (MethodGroupResolveResult)rr.Input;

				if (mgrr.TargetResult.Type.Kind == TypeKind.Delegate && Equals(rr.Conversion.Method, mgrr.TargetResult.Type.GetDelegateInvokeMethod())) {
					var sem1 = _metadataImporter.GetDelegateSemantics(mgrr.TargetResult.Type.GetDefinition());
					var sem2 = _metadataImporter.GetDelegateSemantics(rr.Type.GetDefinition());
					if (sem1.BindThisToFirstParameter != sem2.BindThisToFirstParameter) {
						_errorReporter.Message(Messages._7533, mgrr.TargetType.FullName, rr.Type.FullName);
						return JsExpression.Null;
					}
					if (sem1.ExpandParams != sem2.ExpandParams) {
						_errorReporter.Message(Messages._7537, mgrr.TargetType.FullName, rr.Type.FullName);
						return JsExpression.Null;
					}

					return _runtimeLibrary.CloneDelegate(InnerCompile(mgrr.TargetResult, false), rr.Conversion.Method.DeclaringType, rr.Type, this);	// new D2(d1)
				}

				var methodSemantics = _metadataImporter.GetMethodSemantics(rr.Conversion.Method);
				var delegateSemantics = _metadataImporter.GetDelegateSemantics(rr.Type.GetDefinition());
				switch (methodSemantics.Type) {
					case MethodScriptSemantics.ImplType.NormalMethod:
						return PerformMethodGroupConversionOnNormalMethod(rr.Conversion.Method, rr.Type, rr.Conversion.Method.IsOverridable && !rr.Conversion.IsVirtualMethodLookup, rr.Conversion.Method.IsStatic && rr.Conversion.DelegateCapturesFirstArgument, mgrr.TargetResult, methodSemantics, delegateSemantics);
					case MethodScriptSemantics.ImplType.InlineCode:
						return PerformMethodGroupConversionOnInlineCodeMethod(rr.Conversion.Method, rr.Type, rr.Conversion.Method.IsOverridable && !rr.Conversion.IsVirtualMethodLookup, rr.Conversion.Method.IsStatic && rr.Conversion.DelegateCapturesFirstArgument, mgrr.TargetResult, methodSemantics, delegateSemantics);
					case MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument:
						return PerformMethodGroupConversionOnStaticMethodWithThisAsFirstArgument(rr.Conversion.Method, rr.Type, rr.Conversion.Method.IsOverridable && !rr.Conversion.IsVirtualMethodLookup, mgrr.TargetResult, methodSemantics, delegateSemantics);
					default:
						_errorReporter.Message(Messages._7523, rr.Conversion.Method.FullName, "it is not a normal method");
						return JsExpression.Null;
				}
			}
			else {
				return PerformConversion(VisitResolveResult(rr.Input, true), rr.Conversion, rr.Input.Type, rr.Type, rr);
			}
		}

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
	}
}
