using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.Compiler;

namespace Saltarelle.Compiler.Tests
{
	public class MockNamer : INamer {
		public MockNamer(bool prefixWithDollar = true) {
			GetTypeParameterName     = tp => "$" + tp.Name;
			GetVariableName          = (desired, used) => {
			                               string baseName;
		                                   if (desired != null) {
		                                       baseName = desired.Replace("<>", "$");
		                                       if (prefixWithDollar && !baseName.StartsWith("$"))
		                                           baseName = "$" + baseName;
		                                   }
		                                   else {
		                                       baseName = "$tmp";
		                                   }
			                               if (desired != null && !used.Contains(baseName))
			                                   return baseName;
			                               int i = (desired == null ? 1 : 2);
			                               while (used.Contains(baseName + i.ToString(CultureInfo.InvariantCulture)))
			                                   i++;
			                               return baseName + i.ToString(CultureInfo.InvariantCulture);
			                           };
			GetStateMachineLoopLabel = used => {
			                               string result;
			                               int i = 0;
			                               do {
			                                   result = "$loop" + (++i).ToString(CultureInfo.InvariantCulture);
			                               } while (used.Contains(result));
			                               return result;
			                           };
			GetTypeVariableName      = n => "$" + n.Replace(".", "_");
			
			ThisAlias                                    = "$this";
			FinallyHandlerDesiredName                    = "$finally";
			StateVariableDesiredName                     = "$state";
			YieldResultVariableDesiredName               = "$result";
			AsyncStateMachineVariableDesiredName         = "$sm";
			AsyncDoFinallyBlocksVariableDesiredName      = "$doFinally";
			AsyncTaskCompletionSourceVariableDesiredName = "$tcs";
		}

		public Func<ITypeParameterSymbol, string> GetTypeParameterName { get; set; }
		public Func<string, ISet<string>, string> GetVariableName { get; set; }
		public Func<ISet<string>, string> GetStateMachineLoopLabel { get; set; }
		public Func<string, string> GetTypeVariableName { get; set; }

		public string ThisAlias { get; set; }
		public string FinallyHandlerDesiredName { get; set; }
		public string StateVariableDesiredName { get; set; }
		public string YieldResultVariableDesiredName { get; set; }
		public string AsyncStateMachineVariableDesiredName { get; set; }
		public string AsyncDoFinallyBlocksVariableDesiredName { get; set; }
		public string AsyncTaskCompletionSourceVariableDesiredName { get; set; }

		string INamer.GetTypeParameterName(ITypeParameterSymbol typeParameter) {
			return GetTypeParameterName(typeParameter);
		}

		string INamer.GetVariableName(string desiredName, ISet<string> usedNames) {
			return GetVariableName(desiredName, usedNames);
		}

		string INamer.GetStateMachineLoopLabel(ISet<string> usedNames) {
			return GetStateMachineLoopLabel(usedNames);
		}

		string INamer.GetTypeVariableName(string scriptTypeName) {
			return GetTypeVariableName(scriptTypeName);
		}
	}
}
