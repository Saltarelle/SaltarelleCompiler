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
		JsExpression TypeIs(ICompilation compilation, JsExpression expression, JsExpression targetType);

		/// <summary>
		/// Returns an expression that casts an expression to a specified type, or returns null if the expression is not of that type (equivalent to C# "as").
		/// </summary>
		JsExpression TypeAs(ICompilation compilation, JsExpression expression, JsExpression targetType);

		/// <summary>
		/// Returns an expression that casts an expression to a specified type, or throws an exception if the expression is not of that type (equivalent to C# "(Type)expression").
		/// </summary>
		JsExpression Cast(ICompilation compilation, JsExpression expression, JsExpression targetType);

		/// <summary>
		/// Returns an expression that will instantiate a generic type.
		/// </summary>
		JsExpression InstantiateGenericType(ICompilation compilation, JsExpression type, IEnumerable<JsExpression> typeArguments);
	}
}
