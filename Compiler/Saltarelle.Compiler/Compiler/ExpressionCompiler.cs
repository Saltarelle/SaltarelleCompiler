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

		public NestedFunctionContext(IEnumerable<IVariable> capturedByRefVariables) {
			var s = new HashSet<IVariable>();
			foreach (var v in capturedByRefVariables)
				s.Add(v);
			CapturedByRefVariables = new ReadOnlySet<IVariable>(s);
		}
	}

	public class ExpressionCompiler : ResolveResultVisitor<JsExpression, bool> {
		internal class IsJsExpressionComplexEnoughToGetATemporaryVariable : RewriterVisitorBase<object> {
			private bool _result;

			public static bool Process(JsExpression expression) {
				var v = new IsJsExpressionComplexEnoughToGetATemporaryVariable();
				expression.Accept(v, null);
				return v._result;
			}

			public override JsExpression VisitArrayLiteralExpression(JsArrayLiteralExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression VisitBinaryExpression(JsBinaryExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression VisitCommaExpression(JsCommaExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression VisitInvocationExpression(JsInvocationExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression VisitNewExpression(JsNewExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression VisitObjectLiteralExpression(JsObjectLiteralExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression VisitUnaryExpression(JsUnaryExpression expression, object data) {
				_result = true;
				return expression;
			}
		}

		internal class DoesJsExpressionHaveSideEffects : RewriterVisitorBase<object> {
			private bool _result;

			public static bool Process(JsExpression expression) {
				var v = new DoesJsExpressionHaveSideEffects();
				expression.Accept(v, null);
				return v._result;
			}

			public override JsExpression VisitInvocationExpression(JsInvocationExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression VisitNewExpression(JsNewExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression VisitBinaryExpression(JsBinaryExpression expression, object data) {
				if (expression.NodeType >= ExpressionNodeType.AssignFirst && expression.NodeType <= ExpressionNodeType.AssignLast) {
					_result = true;
					return expression;
				}
				else {
					return base.VisitBinaryExpression(expression, data);
				}
			}

			public override JsExpression VisitUnaryExpression(JsUnaryExpression expression, object data) {
				switch (expression.NodeType) {
					case ExpressionNodeType.PrefixPlusPlus:
					case ExpressionNodeType.PrefixMinusMinus:
					case ExpressionNodeType.PostfixPlusPlus:
					case ExpressionNodeType.PostfixMinusMinus:
					case ExpressionNodeType.Delete:
						_result = true;
						return expression;
					default:
						return base.VisitUnaryExpression(expression, data);
				}
			}
		}

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
		private NestedFunctionContext _nestedFunctionContext;
		private IVariable _objectBeingInitialized;
		private IMethod _methodBeingCompiled;
		private string _filename;
		private TextLocation _location;

		public class Result {
			public JsExpression Expression { get; set; }
			public ReadOnlyCollection<JsStatement> AdditionalStatements { get; private set; }

			public Result(JsExpression expression, IEnumerable<JsStatement> additionalStatements) {
				this.Expression           = expression;
				this.AdditionalStatements = additionalStatements.AsReadOnly();
			}
		}

		public ExpressionCompiler(ICompilation compilation, IMetadataImporter metadataImporter, INamer namer, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, IDictionary<IVariable, VariableData> variables, IDictionary<LambdaResolveResult, NestedFunctionData> nestedFunctions, Func<IType, IVariable> createTemporaryVariable, Func<NestedFunctionContext, StatementCompiler> createInnerCompiler, string thisAlias, NestedFunctionContext nestedFunctionContext, IVariable objectBeingInitialized, IMethod methodBeingCompiled) {
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
		}

		private List<JsStatement> _additionalStatements;

		public Result Compile(string filename, TextLocation location, ResolveResult expression, bool returnValueIsImportant) {
			_filename = filename;
			_location = location;
			_additionalStatements = new List<JsStatement>();
			var expr = VisitResolveResult(expression, returnValueIsImportant);
			return new Result(expr, _additionalStatements);
		}

		public IList<JsStatement> CompileConstructorInitializer(string filename, TextLocation location, IMethod method, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, IList<ResolveResult> initializerStatements, bool currentIsStaticMethod, bool isExpandedForm) {
			_filename = filename;
			_location = location;
			_additionalStatements = new List<JsStatement>();
			var impl = _metadataImporter.GetConstructorSemantics(method);

			if (currentIsStaticMethod) {
				_additionalStatements.Add(new JsVariableDeclarationStatement(_thisAlias, CompileConstructorInvocation(impl, method, argumentsForCall, argumentToParameterMap, initializerStatements, isExpandedForm)));
			}
			else {
				if (impl.ExpandParams && !isExpandedForm) {
					_errorReporter.Message(7502, _filename, _location, method.DeclaringType.FullName + "." + method.DeclaringType.Name);
				}

				var thisAndArguments = CompileThisAndArgumentListForMethodCall(new TypeResolveResult(method.DeclaringType), false, false, argumentsForCall, argumentToParameterMap, impl.ExpandParams && isExpandedForm);
				var jsType           = thisAndArguments[0];
				thisAndArguments[0]  = CompileThis();	// Swap out the TypeResolveResult that we get as default.

				switch (impl.Type) {
					case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
						_additionalStatements.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.MemberAccess(jsType, "call"), thisAndArguments)));
						break;

					case ConstructorScriptSemantics.ImplType.NamedConstructor:
						_additionalStatements.Add(new JsExpressionStatement(JsExpression.Invocation(JsExpression.MemberAccess(JsExpression.MemberAccess(jsType, impl.Name), "call"), thisAndArguments)));
						break;

					case ConstructorScriptSemantics.ImplType.StaticMethod:
						_errorReporter.Message(7503, _filename, _location);
						break;

					case ConstructorScriptSemantics.ImplType.InlineCode:
						_errorReporter.Message(7504, _filename, _location);
						break;

					default:
						_errorReporter.Message(7505, _filename, _location);
						break;
				}
			}

			var result = _additionalStatements;
			_additionalStatements = null;	// Just so noone else messes with it by accident (shouldn't happen).
			return result;
		}

		private Result CloneAndCompile(ResolveResult expression, bool returnValueIsImportant, NestedFunctionContext nestedFunctionContext = null) {
			return new ExpressionCompiler(_compilation, _metadataImporter, _namer, _runtimeLibrary, _errorReporter, _variables, _nestedFunctions, _createTemporaryVariable, _createInnerCompiler, _thisAlias, nestedFunctionContext ?? _nestedFunctionContext, _objectBeingInitialized, _methodBeingCompiled).Compile(_filename, _location, expression, returnValueIsImportant);
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

		private JsExpression InnerCompile(ResolveResult rr, bool usedMultipleTimes, IList<JsExpression> expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression) {
			var result = CloneAndCompile(rr, true);

			bool needsTemporary = usedMultipleTimes && IsJsExpressionComplexEnoughToGetATemporaryVariable.Process(result.Expression);
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
			if (IsNullableType(type))
				type = GetNonNullableType(type);

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
			if (IsNullableType(type))
				type = GetNonNullableType(type);

			return type.Equals(_compilation.FindType(KnownTypeCode.Byte))
			    || type.Equals(_compilation.FindType(KnownTypeCode.UInt16))
				|| type.Equals(_compilation.FindType(KnownTypeCode.UInt32))
				|| type.Equals(_compilation.FindType(KnownTypeCode.UInt64));
		}

		private bool IsNullableType(IType type) {
			return Equals(type.GetDefinition(), _compilation.FindType(KnownTypeCode.NullableOfT));
		}

		private IType GetNonNullableType(IType nullableType) {
			return ((ParameterizedType)nullableType).TypeArguments[0];
		}

		private bool IsNullableBooleanType(IType type) {
			return Equals(type.GetDefinition(), _compilation.FindType(KnownTypeCode.NullableOfT))
			    && Equals(GetNonNullableType(type), _compilation.FindType(KnownTypeCode.Boolean));
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
			var jsTarget = InnerCompile(target.TargetResult, compoundFactory == null);
			var jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, ref jsTarget) : null);
			var access = JsExpression.MemberAccess(jsTarget, fieldName);
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

			if (target is LocalResolveResult || target.Type.Kind == TypeKind.Dynamic) {
				var jsTarget = InnerCompile(target, compoundFactory == null);
				var jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, ref jsTarget) : null);
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
									_errorReporter.Message(7506, _filename, _location);
									return JsExpression.Number(0);
								}
								return CompileArrayAccessCompoundAssignment(mrr.TargetResult, ((CSharpInvocationResolveResult)mrr).Arguments[0], otherOperand, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
							}
							else {
								List<JsExpression> thisAndArguments;
								if (property.IsIndexer) {
									var invocation = (CSharpInvocationResolveResult)target;
									thisAndArguments = CompileThisAndArgumentListForMethodCall(invocation.TargetResult, oldValueIsImportant, oldValueIsImportant, invocation.GetArgumentsForCall(), invocation.GetArgumentToParameterMap(), false);
								}
								else {
									thisAndArguments = new List<JsExpression> { InnerCompile(mrr.TargetResult, oldValueIsImportant) };
								}

								JsExpression oldValue, jsOtherOperand;
								if (oldValueIsImportant) {
									thisAndArguments.Add(CompileMethodInvocation(impl.GetMethod, property.Getter, thisAndArguments, new IType[0], mrr.Member.IsOverridable && !mrr.IsVirtualCall, false));
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
									if (IsJsExpressionComplexEnoughToGetATemporaryVariable.Process(valueToReturn)) {
										// Must be a simple assignment, if we got the value from a getter we would already have created a temporary for it.
										CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(thisAndArguments, valueToReturn);
										var temp = _createTemporaryVariable(target.Type);
										_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, valueToReturn));
										valueToReturn = JsExpression.Identifier(_variables[temp].Name);
									}

									var newValue = (returnValueBeforeChange ? valueFactory(valueToReturn, jsOtherOperand) : valueToReturn);

									thisAndArguments.Add(newValue);
									_additionalStatements.Add(new JsExpressionStatement(CompileMethodInvocation(impl.SetMethod, property.Setter, thisAndArguments, new IType[0], mrr.Member.IsOverridable && !mrr.IsVirtualCall, false)));
									return valueToReturn;
								}
								else {
									thisAndArguments.Add(valueFactory(oldValue, jsOtherOperand));
									return CompileMethodInvocation(impl.SetMethod, property.Setter, thisAndArguments, new IType[0], mrr.Member.IsOverridable && !mrr.IsVirtualCall, false);
								}
							}
						}

						case PropertyScriptSemantics.ImplType.Field: {
							return CompileCompoundFieldAssignment(mrr, otherOperand, impl.FieldName, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
						}

						default: {
							_errorReporter.Message(7507, _filename, _location, property.DeclaringType.FullName + "." + property.Name);
							return JsExpression.Number(0);
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
							_errorReporter.Message(7508, _filename, _location, field.DeclaringType.FullName + "." + field.Name);
							return JsExpression.Number(0);
						default:
							_errorReporter.Message(7509, _filename, _location, field.DeclaringType.FullName + "." + field.Name);
							return JsExpression.Number(0);
					}
				}
				else {
					_errorReporter.InternalError("Target " + mrr.Member.DeclaringType.FullName + "." + mrr.Member.Name + " of compound assignment is neither a property nor a field.", _filename, _location);
					return JsExpression.Number(0);
				}
			}
			else if (target is ArrayAccessResolveResult) {
				var arr = (ArrayAccessResolveResult)target;
				if (arr.Indexes.Count != 1) {
					_errorReporter.Message(7510, _filename, _location);
					return JsExpression.Number(0);
				}
				return CompileArrayAccessCompoundAssignment(arr.Array, arr.Indexes[0], otherOperand, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
			}
			else {
				_errorReporter.InternalError("Unsupported target of assignment: " + target, _filename, _location);
				return JsExpression.Number(0);
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
				var trueBlock  = new JsBlockStatement(trueResult.AdditionalStatements.Concat(new[] { new JsVariableDeclarationStatement(_variables[temp].Name, trueResult.Expression) }));
				var falseBlock = new JsBlockStatement(falseResult.AdditionalStatements.Concat(new[] { new JsVariableDeclarationStatement(_variables[temp].Name, falseResult.Expression) }));
				_additionalStatements.Add(new JsIfStatement(jsTest, trueBlock, falseBlock));
				return JsExpression.Identifier(_variables[temp].Name);
			}
			else {
				return JsExpression.Conditional(jsTest, trueResult.Expression, falseResult.Expression);
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
					_errorReporter.Message(7511, _filename, _location, evt.DeclaringType.FullName + "." + evt.Name);
					return JsExpression.Number(0);
			}
		}

		public override JsExpression VisitResolveResult(ResolveResult rr, bool data) {
			if (rr.IsError) {
				_errorReporter.InternalError("ResolveResult " + rr.ToString() + " is an error.", _filename, _location);
				return JsExpression.Number(0);
			}
			else
				return base.VisitResolveResult(rr, data);
		}

		public override JsExpression VisitOperatorResolveResult(OperatorResolveResult rr, bool returnValueIsImportant) {
			if (rr.UserDefinedOperatorMethod != null) {
				var impl = _metadataImporter.GetMethodSemantics(rr.UserDefinedOperatorMethod);
				if (impl.Type != MethodScriptSemantics.ImplType.NativeOperator) {
					switch (rr.Operands.Count) {
						case 1: {
							Func<JsExpression, JsExpression, JsExpression> invocation = (a, b) => CompileMethodInvocation(impl, rr.UserDefinedOperatorMethod, new[] { _runtimeLibrary.GetScriptType(rr.UserDefinedOperatorMethod.DeclaringType, TypeContext.Instantiation), a }, new IType[0], false, false);
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
									return CompileUnaryOperator(rr.Operands[0], a => CompileMethodInvocation(impl, rr.UserDefinedOperatorMethod, new[] { _runtimeLibrary.GetScriptType(rr.UserDefinedOperatorMethod.DeclaringType, TypeContext.Instantiation), a }, new IType[0], false, false), rr.IsLiftedOperator);
							}
						}

						case 2: {
							Func<JsExpression, JsExpression, JsExpression> invocation = (a, b) => CompileMethodInvocation(impl, rr.UserDefinedOperatorMethod, new[] { _runtimeLibrary.GetScriptType(rr.UserDefinedOperatorMethod.DeclaringType, TypeContext.Instantiation), a, b }, new IType[0], false, false);
							if (IsAssignmentOperator(rr.OperatorType))
								return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, invocation, returnValueIsImportant, rr.IsLiftedOperator);
							else
								return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], invocation, rr.IsLiftedOperator);
						}
					}
					_errorReporter.InternalError("Could not compile call to user-defined operator " + rr.UserDefinedOperatorMethod.DeclaringType.FullName + "." + rr.UserDefinedOperatorMethod.Name, _filename, _location);
					return JsExpression.Number(0);
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
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => CompileMethodInvocation(impl, combine, new[] { _runtimeLibrary.GetScriptType(del, TypeContext.Instantiation), a, b }, new IType[0], false, false), returnValueIsImportant, false);
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
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseXOrAssign, JsExpression.BitwiseXor, returnValueIsImportant, rr.IsLiftedOperator);

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
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => CompileMethodInvocation(impl, remove, new[] { _runtimeLibrary.GetScriptType(del, TypeContext.Instantiation), a, b }, new IType[0], false, false), returnValueIsImportant, false);
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
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => CompileMethodInvocation(impl, combine, new[] { _runtimeLibrary.GetScriptType(del, TypeContext.Instantiation), a, b }, new IType[0], false, false), false);
					}
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Add, rr.IsLiftedOperator);

				case ExpressionType.And:
					if (IsNullableBooleanType(rr.Operands[0].Type))
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => _runtimeLibrary.LiftedBooleanAnd(a, b), false);	// We have already lifted it, so it should not be lifted again.
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseAnd, rr.IsLiftedOperator);

				case ExpressionType.AndAlso:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.LogicalAnd, false);	// Operator does not have a lifted version.

				case ExpressionType.Coalesce:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => _runtimeLibrary.Coalesce(a, b), false);

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
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.LogicalOr, false);	// Operator does not have a lifted version.

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
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => CompileMethodInvocation(impl, remove, new[] { _runtimeLibrary.GetScriptType(del, TypeContext.Instantiation), a, b }, new IType[0], false, false), false);
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
					_errorReporter.InternalError("Unsupported operator " + rr.OperatorType, _filename, _location);
					return JsExpression.Number(0);
			}
		}

		public JsExpression CompileDelegateCombineCall(string filename, TextLocation location, JsExpression a, JsExpression b) {
			var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
			var combine = del.GetMethods().Single(m => m.Name == "Combine" && m.Parameters.Count == 2);
			var impl = _metadataImporter.GetMethodSemantics(combine);
			var thisAndArguments = (combine.IsStatic ? new[] { _runtimeLibrary.GetScriptType(del, TypeContext.Instantiation), a, b } : new[] { a, b });
			return CompileMethodInvocation(impl, combine, thisAndArguments, new IType[0], false, false);
		}

		public JsExpression CompileDelegateRemoveCall(string filename, TextLocation location, JsExpression a, JsExpression b) {
			var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
			var remove = del.GetMethods().Single(m => m.Name == "Remove" && m.Parameters.Count == 2);
			var impl = _metadataImporter.GetMethodSemantics(remove);
			var thisAndArguments = (remove.IsStatic ? new[] { _runtimeLibrary.GetScriptType(del, TypeContext.Instantiation), a, b } : new[] { a, b });
			return CompileMethodInvocation(impl, remove, thisAndArguments, new IType[0], false, false);
		}

		public override JsExpression VisitMethodGroupResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.MethodGroupResolveResult rr, bool returnValueIsImportant) {
			_errorReporter.InternalError("MethodGroupResolveResult should always be the target of a method group conversion, and is handled there", _filename, _location);
			return JsExpression.Number(0);
		}

		public override JsExpression VisitLambdaResolveResult(LambdaResolveResult rr, bool returnValueIsImportant) {
			_errorReporter.InternalError("LambdaResolveResult should always be the target of an anonymous method conversion, and is handled there", _filename, _location);
			return JsExpression.Number(0);
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
						var jsTarget = InnerCompile(rr.TargetResult, false);
						return JsExpression.MemberAccess(jsTarget, impl.FieldName);
					}
					default: {
						_errorReporter.Message(7512, _filename, _location, rr.Member.DeclaringType.FullName + "." + rr.Member.Name);
						return JsExpression.Number(0);
					}
				}
			}
			else if (rr.Member is IField) {
				var impl = _metadataImporter.GetFieldSemantics((IField)rr.Member);
				switch (impl.Type) {
					case FieldScriptSemantics.ImplType.Field:
						return JsExpression.MemberAccess(InnerCompile(rr.TargetResult, false), impl.Name);
					case FieldScriptSemantics.ImplType.Constant:
						return JSModel.Utils.MakeConstantExpression(impl.Value);
					default:
						_errorReporter.Message(7509, _filename, _location, rr.Member.DeclaringType.Name + "." + rr.Member.Name);
						return JsExpression.Number(0);
				}
			}
			else if (rr.Member is IEvent) {
				var eimpl = _metadataImporter.GetEventSemantics((IEvent)rr.Member);
                if (eimpl.Type == EventScriptSemantics.ImplType.NotUsableFromScript) {
					_errorReporter.Message(7511, _filename, _location, rr.Member.DeclaringType.Name + "." + rr.Member.Name);
					return JsExpression.Number(0);
                }

				var fname = _metadataImporter.GetAutoEventBackingFieldName((IEvent)rr.Member);
				return JsExpression.MemberAccess(VisitResolveResult(rr.TargetResult, true), fname);
			}
			else {
				_errorReporter.InternalError("Invalid member " + rr.Member.ToString(), _filename, _location);
				return JsExpression.Number(0);
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

		private List<JsExpression> CompileThisAndArgumentListForMethodCall(ResolveResult target, bool targetUsedMultipleTimes, bool argumentsUsedMultipleTimes, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, bool expandParams) {
			argumentToParameterMap = argumentToParameterMap ?? CreateIdentityArgumentToParameterMap(argumentsForCall.Count);

			var expressions = new List<JsExpression>();
			expressions.Add(InnerCompile(target, targetUsedMultipleTimes));
			foreach (var i in argumentToParameterMap) {
				var a = argumentsForCall[i];
				if (a is ByReferenceResolveResult) {
					var r = (ByReferenceResolveResult)a;
					if (r.ElementResult is LocalResolveResult) {
						expressions.Add(CompileLocal(((LocalResolveResult)r.ElementResult).Variable, true));
					}
					else {
						_errorReporter.Message(7513, _filename, _location);
						expressions.Add(JsExpression.Number(0));
					}
				}
				else
					expressions.Add(InnerCompile(a, argumentsUsedMultipleTimes, expressions));
			}

			if ((argumentToParameterMap.Count != argumentsForCall.Count || argumentToParameterMap.Select((i, n) => new { i, n }).Any(t => t.i != t.n))) {	// If we have an argument to parameter map and it actually performs any reordering.
				// We have to perform argument rearrangement. The easiest way would be for us to just use the argumentsForCall directly, but if we did, we wouldn't evaluate all expressions in left-to-right order.
				var newExpressions = new List<JsExpression>() { expressions[0] };
				for (int i = 0; i < argumentsForCall.Count; i++) {
					int specifiedIndex = -1;
					for (int j = 0; j < argumentToParameterMap.Count; j++) {
						if (argumentToParameterMap[j] == i) {
							specifiedIndex = j;
							break;
						}
					}
					if (specifiedIndex == -1) {
						// The argument was not specified - use the value in the argumentsForCall, which has to be constant.
						newExpressions.Add(VisitResolveResult(argumentsForCall[i], true));
					}
					else {
						// Ensure that all arguments are evaluated in the correct order (doesn't have to be done for ref and out arguments.
						for (int j = 0; j < specifiedIndex; j++) {
							if (argumentToParameterMap[j] > i && ExpressionOrderer.DoesOrderMatter(expressions[specifiedIndex + 1], expressions[j + 1])) {	// This expression used to be evaluated before us, but will now be evaluated after us, so we need to create a temporary.
								var temp = _createTemporaryVariable(argumentsForCall[argumentToParameterMap[j]].Type);
								_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp].Name, expressions[j + 1]));
								expressions[j + 1] = JsExpression.Identifier(_variables[temp].Name);
							}
						}
						newExpressions.Add(expressions[specifiedIndex + 1]);
					}
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
			var typeArguments = method is SpecializedMethod ? ((SpecializedMethod)method).TypeArguments : new IType[0];
			if (impl != null && impl.ExpandParams && !isExpandedForm) {
				_errorReporter.Message(7514, _filename, _location, method.DeclaringType.FullName + "." + method.Name);
			}
			var thisAndArguments = CompileThisAndArgumentListForMethodCall(method.IsStatic ? new TypeResolveResult(method.DeclaringType) : targetResult, impl != null && !impl.IgnoreGenericArguments && typeArguments.Count > 0 && !method.IsStatic, false, argumentsForCall, argumentToParameterMap, impl != null && impl.ExpandParams && isExpandedForm);
			return CompileMethodInvocation(impl, method, thisAndArguments, typeArguments, method.IsOverridable && !isVirtualCall, isExpandedForm);
		}

		private JsExpression CompileMethodInvocation(MethodScriptSemantics impl, IMethod method, IList<JsExpression> thisAndArguments, IList<IType> typeArguments, bool isNonVirtualInvocationOfVirtualMethod, bool isExpandedForm) {
			var unusableTypes = Utils.FindUsedUnusableTypes(typeArguments, _metadataImporter).ToList();
			if (unusableTypes.Count > 0) {
				foreach (var ut in unusableTypes)
					_errorReporter.Message(7515, _filename, _location, ut.FullName, method.DeclaringType.FullName + "." + method.Name);
				return JsExpression.Number(0);
			}

			typeArguments = (impl != null && !impl.IgnoreGenericArguments ? typeArguments : new List<IType>());

			if (impl == null) {
				return JsExpression.Invocation(thisAndArguments[0], thisAndArguments.Skip(1));	// Used for delegate invocations.
			}
			else {
				switch (impl.Type) {
					case MethodScriptSemantics.ImplType.NormalMethod: {
						if (isNonVirtualInvocationOfVirtualMethod) {
							return _runtimeLibrary.CallBase(method.DeclaringType, impl.Name, typeArguments, thisAndArguments);
						}
						else {
							var jsMethod = method.IsStatic && impl.IsGlobal ? (JsExpression)JsExpression.Identifier(impl.Name) : (JsExpression)JsExpression.MemberAccess(thisAndArguments[0], impl.Name);

							if (typeArguments.Count > 0) {
								var genMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments);
								if (method.IsStatic)
									thisAndArguments[0] = JsExpression.Null;
								return JsExpression.Invocation(JsExpression.MemberAccess(genMethod, "call"), thisAndArguments);
							}
							else {
								return JsExpression.Invocation(jsMethod, thisAndArguments.Skip(1));
							}
						}
					}

					case MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument: {
						var jsMethod = impl.IsGlobal ? (JsExpression)JsExpression.Identifier(impl.Name) : (JsExpression)JsExpression.MemberAccess(_runtimeLibrary.GetScriptType(method.DeclaringType, TypeContext.Instantiation), impl.Name);
						if (typeArguments.Count > 0) {
							var genMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments);
							return JsExpression.Invocation(JsExpression.MemberAccess(genMethod, "call"), new[] { JsExpression.Null }.Concat(thisAndArguments));
						}
						else {
							return JsExpression.Invocation(jsMethod, thisAndArguments);
						}
					}

					case MethodScriptSemantics.ImplType.InstanceMethodOnFirstArgument:
						return JsExpression.Invocation(JsExpression.MemberAccess(thisAndArguments[1], impl.Name), thisAndArguments.Skip(2));

					case MethodScriptSemantics.ImplType.InlineCode:
						return InlineCodeMethodCompiler.CompileInlineCodeMethodInvocation(method, impl.LiteralCode, method.IsStatic ? null : thisAndArguments[0], thisAndArguments.Skip(1).ToList(), (t, c) => _runtimeLibrary.GetScriptType(t.Resolve(_compilation), c), isExpandedForm, s => _errorReporter.Message(7525, _filename, _location, s));

					case MethodScriptSemantics.ImplType.NativeIndexer:
						return JsExpression.Index(thisAndArguments[0], thisAndArguments[1]);

					default: {
						_errorReporter.Message(7516, _filename, _location, method.DeclaringType.FullName + "." + method.Name);
						return JsExpression.Number(0);
					}
				}
			}
		}

		private string GetMemberNameForJsonConstructor(IMember member) {
			if (member is IProperty) {
				var currentImpl = _metadataImporter.GetPropertySemantics((IProperty)member);
				if (currentImpl.Type == PropertyScriptSemantics.ImplType.Field) {
					return currentImpl.FieldName;
				}
				else {
					_errorReporter.Message(7517, _filename, _location, member.DeclaringType.FullName + "." + member.Name);
					return null;
				}
			}
			else if (member is IField) {
				var currentImpl = _metadataImporter.GetFieldSemantics((IField)member);
				if (currentImpl.Type == FieldScriptSemantics.ImplType.Field) {
					return currentImpl.Name;
				}
				else {
					_errorReporter.Message(7518, _filename, _location, member.DeclaringType.FullName + "." + member.Name);
					return null;
				}
			}
			else {
				_errorReporter.InternalError("Unsupported member " + member + " in anonymous object initializer.", _filename, _location);
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
							_errorReporter.Message(7527, _filename, _location, member.Name);
						}
						else {
							jsPropertyNames.Add(name);
							expressions.Add(InnerCompile(orr.Operands[1], false, expressions));
						}
					}
				}
				else {
					_errorReporter.InternalError("Expected an assignment to an InitializedObjectResolveResult, got " + orr, _filename, _location);
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

		private JsExpression CompileConstructorInvocation(ConstructorScriptSemantics impl, IMethod method, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, IList<ResolveResult> initializerStatements, bool isExpandedForm) {
			var typeToConstruct = method.DeclaringType;
			var typeToConstructDef = typeToConstruct.GetDefinition();
			if (typeToConstructDef != null && _metadataImporter.GetTypeSemantics(typeToConstructDef).Type == TypeScriptSemantics.ImplType.NotUsableFromScript) {
				_errorReporter.Message(7519, _filename, _location, typeToConstruct.FullName);
				return JsExpression.Number(0);
			}
			if (typeToConstruct is ParameterizedType) {
				var unusableTypes = Utils.FindUsedUnusableTypes(((ParameterizedType)typeToConstruct).TypeArguments, _metadataImporter).ToList();
				if (unusableTypes.Count > 0) {
					foreach (var ut in unusableTypes)
						_errorReporter.Message(7520, _filename, _location, ut.FullName, typeToConstructDef.FullName);
					return JsExpression.Number(0);
				}
			}

			if (impl.Type == ConstructorScriptSemantics.ImplType.Json) {
				return CompileJsonConstructorCall(method, impl, argumentsForCall, argumentToParameterMap, initializerStatements);
			}
			else {
				if (impl.ExpandParams && !isExpandedForm) {
					_errorReporter.Message(7502, _filename, _location, method.DeclaringType.FullName + "." + method.DeclaringType.Name);
				}
				var thisAndArguments = CompileThisAndArgumentListForMethodCall(new TypeResolveResult(method.DeclaringType), false, false, argumentsForCall, argumentToParameterMap, impl.ExpandParams && isExpandedForm);

				JsExpression constructorCall;

				switch (impl.Type) {
					case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
						constructorCall = JsExpression.New(thisAndArguments[0], thisAndArguments.Skip(1));
						break;

					case ConstructorScriptSemantics.ImplType.NamedConstructor:
						constructorCall = JsExpression.New(JsExpression.MemberAccess(thisAndArguments[0], impl.Name), thisAndArguments.Skip(1));
						break;

					case ConstructorScriptSemantics.ImplType.StaticMethod:
						constructorCall = JsExpression.Invocation(impl.IsGlobal ? (JsExpression)JsExpression.Identifier(impl.Name) : (JsExpression)JsExpression.MemberAccess(thisAndArguments[0], impl.Name), thisAndArguments.Skip(1));
						break;

					case ConstructorScriptSemantics.ImplType.InlineCode:
						return InlineCodeMethodCompiler.CompileInlineCodeMethodInvocation(method, impl.LiteralCode, null , thisAndArguments.Skip(1).ToList(), (t, c) => _runtimeLibrary.GetScriptType(t.Resolve(_compilation), c), isExpandedForm, s => _errorReporter.Message(7525, _filename, _location, s));

					default:
						_errorReporter.Message(7505, _filename, _location);
						return JsExpression.Number(0);
				}

				if (initializerStatements != null && initializerStatements.Count > 0) {
					var obj = _createTemporaryVariable(method.DeclaringType);
					var oldObjectBeingInitialized = _objectBeingInitialized;
					_objectBeingInitialized = obj;
					_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[_objectBeingInitialized].Name, constructorCall));
					foreach (var init in initializerStatements) {
						var js = VisitResolveResult(init, false);
						_additionalStatements.Add(new JsExpressionStatement(js));
					}
					_objectBeingInitialized = oldObjectBeingInitialized;

					return JsExpression.Identifier(_variables[obj].Name);
				}
				else {
					return constructorCall;
				}
			}
		}

		public override JsExpression VisitInitializedObjectResolveResult(InitializedObjectResolveResult rr, bool data) {
			return JsExpression.Identifier(_variables[_objectBeingInitialized].Name);
		}

		private JsExpression HandleInvocation(IMember member, ResolveResult targetResult, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap, IList<ResolveResult> initializerStatements, bool isVirtualCall, bool isExpandedForm) {
			if (member is IMethod) {
				if (member.Name == "Invoke" && member.DeclaringType.Kind == TypeKind.Delegate) {
					// Invoke the underlying method instead of calling the Invoke method.
					return CompileMethodInvocation(null, (IMethod)member, targetResult, argumentsForCall, argumentToParameterMap, isVirtualCall, isExpandedForm);
				}
				else {
					var method = (IMethod)member;
					if (method.IsConstructor) {
						if (method.DeclaringType.Kind == TypeKind.Enum) {
							return JsExpression.Number(0);
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
					_errorReporter.InternalError("Cannot invoke property that does not have a get method.", _filename, _location);
					return JsExpression.Number(0);
				}
				return CompileMethodInvocation(impl.GetMethod, property.Getter, targetResult, argumentsForCall, argumentToParameterMap, isVirtualCall, isExpandedForm);
			}
			else {
				_errorReporter.InternalError("Invocation of unsupported member " + member.DeclaringType.FullName + "." + member.Name, _filename, _location);
				return JsExpression.Number(0);
			}
		}

		public override JsExpression VisitInvocationResolveResult(InvocationResolveResult rr, bool data) {
			return HandleInvocation(rr.Member, rr.TargetResult, rr.Arguments, null, rr.InitializerStatements, rr.IsVirtualCall, false);
		}

		public override JsExpression VisitCSharpInvocationResolveResult(CSharpInvocationResolveResult rr, bool returnValueIsImportant) {
			return HandleInvocation(rr.Member, rr.TargetResult, rr.GetArgumentsForCall(), rr.GetArgumentToParameterMap(), rr.InitializerStatements, rr.IsVirtualCall, rr.IsExpandedForm);
		}

		public override JsExpression VisitConstantResolveResult(ConstantResolveResult rr, bool returnValueIsImportant) {
			if (rr.ConstantValue == null && rr.Type.IsReferenceType != true)
				return _runtimeLibrary.Default(rr.Type);
			else
				return JSModel.Utils.MakeConstantExpression(rr.ConstantValue);
		}

		private JsExpression CompileThis() {
			if (_thisAlias != null) {
				return JsExpression.Identifier(_thisAlias);
			}
			else if (_nestedFunctionContext != null && _nestedFunctionContext.CapturedByRefVariables.Count != 0) {
				return JsExpression.MemberAccess(JsExpression.This, _namer.ThisAlias);
			}
			else {
				return JsExpression.This;
			}
		}

		public override JsExpression VisitThisResolveResult(ThisResolveResult rr, bool returnValueIsImportant) {
			return CompileThis();
		}

		private JsExpression CompileLambda(LambdaResolveResult rr, bool returnValue) {
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
				def = _createInnerCompiler(newContext).CompileMethod(rr.Parameters, _variables, (BlockStatement)f.BodyNode);
			}
			else {
				var result = CloneAndCompile(rr.Body, true, nestedFunctionContext: newContext);
				var lastStatement = (returnValue ? (JsStatement)new JsReturnStatement(result.Expression) : (JsStatement)new JsExpressionStatement(result.Expression));
				var jsBody = new JsBlockStatement(MethodCompiler.FixByRefParameters(rr.Parameters, _variables).Concat(result.AdditionalStatements).Concat(new[] { lastStatement }));
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

			return captureObject != null
			     ? _runtimeLibrary.Bind(def, captureObject)
				 : def;
		}

		private JsExpression CompileLocal(IVariable variable, bool returnReference) {
			var data = _variables[variable];
			if (data.UseByRefSemantics) {
				var target = _nestedFunctionContext != null && _nestedFunctionContext.CapturedByRefVariables.Contains(variable)
				           ? (JsExpression)JsExpression.MemberAccess(JsExpression.This, data.Name)	// If using a captured by-ref variable, we access it using this.name.$
						   : (JsExpression)JsExpression.Identifier(data.Name);

				return returnReference ? target : JsExpression.MemberAccess(target, "$");
			}
			else {
				return JsExpression.Identifier(_variables[variable].Name);
			}
		}

		public override JsExpression VisitLocalResolveResult(LocalResolveResult rr, bool returnValueIsImportant) {
			if (rr.Variable is IParameter && ((IParameter)rr.Variable).IsParams && _methodBeingCompiled != null) {
				var impl = _metadataImporter.GetMethodSemantics(_methodBeingCompiled);
				if (impl.ExpandParams) {
					_errorReporter.Message(7521, _filename, _location, rr.Variable.Name);
				}
			}

			return CompileLocal(rr.Variable, false);
		}

		public override JsExpression VisitTypeOfResolveResult(TypeOfResolveResult rr, bool returnValueIsImportant) {
			var unusableTypes = Utils.FindUsedUnusableTypes(new[] { rr.ReferencedType }, _metadataImporter).ToList();
			if (unusableTypes.Count > 0) {
				foreach (var ut in unusableTypes)
					_errorReporter.Message(7522, _filename, _location, ut.FullName);
				return JsExpression.Number(0);
			}
			else
				return _runtimeLibrary.GetScriptType(rr.ReferencedType, TypeContext.TypeOf);
		}

		public override JsExpression VisitTypeResolveResult(TypeResolveResult rr, bool returnValueIsImportant) {
			return _runtimeLibrary.GetScriptType(rr.Type, TypeContext.Instantiation);
		}

		public override JsExpression VisitArrayAccessResolveResult(ArrayAccessResolveResult rr, bool returnValueIsImportant) {
			if (rr.Indexes.Count != 1) {
				_errorReporter.Message(7510, _filename, _location);
				return JsExpression.Number(0);
			}
			var array = InnerCompile(rr.Array, false);
			var index = InnerCompile(rr.Indexes[0], false, ref array);
			return JsExpression.Index(array, index);
		}

		public override JsExpression VisitArrayCreateResolveResult(ArrayCreateResolveResult rr, bool returnValueIsImportant) {
			if (((ArrayType)rr.Type).Dimensions != 1) {
				_errorReporter.Message(7510, _filename, _location);
				return JsExpression.Number(0);
			}
			if (rr.SizeArguments != null) {
				if (rr.SizeArguments[0].IsCompileTimeConstant && Convert.ToInt64(rr.SizeArguments[0].ConstantValue) == 0)
					return JsExpression.ArrayLiteral();

				return _runtimeLibrary.CreateArray(VisitResolveResult(rr.SizeArguments[0], true));
			}
			if (rr.InitializerElements != null) {
				var expressions = new List<JsExpression>();
				foreach (var init in rr.InitializerElements)
					expressions.Add(InnerCompile(init, false, expressions));
				return JsExpression.ArrayLiteral(expressions);
			}
			else {
				return JsExpression.ArrayLiteral();
			}
		}

        public override JsExpression VisitTypeIsResolveResult(TypeIsResolveResult rr, bool returnValueIsImportant) {
			var targetType = IsNullableType(rr.TargetType) ? GetNonNullableType(rr.TargetType) : rr.TargetType;
			return _runtimeLibrary.TypeIs(VisitResolveResult(rr.Input, returnValueIsImportant), rr.Input.Type, targetType);
        }

		public override JsExpression VisitByReferenceResolveResult(ByReferenceResolveResult rr, bool returnValueIsImportant) {
			_errorReporter.InternalError("Resolve result " + rr.ToString() + " should have been handled in method call.", _filename, _location);
			return JsExpression.Number(0);
		}

		public override JsExpression VisitDefaultResolveResult(ResolveResult rr, bool returnValueIsImportant) {
			if (rr.Type.Kind == TypeKind.Null)
				return JsExpression.Null;
			_errorReporter.InternalError("Resolve result " + rr + " is not handled.", _filename, _location);
			return JsExpression.Number(0);
		}

		public override JsExpression VisitConversionResolveResult(ConversionResolveResult rr, bool returnValueIsImportant) {
			if (rr.Conversion.IsIdentityConversion) {
				return VisitResolveResult(rr.Input, true);
			}
			else if (rr.Conversion.IsAnonymousFunctionConversion) {
				var retType = rr.Type.GetDelegateInvokeMethod().ReturnType;
				return CompileLambda((LambdaResolveResult)rr.Input, !retType.Equals(_compilation.FindType(KnownTypeCode.Void)));
			}
			else if (rr.Conversion.IsTryCast) {
				return _runtimeLibrary.TryDowncast(VisitResolveResult(rr.Input, true), rr.Input.Type, IsNullableType(rr.Type) ? GetNonNullableType(rr.Type) : rr.Type);
			}
			else if (rr.Conversion.IsReferenceConversion) {
				var input = VisitResolveResult(rr.Input, true);

				if (rr.Type is ArrayType && rr.Input.Type is ArrayType)	// Array covariance / contravariance.
					return input;
				else if (rr.Type.Kind == TypeKind.Dynamic)
					return input;
				else if (rr.Type.Kind == TypeKind.Delegate && rr.Input.Type.Kind == TypeKind.Delegate && !rr.Type.Equals(_compilation.FindType(KnownTypeCode.MulticastDelegate)) && !rr.Input.Type.Equals(_compilation.FindType(KnownTypeCode.MulticastDelegate)))
					return input;	// Conversion between compatible delegate types.
				else if (rr.Conversion.IsImplicit)
					return _runtimeLibrary.Upcast(input, rr.Input.Type, rr.Type);
				else
					return _runtimeLibrary.Downcast(input, rr.Input.Type, rr.Type);
			}
			else if (rr.Conversion.IsNumericConversion) {
				var result = VisitResolveResult(rr.Input, true);
				if (IsNullableType(rr.Input.Type) && !IsNullableType(rr.Type))
					result = _runtimeLibrary.FromNullable(result);

				if (!IsIntegerType(rr.Input.Type) && IsIntegerType(rr.Type)) {
					result = _runtimeLibrary.FloatToInt(result);

					if (IsNullableType(rr.Input.Type) && IsNullableType(rr.Type)) {
						result = _runtimeLibrary.Lift(result);
					}
				}
				return result;
			}
			else if (rr.Conversion.IsDynamicConversion) {
				var result = VisitResolveResult(rr.Input, true);
				if (IsNullableType(rr.Type)) {
					// Unboxing to nullable type.
					return _runtimeLibrary.Downcast(result, rr.Input.Type, GetNonNullableType(rr.Type));
				}
				else if (rr.Type.Kind == TypeKind.Struct) {
					// Unboxing to non-nullable type.
					return _runtimeLibrary.FromNullable(_runtimeLibrary.Downcast(result, rr.Input.Type, rr.Type));
				}
				else {
					// Converting to a boring reference type.
					return _runtimeLibrary.Downcast(result, rr.Input.Type, rr.Type);
				}
			}
			else if (rr.Conversion.IsNullableConversion || rr.Conversion.IsEnumerationConversion) {
				var result = VisitResolveResult(rr.Input, true);
				if (IsNullableType(rr.Input.Type) && !IsNullableType(rr.Type))
					result = _runtimeLibrary.FromNullable(result);
				return result;
			}
			else if (rr.Conversion.IsMethodGroupConversion) {
				var mgrr = (MethodGroupResolveResult)rr.Input;

				if (mgrr.TargetResult.Type.Kind == TypeKind.Delegate && Equals(rr.Conversion.Method, mgrr.TargetResult.Type.GetDelegateInvokeMethod()))
					return _runtimeLibrary.CloneDelegate(InnerCompile(mgrr.TargetResult, false), rr.Conversion.Method.DeclaringType, rr.Type);	// new D2(d1)

				var impl = _metadataImporter.GetMethodSemantics(rr.Conversion.Method);
				if (impl.Type != MethodScriptSemantics.ImplType.NormalMethod) {
					_errorReporter.Message(7523, _filename, _location, rr.Conversion.Method.DeclaringType + "." + rr.Conversion.Method.Name);
					return JsExpression.Number(0);
				}
				else if (impl.ExpandParams) {
					_errorReporter.Message(7524, _filename, _location, rr.Conversion.Method.DeclaringType + "." + rr.Conversion.Method.Name);
					return JsExpression.Number(0);
				}

				var typeArguments = (rr.Conversion.Method is SpecializedMethod && !impl.IgnoreGenericArguments) ? ((SpecializedMethod)rr.Conversion.Method).TypeArguments : new List<IType>();

				if (rr.Conversion.Method.IsOverridable && !rr.Conversion.IsVirtualMethodLookup) {
					// base.Method
					var jsTarget = InnerCompile(mgrr.TargetResult, true);
					return _runtimeLibrary.BindBaseCall(rr.Conversion.Method.DeclaringType, impl.Name, typeArguments, jsTarget);
				}
				else {
					JsExpression jsTarget, jsMethod;

					if (rr.Conversion.Method.IsStatic) {
						jsTarget = null;
						jsMethod = JsExpression.MemberAccess(_runtimeLibrary.GetScriptType(mgrr.TargetResult.Type, TypeContext.Instantiation), impl.Name);
					}
					else {
						jsTarget = InnerCompile(mgrr.TargetResult, true);
						jsMethod = JsExpression.MemberAccess(jsTarget, impl.Name);
					}

					if (typeArguments.Count > 0) {
						jsMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, typeArguments);
					}

					return jsTarget != null ? _runtimeLibrary.Bind(jsMethod, jsTarget) : jsMethod;
				}
			}
			else if (rr.Conversion.IsBoxingConversion) {
				var result = VisitResolveResult(rr.Input, true);
				if (rr.Type.Kind != TypeKind.Dynamic && (rr.Type is ITypeParameter || rr.Type.Equals(_compilation.FindType(KnownTypeCode.ValueType))))
					result = _runtimeLibrary.Upcast(result, rr.Input.Type, rr.Type);
				else if (rr.Input.Type is ITypeParameter)
					result = _runtimeLibrary.Downcast(result, rr.Input.Type, rr.Type);
				return result;
			}
			else if (rr.Conversion.IsUnboxingConversion) {
				var result = VisitResolveResult(rr.Input, true);
				if (IsNullableType(rr.Type)) {
					return _runtimeLibrary.Downcast(result, rr.Input.Type, GetNonNullableType(rr.Type));
				}
				else {
					result = _runtimeLibrary.Downcast(result, rr.Input.Type, rr.Type);
					if (rr.Type.Kind == TypeKind.Struct)
						result = _runtimeLibrary.FromNullable(result);	// hidden gem in the C# spec: conversions involving type parameter which are not known to not be unboxing are considered unboxing conversions.
					return result;
				}
			}
			else if (rr.Conversion.IsUserDefined) {
				var impl = _metadataImporter.GetMethodSemantics(rr.Conversion.Method);
				return CompileMethodInvocation(impl, rr.Conversion.Method, new TypeResolveResult(rr.Conversion.Method.DeclaringType), new[] { rr.Input }, null, false, false);
			}
			else if (rr.Conversion.IsNullLiteralConversion || rr.Conversion.IsConstantExpressionConversion) {
				return VisitResolveResult(rr.Input, true);
			}

			_errorReporter.InternalError("Conversion " + rr.Conversion + " is not implemented", _filename, _location);
			return JsExpression.Number(0);
		}

		public override JsExpression VisitDynamicMemberResolveResult(DynamicMemberResolveResult rr, bool data) {
			return JsExpression.MemberAccess(VisitResolveResult(rr.Target, true), rr.Member);
		}

		public override JsExpression VisitDynamicInvocationResolveResult(DynamicInvocationResolveResult rr, bool data) {
			if (rr.InvocationType == DynamicInvocationType.ObjectCreation) {
				if (rr.Arguments.Any(arg => arg is NamedArgumentResolveResult)) {
					_errorReporter.Message(7526, _filename, _location);
					return JsExpression.Number(0);
				}
				var methods = ((MethodGroupResolveResult)rr.Target).Methods.ToList();
				var semantics = methods.Select(_metadataImporter.GetConstructorSemantics).ToList();

				if (semantics.Select(s => s.Type).Distinct().Count() > 1) {
					_errorReporter.Message(7531, _filename, _location);
					return JsExpression.Number(0);
				}
				switch (semantics[0].Type) {
					case ConstructorScriptSemantics.ImplType.UnnamedConstructor:
						break;

					case ConstructorScriptSemantics.ImplType.NamedConstructor:
					case ConstructorScriptSemantics.ImplType.StaticMethod:
						if (semantics.Select(s => s.Name).Distinct().Count() > 1) {
							_errorReporter.Message(7531, _filename, _location);
							return JsExpression.Number(0);
						}
						break;

					default:
						_errorReporter.Message(7531, _filename, _location);
						return JsExpression.Number(0);
				}

				return CompileConstructorInvocation(semantics[0], methods[0], rr.Arguments, null, rr.InitializerStatements, false);
			}
			else {
				if (rr.InvocationType == DynamicInvocationType.Indexing && rr.Arguments.Count != 1) {
					_errorReporter.Message(7528, _filename, _location);
					return JsExpression.Number(0);
				}

				var expressions = new List<JsExpression>();
				if (rr.Target is MethodGroupResolveResult) {
					var mgrr = (MethodGroupResolveResult)rr.Target;
					var impl = mgrr.Methods.Select(_metadataImporter.GetMethodSemantics).ToList();
					if (impl.Any(x => x.Type != MethodScriptSemantics.ImplType.NormalMethod)) {
						_errorReporter.Message(7530, _filename, _location);
						return JsExpression.Number(0);
					}
					if (impl.Any(x => x.Name != impl[0].Name)) {
						_errorReporter.Message(7529, _filename, _location);
						return JsExpression.Number(0);
					}
					expressions.Add(JsExpression.MemberAccess(InnerCompile(mgrr.TargetResult, false), impl[0].Name));
				}
				else {
					expressions.Add(InnerCompile(rr.Target, false));
				}

				foreach (var arg in rr.Arguments) {
					if (arg is NamedArgumentResolveResult) {
						_errorReporter.Message(7526, _filename, _location);
						return JsExpression.Number(0);
					}
					expressions.Add(InnerCompile(arg, false, expressions));
				}

				switch (rr.InvocationType) {
					case DynamicInvocationType.Indexing:
						return (JsExpression)JsExpression.Index(expressions[0], expressions[1]);

					case DynamicInvocationType.Invocation:
						return JsExpression.Invocation(expressions[0], expressions.Skip(1));

					default:
						_errorReporter.InternalError("Unsupported dynamic invocation type " + rr.InvocationType, _filename, _location);
						return JsExpression.Number(0);
				}
			}
		}

		public override JsExpression VisitNamedArgumentResolveResult(NamedArgumentResolveResult rr, bool data) {
			return VisitResolveResult(rr.Argument, data);	// Argument names are ignored.
		}
	}
}
