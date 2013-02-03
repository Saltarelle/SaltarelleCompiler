using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Compiler {
	public class NestedFunctionContext {
		public ReadOnlySet<IVariable> CapturedByRefVariables { get; private set; }
		public ReadOnlySet<IVariable> ExpandParamsVariables { get; private set; }

		public NestedFunctionContext(IEnumerable<IVariable> capturedByRefVariables, IEnumerable<IVariable> expandParamsVariables) {
			var crv = new HashSet<IVariable>();
			foreach (var v in capturedByRefVariables)
				crv.Add(v);

			var epv = new HashSet<IVariable>();
			foreach (var v in expandParamsVariables)
				epv.Add(v);

			CapturedByRefVariables = new ReadOnlySet<IVariable>(crv);
			ExpandParamsVariables  = new ReadOnlySet<IVariable>(epv);
		}
	}

	public class ExpressionCompiler : ResolveResultVisitor<JsExpression, bool> {
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
		private IVariable _objectBeingInitialized;

		public class Result {
			public JsExpression Expression { get; set; }
			public ReadOnlyCollection<JsStatement> AdditionalStatements { get; private set; }

			public Result(JsExpression expression, IEnumerable<JsStatement> additionalStatements) {
				this.Expression           = expression;
				this.AdditionalStatements = additionalStatements.AsReadOnly();
			}
		}

		public ExpressionCompiler(ICompilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, IDictionary<IVariable, VariableData> variables, IDictionary<LambdaResolveResult, NestedFunctionData> nestedFunctions, Func<IType, IVariable> createTemporaryVariable, Func<NestedFunctionContext, StatementCompiler> createInnerCompiler, string thisAlias, NestedFunctionContext nestedFunctionContext, IVariable objectBeingInitialized, IMethod methodBeingCompiled, ITypeDefinition typeBeingCompiled) {
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
		}

		private List<JsStatement> _additionalStatements;

		public Result Compile(ResolveResult expression, bool returnValueIsImportant) {
			_additionalStatements = new List<JsStatement>();
			var expr = VisitResolveResult(expression, returnValueIsImportant);
			return new Result(expr, _additionalStatements);
		}

		public IList<JsStatement> CompileConstructorInitializer(IMethod method, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, IList<ResolveResult> initializerStatements, bool currentIsStaticMethod, bool isExpandedForm) {
			_additionalStatements = new List<JsStatement>();
			var impl = _metadataImporter.GetConstructorSemantics(method);

			if (currentIsStaticMethod) {
				_additionalStatements.Add(new JsVariableDeclarationStatement(_thisAlias, CompileConstructorInvocation(impl, method, argumentsForCall, argumentToParameterMap, initializerStatements, isExpandedForm)));
			}
			else {
				if (impl.ExpandParams && !isExpandedForm) {
					_errorReporter.Message(Messages._7502, method.DeclaringType.FullName + "." + method.DeclaringType.Name);
				}

				var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, impl.Type == ConstructorScriptSemantics.ImplType.InlineCode ? impl.LiteralCode : null, GetScriptType(method.DeclaringType, TypeContext.UseStaticMember), false, argumentsForCall, argumentToParameterMap, impl.ExpandParams && isExpandedForm);
				var jsType           = thisAndArguments[0];
				thisAndArguments[0]  = CompileThis();	// Swap out the TypeResolveResult that we get as default.

				switch (impl.Type) {
					case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
						_additionalStatements.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Member(jsType, "call"), thisAndArguments)));
						break;

					case ConstructorScriptSemantics.ImplType.NamedConstructor:
						_additionalStatements.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.Member(JsExpression.Member(jsType, impl.Name), "call"), thisAndArguments)));
						break;

					case ConstructorScriptSemantics.ImplType.StaticMethod:
						_errorReporter.Message(Messages._7503);
						break;

					case ConstructorScriptSemantics.ImplType.InlineCode:
						_errorReporter.Message(Messages._7504);
						break;

					case ConstructorScriptSemantics.ImplType.Json:
						_errorReporter.Message(Messages._7532);
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

		private Result CloneAndCompile(ResolveResult expression, bool returnValueIsImportant, NestedFunctionContext nestedFunctionContext = null) {
			return new ExpressionCompiler(_compilation, _metadataImporter, _namer, _runtimeLibrary, _errorReporter, _variables, _nestedFunctions, _createTemporaryVariable, _createInnerCompiler, _thisAlias, nestedFunctionContext ?? _nestedFunctionContext, _objectBeingInitialized, _methodBeingCompiled, _typeBeingCompiled).Compile(expression, returnValueIsImportant);
		}

		private void CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(IList<JsExpression> expressions, Result newExpressions) {
			for (int i = 0; i < expressions.Count; i++) {
				if (ExpressionOrderer.DoesOrderMatter(expressions[i], newExpressions)) {
					var temp = _createTemporaryVariable(_compilation.FindType(KnownTypeCode.Object));
					_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, expressions[i]));
					expressions[i] = JsExpression.Identifier(_variables[temp].Name);
				}
			}
		}

		private void CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(IList<JsExpression> expressions, JsExpression newExpression) {
			CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(expressions, new Result(newExpression, new JsStatement[0]));
		}

		private JsExpression ResolveTypeParameter(ITypeParameter tp) {
			bool unusable = false;
			switch (tp.OwnerType) {
				case EntityType.TypeDefinition:
					unusable = _metadataImporter.GetTypeSemantics(_typeBeingCompiled).IgnoreGenericArguments;
					break;
				case EntityType.Method:
					unusable = _metadataImporter.GetMethodSemantics(_methodBeingCompiled).IgnoreGenericArguments;
					break;
				default:
					_errorReporter.InternalError("Invalid owner " + tp.OwnerType + " for type parameter " + tp);
					return JsExpression.Null;
			}
			if (unusable) {
				_errorReporter.Message(Messages._7536, tp.Name, tp.OwnerType == EntityType.TypeDefinition ? "type" : "method", tp.OwnerType == EntityType.TypeDefinition ? _methodBeingCompiled.DeclaringTypeDefinition.FullName : _methodBeingCompiled.FullName);
				return JsExpression.Null;
			}
			return JsExpression.Identifier(_namer.GetTypeParameterName(tp));
		}

		private JsExpression GetScriptType(IType type, TypeContext context) {
			return _runtimeLibrary.GetScriptType(type, context, ResolveTypeParameter);
		}

		private JsExpression InnerCompile(ResolveResult rr, bool usedMultipleTimes, IList<JsExpression> expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression) {
			var result = CloneAndCompile(rr, true);

			bool needsTemporary = usedMultipleTimes && IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(result.Expression);
			if (result.AdditionalStatements.Count > 0 || needsTemporary) {
				CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression, result);
			}

			_additionalStatements.AddRange(result.AdditionalStatements);

			if (needsTemporary) {
				var temp = _createTemporaryVariable(rr.Type);
				_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, result.Expression));
				return JsExpression.Identifier(_variables[temp].Name);
			}
			else {
				return result.Expression;
			}
		}

		private JsExpression InnerCompile(ResolveResult rr, bool usedMultipleTimes, ref JsExpression expressionThatHasToBeEvaluatedInOrderBeforeThisExpression) {
			var l = new List<JsExpression>();
			if (expressionThatHasToBeEvaluatedInOrderBeforeThisExpression != null)
				l.Add(expressionThatHasToBeEvaluatedInOrderBeforeThisExpression);
			var r = InnerCompile(rr, usedMultipleTimes, l);
			if (l.Count > 0)
				expressionThatHasToBeEvaluatedInOrderBeforeThisExpression = l[0];
			return r;
		}

		private JsExpression InnerCompile(ResolveResult rr, bool usedMultipleTimes) {
			JsExpression _ = null;
			return InnerCompile(rr, usedMultipleTimes, ref _);
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
			var jsTarget = target.Member.IsStatic ? GetScriptType(target.Member.DeclaringType, TypeContext.UseStaticMember) : InnerCompile(target.TargetResult, compoundFactory == null);
			var jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, ref jsTarget) : null);
			var access = JsExpression.Member(jsTarget, fieldName);
			if (compoundFactory != null) {
				return compoundFactory(access, jsOtherOperand);
			}
			else {
				if (returnValueIsImportant && returnValueBeforeChange) {
					var temp = _createTemporaryVariable(target.Type);
					_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, access));
					_additionalStatements.Add( new JsExpressionStatement(JsExpression.Assign(access, valueFactory(JsExpression.Identifier(_variables[temp].Name), jsOtherOperand))));
					return JsExpression.Identifier(_variables[temp].Name);
				}
				else {
					return JsExpression.Assign(access, valueFactory(access, jsOtherOperand));
				}
			}
		}

		private JsExpression CompileArrayAccessCompoundAssignment(ResolveResult array, ResolveResult index, ResolveResult otherOperand, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool returnValueBeforeChange) {
			var expressions = new List<JsExpression>();
			expressions.Add(InnerCompile(array, compoundFactory == null, expressions));
			expressions.Add(InnerCompile(index, compoundFactory == null, expressions));
			var jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, expressions) : null);
			var access = JsExpression.Index(expressions[0], expressions[1]);

			if (compoundFactory != null) {
				return compoundFactory(access, jsOtherOperand);
			}
			else {
				if (returnValueIsImportant && returnValueBeforeChange) {
					var temp = _createTemporaryVariable(_compilation.FindType(KnownTypeCode.Object));
					_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, access));
					_additionalStatements.Add(new JsExpressionStatement(JsExpression.Assign(access, valueFactory(JsExpression.Identifier(_variables[temp].Name), jsOtherOperand))));
					return JsExpression.Identifier(_variables[temp].Name);
				}
				else {
					return JsExpression.Assign(access, valueFactory(access, jsOtherOperand));
				}
			}
		}

		private JsExpression CompileCompoundAssignment(ResolveResult target, ResolveResult otherOperand, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool isLifted, bool returnValueBeforeChange = false, bool oldValueIsImportant = true) {
			if (isLifted) {
				compoundFactory = null;
				var oldVF       = valueFactory;
				valueFactory    = (a, b) => _runtimeLibrary.Lift(oldVF(a, b));
			}

			if (target is LocalResolveResult || target is DynamicMemberResolveResult || target is DynamicInvocationResolveResult /* Dynamic indexing is an invocation */) {
				JsExpression jsTarget, jsOtherOperand;
				jsTarget = InnerCompile(target, compoundFactory == null);
				if (target is LocalResolveResult) {
					jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false) : null);	// If the variable is a by-ref variable we will get invalid reordering if we force the target to be evaluated before the other operand.
				}
				else {
					jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, ref jsTarget) : null);
				}

				if (compoundFactory != null) {
					return compoundFactory(jsTarget, jsOtherOperand);
				}
				else {
					if (returnValueIsImportant && returnValueBeforeChange) {
						var temp = _createTemporaryVariable(target.Type);
						_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, jsTarget));
						_additionalStatements.Add( new JsExpressionStatement(JsExpression.Assign(jsTarget, valueFactory(JsExpression.Identifier(_variables[temp].Name), jsOtherOperand))));
						return JsExpression.Identifier(_variables[temp].Name);
					}
					else {
						return JsExpression.Assign(jsTarget, valueFactory(jsTarget, jsOtherOperand));
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
								return CompileArrayAccessCompoundAssignment(mrr.TargetResult, ((CSharpInvocationResolveResult)mrr).Arguments[0], otherOperand, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
							}
							else {
								List<JsExpression> thisAndArguments;
								if (property.IsIndexer) {
									var invocation = (CSharpInvocationResolveResult)target;
									thisAndArguments = CompileThisAndArgumentListForMethodCall(invocation.Member, null, InnerCompile(invocation.TargetResult, oldValueIsImportant), oldValueIsImportant, invocation.GetArgumentsForCall(), invocation.GetArgumentToParameterMap(), false);
								}
								else {
									thisAndArguments = new List<JsExpression> { mrr.Member.IsStatic ? GetScriptType(mrr.Member.DeclaringType, TypeContext.UseStaticMember) : InnerCompile(mrr.TargetResult, oldValueIsImportant) };
								}

								JsExpression oldValue, jsOtherOperand;
								if (oldValueIsImportant) {
									thisAndArguments.Add(CompileMethodInvocation(impl.GetMethod, property.Getter, thisAndArguments, mrr.Member.IsOverridable && !mrr.IsVirtualCall, false));
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
										_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, valueToReturn));
										valueToReturn = JsExpression.Identifier(_variables[temp].Name);
									}

									var newValue = (returnValueBeforeChange ? valueFactory(valueToReturn, jsOtherOperand) : valueToReturn);

									thisAndArguments.Add(newValue);
									_additionalStatements.Add(new JsExpressionStatement(CompileMethodInvocation(impl.SetMethod, property.Setter, thisAndArguments, mrr.Member.IsOverridable && !mrr.IsVirtualCall, false)));
									return valueToReturn;
								}
								else {
									thisAndArguments.Add(valueFactory(oldValue, jsOtherOperand));
									return CompileMethodInvocation(impl.SetMethod, property.Setter, thisAndArguments, mrr.Member.IsOverridable && !mrr.IsVirtualCall, false);
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
					return CompileArrayAccessCompoundAssignment(arr.Array, arr.Indexes[0], otherOperand, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
				}
				else {
					var expressions = new List<JsExpression>();
					expressions.Add(InnerCompile(arr.Array, oldValueIsImportant, expressions));
					foreach (var i in arr.Indexes)
						expressions.Add(InnerCompile(i, oldValueIsImportant, expressions));

					JsExpression oldValue, jsOtherOperand;
					if (oldValueIsImportant) {
						expressions.Add(_runtimeLibrary.GetMultiDimensionalArrayValue(expressions[0], expressions.Skip(1)));
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
							_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, valueToReturn));
							valueToReturn = JsExpression.Identifier(_variables[temp].Name);
						}

						var newValue = (returnValueBeforeChange ? valueFactory(valueToReturn, jsOtherOperand) : valueToReturn);

						_additionalStatements.Add(new JsExpressionStatement(_runtimeLibrary.SetMultiDimensionalArrayValue(expressions[0], expressions.Skip(1), newValue)));
						return valueToReturn;
					}
					else {
						return _runtimeLibrary.SetMultiDimensionalArrayValue(expressions[0], expressions.Skip(1), valueFactory(oldValue, jsOtherOperand));
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
			return isLifted ? _runtimeLibrary.Lift(result) : result;
		}

		private JsExpression CompileUnaryOperator(ResolveResult operand, Func<JsExpression, JsExpression> resultFactory, bool isLifted) {
			var jsOperand = InnerCompile(operand, false);
			var result = resultFactory(jsOperand);
			return isLifted ? _runtimeLibrary.Lift(result) : result;
		}

		private JsExpression CompileConditionalOperator(ResolveResult test, ResolveResult truePath, ResolveResult falsePath) {
			var jsTest      = VisitResolveResult(test, true);
			var trueResult  = CloneAndCompile(truePath, true);
			var falseResult = CloneAndCompile(falsePath, true);

			if (trueResult.AdditionalStatements.Count > 0 || falseResult.AdditionalStatements.Count > 0) {
				var temp = _createTemporaryVariable(truePath.Type);
				var trueBlock  = new JsBlockStatement(trueResult.AdditionalStatements.Concat(new[] { new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), trueResult.Expression))  }));
				var falseBlock = new JsBlockStatement(falseResult.AdditionalStatements.Concat(new[] { new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), falseResult.Expression)) }));
				_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, null));
				_additionalStatements.Add(new JsIfStatement(jsTest, trueBlock, falseBlock));
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
				return _runtimeLibrary.Coalesce(jsLeft, jsRight.Expression);
			}
			else {
				var temp = _createTemporaryVariable(resultType);
				var nullBlock  = new JsBlockStatement(jsRight.AdditionalStatements.Concat(new[] { new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), jsRight.Expression))  }));
				_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, jsLeft));
				_additionalStatements.Add(new JsIfStatement(_runtimeLibrary.ReferenceEquals(JsExpression.Identifier(_variables[temp].Name), JsExpression.Null), nullBlock, null));
				return JsExpression.Identifier(_variables[temp].Name);
			}
		}

		private JsExpression CompileEventAddOrRemove(MemberResolveResult target, ResolveResult value, bool isAdd) {
			var evt = (IEvent)target.Member;
			var impl = _metadataImporter.GetEventSemantics(evt);
			switch (impl.Type) {
				case EventScriptSemantics.ImplType.AddAndRemoveMethods: {
					var accessor = isAdd ? evt.AddAccessor : evt.RemoveAccessor;
					return CompileMethodInvocation(isAdd ? impl.AddMethod : impl.RemoveMethod, accessor, target.TargetResult, new[] { value }, new[] { 0 }, target.IsVirtualCall, false);
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
				var ifBlock = new JsBlockStatement(jsRight.AdditionalStatements.Concat(new[] { new JsExpressionStatement(JsExpression.Assign(JsExpression.Identifier(_variables[temp].Name), jsRight.Expression))  }));
				_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, jsLeft));
				JsExpression test = JsExpression.Identifier(_variables[temp].Name);
				if (!isAndAlso)
					test = JsExpression.LogicalNot(test);
				_additionalStatements.Add(new JsIfStatement(test, ifBlock, null));
				return JsExpression.Identifier(_variables[temp].Name);
			}
			else {
				return isAndAlso ? JsExpression.LogicalAnd(jsLeft, jsRight.Expression) : JsExpression.LogicalOr(jsLeft, jsRight.Expression);
			}
		}

		public override JsExpression VisitOperatorResolveResult(OperatorResolveResult rr, bool returnValueIsImportant) {
			if (rr.UserDefinedOperatorMethod != null) {
				var impl = _metadataImporter.GetMethodSemantics(rr.UserDefinedOperatorMethod);
				if (impl.Type != MethodScriptSemantics.ImplType.NativeOperator) {
					switch (rr.Operands.Count) {
						case 1: {
							Func<JsExpression, JsExpression, JsExpression> invocation = (a, b) => CompileMethodInvocation(impl, rr.UserDefinedOperatorMethod, new[] { GetScriptType(rr.UserDefinedOperatorMethod.DeclaringType, TypeContext.UseStaticMember), a }, false, false);
							switch (rr.OperatorType) {
								case ExpressionType.PreIncrementAssign:
									return CompileCompoundAssignment(rr.Operands[0], null, null, invocation, returnValueIsImportant, rr.IsLiftedOperator);
								case ExpressionType.PreDecrementAssign:
									return CompileCompoundAssignment(rr.Operands[0], null, null, invocation, returnValueIsImportant, rr.IsLiftedOperator);
								case ExpressionType.PostIncrementAssign:
									return CompileCompoundAssignment(rr.Operands[0], null, null, invocation, returnValueIsImportant, rr.IsLiftedOperator, returnValueBeforeChange: true);
								case ExpressionType.PostDecrementAssign:
									return CompileCompoundAssignment(rr.Operands[0], null, null, invocation, returnValueIsImportant, rr.IsLiftedOperator, returnValueBeforeChange: true);
								default:
									return CompileUnaryOperator(rr.Operands[0], a => CompileMethodInvocation(impl, rr.UserDefinedOperatorMethod, new[] { GetScriptType(rr.UserDefinedOperatorMethod.DeclaringType, TypeContext.UseStaticMember), a }, false, false), rr.IsLiftedOperator);
							}
						}

						case 2: {
							Func<JsExpression, JsExpression, JsExpression> invocation = (a, b) => CompileMethodInvocation(impl, rr.UserDefinedOperatorMethod, new[] { GetScriptType(rr.UserDefinedOperatorMethod.DeclaringType, TypeContext.UseStaticMember), a, b }, false, false);
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
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => CompileMethodInvocation(impl, combine, new[] { GetScriptType(del, TypeContext.UseStaticMember), a, b }, false, false), returnValueIsImportant, false);
					}
					else {
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.AddAssign, JsExpression.Add, returnValueIsImportant, rr.IsLiftedOperator);
					}

				case ExpressionType.AndAssign:
					if (IsNullableBooleanType(rr.Operands[0].Type))
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => _runtimeLibrary.LiftedBooleanAnd(a, b), returnValueIsImportant, false);
					else
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseAndAssign, JsExpression.BitwiseAnd, returnValueIsImportant, rr.IsLiftedOperator);

				case ExpressionType.DivideAssign:
					if (IsIntegerType(rr.Type))
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => _runtimeLibrary.IntegerDivision(a, b), returnValueIsImportant, rr.IsLiftedOperator);
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
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => _runtimeLibrary.LiftedBooleanOr(a, b), returnValueIsImportant, false);
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
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => CompileMethodInvocation(impl, remove, new[] { GetScriptType(del, TypeContext.UseStaticMember), a, b }, false, false), returnValueIsImportant, false);
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
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => CompileMethodInvocation(impl, combine, new[] { GetScriptType(del, TypeContext.UseStaticMember), a, b }, false, false), false);
					}
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Add, rr.IsLiftedOperator);

				case ExpressionType.And:
					if (IsNullableBooleanType(rr.Operands[0].Type))
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => _runtimeLibrary.LiftedBooleanAnd(a, b), false);	// We have already lifted it, so it should not be lifted again.
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseAnd, rr.IsLiftedOperator);

				case ExpressionType.AndAlso:
					return CompileAndAlsoOrOrElse(rr.Operands[0], rr.Operands[1], true);

				case ExpressionType.Coalesce:
					return CompileCoalesce(rr.Type, rr.Operands[0], rr.Operands[1]);

				case ExpressionType.Divide:
					if (IsIntegerType(rr.Type))
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => _runtimeLibrary.IntegerDivision(a, b), rr.IsLiftedOperator);
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Divide, rr.IsLiftedOperator);

				case ExpressionType.ExclusiveOr:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseXor, rr.IsLiftedOperator);

				case ExpressionType.GreaterThan:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Greater, rr.IsLiftedOperator);

				case ExpressionType.GreaterThanOrEqual:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.GreaterOrEqual, rr.IsLiftedOperator);

				case ExpressionType.Equal:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => rr.Operands[0].Type.IsReferenceType == false || rr.Operands[1].Type.IsReferenceType == false ? JsExpression.Same(a, b) : _runtimeLibrary.ReferenceEquals(a, b), rr.IsLiftedOperator);

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
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => rr.Operands[0].Type.IsReferenceType == false || rr.Operands[1].Type.IsReferenceType == false ? JsExpression.NotSame(a, b) : _runtimeLibrary.ReferenceNotEquals(a, b), rr.IsLiftedOperator);

				case ExpressionType.Or:
					if (IsNullableBooleanType(rr.Operands[0].Type))
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => _runtimeLibrary.LiftedBooleanOr(a, b), false);	// We have already lifted it, so it should not be lifted again.
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
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => CompileMethodInvocation(impl, remove, new[] { GetScriptType(del, TypeContext.UseStaticMember), a, b }, false, false), false);
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
			var thisAndArguments = (combine.IsStatic ? new[] { GetScriptType(del, TypeContext.UseStaticMember), a, b } : new[] { a, b });
			return CompileMethodInvocation(impl, combine, thisAndArguments, false, false);
		}

		public JsExpression CompileDelegateRemoveCall(JsExpression a, JsExpression b) {
			var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
			var remove = del.GetMethods().Single(m => m.Name == "Remove" && m.Parameters.Count == 2);
			var impl = _metadataImporter.GetMethodSemantics(remove);
			var thisAndArguments = (remove.IsStatic ? new[] { GetScriptType(del, TypeContext.UseStaticMember), a, b } : new[] { a, b });
			return CompileMethodInvocation(impl, remove, thisAndArguments, false, false);
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
						return CompileMethodInvocation(impl.GetMethod, getter, rr.TargetResult, new ResolveResult[0], new int[0], rr.IsVirtualCall, false);	// We know we have no arguments because indexers are treated as invocations.
					}
					case PropertyScriptSemantics.ImplType.Field: {
						return JsExpression.Member(rr.Member.IsStatic ? GetScriptType(rr.Member.DeclaringType, TypeContext.UseStaticMember) : InnerCompile(rr.TargetResult, false), impl.FieldName);
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
						return JsExpression.Member(rr.Member.IsStatic ? GetScriptType(rr.Member.DeclaringType, TypeContext.UseStaticMember) : InnerCompile(rr.TargetResult, false), impl.Name);
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
				return JsExpression.Member(rr.Member.IsStatic ? GetScriptType(rr.Member.DeclaringType, TypeContext.UseStaticMember) : VisitResolveResult(rr.TargetResult, true), fname);
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
			_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, expressions[index]));
			expressions[index] = JsExpression.Identifier(_variables[temp].Name);
		}

		private List<JsExpression> CompileThisAndArgumentListForMethodCall(IMember member, string literalCode, JsExpression target, bool argumentsUsedMultipleTimes, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, bool expandParams) {
			IList<InlineCodeToken> tokens = null;
			var expressions = new List<JsExpression>() { target };
			if (literalCode != null) {
				bool hasError = false;
				tokens = InlineCodeMethodCompiler.Tokenize((IMethod)member, literalCode, s => hasError = true);
				if (hasError)
					tokens = null;
			}

			if (tokens != null && target != null && !member.IsStatic && member.EntityType != EntityType.Constructor) {
				int thisUseCount = tokens.Count(t => t.Type == InlineCodeToken.TokenType.This);
				if (thisUseCount > 1 && IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(target)) {
					// Create a temporary for {this}, if required.
					var temp = _createTemporaryVariable(member.DeclaringType);
					_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, expressions[0]));
					expressions[0] = JsExpression.Identifier(_variables[temp].Name);
				}
				else if (thisUseCount == 0 && DoesJsExpressionHaveSideEffects.Analyze(target)) {
					// Ensure that 'this' is evaluated if required, even if not used by the inline code.
					_additionalStatements.Add(new JsExpressionStatement(target));
					expressions[0] = JsExpression.Null;
				}
			}

			argumentToParameterMap = argumentToParameterMap ?? CreateIdentityArgumentToParameterMap(argumentsForCall.Count);

			// Compile the arguments left to right
			foreach (var i in argumentToParameterMap) {
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
							_additionalStatements.Add(new JsExpressionStatement(result.Expression));
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

			if (expandParams && expressions[expressions.Count - 1] is JsArrayLiteralExpression) {
				var arr = (JsArrayLiteralExpression)expressions[expressions.Count - 1];
				expressions.RemoveAt(expressions.Count - 1);
				expressions.AddRange(arr.Elements);
			}

			return expressions;
		}

		private JsExpression CompileMethodInvocation(MethodScriptSemantics impl, IMethod method, ResolveResult targetResult, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, bool isVirtualCall, bool isExpandedForm) {
			if (impl != null && impl.ExpandParams && !isExpandedForm) {
				_errorReporter.Message(Messages._7514, method.DeclaringType.FullName + "." + method.Name);
			}
			bool isNonVirtualInvocationOfVirtualMethod = method.IsOverridable && !isVirtualCall;
			var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, impl.Type == MethodScriptSemantics.ImplType.InlineCode ? (isNonVirtualInvocationOfVirtualMethod ? impl.NonVirtualInvocationLiteralCode : impl.LiteralCode) : null, method.IsStatic ? GetScriptType(method.DeclaringType, TypeContext.UseStaticMember) : InnerCompile(targetResult, impl != null && !impl.IgnoreGenericArguments && method.TypeParameters.Count > 0), false, argumentsForCall, argumentToParameterMap, impl != null && impl.ExpandParams && isExpandedForm);
			return CompileMethodInvocation(impl, method, thisAndArguments, isNonVirtualInvocationOfVirtualMethod, isExpandedForm);
		}

		private JsExpression CompileMethodInvocation(MethodScriptSemantics impl, IMethod method, IList<JsExpression> thisAndArguments, bool isNonVirtualInvocationOfVirtualMethod, bool isExpandedForm) {
			var typeArguments = (method is SpecializedMethod ? ((SpecializedMethod)method).TypeArguments : EmptyList<IType>.Instance);
			var unusableTypes = Utils.FindUsedUnusableTypes(typeArguments, _metadataImporter).ToList();
			if (unusableTypes.Count > 0) {
				foreach (var ut in unusableTypes)
					_errorReporter.Message(Messages._7515, ut.FullName, method.DeclaringType.FullName + "." + method.Name);
				return JsExpression.Null;
			}

			typeArguments = (impl != null && !impl.IgnoreGenericArguments ? typeArguments : new List<IType>());

			switch (impl.Type) {
				case MethodScriptSemantics.ImplType.NormalMethod: {
					if (isNonVirtualInvocationOfVirtualMethod) {
						return _runtimeLibrary.CallBase(method.DeclaringType, impl.Name, typeArguments, thisAndArguments, ResolveTypeParameter);
					}
					else {
						var jsMethod = (JsExpression)JsExpression.Member(thisAndArguments[0], impl.Name);

						if (typeArguments.Count > 0) {
							var genMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments, ResolveTypeParameter);
							if (method.IsStatic)
								thisAndArguments[0] = JsExpression.Null;
							return JsExpression.Invocation(JsExpression.Member(genMethod, "call"), thisAndArguments);
						}
						else {
							return JsExpression.Invocation(jsMethod, thisAndArguments.Skip(1));
						}
					}
				}

				case MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument: {
					var jsMethod = (JsExpression)JsExpression.Member(GetScriptType(method.DeclaringType, TypeContext.UseStaticMember), impl.Name);
					if (typeArguments.Count > 0) {
						var genMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments, ResolveTypeParameter);
						return JsExpression.Invocation(JsExpression.Member(genMethod, "call"), new[] { JsExpression.Null }.Concat(thisAndArguments));
					}
					else {
						return JsExpression.Invocation(jsMethod, thisAndArguments);
					}
				}

				case MethodScriptSemantics.ImplType.InlineCode:
					return CompileInlineCodeMethodInvocation(method, isNonVirtualInvocationOfVirtualMethod ? impl.NonVirtualInvocationLiteralCode : impl.LiteralCode, method.IsStatic ? null : thisAndArguments[0], thisAndArguments.Skip(1).ToList(), isExpandedForm);

				case MethodScriptSemantics.ImplType.NativeIndexer:
					return JsExpression.Index(thisAndArguments[0], thisAndArguments[1]);

				default: {
					_errorReporter.Message(Messages._7516, method.DeclaringType.FullName + "." + method.Name);
					return JsExpression.Null;
				}
			}
		}

		private JsExpression CompileInlineCodeMethodInvocation(IMethod method, string code, JsExpression @this, IList<JsExpression> arguments, bool isParamArrayExpanded) {
			var tokens = InlineCodeMethodCompiler.Tokenize(method, code, s => _errorReporter.Message(Messages._7525, s));
			if (tokens == null)
				return JsExpression.Null;
			return InlineCodeMethodCompiler.CompileInlineCodeMethodInvocation(method, tokens, @this, arguments, r => r.Resolve(_compilation), (t, c) => GetScriptType(t, c), isParamArrayExpanded, s => _errorReporter.Message(Messages._7525, s));
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
				_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[_objectBeingInitialized].Name, objectBeingInitialized));
				foreach (var init in initializerStatements) {
					var js = VisitResolveResult(init, false);
					_additionalStatements.Add(new JsExpressionStatement(js));
				}
				_objectBeingInitialized = oldObjectBeingInitialized;

				return JsExpression.Identifier(_variables[obj].Name);
			}
			else {
				return objectBeingInitialized;
			}
		}

		private JsExpression CompileConstructorInvocation(ConstructorScriptSemantics impl, IMethod method, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, IList<ResolveResult> initializerStatements, bool isExpandedForm) {
			var typeToConstruct = method.DeclaringType;
			var typeToConstructDef = typeToConstruct.GetDefinition();
			if (typeToConstructDef != null && _metadataImporter.GetTypeSemantics(typeToConstructDef).Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_errorReporter.Message(Messages._7519, typeToConstruct.FullName);
				return JsExpression.Null;
			}
			if (typeToConstruct is ParameterizedType) {
				var unusableTypes = Utils.FindUsedUnusableTypes(((ParameterizedType)typeToConstruct).TypeArguments, _metadataImporter).ToList();
				if (unusableTypes.Count > 0) {
					foreach (var ut in unusableTypes)
						_errorReporter.Message(Messages._7520, ut.FullName, typeToConstructDef.FullName);
					return JsExpression.Null;
				}
			}

			if (impl.Type == ConstructorScriptSemantics.ImplType.Json) {
				return CompileJsonConstructorCall(method, impl, argumentsForCall, argumentToParameterMap, initializerStatements);
			}
			else {
				if (impl.ExpandParams && !isExpandedForm) {
					_errorReporter.Message(Messages._7502, method.DeclaringType.FullName + "." + method.DeclaringType.Name);
				}
				var thisAndArguments = CompileThisAndArgumentListForMethodCall(method, impl.Type == ConstructorScriptSemantics.ImplType.InlineCode ? impl.LiteralCode : null, GetScriptType(method.DeclaringType, TypeContext.UseStaticMember), false, argumentsForCall, argumentToParameterMap, impl.ExpandParams && isExpandedForm);

				JsExpression constructorCall;

				switch (impl.Type) {
					case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
						constructorCall = JsExpression.New(thisAndArguments[0], thisAndArguments.Skip(1));
						break;

					case ConstructorScriptSemantics.ImplType.NamedConstructor:
						constructorCall = JsExpression.New(JsExpression.Member(thisAndArguments[0], impl.Name), thisAndArguments.Skip(1));
						break;

					case ConstructorScriptSemantics.ImplType.StaticMethod:
						constructorCall = JsExpression.Invocation((JsExpression)JsExpression.Member(thisAndArguments[0], impl.Name), thisAndArguments.Skip(1));
						break;

					case ConstructorScriptSemantics.ImplType.InlineCode:
						constructorCall = CompileInlineCodeMethodInvocation(method, impl.LiteralCode, null , thisAndArguments.Skip(1).ToList(), isExpandedForm);
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

		private JsExpression HandleInvocation(IMember member, ResolveResult targetResult, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, IList<ResolveResult> initializerStatements, bool isVirtualCall, bool isExpandedForm) {
			if (member is IMethod) {
				if (member.DeclaringType.Kind == TypeKind.Delegate && member.Equals(member.DeclaringType.GetDelegateInvokeMethod())) {
					var sem = _metadataImporter.GetDelegateSemantics(member.DeclaringTypeDefinition);

					if (sem.ExpandParams && !isExpandedForm) {
						_errorReporter.Message(Messages._7534, member.DeclaringType.FullName);
					}
					var thisAndArguments = CompileThisAndArgumentListForMethodCall(member, null, InnerCompile(targetResult, false), false, argumentsForCall, argumentToParameterMap, sem.ExpandParams && isExpandedForm);

					if (sem.BindThisToFirstParameter)
						return JsExpression.Invocation(JsExpression.Member(thisAndArguments[0], "call"), thisAndArguments.Skip(1));
					else
						return JsExpression.Invocation(thisAndArguments[0], thisAndArguments.Skip(1));
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
							var createdObject = CompileMethodInvocation(_metadataImporter.GetMethodSemantics(createInstanceSpec), createInstanceSpec, new JsExpression[] { GetScriptType(activator, TypeContext.UseStaticMember) }, false, false);
							return CompileInitializerStatements(createdObject, method.DeclaringType, initializerStatements);
						}
						else {
							return CompileConstructorInvocation(_metadataImporter.GetConstructorSemantics(method), method, argumentsForCall, argumentToParameterMap, initializerStatements, isExpandedForm);
						}
					}
					else {
						return CompileMethodInvocation(_metadataImporter.GetMethodSemantics(method), method, targetResult, argumentsForCall, argumentToParameterMap, isVirtualCall, isExpandedForm);
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
				return CompileMethodInvocation(impl.GetMethod, property.Getter, targetResult, argumentsForCall, argumentToParameterMap, isVirtualCall, isExpandedForm);
			}
			else {
				_errorReporter.InternalError("Invocation of unsupported member " + member.DeclaringType.FullName + "." + member.Name);
				return JsExpression.Null;
			}
		}

		public override JsExpression VisitInvocationResolveResult(InvocationResolveResult rr, bool data) {
			return HandleInvocation(rr.Member, rr.TargetResult, rr.Arguments, null, rr.InitializerStatements, rr.IsVirtualCall, false);
		}

		public override JsExpression VisitCSharpInvocationResolveResult(CSharpInvocationResolveResult rr, bool returnValueIsImportant) {
			return HandleInvocation(rr.Member, rr.TargetResult, rr.GetArgumentsForCall(), rr.GetArgumentToParameterMap(), rr.InitializerStatements, rr.IsVirtualCall, rr.IsExpandedForm);
		}

		public override JsExpression VisitConstantResolveResult(ConstantResolveResult rr, bool returnValueIsImportant) {
			if (rr.ConstantValue == null)
				return _runtimeLibrary.Default(rr.Type, ResolveTypeParameter);
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
			IEnumerable<IVariable> expandParamsVariables = _nestedFunctionContext != null ? _nestedFunctionContext.ExpandParamsVariables : null;
			if (semantics.ExpandParams) {
				expandParamsVariables = expandParamsVariables != null ? new List<IVariable>(expandParamsVariables) : new List<IVariable>();
				((List<IVariable>)expandParamsVariables).Add(rr.Parameters[rr.Parameters.Count - 1]);
			}

			var newContext = new NestedFunctionContext(capturedByRefVariables, expandParamsVariables ?? new IVariable[0]);

			JsFunctionDefinitionExpression def;
			if (f.BodyNode is Statement) {
				StateMachineType smt = StateMachineType.NormalMethod;
				IType taskGenericArgument = null;
				if (rr.IsAsync) {
					smt = returnType.IsKnownType(KnownTypeCode.Void) ? StateMachineType.AsyncVoid : StateMachineType.AsyncTask;
					taskGenericArgument = returnType is ParameterizedType ? ((ParameterizedType)returnType).TypeArguments[0] : null;
				}

				def = _createInnerCompiler(newContext).CompileMethod(rr.Parameters, _variables, (BlockStatement)f.BodyNode, false, smt, taskGenericArgument);
			}
			else {
				var body = CloneAndCompile(rr.Body, !returnType.IsKnownType(KnownTypeCode.Void), nestedFunctionContext: newContext);
				var lastStatement = returnType.IsKnownType(KnownTypeCode.Void) ? (JsStatement)new JsExpressionStatement(body.Expression) : (JsStatement)new JsReturnStatement(body.Expression);
				var jsBody = new JsBlockStatement(MethodCompiler.FixByRefParameters(rr.Parameters, _variables).Concat(body.AdditionalStatements).Concat(new[] { lastStatement }));
				def = JsExpression.FunctionDefinition(rr.Parameters.Select(p => _variables[p].Name), jsBody);
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

			var result = captureObject != null ? _runtimeLibrary.Bind(def, captureObject) : def;
			if (semantics.BindThisToFirstParameter)
				result = _runtimeLibrary.BindFirstParameterToThis(result);
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
			if (_nestedFunctionContext != null && _nestedFunctionContext.ExpandParamsVariables.Contains(rr.Variable)) {
				_errorReporter.Message(Messages._7521, rr.Variable.Name);
			}
			else if (rr.Variable is IParameter && ((IParameter)rr.Variable).IsParams) {
				if (_methodBeingCompiled != null) {
				    var impl = _metadataImporter.GetMethodSemantics(_methodBeingCompiled);
				    if (impl.ExpandParams) {
				        _errorReporter.Message(Messages._7521, rr.Variable.Name);
				    }
				}
			}

			return CompileLocal(rr.Variable, false);
		}

		public override JsExpression VisitTypeOfResolveResult(TypeOfResolveResult rr, bool returnValueIsImportant) {
			var unusableTypes = Utils.FindUsedUnusableTypes(new[] { rr.ReferencedType }, _metadataImporter).ToList();
			if (unusableTypes.Count > 0) {
				foreach (var ut in unusableTypes)
					_errorReporter.Message(Messages._7522, ut.FullName);
				return JsExpression.Null;
			}
			else
				return GetScriptType(rr.ReferencedType, TypeContext.TypeOf);
		}

		public override JsExpression VisitTypeResolveResult(TypeResolveResult rr, bool returnValueIsImportant) {
			throw new NotSupportedException(rr + " should be handled elsewhere");
		}

		public override JsExpression VisitArrayAccessResolveResult(ArrayAccessResolveResult rr, bool returnValueIsImportant) {
			var expressions = new List<JsExpression>();
			expressions.Add(InnerCompile(rr.Array, false, expressions));
			foreach (var i in rr.Indexes)
				expressions.Add(InnerCompile(i, false, expressions));

			if (rr.Indexes.Count == 1) {
				return JsExpression.Index(expressions[0], expressions[1]);
			}
			else {
				return _runtimeLibrary.GetMultiDimensionalArrayValue(expressions[0], expressions.Skip(1));
			}
		}

		public override JsExpression VisitArrayCreateResolveResult(ArrayCreateResolveResult rr, bool returnValueIsImportant) {
			var at = (ArrayType)rr.Type;

			if (at.Dimensions == 1) {
				if (rr.InitializerElements != null && rr.InitializerElements.Count > 0) {
					var expressions = new List<JsExpression>();
					foreach (var init in rr.InitializerElements)
						expressions.Add(InnerCompile(init, false, expressions));
					return JsExpression.ArrayLiteral(expressions);
				}
				else if (rr.SizeArguments[0].IsCompileTimeConstant && Convert.ToInt64(rr.SizeArguments[0].ConstantValue) == 0) {
					return JsExpression.ArrayLiteral();
				}
				else {
					return _runtimeLibrary.CreateArray(at.ElementType, new[] { InnerCompile(rr.SizeArguments[0], false) }, ResolveTypeParameter);
				}
			}
			else {
				var sizes = new List<JsExpression>();
				foreach (var a in rr.SizeArguments)
					sizes.Add(InnerCompile(a, false, sizes));
				var result = _runtimeLibrary.CreateArray(at.ElementType, sizes, ResolveTypeParameter);

				if (rr.InitializerElements != null && rr.InitializerElements.Count > 0) {
					var temp = _createTemporaryVariable(rr.Type);
					_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, result));
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

						_additionalStatements.Add(new JsExpressionStatement(_runtimeLibrary.SetMultiDimensionalArrayValue(result, indices, expressions[i])));
					}
				}

				return result;
			}
		}

		public override JsExpression VisitTypeIsResolveResult(TypeIsResolveResult rr, bool returnValueIsImportant) {
			var targetType = UnpackNullable(rr.TargetType);
			return _runtimeLibrary.TypeIs(VisitResolveResult(rr.Input, returnValueIsImportant), rr.Input.Type, targetType, ResolveTypeParameter);
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

		private JsExpression PerformConversion(JsExpression input, Conversion c, IType fromType, IType toType) {
			if (c.IsIdentityConversion) {
				return input;
			}
			else if (c.IsTryCast) {
				return _runtimeLibrary.TryDowncast(input, fromType, UnpackNullable(toType), ResolveTypeParameter);
			}
			else if (c.IsReferenceConversion) {
				if (toType is ArrayType && fromType is ArrayType)	// Array covariance / contravariance.
					return input;
				else if (toType.Kind == TypeKind.Dynamic)
					return input;
				else if (toType.Kind == TypeKind.Delegate && fromType.Kind == TypeKind.Delegate && !toType.Equals(_compilation.FindType(KnownTypeCode.MulticastDelegate)) && !fromType.Equals(_compilation.FindType(KnownTypeCode.MulticastDelegate)))
					return input;	// Conversion between compatible delegate types.
				else if (c.IsImplicit)
					return _runtimeLibrary.Upcast(input, fromType, toType, ResolveTypeParameter);
				else
					return _runtimeLibrary.Downcast(input, fromType, toType, ResolveTypeParameter);
			}
			else if (c.IsNumericConversion) {
				var result = input;
				if (fromType.IsKnownType(KnownTypeCode.NullableOfT) && !toType.IsKnownType(KnownTypeCode.NullableOfT))
					result = _runtimeLibrary.FromNullable(result);

				if (!IsIntegerType(fromType) && IsIntegerType(toType)) {
					result = _runtimeLibrary.FloatToInt(result);

					if (fromType.IsKnownType(KnownTypeCode.NullableOfT) && toType.IsKnownType(KnownTypeCode.NullableOfT)) {
						result = _runtimeLibrary.Lift(result);
					}
				}
				return result;
			}
			else if (c.IsDynamicConversion) {
				if (toType.IsKnownType(KnownTypeCode.NullableOfT)) {
					// Unboxing to nullable type.
					return _runtimeLibrary.Downcast(input, fromType, UnpackNullable(toType), ResolveTypeParameter);
				}
				else if (toType.Kind == TypeKind.Struct) {
					// Unboxing to non-nullable type.
					return _runtimeLibrary.FromNullable(_runtimeLibrary.Downcast(input, fromType, toType, ResolveTypeParameter));
				}
				else {
					// Converting to a boring reference type.
					return _runtimeLibrary.Downcast(input, fromType, toType, ResolveTypeParameter);
				}
			}
			else if (c.IsNullableConversion || c.IsEnumerationConversion) {
				if (fromType.IsKnownType(KnownTypeCode.NullableOfT) && !toType.IsKnownType(KnownTypeCode.NullableOfT))
					return _runtimeLibrary.FromNullable(input);
				return input;
			}
			else if (c.IsBoxingConversion) {
				if (toType.Kind != TypeKind.Dynamic) {
					if (fromType.GetAllBaseTypes().Contains(toType))	// Conversion between type parameters are classified as boxing conversions, so it's sometimes an upcast, sometimes a downcast.
						return _runtimeLibrary.Upcast(input, fromType, toType, ResolveTypeParameter);
					else
						return _runtimeLibrary.Downcast(input, fromType, toType, ResolveTypeParameter);
						
				}
				return input;
			}
			else if (c.IsUnboxingConversion) {
				if (toType.IsKnownType(KnownTypeCode.NullableOfT)) {
					return _runtimeLibrary.Downcast(input, fromType, UnpackNullable(toType), ResolveTypeParameter);
				}
				else {
					var result = _runtimeLibrary.Downcast(input, fromType, toType, ResolveTypeParameter);
					if (toType.Kind == TypeKind.Struct)
						result = _runtimeLibrary.FromNullable(result);	// hidden gem in the C# spec: conversions involving type parameter which are not known to not be unboxing are considered unboxing conversions.
					return result;
				}
			}
			else if (c.IsUserDefined) {
				var conversions = new CSharpConversions(_compilation);
				var preConv = conversions.ExplicitConversion(fromType, c.Method.Parameters[0].Type);
				if (!preConv.IsIdentityConversion)
					input = PerformConversion(input, preConv, fromType, c.Method.Parameters[0].Type);

				var impl = _metadataImporter.GetMethodSemantics(c.Method);
				var result = CompileMethodInvocation(impl, c.Method, new[] { GetScriptType(c.Method.DeclaringType, TypeContext.UseStaticMember), input }, false, false);

				var postConv = conversions.ExplicitConversion(c.Method.ReturnType, toType);
				if (!postConv.IsIdentityConversion)
					result = PerformConversion(result, postConv, c.Method.ReturnType, toType);
				return result;
			}
			else if (c.IsNullLiteralConversion || c.IsConstantExpressionConversion) {
				return input;
			}

			_errorReporter.InternalError("Conversion " + c + " is not implemented");
			return JsExpression.Null;
		}

		public override JsExpression VisitConversionResolveResult(ConversionResolveResult rr, bool returnValueIsImportant) {
			if (rr.Conversion.IsAnonymousFunctionConversion) {
				var retType = rr.Type.GetDelegateInvokeMethod().ReturnType;
				return CompileLambda((LambdaResolveResult)rr.Input, retType, _metadataImporter.GetDelegateSemantics(rr.Type.GetDefinition()));
			}
			else if (rr.Conversion.IsMethodGroupConversion) {
				var mgrr = (MethodGroupResolveResult)rr.Input;

				var delegateSemantics = _metadataImporter.GetDelegateSemantics(rr.Type.GetDefinition());

				if (mgrr.TargetResult.Type.Kind == TypeKind.Delegate && Equals(rr.Conversion.Method, mgrr.TargetResult.Type.GetDelegateInvokeMethod())) {
					var sem1 = _metadataImporter.GetDelegateSemantics(mgrr.TargetResult.Type.GetDefinition());
					var sem2 = _metadataImporter.GetDelegateSemantics(rr.Type.GetDefinition());
					if (sem1.BindThisToFirstParameter != sem2.BindThisToFirstParameter) {
						_errorReporter.Message(Messages._7533, mgrr.TargetType.FullName, rr.Type.FullName);
						return JsExpression.Null;
					}

					return _runtimeLibrary.CloneDelegate(InnerCompile(mgrr.TargetResult, false), rr.Conversion.Method.DeclaringType, rr.Type, ResolveTypeParameter);	// new D2(d1)
				}

				var methodSemantics = _metadataImporter.GetMethodSemantics(rr.Conversion.Method);
				if (methodSemantics.Type != MethodScriptSemantics.ImplType.NormalMethod) {
					_errorReporter.Message(Messages._7523, rr.Conversion.Method.DeclaringType + "." + rr.Conversion.Method.Name);
					return JsExpression.Null;
				}
				else if (methodSemantics.ExpandParams  != delegateSemantics.ExpandParams) {
					_errorReporter.Message(Messages._7524, rr.Conversion.Method.DeclaringType + "." + rr.Conversion.Method.Name, rr.Type.FullName);
					return JsExpression.Null;
				}

				var typeArguments = (rr.Conversion.Method is SpecializedMethod && !methodSemantics.IgnoreGenericArguments) ? ((SpecializedMethod)rr.Conversion.Method).TypeArguments : new List<IType>();

				JsExpression result;

				if (rr.Conversion.Method.IsOverridable && !rr.Conversion.IsVirtualMethodLookup) {
					// base.Method
					var jsTarget = InnerCompile(mgrr.TargetResult, true);
					result = _runtimeLibrary.BindBaseCall(rr.Conversion.Method.DeclaringType, methodSemantics.Name, typeArguments, jsTarget, ResolveTypeParameter);
				}
				else {
					JsExpression jsTarget, jsMethod;

					if (rr.Conversion.Method.IsStatic) {
						jsTarget = null;
						jsMethod = JsExpression.Member(GetScriptType(mgrr.TargetResult.Type, TypeContext.UseStaticMember), methodSemantics.Name);
					}
					else {
						jsTarget = InnerCompile(mgrr.TargetResult, true);
						jsMethod = JsExpression.Member(jsTarget, methodSemantics.Name);
					}

					if (typeArguments.Count > 0) {
						jsMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments, ResolveTypeParameter);
					}

					result = jsTarget != null ? _runtimeLibrary.Bind(jsMethod, jsTarget) : jsMethod;
				}

				if (delegateSemantics.BindThisToFirstParameter)
					result = _runtimeLibrary.BindFirstParameterToThis(result);

				return result;
			}
			else {
				return PerformConversion(VisitResolveResult(rr.Input, true), rr.Conversion, rr.Input.Type, rr.Type);
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

				return CompileConstructorInvocation(semantics[0], methods[0], rr.Arguments, null, rr.InitializerStatements, false);
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
					var target = mgrr.TargetResult is TypeResolveResult ? _runtimeLibrary.GetScriptType(mgrr.TargetResult.Type, TypeContext.UseStaticMember, ResolveTypeParameter) : InnerCompile(mgrr.TargetResult, false);
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
						return (JsExpression)JsExpression.Index(expressions[0], expressions[1]);

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
				_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, operand));
				operand = JsExpression.Identifier(_variables[temp].Name);
			}
			else {
				operand = InnerCompile(rr.GetAwaiterInvocation, true);
			}

			if (rr.GetAwaiterInvocation.Type.Kind == TypeKind.Dynamic) {
				_additionalStatements.Add(new JsAwaitStatement(operand, "onCompleted"));
				return JsExpression.Invocation(JsExpression.Member(operand, "getResult"));
			}
			else {
				var getResultMethodImpl   = _metadataImporter.GetMethodSemantics(rr.GetResultMethod);
				var onCompletedMethodImpl = _metadataImporter.GetMethodSemantics(rr.OnCompletedMethod);
	
				if (onCompletedMethodImpl.Type != MethodScriptSemantics.ImplType.NormalMethod) {
					_errorReporter.Message(Messages._7535);
					return JsExpression.Null;
				}
	
				_additionalStatements.Add(new JsAwaitStatement(operand, onCompletedMethodImpl.Name));
				return CompileMethodInvocation(getResultMethodImpl, rr.GetResultMethod, new[] { operand }, false, false);
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
