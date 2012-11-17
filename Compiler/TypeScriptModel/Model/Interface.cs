using System.Collections.Generic;

namespace TypeScriptModel.Model {
	public class Interface : Member {
		public string Name { get; private set; }
		public IReadOnlyCollection<TSType> Extends { get; private set; }
		public IReadOnlyCollection<Member> Members { get; private set; }

		public Interface(string name, IEnumerable<TSType> extends, IEnumerable<Member> members) {
			Name = name;
			Extends = new List<TSType>(extends).AsReadOnly();
			Members = new List<Member>(members).AsReadOnly();
		}
	}
}