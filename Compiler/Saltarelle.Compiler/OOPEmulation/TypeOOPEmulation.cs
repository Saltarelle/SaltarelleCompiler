using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.OOPEmulation {
	public class TypeOOPEmulation {
		/// <summary>
		/// The different phases in OOP emulation for a type. The statements of the first phase for all types will appear in the script before any of the statements for the second phase for any type, and so on.
		/// </summary>
		public ReadOnlyCollection<TypeOOPEmulationPhase> Phases { get; private set; }

		public TypeOOPEmulation(IEnumerable<TypeOOPEmulationPhase> phases) {
			Phases = phases.AsReadOnly();
		}
	}
}