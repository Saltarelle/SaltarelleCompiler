using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests {
	public class MockRuntimeLibrary : IRuntimeLibrary {
		public MockRuntimeLibrary() {
			GetScriptType = (t, o) => {
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
		                			return JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), new[] { new JsTypeReferenceExpression(def.ParentAssembly, t.Name) }.Concat(pt.TypeArguments.Select(a => GetScriptType(a, o))));
			                	}
			                	else if (t is ITypeDefinition) {
			                		var td = (ITypeDefinition)t;
			                		var jsref = new JsTypeReferenceExpression(td.ParentAssembly, t.Name);
			                		if (td.TypeParameterCount > 0)
			                			return JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), new[] { jsref }.Concat(td.TypeParameters.Select(p => GetScriptType(p, o))));
			                		else {
			                			return jsref;
			                		}
			                	}
			                	else if (t is ITypeParameter) {
			                		return JsExpression.Identifier("$" + ((ITypeParameter)t).Name);
			                	}
			                	else {
			                		throw new ArgumentException("Unsupported type + " + t.ToString());
			                	}
			                };
			TypeIs                      = (e, t)        => JsExpression.Invocation(JsExpression.Identifier("$TypeIs"), e, GetScriptType(t, false));
			TryDowncast                 = (e, t)        => JsExpression.Invocation(JsExpression.Identifier("$TryCast"), e, t);
			Downcast                    = (e, s, d)     => JsExpression.Invocation(JsExpression.Identifier("$Cast"), e, GetScriptType(d, false));
			ImplicitReferenceConversion = (e, t)        => JsExpression.Invocation(JsExpression.Identifier("$Upcast"), e, t);
			InstantiateGenericMethod    = (m, a)        => JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericMethod"), new[] { m }.Concat(a));
			MakeException               = (e)           => JsExpression.Invocation(JsExpression.Identifier("$MakeException"), e);
			IntegerDivision             = (n, d)        => JsExpression.Invocation(JsExpression.Identifier("$IntDiv"), n, d);
			FloatToInt                  = (e)           => JsExpression.Invocation(JsExpression.Identifier("$Truncate"), e);
			Coalesce                    = (a, b)        => JsExpression.Invocation(JsExpression.Identifier("$Coalesce"), a, b);
			Lift                        = (e)           => JsExpression.Invocation(JsExpression.Identifier("$Lift"), e);
			FromNullable                = (e)           => JsExpression.Invocation(JsExpression.Identifier("$FromNullable"), e);
			LiftedBooleanAnd            = (a, b)        => JsExpression.Invocation(JsExpression.Identifier("$LiftedBooleanAnd"), a, b);
			LiftedBooleanOr             = (a, b)        => JsExpression.Invocation(JsExpression.Identifier("$LiftedBooleanOr"), a, b);
			Bind                        = (f, t)        => JsExpression.Invocation(JsExpression.Identifier("$Bind"), f, t);
			Default                     = (t)           => JsExpression.Invocation(JsExpression.Identifier("$Default"), t);
			CreateArray                 = (s)           => JsExpression.Invocation(JsExpression.Identifier("$CreateArray"), s);
			CallBase                    = (t, n, ta, a) => JsExpression.Invocation(JsExpression.Identifier("$CallBase"), new[] { t, JsExpression.String(n), JsExpression.ArrayLiteral(ta), JsExpression.ArrayLiteral(a) });
			BindBaseCall                = (t, n, ta, a) => JsExpression.Invocation(JsExpression.Identifier("$BindBaseCall"), new[] { t, JsExpression.String(n), JsExpression.ArrayLiteral(ta), a });
		}

		public Func<IType, bool, JsExpression> GetScriptType { get; set; }
		public Func<JsExpression, IType, JsExpression> TypeIs { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> TryDowncast { get; set; }
		public Func<JsExpression, IType, IType, JsExpression> Downcast { get; set; }
		public Func<JsExpression, IEnumerable<JsExpression>, JsExpression> InstantiateGenericMethod { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> ImplicitReferenceConversion { get; set; }
		public Func<JsExpression, JsExpression> MakeException { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> IntegerDivision { get; set; }
		public Func<JsExpression, JsExpression> FloatToInt { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> Coalesce { get; set; }
		public Func<JsExpression, JsExpression> Lift { get; set; }
		public Func<JsExpression, JsExpression> FromNullable { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> LiftedBooleanAnd { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> LiftedBooleanOr { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> Bind { get; set; }
		public Func<JsExpression, JsExpression> Default { get; set; }
		public Func<JsExpression, JsExpression> CreateArray { get; set; }
		public Func<JsExpression, string, IEnumerable<JsExpression>, IEnumerable<JsExpression>, JsExpression> CallBase { get; set; }
		public Func<JsExpression, string, IEnumerable<JsExpression>, JsExpression, JsExpression> BindBaseCall { get; set; }

		JsExpression IRuntimeLibrary.GetScriptType(IType type, bool returnOpenType) {
			return GetScriptType(type, returnOpenType);
		}
			
		JsExpression IRuntimeLibrary.TypeIs(JsExpression expression, IType targetType) {
			return TypeIs(expression, targetType);
		}

		JsExpression IRuntimeLibrary.TryDowncast(JsExpression expression, JsExpression targetType) {
			return TryDowncast(expression, targetType);
		}

		JsExpression IRuntimeLibrary.Downcast(JsExpression expression, IType sourceType, IType targetType) {
			return Downcast(expression, sourceType, targetType);
		}

		JsExpression IRuntimeLibrary.ImplicitReferenceConversion(JsExpression expression, JsExpression targetType) {
			return ImplicitReferenceConversion(expression, targetType);
		}

		JsExpression IRuntimeLibrary.InstantiateGenericMethod(JsExpression type, IEnumerable<JsExpression> typeArguments) {
			return InstantiateGenericMethod(type, typeArguments);
		}

		JsExpression IRuntimeLibrary.MakeException(JsExpression operand) {
			return MakeException(operand);
		}

		JsExpression IRuntimeLibrary.IntegerDivision(JsExpression numerator, JsExpression denominator) {
			return IntegerDivision(numerator, denominator);
		}

		JsExpression IRuntimeLibrary.FloatToInt(JsExpression operand) {
			return FloatToInt(operand);
		}

		JsExpression IRuntimeLibrary.Coalesce(JsExpression a, JsExpression b) {
			return Coalesce(a, b);
		}

		JsExpression IRuntimeLibrary.Lift(JsExpression expression) {
			return Lift(expression);
		}

		JsExpression IRuntimeLibrary.FromNullable(JsExpression expression) {
			return FromNullable(expression);
		}

		JsExpression IRuntimeLibrary.LiftedBooleanAnd(JsExpression a, JsExpression b) {
			return LiftedBooleanAnd(a, b);
		}

		JsExpression IRuntimeLibrary.LiftedBooleanOr(JsExpression a, JsExpression b) {
			return LiftedBooleanOr(a, b);
		}

		JsExpression IRuntimeLibrary.Bind(JsExpression function, JsExpression target) {
			return Bind(function, target);
		}

		JsExpression IRuntimeLibrary.Default(JsExpression type) {
			return Default(type);
		}

		JsExpression IRuntimeLibrary.CreateArray(JsExpression size) {
			return CreateArray(size);
		}

		JsExpression IRuntimeLibrary.CallBase(JsExpression baseType, string methodName, IEnumerable<JsExpression> typeArguments, IEnumerable<JsExpression> thisAndArguments) {
			return CallBase(baseType, methodName, typeArguments, thisAndArguments);
		}

		JsExpression IRuntimeLibrary.BindBaseCall(JsExpression baseType, string methodName, IEnumerable<JsExpression> typeArguments, JsExpression @this) {
			return BindBaseCall(baseType, methodName, typeArguments, @this);
		}
	}
}