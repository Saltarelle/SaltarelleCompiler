using System;
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

		public virtual JsExpression TypeOf(IType type, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.TypeOf(type, resolveTypeParameter);
		}

		public virtual JsExpression InstantiateType(IType type, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.InstantiateType(type, resolveTypeParameter);
		}

		public virtual JsExpression InstantiateTypeForUseAsTypeArgumentInInlineCode(IType type, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.InstantiateTypeForUseAsTypeArgumentInInlineCode(type, resolveTypeParameter);
		}

		public virtual JsExpression TypeIs(JsExpression expression, IType sourceType, IType targetType, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.TypeIs(expression, sourceType, targetType, resolveTypeParameter);
		}

		public virtual JsExpression TryDowncast(JsExpression expression, IType sourceType, IType targetType, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.TryDowncast(expression, sourceType, targetType, resolveTypeParameter);
		}

		public virtual JsExpression Downcast(JsExpression expression, IType sourceType, IType targetType, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.Downcast(expression, sourceType, targetType, resolveTypeParameter);
		}

		public virtual JsExpression Upcast(JsExpression expression, IType sourceType, IType targetType, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.Upcast(expression, sourceType, targetType, resolveTypeParameter);
		}

		public virtual JsExpression ReferenceEquals(JsExpression a, JsExpression b) {
			return _prev.ReferenceEquals(a, b);
		}

		public virtual JsExpression ReferenceNotEquals(JsExpression a, JsExpression b) {
			return _prev.ReferenceNotEquals(a, b);
		}

		public virtual JsExpression InstantiateGenericMethod(JsExpression method, IEnumerable<IType> typeArguments, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.InstantiateGenericMethod(method, typeArguments, resolveTypeParameter);
		}

		public virtual JsExpression MakeException(JsExpression operand) {
			return _prev.MakeException(operand);
		}

		public virtual JsExpression IntegerDivision(JsExpression numerator, JsExpression denominator) {
			return _prev.IntegerDivision(numerator, denominator);
		}

		public virtual JsExpression FloatToInt(JsExpression operand) {
			return _prev.FloatToInt(operand);
		}

		public virtual JsExpression Coalesce(JsExpression a, JsExpression b) {
			return _prev.Coalesce(a, b);
		}

		public virtual JsExpression Lift(JsExpression expression) {
			return _prev.Lift(expression);
		}

		public virtual JsExpression FromNullable(JsExpression expression) {
			return _prev.FromNullable(expression);
		}

		public virtual JsExpression LiftedBooleanAnd(JsExpression a, JsExpression b) {
			return _prev.LiftedBooleanAnd(a, b);
		}

		public virtual JsExpression LiftedBooleanOr(JsExpression a, JsExpression b) {
			return _prev.LiftedBooleanOr(a, b);
		}

		public virtual JsExpression Bind(JsExpression function, JsExpression target) {
			return _prev.Bind(function, target);
		}

		public virtual JsExpression BindFirstParameterToThis(JsExpression function) {
			return _prev.BindFirstParameterToThis(function);
		}

		public virtual JsExpression Default(IType type, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.Default(type, resolveTypeParameter);
		}

		public virtual JsExpression CreateArray(IType elementType, IEnumerable<JsExpression> sizes, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.CreateArray(elementType, sizes, resolveTypeParameter);
		}

		public virtual JsExpression CloneDelegate(JsExpression source, IType sourceType, IType targetType, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.CloneDelegate(source, sourceType, targetType, resolveTypeParameter);
		}

		public virtual JsExpression CallBase(IMethod method, IEnumerable<JsExpression> thisAndArguments, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.CallBase(method, thisAndArguments, resolveTypeParameter);
		}

		public virtual JsExpression BindBaseCall(IMethod method, JsExpression @this, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.BindBaseCall(method, @this, resolveTypeParameter);
		}

		public virtual JsExpression MakeEnumerator(IType yieldType, JsExpression moveNext, JsExpression getCurrent, JsExpression dispose, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.MakeEnumerator(yieldType, moveNext, getCurrent, dispose, resolveTypeParameter);
		}

		public virtual JsExpression MakeEnumerable(IType yieldType, JsExpression getEnumerator, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.MakeEnumerable(yieldType, getEnumerator, resolveTypeParameter);
		}

		public virtual JsExpression GetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices) {
			return _prev.GetMultiDimensionalArrayValue(array, indices);
		}

		public virtual JsExpression SetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, JsExpression value) {
			return _prev.SetMultiDimensionalArrayValue(array, indices, value);
		}

		public virtual JsExpression CreateTaskCompletionSource(IType taskGenericArgument, Func<ITypeParameter, JsExpression> resolveTypeParameter) {
			return _prev.CreateTaskCompletionSource(taskGenericArgument, resolveTypeParameter);
		}

		public virtual JsExpression SetAsyncResult(JsExpression taskCompletionSource, JsExpression value) {
			return _prev.SetAsyncResult(taskCompletionSource, value);
		}

		public virtual JsExpression SetAsyncException(JsExpression taskCompletionSource, JsExpression exception) {
			return _prev.SetAsyncException(taskCompletionSource, exception);
		}

		public virtual JsExpression GetTaskFromTaskCompletionSource(JsExpression taskCompletionSource) {
			return _prev.GetTaskFromTaskCompletionSource(taskCompletionSource);
		}

		public virtual JsExpression ApplyConstructor(JsExpression constructor, JsExpression argumentsArray) {
			return _prev.ApplyConstructor(constructor, argumentsArray);
		}
	}
}
