using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler {
	/// <summary>
	/// Represents the context for operations in the <see cref="IRuntimeLibrary"/>.
	/// </summary>
	public interface IRuntimeContext {
		/// <summary>
		/// Get the implementation of a type parameter (most likely an identifier). Can also issue errors (but must return something valid) if the type parameter is unavailable.
		/// </summary>
		JsExpression ResolveTypeParameter(ITypeParameter tp);

		/// <summary>
		/// Ensures that an expression can be evaluated multiple times without side effects. Returns the new expression and mutates the list in the <paramref name="expressionsThatMustBeEvaluatedBefore"/> parameter.
		/// </summary>
		/// <param name="expression">Expression that needs to be evaluated multiple times.</param>
		/// <param name="expressionsThatMustBeEvaluatedBefore">Expressions that need to be evaluated before the expression due to C# left-to-right semantics. This list will be mutated if necessary.</param>
		JsExpression EnsureCanBeEvaluatedMultipleTimes(JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore);
	}
}
