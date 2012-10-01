using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.Compiler
{
	public class DefaultNamer : INamer {
		public string GetTypeParameterName(ITypeParameter typeParameter) {
			return typeParameter.Name;
		}

		public string GetVariableName(string desiredName, ISet<string> usedNames) {
			string baseName = (desiredName != null ? desiredName.Replace("<>", "$") : "$t");
			if (desiredName != null && !usedNames.Contains(baseName) && !JSModel.Utils.IsJavaScriptReservedWord(desiredName))
				return baseName;
			int i = 1;
			string name;
			do {
				name = baseName + (i++).ToString(CultureInfo.InvariantCulture);
			} while (usedNames.Contains(name));

			return name;
		}

		public string GetStateMachineLoopLabel(ISet<string> usedNames) {
			string result;
			int i = 0;
			do {
				result = "$sm" + (++i).ToString(CultureInfo.InvariantCulture);
			} while (usedNames.Contains(result));
			return result;
		}

		public string ThisAlias {
			get { return "$this"; }
		}

		public string FinallyHandlerDesiredName {
			get { return "$finally"; }
		}

		public string StateVariableDesiredName {
			get { return "$state"; }
		}

		public string YieldResultVariableDesiredName {
			get { return "$result"; }
		}

		public string AsyncStateMachineVariableDesiredName {
			get { return "$sm"; }
		}

		public string AsyncDoFinallyBlocksVariableDesiredName {
			get { return "$doFinally"; }
		}

		public string AsyncTaskCompletionSourceVariableDesiredName {
			get { return "$tcs"; }
		}
	}
}
