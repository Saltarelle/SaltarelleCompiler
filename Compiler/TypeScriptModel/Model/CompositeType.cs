using System.Collections.Generic;

namespace TypeScriptModel.Model {
	public class CompositeType : TSType {
		public IReadOnlyList<Member> Members { get; private set; }

		public CompositeType(IEnumerable<Member> members) {
			Members = new List<Member>(members).AsReadOnly();
		}
	}
}