using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.RuntimeLibrary {
	public class ScriptSharpRuntimeLibrary : IRuntimeLibrary {
		private readonly INamingConventionResolver _namingConvention;
		private readonly Func<ITypeReference, JsExpression> _createTypeReferenceExpression;

		public ScriptSharpRuntimeLibrary(INamingConventionResolver namingConvention, Func<ITypeReference, JsExpression> createTypeReferenceExpression) {
			_namingConvention = namingConvention;
			_createTypeReferenceExpression = createTypeReferenceExpression;
		}

		public JsExpression GetScriptType(IType type, bool returnOpenType) {
			if (type.TypeParameterCount > 0 && !(type is ParameterizedType) && returnOpenType) {
				// This handles open generic types ( typeof(C<,>) )
				var sem = _namingConvention.GetTypeSemantics(type.GetDefinition());
				return new JsTypeReferenceExpression(type.GetDefinition().ParentAssembly, sem.Type == TypeScriptSemantics.ImplType.NormalType ? sem.Name : "Unusable_type");
			}
			else if (type.Kind == TypeKind.Array) {
				return _createTypeReferenceExpression(KnownTypeReference.Array);
			}
			else if (type.Kind == TypeKind.Delegate) {
				return _createTypeReferenceExpression(KnownTypeReference.Delegate);
			}
			else if (type is ITypeParameter) {
				return JsExpression.Identifier(_namingConvention.GetTypeParameterName((ITypeParameter)type));
			}
			else if (type is ParameterizedType) {
				var pt = (ParameterizedType)type;
				var def = pt.GetDefinition();
				var sem = _namingConvention.GetTypeSemantics(def);
				if (sem.Type == TypeScriptSemantics.ImplType.NormalType && !sem.IgnoreGenericArguments)
					return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Type), "makeGenericType"), new JsTypeReferenceExpression(type.GetDefinition().ParentAssembly, sem.Type == TypeScriptSemantics.ImplType.NormalType ? sem.Name : "Unusable_type"), JsExpression.ArrayLiteral(pt.TypeArguments.Select(a => GetScriptType(a, returnOpenType))));
				else
					return new JsTypeReferenceExpression(def.ParentAssembly, sem.Type == TypeScriptSemantics.ImplType.NormalType ? sem.Name : "Unusable_type");
			}
			else if (type is ITypeDefinition) {
				var td = (ITypeDefinition)type;
				var sem = _namingConvention.GetTypeSemantics(td);
				var jsref = new JsTypeReferenceExpression(td.ParentAssembly, sem.Type == TypeScriptSemantics.ImplType.NormalType ? sem.Name : "Unusable_type");
				if (td.TypeParameterCount > 0) {
					return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Type), "makeGenericType"), new JsTypeReferenceExpression(type.GetDefinition().ParentAssembly, sem.Type == TypeScriptSemantics.ImplType.NormalType ? sem.Name : "Unusable_type"), JsExpression.ArrayLiteral(td.TypeParameters.Select(a => GetScriptType(a, returnOpenType))));
				}
				else {
					return jsref;
				}
			}
			else {
				throw new NotImplementedException();
			}
		}

		public JsExpression TypeIs(JsExpression expression, IType targetType) {
			return JsExpression.Invocation(JsExpression.MemberAccess(GetScriptType(targetType, false), "isInstanceOfType"), expression);
		}

		public JsExpression TryDowncast(JsExpression expression, IType sourceType, IType targetType) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Type), "safeCast"), expression, GetScriptType(targetType, false));
		}

		public JsExpression Downcast(JsExpression expression, IType sourceType, IType targetType) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Type), "cast"), expression, GetScriptType(targetType, false));
		}

		public JsExpression Upcast(JsExpression expression, IType sourceType, IType targetType) {
			return expression;
		}

		public JsExpression InstantiateGenericMethod(JsExpression method, IEnumerable<IType> typeArguments) {
			return JsExpression.Invocation(method, typeArguments.Select(a => GetScriptType(a, false)));
		}

		public JsExpression MakeException(JsExpression operand) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Exception), "wrap"), operand);
		}

		public JsExpression IntegerDivision(JsExpression numerator, JsExpression denominator) {
			// TODO: Obviously not...
			return JsExpression.Binary(ExpressionNodeType.Divide, numerator, denominator);
		}

		public JsExpression FloatToInt(JsExpression operand) {
			throw new NotImplementedException();
		}

		public JsExpression Coalesce(JsExpression a, JsExpression b) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Object), "coalesce"), a, b);
		}

		public JsExpression Lift(JsExpression expression) {
			if (expression is JsUnaryExpression) {
				string methodName = null;
				switch (expression.NodeType) {
					case ExpressionNodeType.LogicalNot: methodName = "not"; goto default;
					case ExpressionNodeType.Negate:     methodName = "neg"; goto default;
					case ExpressionNodeType.Positive:   methodName = "pos"; goto default;
					case ExpressionNodeType.BitwiseNot: methodName = "cpl"; goto default;

					default:
						if (methodName == null)
							throw new ArgumentException("Cannot lift expression " + OutputFormatter.Format(expression, true));
						return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.NullableOfT), methodName), ((JsUnaryExpression)expression).Operand);
				}
			}
			else if (expression is JsBinaryExpression) {
				string methodName = null;
				switch (expression.NodeType) {
					case ExpressionNodeType.Equal:
					case ExpressionNodeType.Same:
					case ExpressionNodeType.NotEqual:
					case ExpressionNodeType.NotSame:
						return expression;

					case ExpressionNodeType.LesserOrEqual:      methodName = "le";   goto default;
					case ExpressionNodeType.GreaterOrEqual:     methodName = "ge";   goto default;
					case ExpressionNodeType.Lesser:             methodName = "lt";   goto default;
					case ExpressionNodeType.Greater:            methodName = "gt";   goto default;
					case ExpressionNodeType.Subtract:           methodName = "sub";  goto default;
					case ExpressionNodeType.Add:                methodName = "add";  goto default;
					case ExpressionNodeType.Modulo:             methodName = "mod";  goto default;
					case ExpressionNodeType.Divide:             methodName = "divf"; goto default;
					case ExpressionNodeType.Multiply:           methodName = "mul";  goto default;
					case ExpressionNodeType.BitwiseAnd:         methodName = "band"; goto default;
					case ExpressionNodeType.BitwiseOr:          methodName = "bor";  goto default;
					case ExpressionNodeType.BitwiseXor:         methodName = "xor";  goto default;
					case ExpressionNodeType.LeftShift:          methodName = "shl";  goto default;
					case ExpressionNodeType.RightShiftSigned:   methodName = "srs";  goto default;
					case ExpressionNodeType.RightShiftUnsigned: methodName = "sru";  goto default;

					default:
						if (methodName == null)
							throw new ArgumentException("Cannot lift expression " + OutputFormatter.Format(expression, true));
						return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.NullableOfT), methodName), ((JsBinaryExpression)expression).Left, ((JsBinaryExpression)expression).Right);
				}
			}
			else {
				throw new ArgumentException("Cannot lift expression " + OutputFormatter.Format(expression, true));
			}
		}

		public JsExpression FromNullable(JsExpression expression) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.NullableOfT), "unbox"), expression);
		}

		public JsExpression LiftedBooleanAnd(JsExpression a, JsExpression b) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.NullableOfT), "and"), a, b);
		}

		public JsExpression LiftedBooleanOr(JsExpression a, JsExpression b) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.NullableOfT), "or"), a, b);
		}

		public JsExpression Bind(JsExpression function, JsExpression target) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Delegate), "mkdel"), target, function);
		}

		public JsExpression Default(IType type) {
			return JsExpression.Invocation(JsExpression.MemberAccess(GetScriptType(type, false), "getDefaultValue"));
		}

		public JsExpression CreateArray(JsExpression size) {
			return JsExpression.New(_createTypeReferenceExpression(KnownTypeReference.Array), size);
		}

		public JsExpression CallBase(IType baseType, string methodName, IList<IType> typeArguments, IEnumerable<JsExpression> thisAndArguments) {
			JsExpression method = JsExpression.MemberAccess(JsExpression.MemberAccess(GetScriptType(baseType, false), "prototype"), methodName);
			
			if (typeArguments != null && typeArguments.Count > 0)
				method = InstantiateGenericMethod(method, typeArguments);

			return JsExpression.Invocation(JsExpression.MemberAccess(method, "call"), thisAndArguments);
		}

		public JsExpression BindBaseCall(IType baseType, string methodName, IEnumerable<IType> typeArguments, JsExpression @this) {
			throw new NotImplementedException();
		}
	}
}
