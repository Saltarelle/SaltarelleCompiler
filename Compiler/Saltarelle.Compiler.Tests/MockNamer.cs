using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.Compiler;

namespace Saltarelle.Compiler.Tests
{
	public class MockNamer : INamer {
		public MockNamer() {
			GetTypeParameterName = tp => "$" + tp.Name;
			GetVariableName      = (desired, used) => {
			                           string baseName;
		                               if (desired != null) {
		                                   baseName = desired.Replace("<>", "$");
		                                   if (!baseName.StartsWith("$"))
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
			ThisAlias                      = "$this";
			FinallyHandlerDesiredName      = "$finally";
			StateVariableName              = "$state";
			YieldResultVariableDesiredName = "$result";
		}

		public Func<ITypeParameter, string> GetTypeParameterName { get; set; }
		public Func<string, ISet<string>, string> GetVariableName { get; set; }

		public string ThisAlias { get; set; }
		public string FinallyHandlerDesiredName { get; set; }
		public string StateVariableName { get; set; }
		public string YieldResultVariableDesiredName { get; set; }

		string INamer.GetTypeParameterName(ITypeParameter typeParameter) {
			return GetTypeParameterName(typeParameter);
		}

		string INamer.GetVariableName(string desiredName, ISet<string> usedNames) {
			return GetVariableName(desiredName, usedNames);
		}
	}
}
