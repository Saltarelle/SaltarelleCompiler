using System.Collections.Generic;

namespace TypeScriptModel {
	public class Interface : Member {
		public string Name { get; private set; }
		public IReadOnlyCollection<TSType> Extends { get; private set; }
		public CompositeType Type { get; private set; }

		public Interface(string name, IEnumerable<TSType> extends, CompositeType type) {
			Name = name;
			Extends = extends != null ? new List<TSType>(extends).AsReadOnly() : null;
			Type = type;
		}
	}
}