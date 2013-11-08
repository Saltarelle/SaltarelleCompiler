using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsSwitchSection {
		/// <summary>
		/// Contains a null entry for the default clause.
		/// </summary>
		public ReadOnlyCollection<JsExpression> Values { get; private set; }
		public JsBlockStatement Body { get; private set; }

		[Obsolete("Use factory method JsStatement.SwitchSection")]
		public JsSwitchSection(IEnumerable<JsExpression> values, JsStatement body) {
			if (values == null) throw new ArgumentNullException("values");
			if (body == null) throw new ArgumentNullException("body");
			Values = values.AsReadOnly();
			if (Values.Count == 0) throw new ArgumentException("Values cannot be empty", "values");
			Body = JsStatement.EnsureBlock(body);
		}
	}
}