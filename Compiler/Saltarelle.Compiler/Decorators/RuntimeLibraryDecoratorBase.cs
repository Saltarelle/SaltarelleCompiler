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

		public JsExpression GetScriptType(IType type, TypeContext context) {
			return _prev.GetScriptType(type, context);
		}

		public JsExpression TypeIs(JsExpression expression, IType sourceType, IType targetType) {
			return _prev.TypeIs(expression, sourceType, targetType);
		}

		public JsExpression TryDowncast(JsExpression expression, IType sourceType, IType targetType) {
			return _prev.TryDowncast(expression, sourceType, targetType);
		}

		public JsExpression Downcast(JsExpression expression, IType sourceType, IType targetType) {
			return _prev.Downcast(expression, sourceType, targetType);
		}

		public JsExpression Upcast(JsExpression expression, IType sourceType, IType targetType) {
			return _prev.Upcast(expression, sourceType, targetType);
		}

		public JsExpression ReferenceEquals(JsExpression a, JsExpression b) {
			return _prev.ReferenceEquals(a, b);
		}

		public JsExpression ReferenceNotEquals(JsExpression a, JsExpression b) {
			return _prev.ReferenceNotEquals(a, b);
		}

		public JsExpression InstantiateGenericMethod(JsExpression method, IEnumerable<IType> typeArguments) {
			return _prev.InstantiateGenericMethod(method, typeArguments);
		}

		public JsExpression MakeException(JsExpression operand) {
			return _prev.MakeException(operand);
		}

		public JsExpression IntegerDivision(JsExpression numerator, JsExpression denominator) {
			return _prev.IntegerDivision(numerator, denominator);
		}

		public JsExpression FloatToInt(JsExpression operand) {
			return _prev.FloatToInt(operand);
		}

		public JsExpression Coalesce(JsExpression a, JsExpression b) {
			return _prev.Coalesce(a, b);
		}

		public JsExpression Lift(JsExpression expression) {
			return _prev.Lift(expression);
		}

		public JsExpression FromNullable(JsExpression expression) {
			return _prev.FromNullable(expression);
		}

		public JsExpression LiftedBooleanAnd(JsExpression a, JsExpression b) {
			return _prev.LiftedBooleanAnd(a, b);
		}

		public JsExpression LiftedBooleanOr(JsExpression a, JsExpression b) {
			return _prev.LiftedBooleanOr(a, b);
		}

		public JsExpression Bind(JsExpression function, JsExpression target) {
			return _prev.Bind(function, target);
		}

		public JsExpression BindFirstParameterToThis(JsExpression function) {
			return _prev.BindFirstParameterToThis(function);
		}

		public JsExpression Default(IType type) {
			return _prev.Default(type);
		}

		public JsExpression CreateArray(IType elementType, IEnumerable<JsExpression> sizes) {
			return _prev.CreateArray(elementType, sizes);
		}

		public JsExpression CloneDelegate(JsExpression source, IType sourceType, IType targetType) {
			return _prev.CloneDelegate(source, sourceType, targetType);
		}

		public JsExpression CallBase(IType baseType, string methodName, IList<IType> typeArguments, IEnumerable<JsExpression> thisAndArguments) {
			return _prev.CallBase(baseType, methodName, typeArguments, thisAndArguments);
		}

		public JsExpression BindBaseCall(IType baseType, string methodName, IList<IType> typeArguments, JsExpression @this) {
			return _prev.BindBaseCall(baseType, methodName, typeArguments, @this);
		}

		public JsExpression MakeEnumerator(IType yieldType, JsExpression moveNext, JsExpression getCurrent, JsExpression dispose) {
			return _prev.MakeEnumerator(yieldType, moveNext, getCurrent, dispose);
		}

		public JsExpression MakeEnumerable(IType yieldType, JsExpression getEnumerator) {
			return _prev.MakeEnumerable(yieldType, getEnumerator);
		}

		public JsExpression GetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices) {
			return _prev.GetMultiDimensionalArrayValue(array, indices);
		}

		public JsExpression SetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, JsExpression value) {
			return _prev.SetMultiDimensionalArrayValue(array, indices, value);
		}

		public JsExpression CreateTaskCompletionSource(IType taskGenericArgument) {
			return _prev.CreateTaskCompletionSource(taskGenericArgument);
		}

		public JsExpression SetAsyncResult(JsExpression taskCompletionSource, JsExpression value) {
			return _prev.SetAsyncResult(taskCompletionSource, value);
		}

		public JsExpression SetAsyncException(JsExpression taskCompletionSource, JsExpression exception) {
			return _prev.SetAsyncException(taskCompletionSource, exception);
		}

		public JsExpression GetTaskFromTaskCompletionSource(JsExpression taskCompletionSource) {
			return _prev.GetTaskFromTaskCompletionSource(taskCompletionSource);
		}
	}
}
