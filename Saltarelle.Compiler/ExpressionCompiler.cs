using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler {
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

			public override JsExpression Visit(JsArrayLiteralExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsBinaryExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsCommaExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsFunctionDefinitionExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsInvocationExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsNewExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsObjectLiteralExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsUnaryExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsMemberAccessExpression expression, object data) {
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

			public override JsExpression Visit(JsInvocationExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsNewExpression expression, object data) {
				_result = true;
				return expression;
			}

			public override JsExpression Visit(JsBinaryExpression expression, object data) {
				if (expression.NodeType >= ExpressionNodeType.AssignFirst && expression.NodeType <= ExpressionNodeType.AssignLast) {
					_result = true;
					return expression;
				}
				else {
					return base.Visit(expression, data);
				}
			}

			public override JsExpression Visit(JsUnaryExpression expression, object data) {
				switch (expression.NodeType) {
					case ExpressionNodeType.PrefixPlusPlus:
					case ExpressionNodeType.PrefixMinusMinus:
					case ExpressionNodeType.PostfixPlusPlus:
					case ExpressionNodeType.PostfixMinusMinus:
					case ExpressionNodeType.Delete:
						_result = true;
						return expression;
					default:
						return base.Visit(expression, data);
				}
			}
		}

		private readonly ICompilation _compilation;
		private readonly INamingConventionResolver _namingConvention;
		private readonly IRuntimeLibrary _runtimeLibrary;
		private readonly IErrorReporter _errorReporter;
		private readonly IDictionary<IVariable, VariableData> _variables;
		private readonly IDictionary<LambdaResolveResult, NestedFunctionData> _nestedFunctions;
		private readonly Func<IType, LocalResolveResult> _createTemporaryVariable;
		private readonly Func<NestedFunctionContext, StatementCompiler> _createInnerCompiler;
		private readonly string _thisAlias;
		private NestedFunctionContext _nestedFunctionContext;
		private IVariable _objectBeingInitialized;

		public class Result {
			public JsExpression Expression { get; set; }
			public ReadOnlyCollection<JsStatement> AdditionalStatements { get; private set; }

			public Result(JsExpression expression, IEnumerable<JsStatement> additionalStatements) {
				this.Expression           = expression;
				this.AdditionalStatements = additionalStatements.AsReadOnly();
			}
		}

		public ExpressionCompiler(ICompilation compilation, INamingConventionResolver namingConvention, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, IDictionary<IVariable, VariableData> variables, IDictionary<LambdaResolveResult, NestedFunctionData> nestedFunctions, Func<IType, LocalResolveResult> createTemporaryVariable, Func<NestedFunctionContext, StatementCompiler> createInnerCompiler, string thisAlias, NestedFunctionContext nestedFunctionContext, IVariable objectBeingInitialized) {
			Require.ValidJavaScriptIdentifier(thisAlias, "thisAlias", allowNull: true);

			_compilation = compilation;
			_namingConvention = namingConvention;
			_runtimeLibrary = runtimeLibrary;
			_errorReporter = errorReporter;
			_variables = variables;
			_nestedFunctions = nestedFunctions;
			_createTemporaryVariable = createTemporaryVariable;
			_createInnerCompiler = createInnerCompiler;
			_thisAlias = thisAlias;
			_nestedFunctionContext = nestedFunctionContext;
			_objectBeingInitialized = objectBeingInitialized;
		}

		private List<JsStatement> _additionalStatements;

		public Result Compile(ResolveResult expression, bool returnValueIsImportant) {
			_additionalStatements = new List<JsStatement>();
			var expr = VisitResolveResult(expression, returnValueIsImportant);
			return new Result(expr, _additionalStatements);
		}

		private Result CloneAndCompile(ResolveResult expression, bool returnValueIsImportant, NestedFunctionContext nestedFunctionContext = null) {
			return new ExpressionCompiler(_compilation, _namingConvention, _runtimeLibrary, _errorReporter, _variables, _nestedFunctions, _createTemporaryVariable, _createInnerCompiler, _thisAlias, nestedFunctionContext ?? _nestedFunctionContext, _objectBeingInitialized).Compile(expression, returnValueIsImportant);
		}

		private void CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(IList<JsExpression> expressions, Result newExpressions) {
			for (int i = 0; i < expressions.Count; i++) {
				if (ExpressionOrderer.DoesOrderMatter(expressions[i], newExpressions)) {
					var temp = _createTemporaryVariable(_compilation.FindType(KnownTypeCode.Object));
					_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp.Variable].Name, expressions[i]));
					expressions[i] = JsExpression.Identifier(_variables[temp.Variable].Name);
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
				_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp.Variable].Name, result.Expression));
				return JsExpression.Identifier(_variables[temp.Variable].Name);
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
				type = ((ParameterizedType)type).TypeArguments[0];

			return type.Equals(_compilation.FindType(KnownTypeCode.Byte))
			    || type.Equals(_compilation.FindType(KnownTypeCode.SByte))
			    || type.Equals(_compilation.FindType(KnownTypeCode.Int16))
			    || type.Equals(_compilation.FindType(KnownTypeCode.UInt16))
			    || type.Equals(_compilation.FindType(KnownTypeCode.Int32))
			    || type.Equals(_compilation.FindType(KnownTypeCode.UInt32))
			    || type.Equals(_compilation.FindType(KnownTypeCode.Int64))
			    || type.Equals(_compilation.FindType(KnownTypeCode.UInt64));
		}

		private bool IsUnsignedType(IType type) {
			if (IsNullableType(type))
				type = ((ParameterizedType)type).TypeArguments[0];

			return type.Equals(_compilation.FindType(KnownTypeCode.Byte))
			    || type.Equals(_compilation.FindType(KnownTypeCode.UInt16))
				|| type.Equals(_compilation.FindType(KnownTypeCode.UInt32))
				|| type.Equals(_compilation.FindType(KnownTypeCode.UInt64));
		}

		private bool IsNullableType(IType type) {
			return type.GetDefinition().Equals(_compilation.FindType(KnownTypeCode.NullableOfT));
		}

		private bool IsNullableBooleanType(IType type) {
			return type.GetDefinition().Equals(_compilation.FindType(KnownTypeCode.NullableOfT))
			    && ((ParameterizedType)type).TypeArguments[0] == _compilation.FindType(KnownTypeCode.Boolean);
		}

		private bool IsDelegateType(IType type) {
			var del = _compilation.FindType(KnownTypeCode.Delegate);
			return type.GetAllBaseTypes().Any(b => b.Equals(del));
		}

		private JsExpression GetJsType(IType type) {
			if (type is ParameterizedType) {
				var pt = (ParameterizedType)type;
				return _runtimeLibrary.InstantiateGenericType(GetJsType(pt.GetDefinition()), pt.TypeArguments.Select(GetJsType));
			}
			else if (type is ITypeDefinition)
				return new JsTypeReferenceExpression((ITypeDefinition)type);
			else if (type is ITypeParameter)
				return JsExpression.Identifier(_namingConvention.GetTypeParameterName(((ITypeParameter)type)));
			else
				throw new NotSupportedException("Unsupported type " + type.ToString());
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
					_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp.Variable].Name, access));
					_additionalStatements.Add( new JsExpressionStatement(JsExpression.Assign(access, valueFactory(JsExpression.Identifier(_variables[temp.Variable].Name), jsOtherOperand))));
					return JsExpression.Identifier(_variables[temp.Variable].Name);
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
					_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp.Variable].Name, access));
					_additionalStatements.Add(new JsExpressionStatement(JsExpression.Assign(access, valueFactory(JsExpression.Identifier(_variables[temp.Variable].Name), jsOtherOperand))));
					return JsExpression.Identifier(_variables[temp.Variable].Name);
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

			if (target is MemberResolveResult) {
				var mrr = (MemberResolveResult)target;

				if (mrr.Member is IProperty) {
					var property = ((MemberResolveResult)target).Member as IProperty;
					var impl = _namingConvention.GetPropertyImplementation(property);

					switch (impl.Type) {
						case PropertyImplOptions.ImplType.GetAndSetMethods: {
							if (impl.SetMethod.Type == MethodImplOptions.ImplType.NativeIndexer) {
								if (!property.IsIndexer || property.Getter.Parameters.Count != 1) {
									_errorReporter.Error("Property " + property.DeclaringType.FullName + "." + property.Name + ", declared as being a native indexer, is not an indexer with exactly one argument.");
									return JsExpression.Number(0);
								}
								return CompileArrayAccessCompoundAssignment(mrr.TargetResult, ((CSharpInvocationResolveResult)mrr).Arguments[0], otherOperand, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
							}
							else {
								List<JsExpression> thisAndArguments;
								if (property.IsIndexer) {
									thisAndArguments = CompileThisAndArgumentListForMethodCall((CSharpInvocationResolveResult)target, oldValueIsImportant, oldValueIsImportant);
								}
								else {
									thisAndArguments = new List<JsExpression> { InnerCompile(mrr.TargetResult, oldValueIsImportant) };
								}

								JsExpression oldValue, jsOtherOperand;
								if (oldValueIsImportant) {
									thisAndArguments.Add(CompileMethodInvocation(impl.GetMethod, property.Getter, thisAndArguments, new IType[0]));
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
										_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp.Variable].Name, valueToReturn));
										valueToReturn = JsExpression.Identifier(_variables[temp.Variable].Name);
									}

									var newValue = (returnValueBeforeChange ? valueFactory(valueToReturn, jsOtherOperand) : valueToReturn);

									thisAndArguments.Add(newValue);
									_additionalStatements.Add(new JsExpressionStatement(CompileMethodInvocation(impl.SetMethod, property.Setter, thisAndArguments, new IType[0])));
									return valueToReturn;
								}
								else {
									thisAndArguments.Add(valueFactory(oldValue, jsOtherOperand));
									return CompileMethodInvocation(impl.SetMethod, property.Setter, thisAndArguments, new IType[0]);
								}
							}
						}

						case PropertyImplOptions.ImplType.Field: {
							return CompileCompoundFieldAssignment(mrr, otherOperand, impl.FieldName, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
						}

						default: {
							_errorReporter.Error("Cannot use property " + property.DeclaringType.FullName + "." + property.Name + " from script.");
							return JsExpression.Number(0);
						}
					}
				}
				else if (mrr.Member is IField) {
					var field = (IField)mrr.Member;
					var impl = _namingConvention.GetFieldImplementation(field);
					if (impl.Type == FieldImplOptions.ImplType.Field) {
						return CompileCompoundFieldAssignment(mrr, otherOperand, impl.Name, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
					}
					else {
						_errorReporter.Error("Field " + field.DeclaringType.FullName + "." + field.Name + " is not usable from script.");
						return JsExpression.Number(0);
					}
				}
				else {
					throw new InvalidOperationException("Target " + mrr.Member.DeclaringType.FullName + "." + mrr.Member.Name + " of compound assignment is neither a property nor a field.");
				}
			}
			else if (target is LocalResolveResult) {
				var jsTarget = InnerCompile(target, compoundFactory == null);
				var jsOtherOperand = (otherOperand != null ? InnerCompile(otherOperand, false, ref jsTarget) : null);
				if (compoundFactory != null) {
					return compoundFactory(jsTarget, jsOtherOperand);
				}
				else {
					if (returnValueIsImportant && returnValueBeforeChange) {
						var temp = _createTemporaryVariable(target.Type);
						_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp.Variable].Name, jsTarget));
						_additionalStatements.Add( new JsExpressionStatement(JsExpression.Assign(jsTarget, valueFactory(JsExpression.Identifier(_variables[temp.Variable].Name), jsOtherOperand))));
						return JsExpression.Identifier(_variables[temp.Variable].Name);
					}
					else {
						return JsExpression.Assign(jsTarget, valueFactory(jsTarget, jsOtherOperand));
					}
				}

			}
			else if (target is ArrayAccessResolveResult) {
				var arr = (ArrayAccessResolveResult)target;
				if (arr.Indexes.Count != 1) {
					_errorReporter.Error("Arrays have to be one-dimensional.");
					return JsExpression.Number(0);
				}
				return CompileArrayAccessCompoundAssignment(arr.Array, arr.Indexes[0], otherOperand, compoundFactory, valueFactory, returnValueIsImportant, returnValueBeforeChange);
			}
			else {
				_errorReporter.Error("Unsupported target of compound assignment: " + target.ToString());
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
				var trueBlock  = new JsBlockStatement(trueResult.AdditionalStatements.Concat(new[] { new JsVariableDeclarationStatement(_variables[temp.Variable].Name, trueResult.Expression) }));
				var falseBlock = new JsBlockStatement(falseResult.AdditionalStatements.Concat(new[] { new JsVariableDeclarationStatement(_variables[temp.Variable].Name, falseResult.Expression) }));
				_additionalStatements.Add(new JsIfStatement(jsTest, trueBlock, falseBlock));
				return JsExpression.Identifier(_variables[temp.Variable].Name);
			}
			else {
				return JsExpression.Conditional(jsTest, trueResult.Expression, falseResult.Expression);
			}
		}

		private JsExpression CompileEventAddOrRemove(MemberResolveResult target, ResolveResult value, bool isAdd) {
			var evt = (IEvent)target.Member;
			var impl = _namingConvention.GetEventImplementation(evt);
			switch (impl.Type) {
				case EventImplOptions.ImplType.AddAndRemoveMethods: {
					var accessor = isAdd ? evt.AddAccessor : evt.RemoveAccessor;
					return CompileMethodInvocation(isAdd ? impl.AddMethod : impl.RemoveMethod, accessor, new CSharpInvocationResolveResult(target.TargetResult, accessor, new[] { value }));
				}
				default:
					_errorReporter.Error("Cannot use event " + target.Member.DeclaringType.FullName + "." + target.Member.Name + " from script.");
					return JsExpression.Number(0);
			}
		}

		public override JsExpression VisitOperatorResolveResult(OperatorResolveResult rr, bool returnValueIsImportant) {
			switch (rr.OperatorType) {
				case ExpressionType.Assign:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.Assign, (a, b) => b, returnValueIsImportant, false, oldValueIsImportant: false);

				// Compound assignment operators

				case ExpressionType.AddAssign:
				case ExpressionType.AddAssignChecked:
					if (rr.Operands[0] is MemberResolveResult && ((MemberResolveResult)rr.Operands[0]).Member is IEvent) {
						return CompileEventAddOrRemove((MemberResolveResult)rr.Operands[0], rr.Operands[1], true);
					}
					else if (IsDelegateType(rr.Operands[0].Type)) {
						var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
						var combine = del.GetMethods().Single(m => m.Name == "Combine" && m.Parameters.Count == 2);
						var impl = _namingConvention.GetMethodImplementation(combine);
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => CompileMethodInvocation(impl, combine, new[] { new JsTypeReferenceExpression(del), a, b }, new IType[0]), returnValueIsImportant, false);
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
					else if (IsDelegateType(rr.Operands[0].Type)) {
						var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
						var remove = del.GetMethods().Single(m => m.Name == "Remove" && m.Parameters.Count == 2);
						var impl = _namingConvention.GetMethodImplementation(remove);
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => CompileMethodInvocation(impl, remove, new[] { new JsTypeReferenceExpression(del), a, b }, new IType[0]), returnValueIsImportant, false);
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
					if (IsDelegateType(rr.Operands[0].Type)) {
						var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
						var combine = del.GetMethods().Single(m => m.Name == "Combine" && m.Parameters.Count == 2);
						var impl = _namingConvention.GetMethodImplementation(combine);
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => CompileMethodInvocation(impl, combine, new[] { new JsTypeReferenceExpression(del), a, b }, new IType[0]), false);
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
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Same, false);	// We are so lucky that performing a lifted equality comparison in JS is the same as in C#, so no need to lift.

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
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.NotSame, false);	// We are so lucky that performing a lifted equality comparison in JS is the same as in C#, so no need to lift.

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
					if (IsDelegateType(rr.Operands[0].Type)) {
						var del = (ITypeDefinition)_compilation.FindType(KnownTypeCode.Delegate);
						var remove = del.GetMethods().Single(m => m.Name == "Remove" && m.Parameters.Count == 2);
						var impl = _namingConvention.GetMethodImplementation(remove);
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => CompileMethodInvocation(impl, remove, new[] { new JsTypeReferenceExpression(del), a, b }, new IType[0]), false);
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
					throw new ArgumentException("Unsupported operator " + rr.OperatorType);
			}
		}

		public override JsExpression VisitMethodGroupResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.MethodGroupResolveResult rr, bool returnValueIsImportant) {
			throw new InvalidOperationException("MethodGroupResolveResult should always be the target of a method group conversion, and is handled there");
		}

		public override JsExpression VisitLambdaResolveResult(LambdaResolveResult rr, bool returnValueIsImportant) {
			throw new InvalidOperationException("LambdaResolveResult should always be the target of an anonymous method conversion, and is handled there");
		}

		public override JsExpression VisitMemberResolveResult(MemberResolveResult rr, bool returnValueIsImportant) {
			if (rr.Member is IProperty) {
				var impl = _namingConvention.GetPropertyImplementation((IProperty)rr.Member);
				switch (impl.Type) {
					case PropertyImplOptions.ImplType.GetAndSetMethods: {
						var getter = ((IProperty)rr.Member).Getter;
						return CompileMethodInvocation(impl.GetMethod, getter, new CSharpInvocationResolveResult(rr.TargetResult, getter, new ResolveResult[0]));	// We know we have no arguments because indexers are treated as invocations.
					}
					case PropertyImplOptions.ImplType.Field: {
						var jsTarget = InnerCompile(rr.TargetResult, true);
						return JsExpression.MemberAccess(jsTarget, impl.FieldName);
					}
					default: {
						_errorReporter.Error("Property " + rr.Member.DeclaringType + "." + rr.Member.Name + " is not usable from script.");
						return JsExpression.Number(0);
					}
				}
			}
			else if (rr.Member is IField) {
				var impl = _namingConvention.GetFieldImplementation((IField)rr.Member);
				if (impl.Type == FieldImplOptions.ImplType.Field) {
					return JsExpression.MemberAccess(VisitResolveResult(rr.TargetResult, true), impl.Name);
				}
				else {
					_errorReporter.Error("Cannot use field " + rr.Member.DeclaringType.Name + "." + rr.Member.Name + " from script.");
					return JsExpression.Number(0);
				}
			}
			else if (rr.Member is IEvent) {
				var eimpl = _namingConvention.GetEventImplementation((IEvent)rr.Member);
				var fimpl = (eimpl.Type != EventImplOptions.ImplType.NotUsableFromScript ? _namingConvention.GetAutoEventBackingFieldImplementation((IEvent)rr.Member) : FieldImplOptions.NotUsableFromScript());
				if (fimpl.Type == FieldImplOptions.ImplType.Field) {
					return JsExpression.MemberAccess(VisitResolveResult(rr.TargetResult, true), fimpl.Name);
				}
				else {
					_errorReporter.Error("Cannot use backing field of event " + rr.Member.DeclaringType + "." + rr.Member.Name + " from script.");
					return JsExpression.Number(0);
				}
			}
			else
				throw new InvalidOperationException("Invalid member " + rr.Member.ToString());
		}

		private List<JsExpression> CompileThisAndArgumentListForMethodCall(CSharpInvocationResolveResult invocation, bool targetUsedMultipleTimes, bool argumentsUsedMultipleTimes) {
			return CompileThisAndArgumentListForMethodCall(invocation.TargetResult, targetUsedMultipleTimes, argumentsUsedMultipleTimes, invocation.Arguments, invocation.GetArgumentsForCall(), invocation.GetArgumentToParameterMap());
		}

		private List<JsExpression> CompileThisAndArgumentListForMethodCall(ResolveResult target, bool targetUsedMultipleTimes, bool argumentsUsedMultipleTimes, IList<ResolveResult> specifiedArguments, IList<ResolveResult> argumentsForCall, IList<int> argumentToParameterMap) {
			var expressions = new List<JsExpression>();
			expressions.Add(InnerCompile(target, targetUsedMultipleTimes));
			foreach (var a in specifiedArguments) {
				if (a is ByReferenceResolveResult) {
					var r = (ByReferenceResolveResult)a;
					if (r.ElementResult is LocalResolveResult) {
						expressions.Add(CompileLocal(((LocalResolveResult)r.ElementResult).Variable, true));
					}
					else {
						_errorReporter.Error("Only locals can be passed by reference.");
						expressions.Add(JsExpression.Number(0));
					}
				}
				else
					expressions.Add(InnerCompile(a, argumentsUsedMultipleTimes, expressions));
			}

			if (argumentToParameterMap != null && (argumentToParameterMap.Count != argumentsForCall.Count || argumentToParameterMap.Select((i, n) => new { i, n }).Any(t => t.i != t.n))) {	// If we have an argument to parameter map and it actually performs any reordering.
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
						if (!(specifiedArguments[specifiedIndex] is ByReferenceResolveResult)) {
							// Ensure that all arguments are evaluated in the correct order (doesn't have to be done for ref and out arguments.
							for (int j = 0; j < specifiedIndex; j++) {
								if (argumentToParameterMap[j] > i && ExpressionOrderer.DoesOrderMatter(expressions[specifiedIndex + 1], expressions[j + 1])) {	// This expression used to be evaluated before us, but will now be evaluated after us, so we need to create a temporary.
									var temp = _createTemporaryVariable(specifiedArguments[j].Type);
									_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp.Variable].Name, expressions[j + 1]));
									expressions[j + 1] = JsExpression.Identifier(_variables[temp.Variable].Name);
								}
							}
						}
						newExpressions.Add(expressions[specifiedIndex + 1]);
					}
				}
				expressions = newExpressions;
			}

			return expressions;
		}

		private JsExpression CompileMethodInvocation(MethodImplOptions impl, IMethod method, CSharpInvocationResolveResult invocation) {
			var typeArguments = method is SpecializedMethod ? ((SpecializedMethod)method).TypeArguments : new IType[0];
			var thisAndArguments = CompileThisAndArgumentListForMethodCall(invocation.TargetResult, impl != null && !impl.IgnoreGenericArguments && typeArguments.Count > 0 && !method.IsStatic, false, invocation.Arguments, invocation.GetArgumentsForCall(), invocation.GetArgumentToParameterMap());
			return CompileMethodInvocation(impl, method, thisAndArguments, typeArguments);
		}

		private JsExpression CompileMethodInvocation(MethodImplOptions impl, IMethod method, IList<JsExpression> thisAndArguments, IList<IType> typeArguments) {
			var jsTypeArguments = (impl != null && !impl.IgnoreGenericArguments && typeArguments.Count > 0 ? typeArguments.Select(GetJsType).ToList() : new List<JsExpression>());

			if (impl == null) {
				return JsExpression.Invocation(thisAndArguments[0], thisAndArguments.Skip(1));	// Used for delegate invocations.
			}
			else {
				switch (impl.Type) {
					case MethodImplOptions.ImplType.NormalMethod: {
						var jsMethod = JsExpression.MemberAccess(thisAndArguments[0], impl.Name);
						if (jsTypeArguments.Count > 0) {
							var genMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, jsTypeArguments);
							if (method.IsStatic)
								thisAndArguments[0] = JsExpression.Null;
							return JsExpression.Invocation(JsExpression.MemberAccess(genMethod, "call"), thisAndArguments);
						}
						else {
							return JsExpression.Invocation(jsMethod, thisAndArguments.Skip(1));
						}
					}

					case MethodImplOptions.ImplType.StaticMethodWithThisAsFirstArgument: {
						var jsMethod = JsExpression.MemberAccess(GetJsType(method.DeclaringType), impl.Name);
						if (jsTypeArguments.Count > 0) {
							var genMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, jsTypeArguments);
							return JsExpression.Invocation(JsExpression.MemberAccess(genMethod, "call"), new[] { JsExpression.Null }.Concat(thisAndArguments));
						}
						else {
							return JsExpression.Invocation(jsMethod, thisAndArguments);
						}
					}

					case MethodImplOptions.ImplType.InstanceMethodOnFirstArgument:
						return JsExpression.Invocation(JsExpression.MemberAccess(thisAndArguments[1], impl.Name), thisAndArguments.Skip(2));

					case MethodImplOptions.ImplType.InlineCode: {
						var allSubstitutions = new List<Tuple<string, JsExpression>>();

						var parameterizedType = method.DeclaringType as ParameterizedType;
						if (parameterizedType != null) {
							var def = parameterizedType.GetDefinition();
							for (int i = 0; i < def.TypeParameters.Count; i++)
								allSubstitutions.Add(Tuple.Create(def.TypeParameters[i].Name, GetJsType(parameterizedType.TypeArguments[i])));
						}

						var specializedMethod = method as SpecializedMethod;
						if (method is SpecializedMethod) {
							for (int i = 0; i < specializedMethod.TypeArguments.Count; i++)
								allSubstitutions.Add(Tuple.Create(specializedMethod.TypeParameters[i].Name, GetJsType(specializedMethod.TypeArguments[i])));
						}
						if (!method.IsStatic)
							allSubstitutions.Add(Tuple.Create("this", thisAndArguments[0]));
						for (int i = 1; i < thisAndArguments.Count; i++)
							allSubstitutions.Add(Tuple.Create(method.Parameters[i - 1].Name, thisAndArguments[i]));

						string format = impl.LiteralCode;
						var fmtarguments = new List<JsExpression>();
						foreach (var s in allSubstitutions) {
							if (format.Contains("{" + s.Item1 + "}")) {
								format = format.Replace("{" + s.Item1 + "}", "{" + fmtarguments.Count.ToString(CultureInfo.InvariantCulture) + "}");
								fmtarguments.Add(s.Item2);
							}
						}

						try {
							string.Format(format, new object[fmtarguments.Count]);
						}
						catch (Exception) {
							_errorReporter.Error("Invalid inline implementation of method " + method.DeclaringType.FullName + "." + method.Name);
							return JsExpression.Number(0);
						}

						return JsExpression.Literal(format, fmtarguments);
					}

					case MethodImplOptions.ImplType.NativeIndexer:
						return JsExpression.Index(thisAndArguments[0], thisAndArguments[1]);

					default: {
						_errorReporter.Error("Method " + method.DeclaringType.FullName + "." + method.Name + " cannot be used from script.");
						return JsExpression.Number(0);
					}
				}
			}
		}

		private JsExpression CompileConstrutorInvocation(ConstructorImplOptions impl, IMethod method, CSharpInvocationResolveResult invocation) {
			var thisAndArguments = CompileThisAndArgumentListForMethodCall(new TypeResolveResult(method.DeclaringType), false, false, invocation.Arguments, invocation.GetArgumentsForCall(), invocation.GetArgumentToParameterMap());

			JsExpression constructorCall;

			switch (impl.Type) {
				case ConstructorImplOptions.ImplType.UnnamedConstructor:
					constructorCall = JsExpression.New(thisAndArguments[0], thisAndArguments.Skip(1));
					break;

				case ConstructorImplOptions.ImplType.NamedConstructor:
					constructorCall = JsExpression.New(JsExpression.MemberAccess(thisAndArguments[0], impl.Name), thisAndArguments.Skip(1));
					break;

				case ConstructorImplOptions.ImplType.StaticMethod:
					constructorCall = JsExpression.Invocation(JsExpression.MemberAccess(thisAndArguments[0], impl.Name), thisAndArguments.Skip(1));
					break;

				default:
					_errorReporter.Error("This constructor cannot be used from script.");
					return JsExpression.Number(0);
			}

			if (invocation.InitializerStatements != null && invocation.InitializerStatements.Count > 0) {
				var obj = _createTemporaryVariable(method.DeclaringType);
				var oldObjectBeingInitialized = _objectBeingInitialized;
				_objectBeingInitialized = obj.Variable;
				_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[_objectBeingInitialized].Name, constructorCall));
				foreach (var init in invocation.InitializerStatements) {
					var js = VisitResolveResult(init, false);
					_additionalStatements.Add(new JsExpressionStatement(js));
				}
				_objectBeingInitialized = oldObjectBeingInitialized;

				return JsExpression.Identifier(_variables[obj.Variable].Name);
			}
			else {
				return constructorCall;
			}
		}

		public override JsExpression VisitInitializedObjectResolveResult(InitializedObjectResolveResult rr, bool data) {
			return JsExpression.Identifier(_variables[_objectBeingInitialized].Name);
		}

		public override JsExpression VisitCSharpInvocationResolveResult(CSharpInvocationResolveResult rr, bool returnValueIsImportant) {
			if (rr.Member is IMethod) {
				if (rr.Member.Name == "Invoke" && IsDelegateType(rr.Member.DeclaringType)) {
					// Invoke the underlying method instead of calling the Invoke method.
					return CompileMethodInvocation(null, (IMethod)rr.Member, rr);
				}
				else {
					var method = (IMethod)rr.Member;
					if (method.IsConstructor) {
						return CompileConstrutorInvocation(_namingConvention.GetConstructorImplementation(method), method, rr);
					}
					else {
						return CompileMethodInvocation(_namingConvention.GetMethodImplementation(method), method, rr);
					}
				}
			}
			else if (rr.Member is IProperty) {
				var property = (IProperty)rr.Member;
				var impl = _namingConvention.GetPropertyImplementation(property);
				if (impl.Type != PropertyImplOptions.ImplType.GetAndSetMethods) {
					_errorReporter.Error("Cannot invoke property that does not have a get method.");
					return JsExpression.Number(0);
				}
				return CompileMethodInvocation(impl.GetMethod, property.Getter, rr);
			}
			else {
				throw new InvalidOperationException("Invocation of unsupported member " + rr.Member.DeclaringType.FullName + "." + rr.Member.Name);
			}
		}

		public override JsExpression VisitConstantResolveResult(ConstantResolveResult rr, bool returnValueIsImportant) {
			if (rr.ConstantValue is bool)
				return (bool)rr.ConstantValue ? JsExpression.True : JsExpression.False;
			else if (rr.ConstantValue is sbyte)
				return JsExpression.Number((sbyte)rr.ConstantValue);
			else if (rr.ConstantValue is byte)
				return JsExpression.Number((byte)rr.ConstantValue);
			else if (rr.ConstantValue is char)
				return JsExpression.Number((char)rr.ConstantValue);
			else if (rr.ConstantValue is short)
				return JsExpression.Number((short)rr.ConstantValue);
			else if (rr.ConstantValue is ushort)
				return JsExpression.Number((ushort)rr.ConstantValue);
			else if (rr.ConstantValue is int)
				return JsExpression.Number((int)rr.ConstantValue);
			else if (rr.ConstantValue is uint)
				return JsExpression.Number((uint)rr.ConstantValue);
			else if (rr.ConstantValue is long)
				return JsExpression.Number((long)rr.ConstantValue);
			else if (rr.ConstantValue is ulong)
				return JsExpression.Number((ulong)rr.ConstantValue);
			else if (rr.ConstantValue is float)
				return JsExpression.Number((float)rr.ConstantValue);
			else if (rr.ConstantValue is double)
				return JsExpression.Number((double)rr.ConstantValue);
			else if (rr.ConstantValue is decimal)
				return JsExpression.Number((double)(decimal)rr.ConstantValue);
			if (rr.ConstantValue is string)
				return JsExpression.String((string)rr.ConstantValue);
			else if (rr.ConstantValue == null) {
				if (rr.Type.IsReferenceType == true)
					return JsExpression.Null;
				else
					return _runtimeLibrary.Default(GetJsType(rr.Type));
			}
			else
				throw new NotSupportedException("Unsupported constant " + rr.ConstantValue.ToString() + "(" + rr.ConstantValue.GetType().ToString() + ")");
		}

		private JsExpression CompileThis() {
			if (_thisAlias != null) {
				return JsExpression.Identifier(_thisAlias);
			}
			else if (_nestedFunctionContext != null && _nestedFunctionContext.CapturedByRefVariables.Count != 0) {
				return JsExpression.MemberAccess(JsExpression.This, _namingConvention.ThisAlias);
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

			JsBlockStatement jsBody;
			if (f.BodyNode is Statement) {
				jsBody = _createInnerCompiler(newContext).Compile((Statement)f.BodyNode);
			}
			else {
				var result = CloneAndCompile(rr.Body, true, nestedFunctionContext: newContext);
				var lastStatement = (returnValue ? (JsStatement)new JsReturnStatement(result.Expression) : (JsStatement)new JsExpressionStatement(result.Expression));
				jsBody = new JsBlockStatement(result.AdditionalStatements.Concat(new[] { lastStatement }));
			}

			var def = JsExpression.FunctionDefinition(rr.Parameters.Select(p => _variables[p].Name), jsBody);
			JsExpression captureObject;
			if (newContext.CapturedByRefVariables.Count > 0) {
				var toCapture = newContext.CapturedByRefVariables.Select(v => new JsObjectLiteralProperty(_variables[v].Name, CompileLocal(v, true))).ToList();
				if (captureThis)
					toCapture.Add(new JsObjectLiteralProperty(_namingConvention.ThisAlias, CompileThis()));
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
			return CompileLocal(rr.Variable, false);
		}

		public override JsExpression VisitTypeOfResolveResult(TypeOfResolveResult rr, bool returnValueIsImportant) {
			return GetJsType(rr.ReferencedType);
		}

		public override JsExpression VisitTypeResolveResult(TypeResolveResult rr, bool returnValueIsImportant) {
			return GetJsType(rr.Type);
		}

		public override JsExpression VisitArrayAccessResolveResult(ArrayAccessResolveResult rr, bool returnValueIsImportant) {
			if (rr.Indexes.Count != 1) {
				_errorReporter.Error("Arrays have to be one-dimensional");
				return JsExpression.Number(0);
			}
			var array = InnerCompile(rr.Array, false);
			var index = InnerCompile(rr.Indexes[0], false, ref array);
			return JsExpression.Index(array, index);
		}

		public override JsExpression VisitArrayCreateResolveResult(ArrayCreateResolveResult rr, bool returnValueIsImportant) {
			if (((ArrayType)rr.Type).Dimensions != 1) {
				_errorReporter.Error("Multi-dimensional arrays are not supported.");
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
			return _runtimeLibrary.TypeIs(VisitResolveResult(rr.Input, returnValueIsImportant), GetJsType(rr.TargetType));
        }

		// TODO: Methods below are UNTESTED and REALLY hacky, but needed for the statement compiler

		public override JsExpression VisitConversionResolveResult(ConversionResolveResult rr, bool returnValueIsImportant) {
			if (rr.Conversion.IsIdentityConversion) {
				return VisitResolveResult(rr.Input, returnValueIsImportant);
			}
			else if (rr.Conversion.IsAnonymousFunctionConversion) {
				var retType = rr.Type.GetMethods().Single(m => m.Name == "Invoke").ReturnType;
				return CompileLambda((LambdaResolveResult)rr.Input, !retType.Equals(_compilation.FindType(KnownTypeCode.Void)));
			}
			else if (rr.Conversion.IsTryCast) {
				return _runtimeLibrary.TryCast(VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			else if (rr.Conversion.IsReferenceConversion) {
				if (rr.Type.Kind == TypeKind.Dynamic)
					return VisitResolveResult(rr.Input, returnValueIsImportant);
				if (rr.Conversion.IsImplicit)
					return _runtimeLibrary.ImplicitReferenceConversion(VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
				else
					return _runtimeLibrary.Cast(VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			else if (rr.Conversion.IsNumericConversion) {
				return VisitResolveResult(rr.Input, returnValueIsImportant);
			}
			else if (rr.Conversion.IsDynamicConversion) {
				return _runtimeLibrary.Cast(VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			else if (rr.Conversion.IsNullableConversion) {
				return VisitResolveResult(rr.Input, returnValueIsImportant);
			}
			else if (rr.Conversion.IsMethodGroupConversion) {
				var mgrr = (MethodGroupResolveResult)rr.Input;
				var impl = _namingConvention.GetMethodImplementation(rr.Conversion.Method);
				if (impl.Type != MethodImplOptions.ImplType.NormalMethod) {
					_errorReporter.Error("Cannot perform method group conversion on " + rr.Conversion.Method.DeclaringType + "." + rr.Conversion.Method.Name + " because it is not a normal method.");
					return JsExpression.Number(0);
				}

				JsExpression jsTarget, jsMethod;

				if (rr.Conversion.Method.IsStatic) {
					jsTarget = null;
					jsMethod = JsExpression.MemberAccess(GetJsType(mgrr.TargetResult.Type), impl.Name);
				}
				else {
					jsTarget = InnerCompile(mgrr.TargetResult, true);
					jsMethod = JsExpression.MemberAccess(jsTarget, impl.Name);
				}

				if (rr.Conversion.Method is SpecializedMethod && !impl.IgnoreGenericArguments) {
					jsMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, ((SpecializedMethod)rr.Conversion.Method).TypeArguments.Select(GetJsType));
				}

				return jsTarget != null ? _runtimeLibrary.Bind(jsMethod, jsTarget) : jsMethod;
			}
			else if (rr.Conversion.IsImplicit) {
				// Null literal conversion have no property, should report this
				return VisitResolveResult(rr.Input, returnValueIsImportant);
			}

			throw new NotImplementedException("Conversion " + rr.Conversion + " is not implemented");
		}

		public override JsExpression VisitByReferenceResolveResult(ByReferenceResolveResult rr, bool returnValueIsImportant) {
			return VisitResolveResult(rr.ElementResult, returnValueIsImportant);
			// Should be like below, but that requires us to implement object construction first.
			// throw new InvalidOperationException("Resolve result " + rr.ToString() + " should have been handled in method call.");
		}

		public override JsExpression VisitDefaultResolveResult(ResolveResult rr, bool returnValueIsImportant) {
			if (rr.Type.Kind == TypeKind.Null)
				return JsExpression.Null;
			throw new NotImplementedException("Resolve result " + rr + " is not handled.");
		}
	}
}
