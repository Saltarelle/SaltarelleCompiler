using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler {
	public interface IRuntimeLibrary {
		/// <summary>
		/// Returns the JS expression that "typeof(type)" should be compiled to.
		/// </summary>
		/// <param name="type">Type to return an expression for.</param>
		JsExpression TypeOf(IType type, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that references a type. This might mean a simple name, a generic instantiation, or something else.
		/// </summary>
		/// <param name="type">Type to return an expression for.</param>
		/// <param name="context">Current context.</param>
		JsExpression InstantiateType(IType type, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that creates a type reference as it is supposed to be when a type is being used as a type argument for an InlineCode method.
		/// </summary>
		/// <param name="type">Type to return an expression for.</param>
		/// <param name="context">Current context.</param>
		JsExpression InstantiateTypeForUseAsTypeArgumentInInlineCode(IType type, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that determines if an expression is of a type (equivalent to C# "is").
		/// This might also represent an unboxing, in which case it must be verified that (any non-null) object can be converted to the target type before returning true.
		/// </summary>
		JsExpression TypeIs(JsExpression expression, IType sourceType, IType targetType, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that casts an expression to a specified type, or returns null if the expression is not of that type (equivalent to C# "as").
		/// This might also represent an unboxing, in which null should be returned if the object can be converted to the target type (eg, when unboxing an integer it must be verified that there are no decimal places in the number).
		/// </summary>
		JsExpression TryDowncast(JsExpression expression, IType sourceType, IType targetType, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that casts a class to a derived class, or throws an exception if the cast is not possible.
		/// This might also represent an unboxing, in which case it must be verified that (any non-null) object can be converted to the target type (eg, when unboxing an integer it must be verified that there are no decimal places in the number).
		/// </summary>
		JsExpression Downcast(JsExpression expression, IType sourceType, IType targetType, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that performs an upcast (equivalent to (IList)list, where list is a List). Note that this might also represent a generic variance conversion.
		/// </summary>
		JsExpression Upcast(JsExpression expression, IType sourceType, IType targetType, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that determines if two reference values are equal.
		/// </summary>
		JsExpression ReferenceEquals(JsExpression a, JsExpression b, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that determines if two reference values are not equal.
		/// </summary>
		JsExpression ReferenceNotEquals(JsExpression a, JsExpression b, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that will instantiate a generic method.
		/// </summary>
		JsExpression InstantiateGenericMethod(JsExpression method, IEnumerable<IType> typeArguments, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that will convert a given expression to an exception. This is used to be able to throw a JS string and catch it as an Exception.
		/// </summary>
		JsExpression MakeException(JsExpression operand, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that will perform integer division.
		/// </summary>
		JsExpression IntegerDivision(JsExpression numerator, JsExpression denominator, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that converts a floating-point number to an integer.
		/// </summary>
		JsExpression FloatToInt(JsExpression operand, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that will perform null coalesce (C#: a ?? b).
		/// </summary>
		JsExpression Coalesce(JsExpression a, JsExpression b, IRuntimeContext context);

		/// <summary>
		/// Generates a lifted version of an expression.
		/// </summary>
		/// <param name="expression">Expression to lift. This will always be an invocation, or a (unary or binary) operator.</param>
		JsExpression Lift(JsExpression expression, IRuntimeContext context);

		/// <summary>
		/// Generates an expression that converts from a nullable type to a non-nullable type (should return the passed-in argument if non-null, throw if it is null).
		/// </summary>
		/// <param name="expression">Expression to ensure that it is non-null.</param>
		JsExpression FromNullable(JsExpression expression, IRuntimeContext context);

		/// <summary>
		/// Generates a call to the lifted boolean &amp; operator, which has the same semantics as the SQL AND operator.
		/// </summary>
		JsExpression LiftedBooleanAnd(JsExpression a, JsExpression b, IRuntimeContext context);

		/// <summary>
		/// Generates a call to the lifted boolean | operator, which has the same semantics as the SQL OR operator.
		/// </summary>
		JsExpression LiftedBooleanOr(JsExpression a, JsExpression b, IRuntimeContext context);

		/// <summary>
		/// Bind a function to a target that will become "this" inside the function.
		/// </summary>
		JsExpression Bind(JsExpression function, JsExpression target, IRuntimeContext context);

		/// <summary>
		/// Returns an expression that invokes the specified function, but the context ('this') in the Javascript will be the first parameter. Used eg. in the delegate argument to jQuery.each().
		/// </summary>
		JsExpression BindFirstParameterToThis(JsExpression function, IRuntimeContext context);

		/// <summary>
		/// Generates an expression that returns the default value for a type (C#: default(T)).
		/// </summary>
		JsExpression Default(IType type, IRuntimeContext context);

		/// <summary>
		/// Generates an expression that creates an array of a specified size (one item for each rank), with all elements initialized to their default values.
		/// </summary>
		JsExpression CreateArray(IType elementType, IEnumerable<JsExpression> sizes, IRuntimeContext context);

		/// <summary>
		/// Generates an expression that copies an existing delegate to a new one.
		/// </summary>
		JsExpression CloneDelegate(JsExpression source, IType sourceType, IType targetType, IRuntimeContext context);

		/// <summary>
		/// Generates an expression to call a base implementation of an overridden method. Note that implementations must handle semantics such as ExpandParams.
		/// </summary>
		/// <param name="method">The method that is being invoked (a SpecializedMethod in case of generic methods).</param>
		/// <param name="thisAndArguments">Arguments to the method, including "this" as the first element.</param>
		/// <param name="context">Current context.</param>
		JsExpression CallBase(IMethod method, IEnumerable<JsExpression> thisAndArguments, IRuntimeContext context);

		/// <summary>
		/// Generates an expression to bind a base implementation of an overridden method. Used when converting a method group to a delegate.
		/// </summary>
		/// <param name="method">The method that is being invoked (a SpecializedMethod in case of generic methods).</param>
		/// <param name="@this">Expression to use for "this" (target of the method call).</param>
		/// <param name="context">Current context.</param>
		JsExpression BindBaseCall(IMethod method, JsExpression @this, IRuntimeContext context);

		/// <summary>
		/// Generates an object that implements the <see cref="IEnumerator{T}"/> interface using the supplied methods.
		/// </summary>
		/// <param name="yieldType">The yield type of the enumerator. <see cref="object"/> if the enumerator is non-generic.</param>
		/// <param name="moveNext">Function to invoke when <see cref="IEnumerator.MoveNext"/> is invoked on the enumerator.</param>
		/// <param name="getCurrent">Function that returns the current value of the enumerator.</param>
		/// <param name="dispose">Function to invoke when <see cref="IDisposable.Dispose"/> is invoked on the enumerator, or null if no dispose is required.</param>
		/// <param name="context">Current context.</param>
		JsExpression MakeEnumerator(IType yieldType, JsExpression moveNext, JsExpression getCurrent, JsExpression dispose, IRuntimeContext context);

		/// <summary>
		/// Generates an object that implements the <see cref="IEnumerable{T}"/> interface using the supplied methods.
		/// </summary>
		/// <param name="yieldType">The yield type of the enumerable. <see cref="object"/> if the enumerable is non-generic.</param>
		/// <param name="getEnumerator">Function to invoke when <see cref="IEnumerable.GetEnumerator"/> is invoked on the enumerator.</param>
		/// <param name="context">Current context.</param>
		JsExpression MakeEnumerable(IType yieldType, JsExpression getEnumerator, IRuntimeContext context);

		/// <summary>
		/// Generates an expression that gets the value at a specific index of a multi-dimensional array.
		/// </summary>
		JsExpression GetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, IRuntimeContext context);

		/// <summary>
		/// Generates an expression that sets the value at a specific index of a multi-dimensional array.
		/// </summary>
		JsExpression SetMultiDimensionalArrayValue(JsExpression array, IEnumerable<JsExpression> indices, JsExpression value, IRuntimeContext context);

		/// <summary>
		/// Generates an expression that creates a TaskCompletionSource.
		/// </summary>
		/// <param name="taskGenericArgument">If the method being built returns a <c>Task&lt;T&gt;</c>, this parameter will contain <c>T</c>. If the method returns a non-generic task, this parameter will be null.</param>
		/// <param name="context">Current context.</param>
		JsExpression CreateTaskCompletionSource(IType taskGenericArgument, IRuntimeContext context);

		/// <summary>
		/// Generates an expression that applies the result of an async method to a TaskCompletionSource.
		/// </summary>
		/// <param name="taskCompletionSource">The TaskCompletionSource instance used in the method.</param>
		/// <param name="value">Value to return. Will be null if the method does not return a value (in which case it must be a method returning a non-generic task).</param>
		JsExpression SetAsyncResult(JsExpression taskCompletionSource, JsExpression value, IRuntimeContext context);

		/// <summary>
		/// Generates an expression that applies an exception to a TaskCompletionSource.
		/// </summary>
		/// <param name="taskCompletionSource">The TaskCompletionSource instance used in the method.</param>
		/// <param name="exception">The exception to return. Note that this may be any object (not necessarily an Exception instance).</param>
		JsExpression SetAsyncException(JsExpression taskCompletionSource, JsExpression exception, IRuntimeContext context);

		/// <summary>
		/// Generates an expression that retrieves the Task instance from a TaskCompletionSource.
		/// </summary>
		/// <param name="taskCompletionSource">The TaskCompletionSource instance used in the method.</param>
		JsExpression GetTaskFromTaskCompletionSource(JsExpression taskCompletionSource, IRuntimeContext context);

		/// <summary>
		/// Generates an expression that invokes a constructor via apply rather than a normal call.
		/// </summary>
		/// <param name="constructor">The constructor to apply.</param>
		/// <param name="argumentsArray">Arguments for the call (should evaluate to an array).</param>
		JsExpression ApplyConstructor(JsExpression constructor, JsExpression argumentsArray, IRuntimeContext context);

		/// <summary>
		/// Generates an expression that copies all properties from an object to another one (not cloning nested objects).
		/// </summary>
		/// <param name="source">The object whose properties are examined.</param>
		/// <param name="target">The object that the properties from <paramref name="source"/> will be added to.</param>
		JsExpression ShallowCopy(JsExpression source, JsExpression target, IRuntimeContext context);
	}
}
