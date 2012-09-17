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

		public DelegateScriptSemantics(bool expandParams = false, bool bindThisToFirstParameter = false) {
			this.ExpandParams = expandParams;
			this.BindThisToFirstParameter = bindThisToFirstParameter;
		}
	}
}
