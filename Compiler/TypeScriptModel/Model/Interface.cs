using System.Collections.Generic;

namespace TypeScriptModel.Model {
	public class Interface : Member {
		public string Name { get; private set; }
		public IReadOnlyList<TypeReference> Extends { get; private set; }
		public IReadOnlyList<Member> Members { get; private set; }

		public Interface(string name, IEnumerable<TypeReference> extends, IEnumerable<Member> members) {
			Name = name;
			Extends = new List<TypeReference>(extends).AsReadOnly();
			Members = new List<Member>(members).AsReadOnly();
		}
	}
}