using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler {
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
		private readonly Func<IType, LocalResolveResult> _createTemporaryVariable;
		private readonly Func<string, bool> _isVariableTemporary;

		public class Result {
			public JsExpression Expression { get; set; }
			public ReadOnlyCollection<JsStatement> AdditionalStatements { get; private set; }

			public Result(JsExpression expression, IEnumerable<JsStatement> additionalStatements) {
				this.Expression           = expression;
				this.AdditionalStatements = additionalStatements.AsReadOnly();
			}
		}

		public ExpressionCompiler(ICompilation compilation, INamingConventionResolver namingConvention, IRuntimeLibrary runtimeLibrary, IErrorReporter errorReporter, IDictionary<IVariable, VariableData> variables, Func<IType, LocalResolveResult> createTemporaryVariable, Func<string, bool> isVariableTemporary) {
			_compilation = compilation;
			_namingConvention = namingConvention;
			_runtimeLibrary = runtimeLibrary;
			_errorReporter = errorReporter;
			_variables = variables;
			_createTemporaryVariable = createTemporaryVariable;
			_isVariableTemporary = isVariableTemporary;
		}

		private List<JsStatement> _additionalStatements;

		public Result Compile(ResolveResult expression, bool returnValueIsImportant) {
			_additionalStatements = new List<JsStatement>();
			var expr = VisitResolveResult(expression, returnValueIsImportant);
			return new Result(expr, _additionalStatements);
		}

		private Result CloneAndCompile(ResolveResult expression, bool returnValueIsImportant) {
			return new ExpressionCompiler(_compilation, _namingConvention, _runtimeLibrary, _errorReporter, _variables, _createTemporaryVariable, _isVariableTemporary).Compile(expression, returnValueIsImportant);
		}

		private bool IsExpressionInvariantToOrder(JsExpression expression) {
			if (expression is JsIdentifierExpression && _isVariableTemporary(((JsIdentifierExpression)expression).Name))
				return true;	// Don't have to reorder expressions which only contain a temporary variable since noone is going to change the value of that variable. This check is important to get sensible results if using this method multiple times on the same list.
			else if (expression is JsThisExpression || expression is JsTypeReferenceExpression)
				return true;
			else
				return false;
		}

		private JsExpression InnerCompile(ResolveResult rr, bool usedMultipleTimes, IList<JsExpression> expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression) {
			var result = CloneAndCompile(rr, true);

			bool needsTemporary = usedMultipleTimes && IsJsExpressionComplexEnoughToGetATemporaryVariable.Process(result.Expression);
			if (result.AdditionalStatements.Count > 0 || needsTemporary) {
				// We have to ensure that everything is ordered correctly. First ensure that all expressions that have to be evaluated first actually are evaluated first.
				for (int i = 0; i < expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression.Count; i++) {
					if (IsExpressionInvariantToOrder(expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression[i]))
						continue;
					var temp = _createTemporaryVariable(_compilation.FindType(KnownTypeCode.Object));
					_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp.Variable].Name, expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression[i]));
					expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression[i] = JsExpression.Identifier(_variables[temp.Variable].Name);
				}
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

		private JsExpression CompileMethodCall(MethodImplOptions impl, JsExpression target, IEnumerable<JsExpression> arguments) {
			// TODO
			if (impl == null) {
				return JsExpression.Invocation(target, arguments);	// Used for delegate invocations.
			}
			else {
				switch (impl.Type) {
					case MethodImplOptions.ImplType.NormalMethod:
						return JsExpression.Invocation(JsExpression.MemberAccess(target, impl.Name), arguments);
					case MethodImplOptions.ImplType.NativeIndexer:
						return JsExpression.Index(target, arguments.Single());
					default:
						throw new NotImplementedException();
				}
			}
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
								var expressions = new List<JsExpression>();
								expressions.Add(InnerCompile(mrr.TargetResult, true, expressions));
								if (property.IsIndexer) {
									var indexerInvocation = (CSharpInvocationResolveResult)target;
									foreach (var a in indexerInvocation.Arguments) {
										expressions.Add(InnerCompile(a, true, expressions));
									}
								}

								JsExpression oldValue, jsOtherOperand;
								if (oldValueIsImportant) {
									expressions.Add(CompileMethodCall(impl.GetMethod, expressions[0], expressions.Skip(1)));
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
									if (IsJsExpressionComplexEnoughToGetATemporaryVariable.Process(valueToReturn)) {
										var temp = _createTemporaryVariable(target.Type);
										_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp.Variable].Name, valueToReturn));
										valueToReturn = JsExpression.Identifier(_variables[temp.Variable].Name);
									}

									var newValue = (returnValueBeforeChange ? valueFactory(valueToReturn, jsOtherOperand) : valueToReturn);

									_additionalStatements.Add(new JsExpressionStatement(CompileMethodCall(impl.SetMethod, expressions[0], expressions.Skip(1).Concat(new[] { newValue }))));
									return valueToReturn;
								}
								else {
									return CompileMethodCall(impl.SetMethod, expressions[0], expressions.Skip(1).Concat(new[] { valueFactory(oldValue, jsOtherOperand) }));
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
					var jsTarget = InnerCompile(target.TargetResult, false);
					var jsValue  = InnerCompile(value, false, ref jsTarget);
					return CompileMethodCall(isAdd ? impl.AddMethod : impl.RemoveMethod, jsTarget, new[] { jsValue });
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
					if (rr.Operands[0] is MemberResolveResult && ((MemberResolveResult)rr.Operands[0]).Member is IEvent)
						return CompileEventAddOrRemove((MemberResolveResult)rr.Operands[0], rr.Operands[1], true);
					else
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.AddAssign, JsExpression.Add, returnValueIsImportant, rr.IsLiftedOperator);

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
					if (rr.Operands[0] is MemberResolveResult && ((MemberResolveResult)rr.Operands[0]).Member is IEvent)
						return CompileEventAddOrRemove((MemberResolveResult)rr.Operands[0], rr.Operands[1], false);
					else
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.SubtractAssign, JsExpression.Subtract, returnValueIsImportant, rr.IsLiftedOperator);

				case ExpressionType.PreIncrementAssign:
					return CompileCompoundAssignment(rr.Operands[0], null, (a, b) => JsExpression.PrefixPlusPlus(a), (a, b) => JsExpression.Add(a, JsExpression.Number(1)), returnValueIsImportant, IsNullableType(rr.Operands[0].Type));

				case ExpressionType.PreDecrementAssign:
					return CompileCompoundAssignment(rr.Operands[0], null, (a, b) => JsExpression.PrefixMinusMinus(a), (a, b) => JsExpression.Subtract(a, JsExpression.Number(1)), returnValueIsImportant, IsNullableType(rr.Operands[0].Type));

				case ExpressionType.PostIncrementAssign:
					return CompileCompoundAssignment(rr.Operands[0], null, (a, b) => JsExpression.PostfixPlusPlus(a), (a, b) => JsExpression.Add(a, JsExpression.Number(1)), returnValueIsImportant, IsNullableType(rr.Operands[0].Type), returnValueBeforeChange: true);

				case ExpressionType.PostDecrementAssign:
					return CompileCompoundAssignment(rr.Operands[0], null, (a, b) => JsExpression.PostfixMinusMinus(a), (a, b) => JsExpression.Subtract(a, JsExpression.Number(1)), returnValueIsImportant, IsNullableType(rr.Operands[0].Type), returnValueBeforeChange: true);

				// Binary non-assigning operators

				case ExpressionType.Add:
				case ExpressionType.AddChecked:
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
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Subtract, rr.IsLiftedOperator);

				// Unary operators

				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
					return CompileUnaryOperator(rr.Operands[0], JsExpression.Negate, IsNullableType(rr.Operands[0].Type));

				case ExpressionType.UnaryPlus:
					return CompileUnaryOperator(rr.Operands[0], JsExpression.Positive, IsNullableType(rr.Operands[0].Type));

				case ExpressionType.Not:
					return CompileUnaryOperator(rr.Operands[0], JsExpression.LogicalNot, IsNullableType(rr.Operands[0].Type));

				case ExpressionType.OnesComplement:
					return CompileUnaryOperator(rr.Operands[0], JsExpression.BitwiseNot, IsNullableType(rr.Operands[0].Type));

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

		// WIP

		public override JsExpression VisitMemberResolveResult(MemberResolveResult rr, bool returnValueIsImportant) {
			var jsTarget = InnerCompile(rr.TargetResult, true);
			if (rr.Member is IProperty) {
				var impl = _namingConvention.GetPropertyImplementation((IProperty)rr.Member);
				switch (impl.Type) {
					case PropertyImplOptions.ImplType.GetAndSetMethods:
						return CompileMethodCall(impl.GetMethod, jsTarget, new JsExpression[0]);	// We know we have no arguments because indexers are treated as invocations.
					case PropertyImplOptions.ImplType.Field:
						return JsExpression.MemberAccess(jsTarget, impl.FieldName);
					default:
						_errorReporter.Error("Property " + rr.Member.DeclaringType + "." + rr.Member.Name + " is not usable from script.");
						return JsExpression.Number(0);
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

		private JsExpression CompileMethodInvocation(MethodImplOptions impl, ResolveResult target, IList<ResolveResult> arguments, IEnumerable<IType> typeArguments, IList<int> argumentToParameterMap) {
			var expressions = new List<JsExpression>();
			expressions.Add(InnerCompile(target, false));
			foreach (var a in arguments) {
				expressions.Add(InnerCompile(a, false, expressions));
			}
			return CompileMethodCall(impl, expressions[0], expressions.Skip(1));
		}

		public override JsExpression VisitCSharpInvocationResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.CSharpInvocationResolveResult rr, bool returnValueIsImportant) {
			// Note: This might also represent a constructor.
			var arguments = rr.Arguments.Select(a => VisitResolveResult(a, true));
			if (rr.Member is IMethod) {
				// TODO: This one might require argument reordering and default argument evaluation
				if (rr.Member.Name == "Invoke" && IsDelegateType(rr.Member.DeclaringType)) {
					// Invoke the underlying method instead of calling the Invoke method.
					return CompileMethodInvocation(null, rr.TargetResult, rr.Arguments, new IType[0], rr.GetArgumentToParameterMap());
				}
				else {
					var method = (IMethod)rr.Member;
					if (method.IsConstructor) {
						return JsExpression.New(new JsTypeReferenceExpression(rr.Member.DeclaringType.GetDefinition()), arguments); // Only temporary - Promised to be fixed in NR
					}
					else {
						return CompileMethodInvocation(_namingConvention.GetMethodImplementation((IMethod)rr.Member), rr.TargetResult, rr.Arguments, new IType[0], rr.GetArgumentToParameterMap());
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
				var expressions = new List<JsExpression>() { InnerCompile(rr.TargetResult, false) };
				foreach (var arg in rr.Arguments)
					expressions.Add(InnerCompile(arg, false, expressions));
				return CompileMethodCall(impl.GetMethod, expressions[0], expressions.Skip(1));
			}
			else {
				throw new InvalidOperationException("Invocation of unsupported member " + rr.Member.DeclaringType.FullName + "." + rr.Member.Name);
			}
		}

		// /WIP


		// TODO: Methods below are UNTESTED and REALLY hacky, but needed for the statement compiler

		public override JsExpression VisitConstantResolveResult(ConstantResolveResult rr, bool returnValueIsImportant) {
			if (rr.ConstantValue is string)
				return JsExpression.String((string)rr.ConstantValue);
			else if (rr.ConstantValue is int)
				return JsExpression.Number((int)rr.ConstantValue);
			else if (rr.ConstantValue is bool)
				return (bool)rr.ConstantValue ? JsExpression.True : JsExpression.False;
			else
				return JsExpression.Null;
		}

		public override JsExpression VisitLocalResolveResult(LocalResolveResult rr, bool returnValueIsImportant) {
			// Only other thing we have to take care of now is if we're accessing a byref variable declared in a parent function, in which case we'd have to return this.variable.$
			var data = _variables[rr.Variable];
			var ident = JsExpression.Identifier(_variables[rr.Variable].Name);
			if (data.UseByRefSemantics)
				return JsExpression.MemberAccess(ident, "$");
			else
				return ident;
		}

		public override JsExpression VisitThisResolveResult(ThisResolveResult rr, bool returnValueIsImportant) {
			return JsExpression.This;
		}

		public override JsExpression VisitConversionResolveResult(ConversionResolveResult rr, bool returnValueIsImportant) {
			if (rr.Conversion.IsIdentityConversion) {
				return VisitResolveResult(rr.Input, returnValueIsImportant);
			}
			if (rr.Conversion.IsTryCast) {
				return _runtimeLibrary.TryCast(VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			else if (rr.Conversion.IsReferenceConversion) {
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
					jsMethod = JsExpression.MemberAccess(VisitResolveResult(new TypeResolveResult(mgrr.TargetResult.Type), true), impl.Name);
				}
				else {
					jsTarget = InnerCompile(mgrr.TargetResult, true);
					jsMethod = JsExpression.MemberAccess(jsTarget, impl.Name);
				}

				if (rr.Conversion.Method is SpecializedMethod && !impl.IgnoreGenericArguments) {
					jsMethod = _runtimeLibrary.InstantiateGenericMethod(jsMethod, ((SpecializedMethod)rr.Conversion.Method).TypeArguments.Select(a => VisitResolveResult(new TypeResolveResult(a), true)));
				}

				return jsTarget != null ? _runtimeLibrary.Bind(jsMethod, jsTarget) : jsMethod;
			}
			else if (rr.Conversion.IsImplicit) {
				// Null literal conversion have no property, should report this
				return VisitResolveResult(rr.Input, returnValueIsImportant);
			}

			throw new NotImplementedException("Conversion " + rr.Conversion + " is not implemented");
		}

		public override JsExpression VisitArrayCreateResolveResult(ArrayCreateResolveResult rr, bool returnValueIsImportant) {
			return JsExpression.ArrayLiteral(rr.InitializerElements.Select(e => VisitResolveResult(e, true)));
		}

		public override JsExpression VisitByReferenceResolveResult(ByReferenceResolveResult rr, bool returnValueIsImportant) {
			return VisitResolveResult(rr.ElementResult, true);
		}

		public override JsExpression VisitArrayAccessResolveResult(ArrayAccessResolveResult rr, bool returnValueIsImportant) {
			if (rr.Indexes.Count != 1)
				_errorReporter.Error("Arrays have to be one-dimensional");
			var array = InnerCompile(rr.Array, false);
			var index = InnerCompile(rr.Indexes[0], false, ref array);
			return JsExpression.Index(array, index);
		}

		public override JsExpression VisitDefaultResolveResult(ResolveResult rr, bool returnValueIsImportant) {
			if (rr.Type.Kind == TypeKind.Null)
				return JsExpression.Null;
			throw new NotImplementedException("Resolve result " + rr + " is not handled.");
		}

		public override JsExpression VisitLambdaResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.LambdaResolveResult rr, bool returnValueIsImportant) {
			return JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.Number(0))); 
		}

		public override JsExpression VisitTypeOfResolveResult(TypeOfResolveResult rr, bool returnValueIsImportant) {
			throw new NotImplementedException();
		}

		public override JsExpression VisitTypeResolveResult(TypeResolveResult rr, bool returnValueIsImportant) {
			return new JsTypeReferenceExpression(rr.Type.GetDefinition());
		}

        public override JsExpression VisitTypeIsResolveResult(TypeIsResolveResult rr, bool returnValueIsImportant) {
			return _runtimeLibrary.TypeIs(VisitResolveResult(rr.Input, returnValueIsImportant), new JsTypeReferenceExpression(rr.TargetType.GetDefinition()));
        }
	}
}
