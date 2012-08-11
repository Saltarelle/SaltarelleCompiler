using System.Collections.Generic;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel.StateMachineRewrite
{
	internal class Section {
		public State State { get; private set; }
		public IList<JsStatement> Statements { get; private set; }

		public Section(State state, IEnumerable<JsStatement> statements) {
			this.State = state;
			this.Statements = new List<JsStatement>(statements);
		}
	}
}