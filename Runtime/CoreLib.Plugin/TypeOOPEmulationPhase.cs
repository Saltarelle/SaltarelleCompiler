using System.Collections.Generic;
using System.Collections.ObjectModel;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler;
using Saltarelle.Compiler.JSModel.ExtensionMethods;
using Saltarelle.Compiler.JSModel.Statements;

namespace CoreLib.Plugin {
	public class TypeOOPEmulationPhase {
		/// <summary>
		/// This is the type that this phase on OOP emulation depends on. All statements in this phase for all of these types are guaranteed to appear in the script before the statements in this phase for the current type.
		/// Note that the different phases for a type's OOP emulation can depend on different types.
		/// </summary>
		public ReadOnlySet<ITypeDefinition> DependentOnTypes { get; private set; }

		/// <summary>
		/// The statements for this phase of OOP emulation.
		/// </summary>
		public ReadOnlyCollection<JsStatement> Statements { get; private set; }

		public TypeOOPEmulationPhase(IEnumerable<ITypeDefinition> dependentOnTypes, IEnumerable<JsStatement> statements) {
			DependentOnTypes = new ReadOnlySet<ITypeDefinition>(dependentOnTypes != null ? new HashSet<ITypeDefinition>(dependentOnTypes) : new HashSet<ITypeDefinition>());
			Statements = statements.AsReadOnly();
		}
	}
}