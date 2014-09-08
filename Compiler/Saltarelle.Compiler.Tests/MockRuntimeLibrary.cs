using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler.Tests {
	public class MockRuntimeLibrary : IRuntimeLibrary {
		private enum TypeContext {
			GenericArgument,
			TypeOf,
			CastTarget,
			GetDefaultValue,
			UseStaticMember,
			BindBaseCall,
		}

		private string GetTypeContextShortName(TypeContext c) {
			switch (c) {
				case TypeContext.GenericArgument: return "ga";
				case TypeContext.TypeOf:          return "to";
				case TypeContext.UseStaticMember: return "sm";
				case TypeContext.CastTarget:      return "ct";
				case TypeContext.GetDefaultValue: return "def";
				case TypeContext.BindBaseCall:    return "bind";
				default: throw new ArgumentException("c");
			}
		}

		public MockRuntimeLibrary() {
			GetTypeOf                                       = (t, c)             => GetScriptType(t, TypeContext.TypeOf, c.ResolveTypeParameter);
			InstantiateType                                 = (t, c)             => GetScriptType(t, TypeContext.UseStaticMember, c.ResolveTypeParameter);
			InstantiateTypeForUseAsTypeArgumentInInlineCode = (t, c)             => GetScriptType(t, TypeContext.GenericArgument, c.ResolveTypeParameter);
			TypeIs                                          = (e, s, t, c)       => JsExpression.Invocation(JsExpression.Identifier("$TypeIs"), e, GetScriptType(t, TypeContext.CastTarget, c.ResolveTypeParameter));
			TryDowncast                                     = (e, s, d, c)       => JsExpression.Invocation(JsExpression.Identifier("$TryCast"), e, GetScriptType(d, TypeContext.CastTarget, c.ResolveTypeParameter));
			Downcast                                        = (e, s, d, c)       => JsExpression.Invocation(JsExpression.Identifier("$Cast"), e, GetScriptType(d, TypeContext.CastTarget, c.ResolveTypeParameter));
			Upcast                                          = (e, s, d, c)       => JsExpression.Invocation(JsExpression.Identifier("$Upcast"), e, GetScriptType(d, TypeContext.CastTarget, c.ResolveTypeParameter));
			ReferenceEquals                                 = (a, b, c)          => JsExpression.Invocation(JsExpression.Identifier("$ReferenceEquals"), a, b);
			ReferenceNotEquals                              = (a, b, c)          => JsExpression.Invocation(JsExpression.Identifier("$ReferenceNotEquals"), a, b);
			InstantiateGenericMethod                        = (m, a, c)          => JsExpression.Invocation(JsExpression.Identifier("$InstantiateGenericMethod"), new[] { m }.Concat(a.Select(x => GetScriptType(x, TypeContext.GenericArgument, c.ResolveTypeParameter))));
			MakeException                                   = (e, c)             => JsExpression.Invocation(JsExpression.Identifier("$MakeException"), e);
			IntegerDivision                                 = (n, d, c)          => JsExpression.Invocation(JsExpression.Identifier("$IntDiv"), n, d);
			FloatToInt                                      = (e, c)             => JsExpression.Invocation(JsExpression.Identifier("$Truncate"), e);
			Coalesce                                        = (a, b, c)          => JsExpression.Invocation(JsExpression.Identifier("$Coalesce"), a, b);
			Lift                                            = (e, c)             => JsExpression.Invocation(JsExpression.Identifier("$Lift"), e);
			FromNullable                                    = (e, c)             => JsExpression.Invocation(JsExpression.Identifier("$FromNullable"), e);
			LiftedBooleanAnd                                = (a, b, c)          => JsExpression.Invocation(JsExpression.Identifier("$LiftedBooleanAnd"), a, b);
			LiftedBooleanOr                                 = (a, b, c)          => JsExpression.Invocation(JsExpression.Identifier("$LiftedBooleanOr"), a, b);
			Bind                                            = (f, t, c)          => JsExpression.Invocation(JsExpression.Identifier("$Bind"), f, t);
			BindFirstParameterToThis                        = (f, c)             => JsExpression.Invocation(JsExpression.Identifier("$BindFirstParameterToThis"), f);
			Default                                         = (t, c)             => t.TypeKind == TypeKind.DynamicType ? (JsExpression)JsExpression.Identifier("$DefaultDynamic") : JsExpression.Invocation(JsExpression.Identifier("$Default"), GetScriptType(t, TypeContext.GetDefaultValue, c.ResolveTypeParameter));
			CreateArray                                     = (t, dim, c)        => JsExpression.Invocation(JsExpression.Identifier("$CreateArray"), new[] { GetScriptType(t, TypeContext.GetDefaultValue, c.ResolveTypeParameter) }.Concat(dim));
			CloneDelegate                                   = (e, s, t, c)       => JsExpression.Invocation(JsExpression.Identifier("$CloneDelegate"), e);
			CallBase                                        = (m, a, c)          => JsExpression.Invocation(JsExpression.Identifier("$CallBase"), new[] { GetScriptType(m.ContainingType, TypeContext.BindBaseCall, c.ResolveTypeParameter), JsExpression.String("$" + m.Name), JsExpression.ArrayLiteral(m.TypeArguments.Select(x => GetScriptType(x, TypeContext.GenericArgument, c.ResolveTypeParameter))), JsExpression.ArrayLiteral(a) });
			BindBaseCall                                    = (m, a, c)          => JsExpression.Invocation(JsExpression.Identifier("$BindBaseCall"), new[] { GetScriptType(m.ContainingType, TypeContext.BindBaseCall, c.ResolveTypeParameter), JsExpression.String("$" + m.Name), JsExpression.ArrayLiteral(m.TypeArguments.Select(x => GetScriptType(x, TypeContext.GenericArgument, c.ResolveTypeParameter))), a });
			MakeEnumerator                                  = (yt, mn, gc, d, c) => JsExpression.Invocation(JsExpression.Identifier("$MakeEnumerator"), new[] { GetScriptType(yt, TypeContext.GenericArgument, c.ResolveTypeParameter), mn, gc, d ?? JsExpression.Null });
			MakeEnumerable                                  = (yt, ge, c)        => JsExpression.Invocation(JsExpression.Identifier("$MakeEnumerable"), new[] { GetScriptType(yt, TypeContext.GenericArgument, c.ResolveTypeParameter), ge });
			GetMultiDimensionalArrayValue                   = (a, i, c)          => JsExpression.Invocation(JsExpression.Identifier("$MultidimArrayGet"), new[] { a }.Concat(i));
			SetMultiDimensionalArrayValue                   = (a, i, v, c)       => JsExpression.Invocation(JsExpression.Identifier("$MultidimArraySet"), new[] { a }.Concat(i).Concat(new[] { v }));
			CreateTaskCompletionSource                      = (t, c)             => JsExpression.Invocation(JsExpression.Identifier("$CreateTaskCompletionSource"), t != null ? GetScriptType(t, TypeContext.GenericArgument, c.ResolveTypeParameter) : JsExpression.String("non-generic"));
			SetAsyncResult                                  = (t, v, c)          => JsExpression.Invocation(JsExpression.Identifier("$SetAsyncResult"), t, v ?? JsExpression.String("<<null>>"));
			SetAsyncException                               = (t, e, c)          => JsExpression.Invocation(JsExpression.Identifier("$SetAsyncException"), t, e);
			GetTaskFromTaskCompletionSource                 = (t, c)             => JsExpression.Invocation(JsExpression.Identifier("$GetTask"), t);
			ApplyConstructor                                = (c, a, x)          => JsExpression.Invocation(JsExpression.Identifier("$ApplyConstructor"), c, a);
			ShallowCopy                                     = (s, t, c)          => JsExpression.Invocation(JsExpression.Identifier("$ShallowCopy"), s, t);
			GetMember                                       = (m, c)             => JsExpression.Invocation(JsExpression.Identifier("$GetMember"), m is IMethodSymbol && ((IMethodSymbol)m).TypeArguments.Length > 0 ? new[] { GetScriptType(m.ContainingType, TypeContext.TypeOf, c.ResolveTypeParameter), JsExpression.String(m.MetadataName), JsExpression.ArrayLiteral(((IMethodSymbol)m).TypeArguments.Select(ta => GetScriptType(ta, TypeContext.GenericArgument, c.ResolveTypeParameter))) } : new[] { GetScriptType(m.ContainingType, TypeContext.TypeOf, c.ResolveTypeParameter), JsExpression.String(m.MetadataName) });
			GetExpressionForLocal                           = (n, a, t, c)       => JsExpression.Invocation(JsExpression.Identifier("$Local"), JsExpression.String(n), GetScriptType(t, TypeContext.TypeOf, c.ResolveTypeParameter), a);
			CloneValueType                                  = (v, t, c)          => JsExpression.Invocation(JsExpression.Identifier("$Clone"), v, GetScriptType(t, TypeContext.TypeOf, c.ResolveTypeParameter));
			InitializeField                                 = (t, n, m, v, c)    => JsExpression.Invocation(JsExpression.Identifier("$Init"), t, JsExpression.String(n), v);
		}

		public Func<ITypeSymbol, IRuntimeContext, JsExpression> GetTypeOf { get; set; }
		public Func<ITypeSymbol, IRuntimeContext, JsExpression> InstantiateType { get; set; }
		public Func<ITypeSymbol, IRuntimeContext, JsExpression> InstantiateTypeForUseAsTypeArgumentInInlineCode { get; set; }
		public Func<JsExpression, ITypeSymbol, ITypeSymbol, IRuntimeContext, JsExpression> TypeIs { get; set; }
		public Func<JsExpression, ITypeSymbol, ITypeSymbol, IRuntimeContext, JsExpression> TryDowncast { get; set; }
		public Func<JsExpression, ITypeSymbol, ITypeSymbol, IRuntimeContext, JsExpression> Downcast { get; set; }
		public Func<JsExpression, ITypeSymbol, ITypeSymbol, IRuntimeContext, JsExpression> Upcast { get; set; }
		public Func<JsExpression, IEnumerable<ITypeSymbol>, IRuntimeContext, JsExpression> InstantiateGenericMethod { get; set; }
		new public Func<JsExpression, JsExpression, IRuntimeContext, JsExpression> ReferenceEquals { get; set; }
		public Func<JsExpression, JsExpression, IRuntimeContext, JsExpression> ReferenceNotEquals { get; set; }
		public Func<JsExpression, IRuntimeContext, JsExpression> MakeException { get; set; }
		public Func<JsExpression, JsExpression, IRuntimeContext, JsExpression> IntegerDivision { get; set; }
		public Func<JsExpression, IRuntimeContext, JsExpression> FloatToInt { get; set; }
		public Func<JsExpression, JsExpression, IRuntimeContext, JsExpression> Coalesce { get; set; }
		public Func<JsExpression, IRuntimeContext, JsExpression> Lift { get; set; }
		public Func<JsExpression, IRuntimeContext, JsExpression> FromNullable { get; set; }
		public Func<JsExpression, JsExpression, IRuntimeContext, JsExpression> LiftedBooleanAnd { get; set; }
		public Func<JsExpression, JsExpression, IRuntimeContext, JsExpression> LiftedBooleanOr { get; set; }
		public Func<JsExpression, JsExpression, IRuntimeContext, JsExpression> Bind { get; set; }
		public Func<JsExpression, IRuntimeContext, JsExpression> BindFirstParameterToThis { get; set; }
		public Func<ITypeSymbol, IRuntimeContext, JsExpression> Default { get; set; }
		public Func<ITypeSymbol, IEnumerable<JsExpression>, IRuntimeContext, JsExpression> CreateArray { get; set; }
		public Func<JsExpression, ITypeSymbol, ITypeSymbol, IRuntimeContext, JsExpression> CloneDelegate { get; set; }
		public Func<IMethodSymbol, IEnumerable<JsExpression>, IRuntimeContext, JsExpression> CallBase { get; set; }
		public Func<IMethodSymbol, JsExpression, IRuntimeContext, JsExpression> BindBaseCall { get; set; }
		public Func<ITypeSymbol, JsExpression, JsExpression, JsExpression, IRuntimeContext, JsExpression> MakeEnumerator { get; set; }
		public Func<ITypeSymbol, JsExpression, IRuntimeContext, JsExpression> MakeEnumerable { get; set; }
		public Func<JsExpression, IEnumerable<JsExpression>, IRuntimeContext, JsExpression> GetMultiDimensionalArrayValue { get; set; }
		public Func<JsExpression, IEnumerable<JsExpression>, JsExpression, IRuntimeContext, JsExpression> SetMultiDimensionalArrayValue { get; set; }
		public Func<ITypeSymbol, IRuntimeContext, JsExpression> CreateTaskCompletionSource { get; set; }
		public Func<JsExpression, JsExpression, IRuntimeContext, JsExpression> SetAsyncResult { get; set; }
		public Func<JsExpression, JsExpression, IRuntimeContext, JsExpression> SetAsyncException { get; set; }
		public Func<JsExpression, IRuntimeContext, JsExpression> GetTaskFromTaskCompletionSource { get; set; }
		public Func<JsExpression, JsExpression, IRuntimeContext, JsExpression> ApplyConstructor { get; set; }
		public Func<JsExpression, JsExpression, IRuntimeContext, JsExpression> ShallowCopy { get; set; }
		public Func<ISymbol, IRuntimeContext, JsExpression> GetMember { get; set; }
		public Func<string, JsExpression, ITypeSymbol, IRuntimeContext, JsExpression> GetExpressionForLocal { get; set; }
		public Func<JsExpression, ITypeSymbol, IRuntimeContext, JsExpression> CloneValueType { get; set; }
		public Func<JsExpression, string, ISymbol, JsExpression, IRuntimeContext, JsExpression> InitializeField { get; set; }

		private JsExpression GetScriptType(ITypeSymbol type, TypeContext context, Func<ITypeParameterSymbol, JsExpression> resolveTypeParameter) {
			string contextName = GetTypeContextShortName(context);
			if (type.IsAnonymousType) {
				return JsExpression.Identifier(contextName + "_$Anonymous");
			}
			else if (type.TypeKind == TypeKind.ArrayType) {
				return JsExpression.Invocation(JsExpression.Identifier(contextName + "_$Array"), GetScriptType(((IArrayTypeSymbol)type).ElementType, TypeContext.GenericArgument, resolveTypeParameter));
			}
			else if (type is INamedTypeSymbol) {
				var nt = (INamedTypeSymbol)type;
				if (nt.IsUnboundGenericType) {
					return new JsTypeReferenceExpression(Common.CreateMockTypeDefinition(contextName + "_" + type.Name, Common.CreateMockAssembly()));
				}
				else {
					var allTypeArguments = nt.GetAllTypeArguments();
					if (allTypeArguments.Count > 0) {
						return JsExpression.Invocation(JsExpression.Identifier(contextName + "_$InstantiateGenericType"), new[] { new JsTypeReferenceExpression(Common.CreateMockTypeDefinition(type.Name, Common.CreateMockAssembly())) }.Concat(allTypeArguments.Select(a => GetScriptType(a, TypeContext.GenericArgument, resolveTypeParameter))));
					}
					else {
						return new JsTypeReferenceExpression(Common.CreateMockTypeDefinition(contextName + "_" + type.Name, Common.CreateMockAssembly()));
					}
				}
			}
			else if (type is ITypeParameterSymbol) {
				return resolveTypeParameter((ITypeParameterSymbol)type);
			}
			else {
				throw new ArgumentException("Unsupported type + " + type);
			}
		}

		JsExpression IRuntimeLibrary.TypeOf(ITypeSymbol type, IRuntimeContext context) {
			return GetTypeOf(type, context);
		}

		JsExpression IRuntimeLibrary.InstantiateType(ITypeSymbol type, IRuntimeContext context) {
			return InstantiateType(type, context);
		}

		JsExpression IRuntimeLibrary.InstantiateTypeForUseAsTypeArgumentInInlineCode(ITypeSymbol type, IRuntimeContext context) {
			return InstantiateTypeForUseAsTypeArgumentInInlineCode(type, context);
		}

		JsExpression IRuntimeLibrary.TypeIs(JsExpression expression, ITypeSymbol sourceType, ITypeSymbol targetType, IRuntimeContext context) {
			return TypeIs(expression, sourceType, targetType, context);
		}

		JsExpression IRuntimeLibrary.TryDowncast(JsExpression expression, ITypeSymbol sourceType, ITypeSymbol targetType, IRuntimeContext context) {
			return TryDowncast(expression, sourceType, targetType, context);
		}

		JsExpression IRuntimeLibrary.Downcast(JsExpression expression, ITypeSymbol sourceType, ITypeSymbol targetType, IRuntimeContext context) {
			return Downcast(expression, sourceType, targetType, context);
		}

		JsExpression IRuntimeLibrary.Upcast(JsExpression expression, ITypeSymbol sourceType, ITypeSymbol targetType, IRuntimeContext context) {
			return Upcast(expression, sourceType, targetType, context);
		}

		JsExpression IRuntimeLibrary.ReferenceEquals(JsExpression a, JsExpression b, IRuntimeContext context) {
			return ReferenceEquals(a, b, context);
		}

		JsExpression IRuntimeLibrary.ReferenceNotEquals(JsExpression a, JsExpression b, IRuntimeContext context) {
			return ReferenceNotEquals(a, b, context);
		}

		JsExpression IRuntimeLibrary.InstantiateGenericMethod(JsExpression type, IEnumerable<ITypeSymbol> typeArguments, IRuntimeContext context) {
			return InstantiateGenericMethod(type, typeArguments, context);
		}

		JsExpression IRuntimeLibrary.MakeException(JsExpression operand, IRuntimeContext context) {
			return MakeException(operand, context);
		}

		JsExpression IRuntimeLibrary.IntegerDivision(JsExpression numerator, JsExpression denominator, IRuntimeContext context) {
			return IntegerDivision(numerator, denominator, context);
		}

		JsExpression IRuntimeLibrary.FloatToInt(JsExpression operand, IRuntimeContext context) {
			return FloatToInt(operand, context);
		}

		JsExpression IRuntimeLibrary.Coalesce(JsExpression a, JsExpression b, IRuntimeContext context) {
			return Coalesce(a, b, context);
		}

		JsExpression IRuntimeLibrary.Lift(JsExpression expression, IRuntimeContext context) {
			return Lift(expression, context);
		}

		JsExpression IRuntimeLibrary.FromNullable(JsExpression expression, IRuntimeContext context) {
			return FromNullable(expression, context);
		}

		JsExpression IRuntimeLibrary.LiftedBooleanAnd(JsExpression a, JsExpression b, IRuntimeContext context) {
			return LiftedBooleanAnd(a, b, context);
		}

		JsExpression IRuntimeLibrary.LiftedBooleanOr(JsExpression a, JsExpression b, IRuntimeContext context) {
			return LiftedBooleanOr(a, b, context);
		}

		JsExpression IRuntimeLibrary.Bind(JsExpression function, JsExpression target, IRuntimeContext context) {
			return Bind(function, target, context);
		}

		JsExpression IRuntimeLibrary.BindFirstParameterToThis(JsExpression function, IRuntimeContext context) {
			return BindFirstParameterToThis(function, context);
		}

		JsExpression IRuntimeLibrary.Default(ITypeSymbol type, IRuntimeContext context) {
			return Default(type, context);
		}

		JsExpression IRuntimeLibrary.CreateArray(ITypeSymbol elementType, IEnumerable<JsExpression> size, IRuntimeContext context) {
			return CreateArray(elementType, size, context);
		}

		JsExpression IRuntimeLibrary.CloneDelegate(JsExpression source, ITypeSymbol sourceType, ITypeSymbol targetType, IRuntimeContext context) {
			return CloneDelegate(source, sourceType, targetType, context);
		}

		JsExpression IRuntimeLibrary.CallBase(IMethodSymbol method, IEnumerable<JsExpression> thisAndArguments, IRuntimeContext context) {
			return CallBase(method, thisAndArguments, context);
		}

		JsExpression IRuntimeLibrary.BindBaseCall(IMethodSymbol method, JsExpression @this, IRuntimeContext context) {
			return BindBaseCall(method, @this, context);
		}

		JsExpression IRuntimeLibrary.MakeEnumerator(ITypeSymbol yieldType, JsExpression moveNext, JsExpression getCurrent, JsExpression dispose, IRuntimeContext context) {
			return MakeEnumerator(yieldType, moveNext, getCurrent, dispose, context);
		}

		JsExpression IRuntimeLibrary.MakeEnumerable(ITypeSymbol yieldType, JsExpression getEnumerator, IRuntimeContext context) {
			return MakeEnumerable(yieldType, getEnumerator, context);
		}

		JsExpression IRuntimeLibrary.GetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, IRuntimeContext context) {
			return GetMultiDimensionalArrayValue(array, indices, context);
		}

		JsExpression IRuntimeLibrary.SetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, JsExpression value, IRuntimeContext context) {
			return SetMultiDimensionalArrayValue(array, indices, value, context);
		}

		JsExpression IRuntimeLibrary.CreateTaskCompletionSource(ITypeSymbol taskGenericArgument, IRuntimeContext context) {
			return CreateTaskCompletionSource(taskGenericArgument, context);
		}

		JsExpression IRuntimeLibrary.SetAsyncResult(JsExpression taskCompletionSource, JsExpression value, IRuntimeContext context) {
			return SetAsyncResult(taskCompletionSource, value, context);
		}

		JsExpression IRuntimeLibrary.SetAsyncException(JsExpression taskCompletionSource, JsExpression exception, IRuntimeContext context) {
			return SetAsyncException(taskCompletionSource, exception, context);
		}

		JsExpression IRuntimeLibrary.GetTaskFromTaskCompletionSource(JsExpression taskCompletionSource, IRuntimeContext context) {
			return GetTaskFromTaskCompletionSource(taskCompletionSource, context);
		}

		JsExpression IRuntimeLibrary.ApplyConstructor(JsExpression constructor, JsExpression argumentsArray, IRuntimeContext context) {
			return ApplyConstructor(constructor, argumentsArray, context);
		}

		JsExpression IRuntimeLibrary.ShallowCopy(JsExpression source, JsExpression target, IRuntimeContext context) {
			return ShallowCopy(source, target, context);
		}

		JsExpression IRuntimeLibrary.GetMember(ISymbol member, IRuntimeContext context) {
			return GetMember(member, context);
		}

		JsExpression IRuntimeLibrary.GetExpressionForLocal(string name, JsExpression accessor, ITypeSymbol type, IRuntimeContext context) {
			return GetExpressionForLocal(name, accessor, type, context);
		}

		JsExpression IRuntimeLibrary.CloneValueType(JsExpression value, ITypeSymbol type, IRuntimeContext context) {
			return CloneValueType(value, type, context);
		}

		JsExpression IRuntimeLibrary.InitializeField(JsExpression jsMember, string scriptName, ISymbol member, JsExpression initialValue, IRuntimeContext context) {
			return InitializeField(jsMember, scriptName, member, initialValue, context);
		}
	}
}