using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler {
	public interface IRuntimeLibrary {
		/// <summary>
		/// Returns an expression that determines if an expression is of a type (equivalent to C# "is").
		/// </summary>
		JsExpression TypeIs(JsExpression expression, JsExpression targetType);

		/// <summary>
		/// Returns an expression that casts an expression to a specified type, or returns null if the expression is not of that type (equivalent to C# "as").
		/// </summary>
		JsExpression TryCast(JsExpression expression, JsExpression targetType);

		/// <summary>
		/// Returns an expression that casts a class to a derived class, or throws an exception if the cast is not possible.
		/// </summary>
		JsExpression Downcast(JsExpression expression, JsExpression targetType);

		/// <summary>
		/// Returns an expression that should unbox a value, or return null if the value to unbox is null. This means verifying that (any non-null) object can be converted to the target type (eg, when unboxing an integer it must be verified that there are no decimal places in the number).
		/// </summary>
		/// <param name="obj">Object to unbox.</param>
		/// <param name="targetType">Target type for the unboxing.</param>
		JsExpression Unbox(JsExpression obj, JsExpression targetType);

		/// <summary>
		/// Returns an expression that should try to unbox a value, or return null if the value could for some reason not be unboxed (eg, when the input is null, or when unboxing an integer it must be verified that there are no decimal places in the number).
		/// </summary>
		/// <param name="obj">Object to try to unbox.</param>
		/// <param name="targetType">Target type for the unboxing.</param>
		JsExpression TryUnbox(JsExpression obj, JsExpression targetType);

		/// <summary>
		/// Returns an expression that performs an implicit reference conversion (equivalent to (IList)list, where list is a List).
		/// </summary>
		JsExpression ImplicitReferenceConversion(JsExpression expression, JsExpression targetType);

		/// <summary>
		/// Returns an expression that will instantiate a generic type.
		/// </summary>
		JsExpression InstantiateGenericType(JsExpression type, IEnumerable<JsExpression> typeArguments);

		/// <summary>
		/// Returns an expression that will instantiate a generic method.
		/// </summary>
		JsExpression InstantiateGenericMethod(JsExpression method, IEnumerable<JsExpression> typeArguments);

		/// <summary>
		/// Returns an expression that will convert a given expression to an exception. This is used to be able to throw a JS string and catch it as an Exception.
		/// </summary>
		JsExpression MakeException(JsExpression operand);

		/// <summary>
		/// Returns an expression that will perform integer division.
		/// </summary>
		JsExpression IntegerDivision(JsExpression numerator, JsExpression denominator);

		/// <summary>
		/// Returns an expression that converts a floating-point number to an integer.
		/// </summary>
		JsExpression FloatToInt(JsExpression operand);

		/// <summary>
		/// Returns an expression that will perform null coalesce (C#: a ?? b).
		/// </summary>
		JsExpression Coalesce(JsExpression a, JsExpression b);

		/// <summary>
		/// Generates a lifted version of an expression.
		/// </summary>
		/// <param name="expression">Expression to lift. This will always be an invocation, or a (unary or binary) operator.</param>
		JsExpression Lift(JsExpression expression);

		/// <summary>
		/// Generates an expression that converts from a nullable type to a non-nullable type (should return the passed-in argument if non-null, throw if it is null).
		/// </summary>
		/// <param name="expression">Expression to ensure that it is non-null.</param>
		JsExpression FromNullable(JsExpression expression);

		/// <summary>
		/// Generates a call to the lifted boolean &amp; operator, which has the same semantics as the SQL AND operator.
		/// </summary>
		JsExpression LiftedBooleanAnd(JsExpression a, JsExpression b);

		/// <summary>
		/// Generates a call to the lifted boolean | operator, which has the same semantics as the SQL OR operator.
		/// </summary>
		JsExpression LiftedBooleanOr(JsExpression a, JsExpression b);

		/// <summary>
		/// Bind a function to a target that will become "this" inside the function.
		/// </summary>
		JsExpression Bind(JsExpression function, JsExpression target);

		/// <summary>
		/// Generates an expression that returns the default value for a type (C#: default(T)).
		/// </summary>
		JsExpression Default(JsExpression type);

		/// <summary>
		/// Generates an expression that creates an array of a specified size, with all elements uninitialized.
		/// </summary>
		JsExpression CreateArray(JsExpression size);

		/// <summary>
		/// Generates an expression to call a base implementation of an overridden method
		/// </summary>
		/// <param name="baseType">Type whose implementation of the method to invoke.</param>
		/// <param name="methodName">Name of the method to invoke.</param>
		/// <param name="typeArguments">Type arguments for the method, or an empty enumerable.</param>
		/// <param name="thisAndArguments">Arguments to the method, including "this" as the first element.</param>
		JsExpression CallBase(JsExpression baseType, string methodName, IEnumerable<JsExpression> typeArguments, IEnumerable<JsExpression> thisAndArguments);

		/// <summary>
		/// Generates an expression to bind a base implementation of an overridden method. Used when converting a method group to a delegate.
		/// </summary>
		/// <param name="baseType">Type whose implementation of the method to bind.</param>
		/// <param name="methodName">Name of the method to bind.</param>
		/// <param name="typeArguments">Type arguments for the method, or an empty enumerable.</param>
		/// <param name="@this">Expression to use for "this" (target of the method call).</param>
		JsExpression BindBaseCall(JsExpression baseType, string methodName, IEnumerable<JsExpression> typeArguments, JsExpression @this);
	}
}
