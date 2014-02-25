﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Decorators {
	public abstract class RuntimeLibraryDecoratorBase : IRuntimeLibrary {
		private readonly IRuntimeLibrary _prev;

		protected RuntimeLibraryDecoratorBase(IRuntimeLibrary prev) {
			_prev = prev;
		}

		public virtual JsExpression TypeOf(IType type, IRuntimeContext context) {
			return _prev.TypeOf(type, context);
		}

		public virtual JsExpression InstantiateType(IType type, IRuntimeContext context) {
			return _prev.InstantiateType(type, context);
		}

		public virtual JsExpression InstantiateTypeForUseAsTypeArgumentInInlineCode(IType type, IRuntimeContext context) {
			return _prev.InstantiateTypeForUseAsTypeArgumentInInlineCode(type, context);
		}

		public virtual JsExpression TypeIs(JsExpression expression, IType sourceType, IType targetType, IRuntimeContext context) {
			return _prev.TypeIs(expression, sourceType, targetType, context);
		}

		public virtual JsExpression TryDowncast(JsExpression expression, IType sourceType, IType targetType, IRuntimeContext context) {
			return _prev.TryDowncast(expression, sourceType, targetType, context);
		}

		public virtual JsExpression Downcast(JsExpression expression, IType sourceType, IType targetType, IRuntimeContext context) {
			return _prev.Downcast(expression, sourceType, targetType, context);
		}

		public virtual JsExpression Upcast(JsExpression expression, IType sourceType, IType targetType, IRuntimeContext context) {
			return _prev.Upcast(expression, sourceType, targetType, context);
		}

		public virtual JsExpression ReferenceEquals(JsExpression a, JsExpression b, IRuntimeContext context) {
			return _prev.ReferenceEquals(a, b, context);
		}

		public virtual JsExpression ReferenceNotEquals(JsExpression a, JsExpression b, IRuntimeContext context) {
			return _prev.ReferenceNotEquals(a, b, context);
		}

		public virtual JsExpression InstantiateGenericMethod(JsExpression method, IEnumerable<IType> typeArguments, IRuntimeContext context) {
			return _prev.InstantiateGenericMethod(method, typeArguments, context);
		}

		public virtual JsExpression MakeException(JsExpression operand, IRuntimeContext context) {
			return _prev.MakeException(operand, context);
		}

		public virtual JsExpression IntegerDivision(JsExpression numerator, JsExpression denominator, IRuntimeContext context) {
			return _prev.IntegerDivision(numerator, denominator, context);
		}

		public virtual JsExpression FloatToInt(JsExpression operand, IRuntimeContext context) {
			return _prev.FloatToInt(operand, context);
		}

		public virtual JsExpression Coalesce(JsExpression a, JsExpression b, IRuntimeContext context) {
			return _prev.Coalesce(a, b, context);
		}

		public virtual JsExpression Lift(JsExpression expression, IRuntimeContext context) {
			return _prev.Lift(expression, context);
		}

		public virtual JsExpression FromNullable(JsExpression expression, IRuntimeContext context) {
			return _prev.FromNullable(expression, context);
		}

		public virtual JsExpression LiftedBooleanAnd(JsExpression a, JsExpression b, IRuntimeContext context) {
			return _prev.LiftedBooleanAnd(a, b, context);
		}

		public virtual JsExpression LiftedBooleanOr(JsExpression a, JsExpression b, IRuntimeContext context) {
			return _prev.LiftedBooleanOr(a, b, context);
		}

		public virtual JsExpression Bind(JsExpression function, JsExpression target, IRuntimeContext context) {
			return _prev.Bind(function, target, context);
		}

		public virtual JsExpression BindFirstParameterToThis(JsExpression function, IRuntimeContext context) {
			return _prev.BindFirstParameterToThis(function, context);
		}

		public virtual JsExpression Default(IType type, IRuntimeContext context) {
			return _prev.Default(type, context);
		}

		public virtual JsExpression CreateArray(IType elementType, IEnumerable<JsExpression> sizes, IRuntimeContext context) {
			return _prev.CreateArray(elementType, sizes, context);
		}

		public virtual JsExpression CloneDelegate(JsExpression source, IType sourceType, IType targetType, IRuntimeContext context) {
			return _prev.CloneDelegate(source, sourceType, targetType, context);
		}

		public virtual JsExpression CallBase(IMethod method, IEnumerable<JsExpression> thisAndArguments, IRuntimeContext context) {
			return _prev.CallBase(method, thisAndArguments, context);
		}

		public virtual JsExpression BindBaseCall(IMethod method, JsExpression @this, IRuntimeContext context) {
			return _prev.BindBaseCall(method, @this, context);
		}

		public virtual JsExpression MakeEnumerator(IType yieldType, JsExpression moveNext, JsExpression getCurrent, JsExpression dispose, IRuntimeContext context) {
			return _prev.MakeEnumerator(yieldType, moveNext, getCurrent, dispose, context);
		}

		public virtual JsExpression MakeEnumerable(IType yieldType, JsExpression getEnumerator, IRuntimeContext context) {
			return _prev.MakeEnumerable(yieldType, getEnumerator, context);
		}

		public virtual JsExpression GetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, IRuntimeContext context) {
			return _prev.GetMultiDimensionalArrayValue(array, indices, context);
		}

		public virtual JsExpression SetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, JsExpression value, IRuntimeContext context) {
			return _prev.SetMultiDimensionalArrayValue(array, indices, value, context);
		}

		public virtual JsExpression CreateTaskCompletionSource(IType taskGenericArgument, IRuntimeContext context) {
			return _prev.CreateTaskCompletionSource(taskGenericArgument, context);
		}

		public virtual JsExpression SetAsyncResult(JsExpression taskCompletionSource, JsExpression value, IRuntimeContext context) {
			return _prev.SetAsyncResult(taskCompletionSource, value, context);
		}

		public virtual JsExpression SetAsyncException(JsExpression taskCompletionSource, JsExpression exception, IRuntimeContext context) {
			return _prev.SetAsyncException(taskCompletionSource, exception, context);
		}

		public virtual JsExpression GetTaskFromTaskCompletionSource(JsExpression taskCompletionSource, IRuntimeContext context) {
			return _prev.GetTaskFromTaskCompletionSource(taskCompletionSource, context);
		}

		public virtual JsExpression ApplyConstructor(JsExpression constructor, JsExpression argumentsArray, IRuntimeContext context) {
			return _prev.ApplyConstructor(constructor, argumentsArray, context);
		}

		public virtual JsExpression ShallowCopy(JsExpression source, JsExpression target, IRuntimeContext context) {
			return _prev.ShallowCopy(source, target, context);
		}

		public virtual JsExpression GetMember(IMember member, IRuntimeContext context) {
			return _prev.GetMember(member, context);
		}

		public virtual JsExpression GetExpressionForLocal(string name, JsExpression accessor, IType type, IRuntimeContext context) {
			return _prev.GetExpressionForLocal(name, accessor, type, context);
		}

		public JsExpression CloneValueType(JsExpression value, IType type, IRuntimeContext context) {
			return _prev.CloneValueType(value, type, context);
		}

		public JsExpression InitializeField(JsExpression jsThis, string scriptName, IMember member, JsExpression initialValue, IRuntimeContext context) {
			return _prev.InitializeField(jsThis, scriptName, member, initialValue, context);
		}
	}
}
