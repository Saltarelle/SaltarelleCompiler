using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler.Compiler {
	public interface INamer {
		string GetTypeParameterName(ITypeParameterSymbol typeParameter);
		string GetVariableName(string desiredName, ISet<string> usedNames);
		string GetStateMachineLoopLabel(ISet<string> usedNames);
		string ThisAlias { get; }
		string FinallyHandlerDesiredName { get; }
		string StateVariableDesiredName { get; }
		string YieldResultVariableDesiredName { get; }
		string AsyncStateMachineVariableDesiredName { get; }
		string AsyncDoFinallyBlocksVariableDesiredName { get; }
		string AsyncTaskCompletionSourceVariableDesiredName { get; }

		/// <summary>
		/// Returns the name for a variable that is used to represent a type in the pattern "var $TYPE = function() {} ... Type.registerClass(global, 'The.Name', $TYPE)"
		/// </summary>
		/// <param name="scriptTypeName">The (fully qualified) name by which the type is known in script (as returned by IMetadataImporter.GetTypeSemantics()).</param>
		string GetTypeVariableName(string scriptTypeName);
	}
}
