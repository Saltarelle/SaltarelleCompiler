using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.ScriptSemantics {
	public class DelegateScriptSemantics {
		/// <summary>
		/// Indicates that the Javascript 'this' should appear as the first argument to the delegate.
		/// </summary>
		public bool BindThisToFirstParameter { get; private set; }

		/// <summary>
		/// Whether the parameter list for the delegate is expanded.
		/// </summary>
		public bool ExpandParams { get; private set; }

		/// <summary>
		/// If non-null, arguments after this one should be omitted if they are not specified in the source code.
		/// </summary>
		public int? OmitUnspecifiedArgumentsFrom { get; private set; }

		public DelegateScriptSemantics(bool expandParams = false, bool bindThisToFirstParameter = false, int? omitUnspecifiedArgumentsFrom = null) {
			ExpandParams = expandParams;
			BindThisToFirstParameter = bindThisToFirstParameter;
			OmitUnspecifiedArgumentsFrom = omitUnspecifiedArgumentsFrom;
		}
	}
}
