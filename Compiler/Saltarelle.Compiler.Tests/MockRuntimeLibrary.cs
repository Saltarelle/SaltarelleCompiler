using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Moq;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests {
	public class MockRuntimeLibrary : IRuntimeLibrary {
		private string GetTypeContextShortName(TypeContext c) {
			switch (c) {
				case TypeContext.InvokeConstructor: return "inst";
				case TypeContext.GenericArgument:   return "ga";
				case TypeContext.TypeOf:            return "to";
				case TypeContext.CastTarget:        return "ct";
				case TypeContext.GetDefaultValue:   return "def";
				case TypeContext.UseStaticMember:   return "sm";
				case TypeContext.InlineCode:        return "ic";
				case TypeContext.BindBaseCall:      return "bind";
				default: throw new ArgumentException("c");
			}
		}

		public MockRuntimeLibrary() {
			GetScriptType = (t, c, rtp) => {
			                    string context = GetTypeContextShortName(c);
			                    if (t.TypeParameterCount > 0 && !(t is ParameterizedType) && c == TypeContext.TypeOf) {
			                        // This handles open generic types ( typeof(C<,>) )
			                        var def = t.GetDefinition();
			                        return new JsTypeReferenceExpression(Common.CreateMockTypeDefinition(context + "_" + def.Name, Common.CreateMockAssembly()));
			                    }
			                    else if (t is ArrayType) {
			                        return JsExpression.Invocation(JsExpression.Identifier(context + "_$Array"), GetScriptType(((ArrayType)t).ElementType, TypeContext.GenericArgument, rtp));
			                    }
			                    else if (t is ParameterizedType) {
			                        var pt = (ParameterizedType)t;
		                            return JsExpression.Invocation(JsExpression.Identifier(context + "_$InstantiateGenericType"), new[] { new JsTypeReferenceExpression(Common.CreateMockTypeDefinition(t.Name, Common.CreateMockAssembly())) }.Concat(pt.TypeArguments.Select(a => GetScriptType(a, TypeContext.GenericArgument, rtp))));
			                    }
			                    else if (t is ITypeDefinition) {
			                        var td = (ITypeDefinition)t;
			                        if (td.TypeParameterCount > 0)
			                            return JsExpression.Invocation(JsExpression.Identifier(context + "_$InstantiateGenericType"), new[] { new JsTypeReferenceExpression(Common.CreateMockTypeDefinition(t.Name, Common.CreateMockAssembly())) }.Concat(td.TypeParameters.Select(p => GetScriptType(p, TypeContext.GenericArgument, rtp))));
			                        else {
			                            return new JsTypeReferenceExpression(Common.CreateMockTypeDefinition(context + "_" + t.Name, Common.CreateMockAssembly()));
			                        }
			                    }
			                    else if (t is ITypeParameter) {
			                        return rtp((ITypeParameter)t);
			                    }
			                    else {
			                        throw new ArgumentException("Unsupported type + " + t.ToString());
			                    }
			                };
			TypeIs                          = (e, s, t, rtp)       => JsExpression.Invocation(JsExpression.Identifier("$TypeIs"), e, GetScriptType(t, TypeContext.CastTarget, rtp));
			TryDowncast                     = (e, s, d, rtp)       => JsExpression.Invocation(JsExpression.Identifier("$TryCast"), e, GetScriptType(d, TypeContext.CastTarget, rtp));
			Downcast                        = (e, s, d, rtp)       => JsExpression.Invocation(JsExpression.Identifier("$Cast"), e, GetScriptType(d, TypeContext.CastTarget, rtp));
			Upcast                          = (e, s, d, rtp)       => JsExpression.Invocation(JsExpression.Identifier("$Upcast"), e, GetScriptType(d, TypeContext.CastTarget, rtp));
			ReferenceEquals                 = (a, b)               => JsExpression.Invocation(JsExpression.Identifier("$ReferenceEquals"), a, b);
			ReferenceNotEquals              = (a, b)               => JsExpression.Invocation(JsExpression.Identifier("$ReferenceNotEquals"), a, b);
			InstantiateGenericMethod        = (m, a, rtp)          => JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericMethod"), new[] { m }.Concat(a.Select(x => GetScriptType(x, TypeContext.GenericArgument, rtp))));
			MakeException                   = (e)                  => JsExpression.Invocation(JsExpression.Identifier("$MakeException"), e);
			IntegerDivision                 = (n, d)               => JsExpression.Invocation(JsExpression.Identifier("$IntDiv"), n, d);
			FloatToInt                      = (e)                  => JsExpression.Invocation(JsExpression.Identifier("$Truncate"), e);
			Coalesce                        = (a, b)               => JsExpression.Invocation(JsExpression.Identifier("$Coalesce"), a, b);
			Lift                            = (e)                  => JsExpression.Invocation(JsExpression.Identifier("$Lift"), e);
			FromNullable                    = (e)                  => JsExpression.Invocation(JsExpression.Identifier("$FromNullable"), e);
			LiftedBooleanAnd                = (a, b)               => JsExpression.Invocation(JsExpression.Identifier("$LiftedBooleanAnd"), a, b);
			LiftedBooleanOr                 = (a, b)               => JsExpression.Invocation(JsExpression.Identifier("$LiftedBooleanOr"), a, b);
			Bind                            = (f, t)               => JsExpression.Invocation(JsExpression.Identifier("$Bind"), f, t);
			BindFirstParameterToThis        = (f)                  => JsExpression.Invocation(JsExpression.Identifier("$BindFirstParameterToThis"), f);
			Default                         = (t, rtp)             => t.Kind == TypeKind.Dynamic ? (JsExpression)JsExpression.Identifier("$DefaultDynamic") : JsExpression.Invocation(JsExpression.Identifier("$Default"), GetScriptType(t, TypeContext.GetDefaultValue, rtp));
			CreateArray                     = (t, dim, rtp)        => JsExpression.Invocation(JsExpression.Identifier("$CreateArray"), new[] { GetScriptType(t, TypeContext.GetDefaultValue, rtp) }.Concat(dim));
			CloneDelegate                   = (e, s, t, rtp)       => JsExpression.Invocation(JsExpression.Identifier("$CloneDelegate"), e);
			CallBase                        = (t, n, ta, a, rtp)   => JsExpression.Invocation(JsExpression.Identifier("$CallBase"), new[] { GetScriptType(t, TypeContext.BindBaseCall, rtp), JsExpression.String(n), JsExpression.ArrayLiteral(ta.Select(x => GetScriptType(x, TypeContext.GenericArgument, rtp))), JsExpression.ArrayLiteral(a) });
			BindBaseCall                    = (t, n, ta, a, rtp)   => JsExpression.Invocation(JsExpression.Identifier("$BindBaseCall"), new[] { GetScriptType(t, TypeContext.BindBaseCall, rtp), JsExpression.String(n), JsExpression.ArrayLiteral(ta.Select(x => GetScriptType(x, TypeContext.GenericArgument, rtp))), a });
			MakeEnumerator                  = (yt, mn, gc, d, rtp) => JsExpression.Invocation(JsExpression.Identifier("$MakeEnumerator"), new[] { GetScriptType(yt, TypeContext.GenericArgument, rtp), mn, gc, d ?? (JsExpression)JsExpression.Null });
			MakeEnumerable                  = (yt, ge, rtp)        => JsExpression.Invocation(JsExpression.Identifier("$MakeEnumerable"), new[] { GetScriptType(yt, TypeContext.GenericArgument, rtp), ge });
			GetMultiDimensionalArrayValue   = (a, i)               => JsExpression.Invocation(JsExpression.Identifier("$MultidimArrayGet"), new[] { a }.Concat(i));
			SetMultiDimensionalArrayValue   = (a, i, v)            => JsExpression.Invocation(JsExpression.Identifier("$MultidimArraySet"), new[] { a }.Concat(i).Concat(new[] { v }));
			CreateTaskCompletionSource      = (t, rtp)             => JsExpression.Invocation(JsExpression.Identifier("$CreateTaskCompletionSource"), t != null ? GetScriptType(t, TypeContext.GenericArgument, rtp) : JsExpression.String("non-generic"));
			SetAsyncResult                  = (t, v)               => JsExpression.Invocation(JsExpression.Identifier("$SetAsyncResult"), t, v ?? JsExpression.String("<<null>>"));
			SetAsyncException               = (t, e)               => JsExpression.Invocation(JsExpression.Identifier("$SetAsyncException"), t, e);
			GetTaskFromTaskCompletionSource = (t)                  => JsExpression.Invocation(JsExpression.Identifier("$GetTask"), t);
		}

		public Func<IType, TypeContext, Func<ITypeParameter, JsExpression>, JsExpression> GetScriptType { get; set; }
		public Func<JsExpression, IType, IType, Func<ITypeParameter, JsExpression>, JsExpression> TypeIs { get; set; }
		public Func<JsExpression, IType, IType, Func<ITypeParameter, JsExpression>, JsExpression> TryDowncast { get; set; }
		public Func<JsExpression, IType, IType, Func<ITypeParameter, JsExpression>, JsExpression> Downcast { get; set; }
		public Func<JsExpression, IType, IType, Func<ITypeParameter, JsExpression>, JsExpression> Upcast { get; set; }
		public Func<JsExpression, IEnumerable<IType>, Func<ITypeParameter, JsExpression>, JsExpression> InstantiateGenericMethod { get; set; }
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
		public Func<JsExpression, JsExpression> BindFirstParameterToThis { get; set; }
		public Func<IType, Func<ITypeParameter, JsExpression>, JsExpression> Default { get; set; }
		public Func<IType, IEnumerable<JsExpression>, Func<ITypeParameter, JsExpression>, JsExpression> CreateArray { get; set; }
		public Func<JsExpression, IType, IType, Func<ITypeParameter, JsExpression>, JsExpression> CloneDelegate { get; set; }
		public Func<IType, string, IEnumerable<IType>, IEnumerable<JsExpression>, Func<ITypeParameter, JsExpression>, JsExpression> CallBase { get; set; }
		public Func<IType, string, IList<IType>, JsExpression, Func<ITypeParameter, JsExpression>, JsExpression> BindBaseCall { get; set; }
		public Func<IType, JsExpression, JsExpression, JsExpression, Func<ITypeParameter, JsExpression>, JsExpression> MakeEnumerator { get; set; }
		public Func<IType, JsExpression, Func<ITypeParameter, JsExpression>, JsExpression> MakeEnumerable { get; set; }
		public Func<JsExpression, IEnumerable<JsExpression>, JsExpression> GetMultiDimensionalArrayValue;
		public Func<JsExpression, IEnumerable<JsExpression>, JsExpression, JsExpression> SetMultiDimensionalArrayValue;
		public Func<IType, Func<ITypeParameter, JsExpression>, JsExpression> CreateTaskCompletionSource;
		public Func<JsExpression, JsExpression, JsExpression> SetAsyncResult;
		public Func<JsExpression, JsExpression, JsExpression> SetAsyncException;
		public Func<JsExpression, JsExpression> GetTaskFromTaskCompletionSource;
		
		JsExpression IRuntimeLibrary.GetScriptType(IType type, TypeContext context, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return GetScriptType(type, context, resolveTypeParameter);
		}
			
		JsExpression IRuntimeLibrary.TypeIs(JsExpression expression, IType sourceType, IType targetType, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return TypeIs(expression, sourceType, targetType, resolveTypeParameter);
		}

		JsExpression IRuntimeLibrary.TryDowncast(JsExpression expression, IType sourceType, IType targetType, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return TryDowncast(expression, sourceType, targetType, resolveTypeParameter);
		}

		JsExpression IRuntimeLibrary.Downcast(JsExpression expression, IType sourceType, IType targetType, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return Downcast(expression, sourceType, targetType, resolveTypeParameter);
		}

		JsExpression IRuntimeLibrary.Upcast(JsExpression expression, IType sourceType, IType targetType, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return Upcast(expression, sourceType, targetType, resolveTypeParameter);
		}

		JsExpression IRuntimeLibrary.ReferenceEquals(JsExpression a, JsExpression b) {
			return ReferenceEquals(a, b);
		}

		JsExpression IRuntimeLibrary.ReferenceNotEquals(JsExpression a, JsExpression b) {
			return ReferenceNotEquals(a, b);
		}

		JsExpression IRuntimeLibrary.InstantiateGenericMethod(JsExpression type, IEnumerable<IType> typeArguments, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return InstantiateGenericMethod(type, typeArguments, resolveTypeParameter);
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

		JsExpression IRuntimeLibrary.BindFirstParameterToThis(JsExpression function) {
			return BindFirstParameterToThis(function);
		}

		JsExpression IRuntimeLibrary.Default(IType type, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return Default(type, resolveTypeParameter);
		}

		JsExpression IRuntimeLibrary.CreateArray(IType elementType, IEnumerable<JsExpression> size, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return CreateArray(elementType, size, resolveTypeParameter);
		}

		JsExpression IRuntimeLibrary.CloneDelegate(JsExpression source, IType sourceType, IType targetType, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return CloneDelegate(source, sourceType, targetType, resolveTypeParameter);
		}

		JsExpression IRuntimeLibrary.CallBase(IType baseType, string methodName, IList<IType> typeArguments, IEnumerable<JsExpression> thisAndArguments, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return CallBase(baseType, methodName, typeArguments, thisAndArguments, resolveTypeParameter);
		}

		JsExpression IRuntimeLibrary.BindBaseCall(IType baseType, string methodName, IList<IType> typeArguments, JsExpression @this, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return BindBaseCall(baseType, methodName, typeArguments, @this, resolveTypeParameter);
		}

		JsExpression IRuntimeLibrary.MakeEnumerator(IType yieldType, JsExpression moveNext, JsExpression getCurrent, JsExpression dispose, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return MakeEnumerator(yieldType, moveNext, getCurrent, dispose, resolveTypeParameter);
		}

		JsExpression IRuntimeLibrary.MakeEnumerable(IType yieldType, JsExpression getEnumerator, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return MakeEnumerable(yieldType, getEnumerator, resolveTypeParameter);
		}

		JsExpression IRuntimeLibrary.GetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices) {
			return GetMultiDimensionalArrayValue(array, indices);
		}

		JsExpression IRuntimeLibrary.SetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, JsExpression value) {
			return SetMultiDimensionalArrayValue(array, indices, value);
		}

		JsExpression IRuntimeLibrary.CreateTaskCompletionSource(IType taskGenericArgument, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return CreateTaskCompletionSource(taskGenericArgument, resolveTypeParameter);
		}

		JsExpression IRuntimeLibrary.SetAsyncResult(JsExpression taskCompletionSource, JsExpression value) {
			return SetAsyncResult(taskCompletionSource, value);
		}

		JsExpression IRuntimeLibrary.SetAsyncException(JsExpression taskCompletionSource, JsExpression exception) {
			return SetAsyncException(taskCompletionSource, exception);
		}

		JsExpression IRuntimeLibrary.GetTaskFromTaskCompletionSource(JsExpression taskCompletionSource) {
			return GetTaskFromTaskCompletionSource(taskCompletionSource);
		}
	}
}