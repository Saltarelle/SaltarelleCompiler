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

		public ScriptSharpRuntimeLibrary(INamingConventionResolver namingConvention) {
			_namingConvention = namingConvention;
		}

		public JsExpression GetScriptType(IType type, bool returnOpenType) {
			if (type is ITypeDefinition) {
				var td = (ITypeDefinition)type;
				var sem = _namingConvention.GetTypeSemantics(td);
				var jsref = new JsTypeReferenceExpression(td.ParentAssembly, sem.Type == TypeScriptSemantics.ImplType.NormalType ? sem.Name : "Unusable_type");
				if (td.TypeParameterCount > 0) {
					throw new NotImplementedException();
				}
				else {
					return jsref;
				}
			}
			else {
				throw new NotImplementedException();
			}
		}

		public JsExpression TypeIs(JsExpression expression, JsExpression targetType) {
			throw new NotImplementedException();
		}

		public JsExpression TryDowncast(JsExpression expression, JsExpression targetType) {
			throw new NotImplementedException();
		}

		public JsExpression Downcast(JsExpression expression, JsExpression targetType) {
			throw new NotImplementedException();
		}

		public JsExpression ImplicitReferenceConversion(JsExpression expression, JsExpression targetType) {
			throw new NotImplementedException();
		}

		public JsExpression InstantiateGenericMethod(JsExpression method, IEnumerable<JsExpression> typeArguments) {
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public JsExpression LiftedBooleanAnd(JsExpression a, JsExpression b) {
			throw new NotImplementedException();
		}

		public JsExpression LiftedBooleanOr(JsExpression a, JsExpression b) {
			throw new NotImplementedException();
		}

		public JsExpression Bind(JsExpression function, JsExpression target) {
			throw new NotImplementedException();
		}

		public JsExpression Default(JsExpression type) {
			throw new NotImplementedException();
		}

		public JsExpression CreateArray(JsExpression size) {
			throw new NotImplementedException();
		}

		public JsExpression CallBase(JsExpression baseType, string methodName, IEnumerable<JsExpression> typeArguments, IEnumerable<JsExpression> thisAndArguments) {
			throw new NotImplementedException();
		}

		public JsExpression BindBaseCall(JsExpression baseType, string methodName, IEnumerable<JsExpression> typeArguments, JsExpression @this) {
			throw new NotImplementedException();
		}
	}
}
/* Old mock implementation of GetScriptType:
	if (t.TypeParameterCount > 0 && !(t is ParameterizedType) && o) {
		// This handles open generic types ( typeof(C<,>) )
		var def = t.GetDefinition();
		return new JsTypeReferenceExpression(def.ParentAssembly, def.FullName);
	}
	else if (t is ArrayType) {
		return JsExpression.Invocation(JsExpression.Identifier("$Array"), GetScriptType(((ArrayType)t).ElementType, o));
	}
	else if (t is ParameterizedType) {
		var pt = (ParameterizedType)t;
		var def = pt.GetDefinition();
		var sem = n.GetTypeSemantics(def);
		if (sem.Type == TypeScriptSemantics.ImplType.NormalType && !sem.IgnoreGenericArguments)
			return JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), new[] { new JsTypeReferenceExpression(def.ParentAssembly, sem.Type == TypeScriptSemantics.ImplType.NormalType ? sem.Name : "Unusable_type") }.Concat(pt.TypeArguments.Select(a => GetScriptType(a, o, n))));
		else
			return new JsTypeReferenceExpression(def.ParentAssembly, sem.Type == TypeScriptSemantics.ImplType.NormalType ? sem.Name : "Unusable_type");
	}
	else if (t is ITypeDefinition) {
		var td = (ITypeDefinition)t;
		var sem = n.GetTypeSemantics(td);
		var jsref = new JsTypeReferenceExpression(td.ParentAssembly, sem.Type == TypeScriptSemantics.ImplType.NormalType ? sem.Name : "Unusable_type");
		if (td.TypeParameterCount > 0)
			return JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), new[] { jsref }.Concat(td.TypeParameters.Select(p => GetScriptType(p, o, n))));
		else {
			return jsref;
		}
	}
	else if (t is ITypeParameter) {
		return JsExpression.Identifier(n.GetTypeParameterName((ITypeParameter)t));
	}
	else {
		throw new ArgumentException("Unsupported type + " + t.ToString());
	}
*/