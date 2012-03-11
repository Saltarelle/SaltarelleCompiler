using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
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

		private bool IsExpressionInvariantToOrder(JsExpression expression) {
			if (expression is JsIdentifierExpression && _isVariableTemporary(((JsIdentifierExpression)expression).Name))
				return true;	// Don't have to reorder expressions which only contain a temporary variable since noone is going to change the value of that variable. This check is important to get sensible results if using this method multiple times on the same list.
			else if (expression is JsThisExpression || expression is JsTypeReferenceExpression)
				return true;
			else
				return false;
		}

		private JsExpression InnerCompile(ResolveResult rr, bool usedMultipleTimes, IList<JsExpression> expressionsThatHaveToBeEvaluatedInOrderBeforeThisExpression) {
			var result = new ExpressionCompiler(_compilation, _namingConvention, _runtimeLibrary, _errorReporter, _variables, _createTemporaryVariable, _isVariableTemporary).Compile(rr, true);

			bool needsTemporary = usedMultipleTimes && IsJsExpressionComplexEnoughToGetATemporaryVariable.Process(result.Expression);
			if (result.AdditionalStatements.Count > 0 || needsTemporary || DoesJsExpressionHaveSideEffects.Process(result.Expression)) {
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
			switch (impl.Type) {
				case MethodImplOptions.ImplType.InstanceMethod:
				case MethodImplOptions.ImplType.StaticMethod:
					return JsExpression.Invocation(JsExpression.MemberAccess(target, impl.Name), arguments);
				default:
					throw new NotImplementedException();
			}
		}

		private bool IsIntegerType(IType type) {
			if (type.GetDefinition().Equals(_compilation.FindType(KnownTypeCode.NullableOfT)))
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
			if (type.GetDefinition().Equals(_compilation.FindType(KnownTypeCode.NullableOfT)))
				type = ((ParameterizedType)type).TypeArguments[0];

			return type.Equals(_compilation.FindType(KnownTypeCode.Byte))
			    || type.Equals(_compilation.FindType(KnownTypeCode.UInt16))
				|| type.Equals(_compilation.FindType(KnownTypeCode.UInt32))
				|| type.Equals(_compilation.FindType(KnownTypeCode.UInt64));
		}

		private JsExpression CompilePropertySetter(IProperty property, MemberResolveResult target, ResolveResult value, bool returnValueIsImportant) {
			var impl = _namingConvention.GetPropertyImplementation(property);

			var expressions = new List<JsExpression>();
			expressions.Add(VisitResolveResult(target.TargetResult, true));
			if (property.IsIndexer) {
				var indexerInvocation = (CSharpInvocationResolveResult)target;
				foreach (var a in indexerInvocation.Arguments) {
					expressions.Add(InnerCompile(a, false, expressions));
				}
			}

			var jsValue = InnerCompile(value, returnValueIsImportant, expressions);

			switch (impl.Type) {
				case PropertyImplOptions.ImplType.GetAndSetMethods: {
					var setter = CompileMethodCall(impl.SetMethod, expressions[0], expressions.Skip(1).Concat(new[] { jsValue }));
					if (returnValueIsImportant) {
						_additionalStatements.Add(new JsExpressionStatement(setter));
						return jsValue;
					}
					else {
						return setter;
					}
				}

				case PropertyImplOptions.ImplType.Field: {
					if (expressions.Count != 1) {
						_errorReporter.Error("Property " + property.DeclaringType.FullName + "." + property.Name + ", declared as being a field, is an indexer.");
						return JsExpression.Number(0);
					}
					return JsExpression.Assign(JsExpression.MemberAccess(expressions[0], impl.FieldName), jsValue);
				}

				case PropertyImplOptions.ImplType.NativeIndexer: {
					if (expressions.Count != 2) {
						_errorReporter.Error("Property " + property.DeclaringType.FullName + "." + property.Name + ", declared as being a native indexer, does not have exactly one argument.");
						return JsExpression.Number(0);
					}
					return JsExpression.Assign(JsExpression.Index(expressions[0], expressions[1]), jsValue);
				}

				default: {
					_errorReporter.Error("Cannot use property " + property.DeclaringType.FullName + "." + property.Name + " from script.");
					return JsExpression.Number(0);
				}
			}
		}

		private JsExpression CompileCompoundAssignment(ResolveResult target, ResolveResult otherOperand, Func<JsExpression, JsExpression, JsExpression> compoundFactory, Func<JsExpression, JsExpression, JsExpression> valueFactory, bool returnValueIsImportant, bool isLifted) {
			if (isLifted) {
				compoundFactory = null;
				var oldVF       = valueFactory;
				valueFactory    = (a, b) => _runtimeLibrary.Lift(_compilation, oldVF(a, b));
			}

			if (target is MemberResolveResult) {
				var mrr = (MemberResolveResult)target;

				if (mrr.Member is IProperty) {
					var property = ((MemberResolveResult)target).Member as IProperty;
					var impl = _namingConvention.GetPropertyImplementation(property);

					switch (impl.Type) {
						case PropertyImplOptions.ImplType.GetAndSetMethods: {
							var expressions = new List<JsExpression>();
							expressions.Add(InnerCompile(mrr.TargetResult, true, expressions));
							if (property.IsIndexer) {
								var indexerInvocation = (CSharpInvocationResolveResult)target;
								foreach (var a in indexerInvocation.Arguments) {
									expressions.Add(InnerCompile(a, true, expressions));
								}
							}

							expressions.Add(CompileMethodCall(impl.GetMethod, expressions[0], expressions.Skip(1)));
							var jsOtherOperand = InnerCompile(otherOperand, false, expressions);
							var newValue = valueFactory(expressions[expressions.Count - 1], jsOtherOperand);
							expressions.RemoveAt(expressions.Count - 1); // Remove the current value because it should not be an argument to the setter.

							if (returnValueIsImportant) {
								if (IsJsExpressionComplexEnoughToGetATemporaryVariable.Process(newValue)) {
									var temp = _createTemporaryVariable(target.Type);
									_additionalStatements.Add(new JsVariableDeclarationStatement(_variables[temp.Variable].Name, newValue));
									newValue = JsExpression.Identifier(_variables[temp.Variable].Name);
								}
								_additionalStatements.Add(new JsExpressionStatement(CompileMethodCall(impl.SetMethod, expressions[0], expressions.Skip(1).Concat(new[] { newValue }))));
								return newValue;
							}
							else {
								return CompileMethodCall(impl.SetMethod, expressions[0], expressions.Skip(1).Concat(new[] { newValue }));
							}
						}

						case PropertyImplOptions.ImplType.Field: {
							var jsTarget = InnerCompile(mrr.TargetResult, compoundFactory == null);
							var jsOtherOperand = InnerCompile(otherOperand, false, ref jsTarget);
							if (compoundFactory != null)
								return compoundFactory(JsExpression.MemberAccess(jsTarget, impl.FieldName), jsOtherOperand);
							else
								return JsExpression.Assign(JsExpression.MemberAccess(jsTarget, impl.FieldName), valueFactory(JsExpression.MemberAccess(jsTarget, impl.FieldName), jsOtherOperand));
						}

						case PropertyImplOptions.ImplType.NativeIndexer: {
							if (!property.IsIndexer || property.Getter.Parameters.Count != 1) {
								_errorReporter.Error("Property " + property.DeclaringType.FullName + "." + property.Name + ", declared as being a native indexer, is not an indexer with exactly one argument.");
								return JsExpression.Number(0);
							}

							var expressions = new List<JsExpression>();
							expressions.Add(InnerCompile(mrr.TargetResult, compoundFactory == null, expressions));
							foreach (var a in ((CSharpInvocationResolveResult)target).Arguments) {
								expressions.Add(InnerCompile(a, compoundFactory == null, expressions));
							}

							var jsOtherOperand = InnerCompile(otherOperand, false, expressions);

							if (compoundFactory != null)
								return compoundFactory(JsExpression.Index(expressions[0], expressions[1]), jsOtherOperand);
							else
								return JsExpression.Assign(JsExpression.Index(expressions[0], expressions[1]), valueFactory(JsExpression.Index(expressions[0], expressions[1]), jsOtherOperand));
						}

						default: {
							_errorReporter.Error("Cannot use property " + property.DeclaringType.FullName + "." + property.Name + " from script.");
							return JsExpression.Number(0);
						}
					}
				}
				else if (mrr.Member is IField) {
					var jsTarget = InnerCompile(mrr.TargetResult, true);
					var jsOtherOperand = InnerCompile(otherOperand, false, ref jsTarget);

					var field = (IField)mrr.Member;
					var impl = _namingConvention.GetFieldImplementation(field);
					if (impl.Type == FieldImplOptions.ImplType.Field) {
						if (compoundFactory != null)
							return compoundFactory(JsExpression.MemberAccess(jsTarget, impl.Name), jsOtherOperand);
						else
							return JsExpression.Assign(JsExpression.MemberAccess(jsTarget, impl.Name), valueFactory(JsExpression.MemberAccess(jsTarget, impl.Name), jsOtherOperand));
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
				var jsTarget = InnerCompile(target, true);
				var jsOtherOperand = InnerCompile(otherOperand, false, ref jsTarget);
				if (compoundFactory != null)
					return compoundFactory(jsTarget, jsOtherOperand);
				else
					return JsExpression.Assign(jsTarget, valueFactory(jsTarget, jsOtherOperand));
			}
			else {
				_errorReporter.Error("Unsupported target of compound assignment: " + target.ToString());
				return JsExpression.Number(0);
			}
		}

		private JsExpression CompileBinaryNonAssigningOperator(ResolveResult left, ResolveResult right, Func<JsExpression, JsExpression, JsExpression> resultFactory) {
			var jsLeft  = InnerCompile(left, false);
			var jsRight = InnerCompile(right, false, ref jsLeft);
			return resultFactory(jsLeft, jsRight);
		}

		public override JsExpression VisitOperatorResolveResult(OperatorResolveResult rr, bool returnValueIsImportant) {
			switch (rr.OperatorType) {
				case ExpressionType.Assign:
					if (rr.Operands[0] is MemberResolveResult && ((MemberResolveResult)rr.Operands[0]).Member is IProperty) {
						var mrr = (MemberResolveResult)rr.Operands[0];
						return CompilePropertySetter((IProperty)mrr.Member, mrr, rr.Operands[1], returnValueIsImportant);
					}
					else {
						return JsExpression.Assign(VisitResolveResult(rr.Operands[0], true), VisitResolveResult(rr.Operands[1], true));
					}

				// Compound assignment operators
				case ExpressionType.AddAssign:
				case ExpressionType.AddAssignChecked:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.AddAssign, JsExpression.Add, returnValueIsImportant, rr.Type.GetDefinition() == _compilation.FindType(KnownTypeCode.NullableOfT));

				case ExpressionType.AndAssign:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseAndAssign, JsExpression.BitwiseAnd, returnValueIsImportant, rr.Type.GetDefinition() == _compilation.FindType(KnownTypeCode.NullableOfT));

				case ExpressionType.DivideAssign:
					if (IsIntegerType(rr.Type))
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], null, (a, b) => _runtimeLibrary.IntegerDivision(_compilation, a, b), returnValueIsImportant, rr.Type.GetDefinition() == _compilation.FindType(KnownTypeCode.NullableOfT));
					else
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.DivideAssign, JsExpression.Divide, returnValueIsImportant, rr.Type.GetDefinition() == _compilation.FindType(KnownTypeCode.NullableOfT));

				case ExpressionType.ExclusiveOrAssign:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseXOrAssign, JsExpression.BitwiseXor, returnValueIsImportant, rr.Type.GetDefinition() == _compilation.FindType(KnownTypeCode.NullableOfT));

				case ExpressionType.LeftShiftAssign:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.LeftShiftAssign, JsExpression.LeftShift, returnValueIsImportant, rr.Type.GetDefinition() == _compilation.FindType(KnownTypeCode.NullableOfT));

				case ExpressionType.ModuloAssign:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.ModuloAssign, JsExpression.Modulo, returnValueIsImportant, rr.Type.GetDefinition() == _compilation.FindType(KnownTypeCode.NullableOfT));

				case ExpressionType.MultiplyAssign:
				case ExpressionType.MultiplyAssignChecked:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.MultiplyAssign, JsExpression.Multiply, returnValueIsImportant, rr.Type.GetDefinition() == _compilation.FindType(KnownTypeCode.NullableOfT));

				case ExpressionType.OrAssign:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseOrAssign, JsExpression.BitwiseOr, returnValueIsImportant, rr.Type.GetDefinition() == _compilation.FindType(KnownTypeCode.NullableOfT));

				case ExpressionType.RightShiftAssign:
					if (IsUnsignedType(rr.Type))
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.RightShiftUnsignedAssign, JsExpression.RightShiftUnsigned, returnValueIsImportant, rr.Type.GetDefinition() == _compilation.FindType(KnownTypeCode.NullableOfT));
					else
						return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.RightShiftSignedAssign, JsExpression.RightShiftSigned, returnValueIsImportant, rr.Type.GetDefinition() == _compilation.FindType(KnownTypeCode.NullableOfT));

				case ExpressionType.SubtractAssign:
				case ExpressionType.SubtractAssignChecked:
					return CompileCompoundAssignment(rr.Operands[0], rr.Operands[1], JsExpression.SubtractAssign, JsExpression.Subtract, returnValueIsImportant, rr.Type.GetDefinition() == _compilation.FindType(KnownTypeCode.NullableOfT));

				// Binary non-assigning operators
				case ExpressionType.Add:
				case ExpressionType.AddChecked:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Add);

				case ExpressionType.And:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseAnd);

				case ExpressionType.AndAlso:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.LogicalAnd);

				case ExpressionType.Coalesce:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => _runtimeLibrary.Coalesce(_compilation, a, b));

				case ExpressionType.Divide:
					if (IsIntegerType(rr.Type))
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], (a, b) => _runtimeLibrary.IntegerDivision(_compilation, a, b));
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Divide);

				case ExpressionType.ExclusiveOr:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseXor);

				case ExpressionType.GreaterThan:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Greater);

				case ExpressionType.GreaterThanOrEqual:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.GreaterOrEqual);

				case ExpressionType.Equal:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Equal);

				case ExpressionType.LeftShift:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.LeftShift);

				case ExpressionType.LessThan:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Lesser);

				case ExpressionType.LessThanOrEqual:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.LesserOrEqual);

				case ExpressionType.Modulo:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Modulo);

				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Multiply);

				case ExpressionType.NotEqual:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.NotEqual);

				case ExpressionType.Or:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.BitwiseOr);

				case ExpressionType.OrElse:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.LogicalOr);

				case ExpressionType.RightShift:
					if (IsUnsignedType(rr.Type))
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.RightShiftUnsignedAssign);
					else
						return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.RightShiftSignedAssign);

				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
					return CompileBinaryNonAssigningOperator(rr.Operands[0], rr.Operands[1], JsExpression.Subtract);

				// TODO Not finished
				case ExpressionType.PreIncrementAssign:
				case ExpressionType.PostIncrementAssign:
				case ExpressionType.PreDecrementAssign:
				case ExpressionType.PostDecrementAssign:
					return JsExpression.PostfixPlusPlus(VisitResolveResult(rr.Operands[0], true));

				case ExpressionType.ArrayLength:
				case ExpressionType.ArrayIndex:
				case ExpressionType.Call:
				case ExpressionType.Conditional:
				case ExpressionType.Constant:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.Invoke:
				case ExpressionType.Lambda:
				case ExpressionType.ListInit:
				case ExpressionType.MemberAccess:
				case ExpressionType.MemberInit:
				case ExpressionType.Negate:
				case ExpressionType.UnaryPlus:
				case ExpressionType.NegateChecked:
				case ExpressionType.New:
				case ExpressionType.NewArrayInit:
				case ExpressionType.NewArrayBounds:
				case ExpressionType.Not:
				case ExpressionType.Parameter:
				case ExpressionType.Quote:
				case ExpressionType.TypeAs:
				case ExpressionType.TypeIs:
				case ExpressionType.Block:
				case ExpressionType.DebugInfo:
				case ExpressionType.Decrement:
				case ExpressionType.Dynamic:
				case ExpressionType.Default:
				case ExpressionType.Extension:
				case ExpressionType.Goto:
				case ExpressionType.Increment:
				case ExpressionType.Index:
				case ExpressionType.Label:
				case ExpressionType.RuntimeVariables:
				case ExpressionType.Loop:
				case ExpressionType.Switch:
				case ExpressionType.Throw:
				case ExpressionType.Try:
				case ExpressionType.Unbox:
				case ExpressionType.TypeEqual:
				case ExpressionType.OnesComplement:
				case ExpressionType.IsTrue:
				case ExpressionType.IsFalse:
					throw new NotImplementedException();
				case ExpressionType.Power:
				case ExpressionType.PowerAssign:
				default:
					throw new ArgumentException("Unsupported operator " + rr.OperatorType);
			}
		}


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
			return JsExpression.Identifier(_variables[rr.Variable].Name);	// Not really, this (might, or it might happen in the step above it) needs to take care of by-ref variables.
		}

		public override JsExpression VisitCSharpInvocationResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.CSharpInvocationResolveResult rr, bool returnValueIsImportant) {
			// Note: This might also represent a constructor.
			var arguments = rr.Arguments.Select(a => VisitResolveResult(a, true));
			if (rr.Member is IMethod && ((IMethod)rr.Member).IsConstructor) {
				return JsExpression.New(new JsTypeReferenceExpression(rr.Member.DeclaringType.GetDefinition()), arguments);
			}
			else {
				return JsExpression.Invocation(JsExpression.MemberAccess(rr.TargetResult != null ? VisitResolveResult(rr.TargetResult, true) : new JsTypeReferenceExpression(rr.Member.DeclaringType.GetDefinition()), rr.Member.Name), arguments);
			}
		}

		public override JsExpression VisitThisResolveResult(ThisResolveResult rr, bool returnValueIsImportant) {
			return JsExpression.This;
		}

		public override JsExpression VisitMemberResolveResult(MemberResolveResult rr, bool returnValueIsImportant) {
			if (rr.Member is IProperty) {
				return JsExpression.Invocation(JsExpression.MemberAccess(VisitResolveResult(rr.TargetResult, false), "get_" + rr.Member.Name));
			}
			else if (rr.Member is IField) {
				var impl = _namingConvention.GetFieldImplementation((IField)rr.Member);
				return JsExpression.MemberAccess(VisitResolveResult(rr.TargetResult, false), impl.Name);
			}
			return base.VisitMemberResolveResult(rr, returnValueIsImportant);
		}

		public override JsExpression VisitConversionResolveResult(ConversionResolveResult rr, bool returnValueIsImportant) {
			if (rr.Conversion.IsIdentityConversion) {
				return VisitResolveResult(rr.Input, returnValueIsImportant);
			}
			if (rr.Conversion.IsTryCast) {
				return _runtimeLibrary.TryCast(_compilation, VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			else if (rr.Conversion.IsReferenceConversion) {
				if (rr.Conversion.IsImplicit)
					return _runtimeLibrary.ImplicitReferenceConversion(_compilation, VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
				else
					return _runtimeLibrary.Cast(_compilation, VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			else if (rr.Conversion.IsNumericConversion) {
				return VisitResolveResult(rr.Input, returnValueIsImportant);
			}
			else if (rr.Conversion.IsDynamicConversion) {
				return _runtimeLibrary.Cast(_compilation, VisitResolveResult(rr.Input, true), new JsTypeReferenceExpression(rr.Type.GetDefinition()));
			}
			else if (rr.Conversion.IsNullableConversion) {
				return VisitResolveResult(rr.Input, returnValueIsImportant);
			}
			throw new NotImplementedException();
		}

		public override JsExpression VisitArrayCreateResolveResult(ArrayCreateResolveResult rr, bool returnValueIsImportant) {
			return JsExpression.ArrayLiteral(rr.InitializerElements.Select(e => VisitResolveResult(e, true)));
		}

		public override JsExpression VisitByReferenceResolveResult(ByReferenceResolveResult rr, bool returnValueIsImportant) {
			return VisitResolveResult(rr.ElementResult, true);
		}

		public override JsExpression VisitArrayAccessResolveResult(ArrayAccessResolveResult rr, bool returnValueIsImportant) {
			throw new NotImplementedException();
		}

		public override JsExpression VisitDefaultResolveResult(ResolveResult rr, bool returnValueIsImportant) {
			if (rr.Type.Kind == TypeKind.Null)
				return JsExpression.Null;
			throw new NotImplementedException();
		}

		public override JsExpression VisitInvocationResolveResult(InvocationResolveResult rr, bool returnValueIsImportant) {
			throw new NotImplementedException();
		}

		public override JsExpression VisitLambdaResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.LambdaResolveResult rr, bool returnValueIsImportant) {
			return JsExpression.FunctionDefinition(new string[0], new JsReturnStatement(JsExpression.Number(0))); 
		}

		public override JsExpression VisitMethodGroupResolveResult(ICSharpCode.NRefactory.CSharp.Resolver.MethodGroupResolveResult rr, bool returnValueIsImportant) {
			var m = rr.Methods.First();
			return JsExpression.MemberAccess(new JsTypeReferenceExpression(m.DeclaringType.GetDefinition()), m.Name);
		}

		public override JsExpression VisitTypeOfResolveResult(TypeOfResolveResult rr, bool returnValueIsImportant) {
			throw new NotImplementedException();
		}

		public override JsExpression VisitTypeResolveResult(TypeResolveResult rr, bool returnValueIsImportant) {
			return new JsTypeReferenceExpression(rr.Type.GetDefinition());
		}

        public override JsExpression VisitTypeIsResolveResult(TypeIsResolveResult rr, bool returnValueIsImportant) {
			return _runtimeLibrary.TypeIs(_compilation, VisitResolveResult(rr.Input, returnValueIsImportant), new JsTypeReferenceExpression(rr.TargetType.GetDefinition()));
        }
	}
}
