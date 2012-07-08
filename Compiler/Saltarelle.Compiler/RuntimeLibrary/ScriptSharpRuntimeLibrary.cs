using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.MetadataImporter;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.RuntimeLibrary {
	public class ScriptSharpRuntimeLibrary : IRuntimeLibrary {
		private readonly IScriptSharpMetadataImporter _metadataImporter;
		private readonly Func<ITypeReference, JsExpression> _createTypeReferenceExpression;

		public ScriptSharpRuntimeLibrary(IScriptSharpMetadataImporter metadataImporter, Func<ITypeReference, JsExpression> createTypeReferenceExpression) {
			_metadataImporter = metadataImporter;
			_createTypeReferenceExpression = createTypeReferenceExpression;
		}

		public JsExpression GetScriptType(IType type, TypeContext context) {
			if (type.TypeParameterCount > 0 && !(type is ParameterizedType) && context == TypeContext.TypeOf) {
				// This handles open generic types ( typeof(C<,>) )
				return _createTypeReferenceExpression(type.GetDefinition().ToTypeReference());
			}
			else if (type.Kind == TypeKind.Enum && (context == TypeContext.CastTarget || context == TypeContext.Instantiation)) {
				var def = type.GetDefinition();
				return _createTypeReferenceExpression(def.EnumUnderlyingType.ToTypeReference());
			}
			else if (type.Kind == TypeKind.Array) {
				return _createTypeReferenceExpression(KnownTypeReference.Array);
			}
			else if (type.Kind == TypeKind.Delegate) {
				return _createTypeReferenceExpression(KnownTypeReference.Delegate);
			}
			else if (type is ITypeParameter) {
				return JsExpression.Identifier(_metadataImporter.GetTypeParameterName((ITypeParameter)type));
			}
			else if (type is ParameterizedType) {
				var pt = (ParameterizedType)type;
				var def = pt.GetDefinition();
				var sem = _metadataImporter.GetTypeSemantics(def);
				if (sem.Type == TypeScriptSemantics.ImplType.NormalType && !sem.IgnoreGenericArguments)
					return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Type), "makeGenericType"), _createTypeReferenceExpression(type.GetDefinition().ToTypeReference()), JsExpression.ArrayLiteral(pt.TypeArguments.Select(a => GetScriptType(a, TypeContext.GenericArgument))));
				else
					return _createTypeReferenceExpression(def.ToTypeReference());
			}
			else if (type is ITypeDefinition) {
				var td = (ITypeDefinition)type;
				if (_metadataImporter.IsRecord(td) && (context == TypeContext.CastTarget || context == TypeContext.Inheritance)) {
					return null;
				}
				else if (td.Kind == TypeKind.Interface && _metadataImporter.IsImported(td)) {
					if (context == TypeContext.CastTarget || context == TypeContext.Inheritance)
						return null;
					else
						return _createTypeReferenceExpression(KnownTypeReference.Object);
				}
				else {
					var sem = _metadataImporter.GetTypeSemantics(td);
					var jsref = _createTypeReferenceExpression(td.ToTypeReference());
					if (td.TypeParameterCount > 0 && !sem.IgnoreGenericArguments) {
						// This handles the case of resolving the current type, eg. to access a static member.
						return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Type), "makeGenericType"), _createTypeReferenceExpression(type.GetDefinition().ToTypeReference()), JsExpression.ArrayLiteral(td.TypeParameters.Select(a => GetScriptType(a, TypeContext.GenericArgument))));
					}
					else {
						return jsref;
					}
				}
			}
			else {
				throw new NotImplementedException();
			}
		}

		public JsExpression TypeIs(JsExpression expression, IType targetType) {
			var jsTarget = GetScriptType(targetType, TypeContext.CastTarget);
			return jsTarget != null ? JsExpression.Invocation(JsExpression.MemberAccess(jsTarget, "isInstanceOfType"), expression) : expression;
		}

		public JsExpression TryDowncast(JsExpression expression, IType sourceType, IType targetType) {
			var jsTarget = GetScriptType(targetType, TypeContext.CastTarget);
			return jsTarget != null ? JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Type), "safeCast"), expression, GetScriptType(targetType, TypeContext.CastTarget)) : expression;
		}

		public JsExpression Downcast(JsExpression expression, IType sourceType, IType targetType) {
			var jsTarget = GetScriptType(targetType, TypeContext.CastTarget);
			return jsTarget != null ? JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Type), "cast"), expression, GetScriptType(targetType, TypeContext.CastTarget)) : expression;
		}

		public JsExpression Upcast(JsExpression expression, IType sourceType, IType targetType) {
			return expression;
		}

		public JsExpression InstantiateGenericMethod(JsExpression method, IEnumerable<IType> typeArguments) {
			return JsExpression.Invocation(method, typeArguments.Select(a => GetScriptType(a, TypeContext.GenericArgument)));
		}

		public JsExpression MakeException(JsExpression operand) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Exception), "wrap"), operand);
		}

		public JsExpression IntegerDivision(JsExpression numerator, JsExpression denominator) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Int32), "div"), numerator, denominator);
		}

		public JsExpression FloatToInt(JsExpression operand) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Int32), "trunc"), operand);
		}

		public JsExpression Coalesce(JsExpression a, JsExpression b) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Object), "coalesce"), a, b);
		}

		public JsExpression Lift(JsExpression expression) {
			if (expression is JsInvocationExpression) {
				var int32 = (JsTypeReferenceExpression)_createTypeReferenceExpression(KnownTypeReference.Int32);

				var ie = (JsInvocationExpression)expression;
				if (ie.Method is JsMemberAccessExpression) {
					var mae = (JsMemberAccessExpression)ie.Method;
					if (mae.Target is JsTypeReferenceExpression && ((JsTypeReferenceExpression)mae.Target).Assembly == int32.Assembly && ((JsTypeReferenceExpression)mae.Target).TypeName == int32.TypeName) {
						if (mae.Member == "div" || mae.Member == "trunc")
							return expression;
					}
				}
			}
			if (expression is JsUnaryExpression) {
				string methodName = null;
				switch (expression.NodeType) {
					case ExpressionNodeType.LogicalNot: methodName = "not"; goto default;
					case ExpressionNodeType.Negate:     methodName = "neg"; goto default;
					case ExpressionNodeType.Positive:   methodName = "pos"; goto default;
					case ExpressionNodeType.BitwiseNot: methodName = "cpl"; goto default;

					default:
						if (methodName != null)
							return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.NullableOfT), methodName), ((JsUnaryExpression)expression).Operand);
						break;
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
					case ExpressionNodeType.Divide:             methodName = "div";  goto default;
					case ExpressionNodeType.Multiply:           methodName = "mul";  goto default;
					case ExpressionNodeType.BitwiseAnd:         methodName = "band"; goto default;
					case ExpressionNodeType.BitwiseOr:          methodName = "bor";  goto default;
					case ExpressionNodeType.BitwiseXor:         methodName = "xor";  goto default;
					case ExpressionNodeType.LeftShift:          methodName = "shl";  goto default;
					case ExpressionNodeType.RightShiftSigned:   methodName = "srs";  goto default;
					case ExpressionNodeType.RightShiftUnsigned: methodName = "sru";  goto default;

					default:
						if (methodName != null)
							return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.NullableOfT), methodName), ((JsBinaryExpression)expression).Left, ((JsBinaryExpression)expression).Right);
						break;
				}
			}

			throw new ArgumentException("Cannot lift expression " + OutputFormatter.Format(expression, true));
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
			return JsExpression.Invocation(JsExpression.MemberAccess(GetScriptType(type, TypeContext.GetDefaultValue), "getDefaultValue"));
		}

		public JsExpression CreateArray(JsExpression size) {
			return JsExpression.New(_createTypeReferenceExpression(KnownTypeReference.Array), size);
		}

		public JsExpression CallBase(IType baseType, string methodName, IList<IType> typeArguments, IEnumerable<JsExpression> thisAndArguments) {
			JsExpression method = JsExpression.MemberAccess(JsExpression.MemberAccess(GetScriptType(baseType, TypeContext.Instantiation), "prototype"), methodName);
			
			if (typeArguments != null && typeArguments.Count > 0)
				method = InstantiateGenericMethod(method, typeArguments);

			return JsExpression.Invocation(JsExpression.MemberAccess(method, "call"), thisAndArguments);
		}

		public JsExpression BindBaseCall(IType baseType, string methodName, IList<IType> typeArguments, JsExpression @this) {
			JsExpression method = JsExpression.MemberAccess(JsExpression.MemberAccess(GetScriptType(baseType, TypeContext.Instantiation), "prototype"), methodName);
			
			if (typeArguments != null && typeArguments.Count > 0)
				method = InstantiateGenericMethod(method, typeArguments);

			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Delegate), "mkdel"), @this, method);
		}
	}
}
