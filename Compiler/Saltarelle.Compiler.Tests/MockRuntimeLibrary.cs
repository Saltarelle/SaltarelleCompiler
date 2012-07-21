using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests {
	public class MockRuntimeLibrary : IRuntimeLibrary {
		public MockRuntimeLibrary() {
			GetScriptType = (t, c) => {
			                	if (t.TypeParameterCount > 0 && !(t is ParameterizedType) && c == TypeContext.TypeOf) {
			                		// This handles open generic types ( typeof(C<,>) )
			                		var def = t.GetDefinition();
			                		return new JsTypeReferenceExpression(def.ParentAssembly, def.FullName);
			                	}
			                	else if (t is ArrayType) {
			                		return JsExpression.Invocation(JsExpression.Identifier("$Array"), GetScriptType(((ArrayType)t).ElementType, TypeContext.GenericArgument));
			                	}
			                	else if (t is ParameterizedType) {
			                		var pt = (ParameterizedType)t;
			                		var def = pt.GetDefinition();
		                			return JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), new[] { new JsTypeReferenceExpression(def.ParentAssembly, t.Name) }.Concat(pt.TypeArguments.Select(a => GetScriptType(a, TypeContext.GenericArgument))));
			                	}
			                	else if (t is ITypeDefinition) {
			                		var td = (ITypeDefinition)t;
			                		var jsref = new JsTypeReferenceExpression(td.ParentAssembly, t.Name);
			                		if (td.TypeParameterCount > 0)
			                			return JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericType"), new[] { jsref }.Concat(td.TypeParameters.Select(p => GetScriptType(p, TypeContext.GenericArgument))));
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
			TypeIs                   = (e, s, t)     => JsExpression.Invocation(JsExpression.Identifier("$TypeIs"), e, GetScriptType(t, TypeContext.CastTarget));
			TryDowncast              = (e, s, d)     => JsExpression.Invocation(JsExpression.Identifier("$TryCast"), e, GetScriptType(d, TypeContext.CastTarget));
			Downcast                 = (e, s, d)     => JsExpression.Invocation(JsExpression.Identifier("$Cast"), e, GetScriptType(d, TypeContext.CastTarget));
			Upcast                   = (e, s, d)     => JsExpression.Invocation(JsExpression.Identifier("$Upcast"), e, GetScriptType(d, TypeContext.CastTarget));
			ReferenceEquals          = (a, b)        => JsExpression.Invocation(JsExpression.Identifier("$ReferenceEquals"), a, b);
			ReferenceNotEquals       = (a, b)        => JsExpression.Invocation(JsExpression.Identifier("$ReferenceNotEquals"), a, b);
			InstantiateGenericMethod = (m, a)        => JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericMethod"), new[] { m }.Concat(a.Select(x => GetScriptType(x, TypeContext.GenericArgument))));
			MakeException            = (e)           => JsExpression.Invocation(JsExpression.Identifier("$MakeException"), e);
			IntegerDivision          = (n, d)        => JsExpression.Invocation(JsExpression.Identifier("$IntDiv"), n, d);
			FloatToInt               = (e)           => JsExpression.Invocation(JsExpression.Identifier("$Truncate"), e);
			Coalesce                 = (a, b)        => JsExpression.Invocation(JsExpression.Identifier("$Coalesce"), a, b);
			Lift                     = (e)           => JsExpression.Invocation(JsExpression.Identifier("$Lift"), e);
			FromNullable             = (e)           => JsExpression.Invocation(JsExpression.Identifier("$FromNullable"), e);
			LiftedBooleanAnd         = (a, b)        => JsExpression.Invocation(JsExpression.Identifier("$LiftedBooleanAnd"), a, b);
			LiftedBooleanOr          = (a, b)        => JsExpression.Invocation(JsExpression.Identifier("$LiftedBooleanOr"), a, b);
			Bind                     = (f, t)        => JsExpression.Invocation(JsExpression.Identifier("$Bind"), f, t);
			Default                  = (t)           => JsExpression.Invocation(JsExpression.Identifier("$Default"), GetScriptType(t, TypeContext.GetDefaultValue));
			CreateArray              = (s)           => JsExpression.Invocation(JsExpression.Identifier("$CreateArray"), s);
			CloneDelegate            = (e, s, t)     => JsExpression.Invocation(JsExpression.Identifier("$CloneDelegate"), e);
			CallBase                 = (t, n, ta, a) => JsExpression.Invocation(JsExpression.Identifier("$CallBase"), new[] { GetScriptType(t, TypeContext.Instantiation), JsExpression.String(n), JsExpression.ArrayLiteral(ta.Select(x => GetScriptType(x, TypeContext.GenericArgument))), JsExpression.ArrayLiteral(a) });
			BindBaseCall             = (t, n, ta, a) => JsExpression.Invocation(JsExpression.Identifier("$BindBaseCall"), new[] { GetScriptType(t, TypeContext.Instantiation), JsExpression.String(n), JsExpression.ArrayLiteral(ta.Select(x => GetScriptType(x, TypeContext.GenericArgument))), a });
		}

		public Func<IType, TypeContext, JsExpression> GetScriptType { get; set; }
		public Func<JsExpression, IType, IType, JsExpression> TypeIs { get; set; }
		public Func<JsExpression, IType, IType, JsExpression> TryDowncast { get; set; }
		public Func<JsExpression, IType, IType, JsExpression> Downcast { get; set; }
		public Func<JsExpression, IType, IType, JsExpression> Upcast { get; set; }
		public Func<JsExpression, IEnumerable<IType>, JsExpression> InstantiateGenericMethod { get; set; }
		new public Func<JsExpression, JsExpression, JsExpression> ReferenceEquals { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> ReferenceNotEquals { get; set; }
		public Func<JsExpression, JsExpression> MakeException { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> IntegerDivision { get; set; }
		public Func<JsExpression, JsExpression> FloatToInt { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> Coalesce { get; set; }
		public Func<JsExpression, JsExpression> Lift { get; set; }
		public Func<JsExpression, JsExpression> FromNullable { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> LiftedBooleanAnd { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> LiftedBooleanOr { get; set; }
		public Func<JsExpression, JsExpression, JsExpression> Bind { get; set; }
		public Func<IType, JsExpression> Default { get; set; }
		public Func<JsExpression, JsExpression> CreateArray { get; set; }
		public Func<JsExpression, IType, IType, JsExpression> CloneDelegate { get; set; }
		public Func<IType, string, IEnumerable<IType>, IEnumerable<JsExpression>, JsExpression> CallBase { get; set; }
		public Func<IType, string, IList<IType>, JsExpression, JsExpression> BindBaseCall { get; set; }

		JsExpression IRuntimeLibrary.GetScriptType(IType type, TypeContext context) {
			return GetScriptType(type, context);
		}
			
		JsExpression IRuntimeLibrary.TypeIs(JsExpression expression, IType sourceType, IType targetType) {
			return TypeIs(expression, sourceType, targetType);
		}

		JsExpression IRuntimeLibrary.TryDowncast(JsExpression expression, IType sourceType, IType targetType) {
			return TryDowncast(expression, sourceType, targetType);
		}

		JsExpression IRuntimeLibrary.Downcast(JsExpression expression, IType sourceType, IType targetType) {
			return Downcast(expression, sourceType, targetType);
		}

		JsExpression IRuntimeLibrary.Upcast(JsExpression expression, IType sourceType, IType targetType) {
			return Upcast(expression, sourceType, targetType);
		}

		JsExpression IRuntimeLibrary.ReferenceEquals(JsExpression a, JsExpression b) {
			return ReferenceEquals(a, b);
		}

		JsExpression IRuntimeLibrary.ReferenceNotEquals(JsExpression a, JsExpression b) {
			return ReferenceNotEquals(a, b);
		}

		JsExpression IRuntimeLibrary.InstantiateGenericMethod(JsExpression type, IEnumerable<IType> typeArguments) {
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

		JsExpression IRuntimeLibrary.Default(IType type) {
			return Default(type);
		}

		JsExpression IRuntimeLibrary.CreateArray(JsExpression size) {
			return CreateArray(size);
		}

		JsExpression IRuntimeLibrary.CloneDelegate(JsExpression source, IType sourceType, IType targetType) {
			return CloneDelegate(source, sourceType, targetType);
		}

		JsExpression IRuntimeLibrary.CallBase(IType baseType, string methodName, IList<IType> typeArguments, IEnumerable<JsExpression> thisAndArguments) {
			return CallBase(baseType, methodName, typeArguments, thisAndArguments);
		}

		JsExpression IRuntimeLibrary.BindBaseCall(IType baseType, string methodName, IList<IType> typeArguments, JsExpression @this) {
			return BindBaseCall(baseType, methodName, typeArguments, @this);
		}
	}
}