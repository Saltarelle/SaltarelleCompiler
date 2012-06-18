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

		public JsExpression InstantiateGenericMethod(JsExpression method, IEnumerable<JsExpression> typeArguments) {
			return JsExpression.Invocation(method, typeArguments);
		}

		public JsExpression MakeException(JsExpression operand) {
			throw new NotImplementedException();
		}

		public JsExpression IntegerDivision(JsExpression numerator, JsExpression denominator) {
			throw new NotImplementedException();
		}

		public JsExpression FloatToInt(JsExpression operand) {
			throw new NotImplementedException();
		}

		public JsExpression Coalesce(JsExpression a, JsExpression b) {
			throw new NotImplementedException();
		}

		public JsExpression Lift(JsExpression expression) {
			throw new NotImplementedException();
		}

		public JsExpression FromNullable(JsExpression expression) {
			// TODO: Obviously not good...
			return expression;
//			throw new NotImplementedException();
		}

		public JsExpression LiftedBooleanAnd(JsExpression a, JsExpression b) {
			throw new NotImplementedException();
		}

		public JsExpression LiftedBooleanOr(JsExpression a, JsExpression b) {
			throw new NotImplementedException();
		}

		public JsExpression Bind(JsExpression function, JsExpression target) {
			return JsExpression.Invocation(JsExpression.MemberAccess(_createTypeReferenceExpression(KnownTypeReference.Delegate), "mkdel"), target, function);
		}

		public JsExpression Default(JsExpression type) {
			throw new NotImplementedException();
		}

		public JsExpression CreateArray(JsExpression size) {
			return JsExpression.New(_createTypeReferenceExpression(KnownTypeReference.Array), size);
		}

		public JsExpression CallBase(JsExpression baseType, string methodName, IEnumerable<JsExpression> typeArguments, IEnumerable<JsExpression> thisAndArguments) {
			throw new NotImplementedException();
		}

		public JsExpression BindBaseCall(JsExpression baseType, string methodName, IEnumerable<JsExpression> typeArguments, JsExpression @this) {
			throw new NotImplementedException();
		}
	}
}
