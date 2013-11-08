﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace Saltarelle.Compiler {
	/// <summary>
	/// This interface can be implemented to support transforming <see cref="JsType"/>s to actual JavaScript.
	/// </summary>
	public interface IOOPEmulator {
		/// <summary>
		/// Rewrite the specified "Objective JS" types to normal JS.
		/// </summary>
		/// <param name="types">Types to rewrute.</param>
		/// <param name="entryPoint">Program entry point, or null if no entry point.</param>
		IList<JsStatement> Process(IEnumerable<JsType> types, IMethod entryPoint);
	}
}
