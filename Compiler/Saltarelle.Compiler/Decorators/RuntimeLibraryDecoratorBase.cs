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

		public virtual JsExpression GetScriptType(IType type, TypeContext context) {
			return _prev.GetScriptType(type, context);
		}

		public virtual JsExpression TypeIs(JsExpression expression, IType sourceType, IType targetType) {
			return _prev.TypeIs(expression, sourceType, targetType);
		}

		public virtual JsExpression TryDowncast(JsExpression expression, IType sourceType, IType targetType) {
			return _prev.TryDowncast(expression, sourceType, targetType);
		}

		public virtual JsExpression Downcast(JsExpression expression, IType sourceType, IType targetType) {
			return _prev.Downcast(expression, sourceType, targetType);
		}

		public virtual JsExpression Upcast(JsExpression expression, IType sourceType, IType targetType) {
			return _prev.Upcast(expression, sourceType, targetType);
		}

		public virtual JsExpression ReferenceEquals(JsExpression a, JsExpression b) {
			return _prev.ReferenceEquals(a, b);
		}

		public virtual JsExpression ReferenceNotEquals(JsExpression a, JsExpression b) {
			return _prev.ReferenceNotEquals(a, b);
		}

		public virtual JsExpression InstantiateGenericMethod(JsExpression method, IEnumerable<IType> typeArguments) {
			return _prev.InstantiateGenericMethod(method, typeArguments);
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

		public virtual JsExpression Default(IType type) {
			return _prev.Default(type);
		}

		public virtual JsExpression CreateArray(IType elementType, IEnumerable<JsExpression> sizes) {
			return _prev.CreateArray(elementType, sizes);
		}

		public virtual JsExpression CloneDelegate(JsExpression source, IType sourceType, IType targetType) {
			return _prev.CloneDelegate(source, sourceType, targetType);
		}

		public virtual JsExpression CallBase(IType baseType, string methodName, IList<IType> typeArguments, IEnumerable<JsExpression> thisAndArguments) {
			return _prev.CallBase(baseType, methodName, typeArguments, thisAndArguments);
		}

		public virtual JsExpression BindBaseCall(IType baseType, string methodName, IList<IType> typeArguments, JsExpression @this) {
			return _prev.BindBaseCall(baseType, methodName, typeArguments, @this);
		}

		public virtual JsExpression MakeEnumerator(IType yieldType, JsExpression moveNext, JsExpression getCurrent, JsExpression dispose) {
			return _prev.MakeEnumerator(yieldType, moveNext, getCurrent, dispose);
		}

		public virtual JsExpression MakeEnumerable(IType yieldType, JsExpression getEnumerator) {
			return _prev.MakeEnumerable(yieldType, getEnumerator);
		}

		public virtual JsExpression GetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices) {
			return _prev.GetMultiDimensionalArrayValue(array, indices);
		}

		public virtual JsExpression SetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, JsExpression value) {
			return _prev.SetMultiDimensionalArrayValue(array, indices, value);
		}

		public virtual JsExpression CreateTaskCompletionSource(IType taskGenericArgument) {
			return _prev.CreateTaskCompletionSource(taskGenericArgument);
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
	}
}
