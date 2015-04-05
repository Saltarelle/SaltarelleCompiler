﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Diagnostics.Contracts
{
	[ScriptNamespace("ss")]
	[Imported(ObeysTypeSystem = true)]
	public static class Contract
	{
		#region User Methods

		#region Assume

		/// <summary>
		/// Instructs code analysis tools to assume the expression <paramref name="condition"/> is true even if it can not be statically proven to always be true.
		/// </summary>
		/// <param name="condition">Expression to assume will always be true.</param>
		/// <remarks>
		/// At runtime this is equivalent to an <seealso cref="System.Diagnostics.Contracts.Contract.Assert(bool)"/>.
		/// </remarks>
		[Pure]
		[Conditional("DEBUG"), Conditional("CONTRACTS_FULL")]
		[InlineCode("{$System.Diagnostics.Contracts.Contract}.Assert(5, function () {{ return {condition}; }})")]
		public static void Assume(bool condition) { }

		/// <summary>
		/// Instructs code analysis tools to assume the expression <paramref name="condition"/> is true even if it can not be statically proven to always be true.
		/// </summary>
		/// <param name="condition">Expression to assume will always be true.</param>
		/// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
		/// <remarks>
		/// At runtime this is equivalent to an <seealso cref="System.Diagnostics.Contracts.Contract.Assert(bool)"/>.
		/// </remarks>
		[Pure]
		[Conditional("DEBUG"), Conditional("CONTRACTS_FULL")]
		[InlineCode("{$System.Diagnostics.Contracts.Contract}.Assert(5, function () {{ return {condition}; }}, {userMessage})")]
		public static void Assume(bool condition, String userMessage) { }

		#endregion Assume

		#region Assert

		/// <summary>
		/// In debug builds, perform a runtime check that <paramref name="condition"/> is true.
		/// </summary>
		/// <param name="condition">Expression to check to always be true.</param>
		[Pure]
		[Conditional("DEBUG"), Conditional("CONTRACTS_FULL")]
		[InlineCode("{$System.Diagnostics.Contracts.Contract}.Assert(4, function () {{ return {condition}; }})")]
		public static void Assert(bool condition) { }

		/// <summary>
		/// In debug builds, perform a runtime check that <paramref name="condition"/> is true.
		/// </summary>
		/// <param name="condition">Expression to check to always be true.</param>
		/// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
		[Pure]
		[Conditional("DEBUG"), Conditional("CONTRACTS_FULL")]
		[InlineCode("{$System.Diagnostics.Contracts.Contract}.Assert(4, function () {{ return {condition}; }}, {userMessage})")]
		public static void Assert(bool condition, String userMessage) { }

		#endregion Assert

		#region Requires

		/// <summary>
		/// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
		/// </summary>
		/// <param name="condition">Boolean expression representing the contract.</param>
		/// <remarks>
		/// This call must happen at the beginning of a method or property before any other code.
		/// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
		/// Use this form when backward compatibility does not force you to throw a particular exception.
		/// </remarks>
		[Pure]
		[Conditional("CONTRACTS_FULL")]
		[InlineCode("{$System.Diagnostics.Contracts.Contract}.Assert(0, function () {{ return {condition}; }})")]
		public static void Requires(bool condition) { }

		/// <summary>
		/// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
		/// </summary>
		/// <param name="condition">Boolean expression representing the contract.</param>
		/// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
		/// <remarks>
		/// This call must happen at the beginning of a method or property before any other code.
		/// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
		/// Use this form when backward compatibility does not force you to throw a particular exception.
		/// </remarks>
		[Pure]
		[Conditional("CONTRACTS_FULL")]
		[InlineCode("{$System.Diagnostics.Contracts.Contract}.Assert(0, function () {{ return {condition}; }}, {userMessage})")]
		public static void Requires(bool condition, String userMessage) { }

		/// <summary>
		/// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
		/// </summary>
		/// <param name="condition">Boolean expression representing the contract.</param>
		/// <remarks>
		/// This call must happen at the beginning of a method or property before any other code.
		/// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
		/// Use this form when you want to throw a particular exception.
		/// </remarks>
		[Pure]
		[InlineCode("{$System.Diagnostics.Contracts.Contract}.Requires({TException}, function () {{ return {condition}; }})")]
		public static void Requires<TException>(bool condition) where TException : Exception { }

		/// <summary>
		/// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
		/// </summary>
		/// <param name="condition">Boolean expression representing the contract.</param>
		/// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
		/// <remarks>
		/// This call must happen at the beginning of a method or property before any other code.
		/// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
		/// Use this form when you want to throw a particular exception.
		/// </remarks>
		[Pure]
		[InlineCode("{$System.Diagnostics.Contracts.Contract}.Requires({TException}, function () {{ return {condition}; }}, {userMessage})")]
		public static void Requires<TException>(bool condition, String userMessage) where TException : Exception { }

		#endregion Requires

		#region Ensures

		/// <summary>
		/// Specifies a public contract such that the expression <paramref name="condition"/> will be true when the enclosing method or property returns normally.
		/// </summary>
		/// <param name="condition">Boolean expression representing the contract.  May include <seealso cref="OldValue"/> and <seealso cref="Result"/>.</param>
		/// <remarks>
		/// This call must happen at the beginning of a method or property before any other code.
		/// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
		/// The contract rewriter must be used for runtime enforcement of this postcondition.
		/// </remarks>
		[Pure]
		[Conditional("CONTRACTS_FULL")]
		[InlineCode("0 /*{condition}*/")]
		public static void Ensures(bool condition) { }

		/// <summary>
		/// Specifies a public contract such that the expression <paramref name="condition"/> will be true when the enclosing method or property returns normally.
		/// </summary>
		/// <param name="condition">Boolean expression representing the contract.  May include <seealso cref="OldValue"/> and <seealso cref="Result"/>.</param>
		/// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
		/// <remarks>
		/// This call must happen at the beginning of a method or property before any other code.
		/// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
		/// The contract rewriter must be used for runtime enforcement of this postcondition.
		/// </remarks>
		[Pure]
		[Conditional("CONTRACTS_FULL")]
		[InlineCode("0 /*{condition} {userMessage}*/")]
		public static void Ensures(bool condition, String userMessage) { }

		/// <summary>
		/// Specifies a contract such that if an exception of type <typeparamref name="TException"/> is thrown then the expression <paramref name="condition"/> will be true when the enclosing method or property terminates abnormally.
		/// </summary>
		/// <typeparam name="TException">Type of exception related to this postcondition.</typeparam>
		/// <param name="condition">Boolean expression representing the contract.  May include <seealso cref="OldValue"/> and <seealso cref="Result"/>.</param>
		/// <remarks>
		/// This call must happen at the beginning of a method or property before any other code.
		/// This contract is exposed to clients so must only reference types and members at least as visible as the enclosing method.
		/// The contract rewriter must be used for runtime enforcement of this postcondition.
		/// </remarks>
		[Pure]
		[Conditional("CONTRACTS_FULL")]
		[InlineCode("0 /*{TException} {condition}*/")]
		public static void EnsuresOnThrow<TException>(bool condition) where TException : Exception { }

		/// <summary>
		/// Specifies a contract such that if an exception of type <typeparamref name="TException"/> is thrown then the expression <paramref name="condition"/> will be true when the enclosing method or property terminates abnormally.
		/// </summary>
		/// <typeparam name="TException">Type of exception related to this postcondition.</typeparam>
		/// <param name="condition">Boolean expression representing the contract.  May include <seealso cref="OldValue"/> and <seealso cref="Result"/>.</param>
		/// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
		/// <remarks>
		/// This call must happen at the beginning of a method or property before any other code.
		/// This contract is exposed to clients so must only reference types and members at least as visible as the enclosing method.
		/// The contract rewriter must be used for runtime enforcement of this postcondition.
		/// </remarks>
		[Pure]
		[Conditional("CONTRACTS_FULL")]
		[InlineCode("0 /*{TException} {condition} {userMessage}*/")]
		public static void EnsuresOnThrow<TException>(bool condition, String userMessage) where TException : Exception { }

		#region Old, Result, and Out Parameters

		/// <summary>
		/// Represents the result (a.k.a. return value) of a method or property.
		/// </summary>
		/// <typeparam name="T">Type of return value of the enclosing method or property.</typeparam>
		/// <returns>Return value of the enclosing method or property.</returns>
		/// <remarks>
		/// This method can only be used within the argument to the <seealso cref="Ensures(bool)"/> contract.
		/// </remarks>
		[Pure]
		[InlineCode("0 /*{T}*/")]
		public static T Result<T>() { return default(T); }

		/// <summary>
		/// Represents the final (output) value of an out parameter when returning from a method.
		/// </summary>
		/// <typeparam name="T">Type of the out parameter.</typeparam>
		/// <param name="value">The out parameter.</param>
		/// <returns>The output value of the out parameter.</returns>
		/// <remarks>
		/// This method can only be used within the argument to the <seealso cref="Ensures(bool)"/> contract.
		/// </remarks>
		[Pure]
		[InlineCode("0 /*{T} {value}*/")]
		public static T ValueAtReturn<T>(out T value) { value = default(T); return value; }

		/// <summary>
		/// Represents the value of <paramref name="value"/> as it was at the start of the method or property.
		/// </summary>
		/// <typeparam name="T">Type of <paramref name="value"/>.  This can be inferred.</typeparam>
		/// <param name="value">Value to represent.  This must be a field or parameter.</param>
		/// <returns>Value of <paramref name="value"/> at the start of the method or property.</returns>
		/// <remarks>
		/// This method can only be used within the argument to the <seealso cref="Ensures(bool)"/> contract.
		/// </remarks>
		[Pure]
		[InlineCode("0 /*{T} {value}*/")]
		public static T OldValue<T>(T value) { return default(T); }

		#endregion Old, Result, and Out Parameters

		#endregion Ensures

		#region Invariant

		/// <summary>
		/// Specifies a contract such that the expression <paramref name="condition"/> will be true after every method or property on the enclosing class.
		/// </summary>
		/// <param name="condition">Boolean expression representing the contract.</param>
		/// <remarks>
		/// This contact can only be specified in a dedicated invariant method declared on a class.
		/// This contract is not exposed to clients so may reference members less visible as the enclosing method.
		/// The contract rewriter must be used for runtime enforcement of this invariant.
		/// </remarks>
		[Pure]
		[Conditional("CONTRACTS_FULL")]
		[InlineCode("0 /*{condition}*/")]
		public static void Invariant(bool condition) { }

		/// <summary>
		/// Specifies a contract such that the expression <paramref name="condition"/> will be true after every method or property on the enclosing class.
		/// </summary>
		/// <param name="condition">Boolean expression representing the contract.</param>
		/// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
		/// <remarks>
		/// This contact can only be specified in a dedicated invariant method declared on a class.
		/// This contract is not exposed to clients so may reference members less visible as the enclosing method.
		/// The contract rewriter must be used for runtime enforcement of this invariant.
		/// </remarks>
		[Pure]
		[Conditional("CONTRACTS_FULL")]
		[InlineCode("0 /*{condition} {userMessage}*/")]
		public static void Invariant(bool condition, String userMessage) { }

		#endregion Invariant

		#region Quantifiers

		#region ForAll

		/// <summary>
		/// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
		/// for all integers starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.
		/// </summary>
		/// <param name="fromInclusive">First integer to pass to <paramref name="predicate"/>.</param>
		/// <param name="toExclusive">One greater than the last integer to pass to <paramref name="predicate"/>.</param>
		/// <param name="predicate">Function that is evaluated from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.</param>
		/// <returns><c>true</c> if <paramref name="predicate"/> returns <c>true</c> for all integers 
		/// starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.</returns>
		/// <seealso cref="System.Collections.Generic.List&lt;T&gt;.TrueForAll"/>
		[Pure]
		[InlineCode("{$System.Diagnostics.Contracts.Contract}.ForAll({fromInclusive}, {toExclusive}, {predicate})")]
		public static bool ForAll(int fromInclusive, int toExclusive, Predicate<int> predicate)
		{
			return false;
		}


		/// <summary>
		/// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
		/// for all elements in the <paramref name="collection"/>.
		/// </summary>
		/// <param name="collection">The collection from which elements will be drawn from to pass to <paramref name="predicate"/>.</param>
		/// <param name="predicate">Function that is evaluated on elements from <paramref name="collection"/>.</param>
		/// <returns><c>true</c> if and only if <paramref name="predicate"/> returns <c>true</c> for all elements in
		/// <paramref name="collection"/>.</returns>
		/// <seealso cref="System.Collections.Generic.List&lt;T&gt;.TrueForAll"/>
		[Pure]
		[InlineCode("{$System.Diagnostics.Contracts.Contract}.ForAll$1({collection}, {predicate})")]
		public static bool ForAll<T>(IEnumerable<T> collection, Predicate<T> predicate)
		{
			return false;
		}

		#endregion ForAll

		#region Exists

		/// <summary>
		/// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
		/// for any integer starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.
		/// </summary>
		/// <param name="fromInclusive">First integer to pass to <paramref name="predicate"/>.</param>
		/// <param name="toExclusive">One greater than the last integer to pass to <paramref name="predicate"/>.</param>
		/// <param name="predicate">Function that is evaluated from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.</param>
		/// <returns><c>true</c> if <paramref name="predicate"/> returns <c>true</c> for any integer
		/// starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.</returns>
		/// <seealso cref="System.Collections.Generic.List&lt;T&gt;.Exists"/>
		[Pure]
		[InlineCode("{$System.Diagnostics.Contracts.Contract}.Exists({fromInclusive}, {toExclusive}, {predicate})")]
		public static bool Exists(int fromInclusive, int toExclusive, Predicate<int> predicate)
		{
			return false;
		}

		/// <summary>
		/// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
		/// for any element in the <paramref name="collection"/>.
		/// </summary>
		/// <param name="collection">The collection from which elements will be drawn from to pass to <paramref name="predicate"/>.</param>
		/// <param name="predicate">Function that is evaluated on elements from <paramref name="collection"/>.</param>
		/// <returns><c>true</c> if and only if <paramref name="predicate"/> returns <c>true</c> for an element in
		/// <paramref name="collection"/>.</returns>
		/// <seealso cref="System.Collections.Generic.List&lt;T&gt;.Exists"/>
		[Pure]
		[InlineCode("{$System.Diagnostics.Contracts.Contract}.Exists$1({collection}, {predicate})")]
		public static bool Exists<T>(IEnumerable<T> collection, Predicate<T> predicate)
		{
			return false;
		}

		#endregion Exists

		#endregion Quantifiers

		#region Misc.

		/// <summary>
		/// Marker to indicate the end of the contract section of a method.
		/// </summary>
		[Conditional("CONTRACTS_FULL")]
		[InlineCode("0")]
		public static void EndContractBlock() { }

		#endregion

		#endregion User Methods
	}
}
