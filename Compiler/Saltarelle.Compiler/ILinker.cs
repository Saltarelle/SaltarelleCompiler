using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler {
	/// <summary>
	/// This interface should be implemented by classes that support importing reference. The purpose of this stage in the pipeline is to get rid of all <see cref="JsTypeReferenceExpression"/>s.
	/// This can be done either trivially, or by transforming the source tree to a RequireJS module, or something else.
	/// </summary>
	public interface ILinker {
		/// <summary>
		/// Import all references in the given statements, thereby ensuring that no <see cref="JsTypeReferenceExpression"/>s are left.
		/// </summary>
		/// <param name="statements">Statements to process.</param>
		IList<JsStatement> Process(IList<JsStatement> statements);

		/// <summary>
		/// Returns an expression that can be used to instruct the linker to insert a reference to the current assembly.
		/// </summary>
		JsExpression CurrentAssemblyExpression { get; }
	}
}
