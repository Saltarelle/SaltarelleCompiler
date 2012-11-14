using System.Collections.Generic;

namespace TypeScriptModel {
	public class CompositeType : TSType {
		public IReadOnlyCollection<Member> Members { get; private set; }

		public CompositeType(IEnumerable<Member> members) {
			Members = new List<Member>(members).AsReadOnly();
		}
	}
}