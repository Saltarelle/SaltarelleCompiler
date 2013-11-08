using System;

namespace Saltarelle.Compiler.JSModel.Statements {
	[Serializable]
	public class JsCatchClause {
		public string Identifier { get; private set; }
		public JsBlockStatement Body { get; private set; }

		[Obsolete("Use factory method JsStatement.CatchClause")]
		public JsCatchClause(string identifier, JsStatement body) {
			if (identifier == null) throw new ArgumentNullException("identifier");
			if (body == null) throw new ArgumentNullException("body");
			Identifier = identifier;
			Body = JsStatement.EnsureBlock(body);
		}
	}
}