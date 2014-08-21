using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.OOPEmulation {
	public class TypeOOPEmulationPhase {
		/// <summary>
		/// This is the type that this phase on OOP emulation depends on. All statements in this phase for all of these types are guaranteed to appear in the script before the statements in this phase for the current type.
		/// Note that the different phases for a type's OOP emulation can depend on different types.
		/// </summary>
		public ReadOnlySet<INamedTypeSymbol> DependentOnTypes { get; private set; }

		/// <summary>
		/// The statements for this phase of OOP emulation.
		/// </summary>
		public ReadOnlyCollection<JsStatement> Statements { get; private set; }

		public TypeOOPEmulationPhase(IEnumerable<INamedTypeSymbol> dependentOnTypes, IEnumerable<JsStatement> statements) {
			DependentOnTypes = new ReadOnlySet<INamedTypeSymbol>(dependentOnTypes != null ? new HashSet<INamedTypeSymbol>(dependentOnTypes) : new HashSet<INamedTypeSymbol>());
			Statements = statements.AsReadOnly();
		}
	}
}